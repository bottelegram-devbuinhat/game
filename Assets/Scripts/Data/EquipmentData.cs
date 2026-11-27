using UnityEngine;

namespace DudeChaos.Data
{
    [CreateAssetMenu(fileName = "EQ_New", menuName = "DudeChaos/Data/Equipment")]
    public class EquipmentData : ScriptableObject
    {
        public string equipmentId = "";
        public string displayName = "";
        public EquipmentKind kind = EquipmentKind.Attachment;
        public AttachmentSlot slot = AttachmentSlot.None;
        public int price = 100;
        public int unlockLevel = 1;
        public float valueA = 0f;
        public float valueB = 0f;
        public float valueC = 0f;
        public float valueD = 0f;
        public WeaponClass[] allowedClasses = new WeaponClass[0];
    }
}
