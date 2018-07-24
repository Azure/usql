using BitMiracle.LibTiff.Classic;
using System;
using System.Collections.Generic;
using System.IO;

namespace SplitTiff
{
    public static class SplitTiff
    {
        public static List<byte[]> Split(Stream inputStream, int chunkWidth, int chunkHeight)
        {
            using (var input = Tiff.ClientOpen("Stream read", "r", inputStream, new TiffStream()))
            {
                if (input.IsTiled())
                    throw new Exception("Cannot process Tiled image!");

                var width = input.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
                var height = input.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
                var stripFactor = input.StripSize() / width;
                var isEncoded = IsEncoded(input);

                var result = new List<byte[]>();
                for (var i = 0; i < height; i += chunkHeight)
                {
                    var stripBuffers = CreateStripBuffers(input, width, height, i, chunkHeight,
                        stripFactor, isEncoded);

                    for (var j = 0; j < width; j += chunkWidth)
                    {
                        result.Add(CreateChunk(input, stripBuffers, width, j,
                            chunkWidth, stripFactor, isEncoded));
                    }
                }

                return result;
            }
        }

        private static byte[] CreateChunk(Tiff input, IReadOnlyList<byte[]> stripBuffers, int width,
            int columnOffset, int chunkWidth, int stripFactor, bool isEncoded)
        {
            // This is to prevent column overflow
            if (columnOffset + chunkWidth >= width)
                chunkWidth = width - columnOffset;

            using (var ms = new MemoryStream())
            {
                using (var output = Tiff.ClientOpen("In-memory write", "w", ms, new TiffStream()))
                {
                    CopyTags(input, output, chunkWidth, stripBuffers.Count);

                    var stripOffset = columnOffset * stripFactor;
                    var stripCount = chunkWidth * stripFactor;

                    for (var i = 0; i < stripBuffers.Count; i++)
                        WriteStrip(output, i, stripBuffers[i], stripOffset, stripCount, isEncoded);
                }

                return ms.ToArray();
            }
        }

        private static byte[][] CreateStripBuffers(Tiff input, int width, int height, int rowOffset,
            int chunkHeight, int stripFactor, bool isEncoded)
        {
            // This is to prevent row overflow
            if (rowOffset + chunkHeight >= height)
                chunkHeight = height - rowOffset;

            var stripBuffers = new byte[chunkHeight][];
            var stripSize = width * stripFactor;

            for (var i = 0; i < chunkHeight; i++)
            {
                stripBuffers[i] = new byte[stripSize];
                ReadStrip(input, i + rowOffset, stripBuffers[i], isEncoded);
            }

            return stripBuffers;
        }

        private static bool IsEncoded(Tiff image)
        {
            var encoded = false;
            var compressionTagValue = image.GetField(TiffTag.COMPRESSION);

            if (compressionTagValue != null)
                encoded = compressionTagValue[0].ToInt() != (int)Compression.NONE;

            return encoded;
        }

        private static void ReadStrip(Tiff input, int stripNumber, byte[] buffer, bool encoded)
        {
            if (encoded)
                input.ReadEncodedStrip(stripNumber, buffer, 0, buffer.Length);
            else
                input.ReadRawStrip(stripNumber, buffer, 0, buffer.Length);
        }

        private static void WriteStrip(Tiff output, int stripNumber, byte[] buffer, int offset, int count, bool encoded)
        {
            if (encoded)
                output.WriteEncodedStrip(stripNumber, buffer, offset, count);
            else
                output.WriteRawStrip(stripNumber, buffer, offset, count);
        }

        private static void CopyTags(Tiff input, Tiff output, int width, int height)
        {
            for (var t = ushort.MinValue; t < ushort.MaxValue; t++)
            {
                var tag = (TiffTag)t;
                var tagValue = input.GetField(tag);

                if (tagValue != null)
                    output.GetTagMethods().SetField(output, tag, tagValue);
            }

            output.SetField(TiffTag.IMAGEWIDTH, width);
            output.SetField(TiffTag.IMAGELENGTH, height);
            output.SetField(TiffTag.ROWSPERSTRIP, height);
        }
    }
}
