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
                        // For an extractor this is the pertinant code :
                        // Set column values and yield return 
                        for (int x = 0; x < colCount; x++)
                        {
                            output.Set<String>(x, strings[x].ToString());
                        }
                        
                        
                        yield return output.AsReadOnly();

                        for (int x = 0; x < colCount; x++)
                        {
                            strings[x].Clear();
                        }

                    }
                    EndOfLine = false;
                    if (c == -1) break;
                }


            }

        }
    }


    [SqlUserDefinedExtractor(AtomicFileProcessing = true)]
    public class MovieLensExtractorMeta : IExtractor
    {
        public const string NEWLINE = "\r\n";
        public MovieLensExtractorMeta()
        {
            
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

        public override IEnumerable<IRow> Extract(IUnstructuredReader input, IUpdatableRow output)
        {
            int colCount = output.Schema.Count;

            StringBuilder[] strings = new StringBuilder[colCount];

            for (int x = 0; x < colCount; x++)
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

                    if (v != colCount - 1)
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
                        // For an extractor this is the pertinant code :
                        // Set column values and yield return 
                        for (int x = 0; x < colCount; x++)
                        {
                            var s = output.Schema[x];
                            if (s.Type.Name.Equals("String", StringComparison.InvariantCultureIgnoreCase))
                            {
                                output.Set(x, strings[x].ToString());
                            }
                            if (s.Type.Name.Equals("Int32", StringComparison.InvariantCultureIgnoreCase))
                            {
                                try {
                                    output.Set(x, Int32.Parse(strings[x].ToString()));
                                }
                                catch
                                {
                                    output.Set(x, -1);

                                }
                            }
                            strings[x].Clear();

                        }
                        yield return output.AsReadOnly();
                        

                    }
                    EndOfLine = false;
                    if (c == -1) break;
                }


            }

        }
    }

}
