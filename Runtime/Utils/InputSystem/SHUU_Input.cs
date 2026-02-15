using System;
using System.Collections.Generic;
using System.Linq;
using SHUU.Utils.Globals;
using SHUU.Utils.Helpers;
using UnityEngine;

namespace SHUU.Utils.InputSystem
{
    #region Data Classes
    public class DynamicInput
    {
        public (KeyCode? key, int? mouse, string axis) bind = (null, null, null);

        public bool direction = false;



        public DynamicInput(KeyCode key, bool direction = true)
        {
            bind = (key, null, null);
            this.direction = direction;
        }

        public DynamicInput(int mouseButton, bool direction = true)
        {
            bind = (null, mouseButton, null);
            this.direction = direction;
        }

        public DynamicInput(InputParser.AxisNames axisName, bool direction = true)
        {
            bind = (null, null, InputParser.GetAxis_WithEnumName(axisName));
            this.direction = direction;
        }

        public DynamicInput(string input)
        {
            if (string.IsNullOrEmpty(input)) return;

            
            if (input.StartsWith("+"))
            {
                input = input.Substring(1);
                direction = true;
            }
            else if (input.StartsWith("-"))
            {
                input = input.Substring(1);
                direction = false;
            }
            else direction = true;

            bind = InputParser.ParseInput(input);
        }


        public bool IsValid()
        {
            int count = 0;

            if (bind.key != null) count++;
            if (bind.mouse != null) count++;
            if (bind.axis != null) count++;

            return count == 1;
        }

        public bool IsKey() => IsValid() && bind.key != null;

        public bool IsMouse() => IsValid() && bind.mouse != null;

        public bool IsAxis() => IsValid() && bind.axis != null;


        public bool TryGetKey(out KeyCode key)
        {
            if (!IsKey())
            {
                key = default;
                
                return false;
            }


            key = bind.key.Value;

            return true;
        }

        public bool TryGetMouse(out int mouse)
        {
            if (!IsMouse())
            {
                mouse = default;
                
                return false;
            }


            mouse = bind.mouse.Value;

            return true;
        }

        public bool TryGetAxis(out string axis)
        {
            if (!IsAxis())
            {
                axis = null;
                
                return false;
            }


            axis = bind.axis;

            return true;
        }
    }
    #endregion



    public static class SHUU_Input
    {
        #region Data Classes
        public static DynamicInput[] CreateDynamicInputArray(params string[] input)
        {
            DynamicInput[] inputs = new DynamicInput[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                inputs[i] = new DynamicInput(input[i]);
            }

            return inputs;
        }

        public static DynamicInput[] CreateDynamicInputArray(params KeyCode[] input)
        {
            DynamicInput[] inputs = new DynamicInput[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                inputs[i] = new DynamicInput(InputParser.InputToString(input[i]));
            }

            return inputs;
        }
        public static DynamicInput[] CreateDynamicInputArray(params int[] input)
        {
            DynamicInput[] inputs = new DynamicInput[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                inputs[i] = new DynamicInput(InputParser.InputToString(input[i]));
            }

            return inputs;
        }

        public static DynamicInput[] CreateDynamicInputArray(params DynamicInput[][] input) => HandyFunctions.MergeArrays(input);


        public struct InputValue
        {
            public float[] values;

            public int AmountOfValues => values != null ? values.Length : 0;



            public InputValue(params float[] values)
            {
                this.values = values;
            }


            public bool HasValue() => AmountOfValues > 0;


            public bool TryGetFloat(out float value)
            {
                if (!HasValue() || values.Length != 1)
                {
                    value = 0f;
                    return false;
                }


                value = values[0];

                return true;
            }

            public bool TryGetVector2(out Vector2 value)
            {
                if (!HasValue() || values.Length < 2)
                {
                    value = default;
                    return false;
                }


                value = new Vector2(values[0], values[1]);

                return true;
            }

