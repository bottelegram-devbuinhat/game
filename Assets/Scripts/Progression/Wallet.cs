using System;
using UnityEngine;

namespace DudeChaos.Progression
{
    public sealed class Wallet
    {
        private int _balance;
        private int _totalEarned;

        public event Action<int> Changed;

        public int Balance => _balance;
        public int TotalEarned => _totalEarned;

        public Wallet(int initialBalance = 0)
        {
            _balance = Mathf.Max(0, initialBalance);
            _totalEarned = _balance;
        }

        public void Add(int amount)
        {
            if (amount <= 0) return;
            long sum = (long)_balance + amount;
            _balance = sum > int.MaxValue ? int.MaxValue : (int)sum;
            long earned = (long)_totalEarned + amount;
            _totalEarned = earned > int.MaxValue ? int.MaxValue : (int)earned;
            Changed?.Invoke(_balance);
        }

        public bool CanAfford(int cost) => cost >= 0 && _balance >= cost;

        public bool TrySpend(int cost)
        {
            if (cost < 0 || _balance < cost)
                return false;
            _balance -= cost;
            Changed?.Invoke(_balance);
            return true;
        }

        public void SetBalance(int balance, int totalEarnedOverride = -1)
        {
            _balance = Mathf.Max(0, balance);
            _totalEarned = totalEarnedOverride >= 0 ? Mathf.Max(_balance, totalEarnedOverride) : Mathf.Max(_totalEarned, _balance);
            Changed?.Invoke(_balance);
        }
    }
}
