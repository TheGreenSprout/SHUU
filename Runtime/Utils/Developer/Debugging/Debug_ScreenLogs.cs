using UnityEngine;
using UnityEngine.UI;

using SHUU.Utils.Helpers;

namespace SHUU.Utils.Developer.Debugging
{
    [DefaultExecutionOrder(-10000)]
    public class Debug_ScreenLogs : Singleton_MonoBehaviour<Debug_ScreenLogs>
    {
        #region Variables
        protected override bool PersistantSingleton() => false;



        private Debug_ScreenLogsProxy _proxy;
        
        public Debug_ScreenLogsProxy proxy
        {
            get => _proxy;
            set
            {
                if (value == null)
                {
                    if (_proxy != null) OnProxyRemoved(_proxy);
                }
                else if (_proxy != value) OnProxyAdded(value);

                _proxy = value;
            }
        }



        [Header("General")]
        public bool listenForDebugLogs = false;

        public bool listenForNormalLogs = true;
        public bool listenForWarningLogs = true;
        public bool listenForErrorLogs = true;
        public bool listenForExceptionLogs = true;
        public bool listenForAssertLogs = true;


        [SerializeField] private Color defaultTextColor = Color.white;

        [SerializeField] private Debug_LogMessage logMessagePrefab;



        [Header("Object Pooling")]
        [Tooltip("If 0, the screen logs won't use an object pool.")]
        [SerializeField] [Min(0)] private int logsPool_initialSize = 10;



        // Internal
        private ScreenLog_Factory factory = null;
        #endregion




        #region Main
        private void OnProxyAdded(Debug_ScreenLogsProxy proxy)
        {
            if (this.proxy != null) this.proxy = null;

            if (!proxy || !proxy.content) return;


            if (listenForDebugLogs)
            {
                Application.logMessageReceived += HandleLog;

                if (!proxy.content.gameObject.activeInHierarchy) proxy.content.gameObject.SetActive(true);
            }
            else if (proxy.content.gameObject.activeInHierarchy) proxy.content.gameObject.SetActive(false);


            CreateFactory(proxy);
        }

        private void OnProxyRemoved(Debug_ScreenLogsProxy proxy)
        {
            if (!proxy || !proxy.content) return;


            if (listenForDebugLogs)
            {
                Application.logMessageReceived -= HandleLog;

                if (proxy.content.gameObject.activeInHierarchy) proxy.content.gameObject.SetActive(false);
            }
        }


        private void CreateFactory(Debug_ScreenLogsProxy proxy)
        {
            if (factory != null) factory.Dispose();

            if (logsPool_initialSize == 0) factory = new Basic_ScreenLog_Factory(logMessagePrefab);
            else if (proxy != null) factory = new ObjectPool_ScreenLog_Factory(logMessagePrefab, logsPool_initialSize, proxy.poolParent);
        }
        #endregion



        #region Logic

        #region Debug Logs
        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            switch (type)
            {
                case LogType.Log:
                    if (!listenForNormalLogs) return;
                    break;
                
                case LogType.Warning:
                    if (!listenForWarningLogs) return;
                    break;
                
                case LogType.Error:
                    if (!listenForErrorLogs) return;
                    break;
                
                case LogType.Exception:
                    if (!listenForExceptionLogs) return;
                    break;
                
                case LogType.Assert:
                    if (!listenForAssertLogs) return;
                    break;
            }


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

            Actual_HandleLog(logString, color.Value);
        }


        private void Actual_HandleLog(string logString, Color color)
        {
            if (factory == null) return;

            factory?.GetLog(proxy?.content, logString, color);

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(proxy?.content);
        }
        #endregion



        #region Screen Logs
        public bool Toggle() => listenForDebugLogs = !listenForDebugLogs;


        public void ScreenLog(string message, Color? color = null)
        {
            if (color == null || color.Value == Color.white) Debug.Log(message);
            else
            {
                if (color.Value == Color.yellow) Debug.LogWarning(message);
                else if (color.Value == Color.red) Debug.LogError(message);
                else Debug.Log(color.Value.GetColorOpenTag_RichText() + message + "</color>");
            }

            if (!listenForDebugLogs) HandleLog(message, color);
        }
        public void ScreenLog_Warning(string message) => ScreenLog(message, Color.yellow);
        public void ScreenLog_Error(string message) => ScreenLog(message, Color.red);
        #endregion
    
        #endregion
    }




    #region Helper class
    public interface ScreenLog_Factory
    {
        public Debug_LogMessage GetLog(RectTransform content, string logString, Color color);

        public void Dispose();
    }



    public class Basic_ScreenLog_Factory : ScreenLog_Factory
    {
        private Debug_LogMessage prefab = null;


        public Basic_ScreenLog_Factory(Debug_LogMessage prefab) => this.prefab = prefab;

        public Debug_LogMessage GetLog(RectTransform content, string logString, Color color) => Object.Instantiate(prefab, content).Init(logString, color);

        public void Dispose() { }
    }


    public class ObjectPool_ScreenLog_Factory : ScreenLog_Factory
    {
        private SHUU_ObjectPool<Debug_LogMessage> pool;


        public ObjectPool_ScreenLog_Factory(Debug_LogMessage prefab, int initPoolSize, Transform parent)
            => pool = new SHUU_ObjectPool<Debug_LogMessage>(prefab, initPoolSize, parent, true, false, "SHUU DebugScreenLogs Pool");

        public Debug_LogMessage GetLog(RectTransform content, string logString, Color color)
        {
            Debug_LogMessage instance = pool?.Get().Init(logString, color, pool);

            if (instance == null) return null;

            instance.gameObject.transform.SetParent(content, false);

            return instance;
        }

        public void Dispose() => pool.Dispose();
    }
    #endregion
}
