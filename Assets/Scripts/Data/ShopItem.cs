using UnityEngine;

namespace DudeChaos.Data
{
    [CreateAssetMenu(fileName = "SHP_New", menuName = "DudeChaos/Data/ShopItem")]
    public class ShopItem : ScriptableObject
    {
        public string itemId = "";
        public string displayName = "";
        public ShopItemType itemType = ShopItemType.Weapon;
        public int price = 1000;
        public int unlockLevel = 1;
        public GameObject prefabReference;
    }
}
