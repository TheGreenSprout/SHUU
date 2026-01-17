using UnityEngine;

public class PointAndClickCameraMovement : MonoBehaviour
{
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
        else
        {
            influence = targetInfluence;
        }


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
}
