using System.Text.Json;
using System.Diagnostics;
using NativeFileDialogSharp;
using System.Runtime.InteropServices;

#pragma warning disable CS8600
#pragma warning disable CS8602
#pragma warning disable CS8604
#pragma warning disable IDE1006
namespace jd_tools;

public unsafe class ScoreFunctions : Base
{
    #if (DEBUGX86 || RELEASEX86)
    public static void ProcessRecordedDataLocal(string recordedAccDataPath)
    {
        List<RecordedAccData> recordedData = JsonSerializer.Deserialize<List<RecordedAccData>>(File.ReadAllText(recordedAccDataPath));
        JdScoring.ScoreManager scoreManager = InitializeScoring(recordedAccDataPath.Replace($@"\{Path.GetFileName(recordedAccDataPath)}", "").Replace(@"accdata", ""), recordedData[0].coachID - 1);
        List<RecordedScore> recordedValues = new();
        int moveID = 0; float lastScore = 0f;
        foreach (RecordedAccData accData in recordedData)
        {
            JdScoring.ScoreResult scoreResult = scoreManager.GetLastScore();
            (int, float) scoreData = GetScoreData(scoreResult, moveID, lastScore, recordedValues);
            moveID = scoreData.Item1; lastScore = scoreData.Item2;
            scoreManager.AddSample(accData.accX, accData.accY, accData.accZ, accData.mapTime);
        }
        scoreManager.EndScore();
        ComparativeJSON json = new()
        {
            comparativeType = ComparativeType.jdScoring,
            values = recordedValues
        };
        File.WriteAllText(Path.Combine(GetOrCreateComparativesDirectory(), "jdScoring.json"), JsonSerializer.Serialize(json));
        ProceedToMainFunction();
    }

    static JdScoring.ScoreManager InitializeScoring(string rootPath, int coachID)
    {
        JdScoring.ScoreManager scoreManager = new();
        Timeline timeline = JsonSerializer.Deserialize<Timeline>(File.ReadAllText(rootPath + "timeline.json"));
        List<_s_MoveFile> moveFiles = new();
        List<Move> moves = timeline.moves.FindAll(x => x.coachID == coachID);
        foreach (string file in Directory.GetFiles(Path.Combine(rootPath, "moves")))
        {
            moveFiles.Add(new()
            {
                name = Path.GetFileName(file).Replace(".msm", ""),
                data = File.ReadAllBytes(file)
            });
        }
        int moveIndex = 0;
        foreach (Move move in moves)
        {
            moveIndex++;
            _s_MoveFile file = moveFiles.Find(x => x.name == move.name);
            scoreManager.LoadClassifier(move.name, file.data);
            scoreManager.LoadMove(move.name, (int)(move.time * 1000), (int)(move.duration * 1000), Convert.ToBoolean(move.goldMove), moveIndex.Equals(moves.Count));
        }
        return scoreManager;
    }

    static (int, float) GetScoreData(JdScoring.ScoreResult scoreResult, int moveID, float lastScore, List<RecordedScore> recordedValues)
    {
        if (scoreResult.moveNum == moveID)
        {
            string feedback = GetFeedback(scoreResult);
            recordedValues.Add(new() { energy = 0f, addedScore = scoreResult.totalScore - lastScore, totalScore = scoreResult.totalScore, feedback = feedback });
            moveID++; lastScore = scoreResult.totalScore;
        }
        return (moveID, lastScore);
    }

    private static string GetFeedback(JdScoring.ScoreResult scoreResult) => scoreResult.rating switch
    {
        0 => scoreResult.isGoldMove ? "MISSYEAH" : "MISS",
        1 => "OK",
        2 => "GOOD",
        3 => "PERFECT",
        4 => "YEAH"
    };
    
