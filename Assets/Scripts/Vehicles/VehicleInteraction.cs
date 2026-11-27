using DudeChaos.Player;
using UnityEngine;

namespace DudeChaos.Vehicles
{
    // Landmines #12 & #19: E within 3.5 m via static non-alloc buffer; clears _current ONLY when TryExit() returns true.
    public sealed class VehicleInteraction : MonoBehaviour
    {
        public const float InteractRadius = 3.5f;
        private static readonly Collider[] OverlapBuffer = new Collider[16];

        [SerializeField] private PlayerMotor playerMotor;
        private IPlayerInput _input;
        private VehicleController _current;

        public VehicleController CurrentVehicle => _current;

        public void Awake()
        {
            if (playerMotor == null) playerMotor = GetComponent<PlayerMotor>();
            _input = GetComponent<IPlayerInput>();
        }

        public void Bind(PlayerMotor motor, IPlayerInput input, VehicleController startingVehicle = null)
        {
            playerMotor = motor;
            _input = input;
            _current = startingVehicle;
        }

        public bool TryInteract(VehicleController candidateOverride = null)
        {
            if (playerMotor == null) return false;

            if (_current != null)
            {
                bool exited = _current.TryExit();
                // Landmine #19: Clear _current ONLY on successful exit!
                if (exited)
                {
                    _current = null;
                }
                return exited;
            }

            VehicleController target = candidateOverride ?? FindNearestVehicle();
            if (target == null) return false;

            if (target.TryEnter(playerMotor))
            {
                _current = target;
                return true;
            }
            return false;
        }

        public VehicleController FindNearestVehicle()
        {
            int count = Physics.OverlapSphereNonAlloc(transform.position, InteractRadius, OverlapBuffer, -1, QueryTriggerInteraction.Ignore);
            VehicleController best = null;
            float bestDistSqr = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                Collider col = OverlapBuffer[i];
                if (col == null) continue;
                if (!col.TryGetComponent(out VehicleController vc)) continue;
                if (vc.HasDriver) continue;
                float dSqr = (vc.transform.position - transform.position).sqrMagnitude;
                if (dSqr < bestDistSqr)
                {
                    bestDistSqr = dSqr;
                    best = vc;
                }
            }
            return best;
        }

        public void Update()
        {
            if (_input == null) _input = GetComponent<IPlayerInput>();
            if (_input != null && _input.InteractPressed)
            {
                TryInteract();
            }
        }
    }
}
