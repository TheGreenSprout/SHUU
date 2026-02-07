using System;

namespace SHUU.Utils.SettingsSytem
{
    public enum SettingType
    {
        Bool,
        Int,
        Float,
        String
    }


    [Serializable]
    public class SettingField
    {
        public string key;

        public SettingType type;

        public bool boolValue;
        public int intValue;
        public float floatValue;
        public string stringValue;

        public bool useMin;
        public bool useMax;

        public int intMin;
        public int intMax;

        public float floatMin;
        public float floatMax;



        public SettingField() { }

        public SettingField(SettingField other)
        {
            key = other.key;
            type = other.type;

            boolValue = other.boolValue;
            intValue = other.intValue;
            floatValue = other.floatValue;
            stringValue = other.stringValue;

            useMin = other.useMin;
            useMax = other.useMax;

            intMin = other.intMin;
            intMax = other.intMax;

            floatMin = other.floatMin;
            floatMax = other.floatMax;
        }

        public Type Type() => type switch
                            {
                                SettingType.Bool => typeof(bool),
                                SettingType.Int => typeof(int),
                                SettingType.Float => typeof(float),
                                SettingType.String => typeof(string),

                                _ => null
                            };
    }
}
