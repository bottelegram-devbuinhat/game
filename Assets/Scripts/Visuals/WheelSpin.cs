using System;
using UnityEngine;

namespace DudeChaos.Visuals
{
    public sealed class WheelSpin : MonoBehaviour
    {
        [SerializeField] private Transform[] wheels = Array.Empty<Transform>();
        [SerializeField] private float wheelRadius = 0.34f;

        public void Step(float signedForwardSpeed, float deltaTime)
        {
            if (wheels == null || wheels.Length == 0) return;
            float deg = AnimationMath.WheelSpinDeltaDegrees(signedForwardSpeed * deltaTime, wheelRadius);
            if (Mathf.Abs(deg) < 0.001f) return;
            Quaternion deltaRot = Quaternion.Euler(deg, 0f, 0f);
            for (int i = 0; i < wheels.Length; i++)
            {
                if (wheels[i] != null)
                    wheels[i].localRotation = wheels[i].localRotation * deltaRot;
            }
        }
    }
}
