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
using System.Collections.Generic;
using Microsoft.Analytics.Interfaces;
using Avro.File;
using Avro.Generic;
using System.IO;
using System;
using Microsoft.Analytics.Types.Sql;
using System.Linq;

namespace Microsoft.Analytics.Samples.Formats.ApacheAvro
{
    [SqlUserDefinedExtractor(AtomicFileProcessing = true)]
    public class AvroExtractor : IExtractor
    {
        private string avroSchema;

        public AvroExtractor(string avroSchema)
        {
            this.avroSchema = avroSchema;
        }

        public override IEnumerable<IRow> Extract(IUnstructuredReader input, IUpdatableRow output)
        {
            var avschema = Avro.Schema.Parse(avroSchema);
            var reader = new GenericDatumReader<GenericRecord>(avschema, avschema);

            using (var ms = new MemoryStream())
            {
                CreateSeekableStream(input, ms);
                ms.Position = 0;

                var fileReader = DataFileReader<GenericRecord>.OpenReader(ms, avschema);

                while (fileReader.HasNext())
                {
                    var avroRecord = fileReader.Next();

                    foreach (var column in output.Schema)
                    {
                        if (avroRecord[column.Name] != null)
                        {
                            // Map
                            if(avroRecord[column.Name] is Dictionary<string, object>)
                            {
                                OutputDictionaryAsMap((Dictionary<string, object>)avroRecord[column.Name], column, output); 
                            }
                            else
                            {
                                output.Set(column.Name, avroRecord[column.Name]);
                            }
                        }
                        else
                        {
                            output.Set<object>(column.Name, null);
                        }
                    }
                    
                    yield return output.AsReadOnly();
                }
            }
        }

        private void OutputDictionaryAsMap(Dictionary<string, object> dict, IColumn column, IUpdatableRow output)
        {
            // int
            if (column.Type == typeof(SqlMap<string, int?>))
            {
                output.Set(column.Name, new SqlMap<string, int?>(dict.ToDictionary(p => p.Key, p => {
                    try
                    {
                        return (int?)Convert.ToInt32(p.Value);
                    }
                    catch
                    {
                        throw new Exception("Type mismatch. Cannot convert source value to integer.");
                    }
                })));
            }
            // string
            else if (column.Type == typeof(SqlMap<string, string>))
            {
                output.Set(column.Name, new SqlMap<string, string>(dict.ToDictionary(p => p.Key, p => p.Value.ToString())));
            }
            // bool
            else if (column.Type == typeof(SqlMap<string, bool?>))
            {
                output.Set(column.Name, new SqlMap<string, bool?>(dict.ToDictionary(p => p.Key, p => (bool?)p.Value)));
            }
            // long
            else if (column.Type == typeof(SqlMap<string, long?>))
            {
                output.Set(column.Name, new SqlMap<string, long?>(dict.ToDictionary(p => p.Key, p => (long?)p.Value)));
            }
            // double
            else if (column.Type == typeof(SqlMap<string, double?>))
            {
                output.Set(column.Name, new SqlMap<string, double?>(dict.ToDictionary(p => p.Key, p => {
                    try
                    {
                        return (double?)Convert.ToInt64(p.Value);
                    }
                    catch
                    {
                        throw new Exception("Type mismatch. Cannot convert source value to double.");
                    }
                })));
            }
            // float
            else if (column.Type == typeof(SqlMap<string, float?>))
            {
                output.Set(column.Name, new SqlMap<string, float?>(dict.ToDictionary(p => p.Key, p => (float?)p.Value)));
            }
            // byte[]
            else if (column.Type == typeof(SqlMap<string, byte[]>))
            {
                output.Set(column.Name, new SqlMap<string, byte[]>(dict.ToDictionary(p => p.Key, p => (byte[])p.Value)));
            }
            else
            {
                throw new Exception($"Unsupported datatype. {column.Type.GetGenericArguments()[1]} is not supported for SQL.MAP.");
            }
        }

        private void CreateSeekableStream(IUnstructuredReader input, MemoryStream output)
        {
            input.BaseStream.CopyTo(output);
        }
    }
}
