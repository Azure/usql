using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using Microsoft.Analytics.UnitTest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Analytics.Samples.Formats.ApacheAvro;
using Avro.Generic;
using Avro;
using Avro.File;
using Avro.IO;

namespace Microsoft.Analytics.Samples.Formats.Tests
{
    [TestClass]
    public class AvroExtractorTest
    {
        public AvroExtractorTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        public IRow SingleColumnRowGenerator<T>()
        {
            var foo = new USqlColumn<T>("Value");
            var columns = new List<IColumn> { foo };
            var schema = new USqlSchema(columns);
            return new USqlRow(schema, null);
        }

        [TestMethod]
        public void AvroExtractor_DatatypeInt_Extracted()
        {
            var schema = @"{""type"":""record"",""name"":""SingleColumnPoco"",""namespace"":""Microsoft.Analytics.Samples.Formats.Tests"",""fields"":[{""name"":""Value"",""type"":""int""}]}";
            var data = new List<SingleColumnPoco<int>>
            {
                new SingleColumnPoco<int>() { Value = 1 },
                new SingleColumnPoco<int>() { Value = 0 },
            };

            var result = ExecuteExtract(data, schema);

            Assert.IsTrue(result[0].Get<int>("Value") == 1);
            Assert.IsTrue(result[1].Get<int>("Value") == 0);
        }

        [TestMethod]
        public void AvroExtractor_DatatypeNullableInt_Extracted()
        {
            var schema = @"{""type"":""record"",""name"":""SingleColumnPoco"",""namespace"":""Microsoft.Analytics.Samples.Formats.Tests"",""fields"":[{""name"":""Value"",""type"":[""null"",""int""]}]}";
            var data = new List<SingleColumnPoco<int?>>
            {
                new SingleColumnPoco<int?>() { Value = 1 },
                new SingleColumnPoco<int?>() { Value = null }
            };

            var result = ExecuteExtract(data, schema);

            Assert.IsTrue(result[0].Get<int?>("Value") == 1);
            Assert.IsTrue(result[1].Get<int?>("Value") == null);
        }

        [TestMethod]
        public void AvroExtractor_DatatypeBoolean_Extracted()
        {
            var schema = @"{""type"":""record"",""name"":""SingleColumnPoco"",""namespace"":""Microsoft.Analytics.Samples.Formats.Tests"",""fields"":[{""name"":""Value"",""type"":""boolean""}]}";
            var data = new List<SingleColumnPoco<bool>>
            {
                new SingleColumnPoco<bool>() { Value = true },
                new SingleColumnPoco<bool>() { Value = false }
            };

            var result = ExecuteExtract(data, schema);

            Assert.IsTrue(result[0].Get<bool>("Value") == true);
            Assert.IsTrue(result[1].Get<bool>("Value") == false);
        }

        [TestMethod]
        public void AvroExtractor_DatatypeNullableBoolean_Extracted()
        {
            var schema = @"{""type"":""record"",""name"":""SingleColumnPoco"",""namespace"":""Microsoft.Analytics.Samples.Formats.Tests"",""fields"":[{""name"":""Value"",""type"":[""null"",""boolean""]}]}";
            var data = new List<SingleColumnPoco<bool?>>
            {
                new SingleColumnPoco<bool?>() { Value = true },
                new SingleColumnPoco<bool?>() { Value = false },
                new SingleColumnPoco<bool?>() { Value = null }
            };

            var result = ExecuteExtract(data, schema);

            Assert.IsTrue(result[0].Get<bool?>("Value") == true);
            Assert.IsTrue(result[1].Get<bool?>("Value") == false);
            Assert.IsTrue(result[2].Get<bool?>("Value") == null);
        }

        [TestMethod]
        public void AvroExtractor_DatatypeLong_Extracted()
        {
            var schema = @"{""type"":""record"",""name"":""SingleColumnPoco"",""namespace"":""Microsoft.Analytics.Samples.Formats.Tests"",""fields"":[{""name"":""Value"",""type"":""long""}]}";
            var data = new List<SingleColumnPoco<long>>
            {
                new SingleColumnPoco<long>() { Value = 9223372036854775807 },
                new SingleColumnPoco<long>() { Value = -9223372036854775807 }
            };

            var result = ExecuteExtract(data, schema);

            Assert.IsTrue(result[0].Get<long>("Value") == 9223372036854775807);
            Assert.IsTrue(result[1].Get<long>("Value") == -9223372036854775807);
        }

