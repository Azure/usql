using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Demo
{
    public class MyTsvExtractor : IExtractor
    {
        private Encoding _encoding = Encoding.UTF8;
        private byte[] _row_delim;
        private char _col_delim;

        public MyTsvExtractor()
        {
            this._row_delim = this._encoding.GetBytes("\r\n");
            this._col_delim = '\t';
        }

        public override IEnumerable<IRow> Extract(IUnstructuredReader input, IUpdatableRow output)
        {
            foreach (var current in input.Split( this._row_delim ))
            {
                using (var streamReader = new StreamReader(current, this._encoding))
                {
                    string line = streamReader.ReadToEnd().Trim();

                    string[] parts = line.Split(this._col_delim);

                    int count = 0;
                    foreach (string part in parts)
                    {
                        output.Set<string>(count, parts[count]);
                        count += 1;
                    }

                }
                yield return output.AsReadOnly();
            }
            yield break;

        }
    }

}
