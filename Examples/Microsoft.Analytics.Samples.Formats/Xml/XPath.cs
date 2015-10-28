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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using Microsoft.Analytics.Types.Sql;

namespace Microsoft.Analytics.Samples.Formats.Xml
{
    /// <summary>The XPath functions provide XPath 1.0 querying on strings.</summary>
    /// <remarks>Unlike Hive, the XPath query is not cached per-statement.
    /// You can cache it yourself with a DECLARE statement:
    ///     DECLARE @xpath = "a/b";
    ///     SELECT XPath.String(xmlColumn, @xpath) AS stringValue FROM table;</remarks>
    public static class XPath
    {
        /// <summary>Return an array of strings containing XML that match the XPath query</summary>
        /// <param name="xml">String containing XML</param>
        /// <param name="xpath">XPath query</param>
        /// <returns>Array of strings containing XML that match the xpath query</returns>
        /// <remarks>The query returns XmlNode.InnerXml, so attribute text is not returned</remarks>
        public static SqlArray<string> FindNodes(string xml, string xpath)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return new SqlArray<string>();
            }

            return FindNodes(Load(xml), xpath);
        }
        
        /// <summary>Return an array of strings containing XML that match the XPath query</summary>
        /// <param name="xml">String containing XML</param>
        /// <param name="xpath">XPath query</param>
        /// <returns>Array of strings containing XML that match the xpath query</returns>
        /// <remarks>The query returns XmlNode.InnerXml, so attribute text is not returned</remarks>
        public static SqlArray<SqlArray<string>> FindNodes(string xml, params string[] xpaths)
        {
            if (string.IsNullOrEmpty(xml) || xpaths.Length == 0)
            {
                return new SqlArray<SqlArray<string>>();
            }

            XmlNode doc = Load(xml);
            return new SqlArray<SqlArray<string>>(xpaths.Select(xpath => FindNodes(doc, xpath)));
        }
 
        /// <summary>Return an array of strings containing text that match the XPath query</summary>
        /// <param name="xml">String containing XML</param>
        /// <param name="xpath">XPath query</param>
        /// <returns>Array of strings that match the xpath query</returns>
        /// <remarks>The query must return text nodes or attributes -- 
        /// otherwise this function returns an empty array.</remarks>
        public static SqlArray<string> Evaluate(string xml, string xpath)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return new SqlArray<string>();
            }

            return Evaluate(Load(xml), xpath);
        }
 
        /// <summary>Return an array of array of strings that match multiple XPath queries</summary>
        /// <param name="xml">String containing XML</param>
        /// <param name="xpath">XPath query</param>
        /// <returns>Array of array of strings that match the xpath query</returns>
        /// <remarks>The queries must return text nodes or attributes -- 
        /// otherwise this function returns an empty array for that query.</remarks>
        public static SqlArray<SqlArray<string>> Evaluate(string xml, params string[] xpaths)
        {
            if (string.IsNullOrEmpty(xml) || xpaths.Length == 0)
            {
                return new SqlArray<SqlArray<string>>();
            }

            XmlNode doc = Load(xml);
            return new SqlArray<SqlArray<string>>(xpaths.Select(xpath => Evaluate(doc, xpath)));
        }

        /// <summary>Return an array of strings that match the XPath query</summary>
        /// <param name="root">Root of the XML to query</param>
        /// <param name="xpath">XPath query</param>
        /// <returns>Array of strings that match the xpath query</returns>
        /// <remarks>The query must return text nodes or attributes -- 
        /// otherwise this function returns an empty array.</remarks>
        private static SqlArray<string> Evaluate(XmlNode root, string xpath) 
        {
            var nodes = root.SelectNodes(xpath).Cast<XmlNode>();
            if (nodes.All(node => node.NodeType == XmlNodeType.Text || node.NodeType == XmlNodeType.Attribute))
            {
                return new SqlArray<string>(nodes.Select(node => node.InnerText));
            }
            else
            {
                return new SqlArray<string>();
            }
        }
        
        /// <summary>Return an array of strings that match the XPath query</summary>
        /// <param name="root">Root of the XML to query</param>
        /// <param name="xpath">XPath query</param>
        /// <returns>Array of strings that match the xpath query</returns>
        /// <remarks>The query must return text nodes or attributes -- 
        /// otherwise this function returns an empty array.</remarks>
        private static SqlArray<string> FindNodes(XmlNode root, string xpath) 
        {
            return new SqlArray<string>(root.SelectNodes(xpath).Cast<XmlNode>().Select(node => node.InnerXml));
        }

        /// <summary>Utility to load XML from a string</summary>
        private static XmlNode Load(string xml)
        {
            var d = new XmlDocument();
            d.LoadXml(xml);
            return d;
        }
    }
}
