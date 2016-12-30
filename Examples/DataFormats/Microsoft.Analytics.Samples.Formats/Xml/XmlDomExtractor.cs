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
    /// <summary>DOM-based XML extractor</summary>
    /// <remarks>Extractors inherit from IExtractor and optionally include 
    /// the SqlUserDefinedExtractor attribute.
    /// 
    /// They convert a sequence of bytes into a sequence of SQLIP rows.
    /// This extractor loads the bytes into a DOM so that it can support
    /// XPath specifications for its rows and columns.
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
    public class XmlDomExtractor : IExtractor
    {
        /// <summary>Path of the XML elements that contain columns.</summary>
        private string rowPath;

        /// <summary>For each column, map from the XML path to the column name</summary>
        private SqlMap<string, string> columnPaths;

        /// <summary>Map namespace prefixes to namespace URIs</summary>
        /// <remarks>If you have a default namespace (without prefix) in your XML document, 
        /// provide a prefix in the map for that namespace URI and use that prefix in the 
        /// XPath expression to select the nodes that are in the default namespace.</remarks>
        private SqlMap<string, string> namespaceDecls;

        /// <summary>New instances are constructed at least once per vertex</summary>
        /// <param name="rowPath">Path of the XML element that contains rows.</param>
        /// <param name="columnPaths">For each column, map from the XML path to the column name. 
        /// It is specified relative to the row element.</param>
        /// <param name="namespaceDecls">For each namespace URI in the document that you want to query, map the prefix to the namespace URI. 
        /// If you have a default namespace (without prefix) in your XML document, 
        /// provide a prefix in the map for that namespace URI and use that prefix in the 
        /// XPath expression to select the nodes that are in the default namespace. 
        /// If there is no namespace URI in the document, the map can be left null.</param>
        /// <remarks>Do not rely on static fields because their values will not cross vertices.</remarks>
        public XmlDomExtractor(string rowPath, SqlMap<string, string> columnPaths, SqlMap<string,string> namespaceDecls = null)
        {
            this.rowPath = rowPath;
            this.columnPaths = columnPaths;
            this.namespaceDecls = namespaceDecls;
        }
        
        /// <summary>Extract is called at least once per vertex</summary>
        /// <param name="input">Wrapper for a Stream</param>
        /// <param name="output">IUpdatableRow uses a mutable builder pattern -- 
        /// set individual fields with IUpdatableRow.Set, then build an immutable IRow by
        /// calling IUpdatableRow.AsReadOnly.</param>
        /// <returns>A sequence of IRows.</returns>
        public override IEnumerable<IRow> Extract(IUnstructuredReader input, IUpdatableRow output)
        {
            // Make sure that all requested columns are of type string
            IColumn column = output.Schema.FirstOrDefault(col => col.Type != typeof(string));
            if (column != null)
            {
                throw new ArgumentException(string.Format("Column '{0}' must be of type 'string', not '{1}'", column.Name, column.Type.Name));
            }

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(input.BaseStream);
            XmlNamespaceManager nsmanager = new XmlNamespaceManager(xmlDocument.NameTable);

            // If namespace declarations have been provided, add them to the namespace manager
            if (this.namespaceDecls != null)
            {
                foreach (var namespaceDecl in this.namespaceDecls)
                {
                    nsmanager.AddNamespace(namespaceDecl.Key, namespaceDecl.Value);
                }
            }

            foreach (XmlNode xmlNode in xmlDocument.DocumentElement.SelectNodes(this.rowPath, nsmanager))
            {
                // IUpdatableRow implements a builder pattern to save memory allocations, 
                // so call output.Set in a loop
                foreach(IColumn col in output.Schema)
                {
                    var explicitColumnMapping = this.columnPaths.FirstOrDefault(columnPath => columnPath.Value == col.Name);
                    XmlNode xml = xmlNode.SelectSingleNode(explicitColumnMapping.Key ?? col.Name, nsmanager);
                    output.Set(explicitColumnMapping.Value ?? col.Name, xml == null ? null : xml.InnerXml);
                }

                // then call output.AsReadOnly to build an immutable IRow.
                yield return output.AsReadOnly();
            }
        }
    }
}
