using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

// TweetAnalysis Code Behind
// Show the use of a U-SQL user-defined function (UDF)
//
namespace TweetAnalysis
{
    public class Udfs
    {
        // SqlArray<string> get_mentions(string tweet)
        //
        // Returns a U-SQL array of string containing the twitter handles that were mentioned inside the tweet.
        //
        public static SqlArray<string> get_mentions(string tweet)
        {
            return new SqlArray<string>(
                tweet.Split(new char[] { ' ', ',', '.', ':', '!', ';', '"', '“' }).Where(x => x.StartsWith("@"))
                );
        }
    }
}
