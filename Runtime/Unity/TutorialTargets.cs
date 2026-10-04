using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Tutorials.Unity
{
    /// <summary>Dynamic native target lookup. Missing/off-screen targets suppress highlighting, never advance the flow.</summary>
    public sealed class TutorialTargets : MonoBehaviour, ITutorialTargetRegistry
    {
        readonly Dictionary<string, Transform> _targets = new Dictionary<string, Transform>();
        public void Register(string id, Transform target)
        { if (!string.IsNullOrWhiteSpace(id) && target != null) _targets[id] = target; }
        public void Unregister(string id, Transform target)
        { if (id != null && _targets.TryGetValue(id, out var current) && current == target) _targets.Remove(id); }
        public bool TryResolve(string id, out object target)
        {
            target = null;
            if (id == null || !_targets.TryGetValue(id, out var transform) || transform == null || !transform.gameObject.activeInHierarchy) return false;
            target = transform; return true;
        }
        void OnDestroy() => _targets.Clear();
    }
}
