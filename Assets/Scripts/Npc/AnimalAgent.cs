using UnityEngine;

namespace DudeChaos.Npc
{
    public sealed class AnimalAgent : MonoBehaviour
    {
        private AnimalSpec _spec;
        private int _seed;

        public AnimalSpecies Species => _spec.species;

        public void Initialise(AnimalSpecies species, int seed, Vector3 pos)
        {
            _spec = AnimalRules.GetSpec(species);
            _seed = seed;
            if (_spec.flies) pos.y = Mathf.Max(2.2f, pos.y);
            transform.position = pos;
            gameObject.SetActive(true);
        }

        public void Step(Vector3 playerPos, float deltaTime, float worldTime)
        {
            float dt = Mathf.Max(0f, deltaTime);
            Vector3 away = transform.position - playerPos;
            float planarDist = new Vector2(away.x, away.z).magnitude;

            Vector3 dir;
            float speed;
            if (planarDist < _spec.fearRadius && planarDist > 0.01f)
            {
                dir = new Vector3(away.x, 0f, away.z).normalized;
                speed = _spec.fleeSpeed;
            }
            else
            {
                dir = NpcSteering.DeterministicWanderDirection(_seed, worldTime);
                speed = _spec.wanderSpeed;
            }

            Vector3 next = transform.position + dir * (speed * dt);
            if (_spec.flies)
            {
                next.y = 3.0f + Mathf.Sin(worldTime * 2.2f + _seed) * 0.6f;
            }
            transform.position = next;
        }
    }
}
