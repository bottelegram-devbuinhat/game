using UnityEngine;

namespace DudeChaos.City
{
    // Teleports to interior island; disables CharacterController during position change (§1.8).
    public sealed class InteriorPortal : MonoBehaviour
    {
        [SerializeField] private Transform destinationAnchor;
        [SerializeField] private bool requiresPass;
        [SerializeField] private string requiredPassItemId = "";

        public bool RequiresPass => requiresPass;

        public void Configure(Transform destination, bool needsPass = false, string passId = "")
        {
            destinationAnchor = destination;
            requiresPass = needsPass;
            requiredPassItemId = passId ?? "";
        }

        public bool TryTeleport(GameObject actor, bool hasPass)
        {
            if (actor == null || destinationAnchor == null)
                return false;
            if (requiresPass && !hasPass)
                return false;

            CharacterController cc = actor.GetComponent<CharacterController>();
            bool wasEnabled = cc != null && cc.enabled;
            if (cc != null) cc.enabled = false;

            actor.transform.position = destinationAnchor.position;
            actor.transform.rotation = destinationAnchor.rotation;

            if (cc != null) cc.enabled = wasEnabled;
            return true;
        }
    }
}
