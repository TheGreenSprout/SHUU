using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace SHUU.Utils.Developer.Console
{
    #region Custom Parameter Classes
    public class OptionalParameter<T>
    {
        private bool hasValue = false;
        
        private T value = default;


        public OptionalParameter()
        {
            hasValue = false;

            value = default;
        }
        public OptionalParameter(T val)
        {
            hasValue = true;

            value = val;
        }

        public bool TryGetValue(out T outValue)
        {
            outValue = value;

            return hasValue;
        }
    }


    public class MutableParameter
    {
        private enum ValueType
        {
            Null,
            Bool,
            Int,
            Float,
            String,
            Char
        }
        private ValueType valueType = ValueType.Null;
        
        public bool? boolValue = null;
        private int? intValue = null;
        private float? floatValue = null;
        private string stringValue = null;
        private char? charValue = null;
        

        public MutableParameter(bool val)
        {
            valueType = ValueType.Bool;

            boolValue = val;
        }
        public MutableParameter(int val)
        {
            valueType = ValueType.Int;

            intValue = val;
        }
        public MutableParameter(float val)
        {
            valueType = ValueType.Float;

            floatValue = val;
        }
        public MutableParameter(string val)
        {
            valueType = ValueType.String;

            stringValue = val;
        }
        public MutableParameter(char val)
        {
            valueType = ValueType.Char;

            charValue = val;
        }


        public bool TryGetValue<T>(out T value)
        {
            value = default;

            switch (valueType)
            {
                case ValueType.Bool when typeof(T) == typeof(bool):
                    value = (T)(object)boolValue;
                    return true;

                case ValueType.Int when typeof(T) == typeof(int):
                    value = (T)(object)intValue;
                    return true;

                case ValueType.Float when typeof(T) == typeof(float):
                    value = (T)(object)floatValue;
                    return true;

                case ValueType.String when typeof(T) == typeof(string):
                    value = (T)(object)stringValue;
                    return true;

                case ValueType.Char when typeof(T) == typeof(char):
                    value = (T)(object)charValue;
                    return true;

                default:
                    return false;
            }
        }

        public Type GetStoredType()
        {
            return valueType switch
            {
                ValueType.Bool => typeof(bool),
                ValueType.Int => typeof(int),
                ValueType.Float => typeof(float),
                ValueType.String => typeof(string),
                ValueType.Char => typeof(char),
                _ => null
            };
        }
    }
    #endregion



    [RequireComponent(typeof(ToggleDevConsole))]
    public class DevConsoleManager : MonoBehaviour
    {
        public static DevConsoleManager instance;


        public static bool devConsole_On => instance.toggleDevConsole.devConsole_On;
        public static bool canToggle_devConsole => instance.toggleDevConsole.canToggle_devConsole;



        [SerializeField] private List<string> optionalParameter_consoleInterpreters = new List<string>() { "-", "~" };



        [SerializeField] private DevConsoleUI devConsoleUI;


        [HideInInspector] public ToggleDevConsole toggleDevConsole;




        private void Awake()
        {
            instance = this;


            DevCommandRegistry.RegisterCommands();


            toggleDevConsole = GetComponent<ToggleDevConsole>();

            toggleDevConsole.toggle += devConsoleUI.Toggle;
        }


        private void OnDestroy()
        {
            toggleDevConsole.toggle -= devConsoleUI.Toggle;
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
                            array.SetValue(Convert.ChangeType(args[i], elementType), i-fixedCount);

                        parsedArgs[parsedArgs.Length-1] = array;
                    }

                    var result = info.Method.Invoke(null, parsedArgs);
                    if (result is ValueTuple<string[], Color?> tuple && tuple.Item1 != null)
                    {
                        PrintDelegate(tuple.Item1, tuple.Item2);
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


        private void ParseParameter(int i, ParameterInfo[] parameters, string[] args, ref object[] parsedArgs)
        {
            ParameterInfo p = parameters[i];

            bool isOptionalParam = p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == typeof(OptionalParameter<>);
            bool isMutableParam = p.ParameterType == typeof(MutableParameter);

            if (i >= args.Length)
            {
                // No argument provided at all
                if (isOptionalParam)
                {
                    parsedArgs[i] = Activator.CreateInstance(p.ParameterType); // empty OptionalParameter<T>
                }
                else
                {
                    throw new Exception($"Missing required argument {p.Name}");
                }

                return;
            }


            string raw = args[i];

            if (isOptionalParam)
            {
                ParseOptional(i, p, ref parsedArgs, raw);

                return;
            }

            if (isMutableParam)
            {
                ParseMutable(i, p, ref parsedArgs, raw);

                return;
            }


            parsedArgs[i] = Convert.ChangeType(args[i], parameters[i].ParameterType);
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
            instance.devConsoleUI.Print(message, textColor);
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
