using UnityEngine;

using Alchemy.Inspector;
using UnityEngine.Events;

namespace SHUU.Utils.UI
{
    public class BlipSliderAction : MonoBehaviour
    {
        #region Variables
        [Title("References")]
        [SerializeField] private GameObject[] blips;


        [Title("Settings")]
        [SerializeField] private bool allowEmptyBlips = true;
        #endregion




        #region Main
        public void SelectBlip(int index)
        {
            DisableBlips();

            if (!allowEmptyBlips)
            {
                for (int i = 0; i < index+1; i++)
                    blips[i].SetActive(true);
            }
            else
            {
                for (int i = 0; i < index; i++)
                    blips[i].SetActive(true);
            }

            BlipUpdate(index);
        }
        #endregion



        #region Override points
        protected virtual void BlipUpdate(int index) { }
        #endregion



        #region Logic
        protected void DisableBlips()
        {
            foreach (var blip in blips)
                blip.SetActive(false);
        }
        #endregion
    }
}
