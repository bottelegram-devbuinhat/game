using UnityEngine;

namespace DudeChaos.Visuals
{
    // Procedural humanoid walk when no Animator clip overrides; skips missing bones (§1.8 & §3.9).
    public sealed class ProceduralWalker : MonoBehaviour
    {
        [SerializeField] private Animator overrideAnimator;
        [SerializeField] private Transform hips;
        [SerializeField] private Transform leftThigh;
        [SerializeField] private Transform rightThigh;
        [SerializeField] private Transform leftCalf;
        [SerializeField] private Transform rightCalf;
        [SerializeField] private Transform leftArm;
        [SerializeField] private Transform rightArm;

        private float _phase;
        private Vector3 _baseHipLocalPos;
        private bool _capturedBase;

        public void Step(float speedMetersPerSecond, float deltaTime)
        {
            if (overrideAnimator != null && overrideAnimator.runtimeAnimatorController != null && overrideAnimator.enabled)
                return;

            if (hips != null && !_capturedBase)
            {
                _baseHipLocalPos = hips.localPosition;
                _capturedBase = true;
            }

            if (speedMetersPerSecond > 0.05f)
                _phase += speedMetersPerSecond * 2.1f * Mathf.Max(0f, deltaTime);
            else
                _phase = 0f;

            WalkPoseAngles pose = AnimationMath.EvaluateWalkPose(speedMetersPerSecond, _phase);

            if (hips != null)
                hips.localPosition = _baseHipLocalPos + new Vector3(0f, pose.hipBobOffset, 0f);
            if (leftThigh != null)
                leftThigh.localRotation = Quaternion.Euler(pose.leftThighPitch, 0f, 0f);
            if (rightThigh != null)
                rightThigh.localRotation = Quaternion.Euler(pose.rightThighPitch, 0f, 0f);
            if (leftCalf != null)
                leftCalf.localRotation = Quaternion.Euler(pose.leftCalfPitch, 0f, 0f);
            if (rightCalf != null)
                rightCalf.localRotation = Quaternion.Euler(pose.rightCalfPitch, 0f, 0f);
            if (leftArm != null)
                leftArm.localRotation = Quaternion.Euler(pose.leftArmPitch, 0f, 0f);
            if (rightArm != null)
                rightArm.localRotation = Quaternion.Euler(pose.rightArmPitch, 0f, 0f);
        }
    }
}
