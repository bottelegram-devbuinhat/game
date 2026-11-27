using System.Linq;
using DudeChaos.City;
using DudeChaos.Player;
using DudeChaos.Progression;
using DudeChaos.Save;
using DudeChaos.Wanted;
using UnityEngine;

namespace DudeChaos.GameFlow
{
    // Captures/applies Format-4 SaveData; autosaves on pause/quit (Android lifecycle, §1.8).
    // Deliberately does NOT resume a half-finished mission.
    public sealed class SaveCoordinator : MonoBehaviour
    {
        private readonly SaveSystem _saveSystem = new SaveSystem();

        private Transform _playerTransform;
        private HealthSystem _playerHealth;
        private Wallet _wallet;
        private ShopSystem _shop;
        private WeaponInventory _inventory;
        private MissionSystem _missions;
        private WantedSystem _wanted;
        private DayNightCycle _clock;
        private HotelService _hotel;
        private StuntSystem _stunts;
        private SideJobDirector _sideJobs;
        private DailyChallengeSystem _dailies;
        private AchievementSystem _achievements;

        public SaveSystem SaveSystem => _saveSystem;

        public void BindSystems(
            Transform playerTransform,
            HealthSystem playerHealth,
            Wallet wallet,
            ShopSystem shop,
            WeaponInventory inventory,
            MissionSystem missions,
            WantedSystem wanted,
            DayNightCycle clock,
            HotelService hotel,
            StuntSystem stunts,
            SideJobDirector sideJobs,
            DailyChallengeSystem dailies,
            AchievementSystem achievements)
        {
            _playerTransform = playerTransform;
            _playerHealth = playerHealth;
            _wallet = wallet;
            _shop = shop;
            _inventory = inventory;
            _missions = missions;
            _wanted = wanted;
            _clock = clock;
            _hotel = hotel;
            _stunts = stunts;
            _sideJobs = sideJobs;
            _dailies = dailies;
            _achievements = achievements;
        }

        public SaveData CaptureSnapshot()
        {
            SaveData d = new SaveData();
            if (_playerTransform != null)
            {
                d.position = _playerTransform.position;
                d.rotationY = _playerTransform.eulerAngles.y;
            }
            if (_playerHealth != null)
            {
                d.health = _playerHealth.CurrentHealth;
                d.armour = _playerHealth.CurrentArmour;
            }
            if (_wallet != null) d.money = _wallet.Balance;
            if (_missions != null)
            {
                d.reputation = _missions.Reputation;
                d.completedMissionIds = _missions.CompletedMissionIds.ToArray();
                d.missionStars = _missions.ExportStarEntries();
                d.variantsCleared = _missions.VariantsCleared;
            }
            if (_wanted != null)
            {
                d.wantedHeat = _wanted.Heat;
                d.wantedLevel = _wanted.Level;
            }
            if (_shop != null) d.unlockedItemIds = _shop.OwnedItemIds.ToArray();
            if (_inventory != null) d.inventoryItemIds = _inventory.ExportIds();
            if (_clock != null) d.hourOfDay = _clock.CurrentHour;
            if (_hotel != null) d.hotelNightsLeft = _hotel.NightsRemaining;
            if (_stunts != null) d.stuntsEarned = _stunts.TotalMoneyEarned;
            if (_sideJobs != null)
            {
                d.sideJobsCompleted = _sideJobs.CompletedCount;
                d.sideJobsFailed = _sideJobs.FailedCount;
            }
            if (_dailies != null)
            {
                d.dailyDayKey = _dailies.CurrentDayKey;
                d.dailyLastDayKey = _dailies.LastPlayedDayKey;
                d.dailyStreak = _dailies.Streak;
                d.dailyProgress = (int[])_dailies.Progress.Clone();
                d.dailyBonusPaid = _dailies.BonusPaid;
                d.dailiesCompleted = _dailies.FullSetsCompleted;
                d.bestStreak = _dailies.BestStreak;
            }
            if (_achievements != null)
                d.achievementIds = _achievements.ExportUnlockedIds();

            // Half-finished mission is deliberately not resumed.
            d.currentMissionId = "";
            d.currentMissionProgress = 0;
            SaveIntegrity.Sanitise(d);
            return d;
        }

        public void ApplySnapshot(SaveData d)
        {
            if (d == null) return;
            SaveIntegrity.Sanitise(d);

            if (_playerTransform != null)
            {
                CharacterController cc = _playerTransform.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                _playerTransform.position = d.position;
                _playerTransform.rotation = Quaternion.Euler(0f, d.rotationY, 0f);
                if (cc != null) cc.enabled = true;
            }
            _playerHealth?.Configure(100f, Mathf.Max(1f, d.health), 100f, d.armour);
            _wallet?.SetBalance(d.money);
            if (_shop != null)
            {
                _shop.Reputation = d.reputation;
                _shop.RestoreOwned(d.unlockedItemIds);
            }
            if (_missions != null)
            {
                _missions.Clear();
                _missions.RestoreProgress(d.completedMissionIds, d.missionStars, d.reputation, d.variantsCleared);
            }
            _wanted?.RestoreState(d.wantedHeat, d.wantedLevel);
            _clock?.SetHour(d.hourOfDay);
            _hotel?.RestoreNights(d.hotelNightsLeft);
            if (_stunts != null) _stunts.TotalMoneyEarned = d.stuntsEarned;
            _sideJobs?.RestoreCounts(d.sideJobsCompleted, d.sideJobsFailed);

            int level = ProgressionCurve.LevelForReputation(d.reputation);
            _dailies?.Restore(d.dailyDayKey, d.dailyLastDayKey, d.dailyStreak, d.dailyProgress, d.dailyBonusPaid, d.dailiesCompleted, d.bestStreak, level);

            if (_achievements != null)
            {
                int threeStars = 0;
                if (d.missionStars != null)
                {
                    for (int i = 0; i < d.missionStars.Length; i++)
                    {
                        if (SaveIntegrity.TryParseStarEntry(d.missionStars[i], out _, out int s) && s >= 3)
                            threeStars++;
                    }
                }
                _achievements.Restore(d.achievementIds, new AchievementProgressSnapshot
                {
                    completedMissions = d.completedMissionIds != null ? d.completedMissionIds.Length : 0,
                    totalThreeStarMissions = threeStars,
                    fullDailySetsWon = d.dailiesCompleted,
                    bestDailyStreak = d.bestStreak,
                    stuntMoneyEarned = d.stuntsEarned,
                    sideJobsCompleted = d.sideJobsCompleted,
                    totalMoneyEarned = _wallet != null ? _wallet.TotalEarned : d.money,
                    variantsCleared = d.variantsCleared
                });
            }
        }

        public void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                _saveSystem.SaveAutosave(CaptureSnapshot());
        }

        public void OnApplicationQuit()
        {
            _saveSystem.SaveAutosave(CaptureSnapshot());
        }
    }
}
