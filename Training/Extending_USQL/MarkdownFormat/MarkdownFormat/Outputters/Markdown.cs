using System.Collections.Generic;
using System.Linq;

using USQLTYPES = Microsoft.Analytics.Types.Sql;
using USQLINTERFACES = Microsoft.Analytics.Interfaces;

namespace MarkdownFormat
{
    [USQLINTERFACES.SqlUserDefinedOutputter(AtomicFileProcessing = true)]
    public class MarkdownOutputter : USQLINTERFACES.IOutputter
    {
        private int row_count;
        public bool OutputHeader;
        public bool OutputHeaderType;

        public MarkdownOutputter( bool outputHeader = false, bool outputHeaderType = false)
        {
            row_count = 0;
            this.OutputHeader = outputHeader;
            this.OutputHeaderType = outputHeaderType;
        }

        public override void Close()
        {
        }

        public override void Output(USQLINTERFACES.IRow row, USQLINTERFACES.IUnstructuredWriter output)
        {
            var streamWriter = new System.IO.StreamWriter(output.BaseStream);

            // Metadata schema initialization to enumerate column names
            var schema = row.Schema;


            if (this.row_count == 0)
            {
                if (this.OutputHeader)
                {
                    streamWriter.Write("|");
                    for (int i = 0; i < schema.Count(); i++)
                    {
                        var col = schema[i];
                        streamWriter.Write(" ");
                        streamWriter.Write(col.Name);
                        streamWriter.Write(" ");
                        if (this.OutputHeaderType)
                        {
                            streamWriter.Write(get_usql_type_name(col.Type));
                            streamWriter.Write(" ");
                        }
                        streamWriter.Write("|");
                    }
                    streamWriter.Write("\r\n");
                    streamWriter.Flush();

                    streamWriter.Write("|");
                    for (int i = 0; i < schema.Count(); i++)
                    {
                        var col = schema[i];
                        streamWriter.Write(" ");
                        streamWriter.Write("---");
                        streamWriter.Write(" ");
                        streamWriter.Write("|");
                    }
                    streamWriter.Write("\r\n");
                    streamWriter.Flush();
                }
            }

            // Data row output
            streamWriter.Write("|");
            for (int i = 0; i < schema.Count(); i++)
            {
                var col = schema[i];
                string val = "";

                try
                {
                    var coltype = col.Type;
                    if (coltype == typeof(string))
                    {
                        val = row.Get<string>(col.Name);
                        val = val ?? "NULL";
                    }
                    else if (coltype == typeof(bool))
                    {
                        val = row.Get<bool>(col.Name).ToString();
                    }
                    else if (coltype == typeof(char))
                    {
                        val = row.Get<char>(col.Name).ToString();
                    }
                    else if (coltype == typeof(float))
                    {
                        val = row.Get<float>(col.Name).ToString();
                    }
                    else if (coltype == typeof(double))
                    {
                        val = row.Get<double>(col.Name).ToString();
                    }
                    else if (coltype == typeof(int))
                    {
                        val = row.Get<int>(col.Name).ToString();
                    }
                    else if (coltype == typeof(long))
                    {
                        val = row.Get<long>(col.Name).ToString();
                    }
                    else if (coltype == typeof(System.Guid))
                    {
                        val = row.Get<System.Guid>(col.Name).ToString();
                    }
                    else if (coltype == typeof(int?))
                    {
                        val = row.Get<int?>(col.Name).ToString();
                        val = val ?? "NULL";
                    }
                    else if (coltype == typeof(long?))
                    {
                        val = row.Get<long?>(col.Name).ToString();
                        val = val ?? "NULL";
                    }
                    else if (coltype == typeof(float?))
                    {
                        val = row.Get<float?>(col.Name).ToString();
                        val = val ?? "NULL";
                    }
                    else if (coltype == typeof(double?))
                    {
                        val = row.Get<double?>(col.Name).ToString();
                        val = val ?? "NULL";
                    }
                    else if (coltype == typeof(USQLTYPES.SqlArray<string>))
                    {
                        val = _Get_val_from_usqlarray<string>(row, col, val);
                    }
                    else if (coltype == typeof(USQLTYPES.SqlArray<int>))
                    {
                        val = _Get_val_from_usqlarray<int>(row, col, val);
                    }
                    else if (coltype == typeof(USQLTYPES.SqlArray<long>))
                    {
                        val = _Get_val_from_usqlarray<long>(row, col, val);
                    }
                    else if (coltype == typeof(USQLTYPES.SqlMap<string,string>))
                    {
                        val = _Get_val_from_usqlmap<string,string>(row, col, val);
                    }
                    else if (coltype == typeof(USQLTYPES.SqlMap<string, int?>))
                    {
                        val = _Get_val_from_usqlmap<string, int?>(row, col, val);
                    }
                    else
                    {
                        val = "UNKNOWNTYPE:" + get_usql_type_name(coltype);
                    }

                }
                catch (System.NullReferenceException exc)
                {
                    // Handling NULL values--keeping them empty
                    val = "NullReferenceException";
                }
                streamWriter.Write(" ");
                streamWriter.Write(val);
                streamWriter.Write(" ");
                streamWriter.Write("|");
            }
            streamWriter.Write("\n");
            streamWriter.Flush();

            this.row_count++;
        }

