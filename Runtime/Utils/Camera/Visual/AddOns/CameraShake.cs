using UnityEngine;
using UnityEngine.Rendering;

using Alchemy.Inspector;

namespace SHUU.UserSide.Addons.CameraShakeSystem
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class CameraShake : MonoBehaviour
    {
        #region Variables
        [Tooltip("Max offset at full trauma, along the camera's own axes.")]
        [SerializeField, BoxGroup("Shake")] private Vector3 maxPositionOffset = new Vector3(0.3f, 0.3f, 0f);

        [Tooltip("Max rotation at full trauma, in degrees (pitch, yaw, roll).")]
        [SerializeField, BoxGroup("Shake")] private Vector3 maxRotationOffset = new Vector3(1.5f, 1.5f, 3f);

        [Tooltip("How fast the shake wobbles.")]
        [SerializeField, BoxGroup("Shake"), Min(0.01f)] private float frequency = 25f;

        [Tooltip("Shake strength is trauma raised to this. Higher keeps small shakes subtle and lets big ones hit hard.")]
        [SerializeField, BoxGroup("Shake"), Min(1f)] private float traumaExponent = 2f;


        [Tooltip("Trauma lost per second after AddTrauma.")]
        [SerializeField, BoxGroup("Timing"), Min(0.01f)] private float decayPerSecond = 1.5f;

        [Tooltip("Keep shaking while Time.timeScale is 0 (paused / hit-stop).")]
        [SerializeField, BoxGroup("Timing")] private bool useUnscaledTime = false;



        public Camera Camera => cam;
        public float Trauma => trauma;
        public bool IsShaking => trauma > 0f;



        // Internal
        private Camera cam;

        private Vector3 savedPosition;
        private Quaternion savedRotation;
        private bool poseApplied;

        private float trauma;
        private float decayRate;
        private float noiseTime;
        private float seed;

        private Vector3 positionOffset;
        private Quaternion rotationOffset = Quaternion.identity;
        #endregion




        #region Main
        private void Awake()
        {
            cam = GetComponent<Camera>();
            seed = (float)(new System.Random(GetInstanceID()).NextDouble() * 1000.0);
        }

        private void OnEnable()
        {
            SHUU_CameraShake.Register(this);

            Camera.onPreCull += ApplyShake;
            Camera.onPostRender += RestorePose;
            RenderPipelineManager.beginCameraRendering += ApplyShake;
            RenderPipelineManager.endCameraRendering += RestorePose;
        }

        private void OnDisable()
        {
            Camera.onPreCull -= ApplyShake;
            Camera.onPostRender -= RestorePose;
            RenderPipelineManager.beginCameraRendering -= ApplyShake;
            RenderPipelineManager.endCameraRendering -= RestorePose;

            if (cam != null) RestorePose(cam);

            SHUU_CameraShake.Unregister(this);
        }


        private void Update()
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            if (trauma <= 0f || dt <= 0f)
            {
                positionOffset = Vector3.zero;
                rotationOffset = Quaternion.identity;
                return;
            }

            trauma = Mathf.Max(0f, trauma - decayRate * dt);
            noiseTime += dt * frequency;

            float strength = Mathf.Pow(trauma, traumaExponent);

            positionOffset = Vector3.Scale(maxPositionOffset, new Vector3(Noise(0), Noise(1), Noise(2))) * strength;
            rotationOffset = Quaternion.Euler(Vector3.Scale(maxRotationOffset, new Vector3(Noise(3), Noise(4), Noise(5))) * strength);
        }


        private float Noise(int channel) => Mathf.PerlinNoise(seed + channel * 17.31f, noiseTime) * 2f - 1f;
        #endregion




        #region Logic

        #region Shaking
        #region XML doc
        /// <summary>
        /// Adds to the current trauma (0-1 total). The shake then fades out at the "Decay Per Second" rate.
        /// </summary>
        /// <param name="amount">How much trauma to add. Around 0.2 is a small bump, 0.6+ is a big hit.</param>
        #endregion
        public void AddTrauma(float amount)
        {
            trauma = Mathf.Clamp01(trauma + amount);
            decayRate = decayPerSecond;
        }

        #region XML doc
        /// <summary>
        /// Starts a shake that fades out over the given duration. A shake weaker than the one already playing is ignored.
        /// </summary>
        /// <param name="intensity">Trauma to start at (0-1).</param>
        /// <param name="duration">Seconds until it fades out.</param>
        #endregion
        public void Shake(float intensity, float duration)
        {
            intensity = Mathf.Clamp01(intensity);
            if (intensity < trauma) return;

            trauma = intensity;
            decayRate = intensity / Mathf.Max(0.01f, duration);
        }

        public void Stop() => trauma = 0f;
        #endregion



        #region Rendering
        private void ApplyShake(ScriptableRenderContext _, Camera rendering) => ApplyShake(rendering);
        private void RestorePose(ScriptableRenderContext _, Camera rendering) => RestorePose(rendering);

        private void ApplyShake(Camera rendering)
        {
            if (rendering != cam || poseApplied || trauma <= 0f) return;

            Transform t = cam.transform;

            savedPosition = t.localPosition;
            savedRotation = t.localRotation;
            poseApplied = true;

            t.SetLocalPositionAndRotation(savedPosition + savedRotation * positionOffset, savedRotation * rotationOffset);
        }

        private void RestorePose(Camera rendering)
        {
            if (rendering != cam || !poseApplied) return;

            cam.transform.SetLocalPositionAndRotation(savedPosition, savedRotation);

            poseApplied = false;
        }
        #endregion

        #endregion




        #region Inspector Logic
        [Button]
        private void InspectorShake(float intensity = 0.6f, float duration = 0.5f)
        {
            if (Application.isPlaying) Shake(intensity, duration);
        }

        [Button]
        private void InspectorTrauma(float trauma = 0.3f)
        {
            if (Application.isPlaying) AddTrauma(trauma);
        }
        #endregion
    }
}
