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
        public static readonly string version = "0.5.0b";
#if (DEBUGX86 || RELEASEX86)
        public static readonly string architecture = "[x86]";
        public static readonly string preset = "JDNOW API";
        public static readonly string[] commands = new string[3]
        {
            "  [0] Switch API preset to UAF",
            "  [1] Generate comparative JDNOW-JSON and JDNEXT-JSON from recorded data",
            "  [2] Compare JDNEXT | UAF | JDNOW from a ready-to-go folder"
            
        };
#elif (DEBUGX64 || RELEASEX64)
        public static readonly string architecture = "[x64]";
        public static readonly string preset = "UAF API";
        public static readonly string[] commands = new string[3]
        {
            "  [0] Switch API preset to JDNOW",
            "  [1] Generate comparative UAF-JSON from recorded data",
            "  [2] Compare JDNEXT | UAF | JDNOW from a ready-to-go folder"
        };
#endif
        public static readonly string header = "Just Dance Scoring APIs Analyzer | Created by Cami" + newLine + $"Version: {version} {architecture}" + newLine + $"Preset: {preset}" + newLine;
        public static string console = "...";
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
        public string data;
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

    public struct ComparativeJSON
    {
        public string mapName;
        public ComparativeType comparativeType;
        public List<RecordedScore> values;
    }

    public enum ComparativeType
    {
        JDNEXT, UAF, JDNOW
    }
}
