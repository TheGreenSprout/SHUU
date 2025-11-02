using UnityEngine;
using SHUU.Utils.SceneManagement;
using SHUU.UserSide;

namespace SHUU.Utils.PersistantInfo
{

[RequireComponent(typeof(SingletonPersistance))]
#region XML doc
/// <summary>
/// Parent of all Singleton persistance scripts.
/// </summary>
#endregion
public class SingletonInfo : SceneSensitiveScript, IfaceSingletonInfo
{
    public string identifier = "SingletonName";
    
    
    
    #region Save info
    #region XML doc
    /// <summary>
    /// Saves all info related to this singleton.
    /// </summary>
    /// <param name="sceneName">Scene in which the singleton currently is.</param>
    #endregion
    public virtual void SaveInfo(string sceneName)
    {
        if (!IsValidScene(sceneName))
        {
            return;
        }



        Debug.LogWarning("SingletonInfo void [SaveInfo()] not set up for object: " + this.name);
    }
    #endregion


    #region Load info
    #region XML doc
    /// <summary>
    /// Loads all info related from this singleton.
    /// </summary>
    /// <param name="sceneName">Scene in which the singleton currently is.</param>
    #endregion
    public virtual void LoadInfo(string sceneName)
    {
        if (!IsValidScene(sceneName))
        {
            return;
        }



        Debug.LogWarning("SingletonInfo void [LoadInfo()] not set up for object: " + this.name);
    }
    #endregion



    #region DTO import/export
    #region XML doc
    /// <summary>
    /// Exports all of this singleton's info to it's corresponding DTO inside the MasterDTO.
    /// </summary>
    /// <param name="masterDTO">A reference to the MasterDTO.</param>
    #endregion
    public virtual void ExportDTO(ref MasterDTO masterDTO)
    {
        Debug.LogWarning("SingletonInfo function (return DTO_InfoClass) [ExportDTO()] not set up for object: " + this.name);
    }


    #region XML doc
    /// <summary>
    /// Imports all of this singleton's info from it's corresponding DTO inside the MasterDTO.
    /// </summary>
    /// <param name="dto">It's corresponding DTO with all the info that needs loading.</param>
    #endregion
    public virtual void ImportDTO(DTO_Info dto)
    {
        Debug.LogWarning("SingletonInfo void [ImportDTO()] not set up for object: " + this.name);
    }
    #endregion
}

}
