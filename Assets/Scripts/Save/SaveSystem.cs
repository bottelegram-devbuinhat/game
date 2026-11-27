using System;
using UnityEngine;

namespace DudeChaos.Save
{
    // 3 manual slots (0, 1, 2) + autosave slot (-1) in PlayerPrefs (§1.8).
    public sealed class SaveSystem
    {
        public const int ManualSlotCount = 3;
        public const int AutosaveSlot = -1;
        private const string KeyPrefix = "DudeChaos.SaveSlot.";

        public event Action<int, SaveData> Saved;
        public event Action<int, SaveData> Loaded;
        public event Action<int> CorruptSlotRejected;

        public static string KeyForSlot(int slot)
        {
            if (slot == AutosaveSlot) return KeyPrefix + "Auto";
            return KeyPrefix + Mathf.Clamp(slot, 0, ManualSlotCount - 1);
        }

        public bool Save(int slot, SaveData data)
        {
            if (slot != AutosaveSlot && (slot < 0 || slot >= ManualSlotCount))
                return false;
            string packed = SaveIntegrity.Pack(data ?? new SaveData());
            PlayerPrefs.SetString(KeyForSlot(slot), packed);
            PlayerPrefs.Save();
            Saved?.Invoke(slot, data);
            return true;
        }

        public bool SaveAutosave(SaveData data) => Save(AutosaveSlot, data);

        public bool TryLoad(int slot, out SaveData data)
        {
            data = null;
            if (slot != AutosaveSlot && (slot < 0 || slot >= ManualSlotCount))
                return false;

            string key = KeyForSlot(slot);
            if (!PlayerPrefs.HasKey(key))
                return false;

            string raw = PlayerPrefs.GetString(key, string.Empty);
            if (!SaveIntegrity.TryUnpack(raw, out SaveData unpacked))
            {
                CorruptSlotRejected?.Invoke(slot);
                return false;
            }

            data = unpacked;
            Loaded?.Invoke(slot, data);
            return true;
        }

        public bool HasValidSave(int slot)
        {
            return TryLoad(slot, out _);
        }

        public void DeleteSlot(int slot)
        {
            PlayerPrefs.DeleteKey(KeyForSlot(slot));
            PlayerPrefs.Save();
        }
    }
}
