# U-SQL Avro Example
This example demonstrates how you can use U-SQL to analyze data stored in Avro files.

## Deploying
The Avro Extractor requires Microsoft.Analytics.Samples.Formats and an updated version of the Microsoft.Hadoop.Avro library which can be found [here](https://github.com/flomader/hadoopsdk).

1. Download the latest version of Microsoft.Hadoop.Avro.zip from [here]( https://github.com/flomader/hadoopsdk/releases).
2. Extract Microsoft.Hadoop.Avro.dll from Microsoft.Hadoop.Avro.zip
3. Clone and open the Microsoft.Analytics.Samples.Formats solution in Visual Studio.
4. Update the reference of the file Microsoft.Hadoop.Avro.dll
5. Build the Microsoft.Analytics.Samples.Formats solution

### Register assemblies
1. Copy the following files to a directory in Azure Data Lake Store (e.g. \Assemblies\Avro):
  * Microsoft.Analytics.Samples.Formats.dll
  * Microsoft.Hadoop.Avro.dll
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
