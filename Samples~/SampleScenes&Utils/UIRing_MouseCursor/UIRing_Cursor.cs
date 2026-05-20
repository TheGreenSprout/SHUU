using UnityEngine;

using SHUU.Utils.Helpers.Interaction;
using SHUU.Utils.UI;

namespace SHUU.Samples.SampleScenesAndUtils.UIRing_MouseCursor
{
    public class UIRing_Cursor : DynamicCursor
    {
        #region Logic
        public UIRing uiRing;


        private float ring_radius;
        private float previous_ring_radius;

        private float target_ring_radius;

        private float base_ring_radius;
        [SerializeField] private float interact_ring_radius = 7.5f;


        [SerializeField] private float radiusLerp = 15f;

        [SerializeField] private float min_radiusLerp_difference = 0.01f;
        #endregion




        #region Logic
        private void Awake()
        {
            ring_radius = uiRing.GetRadius();
            base_ring_radius = ring_radius;

            previous_ring_radius = 0f;
        }


        private void Update()
        {
            if (cursorActive) target_ring_radius = interact_ring_radius;
            else target_ring_radius = base_ring_radius;


            if (Mathf.Abs(target_ring_radius - ring_radius) > min_radiusLerp_difference) ring_radius = Mathf.Lerp(ring_radius, target_ring_radius, radiusLerp * Time.deltaTime);
            else ring_radius = target_ring_radius;


            if (ring_radius != previous_ring_radius) uiRing.SetRadius(ring_radius);
            
            previous_ring_radius = ring_radius;
        }
        #endregion
    }
}
