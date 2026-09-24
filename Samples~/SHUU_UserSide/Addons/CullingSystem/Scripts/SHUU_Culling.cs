using System.Collections.Generic;
using UnityEngine;

using SHUU.Utils.Helpers;

namespace SHUU.UserSide.Addons.CullingSystem
{
    [DefaultExecutionOrder(-100)]
    public class SHUU_Culling : HiddenSingleton_MonoBehaviour<SHUU_Culling>
    {
        #region Variables
        protected override bool PersistantSingleton() => false;



        public Camera targetCamera;

        [SerializeField, Min(1)] private int objectsCheckedPerFrame = 20;



        private readonly List<ICullable> cullables = new();
        private readonly Plane[] frustumPlanes = new Plane[6];

        private int nextIndex;


        private Camera Cam => targetCamera != null ? targetCamera : Camera.main;



        #region Info
        public int RegisteredCount => cullables.Count;

        public int CulledCount
        {
            get
            {
                int count = 0;
                foreach (ICullable cullable in cullables)
                    if (cullable.IsCulled) count++;

                return count;
            }
        }

        public IReadOnlyList<ICullable> Cullables => cullables;
        #endregion

        #endregion




        #region Main
        private void Update()
        {
            if (cullables.Count == 0) return;

            Camera cam = Cam;
            if (cam == null) return;

            GeometryUtility.CalculateFrustumPlanes(cam, frustumPlanes);


            int checkCount = Mathf.Min(objectsCheckedPerFrame, cullables.Count);

            for (int i = 0; i < checkCount; i++)
            {
                nextIndex %= cullables.Count;

                Evaluate(cullables[nextIndex], cam);

                nextIndex++;
            }
        }


        public void ForceReevaluate()
        {
            Camera cam = Cam;
            if (cam != null) GeometryUtility.CalculateFrustumPlanes(cam, frustumPlanes);

            foreach (ICullable cullable in cullables)
                Evaluate(cullable, cam);
        }
        #endregion



        #region Logic

        #region Registration
        public void Register(ICullable cullable)
        {
            if (cullable == null || cullables.Contains(cullable)) return;

            cullables.Add(cullable);

            Camera cam = Cam;
            if (cam != null) GeometryUtility.CalculateFrustumPlanes(cam, frustumPlanes);

            Evaluate(cullable, cam);
        }

        public void Unregister(ICullable cullable) => cullables.Remove(cullable);
        #endregion



        #region Misc
        private void Evaluate(ICullable cullable, Camera cam)
        {
            bool visible = IsVisible(cullable, cam);

            if (visible == !cullable.IsCulled) return;

            if (visible) cullable.OnUnculled();
            else cullable.OnCulled();
        }

        private bool IsVisible(ICullable cullable, Camera cam)
        {
            Transform t = cullable.CullTransform;
            if (t == null || cam == null) return true;


            if (cullable.UseDistanceCulling)
            {
                float sqrDist = (t.position - cam.transform.position).sqrMagnitude;
                if (sqrDist > cullable.MaxDistance * cullable.MaxDistance) return false;
            }

            if (cullable.UseFrustumCulling)
            {
                if (!GeometryUtility.TestPlanesAABB(frustumPlanes, new Bounds(t.position, Vector3.zero))) return false;
            }

            return true;
        }
        #endregion

        #endregion
    }
}
