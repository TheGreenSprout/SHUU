using System;
using System.Collections.Generic;
using System.Linq;
using SHUU.Utils.Helpers;
using UnityEngine;

namespace SHUU.Utils.InputSystem
{
    #region Base Data
    [Serializable]
    public enum InputDeviceType
    {
        General,
        KeyboardMouse,
        Gamepad
    }

    [Serializable]
    public enum InputSourceType
    {
        Key,
        Mouse,
        Axis
    }
    
    
    [Serializable]
    public abstract class InputSource
    {
        public abstract InputSourceType SourceType();
        public abstract InputDeviceType DeviceType();

        public abstract bool Get();
        public abstract bool GetDown();
        public abstract bool GetUp();
        public abstract float GetValue();

        public abstract string String();
    }


    
    [Serializable]
    public class KeySource : InputSource
    {
        public KeyCode key = default;

        public override InputSourceType SourceType() => InputSourceType.Key;
        private InputDeviceType deviceType;
        public override InputDeviceType DeviceType() => deviceType;


        public KeySource() { }
        public KeySource(KeyCode k)
        {
            key = k;

            deviceType = key.IsGamepadKey() ? InputDeviceType.Gamepad : InputDeviceType.KeyboardMouse;
        }
        public KeySource(KeySource other)
        {
            key = other.key;

            deviceType = other.DeviceType();
        }

        public override bool Get() => Input.GetKey(key);
        public override bool GetDown() => Input.GetKeyDown(key);
        public override bool GetUp() => Input.GetKeyUp(key);
        public override float GetValue() => Input.GetKey(key) ? 1f : 0f;

        public override string String() => InputParser.InputToString(key);
    }


    [Serializable]
    public class MouseSource : InputSource
    {
        public int mouse = -1;

        public override InputSourceType SourceType() => InputSourceType.Mouse;
        public override InputDeviceType DeviceType() => InputDeviceType.KeyboardMouse;


        public MouseSource() { }
        public MouseSource(int m) => mouse = m;
        public MouseSource(MouseSource other) => mouse = other.mouse;

        public override bool Get() => Input.GetMouseButton(mouse);
        public override bool GetDown() => Input.GetMouseButtonDown(mouse);
        public override bool GetUp() => Input.GetMouseButtonUp(mouse);
        public override float GetValue() => Input.GetMouseButton(mouse) ? 1f : 0f;

        public override string String() => InputParser.InputToString(mouse);
    }


    [Serializable]
    public class AxisSource : InputSource
    {
        public string axisName = null;
        public bool raw = false;

        public float threshold = 0.5f;
        private float previousValue = 0f;
        private float currentValue = 0f;

        public override InputSourceType SourceType() => InputSourceType.Axis;
        private InputDeviceType deviceType;
        public override InputDeviceType DeviceType() => deviceType;


        public AxisSource() { }
        public AxisSource(string axisName, InputDeviceType deviceType, float threshold = 0.5f, bool raw = true)
        {
            this.axisName = axisName;
            this.raw = raw;

            this.deviceType = deviceType;

            this.threshold = threshold;
        }
        public AxisSource(InputParser.AxisNames axisName, float threshold = 0.5f, bool raw = true)
        {
            (string, InputDeviceType?) data = InputParser.GetEnumNameAxisData(InputParser.GetAxis_WithEnumName(axisName));
            if (string.IsNullOrEmpty(data.Item1) || data.Item2 == null) return;
            string parse = data.Item1;
            if (string.IsNullOrEmpty(parse))
            {
                Debug.LogError("AxisSource creation with InputParser.AxisNames enum went wrong.");
                
                this.axisName = null;
            }
            else this.axisName = InputParser.GetAxis(axisName);

            this.raw = raw;

            this.deviceType = data.Item2.Value;

            this.threshold = threshold;
        }
        public AxisSource(AxisSource other)
        {
            axisName = other.axisName;
            raw = other.raw;

            deviceType = other.DeviceType();

            threshold = other.threshold;
        }

        public void Tick()
        {
            previousValue = currentValue;
            currentValue = GetValue();
        }

        public override bool Get() => GetValue() >= threshold ? true : false;
        public override bool GetDown() => previousValue < threshold && currentValue >= threshold;
        public override bool GetUp() => previousValue >= threshold && currentValue < threshold;
        public override float GetValue() => raw ? Input.GetAxisRaw(axisName) : Input.GetAxis(axisName);

        public override string String() => InputParser.InputToString(axisName);
    }
    #endregion



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

        [SerializeReference] public List<InputSource> validSources = new();


        public InputSet() { }
        public InputSet(InputSet other)
        {
            enabled = other.enabled;

            foreach (var source in other.validSources)
            {
                if (source is KeySource key) validSources.Add(new KeySource(key));
                else if (source is MouseSource mouse) validSources.Add(new MouseSource(mouse));
                else if (source is AxisSource axis) validSources.Add(new AxisSource(axis));
            }
        }


