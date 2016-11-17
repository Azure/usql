using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Analytics.Samples.Formats.Json.Exceptions
{
    public class PathNotFoundException : ApplicationException
    {
        public PathNotFoundException(string message) : base(message)
        {
        }
    }
}