        [TestMethod]
        public void AvroExtractor_DatatypeNullableLong_Extracted()
        {
            var schema = @"{""type"":""record"",""name"":""SingleColumnPoco"",""namespace"":""Microsoft.Analytics.Samples.Formats.Tests"",""fields"":[{""name"":""Value"",""type"":[""null"",""long""]}]}";
            var data = new List<SingleColumnPoco<long?>>
            {
                new SingleColumnPoco<long?>() { Value = 9223372036854775807 },
                new SingleColumnPoco<long?>() { Value = -9223372036854775807 },
                new SingleColumnPoco<long?>() { Value = null }
            };

            var result = ExecuteExtract(data, schema);

            Assert.IsTrue(result[0].Get<long?>("Value") == 9223372036854775807);
            Assert.IsTrue(result[1].Get<long?>("Value") == -9223372036854775807);
            Assert.IsTrue(result[2].Get<long?>("Value") == null);
        }

        [TestMethod]
        public void AvroExtractor_DatatypeFloat_Extracted()
        {
            var schema = @"{""type"":""record"",""name"":""SingleColumnPoco"",""namespace"":""Microsoft.Analytics.Samples.Formats.Tests"",""fields"":[{""name"":""Value"",""type"":""float""}]}";
            var data = new List<SingleColumnPoco<float>>
            {
                new SingleColumnPoco<float>() { Value = 3.5F},
                new SingleColumnPoco<float>() { Value = 0 }
            };

            var result = ExecuteExtract(data, schema);

            Assert.IsTrue(result[0].Get<float>("Value") == 3.5F);
            Assert.IsTrue(result[1].Get<float>("Value") == 0);
        }

        [TestMethod]
        public void AvroExtractor_DatatypeNullableFloat_Extracted()
        {
            var schema = @"{""type"":""record"",""name"":""SingleColumnPoco"",""namespace"":""Microsoft.Analytics.Samples.Formats.Tests"",""fields"":[{""name"":""Value"",""type"":[""null"",""float""]}]}";
            var data = new List<SingleColumnPoco<float?>>
            {
                new SingleColumnPoco<float?>() { Value = 3.5F},
                new SingleColumnPoco<float?>() { Value = 0 },
                new SingleColumnPoco<float?>() { Value = null }
            };

            var result = ExecuteExtract(data, schema);

            Assert.IsTrue(result[0].Get<float?>("Value") == 3.5F);
            Assert.IsTrue(result[1].Get<float?>("Value") == 0);
            Assert.IsTrue(result[2].Get<float?>("Value") == null);
        }

        [TestMethod]
        public void AvroExtractor_DatatypeDouble_Extracted()
        {
            var schema = @"{""type"":""record"",""name"":""SingleColumnPoco"",""namespace"":""Microsoft.Analytics.Samples.Formats.Tests"",""fields"":[{""name"":""Value"",""type"":""double""}]}";
            var data = new List<SingleColumnPoco<double>>
            {
                new SingleColumnPoco<double>() { Value = 3D},
                new SingleColumnPoco<double>() { Value = 0 }
            };

            var result = ExecuteExtract(data, schema);

            Assert.IsTrue(result[0].Get<double>("Value") == 3D);
            Assert.IsTrue(result[1].Get<double>("Value") == 0);
        }

        [TestMethod]
        public void AvroExtractor_DatatypeNullableDouble_Extracted()
        {
            var schema = @"{""type"":""record"",""name"":""SingleColumnPoco"",""namespace"":""Microsoft.Analytics.Samples.Formats.Tests"",""fields"":[{""name"":""Value"",""type"":[""null"",""double""]}]}";
            var data = new List<SingleColumnPoco<double?>>
            {
                new SingleColumnPoco<double?>() { Value = 3D},
                new SingleColumnPoco<double?>() { Value = 0 },
                new SingleColumnPoco<double?>() { Value = null }
            };

            var result = ExecuteExtract(data, schema);

            Assert.IsTrue(result[0].Get<double?>("Value") == 3D);
            Assert.IsTrue(result[1].Get<double?>("Value") == 0);
            Assert.IsTrue(result[2].Get<double?>("Value") == null);
        }

        [TestMethod]
        public void AvroExtractor_DatatypeByte_Extracted()
        {
            var schema = @"{""type"":""record"",""name"":""SingleColumnPoco"",""namespace"":""Microsoft.Analytics.Samples.Formats.Tests"",""fields"":[{""name"":""Value"",""type"":""bytes""}]}";
            byte[] bytes = { 2, 4, 6 };
            var data = new List<SingleColumnPoco<byte[]>>
            {
                new SingleColumnPoco<byte[]>() { Value = bytes }
            };

            var result = ExecuteExtract(data, schema);

            Assert.IsTrue(result[0].Get<byte[]>("Value")[0] == 2);
        }

