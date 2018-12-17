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

using System.Collections;
using System.IO;
using Microsoft.Analytics.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Analytics.Types.Sql;

namespace Microsoft.Analytics.Samples.Formats.Json
{
    /// <summary>
    /// JsonLinesOutputter (sample)
    ///
    ///     IEnumerable[IRow] =>
    ///         { c1:r1v1, c2:r1v2, ...}
    ///         { c1:r2v2, c2:r2v2, ...} 
    ///         ...
    /// Notice that this outputter doesn't require atomic output,
    /// since it produces standalone JSON documents as opposed to a single JSON array.
    /// </summary>
    [SqlUserDefinedOutputter(AtomicFileProcessing = false)]
    public class JsonLinesOutputter : IOutputter
    {
        /// <summary/>
        private JsonTextWriter writer;

        private static readonly KeyValuePair<string, object> KVP = new KeyValuePair<string, object>(string.Empty,string.Empty);
        private const string keyPropName = nameof(KVP.Key);
        private const string valPropName = nameof(KVP.Value);

        /// <summary/>
        public JsonLinesOutputter()
        {
        }

        /// <summary/>
        public override void Output(IRow row, IUnstructuredWriter output)
        {
            if (this.writer == null)
            {
                // Json.Net (writer)
                this.writer = new JsonTextWriter(new StreamWriter(output.BaseStream)) { Formatting = Formatting.None };
            }

            // Row(s)
            WriteRow(row, this.writer);
        }

        /// <summary/>
        public override void Close()
        {
            if (this.writer != null)
            {
                this.writer.Flush();
                this.writer.Close();
            }
        }

        /// <summary/>
        private static void WriteRow(IRow row, JsonTextWriter writer)
        {
            // Row
            //  => { c1:v1, c2:v2, ...}

            // Header
            writer.WriteStartObject();

            // Fields
            var columns = row.Schema;
            for (int i = 0; i < columns.Count; i++)
            {
                // Note: We simply delegate to Json.Net for all data conversions
                //  For data conversions beyond what Json.Net supports, do an explicit projection:
                //      ie: SELECT datetime.ToString(...) AS datetime, ...
                object value = row.Get<object>(i);

                // Note: We don't bloat the JSON with sparse (null) properties
                if (value != null)
                {
                    writer.WritePropertyName(columns[i].Name, escape: true);
                    WriteValue(writer, value);
                }
            }

            // Footer
            writer.WriteEndObject();
            writer.WriteWhitespace(Environment.NewLine);
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        private static void WriteValue(JsonTextWriter writer, object value)
        {
            if (value != null)
            {
                IEnumerable collection = value as IEnumerable;
                Type valueType = value.GetType();

                if (IsArray(collection))
                {
                    // Dictionary
                    if (IsMap(valueType))
                    {
                        WriteMapAsEnumerable(writer, collection);
                    }
                    // Array
                    else
                    {
                        WriteArray(writer, collection);
                    }
                }
                // KeyValue
                else if (IsMap(valueType))
                {
                    WriteKeyValuePair(writer, value);
                }
                else
                    writer.WriteValue(value);
            }
        }

        private static void WriteKeyValuePair(JsonTextWriter writer, object kvp)
        {

            WriteMapAsEnumerable(writer, new[] { kvp });
        }

        private static void WriteArray(JsonTextWriter writer, IEnumerable collection)
        {
            writer.WriteStartArray();
            foreach (var item in collection)
            {
                WriteValue(writer, item);
            }
            writer.WriteEndArray();
        }

        private static void WriteMapAsEnumerable(JsonTextWriter writer, IEnumerable collection)
        {
            writer.WriteStartObject();
            PropertyInfo keyProp = null;
            PropertyInfo valProp = null;
            foreach (var item in collection)
            {
                if (keyProp == null)
                {
                    Type itemType = item.GetType();
                    keyProp = itemType.GetProperty(keyPropName);
                    valProp = itemType.GetProperty(valPropName);
                }
                // ReSharper disable once PossibleNullReferenceException
                writer.WritePropertyName(keyProp.GetValue(item, null).ToString(), escape: true);
                // ReSharper disable once PossibleNullReferenceException
                WriteValue(writer, valProp.GetValue(item, null));
            }
            writer.WriteEndObject();
        }

        private static bool IsArray(IEnumerable collection)
        {
            return collection != null && !(collection is string);
        }

        private static bool IsMap(Type valueType)
        {
            return valueType.IsGenericType &&
                (valueType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>) ||
                valueType.GetGenericTypeDefinition() == typeof(Dictionary<,>) ||
                valueType.GetGenericTypeDefinition() == typeof(SqlMap<,>));
        }
    }
}
