using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TemperatureAnalysis
{
    [SqlUserDefinedExtractor(AtomicFileProcessing = true)]
    public class RecordExtractor : IExtractor
    {

        public RecordExtractor()
        {
        }

        public override IEnumerable<IRow> Extract(IUnstructuredReader input, IUpdatableRow output)
        {

            using (var reader = new StreamReader(input.BaseStream))
            {
                while (true)
                {
                    string line = reader.ReadLine();

                    if (line == null)
                    {
                        break;
                    }

                    var o = Newtonsoft.Json.Linq.JObject.Parse(line);

                    var GatewayName = o.Value<string>("GatewayName");
                    var Timestamp = o.Value<System.DateTime>("Timestamp");
                    var EventProcessedUtcTime = o.Value<System.DateTime>("EventProcessedUtcTime");
                    var PartitionId = o.Value<int?>("PartitionId");


                    var asset = o["Asset"];
                    var DeviceName = asset.Value<string>("DeviceName");
                    var ObjectType = asset.Value<string>("ObjectType");
                    var Instance = asset.Value<int>("Instance");
                    var PresentValue = asset.Value<string>("PresentValue");

                    output.Set<string>("GatewayName", GatewayName);
                    output.Set<System.DateTime>("Timestamp", Timestamp);
                    output.Set<System.DateTime>("EventProcessedUtcTime", EventProcessedUtcTime);
                    output.Set<int?>("PartitionId", PartitionId);
                    output.Set<string>("DeviceName", DeviceName);
                    output.Set<string>("ObjectType", ObjectType);
                    output.Set<int>("Instance", Instance);
                    output.Set<string>("PresentValue", PresentValue);

                    yield return output.AsReadOnly();

                }
            }
        }
    }

}