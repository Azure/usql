﻿using Microsoft.Analytics.Interfaces;
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
using System.Reflection;

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
            var col1 = new USqlColumn<T>("Value");
            var columns = new List<IColumn> { col1 };
            var schema = new USqlSchema(columns);
            return new USqlRow(schema, null);
        }

        public IRow TwoColumnRowGenerator<T>()
        {
            var col1 = new USqlColumn<T>("Value1");
            var col2 = new USqlColumn<T>("Value2");
            var columns = new List<IColumn> { col1, col2 };
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
        public void AvroExtractor_EmptyFile_ReturnNoRow()
        {
            var schema = @"{""type"":""record"",""name"":""SingleColumnPoco"",""namespace"":""Microsoft.Analytics.Samples.Formats.Tests"",""fields"":[{""name"":""Value"",""type"":""string""}]}";
            var data = new List<SingleColumnPoco<string>>();

            var result = ExecuteExtract(data, schema);

            Assert.IsTrue(result.Count == 0);
        }

        [TestMethod]
        public void AvroExtractor_MultipleColumns_ReturnColumnsInOneRow()
        {
            var schema = @"{""type"":""record"",""name"":""TwoColumnPoco"",""namespace"":""Microsoft.Analytics.Samples.Formats.Tests"",""fields"":[{""name"":""Value1"",""type"":""string""},{""name"":""Value2"",""type"":""string""}]}";
            var data = new List<TwoColumnPoco<string>>()
            {
                new TwoColumnPoco<string>() { Value1 = "foo", Value2 = "bar" }
            };

            var result = ExecuteExtract(data, schema);

            Assert.IsTrue(result.Count == 1);
        }

        private IList<IRow> ExecuteExtract<T>(List<SingleColumnPoco<T>> data, string schema)
        {
            var output = SingleColumnRowGenerator<T>().AsUpdatable();

            using (var dataStream = new MemoryStream())
            {
                serializeAvro(dataStream, data, schema);

                var reader = new USqlStreamReader(dataStream);
                var extractor = new AvroExtractor(schema);
                return extractor.Extract(reader, output).ToList();
            }
        }

        private IList<IRow> ExecuteExtract<T>(List<TwoColumnPoco<T>> data, string schema)
        {
            var output = TwoColumnRowGenerator<T>().AsUpdatable();

            using (var dataStream = new MemoryStream())
            {
                serializeAvro(dataStream, data, schema);

                var reader = new USqlStreamReader(dataStream);
                var extractor = new AvroExtractor(schema);
                return extractor.Extract(reader, output).ToList();
            }
        }

        private void serializeAvro<T>(MemoryStream dataStream, List<T> data, string schema)
        {
            var avroSchema = Schema.Parse(schema);
            var writer = new GenericWriter<GenericRecord>(avroSchema);
            var fileWriter = DataFileWriter<GenericRecord>.OpenWriter(writer, dataStream);
            var encoder = new BinaryEncoder(dataStream);

            foreach (T record in data)
            {
                var genericRecord = new GenericRecord(avroSchema as RecordSchema);

                foreach (var prop in typeof(T).GetProperties())
                {
                    genericRecord.Add(prop.Name, prop.GetValue(record));
                }

                fileWriter.Append(genericRecord);
            }

            fileWriter.Flush();
            dataStream.Seek(0, SeekOrigin.Begin);
        }
    }
}