using System.Collections.Generic;
using DudeChaos.Data;

namespace DudeChaos.Progression
{
    // Line 0 = intro, Line 1 = mid-run / fail, Line 2 = payoff; remembers last 3 lines (§1.8).
    public sealed class MissionBanterRules
    {
        private readonly Queue<string> _recent = new Queue<string>(3);

        public IReadOnlyCollection<string> RecentLines => _recent;

        public string GetIntroLine(MissionData mission) => SelectAndRemember(mission, 0);
        public string GetMidOrFailLine(MissionData mission, MissionFailReason reason = MissionFailReason.None) => SelectAndRemember(mission, 1);
        public string GetPayoffLine(MissionData mission) => SelectAndRemember(mission, 2);

        private string SelectAndRemember(MissionData mission, int index)
        {
            if (mission == null || mission.dialogueLines == null || index < 0 || index >= mission.dialogueLines.Length)
                return string.Empty;
            string line = mission.dialogueLines[index] ?? string.Empty;
            if (!string.IsNullOrEmpty(line))
            {
                if (_recent.Count >= 3)
                    _recent.Dequeue();
                _recent.Enqueue(line);
            }
            return line;
        }
    }
}
