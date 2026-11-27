using UnityEngine;

namespace DudeChaos.Progression
{
    public struct MissionGradeResult
    {
        public int totalScore;
        public int completionPoints;
        public int clockPoints;
        public int cleanPoints;
        public int stars;
        public int totalMoneyPayout;
        public string nextStarHint;
    }

    // §1.7 & §2.8 & Landmine #28:
    // 100 pts = 40 completion + 40 clock (full at par = 70% limit, 0 at limit; NoClockScore = 20 when no time limit)
    // + 20 clean (-10/retry, -3/wanted level, max -8 health lost).
    // 1 star = finish; 2 stars >= 70; 3 stars >= 85 AND 0 retries AND 0 wanted AND inside par (and has clock!).
    // Payout bonus: +10% at 2 stars, +25% at 3 stars on base reward ONLY; variant multiplier applies to whole payout.
    public static class MissionGradeRules
    {
        public const int CompletionScore = 40;
        public const int MaxClockScore = 40;
        public const int NoClockScore = 20;
        public const int MaxCleanScore = 20;
        public const float ParFraction = 0.70f;

        public static MissionGradeResult Evaluate(
            int baseRewardMoney,
            int timeBonusMoney,
            float elapsedSeconds,
            float timeLimitSeconds,
            int retries,
            int finalWantedLevel,
            float healthLostFraction01,
            MissionVariant variant = MissionVariant.None)
        {
            int completion = CompletionScore;
            bool hasClock = timeLimitSeconds > 0f;
            float parSeconds = hasClock ? timeLimitSeconds * ParFraction : 0f;

            int clockPts;
            if (!hasClock)
            {
                clockPts = NoClockScore;
            }
            else if (elapsedSeconds <= parSeconds)
            {
                clockPts = MaxClockScore;
            }
            else if (elapsedSeconds >= timeLimitSeconds)
            {
                clockPts = 0;
            }
            else
            {
                float span = Mathf.Max(0.001f, timeLimitSeconds - parSeconds);
                float t = 1f - ((elapsedSeconds - parSeconds) / span);
                clockPts = Mathf.RoundToInt(MaxClockScore * Mathf.Clamp01(t));
            }

            float dmgWeight = MissionModifierRules.DamageGradingMultiplier(variant);
            int healthPenalty = Mathf.Clamp(Mathf.RoundToInt(Mathf.Clamp01(healthLostFraction01) * 8f * dmgWeight), 0, 8);
            int cleanPts = Mathf.Max(0, MaxCleanScore - (Mathf.Max(0, retries) * 10) - (Mathf.Max(0, finalWantedLevel) * 3) - healthPenalty);

            int total = Mathf.Clamp(completion + clockPts + cleanPts, 0, 100);

            bool insidePar = hasClock && elapsedSeconds <= parSeconds + 0.001f;
            int stars = 1;
            if (total >= 85 && retries == 0 && finalWantedLevel == 0 && insidePar)
                stars = 3;
            else if (total >= 70)
                stars = 2;

            float starBonusRate = stars == 3 ? 0.25f : (stars == 2 ? 0.10f : 0f);
            int starBonusMoney = Mathf.RoundToInt(baseRewardMoney * starBonusRate);
            int rawPayout = baseRewardMoney + starBonusMoney + Mathf.Max(0, timeBonusMoney);
            int finalPayout = Mathf.RoundToInt(rawPayout * MissionModifierRules.PayoutMultiplier(variant));

            string hint = BuildNextStarHint(stars, hasClock, elapsedSeconds, parSeconds, retries, finalWantedLevel, total);

            return new MissionGradeResult
            {
                totalScore = total,
                completionPoints = completion,
                clockPoints = clockPts,
                cleanPoints = cleanPts,
                stars = stars,
                totalMoneyPayout = finalPayout,
                nextStarHint = hint
            };
        }

        public static string EncodeStar(string missionId, int stars)
        {
            return $"{missionId}:{Mathf.Clamp(stars, 0, 3)}";
        }

        // Landmine #16: int.TryParse only, never int.Parse.
        public static bool TryDecodeStar(string entry, out string missionId, out int stars)
        {
            missionId = string.Empty;
            stars = 0;
            if (string.IsNullOrWhiteSpace(entry)) return false;
            int idx = entry.LastIndexOf(':');
            if (idx <= 0 || idx >= entry.Length - 1) return false;
            missionId = entry.Substring(0, idx).Trim();
            if (string.IsNullOrEmpty(missionId)) return false;
            if (!int.TryParse(entry.Substring(idx + 1).Trim(), out int parsed))
                return false;
            stars = Mathf.Clamp(parsed, 0, 3);
            return true;
        }

        private static string BuildNextStarHint(
            int stars,
            bool hasClock,
            float elapsed,
            float par,
            int retries,
            int wanted,
            int totalScore)
        {
            if (stars >= 3)
                return string.Empty;
            if (!hasClock)
                return "2 stars max: mission has no time limit";
            if (retries > 0)
                return $"3 stars: finish with 0 retries — this run {retries}";
            if (wanted > 0)
                return $"3 stars: finish with 0 wanted stars — this run {wanted}";
            if (elapsed > par)
                return $"3 stars: finish under {FormatClock(par)} — this run {FormatClock(elapsed)}";
            if (totalScore < 85)
                return $"3 stars: score at least 85 — this run {totalScore}";
            return string.Empty;
        }

        private static string FormatClock(float seconds)
        {
            int totalSec = Mathf.Max(0, Mathf.RoundToInt(seconds));
            return $"{totalSec / 60}:{totalSec % 60:D2}";
        }
    }
}
