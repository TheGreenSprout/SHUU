using TMPro;
using UnityEngine;

using SHUU.Utils.Globals;
using SHUU.Utils.Helpers;
using SHUU.Utils.SceneManagement;

using static SHUU.Utils.Helpers.HandyFunctions;

namespace SHUU.Utils.Developer.Debugging.Systems
{
    public class Debug_LogMessage : MonoBehaviour, IObjectPoolable
    {
        #region Variables
        [SerializeField] private TMP_Text text;


        [SerializeField] private float startBuffer = 1.5f;
        [SerializeField] private float fadeSpeed = 1f;

        [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.5f);



        private Color color;
        private Color ogColor;

        private bool fade = false;

        private SHUU_ObjectPool<Debug_LogMessage> pool;


        private SHUU_Timer timer = null;
        #endregion




        #region Main
        public Debug_LogMessage Init(string message, Color color, SHUU_ObjectPool<Debug_LogMessage> pool)
        {
            text.text = message.RichText_Marked(backgroundColor, true);
            text.color = color;
            
            this.color = color;

            this.pool = pool;


            timer = SHUU_Time.Timer(startBuffer, () => fade = true);

            SceneLoader.OnSceneLoadRequested += Dispose;

            return this;
        }

        private void Dispose(string sceneName = null)
        {
            if (timer != null) timer.Cancel();
            timer = null;

            SceneLoader.OnSceneLoadRequested -= Dispose;

            pool.Return(this);
        }


        private void Update()
        {
            if (!fade) return;


            color.a -= fadeSpeed * Time.deltaTime;
            text.color = color;

            if (color.a <= 0f)
            {
                fade = false;
                
                Dispose();
            }
        }
        #endregion
    
    
    
        #region Override points
        public void SaveDefaults() => ogColor = text.color;
        
        public void RestoreDefaults()
        {
            text.text = "";
            text.color = ogColor;

            color = ogColor;

            fade = false;

            pool = null;
        }
        #endregion
    }
}
