using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FileCopy
{
    [SqlUserDefinedExtractor]
    public class ReadFile : IExtractor
    {
        const int _max_blocksz = 4194304;

        private int _blocksz;
        public ReadFile(int block_size = 2097152)
        {
            if (block_size > _max_blocksz)
            {
                throw new Exception(string.Format("Specified block size of {0} bytes exceeds the maximal limit of {1} bytes. Please specify a lower number", block_size, _max_blocksz));
            }
            this._blocksz = block_size;
        }
        public override IEnumerable<IRow> Extract(IUnstructuredReader input, IUpdatableRow outputrow)
        {
            long length = input.Length;
            long start = input.Start;
            Stream baseStream = input.BaseStream;
            byte[] readBuffer = new byte[this._blocksz];

            while (length > 0)
            {
                var globalPosition = start + baseStream.Position;

                // We need to make sure that we read block size or only the last remainder and not into the 4MB overscan area in the next extent block that is provided to handle row-oriented processing
                var readsize = (int)Math.Min(this._blocksz, length); // Cast from (long) to (int) is safe since Min will never give a value larger than (int) _blocksz.

                Array.Resize<byte>(ref readBuffer, readsize); // Make sure buffer is large enough. Assumes that Resize only resizes if needed.

                var bytesRead = baseStream.Read(readBuffer, 0, readsize);
                if (bytesRead <= 0 || bytesRead > readsize)
                {
                    throw new Exception(string.Format("Unexpected amount of {2} bytes was read starting at global stream position {1}. Expected to read {0} bytes.",
                                            readsize, globalPosition, bytesRead));
                }

                Array.Resize<byte>(ref readBuffer, bytesRead);
                length -= bytesRead;

                outputrow.Set<long>(0, globalPosition); // global position of the block
                outputrow.Set<long>(1, bytesRead); // block size
                outputrow.Set<byte[]>(2, readBuffer); // block data
                yield return outputrow.AsReadOnly();
            }
        }
    }

    [SqlUserDefinedOutputter]
    public class WriteFile : IOutputter
    {
        public override void Output(IRow row, IUnstructuredWriter output)
        {
            ISchema schema = row.Schema;
            for (int i = 0; i < schema.Count; i++)
            {
                object obj = row.Get<object>(i);
                if (obj is byte[])
                {
                    output.BaseStream.Write((byte[])obj, 0, ((byte[])obj).Length);
                }
            }
        }
    }
}