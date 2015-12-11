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
using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Linq;
using System.Xml;

namespace Microsoft.Analytics.Samples.Formats.Xml
{
    /// <summary>XML outputter</summary>
    /// <remarks>Outputters inherit from IOutputter and optionally include 
    /// the SqlUserDefinedOutputter attribute.
    /// 
    /// They write a single SQLIP row to a byte stream. Given a SQLIP rowset,
    /// the XML outputter produces a sequence of XML fragments because of this,
    /// one fragment per row.
    /// 
    /// For example, given this row and the schema (a string, b string, c string):
    /// ("1", "foo", "bar")
    /// ("2", null, "")
    /// 
    /// The outputter will produce
    /// <![CDATA[
    /// <row><a>1</a><b>foo</b><c>bar</c></row>
    /// <row><a>2</a><c/></row>]]>
    /// 
    /// Notice that an empty string produces an empty element,
    /// and null produces a missing element.
    /// Notice that this outputter doesn't require atomic output,
    /// since it produces xml fragments as opposed to a root node
    /// </remarks>
    [SqlUserDefinedOutputter(AtomicFileProcessing = false)]
    public class XmlOutputter : IOutputter
    {
        /// <summary>Name of the XML element that will contain columns from a single row.</summary>
        private string rowPath;
        
        /// <summary>For each column, map from the column name to the XML element name</summary>
        private SqlMap<string, string> columnPaths;
        
        /// <summary>Settings for the XML writer</summary>
        /// <remarks>Because IOuputters output one row at a time, this code
        /// outputs XML fragments -- one per row -- instead of a single document.</remarks>
        private XmlWriterSettings fragmentSettings = new XmlWriterSettings
        {
            ConformanceLevel = ConformanceLevel.Fragment,
            OmitXmlDeclaration = true
        };
        
        /// <summary>New instances are created at least once per vertex</summary>
        /// <param name="rowPath">Name of the XML element that will contain columns from a single row.</param>
        /// <remarks>The column names from the input rowset will be used as the column element names.
        /// Do not rely on static fields because their values are not shared across vertices.</remarks>
        public XmlOutputter(string rowPath)
            : this(rowPath, new SqlMap<string, string>())
        {
        }

        /// <summary>New instances are created at least once per vertex</summary>
        /// <param name="rowPath">Name of the XML element that will contain columns from a single row.</param>
        /// <param name="columnElements">For each column, map from the column name to the XML element name</param>
        /// <remarks>Do not rely on static fields because their values are not shared across vertices.</remarks>
        public XmlOutputter(string rowPath, SqlMap<string, string> columnPaths)
        {
            this.rowPath = rowPath;
            this.columnPaths = columnPaths;
        }

        /// <summary>Output is called at least once per instance</summary>
        /// <param name="input">A SQLIP row</param>
        /// <param name="output">Wrapper for a Stream</param>
        public override void Output(IRow input, IUnstructuredWriter output)
        {
            IColumn badColumn = input.Schema.FirstOrDefault(col => col.Type != typeof(string));
            if (badColumn != null)
            {
                throw new ArgumentException(string.Format("Column '{0}' must be of type 'string', not '{1}'", badColumn.Name, badColumn.Type.Name));
            }

            using (var writer = XmlWriter.Create(output.BaseStream, this.fragmentSettings))
            {
                writer.WriteStartElement(this.rowPath);
                foreach (IColumn col in input.Schema)
                {
                    var value = input.Get<string>(col.Name);
                    if (value != null)
                    {
                        // Skip null values in order to distinguish them from empty strings
                        writer.WriteElementString(this.columnPaths[col.Name] ?? col.Name, value);
                    }
                }
            }
        }
    }
}
