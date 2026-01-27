/*using TMPro;
using UnityEngine;

namespace SHUU
{
    public class DialogueBox_NameDisplayHandler : MonoBehaviour
    {
        // External
        [SerializeField] private GameObject nameDisplay;


        [SerializeField] private TMP_Text nameDisplayText;




        public void DisplayName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                if (!nameDisplay.activeInHierarchy) nameDisplay.SetActive(true);

                nameDisplayText.text = name;
            }
            else
            {
                if (nameDisplay.activeInHierarchy) nameDisplay.SetActive(false);

                nameDisplayText.text = "";
            }
        }
    }
}*/
