using UnityEngine;

namespace DudeChaos.Player
{
    public interface IPlayerInput
    {
        Vector2 Move { get; }
        Vector2 Look { get; }
        bool SprintHeld { get; }
        bool CrouchPressed { get; }
        bool JumpPressed { get; }
        bool FireHeld { get; }
        bool FirePressed { get; }
        bool AimHeld { get; }
        bool ReloadPressed { get; }
        bool MeleePressed { get; }
        bool HeavyMeleePressed { get; }
        bool InteractPressed { get; }
        bool PausePressed { get; }
        bool NitroHeld { get; }

        void SetVirtualMove(Vector2 move);
        void SetVirtualLook(Vector2 look);
    }
}
