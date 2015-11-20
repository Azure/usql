using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Analytics.Interfaces;

[SqlUserDefinedExtractor(AtomicFileProcessing = true)]
public class XmlDomExtractor : IExtractor
{
    private string m_XPath;
     
    public XmlDomExtractor(string xPath)
    {
        this.m_XPath = xPath;
    }

    public override IEnumerable<IRow> Extract(IUnstructuredReader input, IUpdatableRow output)
    {
        List<IRow> rows = new List<IRow>();

        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.Load(input.BaseStream);
        foreach (XmlNode xmlNode in xmlDocument.DocumentElement.SelectNodes(this.m_XPath))
        {
            foreach (IColumn col in output.Schema)
            {
                XmlNode xml = xmlNode.SelectSingleNode(col.Name);
                if (xml != null)
                {
                    object val = Convert.ChangeType(xml.InnerXml, col.Type);
                    output.Set(col.Name, val);
                }
            }

            yield return output.AsReadOnly();

        }
    }
}
