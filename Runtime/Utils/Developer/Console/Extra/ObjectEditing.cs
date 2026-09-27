using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;

namespace SHUU.Utils.Developer.Console
{
    public static class ObjectEditing
    {
        #region Variables
        private const int MaxValueLength = 80;


        private static readonly HashSet<string> UnityNoise = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "hideFlags", "gameObject", "transform", "material", "materials", "mesh" };
        #endregion




        #region Logic

        #region Reading
        public static List<MemberInfo> GetMembers(Type type)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

            List<MemberInfo> members = new List<MemberInfo>();

            foreach (FieldInfo field in type.GetFields(flags))
                if (!IsHidden(field)) members.Add(field);

            foreach (PropertyInfo property in type.GetProperties(flags))
                if (property.CanRead && property.GetIndexParameters().Length == 0 && !IsHidden(property)) members.Add(property);

            members.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));

            return members;
        }

        public static string ReadValue(MemberInfo member, object target)
        {
            try { return FormatValue(GetValue(member, target)); }
            catch (Exception e) { return "(couldn't read: " + Shorten(Unwrap(e).Message) + ")"; }
        }

        public static string FormatValue(object value)
        {
            string text;

            if (IsNull(value)) text = "null";
            else if (value is UnityEngine.Object unityObject) text = $"{unityObject.name} ({unityObject.GetType().Name})";
            else if (value is string words) text = "\"" + words + "\"";
            else if (value is float single) text = single.ToString("0.###", CultureInfo.InvariantCulture);
            else if (value is double number) text = number.ToString("0.###", CultureInfo.InvariantCulture);
            else if (value is Array array) text = $"{array.GetType().GetElementType().Name}[{array.Length}]";
            else if (value is ICollection collection) text = $"{value.GetType().Name} ({collection.Count} items)";
            else text = value.ToString();

            return Shorten(text);
        }


        private static bool IsHidden(MemberInfo member)
        {
            if (member.IsDefined(typeof(ObsoleteAttribute), true)) return true;

            string space = member.DeclaringType?.Namespace;

            return space != null && space.StartsWith("UnityEngine", StringComparison.Ordinal) && UnityNoise.Contains(member.Name);
        }
        #endregion



        #region Writing
        public static bool TryPrepareSet(object root, string path, string text, out SetEdit edit, out string error)
        {
            edit = null;
            error = null;

            string[] segments = path.Split('.');
            object current = root;

            for (int i = 0; i < segments.Length - 1; i++)
            {
                MemberInfo step = QueryParameter.FindMember(current.GetType(), segments[i]);

                if (step == null)
                {
                    error = $"{current.GetType().Name} has no public field or property called '{segments[i]}'.";
                    return false;
                }

                try { current = GetValue(step, current); }
                catch (Exception e)
                {
                    error = $"Couldn't read '{segments[i]}': {Unwrap(e).Message}";
                    return false;
                }

                if (IsNull(current))
                {
                    error = $"'{segments[i]}' is null, so there's nothing to set inside it.";
                    return false;
                }

                if (current.GetType().IsValueType)
                {
                    error = $"'{segments[i]}' is a {current.GetType().Name}, which can't be changed from the inside. Set the whole thing instead (like position 1,2,3).";
                    return false;
                }
            }


            string name = segments[segments.Length - 1];
            MemberInfo member = QueryParameter.FindMember(current.GetType(), name);

            if (member == null)
            {
                error = $"{current.GetType().Name} has no public field or property called '{name}'.";
                return false;
            }


            Type type;
            bool writable;

            if (member is FieldInfo field)
            {
                type = field.FieldType;
                writable = !field.IsInitOnly && !field.IsLiteral;
            }
            else
            {
                PropertyInfo property = (PropertyInfo)member;

                type = property.PropertyType;
                writable = property.CanWrite && property.SetMethod != null && property.SetMethod.IsPublic;
            }

            if (!writable)
            {
                error = $"'{name}' is read-only.";
                return false;
            }

            if (!TryConvert(text, type, out object converted, out string problem))
            {
                error = $"{name}: {problem}";
                return false;
            }


            object target = current;

            edit = new SetEdit(ReadValue(member, target), FormatValue(converted), () =>
            {
                if (member is FieldInfo fieldInfo) fieldInfo.SetValue(target, converted);
                else ((PropertyInfo)member).SetValue(target, converted);
            });

            return true;
        }

        public static bool TryConvert(string text, Type type, out object value, out string error)
        {
            value = null;
            error = null;

            if (type == typeof(string))
            {
                value = text;
                return true;
            }

            if (type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector4))
            {
                int count = type == typeof(Vector2) ? 2 : type == typeof(Vector3) ? 3 : 4;

                if (!TryParseNumbers(text, count, count, out float[] n))
                {
                    error = $"A {type.Name} needs {count} numbers separated by commas, like {(count == 2 ? "1,2" : count == 3 ? "1,2,3" : "1,2,3,4")}.";
                    return false;
                }

                if (count == 2) value = new Vector2(n[0], n[1]);
                else if (count == 3) value = new Vector3(n[0], n[1], n[2]);
                else value = new Vector4(n[0], n[1], n[2], n[3]);

                return true;
            }

            if (type == typeof(Color))
            {
                if (text.IndexOf(',') >= 0)
                {
                    if (TryParseNumbers(text, 3, 4, out float[] n))
                    {
                        value = new Color(n[0], n[1], n[2], n.Length == 4 ? n[3] : 1f);
                        return true;
                    }
                }
                else if (ColorUtility.TryParseHtmlString(text, out Color color))
                {
                    value = color;
                    return true;
                }

                error = "A Color is written like #ff0000, red, or 1,0,0 (red, green, blue and optionally alpha, from 0 to 1).";
                return false;
            }

            if (!type.IsEnum && !typeof(IConvertible).IsAssignableFrom(type))
            {
                error = $"A {type.Name} can't be set from text.";
                return false;
            }

            try
            {
                value = DevConsoleManager.ConvertArgument(text, type, null);
                return true;
            }
            catch (Exception e)
            {
                error = type.IsEnum ? e.Message : $"'{text}' isn't a valid {type.Name}.";
                return false;
            }
        }


        private static bool TryParseNumbers(string text, int min, int max, out float[] numbers)
        {
            string[] parts = text.Split(',');

            numbers = new float[parts.Length];

            if (parts.Length < min || parts.Length > max) return false;

            for (int i = 0; i < parts.Length; i++)
                if (!float.TryParse(parts[i].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out numbers[i])) return false;

            return true;
        }
        #endregion



        #region Helpers
        private static object GetValue(MemberInfo member, object target) => member is FieldInfo field ? field.GetValue(target) : ((PropertyInfo)member).GetValue(target);

        private static bool IsNull(object value) => value == null || (value is UnityEngine.Object unityObject && unityObject == null);

        private static Exception Unwrap(Exception e) => e is TargetInvocationException && e.InnerException != null ? e.InnerException : e;

        private static string Shorten(string text)
        {
            text = text.Replace("\r", " ").Replace("\n", " ");

            return text.Length > MaxValueLength ? text.Substring(0, MaxValueLength) + "..." : text;
        }
        #endregion
    
        #endregion
    
    
    
    
        #region Helper class
        public sealed class SetEdit
        {
            public string oldText;
            public string newText;

            private readonly Action apply;


            public SetEdit(string oldText, string newText, Action apply)
            {
                this.oldText = oldText;
                this.newText = newText;

                this.apply = apply;
            }

            public string Apply()
            {
                try
                {
                    apply?.Invoke();
                    return null;
                }
                catch (Exception e) { return Unwrap(e).Message; }
            }
        }
        #endregion
    }
}
