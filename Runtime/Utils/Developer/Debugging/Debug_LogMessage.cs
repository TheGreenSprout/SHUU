using TMPro;
using UnityEngine;

using SHUU.Utils.Globals;
using SHUU.Utils.Helpers;

namespace SHUU.Utils.Developer.Debugging
{
    public class Debug_LogMessage : MonoBehaviour
    {
        #region Variables
        [SerializeField] private TMP_Text text;


        [SerializeField] private float startBuffer = 1.5f;
        [SerializeField] private float fadeSpeed = 1f;



        private Color color;

        private bool go = false;



        private SHUU_ObjectPool<Debug_LogMessage> pool;
        #endregion




        #region Main
        public Debug_LogMessage Init(string message, Color color, SHUU_ObjectPool<Debug_LogMessage> pool = null)
        {
            text.text = message;
            text.color = color;
            
            this.color = color;

            this.pool = pool;


            SHUU_Time.Timer(startBuffer, () => go = true);


            return this;
        }

        private void Dispose()
        {
            if (pool == null) Destroy(gameObject);
            else pool.Return(this);
        }


        private void Update()
        {
            if (!go) return;


            color.a -= fadeSpeed * Time.deltaTime;
            text.color = color;

            if (color.a <= 0f)
            {
                go = false;
                
                Dispose();
            }
        }
        #endregion
    }
}
