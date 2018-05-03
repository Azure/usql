using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace USQLApplication1
{
    [SqlUserDefinedExtractor]
    public class MyExtractor : IExtractor
    {
        public override IEnumerable<IRow> Extract(IUnstructuredReader input, IUpdatableRow outputrow)
        {
            ulong rowId = 0;
            char column_delimiter = '\t';
            string lastLine = string.Empty;
            string line;
            foreach (var stream in input.Split(new byte[] { 0x0d, 0x0a }))
            {
                var reader = new StreamReader(stream, encoding: Encoding.UTF8);
                line = reader.ReadToEnd();

                if (string.IsNullOrEmpty(line))
                {
                    DiagnosticStream.WriteLine(String.Format("Skip empty line at line {0}.", rowId));
                }
                else if (line.StartsWith("id\t"))
                {
                    DiagnosticStream.WriteLine(String.Format("Skip header line at line {0}.", rowId));
                }
                else
                {
                    try
                    {
                        var tokens = line.Split(column_delimiter);
                        outputrow.Set<int>("id", int.Parse(tokens[0]));
                        outputrow.Set<DateTime>("date", DateTime.Parse(tokens[1]));
                        outputrow.Set<string>("market", tokens[2]);
                        outputrow.Set<string>("searchstring", tokens[3]);
                        outputrow.Set<int>("time", int.Parse(tokens[4]));
                        outputrow.Set<string>("found_urls", tokens[5]);
                        outputrow.Set<string>("visited_urls", tokens[6]);
                    }
                    catch
                    {
                        DiagnosticStream.WriteLine("============\t <afds />====\n==============\xc4");
                        DiagnosticStream.WriteLine(String.Format("Last  line {0}: {1}", rowId - 1, lastLine));
                        DiagnosticStream.WriteLine(String.Format("Error line {0}: {1}", rowId, line));
                        DiagnosticStream.WriteLine("==============================");
                    }

                    yield return outputrow.AsReadOnly();
                }

                lastLine = line;
                ++rowId;
            }
        }
    }
}