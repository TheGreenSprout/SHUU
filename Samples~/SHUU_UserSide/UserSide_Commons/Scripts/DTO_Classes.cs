using System;
using System.Collections.Generic;

namespace SHUU.UserSide.Commons
{

    // DTO: Data Transfer Object --> Make your own and add them to MasterDTO (it's the DTO that will get serialized and saved).

    [Serializable]
    #region XML doc
    /// <summary>
    /// DTO that holds an instance of all the other DTOs (this one gets serialized and saved).
    /// </summary>
    #endregion
    public class MasterDTO
    {
        public Dictionary<DTO_Info, string> dataDictionary;


        public MasterDTO()
        {
            // This is how your MasterDTO should look (with all your data DTOs).
            DTO_Info exampleData = new DTO_ExampleInfo();


            dataDictionary = new Dictionary<DTO_Info, string>
            {
                // This is how your MasterDTO should look (with all your data DTOs).
                { exampleData, exampleData.GetIdentifier() },
            };
        }
    }



    // This is a parent DTO class used to identify all DTOs, keep it.
    #region XML doc
    /// <summary>
    /// Interface to all DTOs.
    /// </summary>
    #endregion
    public interface DTO_Info
    {
        public string GetIdentifier();
    }



    // This is how you should make all your DTO classes:
    [Serializable]
    #region XML doc
    /// <summary>
    /// Example of how to make your own DTOs.
    /// </summary>
    #endregion
    public class DTO_ExampleInfo : DTO_Info
    {
        public string GetIdentifier() { return "Example"; }
        
        
        public Dictionary<int, bool> colorOfItems = new Dictionary<int, bool>();
    }

}