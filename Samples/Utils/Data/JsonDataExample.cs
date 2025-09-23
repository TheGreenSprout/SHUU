using System.Collections.Generic;

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
