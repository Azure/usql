using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace Samples
{
    public class SampleExtractor : IExtractor
    {
        private Encoding _encoding;
        private byte[] _row_delim;
        private char _col_delim;

        public SampleExtractor(Encoding encoding, string row_delim = "\r\n", char col_delim = '\t')
        {
            this._encoding = ((encoding == null) ? Encoding.UTF8 : encoding);
            this._row_delim = this._encoding.GetBytes(row_delim);
            this._col_delim = col_delim;

        }

        public override IEnumerable<IRow> Extract(IUnstructuredReader input, IUpdatableRow output)
        {
            string line;
            //Read the input line by line
            foreach (Stream current in input.Split(_encoding.GetBytes("\r\n")))
            {
                using (StreamReader streamReader = new StreamReader(current, this._encoding))
                {
                    line = streamReader.ReadToEnd().Trim();
                    //Split the input by the column delimiter
                    string[] parts = line.Split(this._col_delim);
                    int count = 0;
                    foreach (string part in parts)
                    {
                        //If its the second column, treat it in a special way, split the column into first name and last name columns
                        if (count == 1)
                        {
                            string[] name = part.Trim().Split(' ');
                            output.Set<string>(count, name[0]);
                            count += 1;
                            output.Set<string>(count, name[1]);
                        }
                        else
                        {
                            output.Set<string>(count, part);
                        }
                        count += 1;
                    }

                }
                yield return output.AsReadOnly();
            }
            yield break;
        }
    }

    public class SampleFunction
    {
        public static bool HasOfficePhone(string phonenumbers)
        {
            return phonenumbers.Contains("office:");
        }
    }

    //Sample Processor to generate First Initial and last name
    [SqlUserDefinedProcessor]
    public class NameProcessor : IProcessor
    {
        // IRow Process(IRow input, IUpdatableRow output)
        // 
        // Actual implementatoin of the user-defined processor. Overwrites the Process method of IProcessor.
        public override IRow Process(IRow input, IUpdatableRow output)
        {
            string first_name = input.Get<string>("first_name");
            string last_name = input.Get<string>("last_name");
            string name = first_name.Substring(0, 1) + "." + last_name;
            output.Set<string>("name", name);
            output.Set<int>("id", Int32.Parse(input.Get<string>("id")));
            output.Set<string>("zipcode", input.Get<string>("zipcode"));
            output.Set<string>("country", input.Get<string>("country"));
            return output.AsReadOnly();
        }
    }

    //User defined aggregate to calculate the total balance by adding or subtracting based on whether its credit or debit
    public class SampleAggregate : IAggregate<string, int, int>
    {
        int balance;
        
        public override void Init()
        {
            balance = 0;
        }
        
        public override void Accumulate(string transaction, int amount)
        {
            if(transaction == "Credit")
            {
                balance += amount;
            }
            if(transaction == "Debit")
            {
                balance -= amount;
            }
        }

        public override int Terminate()
        {
            return balance;
        }

    }
}