            public bool TryGetVector3(out Vector3 value)
            {
                if (!HasValue() || values.Length < 3)
                {
                    value = default;
                    return false;
                }


                value = new Vector3(values[0], values[1], values[2]);

                return true;
            }

            public bool TryGetVector4(out Vector4 value)
            {
                if (!HasValue() || values.Length < 4)
                {
                    value = default;
                    return false;
                }


                value = new Vector4(values[0], values[1], values[2], values[3]);

                return true;
            }
        }


        public static bool IsGamepadKey(this KeyCode key) => key >= KeyCode.JoystickButton0 && key <= KeyCode.Joystick8Button19;
        #endregion



        public static Dictionary<string, InputBindingMap> allInputBindingMaps = new();



        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init() => SHUU_Time.onUpdate += Update;


        public static void Update()
        {
            foreach (var map in allInputBindingMaps.Values)
            {
                foreach (var set in map.inputSets_list)
                {
                    foreach (AxisSource source in set.set.validSources.Where(x => x is AxisSource))
                    {
                        source.Tick();
                    }
                }
            }


            UpdateBufferedInputs();

            UpdateListeners();
        }



        #region Buffered Inputs
        private static Dictionary<InputBindingMap, List<BufferedInput>> down_buffereds = new();
        private static Dictionary<InputBindingMap, List<BufferedInput>> up_buffereds = new();

        private const float defaultBufferTime = 0.15f;

        private class BufferedInput
        {
            public string set;

            public float remainingTime;
            public float bufferDuration;
            public bool requiresAllBindsDown;


            public BufferedInput(string set, float bufferDuration, bool requiresAllBindsDown)
            {
                this.set = set;

                this.remainingTime = bufferDuration;
                this.bufferDuration = bufferDuration;
                this.requiresAllBindsDown = requiresAllBindsDown;
            }

            public void ResetBuffer() => remainingTime = bufferDuration;
            public void Consume() => remainingTime = 0f;
        }


        private static void UpdateBufferedInputs()
        {
            if (down_buffereds != null && down_buffereds.Count > 0) UpdateBufferedInputs(down_buffereds, true);
            if (up_buffereds != null && up_buffereds.Count > 0) UpdateBufferedInputs(up_buffereds, false);
        }

        private static void UpdateBufferedInputs(Dictionary<InputBindingMap, List<BufferedInput>> buffereds, bool direction)
        {
            foreach (var mapPair in buffereds)
            {
                var map = mapPair.Key;
                if (map == null || !map.enabled) continue;

                foreach (var buffered in mapPair.Value)
                {
                    if (direction) if (GetInputDown(map, buffered.set, buffered.requiresAllBindsDown)) buffered.ResetBuffer();
                    else if (GetInputUp(map, buffered.set, buffered.requiresAllBindsDown)) buffered.ResetBuffer();

                    if (buffered.remainingTime > 0f) buffered.remainingTime -= Time.deltaTime;
                }
            }
        }


        private static void RegisterBufferInput(Dictionary<InputBindingMap, List<BufferedInput>> buffereds, InputBindingMap map, string set, float bufferTime = defaultBufferTime, bool requiresAllBindsDown = false)
        {
            if (map == null || string.IsNullOrEmpty(set) || bufferTime <= 0f) return;


            if (!buffereds.TryGetValue(map, out var list))
            {
                list = new();

                buffereds[map] = list;
            }


            if (list.Exists(b => b.set == set && b.requiresAllBindsDown == requiresAllBindsDown)) return;

            list.Add(new BufferedInput(set, bufferTime, requiresAllBindsDown));
        }
        private static void UnregisterBufferInput(Dictionary<InputBindingMap, List<BufferedInput>> buffereds, InputBindingMap map, string set, bool requiresAllBindsDown = false)
        {
            if (map == null || string.IsNullOrEmpty(set)) return;
            if (!buffereds.TryGetValue(map, out var list)) return;


            list.RemoveAll(b => b.set == set && b.requiresAllBindsDown == requiresAllBindsDown);

            if (list.Count == 0) buffereds.Remove(map);
        }
        private static void UnregisterBufferInput(Dictionary<InputBindingMap, List<BufferedInput>> buffereds, InputBindingMap map)
        {
            if (map != null && buffereds.ContainsKey(map)) buffereds.Remove(map);
        }

