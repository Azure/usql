// 
// Copyright (c) Microsoft and contributors.  All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// 
// See the License for the specific language governing permissions and
// limitations under the License.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;

namespace Microsoft.Analytics.Samples.Formats.Xml
{
    /// <summary>Streaming XML extractor</summary>
    /// <remarks>Extractors inherit from IExtractor and optionally include 
    /// the SqlUserDefinedExtractor attribute.
    /// 
    /// They convert a sequence of bytes into a sequence of SQLIP rows.
    /// This extractor reads XML incrementally to avoid loading the whole
    /// document into memory. However, it does not support XPath.
    /// 
    /// For example, given this data and asked to produce the schema (a string, b string):
    /// <![CDATA[<row><a>foo</a><b>3</b></row><row><a/></row>]]>
    /// 
    /// The extractor produces
    /// ("col1", 2, "foo", "3")
    /// ("col1", 2, "", null)
    /// 
    /// Notice that an empty element produces an empty string,
    /// and a missing element produces null.
    /// </remarks>
    [SqlUserDefinedExtractor(AtomicFileProcessing = true)]
	public class XmlExtractor : IExtractor
	{
        /// <summary>Name of the XML element that contains rows.</summary>
		private string rowPath;

        /// <summary>For each column, map from the XML element name to the column name</summary>
        private SqlMap<string, string> columnPaths;

        /// <summary>New instances are constructed at least once per vertex</summary>
        /// <param name="rowElement">Name of the XML element that contains rows.</param>
        /// <param name="columnElements">For each column, map from the XML element name to the column name</param>
        /// <remarks>Do not rely on static fields because their values are not shared across vertices.</remarks>
        public XmlExtractor(string rowPath, SqlMap<string, string> columnPaths)
		{
			this.rowPath = rowPath;
			this.columnPaths = columnPaths;
		}

        /// <summary>The state names in the XML parser finite-state machine.</summary>
        private enum ParseLocation
		{
			Row,
			Column,
			Data
		}

        /// <summary>The current state in the XML parser finite-state machine.</summary>
        private class ParseState
		{
            /// <summary>The current location in the finite-state machine.</summary>
            public ParseLocation Location { get; set; }

            /// <summary>The current element name.</summary>
            /// <remarks>It will map to a column when its value is known.</remarks>
            public string ElementName { get; set; }
            
            /// <summary>XML writer for the current element value.</summary>
            /// <remarks>It is built up from the inner XML of an element, then written to a column.</remarks>
            public XmlWriter ElementWriter { get; set; }

            /// <summary>The current element value.</summary>
            private StringBuilder elementValue;

            /// <summary>Set up the element writer state when constructing a ParseState</summary>
            public ParseState()
			{
				this.elementValue = new StringBuilder();
				this.ElementWriter = XmlWriter.Create(
                    this.elementValue, 
                    new XmlWriterSettings { ConformanceLevel = ConformanceLevel.Fragment });
			}

            /// <summary>Jump to a different location and clear the currrent element buffer.</summary>
            public void ClearAndJump(ParseLocation location)
			{
				this.Location = location;
				this.ElementName = null;
				this.ClearElementValue();
			}
			
            /// <summary>Get the current element value and clear its buffer</summary>
            public string ReadElementValue()
			{
				this.ElementWriter.Flush();
				string s = this.elementValue.ToString();
				this.elementValue.Clear();
				return s;
			}
			
            /// <summary>Clear the buffer used for reading the current element value</summary>
            public void ClearElementValue()
			{
				this.ElementWriter.Flush();
				this.elementValue.Clear();
			}
		}

