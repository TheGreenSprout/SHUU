using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using SHUU.Utils.SettingsSystem;

public class SettingsMenu_Field : MonoBehaviour
{
    #region Variables
    [SerializeField] private TMP_Text label;


    [SerializeField] private SettingsMenu_BoolElement    boolElement;
    [SerializeField] private SettingsMenu_NumElement     numElement_Value;
    [SerializeField] private SettingsMenu_SliderElement  numElement_Slider;
    [SerializeField] private SettingsMenu_StringElement  stringElement;
    [SerializeField] private SettingsMenu_EnumElement    enumElement;



    private SettingsAtlas data;
    private string mapName;
    private string fieldKey;

    private SettingsMenu_IFieldModule module = null;
    #endregion




    #region Main
    public void Init(SettingsAtlas data, string mapName, string fieldKey)
    {
        this.data     = data;
        this.mapName  = mapName;
        this.fieldKey = fieldKey;

        label.text = fieldKey;

        var field = data.GetField(mapName, fieldKey);
        if (field == null) return;

        Action notify = () => data.NotifyChanged(mapName, fieldKey);

        switch (field.type)
        {
            case SettingType.Bool:
                module = new SettingsMenu_BoolModule(boolElement.root, boolElement.label, field, notify);
                break;

            case SettingType.Int:
                NumField(field, NumType.Int, notify);
                break;

            case SettingType.Float:
                NumField(field, NumType.Float, notify);
                break;

            case SettingType.String:
                module = new SettingsMenu_StringModule(stringElement.root, stringElement.input, field, notify);
                break;

            case SettingType.Enum:
                module = new SettingsMenu_EnumModule(enumElement.root, enumElement.dropdown, field, notify);
                break;
        }
    }
    #endregion




    #region Logic
    private void NumField(SettingField field, NumType type, Action notify)
    {
        if (field.useMin && field.useMax)
            module = new SettingsMenu_SliderModule(numElement_Slider.root, numElement_Slider.label, numElement_Slider.slider, field, type, notify);
        else
            module = new SettingsMenu_NumberModule(numElement_Value.root, numElement_Value.input, field, type, notify);
    }


    // Called by UI events (button onClick, input onValueChanged, slider onValueChanged)
    public void OnValueChanged()            => module?.Fetch(Field(), true);
    public void Exceptional_OnValueChanged() => module?.Fetch(Field(), false);

    public void Increment(int increment)
    {
        if (module is SettingsMenu_NumberModule number) number.Increment(Field(), increment);
    }

    public void Refresh() => module?.Refresh(Field());


    private SettingField Field() => data.GetField(mapName, fieldKey);
    #endregion
}




#region Modules
public abstract class SettingsMenu_IFieldModule
{
    public abstract void Refresh(SettingField field);
    public abstract void Fetch(SettingField field, bool check);
}

public enum NumType { Int, Float }


public class SettingsMenu_BoolModule : SettingsMenu_IFieldModule
{
    private TMP_Text label;
    private Action notify;


    public SettingsMenu_BoolModule(GameObject obj, TMP_Text label, SettingField field, Action notify)
    {
        obj.SetActive(true);
        this.label  = label;
        this.notify = notify;
        Refresh(field);
    }


    public override void Refresh(SettingField field) => label.text = field.boolValue ? "True" : "False";

    public override void Fetch(SettingField field, bool check)
    {
        if (field == null || field.type != SettingType.Bool) return;
        field.boolValue = !field.boolValue;
        notify?.Invoke();
        Refresh(field);
    }
}


public class SettingsMenu_StringModule : SettingsMenu_IFieldModule
{
    private TMP_InputField input;
    private Action notify;


    public SettingsMenu_StringModule(GameObject obj, TMP_InputField input, SettingField field, Action notify)
    {
        obj.SetActive(true);
        this.input  = input;
        this.notify = notify;
        Refresh(field);
    }


    public override void Refresh(SettingField field) => input.text = field.stringValue;

    public override void Fetch(SettingField field, bool check)
    {
        if (field == null || field.type != SettingType.String) return;
        string parsed = input.text;
        field.stringValue = string.IsNullOrEmpty(parsed) ? field.stringValue : parsed;
        notify?.Invoke();
        Refresh(field);
    }
}


public class SettingsMenu_NumberModule : SettingsMenu_IFieldModule
{
    private TMP_InputField input;
    private NumType numType;
    private Action notify;


    public SettingsMenu_NumberModule(GameObject obj, TMP_InputField input, SettingField field, NumType numType, Action notify)
    {
        obj.SetActive(true);
        this.input   = input;
        this.numType = numType;
        this.notify  = notify;
        Refresh(field);
    }


    public override void Refresh(SettingField field)
    {
        if (numType == NumType.Int)   input.text = field.intValue.ToString();
        else                          input.text = field.floatValue.ToString("F2");
    }