    static void ProceedToMainFunction()
    {
        Console.Clear();
        string middlePath = @"\";
        #if DEBUGX86
        middlePath = @"bin\x64\Debug\net8.0\";
        #endif
        ProcessStartInfo processStartInfo = new()
        {
            FileName = BuildPath(middlePath, @"jd-tools.exe").Replace(@"Assemblies\", ""),
            Arguments = $"compare"
        };
        Process.Start(processStartInfo);
    }
    #elif (DEBUGX64 || RELEASEX64)
    public static void ProcessRecordedDataLocal()
    {
        WriteStaticHeader(true, $"Select a file...", 0);
        DialogResult dialogResult = Dialog.FileOpen("json", mapsPath);
        if (dialogResult.IsCancelled) { console = "Operation cancelled..."; Program.InitialLogic(); }
        List<RecordedAccData> recordedAccData = new();
        try
        {
            recordedAccData = JsonSerializer.Deserialize<List<RecordedAccData>>(File.ReadAllText(dialogResult.Path));
        }
        catch
        {
            console = "Error: Seems like you have selected an incorrect file, verify your file structure or select a valid one!";
            Program.InitialLogic();
        }
        string rootPath = dialogResult.Path.Replace($@"\{Path.GetFileName(dialogResult.Path)}", "").Replace(@"accdata", "");
        Timeline timeline = JsonSerializer.Deserialize<Timeline>(File.ReadAllText(rootPath + "timeline.json"));
        List<Move> moves = timeline.moves.FindAll(x => x.coachID == recordedAccData[0].coachID - 1);
        List<RecordedScore> recordedValues = new();
        float totalScore = 0f;
        (float goldScoreValue, float moveScoreValue) = GetScoreValues(moves);
        MoveSpaceWrapper.ScoreManager scoreManager = new();
        scoreManager.Init(true, 60f);
        foreach (Move move in moves)
        {
            ScoreResult scoreResult = ComputeMoveSpace(scoreManager, move, recordedAccData, rootPath);
            if (scoreResult.energy > 0.2f)
            {
                float score = GetScore(move, moveScoreValue, goldScoreValue, scoreResult.percentage);
                totalScore += score;
                string feedback = GetFeedback(move, scoreResult.percentage);
                recordedValues.Add(new() { energy = scoreResult.energy, addedScore = score, totalScore = totalScore, feedback = feedback });
                continue;
            }
            recordedValues.Add(new() { energy = scoreResult.energy, addedScore = 0f, totalScore = totalScore, feedback = move.goldMove == 1 ? "MISSYEAH" : "MISS" });
        }
        scoreManager.Dispose();
        ComparativeJSON json = new()
        {
            comparativeType = ComparativeType.MoveSpaceWrapper,
            values = recordedValues
        };
        File.WriteAllText(Path.Combine(GetOrCreateComparativesDirectory(), "MoveSpaceWrapper.json"), JsonSerializer.Serialize(json));
        ProceedToSubFunction(dialogResult.Path);
    }

    public static (float goldValue, float moveValue) GetScoreValues(List<Move> moves)
    {
        int goldCount = 0; int moveCount = 0;
        foreach (Move move in moves)
        {
            if (move.goldMove == 1) goldCount++;
            else moveCount++;
        }
        float moveValue = 13333f / (3.5f * goldCount + moveCount);
        float goldValue = moveValue * 3.5f;
        return (goldValue, moveValue);
    }

    static ScoreResult ComputeMoveSpace(MoveSpaceWrapper.ScoreManager scoreManager, Move move, List<RecordedAccData> recordedAccData, string rootPath)
    {
        MoveFile file = GetMoveFileFromByteArray(File.ReadAllBytes(rootPath + $@"moves\{move.name}.msm"));
        scoreManager.StartMoveAnalysis((void*)file.data, file.length, move.duration);
        List<RecordedAccData> samples = GetSampleDataFromTimeRange(recordedAccData, move.time, move.duration);
        for (int sID = 0; sID < samples.Count; ++sID)
        {
            RecordedAccData sample = samples[sID];
            if (sID == 0) continue;
            float prevRatio = sID == 0 ? 0.0f : (samples[sID - 1].mapTime - move.time) / move.duration;
            float currentRatio = (sample.mapTime - move.time) / move.duration;
            float step = (currentRatio - prevRatio) / sID;
            for (int i = 0; i < sID; ++i)
            {
                float ratio = Clamp(currentRatio - (step * (sID - (i + 1))), 0.0f, 1.0f);
                scoreManager.bUpdateFromProgressRatioAndAccels(ratio, Clamp(sample.accX, -3.4f, 3.4f), Clamp(sample.accY, -3.4f, 3.4f), Clamp(sample.accZ, -3.4f, 3.4f));
            }
        }
        scoreManager.StopMoveAnalysis();
        float scoreEnergy = scoreManager.GetLastMoveEnergyAmount();
        float scorePercentage = scoreManager.GetLastMovePercentageScore();
        Marshal.FreeHGlobal(file.data);
        return new() { energy = scoreEnergy, percentage = scorePercentage };
    }

    static MoveFile GetMoveFileFromByteArray(byte[] data)
    {
        MoveFile file = new()
        {
            data = Marshal.AllocHGlobal(data.Length),
            length = (uint)data.Length
        };
        Marshal.Copy(data, 0, file.data, data.Length);
        return file;
    }

    static List<RecordedAccData> GetSampleDataFromTimeRange(List<RecordedAccData> recordedAccData, float time, float duration) => recordedAccData.Where(accData => accData.mapTime >= time && accData.mapTime <= time + duration).ToList();

    static float Clamp(float value, float min, float max) => Math.Max(min, Math.Min(max, value));

    static float GetScore(Move move, float moveScoreValue, float goldScoreValue, float percentage)
    {
        if (percentage <= 25.0f) return 0f;
        float scoreValue = (move.goldMove == 1 && percentage > 70.0f) ? goldScoreValue : moveScoreValue;
        return Single.Lerp(0f, scoreValue, percentage / 100);
    }

    static string GetFeedback(Move move, float percentage) => (move.goldMove, percentage) switch 
    {
        (1, > 70.0f) => "YEAH",
        (1, < 70.0f) => "MISSYEAH",
        (_, < 25.0f) => "MISS",
        (_, < 50.0f) => "OK",
        (_, < 70.0f) => "GOOD",
        (_, < 80.0f) => "SUPER",
        _ => "PERFECT"
    };

    static void ProceedToSubFunction(string path)
    {
        string middlePath = @"\";
        #if DEBUGX64
        middlePath = @"bin\x86\Debug\net8.0\";
        #elif RELEASEX64
        middlePath = @"Assemblies\";
        #endif
        ProcessStartInfo processStartInfo = new()
        {
            FileName = BuildPath(middlePath, @"jd-tools.exe"),
            Arguments = $"processrecordeddatalocal {path.Replace(" ", "|SPACE|")}"
        };
        Process.Start(processStartInfo);
    }

    public static void Compare()
    {
        string middlePath = @"\";
        #if DEBUGX64
        middlePath = @"bin\x64\Debug\net8.0\";
        #endif
        string comparativesDirectory = BuildPath(middlePath, @"Comparatives\");
        if (!Directory.Exists(comparativesDirectory) || !File.Exists(Path.Combine(comparativesDirectory, "jdScoring.json")) || !File.Exists(Path.Combine(comparativesDirectory, "MoveSpaceWrapper.json")))
        {
            console = "Error: Incorrect structure or missing files at comparatives directory!";
            Directory.Delete(comparativesDirectory, true);
            Program.InitialLogic();
        }
        ComparativeJSON jdScoring = JsonSerializer.Deserialize<ComparativeJSON>(File.ReadAllText(Path.Combine(comparativesDirectory, "jdScoring.json")));
        ComparativeJSON moveSpaceWrapper = JsonSerializer.Deserialize<ComparativeJSON>(File.ReadAllText(Path.Combine(comparativesDirectory, "MoveSpaceWrapper.json")));
        WriteStaticHeader(false, $"Generated comparative:{newLine}{newLine}", 0);
        Console.WriteLine("jdScoring".PadRight(49) + "MoveSpaceWrapper");
        Console.WriteLine(new string('=', 98));
        Console.WriteLine("Energy".PadRight(12) + "Score".PadRight(12) + "Total S.".PadRight(12) + "Feedback".PadRight(12) + "|" + "Energy".PadRight(12) + "Score".PadRight (12) + "Total S.".PadRight(12) + "Feedback".PadRight(12) + "|");
        for (int i = 0; i < jdScoring.values.Count; i++)
        {
            if (i > 0) Console.WriteLine();
            GenerateComparative(jdScoring, i);
            GenerateComparative(moveSpaceWrapper, i);
        }
        Directory.Delete(comparativesDirectory, true);
        Console.WriteLine();
        Console.WriteLine(new string('=', 98));
        Console.WriteLine("Press any key to exit...");
        Console.WriteLine(new string('=', 98));
        Console.CursorVisible = false;
        Console.ReadKey();
        Console.CursorVisible = true;
        console = "...";
        Program.InitialLogic();
    }
    #endif
    static void GenerateComparative(ComparativeJSON comparative, int index)
    {
        string energyOutput = "NA".PadRight(12);    
        if (comparative.comparativeType == ComparativeType.MoveSpaceWrapper)
        {
            energyOutput = comparative.values[index].energy.ToString("n2").PadRight(12);
        }
        string addedScoreOutput = comparative.values[index].addedScore.ToString("n2").PadRight(12);
        string totalScoreOutput = comparative.values[index].totalScore.ToString("n2").PadRight(12);
        string feedbackOutput = comparative.values[index].feedback.PadRight(12);
        Console.Write($"{energyOutput}{addedScoreOutput}{totalScoreOutput}{feedbackOutput}|");
    }

    static void WriteStaticHeader(bool sleep, string log, int commandID)
    {
        Console.Clear();
        Console.WriteLine(header);
        Console.WriteLine($"{commands[commandID].Replace($"[{commandID}] ", "")}{newLine}");
        Console.Write($"[Console]");
        console = log;
        Console.Write($"{newLine}{newLine}{DateTime.Now.ToString("hh:mm:ss")} - {console}");
        if (sleep) Thread.Sleep(500);
    }

    static string GetOrCreateComparativesDirectory()
    {
        string middlePath = @"\";
        #if (DEBUGX86 || DEBUGX64)
        middlePath = @"bin\x64\Debug\net8.0\";
        #endif
        string comparativesDirectory = BuildPath(middlePath, @"Comparatives\");
        if (!Directory.Exists(comparativesDirectory)) Directory.CreateDirectory(comparativesDirectory);
        return comparativesDirectory;
    }
}

public class ScoreResult
{
    public float energy { get; set; }
    public float percentage { get; set; }
}

public class MoveFile
{
    public IntPtr data { get; set; }
    public uint length { get; set; }
}

public class _s_MoveFile
{
    public string? name { get; set; }
    public byte[]? data { get; set; }
}

public class RecordedScore
{
    public float energy { get; set; }
    public float addedScore { get; set; }
    public float totalScore { get; set; }
    public string? feedback { get; set; }
}

public enum ComparativeType
{
    jdScoring, MoveSpaceWrapper
}

public class ComparativeJSON
{
    public ComparativeType comparativeType { get; set; }
    public List<RecordedScore>? values { get; set; }
}
