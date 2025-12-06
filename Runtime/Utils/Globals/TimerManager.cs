using UnityEngine;
using System;

namespace SHUU.Utils.Globals
{

#region XML doc
/// <summary>
/// Manages the creation and behaviour of timers.
/// </summary>
#endregion
public class TimerManager : MonoBehaviour
{
    private void Awake()
    {
        SHUU_GlobalsProxy.timerManager = this;
    }



    #region XML doc
    /// <summary>
    /// Creates a timer that, after the specified time, runs an Action.
    /// </summary>
    /// <param name="duration">The time the Action will be delayed by.</param>
    /// <param name="onComplete">The Action that will be performed.</param>
    #endregion
    public void Create(float duration, Action onComplete){
        GameObject obj = new GameObject("Timer");

        TimerManager instance = obj.AddComponent<TimerManager>();


        instance.StartCoroutine(instance.Run(duration, onComplete));
    }

    #region XML doc
    /// <summary>
    /// Creates a timer that, after the specified time, runs an Action. When creating it, it will be spawned as a child of another object.
    /// </summary>
    /// <param name="obj">The object that will parent the timer.</param>
    /// <param name="duration">The time the Action will be delayed by.</param>
    /// <param name="onComplete">The Action that will be performed.</param>
    #endregion
    public void CreateIn(GameObject obj, float duration, Action onComplete){
        TimerManager instance = obj.AddComponent<TimerManager>();


        instance.StartCoroutine(instance.Run(duration, onComplete));
    }
    
    #region XML doc
    /// <summary>
    /// Creates a timer that, after the specified time, runs an Action. When creating it, it will be spawned as a child of another object.
    /// </summary>
    /// <param name="obj">The object that will parent the timer.</param>
    /// <param name="duration">The time the Action will be delayed by.</param>
    /// <param name="onComplete">The Action that will be performed.</param>
    #endregion
    public void CreateAt(Transform pos, float duration, Action onComplete){
        GameObject obj = Instantiate(new GameObject("Timer"), pos);

        TimerManager instance = obj.AddComponent<TimerManager>();


        instance.StartCoroutine(instance.Run(duration, onComplete));
    }


    #region XML doc
    /// <summary>
    /// Creates a Courtine, runs an Action and destroys itself when done.
    /// </summary>
    /// <param name="duration">The time the Action will be delayed by.</param>
    /// <param name="onComplete">The Action that will be performed.</param>
    /// <returns>Returns the IEnumerator.</returns>
    #endregion
    private System.Collections.IEnumerator Run(float duration, Action onComplete)
    {
        yield return new WaitForSeconds(duration);

        onComplete?.Invoke();


        Destroy(gameObject);
    }
}

}
