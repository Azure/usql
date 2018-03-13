using System.IO;
using System.IO.Compression;
using Microsoft.Analytics.Interfaces;

namespace SplitTiff
{
    [SqlUserDefinedOutputter(AtomicFileProcessing = false)]
    public class ZipOutputter : IOutputter
    {
        private readonly string _filePattern;
        private int _fileCount;

        private readonly MemoryStream _memoryStream = new MemoryStream();
        private readonly ZipArchive _archive;
        private Stream _outputStream;

        public ZipOutputter(string filePattern)
        {
            _filePattern = filePattern;
            _archive = new ZipArchive(_memoryStream, ZipArchiveMode.Create, true);
        }

        public override void Output(IRow input, IUnstructuredWriter output)
        {   
            var file = input.Get<byte[]>(0);
            _outputStream = output.BaseStream;

            var entry = _archive.CreateEntry(string.Format(_filePattern, _fileCount++),
                CompressionLevel.NoCompression);

            using (var ms = new MemoryStream(file))
            using (var entryStream = entry.Open())
            {
                ms.CopyTo(entryStream);
            }
        }

        public override void Close()
        {
            // ZipArchive has to be disposed to make it write its content to its underlying stream
            _archive.Dispose();

            using (var writer = new BinaryWriter(_outputStream))
            {
                writer.Write(_memoryStream.ToArray());
                writer.Flush();
            }

            _memoryStream.Dispose();
        }
    }
}