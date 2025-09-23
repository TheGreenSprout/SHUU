using System;
using SHUU.Utils.Helpers;
using UnityEngine;

public class EverySceneParent : MonoBehaviour
{
    private void Awake()
    {
        Action destroyObj = () => Destroy(this.gameObject);

        SHUU_Timer.Create(0.2f, destroyObj);
    }
}
