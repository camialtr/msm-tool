using MSP_LIB;
using MSPClassifier;
using RecMoveExtractor;
using TmlDtapeMoveExtractor;

#pragma warning disable CS8600
#pragma warning disable CS8618
namespace jd_tools;

public class MoveSpaceFunctions : Base
{
    public static void Experimental()
    {
        WriteStaticHeader(true, "Running...", 1);
        //Register measure set
        MeasuresManager.GetInstance.RegisterMeasuresSet(EMeasuresSet.Acc_Dev_Dir_NP);
        MeasuresManager.GetInstance.PopulateMeasuresSetUsingMeasuresIds(EMeasuresSet.Acc_Dev_Dir_NP, eMeasuresIds.eMeasureId_AccelNormAvg_NP, eMeasuresIds.eMeasureId_AccelDevNormAvg_NP, eMeasuresIds.eMeasureId_AxDevAvg_Dir_NP, eMeasuresIds.eMeasureId_AyDevAvg_Dir_NP, eMeasuresIds.eMeasureId_AzDevAvg_Dir_NP);
        //Items Declarations
        string songName = "IKissedSWT";
        string measureSetName = "Acc_Dev_Dir_NP";
        float AccelSaturationValue = 3.4f;
        List<string> recs = [.. Directory.GetFiles(@"C:\Users\camia\Downloads\IKissedSWT\Timeline\Recording", "*.rec")];
        string tml = @"C:\Users\camia\Downloads\ikissedswt_TML_Dance.dtape";
        string trk = @"C:\Users\camia\Downloads\ikissedswt.trk";
        //Compute measures
        CMoves measures = ComputeMeasures(songName, measureSetName, AccelSaturationValue, recs, tml, trk, null);
        //Save Moves
        GenerateMoveSpaceFiles(songName, measures, @"C:\Users\camia\Downloads\IKissedSWT\GeneratedMoves", 7, true);
        Console.ReadLine();
    }

    public static void GenerateMSMsFromRecordedData()
    {
        WriteStaticHeader(true, "Select a file...", 1);
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
