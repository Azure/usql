//
// Copyright (c) Microsoft and contributors.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//
// See the License for the specific language governing permissions and
// limitations under the License.
//
//
// History
// =======
// 2015-01 * Michael Z. Kadaner : original version.
//
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
            // Map column names to indices once to avoid mapping overhead in the loop.
            var recordCol = output.Schema.IndexOf("record");
            var offsetCol = output.Schema.IndexOf("offset");
            var encodingErrorsCol = output.Schema.IndexOf("encodingErrors");

            var buffer = new byte[maxRecordSize];

            // We need to assign record start offset to each record so that later we can sort records by offsets and
            // output them in the right order. However, since there is no API to get the actual start offset of a record,
            // we keep track of record offsets in the "offset" variable. We start from the split start offset (input.Start)
            // and increment it by record size as we read records. There are two issues with this solution.
            //
            // In the explanations below, let
            // - S(k) and S(k+1) be two adjacent splits, and
            // - E(k) and E(k+1) their corresponding extractors;
            // - S(k+1).Start be the absolute start offset of S(k+1) in the entire file;
            // - R(m), R(m+1), and R(m+2) be three consecutive records, and
            // - DL be record delimiter length in bytes.
            //
            // When extractors read records near the boundary between the splits, the general rules are:
            // * E(k+1) always skips one (possibly) incomplete record at the beginning of S(k+1)
            //   and starts extracting from the next record. In other words, it skips bytes at the
            //   beginning of S(k+1) until it sees a complete record delimiter.
            // * Complementarily, E(k) extracts the last record of S(k) and continues extracting
            //   from S(k+1) until it reads one complete record delimiter that started in S(k+1).
            //
            // More formally:
            // A. If R(m) begins within S(k) and extends into S(k+1) for more than DL bytes,
            //    - E(k) will extract R(m).
            //    - E(k+1) will skip the rest of R(m) including its record delimiter
            //      and extract R(m+1).
            // B. If R(m) begins within S(k) and extends into S(k+1) for exactly DL bytes,
            //    - E(k) will extract R(m).
            //    - E(k+1) will skip record delimiter of R(m) and extract R(m+1).
            // C. If R(m) begins within S(k) and extends into S(k+1) for less than DL bytes,
            //    - E(k) will extract R(m) and R(m+1).
            //    - E(k+1) will skip the last bytes of record delimiter of R(m)
            //      and the entire R(m+1), and extract R(m+2).
            // D. If R(m) begins within S(k) and ends at the split boundary, i.e., the last
            //    byte of the record delimiter of R(m) is the last byte of S(k),
            //    - E(k) will extract R(m) and R(m+1).
            //    - E(k+1) will skip the entire R(m+1) and extract R(m+2).
            //
            // Issue 1: It is impossible to precisely calculate record offsets in non-first splits. Unsolved.
            // Consider layout A. above. E(k+1) will skip the rest of R(m) and start extracting from R(m+1). The skipping
            // happens inside input.Split() call and is invisible to the extractor logic. E(k+1) cannot know how many
            // bytes of R(m) were skipped inside input.Split() and thus cannot assign to R(m+1) its real precise offset.
            // Therefore:
            // Offsets of the records returned by the extractor in all subsequent splits will be smaller than real
            // offsets by (R(m).Tail - DL), where R(m).Tail is the number of bytes in R(m) that extend into S(k+1).
            // Notably, in the case B. above, the returned offsets will be equal to the real offsets.
            //
            // Issue 2: We need to avoid record offset collision on split boundary. Compensated.
            // Consider layout D. above. E(k) will extract R(m+1) and, under certain circumstances, may legitimately
            // assign S(k+1).Start to R(m+1) offset. E(k+1) will skip the entire R(m+1) and start extracting from  R(m+2).
            // The skipping happens inside input.Split() call and is invisible to the extractor logic. E(k+1) will assign
            // the initial value of its local "offset" variable to the offset of R(m+2). If E(k+1) initializes "offset"
            // with input.Start, both R(m+1) and R(m+2) will be assigned the same offset, S(k+1).Start.
            // To avoid this collision:
            // In all subsequent splits, we initialize "offset" with (input.Start + DL). With this technique we guarantee
            // that for each record the assigned offset is never larger than its real precise offset in the entire file.
            // Note that DL is the smallest number of bytes that can be skipped at the beginning of a non-first split.
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
                // Map column names to indices once to avoid mapping overhead for each row.
                this.recordCol = input.Schema.IndexOf("record");
                this.offsetCol = input.Schema.IndexOf("offset");
                this.encodingErrorsCol = input.Schema.IndexOf("encodingErrors");
            }

            var record = input.Get<byte[]>(recordCol);
            var encodingErrors = input.Get<bool>(encodingErrorsCol);

            if (encodingErrors && this.replaceInvalidBytes)
            {
                // Decode the record to substitute invalid bytes with the replacement character.
                // Encode the sanitized record again before writing it to the output file.
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
