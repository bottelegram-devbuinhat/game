using DudeChaos.Progression;
using TMPro;
using UnityEngine;

namespace DudeChaos.UI
{
    public sealed class AchievementToast : MonoBehaviour
    {
        [SerializeField] private GameObject toastContainer;
        [SerializeField] private TMP_Text toastText;
        [SerializeField] private float displayDuration = 3.0f;

        private AchievementSystem _achievements;
        private float _timer;

        public void Bind(AchievementSystem achievements) => _achievements = achievements;

        public void Update()
        {
            if (_achievements == null) return;
            if (_timer > 0f)
            {
                _timer -= Time.unscaledDeltaTime;
                if (_timer <= 0f && toastContainer != null)
                    toastContainer.SetActive(false);
                return;
            }

            if (_achievements.TryDequeueToast(out AchievementDefinition def))
            {
                _timer = displayDuration;
                if (toastContainer != null) toastContainer.SetActive(true);
                if (toastText != null) toastText.text = $"UNLOCKED: {def.title} (+${def.rewardMoney})";
            }
        }
    }
}
