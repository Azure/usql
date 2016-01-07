# Instructor-led Lab: U-SQL Introduction 

# Introduction

The purpose of this lab is to give you a taste of the new Big Data query language U-SQL. 
We assume you have installed the Azure Data Lake Tool for Visual Studio or have access to the Azure Data Lake Analytics Portal and have been given access to the U-SQL Lab account. The lab below will only show you the language and scripts and will not show you how to use the Azure Data Lake Tool or the portal. Your instructor will show you how to run the scripts through the Azure Data Lake Tool and/or the Azure Data Lake Analytics Portal.

# What is U-SQL?
U-SQL is the Big Data query language of the Azure Data Lake Analytics service. 

It evolved out of our internal Big Data language called SCOPE and combines a familiar SQL-like declarative language with the extensibility and programmability provided by C# types and the C# expression language and big data processing concepts such as "schema on reads", custom processors and reducers.

It is however not ANSI SQL nor T-SQL. For starters, its keywords such as SELECT have to be in UPPERCASE. It’s type system and expression language inside select clauses, where predicates etc is C#. This means the data types are C# types and use C# NULL semantics, and the comparison operations inside a predicate follow C# syntax (e.g., a == "foo").

#How do I write U-SQL?
In the current batch service of Azure Data Lake Analytics, U-SQL is written and executed as a batch script. It follows the following general pattern: 

1.	Retrieve data from stored locations in rowset format
	-	Stored locations can be files that will be schematized on read with EXTRACT expressions
	-	Stored locations can be U-SQL tables that are stored in a schematized format
2.	Transform the rowset(s)
	-	Several transformations over the rowsets can be composed in a expression flow format
3.	Store the transformed rowset data
	-	Store it in a file with an OUTPUT statement
	-	Store it in a U-SQL table

In addition, it also supports Data Definition statements such as CREATE TABLE to create metadata artefacts.

#Your first U-SQL Script

Please copy the following U-SQL script into the ADL Tool or query window on your portal, adjust your output filename to make it unique, and submit it. 

	@searchlog =
	    EXTRACT UserId          int,
	            Start           DateTime,
	            Region          string,
	            Query           string,
	            Duration        int?,
	            Urls            string,
	            ClickedUrls     string
	    FROM "/Samples/Data/SearchLog.tsv"
	    USING Extractors.Tsv();
	
	OUTPUT @searchlog   
	TO "/output/<replace_this_with_your_output_name>.csv"
	USING Outputters.Csv();

This U-SQL script has no transformation step. It reads from an input file called SearchLog.tsv, schematizing it while reading and the outputs the intermediate rowset back into the file whose name you specified. The Duration field could be null or of type int, while the UserId cannot be null. Note that the C# string type is always nullable.

Some concepts that this script introduces are:

1. **Rowset variables**: Each query expression that produces a rowset can be assigned to a variable. Variables in U-SQL follow the T-SQL variable naming pattern of @ followed by a name (@searchlog in this case). Note that assignment is not forcing the execution. It is merely naming the expression and gives you the ability to build-up more complex expressions.

2. **EXTRACT** gives you the ability to define a schema on read. The schema is specified by a column name and C# type name pair per column. It uses a so called extractor that can be written by the user. In this case though we are using the built-in Tsv extractor that is provided by the Extractors class since the input data is not comma but TAB separated.

3. **OUTPUT** takes a rowset and serializes it as a comma-separated file into the specified location. Again the outputter can be written by the user, but we are using the built-in Csv outputter provided by the Outputters class.

Assuming you are running on a correctly setup lab account, you should now have a file which contains the result of the query.

You can use scalar variables as well to make your script maintenance easier. In the following we are moving the file paths into two string variables:

	DECLARE @in  string = "/Samples/Data/SearchLog.tsv";
	DECLARE @out string = "/output/<replace_this_with_your_output_name>.csv";
	
	@searchlog =
	    EXTRACT UserId          int,
	            Start           DateTime,
	            Region          string,
	            Query           string,
	            Duration        int?,
	            Urls            string,
	            ClickedUrls     string
	    FROM @in
	    USING Extractors.Tsv();
	
	OUTPUT @searchlog   
	TO @out
	USING Outputters.Csv();

