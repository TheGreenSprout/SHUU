using System;
using System.Collections.Generic;

namespace SHUU.UserSide.Commons
{
    #region Master DTO
    // DTO: Data Transfer Object --> Make your own and the MasterDTO will handle them.

    [Serializable]
    #region XML doc
    /// <summary>
    /// DTO that holds an instance of all the other DTOs (this one gets serialized and saved).
    /// </summary>
    #endregion
    public class MasterDTO
    {
        public Dictionary<string, DTO_Info> dataDictionary;


        public MasterDTO() => dataDictionary = new();
    }
    #endregion


    

    #region DTOs
    // This is a parent DTO class used to identify all DTOs, keep it.
    #region XML doc
    /// <summary>
    /// Interface to all DTOs.
    /// </summary>
    #endregion
    public abstract class DTO_Info
    {
        public virtual string Identifier => GetType().Name;
    }



    // When making your own DTOs, make sure they implement the DTO_Info interface and that they are serializable.
    // Also, the name of the DTO's class will be their identifier (unless you override and make a custom one), so make sure to not have repeat names.



    #region Example DTO
    //! THIS IS AN EXAMPLE DTO CLASS FOR THE SAMPLE SCENE, IF YOU ARE NOT USING IT, DELETE IT.
    // This is how you should make all your DTO classes:
    [Serializable]
    #region XML doc
    /// <summary>
    /// Example of how to make your own DTOs.
    /// </summary>
    #endregion
    public class DTO_ExampleInfo : DTO_Info
    {
        public Dictionary<int, bool> colorOfItems = new Dictionary<int, bool>();
    }
    #endregion
    #endregion
}