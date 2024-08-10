namespace jd_tools
{
    public class Base
    {
        public static readonly string newLine = Environment.NewLine;
        public static readonly string version = "0.2.5b";
        #if (DEBUGX86 || RELEASEX86)
        public static readonly string architecture = "[x86]";
        #elif (DEBUGX64 || RELEASEX64 || DEBUGANYCPU || RELEASEANYCPU)
        public static readonly string architecture = "[x64]";
        #endif
        public static readonly string[] commands = new string[]
        {
            "  [0] Compare scoring API's from recorded data"
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
