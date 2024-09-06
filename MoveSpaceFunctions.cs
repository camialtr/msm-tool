using MSP_LIB;
using RecTool;
using MSPClassifier;
using RecMoveExtractor;
using System.Text.Json;
using TmlDtapeMoveExtractor;
using NativeFileDialogSharp;

#pragma warning disable CS8600
#pragma warning disable CS8602
#pragma warning disable CS8618
namespace jd_tools;

#if (DEBUGX64 || RELEASEX64)
public class MoveSpaceFunctions : Base
{    
    public static void ExperimentalMSM()
    {
        WriteStaticHeader(true, "Running...", 1);
        //Register measure set
        MeasuresManager.GetInstance.RegisterMeasuresSet(EMeasuresSet.Acc_Dev_Dir_NP);
        MeasuresManager.GetInstance.PopulateMeasuresSetUsingMeasuresIds(EMeasuresSet.Acc_Dev_Dir_NP, eMeasuresIds.eMeasureId_AccelNormAvg_NP, eMeasuresIds.eMeasureId_AccelDevNormAvg_NP, eMeasuresIds.eMeasureId_AxDevAvg_Dir_NP, eMeasuresIds.eMeasureId_AyDevAvg_Dir_NP, eMeasuresIds.eMeasureId_AzDevAvg_Dir_NP);
        //Items Declarations
        string songName = "IKissedSWT";
        string measureSetName = "Acc_Dev_Dir_NP";
        float AccelSaturationValue = 3.4f;
        List<string> recs = [.. Directory.GetFiles(@"C:\Games\Just Dance Next\Just Dance Next_Data\Maps\ikissedswt\recording", "*.rec")];
        string tml = @"C:\Users\camia\Downloads\ikissedswt_TML_Dance.dtape";
        string trk = @"C:\Users\camia\Downloads\ikissedswt.trk";
        //Compute measures
        CMoves measures = ComputeMeasures(songName, measureSetName, AccelSaturationValue, recs, tml, trk, null);
        //Save Moves
        GenerateMoveSpaceFiles(songName, measures, @"C:\Games\Just Dance Next\Just Dance Next_Data\Maps\ikissedswt\generated", 7, true);
        Console.ReadLine();
        console = "...";
        Program.InitialLogic();
    }

    public static void ExperimentalREC()
    {
        WriteStaticHeader(true, "Running...", 1);
        RecReader recReader = new(@"C:\Games\Just Dance Next\Just Dance Next_Data\Maps\ikissedswt\recording\Player1_data_09042024_224344.rec");
        RecTool.RecData data = recReader.Data;
        Console.WriteLine(newLine);
        List<PadSample> padSamples = recReader.GetMotionSamplesByPadIndex(0);
        foreach (PadSample padSample in padSamples)
        {
            float fieldSampleCount = padSample.SampleFieldList.Count;
            string padID = padSample.PadId.ToString();
            float date = padSample.Date;
            int id = 0;
            foreach (FieldSample fieldSample in padSample.SampleFieldList)
            {
                id++;
                Console.WriteLine($"Name: {fieldSample.SampleFieldDef.Name} Size: {fieldSample.SampleFieldDef.Size} Date: {date} Progress: {id}/{fieldSampleCount} PadID: {padID} Type: {fieldSample.SampleFieldDef.DataType} Value: {fieldSample.SampleFieldDef.Use} Count: {fieldSample.SampleFieldDef.Count}");
                Console.WriteLine("=====");
                Console.WriteLine($"X: {fieldSample.FloatList[0]}");
                Console.WriteLine($"Y: {fieldSample.FloatList[1]}");
                Console.WriteLine($"Z: {fieldSample.FloatList[2]}");
                Console.WriteLine("=====");
            }
        }
        Console.ReadLine();
        console = "...";
        Program.InitialLogic();
    }

