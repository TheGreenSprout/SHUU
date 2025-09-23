using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using SHUU.Utils.BaseScripts;

namespace SHUU.Utils.Data
{

#region XML doc
/// <summary>
/// Manages all sorts of things relating to data manipulation, such as saving or managing file addresses.
/// </summary>
#endregion
public class DataManager : MonoBehaviour
{
    public static string SAVE_fileInfoSeparator = "#SAVED_INFO#";




    #region Basic functions
    
    #region XML doc
    /// <summary>
    /// Gets a file address from the user.
    /// </summary>
    /// <param name="newFile">Whether the file address being fetched is from a pre-existing file or from a new file that must be created.</param>
    /// <returns>Returns the file address.</returns>
    #endregion
    public static string GetFileAddress(bool newFile){
        BrowserProperties browseProperties = new BrowserProperties();

        // Add a file extension filter to the search.
        browseProperties.filter = "Text files (*.txt) | *.txt";
        browseProperties.filterIndex = 0;


        string address = new FileExplorer().GetFileAddress(browseProperties, newFile);
        if (address != null)
        {
            return address;
        }
        


        Debug.Log("Error getting file address");

        return address;
    }

    #region XML doc
    /// <summary>
    /// Checks for the existance of a file.
    /// </summary>
    /// <param name="address">The address of the file you want to check for.</param>
    /// <returns>Returns true if the file exists, false if it doesn't.</returns>
    #endregion
    public static bool DoesFileExist(string address){
        if (File.Exists(address))
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    #region XML doc
    /// <summary>
    /// Splits a string into all the info it contains, as a list.
    /// </summary>
    /// <param name="data">The string you want to split.</param>
    /// <returns>Returns a string list with all the individual info.</returns>
    #endregion
    public static string[] GetDataListFromString(string data){
        return data.Split(new[] { SAVE_fileInfoSeparator }, StringSplitOptions.None);
    }

    #region XML doc
    /// <summary>
    /// Converts a list of information into one single string.
    /// </summary>
    /// <param name="dataList">The list you want to merge.</param>
    /// <returns>Returns a string with all the info from the list.</returns>
    #endregion
    public static string GetStringFromDataList(List<string> dataList){
        return string.Join(SAVE_fileInfoSeparator, dataList);
    }

    #endregion



    // NOT RECOMMENDED FOR IMPORTANT DATA, TEXT FILES CAN BE EASILY MODIFIED
    // IF SECURITY IS DESIRED, USE ENCRYPTION LOGIC FROM THE SHUU ENCRYPTION FUNCTIONS
    #region Saving/Loading info to/from txt files

    #region XML doc
    /// <summary>
    /// Saves a text file in the user's PC.
    /// </summary>
    /// <param name="infoList">The list of info you want to save.</param>
    /// <param name="customLocationFileName">The location you want the file to be saved in. If this param is null, the user will be prompted to pick an address themselves.</param>
    #endregion
    public static void SaveTxtFile(List<string> infoList, string customLocationFileName){
        string address;
        if (customLocationFileName == null)
        {
            address = GetFileAddress(true);
        }
        else
        {
            address = Application.persistentDataPath + customLocationFileName;
        }
        

        string saveStr = GetStringFromDataList(infoList);


        if (address == null)
        {
            return;
        }


        // Add extension if it doesn't already have it
        if (!address.EndsWith(".txt"))
        {
            address += ".txt";
        }
        File.WriteAllText(address, saveStr);


        Debug.Log("Saved TXT: " + saveStr);
    }

    #region XML doc
    /// <summary>
    /// Loads a text file from the user's PC.
    /// </summary>
    /// <param name="address">The location of the file you want to load. If this param is null, the user will be prompted to pick an address themselves.</param>
    /// <returns>A simple string array with all the info that was saved.</returns>
    #endregion
    public static string[] LoadTxtFile(string address){
        if (address == null)
        {
            address = GetFileAddress(false);
        }
        
        if (DoesFileExist(address))
        {
            string saveString = File.ReadAllText(address);

            Debug.Log("Loaded TXT: " + saveString);

            return GetDataListFromString(saveString);
        }
        else
        {
            Debug.Log("Failed load");

            return new string[]{"Error"};
        }
    }

    #endregion


    // RECOMMENDED FOR SAVING IN GENERAL, INFO IS DECENTLY SECURE.
    // IF FURTHER SECURITY IS DESIRED, USE ENCRYPTION LOGIC FROM THE SHUU ENCRYPTION FUNCTIONS
    #region Saving/Loading info to/from json files
    
    #region EXAMPLE
    /*
     For saving with JSON files, you need to have a custom class to store the info in that will then
     be used to save the info. Heres an example of a custom json data class ('Control + click' on the class name):
    */
        #region Example of a json data class
        /*
        AN EXAMPLE OF A JSON DATA CLASS. YOU NEED TO CREATE A CUSTOM CLASS TO STORE THE INFO YOU WANT TO
        SAVE AND LOAD WITH JSON FILES.
        */

        [System.Serializable]
        #region XML doc
        /// <summary>
        /// Example of a json data file.
        /// </summary>
        #endregion
        public class JsonDataExample
        {
            public string name;
            public int score;
            public List<string> inventory;
        }
        #endregion
    private JsonDataExample jsonDataExample = new JsonDataExample {
        name = "Sprout",
        score = 42,
        inventory = new List<string> { "pepsi", "bracelet" }
    };
    #endregion

    public static void SaveJsonFile<T>(T dataInstance, string customLocationFileName, bool prettyPrint){
        string address;
        if (customLocationFileName == null)
        {
            address = GetFileAddress(true);
        }
        else
        {
            address = Application.persistentDataPath + customLocationFileName;
        }
        

        if (address == null)
        {
            return;
        }


        // Add extension if it doesn't already have it
        if (!address.EndsWith(".json"))
        {
            address += ".json";
        }


        string jsonString = JsonUtility.ToJson(dataInstance, prettyPrint);
        File.WriteAllText(address, jsonString);
        Debug.Log("Saved JSON: " + jsonString);
    }

    public static T LoadJsonFile<T>(string address){
        if (address == null)
        {
            address = GetFileAddress(false);
        }
        
        if (DoesFileExist(address))
        {
            string jsonString = File.ReadAllText(address);
            T data = JsonUtility.FromJson<T>(jsonString);

            Debug.Log("Loaded JSON: " + jsonString);

            return data;
        }
        else
        {
            Debug.Log("Failed load");

            return default;
        }
    }

    #endregion


    /* TODO -- PREFABS
    // NOT RECOMMENDED FOR IMPORTANT DATA, PREFABS CAN BE EASILY MODIFIED
    // IF SECURITY IS DESIRED, USE ENCRYPTION LOGIC FROM THE SHUU ENCRYPTION FUNCTIONS
    #region Saving/Loading info to/from player prefabs

    

    #endregion
    */
}

}
