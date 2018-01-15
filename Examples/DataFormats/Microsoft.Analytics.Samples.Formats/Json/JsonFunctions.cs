// 
// Copyright (c) Microsoft and contributors.  All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// 
// See the License for the specific language governing permissions and
// limitations under the License.
// 
//
// History
// =======
// 2015    * Ed Triou    : original version.
// 2018-01 * Michael Rys : Added support for byte[] to support large JSON structures.
// 2018-01 * Michael Rys : Added support for SqlArray<string>/SqlArray<byte[]>
//
// Future possible work
// ====================
// - Add support for SqlMap and nesting of SqlMaps and SqlArrays.

using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Analytics.Types.Sql;
using System.Text;

namespace Microsoft.Analytics.Samples.Formats.Json
{
    /// <summary>
    /// JsonFunctions (sample)
    ///
    /// </summary>
    public static class JsonFunctions
    {
        /// <summary>
        /// JsonTuple("json", [$e1], [$e2], ...)
        ///     1. Parse Json (once for all paths)
        ///     2. Apply the path expressions
        ///     3. Tuples are returned as MAP[path, value]
        ///             Path  = Path of resolved node (matching the expression)
        ///             Value = Node contents (of the matching nodes)
        ///   ie:
        ///     JsonTuple(json, "id", "name")              -> field names          MAP{ {id, 1 }, {name, Ed } }
        ///     JsonTuple(json, "$.address.zip")           -> nested fields        MAP{ {address.zip, 98052}  }
        ///     JsonTuple(json, "$..address")              -> recursive children   MAP{ {address, 98052}, {order[0].address, 98065}, ...           }
        ///     JsonTuple(json, "$[?(@.id > 1)].id")       -> path expression      MAP{ {id, 2 }, {order[7].id, 4}, ...                            }
        ///     JsonTuple(json)                            -> children             MAP{ {id, 1 }, {name, Ed}, { email, donotreply@live,com }, ...  }
        /// </summary>
        // Takes JSON subtree as string
        public static SqlMap<string, string> JsonTuple(string json, params string[] paths)
        {
            // Delegate
            return JsonTuple<string>(json, paths);
        }
        /// <summary>
        /// Takes JSON subtree as byte[] (preferred if larger than string size limit in U-SQL)
        /// We assume that the byte[] is actually a UTF-8 encoded string. If the byte[] is something else, JsonTuple should fail.
        /// </summary>
        public static SqlMap<string, string> JsonTuple(byte[] json, params string[] paths)
        {
            // Delegate
            return JsonTuple<string>(json, paths);
        }

        /// <summary>
        /// Takes JSON subtree as string
        /// </summary>
        public static SqlMap<string, T> JsonTuple<T>(string json, params string[] paths)
        {
            // Parse (once)
            //  Note: Json.Net NullRefs on <null> input Json
            //        Given <null> is a common column/string value, map to empty set for composability
            var root = string.IsNullOrEmpty(json) ? new JObject() : JToken.Parse(json);
            
            // Apply paths
            if(paths != null && paths.Length > 0)
            {
                return SqlMap.Create( paths.SelectMany( path => ApplyPath<T>(root, path)) );
            }
            
            // Children
            return SqlMap.Create( ApplyPath<T>(root, null) );
        }

        public static SqlMap<string, T> JsonTuple<T>(byte[] json_bytes, params string[] paths)
        {
            var json = System.Text.Encoding.UTF8.GetString(json_bytes);
            // Delegate now to the string input
            return JsonTuple<T>(json, paths);
        }

        /// <summary/>
        private static IEnumerable<KeyValuePair<string,T>> ApplyPath<T>(JToken root, string path)
        {
            // Children
            var children = SelectChildren<T>(root, path);
            foreach(var token in children)
            {
                // Token => T
                var value = (T)JsonFunctions.ConvertToken(token, typeof(T));

                // Tuple(path, value)
                yield return new KeyValuePair<string,T>(token.Path, value);
            }
        }

        /// <summary/>
        private static IEnumerable<JToken> SelectChildren<T>(JToken root, string path)
        {
            // Path specified
            if(!string.IsNullOrEmpty(path))
            {
                return root.SelectTokens(path);
            }

            // Single JObject
            var o = root as JObject;
            if(o != null)
            {
                //  Note: We have to special case JObject.
                //      Since JObject.Children() => JProperty.ToString() => "{"id":1}" instead of value "1".
                return o.PropertyValues();
            }

            // Multiple JObjects
            return root.Children();
        }

