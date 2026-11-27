using DudeChaos.Progression;
using TMPro;
using UnityEngine;

namespace DudeChaos.UI
{
    public sealed class StuntHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text pointsText;
        [SerializeField] private TMP_Text comboText;

        private StuntSystem _stunts;

        public void Bind(StuntSystem stunts) => _stunts = stunts;

        public void Update()
        {
            if (_stunts == null) return;
            bool active = _stunts.PendingPoints > 1f;
            if (pointsText != null)
                pointsText.text = active ? $"{Mathf.RoundToInt(_stunts.PendingPoints)} PTS" : "";
            if (comboText != null)
                comboText.text = active && _stunts.Combo > 1 ? $"x{_stunts.Combo}" : "";
        }
    }
}
