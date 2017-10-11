using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Microsoft.Hadoop.Avro;

namespace Microsoft.Analytics.Samples.Formats.Tests
{
    [DataContract(Name = "Foo", Namespace = "Microsoft.Analytics.Samples.Formats.Tests")]
    public class SingleColumnPoco<T>
    {
        [DataMember]
        [NullableSchema]
        public T Value { get; set; }

    }
}