        /// <summary>Extract is called at least once per instance</summary>
        /// <param name="input">Wrapper for a Stream</param>
        /// <param name="output">IUpdatableRow uses a mutable builder pattern -- 
        /// set individual fields with IUpdatableRow.Set, then build an immutable IRow by
        /// calling IUpdatableRow.AsReadOnly.</param>
        /// <returns>IEnumerable of IRow, one IRow per SQLIP row.</returns>
		public override IEnumerable<IRow> Extract(IUnstructuredReader input, IUpdatableRow output)
		{
            // Make sure that all requested columns are of type string
            IColumn column = output.Schema.FirstOrDefault(col => col.Type != typeof(string));
            if (column != null)
            {
                throw new ArgumentException(string.Format("Column '{0}' must be of type 'string', not '{1}'", column.Name, column.Type.Name));
            }

			var state = new ParseState();
			state.ClearAndJump(ParseLocation.Row);
			using (var reader = XmlReader.Create(input.BaseStream))
			{
				while (reader.Read())
				{
					switch (state.Location)
					{
                        case ParseLocation.Row:
                            // when looking for a new row, we are only interested in elements
                            // whose name matches the requested row element
                            if (reader.NodeType == XmlNodeType.Element && reader.Name == this.rowPath)
                            {
                                // when found, clear the IUpdatableRow's memory
                                // (this is no provided Clear method)
                                for (int i = 0; i < output.Schema.Count; i++)
                                {
                                    output.Set<string>(i, null);
                                }

                                state.ClearAndJump(ParseLocation.Column);
                            }

                            break;
                        case ParseLocation.Column:
                            // When looking for a new column, we are interested in elements
                            // whose name is a key in the columnPaths map or
                            // whose name is in the requested output schema.
                            // This indicates a column whose value needs to be read, 
                            // so prepare for reading it by clearing elementValue.
                            if (reader.NodeType == XmlNodeType.Element
                                && (this.columnPaths.ContainsKey(reader.Name)
                                    || output.Schema.Select(c => c.Name).Contains(reader.Name)))
                            {
                                if (reader.IsEmptyElement)
                                {
                                    // For an empty element, set an empty string 
                                    // and immediately jump to looking for the next column
                                    output.Set(this.columnPaths[reader.Name] ?? reader.Name, state.ReadElementValue());
                                    state.ClearAndJump(ParseLocation.Column);
                                }
                                else
                                {
                                    state.Location = ParseLocation.Data;
                                    state.ElementName = reader.Name;
                                    state.ClearElementValue();
                                }
                            }
                            else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == this.rowPath)
                            {
                                // The other interesting case is an end element whose name matches 
                                // the current row element. This indicates the end of a row, 
                                // so yield the now-complete row and jump to looking for 
                                // another row.
                                yield return output.AsReadOnly();
                                state.ClearAndJump(ParseLocation.Row);
                            }

                            break;
                        case ParseLocation.Data:
                            // Most of the code for reading the value of a column
                            // deals with re-creating the inner XML from discrete elements.
                            // The only jump occurs when the reader hits an end element
                            // whose name matches the current column. In this case, we
                            // need to write the accumulated value to the appropriate 
                            // column in the output row.
                            switch (reader.NodeType)
                            {
                                case XmlNodeType.EndElement:
                                    if (reader.Name == state.ElementName)
                                    {
                                        output.Set(this.columnPaths[state.ElementName] ?? state.ElementName, state.ReadElementValue());
                                        state.ClearAndJump(ParseLocation.Column);
                                    }
                                    else
                                    {
                                        state.ElementWriter.WriteEndElement();
                                    }

                                    break;
                                case XmlNodeType.Element:
                                    state.ElementWriter.WriteStartElement(reader.Name);
                                    state.ElementWriter.WriteAttributes(reader, false);
                                    if (reader.IsEmptyElement)
                                    {
                                        state.ElementWriter.WriteEndElement();
                                    }

                                    break;
                                case XmlNodeType.CDATA:
                                    state.ElementWriter.WriteCData(reader.Value);
                                    break;
                                case XmlNodeType.Comment:
                                    state.ElementWriter.WriteComment(reader.Value);
                                    break;
                                case XmlNodeType.ProcessingInstruction:
                                    state.ElementWriter.WriteProcessingInstruction(reader.Name, reader.Value);
                                    break;
                                default:
                                    state.ElementWriter.WriteString(reader.Value);
                                    break;
                            }

                            break;
                        default:
                            throw new NotImplementedException("StreamFromXml has not implemented a new member of the ParseLocation enum");
                    }
				}

                if (state.Location != ParseLocation.Row)
				{
					throw new ArgumentException("XML document ended without proper closing tags");
				}
			}
		}
	}
}