        public static void RegisterBufferInput_Down(this InputBindingMap map, string set, float bufferTime = defaultBufferTime, bool requiresAllBindsDown = false) =>
            RegisterBufferInput(down_buffereds, map, set, bufferTime, requiresAllBindsDown);
        public static void UnregisterBufferInput_Down(this InputBindingMap map, string set, bool requiresAllBindsDown = false) =>
            UnregisterBufferInput(down_buffereds, map, set, requiresAllBindsDown);
        public static void UnregisterBufferInput_Down(this InputBindingMap map) =>
            UnregisterBufferInput(down_buffereds, map);

        public static void RegisterBufferInput_Up(this InputBindingMap map, string set, float bufferTime = defaultBufferTime, bool requiresAllBindsDown = false) =>
            RegisterBufferInput(up_buffereds, map, set, bufferTime, requiresAllBindsDown);
        public static void UnregisterBufferInput_Up(this InputBindingMap map, string set, bool requiresAllBindsDown = false) =>
            UnregisterBufferInput(up_buffereds, map, set, requiresAllBindsDown);
        public static void UnregisterBufferInput_Up(this InputBindingMap map) =>
            UnregisterBufferInput(up_buffereds, map);


        private static bool GetBufferedInput(Dictionary<InputBindingMap, List<BufferedInput>> buffereds, InputBindingMap map, string set, bool requiresAllBindsDown = false, bool consume = true)
        {
            if (map == null || string.IsNullOrEmpty(set)) return false;
            if (!buffereds.TryGetValue(map, out var list)) return false;


            foreach (var buffered in list)
            {
                if (buffered.set == set && buffered.requiresAllBindsDown == requiresAllBindsDown && buffered.remainingTime > 0f)
                {
                    if (consume) buffered.Consume();

                    return true;
                }
            }

            return false;
        }

        public static bool GetBufferedInput_Down(this InputBindingMap map, string set, bool requiresAllBindsDown = false, bool consume = true) =>
            GetBufferedInput(down_buffereds, map, set, requiresAllBindsDown, consume);

        public static bool GetBufferedInput_Up(this InputBindingMap map, string set, bool requiresAllBindsDown = false, bool consume = true) =>
            GetBufferedInput(up_buffereds, map, set, requiresAllBindsDown, consume);
        #endregion



        #region Listeners
        private static Dictionary<InputBindingMap, Dictionary<string, List<(Action callback, bool requiresAllBindsDown)>>> down_listeners = new();
        private static Dictionary<InputBindingMap, Dictionary<string, List<(Action callback, bool requiresAllBindsDown)>>> up_listeners = new();


        private static void UpdateListeners()
        {
            if (down_listeners != null && down_listeners.Count > 0) UpdateListener(down_listeners, true);
            if (up_listeners != null && up_listeners.Count > 0) UpdateListener(up_listeners, false);
        }
        
        private static void UpdateListener(Dictionary<InputBindingMap, Dictionary<string, List<(Action callback, bool requiresAllBindsDown)>>> listeners, bool direction)
        {
            foreach (var mapEntry in listeners)
            {
                InputBindingMap map = mapEntry.Key;

                if (map == null || !map.enabled) continue;

                foreach (var actionEntry in mapEntry.Value)
                {
                    foreach (var binding in actionEntry.Value)
                    {
                        if (direction)
                        {
                            if (GetInputDown(map, actionEntry.Key, binding.requiresAllBindsDown))
                            {
                                binding.callback?.Invoke();

                                break;
                            }
                        }
                        else
                        {
                            if (GetInputUp(map, actionEntry.Key, binding.requiresAllBindsDown))
                            {
                                binding.callback?.Invoke();

                                break;
                            }
                        }
                    }
                }
            }
        }


