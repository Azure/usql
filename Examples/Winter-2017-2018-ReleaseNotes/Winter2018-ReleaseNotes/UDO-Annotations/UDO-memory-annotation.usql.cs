using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Diagnostics;
using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UDO_Annotations
{

    internal static class MyLimits
    {
        public const long MaxUdoMemory = 2L * 1024 * 1024 * 1024;
    }

    [SqlUserDefinedExtractor]
    [SqlUserDefinedMemory(Max=MyLimits.MaxUdoMemory)]
    public class MyExtractor : IExtractor
    {
        private long max_allocation_size;
        private long increment;
        private int no_buff;
        private byte[][] alloc_mem;

        public MyExtractor(long max_alloc_sz = 10*1024*1024, long incr = 1024*1024){
            max_allocation_size = max_alloc_sz;
            increment = incr;
            no_buff = (int)Math.Ceiling((decimal) max_alloc_sz/(decimal)incr);
            alloc_mem = new byte[no_buff][];
        }

        public override IEnumerable<IRow> Extract(IUnstructuredReader input, IUpdatableRow outputrow)
        {
            outputrow.Set<long>("GC_TotalMem_Start", GC.GetTotalMemory(true));
            outputrow.Set<long>("MaxUDOMemory", MyLimits.MaxUdoMemory);

            var buff_idx = 0;
            var failed = false;
            var gc_mem = GC.GetTotalMemory(true);
            try
            {
                while (buff_idx < no_buff) {
                    alloc_mem[buff_idx] = new byte[increment];
                    alloc_mem[buff_idx][0] = 1; // to avoid it being optimized away
                    buff_idx++;
                    gc_mem = GC.GetTotalMemory(true);
                }
            }
            catch (Exception e)
            {
                failed = true;
                outputrow.Set<string>("error", e.Message);
            }
            outputrow.Set<long>("GC_TotalMem_End", gc_mem);
            outputrow.Set<bool>("failed", failed);
            outputrow.Set<long>("alloc_sz", buff_idx*increment);

            yield return outputrow.AsReadOnly();
        }
    }
}
