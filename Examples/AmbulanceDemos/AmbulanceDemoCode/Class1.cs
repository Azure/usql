using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbulanceDemoCode
{
    // DriverFunctions
    //
    // defines a class of utility functions that will be used by the Extractor and can be used as UDFs
    public class DriverFunctions
    {
        // string AddQuotes(string s)
        //
        // returns s with embedded in " and containing " are doubled.
        public static string AddQuotes(string s)
        {
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }

        // string RemoveOptionalQuotes(string s)
        //
        // Removes Quotes from string s (and de-duplicate contained "") if the string is embedded in "
        public static string RemoveOptionalQuotes(string s)
        {
            return (s.Length > 1 && s[0] == '"' && s[s.Length - 1] == '"') ? s.Substring(1, s.Length - 2).Replace("\"\"", "\"") : s;
        }

        // SqlMap<string, string> ReadStringMap(string val, string map_item_delim = ",", string map_kv_delim = ":")
        //
        // transforms the input string val into a SQL.MAP instance using the provided delimiters to separate key-value pairs and the key and value in each pair.
        // Both the key and value types are string.
        public static SqlMap<string, string> ReadStringMap(string val, string map_item_delim = ",", string map_kv_delim = ":")
        {
            return new SqlMap<string, string>(
                from p in val.Split(new string[]
				{
					map_item_delim
				}, StringSplitOptions.None)
                select new KeyValuePair<string, string>(p.Split(new string[]
				{
					map_kv_delim
				}, StringSplitOptions.None)[0], p.Split(new string[]
				{
					map_kv_delim
				}, StringSplitOptions.None)[1]));
        }

        // SqlArray<int> ReadIntArray(string val, string array_item_delim = ",")
        //
        // returns a SQL.ARRAY<int> from the input string val using the provided array item delimiter.
        public static SqlArray<int> ReadIntArray(string val, string array_item_delim = ",")
        {
            return new SqlArray<int>(
                from x in val.Split(new string[]
				{
					array_item_delim
				}, StringSplitOptions.None)
                select Convert.ToInt32(x));
        }

        // string WriteQuotedStringMap(SqlMap<string, string> m, string map_item_delim = ",", string map_kv_delim = ":")
        //
        // transforms a SQL.MAP<string, string> into a quoted string, using the provided delimiters to delimit keys and values and key-value pairs.
        public static string WriteQuotedStringMap(SqlMap<string, string> m, string map_item_delim = ",", string map_kv_delim = ":")
        {
            return "\"" + string.Join(map_item_delim,
                from p in m
                select string.Format("{0}{1}{2}", p.Key, map_kv_delim, p.Value)) + "\"";
        }

        // string WriteQuotedIntArray(SqlArray<int> a, string array_item_delim = ",")
        //
        // transforms a SQL.ARRAY<int> into a quoted string using the provided array item delimiter.
        public static string WriteQuotedIntArray(SqlArray<int> a, string array_item_delim = ",")
        {
            return "\"" + string.Join<int>(array_item_delim, a) + "\"";
        }
    }

    // DriverExtractor
    //
    // Defines a user-defined extractor that can supports reading CSV-like data into SQL.MAP<string,string> columns and SQL.ARRAY<int> columns.
    // Extractor assume homogeneous row formats and can be parallelized
    //
    // Usage (after registration of assembly and referencing assembly in script, default values shown):
    //   EXTRACT ... FROM ... 
    //   USING new AmbulanceDemoCode.DriverExtractor(row_delim:"\r\n", col_delim: ",",map_item_delim: ",", map_kv_delim:":", array_item_delim:",", encoding:Encoding.UTF8);
    //
    [SqlUserDefinedExtractor(AtomicFileProcessing=false)]
    public class DriverExtractor : IExtractor
    {

        // Class variables that are set with the class initializer
        private byte[] _row_delim;
        private string _col_delim;
        private string _map_item_delim;
        private string _map_kv_delim;
        private string _array_item_delim;
        private Encoding _encoding;

        // DriverExtractor(string row_delim = "\r\n", string col_delim = ",", string map_item_delim = ",", string map_kv_delim = ":", string array_item_delim = ",", Encoding encoding = null)
        //
        // Class initializer that provides optional extractor parameters.
        public DriverExtractor(string row_delim = "\r\n", string col_delim = ",", string map_item_delim = ",", string map_kv_delim = ":", string array_item_delim = ",", Encoding encoding = null)
        {
            this._encoding = ((encoding == null) ? Encoding.UTF8 : encoding);
            this._row_delim = this._encoding.GetBytes(row_delim);
            this._col_delim = col_delim;
            this._map_item_delim = map_item_delim;
            this._map_kv_delim = map_kv_delim;
            this._array_item_delim = array_item_delim;
        }

        // void OutputValueAtCol_I(string c, int i, IUpdatableRow outputrow)
        // 
        // Helper function that takes the string value c and puts it into the column at position i in the output row.
        // The value will be cast to the expected type of the column.
        private void OutputValueAtCol_I(string c, int i, IUpdatableRow outputrow)
        {
            ISchema schema = outputrow.Schema;
            if (schema[i].Type == typeof(SqlMap<string, string>))
            {
                c = DriverFunctions.RemoveOptionalQuotes(c);
                SqlMap<string, string> scopeMap = String.IsNullOrEmpty(c) ? null : DriverFunctions.ReadStringMap(c, this._map_item_delim, this._map_kv_delim);
                outputrow.Set<SqlMap<string, string>>(i, scopeMap);
            }
            else if (schema[i].Type == typeof(SqlArray<int>))
            {
                c = DriverFunctions.RemoveOptionalQuotes(c);
                SqlArray<int> scopeArray = String.IsNullOrEmpty(c) ? null : DriverFunctions.ReadIntArray(c, this._array_item_delim);
                outputrow.Set<SqlArray<int>>(i, scopeArray);
            }
            else if (schema[i].Type == typeof(int))
            {
                int num = Convert.ToInt32(c);
                outputrow.Set<int>(i, num);
            }
            else if (schema[i].Type == typeof(int?))
            {
                int? num2 = (c == "") ? null : new int?(Convert.ToInt32(c));
                outputrow.Set<int?>(i, num2);
            }
            else if (schema[i].Type == typeof(long))
            {
                long num3 = Convert.ToInt64(c);
                outputrow.Set<long>(i, num3);
            }
            else if (schema[i].Type == typeof(long?))
            {
                long? num4 = (c == "") ? null : new long?(Convert.ToInt64(c));
                outputrow.Set<long?>(i, num4);
            }
            else if (schema[i].Type == typeof(DateTime))
            {
                DateTime dateTime = Convert.ToDateTime(c);
                outputrow.Set<DateTime>(i, dateTime);
            }
            else if (schema[i].Type == typeof(DateTime?))
            {
                DateTime? dateTime2 = (c == "") ? null : new DateTime?(Convert.ToDateTime(c));
                outputrow.Set<DateTime?>(i, dateTime2);
            }
            else if (schema[i].Type == typeof(string))
            {
                string text = DriverFunctions.RemoveOptionalQuotes(c);
                outputrow.Set<string>(i, text);
            }
            else
            {
                outputrow.Set<string>(i, c);
            }
        }

        // IEnumerable<IRow> Extract(IUnstructuredReader input, IUpdatableRow outputrow)
        //
        // Actual implementation of DriverExtractor that overwrites the Extract method of IExtractor.
        public override IEnumerable<IRow> Extract(IUnstructuredReader input, IUpdatableRow outputrow)
        {
            foreach (Stream current in input.Split(this._row_delim))
            {
                using (StreamReader streamReader = new StreamReader(current, this._encoding))
                {
                    int num = 0;
                    string[] array = streamReader.ReadToEnd().Split(new string[]{this._col_delim}, StringSplitOptions.None);
                    for (int i = 0; i < array.Length; i++)
                    {
                        string c = array[i];
                        this.OutputValueAtCol_I(c, num++, outputrow);
                    }
                }
                yield return outputrow.AsReadOnly();
            }
            yield break;
        }
    }

    // DriverOutputter
    //
    // Defines a user-defined outputter that can supports writing CSV-like data from SQL.MAP<string,string> columns and SQL.ARRAY<int> columns.
    // Outputter will write homogeneous row formats and can be parallelized
    //
    // Usage (after registration of assembly and referencing assembly in script, default values shown):
    //   EXTRACT ... FROM ... 
    //   USING new AmbulanceDemoCode.DriverOutputter(row_delim:"\r\n", col_delim: ",",map_item_delim: ",", map_kv_delim:":", array_item_delim:",", encoding:Encoding.UTF8);
    //
    [SqlUserDefinedOutputter]
    public class DriverOutputter : IOutputter
    {
        // Class variables that get set by class initializer
        private string _row_delim;
        private string _col_delim;
        private string _map_item_delim;
        private string _map_kv_delim;
        private string _array_item_delim;
        private Encoding _encoding;

        // DriverOutputter(string row_delim = "\r\n", string col_delim = ",", string map_item_delim = ",", string map_kv_delim = ":", string array_item_delim = ",", Encoding encoding = null)
        //
        // Class initializer that provides optional outputter parameters.
        public DriverOutputter(string row_delim = "\r\n", string col_delim = ",", string map_item_delim = ",", string map_kv_delim = ":", string array_item_delim = ",", Encoding encoding = null)
        {
            this._encoding = ((encoding == null) ? Encoding.UTF8 : encoding);
            this._row_delim = row_delim;
            this._col_delim = col_delim;
            this._map_item_delim = map_item_delim;
            this._map_kv_delim = map_kv_delim;
            this._array_item_delim = array_item_delim;
        }

        // void WriteValue(object val, StreamWriter writer)
        //
        // Helper function that takes a value val and writes it into the output stream. It will convert the value to a string serialization.
        private void WriteValue(object val, StreamWriter writer)
        {
            if (val is SqlMap<string, string>)
            {
                writer.Write(DriverFunctions.WriteQuotedStringMap(val as SqlMap<string, string>, this._map_item_delim, this._map_kv_delim));
            }
            else if (val is SqlArray<int>)
            {
                writer.Write(DriverFunctions.WriteQuotedIntArray(val as SqlArray<int>, this._array_item_delim));
            }
            else if (val is string)
            {
                writer.Write(DriverFunctions.AddQuotes(val as string));
            }
            else
            {
                writer.Write(val);
            }
        }

        // void Output(IRow row, IUnstructuredWriter output)
        //
        // Actual implementation of DriverOutputter that overwrites the Output method of IOutputter.
        public override void Output(IRow row, IUnstructuredWriter output)
        {
            using (StreamWriter streamWriter = new StreamWriter(output.BaseStream, this._encoding))
            {
                streamWriter.NewLine = this._row_delim;
                ISchema schema = row.Schema;
                for (int i = 0; i < schema.Count; i++)
                {
                    object val = row.Get<object>(i);
                    if (i > 0)
                    {
                        streamWriter.Write(this._col_delim);
                    }
                    this.WriteValue(val, streamWriter);
                }
                streamWriter.WriteLine();
            }
        }
    }

    // EnglishCountryNames
    //
    // Sample user-defined processor that translates country names into english country names.
    [SqlUserDefinedProcessor]
    public class EnglishCountryNames : IProcessor
    {
        // Private mapping table. Could be intialized from a user-provided resource file (left as exercise to the reader).
        private static IDictionary<string, string> CountryTranslation = new Dictionary<string, string>
		{
			
			{
				"Deutschland",
				"Germany"
			},
			
			{
				"Schwiiz",
				"Switzerland"
			},
			
			{
				"UK",
				"United Kingdom"
			},
			
			{
				"USA",
				"United States of America"
			},
			
			{
				"中国",
				"PR China"
			}
		};

        // IRow Process(IRow input, IUpdatableRow output)
        // 
        // Actual implementatoin of the user-defined processor. Overwrites the Process method of IProcessor.
        public override IRow Process(IRow input, IUpdatableRow output)
        {
            string text = input.Get<string>("country");
            if (EnglishCountryNames.CountryTranslation.Keys.Contains(text))
            {
                text = EnglishCountryNames.CountryTranslation[text];
            }
            output.Set<string>("country", text);
            return output.AsReadOnly();
        }
    }

    // MapPivoter
    //
    // Sample Processor that pivots keys from SQL.MAPs<string, T> columns into its own column.
    // It will take the first key it finds among the SQL.MAP columns and remove the key from the map.
    [SqlUserDefinedProcessor]
    public class MapPivoter : IProcessor
    {
        // IRow Process(IRow input, IUpdatableRow output)
        // 
        // Actual implementatoin of the user-defined processor. Overwrites the Process method of IProcessor.
        public override IRow Process(IRow input, IUpdatableRow output)
        {
            List<string> list = new List<string>();
            foreach (var current in input.Schema)
            {
                if (current.Type.IsGenericType && current.Type.GetGenericTypeDefinition() == typeof(SqlMap) && current.Type.GetGenericArguments()[0] == typeof(string))
                {
                    list.Add(current.Name);
                }
            }

            Dictionary<string, ArrayList> maps_to_be_changed = new Dictionary<string, ArrayList>();
            foreach (var current2 in output.Schema)
            {
                bool flag = list.Contains(current2.Name);
                if (-1 < input.Schema.IndexOf(current2.Name) && !flag)
                {
                    output.Set<object>(current2.Name, input.Get<object>(current2.Name));
                }
                else if (!flag)
                {
                    foreach (string current3 in list)
                    {
                        SqlMap<string, string> sqlMap = input.Get<SqlMap<string, string>>(current3);
                        SqlArray<string> sqlArray = null;
                        List<string> list2 = null;
                        if (sqlMap != null)
                        {
                            sqlArray = sqlMap.Keys;
                            if (sqlMap.Values != null)
                            {
                                list2 = sqlMap.Values.ToList<string>();
                            }
                        }
                        int num = (sqlArray == null) ? -1 : sqlArray.ToList<string>().IndexOf(current2.Name);
                        if (num != -1)
                        {
                            output.Set<string>(current2.Name, list2[num]);
                            if (maps_to_be_changed.Keys.Contains(current3))
                            {
                                maps_to_be_changed[current3].Add(current2.Name);
                            }
                            else
                            {
                                maps_to_be_changed.Add(current3, new ArrayList
								{
									current2.Name
								});
                            }
                            break;
                        }
                        output.Set<object>(current2.Name, current2.Type.IsValueType ? Activator.CreateInstance(current2.Type) : null);
                    }
                }
            }

            using (IEnumerator<IColumn> enumerator = output.Schema.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    IColumn out_col = enumerator.Current;
                    bool flag = list.Contains(out_col.Name);
                    if (flag)
                    {
                        SqlMap<string, string> sqlMap = input.Get<SqlMap<string, string>>(out_col.Name);
                        if (maps_to_be_changed != null && maps_to_be_changed.Keys.Contains(out_col.Name))
                        {
                            sqlMap = new SqlMap<string, string>(
                                from kvp in sqlMap
                                where !maps_to_be_changed[out_col.Name].Contains(kvp.Key)
                                select kvp);
                        }
                        output.Set<SqlMap<string, string>>(out_col.Name, sqlMap);
                    }
                }
            }
            return output.AsReadOnly();
        }
    }

}
