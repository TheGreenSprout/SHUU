using UnityEngine;

namespace SHUU.Utils.Helpers
{

public class LookAtObj : MonoBehaviour
{
    public Transform target;


    public float rotationSpeed = 5f;


    public bool lockX = false;
    public bool lockY = false;
    public bool lockZ = true;




    private void Update()
    {
        if (target == null) return;


        Vector3 direction = target.position - transform.position;

        Quaternion targetRotation = Quaternion.LookRotation(direction);


        Vector3 euler = targetRotation.eulerAngles;
        Vector3 currentEuler = transform.rotation.eulerAngles;

        if (lockX) euler.x = currentEuler.x;
        if (lockY) euler.y = currentEuler.y;
        if (lockZ) euler.z = currentEuler.z;

        targetRotation = Quaternion.Euler(euler);


        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}

}