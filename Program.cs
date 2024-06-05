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
            Console.WriteLine("Just Dance Scoring Analysis by Cami");
            Console.WriteLine("Version: 0.0.1a");
#if (DEBUGX86 || RELEASEX86)
            Console.WriteLine("Preset: UAF API");
#elif (DEBUGX64 || RELEASEX64)
            Console.WriteLine("Preset: JDN API");
#endif
            Console.WriteLine();
            Console.WriteLine("Select one of the options:");
            Console.WriteLine("");
        }
    }

    public struct ScoringRecorder
    {
        public string mapName;
        public int coachID;
        public float recordedFinalScore;
        public List<Move> moves;
        public List<RecordedAccData> recordedAccData;
        public List<RecordedScore> recordedScore;
    }

    public struct Move
    {
        public float time;
        public float duration;
        public string name;
        public int goldMove;
    }

    public struct RecordedAccData
    {
        public float accX;
        public float accY;
        public float accZ;
        public float mapTime;
    }

    public struct RecordedScore
    {
        public string feedback;
        public float addedScore;
        public float totalScore;
    }
}
