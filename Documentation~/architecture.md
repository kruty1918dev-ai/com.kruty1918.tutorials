# Architecture and lifecycle

The core assembly `Kruty1918.Tutorials` has no engine references. `TutorialDefinition` owns ordered stable steps; a predicate reads a `TutorialObservation` containing an action and evaluated boolean facts. The runner owns progress only. No scene lookup, input mutation, filesystem, timer, analytics or network is involved.

`TutorialProgress` stores completed step IDs and claimed reward IDs. Restore filters unknown step IDs and keeps matching IDs across definition revisions. Adding a step makes it pending; removing a step does not grant a reward on its own until the host explicitly attempts the reward. Renaming a step is a new requirement. A different flow ID starts fresh. Hosts needing semantic migrations should transform progress before constructing the runner.

One observation can complete one current step. Multiple gameplay facts can be reported as distinct evaluated observations when they represent distinct real actions. Skip/resume does not manufacture completion. A presenter is notified at binding and on meaningful state changes.

The optional Unity assembly provides a dynamic `Transform` registry. It excludes destroyed and inactive targets. It deliberately supplies no pointer blocker or screen overlay; host applications choose a clear, accessible visual and refresh it after screen resizing or UI reconciliation.

Rewards are identifiers rather than purchases or currencies. The sink is idempotent by ID. The host must persist cosmetic ownership and claimed IDs together and roll both back on write failure. Retrying an idempotent sink after a crash cannot duplicate a grant.

All public methods are synchronous; call them on the same thread that owns your game state. Predicates should be fast and side-effect free. Rebind presenters and native targets after scene changes; unbind presenters before destroying their UI.
