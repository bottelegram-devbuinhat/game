using DudeChaos.Data;
using DudeChaos.Progression;
using DudeChaos.Wanted;
using UnityEngine;

namespace DudeChaos.GameFlow
{
    // Event-only daily challenge bridge — NO Update() scan (§2.8 & P7).
    public sealed class ChallengeBinder : MonoBehaviour
    {
        private DailyChallengeSystem _dailies;
        private StuntSystem _stunts;
        private BountySystem _bounty;
        private SideJobDirector _sideJobs;
        private MissionSystem _missions;
        private WantedSystem _wanted;

        public void Bind(
            DailyChallengeSystem dailies,
            StuntSystem stunts,
            BountySystem bounty,
            SideJobDirector sideJobs,
            MissionSystem missions,
            WantedSystem wanted)
        {
            UnbindAll();
            _dailies = dailies;
            _stunts = stunts;
            _bounty = bounty;
            _sideJobs = sideJobs;
            _missions = missions;
            _wanted = wanted;

            if (_stunts != null) _stunts.StuntBanked += OnStuntBanked;
            if (_bounty != null) _bounty.Awarded += OnBountyAwarded;
            if (_sideJobs != null) _sideJobs.JobResolved += OnSideJobResolved;
            if (_missions != null) _missions.MissionCompleted += OnMissionCompleted;
            if (_wanted != null) _wanted.LevelChanged += OnWantedChanged;
        }

        public void OnDestroy() => UnbindAll();

        private void UnbindAll()
        {
            if (_stunts != null) _stunts.StuntBanked -= OnStuntBanked;
            if (_bounty != null) _bounty.Awarded -= OnBountyAwarded;
            if (_sideJobs != null) _sideJobs.JobResolved -= OnSideJobResolved;
            if (_missions != null) _missions.MissionCompleted -= OnMissionCompleted;
            if (_wanted != null) _wanted.LevelChanged -= OnWantedChanged;
        }

        private void OnStuntBanked(int score, int money) => _dailies?.ReportActivity(DailyActivityKind.StuntScore, score);
        private void OnBountyAwarded(int amount, CrimeType crime) => _dailies?.ReportActivity(DailyActivityKind.BountyMoney, amount);
        private void OnSideJobResolved(bool won, int pay)
        {
            if (won) _dailies?.ReportActivity(DailyActivityKind.SideJobs, 1);
        }
        private void OnMissionCompleted(MissionData m, MissionGradeResult g) => _dailies?.ReportActivity(DailyActivityKind.Missions, 1);
        private void OnWantedChanged(int prev, int curr)
        {
            if (prev > 0 && curr == 0)
                _dailies?.ReportActivity(DailyActivityKind.EvadePolice, 1);
        }
    }
}
