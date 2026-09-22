using UnityEngine;

namespace SHUU.Utils.Developer.Debugging.Systems
{
    [DefaultExecutionOrder(-9999)]
    public class Debug_ColliderTracker : MonoBehaviour
    {
        private void Awake() => Debug_ColliderVisualizer.Instance?.proxy?.OnColliderAdded(this);
        private void OnDestroy() => Debug_ColliderVisualizer.Instance?.proxy?.OnColliderRemoved(this);
    }
}
