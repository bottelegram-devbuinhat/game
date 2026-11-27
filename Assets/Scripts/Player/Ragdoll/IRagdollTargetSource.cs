using UnityEngine;

namespace DudeChaos.Player
{
    public interface IRagdollTargetSource
    {
        Vector3 Position { get; }
        Transform PelvisTransform { get; }
        bool IsActiveRagdoll { get; }
        bool IsFullSimulation { get; }
        void SetFullSimulation(bool fullSim);
    }
}
