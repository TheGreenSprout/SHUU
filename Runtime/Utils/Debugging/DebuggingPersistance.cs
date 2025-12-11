using UnityEngine;

namespace SHUU.Utils.Developer.Debugging
{
    public class DebuggingPersistance : MonoBehaviour
    {
        public static DebuggingPersistance Singleton_Instance { get; private set; }




        private void Awake()
        {
            if (Singleton_Instance != null && Singleton_Instance != this)
            {
                Destroy(gameObject);
                return;
            }


            Singleton_Instance = this;

            transform.parent = null;

            DontDestroyOnLoad(gameObject);
        }
    }
}
