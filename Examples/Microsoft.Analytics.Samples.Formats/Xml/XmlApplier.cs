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
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Microsoft.Analytics.Samples.Formats.Xml
{
    /// <summary>DOM-based XML applier</summary>
    /// <remarks>Appliers inherit from IApplier and optionally include 
    /// the SqlUserDefinedApplier attribute.
    /// 
    /// They convert a single SQLIP row into a sequence of SQLIP rows.
    /// 
    /// For example, given this row and asked to read the third column:
    /// ("col1", 2, "<![CDATA[<row><a>foo</a><b>3</b></row><row><a/></row>]]>")
    /// 
    /// An applier with the requested schema (a string, b string) produces
    /// ("col1", 2, "foo", "3")
    /// ("col1", 2, "", null)
    /// 
    /// Notice that an empty element produces an empty string,
    /// and a missing element produces null.
    /// 
    /// </remarks>
    [SqlUserDefinedApplier]
    public class XmlApplier : IApplier
    {
        /// <summary>In the input row, the name of the column containing XML. The column must be a string.</summary>
        private string xmlColumnName;

        /// <summary>Path of the XML element that contains rows.</summary>
        private string rowPath;

        /// <summary>For each column, map from the XML path to the column name</summary>
        private SqlMap<string, string> columnPaths;

        /// <summary>New instances are constructed at least once per vertex</summary>
        /// <param name="xmlColumnName">In the input row, the name of the column containing XML. The column must be a string.</param>
        /// <param name="rowPath">Path of the XML element that contains rows.</param>
        /// <param name="columnPaths">For each column, map from the XML path to the column name. 
        /// It is specified relative to the row element.</param>
        /// <remarks>Arguments to appliers must not be column references. 
        /// The arguments must be able to be calculated at compile time.</remarks>
        public XmlApplier(string xmlColumnName, string rowPath, SqlMap<string, string> columnPaths)
        {
            this.xmlColumnName = xmlColumnName;
            this.rowPath = rowPath;
            this.columnPaths = columnPaths;
        }

        /// <summary>Apply is called at least once per instance</summary>
        /// <param name="input">A SQLIP row</param>
        /// <param name="output">A SQLIP updatable row.</param>
        /// <returns>IEnumerable of IRow, one IRow per SQLIP row.</returns>
        /// <remarks>Because applier constructor arguments cannot depend on
        /// column references, the name of the column to parse is given as a string. Then
        /// the actual column value is obtained by calling IRow.Get. The rest of the code
        /// is the same as XmlDomExtractor.</remarks>
        public override IEnumerable<IRow> Apply(IRow input, IUpdatableRow output)
        {
            // Make sure that all requested columns are of type string
            IColumn column = output.Schema.FirstOrDefault(col => col.Type != typeof(string));
            if (column != null)
            {
                throw new ArgumentException(string.Format("Column '{0}' must be of type 'string', not '{1}'", column.Name, column.Type.Name));
            }
            
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(input.Get<string>(this.xmlColumnName));
            foreach (XmlNode xmlNode in xmlDocument.DocumentElement.SelectNodes(this.rowPath))
            {
                // IUpdatableRow implements a builder pattern to save memory allocations, 
                // so call output.Set in a loop
                foreach(IColumn col in output.Schema)
                {
                    var explicitColumnMapping = this.columnPaths.FirstOrDefault(columnPath => columnPath.Value == col.Name);
                    XmlNode xml = xmlNode.SelectSingleNode(explicitColumnMapping.Key ?? col.Name);
                    output.Set(explicitColumnMapping.Value ?? col.Name, xml == null ? null : xml.InnerXml);
                }

                // then call output.AsReadOnly to build an immutable IRow.
                yield return output.AsReadOnly();
            }
        }
    }
}