        /// <summary>
        ///  convert the JToken value to the appropriate string serialization of the JToken's type.
        ///  </summary>
        internal static string GetTokenString(JToken token)
        {
            switch(token.Type)
            {
                case JTokenType.Null:
                case JTokenType.Undefined:
                    return null;

                case JTokenType.String:
                    return (string)token;

                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.Boolean:
                    // For scalars we simply delegate to Json.Net (JsonConvert) for string conversions
                    //  This ensures the string conversion matches the JsonTextWriter
                    return JsonConvert.ToString(((JValue)token).Value);

                case JTokenType.Date:
                case JTokenType.TimeSpan:
                case JTokenType.Guid:
                    // For scalars we simply delegate to Json.Net (JsonConvert) for string conversions
                    //  Note: We want to leverage JsonConvert to ensure the string conversion matches the JsonTextWriter
                    //        However that places surrounding quotes for these data types.
                    var v = JsonConvert.ToString(((JValue)token).Value);
                    return v != null && v.Length > 2 && v[0] == '"' && v[v.Length-1] == '"' ? v.Substring(1, v.Length-2) : v;

                default:
                    // For containers we delegate to Json.Net (JToken.ToString/WriteTo) which is capable of serializing all data types, including nested containers
                    return token.ToString();
            }
        }

        /// <summary>
        ///  Convert the JToken value to the appropriate byte[] serialization of the JToken's type. 
        ///  If the JToken type is not supported (e.g., Raw and Bytes), an error is raised.
        ///  
        ///  We convert non-string values into strings (and not some more efficient variable-binary encoding)
        ///  to handle the case where data is heterogeneous between different instances and need to be surfaced consistently. 
        ///  E.g., age is a string on one object and an integer on the other. This allows the query writer to handle all values the same.
        ///  </summary>
        internal static byte[] GetTokenByteArray(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Null:
                case JTokenType.Undefined:
                    return null;

                // in case of string, use UTF-8 encoding to save space over UTF-16 assuming most JSON is numbers and ASCII.
                // A future extension could offer other encodings as options.
                case JTokenType.String:
                    return Encoding.UTF8.GetBytes((string)token);

                // For non-scalar objects, we serialize them into a string before we convert them into byte[].
                case JTokenType.Object:
                case JTokenType.Array:
                    return Encoding.UTF8.GetBytes(token.ToString());

                // For numeric scalars we simply delegate to Json.Net (JsonConvert) for string conversions.
                // This ensures the string conversion matches the JsonTextWriter.
                // Then we convert it into a UTF-8 byte array to keep it consistent with the textual representation. 
                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.Boolean:
                    return Encoding.UTF8.GetBytes(JsonConvert.ToString(((JValue)token).Value));

                // For non-numeric scalars we simply delegate to Json.Net (JsonConvert) for string conversions
                // Note: We want to leverage JsonConvert to ensure the string conversion matches the JsonTextWriter
                //       However that places surrounding quotes for these data types. Thus we drop the quotes.
                // Then we convert it into a UTF-8 byte array to keep it consistent with the textual representation. 
                case JTokenType.Date:
                case JTokenType.TimeSpan:
                case JTokenType.Guid:
                    var v = JsonConvert.ToString(((JValue)token).Value);
                    return Encoding.UTF8.GetBytes(
                        v != null && v.Length > 2 && v[0] == '"' && v[v.Length - 1] == '"' ? v.Substring(1, v.Length - 2) : v
                        );

                default:
                    // For other token types (e.g., Raw and Bytes etc) we currently just raise an error.
                    throw new JsonSerializationException(
                        string.Format(typeof(JsonFunctions).Namespace + " converting JSON Token '{0}' from type '{1}' to 'byte[]' is not supported.", token.Path, token.Type.ToString()),
                        null);
            }
        }

