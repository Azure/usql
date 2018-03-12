using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Analytics.Interfaces;

namespace Microsoft.Analytics.Samples.Formats.Json
{
    [SqlUserDefinedExtractor(AtomicFileProcessing = false)]
    public class MultiLineJsonExtractor : JsonExtractor
    {

        private readonly byte[] rowdelim;

        public MultiLineJsonExtractor(string rowpath = null, Encoding encoding = null, string rowdelim = "\r\n") : base(rowpath)
        {            
            this.rowdelim = (encoding ?? Encoding.UTF8).GetBytes(rowdelim);
        }

        public override IEnumerable<IRow> Extract(IUnstructuredReader input, IUpdatableRow output)
        {
            foreach (Stream currentline in input.Split(this.rowdelim))
            {
                foreach (var row in Extract(currentline, output))
                    yield return row;
            }
        }
    }
}
