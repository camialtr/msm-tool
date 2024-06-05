#if (DEBUGX86 || RELEASEX86)
using JDNow;
#elif (DEBUGX64 || RELEASEX64)
using MoveSpaceWrapper;
#endif

namespace scoring_analysis
{
    internal unsafe class Program
    {
        private static readonly string newLine = Environment.NewLine;
        private static readonly string version = "0.0.1a";
#if (DEBUGX86 || RELEASEX86)
        private static readonly string preset = "UAF API";
#elif (DEBUGX64 || RELEASEX64)
        private static readonly string preset = "JDN API";
#endif
        private static readonly string header = "Just Dance Scoring APIs Analyzer | Created by Cami" + newLine + $"Version: {version}" + newLine + $"Preset: {preset}" + newLine;

        static void Main(string[] args)
        {
            Console.WriteLine(header);
            Console.WriteLine($"Select an option below: {newLine}");
            string[] commands = new string[2]
            {
                "[0] Example Option",
                "[1] Example Option"
            };
            foreach (string command in commands)
            {
                Console.WriteLine(command.PadLeft(20));
            }
            Console.Write($"{newLine}Type code: ");
            Console.Read();
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