        /// <summary>
        ///  convert the JToken value to one of:
        ///  If the JToken is an array, we map it into the corresponding Array with each value being represented as a string,
        ///  otherwise serialize the JToken's string representation into a singleton string array.
        ///  
        ///  We allow lifting of singletons into arrays because JSON has no schema and may use an array on one property instance
        ///  and a scalar on the next. 
        ///  
        ///  Note: a string in an array is still limited in size like a normal string.
        ///  </summary>
        internal static SqlArray<string> GetTokenSqlArrayOfString(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Null:
                case JTokenType.Undefined:
                    return null;

                case JTokenType.String:
                    return SqlArray.Create(Enumerable.Repeat((string)token, 1));

                case JTokenType.Array:
                    return SqlArray.Create(ApplyPath<string>(token, null).Select((kv) => kv.Value));

                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.Boolean:
                    // For scalars we simply delegate to Json.Net (JsonConvert) for string conversions
                    //  This ensures the string conversion matches the JsonTextWriter
                    return SqlArray.Create(Enumerable.Repeat(JsonConvert.ToString(((JValue)token).Value),1));

                case JTokenType.Date:
                case JTokenType.TimeSpan:
                case JTokenType.Guid:
                    // For scalars we simply delegate to Json.Net (JsonConvert) for string conversions
                    //  Note: We want to leverage JsonConvert to ensure the string conversion matches the JsonTextWriter
                    //        However that places surrounding quotes for these data types.
                    var v = JsonConvert.ToString(((JValue)token).Value);
                    return SqlArray.Create(Enumerable.Repeat(
                        v != null && v.Length > 2 && v[0] == '"' && v[v.Length - 1] == '"' ? v.Substring(1, v.Length - 2) : v
                        , 1));

                default:
                    // For non-array containers we delegate to Json.Net (JToken.ToString/WriteTo) which is capable of serializing all data types, including nested containers
                    return SqlArray.Create(Enumerable.Repeat(token.ToString(),1));
            }
        }

        /// <summary>
        ///  convert the JToken value to one of:
        ///  If the JToken is an array, we map it into the corresponding Array with each value being represented as a byte array,
        ///  otherwise serialize the JToken's string representation into a singleton array of a byte array.
        ///  
        ///  We allow lifting of singletons into arrays because JSON has no schema and may use an array on one property instance
        ///  and a scalar on the next. 
        ///  
        ///  Note: This is needed if you suspect your string value to be larger than a string size.
        ///  </summary>
        internal static SqlArray<byte[]> GetTokenSqlArrayOfBytes(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Null:
                case JTokenType.Undefined:
                    return null;

                case JTokenType.String:
                    return SqlArray.Create(Enumerable.Repeat(Encoding.UTF8.GetBytes((string)token), 1));

                case JTokenType.Array:
                    return SqlArray.Create(ApplyPath<string>(token, null).Select((kv) => Encoding.UTF8.GetBytes(kv.Value)));

                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.Boolean:
                    // For scalars we simply delegate to Json.Net (JsonConvert) for string conversions
                    //  This ensures the string conversion matches the JsonTextWriter
                    return SqlArray.Create(Enumerable.Repeat(Encoding.UTF8.GetBytes(JsonConvert.ToString(((JValue)token).Value)), 1));

                case JTokenType.Date:
                case JTokenType.TimeSpan:
                case JTokenType.Guid:
                    // For scalars we simply delegate to Json.Net (JsonConvert) for string conversions
                    //  Note: We want to leverage JsonConvert to ensure the string conversion matches the JsonTextWriter
                    //        However that places surrounding quotes for these data types.
                    var v = JsonConvert.ToString(((JValue)token).Value);
                    return SqlArray.Create(Enumerable.Repeat( Encoding.UTF8.GetBytes(
                        v != null && v.Length > 2 && v[0] == '"' && v[v.Length - 1] == '"' ? v.Substring(1, v.Length - 2) : v)
                        , 1));

                default:
                    // For non-array containers we delegate to Json.Net (JToken.ToString/WriteTo) which is capable of serializing all data types, including nested containers
                    return SqlArray.Create(Enumerable.Repeat(Encoding.UTF8.GetBytes(token.ToString()), 1));
            }
        }

        /// <summary/>
        internal static object ConvertToken(JToken token, Type type)
        {
            try
            { 
                // If the expected type is string, we convert the JToken value to the appropriate string serialization
                if(type == typeof(string))
                {
                    return JsonFunctions.GetTokenString(token);
                }
                // If the expected type is byte[], we serialize the JToken's string representation into a byte[] (UTF-8 encoded).
                else if (type == typeof(byte[]))
                {
                    return JsonFunctions.GetTokenByteArray(token);
                }
                // To support JSON arrays and reduce the need to map long arrays first to byte[],
                // we map either the JToken Array into the SqlArray,
                // or otherwise serialize the JToken's string representation into a singleton array.
                else if (type == typeof(SqlArray<string>))
                {
                    return JsonFunctions.GetTokenSqlArrayOfString(token);
                }
                // If an item in the array is still too long for a string, a SqlArray<byte[]> can be used
                // to work around the string size limit,
                else if (type == typeof(SqlArray<byte[]>))
                {
                    return JsonFunctions.GetTokenSqlArrayOfBytes(token);
                }
                // Otherwise, we simply delegate to Json.Net for data conversions
                return token.ToObject(type);
            }
            catch(Exception e)
            {
                // Make this easier to debug (with field and type context)
                //  Note: We don't expose the actual value to be converted in the error message (since it might be sensitive, information disclosure)
                throw new JsonSerializationException(
                    string.Format(typeof(JsonToken).Namespace + " failed to deserialize '{0}' from '{1}' to '{2}'", token.Path, token.Type.ToString(), type.FullName), 
                    e);
            }
        }
    }
}
