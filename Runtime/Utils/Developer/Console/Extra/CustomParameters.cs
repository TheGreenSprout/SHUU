using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;

namespace SHUU.Utils.Developer.Console
{
    #region Return
    public sealed class CommandReturn
    {
        #region Variables
        public string[] output = null;
        public Color? color = null;
        #endregion



        #region Main
        public CommandReturn(Color? color, params string[] output)
        {
            this.output = output;
            this.color = color;
        }

        public CommandReturn(params string[] output) => this.output = output;
        public CommandReturn(bool result) : this(result ? null : Color.red, result ? "Command successfully executed." : "Something went wrong, command aborted.") { }

        public CommandReturn(params CommandReturn[] returns)
        {
            color = returns[0]?.color;

            List<string> output = new();
            foreach (var ret in returns)
            {
                if (ret == null) continue;

                if (ret.output != null) output.AddRange(ret.output);  
            }
            this.output = output.ToArray();
        }


        public static CommandReturn Green(params string[] message) => new CommandReturn(Color.green, message);
        public static CommandReturn Red(params string[] message) => new CommandReturn(Color.red, message);
        public static CommandReturn Yellow(params string[] message) => new CommandReturn(Color.yellow, message);
        #endregion
    }
    #endregion





    #region Parameters

    #region Optional
    public sealed class OptionalParameter<T>
    {
        #region Variables
        private bool hasValue = false;
        
        private T value = default;
        #endregion



        #region Main
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
        #endregion


        #region Logic
        public bool TryGetValue(out T outValue)
        {
            outValue = value;

            return hasValue;
        }

        public override string ToString()
        {
            if (!hasValue) return DevConsoleManager.Instance.optionalParameter_consoleInterpreters[0];
            else return value.ToString();
        }
        #endregion
    }
    #endregion




    #region Mutable
    public sealed class MutableParameter
    {
        #region Variables
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
        #endregion
        


        #region Main
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
        #endregion


        #region Logic
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
        #endregion
    }
    #endregion




    #region Query
    public sealed class QueryParameter
    {
        #region Variables
        private enum Operator
        {
            Equal,
            NotEqual,
            Less,
            LessOrEqual,
            Greater,
            GreaterOrEqual,
            Contains
        }

        private sealed class Condition
        {
            public string path;
            public string[] segments;
            public Operator op;
            public string value;
        }


        private static readonly char[] OperatorCharacters = { '=', '!', '<', '>', '~' };

        private static readonly Dictionary<(Type, string), MemberInfo> MemberCache = new Dictionary<(Type, string), MemberInfo>();


        private readonly List<Condition> conditions = new List<Condition>();


        public string Raw { get; }

        public bool IsEmpty => conditions.Count == 0;
        #endregion




        #region Main
        private QueryParameter(string raw) => Raw = raw;

        public static QueryParameter Parse(string raw)
        {
            QueryParameter query = new QueryParameter(raw ?? "");

            foreach (string part in query.Raw.Split(','))
            {
                string text = part.Trim();

                if (text.Length == 0 || text == "*") continue;

                query.conditions.Add(ParseCondition(text));
            }

            return query;
        }

        public override string ToString() => Raw;
        #endregion




        /*
        ⚠️‼️ AI ASSISTED SNIPPET

        This code snippet was written with the assistance of AI.
        */
        #region Logic

        #region Fetch
        #region XML doc
        /// <summary>
        /// The items that match. Throws a QueryException if a field doesn't exist or can't be compared with the value.
        /// </summary>
        #endregion
        public List<T> Filter<T>(IEnumerable<T> source)
        {
            List<T> matches = new List<T>();

            foreach (T item in source)
                if (Matches(item)) matches.Add(item);

            return matches;
        }

        public bool Matches(object item)
        {
            if (IsNull(item)) return false;

            foreach (Condition condition in conditions)
                if (!Matches(item, condition)) return false;

            return true;
        }
        #endregion



        #region Parsing
        private static Condition ParseCondition(string text)
        {
            int index = text.IndexOfAny(OperatorCharacters);

            if (index < 0) throw new QueryException($"'{text}' has no operator (use = != < <= > >= ~).");
            if (index == 0) throw new QueryException($"'{text}' has no field before the operator.");


            string path = text.Substring(0, index);
            string[] segments = path.Split('.');

            foreach (string segment in segments)
                if (segment.Length == 0) throw new QueryException($"'{path}' isn't a valid field.");


            char symbol = text[index];
            char next = index + 1 < text.Length ? text[index + 1] : '\0';

            Operator op;
            int length = 1;

            switch (symbol)
            {
                case '=':
                    op = Operator.Equal;
                    break;

                case '~':
                    op = Operator.Contains;
                    break;

                case '!':
                    if (next != '=') throw new QueryException($"'{text}': '!' has to be followed by '=' (!=).");

                    op = Operator.NotEqual;
                    length = 2;
                    break;

                case '<':
                    op = next == '=' ? Operator.LessOrEqual : Operator.Less;
                    if (next == '=') length = 2;
                    break;

                default:
                    op = next == '=' ? Operator.GreaterOrEqual : Operator.Greater;
                    if (next == '=') length = 2;
                    break;
            }

            return new Condition { path = path, segments = segments, op = op, value = text.Substring(index + length) };
        }
        #endregion



