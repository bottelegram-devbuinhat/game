using System;
using System.Text;
using UnityEngine;

namespace DudeChaos.Save
{
    // FNV-1a checksum, version gate 1..4, migration v1->v4, Sanitise post-load repair, int.TryParse everywhere (Landmine #16).
    public static class SaveIntegrity
    {
        public const int MaxArrayLength = 256;

        public static uint ComputeFnv1a(string payload)
        {
            if (payload == null) return 0u;
            byte[] bytes = Encoding.UTF8.GetBytes(payload);
            uint hash = 2166136261u;
            for (int i = 0; i < bytes.Length; i++)
            {
                hash ^= bytes[i];
                hash *= 16777619u;
            }
            return hash;
        }

        public static bool IsSupportedVersion(int formatVersion)
        {
            return formatVersion >= 1 && formatVersion <= SaveData.CurrentFormatVersion;
        }

        public static string Pack(SaveData data)
        {
            SaveData copy = data ?? new SaveData();
            Sanitise(copy);
            string payload = JsonUtility.ToJson(copy);
            SaveEnvelope env = new SaveEnvelope
            {
                formatVersion = SaveData.CurrentFormatVersion,
                checksum = ComputeFnv1a(payload),
                payload = payload
            };
            return JsonUtility.ToJson(env);
        }

        public static bool TryUnpack(string rawEnvelopeJson, out SaveData data)
        {
            data = null;
            if (string.IsNullOrWhiteSpace(rawEnvelopeJson))
                return false;

            SaveEnvelope env;
            try
            {
                env = JsonUtility.FromJson<SaveEnvelope>(rawEnvelopeJson);
            }
            catch
            {
                return false;
            }

            if (env == null || !IsSupportedVersion(env.formatVersion) || string.IsNullOrEmpty(env.payload))
                return false;

            uint actualChecksum = ComputeFnv1a(env.payload);
            if (actualChecksum != env.checksum)
                return false;

            SaveData parsed;
            try
            {
                parsed = JsonUtility.FromJson<SaveData>(env.payload);
            }
            catch
            {
                return false;
            }

            if (parsed == null)
                return false;

            MigrateAndSanitise(parsed, env.formatVersion);
            data = parsed;
            return true;
        }

        public static void MigrateAndSanitise(SaveData data, int fromVersion)
        {
            if (data == null) return;
            if (fromVersion < 2)
            {
                data.hourOfDay = data.hourOfDay <= 0f ? 9f : data.hourOfDay;
                data.hotelNightsLeft = Mathf.Clamp(data.hotelNightsLeft, 0, 7);
            }
            if (fromVersion < 3)
            {
                if (data.dailyProgress == null || data.dailyProgress.Length != 3)
                    data.dailyProgress = new int[3];
                data.dailyStreak = Mathf.Max(0, data.dailyStreak);
            }
            if (fromVersion < 4)
            {
                data.achievementIds = data.achievementIds ?? Array.Empty<string>();
                data.dailiesCompleted = Mathf.Max(0, data.dailiesCompleted);
                data.bestStreak = Mathf.Max(data.bestStreak, data.dailyStreak);
                data.variantsCleared = Mathf.Max(0, data.variantsCleared);
            }
            Sanitise(data);
        }

