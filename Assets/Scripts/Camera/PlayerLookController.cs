using DudeChaos.Player;
using UnityEngine;

namespace DudeChaos.CameraRig
{
    public sealed class PlayerLookController : MonoBehaviour
    {
        [SerializeField] private ThirdPersonCameraRig cameraRig;
        [SerializeField] private float lookSensitivity = 1.0f;
        [SerializeField] private bool invertY = false;

        private IPlayerInput _input;

        public float LookSensitivity
        {
            get => lookSensitivity;
            set => lookSensitivity = Mathf.Clamp(value, 0.1f, 5f);
        }

        public bool InvertY
        {
            get => invertY;
            set => invertY = value;
        }

        public void Awake()
        {
            if (cameraRig == null) cameraRig = GetComponent<ThirdPersonCameraRig>();
            _input = GetComponent<IPlayerInput>();
        }

        public void Bind(IPlayerInput input, ThirdPersonCameraRig rig)
        {
            _input = input;
            cameraRig = rig;
        }

        public void Tick()
        {
            if (_input == null || cameraRig == null) return;
            Vector2 delta = _input.Look * lookSensitivity;
            float pitchDelta = invertY ? delta.y : -delta.y;
            cameraRig.AddLookDelta(delta.x, pitchDelta);
        }

        public void Update() => Tick();
    }
}
