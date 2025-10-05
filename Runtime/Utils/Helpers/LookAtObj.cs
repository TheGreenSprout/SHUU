using UnityEngine;

namespace SHUU.Utils.Helpers
{

public class LookAtObj : MonoBehaviour
{
    public Transform target;

    private Quaternion? cacheRotation;


    public float rotationSpeed = 5f;


    public bool lockX = false;
    public bool lockY = false;
    public bool lockZ = true;




    private void Awake() {
        cacheRotation = null;
    }


    private void Update()
    {
        if (target == null)
        {
            if (transform.rotation != cacheRotation && cacheRotation != null)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, (Quaternion)cacheRotation, rotationSpeed * Time.deltaTime);
            }
            else
            {
                cacheRotation = null;
            }
                

            return;
        }


        if (cacheRotation == null)
        {
            cacheRotation = this.transform.rotation;
        }


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