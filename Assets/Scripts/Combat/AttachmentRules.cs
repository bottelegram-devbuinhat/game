using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.Combat
{
    public static class AttachmentRules
    {
        public static bool CanAttach(EquipmentData attachment, WeaponClass weaponClass)
        {
            if (attachment == null || attachment.kind != EquipmentKind.Attachment)
                return false;
            if (attachment.slot == AttachmentSlot.None)
                return false;
            if (weaponClass == WeaponClass.Melee)
                return false;
            if (attachment.allowedClasses != null && attachment.allowedClasses.Length > 0)
            {
                for (int i = 0; i < attachment.allowedClasses.Length; i++)
                {
                    if (attachment.allowedClasses[i] == weaponClass)
                        return true;
                }
                return false;
            }
            return true;
        }

        // §2.3: Optic's valueA is zoom.
        public static float GetOpticZoom(EquipmentData optic)
        {
            if (optic == null || optic.kind != EquipmentKind.Attachment || optic.slot != AttachmentSlot.Optic)
                return 1.0f;
            return Mathf.Max(1.0f, optic.valueA);
        }

        public static float GetRecoilMultiplier(EquipmentData muzzle, EquipmentData underbarrel, EquipmentData stock)
        {
            float mult = 1.0f;
            if (muzzle != null && muzzle.slot == AttachmentSlot.Muzzle && muzzle.valueA > 0f)
                mult *= muzzle.valueA;
            if (underbarrel != null && underbarrel.slot == AttachmentSlot.Underbarrel && underbarrel.valueB > 0f)
                mult *= underbarrel.valueB;
            if (stock != null && stock.slot == AttachmentSlot.Stock && stock.valueB > 0f)
                mult *= stock.valueB;
            return Mathf.Clamp(mult, 0.2f, 2.0f);
        }

        public static float GetSpreadMultiplier(EquipmentData underbarrel, EquipmentData laser)
        {
            float mult = 1.0f;
            if (underbarrel != null && underbarrel.slot == AttachmentSlot.Underbarrel && underbarrel.valueB > 0f)
                mult *= underbarrel.valueB;
            if (laser != null && laser.slot == AttachmentSlot.Laser && laser.valueB > 0f)
                mult *= laser.valueB;
            return Mathf.Clamp(mult, 0.3f, 2.0f);
        }

        public static float GetNoiseMultiplier(EquipmentData muzzle)
        {
            if (muzzle != null && muzzle.slot == AttachmentSlot.Muzzle && muzzle.valueB > 0f)
                return Mathf.Clamp(muzzle.valueB, 0.1f, 2.0f);
            return 1.0f;
        }

        public static int GetEffectiveMagazineSize(int baseMagazine, EquipmentData magazineAttachment)
        {
            if (baseMagazine <= 0) return 0;
            int extra = 0;
            if (magazineAttachment != null && magazineAttachment.slot == AttachmentSlot.Magazine && magazineAttachment.valueA > 0f)
                extra = Mathf.RoundToInt(magazineAttachment.valueA);
            return Mathf.Max(1, baseMagazine + extra);
        }

        public static float GetReloadMultiplier(EquipmentData magazineAttachment)
        {
            if (magazineAttachment != null && magazineAttachment.slot == AttachmentSlot.Magazine && magazineAttachment.valueC > 0f)
                return Mathf.Clamp(magazineAttachment.valueC, 0.4f, 2.0f);
            return 1.0f;
        }
    }
}
