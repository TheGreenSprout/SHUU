using System;
using System.Collections.Generic;
using SHUU.Utils.Helpers;
using UnityEngine;

namespace SHUU.Utils.InputSystem
{
    #region Data Classes
    public struct DynamicInput
    {
        public (KeyCode?, int?) bind;

        public bool direction;



        public DynamicInput(KeyCode key, bool direction = true)
        {
            bind = (key, null);
            this.direction = direction;
        }

        public DynamicInput(int mouseButton, bool direction = true)
        {
            bind = (null, mouseButton);
            this.direction = direction;
        }

        public DynamicInput(string input)
        {
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
            return !(((bind.Item1 == null) && (bind.Item2 == null)) || ((bind.Item1 != null) && (bind.Item2 != null)));
        }

        public bool IsKey() => IsValid() && bind.Item1 != null;

        public bool IsMouse() => IsValid() && bind.Item2 != null;


        public bool TryGetKey(out KeyCode key)
        {
            if (!IsKey())
            {
                key = default;
                
                return false;
            }


            key = bind.Item1.Value;

            return true;
        }

        public bool TryGetMouse(out int mouse)
        {
            if (!IsMouse())
            {
                mouse = default;
                
                return false;
            }


            mouse = bind.Item2.Value;

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

            public int ammountOfValues => values != null ? values.Length : 0;



            public InputValue(params float[] values)
            {
                this.values = values;
            }


            public bool HasValue() => ammountOfValues > 0;


            public bool TryGetFloat(out float value, int axis = 0)
            {
                if (!HasValue() || !values.IndexIsValid(axis))
                {
                    value = 0f;
                    return false;
                }


                value = values[axis];

                return true;
            }

            public bool TryGetVector2(out Vector2 value, int axis1 = 0, int axis2 = 1)
            {
                if (!HasValue() || values.Length < 2 || !values.IndexIsValid(axis1) || !values.IndexIsValid(axis2))
                {
                    value = default;
                    return false;
                }


                value = new Vector2(values[axis1], values[axis2]);

                return true;
            }

            public bool TryGetVector3(out Vector3 value, int axis1 = 0, int axis2 = 1, int axis3 = 2)
            {
                if (!HasValue() || values.Length < 3 || !values.IndexIsValid(axis1) || !values.IndexIsValid(axis2) || !values.IndexIsValid(axis3))
                {
                    value = default;
                    return false;
                }


                value = new Vector3(values[axis1], values[axis2], values[axis3]);

                return true;
            }

            public bool TryGetVector4(out Vector4 value, int axis1 = 0, int axis2 = 1, int axis3 = 2, int axis4 = 3)
            {
                if (!HasValue() || values.Length < 4 || !values.IndexIsValid(axis1) || !values.IndexIsValid(axis2) || !values.IndexIsValid(axis3) || !values.IndexIsValid(axis4))
                {
                    value = default;
                    return false;
                }


                value = new Vector4(values[axis1], values[axis2], values[axis3], values[axis4]);

                return true;
            }
        }
        #endregion



        public static void Update()
        {
            UpdateBufferedInputs();

            UpdateListeners();
        }



        #region Buffered Inputs
        private static Dictionary<(InputBindingMap, string), BufferedInput> down_buffereds = new();
        private static Dictionary<(InputBindingMap, string), BufferedInput> up_buffereds = new();

        private const float defaultBufferTime = 0.15f;

        private class BufferedInput
        {
            public float remainingTime;
            public float bufferDuration;
            public bool requiresAllBindsDown;


            public BufferedInput(float bufferDuration, bool requiresAllBindsDown)
            {
                this.remainingTime = bufferDuration;
                this.bufferDuration = bufferDuration;
                this.requiresAllBindsDown = requiresAllBindsDown;
            }

            public void ResetBuffer() => remainingTime = bufferDuration;
        }


