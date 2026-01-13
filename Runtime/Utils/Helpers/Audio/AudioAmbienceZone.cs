using System;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

namespace SHUU.Utils.Helpers.Audio
{
    public class AudioAmbienceZone : MonoBehaviour
    {
        public enum AmbienceZoneType
        {
            Collider,
            Path
        }
        public AmbienceZoneType ambienceZoneType = AmbienceZoneType.Collider;

        [SerializeReference] public IAudioAmbienceZone_Data data;


        [SerializeField] private Transform player;
        [SerializeField] private Transform ambience;




        private void Awake()
        {
            EnsureCorrectData();

            data?.Setup();
        }

        private void OnValidate() => EnsureCorrectData();


        private void EnsureCorrectData()
        {
            Type wantedType = ambienceZoneType switch
            {
                AmbienceZoneType.Collider => typeof(AudioAmbienceZone_Collider),
                AmbienceZoneType.Path => typeof(AudioAmbienceZone_Path),
                _ => null
            };

            if (wantedType == null) return;

            if (data == null || data.GetType() != wantedType) data = (IAudioAmbienceZone_Data)Activator.CreateInstance(wantedType);
        }


        private void Update()
        {
            if (data == null || player == null || ambience == null) return;

            ambience.position = data.Logic(player);
        }
    }




    public interface IAudioAmbienceZone_Data
    {
        public abstract void Setup();
        public abstract Vector3 Logic(Transform player);
    }



    [Serializable]
    public class AudioAmbienceZone_Collider : IAudioAmbienceZone_Data
    {
        public Collider collider;
        
        public void Setup()
        {
            if (collider is MeshCollider) ((MeshCollider)collider).convex = true;
        }
        public Vector3 Logic(Transform player) => collider.ClosestPoint(player.position);
    }


    [Serializable]
    public class AudioAmbienceZone_Path : IAudioAmbienceZone_Data
    {
        public SplineContainer splineContainer;
        public int resolution = 16;

        [Tooltip("The spline must be closed (loop)")]
        public bool interpretAsArea = false;

        public void Setup() { }
        public Vector3 Logic(Transform player)
        {
            if (interpretAsArea && splineContainer.Spline.Closed) return AreaLogic(player);
            
            return PathLogic(player);
        }

        private Vector3 AreaLogic(Transform player)
        {
            Spline spline = splineContainer.Spline;

            float3 localPlayerPos = splineContainer.transform.worldToLocalMatrix.MultiplyPoint3x4(player.position);

            SplineUtility.GetNearestPoint(
                spline,
                localPlayerPos,
                out float3 nearestLocal,
                out float t,
                resolution
            );

            Vector3 worldPos = splineContainer.transform.TransformPoint((Vector3)nearestLocal);

            Vector3 tangent = splineContainer.transform.TransformDirection(spline.EvaluateTangent(t)).normalized;

            Vector3 splineRight = Vector3.Cross(Vector3.up, tangent);
            Vector3 toPlayer = player.position - worldPos;

            if (Vector3.Dot(toPlayer, splineRight) > 0f) return player.position;

            return worldPos;
        }

        private Vector3 PathLogic(Transform player)
        {
            Spline spline = splineContainer.Spline;

            float3 localPlayerPos = splineContainer.transform.worldToLocalMatrix.MultiplyPoint3x4(player.position);

            SplineUtility.GetNearestPoint(
                spline,
                localPlayerPos,
                out float3 nearestLocal,
                out float t,
                resolution
            );

            return splineContainer.transform.TransformPoint((Vector3)nearestLocal);
        }
    }
}
