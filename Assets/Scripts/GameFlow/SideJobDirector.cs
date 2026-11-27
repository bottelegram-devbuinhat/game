using System;
using DudeChaos.City;
using DudeChaos.Progression;
using UnityEngine;

namespace DudeChaos.GameFlow
{
    // Landmine #5: GameFlow calls AchievementSystem.NotifySideJobs(int); Progression never knows GameFlow exists.
    public sealed class SideJobDirector : MonoBehaviour
    {
        private Wallet _wallet;
        private AchievementSystem _achievements;
        private int _completed;
        private int _failed;

        public event Action<bool, int> JobResolved;

        public int CompletedCount => _completed;
        public int FailedCount => _failed;

        public void Bind(Wallet wallet, AchievementSystem achievements = null)
        {
            _wallet = wallet;
            _achievements = achievements;
        }

        public void CompleteJob(SideJobOffer offer)
        {
            _completed++;
            _wallet?.Add(offer.rewardMoney);
            _achievements?.NotifySideJobs(_completed);
            JobResolved?.Invoke(true, offer.rewardMoney);
        }

        public void FailJob()
        {
            _failed++;
            JobResolved?.Invoke(false, 0);
        }

        public void RestoreCounts(int completed, int failed)
        {
            _completed = Mathf.Max(0, completed);
            _failed = Mathf.Max(0, failed);
            _achievements?.NotifySideJobs(_completed);
        }
    }
}
