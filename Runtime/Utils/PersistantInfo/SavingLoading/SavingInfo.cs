using UnityEngine;
using SHUU.Utils.SceneManagement;
using SHUU.UserSide.Commons;

namespace SHUU.Utils.PersistantInfo.SavingLoading
{

[RequireComponent(typeof(SavingPersistance))]
#region XML doc
/// <summary>
/// Parent of all saving info persistance scripts.
/// </summary>
#endregion
public class SavingInfo : SceneSensitiveScript, IfaceSavingInfo
{
    [Tooltip("Identifier must be the same as it's corresponding DTO_Info class")]
    public string identifier = "Info Name";
    
    
    
    #region Save info
    #region XML doc
    /// <summary>
    /// Saves all info related to this saving info.
    /// </summary>
    /// <param name="sceneName">Scene in which the saving info currently is.</param>
    #endregion
    public virtual void SaveInfo(string sceneName)
    {
        if (!IsValidScene(sceneName))
        {
            return;
        }



        Debug.LogWarning("SavingInfo void [SaveInfo()] not set up for object: " + this.name);
    }
    #endregion


    #region Load info
    #region XML doc
    /// <summary>
    /// Loads all info related from this saving info.
    /// </summary>
    /// <param name="sceneName">Scene in which the saving info currently is.</param>
    #endregion
    public virtual void LoadInfo(string sceneName)
    {
        if (!IsValidScene(sceneName))
        {
            return;
        }



        Debug.LogWarning("SavingInfo void [LoadInfo()] not set up for object: " + this.name);
    }
    #endregion



    #region DTO import/export
    #region XML doc
    /// <summary>
    /// Exports all of this saving info's info to it's corresponding DTO inside the MasterDTO.
    /// </summary>
    /// <param name="masterDTO">A reference to the MasterDTO.</param>
    #endregion
    public virtual DTO_Info ExportDTO()
    {
        Debug.LogWarning("SavingInfo function (return DTO_InfoClass) [ExportDTO()] not set up for object: " + this.name);

        return null;
    }


    #region XML doc
    /// <summary>
    /// Imports all of this saving info's info from it's corresponding DTO inside the MasterDTO.
    /// </summary>
    /// <param name="dto">It's corresponding DTO with all the info that needs loading.</param>
    #endregion
    public virtual void ImportDTO(DTO_Info dto)
    {
        Debug.LogWarning("SavingInfo void [ImportDTO()] not set up for object: " + this.name);
    }
    #endregion
}

}
