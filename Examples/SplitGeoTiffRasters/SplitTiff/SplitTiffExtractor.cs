using System.Collections.Generic;
using System.IO;
using Microsoft.Analytics.Interfaces;

namespace SplitTiff
{
    [SqlUserDefinedExtractor(AtomicFileProcessing = true)]
    public class SplitTiffExtractor : IExtractor
    {
        private readonly int _chunkWidth;
        private readonly int _chunkHeight;

        public SplitTiffExtractor(int chunkWidth, int chunkHeight)
        {
            _chunkWidth = chunkWidth;
            _chunkHeight = chunkHeight;
        }

        public override IEnumerable<IRow> Extract(IUnstructuredReader input, IUpdatableRow output)
        {
            using (var memoryStream = new MemoryStream())
            {
                // LibTiff.Net internally calls something ADLA stream doesn't support
                input.BaseStream.CopyTo(memoryStream);
                memoryStream.Position = 0;

                var tiffArray = SplitTiff.Split(memoryStream, _chunkWidth, _chunkHeight);

                foreach (var tiff in tiffArray)
                {
                    output.Set("tiff", tiff);
                    yield return output.AsReadOnly();
                }
            }
        }
    }
}