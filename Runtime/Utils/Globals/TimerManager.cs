using UnityEngine;
using System;
using System.Collections;

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
    public void Create(float duration, Action onComplete) => StartCoroutine(Run(duration, onComplete));


    #region XML doc
    /// <summary>
    /// Creates a Courtine, runs an Action and destroys itself when done.
    /// </summary>
    /// <param name="duration">The time the Action will be delayed by.</param>
    /// <param name="onComplete">The Action that will be performed.</param>
    /// <returns>Returns the IEnumerator.</returns>
    #endregion
    private IEnumerator Run(float duration, Action onComplete)
    {
        yield return new WaitForSeconds(duration);

        onComplete?.Invoke();
    }
}

}
