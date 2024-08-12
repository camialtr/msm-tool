#pragma warning disable IDE1006
namespace jd_tools;

public class Base
{
    public static readonly string newLine = Environment.NewLine;
    public static readonly string version = "0.2.5b";
    #if (DEBUGX86 || RELEASEX86)
    public static readonly string architecture = "[x86]";
    #elif (DEBUGX64 || RELEASEX64)
    public static readonly string architecture = "[x64]";
    #endif
    public static readonly string[] commands =
    [
        "  [0] Compare scoring API's from recorded data"
    ];
    public static readonly string header = "Just Dance Tools | Created by Cami" + newLine + $"Version: {version} {architecture}" + newLine;
    public static string console = "...";
    public static string mapsPath = "";

    public static string BuildPath(string middlePath, string endPath) => @$"{Environment.CurrentDirectory}\" + middlePath + endPath;
}
public class Settings
{
    public string? mapsPath { get; set; }
}    
public class RecordedAccData
{
    public int coachID { get; set; }
    public float accX { get; set; }
    public float accY { get; set; }
    public float accZ { get; set; }
    public float mapTime { get; set; }
}    
public class Timeline
{
    public List<Move>? moves { get; set; }
}
public class Move
{
    public float time { get; set; }
    public float duration { get; set; }
    public string? name { get; set; }
    public int goldMove { get; set; }
    public int coachID { get; set; }
}
