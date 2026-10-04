/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace SHUU._Editor.Utils.PlayModeSaver
{
    [Serializable]
    internal class ComponentSnapshot
    {
        #region Variables
        public SceneObjectLocator locator;

        public List<PropertyValue> values = new List<PropertyValue>();


        private const string ArraySizeSuffix = ".Array.size";
        private const string SceneReferencePrefix = "scene:";

        private static readonly HashSet<string> StructuralProperties = new HashSet<string>
        {
            "m_Script", "m_GameObject", "m_Father", "m_Children", "m_RootOrder",
            "m_PrefabInstance", "m_PrefabAsset", "m_CorrespondingSourceObject", "m_ObjectHideFlags"
        };

        private static readonly PropertyInfo GradientValue = typeof(SerializedProperty).GetProperty("gradientValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private enum ReadResult { Value, NoValue, SceneReference, Unsupported }
        #endregion




        #region Capture
        public static ComponentSnapshot Capture(Component component, SaveReport report, out string problem)
        {
            if (!SceneObjectLocator.TryCreate(component, out SceneObjectLocator locator, out problem)) return null;

            ComponentSnapshot snapshot = new ComponentSnapshot { locator = locator };


            using (SerializedObject serializedObject = new SerializedObject(component))
            {
                SerializedProperty property = serializedObject.GetIterator();
                bool enterChildren = true;

                while (property.NextVisible(enterChildren))
                {
                    enterChildren = property.propertyType == SerializedPropertyType.Generic;

                    if (StructuralProperties.Contains(property.propertyPath))
                    {
                        enterChildren = false;

                        continue;
                    }

                    switch (TryRead(property, locator.scenePath, out string value))
                    {
                        case ReadResult.Value:
                            snapshot.values.Add(new PropertyValue { path = property.propertyPath, kind = (int)property.propertyType, value = value });
                            break;

                        case ReadResult.SceneReference:
                            report.skippedReferences++;
                            break;

                        case ReadResult.Unsupported:
                            report.unsupported++;
                            break;
                    }
                }
            }

            problem = null;

            return snapshot;
        }


        private static ReadResult TryRead(SerializedProperty property, string ownerScenePath, out string value)
        {
            value = null;

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer: value = PropertyCodec.FromLong(IsLong(property) ? property.longValue : property.intValue); return ReadResult.Value;
                case SerializedPropertyType.Boolean: value = property.boolValue ? "1" : "0"; return ReadResult.Value;
                case SerializedPropertyType.Float: value = PropertyCodec.FromDouble(IsDouble(property) ? property.doubleValue : property.floatValue); return ReadResult.Value;
                case SerializedPropertyType.String: value = property.stringValue ?? ""; return ReadResult.Value;
                case SerializedPropertyType.LayerMask: value = PropertyCodec.JoinInts(property.intValue); return ReadResult.Value;
                case SerializedPropertyType.Enum: value = PropertyCodec.JoinInts(property.intValue); return ReadResult.Value;
                case SerializedPropertyType.Character: value = PropertyCodec.JoinInts(property.intValue); return ReadResult.Value;
                case SerializedPropertyType.ArraySize: value = PropertyCodec.JoinInts(property.intValue); return ReadResult.Value;

                case SerializedPropertyType.Color: { Color c = property.colorValue; value = PropertyCodec.JoinFloats(c.r, c.g, c.b, c.a); return ReadResult.Value; }
                case SerializedPropertyType.Vector2: { Vector2 v = property.vector2Value; value = PropertyCodec.JoinFloats(v.x, v.y); return ReadResult.Value; }
                case SerializedPropertyType.Vector3: { Vector3 v = property.vector3Value; value = PropertyCodec.JoinFloats(v.x, v.y, v.z); return ReadResult.Value; }
                case SerializedPropertyType.Vector4: { Vector4 v = property.vector4Value; value = PropertyCodec.JoinFloats(v.x, v.y, v.z, v.w); return ReadResult.Value; }
                case SerializedPropertyType.Quaternion: { Quaternion q = property.quaternionValue; value = PropertyCodec.JoinFloats(q.x, q.y, q.z, q.w); return ReadResult.Value; }
                case SerializedPropertyType.Rect: { Rect r = property.rectValue; value = PropertyCodec.JoinFloats(r.x, r.y, r.width, r.height); return ReadResult.Value; }
                case SerializedPropertyType.Bounds: { Bounds b = property.boundsValue; value = PropertyCodec.JoinFloats(b.center.x, b.center.y, b.center.z, b.extents.x, b.extents.y, b.extents.z); return ReadResult.Value; }

                case SerializedPropertyType.Vector2Int: { Vector2Int v = property.vector2IntValue; value = PropertyCodec.JoinInts(v.x, v.y); return ReadResult.Value; }
                case SerializedPropertyType.Vector3Int: { Vector3Int v = property.vector3IntValue; value = PropertyCodec.JoinInts(v.x, v.y, v.z); return ReadResult.Value; }
                case SerializedPropertyType.RectInt: { RectInt r = property.rectIntValue; value = PropertyCodec.JoinInts(r.x, r.y, r.width, r.height); return ReadResult.Value; }
                case SerializedPropertyType.BoundsInt: { BoundsInt b = property.boundsIntValue; value = PropertyCodec.JoinInts(b.position.x, b.position.y, b.position.z, b.size.x, b.size.y, b.size.z); return ReadResult.Value; }

                case SerializedPropertyType.Hash128: value = property.hash128Value.ToString(); return ReadResult.Value;

                case SerializedPropertyType.AnimationCurve: value = EncodeCurve(property.animationCurveValue); return ReadResult.Value;

                case SerializedPropertyType.Gradient:
                    if (GradientValue == null) return ReadResult.Unsupported;

                    value = EncodeGradient((Gradient)GradientValue.GetValue(property));
                    return ReadResult.Value;

                case SerializedPropertyType.ObjectReference:
                    return ReadReference(property, ownerScenePath, out value);

                case SerializedPropertyType.Generic:
                    return ReadResult.NoValue;

                default:
                    return ReadResult.Unsupported;
            }
        }


        private static ReadResult ReadReference(SerializedProperty property, string ownerScenePath, out string value)
        {
            Object reference = property.objectReferenceValue;

            if (reference == null)
            {
                value = "";

                return property.objectReferenceInstanceIDValue == 0 ? ReadResult.Value : ReadResult.SceneReference;
            }

            if (EditorUtility.IsPersistent(reference))
            {
                value = GlobalObjectId.GetGlobalObjectIdSlow(reference).ToString();

                return ReadResult.Value;
            }

            if (SceneObjectLocator.TryCreate(reference, out SceneObjectLocator locator, out _) && locator.scenePath == ownerScenePath)
            {
                value = SceneReferencePrefix + JsonUtility.ToJson(locator);

                return ReadResult.Value;
            }

            value = null;

            return ReadResult.SceneReference;
        }


        private static bool IsLong(SerializedProperty property) => property.type == "long" || property.type == "ulong";
        private static bool IsDouble(SerializedProperty property) => property.type == "double";
        #endregion



        #region Apply
        public bool TryFind(out Component component, out string problem)
        {
            component = null;

            if (!locator.TryFind(out Object found, out problem)) return false;

            component = found as Component;

            if (component != null) return true;

            problem = $"'{locator.Description}' isn't a component";

            return false;
        }


        public string Description => locator == null ? "?" : locator.Description;


        public void ApplyTo(Component component, SaveReport report)
        {
            string owner = component.GetType().Name;

            using (SerializedObject serializedObject = new SerializedObject(component))
            {
                foreach (PropertyValue saved in values)
                {
                    try
                    {
                        if (TryApplyValue(serializedObject, saved, out string problem))
                        {
                            report.saved++;

                            if (saved.kind == (int)SerializedPropertyType.ObjectReference && saved.value.StartsWith(SceneReferencePrefix, StringComparison.Ordinal)) report.sceneReferences++;
                        }
                        else
                        {
                            report.failed++;

                            if (problem != null) PlayModeComponentSaver.Warn($"Couldn't restore '{saved.path}' on {owner}: {problem}.");
                        }
                    }
                    catch (Exception exception)
                    {
                        report.failed++;

                        PlayModeComponentSaver.Warn($"Couldn't restore '{saved.path}' on {owner}: {exception.Message}");
                    }
                }

                serializedObject.ApplyModifiedProperties();
            }
        }


        private static bool TryApplyValue(SerializedObject serializedObject, PropertyValue saved, out string problem)
        {
            problem = null;

            if (saved.kind == (int)SerializedPropertyType.ArraySize && saved.path.EndsWith(ArraySizeSuffix, StringComparison.Ordinal))
            {
                SerializedProperty array = serializedObject.FindProperty(saved.path.Substring(0, saved.path.Length - ArraySizeSuffix.Length));

                if (array == null || !array.isArray) return false;

                array.arraySize = PropertyCodec.SplitInts(saved.value)[0];

                return true;
            }


            SerializedProperty property = serializedObject.FindProperty(saved.path);

            if (property == null || (int)property.propertyType != saved.kind) return false;

            return TryWrite(property, saved.value, out problem);
        }


        private static bool TryWrite(SerializedProperty property, string value, out string problem)
        {
            problem = null;

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                {
                    long number = PropertyCodec.ToLong(value);

                    if (IsLong(property)) property.longValue = number;
                    else property.intValue = (int)number;

                    return true;
                }

                case SerializedPropertyType.Boolean: property.boolValue = value == "1"; return true;

                case SerializedPropertyType.Float:
                {
                    double number = PropertyCodec.ToDouble(value);

                    if (IsDouble(property)) property.doubleValue = number;
                    else property.floatValue = (float)number;

                    return true;
                }
                case SerializedPropertyType.String: property.stringValue = value; return true;
                case SerializedPropertyType.LayerMask: property.intValue = PropertyCodec.SplitInts(value)[0]; return true;
                case SerializedPropertyType.Enum: property.intValue = PropertyCodec.SplitInts(value)[0]; return true;
                case SerializedPropertyType.Character: property.intValue = PropertyCodec.SplitInts(value)[0]; return true;

                case SerializedPropertyType.Color: { float[] f = PropertyCodec.SplitFloats(value); property.colorValue = new Color(f[0], f[1], f[2], f[3]); return true; }
                case SerializedPropertyType.Vector2: { float[] f = PropertyCodec.SplitFloats(value); property.vector2Value = new Vector2(f[0], f[1]); return true; }
                case SerializedPropertyType.Vector3: { float[] f = PropertyCodec.SplitFloats(value); property.vector3Value = new Vector3(f[0], f[1], f[2]); return true; }
                case SerializedPropertyType.Vector4: { float[] f = PropertyCodec.SplitFloats(value); property.vector4Value = new Vector4(f[0], f[1], f[2], f[3]); return true; }
                case SerializedPropertyType.Quaternion: { float[] f = PropertyCodec.SplitFloats(value); property.quaternionValue = new Quaternion(f[0], f[1], f[2], f[3]); return true; }
                case SerializedPropertyType.Rect: { float[] f = PropertyCodec.SplitFloats(value); property.rectValue = new Rect(f[0], f[1], f[2], f[3]); return true; }
                case SerializedPropertyType.Bounds: { float[] f = PropertyCodec.SplitFloats(value); property.boundsValue = new Bounds { center = new Vector3(f[0], f[1], f[2]), extents = new Vector3(f[3], f[4], f[5]) }; return true; }

                case SerializedPropertyType.Vector2Int: { int[] i = PropertyCodec.SplitInts(value); property.vector2IntValue = new Vector2Int(i[0], i[1]); return true; }
                case SerializedPropertyType.Vector3Int: { int[] i = PropertyCodec.SplitInts(value); property.vector3IntValue = new Vector3Int(i[0], i[1], i[2]); return true; }
                case SerializedPropertyType.RectInt: { int[] i = PropertyCodec.SplitInts(value); property.rectIntValue = new RectInt(i[0], i[1], i[2], i[3]); return true; }
                case SerializedPropertyType.BoundsInt: { int[] i = PropertyCodec.SplitInts(value); property.boundsIntValue = new BoundsInt(new Vector3Int(i[0], i[1], i[2]), new Vector3Int(i[3], i[4], i[5])); return true; }

                case SerializedPropertyType.Hash128: property.hash128Value = Hash128.Parse(value); return true;

                case SerializedPropertyType.AnimationCurve: property.animationCurveValue = DecodeCurve(value); return true;

                case SerializedPropertyType.Gradient:
                    if (GradientValue == null) return false;

                    GradientValue.SetValue(property, DecodeGradient(value));
                    return true;

                case SerializedPropertyType.ObjectReference:
                    return TryWriteReference(property, value, out problem);

                default:
                    return false;
            }
        }


        private static bool TryWriteReference(SerializedProperty property, string value, out string problem)
        {
            problem = null;

            if (value.Length == 0)
            {
                property.objectReferenceValue = null;

                return true;
            }


            Object reference;

            if (value.StartsWith(SceneReferencePrefix, StringComparison.Ordinal))
            {
                SceneObjectLocator locator = JsonUtility.FromJson<SceneObjectLocator>(value.Substring(SceneReferencePrefix.Length));

                if (locator == null)
                {
                    problem = "the reference wasn't written down properly";

                    return false;
                }

                if (!locator.TryFind(out reference, out string notFound))
                {
                    problem = $"the object it pointed at: {notFound}";

                    return false;
                }
            }
            else
            {
                if (!GlobalObjectId.TryParse(value, out GlobalObjectId id)) return false;

                reference = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);

                if (reference == null)
                {
                    problem = "the asset it pointed at isn't there anymore";

                    return false;
                }
            }


            property.objectReferenceValue = reference;

            if (property.objectReferenceValue != reference)
            {
                problem = $"the field doesn't take a {reference.GetType().Name}";

                return false;
            }

            return true;
        }
        #endregion



        #region Curves and gradients
        private static string EncodeCurve(AnimationCurve curve)
        {
            List<float> numbers = new List<float> { (float)curve.preWrapMode, (float)curve.postWrapMode };

            foreach (Keyframe key in curve.keys)
            {
                numbers.Add(key.time);
                numbers.Add(key.value);
                numbers.Add(key.inTangent);
                numbers.Add(key.outTangent);
                numbers.Add(key.inWeight);
                numbers.Add(key.outWeight);
                numbers.Add((float)key.weightedMode);
            }

            return PropertyCodec.JoinFloats(numbers.ToArray());
        }

        private static AnimationCurve DecodeCurve(string value)
        {
            float[] n = PropertyCodec.SplitFloats(value);

            Keyframe[] keys = new Keyframe[(n.Length - 2) / 7];

            for (int i = 0; i < keys.Length; i++)
            {
                int at = 2 + i * 7;

                keys[i] = new Keyframe(n[at], n[at + 1], n[at + 2], n[at + 3], n[at + 4], n[at + 5]) { weightedMode = (WeightedMode)(int)n[at + 6] };
            }

            return new AnimationCurve(keys) { preWrapMode = (WrapMode)(int)n[0], postWrapMode = (WrapMode)(int)n[1] };
        }


        private static string EncodeGradient(Gradient gradient)
        {
            GradientColorKey[] colorKeys = gradient.colorKeys;
            GradientAlphaKey[] alphaKeys = gradient.alphaKeys;

            List<float> numbers = new List<float> { (float)gradient.mode, colorKeys.Length, alphaKeys.Length };

            foreach (GradientColorKey key in colorKeys)
            {
                numbers.Add(key.color.r);
                numbers.Add(key.color.g);
                numbers.Add(key.color.b);
                numbers.Add(key.color.a);
                numbers.Add(key.time);
            }

            foreach (GradientAlphaKey key in alphaKeys)
            {
                numbers.Add(key.alpha);
                numbers.Add(key.time);
            }

            return PropertyCodec.JoinFloats(numbers.ToArray());
        }

        private static Gradient DecodeGradient(string value)
        {
            float[] n = PropertyCodec.SplitFloats(value);

            GradientColorKey[] colorKeys = new GradientColorKey[(int)n[1]];
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[(int)n[2]];

            int at = 3;

            for (int i = 0; i < colorKeys.Length; i++, at += 5)
                colorKeys[i] = new GradientColorKey(new Color(n[at], n[at + 1], n[at + 2], n[at + 3]), n[at + 4]);

            for (int i = 0; i < alphaKeys.Length; i++, at += 2)
                alphaKeys[i] = new GradientAlphaKey(n[at], n[at + 1]);

            Gradient gradient = new Gradient { mode = (GradientMode)(int)n[0] };
            gradient.SetKeys(colorKeys, alphaKeys);

            return gradient;
        }
        #endregion
    }



    [Serializable]
    internal class PropertyValue
    {
        public string path;
        public int kind;
        public string value;
    }



    [Serializable]
    internal class SaveReport
    {
        public int saved;
        public int sceneReferences;
        public int skippedReferences;
        public int unsupported;
        public int failed;
    }
}
#endif
