using System;

namespace SHUU.Utils.Developer.Console
{
    public class DevConsoleCommandAttribute : Attribute
    {
        public string _name;

        public string _description;




        public DevConsoleCommandAttribute(string name, string description = "")
        {
            _name = name;
            
            _description = description;
        }
    }
}
