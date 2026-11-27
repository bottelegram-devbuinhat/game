using System;
using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.Vehicles
{
    public sealed class VehicleLock : MonoBehaviour
    {
        [SerializeField] private bool isLocked;
        [SerializeField] private ArmourClass armourClass = ArmourClass.Unarmoured;
        [SerializeField] private HandlingProfile profile = HandlingProfile.Sports;

        private float _hotwireProgress;

        public event Action Unlocked;
        public event Action<int> BreakInStarted;

        public bool IsLocked => isLocked;
        public float HotwireProgress => _hotwireProgress;

        public void Configure(bool locked, HandlingProfile handlingProfile, ArmourClass armour)
        {
            isLocked = locked;
            profile = handlingProfile;
            armourClass = armour;
            _hotwireProgress = 0f;
        }

        public bool StepHotwire(float deltaTime)
        {
            if (!isLocked) return true;
            if (!ArmourRules.CanHotwire(armourClass)) return false;

            if (_hotwireProgress <= 0f)
            {
                BreakInStarted?.Invoke(VehicleLockRules.BreakInCrimeHeat(armourClass));
            }

            float needed = VehicleLockRules.HotwireDurationSeconds(profile, armourClass);
            _hotwireProgress += Mathf.Max(0f, deltaTime);
            if (_hotwireProgress >= needed)
            {
                isLocked = false;
                _hotwireProgress = needed;
                Unlocked?.Invoke();
                return true;
            }
            return false;
        }

        public void UnlockWithKey()
        {
            if (!isLocked) return;
            isLocked = false;
            Unlocked?.Invoke();
        }
    }
}
