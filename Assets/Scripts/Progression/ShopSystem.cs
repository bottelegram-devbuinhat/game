using System;
using System.Collections.Generic;
using DudeChaos.Data;

namespace DudeChaos.Progression
{
    public sealed class ShopSystem
    {
        private readonly Wallet _wallet;
        private readonly HashSet<string> _ownedIds = new HashSet<string>();
        private int _reputation;

        public event Action<ShopItem> ItemPurchased;

        public Wallet Wallet => _wallet;
        public int Reputation
        {
            get => _reputation;
            set => _reputation = value < 0 ? 0 : value;
        }
        public int PlayerLevel => ProgressionCurve.LevelForReputation(_reputation);
        public IReadOnlyCollection<string> OwnedItemIds => _ownedIds;

        public ShopSystem(Wallet wallet)
        {
            _wallet = wallet ?? new Wallet();
        }

        public bool Owns(string itemId) => !string.IsNullOrEmpty(itemId) && _ownedIds.Contains(itemId);

        public ShopPurchaseResult TryBuy(ShopItem item)
        {
            ShopPurchaseResult eval = ShopRules.Evaluate(item, PlayerLevel, _wallet.Balance, _ownedIds);
            if (eval != ShopPurchaseResult.Success)
                return eval;
            if (!_wallet.TrySpend(item.price))
                return ShopPurchaseResult.InsufficientFunds;
            _ownedIds.Add(item.itemId);
            ItemPurchased?.Invoke(item);
            return ShopPurchaseResult.Success;
        }

        public void RestoreOwned(IEnumerable<string> ids)
        {
            _ownedIds.Clear();
            if (ids == null) return;
            foreach (string id in ids)
            {
                if (!string.IsNullOrEmpty(id))
                    _ownedIds.Add(id);
            }
        }
    }
}
