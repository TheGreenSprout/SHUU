using System.Collections.Generic;
using System.IO;
using SHUU.Utils.InputSystem;
using UnityEngine;

namespace SHUU.Utils.Developer.Console
{
    #region Serializable Items
    [System.Serializable]
    public struct ClassicBind
    {
        public KeyCode key;
        public int mouse;
        public string axis;

        public static ClassicBind From((KeyCode?, int?, string) input)
        {
            return new ClassicBind
            {
                key = input.Item1 ?? KeyCode.None,
                mouse = input.Item2 ?? -1,
                axis = input.Item3 ?? null
            };
        }

        public (KeyCode?, int?, string) ToTuple()
        {
            if (key != KeyCode.None) return ((KeyCode?)key, null, null);
            else if (axis != null) return (null, null, axis);
            else return (null, (int?)mouse, null);
        }
    }

    [System.Serializable]
    public struct InputSystemBind
    {
        public string mapName;
        public string setName;

        public static InputSystemBind From((InputBindingMap, string) input)
        {
            return new InputSystemBind
            {
                mapName = input.Item1.mapName,
                setName = input.Item2
            };
        }
    }


    [System.Serializable]
    public class ClassicCommandEntry
    {
        public ClassicBind input;
        public string[] command;
    }

    [System.Serializable]
    public class InputSystemCommandEntry
    {
        public InputSystemBind binding;
        public string[] command;
    }


    [System.Serializable]
    public class BoundCommandsSaveData
    {
        public List<ClassicCommandEntry> classicBinds = new();
        public List<InputSystemCommandEntry> inputSystemBinds = new();
    }
    #endregion



    [RequireComponent(typeof(DevConsoleManager))]
    public class BoundCommands : MonoBehaviour
    {
        private static string filePath => Path.Combine(Application.persistentDataPath, "bound_commands" + ".json");



        private static Dictionary<(KeyCode?, int?, string), string[]> boundCommands = new Dictionary<(KeyCode?, int?, string), string[]>();
        private static Dictionary<(InputBindingMap, string), string[]> is_boundCommands = new Dictionary<(InputBindingMap, string), string[]>();


        private DevConsoleManager devConsoleManager;




        private void Awake()
        {
            devConsoleManager = GetComponent<DevConsoleManager>();


            Load();
        }


        private void OnApplicationQuit() => Save();



        public static void BindCommand((KeyCode?, int?, string) input, string[] commandData)
        {
            boundCommands.Add(input, commandData);
        }
        public static void UnBindCommands((KeyCode?, int?, string) input)
        {
            boundCommands.Remove(input);
        }

        public static void BindCommand((InputBindingMap, string) binding, string[] commandData)
        {
            is_boundCommands.Add(binding, commandData);
        }
        public static void UnBindCommands((InputBindingMap, string) binding)
        {
            is_boundCommands.Remove(binding);
        }



        private bool GetInputDown((KeyCode?, int?, string) input)
        {
            if (input.Item1 != null)
            {
                return Input.GetKeyDown(input.Item1.Value);
            }
            else if (input.Item2 != null)
            {
                return Input.GetMouseButtonDown(input.Item2.Value);
            }
            else if (input.Item3 != null)
            {
                return Input.GetAxisRaw(input.Item3) > 0;
            }

            return false;
        }


        private void Update()
        {
            if (devConsoleManager.devConsoleUI.gameObject.activeInHierarchy && devConsoleManager.inputFieldActive) return;


            
            foreach (var kvp in boundCommands)
            {
                if (GetInputDown(kvp.Key))
                {
                    devConsoleManager.ProcessInput(string.Join(" ", kvp.Value));
                }
            }

            foreach (var kvp in is_boundCommands)
            {
                if (SHUU_Input.GetInputDown(kvp.Key.Item1, kvp.Key.Item2))
                {
                    devConsoleManager.ProcessInput(string.Join(" ", kvp.Value));
                }
            }
        }



        private static void Save()
        {
            BoundCommandsSaveData data = new();

            foreach (var kvp in boundCommands)
            {
                data.classicBinds.Add(new ClassicCommandEntry
                {
                    input = ClassicBind.From(kvp.Key),
                    command = kvp.Value
                });
            }

            foreach (var kvp in is_boundCommands)
            {
                data.inputSystemBinds.Add(new InputSystemCommandEntry
                {
                    binding = InputSystemBind.From(kvp.Key),
                    command = kvp.Value
                });
            }

            string json = JsonUtility.ToJson(data, true);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                File.WriteAllText(filePath, json);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to save settings: {ex}");
            }
        }


        private static void Load()
        {
            if (!File.Exists(filePath)) return;

            try
            {
                string json = File.ReadAllText(filePath);
                BoundCommandsSaveData data =
                    JsonUtility.FromJson<BoundCommandsSaveData>(json);

                if (data == null) return;

                boundCommands.Clear();
                is_boundCommands.Clear();

                foreach (var entry in data.classicBinds)
                {
                    boundCommands[entry.input.ToTuple()] = entry.command;
                }

                foreach (var entry in data.inputSystemBinds)
                {
                    InputBindingMap map =
                        SHUU_Input.RetrieveBindingMap(entry.binding.mapName);

                    if (map == null) continue;

                    is_boundCommands[(map, entry.binding.setName)] = entry.command;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load settings: {ex}");
            }
        }
    }
}