        private static void RegisterListener(Dictionary<InputBindingMap, Dictionary<string, List<(Action callback, bool requiresAllBindsDown)>>> listeners, InputBindingMap map, string set, Action callback, bool requiresAllBindsDown = false)
        {
            if (map == null || callback == null || string.IsNullOrEmpty(set)) return;


            if (!listeners.TryGetValue(map, out var actionDict))
            {
                actionDict = new();

                listeners[map] = actionDict;
            }

            if (!actionDict.TryGetValue(set, out var bindings))
            {
                bindings = new();

                actionDict[set] = bindings;
            }


            if (!bindings.Any(b => b.callback == callback && b.requiresAllBindsDown == requiresAllBindsDown)) bindings.Add((callback, requiresAllBindsDown));
        }
        private static void UnregisterListener(Dictionary<InputBindingMap, Dictionary<string, List<(Action callback, bool requiresAllBindsDown)>>> listeners, InputBindingMap map, string set, Action callback)
        {
            if (map == null || string.IsNullOrEmpty(set) || callback == null) return;


            if (!listeners.TryGetValue(map, out var actionDict)) return;
            if (!actionDict.TryGetValue(set, out var actionList)) return;

            actionList.RemoveAll(a => a.callback == callback);

            if (actionDict.Count == 0) listeners.Remove(map);
        }
        private static void UnregisterListener(Dictionary<InputBindingMap, Dictionary<string, List<(Action callback, bool requiresAllBindsDown)>>> listeners, InputBindingMap map, string set)
        {
            if (map == null || string.IsNullOrEmpty(set)) return;


            if (!listeners.TryGetValue(map, out var actionDict)) return;

            actionDict.Remove(set);

            if (actionDict.Count == 0) listeners.Remove(map);
        }
        private static void UnregisterListener(Dictionary<InputBindingMap, Dictionary<string, List<(Action callback, bool requiresAllBindsDown)>>> listeners, InputBindingMap map)
        {
            if (map == null) return;
            
            if (listeners.ContainsKey(map)) listeners.Remove(map);
        }

        public static void RegisterListener_Down(this InputBindingMap map, string set, Action callback, bool requiresAllBindsDown = false) =>
            RegisterListener(down_listeners, map, set, callback, requiresAllBindsDown);
        public static void UnregisterListener_Down(this InputBindingMap map, string set, Action callback) =>
            UnregisterListener(down_listeners, map, set, callback);
            public static void UnregisterListener_Down(this InputBindingMap map, string set) =>
            UnregisterListener(down_listeners, map, set);
        public static void UnregisterListener_Down(this InputBindingMap map) =>
            UnregisterListener(down_listeners, map);

        public static void RegisterListener_Up(this InputBindingMap map, string set, Action callback, bool requiresAllBindsDown = false) =>
            RegisterListener(up_listeners, map, set, callback, requiresAllBindsDown);
        public static void UnregisterListener_Up(this InputBindingMap map, string set, Action callback) =>
            UnregisterListener(up_listeners, map, set, callback);
        public static void UnregisterListener_Up(this InputBindingMap map, string set) =>
            UnregisterListener(up_listeners, map, set);
        public static void UnregisterListener_Up(this InputBindingMap map) =>
            UnregisterListener(up_listeners, map);
        #endregion



