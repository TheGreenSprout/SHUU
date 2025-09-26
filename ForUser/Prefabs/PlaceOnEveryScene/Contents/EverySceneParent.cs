using UnityEngine;

namespace SHUU.ForUser
{

public class EverySceneParent : MonoBehaviour
{
    private void Awake()
    {
        Invoke(nameof(DestroyThisObj), 0.2f);
    }
    
    private void DestroyThisObj()
    {
        Destroy(this.gameObject);
    }
}

}