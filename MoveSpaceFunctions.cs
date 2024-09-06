using MSP_LIB;
using RecTool;
using MSPClassifier;
using RecMoveExtractor;
using System.Text.Json;
using System.Text.Json.Serialization;
using TmlDtapeMoveExtractor;
using NativeFileDialogSharp;
using System.Reflection.Metadata;

#pragma warning disable CS8600
#pragma warning disable CS8602
#pragma warning disable CS8618
#pragma warning disable CS8625
namespace jd_tools;

#if (DEBUGX64 || RELEASEX64)
public class MoveSpaceFunctions : Base
{

    public static void GenerateMSMsFromRecordedData()
    {
        console = "...";
        Console.Clear();
        Console.WriteLine(header);
        Console.WriteLine("  Insert MapName assuring you're using the correct formatting pattern. Example: 'IKissedSWT', 'WhineUp'");
        Console.Write($"{newLine}Type code: ");
        Console.Write($"{newLine}{newLine}[Console]");
        Console.Write($"{newLine}{newLine}{DateTime.Now.ToString("hh:mm:ss")} - {console}");
        Console.SetCursorPosition(11, 5);
        string? mapName = Console.ReadLine();
        if (mapName == null || mapName == "")
        {
            console = "Invalid MapName, try again!";
            Program.InitialLogic();
        }
        Console.Clear();
        Console.WriteLine(header);
        Console.WriteLine("  Insert the coach index witch you want to create MSM's for");
        Console.Write($"{newLine}Type code: ");
        Console.Write($"{newLine}{newLine}[Console]");
        Console.Write($"{newLine}{newLine}{DateTime.Now.ToString("hh:mm:ss")} - {console}");
        Console.SetCursorPosition(11, 5);
        int coachId = 1;
        try 
        {
            coachId = Convert.ToInt32(Console.ReadLine());
        }
        catch { }
        if (coachId < 1 || coachId > 4)
        {
            console = "Invalid index, try again!";
            Program.InitialLogic();
        }
        DialogResult dialogResult = Dialog.FolderPicker(mapsPath);
        if (dialogResult.IsCancelled) { console = "Operation cancelled..."; Program.InitialLogic(); }
        if (mapName.ToLower() != Path.GetFileName(dialogResult.Path))
        {
            console = "MapName doesn't match...";
            Program.InitialLogic();
        }
        if (Directory.Exists(Path.Combine(dialogResult.Path, "accdata")) && File.Exists(Path.Combine(dialogResult.Path, "musictrack.json")) && File.Exists(Path.Combine(dialogResult.Path, "timeline.json")))
        {
            if (Directory.GetFiles(Path.Combine(dialogResult.Path, "accdata")).Length >= 5)
            {
                GenerateRECs(mapName, dialogResult.Path);
                GenerateLUAs(mapName, coachId, dialogResult.Path);
                GenerateMSMs(mapName, dialogResult.Path);
                console = $"Success! Moves available at {mapName.ToLower()}/generated...";
                Program.InitialLogic();
            }
            else
            {
                console = "Not enough data to generate MSM's...";
                Program.InitialLogic();
            }
        }
        else
        {
            console = "Incorrect folder structure! Select a correct one...";
            Program.InitialLogic();
        }
    }

    public static void GenerateRECs(string mapName, string path)
    {
        WriteStaticHeader(true, "Creating REC's...", 1);
        if (Directory.Exists(@$"{path}\recording")) Directory.Delete(@$"{path}\recording", true);
        Directory.CreateDirectory(@$"{path}\recording");
        foreach (string file in Directory.GetFiles(@$"{path}\accdata", "*.json"))
        {
            string recFile = file.Replace("accdata", "recording").Replace(".json", ".rec");
            List<FieldDef> fieldDefList = 
            [
                RecWriter.CreateFieldDef(RecDataFormat.FIELD_TIME, FieldUse.FieldUse_Time, false),
                RecWriter.CreateFieldDef(RecDataFormat.FIELD_ACCEL_NX + "1_", FieldUse.FieldUse_MotionData, true),
                RecWriter.CreateFieldDef(RecDataFormat.FIELD_GYRO_NX + "1A", FieldUse.FieldUse_MotionData, true)
            ];
            HeaderInfo headerInfo = new()
            {
                FieldDefList = fieldDefList,
                MapName = mapName,
                FormatName = "NX_ACCQD",
                VersionId = 4U
            };
            RecWriter recWriter = new(headerInfo, recFile);
            List<RecordedAccData> accData = JsonSerializer.Deserialize<List<RecordedAccData>>(File.ReadAllText(file));
            foreach (RecordedAccData recordedAccData in accData)
            {
                ExtendedChunkData chunkData = RecWriter.CreateChunkData(recordedAccData.mapTime);
                chunkData.AddPadSample(
                [
                    new() 
                    {
                        SampleFieldDef = RecWriter.CreateFieldDef(RecDataFormat.FIELD_ACCEL_NX + "1_", FieldUse.FieldUse_MotionData, true),
                        FloatList = [ recordedAccData.accX, recordedAccData.accY, recordedAccData.accZ ]
                    },
                    new()
                    {
                        SampleFieldDef = RecWriter.CreateFieldDef(RecDataFormat.FIELD_GYRO_NX + "1A", FieldUse.FieldUse_MotionData, true),
                        FloatList = [ 0f, 0f, 0f ]
                    }
                ]);
                recWriter.AppendSample(chunkData);
            }
            recWriter.SaveRec();
            RecReader recReader = new(file.Replace("accdata", "recording").Replace(".json", ".rec"));
        }
    }

