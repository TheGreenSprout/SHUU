using UnityEngine;

namespace SHUU.Utils.InputSystem
{

    public static class SHUU_Input
    {
        #region Data Classes
        public class DynamicInput
        {
            public (KeyCode?, int?) bind = (null, null);



            public DynamicInput(KeyCode key)
            {
                bind = (key, null);
            }

            public DynamicInput(int mouseButton)
            {
                bind = (null, mouseButton);
            }

            public DynamicInput(string input)
            {
                bind = InputParser.ParseInput(input);
            }


            public bool IsValid()
            {
                return !(((bind.Item1 == null) && (bind.Item2 == null)) || ((bind.Item1 != null) && (bind.Item2 != null)));
            }

            public bool IsKey()
            {
                return IsValid() && bind.Item1 != null;
            }

            public bool IsMouse()
            {
                return IsValid() && bind.Item2 != null;
            }


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

        public static DynamicInput[] CreateDynamicInputArray(params string[] input)
        {
            DynamicInput[] inputs = new DynamicInput[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                inputs[i] = new DynamicInput(input[i]);
            }

            return inputs;
        }


        public class InputValue
        {
            public float? x = null;

            public float? y = null;



            public bool HasValue()
            {
                return x != null || y != null;
            }

            public bool IsFloat()
            {
                return x != null && y != null;
            }

            public bool IsVector2()
            {
                return x != null && y != null;
            }


            public bool TryGetFloat(out float value)
            {
                if (!HasValue() || IsVector2())
                {
                    value = 0f;
                    return false;
                }


                value = x.Value;

                return true;
            }

            public bool TryGetVector2(out Vector2 value)
            {
                if (!HasValue() || !IsVector2())
                {
                    value = default;
                    return false;
                }


                value = new Vector2(x.Value, y.Value);

                return true;
            }
        }
        #endregion



        #region General Bindings
        public static bool GetInput(InputBindingMap map, string name, bool requiresAllBindsDown = false)
        {
            (InputSet, Composite_InputSet) setTouple = RetrieveInputSet(map, name);

            if (setTouple.Item1 != null)
            {
                return GetSingleInput(setTouple.Item1, requiresAllBindsDown);
            }
            else if (setTouple.Item2 != null)
            {
                return GetCompositeInput(setTouple.Item2, requiresAllBindsDown);
            }
            else Debug.LogWarning($"SHUU_Input: InputSet '{name}' not found in map '{map.mapName}'");


            return false;
        }

        public static InputValue GetInputValue(InputBindingMap map, string name, bool requiresAllBindsDown = false)
        {
            (InputSet, Composite_InputSet) setTouple = RetrieveInputSet(map, name);

            if (setTouple.Item1 != null)
            {
                return new InputValue{ x = GetSingleInputValue(setTouple.Item1, requiresAllBindsDown) };
            }
            else if (setTouple.Item2 != null)
            {
                Vector2 compostiveVal = GetCompositeInputValue(setTouple.Item2, requiresAllBindsDown);

                return new InputValue{ x = compostiveVal.x, y = compostiveVal.y };
            }
            else Debug.LogWarning($"SHUU_Input: InputSet '{name}' not found in map '{map.mapName}'");


            return new InputValue();
        }
        public static float GetInputValue_float(InputBindingMap map, string name, bool requiresAllBindsDown = false)
        {
            (InputSet, Composite_InputSet) setTouple = RetrieveInputSet(map, name);

            if (setTouple.Item1 != null)
            {
                return GetSingleInputValue(setTouple.Item1, requiresAllBindsDown);
            }
            else if (setTouple.Item2 != null) Debug.LogWarning($"SHUU_Input: InputSet '{name}' in map '{map.mapName}' is composite, not single.");
            else Debug.LogWarning($"SHUU_Input: InputSet '{name}' not found in map '{map.mapName}'");


            return 0f;
        }
        public static Vector2 GetInputValue_Vector2(InputBindingMap map, string name, bool requiresAllBindsDown = false)
        {
            (InputSet, Composite_InputSet) setTouple = RetrieveInputSet(map, name);

            if (setTouple.Item1 != null)
            {
                Debug.LogWarning($"SHUU_Input: Composite_InputSet '{name}' not found in map '{map.mapName}'. Returning float in x val.");

                return new Vector2(GetSingleInputValue(setTouple.Item1, requiresAllBindsDown), 0);
            }
            else if (setTouple.Item2 != null)
            {
                Vector2 compostiveVal = GetCompositeInputValue(setTouple.Item2, requiresAllBindsDown);

                return new Vector2(compostiveVal.x, compostiveVal.y);
            }
            else Debug.LogWarning($"SHUU_Input: InputSet '{name}' not found in map '{map.mapName}'");


            return Vector2.zero;
        }


