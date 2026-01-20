using System;
using System.Collections.Generic;
using SHUU.Utils.Helpers;
using UnityEngine;

//! Requires a way to retrieve all the bindings from the map, so they can be for example displayed on an options menu.

namespace SHUU.Utils.InputSystem
{
    #region Set Classes
    public interface IInputSet
    {
        public bool GetInput(bool requiresAllBindsDown = false);
        public bool GetInputDown(bool requiresAllBindsDown = false);
        public bool GetInputUp(bool requiresAllBindsDown = false);


        public void ClearBindings();
    }



    [Serializable]
    public class NAMED_InputSet
    {
        public string name;

        public InputSet set = new InputSet();
    }

    [Serializable]
    public class InputSet : IInputSet
    {
        public bool enabled = true;

        public List<KeyCode> valid_keyBinds = new List<KeyCode>();
        public List<int> valid_mouseBinds = new List<int>();


        public void AddBinding(DynamicInput bind)
        {
            if (!bind.IsValid()) return;


            if (bind.TryGetKey(out KeyCode key))
            {
                if (valid_keyBinds.Contains(key)) Debug.LogError("Repeated Keyboard Binding on AddBind().");

                AddBinding(key);
            }
            else if (bind.TryGetMouse(out int mouse))
            {
                if (valid_mouseBinds.Contains(mouse)) Debug.LogError("Repeated Mouse Binding on AddBind().");

                AddBinding(mouse);
            }
            else Debug.LogError("Invalid Dynamic Input on AddBind().");
        }
        public void AddBinding(KeyCode bind) {
            if (!valid_keyBinds.Contains(bind)) valid_keyBinds.Add(bind);
            else Debug.LogWarning("Repeated Keyboard Binding on AddBind().");
        }
        public void AddBinding(int bind) {
            if (!valid_mouseBinds.Contains(bind)) valid_mouseBinds.Add(bind);
            else Debug.LogWarning("Repeated Mouse Binding on AddBind().");
        }

        public void RemoveBinding(DynamicInput bind)
        {
            if (!bind.IsValid()) return;

            
            if (bind.TryGetKey(out KeyCode key))
            {
                RemoveBinding(key);
            }
            else if (bind.TryGetMouse(out int mouse))
            {
                RemoveBinding(mouse);
            }
            else Debug.LogError("Invalid Dynamic Input on RemoveBind().");
        }
        public void RemoveBinding(KeyCode bind) { valid_keyBinds.Remove(bind); }
        public void RemoveBinding(int bind) { valid_mouseBinds.Remove(bind); }

        public void ClearBindings()
        {
            valid_keyBinds.Clear();
            valid_mouseBinds.Clear();
        }


        public bool GetInput(bool requiresAllBindsDown = false)
        {
            bool result = requiresAllBindsDown;

            foreach (KeyCode input in valid_keyBinds)
            {
                if (!requiresAllBindsDown) result |= Input.GetKey(input);
                else result &= Input.GetKey(input);
            }
            foreach (int mouseButton in valid_mouseBinds)
            {
                if (!requiresAllBindsDown) result |= Input.GetMouseButton(mouseButton);
                else result &= Input.GetMouseButton(mouseButton);
            }

            return result;
        }

        public bool GetInputDown(bool requiresAllBindsDown = false)
        {
            bool result = false;

            foreach (KeyCode input in valid_keyBinds)
            {
                result |= Input.GetKeyDown(input);
            }
            foreach (int mouseButton in valid_mouseBinds)
            {
                result |= Input.GetMouseButtonDown(mouseButton);
            }


            if (requiresAllBindsDown) result &= GetInput(true);

            return result;
        }
        public bool GetInputUp(bool requiresAllBindsDown = false)
        {
            bool result = false;

            foreach (KeyCode input in valid_keyBinds)
            {
                result |= Input.GetKeyUp(input);
            }
            foreach (int mouseButton in valid_mouseBinds)
            {
                result |= Input.GetMouseButtonUp(mouseButton);
            }


            if (requiresAllBindsDown) result &= GetInput(true);

            return result;
        }

        public float GetInputValue(bool requiresAllBindsDown = false) => GetInput(requiresAllBindsDown) ? 1f : 0f;
    }


    [Serializable]
    public class NAMED_Composite_InputSet
    {
        public string name;

        public Composite_InputSet set = new Composite_InputSet();
    }

