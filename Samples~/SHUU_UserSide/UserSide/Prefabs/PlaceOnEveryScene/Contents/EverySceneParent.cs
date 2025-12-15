using UnityEngine;

namespace SHUU.UserSide
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