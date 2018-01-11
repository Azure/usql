using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Microsoft.Analytics.Samples.Formats.Tests
{
    [DataContract(Name = "SingleColumnPoco", Namespace = "Microsoft.Analytics.Samples.Formats.Tests")]
    public class SingleColumnPoco<T>
    {
        [DataMember]
        public T Value { get; set; }

    }
}