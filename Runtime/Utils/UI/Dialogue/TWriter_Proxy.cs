using SHUU.Utils.UI.Dialogue;
using UnityEngine;

namespace SHUU.Utils.UI
{
    
    [DisallowMultipleComponent]

    [RequireComponent(typeof(DialogueInputAddon))]
    public class TWriter_Proxy : MonoBehaviour
    {
        [Header("References")]
        public TypewriterText[] allTWriterScrs;



        [Header("Settings")]
        public float charactersPerSecond = 20f;

        public float interpunctuationDelay = 0.5f;
        public float semi_interpunctuationDelay = 0.25f;


        public bool quickSkip = false;

        public int skipSpeedup = 5;
        [Range(0.1f, 0.5f)] public float sendDoneDelay = 0.25f;

        


        private void Awake()
        {
            DialogueInputAddon inputAddon = GetComponent<DialogueInputAddon>();

            
            foreach (TypewriterText twriter in allTWriterScrs)
            {
                twriter.input = inputAddon;


                twriter.charactersPerSecond = charactersPerSecond;

                twriter.interpunctuationDelay = interpunctuationDelay;
                twriter.semi_interpunctuationDelay = semi_interpunctuationDelay;


                twriter.skipSpeedup = skipSpeedup;
                twriter.sendDoneDelay = sendDoneDelay;

                twriter.quickSkip = quickSkip;
            }
        }
    }
}
