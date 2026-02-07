using System;
using SHUU.Utils.SettingsSytem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


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
#endregion


public class SettingsMenu_Field : MonoBehaviour
{
    [SerializeField] private TMP_Text label;


    [SerializeField] private SettingsMenu_BoolElement boolElement;
    [SerializeField] private SettingsMenu_NumElement numElement_Value;
    [SerializeField] private SettingsMenu_SliderElement numElement_Slider;
    [SerializeField] private SettingsMenu_StringElement stringElement;



    private SettingsData data;

    private string field;


    private SettingsMenu_IFieldModule module = null;

    


    public void Init(SettingsData data, string field)
    {
        this.data = data;
        this.field = field;

        label.text = field;


        switch (data.GetSettingField(field).type)
        {
            case SettingType.Bool:
                module = new SettingsMenu_BoolModule(boolElement.root, boolElement.label, data.GetSettingField(field));
                break;

            case SettingType.Int:
                NumField(typeof(int));
                break;

            case SettingType.Float:
                NumField(typeof(float));
                break;

            case SettingType.String:
                module = new SettingsMenu_StringModule(stringElement.root, stringElement.input, data.GetSettingField(field));
                break;
        }
    }

    private void NumField(Type type)
    {
        SettingField f = data.GetSettingField(field);

        if (f == null) return;

        
        if (f.useMax && f.useMin) module = new SettingsMenu_SliderModule(numElement_Slider.root, numElement_Slider.label, numElement_Slider.slider, f, type == typeof(int) ? NumType.Int : NumType.Float);
        else module = new SettingsMenu_NumberModule(numElement_Value.root, numElement_Value.input, f, type == typeof(int) ? NumType.Int : NumType.Float);
    }


    public void OnValueChanged() => module?.Fetch(data.GetSettingField(field), true);
    public void Exceptional_OnValueChanged() => module?.Fetch(data.GetSettingField(field), false);

    public void Increment(int increment)
    {
        if (module == null) return;

        if (module is SettingsMenu_NumberModule number) number.Increment(data.GetSettingField(field), increment);
    }

    public void Refresh() => module?.Refresh(data.GetSettingField(field));
}



#region Modules
public abstract class SettingsMenu_IFieldModule
{
    public abstract void Refresh(SettingField field);

    public abstract void Fetch(SettingField field, bool check);
}

public enum NumType
{
    Int,
    Float
}


public class SettingsMenu_BoolModule : SettingsMenu_IFieldModule
{
    private TMP_Text label;


    public SettingsMenu_BoolModule(GameObject obj, TMP_Text label, SettingField field)
    {
        obj.SetActive(true);

        this.label = label;

        Refresh(field);
    }


    public override void Refresh(SettingField field)
    {
        label.text = field.boolValue ? "True" : "False";
    }

    public override void Fetch(SettingField field, bool check = true)
    {
        if (field == null || field.Type() != typeof(bool)) return;

        field.boolValue = !field.boolValue;

        Refresh(field);
    }
}

public class SettingsMenu_StringModule : SettingsMenu_IFieldModule
{
    private TMP_InputField input;


    public SettingsMenu_StringModule(GameObject obj, TMP_InputField input, SettingField field)
    {
        obj.SetActive(true);

        this.input = input;

        Refresh(field);
    }


    public override void Refresh(SettingField field)
    {
        input.text = field.stringValue;
    }

    public override void Fetch(SettingField field, bool check = true)
    {
        if (field == null || field.Type() != typeof(string)) return;

        string parse = input.text;
        field.stringValue = string.IsNullOrEmpty(parse) ? field.stringValue : parse;

        Refresh(field);
    }
}

public class SettingsMenu_NumberModule : SettingsMenu_IFieldModule
{
    private TMP_InputField input;

    private NumType numType;


    public SettingsMenu_NumberModule(GameObject obj, TMP_InputField input, SettingField field, NumType numType)
    {
        obj.SetActive(true);

        this.input = input;
        this.numType = numType;

        Refresh(field);
    }


    public override void Refresh(SettingField field)
    {
        if (numType == NumType.Int) input.text = field.intValue.ToString();
        else if (numType == NumType.Float) input.text = field.floatValue.ToString("F2");
    }

    public override void Fetch(SettingField field, bool check = true)
    {
        if (field == null) return;

        if (numType == NumType.Int)
        {
            if (field.Type() != typeof(int)) return;

            if (!int.TryParse(input.text, out int parseInt))
            {
                input.text = field.intValue.ToString();
                
                return;
            }


            if (field.useMax && parseInt > field.intMax) parseInt = field.intMax;
            if (field.useMin && parseInt < field.intMin) parseInt = field.intMin;

            field.intValue = parseInt;
        }
        else
        {
            if (field.Type() != typeof(float)) return;

            if (!float.TryParse(input.text, out float parseFloat))
            {
                input.text = field.floatValue.ToString();
                
                return;
            }


            if (field.useMax && parseFloat > field.floatMax) parseFloat = field.floatMax;
            if (field.useMin && parseFloat < field.floatMin) parseFloat = field.floatMin;
            
            field.floatValue = parseFloat;
        }
        

        Refresh(field);
    }

    public void Increment(SettingField field, int increment)
    {
        if (increment == 0) return;
        
        if (numType == NumType.Int) input.text = (field.intValue + increment).ToString();
        else if (numType == NumType.Float) input.text = (field.floatValue + increment).ToString();
        

        Fetch(field);
    }
}


public class SettingsMenu_SliderModule : SettingsMenu_IFieldModule
{
    private TMP_InputField value;
    private Slider slider;

    private NumType numType;


    public SettingsMenu_SliderModule(GameObject obj, TMP_InputField value, Slider slider, SettingField field, NumType numType)
    {
        obj.SetActive(true);

        this.slider = slider;
        this.value = value;
        this.numType = numType;

        if (numType == NumType.Int) this.slider.wholeNumbers = true;
        else this.slider.wholeNumbers = false;

        
        if (numType == NumType.Int)
        {
            this.slider.maxValue = field.intMax;
            this.slider.minValue = field.intMin;
        }
        else if (numType == NumType.Float)
        {
            this.slider.maxValue = field.floatMax;
            this.slider.minValue = field.floatMin;
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
        else if (numType == NumType.Float)
        {
            value.text = field.floatValue.ToString("F2");
            if (slider.value != field.floatValue) slider.value = field.floatValue;
        }
    }

    public override void Fetch(SettingField field, bool fromSlider)
    {
        if (field == null) return;

        if (numType == NumType.Int)
        {
            if (field.Type() != typeof(int)) return;

            int parseInt;
            if (fromSlider)
            {
                parseInt = (int)slider.value;
                if (parseInt == field.intValue) return;
            }
            else if (!int.TryParse(value.text, out parseInt))
            {
                value.text = slider.value.ToString();

                return;
            }


            if (field.useMax && parseInt > field.intMax) parseInt = field.intMax;
            if (field.useMin && parseInt < field.intMin) parseInt = field.intMin;

            field.intValue = parseInt;
        }
        else
        {
            if (field.Type() != typeof(float)) return;


            float parseFloat;
            if (fromSlider)
            {
                parseFloat = slider.value;
                if (parseFloat == field.floatValue) return;
            }
            else if (!float.TryParse(value.text, out parseFloat))
            {
                value.text = slider.value.ToString();
                
                return;
            }


            if (field.useMax && parseFloat > field.floatMax) parseFloat = field.floatMax;
            if (field.useMin && parseFloat < field.floatMin) parseFloat = field.floatMin;
            
            field.floatValue = parseFloat;
        }
        

        Refresh(field);
    }
}
#endregion