    /*[Serializable]
    public class Composite_InputSet
    {
        public bool enabled
        {
            get
            {
                foreach (InputSet part in parts)
                {
                    if (!part.enabled) return false;
                }

                return true;
            }

            set
            {
                foreach (InputSet part in parts)
                {
                    part.enabled = value;
                }
            }
        }

        public InputSet[] parts = new InputSet[4]
        {
            new InputSet(), new InputSet(),
            new InputSet(), new InputSet()
        };


        public void AddBinding(SHUU_Input.DynamicInput bind, int index)
        {
            if (!bind.IsValid()) return;

            if (parts == null || parts.Length != 4) parts = new InputSet[4] { new InputSet(), new InputSet(), new InputSet(), new InputSet() }; 

            
            if (bind.TryGetKey(out KeyCode key))
            {
                AddBinding(key, index);
            }
            else if (bind.TryGetMouse(out int mouse))
            {
                AddBinding(mouse, index);
            }
            else Debug.LogError("Invalid Dynamic Input on Composite AddBind().");
        }
        public void AddBinding(KeyCode bind, int index)
        {
            if (parts[index].valid_keyBinds.Contains(bind)) Debug.LogError("Repeated Keyboard Binding on Composite AddBind().");

            parts[index].AddBinding(bind);
        }
        public void AddBinding(int bind, int index)
        {
            if (parts[index].valid_mouseBinds.Contains(bind)) Debug.LogError("Repeated Mouse Binding on Composite AddBind().");

            parts[index].AddBinding(bind);
        }

        public void RemoveBinding(SHUU_Input.DynamicInput bind, int index)
        {
            if (!bind.IsValid()) return;

            
            if (bind.TryGetKey(out KeyCode key))
            {
                RemoveBinding(key, index);
            }
            else if (bind.TryGetMouse(out int mouse))
            {
                RemoveBinding(mouse, index);
            }
            else Debug.LogError("Invalid Dynamic Input on Composite RemoveBind().");
        }
        public void RemoveBinding(KeyCode bind, int index) { parts[index].RemoveBinding(bind); }
        public void RemoveBinding(int bind, int index) { parts[index].RemoveBinding(bind); }

        public void ClearBindings()
        {
            foreach (InputSet set in parts) set.ClearBindings();
        }
    }*/
    [Serializable]
    public class Composite_InputSet : IInputSet
    {
        public bool enabled
        {
            get
            {
                foreach (Composite_Axis axis in axes)
                {
                    if (!axis.enabled) return false;
                }

                return true;
            }

            set
            {
                foreach (Composite_Axis axis in axes) axis.enabled = value;
            }
        }

        public int axisCount
        {
            get => axes.Count;
            set
            {
                if (value < 0) value = 0;

                while (axes.Count < value)
                {
                    axes.Add(new Composite_Axis());
                }

                while (axes.Count > value)
                {
                    axes.RemoveAt(axes.Count - 1);
                }
            }
        }

        public List<Composite_Axis> axes = new();


        public void AddBinding(DynamicInput bind, int index)
        {
            if (!bind.IsValid() || !axes.IndexIsValid(index)) return;

            
            if (bind.TryGetKey(out KeyCode key))
            {
                AddBinding(key, index, bind.direction);
            }
            else if (bind.TryGetMouse(out int mouse))
            {
                AddBinding(mouse, index, bind.direction);
            }
            else Debug.LogError("Invalid Dynamic Input on Composite RemoveBind().");
        }
        public void AddBinding(KeyCode bind, int index, bool direction) => axes[index].AddBinding(bind, direction);
        public void AddBinding(int bind, int index, bool direction) => axes[index].AddBinding(bind, direction);

        public void RemoveBinding(DynamicInput bind, int index)
        {
            if (!bind.IsValid() || !axes.IndexIsValid(index)) return;

            
            if (bind.TryGetKey(out KeyCode key))
            {
                RemoveBinding(key, index, bind.direction);
            }
            else if (bind.TryGetMouse(out int mouse))
            {
                RemoveBinding(mouse, index, bind.direction);
            }
            else Debug.LogError("Invalid Dynamic Input on Composite RemoveBind().");
        }
        public void RemoveBinding(KeyCode bind, int index, bool direction) => axes[index].RemoveBinding(bind, direction);
        public void RemoveBinding(int bind, int index, bool direction) => axes[index].RemoveBinding(bind, direction);

        public void ClearBindings()
        {
            foreach (Composite_Axis axis in axes) axis.ClearBindings();
        }


        public bool GetInput(bool requiresAllBindsDown = false)
        {
            bool result = false;

            foreach (Composite_Axis axis in axes)
            {
                result |= axis.GetInput(requiresAllBindsDown);
            }

            return result;
        }

        public bool GetInputDown(bool requiresAllBindsDown = false)
        {
            bool result = false;

            foreach (Composite_Axis axis in axes)
            {
                result |= axis.GetInputDown(requiresAllBindsDown);
            }

            return result;
        }