        [TestMethod]
        public void AvroExtractor_DatatypeNullableByte_Extracted()
        {
            var schema = @"{""type"":""record"",""name"":""SingleColumnPoco"",""namespace"":""Microsoft.Analytics.Samples.Formats.Tests"",""fields"":[{""name"":""Value"",""type"":[""null"",""bytes""]}]}";
            byte[] bytes = { 2, 4, 6 };
            var data = new List<SingleColumnPoco<byte[]>>
            {
                new SingleColumnPoco<byte[]>() { Value = bytes },
                new SingleColumnPoco<byte[]>() { Value = null }
            };

            var result = ExecuteExtract(data, schema);

            Assert.IsTrue(result[0].Get<byte[]>("Value")[0] == 2);
            Assert.IsTrue(result[1].Get<byte[]>("Value") == null);
        }

        [TestMethod]
        public void AvroExtractor_DatatypeString_Extracted()
        {
            var schema = @"{""type"":""record"",""name"":""SingleColumnPoco"",""namespace"":""Microsoft.Analytics.Samples.Formats.Tests"",""fields"":[{""name"":""Value"",""type"":""string""}]}";
            var data = new List<SingleColumnPoco<string>>
            {
                new SingleColumnPoco<string>() { Value = "asdf" },
                new SingleColumnPoco<string>() { Value = "" }
            };

            var result = ExecuteExtract(data, schema);

            Assert.IsTrue(result[0].Get<string>("Value") == "asdf");
            Assert.IsTrue(result[1].Get<string>("Value") == "");
        }

        [TestMethod]
        public void AvroExtractor_DatatypeNullableString_Extracted()
        {
            var schema = @"{""type"":""record"",""name"":""SingleColumnPoco"",""namespace"":""Microsoft.Analytics.Samples.Formats.Tests"",""fields"":[{""name"":""Value"",""type"":[""null"",""string""]}]}";
            var data = new List<SingleColumnPoco<string>>
            {
                new SingleColumnPoco<string>() { Value = "asdf" },
                new SingleColumnPoco<string>() { Value = null }
            };

            var result = ExecuteExtract(data, schema);

            Assert.IsTrue(result[0].Get<string>("Value") == "asdf");
            Assert.IsTrue(result[1].Get<string>("Value") == null);
        }

        [TestMethod]
        public void AvroExtractor_DatatypeMap_MapOfInt_Extracted()
        {
            var schema = @"{""fields"":[{""name"": ""Value"",""type"": { ""type"": ""map"",""values"": ""int""}}],""name"": ""SingleColumnPoco"",""namespace"": ""Microsoft.Analytics.Samples.Formats.Tests"",""type"": ""record""}";
            var dict = new Dictionary<string, object> { { "item1", 1 } };
            var data = new List<SingleColumnPoco<Dictionary<string, object>>>
            {
                new SingleColumnPoco<Dictionary<string, object>>() { Value = dict }
            };

            var result = ExecuteExtract<Dictionary<string, object>, SqlMap<string, int?>>(data, schema);

            Assert.IsNotNull(result[0].Get<SqlMap<string, int?>>("Value"));
            Assert.IsTrue(result[0].Get<SqlMap<string, int?>>("Value").Values[0] == 1);
        }

        [TestMethod]
        public void AvroExtractor_DatatypeMap_MapOfBool_Extracted()
        {
            var schema = @"{""fields"":[{""name"": ""Value"",""type"": { ""type"": ""map"",""values"": ""boolean""}}],""name"": ""SingleColumnPoco"",""namespace"": ""Microsoft.Analytics.Samples.Formats.Tests"",""type"": ""record""}";
            var dict = new Dictionary<string, object> { { "item1", true } };
            var data = new List<SingleColumnPoco<Dictionary<string, object>>>
            {
                new SingleColumnPoco<Dictionary<string, object>>() { Value = dict }
            };

            var result = ExecuteExtract<Dictionary<string, object>, SqlMap<string, bool?>>(data, schema);

            Assert.IsNotNull(result[0].Get<SqlMap<string, bool?>>("Value"));
            Assert.IsTrue(result[0].Get<SqlMap<string, bool?>>("Value").Values[0] == true);
        }

