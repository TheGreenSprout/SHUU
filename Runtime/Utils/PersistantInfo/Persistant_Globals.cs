using UnityEngine;
using SHUU.Utils.PersistantInfo.SavingLoading;

namespace SHUU.Utils.PersistantInfo
{

[RequireComponent(typeof(SingletonPersistance))]
#region XML doc
/// <summary>
/// Contains references needed for the Singleton persistance logic. Needs to exist alongside the Singleton scripts.
/// </summary>
#endregion
public class Persistant_Globals : MonoBehaviour
{
    public static SingletonPersistance sigletonInfo;
    
    
    public static SaveFilesManager saveFilesManager;
}

}
