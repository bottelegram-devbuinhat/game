using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.Combat
{
    public enum SupportType
    {
        UavScan = 0,
        UavJammer = 1,
        StrikeDrone = 2,
        AmmoDrop = 3,
        Minefield = 4,
        AutoTurret = 5,
        ArmouredCar = 6,
        MortarTeam = 7,
        Airstrike = 8,
        AntiAir = 9,
        RocketArtillery = 10,
        Gunship = 11,
        TankSupport = 12
    }

    public struct SupportSpec
    {
        public SupportType type;
        public float durationSeconds;
        public float damage;
        public float radius;
        public float cooldownSeconds;
    }

    public static class SupportRules
    {
        public static SupportSpec GetSpec(EquipmentData support)
        {
            if (support == null || support.kind != EquipmentKind.Support)
            {
                return new SupportSpec
                {
                    type = SupportType.UavScan,
                    durationSeconds = 25f,
                    damage = 0f,
                    radius = 120f,
                    cooldownSeconds = 45f
                };
            }

            string id = support.equipmentId ?? string.Empty;
            SupportType t = SupportType.UavScan;
            if (id.Contains("jammer")) t = SupportType.UavJammer;
            else if (id.Contains("strike")) t = SupportType.StrikeDrone;
            else if (id.Contains("ammo")) t = SupportType.AmmoDrop;
            else if (id.Contains("mine")) t = SupportType.Minefield;
            else if (id.Contains("turret")) t = SupportType.AutoTurret;
            else if (id.Contains("armoured")) t = SupportType.ArmouredCar;
            else if (id.Contains("mortar")) t = SupportType.MortarTeam;
            else if (id.Contains("airstrike")) t = SupportType.Airstrike;
            else if (id.Contains("anti_air")) t = SupportType.AntiAir;
            else if (id.Contains("rocket")) t = SupportType.RocketArtillery;
            else if (id.Contains("gunship")) t = SupportType.Gunship;
            else if (id.Contains("tank")) t = SupportType.TankSupport;

            return new SupportSpec
            {
                type = t,
                durationSeconds = Mathf.Max(0f, support.valueA),
                damage = Mathf.Max(0f, support.valueB),
                radius = Mathf.Max(0f, support.valueC),
                cooldownSeconds = Mathf.Clamp(support.price / 60f, 15f, 180f)
            };
        }

        public static bool CanCallSupport(float lastCallTime, float now, SupportSpec spec, bool indoors)
        {
            if (indoors) return false;
            return (now - lastCallTime) >= spec.cooldownSeconds;
        }
    }
}
