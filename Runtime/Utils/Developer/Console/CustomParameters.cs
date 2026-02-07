using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using SHUU.Utils.Helpers;
using UnityEngine;

namespace SHUU.Utils.Developer.Console
{
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
        public CommandReturn(bool result) : this(result ? null : Color.red, result ? "Command successfully executed." : "Something went wrong, command aborted.") { }
    }




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
                    value = (T)(object)boolValue;
                    return true;

                case ValueType.Int when typeof(T) == typeof(int):
                    value = (T)(object)intValue;
                    return true;

                case ValueType.Float when typeof(T) == typeof(float):
                    value = (T)(object)floatValue;
                    return true;

                case ValueType.String when typeof(T) == typeof(string):
                    value = (T)(object)stringValue;
                    return true;

                case ValueType.Char when typeof(T) == typeof(char):
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
    }




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
