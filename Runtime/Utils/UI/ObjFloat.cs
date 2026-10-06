/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Alchemy.Inspector;

using SHUU.InnerWorkings.Preferences;

namespace SHUU.Utils.UI
{
    [DisallowMultipleComponent, HideScriptField, AddComponentMenu("SHUU/UI/UI Float")]
    public class ObjFloat : MonoBehaviour
    {
        #region Variables

        #region FloatPattern
        public enum FloatPattern
        {
            [InspectorName("Drift (each axis on its own)")] Drift,
            [InspectorName("Orbit (circle or ellipse)")] Orbit,
            [InspectorName("Bob (back and forth)")] Bob,
            [InspectorName("Figure eight")] FigureEight,
            [InspectorName("Wander (smooth random)")] Wander
        }
        #endregion



        #region EditorOptions
#if UNITY_EDITOR
        [System.Serializable]
        private class EditorOptions
        {
            public bool drawGizmos = true;

            [System.NonSerialized] public ObjFloat owner;

            [Button, LabelText("Test Shake"), HideInEditMode]
            private void TestShake()
            {
                if (owner != null) owner.ShakeAndScale();
            }
        }
#endif
        #endregion



        #region Inspector

        #region Float BoxGroup
        [Tooltip("Turns the floating on and off. The shake works either way.")]
        [BoxGroup("Float"), SerializeField] private bool floatEnabled = true;

        [Tooltip("The shape of the movement.\n\nDrift: each axis moves on its own rhythm, so every element traces its own path.\nOrbit: a clean circle (or an ellipse, if the axes differ) between X and Y, or X and Z.\nBob: back and forth along a line; with Axis Scale (0, 1, 0) that's straight up and down.\nFigure eight: a lying eight.\nWander: smooth random drifting.")]
        [BoxGroup("Float"), SerializeField] private FloatPattern pattern = FloatPattern.Drift;

        [Tooltip("How far it moves, in units (canvas units on UI, world units on anything else). Each axis is multiplied by Axis Scale. 0 = it doesn't float.")]
        [BoxGroup("Float")] public float floatStrength = 1.0f;

        [Tooltip("How fast it moves, in radians per second: 1 is one full cycle every 6.3 seconds, 6.28 is one every second. 0 = it doesn't move.")]
        [BoxGroup("Float")] public float floatSpeed = 1.0f;

        [Tooltip("Seconds the floating takes to ease in when it starts and to ease out when it stops, so it doesn't pop. 0 = right away.")]
        [BoxGroup("Float"), SerializeField, Min(0f)] private float fadeTime = 0.25f;

        [Tooltip("Every element starts at a random point of its movement, so a group of them doesn't move in sync. Turn it off to start all of them at the Start Phase.")]
        [BoxGroup("Float"), SerializeField] private bool randomStartPhase = true;

        [Tooltip("Where in its cycle it starts, in degrees (when Random Start Phase is off).")]
        [BoxGroup("Float"), ShowIf(nameof(FixedStartPhase)), Indent, SerializeField, Range(0f, 360f)] private float startPhase = 0f;

        [Tooltip("Ignore Time.timeScale, so it keeps moving while the game is paused (menus).")]
        [BoxGroup("Float"), SerializeField] private bool useUnscaledTime = true;
        #endregion


        #region Axes BoxGroup
        [Tooltip("How much of the strength each axis gets. (1, 0, 0) floats only side to side, (0, 1, 0) only up and down, (0, 0, 1) only in depth, (0, 0, 0) nothing. Which axes are used at all is set by the X, Y and Z toggles below.")]
        [BoxGroup("Axes"), SerializeField] private Vector3 axisScale = Vector3.one;

        [Tooltip("Turns the movement's axes, in degrees, so it can go along a diagonal or in another plane. 0 = the parent's own axes.")]
        [BoxGroup("Axes"), SerializeField] private Vector3 axisRotation = Vector3.zero;