#Transforming your Rowset

A rowset can be transformed by applying U-SQL SELECT expressions. First let's do a simple filter. Since the data is in a file and you have to produce a file on output, you will always have an EXTRACT and OUTPUT as part of your U-SQL Script (see later for some different options).

	@searchlog =
	    EXTRACT UserId          int,
	            Start           DateTime,
	            Region          string,
	            Query           string,
	            Duration        int?,
	            Urls            string,
	            ClickedUrls     string
	    FROM "/Samples/Data/SearchLog.tsv"
	    USING Extractors.Tsv();
	
	@rs1 =
	    SELECT Start, Region, Duration
	    FROM @searchlog
	    WHERE Region == "en-gb";
	
	OUTPUT @rs1   
	TO "/output/<replace_this_with_your_output_name>.csv"
	USING Outputters.Csv();

Note that the WHERE clause is using a boolean C# expression and thus the comparison operation is == (and not the = sign you may be familiar with from traditional SQL).

You can also perform more complex filters by combining them with logical conjunctions (ands) and disjunctions (ors) and can even use the full might of the C# expression language to do your own expressions and functions. U-SQL provides support for both AND and OR which will reorder the predicate conditions and && and || which provide order guarantee and short cutting.

The following query makes use of the DateTime.Parse() method because there is no C# literal for the DateTime type and uses a conjunction.

	@searchlog =
	    EXTRACT UserId          int,
	            Start           DateTime,
	            Region          string,
	            Query           string,
	            Duration        int?,
	            Urls            string,
	            ClickedUrls     string
	    FROM "/Samples/Data/SearchLog.tsv"
	    USING Extractors.Tsv();
	
	@rs1 =
	    SELECT Start, Region, Duration
	    FROM @searchlog
	    WHERE Region == "en-gb";
	
	@rs1 =
	    SELECT Start, Region, Duration
	    FROM @rs1
	    WHERE Start >= DateTime.Parse("2012/02/16") AND Start <= DateTime.Parse("2012/02/17");
	
	OUTPUT @rs1   
	TO "/output/<replace_this_with_your_output_name>.csv"
	USING Outputters.Csv();

Note that the query is operating on the result of the first rowset and thus the result is a composition of the two filters. You can also reuse a variable name and the names are scoped lexically.

#Ordering, Grouping and Aggregation

Often you may want to perform some analytics as part of your queries. U-SQL provides you with the familiar ORDER BY, GROUP BY and aggregations:

	DECLARE @outpref string = "/output/<replace_this_with_your_output_name>";
	DECLARE @out1    string = @outpref+"_agg.csv";
	DECLARE @out2    string = @outpref+"_top5agg.csv";
	
	@searchlog =
	    EXTRACT UserId          int,
	            Start           DateTime,
	            Region          string,
	            Query           string,
	            Duration        int?,
	            Urls            string,
	            ClickedUrls     string
	    FROM "/Samples/Data/SearchLog.tsv"
	    USING Extractors.Tsv();
	
	@rs1 =
	    SELECT
	        Region,
	        SUM(Duration) AS TotalDuration
	    FROM @searchlog
	    GROUP BY Region;
	
	@res =
	    SELECT *
	    FROM @rs1
	    ORDER BY TotalDuration DESC
	    FETCH 5 ROWS;
	
	OUTPUT @rs1
	TO @out1
	ORDER BY TotalDuration DESC
	USING Outputters.Csv();
	
	OUTPUT @res
	TO @out2 
	ORDER BY TotalDuration DESC
	USING Outputters.Csv();

