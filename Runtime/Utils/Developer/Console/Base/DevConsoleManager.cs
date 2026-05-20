using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SHUU.Utils.Developer.Console
{
    [RequireComponent(typeof(DevConsoleInput))]
    public class DevConsoleManager : MonoBehaviour
    {
        #region Variables
        public static DevConsoleManager instance;


        public static bool devConsole_On => DevConsoleInput.devConsole_On;
        public static bool canToggle_devConsole => DevConsoleInput.canToggle_devConsole;



        public List<string> optionalParameter_consoleInterpreters = new List<string>() { "-", "~" };


        [Tooltip("The higher on the list (closer to index 0) the tag is, the higher the priority")]
        public List<string> tagList = new List<string>() {
            "Information",
            "Utilities",
            "Debug",
            "Classic Input",
            "Input System",
            "Untagged"
        };



        public DevConsoleUI devConsoleUI;

        [HideInInspector] public bool inputFieldActive => devConsoleUI.inputFieldActive;


        [HideInInspector] public DevConsoleInput inputModule;
        #endregion




        #region Main
        private void Awake()
        {
            instance = this;


            DevCommandRegistry.RegisterCommands();


            inputModule = GetComponent<DevConsoleInput>();

            inputModule.toggle += devConsoleUI.Toggle;
        }


        private void OnDestroy() => inputModule.toggle -= devConsoleUI.Toggle;
        #endregion



        #region Logic

        #region Input processing
        public static bool ProcessConsoleInput(string input) => instance.ProcessInput(input);
        public bool ProcessInput(string input)
        {
            if (input == null) return false;

            PrintDelegate($"> {input}");

            if (string.IsNullOrWhiteSpace(input)) return false;


            if (!input.ToLower().StartsWith("setvar"))
            {
                while (input.Contains("$"))
                {
                    input = SavedConsoleVariables.ParseVariable(input, out CommandReturn varError);

                    if (varError != null) PrintDelegate(varError.output, varError.color);
                }
            }

            string[] parts = input.Split(' ');
            string cmd = parts[0];
            string[] args = parts.Skip(1).ToArray();

            if (DevCommandRegistry.TryGet(cmd, out var info))
            {
                try
                {
                    var parameters = info.Method.GetParameters();
                    object[] parsedArgs = new object[parameters.Length];

                    bool hasParamsArray = parameters.Length > 0 && Attribute.IsDefined(parameters[parameters.Length-1], typeof(ParamArrayAttribute));
                    int fixedCount = hasParamsArray ? parameters.Length - 1 : parameters.Length;


                    for (int i = 0; i < fixedCount; i++)
                    {
                        ParseParameter(i, parameters, args, ref parsedArgs);
                    }


                    if (fixedCount != parameters.Length)
                    {
                        Type elementType = parameters[parameters.Length-1].ParameterType.GetElementType();
                        Array array = Array.CreateInstance(elementType, args.Length-fixedCount);

                        for (int i = fixedCount; i < args.Length; i++)
                        {
                            array.SetValue(Convert.ChangeType(args[i], elementType), i-fixedCount);
                        }

                        parsedArgs[parsedArgs.Length-1] = array;
                    }


                    var result = info.Method.Invoke(null, parsedArgs);

                    if (result is CommandReturn ret && ret.output != null) PrintDelegate(ret.output, ret.color);
                    else if (result is ValueTuple<string[], Color?> tuple && tuple.Item1 != null) PrintDelegate(tuple.Item1, tuple.Item2);
                    else PrintDelegate("Command executed successfully.", Color.green);
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
        #endregion



        #region Parsing
        private void ParseParameter(int i, ParameterInfo[] parameters, string[] args, ref object[] parsedArgs)
        {
            ParameterInfo p = parameters[i];
            Type paramType = p.ParameterType;

            bool isOptionalParam = paramType.IsGenericType && paramType.GetGenericTypeDefinition() == typeof(OptionalParameter<>);
            bool isMutableParam = paramType == typeof(MutableParameter);

            // No argument
            if (i >= args.Length)
            {
                if (isOptionalParam)
                {
                    parsedArgs[i] = Activator.CreateInstance(paramType);
                    return;
                }

                throw new Exception($"Missing required argument {p.Name}");
            }

            string raw = args[i];

            // Optional<T>
            if (isOptionalParam)
            {
                ParseOptional(i, p, ref parsedArgs, raw);
                return;
            }

            // MutableParameter
            if (isMutableParam)
            {
                ParseMutable(i, p, ref parsedArgs, raw);
                return;
            }

            // Primitive
            parsedArgs[i] = Convert.ChangeType(raw, paramType);
        }


        private void ParseOptional(int i, ParameterInfo p, ref object[] parsedArgs, string raw)
        {
            Type innerType = p.ParameterType.GetGenericArguments()[0];

            if (instance.optionalParameter_consoleInterpreters.Contains(raw))
            {
                parsedArgs[i] = Activator.CreateInstance(p.ParameterType);
                return;
            }

            // Optional<MutableParameter>
            if (innerType == typeof(MutableParameter))
            {
                ParseMutable(i, p, ref parsedArgs, raw);
                var mutable = parsedArgs[i];
                parsedArgs[i] = Activator.CreateInstance(p.ParameterType, mutable);
                return;
            }

            // Optional<T>
            object converted = Convert.ChangeType(raw, innerType);
            parsedArgs[i] = Activator.CreateInstance(p.ParameterType, converted);
        }

        private void ParseMutable(int i, ParameterInfo p, ref object[] parsedArgs, string raw)
        {
            if (bool.TryParse(raw, out bool b))
            {
                parsedArgs[i] = new MutableParameter(b);
                return;
            }

            if (int.TryParse(raw, out int intVal))
            {
                parsedArgs[i] = new MutableParameter(intVal);
                return;
            }

            if (float.TryParse(raw, out float floatVal))
            {
                parsedArgs[i] = new MutableParameter(floatVal);
                return;
            }

            if (raw.Length == 1)
            {
                parsedArgs[i] = new MutableParameter(raw[0]);
                return;
            }

            parsedArgs[i] = new MutableParameter(raw);
        }
        #endregion



        #region Print
        public void PrintDelegate(string message, Color? textColor = null)
        {
            if (!string.IsNullOrEmpty(message)) instance.devConsoleUI.Print(message, textColor);
        }
        public void PrintDelegate(string[] message, Color? textColor = null) => instance.devConsoleUI.Print(textColor, message);
        public void PrintDelegate(params (string, Color?)[] message) => instance.devConsoleUI.Print(message);


        public static void PrintOnConsole(string message, Color? textColor = null) => instance.PrintDelegate(message, textColor);
        public static void PrintOnConsole(string[] message, Color? textColor = null) => instance.PrintDelegate(message, textColor);
        public static void PrintOnConsole(params (string, Color?)[] message) => instance.PrintDelegate(message);
        #endregion
    
        #endregion
    }
}
