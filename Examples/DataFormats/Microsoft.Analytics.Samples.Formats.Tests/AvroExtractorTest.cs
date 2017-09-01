using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using Microsoft.Analytics.UnitTest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Analytics.Samples.Formats.Avro;
using Microsoft.Hadoop.Avro.Container;
using Microsoft.Hadoop.Avro;

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
            var output = SingleColumnRowGenerator<int>().AsUpdatable();
            var data = new List<SingleColumnPoco<int>>
            {
                new SingleColumnPoco<int>() { Value = 1 },
                new SingleColumnPoco<int>() { Value = 0 },
            };

            var result = ExecuteExtract(data);

            Assert.IsTrue(result[0].Get<int>("Value") == 1);
            Assert.IsTrue(result[1].Get<int>("Value") == 0);
        }

        [TestMethod]
        public void AvroExtractor_DatatypeNullableInt_Extracted()
        {
            var output = SingleColumnRowGenerator<int?>().AsUpdatable();

            var data = new List<SingleColumnPoco<int?>>
            {
                new SingleColumnPoco<int?>() { Value = 1 },
                new SingleColumnPoco<int?>() { Value = null }
            };

            var result = ExecuteExtract(data);

            Assert.IsTrue(result[0].Get<int?>("Value") == 1);
            Assert.IsTrue(result[1].Get<int?>("Value") == null);
        }

        [TestMethod]
        public void AvroExtractor_DatatypeBoolean_Extracted()
        {
            var data = new List<SingleColumnPoco<bool>>
            {
                new SingleColumnPoco<bool>() { Value = true },
                new SingleColumnPoco<bool>() { Value = false }
            };

            var result = ExecuteExtract(data);

            Assert.IsTrue(result[0].Get<bool>("Value") == true);
            Assert.IsTrue(result[1].Get<bool>("Value") == false);
        }

        [TestMethod]
        public void AvroExtractor_DatatypeNullableBoolean_Extracted()
        {
            var output = SingleColumnRowGenerator<bool?>().AsUpdatable();
            var data = new List<SingleColumnPoco<bool?>>
            {
                new SingleColumnPoco<bool?>() { Value = true },
                new SingleColumnPoco<bool?>() { Value = false },
                new SingleColumnPoco<bool?>() { Value = null }
            };

            var result = ExecuteExtract(data);

            Assert.IsTrue(result[0].Get<bool?>("Value") == true);
            Assert.IsTrue(result[1].Get<bool?>("Value") == false);
            Assert.IsTrue(result[2].Get<bool?>("Value") == null);
        }

        [TestMethod]
        public void AvroExtractor_DatatypeLong_Extracted()
        {
            var output = SingleColumnRowGenerator<long>().AsUpdatable();
            var data = new List<SingleColumnPoco<long>>
            {
                new SingleColumnPoco<long>() { Value = 9223372036854775807 },
                new SingleColumnPoco<long>() { Value = -9223372036854775807 }
            };

            var result = ExecuteExtract(data);

            Assert.IsTrue(result[0].Get<long>("Value") == 9223372036854775807);
            Assert.IsTrue(result[1].Get<long>("Value") == -9223372036854775807);
        }

        [TestMethod]
        public void AvroExtractor_DatatypeNullableLong_Extracted()
        {
            var output = SingleColumnRowGenerator<long?>().AsUpdatable();
            var data = new List<SingleColumnPoco<long?>>
            {
                new SingleColumnPoco<long?>() { Value = 9223372036854775807 },
                new SingleColumnPoco<long?>() { Value = -9223372036854775807 },
                new SingleColumnPoco<long?>() { Value = null }
            };

            var result = ExecuteExtract(data);

            Assert.IsTrue(result[0].Get<long?>("Value") == 9223372036854775807);
            Assert.IsTrue(result[1].Get<long?>("Value") == -9223372036854775807);
            Assert.IsTrue(result[2].Get<long?>("Value") == null);
        }

        [TestMethod]
        public void AvroExtractor_DatatypeFloat_Extracted()
        {
            var output = SingleColumnRowGenerator<float>().AsUpdatable();
            var data = new List<SingleColumnPoco<float>>
            {
                new SingleColumnPoco<float>() { Value = 3.5F},
                new SingleColumnPoco<float>() { Value = 0 }
            };

            var result = ExecuteExtract(data);

            Assert.IsTrue(result[0].Get<float>("Value") == 3.5F);
            Assert.IsTrue(result[1].Get<float>("Value") == 0);
        }

        [TestMethod]
        public void AvroExtractor_DatatypeNullableFloat_Extracted()
        {
            var output = SingleColumnRowGenerator<float?>().AsUpdatable();
            var data = new List<SingleColumnPoco<float?>>
            {
                new SingleColumnPoco<float?>() { Value = 3.5F},
                new SingleColumnPoco<float?>() { Value = 0 },
                new SingleColumnPoco<float?>() { Value = null }
            };

            var result = ExecuteExtract(data);

            Assert.IsTrue(result[0].Get<float?>("Value") == 3.5F);
            Assert.IsTrue(result[1].Get<float?>("Value") == 0);
            Assert.IsTrue(result[2].Get<float?>("Value") == null);
        }

        [TestMethod]
        public void AvroExtractor_DatatypeDouble_Extracted()
        {
            var output = SingleColumnRowGenerator<double>().AsUpdatable();
            var data = new List<SingleColumnPoco<double>>
            {
                new SingleColumnPoco<double>() { Value = 3D},
                new SingleColumnPoco<double>() { Value = 0 }
            };

            var result = ExecuteExtract(data);

            Assert.IsTrue(result[0].Get<double>("Value") == 3D);
            Assert.IsTrue(result[1].Get<double>("Value") == 0);
        }

        [TestMethod]
        public void AvroExtractor_DatatypeNullableDouble_Extracted()
        {
            var output = SingleColumnRowGenerator<double?>().AsUpdatable();
            var data = new List<SingleColumnPoco<double?>>
            {
                new SingleColumnPoco<double?>() { Value = 3D},
                new SingleColumnPoco<double?>() { Value = 0 },
                new SingleColumnPoco<double?>() { Value = null }
            };

            var result = ExecuteExtract(data);

            Assert.IsTrue(result[0].Get<double?>("Value") == 3D);
            Assert.IsTrue(result[1].Get<double?>("Value") == 0);
            Assert.IsTrue(result[2].Get<double?>("Value") == null);
        }

        [TestMethod]
        public void AvroExtractor_DatatypeByte_Extracted()
        {
            var output = SingleColumnRowGenerator<byte[]>().AsUpdatable();
            byte[] bytes = { 2, 4, 6 };
            var data = new List<SingleColumnPoco<byte[]>>
            {
                new SingleColumnPoco<byte[]>() { Value = bytes }
            };

            var result = ExecuteExtract(data);

            Assert.IsTrue(result[0].Get<byte[]>("Value")[0] == 2);
        }

        [TestMethod]
        public void AvroExtractor_DatatypeNullableByte_Extracted()
        {
            var output = SingleColumnRowGenerator<byte[]>().AsUpdatable();
            byte[] bytes = { 2, 4, 6 };
            var data = new List<SingleColumnPoco<byte[]>>
            {
                new SingleColumnPoco<byte[]>() { Value = bytes },
                new SingleColumnPoco<byte[]>() { Value = null }
            };

            var result = ExecuteExtract(data);

            Assert.IsTrue(result[0].Get<byte[]>("Value")[0] == 2);
            Assert.IsTrue(result[1].Get<byte[]>("Value") == null);
        }

        [TestMethod]
        public void AvroExtractor_DatatypeString_Extracted()
        {
            var output = SingleColumnRowGenerator<string>().AsUpdatable();
            var data = new List<SingleColumnPoco<string>>
            {
                new SingleColumnPoco<string>() { Value = "asdf" },
                new SingleColumnPoco<string>() { Value = "" }
            };

            var result = ExecuteExtract(data);

            Assert.IsTrue(result[0].Get<string>("Value") == "asdf");
            Assert.IsTrue(result[1].Get<string>("Value") == "");
        }

        [TestMethod]
        public void AvroExtractor_DatatypeNullableString_Extracted()
        {
            var output = SingleColumnRowGenerator<string>().AsUpdatable();

            var data = new List<SingleColumnPoco<string>>
            {
                new SingleColumnPoco<string>() { Value = "asdf" },
                new SingleColumnPoco<string>() { Value = null }
            };

            var result = ExecuteExtract(data);

            Assert.IsTrue(result[0].Get<string>("Value") == "asdf");
            Assert.IsTrue(result[1].Get<string>("Value") == null);
        }

        [TestMethod]
        public void AvroExtractor_EmptyFile_ReturnNoRow()
        {
            var output = SingleColumnRowGenerator<string>().AsUpdatable();

            var data = new List<SingleColumnPoco<string>>();

            var result = ExecuteExtract(data);

            Assert.IsTrue(result.Count == 0);
        }

        private IList<IRow> ExecuteExtract<T>(List<SingleColumnPoco<T>> data)
        {
            var output = SingleColumnRowGenerator<T>().AsUpdatable();

            using (var dataStream = new MemoryStream())
            {
                serializeAvro(dataStream, data);

                var schema = getAvroSchema<SingleColumnPoco<T>>();

                var reader = new USqlStreamReader(dataStream);
                var extractor = new AvroExtractor(schema);
                return extractor.Extract(reader, output).ToList();
            }
        }

        private void serializeAvro<T>(MemoryStream dataStream, List<T> data)
        {
            using (var avroWriter = AvroContainer.CreateWriter<T>(dataStream, Codec.Deflate))
            {
                using (var seqWriter = new SequentialWriter<T>(avroWriter, 24))
                {
                    data.ForEach(seqWriter.Write);
                }
            }
        }

        private string getAvroSchema<T>()
        {
            var avroSerializer = AvroSerializer.Create<T>();
            return avroSerializer.ReaderSchema.ToString();
        }
    }
}