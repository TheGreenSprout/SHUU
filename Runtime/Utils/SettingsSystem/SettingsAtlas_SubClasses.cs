using System;
using System.Collections.Generic;

namespace SHUU.Utils.SettingsSystem
{
    #region Enums
    public enum SettingType
    {
        Bool,
        Int,
        Float,
        String,
        Enum
    }
    #endregion




    #region SettingField
    [Serializable]
    public class SettingField
    {
        #region Variables
        public string key;


        public SettingType type;

        public bool boolValue;
        public int intValue;
        public float floatValue;
        public string stringValue;

        public string enumTypeName;
        public int enumValue;


        public bool useMin;
        public bool useMax;

        public int intMin;
        public int intMax;
        public float floatMin;
        public float floatMax;
        #endregion



        #region Main
        public SettingField() { }

        public SettingField(SettingField other)
        {
            key = other.key;


            type = other.type;

            boolValue = other.boolValue;
            intValue = other.intValue;
            floatValue = other.floatValue;
            stringValue = other.stringValue;

            enumTypeName = other.enumTypeName;
            enumValue = other.enumValue;


            useMin = other.useMin;
            useMax = other.useMax;

            intMin = other.intMin;
            intMax = other.intMax;
            floatMin = other.floatMin;
            floatMax = other.floatMax;
        }
        #endregion


        #region Logic
        public Type Type()
        {
            return type switch
            {
                SettingType.Bool => typeof(bool),
                SettingType.Int => typeof(int),
                SettingType.Float => typeof(float),
                SettingType.String => typeof(string),
                SettingType.Enum => ResolveEnumType(),

                _ => null
            };
        }

        private Type ResolveEnumType()
        {
            if (string.IsNullOrWhiteSpace(enumTypeName)) return null;

            var type = System.Type.GetType(enumTypeName);

            if (type != null) return type;

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(enumTypeName);

                if (type != null) return type;
            }