        public bool GetInputUp(bool requiresAllBindsDown = false)
        {
            bool result = false;

            foreach (Composite_Axis axis in axes)
            {
                result |= axis.GetInputUp(requiresAllBindsDown);
            }

            return result;
        }

        public float GetAxisValue(int axis, bool requiresAllBindsDown = false)
        {
            if (!axes.IndexIsValid(axis)) return 0f;

            return axes[axis].GetInputValue(requiresAllBindsDown);
        }

        public Vector2 Get2AxisValue(int axis1, int axis2, bool requiresAllBindsDown = false)
        {
            return new Vector2(
                GetAxisValue(axis1, requiresAllBindsDown),
                GetAxisValue(axis2, requiresAllBindsDown)
            );
        }

        public Vector3 Get3AxisValue(int axis1, int axis2, int axis3, bool requiresAllBindsDown = false)
        {
            return new Vector3(
                GetAxisValue(axis1, requiresAllBindsDown),
                GetAxisValue(axis2, requiresAllBindsDown),
                GetAxisValue(axis3, requiresAllBindsDown)
            );
        }
    }

    [Serializable]
    public class Composite_Axis : IInputSet
    {
        public bool enabled
        {
            get => positiveSet.enabled && negativeSet.enabled;

            set
            {
                positiveSet.enabled = value;
                negativeSet.enabled = value;
            }
        }

        public InputSet positiveSet = new InputSet();
        public InputSet negativeSet = new InputSet();


        public void AddBinding(KeyCode bind, bool direction)
        {
            if (direction) positiveSet.AddBinding(bind);
            else negativeSet.AddBinding(bind);
        }
        public void AddBinding(int bind, bool direction)
        {
            if (direction) positiveSet.AddBinding(bind);
            else negativeSet.AddBinding(bind);
        }

        public void RemoveBinding(KeyCode bind, bool direction)
        {
            if (direction) positiveSet.RemoveBinding(bind);
            else negativeSet.RemoveBinding(bind);
        }
        public void RemoveBinding(int bind, bool direction)
        {
            if (direction) positiveSet.RemoveBinding(bind);
            else negativeSet.RemoveBinding(bind);
        }

        public void ClearBindings()
        {
            positiveSet.ClearBindings();
            negativeSet.ClearBindings();
        }


        public bool GetInput(bool requiresAllBindsDown = false) => positiveSet.GetInput(requiresAllBindsDown) || negativeSet.GetInput(requiresAllBindsDown);

        public bool GetInputDown(bool requiresAllBindsDown = false) => positiveSet.GetInputDown(requiresAllBindsDown) || negativeSet.GetInputDown(requiresAllBindsDown);

        public bool GetInputUp(bool requiresAllBindsDown = false) => positiveSet.GetInputUp(requiresAllBindsDown) || negativeSet.GetInputUp(requiresAllBindsDown);

        public float GetInputValue(bool requiresAllBindsDown = false) => positiveSet.GetInputValue(requiresAllBindsDown) - negativeSet.GetInputValue(requiresAllBindsDown);
    }
    #endregion



    [CreateAssetMenu(fileName = "New Input Binding Map", menuName = "SHUU/Input System/Input Binding Map")]
    public class InputBindingMap : ScriptableObject
    {
        public string mapName;

        public bool enabled = true;


        [HideInInspector] public InputBindingMap_Data defaultData = null;



        [Header("Single Input Sets")]
        public List<NAMED_InputSet> inputSets_list = new List<NAMED_InputSet>();
        [HideInInspector] public Dictionary<string, InputSet> inputSets_dict = null;


        [Header("Composite Input Sets")]
        public List<NAMED_Composite_InputSet> compositeSets_list = new List<NAMED_Composite_InputSet>();
        [HideInInspector] public Dictionary<string, Composite_InputSet> compositeSets_dict = null;




        private void OnEnable()
        {
            BuildDictionaries();
        }



        #region Cache Dictionaries
        public void ForceBuildDictionaries()
        {
            inputSets_dict = null;
            compositeSets_dict = null;

            BuildDictionaries();
        }


