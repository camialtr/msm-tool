namespace jd_tools
{
    internal class Base
    {
        public static readonly string newLine = Environment.NewLine;
        public static readonly string version = "0.2.2b";
#if (DEBUGX86 || RELEASEX86)
        public static readonly string architecture = "[x86]";
#elif (DEBUGX64 || RELEASEX64)
        public static readonly string architecture = "[x64]";
#endif
        public static readonly string[] commands = new string[1]
        {
            "  [0] Compare scoring API's from recorded data"
        };
        public static readonly string header = "Just Dance Tools | Created by Cami" + newLine + $"Version: {version} {architecture}" + newLine;
        public static string console = "...";
        public static string mapsPath = "";
    }

    public struct Settings
    {
        public string mapsPath;
    }

    public enum ComparativeType
    {
        jdScoring, MoveSpaceWrapper
    }

    public struct RecordedAccData
    {
        public int coachID;
        public float accX;
        public float accY;
        public float accZ;
        public float mapTime;
    }

    public struct MoveFile
    {
        public string name;
        public byte[] data;
    }

    public struct Timeline
    {
        public List<Move> moves;
    }

    public struct Move
    {
        public float time;
        public float duration;
        public string name;
        public int goldMove;
        public int coachID;
    }    

    public struct RecordedScore
    {
        public float energy;
        public float addedScore;
        public float totalScore;
        public string feedback;
    }

    public struct ComparativeJSON
    {
        public ComparativeType comparativeType;
        public List<RecordedScore> values;
    }
}
