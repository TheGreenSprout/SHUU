using System;
using System.Linq;
using UnityEngine;

namespace SHUU.Utils.Developer.Console
{

    public class DevConsoleManager : MonoBehaviour
    {
        [SerializeField] private DevConsoleUI devConsoleUI;


        [SerializeField] private KeyCode toggleKey = KeyCode.BackQuote;




        private void Awake()
        {
            DevCommandRegistry.RegisterCommands();
        }


        private void Update()
        {
            if (Input.GetKeyDown(toggleKey)) devConsoleUI.Toggle();
        }



        public bool ProcessInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;

            PrintDelegate($"> {input}");

            string[] parts = input.Split(' ');
            string cmd = parts[0];
            string[] args = parts.Skip(1).ToArray();

            if (DevCommandRegistry.TryGet(cmd, out var info))
            {
                try
                {
                    var parameters = info.Method.GetParameters();
                    object[] parsedArgs = new object[parameters.Length];

                    for (int i = 0; i < parameters.Length; i++)
                        parsedArgs[i] = Convert.ChangeType(args[i], parameters[i].ParameterType);

                    var result = info.Method.Invoke(null, parsedArgs);
                    if (result is ValueTuple<string[], Color?> tuple && tuple.Item1 != null)
                    {
                        foreach (var line in tuple.Item1) PrintDelegate(line, tuple.Item2);
                    }
                }
                catch (Exception ex)
                {
                    PrintDelegate($"Error: {ex.Message}", Color.red);

                    return false;
                }
            }
            else
            {
                PrintDelegate($"Unknown command: {cmd}", Color.red);

                return false;
            }


            return true;
        }



        public void PrintDelegate(string message, Color? textColor = null)
        {
            devConsoleUI.Print(message, textColor);
        }
    }

}
