using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;

using Alchemy.Inspector;

namespace SHUU.Utils.Developer.Console
{
    [RequireComponent(typeof(DevConsoleInput))]
    public class DevConsoleManager : MonoBehaviour
    {
        #region Variables
        public static DevConsoleManager Instance;


        public static bool DevConsole_On => DevConsoleInput.DevConsole_On;
        public static bool CanToggle_devConsole => DevConsoleInput.CanToggle_devConsole;



        public DevConsoleUI devConsoleUI;


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



        [HideInInspector] public bool inputFieldActive => devConsoleUI.inputFieldActive;

        [HideInInspector] public DevConsoleInput inputModule;



        public bool typewriterLines = true;

        [ShowIf(nameof(typewriterLines))] public float typewriterDelay = 0.05f;

        [ShowIf(nameof(typewriterLines))] public bool playTypewriterSFX = false;
        private bool showAudioClipField => typewriterLines && playTypewriterSFX;
        [ShowIf(nameof(showAudioClipField))] public AudioClip typewriterSFX = null;
        private AudioClip _typewriterSFX => playTypewriterSFX ? typewriterSFX : null;
        #endregion




        #region Main
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Instance = null;


        private void Awake()
        {
            Instance = this;


            DevCommandRegistry.RegisterCommands();


            inputModule = GetComponent<DevConsoleInput>();

            inputModule.toggle += devConsoleUI.Toggle;

            ConsoleScripts.EnsureAutoexec();
        }


        private void OnDestroy()
        {
            inputModule.toggle -= devConsoleUI.Toggle;

            ConsoleScripts.Cancel();
        }
        #endregion



        #region Logic

        #region Input processing
        internal bool firstInput = true;


        public static bool ProcessConsoleInput(string input) => Instance.ProcessInput(input);
        public bool ProcessInput(string input)
        {
            if (input == null) return false;

            if (!firstInput) PrintDelegate(" ");
            else firstInput = false;
            PrintDelegate($"> {input}");

            if (string.IsNullOrWhiteSpace(input)) return false;


            if (ParseCommand(input, out var cmd, out var args, out var info, out CommandReturn lineError))
            {
                try
                {
                    var parameters = info.Method.GetParameters();
                    object[] parsedArgs = new object[parameters.Length];

                    bool hasParamsArray = parameters.Length > 0 && Attribute.IsDefined(parameters[parameters.Length-1], typeof(ParamArrayAttribute));
                    int fixedCount = hasParamsArray ? parameters.Length - 1 : parameters.Length;


                    for (int i = 0; i < fixedCount; i++)
                        ParseParameter(i, parameters, args, ref parsedArgs);


                    if (fixedCount != parameters.Length)
                    {
                        Type elementType = parameters[parameters.Length-1].ParameterType.GetElementType();
                        Array array = Array.CreateInstance(elementType, args.Length-fixedCount);

                        for (int i = fixedCount; i < args.Length; i++)
                            array.SetValue(ConvertArgument(args[i], elementType, null), i-fixedCount);

                        parsedArgs[parsedArgs.Length-1] = array;
                    }


                    var result = info.Method.Invoke(null, parsedArgs);

                    if (result is CommandReturn ret && ret.output != null) PrintDelegate(ret.output, ret.color);
                    else if (result is ValueTuple<string[], Color?> tuple && tuple.Item1 != null) PrintDelegate(tuple.Item1, tuple.Item2);
                    else PrintDelegate("Command executed successfully.", Color.green);
                }
                catch (Exception ex)
                {
                    PrintDelegate($"Error: {Unwrap(ex).Message}", Color.red);

                    return false;
                }
            }
            else if (lineError != null)
            {
                PrintDelegate(lineError.output, lineError.color);

                return false;
            }
            else
            {
                PrintDelegate($"Unknown command: '{cmd}'", Color.red);

                return false;
            }


            return true;
        }