    public static void GenerateLUAs(string mapName, int coachID, string path)
    {
        WriteStaticHeader(true, "Creating LUA's...", 1);
        if (Directory.Exists(@$"{path}\lua")) Directory.Delete(@$"{path}\lua", true);
        Directory.CreateDirectory(@$"{path}\lua");
        MusicTrack musicTrack = JsonSerializer.Deserialize<MusicTrack>(File.ReadAllText(Path.Combine(path, "musictrack.json")));
        List<int> markers = [];
        foreach (float beat in musicTrack.beats) markers.Add((int)(beat * 48000));
        TRK trk = new()
        {
            Format = 0,
            Markers = markers,
            AudioFilePath = $"maps\\{mapName}\\audio\\{mapName}.wav",
            VideoStartTime = $"-00:00:{((int)musicTrack.videoStartTime).ToString("00")}",
            StartBeat = Convert.ToInt32("-" + musicTrack.startBeat.ToString()), //TODO: Possibly will need to fix
            EndBeat = musicTrack.endBeat
        };
        JsonSerializerOptions options = new() { WriteIndented = true };
        File.WriteAllText(Path.Combine(path, "lua", $"{mapName}.trk"), JsonSerializer.Serialize(trk, options));
        Timeline timeline = JsonSerializer.Deserialize<Timeline>(File.ReadAllText(Path.Combine(path, "timeline.json")));
        List<Move> moves = timeline.moves.FindAll(x => x.coachID == coachID - 1);
        TML tml = new()
        {
            Format = 0,
            Tracks =
            [
                new ()
                {
                    Type = "JD.DTO.Tape.Tracks.MoveTrackDto, JD.DTO",
                    CoachID = coachID - 1,
                    MoveType = 0,
                    Id = 4094799440,
                    Name = "Moves1"
                }
            ],
            MetaInfos = [],
            ActorPaths = [],
            MapName = mapName,
            SoundwichEvent = "",
            TapeClock = 0,
            TapeBarCount = 1,
            FreeResourcesAfterPlay = false
        };
        List<MotionClipDto> motionClips = [];
        foreach (Move move in moves)
        {
            bool goldMove = move.goldMove == 1 ? true : false;
            motionClips.Add(new() 
            {
                Type = "JD.DTO.Tape.Clips.DanceTape.MotionClipDto, JD.DTO",
                ClassifierPath = $@"{path}\moves\{move.name}.msm".Replace("\\", "/"),
                GoldMove = goldMove,
                CoachID = move.coachID,
                MoveType = 0,
                Color = "Red",
                MotionPlatformSpecifics = new MotionPlatformSpecifics
                {
                    X360 = new Platform { ScoreScale = 1.0, ScoreSmoothing = 0.0, LowThreshold = 0.2, HighThreshold = 1.0 },
                    DURANGO = new Platform { ScoreScale = 1.0, ScoreSmoothing = 0.0, LowThreshold = 0.2, HighThreshold = 1.0 },
                    ORBIS = new Platform { ScoreScale = 1.0, ScoreSmoothing = 0.0, LowThreshold = -0.2, HighThreshold = 0.6 },
                    POSENET = new Platform { ScoreScale = 1.0, ScoreSmoothing = 0.0, LowThreshold = 0.2, HighThreshold = 1.0 },
                    BLAZEPOSE = new Platform { ScoreScale = 1.0, ScoreSmoothing = 0.0, LowThreshold = 0.2, HighThreshold = 1.0 },
                },
                Id = 1262207903,
                TrackId = 4094799440,
                IsActive = true,
                StartTime = MsToMarker(move.time, musicTrack.beats),
                Duration = MsToMarker(move.duration, musicTrack.beats),
            });
        }
        tml.Clips = motionClips;
        File.WriteAllText(Path.Combine(path, "lua", $"{mapName}_TML_Dance.dtape"), JsonSerializer.Serialize(tml, options));
    }