        private bool ContainsBind(DynamicInput bind)
        {
            if (bind.TryGetKey(out KeyCode key)) return ContainsBind(key);
            else if (bind.TryGetMouse(out int mouse)) return ContainsBind(mouse);
            else if (bind.TryGetAxis(out string axis)) return ContainsBind(axis);


            Debug.LogError("Invalid Dynamic Input on AddBind().");

            return true;
        }
        private bool ContainsBind(KeyCode key)
        {
            foreach (var source in validSources)
            {
                if (source is KeySource s && s.key == key) return true;
            }

            return false;
        }
        private bool ContainsBind(int mouse)
        {
            foreach (var source in validSources)
            {
                if (source is MouseSource s && s.mouse == mouse) return true;
            }

            return false;
        }
        private bool ContainsBind(string axisName)
        {
            foreach (var source in validSources)
            {
                if (source is AxisSource s && s.axisName == axisName) return true;
            }

            return false;
        }

        public void AddBinding(DynamicInput bind)
        {
            if (!bind.IsValid()) return;


            if (bind.TryGetKey(out KeyCode key)) AddBinding(key);
            else if (bind.TryGetMouse(out int mouse)) AddBinding(mouse);
            else if (bind.TryGetAxis(out string axis)) AddBinding(axis);
            else Debug.LogError("Invalid Dynamic Input on AddBind().");
        }
        public void AddBinding(KeyCode bind) {
            if (ContainsBind(bind))
            {
                Debug.LogWarning("Repeated Key Binding on AddBind().");

                return;
            }

            validSources.Add(new KeySource(bind));
        }
        public void AddBinding(int bind) {
            if (ContainsBind(bind))
            {
                Debug.LogWarning("Repeated Mouse Binding on AddBind().");

                return;
            }

            validSources.Add(new MouseSource(bind));
        }
        public void AddBinding(string bind) {
            if (ContainsBind(bind))
            {
                Debug.LogWarning("Repeated Axis Binding on AddBind().");

                return;
            }

            (string, InputDeviceType?) data = InputParser.GetEnumNameAxisData(bind);
            if (data == (null, null) || data.Item1 == null || data.Item2 == null)
            {
                Debug.LogWarning("Invalid Axis Binding on AddBind().");

                return;
            }

            validSources.Add(new AxisSource(data.Item1, data.Item2.Value));
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
            else if (bind.TryGetAxis(out string axis))
            {
                RemoveBinding(axis);
            }
            else Debug.LogError("Invalid Dynamic Input on RemoveBind().");
        }
        public void RemoveBinding(KeyCode bind) => validSources = validSources.Where(x => !(x is KeySource s && s.key == bind)).ToList();
        public void RemoveBinding(int bind) => validSources = validSources.Where(x => !(x is MouseSource s && s.mouse == bind)).ToList();
        public void RemoveBinding(string bind) => validSources = validSources.Where(x => !(x is AxisSource s && s.axisName == bind)).ToList();

        public void ClearBindings() => validSources.Clear();


        public bool GetInput(bool requiresAllBindsDown = false)
        {
            if (!enabled || validSources.Count == 0) return false;


            bool result = requiresAllBindsDown;

            foreach (var input in validSources)
            {
                if (!requiresAllBindsDown) result |= input.Get();
                else result &= input.Get();
            }

            return result;
        }

        public bool GetInputDown(bool requiresAllBindsDown = false)
        {
            if (!enabled || validSources.Count == 0) return false;

            
            bool result = false;

            foreach (var input in validSources)
            {
                result |= input.GetDown();
            }


            if (requiresAllBindsDown) result &= GetInput(true);

            return result;
        }
        public bool GetInputUp(bool requiresAllBindsDown = false)
        {
            if (!enabled || validSources.Count == 0) return false;

            
            bool result = false;

            foreach (var input in validSources)
            {
                result |= input.GetUp();
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
            if (!bind.IsValid() || !axes.IndexIsValid(index) || bind.IsAxis()) return;

            
            if (bind.TryGetKey(out KeyCode key))
            {
                AddBinding(key, index, bind.direction);
            }
            else if (bind.TryGetMouse(out int mouse))
            {
                AddBinding(mouse, index, bind.direction);
            }
            else Debug.LogError("Invalid Dynamic Input on Composite AddBinding().");
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
            else Debug.LogError("Invalid Dynamic Input on Composite RemoveBinding().");
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



    [Serializable]
    public class InputBindingMap_Data
    {
        public bool hasValue = false;


        public List<NAMED_InputSet> inputSets_list = null;
        public List<NAMED_Composite_InputSet> compositeSets_list = null;



        public InputBindingMap_Data() { }
        
        public InputBindingMap_Data(InputBindingMap map)
        {
            hasValue = true;

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
