using UnityEngine;

public class PointAndClickCameraMovement : MonoBehaviour
{
    [SerializeField] private float sensitivity = 0.1f;


    [SerializeField] private float minXRotation = -30f;
    [SerializeField] private float maxXRotation = 30f;
    [SerializeField] private float minYRotation = -30f;
    [SerializeField] private float maxYRotation = 30f;



    private Vector2 screenCenter;




    void Start()
    {
        screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
    }


    void Update()
    {
        Vector2 mousePosition = Input.mousePosition;

        Vector2 offset = (mousePosition - screenCenter) / screenCenter;


        float targetRotationX = Mathf.Clamp(offset.y * 10 * -sensitivity, minYRotation, maxYRotation);
        float targetRotationY = Mathf.Clamp(-offset.x * 10 * -sensitivity, minXRotation, maxXRotation);


        transform.localRotation = Quaternion.Euler(targetRotationX, targetRotationY, 0);
    }
}
