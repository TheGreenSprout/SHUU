namespace SHUU.UserSide.Addons.SavingSystem.ForUser
{
    #region XML doc
    /// <summary>
    /// Information ABOUT a save file (not the game data itself), so a save menu can show it without loading the whole save.
    /// THIS IS THE CLASS YOU CUSTOMIZE: add whatever you want to know about a save.
    /// </summary>
    #endregion
    public class SaveFileInfo : SaveFileInfoBase
    {
        // Every public field or property you add gets saved and loaded automatically (json). Old saves that don't have your new fields just get their default value.
        // Show it in your menu with:  SavingManager.Instance.Saves[i].info.yourField




        #region Your data
        //! THESE ARE EXAMPLES, DELETE THEM AND ADD YOUR OWN.
        //public string currentLevel;
        //public int playerLevel;
        //public string difficulty = "Normal";
        #endregion




        #region XML doc
        /// <summary>
        /// Called every time the game is saved, right before this info is written. Fill in your fields here.
        /// To give the save a thumbnail, assign a readable Texture2D to "thumbnail" (or use SHUU_Saving.SaveWithScreenshot to have one taken for you).
        /// </summary>
        #endregion
        public void OnSaving()
        {
            //currentLevel = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        }
    }
}
