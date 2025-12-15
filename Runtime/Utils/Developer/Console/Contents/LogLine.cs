using TMPro;
using UnityEngine;

namespace SHUU.Utils.Developer.Console
{
    public class LogLine : MonoBehaviour
    {
        [SerializeField] private TMP_Text logText;




        public void Setup(string text, Color color)
        {
            logText.text = text;
            logText.color = color;
        }
    }
}
