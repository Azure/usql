using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Microsoft.Analytics.Samples.Formats.Tests
{
    [DataContract(Name = "TwoColumnPoco", Namespace = "Microsoft.Analytics.Samples.Formats.Tests")]
    public class TwoColumnPoco<T>
    {
        [DataMember]
        public T Value1 { get; set; }
        public T Value2 { get; set; }
    }
}