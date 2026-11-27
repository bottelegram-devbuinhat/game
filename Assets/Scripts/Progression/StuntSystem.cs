using System;
using UnityEngine;

namespace DudeChaos.Progression
{
    public sealed class StuntSystem
    {
        private readonly Wallet _wallet;
        private float _pendingPoints;
        private int _combo = 1;
        private float _idleTimer;
        private int _totalMoneyEarned;

        public event Action<int, int> StuntBanked;
        public event Action ComboLost;

        public float PendingPoints => _pendingPoints;
        public int Combo => _combo;
        public int TotalMoneyEarned
        {
            get => _totalMoneyEarned;
            set => _totalMoneyEarned = Mathf.Max(0, value);
        }

        public StuntSystem(Wallet wallet)
        {
            _wallet = wallet ?? new Wallet();
        }

        public void AddDrift(float slipAngle, float speed, float dt)
        {
            float pts = StuntRules.DriftPointsPerSecond(slipAngle, speed) * Mathf.Max(0f, dt);
            if (pts > 0f)
            {
                _pendingPoints += pts;
                _idleTimer = 0f;
            }
        }

        public void AddAirtime(float speed, float dt)
        {
            float pts = StuntRules.AirtimePointsPerSecond(speed) * Mathf.Max(0f, dt);
            if (pts > 0f)
            {
                _pendingPoints += pts;
                _idleTimer = 0f;
            }
        }

        public void AddNearMiss(float relativeSpeed)
        {
            int pts = StuntRules.NearMissPoints(relativeSpeed);
            if (pts > 0)
            {
                _pendingPoints += pts;
                _combo = StuntRules.ComboCap(_combo + 1);
                _idleTimer = 0f;
            }
        }

        public void OnCrash()
        {
            if (_pendingPoints <= 0f) return;
            _pendingPoints = 0f;
            _combo = 1;
            _idleTimer = 0f;
            ComboLost?.Invoke();
        }

        public void Tick(float deltaTime)
        {
            if (_pendingPoints <= 0f) return;
            _idleTimer += Mathf.Max(0f, deltaTime);
            if (_idleTimer >= StuntRules.SettleDelaySeconds)
            {
                BankNow();
            }
        }

        public int BankNow()
        {
            if (_pendingPoints <= 0f) return 0;
            int totalScore = Mathf.RoundToInt(_pendingPoints * _combo);
            int money = StuntRules.PointsToMoney(totalScore);
            _pendingPoints = 0f;
            _combo = 1;
            _idleTimer = 0f;
            if (money > 0)
            {
                _totalMoneyEarned += money;
                _wallet.Add(money);
            }
            StuntBanked?.Invoke(totalScore, money);
            return money;
        }
    }
}
