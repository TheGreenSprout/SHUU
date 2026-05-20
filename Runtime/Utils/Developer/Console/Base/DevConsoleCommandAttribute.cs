using System;

namespace SHUU.Utils.Developer.Console
{
    public class DevConsoleCommandAttribute : Attribute
    {
        #region Variables
        public string name;

        public string description;


        public string tag;
        #endregion




        #region Main
        public DevConsoleCommandAttribute(string _name, string _description = "", string _tag = "Untagged")
        {
            name = _name;
            description = _description;

            tag = _tag;
        }
        #endregion
    }
}
