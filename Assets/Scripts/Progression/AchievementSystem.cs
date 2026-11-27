using System;
using System.Collections.Generic;

namespace DudeChaos.Progression
{
    // Landmine #5: GameFlow calls NotifySideJobs(int); Progression never references GameFlow.
    // Toast queue capped at 3; Restore then rescore (§1.8).
    public sealed class AchievementSystem
    {
        public const int MaxToastQueue = 3;

        private readonly Wallet _wallet;
        private readonly HashSet<string> _unlocked = new HashSet<string>();
        private readonly Queue<AchievementDefinition> _toastQueue = new Queue<AchievementDefinition>(MaxToastQueue);
        private int _sideJobsCompleted;

        public event Action<AchievementDefinition> Unlocked;

        public IReadOnlyCollection<string> UnlockedIds => _unlocked;
        public int ToastQueueCount => _toastQueue.Count;
        public int SideJobsCompleted => _sideJobsCompleted;

        public AchievementSystem(Wallet wallet)
        {
            _wallet = wallet ?? new Wallet();
        }

        public void NotifySideJobs(int completedCount)
        {
            if (completedCount > _sideJobsCompleted)
                _sideJobsCompleted = completedCount;
        }

        public void Evaluate(AchievementProgressSnapshot snapshot)
        {
            if (_sideJobsCompleted > snapshot.sideJobsCompleted)
                snapshot.sideJobsCompleted = _sideJobsCompleted;

            for (int i = 0; i < AchievementRules.All.Length; i++)
            {
                AchievementDefinition def = AchievementRules.All[i];
                if (_unlocked.Contains(def.id)) continue;
                if (AchievementRules.IsSatisfied(def.id, snapshot))
                {
                    _unlocked.Add(def.id);
                    _wallet.Add(def.rewardMoney);
                    if (_toastQueue.Count < MaxToastQueue)
                        _toastQueue.Enqueue(def);
                    Unlocked?.Invoke(def);
                }
            }
        }

        public bool TryDequeueToast(out AchievementDefinition toast)
        {
            if (_toastQueue.Count > 0)
            {
                toast = _toastQueue.Dequeue();
                return true;
            }
            toast = default;
            return false;
        }

        public void Restore(IEnumerable<string> unlockedIds, AchievementProgressSnapshot rescoreSnapshot)
        {
            _unlocked.Clear();
            _toastQueue.Clear();
            if (unlockedIds != null)
            {
                foreach (string id in unlockedIds)
                {
                    if (!string.IsNullOrEmpty(id))
                        _unlocked.Add(id);
                }
            }
            _sideJobsCompleted = Math.Max(_sideJobsCompleted, rescoreSnapshot.sideJobsCompleted);
            rescoreSnapshot.sideJobsCompleted = _sideJobsCompleted;
            Evaluate(rescoreSnapshot);
        }

        public string[] ExportUnlockedIds()
        {
            string[] arr = new string[_unlocked.Count];
            _unlocked.CopyTo(arr);
            return arr;
        }
    }
}
