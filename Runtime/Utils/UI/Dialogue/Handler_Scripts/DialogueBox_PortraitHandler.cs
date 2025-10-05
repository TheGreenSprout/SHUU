using System;
using System.Collections.Generic;
using UnityEngine;

namespace SHUU.Utils.UI.Dialogue
{

    [DisallowMultipleComponent]
    public class DialogueBox_PortraitHandler : MonoBehaviour
    {
        // Internal
        [HideInInspector] public List<CharacterPortrait_Reference> allPortraitScripts = new List<CharacterPortrait_Reference>();



        // External
        [SerializeField] private Transform characterPortraitSpawnParent = null;


        [SerializeField] private bool accummulatePortraits = true;




        private bool IsCharacterInConversation(ref CharacterPortrait_Reference characterPortraitScr)
        {
            bool check = true;

            foreach (CharacterPortrait_Reference portraitScr in allPortraitScripts)
            {
                if (portraitScr.IDENTIFIER().Equals(characterPortraitScr.IDENTIFIER()))
                {
                    characterPortraitScr = portraitScr;


                    check = false;
                }
            }

            return check;
        }

        public CharacterPortrait_Reference CharacterTalks(GameObject portraitPrefab)
        {
            if (portraitPrefab == null)
            {
                return null;
            }



            CharacterPortrait_Reference portraitInstanceScr = portraitPrefab.GetComponent<CharacterPortrait_Reference>();


            if (!accummulatePortraits)
            {
                if (allPortraitScripts[0].IDENTIFIER().Equals(portraitInstanceScr.IDENTIFIER()))
                {
                    portraitInstanceScr = allPortraitScripts[0];


                    portraitInstanceScr.BeginLine();
                }
                else
                {
                    allPortraitScripts[0].Delete();
                    allPortraitScripts.Clear();


                    GameObject portraitInstance = Instantiate(portraitPrefab, characterPortraitSpawnParent);

                    portraitInstanceScr = portraitInstance.GetComponent<CharacterPortrait_Reference>();

                    portraitInstanceScr.Initialize(this, allPortraitScripts.Count);


                    allPortraitScripts.Add(portraitInstanceScr);


                    portraitInstanceScr.Appear();
                }
            }
            else
            {
                if (!IsCharacterInConversation(ref portraitInstanceScr))
                {
                    GameObject portraitInstance = Instantiate(portraitPrefab, characterPortraitSpawnParent);

                    portraitInstanceScr = portraitInstance.GetComponent<CharacterPortrait_Reference>();

                    portraitInstanceScr.Initialize(this, allPortraitScripts.Count);


                    allPortraitScripts.Add(portraitInstanceScr);


                    portraitInstanceScr.Appear();
                }
                else
                {
                    portraitInstanceScr.BeginLine();
                }
            }


            return portraitInstanceScr;
        }


        public void AllPortraitsFocus()
        {
            foreach (CharacterPortrait_Reference portraitScr in allPortraitScripts)
            {
                portraitScr.BeginLine();
            }
        }

        public void AllPortraitsUnFocus()
        {
            foreach (CharacterPortrait_Reference portraitScr in allPortraitScripts)
            {
                portraitScr.EndLine();
            }
        }



        public void EndDialogue()
        {
            foreach (CharacterPortrait_Reference portrait in allPortraitScripts)
            {
                portrait.Delete();
            }
            
            allPortraitScripts.Clear();
        }
    }

}
