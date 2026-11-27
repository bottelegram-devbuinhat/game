using DudeChaos.Data;
using UnityEngine;

namespace DudeChaos.Progression
{
    public enum MissionRunState
    {
        Idle = 0,
        Running = 1,
        Completed = 2,
        Failed = 3
    }

    public enum MissionFailReason
    {
        None = 0,
        Timeout = 1,
        PlayerDied = 2,
        WantedTooHigh = 3,
        Abandoned = 4
    }

    // Landmine #14: Uses its own _nextIndex (NEVER _done.Count on a HashSet), so duplicate objective keys work.
    public sealed class MissionRun
    {
        public const int MaxRetries = 3;
        public const float MaxTimeBonusRatio = 0.34f;

        private MissionData _mission;
        private MissionVariant _variant;
        private MissionRunState _state = MissionRunState.Idle;
        private MissionFailReason _failReason = MissionFailReason.None;
        private int _nextIndex;
        private float _elapsedSeconds;
        private float _effectiveLimitSeconds;
        private int _effectiveFailWanted;
        private int _retries;

        public MissionData Mission => _mission;
        public MissionVariant Variant => _variant;
        public MissionRunState State => _state;
        public MissionFailReason FailReason => _failReason;
        public int CompletedStepCount => _nextIndex;
        public int TotalSteps => GetRequiredStepsCount(_mission);
        public int Retries => _retries;
        public float ElapsedSeconds => _elapsedSeconds;
        public float TimeLimitSeconds => _effectiveLimitSeconds;
        public float TimeLeft => _effectiveLimitSeconds > 0f ? Mathf.Max(0f, _effectiveLimitSeconds - _elapsedSeconds) : 0f;

        public string CurrentObjectiveText
        {
            get
            {
                if (_mission == null || _mission.objectives == null || _mission.objectives.Length == 0)
                    return string.Empty;
                int idx = Mathf.Clamp(_nextIndex, 0, _mission.objectives.Length - 1);
                return _mission.objectives[idx] ?? string.Empty;
            }
        }

        public void Start(MissionData mission, MissionVariant variant = MissionVariant.None)
        {
            _mission = mission;
            _variant = variant;
            _nextIndex = 0;
            _elapsedSeconds = 0f;
            _retries = 0;
            _failReason = MissionFailReason.None;
            _effectiveLimitSeconds = MissionModifierRules.EffectiveTimeLimit(mission, variant);
            _effectiveFailWanted = MissionModifierRules.EffectiveFailWantedLevel(mission, variant);
            _state = mission != null ? MissionRunState.Running : MissionRunState.Idle;
        }

        public bool ReportCondition(string conditionKey)
        {
            if (_state != MissionRunState.Running || _mission == null || string.IsNullOrEmpty(conditionKey))
                return false;

            string[] conds = _mission.completionConditions;
            if (conds != null && conds.Length > 0)
            {
                if (_nextIndex >= conds.Length) return false;
                if (conds[_nextIndex] != conditionKey)
                    return false;

                _nextIndex++;
                if (_nextIndex >= conds.Length)
                {
                    _state = MissionRunState.Completed;
                }
                return true;
            }

            int total = TotalSteps;
            _nextIndex++;
            if (_nextIndex >= total)
                _state = MissionRunState.Completed;
            return true;
        }

        public void Tick(float deltaTime, int currentWantedLevel, bool playerDead)
        {
            if (_state != MissionRunState.Running) return;

            if (playerDead)
            {
                Fail(MissionFailReason.PlayerDied);
                return;
            }

            if (_effectiveFailWanted > 0 && currentWantedLevel >= _effectiveFailWanted)
            {
                Fail(MissionFailReason.WantedTooHigh);
                return;
            }

            _elapsedSeconds += Mathf.Max(0f, deltaTime);
            if (_effectiveLimitSeconds > 0f && _elapsedSeconds >= _effectiveLimitSeconds)
            {
                _elapsedSeconds = _effectiveLimitSeconds;
                Fail(MissionFailReason.Timeout);
            }
        }

        public void Fail(MissionFailReason reason)
        {
            if (_state != MissionRunState.Running) return;
            _state = MissionRunState.Failed;
            _failReason = reason;
        }

        public void Abandon() => Fail(MissionFailReason.Abandoned);

        // Landmine #14: Retry resets _nextIndex and clock, capped at 3 retries.
        public bool TryRetry()
        {
            if (_state != MissionRunState.Failed || _mission == null || !_mission.retryable)
                return false;
            if (_retries >= MaxRetries)
                return false;

            _retries++;
            _nextIndex = 0;
            _elapsedSeconds = 0f;
            _failReason = MissionFailReason.None;
            _state = MissionRunState.Running;
            return true;
        }

        public int ComputeTimeBonusMoney()
        {
            if (_mission == null || _state != MissionRunState.Completed || _effectiveLimitSeconds <= 0f)
                return 0;
            float rawBonus = TimeLeft * Mathf.Max(0f, _mission.timeBonusPerSecond);
            int cap = Mathf.FloorToInt(_mission.rewardsMoney * MaxTimeBonusRatio);
            return Mathf.Clamp(Mathf.RoundToInt(rawBonus), 0, cap);
        }

        public void Clear()
        {
            _mission = null;
            _variant = MissionVariant.None;
            _state = MissionRunState.Idle;
            _failReason = MissionFailReason.None;
            _nextIndex = 0;
            _elapsedSeconds = 0f;
            _retries = 0;
        }

        private static int GetRequiredStepsCount(MissionData mission)
        {
            if (mission == null) return 0;
            if (mission.completionConditions != null && mission.completionConditions.Length > 0)
                return mission.completionConditions.Length;
            if (mission.objectives != null && mission.objectives.Length > 0)
                return mission.objectives.Length;
            return 1;
        }
    }
}
