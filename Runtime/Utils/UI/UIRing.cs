/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



using UnityEngine;
using UnityEngine.UI;

namespace SHUU.Utils.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class UIRing : MaskableGraphic
    {
        #region Variables
        [Range(3, 360)] [SerializeField] private int segments = 100;


        [SerializeField] private float radius = 100f;

        [SerializeField] private float thickness = 10f;
        #endregion




        #region Main
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            float angleStep = 360f / segments;
            float outerRadius = radius;
            float innerRadius = radius - thickness;

            for (int i = 0; i < segments; i++)
            {
                float angle = angleStep * i * Mathf.Deg2Rad;
                float nextAngle = angleStep * (i + 1) * Mathf.Deg2Rad;

                Vector2 outerStart = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * outerRadius;
                Vector2 outerEnd   = new Vector2(Mathf.Cos(nextAngle), Mathf.Sin(nextAngle)) * outerRadius;
                Vector2 innerStart = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * innerRadius;
                Vector2 innerEnd   = new Vector2(Mathf.Cos(nextAngle), Mathf.Sin(nextAngle)) * innerRadius;

                int startIndex = vh.currentVertCount;

                vh.AddVert(outerStart, color, Vector2.zero);
                vh.AddVert(outerEnd, color, Vector2.zero);
                vh.AddVert(innerEnd, color, Vector2.zero);
                vh.AddVert(innerStart, color, Vector2.zero);

                vh.AddTriangle(startIndex + 0, startIndex + 1, startIndex + 2);
                vh.AddTriangle(startIndex + 2, startIndex + 3, startIndex + 0);
            }
        }
        #endregion



        #region Logic
        public void SetRadius(float r)
        {
            radius = r;

            SetVerticesDirty();
        }

        public float GetRadius() => radius;
        #endregion
    }
}