    public static int MsToMarker(float ms, List<float> beats)
    {
        int backIndex = 0;
        int frontIndex = 0;
        for (int i = 0; i < beats.Count - 1; i++)
        {
            if (ms >= beats[i] && ms <= beats[i + 1])
            {
                backIndex = i;
                frontIndex = i + 1;
                break;
            }
        }
        double back = beats[backIndex];
        double front = beats[frontIndex];
        double decimalPart = (ms - back) / (front - back);
        double marker = backIndex + decimalPart;
        return (int)Math.Round(marker * 24);
    }

    public static void GenerateMSMs(string mapName, string path)
    {
        WriteStaticHeader(true, "Computing MSM's...", 1);
        if (Directory.Exists(@$"{path}\generated")) Directory.Delete(@$"{path}\generated", true);
        Directory.CreateDirectory(@$"{path}\generated");
        MeasuresManager.GetInstance.RegisterMeasuresSet(EMeasuresSet.Acc_Dev_Dir_NP);
        MeasuresManager.GetInstance.PopulateMeasuresSetUsingMeasuresIds(EMeasuresSet.Acc_Dev_Dir_NP, eMeasuresIds.eMeasureId_AccelNormAvg_NP, eMeasuresIds.eMeasureId_AccelDevNormAvg_NP, eMeasuresIds.eMeasureId_AxDevAvg_Dir_NP, eMeasuresIds.eMeasureId_AyDevAvg_Dir_NP, eMeasuresIds.eMeasureId_AzDevAvg_Dir_NP);        
        CMoves measures = ComputeMeasures(mapName, "Acc_Dev_Dir_NP", 3.4f, [.. Directory.GetFiles(@$"{path}\recording")], $@"{path}\lua\{mapName}_TML_Dance.dtape", $@"{path}\lua\{mapName}.trk", null);
        GenerateMoveSpaceFiles(mapName, measures, @$"{path}\generated", 7, true);
    }
    
    private static Func<double, string, bool> eUpdateProgression;

    public static CMoves ComputeMeasures(string songName, string measuresSetName, float accelSaturationValue, List<string> recs, string tmlOrTpl, string trk, Func<double, string, bool> updateProgression)
    {
        eUpdateProgression = updateProgression;
        CMoves measures = new();
        measures.Init((Action<double, string>)null);
        TmlDtapeMoveReader moveReader = new(tmlOrTpl, trk);
        for (int i = 0; i < recs.Count; i++)
        {
            RecCleaner recCleaner = new(new(recs[i]), moveReader);
            RecHelp.RecNameInfo recNameInfo = RecHelp.GetInfoFromRecFileName(Path.GetFileName(recs[i]));
            measures.GetMoveInstanceFromRecCleaner(recCleaner, songName, recNameInfo.MovesNum, null, recs[i]);
            double progress = 100.0 * i / recs.Count;
            if (updateProgression != null && !updateProgression(progress, $"Processing file {Path.GetFileName(recs[i])}")) return measures;
        }
        EMeasuresSet measuresSetId = MeasuresManager.GetInstance.GetMeasuresSetIdWithMeasuresSetName(measuresSetName);
        ulong measuresSetBitField = measuresSetId != EMeasuresSet.Max ? MeasuresManager.GetInstance.GetMeasuresSetBitFieldWithItsId(measuresSetId) : throw new Exception($"ERROR : Measureset id doesn't exist : {measuresSetName}");
        measures.ComputeMeasures(measuresSetBitField, 0.0f, accelSaturationValue, 60f, 7);
        return measures;
    }

