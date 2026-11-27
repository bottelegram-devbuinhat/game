using DudeChaos.City;
using TMPro;
using UnityEngine;

namespace DudeChaos.UI
{
    public sealed class SideMissionBoard : MonoBehaviour
    {
        [SerializeField] private TMP_Text offerDescriptionText;
        private SideJobOffer _currentOffer;

        public SideJobOffer CurrentOffer => _currentOffer;

        public void ShowOffer(SideJobOffer offer)
        {
            _currentOffer = offer;
            if (offerDescriptionText != null)
                offerDescriptionText.text = $"{offer.kind} — ${offer.rewardMoney} ({Mathf.RoundToInt(offer.timeLimitSeconds)}s)";
        }
    }
}
