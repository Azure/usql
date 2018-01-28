# U-SQL Examples 

This directory contains U-SQL sample solutions and sample data. 

Unless otherwise noted, they are provided as VisualStudio solutions. 

- [***Sample Data***](Samples/Data) : Contains sample data sets.

The individual projects are:

- [***Data Formats***](DataFormats) : Contains samples for working with
data formats such as XML and JSON.

- [***Ambulance Demo Samples***](AmbulanceDemo) : Contains several
topical demo scripts for operating on the AmbulanceDemo samples.
It covers topics such as federated queries over WASB and SQL data, file
sets, partitioned tables, custom extractors and processors, windowing
functions etc. Note that the federated query samples need additional
access and resources that may not be accessible from your account.

- [***U-SQL Hands-On Lab Samples***](IntroHOL-USQL) : Contains the
scripts from the [U-SQL Hands-On Lab](http://aka.ms/usql-hol).

- [***Tweet Analysis Samples***](TweetAnalysis) : Contains scripts doing
tweet analysis (see also VS Blog entries
[U-SQL Introduction](http://blogs.msdn.com/b/visualstudio/archive/2015/09/28/introducing-u-sql.aspx)
and
[U-SQL UDF](http://blogs.msdn.com/b/visualstudio/archive/2015/10/28/writing-and-using-custom-code-in-u-sql-user-defined-functions.aspx)
).

- [***Outputting Column Names***](HeaderOutputter) : Contains custom
outputter that writes the column names and optionally their type of
a rowset into a file.

- [***Handle Encoding Issues***](HandleEncoding) : Contains sample
scripts that help dealing with various data encoding issues. Currently,
the only available sample is FixEncodingErrors.
