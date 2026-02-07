using SHUU.Utils.Helpers;
using UnityEngine;

namespace SHUU.Utils.Developer.Debugging
{
    public class Debug_ScreenLogs : MonoBehaviour
    {
        public static Debug_ScreenLogs instance;


        private Debug_ScreenLogsProxy _proxy;
        public Debug_ScreenLogsProxy proxy
        {
            get => _proxy;
            set
            {
                if (value == null) OnProxyRemoved(_proxy);
                else OnProxyAdded(value);

                _proxy = value;
            }
        }



        public bool listenForDebugLog = false;


        [SerializeField] private Color defaultTextColor = Color.white;

        [SerializeField] private Debug_LogMessage logMessagePrefab;




        private void Awake() => instance = this;

        
        private void OnProxyAdded(Debug_ScreenLogsProxy proxy)
        {
            if (!proxy || !proxy.content) return;


            if (listenForDebugLog)
            {
                Application.logMessageReceived += HandleLog;

                if (!proxy.content.gameObject.activeInHierarchy) proxy.content.gameObject.SetActive(true);
            }
            else if (proxy.content.gameObject.activeInHierarchy) proxy.content.gameObject.SetActive(false);
        }

        private void OnProxyRemoved(Debug_ScreenLogsProxy proxy)
        {
            if (!proxy || !proxy.content) return;


            if (listenForDebugLog)
            {
                Application.logMessageReceived -= HandleLog;

                if (proxy.content.gameObject.activeInHierarchy) proxy.content.gameObject.SetActive(false);
            }
        }



        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            Color color = type switch
            {
                LogType.Warning => Color.yellow,
                LogType.Error or LogType.Exception => Color.red,
                _ => defaultTextColor
            };

            HandleLog(logString, color);
        }

        private void HandleLog(string logString, Color? color)
        {
            if (color == null) color = defaultTextColor;

            string[] allLines = logString.Split("\n");

            foreach (string line in allLines)
            {
                Actual_HandleLog(line, color.Value);
            }
        }
        private void Actual_HandleLog(string logString, Color color) => Instantiate(logMessagePrefab, proxy?.content).Init(logString, color);


        public void ScreenLog(string message, Color? color = null)
        {
            if (color == null || color.Value == Color.white) Debug.Log(message);
            else
            {
                if (color.Value == Color.yellow) Debug.LogWarning(message);
                else if (color.Value == Color.red) Debug.LogError(message);
                else Debug.Log(color.Value.GetColorOpenTag_RichText() + message + "</color>");
            }

            if (!listenForDebugLog) HandleLog(message, color);
        }
        public void ScreenLog_Warning(string message) => ScreenLog(message, Color.yellow);
        public void ScreenLog_Error(string message) => ScreenLog(message, Color.red);
    }
}
