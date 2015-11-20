# U-SQL End to End Examples

This folder contains several solutions, which demonstrate how to build U-SQL extractors.

##Microsoft.Analytics.Samples Solution
It contains projects, which demonstrates how to implement XML and JSON extractors.

##XmlExtractor.U-SqlSample Solution
This solution contains an end to end example which demonstrates on example of XML extractor:

1.  How to implement an Extractor (In this case XML extractor)
2.  How to write Unit Test for extractor
3.  How to deploy Extractor in Data Lake 
4.  How to write U-SQL script which uses extractor

### How to implement extractor ?
Following example shows implementation of extractor. For more information please take a look on Microsoft.Analytics.Samples solution.

```
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
```

### How to write Unit Test for extractor ?

To create a unit test for extractor, please create common unit test project. As next add references to following assemblies:

- Microsoft.Analytics.Interfaces

  Defines interfaces like: IRow, IExtractor, ISchema etc.

- Microsoft.Analytics.Types

    Contains SQL primitive types like: SQLBit, SqlInt16,...

-  Microsoft.Analytics.UnitTests

    Contains "Mock-Implementation"" of interfaces required for testing like USqlRow, USqlSchema, USqlRowset etc.

Unit Test method, which instantiates (uses) XmlDomExtractor can be implemented as shown in the code snippet below. This sample invokes a method Extract(,) by passing two arguments: IUnstructuredReader and IUpdatableRow. 
IUnstructuredReader defines the stream of data, which has to be structured by extractor.
IUpdatableRow defines rowset of data, which has to be filled by extractor.

```
                USqlStreamReader reader = new USqlStreamReader(st.BaseStream);

                USqlUpdatableRow updRow = getUpdatableRow();

                XmlDomExtractor extractor = new XmlDomExtractor("Locations/Location");

                var result = extractor.Extract(reader, updRow);

                int cnt = 0;
                foreach(var item in result)
                {
                    cnt++;
                    Debug.WriteLine(item.Get<long>("Id"));
                }


```

Before extractor is invoked by calling method Extract(,), the instance of implementation of IUpdatableRow has to be created. Following two methods demonstrate how to do that.


```
       private USqlUpdatableRow getUpdatableRow()
        {
            ISchema schema = new USqlSchema(getColumns());

            IRow row = new USqlRow(schema, new object[schema.Count]);

            USqlUpdatableRow updRow = new USqlUpdatableRow(row);
            return updRow;
        }

     private IColumn[] getColumns()
        {
            List<IColumn> columns = new List<IColumn>();

            columns.Add(new USqlColumn<long>("Id"));
            columns.Add(new USqlColumn<string>("City"));
            columns.Add(new USqlColumn<double>("ForecastTemp"));
            columns.Add(new USqlColumn<double>("ForecastHumidity"));

            return columns.ToArray();
        }

 
```

In this example following XML file is used (Contained in project XmlExtractor.UnitTests in folder 'TestFiles'):

```
<Root>
  <Locations>
    <Location>
      <Id>77</Id>
      <City>Frankfurt</City>
      <ForecastTemp>27.7</ForecastTemp>
      <ForecastHumidity>1200</ForecastHumidity>
    </Location>

    <Location>
      <Id>78</Id>
      <City>Berlin</City>
      <ForecastTemp>24.1</ForecastTemp>
      <ForecastHumidity>1100</ForecastHumidity>
    </Location>
     . . .
</Locations>
</Root>

```

The goal of this extractor is to read locations from XML file and to build a rowest equivalent to column array created by method getColumns(). In other words, that method returns IColumn[], which represents the schema of data contained in XML file or any other binary file. By implementing of extractors, we have to be able to grab all
columns from binary file. By writing of unit tests methods like getColumns() should deal with all possible columns, which can be extracted. For example, if there would be a column GeoLocation in XML file at path "/Location/GeoLocation/", test should include this column.

In the real execution process Azure Data Analytics creates schema (columns) on the fly during execution of the script. To understand this, take a look on following script:

```EXTRACT A int, B string, C double```


This script will create a schema of 3 columns: A, B and C. To emulate this in unit test we could use following code:


 ``` columns.Add(new USqlColumn<int>("A"));
            columns.Add(new USqlColumn<string>("B"));
            columns.Add(new USqlColumn<double>("C"));```



### How to deploy Extractor in Data Lake ?
To deploy Extractor to Azure Data Lake do following.

#### Create Database in DataLake
After you created a database you can expand database node in Server Explorer. 
Database will hold all customization assemblies, which you will have to register. There are two ways to register an assembly: Register Assembly by Server Explorer Wizard and Register Assembly from U-Sql script.

#### Register assembly by Server Explorer Wizard

Right Mouse click on node 'Assemblies' and then Register Assembly' . In the wizard, you can add a assembly from local disk, which is your build folder or you can upload it from cloud. After upload of assembly, you can set (change) 'Assembly Name'. This is the name, which will be later used by SQL statement:

 ```REFERENCE ASSEMBLY <assembly name>```

Note that wizard enables you to add dependent assemblies ('Managed Dependencies'). Use this button to add all dependent assemblies. Please note that you SHOULD NOT ass any assembly from namespace Microsoft.Analytics, because these are system assemblies, which already exist in database hive.

Add additional files can be used to upload any other file, which can be referred from the U-SQL script.

### How to write U-SQL script which uses extractor ?
At the end U-SQL script has to be implemented, which uses custom extractor.
If the assembly with extractor (or some other customization) is NOT uploaded (registered) in data lake database, you will have to do following.
Upload the assembly and all its dependencies to data lake storage and then use following statements in the script:

####Example I:

```
    USE sampledb1;
    CREATE ASSEMBLY asmXmlExtractor FROM @"mafs://accounts/sandbox/fs/DLLs/XmlExtractor.dll";      
    REFERENCE ASSEMBLY asmXmlExtractor;
```

####Example II:
Developers will most likely upload assemblies by using wizard. In that case U-SQL script does not require statement 'CREATE ASSEMBLY '
This is the script, which references the assembly:

```
    USE sampledb1;
    REFERENCE ASSEMBLY asmXmlExtractor;
```

'asmXmlExtractor' is the name which you set (or has been automatically set) by using wizard.

```
    USE sampledb1;
    REFERENCE ASSEMBLY asmXmlExtractor;
	
	DECLARE @INPUT_FILE string = "/Samples/XmlExtractor/TestFile1.xml";
	
	@XmlRows =
    EXTRACT Id int,
            City string,
            ForecastTemp double,
            ForecastHumidity string
    FROM @INPUT_FILE
    USING new XmlDomExtractor("Locations/Location");
```
