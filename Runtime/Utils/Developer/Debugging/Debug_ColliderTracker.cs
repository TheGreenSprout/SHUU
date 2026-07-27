using UnityEngine;

namespace SHUU.Utils.Developer.Debugging
{
    [DefaultExecutionOrder(-9999)]
    public class Debug_ColliderTracker : MonoBehaviour
    {
        private void Awake()   => Debug_ColliderVisualizer.instance?.proxy?.OnColliderAdded(this);
        private void OnDestroy() => Debug_ColliderVisualizer.instance?.proxy?.OnColliderRemoved(this);
    }
}
