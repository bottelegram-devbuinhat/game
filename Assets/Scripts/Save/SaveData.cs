using System;
using UnityEngine;

namespace DudeChaos.Save
{
    // Format-4 payload & envelope (§1.8 lines 331-338).
    [Serializable]
    public sealed class SaveData
    {
        public const int CurrentFormatVersion = 4;

        public Vector3 position = Vector3.zero;
        public float rotationY = 0f;
        public float health = 100f;
        public float armour = 0f;
        public int money = 0;
        public int reputation = 0;
        public float wantedHeat = 0f;
        public int wantedLevel = 0;
        public string[] unlockedItemIds = Array.Empty<string>();
        public string[] inventoryItemIds = Array.Empty<string>();
        public string currentMissionId = "";
        public int currentMissionProgress = 0;
        public string[] completedMissionIds = Array.Empty<string>();
        public float hourOfDay = 9.0f;
        public int hotelNightsLeft = 0;
        public int stuntsEarned = 0;
        public int sideJobsCompleted = 0;
        public int sideJobsFailed = 0;
        public string[] missionStars = Array.Empty<string>();
        public int dailyDayKey = 0;
        public int dailyLastDayKey = 0;
        public int dailyStreak = 0;
        public int[] dailyProgress = new int[3];
        public bool dailyBonusPaid = false;
        public string[] achievementIds = Array.Empty<string>();
        public int dailiesCompleted = 0;
        public int bestStreak = 0;
        public int variantsCleared = 0;
    }

    [Serializable]
    public sealed class SaveEnvelope
    {
        public int formatVersion = SaveData.CurrentFormatVersion;
        public uint checksum = 0;
        public string payload = "";
    }
}