        private static void UpdateBufferedInputs()
        {
            if (down_buffereds != null && down_buffereds.Count > 0) UpdateBuffereds_Down();
            if (up_buffereds != null && up_buffereds.Count > 0) UpdateBuffereds_Up();
        }

        private static void UpdateBuffereds_Down()
        {
            List<(InputBindingMap, string)> keys = new(down_buffereds.Keys);

            foreach (var key in keys)
            {
                if (GetInputDown(key.Item1, key.Item2, down_buffereds[key].requiresAllBindsDown)) down_buffereds[key].ResetBuffer();


                if (down_buffereds[key].remainingTime > 0f) down_buffereds[key].remainingTime -= Time.deltaTime;
            }
        }
        private static void UpdateBuffereds_Up()
        {
            List<(InputBindingMap, string)> keys = new(up_buffereds.Keys);

            foreach (var key in keys)
            {
                if (GetInputDown(key.Item1, key.Item2, up_buffereds[key].requiresAllBindsDown)) up_buffereds[key].ResetBuffer();

                if (up_buffereds[key].remainingTime > 0f) up_buffereds[key].remainingTime -= Time.deltaTime;
            }
        }


        public static void RegisterBufferInput_Down(this InputBindingMap map, string set, float bufferTime = defaultBufferTime, bool requiresAllBindsDown = false)
        {
            if (down_buffereds == null) down_buffereds = new();

            if (map == null || string.IsNullOrEmpty(set) || bufferTime <= 0f) return;

            if (down_buffereds.ContainsKey((map, set))) return;


            down_buffereds.Add((map, set), new BufferedInput(bufferTime, requiresAllBindsDown));
        }
        public static void UnregisterBufferInput_Down(this InputBindingMap map, string set, bool requiresAllBindsDown = false)
        {
            if (down_buffereds == null || down_buffereds.Count == 0) return;

            if (map == null || string.IsNullOrEmpty(set)) return;

            if (!down_buffereds.ContainsKey((map, set))) return;


            down_buffereds.Remove((map, set));
        }

        public static void RegisterBufferInput_Up(this InputBindingMap map, string set, float bufferTime = defaultBufferTime, bool requiresAllBindsDown = false)
        {
            if (up_buffereds == null) up_buffereds = new();

            if (map == null || string.IsNullOrEmpty(set) || bufferTime <= 0f) return;

            if (up_buffereds.ContainsKey((map, set))) return;


            up_buffereds.Add((map, set), new BufferedInput(bufferTime, requiresAllBindsDown));
        }
        public static void UnregisterBufferInput_Up(this InputBindingMap map, string set, bool requiresAllBindsDown = false)
        {
            if (up_buffereds == null || up_buffereds.Count == 0) return;

            if (map == null || string.IsNullOrEmpty(set)) return;

            if (!up_buffereds.ContainsKey((map, set))) return;


            up_buffereds.Remove((map, set));
        }


        public static bool GetBufferedInput_Down(this InputBindingMap map, string set, bool requiresAllBindsDown = false)
        {
            if (down_buffereds == null || down_buffereds.Count == 0) return false;

            if (map == null || string.IsNullOrEmpty(set)) return false;

            if (!down_buffereds.ContainsKey((map, set))) return false;


            return down_buffereds[(map, set)].remainingTime > 0f;
        }

        public static bool GetBufferedInput_Up(this InputBindingMap map, string set, bool requiresAllBindsDown = false)
        {
            if (up_buffereds == null || up_buffereds.Count == 0) return false;

            if (map == null || string.IsNullOrEmpty(set)) return false;

            if (!up_buffereds.ContainsKey((map, set))) return false;
            

            return up_buffereds[(map, set)].remainingTime > 0f;
        }
        #endregion



        #region Listeners
        private static Dictionary<Action, (InputBindingMap, string, bool)> down_listeners = new();
        private static Dictionary<Action, (InputBindingMap, string, bool)> up_listeners = new();


