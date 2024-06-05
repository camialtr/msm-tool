using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace scoring_analysis
{
    internal class Base
    {
        public static readonly string newLine = Environment.NewLine;
        public static readonly string version = "0.0.1a";
#if (DEBUGX86 || RELEASEX86)
        public static readonly string preset = "JDN API";
        public static readonly string[] commands = new string[2]
        {
            "  [0] Example Option to JDNOW",
            "  [1] Example Option to JDNOW"
        };
#elif (DEBUGX64 || RELEASEX64)
        public static readonly string preset = "UAF API";
        public static readonly string[] commands = new string[2]
        {
            "  [0] Example Option to UAF",
            "  [1] Example Option to UAF"
        };
#endif
        public static readonly string header = "Just Dance Scoring APIs Analyzer | Created by Cami" + newLine + $"Version: {version}" + newLine + $"Preset: {preset}" + newLine;
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
