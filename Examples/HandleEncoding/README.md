# Handle Encoding
These examples show how to handle various data encoding issues.

## FixEncodingErrors
The sample script copies its input file to the output file while
substituting the bytes invalid in the given encoding with replacement
character (U+FFFD). Additionally, records containing invalid bytes will
be written to errors file, optionally prepended by their offsets in the
original file.

The following script parameters can be used to modify script behavior:

- `@inputFile` - File to read and validate. If the input file name
is `EncodingErrorSample.txt`:
  - The output file will be `EncodingErrorSample.filtered.txt`.
  - The errors file will be `EncodingErrorSample.errors.txt`.

- `@codePage` - Code page to validate the file. The folloing code pages
are supported:

  | Constant Name      | CP Number |
  | :---               |      ---: |
  | `@codePageUtf8`    |     65001 |
  | `@codePageUtf16`   |      1200 |
  | `@codePageUtf16be` |      1201 |
  | `@codePageUtf32`   |     12000 |
  | `@codePageUtf32be` |     12001 |

- `@rowDelimiter` - The string is used to break input file into records.
Row delimiters must occur in the input file at least once in 4MB. The
output file will have row delimiter after the last record even if the
input file did not have it.

- `@skipInvalidRecordsInOutputfile` - If `true`, records containing
invalid bytes will _not_ be copied to the output file.

- `@replaceInvalidBytesInErrorsFile` - If `true`, invalid bytes in the
records writtent to the errors file will be substituted with the
replacement charcter (U+FFFD).

- `@offsetFormatInErrorsFile` - C# composite format string that is used
to output offsets of invalid records to the errors file. The string
should contain format item with index `0`, e.g. `"{0}:\t"`. If `null`,
the offsets will not be written.
