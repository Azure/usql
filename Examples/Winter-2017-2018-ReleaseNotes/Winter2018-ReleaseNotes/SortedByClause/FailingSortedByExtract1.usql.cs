using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Winter2018ReleaseNotes
{
    public static class UpdatableRowExtensions
    {
        public static void SetColumnIfRequested<T>(this IUpdatableRow source, string colName, Func<T> expr)
        {
            var colIdx = source.Schema.IndexOf(colName);
            if (colIdx != -1)
            { source.Set<T>(colIdx, expr()); }
        }
    }

    public class SLExtractor : IExtractor {
        public override IEnumerable<IRow> Extract(IUnstructuredReader input, IUpdatableRow output)
        {
            foreach (Stream current in input.Split())
            {
                using (StreamReader streamReader = new StreamReader(current, Encoding.UTF8 ))
                {
                    string[] array = streamReader.ReadToEnd().Split(new string[]{"\t"}, StringSplitOptions.None);
 
                    output.SetColumnIfRequested("UserId", () => Int32.Parse(array[0]));
                    output.SetColumnIfRequested("Start", () => DateTime.Parse(array[1]));
                    output.SetColumnIfRequested("Region", () => (array[2]));
                    output.SetColumnIfRequested("Query", () => (array[3]));
                    output.SetColumnIfRequested("Duration", () => Int32.Parse(array[4]));
                    output.SetColumnIfRequested("Urls", () => (array[5]));
                    output.SetColumnIfRequested("ClickedUrls", () => (array[6]));
                }
                yield return output.AsReadOnly();
            }
        }
    }
}