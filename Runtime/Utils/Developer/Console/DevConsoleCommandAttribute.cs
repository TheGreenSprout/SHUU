using System;

namespace SHUU.Utils.Developer.Console
{
    public class DevConsoleCommandAttribute : Attribute
    {
        public string name;

        public string description;


        public string tag;




        public DevConsoleCommandAttribute(string _name, string _description = "", string _tag = "Untagged")
        {
            name = _name;
            description = _description;

            tag = _tag;
        }
    }
}
