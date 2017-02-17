using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Contoso
{
    public static class Helpers
    {
        public static string Normalize(string s)
        {
            s = s.Trim();
            s = s.ToUpper();
            return s;
        }
    }
}