        [TestMethod]
        public void AvroExtractor_DatatypeMap_MapOfLong_Extracted()
        {
            var schema = @"{""fields"":[{""name"": ""Value"",""type"": { ""type"": ""map"",""values"": ""long""}}],""name"": ""SingleColumnPoco"",""namespace"": ""Microsoft.Analytics.Samples.Formats.Tests"",""type"": ""record""}";
            var dict = new Dictionary<string, object> { { "item1", 0x7FFFFFFFFFFFFFFF } };
            var data = new List<SingleColumnPoco<Dictionary<string, object>>>
            {
                new SingleColumnPoco<Dictionary<string, object>>() { Value = dict }
            };

            var result = ExecuteExtract<Dictionary<string, object>, SqlMap<string, long?>>(data, schema);

            Assert.IsNotNull(result[0].Get<SqlMap<string, long?>>("Value"));
            Assert.IsTrue(result[0].Get<SqlMap<string, long?>>("Value").Values[0] == 0x7FFFFFFFFFFFFFFF);
        }

        [TestMethod]
        public void AvroExtractor_DatatypeMap_MapOfDouble_Extracted()
        {
            var schema = @"{""fields"":[{""name"": ""Value"",""type"": { ""type"": ""map"",""values"": ""double""}}],""name"": ""SingleColumnPoco"",""namespace"": ""Microsoft.Analytics.Samples.Formats.Tests"",""type"": ""record""}";
            var dict = new Dictionary<string, object> { { "item1", 3D } };
            var data = new List<SingleColumnPoco<Dictionary<string, object>>>
            {
                new SingleColumnPoco<Dictionary<string, object>>() { Value = dict }
            };

            var result = ExecuteExtract<Dictionary<string, object>, SqlMap<string, double?>>(data, schema);

            Assert.IsNotNull(result[0].Get<SqlMap<string, double?>>("Value"));
            Assert.IsTrue(result[0].Get<SqlMap<string, double?>>("Value").Values[0] == 3D);
        }

        [TestMethod]
        public void AvroExtractor_DatatypeMap_MapOfString_Extracted()
        {
            var schema = @"{""fields"":[{""name"": ""Value"",""type"": { ""type"": ""map"",""values"": ""string""}}],""name"": ""SingleColumnPoco"",""namespace"": ""Microsoft.Analytics.Samples.Formats.Tests"",""type"": ""record""}";
            var dict = new Dictionary<string, object> { { "item1", "value1" } };
            var data = new List<SingleColumnPoco<Dictionary<string, object>>>
            {
                new SingleColumnPoco<Dictionary<string, object>>() { Value = dict }
            };

            var result = ExecuteExtract<Dictionary<string, object>, SqlMap<string, string>>(data, schema);

            Assert.IsNotNull(result[0].Get<SqlMap<string, string>>("Value"));
            Assert.IsTrue(result[0].Get<SqlMap<string, string>>("Value").Values[0] == "value1");
        }

        [TestMethod]
        public void AvroExtractor_DatatypeMap_MapOfFloat_Extracted()
        {
            var schema = @"{""fields"":[{""name"": ""Value"",""type"": { ""type"": ""map"",""values"": ""float""}}],""name"": ""SingleColumnPoco"",""namespace"": ""Microsoft.Analytics.Samples.Formats.Tests"",""type"": ""record""}";
            var dict = new Dictionary<string, object> { { "item1", 3.5F } };
            var data = new List<SingleColumnPoco<Dictionary<string, object>>>
            {
                new SingleColumnPoco<Dictionary<string, object>>() { Value = dict }
            };

            var result = ExecuteExtract<Dictionary<string, object>, SqlMap<string, float?>>(data, schema);

            Assert.IsNotNull(result[0].Get<SqlMap<string, float?>>("Value"));
            Assert.IsTrue(result[0].Get<SqlMap<string, float?>>("Value").Values[0] == 3.5F);
        }

        [TestMethod]
        public void AvroExtractor_DatatypeMap_MapOfByte_Extracted()
        {
            var schema = @"{""fields"":[{""name"": ""Value"",""type"": { ""type"": ""map"",""values"": ""bytes""}}],""name"": ""SingleColumnPoco"",""namespace"": ""Microsoft.Analytics.Samples.Formats.Tests"",""type"": ""record""}";
            byte[] bytes = { 2, 4, 6 };
            var dict = new Dictionary<string, object> { { "item1", bytes } };
            var data = new List<SingleColumnPoco<Dictionary<string, object>>>
            {
                new SingleColumnPoco<Dictionary<string, object>>() { Value = dict }
            };

            var result = ExecuteExtract<Dictionary<string, object>, SqlMap<string, byte[]>>(data, schema);

            Assert.IsNotNull(result[0].Get<SqlMap<string, byte[]>>("Value"));
            Assert.IsTrue(result[0].Get<SqlMap<string, byte[]>>("Value").Values[0].SequenceEqual(bytes));
        }

