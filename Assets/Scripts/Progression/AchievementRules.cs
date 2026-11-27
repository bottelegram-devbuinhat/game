using System.Collections.Generic;

namespace DudeChaos.Progression
{
    public struct AchievementDefinition
    {
        public string id;
        public string title;
        public int rewardMoney;
    }

    public struct AchievementProgressSnapshot
    {
        public int completedMissions;
        public int totalThreeStarMissions;
        public int fullDailySetsWon;
        public int bestDailyStreak;
        public int stuntMoneyEarned;
        public int sideJobsCompleted;
        public int totalMoneyEarned;
        public int variantsCleared;
    }

    // 13 achievements (§1.8 & §2.8): 10 read from existing save state, 3 use counters; rewards 500..30,000 $.
    public static class AchievementRules
    {
        public static readonly AchievementDefinition[] All = new AchievementDefinition[]
        {
            new AchievementDefinition { id = "ach_first_job", title = "First Delivery", rewardMoney = 500 },
            new AchievementDefinition { id = "ach_act_one", title = "Out of the Box", rewardMoney = 1500 },
            new AchievementDefinition { id = "ach_halfway", title = "City Regular", rewardMoney = 3500 },
            new AchievementDefinition { id = "ach_story_clear", title = "No Rules Left", rewardMoney = 15000 },
            new AchievementDefinition { id = "ach_star_rookie", title = "Clean Work", rewardMoney = 1200 },
            new AchievementDefinition { id = "ach_star_master", title = "Perfectionist", rewardMoney = 30000 },
            new AchievementDefinition { id = "ach_stunt_driver", title = "Sideways", rewardMoney = 1000 },
            new AchievementDefinition { id = "ach_stunt_pro", title = "Tyre Smoke", rewardMoney = 4500 },
            new AchievementDefinition { id = "ach_side_hustle", title = "Night Shift", rewardMoney = 2000 },
            new AchievementDefinition { id = "ach_big_earner", title = "Six Figures", rewardMoney = 8000 },
            new AchievementDefinition { id = "ach_daily_set", title = "Full Routine", rewardMoney = 1500 },
            new AchievementDefinition { id = "ach_streak_5", title = "Five Day Fever", rewardMoney = 5000 },
            new AchievementDefinition { id = "ach_variant_clear", title = "Hard Mode", rewardMoney = 4000 }
        };

        public static bool IsSatisfied(string achievementId, AchievementProgressSnapshot s)
        {
            switch (achievementId)
            {
                case "ach_first_job": return s.completedMissions >= 1;
                case "ach_act_one": return s.completedMissions >= 5;
                case "ach_halfway": return s.completedMissions >= 10;
                case "ach_story_clear": return s.completedMissions >= 20;
                case "ach_star_rookie": return s.totalThreeStarMissions >= 3;
                case "ach_star_master": return s.totalThreeStarMissions >= 20;
                case "ach_stunt_driver": return s.stuntMoneyEarned >= 1000;
                case "ach_stunt_pro": return s.stuntMoneyEarned >= 10000;
                case "ach_side_hustle": return s.sideJobsCompleted >= 10;
                case "ach_big_earner": return s.totalMoneyEarned >= 100000;
                case "ach_daily_set": return s.fullDailySetsWon >= 1;
                case "ach_streak_5": return s.bestDailyStreak >= 5;
                case "ach_variant_clear": return s.variantsCleared >= 3;
                default: return false;
            }
        }
    }
}
