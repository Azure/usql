using Microsoft.Analytics.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HandleEncoding
{
    /// <summary>
    /// Reads delimited records (byte sequences) from the input file.
    /// Tests if the byte sequences is valid for the given code page.
    /// </summary>
    /// <remarks>
    /// Extracted schema: { record byte[], offset long, encodingErrors bool }, where
    /// "record" is the original byte sequence,
    /// "offset" is the absolute offset of the record in the input file (but see explanations below), and
    /// "encodingErrors" indicates whether the byte sequence is valid for the given code page.
    /// </remarks>
    public class EncodingValidatingExtractor : IExtractor
    {
        private const int maxRecordSize = 4 * 1024 * 1024;

        private readonly Encoding throwingEncoding;
        private readonly byte[] rowDelimiter;

        public EncodingValidatingExtractor(int codePage, string rowDelimiter)
        {
            this.throwingEncoding = Helper.GetEncoding(codePage, true);
            this.rowDelimiter = Helper.GetRowDelimiter(this.throwingEncoding, rowDelimiter);
        }

        public override IEnumerable<IRow> Extract(IUnstructuredReader input, IUpdatableRow output)
        {
            var recordCol = output.Schema.IndexOf("record");
            var offsetCol = output.Schema.IndexOf("offset");
            var encodingErrorsCol = output.Schema.IndexOf("encodingErrors");

            var buffer = new byte[maxRecordSize];

            // We need to assign record start offset to each record so that later we can sort records by offsets and
            // output them in the right order. However, since there is no API to get the actual start offset of a record,
            // we keep track of record offsets in the "offset" variable. We start from the split start offset (input.Start)
            // and increment it by record size as we read records.
            //
            // There are two issues here. In the explanations below, let S(k) and S(k+1)
            // be two adjacent splits, and E(k) and E(k+1) their corresponding extractors.
            //
            // 1. It is impossible to precisely calculate record offsets in non-first splits.
            // Let a record R(m) start within S(k) and extend into S(k+1). E(k) will extract the entire R(m).
            // E(k+1) will skip the rest of R(m) and start extracting from the next record, R(m+1).
            // This happens inside input.Split() call and is invisible to the extractor logic.
            // E(k+1) cannot know how many bytes of R(m) were skipped inside input.Split() and thus cannot assign
            // to R(m+1) it's real precise offset. Therefore:
            // Offsets assigned to all records of all subsequent splits will be smaller than their real offsets
            // by the number of bytes belonging to the last record of the previous split that extends into the current split.
            //
            // 2. We need to avoid record offset collision on split boundary.
            // Let a record R(m) start exactly at S(k+1).Start. R(m) will be extracted by E(k), which (under certain circumstances)
            // may legitimately assign S(k+1).Start to its offset. E(k+1) will skip the entire R(m) and start extracting
            // from the next record, R(m+1). This happens inside input.Split() call and is invisible to the extractor logic.
            // E(k+1) will assign the initial value of its local "offset" variable to the offset of R(m+1).
            // If E(k+1) initializes "offset" with S(k+1).Start, both R(m) and R(m+1) will be assigned the same offset.
            // To avoid this collision, in all subsequesnt splits, we add the length of the row delimiter to input.Start,
            // the row delimiter length being the smallest number of bytes that can be skipped at the beginning of a non-first split.
            var offset = input.Start + (input.Start > 0 ? this.rowDelimiter.Length : 0);

            foreach (var recordStream in input.Split(this.rowDelimiter))
            {
                var bytesRead = recordStream.Read(buffer, 0, buffer.Length);
                var record = Helper.ConcatArrays(buffer, bytesRead, this.rowDelimiter);

                output.Set(recordCol, record);

                output.Set(offsetCol, offset);
                offset += record.Length;

                try
                {
                    // We only need to validate encoding; we do not need actual encoded string.
                    this.throwingEncoding.GetCharCount(record);
                    output.Set(encodingErrorsCol, false);
                }
                catch (DecoderFallbackException)
                {
                    output.Set(encodingErrorsCol, true);
                }

                yield return output.AsReadOnly();
            }
        }
    }

    /// <summary>
    /// Outputs the records extracted by <see cref="EncodingValidatingExtractor"/>.
    /// </summary>
    /// <remarks>
    /// Can output the records verbatim or substitute invalid bytes with Unicode replacement character (U+FFFD).
    /// Provides an option to output the record offset in front of the record.
    /// </remarks>
    public class RecordOutputter : IOutputter
    {
        // Initialize for default constructor. Constructor with parameters will override everything.
        private bool replaceInvalidBytes = false;
        private string offsetFormat = null;

        private readonly Encoding nonThrowingEncoding = null;

        private int recordCol = -1;
        private int offsetCol = -1;
        private int encodingErrorsCol = -1;

        /// <summary>
        /// Creates <c>RecordOutputter</c>, which will write the records verbatim,
        /// without substituting invalid bytes or prepending the records with thier offset.
        /// </summary>
        public RecordOutputter()
        {
        }

        /// <summary>
        /// Creates new <c>RecordOutputter</c>, which optionally substitutes invalid bytes
        /// or outputs record offsets.
        /// </summary>
        /// <param name="codePage">The code page is used to substitute invalid bytes with the replacement
        /// character (U+FFFD) and to encode the formatted record offsets.</param>
        /// <param name="replaceInvalidBytes">If <c>true</c>, the invalid bytes will be substituted
        /// with the replacement character (U+FFFD); otherwise, the records will be output verbatim.</param>
        /// <param name="offsetFormat">Format string used to output record offset in front of the record.
        /// If <c>null</c>, the offset will not be output.</param>
        public RecordOutputter(int codePage, bool replaceInvalidBytes, string offsetFormat)
        {
            this.replaceInvalidBytes = replaceInvalidBytes;
            this.offsetFormat = offsetFormat;

            if (replaceInvalidBytes || !string.IsNullOrEmpty(offsetFormat))
            {
                this.nonThrowingEncoding = Helper.GetEncoding(codePage, false);
            }
        }

        public override void Output(IRow input, IUnstructuredWriter output)
        {
            if (this.recordCol == -1)
            {
                this.recordCol = input.Schema.IndexOf("record");
                this.offsetCol = input.Schema.IndexOf("offset");
                this.encodingErrorsCol = input.Schema.IndexOf("encodingErrors");
            }

            var record = input.Get<byte[]>(recordCol);
            var encodingErrors = input.Get<bool>(encodingErrorsCol);

            if (encodingErrors && this.replaceInvalidBytes)
            {
                record = this.nonThrowingEncoding.GetBytes(this.nonThrowingEncoding.GetString(record));
            }

            if (this.offsetFormat != null)
            {
                output.BaseStream.Write(
                    this.nonThrowingEncoding.GetBytes(string.Format(this.offsetFormat, input.Get<long>(this.offsetCol))));

                var bom = this.nonThrowingEncoding.GetPreamble();
                if (record.Take(bom.Length).SequenceEqual(bom))
                {
                    // This should happen once per file. Prefer code simplicity to performance.
                    record = record.Skip(bom.Length).ToArray();
                }
            }

            output.BaseStream.Write(record);
        }
    }

    internal static class Helper
    {
        private enum CodePage
        {
            Utf8 = 65001,
            Utf16 = 1200,
            Utf16be = 1201,
            Utf32 = 12000,
            Utf32be = 12001
        }

        internal static Encoding GetEncoding(int codePage, bool throwing)
        {
            switch ((CodePage)codePage)
            {
                case CodePage.Utf8:
                    return new UTF8Encoding(encoderShouldEmitUTF8Identifier: true, throwOnInvalidBytes: throwing);

                case CodePage.Utf16:
                    return new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: throwing);

                case CodePage.Utf16be:
                    return new UnicodeEncoding(bigEndian: true, byteOrderMark: true, throwOnInvalidBytes: throwing);

                case CodePage.Utf32:
                    return new UTF32Encoding(bigEndian: false, byteOrderMark: true, throwOnInvalidCharacters: throwing);

                case CodePage.Utf32be:
                    return new UTF32Encoding(bigEndian: true, byteOrderMark: true, throwOnInvalidCharacters: throwing);

                default:
                    throw new ArgumentException(string.Format("Invalid code page {0}.", codePage));
            }
        }

        internal static byte[] GetRowDelimiter(Encoding encoding, string rowDelimiter)
        {
            if (string.IsNullOrEmpty(rowDelimiter))
            {
                throw new ArgumentException("Row delimiter must not be empty.");
            }

            return encoding.GetBytes(rowDelimiter);
        }

        internal static byte[] ConcatArrays(byte[] first, int firstLength, byte[] second)
        {
            var result = new byte[firstLength + second.Length];
            Array.Copy(first, result, firstLength);
            Array.Copy(second, 0, result, firstLength, second.Length);
            return result;
        }

        internal static void Write(this Stream stream, byte[] bytes)
        {
            stream.Write(bytes, 0, bytes.Length);
        }
    }
}
