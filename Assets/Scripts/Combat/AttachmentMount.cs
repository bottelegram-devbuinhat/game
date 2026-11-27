using System.Collections.Generic;
using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.Combat
{
    public sealed class AttachmentMount : MonoBehaviour
    {
        private readonly Dictionary<AttachmentSlot, EquipmentData> _slots = new Dictionary<AttachmentSlot, EquipmentData>();

        public bool TryEquip(EquipmentData attachment, WeaponClass weaponClass)
        {
            if (!AttachmentRules.CanAttach(attachment, weaponClass))
                return false;
            _slots[attachment.slot] = attachment;
            return true;
        }

        public void Remove(AttachmentSlot slot) => _slots.Remove(slot);
        public void ClearAll() => _slots.Clear();

        public EquipmentData Get(AttachmentSlot slot)
        {
            return _slots.TryGetValue(slot, out EquipmentData eq) ? eq : null;
        }

        public float Zoom => AttachmentRules.GetOpticZoom(Get(AttachmentSlot.Optic));
        public float RecoilMultiplier => AttachmentRules.GetRecoilMultiplier(
            Get(AttachmentSlot.Muzzle),
            Get(AttachmentSlot.Underbarrel),
            Get(AttachmentSlot.Stock));
        public float SpreadMultiplier => AttachmentRules.GetSpreadMultiplier(
            Get(AttachmentSlot.Underbarrel),
            Get(AttachmentSlot.Laser));
        public float NoiseMultiplier => AttachmentRules.GetNoiseMultiplier(Get(AttachmentSlot.Muzzle));
        public float ReloadMultiplier => AttachmentRules.GetReloadMultiplier(Get(AttachmentSlot.Magazine));
        public int ModifyMagazine(int baseSize) => AttachmentRules.GetEffectiveMagazineSize(baseSize, Get(AttachmentSlot.Magazine));
    }
}
