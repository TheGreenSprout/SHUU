using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SHUU.Utils.Developer.Console
{
    public static class DevCommandRegistry
    {
        #region Variables
        #region Helper class
        public struct DevCommandInfo
        {
            public MethodInfo Method;
            public string Description;

            public string Tag;

            public int Order;
        }
        #endregion
        
        private static int OrderCounter = 0;


        private static Dictionary<string, DevCommandInfo> Commands = new();
        #endregion




        #region Logic
        public static void RegisterCommands()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                foreach (var type in assembly.GetTypes())
                    foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        var attr = method.GetCustomAttribute<DevConsoleCommandAttribute>();
                        if (attr != null)
                        {
                            Commands[attr.name.ToLower()] = new DevCommandInfo
                            {
                                Method = method,
                                Description = attr.description,
                                Tag = attr.tag,
                                Order = OrderCounter++
                            };
                        }
                    }
        }


        public static bool TryGet(string name, out DevCommandInfo info) => Commands.TryGetValue(name.ToLower(), out info);

        public static IEnumerable<(string, DevCommandInfo)> AllCommands() => Commands.Select(pair => (pair.Key, pair.Value));
        #endregion
    }
}
