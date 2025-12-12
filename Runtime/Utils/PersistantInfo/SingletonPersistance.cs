using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SHUU.Utils.SceneManagement;

namespace SHUU.Utils.PersistantInfo
{

#region XML doc
/// <summary>
/// Handles logic related to keeping track of all the Singleton persistance scripts, and manages them.
/// </summary>
#endregion
public class SingletonPersistance : MonoBehaviour
{
    public static SingletonPersistance Singleton_Instance { get; private set; }



    public string IDENTIFIER = "Singleton";


    [SerializeField] private GameObject scripts;




    private void Awake()
    {
        if (Singleton_Instance != null && Singleton_Instance != this && this.IDENTIFIER == Singleton_Instance.IDENTIFIER)
        {
            Destroy(gameObject);

            return;
        }



        Singleton_Instance = this;

        transform.parent = null;

        DontDestroyOnLoad(gameObject);


        scripts.SetActive(true);
    }
}

}