        public static void Sanitise(SaveData data)
        {
            if (data == null) return;

            if (float.IsNaN(data.position.x) || float.IsInfinity(data.position.x)) data.position.x = 0f;
            if (float.IsNaN(data.position.y) || float.IsInfinity(data.position.y)) data.position.y = 0f;
            if (float.IsNaN(data.position.z) || float.IsInfinity(data.position.z)) data.position.z = 0f;
            data.position.x = Mathf.Clamp(data.position.x, -5000f, 5000f);
            data.position.y = Mathf.Clamp(data.position.y, -200f, 2000f);
            data.position.z = Mathf.Clamp(data.position.z, -5000f, 5000f);

            if (float.IsNaN(data.rotationY) || float.IsInfinity(data.rotationY)) data.rotationY = 0f;
            data.rotationY = Mathf.Repeat(data.rotationY, 360f);

            data.health = float.IsNaN(data.health) ? 100f : Mathf.Clamp(data.health, 0f, 500f);
            data.armour = float.IsNaN(data.armour) ? 0f : Mathf.Clamp(data.armour, 0f, 500f);
            data.money = Mathf.Max(0, data.money);
            data.reputation = Mathf.Max(0, data.reputation);
            data.wantedHeat = float.IsNaN(data.wantedHeat) ? 0f : Mathf.Clamp(data.wantedHeat, 0f, 1000f);
            data.wantedLevel = Mathf.Clamp(data.wantedLevel, 0, 6);
            data.hourOfDay = float.IsNaN(data.hourOfDay) ? 9f : Mathf.Repeat(data.hourOfDay, 24f);
            data.hotelNightsLeft = Mathf.Clamp(data.hotelNightsLeft, 0, 7);
            data.stuntsEarned = Mathf.Max(0, data.stuntsEarned);
            data.sideJobsCompleted = Mathf.Max(0, data.sideJobsCompleted);
            data.sideJobsFailed = Mathf.Max(0, data.sideJobsFailed);
            data.dailyStreak = Mathf.Clamp(data.dailyStreak, 0, 3650);
            data.dailiesCompleted = Mathf.Max(0, data.dailiesCompleted);
            data.bestStreak = Mathf.Max(data.dailyStreak, Mathf.Max(0, data.bestStreak));
            data.variantsCleared = Mathf.Max(0, data.variantsCleared);

            // Half-finished mission is deliberately not resumed (§1.8).
            data.currentMissionId = "";
            data.currentMissionProgress = 0;

            data.unlockedItemIds = TrimArray(data.unlockedItemIds, MaxArrayLength);
            data.inventoryItemIds = TrimArray(data.inventoryItemIds, MaxArrayLength);
            data.completedMissionIds = TrimArray(data.completedMissionIds, 64);
            data.missionStars = SanitiseStarEntries(data.missionStars, 64);
            data.achievementIds = TrimArray(data.achievementIds, 64);

            if (data.dailyProgress == null || data.dailyProgress.Length != 3)
            {
                int[] fixedArr = new int[3];
                if (data.dailyProgress != null)
                {
                    int copyLen = Math.Min(3, data.dailyProgress.Length);
                    for (int i = 0; i < copyLen; i++)
                        fixedArr[i] = Mathf.Max(0, data.dailyProgress[i]);
                }
                data.dailyProgress = fixedArr;
            }
            else
            {
                for (int i = 0; i < 3; i++)
                    data.dailyProgress[i] = Mathf.Max(0, data.dailyProgress[i]);
            }
        }

        // Landmine #16: NEVER call int.Parse on save strings (overflow throws). Always int.TryParse.
        public static bool TryParseStarEntry(string encoded, out string missionId, out int stars)
        {
            missionId = string.Empty;
            stars = 0;
            if (string.IsNullOrWhiteSpace(encoded)) return false;
            int colon = encoded.LastIndexOf(':');
            if (colon <= 0 || colon >= encoded.Length - 1) return false;
            string id = encoded.Substring(0, colon).Trim();
            string numPart = encoded.Substring(colon + 1).Trim();
            if (string.IsNullOrEmpty(id) || !int.TryParse(numPart, out int parsed))
                return false;
            missionId = id;
            stars = Mathf.Clamp(parsed, 0, 3);
            return true;
        }

        private static string[] SanitiseStarEntries(string[] entries, int cap)
        {
            if (entries == null || entries.Length == 0)
                return Array.Empty<string>();
            int limit = Math.Min(cap, entries.Length);
            var valid = new System.Collections.Generic.List<string>(limit);
            for (int i = 0; i < limit; i++)
            {
                if (TryParseStarEntry(entries[i], out string id, out int s))
                    valid.Add($"{id}:{s}");
            }
            return valid.ToArray();
        }

        private static string[] TrimArray(string[] source, int maxLen)
        {
            if (source == null || source.Length == 0)
                return Array.Empty<string>();
            if (source.Length <= maxLen)
                return source;
            string[] trimmed = new string[maxLen];
            Array.Copy(source, trimmed, maxLen);
            return trimmed;
        }
    }
}
