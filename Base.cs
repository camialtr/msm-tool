namespace jd_tools
{
    internal class Base
    {
        public static readonly string newLine = Environment.NewLine;
        public static readonly string version = "0.2.1a";
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
        public const string mapsPath = @"D:\Just Dance\just-dance-next\Just Dance Next_Data\Maps";
    }

    public struct RecordedScore
    {
        public string feedback;
        public float addedScore;
        public float totalScore;
    }

    public struct ComparativeJSON
    {
        public ComparativeType comparativeType;
        public List<RecordedScore> values;
    }

    public enum ComparativeType
    {
        jdScoring, MoveSpaceWrapper
    }

    public struct NewRecordedAccData
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
}
