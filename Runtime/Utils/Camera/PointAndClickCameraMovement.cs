using UnityEngine;

using SHUU.Utils.Helpers;

public class PointAndClickCameraMovement : MonoBehaviour
{
    #region Variables
    [SerializeField] private float sensitivity = 0.1f;


    [SerializeField] private float minXRotation = -30f;
    [SerializeField] private float maxXRotation = 30f;
    [SerializeField] private float minYRotation = -30f;
    [SerializeField] private float maxYRotation = 30f;



    [Tooltip("If greater than 0 it will smoothly enable/disable the camera movement")]
    [SerializeField] [Min(0f)] private float smoothTime = 0.25f;

    private float influence = 0f;
    private float targetInfluence = 1f;



    private Vector2 screenCenter;
    #endregion




    #region Main
    private void Awake()
    {
        screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        influence = 0f;
    }


    private void OnEnable()
    {
        if (!disable) targetInfluence = 1f;
    }

    private bool disable = false;
    private void OnDisable()
    {
        if (smoothTime <= 0f)
        {
            transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            
            influence = 0f;

            return;
        }


        if (disable)
        {
            disable = false;

            transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

            influence = 0f;

            return;
        }

        
        disable = true;

        targetInfluence = 0f;

        this.enabled = true;
    }


    private void Update()
    {
        if (smoothTime > 0f)
        {
            influence = Mathf.MoveTowards(
                influence,
                targetInfluence,
                Time.deltaTime / smoothTime
            );

            influence = Mathf.Clamp01(influence);
        }
        else influence = targetInfluence;


        if (influence <= 0f && disable)
        {
            this.enabled = false;

            return;
        }


        Vector2 mousePosition = Input.mousePosition;

        Vector2 offset = (mousePosition - screenCenter) / screenCenter;


        float targetRotationX = Mathf.Clamp(offset.y * 10f * -sensitivity, minYRotation, maxYRotation);
        float targetRotationY = Mathf.Clamp(-offset.x * 10f * -sensitivity, minXRotation, maxXRotation);


        transform.localRotation = Quaternion.Euler(targetRotationX * influence, targetRotationY * influence, 0f);
    }
    #endregion



    #region Gizmos
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 origin = transform.position;
        Quaternion space = transform.parent != null ? transform.parent.rotation : Quaternion.identity;
        float length = SHUU_Gizmos.Size(origin, 2f);

        float reach = 10f * Mathf.Abs(sensitivity);

        DrawRange(origin, space, minYRotation, maxYRotation, minXRotation, maxXRotation, length, SHUU_Gizmos.Orange.WithAlpha(0.5f));
        DrawRange(origin, space, Mathf.Clamp(-reach, minYRotation, maxYRotation), Mathf.Clamp(reach, minYRotation, maxYRotation), Mathf.Clamp(-reach, minXRotation, maxXRotation), Mathf.Clamp(reach, minXRotation, maxXRotation), length, SHUU_Gizmos.Blue);

        SHUU_Gizmos.Line(origin, origin + space * Vector3.forward * length, SHUU_Gizmos.Green);
    }

    private static void DrawRange(Vector3 origin, Quaternion space, float minPitch, float maxPitch, float minYaw, float maxYaw, float length, Color color)
    {
        Vector3 Point(float pitch, float yaw) => origin + space * Quaternion.Euler(pitch, yaw, 0f) * Vector3.forward * length;

        const int steps = 12;

        for (int i = 0; i < steps; i++)
        {
            float a = i / (float)steps;
            float b = (i + 1) / (float)steps;

            SHUU_Gizmos.Line(Point(minPitch, Mathf.Lerp(minYaw, maxYaw, a)), Point(minPitch, Mathf.Lerp(minYaw, maxYaw, b)), color);
            SHUU_Gizmos.Line(Point(maxPitch, Mathf.Lerp(minYaw, maxYaw, a)), Point(maxPitch, Mathf.Lerp(minYaw, maxYaw, b)), color);
            SHUU_Gizmos.Line(Point(Mathf.Lerp(minPitch, maxPitch, a), minYaw), Point(Mathf.Lerp(minPitch, maxPitch, b), minYaw), color);
            SHUU_Gizmos.Line(Point(Mathf.Lerp(minPitch, maxPitch, a), maxYaw), Point(Mathf.Lerp(minPitch, maxPitch, b), maxYaw), color);
        }

        SHUU_Gizmos.Line(origin, Point(minPitch, minYaw), color);
        SHUU_Gizmos.Line(origin, Point(minPitch, maxYaw), color);
        SHUU_Gizmos.Line(origin, Point(maxPitch, minYaw), color);
        SHUU_Gizmos.Line(origin, Point(maxPitch, maxYaw), color);
    }
#endif
    #endregion
}
