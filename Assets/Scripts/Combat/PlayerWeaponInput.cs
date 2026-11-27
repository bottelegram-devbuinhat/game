using DudeChaos.Player;
using UnityEngine;

namespace DudeChaos.Combat
{
    // Landmine #12: Glue component bridging IPlayerInput -> WeaponController.TryFire & auto-reload.
    public sealed class PlayerWeaponInput : MonoBehaviour
    {
        [SerializeField] private WeaponController weaponController;
        [SerializeField] private MeleeController meleeController;

        private IPlayerInput _input;
        private PlayerMotor _motor;

        public void Awake()
        {
            if (weaponController == null) weaponController = GetComponent<WeaponController>();
            if (meleeController == null) meleeController = GetComponent<MeleeController>();
            _input = GetComponent<IPlayerInput>();
            _motor = GetComponent<PlayerMotor>();
        }

        public void Bind(IPlayerInput input, WeaponController weapon, MeleeController melee = null)
        {
            _input = input;
            weaponController = weapon;
            meleeController = melee;
        }

        public void Tick()
        {
            if (_input == null) _input = GetComponent<IPlayerInput>();
            if (_input == null) return;
            if (_motor != null && !_motor.StateMachine.IsLocomotionAllowed)
                return;

            if (meleeController != null)
            {
                if (_input.HeavyMeleePressed)
                {
                    meleeController.TrySwing(true);
                    return;
                }
                if (_input.MeleePressed)
                {
                    meleeController.TrySwing(false);
                    return;
                }
            }

            if (weaponController == null || weaponController.CurrentWeapon == null)
                return;

            if (_input.ReloadPressed)
            {
                weaponController.StartReload();
                return;
            }

            bool fired = weaponController.TryFire(_input.FireHeld, _input.FirePressed);
            if (!fired && (_input.FirePressed || _input.FireHeld) && weaponController.AmmoInMagazine <= 0 && !weaponController.IsReloading)
            {
                weaponController.StartReload();
            }
        }

        public void Update() => Tick();
    }
}
