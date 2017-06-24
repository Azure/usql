using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MVADemo
{

    [SqlUserDefinedOutputter(AtomicFileProcessing = true)]
    public class MarkdownOutputter : IOutputter
    {
        public MarkdownOutputter()
        {
        }

        public override void Close()
        {
        }

        public override void Output(IRow row, IUnstructuredWriter output)
        {
            var streamWriter = new StreamWriter(output.BaseStream, Encoding.UTF8);

            // Metadata schema initialization to enumerate column names
            var schema = row.Schema;

            // Data row output
            streamWriter.Write("|");

            for (int i = 0; i < schema.Count(); i++)
            {
                var col = schema[i];
                string val = "";
                try
                {
                    // Data type enumeration--required to match the distinct list of types from OUTPUT statement
                    switch (col.Type.Name.ToString().ToLower())
                    {
                        case "string":
                            val = row.Get<string>(col.Name).ToString();
                            break;
                        case "int":
                            val = row.Get<int>(col.Name).ToString();
                            break;
                        case "float":
                            val = row.Get<float>(col.Name).ToString();
                            break;
                        case "double":
                            val = row.Get<double>(col.Name).ToString();
                            break;
                        case "guid":
                            val = row.Get<Guid>(col.Name).ToString();
                            break;
                        default: break;
                    }
                }
                catch (System.NullReferenceException)
                {
                    // Handling NULL values--keeping them empty
                    val = "ERR";
                }
                streamWriter.Write(val);
                streamWriter.Write("|");
            }
            streamWriter.Write("\n");
            streamWriter.Flush();
        }
    }
}
