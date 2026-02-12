using System.Collections.Generic;
using SHUU.Utils.Globals;
using TMPro;
using UnityEngine;

namespace SHUU.Utils.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class ShiftingText : MonoBehaviour
    {
        [SerializeField] private List<string> textVariations = new();

        [SerializeField] private float shiftInterval = .5f;


        [SerializeField] private bool pingPong = false;



        private int current = -1;
        [SerializeField] [Range(-1, 1)] private int dir = 1;

        private TMP_Text text;


        private bool invoked = false;




        private void Awake()
        {
            text = GetComponent<TMP_Text>();


            if (textVariations.Count == 0) return;

            if (dir >= 0)
            {
                text.text = textVariations[0];
                current = 0;
            }
            else if (dir == -1)
            {
                text.text = textVariations[textVariations.Count - 1];
                current = textVariations.Count - 1;
            }
        }


        private void OnEnable()
        {
            if (!invoked)
            {
                invoked = true;

                SHUU_Time.Timer(shiftInterval, Shift);
            }
        }



        private void Shift()
        {
            if (!this.gameObject.activeInHierarchy)
            {
                invoked = false;

                return;
            }


            current += dir;
            if (current >= textVariations.Count)
            {
                if (pingPong)
                {
                    current = textVariations.Count-2;

                    dir = -1;
                }
                else current = 0;
            }
            else if (current < 0)
            {
                if (pingPong)
                {
                    current = 1;

                    dir = 1;
                }
                else current = textVariations.Count-1;
            }


            text.text = textVariations[current];


            SHUU_Time.Timer(shiftInterval, Shift);
        }
    }
}
