using System;
using DudeChaos.Data;
using DudeChaos.Wanted;
using UnityEngine;

namespace DudeChaos.Progression
{
    public sealed class BountySystem
    {
        private readonly Wallet _wallet;
        private WantedSystem _wanted;
        private float _lastAwardTime = -999f;
        private float _windowStartTime = -999f;
        private int _countInWindow;

        public event Action<int, CrimeType> Awarded;

        public BountySystem(Wallet wallet, WantedSystem wanted = null)
        {
            _wallet = wallet ?? new Wallet();
            if (wanted != null) Bind(wanted);
        }

        public void Bind(WantedSystem wanted)
        {
            if (_wanted != null)
                _wanted.CrimeReported -= OnCrimeReported;
            _wanted = wanted;
            if (_wanted != null)
                _wanted.CrimeReported += OnCrimeReported;
        }

        public void Unbind()
        {
            if (_wanted != null)
            {
                _wanted.CrimeReported -= OnCrimeReported;
                _wanted = null;
            }
        }

        private void OnCrimeReported(CrimeType crime, float heatAdded)
        {
            ProcessCrime(crime, Time.time);
        }

        public int ProcessCrime(CrimeType crime, float currentTimeSeconds)
        {
            if ((currentTimeSeconds - _lastAwardTime) < BountyRules.AntiDoublePayGapSeconds)
                return 0;

            if ((currentTimeSeconds - _windowStartTime) > BountyRules.WindowSeconds)
            {
                _windowStartTime = currentTimeSeconds;
                _countInWindow = 0;
            }

            float mult = BountyRules.DiminishingMultiplier(_countInWindow);
            int amount = Mathf.Max(1, Mathf.RoundToInt(BountyRules.BaseBountyForCrime(crime) * mult));

            _lastAwardTime = currentTimeSeconds;
            _countInWindow++;
            _wallet.Add(amount);
            Awarded?.Invoke(amount, crime);
            return amount;
        }
    }
}
