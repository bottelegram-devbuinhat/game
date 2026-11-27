using System;
using System.Collections.Generic;
using DudeChaos.Data;
using DudeChaos.Player;
using DudeChaos.Wanted;
using UnityEngine;

namespace DudeChaos.Progression
{
    public sealed class MissionSystem : MonoBehaviour
    {
        private readonly MissionRun _run = new MissionRun();
        private readonly Dictionary<string, int> _bestStars = new Dictionary<string, int>();
        private readonly HashSet<string> _completedIds = new HashSet<string>();
        private readonly MissionBanterRules _banter = new MissionBanterRules();

        private Wallet _wallet;
        private HealthSystem _playerHealth;
        private WantedSystem _wantedSystem;
        private float _startHealth;
        private int _reputation;
        private int _variantsCleared;

        public event Action<MissionData> MissionStarted;
        public event Action<MissionData, MissionGradeResult> MissionCompleted;
        public event Action<MissionData, MissionFailReason> MissionFailed;

        public MissionRun CurrentRun => _run;
        public MissionBanterRules Banter => _banter;
        public float TimeLeft => _run.TimeLeft;
        public string CurrentObjective => _run.CurrentObjectiveText;
        public int Retries => _run.Retries;
        public int Reputation => _reputation;
        public int VariantsCleared => _variantsCleared;
        public IReadOnlyCollection<string> CompletedMissionIds => _completedIds;
        public IReadOnlyDictionary<string, int> BestStars => _bestStars;

        public void Bind(Wallet wallet, HealthSystem playerHealth = null, WantedSystem wanted = null)
        {
            _wallet = wallet;
            _playerHealth = playerHealth;
            _wantedSystem = wanted;
        }

        public bool StartMission(MissionData mission, MissionVariant variant = MissionVariant.None)
        {
            if (mission == null) return false;
            bool alreadyDone = _completedIds.Contains(mission.missionId);
            if (!MissionModifierRules.CanRunVariant(variant, alreadyDone))
                return false;

            _startHealth = _playerHealth != null ? _playerHealth.CurrentHealth : 100f;
            _run.Start(mission, variant);
            _banter.GetIntroLine(mission);
            MissionStarted?.Invoke(mission);
            return true;
        }

        public bool ReportCondition(string conditionKey)
        {
            if (_run.State != MissionRunState.Running) return false;
            bool progressed = _run.ReportCondition(conditionKey);
            if (progressed && _run.State == MissionRunState.Completed)
            {
                CompleteActiveRun();
            }
            return progressed;
        }

        public void Tick(float deltaTime)
        {
            if (_run.State != MissionRunState.Running) return;
            int wanted = _wantedSystem != null ? _wantedSystem.Level : 0;
            bool dead = _playerHealth != null && !_playerHealth.IsAlive;
            _run.Tick(deltaTime, wanted, dead);
            if (_run.State == MissionRunState.Failed)
            {
                _banter.GetMidOrFailLine(_run.Mission, _run.FailReason);
                MissionFailed?.Invoke(_run.Mission, _run.FailReason);
            }
        }

        public void Update() => Tick(Time.deltaTime);

        public bool TryRetry()
        {
            bool ok = _run.TryRetry();
            if (ok && _playerHealth != null)
                _startHealth = _playerHealth.CurrentHealth;
            return ok;
        }

        public void Clear() => _run.Clear();

        public int GetBestStars(string missionId)
        {
            return !string.IsNullOrEmpty(missionId) && _bestStars.TryGetValue(missionId, out int s) ? s : 0;
        }

        public string[] ExportStarEntries()
        {
            var list = new List<string>(_bestStars.Count);
            foreach (var kv in _bestStars)
                list.Add(MissionGradeRules.EncodeStar(kv.Key, kv.Value));
            return list.ToArray();
        }

        public void RestoreProgress(IEnumerable<string> completedIds, IEnumerable<string> starEntries, int reputation, int variantsCleared = 0)
        {
            _completedIds.Clear();
            if (completedIds != null)
            {
                foreach (string id in completedIds)
                {
                    if (!string.IsNullOrEmpty(id)) _completedIds.Add(id);
                }
            }

            _bestStars.Clear();
            if (starEntries != null)
            {
                foreach (string entry in starEntries)
                {
                    if (MissionGradeRules.TryDecodeStar(entry, out string id, out int stars))
                        _bestStars[id] = stars;
                }
            }

            _reputation = Mathf.Max(0, reputation);
            _variantsCleared = Mathf.Max(0, variantsCleared);
        }

        private void CompleteActiveRun()
        {
            MissionData m = _run.Mission;
            int wanted = _wantedSystem != null ? _wantedSystem.Level : 0;
            float hpLostFrac = 0f;
            if (_playerHealth != null && _playerHealth.MaxHealth > 0f)
            {
                float lost = Mathf.Max(0f, _startHealth - _playerHealth.CurrentHealth);
                hpLostFrac = Mathf.Clamp01(lost / _playerHealth.MaxHealth);
            }

            int timeBonus = _run.ComputeTimeBonusMoney();
            MissionGradeResult grade = MissionGradeRules.Evaluate(
                m.rewardsMoney,
                timeBonus,
                _run.ElapsedSeconds,
                _run.TimeLimitSeconds,
                _run.Retries,
                wanted,
                hpLostFrac,
                _run.Variant);

            _completedIds.Add(m.missionId);
            if (!_bestStars.TryGetValue(m.missionId, out int prevBest) || grade.stars > prevBest)
                _bestStars[m.missionId] = grade.stars;

            if (_run.Variant != MissionVariant.None)
                _variantsCleared++;

            _wallet?.Add(grade.totalMoneyPayout);
            _reputation += Mathf.Max(0, m.rewardsReputation);
            _banter.GetPayoffLine(m);
            MissionCompleted?.Invoke(m, grade);
        }
    }
}
