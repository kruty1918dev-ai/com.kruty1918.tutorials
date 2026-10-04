using System;
using System.Collections.Generic;
using NUnit.Framework;
namespace Kruty1918.Tutorials.Tests
{
    public sealed class TutorialRunnerTests
    {
        static TutorialDefinition Definition() => new TutorialDefinition("camp", 1, new[] {
            new TutorialStep("select", "select", "card", new TutorialPredicate(o=>o.ActionId=="select")),
            new TutorialStep("place", "place", "board", new TutorialPredicate(o=>o.ActionId=="place" && o.Has("route")))
        }, "pennant");
        static TutorialObservation Place(bool valid) => new TutorialObservation("place", new Dictionary<string,bool>{{"route",valid}});
        [Test] public void InvalidFactsCannotAdvance()
        { var r=new TutorialRunner(Definition()); Assert.IsFalse(r.Observe(Place(true))); r.Observe(new TutorialObservation("select")); Assert.IsFalse(r.Observe(Place(false))); Assert.IsTrue(r.Observe(Place(true))); Assert.IsTrue(r.Completed); }
        [Test] public void SkipResumePreservesEvidenceAndDoesNotReward()
        { var r=new TutorialRunner(Definition());r.Observe(new TutorialObservation("select"));r.Skip();Assert.IsNull(r.Current);Assert.IsFalse(r.RewardPending);r.Resume();Assert.AreEqual("place",r.Current.Id); }
        [Test] public void RestoringIgnoresUnknownSteps()
        { var r=new TutorialRunner(Definition(),new TutorialProgress{flowId="camp",completedSteps=new[]{"select","unknown","select"}});Assert.AreEqual("place",r.Current.Id);Assert.AreEqual(1,r.Progress.completedSteps.Length); }
        [Test] public void OneObservationAdvancesOnlyOneStep()
        { var r=new TutorialRunner(new TutorialDefinition("same",1,new[]{new TutorialStep("a","a",null,new TutorialPredicate(o=>true)),new TutorialStep("b","b",null,new TutorialPredicate(o=>true))}));r.Observe(new TutorialObservation("x"));Assert.AreEqual("b",r.Current.Id); }
        sealed class Reward:ITutorialRewardSink {public int Calls;public bool TryGrant(string id){Calls++;return true;}}
        [Test] public void RewardIsOnceAcrossRestore()
        { var r=new TutorialRunner(Definition());r.Observe(new TutorialObservation("select"));r.Observe(Place(true));var sink=new Reward();Assert.IsTrue(r.ClaimReward(sink));Assert.IsFalse(r.ClaimReward(sink));var reload=new TutorialRunner(Definition(),r.Progress);Assert.IsFalse(reload.ClaimReward(sink));Assert.AreEqual(1,sink.Calls); }
        [Test] public void DuplicateStepIdsAreRejected()
        {Assert.Throws<ArgumentException>(()=>new TutorialDefinition("camp",1,new[]{new TutorialStep("a","a",null,new TutorialPredicate(o=>true)),new TutorialStep("a","b",null,new TutorialPredicate(o=>true))}));}
    }
}