        [Tooltip("A speed multiplier for each axis. When they differ the path becomes a woven curve (like (1, 2, 1) on Bob).")]
        [BoxGroup("Axes"), SerializeField] private Vector3 axisSpeed = Vector3.one;

        [Tooltip("On: the float and the shake can move it along X. Off: X is locked. (Turning has its own angle per axis, where 0 = no turn.)")]
        [BoxGroup("Axes"), LabelText("X"), SerializeField] private bool haveX = false;
        private bool lockX => !haveX;

        [Tooltip("On: the float and the shake can move it along Y. Off: Y is locked.")]
        [BoxGroup("Axes"), LabelText("Y"), SerializeField] private bool haveY = true;
        private bool lockY => !haveY;

        [Tooltip("On: the float and the shake can move it along Z. Off: Z is locked.")]
        [BoxGroup("Axes"), LabelText("Z"), SerializeField] private bool haveZ = false;
        private bool lockZ => !haveZ;
        #endregion


        #region Sway FoldoutGroup
        [Tooltip("How far it turns around each axis, in degrees either way. 0 = it doesn't turn on that axis.")]
        [FoldoutGroup("Sway"), SerializeField] private Vector3 swayAngles = Vector3.zero;

        [Tooltip("How fast it turns, in radians per second.")]
        [FoldoutGroup("Sway"), SerializeField] private float swaySpeed = 1f;
        #endregion


        #region Pulse FoldoutGroup
        [Tooltip("How much it grows and shrinks, as a fraction of its size: 0.05 is 5% either way. 0 = it doesn't pulse.")]
        [FoldoutGroup("Pulse"), SerializeField, Range(0f, 0.5f)] private float pulseAmount = 0f;

        [Tooltip("How fast it pulses, in radians per second.")]
        [FoldoutGroup("Pulse"), SerializeField] private float pulseSpeed = 1f;
        #endregion


        #region Shake BoxGroup
        [Tooltip("How long the shake lasts, in seconds.")]
        [BoxGroup("Shake")] public float shakeDuration = 0.25f;

        [Tooltip("How far the shake throws it, in units.")]
        [BoxGroup("Shake")] public float shakeMagnitude = 0.5f;

        [Tooltip("How big it gets at the peak of the pop, as a multiplier of its size: 1.5 is 50% bigger. 1 = it doesn't change size.")]
        [BoxGroup("Shake")] public float scaleMagnitude = 1.2f;

        [Tooltip("Keep floating while it shakes. When off, the floating eases out for the shake and back in after it.")]
        [BoxGroup("Shake"), SerializeField] private bool floatWhileShaking = true;
        #endregion


        #region Shake Details FoldoutGroup
        [Tooltip("How much of the shake each axis gets. (1, 0, 0) shakes only side to side.")]
        [FoldoutGroup("Shake Details"), SerializeField] private Vector3 shakeAxisScale = Vector3.one;

        [Tooltip("How far the shake turns it around each axis, in degrees either way. 0 = it doesn't turn on that axis.")]
        [FoldoutGroup("Shake Details"), SerializeField] private Vector3 shakeRotation = Vector3.zero;

        [Tooltip("How many times a second the shake jumps to a new place. Low numbers look like stop motion. 0 = every frame.")]
        [FoldoutGroup("Shake Details"), SerializeField, Min(0f)] private float shakeStepsPerSecond = 0f;

        [Tooltip("How strong the shake is over its duration: 1 is full strength, 0 is none. The default fades out.")]
        [FoldoutGroup("Shake Details"), SerializeField] private AnimationCurve shakeFalloff = AnimationCurve.Linear(0f, 1f, 1f, 0f);

        [Tooltip("The size of the pop over the duration of the shake: 0 is its normal size, 1 is the Scale Magnitude. The default swells quickly and settles back.")]
        [FoldoutGroup("Shake Details"), SerializeField] private AnimationCurve scaleCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.25f, 1f), new Keyframe(1f, 0f));
        #endregion


        #region Events FoldoutGroup
        [FoldoutGroup("Events"), SerializeField] private UnityEvent onShakeStart = new();

        [FoldoutGroup("Events"), SerializeField] private UnityEvent onShakeEnd = new();
        #endregion
        

        #region Editor InlineGroup