        #region Input Retrieval
        public static bool GetInput(this InputBindingMap map, string set, bool requiresAllBindsDown = false)
        {
            if (!map.enabled)
            {
                //Debug.LogWarning($"SHUU_Input: InputMap '{map.mapName}' not enabled.");

                return false;
            }


            IInputSet setInterface = RetrieveInputSet(map, set);

            if (setInterface != null) return setInterface.GetInput(requiresAllBindsDown);
            else Debug.LogWarning($"SHUU_Input: InputSet '{set}' not found in map '{map.mapName}'");


            return false;
        }

        public static InputValue GetInputValue(this InputBindingMap map, string set, bool requiresAllBindsDown = false)
        {
            if (!map.enabled)
            {
                //Debug.LogWarning($"SHUU_Input: InputMap '{map.mapName}' not enabled.");

                return new InputValue();
            }


            IInputSet iInputSet = RetrieveInputSet(map, set);

            if (iInputSet is InputSet singleSet)
            {
                return new InputValue(singleSet.GetInputValue(requiresAllBindsDown));
            }
            else if (iInputSet is Composite_InputSet compositeSet)
            {
                float[] axesValues = new float[compositeSet.axisCount];
                for (int i = 0; i < compositeSet.axisCount; i++)
                {
                    axesValues[i] = compositeSet.GetAxisValue(i, requiresAllBindsDown);
                }
                
                return new InputValue(axesValues);
            }
            else Debug.LogWarning($"SHUU_Input: InputSet '{set}' not found in map '{map.mapName}'");


            return new InputValue();
        }


        public static bool GetInputDown(this InputBindingMap map, string set, bool requiresAllBindsDown = false)
        {
            if (!map.enabled)
            {
                //Debug.LogWarning($"SHUU_Input: InputMap '{map.mapName}' not enabled.");

                return false;
            }


            IInputSet setInterface = RetrieveInputSet(map, set);

            if (setInterface != null) return setInterface.GetInputDown(requiresAllBindsDown);
            else Debug.LogWarning($"SHUU_Input: InputSet '{set}' not found in map '{map.mapName}'");


            return false;
        }


        public static bool GetInputUp(this InputBindingMap map, string set, bool requiresAllBindsDown = false)
        {
            if (!map.enabled)
            {
                //Debug.LogWarning($"SHUU_Input: InputMap '{map.mapName}' not enabled.");

                return false;
            }


            IInputSet setInterface = RetrieveInputSet(map, set);

            if (setInterface != null) return setInterface.GetInputUp(requiresAllBindsDown);
            else Debug.LogWarning($"SHUU_Input: InputSet '{set}' not found in map '{map.mapName}'");

            
            return false;
        }
        #endregion


    
        #region Bindings Management
        public static void AddInputBinds(this InputBindingMap map, string name, params DynamicInput[] binds)
        {
            IInputSet setInterface = RetrieveInputSet(map, name);


            if (setInterface == null)
            {
                Debug.LogWarning($"SHUU_Input: InputSet '{name}' not found in map '{map.mapName}'.");
                
                return;
            }


            AddInputBinds(setInterface, binds);
        }

        public static void AddInputBinds(IInputSet setInterface, params DynamicInput[] binds)
        {
            if (setInterface is InputSet singleSet)
            {
                foreach (DynamicInput bind in binds)
                {
                    singleSet.AddBinding(bind);
                }
            }
            else if (setInterface is Composite_InputSet compositeSet)
            {
                if (compositeSet.axisCount == 0 || binds.Length % compositeSet.axisCount != 0)
                {
                    Debug.LogWarning($"SHUU_Input: Amount of binds for Composite set is invalid, must be a multiple of {compositeSet.axisCount}.");

                    return;
                }


                int index = 0;
                int count = 0;
                int sectorCap = binds.Length / compositeSet.axisCount;
                foreach (DynamicInput bind in binds)
                {
                    compositeSet.AddBinding(bind, index);

                    count++;
                    if (count == sectorCap) index++;
                }
            }
            else Debug.LogWarning($"SHUU_Input: InputSet is invalid.");
        }


