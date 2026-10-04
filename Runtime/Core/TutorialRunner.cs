using System;
using System.Collections.Generic;

namespace Kruty1918.Tutorials
{
    /// <summary>Persist stable step IDs rather than an index. Presenters and game rules remain host-owned.</summary>
    [Serializable]
    public sealed class TutorialProgress
    {
        public string flowId;
        public int version;
        public string[] completedSteps = Array.Empty<string>();
        public string[] claimedRewards = Array.Empty<string>();
        public bool skipped;
    }

    public sealed class TutorialObservation
    {
        public string ActionId { get; }
        readonly IReadOnlyDictionary<string, bool> _facts;
        public TutorialObservation(string actionId, IReadOnlyDictionary<string, bool> facts = null)
        { ActionId = actionId ?? ""; _facts = facts; }
        public bool Has(string fact) => _facts != null && _facts.TryGetValue(fact, out var value) && value;
    }

    public interface ITutorialPredicate { bool Evaluate(TutorialObservation observation); }
    public sealed class TutorialPredicate : ITutorialPredicate
    {
        readonly Func<TutorialObservation, bool> _evaluate;
        public TutorialPredicate(Func<TutorialObservation, bool> evaluate)
            => _evaluate = evaluate ?? throw new ArgumentNullException(nameof(evaluate));
        public bool Evaluate(TutorialObservation observation) => observation != null && _evaluate(observation);
    }
    public sealed class TutorialStep
    {
        public string Id { get; }
        public string TextKey { get; }
        public string TargetId { get; }
        public string PortraitId { get; }
        public ITutorialPredicate Predicate { get; }
        public TutorialStep(string id, string textKey, string targetId, ITutorialPredicate predicate, string portraitId = null)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("A stable step ID is required.", nameof(id));
            Id = id; TextKey = textKey; TargetId = targetId; PortraitId = portraitId;
            Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }
    }
    public sealed class TutorialDefinition
    {
        public string Id { get; }
        public int Version { get; }
        public string RewardId { get; }
        public IReadOnlyList<TutorialStep> Steps { get; }
        public TutorialDefinition(string id, int version, IEnumerable<TutorialStep> steps, string rewardId = null)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("A flow ID is required.", nameof(id));
            if (version < 1) throw new ArgumentOutOfRangeException(nameof(version));
            Id = id; Version = version; RewardId = rewardId;
            var list = new List<TutorialStep>(steps ?? throw new ArgumentNullException(nameof(steps)));
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var step in list)
                if (step == null || !ids.Add(step.Id)) throw new ArgumentException("Step IDs must be unique.", nameof(steps));
            if (list.Count == 0) throw new ArgumentException("A flow needs at least one step.", nameof(steps));
            Steps = list.AsReadOnly();
        }
    }
    public interface ITutorialPresenter { void Show(TutorialStep step); void Hide(); }
    public interface ITutorialTargetRegistry { bool TryResolve(string targetId, out object target); }
    /// <summary>Grant must be idempotent by reward ID. Persist the grant and progress together in the host.</summary>
    public interface ITutorialRewardSink { bool TryGrant(string rewardId); }

    /// <summary>Only a real, evaluated observation advances one step. No clocks, input capture or forced rewards.</summary>
    public sealed class TutorialRunner
    {
        readonly HashSet<string> _completed = new HashSet<string>(StringComparer.Ordinal);
        readonly HashSet<string> _claimed = new HashSet<string>(StringComparer.Ordinal);
        readonly TutorialDefinition _definition;
        readonly TutorialProgress _progress;
        ITutorialPresenter _presenter;
        public event Action Changed;
        public TutorialProgress Progress => _progress;
        public bool Skipped => _progress.skipped;
        public bool Completed => _completed.Count == _definition.Steps.Count;
        public bool RewardPending => Completed && !string.IsNullOrEmpty(_definition.RewardId) && !_claimed.Contains(_definition.RewardId);
        public TutorialStep Current
        {
            get
            {
                if (Skipped) return null;
                foreach (var step in _definition.Steps) if (!_completed.Contains(step.Id)) return step;
                return null;
            }
        }
        public TutorialRunner(TutorialDefinition definition, TutorialProgress progress = null)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
            _progress = progress ?? new TutorialProgress();
            if (_progress.flowId == definition.Id)
            {
                foreach (var id in _progress.completedSteps ?? Array.Empty<string>())
                    foreach (var step in definition.Steps) if (step.Id == id) { _completed.Add(id); break; }
                foreach (var id in _progress.claimedRewards ?? Array.Empty<string>())
                    if (!string.IsNullOrEmpty(id)) _claimed.Add(id);
            }
            else _progress.skipped = false;
            _progress.flowId = definition.Id; _progress.version = definition.Version; Sync();
        }
        public void BindPresenter(ITutorialPresenter presenter) { _presenter?.Hide(); _presenter = presenter; Present(); }
        public bool Observe(TutorialObservation observation)
        {
            var current = Current;
            if (current == null || !current.Predicate.Evaluate(observation)) return false;
            _completed.Add(current.Id); Notify(); return true;
        }
        public bool IsComplete(string stepId) => stepId != null && _completed.Contains(stepId);
        public void Skip() { if (Skipped || Completed) return; _progress.skipped = true; Notify(); }
        /// <summary>Resume remaining steps; already evaluated steps and claimed rewards survive.</summary>
        public void Resume() { if (!Skipped) return; _progress.skipped = false; Notify(); }
        public bool ClaimReward(ITutorialRewardSink sink)
        {
            if (!RewardPending || sink == null || !sink.TryGrant(_definition.RewardId)) return false;
            _claimed.Add(_definition.RewardId); Notify(); return true;
        }
        void Notify() { Sync(); Present(); Changed?.Invoke(); }
        void Present() { var step = Current; if (step == null) _presenter?.Hide(); else _presenter?.Show(step); }
        void Sync()
        {
            var ordered = new List<string>();
            foreach (var step in _definition.Steps) if (_completed.Contains(step.Id)) ordered.Add(step.Id);
            _progress.completedSteps = ordered.ToArray();
            var rewards = new List<string>(_claimed); rewards.Sort(StringComparer.Ordinal);
            _progress.claimedRewards = rewards.ToArray();
        }
    }
}
