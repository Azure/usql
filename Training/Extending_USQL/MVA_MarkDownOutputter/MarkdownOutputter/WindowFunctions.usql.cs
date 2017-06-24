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
        private int row_count;

        public MarkdownOutputter()
        {
            row_count = 0;
        }

        public override void Close()
        {
        }

        public override void Output(IRow row, IUnstructuredWriter output)
        {
            var streamWriter = new StreamWriter(output.BaseStream);

            // Metadata schema initialization to enumerate column names
            var schema = row.Schema;


            if (this.row_count == 0)
            {
                streamWriter.Write("|");
                for (int i = 0; i < schema.Count(); i++)
                {
                    var col = schema[i];
                    streamWriter.Write(" ");
                    streamWriter.Write(col.Name);
                    streamWriter.Write(" ");
                    streamWriter.Write("|");
                }
                streamWriter.Write("\r\n");
                streamWriter.Flush();

                streamWriter.Write("|");
                for (int i = 0; i < schema.Count(); i++)
                {
                    var col = schema[i];
                    streamWriter.Write(" ");
                    streamWriter.Write("---");
                    streamWriter.Write(" ");
                    streamWriter.Write("|");
                }
                streamWriter.Write("\r\n");
                streamWriter.Flush();
            }

            // Data row output
            streamWriter.Write("|");
            for (int i = 0; i < schema.Count(); i++)
            {
                var col = schema[i];
                string val = "";

                try
                {
                    var coltype = col.Type;
                    if (coltype == typeof(string))
                    {
                        val = row.Get<string>(col.Name).ToString();
                        val = val ?? "NULL";
                    }
                    else if (coltype == typeof(float))
                    {
                        val = row.Get<float>(col.Name).ToString();
                    }
                    else if (coltype == typeof(double))
                    {
                        val = row.Get<double>(col.Name).ToString();
                    }
                    else if (coltype == typeof(long))
                    {
                        val = row.Get<long>(col.Name).ToString();
                    }
                    else if (coltype == typeof(Guid))
                    {
                        val = row.Get<Guid>(col.Name).ToString();
                    }
                    else if (coltype == typeof(int?))
                    {
                        val = row.Get <int?> (col.Name).ToString();
                        val = val ?? "NULL";
                    }
                    else if (coltype == typeof(long?))
                    {
                        val = row.Get<long?>(col.Name).ToString();
                        val = val ?? "NULL";
                    }
                    else if (coltype == typeof(float?))
                    {
                        val = row.Get<float?>(col.Name).ToString();
                        val = val ?? "NULL";
                    }
                    else if (coltype == typeof(double?))
                    {
                        val = row.Get < double?> (col.Name).ToString();
                        val = val ?? "NULL";
                    }
                    else
                    {
                        val = "UNKNOWNTYPE";
                    }

                }
                catch (System.NullReferenceException)
                {
                    // Handling NULL values--keeping them empty
                    val = "NULL";
                }
                streamWriter.Write(" ");
                streamWriter.Write(val);
                streamWriter.Write(" ");
                streamWriter.Write("|");
            }
            streamWriter.Write("\n");
            streamWriter.Flush();
            
        this.row_count++;
        }
    }
}
