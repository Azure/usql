using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Analytics.Interfaces;
using Newtonsoft.Json.Linq;
using Microsoft.Analytics.Samples.Formats.Json.Exceptions;

namespace Microsoft.Analytics.Samples.Formats.Json
{
    public class MultiLevelJsonExtractor : JsonExtractor
    {
        private string[] jsonPaths;
        private readonly bool bypassWarning;

        /// <summary>
        /// Use it by supplying multiple levels of Json Paths.  They will be assigned to the schema by index.  
        /// </summary>
        /// <param name="rowpath">The base path to start from.</param>
        /// <param name="bypassWarning">If you want an error when a path isn't found leave as false.  If you don't want errors and a null result, set to true.</param>
        /// <param name="jsonPaths">Paths in the Json Document.  If it isn't found at the "rowpath" level it will recurse to the top of the tree to locate it.</param>
        public MultiLevelJsonExtractor(string rowpath = null, bool bypassWarning = false, params string[] jsonPaths)
            : base(rowpath)
        {
            this.jsonPaths = jsonPaths;
            this.bypassWarning = bypassWarning;
        }

        protected override void JObjectToRow(JObject o, IUpdatableRow row)
        {
            if (jsonPaths.Length == 0)
            {
                base.JObjectToRow(o, row);
            }
            else
            {
                for (int i = 0; i < jsonPaths.Length; i++)
                {
                    var path = jsonPaths[i];
                    var jObj = findByPath(o, path);
                    object value = null;

                    if (jObj != default(JToken))
                    {
                        var schemaColumn = row.Schema[i];
                        value = JsonFunctions.ConvertToken(jObj, schemaColumn.Type) ?? schemaColumn.DefaultValue;
                    }
                    row.Set<object>(i, value);
                }
            }
        }

        /// <summary>
        /// Recursively walks up the tree to find a path if it wasn't found at the device level
        /// </summary>
        /// <param name="o"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private JToken findByPath(JToken o, string path)
        {
            var jObj = o.SelectToken(path);

            if (jObj == null)
            {
                if (o.Parent == null || !(o.Parent is JToken))
                {
                    if (bypassWarning)
                    {
                        return default(JObject);
                    }
                    throw new PathNotFoundException(string.Format("Path {0} could not be found!", path));
                }

                return findByPath(o.Parent, path);
            }

            return jObj;
        }
    }
}