    public static void GenerateMoveSpaceFiles(string songName, CMoves moves, string msmPath, uint classifierFormatVersionNumber, bool isTargetPC)
    {
        if (moves.MoveModelsCount == 0) throw new InvalidOperationException("No move models found.");
        if (!Directory.Exists(msmPath)) throw new DirectoryNotFoundException($"The directory '{msmPath}' does not exist.");
        if (Directory.GetFiles(msmPath).Length != 0) foreach (string file in Directory.GetFiles(msmPath)) File.Delete(file);
        for (int index = 0; index < moves.MoveModelsCount; index++)
        {
            MoveModel moveModel = moves.GetMoveModel(index);
            string moveModelName = moveModel.m_sMoveModelName;
            int nullIndex = moveModelName.IndexOf('\0');
            if (nullIndex != -1) moveModelName = moveModelName[..nullIndex];
            string msmFilePath = Path.Combine(msmPath, $"{moveModel.m_sSongName}_{moveModelName}.msm");
            Classifier classifier = new(moveModel.m_sSongName, moveModelName);
            List<int> familyList = moves.GetMoveModel(index).GetFamilyList();
            int familyCount = familyList.Count;
            classifier.ComputeClassifier(moveModel, EForceAlgoType.ForceNaiveBayes);
            classifier.ExportClassifierFile(msmFilePath, classifierFormatVersionNumber, isTargetPC);
            if (familyCount > 1)
            {
                for (int familyIndex = 1; familyIndex < familyCount; familyIndex++)
                {
                    Classifier familyClassifier = new(moveModel.m_sSongName, moveModelName);
                    familyClassifier.ComputeClassifier(moveModel, EForceAlgoType.ForceNaiveBayes, familyList[familyIndex]);
                    familyClassifier.AddClassifierDataBlock(msmFilePath);
                }
            }
        }
        foreach (string file in Directory.GetFiles(msmPath)) File.Move(file, file.ToLower());
    }
}
#endif

public class TRK
{
    [JsonPropertyName("$format")]
    public int Format { get; set; }
    public List<int> Markers { get; set; }
    public List<Signature> Signatures { get; set; }
    public List<Section> Sections { get; set; }
    public List<Comment> Comments { get; set; }
    public List<Ambiance> Ambiances { get; set; }
    public string AudioFilePath { get; set; }
    public double StartBeat { get; set; }
    public double EndBeat { get; set; }
    public string VideoStartTime { get; set; }
    public double Volume { get; set; }
    public double PreviewEntry { get; set; }
    public double PreviewLoopStart { get; set; }
    public double PreviewLoopEnd { get; set; }
    public double FadeInDuration { get; set; }
    public double FadeOutDuration { get; set; }
    public double FadeStartBeat { get; set; }
    public double FadeEndBeat { get; set; }
    public bool UseFadeStartBeat { get; set; }
    public bool UseFadeEndBeat { get; set; }
    public int FadeInType { get; set; }
    public int FadeOutType { get; set; }
}

public class Signature
{
    public string Comment { get; set; }
    public double Marker { get; set; }
    public int Beat { get; set; }
}

public class Section
{
    public int SectionType { get; set; }
    public double Marker { get; set; }
    public string Comment { get; set; }
}

public class Comment
{
    public string CommentText { get; set; }
    public int CommentType { get; set; }
    public double Marker { get; set; }
}

public class Ambiance
{
    public string AudioFilePath { get; set; }
    public double Marker { get; set; }
    public string Comment { get; set; }
}

//DTO

public class MotionPlatformSpecifics
{
    public Platform X360 { get; set; }
    public Platform DURANGO { get; set; }
    public Platform ORBIS { get; set; }
    public Platform POSENET { get; set; }
    public Platform BLAZEPOSE { get; set; }
}
public class Platform
{
    public double ScoreScale { get; set; }
    public double ScoreSmoothing { get; set; }
    public double LowThreshold { get; set; }
    public double HighThreshold { get; set; }
}
public class MotionClipDto
{
    [JsonPropertyName("$type")]
    public string Type { get; set; }
    public string ClassifierPath { get; set; }
    public bool GoldMove { get; set; }
    public int CoachID { get; set; }
    public int MoveType { get; set; }
    public string Color { get; set; }
    public MotionPlatformSpecifics MotionPlatformSpecifics { get; set; }
    public long Id { get; set; }
    public long TrackId { get; set; }
    public bool IsActive { get; set; }
    public int StartTime { get; set; }
    public int Duration { get; set; }
}
public class MoveTrackDto
{
    [JsonPropertyName("$type")]
    public string Type { get; set; }
    public int CoachID { get; set; }
    public int MoveType { get; set; }
    public long Id { get; set; }
    public string Name { get; set; }
}
public class TML
{
    [JsonPropertyName("$format")]
    public int Format { get; set; }
    public List<MotionClipDto> Clips { get; set; }
    public List<MoveTrackDto> Tracks { get; set; }
    public List<object> MetaInfos { get; set; }
    public List<object> ActorPaths { get; set; }
    public string MapName { get; set; }
    public string SoundwichEvent { get; set; }
    public int TapeClock { get; set; }
    public int TapeBarCount { get; set; }
    public bool FreeResourcesAfterPlay { get; set; }
}