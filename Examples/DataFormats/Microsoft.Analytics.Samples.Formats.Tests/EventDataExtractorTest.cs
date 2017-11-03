using Avro;
using Avro.File;
using Avro.Generic;
using Avro.IO;
using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Samples.Formats.ApacheAvro;
using Microsoft.Analytics.UnitTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Analytics.Samples.Formats.Tests
{
    [TestClass]

    public class EventDataExtractorTest
    {

        private const string avroSchema = @"
{
            ""type"":""record"",
            ""name"":""EventData"",
            ""namespace"":""Microsoft.ServiceBus.Messaging"",
            ""fields"":[
                {""name"":""SequenceNumber"",""type"":""long""},
                {""name"":""Offset"",""type"":""string""},
                {""name"":""EnqueuedTimeUtc"",""type"":""string""},
                {""name"":""SystemProperties"",""type"":{""type"":""map"",""values"":[""long"",""double"",""string"",""bytes""]}},
                {""name"":""Properties"",""type"":{""type"":""map"",""values"":[""long"",""double"",""string"",""bytes""]}},
                {""name"":""Body"",""type"":[""null"",""bytes""]}
            ]
        }";

        private const string extractMap = @"
{
    ""SequenceNumber"": {""AvroField"": ""SequenceNumber""},    
    ""EnqueuedTimeUtc"": {""AvroField"": ""EnqueuedTimeUtc""},
    ""Body"": {""AvroField"": ""Body""},
    ""Route"": {""AvroField"": ""Properties"",""Key"": ""internal_source""}
}";





        [TestMethod]
        public void SchemaTest()
        {
            //
            var output = SingleColumnRowGenerator<string>("Route").AsUpdatable();

            var data = new List<EventDataPoco>
            {
                new EventDataPoco()
                {
                    Body = Encoding.UTF8.GetBytes("This is a test"),
                    EnqueuedTimeUtc = DateTime.UtcNow.ToString("s"),
                    Offset = Guid.NewGuid().ToString(),
                    Properties = new Dictionary<string, object>(){
                        { "internal_source", "mapping"}
                    },
                    SequenceNumber = 1000,
                    SystemProperties = new Dictionary<string, object>()
                }
            };

            var result = ExecuteExtract(data, output);

            Assert.IsTrue(result[0].Get<string>("Route") == "mapping");


        }


        private IList<IRow> ExecuteExtract(List<EventDataPoco> data, IUpdatableRow output)
        {

            using (var dataStream = new MemoryStream())
            {
                SerializeAvro(dataStream, data, avroSchema);

                var reader = new USqlStreamReader(dataStream);
                var extractor = new EventDataExtractor(avroSchema, extractMap);
                return extractor.Extract(reader, output).ToList();

            }
        }


        private void SerializeAvro(MemoryStream dataStream, List<EventDataPoco> data, string schema)
        {
            var avroSchema = Schema.Parse(schema);
            var writer = new GenericWriter<GenericRecord>(avroSchema);
            var fileWriter = DataFileWriter<GenericRecord>.OpenWriter(writer, dataStream);
            var encoder = new BinaryEncoder(dataStream);

            foreach (EventDataPoco record in data)
            {
                var genericRecord = new GenericRecord(avroSchema as RecordSchema);

                genericRecord.Add("Body", record.Body);
                genericRecord.Add("Offset", record.Offset);
                genericRecord.Add("Properties", record.Properties);
                genericRecord.Add("SystemProperties", record.SystemProperties);
                genericRecord.Add("EnqueuedTimeUtc", record.EnqueuedTimeUtc);
                genericRecord.Add("SequenceNumber", record.SequenceNumber);

                fileWriter.Append(genericRecord);
            }

            fileWriter.Flush();
            dataStream.Seek(0, SeekOrigin.Begin);
        }



        public IRow SingleColumnRowGenerator<T>(string name)
        {
            var foo = new USqlColumn<T>(name);
            var columns = new List<IColumn> { foo };
            var schema = new USqlSchema(columns);
            return new USqlRow(schema, null);
        }
    }
}
