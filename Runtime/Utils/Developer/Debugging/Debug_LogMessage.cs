using SHUU.Utils.Globals;
using TMPro;
using UnityEngine;

namespace SHUU.Utils.Developer.Debugging
{
    public class Debug_LogMessage : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;


        [SerializeField] private float startBuffer = 1.5f;
        [SerializeField] private float fadeSpeed = 1f;



        private Color color;

        private bool go = false;




        public void Init(string message, Color color)
        {
            text.text = message;
            text.color = color;
            
            this.color = color;


            SHUU_Time.Create(startBuffer, () => go = true);
        }


        private void Update()
        {
            if (!go) return;


            color.a -= fadeSpeed * Time.deltaTime;
            text.color = color;

            if (color.a <= 0f) Destroy(gameObject);
        }
    }
}