        public static bool GetInputDown(InputBindingMap map, string name, bool requiresAllBindsDown = false)
        {
            (InputSet, Composite_InputSet) setTouple = RetrieveInputSet(map, name);

            if (setTouple.Item1 != null)
            {
                return GetSingleInputDown(setTouple.Item1, requiresAllBindsDown);
            }
            else if (setTouple.Item2 != null)
            {
                return GetCompositeInputDown(setTouple.Item2, requiresAllBindsDown);
            }
            else Debug.LogWarning($"SHUU_Input: InputSet '{name}' not found in map '{map.mapName}'");


            return false;
        }


        public static bool GetInputUp(InputBindingMap map, string name, bool requiresAllBindsDown = false)
        {
            (InputSet, Composite_InputSet) setTouple = RetrieveInputSet(map, name);

            if (setTouple.Item1 != null)
            {
                return GetSingleInputUp(setTouple.Item1, requiresAllBindsDown);
            }
            else if (setTouple.Item2 != null)
            {
                return GetCompositeInputUp(setTouple.Item2, requiresAllBindsDown);
            }
            else Debug.LogWarning($"SHUU_Input: InputSet '{name}' not found in map '{map.mapName}'");

            
            return false;
        }
        #endregion



        #region Single Bindings
        private static bool GetSingleInput(InputSet set, bool requiresAllBindsDown = false)
        {
            bool result = requiresAllBindsDown;

            foreach (KeyCode input in set.valid_keyBinds)
            {
                if (!requiresAllBindsDown) result |= Input.GetKey(input);
                else result &= Input.GetKey(input);
            }
            foreach (int mouseButton in set.valid_mouseBinds)
            {
                if (!requiresAllBindsDown) result |= Input.GetMouseButton(mouseButton);
                else result &= Input.GetMouseButton(mouseButton);
            }

            return result;
        }

        private static float GetSingleInputValue(InputSet set, bool requiresAllBindsDown = false)
        {
            return GetSingleInput(set, requiresAllBindsDown) ? 1f : 0f;
        }
        
        
        private static bool GetSingleInputDown(InputSet set, bool requiresAllBindsDown = false)
        {
            bool result = false;

            foreach (KeyCode input in set.valid_keyBinds)
            {
                result |= Input.GetKeyDown(input);
            }
            foreach (int mouseButton in set.valid_mouseBinds)
            {
                result |= Input.GetMouseButtonDown(mouseButton);
            }


            if (requiresAllBindsDown) result &= GetSingleInput(set, true);

            return result;
        }


        private static bool GetSingleInputUp(InputSet set, bool requiresAllBindsDown = false)
        {
            bool result = false;

            foreach (KeyCode input in set.valid_keyBinds)
            {
                result |= Input.GetKeyUp(input);
            }
            foreach (int mouseButton in set.valid_mouseBinds)
            {
                result |= Input.GetMouseButtonUp(mouseButton);
            }


            if (requiresAllBindsDown) result &= GetSingleInput(set, true);

            return result;
        }
        #endregion



        #region Composite Bindings
        private static bool GetCompositeInput(Composite_InputSet compositeSet, bool requiresAllBindsDown = false)
        {
            bool result = false;

            foreach (InputSet set in compositeSet.parts)
            {
                result |= GetSingleInput(set, requiresAllBindsDown);
            }

            return result;
        }

        // [+x, -x, +y, -y]
        public static Vector2 GetCompositeInputValue(Composite_InputSet compositeSet, bool requiresAllBindsDown = false)
        {
            Vector2 result = Vector2.zero;

            int index = 0;
            foreach (InputSet set in compositeSet.parts)
            {
                float value = GetSingleInputValue(set, requiresAllBindsDown);


                switch (index)
                {
                    case 0:
                        result.x += value;
                        break;
                    case 1:
                        result.x -= value;
                        break;
                    case 2:
                        result.y += value;
                        break;
                    case 3:
                        result.y -= value;
                        break;
                }


                index++;
            }

            return result;
        }

        
        private static bool GetCompositeInputDown(Composite_InputSet compositeSet, bool requiresAllBindsDown = false)
        {
            bool result = false;

            foreach (InputSet set in compositeSet.parts)
            {
                result |= GetSingleInputDown(set, requiresAllBindsDown);
            }

            return result;
        }


        private static bool GetCompositeInputUp(Composite_InputSet compositeSet, bool requiresAllBindsDown = false)
        {
            bool result = false;

            foreach (InputSet set in compositeSet.parts)
            {
                result |= GetSingleInputUp(set, requiresAllBindsDown);
            }

            return result;
        }
        #endregion


    
        #region Bindings Management
        public static void AddInputBinds(InputBindingMap map, string name, params DynamicInput[] binds)
        {
            (InputSet, Composite_InputSet) touple = RetrieveInputSet(map, name);


            if (touple == (null, null))
            {
                Debug.LogWarning($"SHUU_Input: InputSet '{name}' not found in map '{map.mapName}'.");
                
                return;
            }


            AddInputBinds(touple, binds);
        }

