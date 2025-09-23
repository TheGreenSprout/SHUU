using System;
using System.Collections.Generic;

// Take this script out of the package and use it as your own.
// DTO: Data Transfer Object --> Make your own and add them to MasterDTO (it's the DTO that will get serialized and saved).

[Serializable]
#region XML doc
/// <summary>
/// DTO that holds an instance of all the other DTOs (this one gets serialized and saved).
/// </summary>
#endregion
public class MasterDTO
{
    // This is how your MasterDTO should look (with all your data DTOs).
    public DTO_ExampleInfo exampleData;
}



// This is a parent DTO class used to identify all DTOs, keep it.
[Serializable]
#region XML doc
/// <summary>
/// Parent to all DTOs.
/// </summary>
#endregion
public class DTO_InfoClass { }



// This is how you should make all your DTO classes:
[Serializable]
#region XML doc
/// <summary>
/// Example of how to make your own DTOs.
/// </summary>
#endregion
public class DTO_ExampleInfo : DTO_InfoClass
{
    public Dictionary<int, bool> colorOfItems = new Dictionary<int, bool>();
}