        public static void RemoveInputBinds(this InputBindingMap map, string name, params DynamicInput[] binds)
        {
            IInputSet setInterface = RetrieveInputSet(map, name);


            if (setInterface == null)
            {
                Debug.LogWarning($"SHUU_Input: InputSet '{name}' not found in map '{map.mapName}'.");
                
                return;
            }


            RemoveInputBinds(setInterface, binds);
        }

        public static void RemoveInputBinds(IInputSet setInterface, params DynamicInput[] binds)
        {
            if (setInterface is InputSet singleSet)
            {
                foreach (DynamicInput bind in binds)
                {
                    singleSet.RemoveBinding(bind);
                }
            }
            else if (setInterface is Composite_InputSet compositeSet)
            {
                if (compositeSet.axisCount == 0 || binds.Length % compositeSet.axisCount != 0)
                {
                    Debug.LogWarning($"SHUU_Input: Amount of binds for Composite set is invalid, must be a multiple of {compositeSet.axisCount}.");

                    return;
                }


                int index = 0;
                int count = 0;
                int sectorCap = binds.Length / compositeSet.axisCount;
                foreach (DynamicInput bind in binds)
                {
                    compositeSet.RemoveBinding(bind, index);

                    count++;
                    if (count == sectorCap) index++;
                }
            }
            else Debug.LogWarning($"SHUU_Input: InputSet is invalid.");
        }


        public static void ClearInputBinds(this InputBindingMap map, string name)
        {
            IInputSet setInterface = RetrieveInputSet(map, name);


            if (setInterface == null)
            {
                Debug.LogWarning($"SHUU_Input: InputSet '{name}' not found in map '{map.mapName}'.");
                
                return;
            }


            setInterface.ClearBindings();
        }


        public static void RebindInputSet(this InputBindingMap map, string name, params DynamicInput[] binds)
        {
            IInputSet setInterface = RetrieveInputSet(map, name);


            if (setInterface == null)
            {
                Debug.LogWarning($"SHUU_Input: InputSet '{name}' not found in map '{map.mapName}'.");
                
                return;
            }


            RebindInputSet(setInterface, binds);
        }

        public static void RebindInputSet(IInputSet setInterface, params DynamicInput[] binds)
        {
            setInterface.ClearBindings();

            AddInputBinds(setInterface, binds);
        }
        #endregion
    
    
    
        #region Misc
        public static InputSet RetrieveSingleInputSet(this InputBindingMap map, string name)
        {
            if (map.TryGetSingleSet(name, out InputSet singleSet))
            {
                return singleSet;
            }
            else Debug.LogWarning($"SHUU_Input: Single InputSet '{name}' not found in map '{map.mapName}'.");


            return null;
        }

        public static Composite_InputSet RetrieveCompositeInputSet(this InputBindingMap map, string name)
        {
            if (map.TryGetCompositeSet(name, out Composite_InputSet compositeSet))
            {
                return compositeSet;
            }
            else Debug.LogWarning($"SHUU_Input: Composite InputSet '{name}' not found in map '{map.mapName}'.");


            return null;
        }


        public static IInputSet RetrieveInputSet(this InputBindingMap map, string name)
        {
            if (map.TryGetSingleSet(name, out InputSet singleSet))
            {
                return singleSet;
            }
            else if (map.TryGetCompositeSet(name, out Composite_InputSet compositeSet))
            {
                return compositeSet;
            }
            else Debug.LogWarning($"SHUU_Input: InputSet '{name}' not found in map '{map.mapName}'.");


            return null;
        }


        public static InputBindingMap RetrieveBindingMap(string name)
        {
            if (allInputBindingMaps == null) return null;


            InputBindingMap _map = null;

            foreach (InputBindingMap map in allInputBindingMaps.Values)
            {
                if (map.mapName.ToLower() == name.ToLower())
                {
                    _map = map;
                }
            }

            return _map;
        }
        #endregion
    }
}
