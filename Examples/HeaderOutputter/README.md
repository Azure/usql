#Column Name Outputter

##Description

Sample Outputter that just outputs the rowset schema as "header". It always writes the names of the columns and provides the option to write the types in a second row.
It does not output the content of the rowset (That is left as an exercise to the reader).

At this point, the outputter requires atomic file processing (ie, the file is not split into parallel extends). 
Once the Output model provides a way to identify the beginning of the file, this restriction can be removed.

##Classes

- `HeaderOutputter`
  - Parameters:
        - `row_delim` sets the characters to separate the rows. Default: \r\n.
        - `col_delim` sets the character to separate the columns. Default: ','.
        - `with_types` indicates whether the type row should be included. Default: false.
        - `quoting` indicates whether the column content is quoted with double quotes (and double quotes will be doubled). Default: true
        - `encoding` sets the encoding used to set the file's encoding. Default: UTF8.
- `Factory`
  - Provides two factory methods to create an outputter without types and an outputter with types.

##Usage Examples

    OUTPUT @res USING new HeaderOutputter.HeaderOutputter(quoting:true, with_types:true, encoding:Encoding.Unicode); 
    OUTPUT @res USING HeaderOutputter.Factory.Columns(col_delim:'\t');
    OUTPUT @res USING HeaderOutputter.Factory.ColumnsAndTypes(col_delim:'\t');
