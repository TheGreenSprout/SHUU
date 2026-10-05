using UnityEngine;
using TMPro;

using Object = UnityEngine.Object;

namespace SHUU.Utils.UI
{
    [ExecuteAlways, DisallowMultipleComponent, RequireComponent(typeof(TMP_Text)), AddComponentMenu("SHUU/UI/Text Vertex Jitter")]
    public class TextVertexJitter : MonoBehaviour
    {
        #region Variables

        #region JitterMode
        public enum JitterMode
        {
            [InspectorName("Whole letters")] WholeLetters,
            [InspectorName("Each corner")] EachCorner
        }
        #endregion



        #region Inspector
        [Tooltip("Whole letters: each letter jumps around as a unit. Each corner: the four corners of every letter move on their own, which also warps the letters.")]
        [SerializeField] private JitterMode mode = JitterMode.WholeLetters;

        [Tooltip("How far a letter moves, as a fraction of its height: 0.05 is a twentieth of a letter.")]
        [SerializeField, Min(0f)] private float amplitude = 0.05f;

        [Tooltip("How much each letter turns, in degrees either way (Whole letters only).")]
        [SerializeField, Range(0f, 45f)] private float rotation = 0f;

        [Tooltip("How many times a second the letters jump to a new place. Low numbers look like stop motion. 0 = every frame.")]
        [SerializeField, Min(0f)] private float stepsPerSecond = 12f;

        [Tooltip("Different seeds jitter differently, so two texts don't shake in sync.")]
        [SerializeField] private int seed = 0;
        #endregion



        #region Internal
        private TMP_Text text;

        private int lastStep = int.MinValue;
        private bool dirty = true;
        private bool applying;
        #endregion



        #region Fetchers
        public JitterMode Mode
        {
            get => mode;
            set { mode = value; Refresh(); }
        }

        public float Amplitude
        {
            get => amplitude;
            set { amplitude = Mathf.Max(0f, value); Refresh(); }
        }

        public float Rotation
        {
            get => rotation;
            set { rotation = Mathf.Clamp(value, 0f, 45f); Refresh(); }
        }

        public float StepsPerSecond
        {
            get => stepsPerSecond;
            set { stepsPerSecond = Mathf.Max(0f, value); Refresh(); }
        }
        #endregion

        #endregion




        #region Main
        private void OnEnable()
        {
            text = GetComponent<TMP_Text>();

            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);

            Refresh();
        }

        private void OnDisable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);

            if (text != null) text.ForceMeshUpdate();
        }

        private void OnValidate() => Refresh();


        private void LateUpdate()
        {
            if (text == null || !text.isActiveAndEnabled) return;

            int step = CurrentStep();

            if (!dirty && step == lastStep) return;

            Apply(step);

            lastStep = step;
            dirty = false;
        }


        public void Refresh() => dirty = true;

        private void OnTextChanged(Object changed)
        {
            if (!applying && changed == text) dirty = true;
        }
        #endregion



        #region Logic
        private int CurrentStep()
        {
            if (stepsPerSecond <= 0f) return Time.frameCount;

            return (int)(Time.realtimeSinceStartupAsDouble * stepsPerSecond);
        }


        private void Apply(int step)
        {
            applying = true;

            try
            {
                text.ForceMeshUpdate();

                TMP_TextInfo info = text.textInfo;

                for (int i = 0; i < info.characterCount; i++)
                {
                    TMP_CharacterInfo character = info.characterInfo[i];

                    if (!character.isVisible) continue;

                    Vector3[] vertices = info.meshInfo[character.materialReferenceIndex].vertices;
                    int first = character.vertexIndex;

                    if (first + 3 >= vertices.Length) continue;

                    float unit = (character.ascender - character.descender) * amplitude;

                    if (mode == JitterMode.WholeLetters) JitterWhole(vertices, first, unit, step, i);
                    else JitterCorners(vertices, first, unit, step, i);
                }

                text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
            }
            finally { applying = false; }
        }


        private void JitterWhole(Vector3[] vertices, int first, float unit, int step, int index)
        {
            Vector3 offset = new Vector3(Random(step, index, 0), Random(step, index, 1), 0f) * unit;

            Vector3 center = (vertices[first] + vertices[first + 1] + vertices[first + 2] + vertices[first + 3]) * 0.25f;
            Quaternion turn = rotation > 0f ? Quaternion.Euler(0f, 0f, Random(step, index, 2) * rotation) : Quaternion.identity;

            for (int j = 0; j < 4; j++)
                vertices[first + j] = center + turn * (vertices[first + j] - center) + offset;
        }

        private void JitterCorners(Vector3[] vertices, int first, float unit, int step, int index)
        {
            for (int j = 0; j < 4; j++)
                vertices[first + j] += new Vector3(Random(step, index, j * 2), Random(step, index, j * 2 + 1), 0f) * unit;
        }


        private float Random(int step, int index, int channel)
        {
            uint x = (uint)seed * 2654435761u;
            x ^= (uint)step * 747796405u;
            x ^= (uint)index * 2891336453u;
            x ^= (uint)channel * 277803737u;

            x ^= x >> 16;
            x *= 0x7feb352du;
            x ^= x >> 15;
            x *= 0x846ca68bu;
            x ^= x >> 16;

            return x / (float)uint.MaxValue * 2f - 1f;
        }
        #endregion
    }
}
