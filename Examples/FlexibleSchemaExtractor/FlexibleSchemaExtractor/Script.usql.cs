using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FlexibleSchemaExtractor
{
    public class FlexExtractor : IExtractor
    {
        private Encoding _encoding;
        private byte[] _row_delim;
        private string _col_delim;

        public FlexExtractor(Encoding encoding = null, string row_delim = "\r\n", string col_delim = ",")
        {
            this._encoding = ((encoding == null) ? Encoding.UTF8 : encoding);
            this._row_delim = this._encoding.GetBytes(row_delim);
            this._col_delim = col_delim;
        }

        public override IEnumerable<IRow> Extract(IUnstructuredReader input, IUpdatableRow output)
        {
            var colsInSchema = output.Schema.Count;

            // let's check global assumptions
            // - first 4 provided columns are int, string, int, decimal.

            if (   output.Schema[0].Type != typeof(System.Int32) 
                || output.Schema[1].Type != typeof(System.String)
                || output.Schema[2].Type != typeof(System.Int32)
                || output.Schema[3].Type != typeof(System.Decimal)
               )
            {
                throw new Exception("First 4 columns are not of expected types int32, string, int32, decimal.");
            }

            foreach (Stream currentline in input.Split(this._row_delim))
            {
                using (StreamReader lineReader = new StreamReader(currentline, this._encoding))
                {
                    string[] columns = lineReader.ReadToEnd().Split( new string[] { this._col_delim }
                                                                   , StringSplitOptions.None);
                    var colsInData = columns.Length;

                    // let's check row level assumptions
                    // - if less columns are specified, then last column needs to be of type SqlMap<Int32, string>

                    if (   colsInData > colsInSchema 
                        && output.Schema[colsInSchema - 1].Type != typeof(SqlMap<Int32, string>))
                    {
                        throw new Exception(
                            "Too many columns detected and last column is not of type SqlMap<Int32,string>. " 
                          + "Add a final column of type SqlMap<Int32,string> into your extract schema.");
                    }

                    // Set first 4 fixed columns
                    output.Set<Int32>(0, Int32.Parse(columns[0]));
                    output.Set<String>(1, columns[1]);
                    output.Set<Int32>(2, Int32.Parse(columns[2]));
                    output.Set<Decimal>(3, Decimal.Parse(columns[3]));

                    // Fill all remaining columns except the last which may be a map
                    for (int i = 4; i < Math.Min(colsInData, colsInSchema) - 1; i++)
                    {
                        output.Set<string>(i, columns[i]);
                    }

                    // Now handle last column: if it is a map
                    if (   colsInData >= colsInSchema 
                        && output.Schema[colsInSchema - 1].Type == typeof(SqlMap<Int32, string>))
                    {
                        var sqlmap = new Dictionary<Int32, string>();
                        for (int j = colsInSchema - 1; j < colsInData; j++)
                        {
                            sqlmap.Add(j - colsInSchema + 1, columns[j]);
                        }
                        output.Set<SqlMap<Int32, string>>(colsInSchema - 1, new SqlMap<Int32, string>(sqlmap));
                    }
                    // Now handle last column: if it is not a map
                    else if (colsInData == Math.Min(colsInData, colsInSchema))
                    {
                        output.Set<string>(colsInData - 1, columns[colsInData - 1]);
                    }

                    yield return output.AsReadOnly();
                }
            }
        }
    }
}
