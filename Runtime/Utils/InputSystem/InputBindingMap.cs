using System;
using System.Collections.Generic;
using UnityEngine;

//! Requires a way to retrieve all the bindings from the map, so they can be for example displayed on an options menu.

namespace SHUU.Utils.InputSystem
{
    #region Set Classes
    [Serializable]
    public class NAMED_InputSet
    {
        public string name;

        public InputSet set = new InputSet();
    }

    [Serializable]
    public class InputSet
    {
        public List<KeyCode> valid_keyBinds = new List<KeyCode>();
        public List<int> valid_mouseBinds = new List<int>();


        public void AddBinding(SHUU_Input.DynamicInput bind)
        {
            if (!bind.IsValid()) return;


            if (bind.TryGetKey(out KeyCode key))
            {
                AddBinding(key);
            }
            else if (bind.TryGetMouse(out int mouse))
            {
                AddBinding(mouse);
            }
            else Debug.LogError("Invalid Dynamic Input on AddBind().");
        }
        public void AddBinding(KeyCode bind) { if (!valid_keyBinds.Contains(bind)) valid_keyBinds.Add(bind); }
        public void AddBinding(int bind) { if (!valid_mouseBinds.Contains(bind)) valid_mouseBinds.Add(bind); }

        public void RemoveBinding(SHUU_Input.DynamicInput bind)
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
    }


    [Serializable]
    public class NAMED_Composite_InputSet
    {
        public string name;

        public Composite_InputSet set = new Composite_InputSet();
    }

    [Serializable]
    public class Composite_InputSet
    {
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
                parts[index].AddBinding(key);
            }
            else if (bind.TryGetMouse(out int mouse))
            {
                parts[index].AddBinding(mouse);
            }
            else Debug.LogError("Invalid Dynamic Input on Composite AddBind().");
        }
        public void AddBinding(KeyCode bind, int index) { parts[index].AddBinding(bind); }
        public void AddBinding(int bind, int index) { parts[index].AddBinding(bind); }

        public void RemoveBinding(SHUU_Input.DynamicInput bind, int index)
        {
            if (!bind.IsValid()) return;

            
            if (bind.TryGetKey(out KeyCode key))
            {
                parts[index].RemoveBinding(key);
            }
            else if (bind.TryGetMouse(out int mouse))
            {
                parts[index].RemoveBinding(mouse);
            }
            else Debug.LogError("Invalid Dynamic Input on Composite RemoveBind().");
        }
        public void RemoveBinding(KeyCode bind, int index) { parts[index].RemoveBinding(bind); }
        public void RemoveBinding(int bind, int index) { parts[index].RemoveBinding(bind); }

        public void ClearBindings()
        {
            foreach (InputSet set in parts)
            {
                set.ClearBindings();
            }
        }
    }
    #endregion



    [CreateAssetMenu(fileName = "New Input Binding Map", menuName = "SHUU/Input System/Input Binding Map")]
    public class InputBindingMap : ScriptableObject
    {
        public string mapName;


        public bool enabled = true;



        [Header("Single Input Sets")]
        public List<NAMED_InputSet> inputSets_list = new List<NAMED_InputSet>();


        [Header("Composite Input Sets")]
        public List<NAMED_Composite_InputSet> compositeSets_list = new List<NAMED_Composite_InputSet>();



        
        #region Retrieval
        public bool TryGetSingleSet(string name, out InputSet set)
        {
            set = null;

            if (!enabled) return false;

            foreach (NAMED_InputSet namedSet in inputSets_list)
            {
                if (namedSet.name.ToLower() == name.ToLower())
                {
                    set = namedSet.set;

                    return true;
                }
            }

            return false;
        }

        public bool TryGetCompositeSet(string name, out Composite_InputSet set)
        {
            set = null;

            if (!enabled) return false;

            foreach (NAMED_Composite_InputSet namedSet in compositeSets_list)
            {
                if (namedSet.name.ToLower() == name.ToLower())
                {
                    set = namedSet.set;

                    return true;
                }
            }

            return false;
        }
        #endregion



        #region Saving/Loading
        public InputBindingMap_Data ToData()
        {
            return new InputBindingMap_Data
            {
                mapName = this.mapName,
                enabled = this.enabled,
                inputSets_list = this.inputSets_list,
                compositeSets_list = this.compositeSets_list
            };
        }


        public void LoadFromData(InputBindingMap_Data data)
        {
            this.enabled = data.enabled;
            this.inputSets_list = data.inputSets_list;
            this.compositeSets_list = data.compositeSets_list;
        }
        #endregion
    }

    [Serializable]
    public class InputBindingMap_Data
    {
        public string mapName;
        public bool enabled;

        public List<NAMED_InputSet> inputSets_list;
        public List<NAMED_Composite_InputSet> compositeSets_list;
    }
}
