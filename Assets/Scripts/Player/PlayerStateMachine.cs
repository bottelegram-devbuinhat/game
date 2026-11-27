using System;
using System.Collections.Generic;
using DudeChaos.Data;

namespace DudeChaos.Player
{
    // Plain C# state machine with explicit transition table, ragdoll override rules and vehicle lockout (§1.8).
    // Landmine #13: InVehicle CANNOT transition directly to Ragdoll; ExitingVehicle CAN.
    public sealed class PlayerStateMachine
    {
        private static readonly HashSet<(PlayerState from, PlayerState to)> LegalTransitions = BuildTransitions();
        private static readonly HashSet<PlayerState> RagdollSources = new HashSet<PlayerState>
        {
            PlayerState.Idle,
            PlayerState.Walk,
            PlayerState.Run,
            PlayerState.Sprint,
            PlayerState.Crouch,
            PlayerState.Jump,
            PlayerState.Fall,
            PlayerState.Land,
            PlayerState.EnteringVehicle,
            PlayerState.ExitingVehicle,
            PlayerState.GettingUp
        };

        private PlayerState _current = PlayerState.Idle;
        private bool _vehicleLockout;

        public event Action<PlayerState, PlayerState> StateChanged;

        public PlayerState Current => _current;
        public bool IsInVehicleFlow =>
            _current == PlayerState.EnteringVehicle ||
            _current == PlayerState.InVehicle ||
            _current == PlayerState.ExitingVehicle;

        public bool IsLocomotionAllowed =>
            !_vehicleLockout &&
            _current != PlayerState.EnteringVehicle &&
            _current != PlayerState.InVehicle &&
            _current != PlayerState.ExitingVehicle &&
            _current != PlayerState.Ragdoll &&
            _current != PlayerState.GettingUp &&
            _current != PlayerState.Dead;

        public bool VehicleLockout
        {
            get => _vehicleLockout;
            set => _vehicleLockout = value;
        }

        public static bool CanRagdollFrom(PlayerState state) => RagdollSources.Contains(state);

        public static bool IsTransitionLegal(PlayerState from, PlayerState to)
        {
            if (from == to) return false;
            if (to == PlayerState.Ragdoll) return RagdollSources.Contains(from);
            return LegalTransitions.Contains((from, to));
        }

        public bool CanTransitionTo(PlayerState next)
        {
            if (_vehicleLockout && (next == PlayerState.EnteringVehicle || next == PlayerState.InVehicle))
                return false;
            return IsTransitionLegal(_current, next);
        }

        public bool TryTransition(PlayerState next)
        {
            if (!CanTransitionTo(next))
                return false;

            PlayerState prev = _current;
            _current = next;
            StateChanged?.Invoke(prev, _current);
            return true;
        }

        public bool TryEnterRagdoll(RagdollActivationReason reason)
        {
            if (!RagdollSources.Contains(_current))
                return false;
            PlayerState prev = _current;
            _current = PlayerState.Ragdoll;
            StateChanged?.Invoke(prev, _current);
            return true;
        }

        public bool ForceDie()
        {
            if (_current == PlayerState.Dead)
                return false;
            PlayerState prev = _current;
            _current = PlayerState.Dead;
            StateChanged?.Invoke(prev, _current);
            return true;
        }

        public void ResetToIdle()
        {
            PlayerState prev = _current;
            _current = PlayerState.Idle;
            _vehicleLockout = false;
            if (prev != _current)
                StateChanged?.Invoke(prev, _current);
        }

        private static HashSet<(PlayerState, PlayerState)> BuildTransitions()
        {
            var set = new HashSet<(PlayerState, PlayerState)>();
            PlayerState[] groundLocomotion =
            {
                PlayerState.Idle,
                PlayerState.Walk,
                PlayerState.Run,
                PlayerState.Sprint,
                PlayerState.Crouch,
                PlayerState.Land
            };

            foreach (var a in groundLocomotion)
            {
                foreach (var b in groundLocomotion)
                {
                    if (a != b) set.Add((a, b));
                }
                set.Add((a, PlayerState.Jump));
                set.Add((a, PlayerState.Fall));
                set.Add((a, PlayerState.EnteringVehicle));
                set.Add((a, PlayerState.Dead));
            }

            set.Add((PlayerState.Jump, PlayerState.Fall));
            set.Add((PlayerState.Jump, PlayerState.Land));
            set.Add((PlayerState.Jump, PlayerState.Idle));
            set.Add((PlayerState.Jump, PlayerState.Dead));

            set.Add((PlayerState.Fall, PlayerState.Land));
            set.Add((PlayerState.Fall, PlayerState.Idle));
            set.Add((PlayerState.Fall, PlayerState.Dead));

            set.Add((PlayerState.EnteringVehicle, PlayerState.InVehicle));
            set.Add((PlayerState.EnteringVehicle, PlayerState.Idle));
            set.Add((PlayerState.EnteringVehicle, PlayerState.Dead));

            // Landmine #13: InVehicle can ONLY go to ExitingVehicle or Dead (never directly to Ragdoll or Idle).
            set.Add((PlayerState.InVehicle, PlayerState.ExitingVehicle));
            set.Add((PlayerState.InVehicle, PlayerState.Dead));

            set.Add((PlayerState.ExitingVehicle, PlayerState.Idle));
            set.Add((PlayerState.ExitingVehicle, PlayerState.Fall));
            set.Add((PlayerState.ExitingVehicle, PlayerState.Dead));

            set.Add((PlayerState.Ragdoll, PlayerState.GettingUp));
            set.Add((PlayerState.Ragdoll, PlayerState.Dead));

            set.Add((PlayerState.GettingUp, PlayerState.Idle));
            set.Add((PlayerState.GettingUp, PlayerState.Walk));
            set.Add((PlayerState.GettingUp, PlayerState.Dead));

            return set;
        }
    }
}
