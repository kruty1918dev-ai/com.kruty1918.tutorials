# Gentle Tutorials for Unity

Teach a game through calm, evaluated actions. A step closes when the player actually succeeds; a timer or a button press alone never pretends that they learned a rule.

**Unity 2021.3+ · MIT · no SDK, networking, analytics or mandatory UI framework**

## Install

Unity → Window → Package Manager → **Add package from git URL**:

```text
https://github.com/kruty1918dev-ai/com.kruty1918.tutorials.git#v0.1.2
```

Start with the [five-minute integration](Documentation~/quick-start.md), then import **Gentle Guide** from the package's Samples tab.

## What it does

- Stable flow and step IDs, pure C# predicates and Unity-serializable progress.
- An observation advances one step; invalid evaluated actions cannot complete it. Missing targets hide highlights without hiding the instruction.
- Skip and resume without losing already earned steps. Skipping gives access to your game, not an unearned tutorial reward.
- Separate presenters, native targets and reward adapters. Works with uGUI, UI Toolkit, UnityHTML, world markers or your own UI.
- A once-only reward boundary with an explicit idempotent host contract.

The package does not disable your controls, decide game rules, save files or award real currency. Those remain application responsibilities. Its pure core has no Unity references; its optional adapter resolves live `Transform` targets.

## Typical integration

```csharp
var step = new TutorialStep("place-with-path", "guide.place", "guest-card",
    new TutorialPredicate(o => o.ActionId == "place" && o.Has("route-valid")));
var guide = new TutorialRunner(new TutorialDefinition("camp", 1,
    new[] { step }, "trail-pennant"), loadedProgress);
guide.Changed += SaveGuideAndCosmeticsTogether;
guide.Observe(new TutorialObservation("place", evaluatedFacts));
```

Build explanations around the current cue, keep essential settings available, and offer a visible way to skip. Presenters should respect text scaling, reduced motion, safe areas and localization.

## Validation and contributions

Run standalone checks with `dotnet run --project Tools~/CoreChecks`. Add the package to `testables` in your project's manifest to run its EditMode tests. [Architecture and lifecycle](Documentation~/architecture.md) explains versioning, rewards and reappearing targets.

Issues and focused pull requests are welcome. Include the Unity version, minimal reproduction and expected cue or progress state. Do not attach personal save files. The package includes no game-specific or licensed third-party art.
