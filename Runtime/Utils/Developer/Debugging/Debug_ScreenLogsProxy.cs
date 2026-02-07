using UnityEngine;

namespace SHUU.Utils.Developer.Debugging
{
    public class Debug_ScreenLogsProxy : MonoBehaviour
    {
        public Transform content;




        private void OnEnable()
        {
            if (Debug_ScreenLogs.instance) Debug_ScreenLogs.instance.proxy = this;
            else Invoke(nameof(OnEnable), 0.01f);
        }

        private void OnDisable()
        {
            if (Debug_ScreenLogs.instance) Debug_ScreenLogs.instance.proxy = null;
        }
    }
}
