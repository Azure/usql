# Handle Encoding
This example shows various ways to handle invalid encodings in text files.

## FixEncodingErrors.usql
This sample script copies its input file to the output file while
substituting the bytes that are invalid in the given encoding with the
replacement character (U+FFFD). Additionally, records containing invalid
bytes will be written to an error file, optionally prepended with their
offsets in the original file.

The following script parameters can be used to modify the script's
behavior:

- `@inputFile` - File to read and validate. If the input file name
is `EncodingErrorSample.txt`:
  - The output file will be `EncodingErrorSample.filtered.txt`.
  - The errors file will be `EncodingErrorSample.errors.txt`.

- `@codePage` - Code page to validate the file with. The following code
pages are supported:

  | Constant Name      | CP Number |
  | :---               |      ---: |
  | `@codePageUtf8`    |     65001 |
  | `@codePageUtf16`   |      1200 |
  | `@codePageUtf16be` |      1201 |
  | `@codePageUtf32`   |     12000 |
  | `@codePageUtf32be` |     12001 |

- `@rowDelimiter` - This string is used to break the input file into
records. Row delimiters must occur in the input file at least once every
4MB. The output file will have row delimiter after the last record even
if the input file did not have it.

- `@removeInvalidInputRecordsFromOutputfile` - If `true`, the records
containing invalid bytes will _not_ be copied to the output file.

- `@replaceInvalidBytesInErrorsFile` - If `true`, invalid bytes in the
records written to the error file will be substituted with the
replacement charcter (U+FFFD). This is useful if you want to analyze the
errors with a tool (e.g., editor) that complains about invalid
encoding.

- `@offsetFormatInErrorsFile` - This specifies a C# composite format
string that is used to output offsets of invalid records to the error
file. The string should contain a format item with index `0`, e.g.
`"{0}:\t"`. If `null`, the offsets will not be written.