    public override void Fetch(SettingField field, bool check)
    {
        if (field == null) return;

        if (numType == NumType.Int)
        {
            if (field.type != SettingType.Int) return;
            if (!int.TryParse(input.text, out int i)) { input.text = field.intValue.ToString(); return; }
            if (field.useMax) i = Mathf.Min(i, field.intMax);
            if (field.useMin) i = Mathf.Max(i, field.intMin);
            field.intValue = i;
        }
        else
        {
            if (field.type != SettingType.Float) return;
            if (!float.TryParse(input.text, out float f)) { input.text = field.floatValue.ToString(); return; }
            if (field.useMax) f = Mathf.Min(f, field.floatMax);
            if (field.useMin) f = Mathf.Max(f, field.floatMin);
            field.floatValue = f;
        }

        notify?.Invoke();
        Refresh(field);
    }

    public void Increment(SettingField field, int increment)
    {
        if (increment == 0) return;
        input.text = numType == NumType.Int
            ? (field.intValue   + increment).ToString()
            : (field.floatValue + increment).ToString();
        Fetch(field, false);
    }
}


public class SettingsMenu_SliderModule : SettingsMenu_IFieldModule
{
    private TMP_InputField value;
    private Slider slider;
    private NumType numType;
    private Action notify;


    public SettingsMenu_SliderModule(GameObject obj, TMP_InputField value, Slider slider, SettingField field, NumType numType, Action notify)
    {
        obj.SetActive(true);
        this.value   = value;
        this.slider  = slider;
        this.numType = numType;
        this.notify  = notify;

        slider.wholeNumbers = numType == NumType.Int;

        if (numType == NumType.Int)
        {
            slider.minValue = field.intMin;
            slider.maxValue = field.intMax;
        }
        else
        {
            slider.minValue = field.floatMin;
            slider.maxValue = field.floatMax;
        }

        Refresh(field);
    }


    public override void Refresh(SettingField field)
    {
        if (numType == NumType.Int)
        {
            value.text = field.intValue.ToString();
            if (slider.value != field.intValue) slider.value = field.intValue;
        }
        else
        {
            value.text = field.floatValue.ToString("F2");
            if (slider.value != field.floatValue) slider.value = field.floatValue;
        }
    }

    // fromSlider = true: driven by slider; false: driven by input field
    public override void Fetch(SettingField field, bool fromSlider)
    {
        if (field == null) return;

        if (numType == NumType.Int)
        {
            if (field.type != SettingType.Int) return;
            int i;
            if (fromSlider)
            {
                i = (int)slider.value;
                if (i == field.intValue) return;
            }
            else if (!int.TryParse(value.text, out i)) { value.text = field.intValue.ToString(); return; }
            if (field.useMax) i = Mathf.Min(i, field.intMax);
            if (field.useMin) i = Mathf.Max(i, field.intMin);
            field.intValue = i;
        }
        else
        {
            if (field.type != SettingType.Float) return;
            float f;
            if (fromSlider)
            {
                f = slider.value;
                if (f == field.floatValue) return;
            }
            else if (!float.TryParse(value.text, out f)) { value.text = field.floatValue.ToString(); return; }
            if (field.useMax) f = Mathf.Min(f, field.floatMax);
            if (field.useMin) f = Mathf.Max(f, field.floatMin);
            field.floatValue = f;
        }

        notify?.Invoke();
        Refresh(field);
    }
}


public class SettingsMenu_EnumModule : SettingsMenu_IFieldModule
{
    private TMP_Dropdown dropdown;
    private Action notify;


    public SettingsMenu_EnumModule(GameObject obj, TMP_Dropdown dropdown, SettingField field, Action notify)
    {
        obj.SetActive(true);
        this.dropdown = dropdown;
        this.notify   = notify;

        var enumType = field.Type();
        if (enumType != null && enumType.IsEnum)
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(new List<string>(Enum.GetNames(enumType)));
        }

        Refresh(field);
    }


    public override void Refresh(SettingField field)
    {
        if (dropdown.value != field.enumValue) dropdown.value = field.enumValue;
    }

    public override void Fetch(SettingField field, bool check)
    {
        if (field == null || field.type != SettingType.Enum) return;
        field.enumValue = dropdown.value;
        notify?.Invoke();
        Refresh(field);
    }
}
#endregion




#region Exposed elements
[Serializable]
public struct SettingsMenu_BoolElement
{
    public GameObject root;
    public TMP_Text label;
}

[Serializable]
public struct SettingsMenu_NumElement
{
    public GameObject root;
    public TMP_InputField input;
}

[Serializable]
public struct SettingsMenu_SliderElement
{
    public GameObject root;
    public TMP_InputField label;
    public Slider slider;
}

[Serializable]
public struct SettingsMenu_StringElement
{
    public GameObject root;
    public TMP_InputField input;
}

[Serializable]
public struct SettingsMenu_EnumElement
{
    public GameObject root;
    public TMP_Dropdown dropdown;
}
#endregion
