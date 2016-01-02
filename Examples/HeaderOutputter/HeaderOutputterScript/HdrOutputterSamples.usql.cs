// Copyright 2016 Microsoft Corp.
// Author: Michael Rys (mrys)

using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Sample Outputter that just outputs the rowset schema as "header". It always writes the names of the columns and provides the option to write the types in a second row.
// It does not output the content of the file (That is left as an exercise to the reader)
//
// At this point, the outputter requires atomic file processing (ie, the file is not split into parallel extends). 
// Once the Output model provides a way to identify the beginning of the file, this restriction can be removed.
//
// USAGE examples:
//
// OUTPUT @res USING new HeaderOutputter.HeaderOutputter(quoting:true, with_types:true, encoding:Encoding.Unicode); 
// OUTPUT @res USING HeaderOutputter.Factory.Columns();
// OUTPUT @res USING HeaderOutputter.Factory.ColumnsAndTypes();

namespace HeaderOutputter
{
    [SqlUserDefinedOutputter(AtomicFileProcessing=true)]
    public class HeaderOutputter : IOutputter
    {
        private string   _row_delim;
        private char     _col_delim;
        private bool     _with_types;
        private Encoding _encoding;
        private bool     _quoting;
        private bool     _first_row_written = false;  // Makes sure we only write one header per file

        // Parameter initialization
        //
        // row_delim sets the characters to separate the rows. Default: \r\n.
        // col_delim sets the character to separate the columns. Default: ','.
        // with_types indicates whether the type row should be included. Default: false.
        // quoting indicates whether the column content is quoted with double quotes (and double quotes will be doubled). Default: true
        // encoding sets the encoding used to set the file's encoding. Default: UTF8.
        //
        public HeaderOutputter(string row_delim = "\r\n",  char col_delim = ',', bool with_types = false, bool quoting = true, Encoding encoding = null)
        {
            this._encoding = ((encoding == null) ? Encoding.UTF8 : encoding);
            this._row_delim = row_delim;
            this._col_delim = col_delim;
            this._with_types = with_types;
            this._quoting = quoting;
         }

        // AddQuotes
        //
        // Quotes the provided string with double quotes and doubles the contained double quotes.
        //
        public static string AddQuotes(string s)
        {
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }

        // Output
        //
        // Outputs the names of the rowset columns in a column separated row and optionally adds their types in a second row.
        //
        public override void Output(IRow row, IUnstructuredWriter output)
        {
            if (_first_row_written) { return; }
            using (StreamWriter streamWriter = new StreamWriter(output.BaseStream, this._encoding))
            {
                streamWriter.NewLine = this._row_delim;
                ISchema schema = row.Schema;
                for (int i = 0; i < schema.Count(); i++)
                {
                    var col = schema[i];
                    if (i > 0)
                    {
                        streamWriter.Write(this._col_delim);
                    }
                    var val = _quoting ? AddQuotes(col.Name) : col.Name;
                    streamWriter.Write(val);
                }
                streamWriter.WriteLine();
                if (_with_types)
                {
                    for (int i = 0; i < schema.Count(); i++)
                    {
                        var col = schema[i];
                        if (i > 0)
                        {
                            streamWriter.Write(this._col_delim);
                        }
                        var val = _quoting ? AddQuotes(col.Type.FullName) : col.Type.FullName;
                        streamWriter.Write(val);
                    }
                    streamWriter.WriteLine();
                }
            }
            _first_row_written = true;
        }
    }

    // Define the factory classes
    public static class Factory {
        public static HeaderOutputter Columns(string row_delim = "\r\n", char col_delim = ',', bool quoting = true, Encoding encoding = null) 
        { 
            return new HeaderOutputter(row_delim,  col_delim , false, quoting, encoding); 
        }

        public static HeaderOutputter ColumnsAndTypes(string row_delim = "\r\n", char col_delim = ',', bool quoting = true, Encoding encoding = null) 
        { 
            return new HeaderOutputter(row_delim, col_delim, true, quoting, encoding); 
        }
    }
}
          