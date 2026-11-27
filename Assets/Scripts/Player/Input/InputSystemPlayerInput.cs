using UnityEngine;
using UnityEngine.InputSystem;

namespace DudeChaos.Player.Input
{
    // Polls Keyboard.current / Mouse.current / Gamepad.current directly; exposes virtual hooks for touch HUD (§1.8).
    // Landmine #3: Mouse state is read from Mouse.current, never Keyboard.
    public sealed class InputSystemPlayerInput : MonoBehaviour, IPlayerInput
    {
        private Vector2 _virtualMove;
        private Vector2 _virtualLook;
        private bool _virtualSprint;
        private bool _virtualJumpOnce;
        private bool _virtualCrouchOnce;
        private bool _virtualFireHeld;
        private bool _virtualFireOnce;
        private bool _virtualAimHeld;
        private bool _virtualReloadOnce;
        private bool _virtualMeleeOnce;
        private bool _virtualHeavyMeleeOnce;
        private bool _virtualInteractOnce;
        private bool _virtualPauseOnce;
        private bool _virtualNitroHeld;

        public Vector2 Move
        {
            get
            {
                Vector2 v = _virtualMove;
                Keyboard kb = Keyboard.current;
                if (kb != null)
                {
                    float x = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
                    float y = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);
                    Vector2 kbVec = new Vector2(x, y);
                    if (kbVec.sqrMagnitude > v.sqrMagnitude)
                        v = kbVec;
                }
                Gamepad gp = Gamepad.current;
                if (gp != null)
                {
                    Vector2 stick = gp.leftStick.ReadValue();
                    if (stick.sqrMagnitude > v.sqrMagnitude)
                        v = stick;
                }
                return Vector2.ClampMagnitude(v, 1f);
            }
        }

        public Vector2 Look
        {
            get
            {
                Vector2 look = _virtualLook;
                Mouse mouse = Mouse.current;
                if (mouse != null)
                {
                    look += mouse.delta.ReadValue() * 0.1f;
                }
                Gamepad gp = Gamepad.current;
                if (gp != null)
                {
                    look += gp.rightStick.ReadValue() * 2.5f;
                }
                return look;
            }
        }

        public bool SprintHeld =>
            _virtualSprint ||
            (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed) ||
            (Gamepad.current != null && Gamepad.current.leftStickButton.isPressed);

        public bool CrouchPressed =>
            Consume(ref _virtualCrouchOnce) ||
            (Keyboard.current != null && (Keyboard.current.cKey.wasPressedThisFrame || Keyboard.current.leftCtrlKey.wasPressedThisFrame)) ||
            (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame);

        public bool JumpPressed =>
            Consume(ref _virtualJumpOnce) ||
            (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);

        public bool FireHeld =>
            _virtualFireHeld ||
            (Mouse.current != null && Mouse.current.leftButton.isPressed) ||
            (Gamepad.current != null && Gamepad.current.rightTrigger.isPressed);

        public bool FirePressed =>
            Consume(ref _virtualFireOnce) ||
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.rightTrigger.wasPressedThisFrame);

        public bool AimHeld =>
            _virtualAimHeld ||
            (Mouse.current != null && Mouse.current.rightButton.isPressed) ||
            (Gamepad.current != null && Gamepad.current.leftTrigger.isPressed);

        public bool ReloadPressed =>
            Consume(ref _virtualReloadOnce) ||
            (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame);

        public bool MeleePressed =>
            Consume(ref _virtualMeleeOnce) ||
            (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.rightShoulder.wasPressedThisFrame);

        public bool HeavyMeleePressed =>
            Consume(ref _virtualHeavyMeleeOnce) ||
            (Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame);

        public bool InteractPressed =>
            Consume(ref _virtualInteractOnce) ||
            (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame);

        public bool PausePressed =>
            Consume(ref _virtualPauseOnce) ||
            (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame);

        public bool NitroHeld =>
            _virtualNitroHeld ||
            (Keyboard.current != null && Keyboard.current.qKey.isPressed) ||
            (Gamepad.current != null && Gamepad.current.leftShoulder.isPressed);

        public void SetVirtualMove(Vector2 move) => _virtualMove = Vector2.ClampMagnitude(move, 1f);
        public void SetVirtualLook(Vector2 look) => _virtualLook = look;
        public void SetVirtualSprint(bool held) => _virtualSprint = held;
        public void SetVirtualFire(bool held, bool pressedThisFrame = false)
        {
            _virtualFireHeld = held;
            if (pressedThisFrame) _virtualFireOnce = true;
        }
        public void TriggerVirtualJump() => _virtualJumpOnce = true;
        public void TriggerVirtualCrouch() => _virtualCrouchOnce = true;
        public void TriggerVirtualReload() => _virtualReloadOnce = true;
        public void TriggerVirtualMelee(bool heavy = false)
        {
            if (heavy) _virtualHeavyMeleeOnce = true;
            else _virtualMeleeOnce = true;
        }
        public void TriggerVirtualInteract() => _virtualInteractOnce = true;
        public void TriggerVirtualPause() => _virtualPauseOnce = true;
        public void SetVirtualNitro(bool held) => _virtualNitroHeld = held;

        private static bool Consume(ref bool flag)
        {
            if (!flag) return false;
            flag = false;
            return true;
        }
    }
}
