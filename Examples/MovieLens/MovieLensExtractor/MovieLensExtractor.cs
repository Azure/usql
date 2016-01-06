using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieLensExtractor
{

    [SqlUserDefinedExtractor(AtomicFileProcessing = true)]
    public class MovieLensExtractor : IExtractor
    {
        public const string NEWLINE = "\r\n";
        private int colCount; 
        public MovieLensExtractor(int ColCount)
        {
            this.colCount = ColCount;
        }

      
        public bool hasEnded(ref StringBuilder str)
        {
            if (str.Length <= 2)
            {
                return false;
            }
            if (str[str.Length - 1] == ':' && str[str.Length - 2] == ':')
            {
                return true;
            }
            return false;

        }
        public bool hasEndedEOL(ref StringBuilder str)
        {
            if (str.Length == 0)
            {
                return false;
            }
            if (str[str.Length - 1] == 0x10)
            {
                return true;
            }
            return false;

        }
        public override IEnumerable<IRow> Extract(IUnstructuredReader input, IUpdatableRow output)
        {
            StringBuilder[] strings = new StringBuilder[colCount];

            for(int x = 0; x < colCount; x++)
            {
                strings[x] = new StringBuilder();
            }
            int v = 0;
            bool EndOfLine = false;
            while (true)
            {
                
                int c = input.BaseStream.ReadByte();
                if (c != -1)
                {
                    
                    if (v != colCount-1)
                    {
                        strings[v].Append((char)c);
                        if (hasEnded(ref strings[v]))
                        {
                            strings[v].Length = strings[v].Length - 2;

                            v++;
                        }

                    }
                    else
                    {
                        if (c == 10)
                        {
                            EndOfLine = true;

                        }
                        else
                        {
                            strings[v].Append((char)c);
                        }
                    }
                }
                if (EndOfLine || c == -1)
                {
                    if (v != 0)
                    {
                        v = 0;
                        for (int x = 0; x < colCount; x++)
                        {
                            output.Set<string>(x,strings[colCount].ToString());
                            strings[colCount].Clear();
                        }
                        
                        yield return output.AsReadOnly();
                        
                    }
                    EndOfLine = false;
                    if (c == -1) break;
                }


            }

        }
    }
    [SqlUserDefinedExtractor(AtomicFileProcessing = true)]
    public class UserExtractor : MovieLensExtractor    {

        UserExtractor() :base(5)
        {
             
        }
       
    }
    [SqlUserDefinedExtractor(AtomicFileProcessing = true)]
    public class MovieExtractor : MovieLensExtractor
    {
        MovieExtractor() : base(3) { }
       
    }
    [SqlUserDefinedExtractor(AtomicFileProcessing = true)]
    public class RatingsExtractor : MovieLensExtractor
    {
        RatingsExtractor() : base(4){}

       
    }
}
