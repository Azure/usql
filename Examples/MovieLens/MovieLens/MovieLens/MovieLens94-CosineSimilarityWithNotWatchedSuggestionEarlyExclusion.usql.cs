using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MovieLens
{

    public class udfs
    {

        static public int TestMovieInArr(Int32 Movie,SqlArray<int> MovieList)
        {
            foreach(int x in MovieList)
            {
                if (x == Movie)
                {
                    return 1;
                }
            }
            return 0;

        }
    }    
}
