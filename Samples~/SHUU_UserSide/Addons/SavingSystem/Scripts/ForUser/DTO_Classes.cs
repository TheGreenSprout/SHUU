using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SHUU.UserSide.Addons.SavingSystem.ForUser
{
    #region Master DTO
    // DTO: Data Transfer Object --> Make your own and the MasterDTO will handle them.

    [Serializable]
    #region XML doc
    /// <summary>
    /// Holds the saved data of all the DTOs (this one gets serialized and saved).
    /// Every DTO is kept as plain json under its identifier, and each saving info only turns its own piece back into a DTO when a save is loaded.
    /// That way a DTO that was renamed or removed only affects its own data, the rest of the save still loads.
    /// </summary>
    #endregion
    public class MasterDTO
    {
        #region XML doc
        /// <summary>
        /// Version of the save file layout. A save from a newer version than the code understands is refused instead of being overwritten.
        /// Only raise it when the layout of the file itself changes. Adding fields to your own DTOs doesn't need it, missing fields just take their defaults.
        /// </summary>
        #endregion
        public const int CurrentFormatVersion = 1;
        public int formatVersion = CurrentFormatVersion;


        #region XML doc
        /// <summary>
        /// The json of every DTO, by identifier.
        /// </summary>
        #endregion
        public Dictionary<string, JToken> dataDictionary;



        #region Main
        public MasterDTO() => dataDictionary = new();
        #endregion
    }
    #endregion




    #region DTOs

    //! This is a parent DTO class used to identify all DTOs, keep it.

    #region XML doc
    /// <summary>
    /// Base class of all DTOs.
    /// </summary>
    #endregion
    public abstract class DTO_Info
    {
        #region XML doc
        /// <summary>
        /// The key this DTO's data is saved under. It's the name of the class unless you override it.
        /// Renaming a class changes its identifier, and the saves that were made before can't find their data anymore.
        /// If you plan to rename a DTO (or want to be safe), override this with a fixed text: public override string Identifier => "PlayerInventory";
        /// </summary>
        #endregion
        [JsonIgnore] public virtual string Identifier => GetType().Name;
    }



    // When making your own DTOs, make sure they inherit from DTO_Info and that they are serializable.
    // Also, the name of the DTO's class will be their identifier (unless you override and make a custom one), so make sure to not have repeat names.
    #region Custom DTOs
    #endregion
    
    #endregion
}
