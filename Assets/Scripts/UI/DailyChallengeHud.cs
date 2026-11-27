using DudeChaos.Progression;
using TMPro;
using UnityEngine;

namespace DudeChaos.UI
{
    public sealed class DailyChallengeHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text[] slotLabels = new TMP_Text[3];
        [SerializeField] private TMP_Text streakLabel;

        private DailyChallengeSystem _dailies;

        public void Bind(DailyChallengeSystem system)
        {
            if (_dailies != null) _dailies.Updated -= Refresh;
            _dailies = system;
            if (_dailies != null)
            {
                _dailies.Updated += Refresh;
                Refresh();
            }
        }

        public void Refresh()
        {
            if (_dailies == null) return;
            for (int i = 0; i < slotLabels.Length && i < _dailies.Specs.Length; i++)
            {
                if (slotLabels[i] == null) continue;
                var spec = _dailies.Specs[i];
                slotLabels[i].text = $"{spec.kind}: {_dailies.Progress[i]}/{spec.target} (${spec.rewardMoney})";
            }
            if (streakLabel != null)
                streakLabel.text = $"Streak: {_dailies.Streak}d";
        }
    }
}
