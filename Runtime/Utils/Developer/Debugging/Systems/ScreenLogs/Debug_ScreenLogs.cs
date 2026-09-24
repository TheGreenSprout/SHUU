using UnityEngine;
using UnityEngine.UI;

using SHUU.Utils.Helpers;

namespace SHUU.Utils.Developer.Debugging.Systems
{
    [DefaultExecutionOrder(-10000)]
    public class Debug_ScreenLogs : HiddenSingleton_MonoBehaviour<Debug_ScreenLogs>
    {
        #region Variables

        #region Singleton
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
        #endregion



        #region Inspector
        private bool active => SHUU_Debug.Instance.screenLogs_enabled;


        private bool listenForDebugLogs => SHUU_Debug.Instance.screenLogs_listenForDebugLogs;

        private bool listenForNormalLogs => SHUU_Debug.Instance.screenLogs_listenForNormalLogs;
        private bool listenForWarningLogs => SHUU_Debug.Instance.screenLogs_listenForWarningLogs;
        private bool listenForErrorLogs => SHUU_Debug.Instance.screenLogs_listenForErrorLogs;
        private bool listenForExceptionLogs => SHUU_Debug.Instance.screenLogs_listenForExceptionLogs;
        private bool listenForAssertLogs => SHUU_Debug.Instance.screenLogs_listenForAssertLogs;


        private Color defaultTextColor => SHUU_Debug.Instance.screenLogs_defaultTextColor;

        private Debug_LogMessage logMessagePrefab => SHUU_Debug.Instance.screenLogs_logMessagePrefab;


        private int initialPoolSize => SHUU_Debug.Instance.screenLogs_initialPoolSize;
        #endregion



        // Internal
        private ScreenLog_Factory factory = null;

        #endregion




        #region Main
        private void OnProxyAdded(Debug_ScreenLogsProxy proxy)
        {
            if (this.proxy != null) this.proxy = null;

            if (!proxy || !proxy.content) return;

            
            Application.logMessageReceived += HandleLog;
            
            if (!proxy.content.gameObject.activeInHierarchy) proxy.content.gameObject.SetActive(true);


            CreateFactory(proxy);
        }

        private void OnProxyRemoved(Debug_ScreenLogsProxy proxy)
        {
            if (!proxy || !proxy.content) return;


            Application.logMessageReceived -= HandleLog;
            
            if (proxy.content.gameObject.activeInHierarchy) proxy.content.gameObject.SetActive(false);
        }


        private void CreateFactory(Debug_ScreenLogsProxy proxy)
        {
            if (factory != null) factory.Dispose();

            if (!listenForDebugLogs) return;

            if (initialPoolSize == 0) factory = new Basic_ScreenLog_Factory(logMessagePrefab);
            else if (proxy != null) factory = new ObjectPool_ScreenLog_Factory(logMessagePrefab, initialPoolSize, proxy.poolParent);
        }
        #endregion



        #region Logic

        #region Debug Logs
        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            if (!active || !listenForDebugLogs) return;


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
        public void ScreenLog(string message, Color? color = null)
        {
            if (!active) return;

            
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
        private SHUU_ObjectPool<Debug_LogMessage> _pool = null;
        private SHUU_ObjectPool<Debug_LogMessage> pool
        {
            get
            {
                if (_pool == null) _pool = new SHUU_ObjectPool<Debug_LogMessage>(prefab, initPoolSize, parent, autoRestore: false, poolName: "SHUU DebugScreenLogs Pool");

                return _pool;
            }
        }

        private Debug_LogMessage prefab;
        private int initPoolSize;
        private Transform parent;


        public ObjectPool_ScreenLog_Factory(Debug_LogMessage prefab, int initPoolSize, Transform parent)
        {
            this.prefab = prefab;
            this.initPoolSize = initPoolSize;
            this.parent = parent;
        }

        public Debug_LogMessage GetLog(RectTransform content, string logString, Color color)
        {
            Debug_LogMessage Instance = pool?.Get().Init(logString, color, pool);

            if (Instance == null) return null;

            Instance.gameObject.transform.SetParent(content, false);

            return Instance;
        }

        public void Dispose() => pool.Dispose();
    }
    #endregion
}
