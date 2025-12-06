using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

//! Read and write your own way.

namespace SHUU.Utils.Developer.Console
{
    public static class DevCommandRegistry
    {
        public struct DevCommandInfo
        {
            public MethodInfo Method;
            public string Description;
        }

        private static Dictionary<string, DevCommandInfo> commands = new();




        public static void RegisterCommands()
        {
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var attr = method.GetCustomAttribute<DevConsoleCommandAttribute>();

                    if (attr != null)
                    {
                        commands[attr._name.ToLower()] = new DevCommandInfo
                        {
                            Method = method,
                            Description = attr._description
                        };
                    }
                }
            }
        }


        public static bool TryGet(string name, out DevCommandInfo info) => commands.TryGetValue(name.ToLower(), out info);

        public static IEnumerable<(string, DevCommandInfo)> AllCommands() => commands.Select(pair => (pair.Key, pair.Value));
    }
}
