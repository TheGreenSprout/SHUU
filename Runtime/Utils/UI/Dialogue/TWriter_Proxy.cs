using UnityEngine;

namespace SHUU.Utils.UI
{
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
            foreach (TypewriterText twriter in allTWriterScrs)
            {
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
