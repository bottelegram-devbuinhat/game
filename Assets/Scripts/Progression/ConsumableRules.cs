using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.Progression
{
    public struct ConsumableEffect
    {
        public float healAmount;
        public float armourAmount;
        public float useSeconds;
        public int carryLimit;
        public float speedBuffMultiplier;
        public float buffDurationSeconds;
        public float postCrashSlowSeconds;
    }

    public static class ConsumableRules
    {
        public static ConsumableEffect Evaluate(EquipmentData item)
        {
            if (item == null || item.kind != EquipmentKind.Consumable)
                return default;

            bool isAdrenaline = item.equipmentId != null && item.equipmentId.Contains("adrenaline");
            bool isEnergy = item.equipmentId != null && item.equipmentId.Contains("energy");

            if (isAdrenaline || isEnergy)
            {
                return new ConsumableEffect
                {
                    healAmount = Mathf.Max(0f, item.valueA),
                    armourAmount = 0f,
                    useSeconds = 1.0f,
                    carryLimit = Mathf.Max(1, Mathf.RoundToInt(item.valueD)),
                    speedBuffMultiplier = Mathf.Max(1f, item.valueB),
                    buffDurationSeconds = Mathf.Max(1f, item.valueC),
                    postCrashSlowSeconds = isAdrenaline ? 3.5f : 0f
                };
            }

            return new ConsumableEffect
            {
                healAmount = Mathf.Max(0f, item.valueA),
                armourAmount = Mathf.Max(0f, item.valueB),
                useSeconds = Mathf.Max(0.2f, item.valueC),
                carryLimit = Mathf.Max(1, Mathf.RoundToInt(item.valueD)),
                speedBuffMultiplier = 1.0f,
                buffDurationSeconds = 0f,
                postCrashSlowSeconds = 0f
            };
        }
    }
}
