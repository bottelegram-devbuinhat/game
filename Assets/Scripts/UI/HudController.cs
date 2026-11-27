using DudeChaos.Combat;
using DudeChaos.Data;
using DudeChaos.Player;
using DudeChaos.Progression;
using DudeChaos.Wanted;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DudeChaos.UI
{
    public sealed class HudController : MonoBehaviour
    {
        [SerializeField] private Slider healthBar;
        [SerializeField] private Slider armourBar;
        [SerializeField] private TMP_Text wantedText;
        [SerializeField] private TMP_Text ammoText;
        [SerializeField] private TMP_Text moneyText;
        [SerializeField] private TMP_Text objectiveText;
        [SerializeField] private TMP_Text clockText;
        [SerializeField] private TMP_Text fpsText;
        [SerializeField] private Image hitMarker;

        private float _hitMarkerTimer;
        private bool _showFps;

        public void SetShowFps(bool show)
        {
            _showFps = show;
            if (fpsText != null) fpsText.gameObject.SetActive(show);
        }

        public void BindHealth(HealthSystem health)
        {
            if (health == null) return;
            health.Changed += UpdateVitals;
            UpdateVitals(health.CurrentHealth, health.CurrentArmour);
        }

        public void BindWeapon(WeaponController weapon)
        {
            if (weapon == null) return;
            weapon.AmmoChanged += UpdateAmmo;
            weapon.ShotResolvedUi += OnShotResolved;
            UpdateAmmo(weapon.AmmoInMagazine, weapon.MaxMagazineSize);
        }

        public void BindWanted(WantedSystem wanted)
        {
            if (wanted == null) return;
            wanted.LevelChanged += (_, lvl) => UpdateWanted(lvl);
            UpdateWanted(wanted.Level);
        }

        public void BindWallet(Wallet wallet)
        {
            if (wallet == null) return;
            wallet.Changed += UpdateMoney;
            UpdateMoney(wallet.Balance);
        }

        public void UpdateVitals(float hp, float armour)
        {
            if (healthBar != null) healthBar.value = hp;
            if (armourBar != null) armourBar.value = armour;
        }

        public void UpdateWanted(int stars)
        {
            if (wantedText != null)
                wantedText.text = stars > 0 ? new string('*', Mathf.Clamp(stars, 0, 6)) : "";
        }

        public void UpdateAmmo(int inMag, int maxMag)
        {
            if (ammoText != null)
                ammoText.text = maxMag > 0 ? $"{inMag}/{maxMag}" : "--";
        }

        public void UpdateMoney(int dollars)
        {
            if (moneyText != null)
                moneyText.text = $"${dollars:N0}";
        }

        public void UpdateObjective(string objective)
        {
            if (objectiveText != null)
                objectiveText.text = objective ?? "";
        }

        public void UpdateCityClock(float hour)
        {
            if (clockText == null) return;
            int h = Mathf.FloorToInt(hour) % 24;
            int m = Mathf.FloorToInt((hour - Mathf.Floor(hour)) * 60f);
            clockText.text = $"{h:D2}:{m:D2}";
        }

        private void OnShotResolved(bool didHit, BodyPart part)
        {
            if (!didHit || hitMarker == null) return;
            _hitMarkerTimer = 0.12f;
            hitMarker.enabled = true;
            hitMarker.color = part == BodyPart.Head ? Color.red : Color.white;
        }

        public void Update()
        {
            if (_hitMarkerTimer > 0f)
            {
                _hitMarkerTimer -= Time.unscaledDeltaTime;
                if (_hitMarkerTimer <= 0f && hitMarker != null)
                    hitMarker.enabled = false;
            }

            if (_showFps && fpsText != null && Time.unscaledDeltaTime > 0.0001f)
            {
                fpsText.text = $"{Mathf.RoundToInt(1f / Time.unscaledDeltaTime)} FPS";
            }
        }
    }
}
