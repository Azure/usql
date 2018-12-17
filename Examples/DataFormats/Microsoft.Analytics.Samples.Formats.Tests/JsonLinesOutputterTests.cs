using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Analytics.Samples.Formats.Json;
using System.IO;
using Microsoft.Analytics.UnitTest;
using Microsoft.Analytics.Interfaces;
using System.Collections.Generic;
using System.Text;
using Microsoft.Analytics.Types.Sql;

namespace Microsoft.Analytics.Samples.Formats.Tests
{
    [TestClass]
    public class JsonLinesOutputterTests
    {
        [TestMethod]
        public void JsonOutputter_DatatypeShort_Outputted()
        {
            USqlSchema schema = new USqlSchema(
                new USqlColumn<short>("a"),
                new USqlColumn<short>("b")
            );
            short a = 0;
            short b = 1;
            object[] values = new object[2] { a, b };
            var row = new USqlRow(schema, values);

            var expected = "{\"a\":0,\"b\":1}";
            var actual = GetOutputterResult(row);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void JsonOutputter_DatatypeNullableShort_Outputted()
        {
            USqlSchema schema = new USqlSchema(
                new USqlColumn<short?>("a"),
                new USqlColumn<short?>("b"),
                new USqlColumn<short?>("c")
            );
            short a = 0;
            short b = 1;
            object[] values = new object[3] { a, b, null };
            var row = new USqlRow(schema, values);

            var expected = "{\"a\":0,\"b\":1}";
            var actual = GetOutputterResult(row);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void JsonOutputter_DatatypeInt_Outputted()
        {
            USqlSchema schema = new USqlSchema(
                new USqlColumn<int>("a"),
                new USqlColumn<int>("b")
            );
            object[] values = new object[2] { 0, 1 };
            var row = new USqlRow(schema, values);

            var expected = "{\"a\":0,\"b\":1}";
            var actual = GetOutputterResult(row);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void JsonOutputter_DatatypeNullableInt_Outputted()
        {
            USqlSchema schema = new USqlSchema(
                new USqlColumn<int?>("a"),
                new USqlColumn<int?>("b"),
                new USqlColumn<int?>("c")
            );
            object[] values = new object[3] { 0, 1, null };
            var row = new USqlRow(schema, values);

            var expected = "{\"a\":0,\"b\":1}";
            var actual = GetOutputterResult(row);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void JsonOutputter_DatatypeLong_Outputted()
        {
            USqlSchema schema = new USqlSchema(
                new USqlColumn<long>("a"),
                new USqlColumn<long>("b")
            );
            object[] values = new object[2] { 9223372036854775807, -9223372036854775807 };
            var row = new USqlRow(schema, values);

            var expected = "{\"a\":9223372036854775807,\"b\":-9223372036854775807}";
            var actual = GetOutputterResult(row);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void JsonOutputter_DatatypeNullableLong_Outputted()
        {
            USqlSchema schema = new USqlSchema(
                new USqlColumn<long?>("a"),
                new USqlColumn<long?>("b"),
                new USqlColumn<long?>("c")
            );
            object[] values = new object[3] { 9223372036854775807, -9223372036854775807, null };
            var row = new USqlRow(schema, values);

            var expected = "{\"a\":9223372036854775807,\"b\":-9223372036854775807}";
            var actual = GetOutputterResult(row);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void JsonOutputter_DatatypeFloat_Outputted()
        {
            USqlSchema schema = new USqlSchema(
                new USqlColumn<float>("a"),
                new USqlColumn<float>("b")
            );
            object[] values = new object[2] { 3.5F, 0F };
            var row = new USqlRow(schema, values);

            var expected = "{\"a\":3.5,\"b\":0.0}";
            var actual = GetOutputterResult(row);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void JsonOutputter_DatatypeNullableFloat_Outputted()
        {
            USqlSchema schema = new USqlSchema(
                new USqlColumn<float?>("a"),
                new USqlColumn<float?>("b"),
                new USqlColumn<float?>("c")
            );
            object[] values = new object[3] { 3.5F, 0F, null };
            var row = new USqlRow(schema, values);

            var expected = "{\"a\":3.5,\"b\":0.0}";
            var actual = GetOutputterResult(row);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void JsonOutputter_DatatypeDouble_Outputted()
        {
            USqlSchema schema = new USqlSchema(
                new USqlColumn<double>("a"),
                new USqlColumn<double>("b")
            );
            object[] values = new object[2] { 3.5D, 0D };
            var row = new USqlRow(schema, values);

            var expected = "{\"a\":3.5,\"b\":0.0}";
            var actual = GetOutputterResult(row);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void JsonOutputter_DatatypeNullableDouble_Outputted()
        {
            USqlSchema schema = new USqlSchema(
                new USqlColumn<double?>("a"),
                new USqlColumn<double?>("b"),
                new USqlColumn<double?>("c")
            );
            object[] values = new object[3] { 3.5, 0D, null };
            var row = new USqlRow(schema, values);

            var expected = "{\"a\":3.5,\"b\":0.0}";
            var actual = GetOutputterResult(row);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void JsonOutputter_DatatypeDecimal_Outputted()
        {
            USqlSchema schema = new USqlSchema(
                new USqlColumn<decimal>("a"),
                new USqlColumn<decimal>("b")
            );
            object[] values = new object[2] { 350.5M, 0M };
            var row = new USqlRow(schema, values);

            var expected = "{\"a\":350.5,\"b\":0.0}";
            var actual = GetOutputterResult(row);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void JsonOutputter_DatatypeNullableDecimal_Outputted()
        {
            USqlSchema schema = new USqlSchema(
                new USqlColumn<decimal?>("a"),
                new USqlColumn<decimal?>("b"),
                new USqlColumn<decimal?>("c")
            );
            object[] values = new object[3] { 350.5M, 0M, null };
            var row = new USqlRow(schema, values);

            var expected = "{\"a\":350.5,\"b\":0.0}";
            var actual = GetOutputterResult(row);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void JsonOutputter_DatatypeByte_Outputted()
        {
            USqlSchema schema = new USqlSchema(
                new USqlColumn<byte>("a"),
                new USqlColumn<byte>("b")
            );

            byte a = 2;
            byte b = 4;

            object[] values = new object[2] { a, b };
            var row = new USqlRow(schema, values);

            var expected = "{\"a\":2,\"b\":4}";
            var actual = GetOutputterResult(row);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void JsonOutputter_DatatypeNullableBytes_Outputted()
        {
            USqlSchema schema = new USqlSchema(
                new USqlColumn<byte?>("a"),
                new USqlColumn<byte?>("b")
            );

            byte? a = 2;
            byte? b = null;

            object[] values = new object[2] { a, b };
            var row = new USqlRow(schema, values);

            var expected = "{\"a\":2}";
            var actual = GetOutputterResult(row);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void JsonOutputter_DatatypeBoolean_Outputted()
        {
            USqlSchema schema = new USqlSchema(
                new USqlColumn<bool>("a"),
                new USqlColumn<bool>("b")
            );
            object[] values = new object[2] { true, false };
            var row = new USqlRow(schema, values);

            var expected = "{\"a\":true,\"b\":false}";
            var actual = GetOutputterResult(row);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void JsonOutputter_DatatypeNullableBoolean_Outputted()
        {
            USqlSchema schema = new USqlSchema(
                new USqlColumn<bool?>("a"),
                new USqlColumn<bool?>("b"),
                new USqlColumn<bool?>("c")
            );
            object[] values = new object[3] { true, false, null };
            var row = new USqlRow(schema, values);

            var expected = "{\"a\":true,\"b\":false}";
            var actual = GetOutputterResult(row);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void JsonOutputter_DatatypeString_Outputted()
        {
            USqlSchema schema = new USqlSchema(
                new USqlColumn<string>("a"),
                new USqlColumn<string>("b"),
                new USqlColumn<string>("c")
            );
            object[] values = new object[3] { "test", "", null };
            var row = new USqlRow(schema, values);

            var expected = "{\"a\":\"test\",\"b\":\"\"}";
            var actual = GetOutputterResult(row);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void JsonOutputter_DatatypeChar_Outputted()
        {
            USqlSchema schema = new USqlSchema(
                new USqlColumn<char>("a"),
                new USqlColumn<char>("b")
            );
            object[] values = new object[2] { 'a', ' ' };
            var row = new USqlRow(schema, values);

            var expected = "{\"a\":\"a\",\"b\":\" \"}";
            var actual = GetOutputterResult(row);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void JsonOutputter_DatatypeNullableChar_Outputted()
        {
            USqlSchema schema = new USqlSchema(
                new USqlColumn<char?>("a"),
                new USqlColumn<char?>("b"),
                new USqlColumn<char?>("c")
            );
            object[] values = new object[3] { 'a', ' ', null };
            var row = new USqlRow(schema, values);

            var expected = "{\"a\":\"a\",\"b\":\" \"}";
            var actual = GetOutputterResult(row);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void JsonOutputter_DatatypeDateTime_Outputted()
        {
            USqlSchema schema = new USqlSchema(
                new USqlColumn<DateTime>("a")
            );
            object[] values = new object[1] { new DateTime(2010, 01, 05) };
            var row = new USqlRow(schema, values);

            var expected = "{\"a\":\"2010-01-05T00:00:00\"}";
            var actual = GetOutputterResult(row);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void JsonOutputter_DatatypeNullableDateTime_Outputted()
        {
            USqlSchema schema = new USqlSchema(
                new USqlColumn<DateTime?>("a"),
                new USqlColumn<DateTime?>("b")
            );
            object[] values = new object[2] { new DateTime(2010, 01, 05), null };
            var row = new USqlRow(schema, values);

            var expected = "{\"a\":\"2010-01-05T00:00:00\"}";
            var actual = GetOutputterResult(row);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void JsonOutputter_DatatypeArrayOfPrimitiveTypes_Outputted()
        {
            USqlSchema schema = new USqlSchema(
                new USqlColumn<short?[]>("a"),
                new USqlColumn<int?[]>("b"),
                new USqlColumn<long?[]>("c"),
                new USqlColumn<float?[]>("d"),
                new USqlColumn<double?[]>("e"),
                new USqlColumn<decimal?[]>("f"),
                new USqlColumn<byte?[]>("g"),
                new USqlColumn<bool?[]>("h"),
                new USqlColumn<string[]>("i"),
                new USqlColumn<char?[]>("j"),
                new USqlColumn<DateTime?[]>("k")
            );
            object[] values = new object[11] {
                new short?[3] { 0, 1, null },
                new int?[3] { 0, 1, null },
                new long?[3] { 9223372036854775807, -9223372036854775807, null },
                new float?[3] { 3.5F, 0F, null },
                new double?[3] { 3.5D, 0D, null },
                new decimal?[3] { 205.2M, 0M, null },
                new byte?[3] { 2, 4, null },
                new bool?[3] { true, false, null },
                new string[3] { "test", "", null },
                new char?[3] { 'a', ' ', null },
                new DateTime?[3] { new DateTime(2010, 01, 05), new DateTime(2015, 05, 06), null }
            };

            var row = new USqlRow(schema, values);

            var expected = "{" +
                "\"a\":[0,1]," +
                "\"b\":[0,1]," +
                "\"c\":[9223372036854775807,-9223372036854775807]," +
                "\"d\":[3.5,0.0]," +
                "\"e\":[3.5,0.0]," +
                "\"f\":[205.2,0.0]," +
                "\"g\":[2,4]," +
                "\"h\":[true,false]," +
                "\"i\":[\"test\",\"\"]," +
                "\"j\":[\"a\",\" \"]," +
                "\"k\":[\"2010-01-05T00:00:00\",\"2015-05-06T00:00:00\"]" +
                "}";
            var actual = GetOutputterResult(row);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void JsonOutputter_DatatypeDictionaryOfPrimitiveTypes_Outputted()
        {
            USqlSchema schema = new USqlSchema(
                new USqlColumn<Dictionary<string, object>>("a")
            );
            var dict = new Dictionary<string, object>
            {
                { "short", (short)3 },
                { "int", 3 },
                { "long", 9223372036854775807 },
                { "float", 3.5F },
                { "double", 3.5D },
                { "decimal", 205.2M },
                { "byte", (byte)3 },
                { "bool", true },
                { "string", "test" },
                { "char", 'a' },
                { "DateTime", new DateTime(2015, 05, 06) }
            };

            object[] values = new object[1] { dict };

            var row = new USqlRow(schema, values);

            var expected = "{\"a\":{" +
                "\"short\":3," +
                "\"int\":3," +
                "\"long\":9223372036854775807," +
                "\"float\":3.5," +
                "\"double\":3.5," +
                "\"decimal\":205.2," +
                "\"byte\":3," +
                "\"bool\":true," +
                "\"string\":\"test\"," +
                "\"char\":\"a\"," +
                "\"DateTime\":\"2015-05-06T00:00:00\"" +
                "}}";
            var actual = GetOutputterResult(row);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void JsonOutputter_DatatypeSqlMapOfPrimitiveTypes_Outputted()
        {
            USqlSchema schema = new USqlSchema(
                new USqlColumn<SqlMap<string, object>>("a")
            );
            var map = new SqlMap<string, object>(new Dictionary<string, object>
            {
                { "short", (short)3 },
                { "int", 3 },
                { "long", 9223372036854775807 },
                { "float", 3.5F },
                { "double", 3.5D },
                { "decimal", 205.2M },
                { "byte", (byte)3 },
                { "bool", true },
                { "string", "test" },
                { "char", 'a' },
                { "DateTime", new DateTime(2015, 05, 06) }
            });

            object[] values = new object[1] { map };

            var row = new USqlRow(schema, values);

            var expected = "{\"a\":{" +
                "\"short\":3," +
                "\"int\":3," +
                "\"long\":9223372036854775807," +
                "\"float\":3.5," +
                "\"double\":3.5," +
                "\"decimal\":205.2," +
                "\"byte\":3," +
                "\"bool\":true," +
                "\"string\":\"test\"," +
                "\"char\":\"a\"," +
                "\"DateTime\":\"2015-05-06T00:00:00\"" +
                "}}";
            var actual = GetOutputterResult(row);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void JsonOutputter_DatatypeKeyValuePairPrimitiveTypes_Outputted()
        {
            USqlSchema schema = new USqlSchema(
                new USqlColumn<KeyValuePair<string, object>>("a")
            );
            var keyValuePair = new KeyValuePair<string, object>("int", 3);

            object[] values = new object[1] { keyValuePair };

            var row = new USqlRow(schema, values);

            var expected = "{\"a\":{\"int\":3}}";
            var actual = GetOutputterResult(row);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void JsonOutputter_DatatypeArrayOfMaps_Outputted()
        {
            USqlSchema schema = new USqlSchema(
                new USqlColumn<Dictionary<string, object>[]>("a")
            );

            Dictionary<string, object>[] dictArray = new Dictionary<string, object>[2]
            {
                new Dictionary<string, object>
                {
                    { "test1", "asd" },
                    { "test2", 3 },
                },
                new Dictionary<string, object>
                {
                    { "test1", "das" },
                    { "test2", 3 },
                }
            };

            object[] values = new object[1] { dictArray };

            var row = new USqlRow(schema, values);

            var expected = "{\"a\":[{\"test1\":\"asd\",\"test2\":3},{\"test1\":\"das\",\"test2\":3}]}";
            var actual = GetOutputterResult(row);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void JsonOutputter_DatatypeMapOfArrays_Outputted()
        {
            USqlSchema schema = new USqlSchema(
                new USqlColumn<Dictionary<string, object>>("a")
            );

            Dictionary<string, object> dict = new Dictionary<string, object>
            {
                { "test1", new int[]{ 2, 3 } },
                { "test2", new string[]{ "asd", "" } },
            };

            object[] values = new object[1] { dict };

            var row = new USqlRow(schema, values);

            var expected = "{\"a\":{\"test1\":[2,3],\"test2\":[\"asd\",\"\"]}}";
            var actual = GetOutputterResult(row);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void JsonOutputter_DatatypeMapOfArrays_MultiRow_Outputted()
        {
            USqlSchema schema = new USqlSchema(
                new USqlColumn<Dictionary<string, object>>("a")
            );

            List<USqlRow> rows = new List<USqlRow>();
            Dictionary<string, object> dict = new Dictionary<string, object>
            {
                { "test1", new int[]{ 2, 3 } },
                { "test2", new string[]{ "asd", "" } },
            };

            rows.Add(new USqlRow(schema, new object[] { dict }));

            dict = new Dictionary<string, object>
            {
                { "test3", new int[]{ 1, 4 } },
                { "test4", new string[]{ "foo", "bar" } },
            };

            rows.Add(new USqlRow(schema, new object[] { dict }));


            var expected = "{\"a\":{\"test1\":[2,3],\"test2\":[\"asd\",\"\"]}}"+
                Environment.NewLine +
                "{\"a\":{\"test3\":[1,4],\"test4\":[\"foo\",\"bar\"]}}";
            var actual = GetOutputterResult(rows);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void JsonOutputter_DatatypeComplex_Outputted()
        {
            USqlSchema schema = new USqlSchema(
                new USqlColumn<Dictionary<string, object>>("a")
            );

            Dictionary<string, object> complex = new Dictionary<string, object>
            {
                {
                    "test1", new Dictionary<string, object>(){ { "nested", 1}, { "nestedArray", new int[] { 1, 2 } } }
                },
                {
                    "test2", new Dictionary<string, object>[]
                    {
                        new Dictionary<string, object>
                        {
                            { "test1", "asd" },
                            { "test2", 3 },
                        },
                        new Dictionary<string, object>
                        {
                            { "test1", "das" },
                            { "test2", 3 },
                        }
                    }
                }
            };

            object[] values = new object[1] { complex };

            var row = new USqlRow(schema, values);

            var expected = "{\"a\":" +
                "{" +
                    "\"test1\":" +
                    "{" +
                        "\"nested\":1," +
                        "\"nestedArray\":[1,2]" +
                    "}," +
                    "\"test2\":" +
                    "[" +
                        "{\"test1\":\"asd\",\"test2\":3}," +
                        "{\"test1\":\"das\",\"test2\":3}" +
                    "]" +
                "}" +
                "}";
            var actual = GetOutputterResult(row);

            Assert.AreEqual(expected, actual);
        }

        private string GetOutputterResult(IEnumerable<IRow> rows)
        {
            var outputter = new JsonLinesOutputter();

            using (var ms = new MemoryStream())
            {
                var unstructuredWriter = new USqlStreamWriter(ms);
                foreach (IRow row in rows)
                {
                    outputter.Output(row, unstructuredWriter);
                }                
                outputter.Close();
                var output = Encoding.ASCII.GetString(ms.ToArray()).TrimEnd();
                return output;
            }
        }

        private string GetOutputterResult(IRow row)
        {
            return GetOutputterResult(new[] { row });
        }
    }
}
