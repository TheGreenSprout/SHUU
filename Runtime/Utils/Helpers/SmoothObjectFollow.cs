using UnityEngine;

public class SmoothObjectFollow : MonoBehaviour
{
    [Header("Control variables")]
    [SerializeField] private bool positionFollow = true;
    [SerializeField] private bool rotationFollow = true;


    [SerializeField] private Transform target;

    // how long (in seconds) it takes to get about 63% of the way there
    [SerializeField] private float followSmoothTime = 0.3f;



    [Header("Position Smoothing")]
    private Vector3 _posVelocity;


    [SerializeField] private float positionSmoothTime = 0.3f;




    void LateUpdate()
    {
        if (target == null) return;



        float t = 1f - Mathf.Exp(-Time.deltaTime / followSmoothTime);


        if (positionFollow)
        {
            transform.position = Vector3.SmoothDamp(transform.position, target.position, ref _posVelocity, positionSmoothTime);
        }
        
        if (rotationFollow)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, t);
        }
    }
}
