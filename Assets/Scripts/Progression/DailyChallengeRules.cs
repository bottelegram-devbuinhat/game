using System;
using UnityEngine;

namespace DudeChaos.Progression
{
    public enum DailyActivityKind
    {
        StuntScore = 0,
        BountyMoney = 1,
        SideJobs = 2,
        Missions = 3,
        EvadePolice = 4
    }

    public struct DailyChallengeSpec
    {
        public DailyActivityKind kind;
        public int target;
        public int rewardMoney;
    }

    // §1.8 & §2.8 & Landmines #15 & #17:
    // - Safe DateOf validation (never throws on 31 April, 29 Feb non-leap, year > 9999).
    // - Always produces 3 distinct activity kinds per day (unused-kind fallback).
    // - Streak multiplier 1.0 -> 1.5 capped at 5 days; 30% full-set bonus.
    public static class DailyChallengeRules
    {
        public const int SlotsPerDay = 3;
        public const int KindCount = 5;
        public const float FullSetBonusRatio = 0.30f;

        public static int EncodeDayKey(DateTime utcDate)
        {
            return utcDate.Year * 10000 + utcDate.Month * 100 + utcDate.Day;
        }

        // Landmine #15: Validate year <= 9999 and DateTime.DaysInMonth BEFORE constructing DateTime.
        public static bool TryDateOf(int dayKey, out DateTime date)
        {
            date = new DateTime(2026, 1, 1);
            if (dayKey < 10101) return false;
            int year = dayKey / 10000;
            int month = (dayKey / 100) % 100;
            int day = dayKey % 100;

            if (year < 1 || year > 9999) return false;
            if (month < 1 || month > 12) return false;
            int maxDays = DateTime.DaysInMonth(year, month);
            if (day < 1 || day > maxDays) return false;

            date = new DateTime(year, month, day);
            return true;
        }

        public static DateTime SafeDateOf(int dayKey, DateTime fallback)
        {
            return TryDateOf(dayKey, out DateTime d) ? d : fallback;
        }

        public static bool IsConsecutiveDay(int previousDayKey, int currentDayKey)
        {
            if (!TryDateOf(previousDayKey, out DateTime prev) || !TryDateOf(currentDayKey, out DateTime curr))
                return false;
            return (curr.Date - prev.Date).TotalDays == 1.0;
        }

        public static int ComputeUpdatedStreak(int currentStreak, int lastPlayedDayKey, int todayDayKey)
        {
            if (lastPlayedDayKey == todayDayKey && currentStreak > 0)
                return currentStreak;
            if (IsConsecutiveDay(lastPlayedDayKey, todayDayKey))
                return Mathf.Max(1, currentStreak + 1);
            return 1;
        }

        public static float StreakMultiplier(int streakDays)
        {
            if (streakDays <= 1) return 1.0f;
            int steps = Mathf.Clamp(streakDays - 1, 0, 4);
            return 1.0f + steps * 0.125f; // Day 1=1.0, 2=1.125, 3=1.25, 4=1.375, >=5=1.5
        }

        // Landmine #17: Guarantee 3 distinct kinds on every single day; fill collisions from unused kinds.
        public static DailyChallengeSpec[] GenerateDailySet(int dayKey, int playerLevel)
        {
            DailyChallengeSpec[] result = new DailyChallengeSpec[SlotsPerDay];
            bool[] used = new bool[KindCount];
            uint seed = HashDay(dayKey);

            for (int slot = 0; slot < SlotsPerDay; slot++)
            {
                seed = NextHash(seed + (uint)(slot * 7919));
                int pick = (int)(seed % KindCount);
                int guard = 0;
                while (used[pick] && guard < KindCount)
                {
                    pick = (pick + 1) % KindCount;
                    guard++;
                }
                if (used[pick])
                {
                    for (int k = 0; k < KindCount; k++)
                    {
                        if (!used[k])
                        {
                            pick = k;
                            break;
                        }
                    }
                }
                used[pick] = true;

                DailyActivityKind kind = (DailyActivityKind)pick;
                seed = NextHash(seed);
                float jitter = 0.90f + ((seed % 201u) / 1000f); // 0.90 .. 1.10 (±10%)
                int target = ComputeScaledTarget(kind, playerLevel, jitter);
                int baseReward = ComputeBaseReward(kind, playerLevel);

                result[slot] = new DailyChallengeSpec
                {
                    kind = kind,
                    target = target,
                    rewardMoney = baseReward
                };
            }
            return result;
        }

        public static int ComputeFullSetBonus(DailyChallengeSpec[] specs, int streakDays)
        {
            if (specs == null || specs.Length == 0) return 0;
            int sum = 0;
            for (int i = 0; i < specs.Length; i++)
                sum += specs[i].rewardMoney;
            return Mathf.RoundToInt(sum * StreakMultiplier(streakDays) * FullSetBonusRatio);
        }

        private static int ComputeScaledTarget(DailyActivityKind kind, int level, float jitter)
        {
            int lvl = Mathf.Max(1, level);
            float raw;
            switch (kind)
            {
                case DailyActivityKind.StuntScore:
                    raw = (400f + lvl * 80f) * jitter;
                    return Mathf.Max(100, RoundToTen(raw));
                case DailyActivityKind.BountyMoney:
                    raw = (300f + lvl * 70f) * jitter;
                    return Mathf.Max(100, RoundToTen(raw));
                case DailyActivityKind.SideJobs:
                    return Mathf.Clamp(1 + lvl / 6, 1, 5);
                case DailyActivityKind.Missions:
                    return Mathf.Clamp(1 + lvl / 8, 1, 4);
                case DailyActivityKind.EvadePolice:
                    return Mathf.Clamp(1 + lvl / 5, 1, 5);
                default:
                    return 10;
            }
        }

        private static int ComputeBaseReward(DailyActivityKind kind, int level)
        {
            int lvl = Mathf.Max(1, level);
            return 350 + lvl * 65 + ((int)kind * 40);
        }

        private static int RoundToTen(float value)
        {
            return Mathf.Max(10, Mathf.RoundToInt(value / 10f) * 10);
        }

        private static uint HashDay(int dayKey)
        {
            unchecked
            {
                uint x = (uint)dayKey ^ 0x9E3779B9u;
                return NextHash(x);
            }
        }

        private static uint NextHash(uint x)
        {
            unchecked
            {
                x ^= x << 13;
                x ^= x >> 17;
                x ^= x << 5;
                return x == 0u ? 0x6D2B79F5u : x;
            }
        }
    }
}