        [TestMethod]
        public void AvroExtractor_DatatypeMap_MapOfInt_EmptyMap_Extracted()
        {
            var schema = @"{""fields"":[{""name"": ""Value"",""type"": { ""type"": ""map"",""values"": [""int"",""null""]}}],""name"": ""SingleColumnPoco"",""namespace"": ""Microsoft.Analytics.Samples.Formats.Tests"",""type"": ""record""}";
            var dict = new Dictionary<string, object>();
            var data = new List<SingleColumnPoco<Dictionary<string, object>>>
            {
                new SingleColumnPoco<Dictionary<string, object>>() { Value = dict }
            };

            var result = ExecuteExtract<Dictionary<string, object>, SqlMap<string, int?>>(data, schema);

            Assert.IsNotNull(result[0].Get<SqlMap<string, int?>>("Value"));
            Assert.IsTrue(result[0].Get<SqlMap<string, int?>>("Value").Values.Count == 0);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Unsupported datatype for SQL.MAP.")]
        public void AvroExtractor_DatatypeMap_MapOfUnspportedType_Exception()
        {
            var schema = @"{""fields"":[{""name"": ""Value"",""type"": { ""type"": ""map"",""values"": ""int""}}],""name"": ""SingleColumnPoco"",""namespace"": ""Microsoft.Analytics.Samples.Formats.Tests"",""type"": ""record""}";
            var dict = new Dictionary<string, object> { { "item1", 1 } };
            var data = new List<SingleColumnPoco<Dictionary<string, object>>>
            {
                new SingleColumnPoco<Dictionary<string, object>>() { Value = dict }
            };

            var result = ExecuteExtract<Dictionary<string, object>, SqlMap<string, int[]>>(data, schema);

            Assert.IsNotNull(result[0].Get<SqlMap<string, int>>("Value"));
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "")]
        public void AvroExtractor_DatatypeMap_DatatypeMismatch_Exception()
        {
            var schema = @"{""fields"":[{""name"": ""Value"",""type"": { ""type"": ""map"",""values"": ""string""}}],""name"": ""SingleColumnPoco"",""namespace"": ""Microsoft.Analytics.Samples.Formats.Tests"",""type"": ""record""}";
            var dict = new Dictionary<string, object> { { "item1", "asdf" } };
            var data = new List<SingleColumnPoco<Dictionary<string, object>>>
            {
                new SingleColumnPoco<Dictionary<string, object>>() { Value = dict }
            };

            var result = ExecuteExtract<Dictionary<string, object>, SqlMap<string, int?>>(data, schema);
        }

        [TestMethod]
        public void AvroExtractor_EmptyFile_ReturnNoRow()
        {
            var schema = @"{""type"":""record"",""name"":""SingleColumnPoco"",""namespace"":""Microsoft.Analytics.Samples.Formats.Tests"",""fields"":[{""name"":""Value"",""type"":""string""}]}";
            var data = new List<SingleColumnPoco<string>>();

            var result = ExecuteExtract(data, schema);

            Assert.IsTrue(result.Count == 0);
        }

        private IList<IRow> ExecuteExtract<T>(List<SingleColumnPoco<T>> data, string schema)
        {
            return ExecuteExtract<T,T>(data, schema);
        }

        private IList<IRow> ExecuteExtract<T,O>(List<SingleColumnPoco<T>> data, string schema)
        {
            var output = SingleColumnRowGenerator<O>().AsUpdatable();

            using (var dataStream = new MemoryStream())
            {
                serializeAvro(dataStream, data, schema);

                var reader = new USqlStreamReader(dataStream);
                var extractor = new AvroExtractor(schema);
                return extractor.Extract(reader, output).ToList();
            }
        }

        private void serializeAvro<T>(MemoryStream dataStream, List<SingleColumnPoco<T>> data, string schema)
        {
            var avroSchema = Schema.Parse(schema);
            var writer = new GenericWriter<GenericRecord>(avroSchema);
            var fileWriter = DataFileWriter<GenericRecord>.OpenWriter(writer, dataStream);
            var encoder = new BinaryEncoder(dataStream);

            foreach (SingleColumnPoco<T> record in data)
            {
                var genericRecord = new GenericRecord(avroSchema as RecordSchema);

                genericRecord.Add("Value", record.Value);

                fileWriter.Append(genericRecord);
            }

            fileWriter.Flush();
            dataStream.Seek(0, SeekOrigin.Begin);
        }
    }
}