        private static string _Get_val_from_usqlarray<T>(USQLINTERFACES.IRow row, USQLINTERFACES.IColumn col, string val)
        {
            var arr = row.Get<USQLTYPES.SqlArray<T>>(col.Name);

            if (arr != null)
            {
                var sb = new System.Text.StringBuilder();
                sb.Append("SqlArray<");
                sb.Append(get_usql_type_name(typeof(T)));
                sb.Append(">{ ");

                for (int j = 0; j < arr.Count; j++)
                {
                    if (j > 0)
                    {
                        sb.Append(", ");
                    }

                    sb.Append("\"");
                    sb.Append(arr[j].ToString());
                    sb.Append("\"");
                }

                sb.Append(" }");
                val = sb.ToString();
            }
            else
            {
                val = "NULL";
            }
            return val;
        }

        private static string _Get_val_from_usqlmap<K,V>(USQLINTERFACES.IRow row, USQLINTERFACES.IColumn col, string val)
        {
            var map = row.Get<USQLTYPES.SqlMap<K,V>>(col.Name);

            if (map != null)
            {
                var sb = new System.Text.StringBuilder();
                sb.Append("SqlMap<");
                sb.Append(get_usql_type_name(typeof(K)));
                sb.Append(", ");
                sb.Append(get_usql_type_name(typeof(V)));
                sb.Append(">{ ");

                int kn = 0;
                foreach (var key in map.Keys)
                {
                    if (kn > 0)
                    {
                        sb.Append("; ");
                    }

                    V xval = map[key];
                    string val_str = "NULL";
                    if (xval != null)
                    {
                        val_str = xval.ToString();
                    }

                    var key_str = key.ToString();
                    sb.AppendFormat("{0}={1}", key_str, val_str);
 
                    kn++;
                }

                sb.Append(" }");
                val = sb.ToString();
            }
            else
            {
                val = "NULL";
            }
            return val;
        }

        private static string get_usql_type_name(System.Type coltype)
        {
            if (coltype == typeof(string))
            {
                return "string";
            }
            else if (coltype == typeof(char))
            {
                return "char";
            }
            else if (coltype == typeof(float))
            {
                return "float";
            }
            else if (coltype == typeof(double))
            {
                return "double";
            }
            else if (coltype == typeof(int))
            {
                return "int";
            }
            else if (coltype == typeof(long))
            {
                return "long";
            }
            else if (coltype == typeof(System.Guid))
            {
                return "Guid";
            }
            else if (coltype == typeof(int?))
            {
                return "int?";
            }
            else if (coltype == typeof(long?))
            {
                return "long?";
            }
            else if (coltype == typeof(float?))
            {
                return "float?";
            }
            else if (coltype == typeof(double?))
            {
                return "double?";
            }
            else
            {
                return coltype.Name.Replace("`", "-");
            }
        }

    }
}