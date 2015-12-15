#Tweet Analysis Sample

##Story Line

We receive a set of tweet files downloaded from http://tweetdownload.net and start out with doing some exploration of the data.

First, we do a [simple count of all the tweets per tweet authors in a single file](https://github.com/MicrosoftBigData/usql/blob/master/Examples/TweetAnalysis/TweetAnalysis/1-SimpleAnalysis-1File.usql). 
Next we also [investigate the mentions in the tweets](https://github.com/MicrosoftBigData/usql/blob/master/Examples/TweetAnalysis/TweetAnalysis/2-ExtractMentions-InlineCode-1File.usql).
We then [refactor the code into code-behind](https://github.com/MicrosoftBigData/usql/blob/master/Examples/TweetAnalysis/TweetAnalysis/3-ExtractMentions-CodeBehind-1File.usql) 
and [make it available for reuse in a registered assembly](https://github.com/MicrosoftBigData/usql/blob/master/Examples/TweetAnalysis/TweetAnalysis/4-ExtractMentions-RefAsm-1File.usql). 
Then we do the [analysis over all files and include some more detailed information about the lineage of the data](https://github.com/MicrosoftBigData/usql/blob/master/Examples/TweetAnalysis/TweetAnalysis/5-ExtractMentions-RefAsm-FileSet.usql) 
(who mentioned and which files did provide the tweet).

After we decided on the schema, we finally decide to [make the processed data on tweet authors and their mentions available as a table](https://github.com/MicrosoftBigData/usql/blob/master/Examples/TweetAnalysis/TweetAnalysis/6-CreateMentionTables.usql), 
and [write some adhoc analytical queries](https://github.com/MicrosoftBigData/usql/blob/master/Examples/TweetAnalysis/TweetAnalysis/7-TweetAnalysis-WindowingExpr.usql), that show that while Raghu is not a frequent tweeter, he is very influential :).

Note that versions of these sample queries were used in the [U-SQL introduction](http://blogs.msdn.com/b/visualstudio/archive/2015/09/28/introducing-u-sql.aspx) 
and [U-SQL UDF](http://blogs.msdn.com/b/visualstudio/archive/2015/10/28/writing-and-using-custom-code-in-u-sql-user-defined-functions.aspx) blog posts on the [VS MSDN blog](http://blogs.msdn.com/b/visualstudio/).
