using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

using SHUU.UserSide.Addons.SavingSystem.ForUser;

using static SHUU.Utils.Helpers.HandyFunctions;

namespace SHUU.UserSide.Addons.SavingSystem
{
    #region XML doc
    /// <summary>
    /// How save files are turned into json and back.
    /// Every DTO is kept as plain json inside the save, and turned into its class by the saving info that owns it (which knows the class), so the file doesn't
    /// say which class to create for a DTO. Type names are still used for polymorphic fields inside a DTO (a field of a base type holding a derived one),
    /// and a hand-edited file could ask for any type to be created there. To stop that, only DTOs and a few harmless types are accepted.
    /// If you need something else inside a DTO's polymorphic fields, allow it with <see cref="AllowType"/> or <see cref="AllowAssembly"/>.
    /// </summary>
    #endregion
    public static class SaveSerialization
    {
        #region Variables
        private static readonly SaveTypeBinder Binder = new SaveTypeBinder();


        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            SerializationBinder = Binder,
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };

        private static readonly JsonSerializer Serializer = JsonSerializer.Create(Settings);
        #endregion




        #region Logic
        public static string Serialize<T>(T value) => JsonConvert.SerializeObject(value, Settings);

        public static T Deserialize<T>(string json) => JsonConvert.DeserializeObject<T>(json, Settings);


        #region XML doc
        /// <summary>
        /// Turns a DTO into the plain json that goes into the MasterDTO.
        /// </summary>
        #endregion
        internal static JToken ToToken(DTO_Info dto) => dto == null ? null : JToken.FromObject(dto, Serializer);

        #region XML doc
        /// <summary>
        /// Turns the json of a DTO back into the DTO class a saving info expects. Throws if the json doesn't fit that class.
        /// </summary>
        #endregion
        internal static DTO_Info ToDTO(JToken token, Type dtoType)
        {
            if (token == null || token.Type == JTokenType.Null) return null;


            if (token is JObject obj) obj.Remove("$type");

            return (DTO_Info)token.ToObject(dtoType, Serializer);
        }


        #region XML doc
        /// <summary>
        /// Lets a type be created from a save file even though it's not a DTO.
        /// </summary>
        #endregion
        public static void AllowType<T>() => Binder.Allow(typeof(T));

        #region XML doc
        /// <summary>
        /// Lets every type of an assembly be created from a save file even though they're not DTOs.
        /// </summary>
        #endregion
        public static void AllowAssembly(Assembly assembly) => Binder.Allow(assembly);


        #region Parse
        internal static SaveParseResult ParseMaster(string json, out MasterDTO master, out string error)
        {
            master = null;
            error = null;


            if (string.IsNullOrWhiteSpace(json))
            {
                error = "The file is empty.";

                return SaveParseResult.Corrupt;
            }


            try { master = JsonConvert.DeserializeObject<MasterDTO>(json, Settings); }
            catch (Exception e)
            {
                error = e.DescribeException();
                master = null;

                // A cut off file makes the deserializer throw a serialization exception, not a syntax one, so check the syntax on its own.
                return IsValidJson(json) ? SaveParseResult.Incompatible : SaveParseResult.Corrupt;
            }


            if (master == null)
            {
                error = "The file doesn't contain a save.";

                return SaveParseResult.Corrupt;
            }

            if (master.dataDictionary == null)
            {
                error = "The save has no data.";
                master = null;

                return SaveParseResult.Incompatible;
            }

            if (master.formatVersion > MasterDTO.CurrentFormatVersion)
            {
                error = $"The save uses format {master.formatVersion}, but this version only understands up to format {MasterDTO.CurrentFormatVersion}.";
                master = null;

                return SaveParseResult.Incompatible;
            }


            return SaveParseResult.Ok;
        }

        private static bool IsValidJson(string json)
        {
            try
            {
                JToken.Parse(json);

                return true;
            }
            catch (JsonReaderException) { return false; }
            catch (Exception) { return true; }
        }
        #endregion

        #endregion
    



        #region Type binder
        private sealed class SaveTypeBinder : ISerializationBinder
        {
            #region Variables
            private static readonly HashSet<Type> GenericContainers = new HashSet<Type>
            {
                typeof(List<>),
                typeof(Dictionary<,>),
                typeof(HashSet<>),
                typeof(Queue<>),
                typeof(Stack<>),
                typeof(LinkedList<>),
                typeof(SortedDictionary<,>),
                typeof(SortedList<,>),
                typeof(KeyValuePair<,>),
                typeof(Nullable<>)
            };


            private readonly DefaultSerializationBinder inner = new DefaultSerializationBinder();

            private readonly object gate = new object();
            private readonly HashSet<Type> allowedTypes = new HashSet<Type>();
            private readonly HashSet<Assembly> allowedAssemblies = new HashSet<Assembly>();
            #endregion



            #region Logic
            public Type BindToType(string assemblyName, string typeName)
            {
                Type type = inner.BindToType(assemblyName, typeName);

                if (!IsAllowed(type))
                    throw new JsonSerializationException($"The type '{type}' is not allowed in save files. Make it a DTO_Info, or allow it with SaveSerialization.AllowType / AllowAssembly.");

                return type;
            }

            public void BindToName(Type serializedType, out string assemblyName, out string typeName) => inner.BindToName(serializedType, out assemblyName, out typeName);


            public void Allow(Type type) { lock (gate) allowedTypes.Add(type); }
            public void Allow(Assembly assembly) { lock (gate) allowedAssemblies.Add(assembly); }


            private bool IsAllowed(Type type)
            {
                if (type == null) return false;

                if (type.IsArray) return IsAllowed(type.GetElementType());

                if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime) || type == typeof(TimeSpan) || type == typeof(Guid)) return true;

                if (typeof(DTO_Info).IsAssignableFrom(type)) return true;

                // Vector3, Quaternion, Color...
                if (type.IsValueType && type.Namespace == "UnityEngine") return true;


                if (type.IsGenericType && GenericContainers.Contains(type.GetGenericTypeDefinition()))
                {
                    foreach (Type argument in type.GetGenericArguments())
                        if (!IsAllowed(argument)) return false;

                    return true;
                }


                lock (gate) return allowedTypes.Contains(type) || allowedAssemblies.Contains(type.Assembly);
            }
            #endregion
        }
        #endregion
    }





    #region Parse result
    internal enum SaveParseResult
    {
        Ok,

        // Not valid json (cut off, empty, garbage...). This is what a crash while writing looks like.
        Corrupt,

        // Valid json that the current code can't read (the save is from a newer version, it doesn't have the layout of a save...). Nothing is wrong with the file itself.
        Incompatible
    }
    #endregion
}