The above query finds the total duration per region and then outputs the top 5 durations in order.
U-SQL's rowsets do not preserve their order for the next query. Thus, if you want an ordered output, please add the order by to the OUTPUT statement as shown above. To avoid giving the impression that U-SQL's ORDER BY provides ordering beyond the ability to order a result to take the first or last N rows in a SELECT, U-SQL's ORDER BY has to be combined with the FETCH clause in a SELECT expression.
You can also use the HAVING clause to restrict the output to groups that satisfy the HAVING condition:

	@searchlog =
	    EXTRACT UserId          int,
	            Start           DateTime,
	            Region          string,
	            Query           string,
	            Duration        int?,
	            Urls            string,
	            ClickedUrls     string
	    FROM "/Samples/Data/SearchLog.tsv"
	    USING Extractors.Tsv();
	
	@res =
	    SELECT
	        Region,
	        SUM(Duration) AS TotalDuration
	    FROM @searchlog
	    GROUP BY Region
	    HAVING SUM(Duration) > 200;
	
	OUTPUT @res
	TO "/output/<replace_this_with_your_output_name>.csv"
	ORDER BY TotalDuration DESC
	USING Outputters.Csv();

# Creating a database, a view, a table-valued function, and a table

If you don't want to always read from files or write to files, you can use the U-SQL metadata objects to add additional abstractions. You create them in the context of a database and schema. Every U-SQL script will always run with a default database (master) and default schema (dbo) as its default context. You can create your own database and/or schema and can change the context using the USE statement.

But let's start with encapsulating parts of the queries above for future sharing with views and table-valued functions.

# Creating a view

You can encapsulate a U-SQL expression in a view for future reuse.
Since we always used the same extract in the examples above, it makes sense to create a view to encapsulate it and store it for reuse in the U-SQL meta data catalog.

The following script creates a view SearchlogView in the default database and schema:

	DROP VIEW IF EXISTS SearchlogView;
	CREATE VIEW SearchlogView AS  
	    EXTRACT UserId          int,
	            Start           DateTime,
	            Region          string,
	            Query           string,
	            Duration        int?,
	            Urls            string,
	            ClickedUrls     string
	    FROM "/Samples/Data/SearchLog.tsv"
	    USING Extractors.Tsv();

Note that the first statement drops an already existing definition of the view and then creates the version that we want to use. 

This now gives us the ability to use the view without having to worry on how to schematize the data everytime and use it, instead of the EXTRACT expression, in the earlier scripts. E.g.,

	@res =
	    SELECT
	        Region,
	        SUM(Duration) AS TotalDuration
	    FROM SearchlogView
	    GROUP BY Region
	    HAVING SUM(Duration) > 200;
	
	OUTPUT @res
	TO "/output/<replace_this_with_your_output_name>.csv"
	ORDER BY TotalDuration DESC
	USING Outputters.Csv();

# Creating a table-valued function

If you want to encapsulate several statements or want to add parameterization, you can create a table-valued function. 

The following script creates a function RegionalSearchlog() in the default database and schema that adds a @region parameter for filtering the previously specified view on the Region. The parameter is defaulted to "en-gb".

	DROP FUNCTION IF EXISTS RegionalSearchlog;
	CREATE FUNCTION RegionalSearchlog(@region string = "en-gb") 
	RETURNS @searchlog TABLE
	  (
	            UserId          int,
	            Start           DateTime,
	            Region          string,
	            Query           string,
	            Duration        int?,
	            Urls            string,
	            ClickedUrls     string
	  )
	AS BEGIN 
	  @searchlog =
	    SELECT * FROM SearchlogView
	    WHERE Region == @region;
	END;

The first statement drops an already existing definition of the function and then creates the version that we want to use. Now you can use the function as in the following case where we get the Start, Region and Duration for the default region:

	@rs1 =
	    SELECT Start, Region, Duration
	    FROM RegionalSearchlog(DEFAULT) AS S;

	OUTPUT @rs1   
	TO "/output/<replace_this_with_your_output_name>.csv"
	USING Outputters.Csv();
	      
# Creating a table

Creating a table is similar to creating a table in a relational database such as SQL Server. You either create a table with a predefined schema or create a table and infer the schema from the query that populates the table (also known as CREATE TABLE AS SELECT or CTAS).

The benefit of a table over just a view or table-valued function is that the data is stored in a more optimized format for the query processor to operate on: The data is indexed and partitioned and stores the data in its native data type representation.

Now let's decide to persist the searchlog data in a schematized format in a table called Searchlog in your own database. The script 

1.	Creates the database (please use your name or another unique name)
2.	Sets the context to the created database
3.	Creates the table. To show two ways of creating a table, we actually create two tables:
	a.	SearchLog1 is created apriori
	b.	SearchLog2 is created based on the View that encapsulates the extraction expression (basically a CTAS)
