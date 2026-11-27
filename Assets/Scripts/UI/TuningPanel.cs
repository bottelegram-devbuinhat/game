using DudeChaos.Progression;
using DudeChaos.Vehicles;
using TMPro;
using UnityEngine;

namespace DudeChaos.UI
{
    public sealed class TuningPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text statusText;
        private readonly int[] _levels = new int[6];

        public int GetLevel(TuningCategory cat) => _levels[(int)cat];

        public bool TryUpgrade(TuningCategory category, Wallet wallet)
        {
            if (wallet == null) return false;
            int cur = _levels[(int)category];
            int cost = TuningRules.GetUpgradeCost(category, cur);
            if (cost <= 0 || !wallet.TrySpend(cost))
                return false;
            _levels[(int)category] = cur + 1;
            if (statusText != null)
                statusText.text = $"{category} Lv.{_levels[(int)category]}";
            return true;
        }
    }
}
