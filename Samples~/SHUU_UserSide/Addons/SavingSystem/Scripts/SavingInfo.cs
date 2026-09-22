using System;
using UnityEngine;

using SHUU.Utils.SceneManagement;
using SHUU.UserSide.Addons.SavingSystem.ForUser;

namespace SHUU.UserSide.Addons.SavingSystem
{
    [RequireComponent(typeof(SavingManager))]
    #region XML doc
    /// <summary>
    /// Parent of all saving info persistence scripts. Put them on the same GameObject as the SavingManager.
    /// If SaveInfo throws, the save file isn't written.
    /// </summary>
    #endregion
    public abstract class SavingInfo<T> : SceneSensitiveScript, ISavingInfo where T : DTO_Info, new()
    {
        #region Variables
        private string _Identifier = null;
        [HideInInspector] public string Identifier
        {
            get
            {
                if (_Identifier == null) _Identifier = new T().Identifier;

                return _Identifier;
            }
        }


        public Type DtoType => typeof(T);
        #endregion




        #region Override Points

        #region Local
        public void SaveInfo(string sceneName)
        {
            if (!IsValidScene(sceneName)) return;

            SaveInfo();
        }

        #region XML doc
        /// <summary>
        /// Saves all info related to this saving info (from the scene into memory).
        /// </summary>
        #endregion
        protected abstract void SaveInfo();


        public virtual void LoadInfo(string sceneName)
        {
            if (!IsValidScene(sceneName)) return;

            LoadInfo();
        }

        #region XML doc
        /// <summary>
        /// Loads all info related to this saving info (from memory into the scene).
        /// </summary>
        #endregion
        protected abstract void LoadInfo();
        #endregion



        #region Global
        DTO_Info ISavingInfo.ExportDTO() => ExportDTO();

        #region XML doc
        /// <summary>
        /// Exports all of this saving info's info to its DTO (the manager puts it in the MasterDTO, that then gets serialized to a save file).
        /// </summary>
        #endregion
        protected abstract T ExportDTO();


        public void ImportDTO(DTO_Info dto)
        {
            if (dto is T tDto) ImportDTO(tDto);
            else SavingLog.Error($"Trying to import a DTO of type {(dto == null ? "null" : dto.GetType().Name)} to a saving info script that expects a DTO of type {typeof(T).Name}.");
        }

        #region XML doc
        /// <summary>
        /// Imports all of this saving info's info from its DTO (loaded from the MasterDTO of a save file).
        /// </summary>
        /// <param name="dto">Its corresponding DTO with all the info that needs loading.</param>
        #endregion
        protected abstract void ImportDTO(T dto);
        #endregion

        #endregion
    }
}