        public static void AddInputBinds((InputSet, Composite_InputSet) setTouple, params DynamicInput[] binds)
        {
            if (setTouple.Item1 != null)
            {
                foreach (DynamicInput bind in binds)
                {
                    setTouple.Item1.AddBinding(bind);
                }
            }
            else if (setTouple.Item2 != null)
            {
                if (binds.Length % 4 != 0)
                {
                    Debug.LogWarning($"SHUU_Input: Ammount of binds for Composite set is invalid, must be a multiple of 4.");

                    return;
                }


                int index = 0;
                foreach (DynamicInput bind in binds)
                {
                    setTouple.Item2.AddBinding(bind, index);

                    index++;
                    if (index == 4) index = 0;
                }
            }
        }


        public static void RemoveInputBinds(InputBindingMap map, string name, params DynamicInput[] binds)
        {
            (InputSet, Composite_InputSet) touple = RetrieveInputSet(map, name);


            if (touple == (null, null))
            {
                Debug.LogWarning($"SHUU_Input: InputSet '{name}' not found in map '{map.mapName}'.");
                
                return;
            }


            RemoveInputBinds(touple, binds);
        }

        public static void RemoveInputBinds((InputSet, Composite_InputSet) setTouple, params DynamicInput[] binds)
        {
            if (setTouple.Item1 != null)
            {
                foreach (DynamicInput bind in binds)
                {
                    setTouple.Item1.RemoveBinding(bind);
                }
            }
            else if (setTouple.Item2 != null)
            {
                if (binds.Length % 4 != 0)
                {
                    Debug.LogWarning($"SHUU_Input: Ammount of binds for Composite set is invalid, must be a multiple of 4.");

                    return;
                }


                int index = 0;
                foreach (DynamicInput bind in binds)
                {
                    setTouple.Item2.RemoveBinding(bind, index);

                    index++;
                    if (index == 4) index = 0;
                }
            }
        }


        public static void ClearInputBinds(InputBindingMap map, string name)
        {
            (InputSet, Composite_InputSet) touple = RetrieveInputSet(map, name);


            if (touple == (null, null))
            {
                Debug.LogWarning($"SHUU_Input: InputSet '{name}' not found in map '{map.mapName}'.");
                
                return;
            }


            ClearInputBinds(touple);
        }

        public static void ClearInputBinds((InputSet, Composite_InputSet) setTouple)
        {
            if (setTouple.Item1 != null)
            {
                setTouple.Item1.ClearBindings();
            }
            else if (setTouple.Item2 != null)
            {
                setTouple.Item2.ClearBindings();
            }
        }


        public static void RebindInputSet(InputBindingMap map, string name, params DynamicInput[] binds)
        {
            (InputSet, Composite_InputSet) touple = RetrieveInputSet(map, name);


            if (touple == (null, null))
            {
                Debug.LogWarning($"SHUU_Input: InputSet '{name}' not found in map '{map.mapName}'.");
                
                return;
            }


            RebindInputSet(touple, binds);
        }

        public static void RebindInputSet((InputSet, Composite_InputSet) setTouple, params DynamicInput[] binds)
        {
            ClearInputBinds(setTouple);

            AddInputBinds(setTouple, binds);
        }
        #endregion
    
    
    
        #region Misc
        public static InputSet RetrieveSingleInputSet(InputBindingMap map, string name)
        {
            if (map.TryGetSingleSet(name, out InputSet singleSet))
            {
                return singleSet;
            }
            else Debug.LogWarning($"SHUU_Input: InputSet '{name}' not found in map '{map.mapName}'.");



            return null;
        }

        public static Composite_InputSet RetrieveCompositeInputSet(InputBindingMap map, string name)
        {
            if (map.TryGetCompositeSet(name, out Composite_InputSet compositeSet))
            {
                return compositeSet;
            }
            else Debug.LogWarning($"SHUU_Input: InputSet '{name}' not found in map '{map.mapName}'.");



            return null;
        }


        public static (InputSet, Composite_InputSet) RetrieveInputSet(InputBindingMap map, string name)
        {
            if (map.TryGetSingleSet(name, out InputSet singleSet))
            {
                return (singleSet, null);
            }
            else if (map.TryGetCompositeSet(name, out Composite_InputSet compositeSet))
            {
                return (null, compositeSet);
            }
            else Debug.LogWarning($"SHUU_Input: InputSet '{name}' not found in map '{map.mapName}'.");


            return (null, null);
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