            return null;
        }
        #endregion
    }
    #endregion



    #region Containers

    #region SettingFieldsContainer
    [Serializable]
    public abstract class SettingFieldsContainer
    {
        #region Variables
        public List<SettingField> fields = new();
        #endregion



        #region Setting Field access
        public virtual SettingField GetSettingField(string key) => fields.Find(x => x.key == key);

        public Type GetSettingFieldType(string key) => GetSettingField(key)?.Type();
        #endregion


        #region Getters
        public virtual bool GetBool (string key) => GetSettingField(key)?.boolValue ?? false;
        public virtual int GetInt (string key) => GetSettingField(key)?.intValue ?? 0;
        public virtual float  GetFloat (string key) => GetSettingField(key)?.floatValue ?? 0f;
        public virtual string GetString(string key) => GetSettingField(key)?.stringValue ?? "";

        public virtual int GetEnumInt(string key) => GetSettingField(key)?.enumValue ?? 0;

        public virtual T GetEnum<T>(string key) where T : Enum
        {
            var field = GetSettingField(key);

            if (field == null || field.type != SettingType.Enum) return default;
            return (T)Enum.ToObject(typeof(T), field.enumValue);
        }
        #endregion


        #region Setters
        public virtual bool SetField(string key, object value)
        {
            var field = GetSettingField(key);
            if (field == null) return false;

            switch (field.type)
            {
                case SettingType.Bool when value is bool b:  return SetBool(key, b);

                case SettingType.Int when value is int i:  return SetInt(key, i);

                case SettingType.Float when value is float f:  return SetFloat(key, f);

                case SettingType.String when value is string s:  return SetString(key, s);

                case SettingType.Enum when value is int ei: return SetEnumInt(key, ei);
                case SettingType.Enum when value is Enum e:  return SetEnumInt(key, Convert.ToInt32(e));
            }

            return false;
        }

        public virtual bool SetBool(string key, bool value)
        {
            var field = GetSettingField(key);
            if (field == null || field.type != SettingType.Bool) return false;
            field.boolValue = value;
            return true;
        }

        public virtual bool SetInt(string key, int value)
        {
            var field = GetSettingField(key);
            if (field == null || field.type != SettingType.Int) return false;
            if (field.useMin) value = UnityEngine.Mathf.Max(value, field.intMin);
            if (field.useMax) value = UnityEngine.Mathf.Min(value, field.intMax);
            field.intValue = value;
            return true;
        }

        public virtual bool SetFloat(string key, float value)
        {
            var field = GetSettingField(key);
            if (field == null || field.type != SettingType.Float) return false;
            if (field.useMin) value = UnityEngine.Mathf.Max(value, field.floatMin);
            if (field.useMax) value = UnityEngine.Mathf.Min(value, field.floatMax);
            field.floatValue = value;
            return true;
        }

        public virtual bool SetString(string key, string value)
        {
            var field = GetSettingField(key);
            if (field == null || field.type != SettingType.String) return false;
            field.stringValue = value;
            return true;
        }

        public virtual bool SetEnumInt(string key, int value)
        {
            var field = GetSettingField(key);
            if (field == null || field.type != SettingType.Enum) return false;
            field.enumValue = value;
            return true;
        }
        public virtual bool SetEnum<T>(string key, T value) where T : Enum => SetEnumInt(key, Convert.ToInt32(value));
        #endregion
    }
    #endregion



    #region SettingGroup
    [Serializable]
    public class SettingGroup : SettingFieldsContainer
    {
        #region Variables
        public string groupName;

        public static readonly SettingGroup Null = new NullSettingGroup();
        public virtual bool isNull => false;
        #endregion



        #region Main
        public SettingGroup() { }

        public SettingGroup(SettingGroup other)
        {
            groupName = other.groupName;

            fields = new();
            foreach (var f in other.fields)
                fields.Add(new SettingField(f));
        }
        #endregion
    }

    
    internal sealed class NullSettingGroup : SettingGroup
    {
        #region Variables
        public override bool isNull => true;
        #endregion



        #region Getters
        public override bool GetBool(string key) => false;
        public override int GetInt(string key) => 0;
        public override float GetFloat(string key) => 0f;
        public override string GetString(string key) => "";
        public override int GetEnumInt(string key) => 0;
        public override T GetEnum<T>(string key) => default;
        #endregion


        #region Setters
        public override bool SetField(string key, object value) => false;
        public override bool SetBool(string key, bool value) => false;
        public override bool SetInt(string key, int value) => false;
        public override bool SetFloat(string key, float value) => false;
        public override bool SetString(string key, string value) => false;
        public override bool SetEnumInt(string key, int value) => false;
        public override bool SetEnum<T>(string key, T value) => false;
        #endregion
    }
    #endregion



    #region SettingMap
    [Serializable]
    public class SettingMap : SettingFieldsContainer
    {
        #region Variables
        public string mapName;


        public List<SettingGroup> groups = new();

        public string lastDefaultSetDateTime = "No Default Set";
        public SettingMap_Data defaultFields;


        public static readonly SettingMap Null = new NullSettingMap();
        public virtual bool isNull => false;
        #endregion



        #region Main
        public SettingMap() { }

        public SettingMap(SettingMap other)
        {
            mapName = other.mapName;

            fields = new List<SettingField>();
            foreach (var f in other.fields)
                fields.Add(new SettingField(f));

            groups = new List<SettingGroup>();
            foreach (var g in other.groups)
                groups.Add(new SettingGroup(g));

            lastDefaultSetDateTime = other.lastDefaultSetDateTime;

            if (other.defaultFields != null) defaultFields = new SettingMap_Data(other.defaultFields);
        }
        #endregion

        
        #region Logic

        #region Access
        public SettingGroup GetGroup(string groupName) => groups.Find(g => g.groupName == groupName) ?? SettingGroup.Null;

        public override SettingField GetSettingField(string key)
        {
            var field = fields.Find(x => x.key == key);
            if (field != null) return field;

            foreach (var group in groups)
            {
                field = group.GetSettingField(key);
                
                if (field != null) return field;
            }

            return null;
        }
        #endregion


        #region Defaults
        public void SaveAsDefaults()
        {
            defaultFields = new SettingMap_Data(this);

            lastDefaultSetDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public bool RestoreDefaults()
        {
            if (defaultFields == null || !defaultFields.hasValue) return false;

            ApplySnapshot(defaultFields);
            return true;
        }

        internal void ApplySnapshot(SettingMap_Data snapshot)
        {
            fields.Clear();
            foreach (var f in snapshot.fields)
                fields.Add(new SettingField(f));

            var restoredGroups = new List<SettingGroup>();
            foreach (var groupSnapshot in snapshot.groups)
            {
                var group = groups.Find(g => g.groupName == groupSnapshot.groupName);
                if (group == null) group = new SettingGroup { groupName = groupSnapshot.groupName };

                group.fields.Clear();
                foreach (var f in groupSnapshot.fields)
                    group.fields.Add(new SettingField(f));

                restoredGroups.Add(group);
            }
            groups = restoredGroups;
        }
        #endregion

        #endregion
    }


    internal sealed class NullSettingMap : SettingMap
    {
        #region Variables
        public override bool isNull => true;
        #endregion



        #region Getters
        public override bool GetBool(string key) => false;
        public override int GetInt(string key) => 0;
        public override float GetFloat(string key) => 0f;
        public override string GetString(string key) => "";
        public override int GetEnumInt(string key) => 0;
        public override T GetEnum<T>(string key) => default;
        #endregion


        #region Setters
        public override bool SetField(string key, object value) => false;
        public override bool SetBool(string key, bool value) => false;
        public override bool SetInt(string key, int value) => false;
        public override bool SetFloat(string key, float value) => false;
        public override bool SetString(string key, string value) => false;
        public override bool SetEnumInt(string key, int value) => false;
        public override bool SetEnum<T>(string key, T value) => false;
        #endregion
    }
    #endregion
    
    #endregion



    #region Default Data Snapshots
    [Serializable]
    public class SettingGroup_Data
    {
        #region Variables
        public string groupName;


        public bool hasValue = false;

        public List<SettingField> fields = new();
        #endregion



        #region Main
        public SettingGroup_Data() { }

        public SettingGroup_Data(SettingGroup group)
        {
            groupName = group.groupName;


            hasValue = true;

            fields = new List<SettingField>();
            foreach (var f in group.fields)
                fields.Add(new SettingField(f));
        }

        public SettingGroup_Data(SettingGroup_Data other)
        {
            groupName = other.groupName;
            

            hasValue = other.hasValue;

            fields = new List<SettingField>();
            foreach (var f in other.fields)
                fields.Add(new SettingField(f));
        }
        #endregion
    }


    [Serializable]
    public class SettingMap_Data
    {
        #region Variables
        public string mapName;


        public bool hasValue = false;

        public List<SettingField> fields = new();
        public List<SettingGroup_Data> groups = new();
        #endregion



        #region Main
        public SettingMap_Data() { }

        public SettingMap_Data(SettingMap map)
        {
            mapName  = map.mapName;


            hasValue = true;

            fields = new List<SettingField>();
            foreach (var f in map.fields)
                fields.Add(new SettingField(f));

            groups = new List<SettingGroup_Data>();
            foreach (var g in map.groups)
                groups.Add(new SettingGroup_Data(g));
        }

        public SettingMap_Data(SettingMap_Data other)
        {
            mapName  = other.mapName;

            
            hasValue = other.hasValue;

            fields = new List<SettingField>();
            foreach (var f in other.fields)
                fields.Add(new SettingField(f));

            groups = new List<SettingGroup_Data>();
            foreach (var g in other.groups)
                groups.Add(new SettingGroup_Data(g));
        }
        #endregion
    }


    [Serializable]
    public class SettingsAtlas_Data
    {
        #region Variables
        public bool hasValue = false;

        public List<SettingMap_Data> maps = new();
        #endregion



        #region Main
        public SettingsAtlas_Data() { }

        public SettingsAtlas_Data(SettingsAtlas asset)
        {
            hasValue = true;

            maps = new List<SettingMap_Data>();
            foreach (var m in asset.maps)
                maps.Add(new SettingMap_Data(m));
        }

        public SettingsAtlas_Data(SettingsAtlas_Data other)
        {
            hasValue = other.hasValue;

            maps = new List<SettingMap_Data>();
            foreach (var m in other.maps)
                maps.Add(new SettingMap_Data(m));
        }
        #endregion
    }
    #endregion
}