        public void BuildDictionaries()
        {
            if (inputSets_dict == null)
            {
                inputSets_dict = new Dictionary<string, InputSet>(StringComparer.OrdinalIgnoreCase);

                foreach (NAMED_InputSet namedSet in inputSets_list)
                {
                    if (namedSet == null || string.IsNullOrEmpty(namedSet.name))
                        continue;

                    if (!inputSets_dict.ContainsKey(namedSet.name.Trim()))
                    {
                        inputSets_dict.Add(namedSet.name.Trim(), namedSet.set);
                    }
                    else
                    {
                        Debug.LogError($"Duplicate InputSet name detected in InputBindingMap '{mapName}': '{namedSet.name}'. " +
                                "Only the first entry will be stored.");
                    }
                }
            }

            if (compositeSets_dict == null)
            {
                compositeSets_dict = new Dictionary<string, Composite_InputSet>(StringComparer.OrdinalIgnoreCase);

                foreach (NAMED_Composite_InputSet namedSet in compositeSets_list)
                {
                    if (namedSet == null || string.IsNullOrEmpty(namedSet.name))
                        continue;

                    if (!compositeSets_dict.ContainsKey(namedSet.name.Trim()))
                    {
                        compositeSets_dict.Add(namedSet.name.Trim(), namedSet.set);
                    }
                    else
                    {
                        Debug.LogError($"Duplicate Composite_InputSet name detected in InputBindingMap '{mapName}': '{namedSet.name}'. " +
                                "Only the first entry will be stored.");
                    }
                }
            }
        }
        #endregion



        #region Retrieval
        public bool TryGetSingleSet(string name, out InputSet set)
        {
            set = null;

            if (!enabled) return false;


            set = inputSets_dict.GetValueOrDefault(name, null);


            if (set == null || !set.enabled) return false;

            return true;
        }

        public bool TryGetCompositeSet(string name, out Composite_InputSet set)
        {
            set = null;

            if (!enabled) return false;


            set = compositeSets_dict.GetValueOrDefault(name, null);


            if (set == null || !set.enabled) return false;

            return true;
        }


        public List<NAMED_InputSet> GetAllSingleSets() => inputSets_list;
        public List<NAMED_Composite_InputSet> GetAllCompositeSets() => compositeSets_list;

        public (List<NAMED_InputSet>, List<NAMED_Composite_InputSet>) GetAllSets() => (inputSets_list, compositeSets_list);

        public NAMED_InputSet GetSingleSet(string name)
        {
            foreach (NAMED_InputSet namedSet in inputSets_list)
            {
                if (namedSet.name.ToLower().Equals(name.ToLower())) return namedSet;
            }


            Debug.LogWarning($"No InputSet named '{name}' found in InputBindingMap '{mapName}'.");

            return null;
        }
        public NAMED_Composite_InputSet GetCompositeSet(string name)
        {
            foreach (NAMED_Composite_InputSet namedSet in compositeSets_list)
            {
                if (namedSet.name.ToLower().Equals(name.ToLower())) return namedSet;
            }


            Debug.LogWarning($"No Composite InputSet named '{name}' found in InputBindingMap '{mapName}'.");

            return null;
        }
        #endregion



        #region Defaults
        public void SetDefaultData() => defaultData = ToData();


        public void ResetToDefault()
        {
            if (defaultData == null)
            {
                Debug.LogWarning($"InputBindingMap '{mapName}' has no default data to reset to.");

                return;
            }


            LoadFromData(defaultData);
        }
        #endregion



        #region Saving/Loading
        public InputBindingMap_Data ToData() => new InputBindingMap_Data(this);


        public void LoadFromData(InputBindingMap_Data data)
        {
            this.enabled = data.enabled;

            this.defaultData = new InputBindingMap_Data(data.defaultData);

            this.inputSets_list = new List<NAMED_InputSet>(data.inputSets_list);
            this.compositeSets_list = new List<NAMED_Composite_InputSet>(data.compositeSets_list);


            ForceBuildDictionaries();
        }
        #endregion
    }



    [Serializable]
    public class InputBindingMap_Data
    {
        public string mapName = null;
        public bool enabled = false;

        public InputBindingMap_Data defaultData = null;
    
        public List<NAMED_InputSet> inputSets_list = null;
        public List<NAMED_Composite_InputSet> compositeSets_list = null;



        public InputBindingMap_Data() { }

        public InputBindingMap_Data(InputBindingMap map)
        {
            this.mapName = map.mapName;
            this.enabled = map.enabled;

            this.defaultData = map.defaultData;

            this.inputSets_list = new List<NAMED_InputSet>(map.inputSets_list);
            this.compositeSets_list = new List<NAMED_Composite_InputSet>(map.compositeSets_list);
        }

        public InputBindingMap_Data(InputBindingMap_Data other)
        {
            this.mapName = other.mapName;
            this.enabled = other.enabled;

            this.defaultData = other.defaultData;

            this.inputSets_list = new List<NAMED_InputSet>(other.inputSets_list);
            this.compositeSets_list = new List<NAMED_Composite_InputSet>(other.compositeSets_list);
        }
    }
}
