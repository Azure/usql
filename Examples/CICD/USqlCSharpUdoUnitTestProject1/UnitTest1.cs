using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Interfaces.Streaming;
using Microsoft.Analytics.Types.Sql;
using Microsoft.Analytics.UnitTest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpUDO;

// This is an example of how to set up test cases for C# user defined processor
// Create a U-SQL C# UDO Unit Test Sample Project to learn more about how to set up test cases for other user defined operators

namespace USqlCSharpUdoUnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {

        /// <summary>
        /// Test case for user defined processor that is defined in CSharpUDO project
        /// </summary>
        [TestMethod]
        public void TestMyProcessor()
        {
            // Define the schema for processor input rowset
            // Schema: "a:int, b:int"
            //
            USqlColumn<int> col1 = new USqlColumn<int>("col1");
            USqlColumn<int> col2 = new USqlColumn<int>("col2");
            List<IColumn> columns = new List<IColumn> { col1, col2 };
            USqlSchema schema = new USqlSchema(columns);

            // Generate one row with specified column values as input rowset
            //
            object[] values = new object[2] { 0, 0 };
            IRow input = new USqlRow(schema, values);
            IUpdatableRow output = input.AsUpdatable();

            // Create processor instance for testing and run the processor with fake input
            //
            MyProcessor processor = new MyProcessor();
            IRow newOutput = processor.Process(input, output);

            //Verify results for processor output
            //
            Assert.IsTrue(newOutput.Schema.Count == 2);
            Assert.IsTrue(newOutput.Get<int>(0) == 1);
            Assert.IsTrue(newOutput.Get<int>(1) == 5);
        }
    }
}
