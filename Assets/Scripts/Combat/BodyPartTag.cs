using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.Combat
{
    // Maps bone/collider -> BodyPart + inspector-assigned damage target + impact material (§1.8).
    // Note: Unity cannot GetComponentInParent<TInterface>(), so damageTarget is a serialized MonoBehaviour.
    public sealed class BodyPartTag : MonoBehaviour
    {
        [SerializeField] private BodyPart bodyPart = BodyPart.Torso;
        [SerializeField] private MonoBehaviour damageTarget;
        [SerializeField] private ImpactMaterialData impactMaterial;

        public BodyPart Part => bodyPart;
        public IDamageable Damageable => damageTarget as IDamageable;
        public ImpactMaterialData ImpactMaterial => impactMaterial;

        public void Configure(BodyPart part, MonoBehaviour target, ImpactMaterialData material = null)
        {
            bodyPart = part;
            damageTarget = target;
            impactMaterial = material;
        }
    }
}
