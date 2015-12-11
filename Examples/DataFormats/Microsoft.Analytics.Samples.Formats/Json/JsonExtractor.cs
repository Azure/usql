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
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Analytics.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Analytics.Samples.Formats.Json
{
    /// <summary>
    /// JsonExtractor (sample)
    ///
    ///     [
    ///         { c1:r1v1, c2:r1v2, ...}, 
    ///         { c1:r2v2, c2:r2v2, ...}, 
    ///         ...
    ///     ] 
    ///     => IEnumerable[IRow]
    ///     
    /// </summary>
    [SqlUserDefinedExtractor(AtomicFileProcessing=true)]
    public class JsonExtractor : IExtractor
    {
        /// <summary/>
        private string rowpath;
        
        /// <summary/>
        public JsonExtractor(string rowpath = null)
        {
            this.rowpath = rowpath;
        }
        
        /// <summary/>
        public override IEnumerable<IRow>       Extract(IUnstructuredReader input, IUpdatableRow output)
        {
            // Json.Net
            using(var reader = new JsonTextReader(new StreamReader(input.BaseStream)))
            {
                // Parse Json
                //  TODO: Json.Net fails with empty input files
                var root = JToken.ReadFrom(reader);

                // Rows
                //  All objects are represented as rows
                foreach(JObject o in SelectChildren(root, this.rowpath))
                {
                    // All fields are represented as columns
                    this.JObjectToRow(o, output);

                    yield return output.AsReadOnly();
                }
            }
        }

        /// <summary/>
        private static IEnumerable<JObject>     SelectChildren(JToken root, string path)
        {
            // JObject children (only)
            //   As JObject(fields) have a clear mapping to Row(columns) as opposed to JArray (positional) or JValue(scalar)
            //  Note: 
            //   We ignore other types (as opposed to fail fast) since JSON supports heterogeneous (schema)
            //   We support the values that can be mapped, without failing all of them if one of happens to not be an Object.

            // Path specified
            if(!string.IsNullOrEmpty(path))
            {
                return root.SelectTokens(path).OfType<JObject>();
            }
            
            // Single JObject
            var o = root as JObject;
            if(o != null)
            {
                return new []{o};
            }

            // Multiple JObjects
            return root.Children().OfType<JObject>();
        }
        
        /// <summary/>
        protected virtual void                  JObjectToRow(JObject o, IUpdatableRow row)
        {
            foreach(var c in row.Schema)
            {
                JToken token = null;
                object value = c.DefaultValue;
                
                // All fields are represented as columns
                //  Note: Each JSON row/payload can contain more or less columns than those specified in the row schema
                //  We simply update the row for any column that matches (and in any order).
                if(o.TryGetValue(c.Name, out token) && token != null)
                {
                    // Note: We simply delegate to Json.Net for all data conversions
                    //  For data conversions beyond what Json.Net supports, do an explicit projection:
                    //      ie: SELECT DateTime.Parse(datetime) AS datetime, ...
                    //  Note: Json.Net incorrectly returns null even for some non-nullable types (sbyte)
                    //      We have to correct this by using the default(T) so it can fit into a row value
                    value = JsonFunctions.ConvertToken(token, c.Type) ?? c.DefaultValue;
                }

                // Update
                row.Set<object>(c.Name, value);
            }
        }
    }
}
