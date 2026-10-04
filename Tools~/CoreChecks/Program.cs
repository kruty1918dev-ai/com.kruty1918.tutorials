using System;
using System.Collections.Generic;
using Kruty1918.Tutorials;
static class Program
{
    sealed class Sink:ITutorialRewardSink {public int Calls;public bool TryGrant(string id){Calls++;return true;}}
    static void Check(bool ok,string label){if(!ok)throw new Exception(label);Console.WriteLine("PASS "+label);}
    static void Main()
    {
        var d=new TutorialDefinition("camp",1,new[]{new TutorialStep("select","select","card",new TutorialPredicate(o=>o.ActionId=="select")),new TutorialStep("place","place","board",new TutorialPredicate(o=>o.ActionId=="place"&&o.Has("valid")))},"pennant");
        var r=new TutorialRunner(d);Check(!r.Observe(new TutorialObservation("place")),"out-of-order action ignored");
        Check(r.Observe(new TutorialObservation("select")),"real selection advances");r.Skip();Check(r.Current==null&&!r.RewardPending,"skip gives no reward");r.Resume();Check(r.Current.Id=="place","resume keeps earned step");
        Check(!r.Observe(new TutorialObservation("place",new Dictionary<string,bool>{{"valid",false}})),"invalid drop ignored");
        Check(r.Observe(new TutorialObservation("place",new Dictionary<string,bool>{{"valid",true}})),"evaluated valid drop advances");
        var sink=new Sink();Check(r.ClaimReward(sink),"complete flow grants reward");Check(!r.ClaimReward(sink)&&sink.Calls==1,"repeat grant suppressed");
        var restored=new TutorialRunner(d,r.Progress);Check(restored.Completed&&!restored.ClaimReward(sink),"restart preserves reward");
        var migrated=new TutorialRunner(d,new TutorialProgress{flowId="camp",completedSteps=new[]{"select","select","obsolete"}});Check(migrated.Current.Id=="place"&&migrated.Progress.completedSteps.Length==1,"stable IDs filter stale duplicates");
        var other=new TutorialRunner(d,new TutorialProgress{flowId="other",skipped=true,completedSteps=new[]{"select"}});Check(other.Current.Id=="select"&&!other.Skipped,"other flow starts fresh");
        var same=new TutorialRunner(new TutorialDefinition("same",1,new[]{new TutorialStep("a","a",null,new TutorialPredicate(o=>true)),new TutorialStep("b","b",null,new TutorialPredicate(o=>true))}));same.Observe(new TutorialObservation("x"));Check(same.Current.Id=="b","one observation advances one step");
    }
}
