#if (DEBUGX86 || RELEASEX86)
using JDNow;
#elif (DEBUGX64 || RELEASEX64)
using MoveSpaceWrapper;
#endif

namespace scoring_analysis
{
    internal unsafe class Program
    {
        static void Main(string[] args)
        {

        }
    }
}
