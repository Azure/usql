using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReduceSample
{
    [SqlUserDefinedReducer(IsRecursive = true)]                                                                        // not sure if it can run recursive yet. Need to test with large data sets.
    public class RangeReducer : IReducer
    {
        public override IEnumerable<IRow> Reduce(IRowset input, IUpdatableRow output)
        {
            // Init aggregation values
            bool first_row_processed = false;
            var begin = DateTime.MaxValue; // Dummy value to make compiler happy
            var end = DateTime.MinValue; // Dummy value to make compiler happy

            // requires that the reducer is PRESORTED on begin and READONLY on the reduce key.
            foreach (var row in input.Rows)
            {
                // Initialize the first interval with the first row if i is 0
                if (!first_row_processed)
                {
                    first_row_processed = true; // mark that we handled the first row
                    begin = row.Get<DateTime>("begin");
                    end = row.Get<DateTime>("end");
                    // If the end is just a time and not a date, it can be earlier than the begin, indicating it is on the next day.
                    // This let's fix up the end to the next day in that case
                    if (end < begin) { end = end.AddDays(1); }
                }
                else
                {
                    var b = row.Get<DateTime>("begin");
                    var e = row.Get<DateTime>("end");
                    // fix up the date if end is earlier than begin
                    if (e < b) { e = e.AddDays(1); }

                    // if the begin is still inside the interval, increase the interval if it is longer
                    if (b <= end)
                    {
                        // if the new end time is later than the current, extend the interval
                        if (e > end) { end = e; }
                    }
                    else // output the previous interval and start a new one
                    {
                        output.Set<DateTime>("begin", begin);
                        output.Set<DateTime>("end", end);
                        yield return output.AsReadOnly();
                        begin = b; end = e;
                    } // if
                } // if
            } // foreach

            // now output the last interval
            output.Set<DateTime>("begin", begin);
            output.Set<DateTime>("end", end);
            yield return output.AsReadOnly();
        } // Reduce

    } // RangeReducer
} // ReduceSample