        public static bool ProcessConsoleInputDirect(string input, out CommandReturn output) => Instance.ProcessInputDirect(input, out output);
        public bool ProcessInputDirect(string input, out CommandReturn output)
        {
            output = null;


            if (input == null) return false;

            if (!firstInput) PrintDelegate(" ");
            else firstInput = false;
            PrintDelegate($"> {input}");

            if (string.IsNullOrWhiteSpace(input)) return false;


            if (ParseCommand(input, out var cmd, out var args, out var info, out CommandReturn lineError))
            {
                try
                {
                    var parameters = info.Method.GetParameters();
                    object[] parsedArgs = new object[parameters.Length];

                    bool hasParamsArray = parameters.Length > 0 && Attribute.IsDefined(parameters[parameters.Length-1], typeof(ParamArrayAttribute));
                    int fixedCount = hasParamsArray ? parameters.Length - 1 : parameters.Length;


                    for (int i = 0; i < fixedCount; i++)
                        ParseParameter(i, parameters, args, ref parsedArgs);


                    if (fixedCount != parameters.Length)
                    {
                        Type elementType = parameters[parameters.Length-1].ParameterType.GetElementType();
                        Array array = Array.CreateInstance(elementType, args.Length-fixedCount);

                        for (int i = fixedCount; i < args.Length; i++)
                            array.SetValue(ConvertArgument(args[i], elementType, null), i-fixedCount);

                        parsedArgs[parsedArgs.Length-1] = array;
                    }


                    var result = info.Method.Invoke(null, parsedArgs);

                    if (result is CommandReturn ret && ret.output != null)
                    {
                        output = new CommandReturn(ret.color, ret.output);
                        PrintDelegate(ret.output, ret.color, true);
                    }
                    else if (result is ValueTuple<string[], Color?> tuple && tuple.Item1 != null)
                    {
                        output = new CommandReturn(tuple.Item2, tuple.Item1);
                        PrintDelegate(tuple.Item1, tuple.Item2, true);
                    }
                    else
                    {
                        output = new CommandReturn(Color.green, "Command executed successfully.");
                        PrintDelegate("Command executed successfully.", Color.green);
                    }
                }
                catch (Exception ex)
                {
                    output = new CommandReturn(Color.red, $"Error: {Unwrap(ex).Message}");
                    PrintDelegate($"Error: {Unwrap(ex).Message}", Color.red);

                    return false;
                }
            }
            else if (lineError != null)
            {
                output = lineError;
                PrintDelegate(lineError.output, lineError.color);

                return false;
            }
            else
            {
                output = new CommandReturn(Color.red, $"Unknown command: '{cmd}'");
                PrintDelegate($"Unknown command: '{cmd}'", Color.red);

                return false;
            }


            return true;
        }
        #endregion



        #region Parsing
        private const int MaxVariableExpansions = 16;
        private const int MaxExpandedLength = 10000;

        private bool ParseCommand(string input, out string cmd, out string[] args, out DevCommandRegistry.DevCommandInfo info, out CommandReturn lineError)
        {
            args = null;
            cmd = null;

            info = default;
            lineError = null;


            if (input == null || string.IsNullOrWhiteSpace(input)) return false;

            if (!input.ToLower().StartsWith("setvar") && !ExpandVariables(ref input, out lineError)) return false;

            if (!ConsoleTokenizer.TryTokenize(input, out string[] parts, out string quoteError))
            {
                lineError = CommandReturn.Red(quoteError);

                return false;
            }

            // Nothing left (a variable that held nothing, say).
            if (parts.Length == 0)
            {
                cmd = "";

                return false;
            }

            cmd = parts[0];
            args = parts.Skip(1).ToArray();


            return DevCommandRegistry.TryGet(cmd, out info);
        }


        private static Exception Unwrap(Exception ex) => ex is TargetInvocationException && ex.InnerException != null ? ex.InnerException : ex;


        private static bool ExpandVariables(ref string input, out CommandReturn error)
        {
            error = null;

            for (int pass = 0; pass < MaxVariableExpansions; pass++)
            {
                if (!input.Contains("$")) return true;

                string expanded = SavedConsoleVariables.ParseVariable(input, out error, MaxExpandedLength);
                if (error != null) return false;

                if (expanded == input) return true;

                input = expanded;
            }

            error = CommandReturn.Red("Too many nested variables (does a variable refer to itself?).");

            return false;
        }


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

