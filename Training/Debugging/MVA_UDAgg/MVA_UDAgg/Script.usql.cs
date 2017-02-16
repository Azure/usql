using Microsoft.Analytics.Interfaces;

namespace MVA_UDAgg
{
    public class MySum : IAggregate<int, int, long>
    {
        long total;

        public override void Init()
        {
            total = 0;
        }

        public override void Accumulate(int i1, int i2)
        {
            total += (i1*i1);
        }

        public override long Terminate()
        {
            return total;
        }
    }
}
