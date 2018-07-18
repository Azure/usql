using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Interfaces.Streaming;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// This project defines a C# user defined processor
// The project is reference by DatabaseProject to create an assembly
// The project is also referenced by USqlCSharpUdoUnitTestProject1 to create and run test cases for this processor


namespace CSharpUDO
{
    [SqlUserDefinedProcessor]
    public class MyProcessor : IProcessor
    {
        public override IRow Process(IRow input, IUpdatableRow output)
        {
            int a = input.Get<int>("col1");
            int b = input.Get<int>("col2");
            output.Set<int>("col1", a + 1);
            output.Set<int>("col2", b + 5);
            return output.AsReadOnly();
        }
    }
}