    public static void ExperimentalACCToREC()
    {
        WriteStaticHeader(true, "Running...", 1);
        foreach (string file in Directory.GetFiles(@"C:\Games\Just Dance Next\Just Dance Next_Data\Maps\ikissedswt\accdata", "*.json"))
        {
            string recFile = file.Replace("accdata", "recording").Replace(".json", ".rec");
            List<FieldDef> fieldDefList = 
            [
                RecWriter.CreateFieldDef(RecDataFormat.FIELD_TIME, FieldUse.FieldUse_Time, false),
                RecWriter.CreateFieldDef(RecDataFormat.FIELD_ACCEL_NX + "1_", FieldUse.FieldUse_MotionData, true),
                RecWriter.CreateFieldDef(RecDataFormat.FIELD_GYRO_NX + "1A", FieldUse.FieldUse_MotionData, true)
            ];
            RecWriter recWriter = new(new()
            {
                FieldDefList = fieldDefList,
                MapName = "WhineUp",
                FormatName = "NX_ACCQD",
                VersionId = 4U
            }, recFile);
            List<RecordedAccData> accData = JsonSerializer.Deserialize<List<RecordedAccData>>(File.ReadAllText(file));
            foreach (RecordedAccData recordedAccData in accData)
            {
                ExtendedChunkData chunkData = RecWriter.CreateChunkData(recordedAccData.mapTime);
                chunkData.PadNumber = 1;
                chunkData.AddPadSample(
                [
                    new() 
                    {
                        SampleFieldDef = RecWriter.CreateFieldDef(RecDataFormat.FIELD_ACCEL_NX + "1_", FieldUse.FieldUse_MotionData, true),
                        FloatList = [ recordedAccData.accX, recordedAccData.accY, recordedAccData.accZ ],

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
        console = "...";
        Program.InitialLogic();
    }

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
                GenerateRecs(mapName, dialogResult.Path);
                GenerateMSMs(mapName, coachId, dialogResult.Path);
                console = "...";
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

    public static void GenerateRecs(string mapName, string path)
    {
        WriteStaticHeader(true, "Creating Recs...", 1);
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
            RecWriter recWriter = new(new()
            {
                FieldDefList = fieldDefList,
                MapName = mapName,
                FormatName = "NX_ACCQD",
                VersionId = 4U
            }, recFile);
            List<RecordedAccData> accData = JsonSerializer.Deserialize<List<RecordedAccData>>(File.ReadAllText(file));
            foreach (RecordedAccData recordedAccData in accData)
            {
                ExtendedChunkData chunkData = RecWriter.CreateChunkData(recordedAccData.mapTime);
                chunkData.PadNumber = 1;
                chunkData.AddPadSample(
                [
                    new() 
                    {
                        SampleFieldDef = RecWriter.CreateFieldDef(RecDataFormat.FIELD_ACCEL_NX + "1_", FieldUse.FieldUse_MotionData, true),
                        FloatList = [ recordedAccData.accX, recordedAccData.accY, recordedAccData.accZ ],

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

    public static void GenerateMSMs(string mapName, int coachId, string path)
    {
        WriteStaticHeader(true, "Creating MSM's...", 1);
        MeasuresManager.GetInstance.RegisterMeasuresSet(EMeasuresSet.Acc_Dev_Dir_NP);
        MeasuresManager.GetInstance.PopulateMeasuresSetUsingMeasuresIds(EMeasuresSet.Acc_Dev_Dir_NP, eMeasuresIds.eMeasureId_AccelNormAvg_NP, eMeasuresIds.eMeasureId_AccelDevNormAvg_NP, eMeasuresIds.eMeasureId_AxDevAvg_Dir_NP, eMeasuresIds.eMeasureId_AyDevAvg_Dir_NP, eMeasuresIds.eMeasureId_AzDevAvg_Dir_NP);        
        CMoves measures = ComputeMeasures(mapName, "Acc_Dev_Dir_NP", 3.4f, [.. Directory.GetFiles(@$"{path}\recording")], @"C:\Users\camia\Downloads\ikissedswt_TML_Dance.dtape", @"C:\Users\camia\Downloads\ikissedswt.trk", null);
        if (Directory.Exists(@$"{path}\generated")) Directory.Delete(@$"{path}\generated", true);
        Directory.CreateDirectory(@$"{path}\generated");
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