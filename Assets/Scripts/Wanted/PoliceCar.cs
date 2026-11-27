using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.Wanted
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PoliceCar : MonoBehaviour
    {
        [SerializeField] private float maxSpeed = 38f;
        [SerializeField] private float acceleration = 16f;
        [SerializeField] private float brakeForce = 30f;
        [SerializeField] private float maxSteerAngle = 36f;

        private Rigidbody _rb;
        private float _stuckTimer;
        private float _reverseTimer;
        private int _wantedLevel;

        public void Awake()
        {
            _rb = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
            _rb.centerOfMass = new Vector3(0f, -0.30f, 0f);
        }

        public void SetWantedLevel(int level) => _wantedLevel = level;

        public void StepPursuit(Vector3 targetPos, float fixedDeltaTime)
        {
            if (_rb == null || _wantedLevel <= 0) return;
            float dt = Mathf.Max(0f, fixedDeltaTime);
            Vector3 toTarget = targetPos - transform.position;
            float dist = toTarget.magnitude;
            float speed = _rb.linearVelocity.magnitude;

            if (_reverseTimer > 0f)
            {
                _reverseTimer -= dt;
                _rb.AddForce(-transform.forward * (acceleration * 0.8f), ForceMode.Acceleration);
                transform.rotation = transform.rotation * Quaternion.Euler(0f, -45f * dt, 0f);
                return;
            }

            float steer = PoliceChaseMath.ComputeSteerInput(transform.forward, toTarget, maxSteerAngle);
            bool allowRam = _wantedLevel >= 3;
            bool brake = PoliceChaseMath.ShouldBrake(dist, speed, allowRam);

            if (brake)
            {
                _rb.linearVelocity = Vector3.MoveTowards(_rb.linearVelocity, Vector3.zero, brakeForce * dt);
                _stuckTimer = 0f;
            }
            else
            {
                if (speed < maxSpeed)
                    _rb.AddForce(transform.forward * acceleration, ForceMode.Acceleration);

                if (speed < 0.6f)
                    _stuckTimer += dt;
                else
                    _stuckTimer = 0f;

                if (PoliceChaseMath.IsWedged(1f, speed, _stuckTimer))
                {
                    _stuckTimer = 0f;
                    _reverseTimer = 1.1f;
                }
            }

            if (speed > 0.5f)
            {
                transform.rotation = transform.rotation * Quaternion.Euler(0f, steer * maxSteerAngle * 2.2f * dt, 0f);
            }
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (collision == null || collision.collider == null || _rb == null) return;
            float speed = _rb.linearVelocity.magnitude;
            if (speed < 8f) return;
            if (collision.collider.TryGetComponent(out IDamageable target))
            {
                target.ApplyDamage(new DamageInfo(
                    speed * 2.5f,
                    BodyPart.Torso,
                    transform.position,
                    transform.forward,
                    speed * 18f,
                    RagdollActivationReason.VehicleImpact,
                    gameObject));
            }
        }
    }
}
