using Microsoft.Analytics.Interfaces;

namespace MVA_UDAgg
{
    // [SqlUserDefinedReducer(IsRecursive = true)]
    public class MySum : IAggregate<int, long>
    {
        long total;

        public override void Init()
        {
            total = 0;
        }

        public override void Accumulate(int value)
        {
            total += value;
        }

        public override long Terminate()
        {
            return total;
        }
    }
}
