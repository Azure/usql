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
using Microsoft.Hadoop.Avro;
using Microsoft.Hadoop.Avro.Container;

namespace Microsoft.Analytics.Samples.Formats.Avro
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
            var serializer = AvroSerializer.CreateGeneric(avroSchema);
            using (var genericReader = AvroContainer.CreateGenericReader(input.BaseStream))
            {
                using (var reader = new SequentialReader<dynamic>(genericReader))
                { 
                    foreach (var obj in reader.Objects)
                    {
                        foreach (var column in output.Schema)
                        {                            
                            output.Set(column.Name, obj[column.Name]);
                        }         

                        yield return output.AsReadOnly();
                    }
                }
            }
        }
    }
}
