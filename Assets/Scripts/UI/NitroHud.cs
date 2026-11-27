using DudeChaos.Vehicles;
using UnityEngine;
using UnityEngine.UI;

namespace DudeChaos.UI
{
    public sealed class NitroHud : MonoBehaviour
    {
        [SerializeField] private Slider nitroBar;
        [SerializeField] private NitroSystem activeNitro;

        public void Bind(NitroSystem nitro) => activeNitro = nitro;

        public void Update()
        {
            if (nitroBar == null) return;
            nitroBar.gameObject.SetActive(activeNitro != null);
            if (activeNitro != null)
                nitroBar.value = activeNitro.NormalizedCharge;
        }
    }
}
