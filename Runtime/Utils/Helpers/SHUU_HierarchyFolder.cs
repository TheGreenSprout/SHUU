using UnityEngine;
using Alchemy.Inspector;

namespace SHUU.Utils
{
    [ExecuteAlways, HideScriptField]
    public class SHUU_HierarchyFolder : MonoBehaviour
    {
        #region Enums
        public enum TextAlignment  { Left, Center }
        public enum DissolveOnPlay { Off, PlayMode, Build }
        public enum PrefabTintMode { None, Root, Always }
        #endregion




        #region Variables

        [SerializeField] private DissolveOnPlay dissolveOnPlay = DissolveOnPlay.Build;



        #region Style BoxGroup
        [BoxGroup("Style")] public FontStyle textStyle = FontStyle.Bold;
        [BoxGroup("Style")] public TextAlignment textAlignment = TextAlignment.Left;


        [BoxGroup("Style"), Range(0.5f, 1.3f)] public float iconSize = 1f;

        [BoxGroup("Style")] public Vector2 iconOffset = new Vector2(-1.2f, 0f);

        [BoxGroup("Style"), LabelText("Custom Icon")] public Texture2D customIcon = null;
        [BoxGroup("Style"), LabelText("Custom Gradient")] public Texture2D customGradient = null;
        #endregion


        
        #region Colors BoxGroup
        [BoxGroup("Colors"), LabelText("Icon")] public bool coloredIcon = true;

        [BoxGroup("Colors"), ShowIf("coloredIcon"), HideLabel] public Color iconColor = new Color(0.82f, 0.82f, 0.82f, 1f);


        [BoxGroup("Colors"), LabelText("Text"), HorizontalLine] public bool coloredText = false;

        [BoxGroup("Colors"), ShowIf("coloredText"), HideLabel] public Color textColor = new Color(0.82f, 0.82f, 0.82f, 1f);


        [BoxGroup("Colors"), LabelText("Highlight"), HorizontalLine] public bool coloredHighlight = false;

        [BoxGroup("Colors"), ShowIf("coloredHighlight"), HideLabel] public Color highlightColor = new Color(0.4f, 0.75f, 1f, 1f);
        #endregion



        #region Prefab BoxGroup
        [BoxGroup("Prefab")] public PrefabTintMode prefabTint = PrefabTintMode.Always;

        private bool _prefabTintEnabled => prefabTint != PrefabTintMode.None;


        [BoxGroup("Prefab"), ShowIf("_prefabTintEnabled"), LabelText("Icon Color")] public Color prefabIconTint = new Color(0.5f, 0.85f, 1f, 1f);
        [BoxGroup("Prefab"), ShowIf("_prefabTintEnabled"), LabelText("Text Color")] public Color prefabTextTint = new Color(0.45f, 0.62f, 0.86f, 1f);
        [BoxGroup("Prefab"), ShowIf("_prefabTintEnabled"), LabelText("Highlight Color")] public Color prefabHighlightTint = new Color(0.5f, 0.85f, 1f, 1f);

        [BoxGroup("Prefab"), ShowIf("_prefabTintEnabled"), LabelText("Strength"), Range(0f, 1f)] public float prefabTintStrength = 0.8f;
        #endregion

        #endregion




        #region Main
        private void Awake()
        {
            if (!Application.isPlaying || dissolveOnPlay == DissolveOnPlay.Off) return;
            
            #if UNITY_EDITOR
            if (dissolveOnPlay != DissolveOnPlay.PlayMode) return;
            #endif


            Dissolve();
        }

        public void Dissolve()
        {
            Transform parent = transform.parent;

            while (transform.childCount > 0)
                transform.GetChild(0).SetParent(parent, true);

            Destroy(gameObject);
        }
        #endregion
    }
}
