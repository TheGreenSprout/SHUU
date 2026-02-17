/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



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
        public static DevConsoleManager instance;


        public static bool devConsole_On => DevConsoleInput.devConsole_On;
        public static bool canToggle_devConsole => DevConsoleInput.canToggle_devConsole;



        [SerializeField] private List<string> optionalParameter_consoleInterpreters = new List<string>() { "-", "~" };


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


        [HideInInspector] public DevConsoleInput input;




        private void Awake()
        {
            instance = this;


            DevCommandRegistry.RegisterCommands();


            input = GetComponent<DevConsoleInput>();

            input.toggle += devConsoleUI.Toggle;
        }


        private void OnDestroy()
        {
            input.toggle -= devConsoleUI.Toggle;
        }



        public static bool ProcessConsoleInput(string input, (string[], Color?)? printOutput = null)
        {
            return instance.ProcessInput(input, printOutput);
        }
        public bool ProcessInput(string input, (string[], Color?)? printOutput = null)
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

                    bool hasParamsArray = parameters.Length > 0 && Attribute.IsDefined(parameters[parameters.Length-1], typeof(ParamArrayAttribute));
                    int fixedCount = hasParamsArray ? parameters.Length - 1 : parameters.Length;

                    // Runs for every parameter that isnt part of the params array
                    for (int i = 0; i < fixedCount; i++)
                    {
                        ParseParameter(i, parameters, args, ref parsedArgs);
                    }

                    // Only runs if theres a param array at the end.
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


        private void ParseParameter(int i, ParameterInfo[] parameters, string[] args, ref object[] parsedArgs)
        {
            ParameterInfo p = parameters[i];
            Type paramType = p.ParameterType;

            bool isOptionalParam = paramType.IsGenericType && paramType.GetGenericTypeDefinition() == typeof(OptionalParameter<>);
            bool isMutableParam = paramType == typeof(MutableParameter);

            // No argument provided
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

            // Primitive fallback
            parsedArgs[i] = Convert.ChangeType(raw, paramType);
        }


        private void ParseOptional(int i, ParameterInfo p, ref object[] parsedArgs, string raw)
        {
            Type innerType = p.ParameterType.GetGenericArguments()[0];

            // Explicit "no value"
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
            // Try bool
            if (bool.TryParse(raw, out bool b))
            {
                parsedArgs[i] = new MutableParameter(b);
                return;
            }

            // Try int
            if (int.TryParse(raw, out int intVal))
            {
                parsedArgs[i] = new MutableParameter(intVal);
                return;
            }

            // Try float
            if (float.TryParse(raw, out float floatVal))
            {
                parsedArgs[i] = new MutableParameter(floatVal);
                return;
            }

            // Try char (single character only)
            if (raw.Length == 1)
            {
                parsedArgs[i] = new MutableParameter(raw[0]);
                return;
            }

            // Fallback: string
            parsedArgs[i] = new MutableParameter(raw);
        }


        public void PrintDelegate(string message, Color? textColor = null)
        {
            if (!string.IsNullOrEmpty(message)) instance.devConsoleUI.Print(message, textColor);
        }
        public void PrintDelegate(string[] message, Color? textColor = null)
        {
            foreach (string line in message)
            {
                instance.devConsoleUI.Print(line, textColor);
            }
        }

        public static void PrintOnConsole(string message, Color? textColor = null) => instance.PrintDelegate(message, textColor);
        public static void PrintOnConsole(string[] message, Color? textColor = null) => instance.PrintDelegate(message, textColor);
    }

}
