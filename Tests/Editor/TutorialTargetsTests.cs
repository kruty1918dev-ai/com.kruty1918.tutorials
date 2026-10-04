using Kruty1918.Tutorials.Unity;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Tutorials.Tests
{
    public sealed class TutorialTargetsTests
    {
        [Test] public void MissingInactiveAndDestroyedTargetsAreSafe()
        {
            var host=new GameObject("registry");var target=new GameObject("target");
            try
            {
                var registry=host.AddComponent<TutorialTargets>();Assert.IsFalse(registry.TryResolve("card",out _));
                registry.Register("card",target.transform);Assert.IsTrue(registry.TryResolve("card",out _));
                target.SetActive(false);Assert.IsFalse(registry.TryResolve("card",out _));target.SetActive(true);
                Object.DestroyImmediate(target);Assert.IsFalse(registry.TryResolve("card",out _));
            }
            finally {if(target!=null)Object.DestroyImmediate(target);Object.DestroyImmediate(host);}
        }
        [Test] public void OldMountCannotUnregisterItsReplacement()
        {
            var host=new GameObject("registry");var old=new GameObject("old");var current=new GameObject("current");
            try
            {
                var registry=host.AddComponent<TutorialTargets>();registry.Register("card",old.transform);
                registry.Register("card",current.transform);registry.Unregister("card",old.transform);
                Assert.IsTrue(registry.TryResolve("card",out var resolved));Assert.AreSame(current.transform,resolved);
                registry.Unregister("card",current.transform);Assert.IsFalse(registry.TryResolve("card",out _));
            }
            finally {Object.DestroyImmediate(old);Object.DestroyImmediate(current);Object.DestroyImmediate(host);}
        }
    }
}
