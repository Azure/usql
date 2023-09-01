# U-SQL Avro Example
This example demonstrates how you can use U-SQL to analyze data stored in Avro files.

## Build
1. Clone the U-SQL repo (https://github.com/Azure/usql.git)
1. From the cloned repo open usql/Examples/DataFormats/Microsoft.Analytics.Samples.sln in Visual Studio 2017
2. Build the Microsoft.Analytics.Samples solution

### Register assemblies
1. Copy the following files from your build directory to a directory in Azure Data Lake Store (e.g. \Assemblies\Avro):
  * Microsoft.Analytics.Samples.Formats.dll
  * Avro.dll
  * log4net.dll
  * Newtonsoft.Json.dll
2. Create a database (e.g. run 1-CreateDB.usql.cs), switch to the new database 
3. Check file paths in 2-RegisterAssemblies.usql and update them if necessary
4. register the assemblies which have previously been uploaded to ADLS by submitting 2-RegisterAssemblies.usql

### Upload sample data
1. Get an Avro sample file which contains twitter data from [here](../Samples/Data/Avro/twitter.avro).
2. Use the Azure Data Lake Explorer (in Visual Studio, or the Azure Portal) or any other ADLS client to upload twitter.avro to a directory in Azure Data Lake Store (e.g. /TwitterStream/2016/12/twitter.avro)
3. Check file paths in 3-SimpleAvro.usql and update them if necessary

### Run the sample job
1. Submit 3-SimpleAvro.usql and wait for the U-SQL to finish.