        private static void UpdateListeners()
        {
            if (down_listeners != null && down_listeners.Count > 0) UpdateListener_Down();
            if (up_listeners != null && up_listeners.Count > 0) UpdateListener_Up();
        }

        private static void UpdateListener_Down()
        {
            foreach (var listener in down_listeners)
            {
                bool inputState = GetInputDown(listener.Value.Item1, listener.Value.Item2, listener.Value.Item3);

                if (inputState) listener.Key?.Invoke();
            }
        }
        private static void UpdateListener_Up()
        {
            foreach (var listener in up_listeners)
            {
                bool inputState = GetInputUp(listener.Value.Item1, listener.Value.Item2, listener.Value.Item3);

                if (inputState) listener.Key?.Invoke();
            }
        }


        public static void RegisterListener_Down(this InputBindingMap map, string set, Action callback, bool requiresAllBindsDown = false)
        {
            if (down_listeners == null) down_listeners = new();

            if (callback == null || map == null || string.IsNullOrEmpty(set)) return;


            down_listeners.Add(callback, (map, set, requiresAllBindsDown));
        }
        public static void UnregisterListener_Down(Action callback)
        {
            if (down_listeners == null || down_listeners.Count == 0 || !down_listeners.ContainsKey(callback)) return;

            if (callback == null) return;


            down_listeners.Remove(callback);
        }

        public static void RegisterListener_Up(this InputBindingMap map, string set, Action callback, bool requiresAllBindsDown = false)
        {
            if (up_listeners == null) up_listeners = new();

            if (callback == null || map == null || string.IsNullOrEmpty(set)) return;


            up_listeners.Add(callback, (map, set, requiresAllBindsDown));
        }
        public static void UnregisterListener_Up(Action callback)
        {
            if (up_listeners == null || up_listeners.Count == 0 || !up_listeners.ContainsKey(callback)) return;

            if (callback == null) return;


            up_listeners.Remove(callback);
        }
        #endregion



        #region Input Retrieval
        public static bool GetInput(this InputBindingMap map, string set, bool requiresAllBindsDown = false)
        {
            if (!map.enabled)
            {
                Debug.LogWarning($"SHUU_Input: InputMap '{map.mapName}' not enabled.");

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
                Debug.LogWarning($"SHUU_Input: InputMap '{map.mapName}' not enabled.");

                return new InputValue();
            }


            IInputSet setTouple = RetrieveInputSet(map, set);

            if (setTouple is InputSet singleSet)
            {
                return new InputValue(singleSet.GetInputValue(requiresAllBindsDown));
            }
            else if (setTouple is Composite_InputSet compositeSet)
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
                Debug.LogWarning($"SHUU_Input: InputMap '{map.mapName}' not enabled.");

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
                Debug.LogWarning($"SHUU_Input: InputMap '{map.mapName}' not enabled.");

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
                if (binds.Length % compositeSet.axisCount != 0)
                {
                    Debug.LogWarning($"SHUU_Input: Ammount of binds for Composite set is invalid, must be a multiple of {compositeSet.axisCount}.");

                    return;
                }


                int index = 0;
                foreach (DynamicInput bind in binds)
                {
                    compositeSet.AddBinding(bind, index);

                    index++;
                    if (index == compositeSet.axisCount) index = 0;
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
                if (binds.Length % compositeSet.axisCount != 0)
                {
                    Debug.LogWarning($"SHUU_Input: Ammount of binds for Composite set is invalid, must be a multiple of {compositeSet.axisCount}.");

                    return;
                }


                int index = 0;
                foreach (DynamicInput bind in binds)
                {
                    compositeSet.RemoveBinding(bind, index);

                    index++;
                    if (index == compositeSet.axisCount) index = 0;
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
            InputBindingMap _map = null;

            foreach (InputBindingMap map in InputTracker.allInputBindingMaps.Values)
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
