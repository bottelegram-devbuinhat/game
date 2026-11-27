using DudeChaos.Data;
using DudeChaos.Progression;
using TMPro;
using UnityEngine;

namespace DudeChaos.UI
{
    // §1.8: Name, current objective, 2/3 progress, clock turns red <= 30 s, fail banner + retry hint,
    // star banner + missing-star hint for 5 s.
    public sealed class MissionHud : MonoBehaviour
    {
        public const float RedClockThresholdSeconds = 30f;
        public const float BannerDisplaySeconds = 5f;

        [SerializeField] private TMP_Text missionTitleText;
        [SerializeField] private TMP_Text objectiveProgressText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text bannerText;
        [SerializeField] private TMP_Text subHintText;

        private MissionSystem _missionSystem;
        private float _bannerTimer;

        public void Bind(MissionSystem system)
        {
            if (_missionSystem != null)
            {
                _missionSystem.MissionCompleted -= OnCompleted;
                _missionSystem.MissionFailed -= OnFailed;
            }
            _missionSystem = system;
            if (_missionSystem != null)
            {
                _missionSystem.MissionCompleted += OnCompleted;
                _missionSystem.MissionFailed += OnFailed;
            }
        }

        public void Refresh()
        {
            if (_missionSystem == null) return;
            MissionRun run = _missionSystem.CurrentRun;
            if (run.State == MissionRunState.Running && run.Mission != null)
            {
                if (missionTitleText != null)
                    missionTitleText.text = run.Mission.title;
                if (objectiveProgressText != null)
                    objectiveProgressText.text = $"{run.CurrentObjectiveText} ({run.CompletedStepCount}/{run.TotalSteps})";
                if (timerText != null)
                {
                    float left = run.TimeLeft;
                    int sec = Mathf.CeilToInt(left);
                    timerText.text = run.TimeLimitSeconds > 0f ? $"{sec / 60}:{sec % 60:D2}" : "";
                    timerText.color = (run.TimeLimitSeconds > 0f && left <= RedClockThresholdSeconds) ? Color.red : Color.white;
                }
            }
        }

        private void OnCompleted(MissionData mission, MissionGradeResult grade)
        {
            _bannerTimer = BannerDisplaySeconds;
            if (bannerText != null)
                bannerText.text = $"{mission.title} — {grade.stars} STAR(S) (+${grade.totalMoneyPayout})";
            if (subHintText != null)
                subHintText.text = grade.nextStarHint ?? "";
        }

        private void OnFailed(MissionData mission, MissionFailReason reason)
        {
            _bannerTimer = BannerDisplaySeconds;
            if (bannerText != null)
                bannerText.text = $"MISSION FAILED ({reason})";
            if (subHintText != null)
                subHintText.text = "Retry available";
        }

        public void Update()
        {
            Refresh();
            if (_bannerTimer > 0f)
            {
                _bannerTimer -= Time.unscaledDeltaTime;
                if (_bannerTimer <= 0f)
                {
                    if (bannerText != null) bannerText.text = "";
                    if (subHintText != null) subHintText.text = "";
                }
            }
        }
    }
}
