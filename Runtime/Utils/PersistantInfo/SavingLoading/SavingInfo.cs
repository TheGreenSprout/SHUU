using UnityEngine;

using SHUU.Utils.SceneManagement;
using SHUU.UserSide.Commons;
using SHUU.UserSide.Commons.InnerWorkings.ScriptableObjects;

namespace SHUU.Utils.PersistantInfo.SavingLoading
{
    [RequireComponent(typeof(SavingManager))]
    #region XML doc
    /// <summary>
    /// Parent of all saving info persistance scripts.
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



        private static bool debugLogEmission => SHUU_Preferences.instance.saving_debugLogEmission;
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
        /// Saves all info related to this saving info.
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
        /// Loads all info related from this singleton.
        /// </summary>
        #endregion
        protected abstract void LoadInfo();
        #endregion



        #region Global
        DTO_Info ISavingInfo.ExportDTO() => ExportDTO();

        #region XML doc
        /// <summary>
        /// Exports all of this singleton's info to it's corresponding DTO inside the MasterDTO.
        /// </summary>
        /// <param name="masterDTO">A reference to the MasterDTO.</param>
        #endregion
        protected abstract T ExportDTO();


        public void ImportDTO(DTO_Info dto)
        {
            if (dto is T tDto) ImportDTO(tDto);
            else if (debugLogEmission) Debug.LogError($"Trying to import a DTO of type {dto.GetType().Name} to a saving info script that expects a DTO of type {typeof(T).Name}.");
        }

        #region XML doc
        /// <summary>
        /// Imports all of this singleton's info from it's corresponding DTO inside the MasterDTO.
        /// </summary>
        /// <param name="dto">It's corresponding DTO with all the info that needs loading.</param>
        #endregion
        protected abstract void ImportDTO(T dto);
        #endregion

        #endregion
    }
}
