This sample project provides examples of using U-SQL to parse JSON files using
the user defined functions and operators provided in the
[Microsoft.Analytics.Samples.Formats](https://github.com/Azure/usql/tree/master/Examples/DataFormats/Microsoft.Analytics.Samples.Formats/Json)
project. There are two examples provided

1.       Parsing a JSON file with a flat structure
(radiowebsite/small\_radio\_json.json).

2.       Parsing a JSON file that contains nested structures (donut.json).

This project helps you get started on using U-SQL to process JSON files, you can
use this as starter code for JSON processing.

 

Building
========

This project has been tested for building using Visual Studio 2015. Please make
sure you install the [Azure Data Lake Tools for Visual
Studio](https://www.microsoft.com/en-us/download/details.aspx?id=49504) and also
the Azure SDK using the [Web Platform
Installer](https://www.microsoft.com/web/downloads/platform.aspx).

You will also need to build the
[Microsoft.Analytics.Samples.Formats](https://github.com/Azure/usql/tree/master/Examples/DataFormats/Microsoft.Analytics.Samples.Formats/Json)
project. After building the project, make sure you register the assemblies
Microsoft.Analytics.Samples.Formats.dll and Newtonsoft.Json.dll.

For more information on registering assemblies either via the Visual Studio UI
or in U-SQL code, please visit the Deploying section of the
[readme](https://github.com/Azure/usql/blob/master/Examples/DataFormats/Microsoft.Analytics.Samples.Formats/readme.md)
file.

Once you are done, you can simply open the sln file in Visual Studio or use the
command line to build the samples project.

 

Copying data files
==================

To ensure you are able to run the samples successfully, please copy the data
files to the /Samples/Output location using the instructions below.

Running locally on your machine
-------------------------------

1.       Make sure you know your Local Run DataRoot directory. In Visual Studio,
click on Data Lake-\>Options and Settings, in the Options dialog, look for the
value of Local Run DataRoot folder, all your data files used in your U-SQL code
will be expressed as relative paths from this folder.

 
2.       In your DataRoot directory, make sure you have the files copied in the
same folders that are referenced in your U-SQL code.

 

In this example, in your DataRoot directory,

·         Data for the first sample (JsonParsing.usql) is provided in
/Examples/Samples/json/radiowebsite/small\_radio\_json.json.

·         Data for the second example (NestedJsonParsing.usql) is provided in
/Examples/Samples/json/donut.json.

 

Running on your ADLA account
----------------------------

Make sure your code is copied in the same folder structure as listed above in
your default Azure Data Lake Store account.

 

Questions, Comments or Feedback
===============================

Please mail <usql@microsoft.com> with your questions, comments or feedback.
