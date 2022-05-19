using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace System
{
    public static class ObjectExtensions
    {
        public static object As(this object value, Type type, object defaultValue)
        {
            try
            {
                if (value == null) { }
                else if (value.GetType() == type)
                    defaultValue = value;
                else if (type == typeof(Guid) && value is string)
                    defaultValue = new Guid((string)value);
                else if (type == typeof(DateTimeOffset) && value is string)
                    defaultValue = DateTimeOffset.Parse((string)value);
                else if (type.IsEnum && value is string)
                    defaultValue = ((string)value).ToEnum(type);
                else if (type.IsEnum && (value is int || value is long))
                    defaultValue = ((long)value).ToEnum(type);
                else if (value is JsonNode)
                    defaultValue = ((JsonNode)value).ToJsonString().FromJSON(type);
                else if (value != null)
                    defaultValue = Convert.ChangeType(value, type);
            }
            catch (InvalidCastException)
            {
                defaultValue = value;
            }
            catch { }

            return defaultValue;
        }

        /// <summary>
        ///     Method will use the <see cref="System.Convert.ChangeType" /> method to change the type of the object to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to change the object to.</typeparam>
        /// <param name="value">The object to change the type of.</param>
        /// <param name="defaultValue">The default value to return if the object cannot be converted to the type.</param>
        /// <returns>Returns the changed type value of the object.</returns>
        /// <example>
        ///     This method can be used in many ways.  It can be used to get a string value as a boolean:
        ///     <code>
        ///         bool value = "true".As<bool>(false);
        ///     </code>
        /// </example>
        public static T As<T>(this object value, T defaultValue = default(T))
        {
            return (T)As(value, typeof(T), defaultValue);
        }

        public static async Task<MemoryStream> CopyTo(this Stream value)
        {
            var stream = new MemoryStream();

            if (value.CanSeek)
                value.Seek(0, SeekOrigin.Begin);

            await value.CopyToAsync(stream);

            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }

        //public static void DeepMap(this JsonObject mapTo, JsonObject mapFrom, IDictionary<string, string> map)
        //{
        //    map.Each(
        //        (m) =>
        //        {
        //            var from = mapFrom.SelectToken(m.Key);

        //            mapTo.DotSet(m.Value, from);
        //        });
        //}

        //public static void DotSet<T>(this JsonObject value, string dotPath, T newValue)
        //{
        //    var token = value.SelectToken(dotPath);

        //    if (token == null)
        //    {
        //        var splits = dotPath.Split('.');

        //        var working = value;

        //        splits.Each(
        //            (split) =>
        //            {
        //                var sub = working.SelectToken(split);

        //                if (sub.IsNullOrEmpty())
        //                {
        //                    JsonNode propVal = null;

        //                    if (split == splits.Last())
        //                    {
        //                        if (newValue != null)
        //                            propVal = JsonNode.FromObject(newValue);
        //                    }
        //                    else
        //                        propVal = new { }.JSONConvert<JObject>();

        //                    if (sub == null)
        //                        working.Add(split, propVal);
        //                    else
        //                        working[split] = propVal;

        //                    sub = working.SelectToken(split);
        //                }

        //                if (sub is JObject)
        //                {
        //                    working = sub as JObject;

        //                    return false;
        //                }
        //                else
        //                    return true;
        //            });
        //    }
        //    else
        //        token.Replace(JsonNode.FromObject(newValue));
        //}

        public static void FireAndForget(this Task task)
        {
            Task.Run(async () => await task).ConfigureAwait(false);
        }

        public static bool IsEmpty(this Guid value)
        {
            return value == Guid.Empty;
        }

        public static bool IsNullOrEmpty(this JsonNode token)
        {
            return (token == null);// ||
                                   //(token.Type == JsonNodeType.Null ||
                                   //(token.Type == JsonNodeType.Array && !token.HasValues) ||
                                   //(token.Type == JsonNodeType.Object && !token.HasValues) ||
                                   //(token.Type == JsonNodeType.String && token.ToString() == String.Empty));
        }

        public static byte[] ToBytes(this object value)
        {
            var stream = new MemoryStream();

            var binFormatter = new BinaryFormatter();

            binFormatter.Serialize(stream, value);

            return stream.ToArray();
        }

        public static object FromBytes(this byte[] bytes)
        {
            MemoryStream stream = new MemoryStream(bytes);

            var binFormatter = new BinaryFormatter();

            return binFormatter.Deserialize(stream);
        }

        public static Guid GetUniqueGuid(this IEnumerable<Guid> existingGuids)
        {
            Guid newGuid;

            do
            {
                newGuid = Guid.NewGuid();
            }
            while (!existingGuids.IsNullOrEmpty() && existingGuids.Contains(newGuid));

            return newGuid;
        }

        public static T DataContractDeserialize<T>(this byte[] value)
        {
            var srlzr = new DataContractSerializer(typeof(T));

            using var stream = new MemoryStream(value);

            return (T)srlzr.ReadObject(stream);
        }

        public static byte[] DataContractSerialize<T>(this T value)
        {
            var srlzr = new DataContractSerializer(typeof(T));

            using var stream = new MemoryStream();

            srlzr.WriteObject(stream, value);

            return stream.ToArray();
        }

        public static T JSONConvert<T>(this object value, JsonSerializerOptions options = null)
        {
            return value.JSONConvert<T>(options, options);
        }

        public static T JSONConvert<T>(this object value, JsonSerializerOptions toOptions,
            JsonSerializerOptions fromOptions)
        {
            var json = value.ToJSON(toOptions);

            var val = json.FromJSON<T>(fromOptions);

            return val;
        }

        //public static T Merge<T>(this T target, T source)
        //{
        //    var targetMap = target is JsonNode ? target.As<JsonNode>() : target.JSONConvert<JsonNode>();

        //    var sourceMap = source is JsonNode ? source.As<JsonNode>() : source.JSONConvert<JsonNode>();

        //    var children = sourceMap.Children().Cast<JProperty>();

        //    children.Each(s =>
        //    {
        //        var targetToken = targetMap[s.Name];

        //        var sourceToken = sourceMap[s.Name];

        //        if ((sourceToken is JValue && ((JValue)sourceToken).Value != null) ||
        //            (!(sourceToken is JValue) && sourceToken.HasValues))
        //        {
        //            if (targetToken != null)
        //            {
        //                var replaceToken = sourceToken;

        //                if (!(sourceToken is JValue))
        //                    replaceToken = targetToken.Merge(sourceToken);

        //                targetToken.Replace(replaceToken);
        //            }
        //            else
        //                ((JObject)targetMap).Add(s.Name, sourceToken);
        //        }
        //    });

        //    return targetMap.JSONConvert<T>();
        //}

        public static dynamic ToDynamic(this object value)
        {
            IDictionary<string, object> expando = new ExpandoObject();

            if (value != null)
                foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
                    expando.Add(property.Name, property.GetValue(value));

            return expando as ExpandoObject;
        }

        public static T ToEnum<T>(this long value)
        {
            return (T)ToEnum(value, typeof(T));
        }

        public static object ToEnum(this long value, Type type)
        {
            return Enum.ToObject(type, value);
        }

        public static T ToEnum<T>(this int value)
        {
            return (T)ToEnum(value, typeof(T));
        }

        public static object ToEnum(this int value, Type type)
        {
            return ToEnum((long)value, type);
        }

        public static long ToEpoch(this DateTime date, DateTime? epoch = null)
        {
            if (date == null)
                return Int32.MinValue;

            if (!epoch.HasValue)
                epoch = new DateTime(1970, 1, 1);

            var epochTimeSpan = date - epoch.Value;

            return (long)epochTimeSpan.TotalSeconds;
        }

        public static string ToJSON(this object value, JsonSerializerOptions options = null)
        {
            if (value == null)
                return String.Empty;

            return JsonSerializer.Serialize(value, options);
        }
    }
}
