using UnityEngine;
using SHUU.Utils.PersistantInfo.SavingLoading;

namespace SHUU.Utils.PersistantInfo
{

#region XML doc
/// <summary>
/// Contains references needed for the Singleton persistance logic. Needs to exist alongside the Singleton scripts.
/// </summary>
#endregion
public class Persistant_Globals : MonoBehaviour
{
    public static SavingPersistance savingInfo;
    
    
    public static SaveFilesManager saveFilesManager;
}

}
