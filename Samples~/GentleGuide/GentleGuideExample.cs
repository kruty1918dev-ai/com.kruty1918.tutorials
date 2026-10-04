using System.Collections.Generic;
using UnityEngine;
using Kruty1918.Tutorials;

namespace GentleTutorialSample
{
    /// <summary>Demo only. A production host feeds facts from its evaluated game rules.</summary>
    public sealed class GentleGuideExample : MonoBehaviour, ITutorialPresenter, ITutorialRewardSink
    {
        [SerializeField] TutorialProgress progress = new TutorialProgress();
        TutorialRunner _runner; string _cue; bool _reward;
        void Awake()
        {
            _runner = new TutorialRunner(new TutorialDefinition("sample", 1, new[] {
                new TutorialStep("choose", "Choose a campsite", "campsite", new TutorialPredicate(o => o.ActionId == "choose")),
                new TutorialStep("safe", "Keep the entrance clear", "entrance", new TutorialPredicate(o => o.ActionId == "place" && o.Has("clear-entrance")))
            }, "sample-pennant"), progress);
            _runner.BindPresenter(this);
        }
        public void Choose() => _runner.Observe(new TutorialObservation("choose"));
        public void Place(bool clearEntrance)
        {
            _runner.Observe(new TutorialObservation("place", new Dictionary<string, bool> { ["clear-entrance"] = clearEntrance }));
            _runner.ClaimReward(this);
        }
        public void Show(TutorialStep step) => _cue = step.TextKey;
        public void Hide() => _cue = null;
        public bool TryGrant(string rewardId) { _reward = true; return true; }
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(24, 24, 340, 250), GUI.skin.box);
            GUILayout.Label(_cue ?? (_reward ? "Your pennant is ready." : "Explore at your own pace."));
            if (GUILayout.Button("Choose campsite")) Choose();
            if (GUILayout.Button("Place with blocked entrance")) Place(false);
            if (GUILayout.Button("Place with clear entrance")) Place(true);
            if (GUILayout.Button(_runner.Skipped ? "Learn again" : "Skip guide")) { if (_runner.Skipped) _runner.Resume(); else _runner.Skip(); }
            GUILayout.EndArea();
        }
        void OnDestroy() => _runner?.BindPresenter(null);
    }
}
