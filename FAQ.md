
# U-SQL

[http://aka.ms/usqlfaq](http://aka.ms/usqlfaq)


### Using the built-in U-SQL Extractors is there any way to record the input lines that are invalid so that they can be analyzed later?

You cannot capture errors with the built-in extractors. 

As a workaround, you can build your own extractor that uses a try catch and other checks and then add a column that stores the error and the input.*

NOTE: because you may have a large amount in information in the debug column, you can use byte[] or string. Note that strings have a maximum size of 128KB.

### Can I read/write files stored in ADLS or WASB using code that is running within a U-SQL User-defined Operator (UDO)?

No. This is disallowed.

### Q: Can I get a single value back from a rowset?

You may be trying to do something like ehat is shown below - and expect MaxDate to be a single scalar value

    @MaxSize = SELECT MAX(Size) AS MaxSize FROM input;

U-SQL does not support retrieving a single scalar value from a rowset. You can only retrieve another rowset.

Thus, @MaxSize is not a value of type long, but instead is a rowset with a single row and a single column of type long called MaxSize.

### Q: How can I do something like Excel's AVERAGEIF( )?

Use C# expression syntax, for example if the column you want (called foo) is of type int then use

    @rows = SELECT AVG(foo > 0 ? (int?)foo : (int?)null) AS AvgFoo 

### Q: How can a create an "empty" rowset - a rowset with zero rows?

You have several options:

_Option 1_: Create an empty table that you refer to when you need it in subsequent scripts. Note that since you are not inserting any data, you will not need to specify an index:

    CREATE TABLE Empty(a int, b int, c int); // Note: you still need to provide a schema
    
Then you can say:

    @empty = SELECT * FROM Empty;

_Option 2_: Create an empty rowset by reading from an empty file set:

    @empty = EXTRACT a int, b int, c int 
             FROM "{*}.doesnotexist"
             USING Extractors.Csv();
             
_Option 3_: Create an empty rowset by an always false predicate:

    @empty = SELECT * FROM (VALUES(1,2,3)) AS T(a,b,c) WHERE a != 1;

### Q: Can my U-SQL script make a network call to some other machine?

No. This is explicitly disallowed and unsupported. 

For a specific scenario, please file a request at http://aka.ms/adlfeedback.

### Q: How can I test if a stream exists in a U-SQL script?

There is no way to do this from within a U-SQL Script.

### Q: How can I do a correlated subquery like in SQL?

U-SQL  doesn't have correlated subqueries.

Most scenarios for correlated subquery can be expressed via a JOIN. Use that technique instead.

### Q: How can I Assign Unique IDs to rows?

Use Window Functions.

    @rs1 = SELECT *, ROW_NUMBER() OVER ( ) AS unique_id 
         FROM @unique_id;  

Do not use Guid.NewGuid() to create unique ids for rows since the Guid creation is not deterministic and can lead to job aborts if a node is getting retried.

### Q: How can get a random sampling of rows from a rowset?

U-SQL doesn't have a real random sampling method.

However if you only care about getting a subset of rows and don't really care about
truwe random sampling then you can use the ROW_NUMBER Window Function as shown below where only ever 
one out of thousand rows are returned.

    @rs1 = SELECT *, ROW_NUMBER() OVER ( ) AS rownum 
         FROM @rs0;
         
    @rs1 = SELECT * FROM @rs1
         WHERE ( (rownum % 1000) == 0 );  

### Q: Can I have a different schema for each row in a a U-SQL rowset?

No. A U-SQL RowSet must contains rows and each row must have the same schema: same column names, with the same types, in the same order. Consider using a SQL.MAP typed column for semistructured data.

If you want to union two rowsets with different schemas, null padd the missing columns in the SELECT clauses.

### Q: How can I Extract all the files in a folder?

You can do this by using the FileSet syntax.

    @rows =
        EXTRACT Name string, ID int
        FROM "/dimensiondata/carrier/{*}"
        USING Extractors.Csv();

# U-SQL Catalog

### Q: Is there an API to read or write a U-SQL table locally?

There is an API to read meta data object description that is being exposed in the REST APIs, the SDKs, the Portal and the ADL Tools in VisualStudio. 

The only supported way of creating a catalog object such as a table is to run a U-SQL script. The ADL Tools provide a UI to help you create the script.

There is currently no API to preview a table. Please submit a script that extracts the requested data, write it to a file and preview the file.

