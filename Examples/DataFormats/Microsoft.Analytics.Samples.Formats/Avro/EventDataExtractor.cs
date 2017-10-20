// 
// Copyright (c) Microsoft and contributors. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Runtime.Serialization;
using System;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Analytics.Samples.Formats.ApacheAvro
{


    public class OutputToAvroMap
    {
        public string AvroField { get; set; }
        public string Key { get; set; }
        public string Type { get; set; }
    }



    [SqlUserDefinedExtractor(AtomicFileProcessing = true)]
    public class EventDataExtractor : IExtractor
    {
        /*
         Sample Extract Map:
         DECLARE @extractMap string = @"
{
    ""EnqueuedTimeUtc"": {""AvroField"": ""EnqueuedTimeUtc""},
    ""Body"": {""AvroField"": ""Body""},
    ""Prop1"": {""AvroField"": ""Properties"",""Key"": ""MyProp1"",""Type"": ""string""}
}";
             */

        private readonly string avroSchema;
        private readonly string extractMap;

        public EventDataExtractor(string avroSchema, string extractMap)
        {
            this.avroSchema = avroSchema;
            this.extractMap = extractMap;
        }

        public override IEnumerable<IRow> Extract(IUnstructuredReader input, IUpdatableRow output)
        {
            var avschema = Avro.Schema.Parse(avroSchema);
            var extractSchema = JsonConvert.DeserializeObject<Dictionary<string, OutputToAvroMap>>(extractMap);

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
                        var map = extractSchema[column.Name];
                        if (avroRecord[map.AvroField] != null) 
                        {
                            if (!string.IsNullOrEmpty(map.Key))
                            {

                                var bag = (Dictionary<string, object>)avroRecord[map.AvroField];
                                object val;
                                if (bag.TryGetValue(map.Key, out val) == false)
                                    output.Set<object>(map.Key, null);
                                else
                                {
                                    switch (map.Type.ToLower())
                                    {
                                        case "int":
                                            output.Set(column.Name,(int)val); break;
                                        case "datetime":
                                            output.Set(column.Name, (DateTime)val); break;
                                        case "float":
                                            output.Set(column.Name, (float)val); break;
                                        case "string":
                                        default:
                                            output.Set(column.Name, val.ToString()); break;
                                    }
                                }
 
                            }
                            else
                                output.Set(column.Name, avroRecord[map.AvroField]);
                        }
                        else
                        {
                            output.Set<object>(column.Name, null);
                        }

                        yield return output.AsReadOnly();
                    }


                }

            }
        }


        private void CreateSeekableStream(IUnstructuredReader input, MemoryStream output)
        {
            input.BaseStream.CopyTo(output);
        }
    }
}