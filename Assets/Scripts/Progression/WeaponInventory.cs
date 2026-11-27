using System.Collections.Generic;
using DudeChaos.Data;

namespace DudeChaos.Progression
{
    public sealed class WeaponInventory
    {
        private readonly List<WeaponData> _weapons = new List<WeaponData>();
        private int _activeIndex = -1;

        public int Count => _weapons.Count;
        public WeaponData ActiveWeapon => (_activeIndex >= 0 && _activeIndex < _weapons.Count) ? _weapons[_activeIndex] : null;

        public bool AddWeapon(WeaponData weapon)
        {
            if (weapon == null || string.IsNullOrEmpty(weapon.weaponId))
                return false;
            for (int i = 0; i < _weapons.Count; i++)
            {
                if (_weapons[i].weaponId == weapon.weaponId)
                    return false;
            }
            _weapons.Add(weapon);
            if (_activeIndex < 0) _activeIndex = 0;
            return true;
        }

        public bool SelectSlot(int index)
        {
            if (index < 0 || index >= _weapons.Count) return false;
            _activeIndex = index;
            return true;
        }

        public string[] ExportIds()
        {
            string[] ids = new string[_weapons.Count];
            for (int i = 0; i < _weapons.Count; i++)
                ids[i] = _weapons[i].weaponId;
            return ids;
        }
    }
}
