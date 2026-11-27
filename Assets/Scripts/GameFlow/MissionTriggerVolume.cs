using DudeChaos.Data;
using DudeChaos.Progression;
using UnityEngine;

namespace DudeChaos.GameFlow
{
    public sealed class MissionTriggerVolume : MonoBehaviour
    {
        [SerializeField] private MissionData mission;
        [SerializeField] private MissionSystem missionSystem;

        public string TriggerId => mission != null ? mission.startTrigger : string.Empty;

        public void Configure(MissionData data, MissionSystem system)
        {
            mission = data;
            missionSystem = system;
        }

        public bool TryActivate()
        {
            if (mission == null || missionSystem == null)
                return false;
            if (missionSystem.CurrentRun.State == MissionRunState.Running)
                return false;
            return missionSystem.StartMission(mission);
        }
    }
}
