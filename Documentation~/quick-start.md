# Quick start

## 1. Define evaluated steps

Create your definition once. IDs are stable; text keys resolve through your localization system. The target ID is a presentation hint, and the portrait ID is optional.

```csharp
var definition = new TutorialDefinition("first-camp", 1, new[] {
    new TutorialStep("select", "guide.select", "guest-card",
        new TutorialPredicate(o => o.ActionId == "select")),
    new TutorialStep("place", "guide.place", "board",
        new TutorialPredicate(o => o.ActionId == "place" && o.Has("valid-route")))
}, "pennant");
```

## 2. Restore and present

Store `TutorialProgress` in your own save. Construct `TutorialRunner(definition, savedProgress)`, then bind an `ITutorialPresenter`. Render `step.TextKey` in your language and resolve `step.TargetId` each time your UI mounts. A missing target hides its highlight, not its instruction.

`TutorialTargets` is an optional Unity registry: `Register("guest-card", cardTransform)` after mount and `Unregister` before removal. A replaced pooled UI object must be registered again.

## 3. Observe the game, then persist

After your real command succeeds and your rules evaluate, call `Observe` with named facts. Never use preview movement, elapsed time or invalid drops as proof of success. Keep the cue visible during a drag; put preview feedback beside it.

`Changed` fires only on changes. Save `runner.Progress` and any associated cosmetic ownership together. `JsonUtility` can serialize the public progress fields.

## 4. Skip, resume and reward

`Skip()` hides the guide. Your game should unlock its ordinary controls independently, preserving important settings and legal information. `Resume()` returns to the first unfinished step, retaining earlier evidence.

When `RewardPending` is true, use `ClaimReward(yourSink)`. `ITutorialRewardSink.TryGrant(rewardId)` must be idempotent, and its result plus tutorial progress must be saved in one application transaction. If save fails, restore both from the last durable save before retrying. Skipping alone never creates `RewardPending`.

## 5. Test the experience

Test valid and invalid actions, restart mid-step, skip/resume, absent or replaced targets, different languages, text scaling and reduced motion. Reward grants must survive a restart and remain once-only.
