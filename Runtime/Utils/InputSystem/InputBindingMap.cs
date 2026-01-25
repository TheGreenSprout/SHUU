using System;
using System.Collections.Generic;
using SHUU.Utils.Developer.Debugging;
using SHUU.Utils.Helpers;
using UnityEngine;

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
        public string name = "";

        public InputSet set = new InputSet();


        public NAMED_InputSet() { }
        public NAMED_InputSet(NAMED_InputSet other)
        {
            name = other.name;
            set = new InputSet(other.set);

            overrideAction = other.overrideAction;
        }


        public Action<NAMED_InputSet> overrideAction;
    }

    [Serializable]
    public class InputSet : IInputSet
    {
        public bool enabled = true;

        public List<KeyCode> valid_keyBinds = new List<KeyCode>();
        public List<int> valid_mouseBinds = new List<int>();


        public InputSet() { }
        public InputSet(InputSet other)
        {
            enabled = other.enabled;

            valid_keyBinds = new();
            valid_keyBinds.CopyFrom_List(other.valid_keyBinds);
            valid_mouseBinds = new();
            valid_mouseBinds.CopyFrom_List(other.valid_mouseBinds);
        }


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
            if (!enabled) return false;


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
            if (!enabled) return false;

            
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
            if (!enabled) return false;

            
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
        public string name = "";

        public Composite_InputSet set = new Composite_InputSet();


        public NAMED_Composite_InputSet() { }
        public NAMED_Composite_InputSet(NAMED_Composite_InputSet other)
        {
            name = other.name;
            set = new Composite_InputSet(other.set);

            overrideAction = other.overrideAction;
        }


        public Action<NAMED_Composite_InputSet> overrideAction;
    }

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
                if (value > 4) value = 4;

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


        public Composite_InputSet() { }
        public Composite_InputSet(Composite_InputSet other)
        {
            axes = new();
            axes.CopyFrom_List_CopyContructors(other.axes, x => new Composite_Axis(x));
        }


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
            if (!enabled) return false;

            
            bool result = false;

            foreach (Composite_Axis axis in axes)
            {
                result |= axis.GetInput(requiresAllBindsDown);
            }

            return result;
        }

        public bool GetInputDown(bool requiresAllBindsDown = false)
        {
            if (!enabled) return false;

            
            bool result = false;

            foreach (Composite_Axis axis in axes)
            {
                result |= axis.GetInputDown(requiresAllBindsDown);
            }

            return result;
        }

        public bool GetInputUp(bool requiresAllBindsDown = false)
        {
            if (!enabled) return false;

            
            bool result = false;

            foreach (Composite_Axis axis in axes)
            {
                result |= axis.GetInputUp(requiresAllBindsDown);
            }

            return result;
        }

        public float GetAxisValue(int axis, bool requiresAllBindsDown = false)
        {
            if (!axes.IndexIsValid(axis) || !enabled) return 0f;

            return axes[axis].GetInputValue(requiresAllBindsDown);
        }

        public Vector2 Get2AxisValue(int axis1, int axis2, bool requiresAllBindsDown = false)
        {
            if (!enabled) return Vector2.zero;


            return new Vector2(
                GetAxisValue(axis1, requiresAllBindsDown),
                GetAxisValue(axis2, requiresAllBindsDown)
            );
        }

        public Vector3 Get3AxisValue(int axis1, int axis2, int axis3, bool requiresAllBindsDown = false)
        {
            if (!enabled) return Vector3.zero;

            
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
            get => positiveSet.set.enabled && negativeSet.set.enabled;

            set
            {
                positiveSet.set.enabled = value;
                negativeSet.set.enabled = value;
            }
        }

        public NAMED_InputSet positiveSet = new NAMED_InputSet();
        public NAMED_InputSet negativeSet = new NAMED_InputSet();


        public Composite_Axis()
        {
            if (string.IsNullOrEmpty(positiveSet.name)) positiveSet.name = "Positive";

            if (string.IsNullOrEmpty(negativeSet.name)) negativeSet.name = "Negative";
        }
        public Composite_Axis(Composite_Axis other)
        {
            positiveSet = new NAMED_InputSet(other.positiveSet);
            negativeSet = new NAMED_InputSet(other.negativeSet);
        }


        public void AddBinding(KeyCode bind, bool direction)
        {
            if (direction) positiveSet.set.AddBinding(bind);
            else negativeSet.set.AddBinding(bind);
        }
        public void AddBinding(int bind, bool direction)
        {
            if (direction) positiveSet.set.AddBinding(bind);
            else negativeSet.set.AddBinding(bind);
        }

        public void RemoveBinding(KeyCode bind, bool direction)
        {
            if (direction) positiveSet.set.RemoveBinding(bind);
            else negativeSet.set.RemoveBinding(bind);
        }
        public void RemoveBinding(int bind, bool direction)
        {
            if (direction) positiveSet.set.RemoveBinding(bind);
            else negativeSet.set.RemoveBinding(bind);
        }

        public void ClearBindings()
        {
            positiveSet.set.ClearBindings();
            negativeSet.set.ClearBindings();
        }


        public bool GetInput(bool requiresAllBindsDown = false) => enabled ? positiveSet.set.GetInput(requiresAllBindsDown) || negativeSet.set.GetInput(requiresAllBindsDown) : false;

        public bool GetInputDown(bool requiresAllBindsDown = false) => enabled ? positiveSet.set.GetInputDown(requiresAllBindsDown) || negativeSet.set.GetInputDown(requiresAllBindsDown) : false;

        public bool GetInputUp(bool requiresAllBindsDown = false) => enabled ? positiveSet.set.GetInputUp(requiresAllBindsDown) || negativeSet.set.GetInputUp(requiresAllBindsDown) : false;

        public float GetInputValue(bool requiresAllBindsDown = false) => enabled ? positiveSet.set.GetInputValue(requiresAllBindsDown) - negativeSet.set.GetInputValue(requiresAllBindsDown) : 0f;
    }
    #endregion



    [CreateAssetMenu(fileName = "New Input Binding Map", menuName = "SHUU/Input System/Input Binding Map")]
    public class InputBindingMap : ScriptableObject
    {
        public string mapName;

        public bool enabled = true;


        public string lastDefaultSetDateTime = "No Default Set";
        [SerializeField] private InputBindingMap_Data defaultData = null;



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

        public void OnDisable()
        {
            foreach (var set in inputSets_list)
            {
                set.overrideAction = null;
            }

            foreach (var set in compositeSets_list)
            {
                set.overrideAction = null;
            }
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
                    if (namedSet == null || string.IsNullOrEmpty(namedSet.name)) continue;

                    if (!inputSets_dict.ContainsKey(namedSet.name.Trim())) inputSets_dict.Add(namedSet.name.Trim(), namedSet.set);
                    else Debug.LogError($"Duplicate InputSet name detected in InputBindingMap '{mapName}': '{namedSet.name}'. " + "Only the first entry will be stored.");
                }
            }

            if (compositeSets_dict == null)
            {
                compositeSets_dict = new Dictionary<string, Composite_InputSet>(StringComparer.OrdinalIgnoreCase);

                foreach (NAMED_Composite_InputSet namedSet in compositeSets_list)
                {
                    if (namedSet == null || string.IsNullOrEmpty(namedSet.name)) continue;

                    if (!compositeSets_dict.ContainsKey(namedSet.name.Trim())) compositeSets_dict.Add(namedSet.name.Trim(), namedSet.set);
                    else Debug.LogError($"Duplicate Composite_InputSet name detected in InputBindingMap '{mapName}': '{namedSet.name}'. " + "Only the first entry will be stored.");
                }
            }
        }
        #endregion



        #region Retrieval
        public bool TryGetSingleSet(string name, out InputSet set)
        {
            set = null;


            if (inputSets_dict == null || inputSets_dict.Count == 0) BuildDictionaries();
            if (inputSets_dict.Count == 0) return false;
            

            if (!enabled) return false;


            set = inputSets_dict.GetValueOrDefault(name, null);


            if (set == null || !set.enabled) return false;

            return true;
        }

        public bool TryGetCompositeSet(string name, out Composite_InputSet set)
        {
            set = null;


            if (compositeSets_dict == null || compositeSets_dict.Count == 0) BuildDictionaries();
            if (compositeSets_dict.Count == 0) return false;


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
        public void SetDefaultData()
        {
            lastDefaultSetDateTime = Stats.timestamp;

            defaultData = ToData();
        }


        public void ResetToDefault()
        {
            if (defaultData == null || !defaultData.hasValue)
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
            if (data == null || !data.hasValue)
            {
                Debug.LogWarning($"InputBindingMap '{mapName}' cannot load invalid data.");

                return;
            }


            this.enabled = data.enabled;

            List<NAMED_InputSet> single_copyList = new();
            for (int i = 0; i < data.inputSets_list.Count; i++)
            {
                NAMED_InputSet newSet = new NAMED_InputSet(data.inputSets_list[i]);
                this.inputSets_list[i].overrideAction?.Invoke(newSet);
                
                single_copyList.Add(newSet);
            }
            this.inputSets_list.CopyFrom_List_CopyContructors(single_copyList, x => new NAMED_InputSet(x));
            
            List<NAMED_Composite_InputSet> composite_copyList = new();
            for (int i = 0; i < data.compositeSets_list.Count; i++)
            {
                NAMED_Composite_InputSet newSet = new NAMED_Composite_InputSet(data.compositeSets_list[i]);
                this.compositeSets_list[i].overrideAction?.Invoke(newSet);
                
                composite_copyList.Add(newSet);
            }
            this.compositeSets_list.CopyFrom_List_CopyContructors(composite_copyList, x => new NAMED_Composite_InputSet(x));

            ForceBuildDictionaries();
        }
        #endregion
    }



    [Serializable]
    public class InputBindingMap_Data
    {
        public bool hasValue = false;

        public string mapName = null;
        public bool enabled = false;

        public List<NAMED_InputSet> inputSets_list = null;
        public List<NAMED_Composite_InputSet> compositeSets_list = null;



        public InputBindingMap_Data(InputBindingMap map)
        {
            hasValue = true;

            this.mapName = map.mapName;
            this.enabled = map.enabled;

            this.inputSets_list = new List<NAMED_InputSet>();
            foreach (var set in map.inputSets_list)
            {
                this.inputSets_list.Add(new NAMED_InputSet(set));
            }
            this.compositeSets_list = new List<NAMED_Composite_InputSet>();
            foreach (var set in map.compositeSets_list)
            {
                this.compositeSets_list.Add(new NAMED_Composite_InputSet(set));
            }
        }

        public InputBindingMap_Data(InputBindingMap_Data other)
        {
            hasValue = true;

            this.mapName = other.mapName;
            this.enabled = other.enabled;

            this.inputSets_list = new List<NAMED_InputSet>();
            foreach (var set in other.inputSets_list)
            {
                this.inputSets_list.Add(new NAMED_InputSet(set));
            }
            this.compositeSets_list = new List<NAMED_Composite_InputSet>();
            foreach (var set in other.compositeSets_list)
            {
                this.compositeSets_list.Add(new NAMED_Composite_InputSet(set));
            }
        }
    }
}
