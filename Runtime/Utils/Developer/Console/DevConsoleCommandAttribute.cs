using System;

namespace SHUU.Utils.Developer.Console
{
    public class DevConsoleCommandAttribute : Attribute
    {
        public string name;

        public string description;




        public DevConsoleCommandAttribute(string _name, string _description = "")
        {
            name = _name;
            
            description = _description;
        }
    }
}
