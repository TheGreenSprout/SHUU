using SHUU.UserSide;


namespace SHUU.Utils.PersistantInfo
{

#region XML doc
/// <summary>
/// Interface implemented by all Singleton persistance scripts.
/// </summary>
#endregion
public interface IfaceSingletonInfo
{
    #region XML doc
    /// <summary>
    /// Saves all info related to this singleton.
    /// </summary>
    /// <param name="sceneName">Scene in which the singleton currently is.</param>
    #endregion
    public void SaveInfo(string sceneName);
    #region XML doc
    /// <summary>
    /// Loads all info related from this singleton.
    /// </summary>
    /// <param name="sceneName">Scene in which the singleton currently is.</param>
    #endregion
    public void LoadInfo(string sceneName);

    #region XML doc
    /// <summary>
    /// Exports all of this singleton's info to it's corresponding DTO inside the MasterDTO.
    /// </summary>
    /// <param name="masterDTO">A reference to the MasterDTO.</param>
    #endregion
    public DTO_Info ExportDTO();
    #region XML doc
    /// <summary>
    /// Imports all of this singleton's info from it's corresponding DTO inside the MasterDTO.
    /// </summary>
    /// <param name="dto">It's corresponding DTO with all the info that needs loading.</param>
    #endregion
    public void ImportDTO(DTO_Info dto);
}

}
