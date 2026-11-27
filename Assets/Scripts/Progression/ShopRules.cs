using System.Collections.Generic;
using DudeChaos.Data;

namespace DudeChaos.Progression
{
    public enum ShopPurchaseResult
    {
        Success = 0,
        InvalidItem = 1,
        AlreadyOwned = 2,
        LockedByLevel = 3,
        InsufficientFunds = 4
    }

    public static class ShopRules
    {
        public static ShopPurchaseResult Evaluate(
            ShopItem item,
            int playerLevel,
            int walletBalance,
            ISet<string> ownedItemIds)
        {
            if (item == null || string.IsNullOrEmpty(item.itemId) || item.price < 0)
                return ShopPurchaseResult.InvalidItem;
            if (ownedItemIds != null && ownedItemIds.Contains(item.itemId))
                return ShopPurchaseResult.AlreadyOwned;
            if (playerLevel < item.unlockLevel)
                return ShopPurchaseResult.LockedByLevel;
            if (walletBalance < item.price)
                return ShopPurchaseResult.InsufficientFunds;
            return ShopPurchaseResult.Success;
        }
    }
}
