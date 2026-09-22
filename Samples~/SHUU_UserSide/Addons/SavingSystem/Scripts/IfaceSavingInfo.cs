using System;

using SHUU.UserSide.Addons.SavingSystem.ForUser;

namespace SHUU.UserSide.Addons.SavingSystem
{
    #region XML doc
    /// <summary>
    /// Interface implemented by all saving info scripts (inherit from SavingInfo instead of implementing this yourself).
    /// </summary>
    #endregion
    public interface ISavingInfo
    {
        #region Variables
        public string Identifier { get; }


        #region XML doc
        /// <summary>
        /// The DTO class this saving info saves and loads. The saved json of this saving info is turned into this class when loading.
        /// </summary>
        #endregion
        public Type DtoType { get; }
        #endregion




        #region Override points

        #region Local
        #region XML doc
        /// <summary>
        /// Saves all info related to this saving info (from the scene into memory).
        /// </summary>
        /// <param name="sceneName">Scene in which the saving info currently is.</param>
        #endregion
        public void SaveInfo(string sceneName);

        #region XML doc
        /// <summary>
        /// Loads all info related to this saving info (from memory into the scene).
        /// </summary>
        /// <param name="sceneName">Scene in which the saving info currently is.</param>
        #endregion
        public void LoadInfo(string sceneName);
        #endregion



        #region Global
        #region XML doc
        /// <summary>
        /// Exports all of this saving info's info to its DTO (the manager puts it in the MasterDTO, that then gets serialized to a save file).
        /// </summary>
        #endregion
        public DTO_Info ExportDTO();

        #region XML doc
        /// <summary>
        /// Imports all of this saving info's info from its DTO (loaded from the MasterDTO of a save file).
        /// </summary>
        /// <param name="dto">Its corresponding DTO with all the info that needs loading.</param>
        #endregion
        public void ImportDTO(DTO_Info dto);
        #endregion

        #endregion
    }
}
