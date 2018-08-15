using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;


namespace Microsoft.Analytics.Samples.Formats.ApacheAvro
{


    [DataContract(Namespace = "Microsoft.ServiceBus.Messaging")]
    public class EventDataPoco
    {
        [DataMember(Name = "SequenceNumber")]
        public long SequenceNumber { get; set; }

        [DataMember(Name = "Offset")]
        public string Offset { get; set; }

        [DataMember(Name = "EnqueuedTimeUtc")]
        public string EnqueuedTimeUtc { get; set; }

        [DataMember(Name = "Body")]
        public byte[] Body { get; set; }

        [DataMember(Name = "SystemProperties")]
        public IDictionary<string, object> SystemProperties { get; set; }

        [DataMember(Name = "Properties")]
        public IDictionary<string, object> Properties { get; set; }


        public EventDataPoco(dynamic record)
        {
            SequenceNumber = (long)record.SequenceNumber;
            Offset = (string)record.Offset;
            EnqueuedTimeUtc = (string)record.EnqueuedTimeUtc;
            SystemProperties = (Dictionary<string, object>)record.SystemProperties;
            Properties = (Dictionary<string, object>)record.Properties;
            Body = (byte[])record.Body;
        }

        public EventDataPoco() { }

    }
}
