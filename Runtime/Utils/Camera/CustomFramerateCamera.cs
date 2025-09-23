using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CustomFramerateCamera : MonoBehaviour
{
    private void Awake()
    {
        CustomFramerateManager.camerasToRender.Add(this.GetComponent<Camera>());
    }
}