#if UNITY_EDITOR
        [InlineGroup("Editor"), SerializeField] private EditorOptions editor = new();
#endif
        #endregion

        #endregion



        #region Internal
        [SerializeField, HideInInspector] private int dataVersion;

        private RectTransform rect;

        private float elapsed;
        private float floatWeight;

        private Vector3 randomPhase;
        private Vector3 noiseSeed;

        private bool shaking;
        private float shakeTime;
        private float shakeIntensity = 1f;
        private float shakeStepTimer;
        private Vector3 shakeDirection;
        private Vector3 shakeTurn;

        private Vector3 basePosition;
        private Vector3 writtenPosition;
        private bool positionTouched;

        private Quaternion baseRotation;
        private Quaternion writtenRotation;
        private bool rotationTouched;

        private Vector3 baseScale;
        private Vector3 writtenScale;
        private bool scaleTouched;

        private Vector3 appliedOffset;



        private static bool DebugLogEmission => SHUUPreferences_UI.Instance != null && SHUUPreferences_UI.Instance.debugLogEmission;
        #endregion



        #region Fetchers
        public bool FloatEnabled
        {
            get => floatEnabled;
            set => floatEnabled = value;
        }

        public FloatPattern Pattern
        {
            get => pattern;
            set => pattern = value;
        }

        public Vector3 AxisScale
        {
            get => axisScale;
            set => axisScale = value;
        }

        public Vector3 AxisRotation
        {
            get => axisRotation;
            set => axisRotation = value;
        }

        public bool LockX
        {
            get => lockX;
            set => haveX = !value;
        }

        public bool LockY
        {
            get => lockY;
            set => haveY = !value;
        }

        public bool LockZ
        {
            get => lockZ;
            set => haveZ = !value;
        }

        public bool IsShaking => shaking;

        private bool FixedStartPhase => !randomStartPhase;
        #endregion



        #region Position
        private Vector3 Position
        {
            get => rect != null ? rect.anchoredPosition3D : transform.localPosition;
            set
            {
                if (rect != null) rect.anchoredPosition3D = value;
                else transform.localPosition = value;
            }
        }
        #endregion

        #endregion




        #region Main
        private void Awake()
        {
            rect = GetComponent<RectTransform>();

            randomPhase = new Vector3(Random.Range(0f, 2f * Mathf.PI), Random.Range(0f, 2f * Mathf.PI), Random.Range(0f, 2f * Mathf.PI));
            noiseSeed = new Vector3(Random.Range(0f, 1000f), Random.Range(0f, 1000f), Random.Range(0f, 1000f));
        }

        private void OnEnable()
        {
            floatWeight = 0f;

#if UNITY_EDITOR
            editor.owner = this;
#endif
        }

        private void OnDisable()
        {
            shaking = false;

            ApplyPosition(Vector3.zero);
            ApplyRotation(Vector3.zero);
            ApplyScale(1f);

            appliedOffset = Vector3.zero;
        }


        private void OnValidate()
        {
            Migrate();

            shakeDuration = Mathf.Max(0f, shakeDuration);
            shakeMagnitude = Mathf.Max(0f, shakeMagnitude);
            scaleMagnitude = Mathf.Max(0f, scaleMagnitude);
        }

        private void Migrate()
        {
            if (dataVersion >= 1) return;

            dataVersion = 1;

            if (Mathf.Approximately(axisScale.z, 0f)) axisScale.z = 1f;
            if (Mathf.Approximately(shakeAxisScale.z, 0f)) shakeAxisScale.z = 1f;
        }

        private void Reset()
        {
            if (GetComponent<RectTransform>() != null) return;

            floatStrength = 0.1f;
            shakeMagnitude = 0.1f;
        }


        private void LateUpdate()
        {
            float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            elapsed += deltaTime;

            bool floating = floatEnabled && (!shaking || floatWhileShaking);
            float target = floating ? 1f : 0f;

            floatWeight = fadeTime > 0f ? Mathf.MoveTowards(floatWeight, target, deltaTime / fadeTime) : target;

            Vector3 offset = Vector3.zero;
            Vector3 angles = Vector3.zero;
            float scale = 1f;

            if (floatWeight > 0f)
            {
                offset = FloatOffset() * floatWeight;

                if (swayAngles != Vector3.zero)
                {
                    angles = new Vector3
                    (
                        Mathf.Sin(elapsed * swaySpeed + PhaseX) * swayAngles.x,
                        Mathf.Sin(elapsed * swaySpeed + PhaseY) * swayAngles.y,
                        Mathf.Sin(elapsed * swaySpeed + PhaseZ) * swayAngles.z
                    ) * floatWeight;
                }

                if (pulseAmount != 0f) scale = 1f + Mathf.Sin(elapsed * pulseSpeed + PhaseY) * pulseAmount * floatWeight;
            }

            if (shaking) UpdateShake(deltaTime, ref offset, ref angles, ref scale);

            offset = ApplyLocks(offset);
            appliedOffset = offset;

            ApplyPosition(offset);
            ApplyRotation(angles);
            ApplyScale(scale);
        }
        #endregion




        #region Logic

        #region Shake
        public void ShakeAndScale() => ShakeAndScale(1f);

        public void ShakeAndScale(float intensity)
        {
            if (!isActiveAndEnabled)
            {
                if (DebugLogEmission) Debug.LogWarning("UIfloat can't shake while it's disabled.", this);

                return;
            }

            if (shakeDuration <= 0f) return;

            bool wasShaking = shaking;

            shaking = true;
            shakeTime = 0f;
            shakeStepTimer = 0f;
            shakeIntensity = Mathf.Max(0f, intensity);

            if (!wasShaking) onShakeStart?.Invoke();
        }

        public void StopShake()
        {
            if (shaking) EndShake();
        }


        private void UpdateShake(float deltaTime, ref Vector3 offset, ref Vector3 angles, ref float scale)
        {
            shakeTime += deltaTime;

            float t = shakeDuration > 0f ? shakeTime / shakeDuration : 1f;

            if (t >= 1f)
            {
                EndShake();

                return;
            }

            shakeStepTimer -= deltaTime;

            if (shakeStepTimer <= 0f)
            {
                shakeDirection = RandomVector();
                shakeTurn = RandomVector();

                shakeStepTimer = shakeStepsPerSecond > 0f ? 1f / shakeStepsPerSecond : 0f;
            }

            float strength = Evaluate(shakeFalloff, t, 1f - t) * shakeIntensity;
            float pop = Evaluate(scaleCurve, t, Mathf.Sin(t * Mathf.PI)) * shakeIntensity;

            offset += Vector3.Scale(shakeDirection, shakeAxisScale) * (shakeMagnitude * strength);
            angles += Vector3.Scale(shakeTurn, shakeRotation) * strength;
            scale *= 1f + (scaleMagnitude - 1f) * pop;
        }

        private void EndShake()
        {
            shaking = false;

            onShakeEnd?.Invoke();
        }

        private static Vector3 RandomVector() => new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));

        private static float Evaluate(AnimationCurve curve, float t, float fallback) => curve != null && curve.length > 0 ? curve.Evaluate(t) : fallback;
        #endregion


        #region Float
        private float PhaseX => randomStartPhase ? randomPhase.x : startPhase * Mathf.Deg2Rad;
        private float PhaseY => randomStartPhase ? (pattern == FloatPattern.Drift ? randomPhase.y : randomPhase.x) : startPhase * Mathf.Deg2Rad;
        private float PhaseZ => randomStartPhase ? (pattern == FloatPattern.Drift ? randomPhase.z : randomPhase.x) : startPhase * Mathf.Deg2Rad;


        private Vector3 FloatOffset()
        {
            Vector3 amplitude = floatStrength * axisScale;

            if (amplitude.sqrMagnitude < 1e-10f) return Vector3.zero;

            float tx = elapsed * floatSpeed * axisSpeed.x;
            float ty = elapsed * floatSpeed * axisSpeed.y;
            float tz = elapsed * floatSpeed * axisSpeed.z;

            Vector3 shape;

            switch (pattern)
            {
                case FloatPattern.Bob:
                    shape = new Vector3(Mathf.Sin(tx + PhaseX), Mathf.Sin(ty + PhaseY), Mathf.Sin(tz + PhaseZ));
                    break;

                case FloatPattern.FigureEight:
                    shape = new Vector3(Mathf.Sin(tx + PhaseX), Mathf.Sin(2f * (ty + PhaseY)), Mathf.Cos(tz + PhaseZ));
                    break;

                case FloatPattern.Wander:
                    shape = new Vector3(Noise(noiseSeed.x, tx), Noise(noiseSeed.y, ty), Noise(noiseSeed.z, tz));
                    break;

                default:
                    shape = new Vector3(Mathf.Sin(tx + PhaseX), Mathf.Cos(ty + PhaseY), Mathf.Cos(tz + PhaseZ));
                    break;
            }

            Vector3 offset = Vector3.Scale(shape, amplitude);

            if (axisRotation != Vector3.zero) offset = Quaternion.Euler(axisRotation) * offset;

            return offset;
        }

        private static float Noise(float seed, float time) => Mathf.Clamp((Mathf.PerlinNoise(seed, time * 0.35f) - 0.5f) * 2.4f, -1f, 1f);


        private Vector3 ApplyLocks(Vector3 value)
        {
            if (lockX) value.x = 0f;
            if (lockY) value.y = 0f;
            if (lockZ) value.z = 0f;

            return value;
        }
        #endregion


        #region Applying
        private void ApplyPosition(Vector3 offset)
        {
            bool active = offset.sqrMagnitude > 1e-10f;

            if (!active && !positionTouched) return;

            Vector3 current = Position;

            if (!positionTouched || (current - writtenPosition).sqrMagnitude > 1e-6f) basePosition = current;

            Position = basePosition + offset;

            writtenPosition = Position;
            positionTouched = active;
        }

        private void ApplyRotation(Vector3 angles)
        {
            bool active = angles.sqrMagnitude > 1e-8f;

            if (!active && !rotationTouched) return;

            if (!rotationTouched || Quaternion.Angle(transform.localRotation, writtenRotation) > 0.01f) baseRotation = transform.localRotation;

            transform.localRotation = baseRotation * Quaternion.Euler(angles);

            writtenRotation = transform.localRotation;
            rotationTouched = active;
        }

        private void ApplyScale(float factor)
        {
            bool active = Mathf.Abs(factor - 1f) > 1e-5f;

            if (!active && !scaleTouched) return;

            if (!scaleTouched || (transform.localScale - writtenScale).sqrMagnitude > 1e-8f) baseScale = transform.localScale;

            transform.localScale = rect != null ? new Vector3(baseScale.x * factor, baseScale.y * factor, baseScale.z) : baseScale * factor;

            writtenScale = transform.localScale;
            scaleTouched = active;
        }
        #endregion

        #endregion



        #region Editor
