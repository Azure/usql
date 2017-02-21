using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MVA_MaxParallelActivitiesReducer
{
    public class RangeReducer : IReducer
    {
        public override IEnumerable<IRow> Reduce(IRowset input, IUpdatableRow output)
        {
            int acc = 0;
            int max = 0;

            foreach (var row in input.Rows)
            {

                var timestamp = row.Get<DateTime>("timestamp");
                var op = row.Get<string>("op");
                if (op == "start")
                {
                    acc++;
                }
                else
                {
                    acc--;
                    if (acc < 0)
                    {
                        acc = 0;
                    }
                }

                max = System.Math.Max(max, acc);

            }

            output.Set<string>("cohort", "FOO");
            output.Set<int>("max", max);

            yield return output.AsReadOnly();

        }
    } 
}
