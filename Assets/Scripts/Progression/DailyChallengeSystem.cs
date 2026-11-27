using System;
using UnityEngine;

namespace DudeChaos.Progression
{
    public sealed class DailyChallengeSystem
    {
        private readonly Wallet _wallet;
        private Func<DateTime> _clockProvider = () => DateTime.UtcNow;

        private DailyChallengeSpec[] _specs = new DailyChallengeSpec[DailyChallengeRules.SlotsPerDay];
        private int[] _progress = new int[DailyChallengeRules.SlotsPerDay];
        private bool[] _claimed = new bool[DailyChallengeRules.SlotsPerDay];
        private int _currentDayKey;
        private int _lastPlayedDayKey;
        private int _streak = 1;
        private int _bestStreak = 1;
        private int _fullSetsCompleted;
        private bool _bonusPaid;
        private int _playerLevel = 1;

        public event Action Updated;
        public event Action FullSetCompleted;

        public DailyChallengeSpec[] Specs => _specs;
        public int[] Progress => _progress;
        public int CurrentDayKey => _currentDayKey;
        public int LastPlayedDayKey => _lastPlayedDayKey;
        public int Streak => _streak;
        public int BestStreak => _bestStreak;
        public int FullSetsCompleted => _fullSetsCompleted;
        public bool BonusPaid => _bonusPaid;

        public DailyChallengeSystem(Wallet wallet)
        {
            _wallet = wallet ?? new Wallet();
        }

        public void SetDayOverride(Func<DateTime> clockOverride)
        {
            _clockProvider = clockOverride ?? (() => DateTime.UtcNow);
        }

        public void EnsureCurrentDay(int playerLevel)
        {
            _playerLevel = Mathf.Max(1, playerLevel);
            int today = DailyChallengeRules.EncodeDayKey(_clockProvider());
            if (_currentDayKey != today)
            {
                RollOverToDay(today);
            }
        }

        public void RollOverToDay(int newDayKey)
        {
            if (!DailyChallengeRules.TryDateOf(newDayKey, out _))
                newDayKey = DailyChallengeRules.EncodeDayKey(DateTime.UtcNow);

            if (_currentDayKey != 0 && _currentDayKey != newDayKey)
                _lastPlayedDayKey = _currentDayKey;

            _streak = DailyChallengeRules.ComputeUpdatedStreak(_streak, _lastPlayedDayKey, newDayKey);
            if (_streak > _bestStreak) _bestStreak = _streak;
            _currentDayKey = newDayKey;
            _lastPlayedDayKey = newDayKey;
            _specs = DailyChallengeRules.GenerateDailySet(_currentDayKey, _playerLevel);
            _progress = new int[DailyChallengeRules.SlotsPerDay];
            _claimed = new bool[DailyChallengeRules.SlotsPerDay];
            _bonusPaid = false;
            Updated?.Invoke();
        }

        public void ReportActivity(DailyActivityKind kind, int amount)
        {
            if (amount <= 0) return;
            EnsureCurrentDay(_playerLevel);

            bool anyChange = false;
            for (int i = 0; i < _specs.Length; i++)
            {
                if (_specs[i].kind != kind) continue;
                int prev = _progress[i];
                _progress[i] = Mathf.Min(_specs[i].target, _progress[i] + amount);
                if (_progress[i] != prev) anyChange = true;

                if (!_claimed[i] && _progress[i] >= _specs[i].target)
                {
                    _claimed[i] = true;
                    int payout = Mathf.RoundToInt(_specs[i].rewardMoney * DailyChallengeRules.StreakMultiplier(_streak));
                    _wallet.Add(payout);
                }
            }

            if (!_bonusPaid && _claimed[0] && _claimed[1] && _claimed[2])
            {
                _bonusPaid = true;
                _fullSetsCompleted++;
                _wallet.Add(DailyChallengeRules.ComputeFullSetBonus(_specs, _streak));
                FullSetCompleted?.Invoke();
            }

            if (anyChange)
                Updated?.Invoke();
        }

        public void Restore(
            int dayKey,
            int lastDayKey,
            int streak,
            int[] progress,
            bool bonusPaid,
            int fullSetsCompleted,
            int bestStreak,
            int playerLevel)
        {
            _playerLevel = Mathf.Max(1, playerLevel);
            _lastPlayedDayKey = lastDayKey;
            _streak = Mathf.Max(1, streak);
            _bestStreak = Mathf.Max(_streak, bestStreak);
            _fullSetsCompleted = Mathf.Max(0, fullSetsCompleted);

            if (DailyChallengeRules.TryDateOf(dayKey, out _))
            {
                _currentDayKey = dayKey;
                _specs = DailyChallengeRules.GenerateDailySet(_currentDayKey, _playerLevel);
                _progress = new int[DailyChallengeRules.SlotsPerDay];
                _claimed = new bool[DailyChallengeRules.SlotsPerDay];
                for (int i = 0; i < DailyChallengeRules.SlotsPerDay; i++)
                {
                    int val = (progress != null && i < progress.Length) ? Mathf.Max(0, progress[i]) : 0;
                    _progress[i] = Mathf.Min(_specs[i].target, val);
                    _claimed[i] = _progress[i] >= _specs[i].target;
                }
                _bonusPaid = bonusPaid;
            }
            else
            {
                EnsureCurrentDay(_playerLevel);
            }
        }
    }
}
