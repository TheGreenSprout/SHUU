using System;
using System.Collections.Generic;
using UnityEngine;

namespace SHUU.Utils.Developer.Console
{
    #region Return
    public class CommandReturn
    {
        public string[] output = null;
        public Color? color = null;



        public CommandReturn(Color? color, params string[] output)
        {
            this.output = output;
            this.color = color;
        }

        public CommandReturn(params string[] output) => this.output = output;
        public CommandReturn(bool result)
            : this(result ? null : Color.red, result ? "Command successfully executed." : "Something went wrong, command aborted.") { }

        public CommandReturn(params CommandReturn[] returns)
        {
            this.color = returns[0]?.color;

            List<string> output = new();
            foreach (var ret in returns)
            {
                if (ret == null) continue;

                if (ret.output != null) output.AddRange(ret.output);  
            }
            this.output = output.ToArray();
        }
    }
    #endregion




    #region Parameters
    public class OptionalParameter<T>
    {
        private bool hasValue = false;
        
        private T value = default;


        public OptionalParameter()
        {
            hasValue = false;

            value = default;
        }
        public OptionalParameter(T val)
        {
            hasValue = true;

            value = val;
        }

        public bool TryGetValue(out T outValue)
        {
            outValue = value;

            return hasValue;
        }

        public override string ToString()
        {
            if (!hasValue) return DevConsoleManager.instance.optionalParameter_consoleInterpreters[0];
            else return value.ToString();
        }
    }



    public class MutableParameter
    {
        private enum ValueType
        {
            Null,
            Bool,
            Int,
            Float,
            String,
            Char
        }
        private ValueType valueType = ValueType.Null;
        
        public bool? boolValue = null;
        private int? intValue = null;
        private float? floatValue = null;
        private string stringValue = null;
        private char? charValue = null;
        

        public MutableParameter(bool val)
        {
            valueType = ValueType.Bool;

            boolValue = val;
        }
        public MutableParameter(int val)
        {
            valueType = ValueType.Int;

            intValue = val;
        }
        public MutableParameter(float val)
        {
            valueType = ValueType.Float;

            floatValue = val;
        }
        public MutableParameter(string val)
        {
            valueType = ValueType.String;

            stringValue = val;
        }
        public MutableParameter(char val)
        {
            valueType = ValueType.Char;

            charValue = val;
        }


        public bool TryGetValue<T>(out T value)
        {
            value = default;

            switch (valueType)
            {
                case ValueType.Bool when typeof(T) == typeof(bool):
                    if (boolValue == null) return false;

                    value = (T)(object)boolValue;
                    return true;

                case ValueType.Int when typeof(T) == typeof(int):
                    if (intValue == null) return false;

                    value = (T)(object)intValue;
                    return true;

                case ValueType.Float when typeof(T) == typeof(float):
                    if (floatValue == null) return false;

                    value = (T)(object)floatValue;
                    return true;

                case ValueType.String when typeof(T) == typeof(string):
                    if (stringValue == null) return false;

                    value = (T)(object)stringValue;
                    return true;

                case ValueType.Char when typeof(T) == typeof(char):
                    if (charValue == null) return false;

                    value = (T)(object)charValue;
                    return true;

                default:
                    return false;
            }
        }

        public Type GetStoredType()
        {
            return valueType switch
            {
                ValueType.Bool => typeof(bool),
                ValueType.Int => typeof(int),
                ValueType.Float => typeof(float),
                ValueType.String => typeof(string),
                ValueType.Char => typeof(char),
                _ => null
            };
        }

        public override string ToString()
        {
            if (boolValue != null) return boolValue.ToString();
            else if (intValue != null) return intValue.ToString();
            else if (floatValue != null) return floatValue.ToString();
            else if (stringValue != null) return stringValue;
            else if (charValue != null) return charValue.ToString();
            
            else return "";
        }
    }
    #endregion




    #region Queries
    public enum QueryType
    {
        Null,
        Invalid,
        Where,
        Sort,
        Find
    }



    public class SimpleQuery<T>
    {
        public QueryType type = QueryType.Invalid;
        public Func<T, bool> predicate = null;

        public bool hasValue => type != QueryType.Null;
        public bool IsValid => hasValue && type != QueryType.Invalid && predicate != null;


        public SimpleQuery() { }
        public SimpleQuery(QueryType type, Func<T, bool> predicate)
        {
            this.type = type;
            this.predicate = predicate;
        }
    }
    #endregion
}