        #region Reading fields
        private static bool Matches(object item, Condition condition) => Compare(Resolve(item, condition), condition);

        private static object Resolve(object item, Condition condition)
        {
            object current = item;

            foreach (string segment in condition.segments)
            {
                if (IsNull(current)) return null;

                Type type = current.GetType();

                MemberInfo member = FindMember(type, segment);
                if (member == null) throw new QueryException($"{type.Name} has no public field or property called '{segment}' (in '{condition.path}').");

                try { current = member is FieldInfo field ? field.GetValue(current) : ((PropertyInfo)member).GetValue(current); }
                catch (TargetInvocationException e) { throw new QueryException($"Couldn't read '{condition.path}' on {item.GetType().Name}: {(e.InnerException ?? e).Message}"); }
            }

            return IsNull(current) ? null : current;
        }

        #region XML doc
        /// <summary>
        /// The public field or property of a type with that name (not case sensitive), or null if there isn't one.
        /// </summary>
        #endregion
        public static MemberInfo FindMember(Type type, string name)
        {
            if (MemberCache.TryGetValue((type, name), out MemberInfo cached)) return cached;


            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

            MemberInfo found = null;

            try { found = type.GetField(name, flags); }
            catch (AmbiguousMatchException) { }

            if (found == null)
            {
                foreach (PropertyInfo property in type.GetProperties(flags))
                {
                    if (!string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase) || !property.CanRead || property.GetIndexParameters().Length > 0) continue;

                    found = property;
                    break;
                }
            }

            MemberCache[(type, name)] = found;

            return found;
        }
        #endregion



        #region Comparing
        private static bool Compare(object value, Condition condition)
        {
            bool wantsNull = string.Equals(condition.value, "null", StringComparison.OrdinalIgnoreCase);
            bool isEquality = condition.op == Operator.Equal || condition.op == Operator.NotEqual;

            if (value == null)
            {
                if (!isEquality) return false;

                return condition.op == Operator.Equal ? wantsNull : !wantsNull;
            }

            if (wantsNull && isEquality) return condition.op == Operator.NotEqual;


            switch (condition.op)
            {
                case Operator.Contains: return Text(value).IndexOf(condition.value, StringComparison.OrdinalIgnoreCase) >= 0;

                case Operator.Equal: return AreEqual(value, condition);
                case Operator.NotEqual: return !AreEqual(value, condition);

                default: return CompareOrder(value, condition);
            }
        }

        private static bool AreEqual(object value, Condition condition)
        {
            string wanted = condition.value;

            if (value is string text) return string.Equals(text, wanted, StringComparison.OrdinalIgnoreCase);

            if (value is bool flag)
            {
                if (!bool.TryParse(wanted, out bool wantedFlag)) throw new QueryException($"'{wanted}' isn't true or false (for '{condition.path}').");

                return flag == wantedFlag;
            }

            Type type = value.GetType();

            if (type.IsEnum)
            {
                if (!Enum.TryParse(type, wanted, true, out object wantedValue)) throw new QueryException($"'{wanted}' isn't a value of {type.Name} (for '{condition.path}').");

                return value.Equals(wantedValue);
            }

            if (IsNumber(value)) return CompareNumber(value, ParseNumber(wanted, condition)) == 0;

            return string.Equals(Text(value), wanted, StringComparison.OrdinalIgnoreCase);
        }

        private static bool CompareOrder(object value, Condition condition)
        {
            if (!IsNumber(value)) throw new QueryException($"'{condition.path}' isn't a number, so it can't be compared with {Symbol(condition.op)}.");

            int order = CompareNumber(value, ParseNumber(condition.value, condition));

            switch (condition.op)
            {
                case Operator.Less: return order < 0;
                case Operator.LessOrEqual: return order <= 0;
                case Operator.Greater: return order > 0;

                default: return order >= 0;
            }
        }
        #endregion



        #region Helpers
        private static bool IsNull(object value) => value == null || (value is UnityEngine.Object unityObject && unityObject == null);

        private static bool IsNumber(object value) => value is sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal;

        private static int CompareNumber(object value, double wanted) => value is float number ? number.CompareTo((float)wanted) : Convert.ToDouble(value).CompareTo(wanted);

        private static double ParseNumber(string text, Condition condition)
        {
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double number)) return number;

            throw new QueryException($"'{text}' isn't a number (for '{condition.path}').");
        }

        private static string Text(object value) => value is UnityEngine.Object unityObject ? unityObject.name : value.ToString();

        private static string Symbol(Operator op)
        {
            switch (op)
            {
                case Operator.Less: return "<";
                case Operator.LessOrEqual: return "<=";
                case Operator.Greater: return ">";
                case Operator.GreaterOrEqual: return ">=";

                default: return op.ToString();
            }
        }
        #endregion

        #endregion
    }



    public class QueryException : Exception
    {
        public QueryException(string message) : base(message) { }
    }
    #endregion

    #endregion
}
