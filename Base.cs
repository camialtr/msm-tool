namespace jd_tools
{
    internal class Base
    {
        public static readonly string newLine = Environment.NewLine;
        public static readonly string version = "0.2.3b";
#if (DEBUGX86 || RELEASEX86)
        public static readonly string architecture = "[x86]";
#elif (DEBUGX64 || RELEASEX64)
        public static readonly string architecture = "[x64]";
#endif
        public static readonly string[] commands = new string[]
        {
            "  [0] Compare scoring API's from recorded data",
        };
        public static readonly string[] compareCommands = new string[]
        {
            "  [0] Back",
            "  [1] Compare scoring API's locally",
            "  [2] Compare scoring API's online (Requires API link)",
        };
        public static readonly string header = "Just Dance Tools | Created by Cami" + newLine + $"Version: {version} {architecture}" + newLine;
        public static string console = "...";
        public static string mapsPath = "";
        public static string apiLink = "";
    }

    public struct Settings
    {
        public string mapsPath;
        public string apiLink;
    }    

    public struct RecordedAccData
    {
        public int coachID;
        public float accX;
        public float accY;
        public float accZ;
        public float mapTime;
    }

    public struct ScoreRequest
    {
        public Move move;
        public byte[] moveFileData;
        public List<RecordedAccData> recordedAccData;
    }

    public struct MoveFile
    {
        public IntPtr data;
        public uint length;
    }

    public struct _s_MoveFile
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

    public struct ScoreResult
    {
        public float energy;
        public float percentage;
    }

    public struct RecordedScore
    {
        public float energy;
        public float addedScore;
        public float totalScore;
        public string feedback;
    }

    public enum ComparativeType
    {
        jdScoring, MoveSpaceWrapper
    }

    public struct ComparativeJSON
    {
        public ComparativeType comparativeType;
        public List<RecordedScore> values;
    }
}
