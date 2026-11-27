using DudeChaos.Data;
using DudeChaos.Progression;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DudeChaos.UI
{
    public sealed class ConsumableHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text itemNameText;
        [SerializeField] private TMP_Text countText;
        [SerializeField] private Slider useProgressSlider;

        public void Display(EquipmentData item, int count, float useProgress01)
        {
            if (itemNameText != null)
                itemNameText.text = item != null ? item.displayName : "";
            if (countText != null)
            {
                int cap = item != null ? ConsumableRules.Evaluate(item).carryLimit : 0;
                countText.text = item != null ? $"{count}/{cap}" : "";
            }
            if (useProgressSlider != null)
                useProgressSlider.value = Mathf.Clamp01(useProgress01);
        }
    }
}