            // QueryParameter
            if (paramType == typeof(QueryParameter))
            {
                parsedArgs[i] = QueryParameter.Parse(raw);
                return;
            }

            // Primitive or enum
            parsedArgs[i] = ConvertArgument(raw, paramType, p.Name);
        }


        public static object ConvertArgument(string raw, Type type, string parameterName)
        {
            if (!type.IsEnum)
            {
                // Not through Convert.ChangeType: it would read "0,5" as 5 (the comma as a thousands separator).
                if (type == typeof(float)) return float.Parse(raw, NumberStyles.Float, CultureInfo.InvariantCulture);
                if (type == typeof(double)) return double.Parse(raw, NumberStyles.Float, CultureInfo.InvariantCulture);
                if (type == typeof(decimal)) return decimal.Parse(raw, NumberStyles.Float, CultureInfo.InvariantCulture);

                return Convert.ChangeType(raw, type, CultureInfo.InvariantCulture);
            }


            bool isFlags = Attribute.IsDefined(type, typeof(FlagsAttribute));

            if ((isFlags || !raw.Contains(',')) && Enum.TryParse(type, raw, true, out object value) && (isFlags || Enum.IsDefined(type, value))) return value;

            throw new Exception($"'{raw}' isn't a valid {parameterName ?? type.Name}. Use one of: {string.Join(", ", Enum.GetNames(type)).ToLowerInvariant()}.");
        }


        private void ParseOptional(int i, ParameterInfo p, ref object[] parsedArgs, string raw)
        {
            Type innerType = p.ParameterType.GetGenericArguments()[0];

            if (Instance.optionalParameter_consoleInterpreters.Contains(raw))
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

            // Optional<QueryParameter>
            if (innerType == typeof(QueryParameter))
            {
                parsedArgs[i] = Activator.CreateInstance(p.ParameterType, QueryParameter.Parse(raw));
                return;
            }

            // Optional<T>
            object converted = ConvertArgument(raw, innerType, p.Name);
            parsedArgs[i] = Activator.CreateInstance(p.ParameterType, converted);
        }

        private void ParseMutable(int i, ParameterInfo p, ref object[] parsedArgs, string raw)
        {
            if (bool.TryParse(raw, out bool b))
            {
                parsedArgs[i] = new MutableParameter(b);
                return;
            }

            if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intVal))
            {
                parsedArgs[i] = new MutableParameter(intVal);
                return;
            }

            if (float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatVal))
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
            if (message != null) Instance.devConsoleUI.Print(message, textColor);
        }

        public void PrintDelegate(string[] message, Color? textColor = null, bool ignoreTypewriter = false)
        {
            if (!typewriterLines || ignoreTypewriter) Instance.devConsoleUI.Print(textColor, message);
            else Instance.devConsoleUI.PrintGradually(typewriterDelay, 0, _typewriterSFX, textColor, message);
        }

        public void PrintDelegate(params (string, Color?)[] message)
        {
            if (!typewriterLines) Instance.devConsoleUI.Print(message);
            else Instance.devConsoleUI.PrintGradually(typewriterDelay, 0, _typewriterSFX, message);
        }
        public void PrintDelegate(bool ignoreTypewriter = false, params (string, Color?)[] message)
        {
            if (!typewriterLines || ignoreTypewriter) Instance.devConsoleUI.Print(message);
            else Instance.devConsoleUI.PrintGradually(typewriterDelay, 0, _typewriterSFX, message);
        }


        public static void PrintOnConsole(string message, Color? textColor = null) => Instance.PrintDelegate(message, textColor);
        public static void PrintOnConsole(string[] message, Color? textColor = null) => Instance.PrintDelegate(message, textColor);
        public static void PrintOnConsole(params (string, Color?)[] message) => Instance.PrintDelegate(message);
        #endregion
    
        #endregion
    }
}
