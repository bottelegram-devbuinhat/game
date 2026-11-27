using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.Player
{
    public sealed class PelvisFollowTarget : MonoBehaviour
    {
        [SerializeField] private RagdollController ragdoll;
        [SerializeField] private RagdollSettings settings;
        [SerializeField] private Transform standingAnchor;

        private Vector3 _velocity;

        public float LagSeconds => settings != null ? settings.pelvisFollowLag : 0.08f;

        public void Configure(RagdollController controller, Transform standing, RagdollSettings ragdollSettings)
        {
            ragdoll = controller;
            standingAnchor = standing;
            settings = ragdollSettings;
        }

        public void Step(float deltaTime)
        {
            Transform source = (ragdoll != null && ragdoll.IsActiveRagdoll)
                ? ragdoll.PelvisTransform
                : (standingAnchor != null ? standingAnchor : transform);

            if (source == null) return;
            transform.position = Vector3.SmoothDamp(
                transform.position,
                source.position,
                ref _velocity,
                LagSeconds,
                float.PositiveInfinity,
                Mathf.Max(0.0001f, deltaTime));
        }

        public void LateUpdate()
        {
            Step(Time.deltaTime);
        }
    }
}
