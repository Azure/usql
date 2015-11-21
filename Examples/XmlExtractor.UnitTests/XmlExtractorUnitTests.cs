using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Microsoft.Analytics.UnitTest;
using Microsoft.Analytics.Interfaces;
using System.Collections.Generic;
using System.Diagnostics;

namespace UnitTests
{
    [TestClass]
    public class XmlExtractorUnitTests
    {
        private IColumn[] getColumns()
        {
            List<IColumn> columns = new List<IColumn>();

            columns.Add(new USqlColumn<long>("Id"));
            columns.Add(new USqlColumn<string>("City"));
            columns.Add(new USqlColumn<double>("ForecastTemp"));
            columns.Add(new USqlColumn<double>("ForecastHumidity"));

            return columns.ToArray();
        }

        private USqlUpdatableRow getUpdatableRow()
        {
            ISchema schema = new USqlSchema(getColumns());

            IRow row = new USqlRow(schema, new object[schema.Count]);

            USqlUpdatableRow updRow = new USqlUpdatableRow(row);
            return updRow;
        }

        [TestMethod]
        public void LoadExtractorTest( )
        {
            string file = "TestFiles\\TestFile1.xml";

            using (var st = new StreamReader(file))
            {
                USqlStreamReader reader = new USqlStreamReader(st.BaseStream);

                USqlUpdatableRow updRow = getUpdatableRow();

                XmlDomExtractor extractor = new XmlDomExtractor("Locations/Location");

                var result = extractor.Extract(reader, updRow);

                int cnt = 0;
                foreach(var item in result)
                {
                    cnt++;
                    Debug.WriteLine(item.Get<long>("Id"));
                }
            }
        }     
    }


}
