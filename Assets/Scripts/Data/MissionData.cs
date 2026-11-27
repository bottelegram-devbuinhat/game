using UnityEngine;

namespace DudeChaos.Data
{
    [CreateAssetMenu(fileName = "MIS_New", menuName = "DudeChaos/Data/Mission")]
    public class MissionData : ScriptableObject
    {
        public string missionId = "";
        public string title = "";
        public string[] objectives = new string[0];
        public int rewardsMoney = 750;
        public int rewardsReputation = 10;
        public string startTrigger = "";
        public string[] completionConditions = new string[0];
        public string[] dialogueLines = new string[3];
        public float timeLimitSeconds = 300f;
        public int failAtWantedLevel = 3;
        public bool retryable = true;
        public float timeBonusPerSecond = 0.5f;
    }
}