#if UNITY_EDITOR
        private static readonly List<(Vector3 from, Vector3 to)> gizmoSegments = new();
        private static readonly float[] gizmoSigns = { -1f, 1f };


        private void OnDrawGizmosSelected()
        {
            if (!editor.drawGizmos) return;


            Matrix4x4 space = transform.parent != null ? transform.parent.localToWorldMatrix : Matrix4x4.identity;
            Vector3 home = transform.localPosition - appliedOffset;

            Vector3 floatArea = ApplyLocks(floatStrength * axisScale);
            Vector3 shakeArea = ApplyLocks(shakeMagnitude * shakeAxisScale);

            Matrix4x4 floatSpace = space * Matrix4x4.TRS(home, Quaternion.Euler(axisRotation), Vector3.one);
            Matrix4x4 shakeSpace = space * Matrix4x4.TRS(home, Quaternion.identity, Vector3.one);

            bool floatOnTop = FloatOnTop(floatArea, shakeArea);

            Color floatColor = new Color(0.35f, 0.85f, 1f, floatOnTop ? 1f : 0.55f);
            Color shakeColor = new Color(1f, 0.6f, 0.25f, floatOnTop ? 0.55f : 1f);

            Matrix4x4 lift = Matrix4x4.Translate(GizmoLift(space.MultiplyPoint3x4(home)));

            if (floatOnTop)
            {
                DrawArea(shakeSpace, shakeArea, shakeColor, false);
                DrawArea(lift * floatSpace, floatArea, floatColor, pattern == FloatPattern.Bob);
            }
            else
            {
                DrawArea(floatSpace, floatArea, floatColor, pattern == FloatPattern.Bob);
                DrawArea(lift * shakeSpace, shakeArea, shakeColor, false);
            }

            Gizmos.matrix = Matrix4x4.identity;
        }

        private static bool FloatOnTop(Vector3 floatArea, Vector3 shakeArea) => floatArea.sqrMagnitude <= shakeArea.sqrMagnitude;

        private static Vector3 GizmoLift(Vector3 worldPoint)
        {
            Camera view = Camera.current;

            if (view == null) return Vector3.zero;

            float distance = view.orthographic ? view.orthographicSize : Vector3.Distance(view.transform.position, worldPoint);

            return -view.transform.forward * (distance * 0.002f);
        }

        private static void DrawArea(Matrix4x4 space, Vector3 extent, Color color, bool path)
        {
            gizmoSegments.Clear();
            AreaSegments(extent, path, gizmoSegments);

            Gizmos.matrix = space;
            Gizmos.color = color;

            foreach ((Vector3 from, Vector3 to) in gizmoSegments) Gizmos.DrawLine(from, to);
        }

        private static void AreaSegments(Vector3 extent, bool path, List<(Vector3 from, Vector3 to)> segments)
        {
            bool x = Mathf.Abs(extent.x) > 1e-5f;
            bool y = Mathf.Abs(extent.y) > 1e-5f;
            bool z = Mathf.Abs(extent.z) > 1e-5f;

            Vector3 ex = x ? new Vector3(extent.x, 0f, 0f) : Vector3.zero;
            Vector3 ey = y ? new Vector3(0f, extent.y, 0f) : Vector3.zero;
            Vector3 ez = z ? new Vector3(0f, 0f, extent.z) : Vector3.zero;

            int active = (x ? 1 : 0) + (y ? 1 : 0) + (z ? 1 : 0);

            switch (active)
            {
                case 1:
                    Vector3 line = ex + ey + ez;

                    segments.Add((-line, line));
                    break;

                case 2:
                    Vector3 u = x ? ex : ey;
                    Vector3 v = z ? ez : ey;

                    segments.Add((-u - v, u - v));
                    segments.Add((u - v, u + v));
                    segments.Add((u + v, -u + v));
                    segments.Add((-u + v, -u - v));
                    break;

                case 3:
                    foreach (float a in gizmoSigns)
                    {
                        foreach (float b in gizmoSigns)
                        {
                            segments.Add((-ex + a * ey + b * ez, ex + a * ey + b * ez));
                            segments.Add((a * ex - ey + b * ez, a * ex + ey + b * ez));
                            segments.Add((a * ex + b * ey - ez, a * ex + b * ey + ez));
                        }
                    }
                    break;
            }

            if (path && active > 1) segments.Add((-(ex + ey + ez), ex + ey + ez));
        }
#endif
        #endregion
    }
}