Note that for scalability, **U-SQL requires you to define the index** for the table before you can insert. 
4.	Insert the data into SearchLog1.

		DROP DATABASE IF EXISTS <insert your name>;
		CREATE DATABASE <insert your name>;
		USE DATABASE <insert your name>;
		
		DROP TABLE IF EXISTS SearchLog1;
		DROP TABLE IF EXISTS SearchLog2;
		
		CREATE TABLE SearchLog1 (
		            UserId          int,
		            Start           DateTime,
		            Region          string,
		            Query           string,
		            Duration        int?,
		            Urls            string,
		            ClickedUrls     string,
		
		            INDEX sl_idx CLUSTERED (UserId ASC) 
		                  PARTITIONED BY HASH (UserId)
		  );
		
		INSERT INTO SearchLog1 SELECT * FROM master.dbo.SearchlogView;
		
		CREATE TABLE SearchLog2(
		       INDEX sl_idx CLUSTERED (UserId ASC) 
		             PARTITIONED BY HASH (UserId)
		) AS SELECT * FROM master.dbo.SearchlogView; // You can use EXTRACT or SELECT in the AS clause

# Querying from a Table

You can now query the tables in the same way you queried over the unstructured data. Instead of creating a rowset using EXTRACT, you now can just refer to the table name. Taking the earlier transformations, the script looks like (please update the database name to the name you chose in the earlier step):
	
	@rs1 =
	    SELECT
	        Region,
	        SUM(Duration) AS TotalDuration
	    FROM <insert your DB name>.dbo.SearchLog2
	    GROUP BY Region;
	
	@res =
	    SELECT *
	    FROM @rs1
	    ORDER BY TotalDuration DESC
	    FETCH 5 ROWS;
	
	OUTPUT @res
	TO "/output/<replace_this_with_your_output_name>.csv"
	ORDER BY TotalDuration DESC
	USING Outputters.Csv();

Note that you currently cannot run a SELECT on a table in the same script as the script where you create that table.

# Joining Data

U-SQL provides you most of the common join operators such as INNER JOIN, LEFT/RIGHT/FULL OUTER JOIN, SEMI JOIN etc. to join not only tables but any rowsets -- even those produced from files. 

	@adlog =
	    EXTRACT UserId int,
	            Ad string,
	            Clicked int
	    FROM "/Samples/Data/AdsLog.tsv"
	    USING Extractors.Tsv();
	
	@join =
	    SELECT a.Ad, s.Query, s.Start AS Date
	    FROM @adlog AS a JOIN <insert your DB name>.dbo.SearchLog1 AS s 
	                     ON a.UserId == s.UserId
	    WHERE a.Clicked == 1;
	
	OUTPUT @join   
	TO "/output/<replace_this_with_your_output_name>.csv"
	USING Outputters.Csv();

The above script joins the searchlog with an ad impression log and gives us the ads for the query string for a given date.

Some notes:

1.	U-SQL only supports the ANSI compliant join syntax: Rowset1 JOIN Rowset2 ON predicate. The old syntax of FROM Rowset1, Rowset2 WHERE predicate is NOT supported.
2.	The predicate in a JOIN has to be an equality join and no expression. If you want to use an expression, add it to a previous rowset's select clause. If you want to do a different comparison, you can move it into the WHERE clause.

#Conclusion

This lab has hopefully given you a small taste of U-SQL. As you would expect, there are many more advanced features that the introduction lab cannot cover, such as 
-	How to use CROSS APPLY to unpack parts of strings, arrays and maps into rows
-	operating over partitioned sets of data (file sets and partitioned tables)
-	writing your own user defined operators such as extractors, outputters, processors, user-defined aggregators in C#
-	how to use the U-SQL windowing functions
-	how to further manage your U-SQL code with views, table-valued functions and stored procedures
-	how to run arbitrary custom code on your processing nodes
-	how to connect to SQL Azure databases and federate queries across them and your U-SQL and Azure Data Lake data.

We hope you come back and use Azure Data Lake Analytics and U-SQL for your Big Data processing needs!
