#if (DEBUGX86 || RELEASEX86)
using JDNow;
#elif (DEBUGX64 || RELEASEX64)
using MoveSpaceWrapper;
#endif
using Newtonsoft.Json;
using System.Diagnostics;
using System.ComponentModel;
using NativeFileDialogSharp;

namespace scoring_analysis
{
    internal unsafe class Program : Base
    {
        static void Main()
        {
            InitialLogic();
        }
        static void NotImplemented()
        {
            console = "This function is not yet implemented, choose another!";
            InitialLogic();
        }

        static void InitialLogic()
        {
            Console.Clear();
            Console.WriteLine(header);
            Console.WriteLine($"Select an option below: {newLine}");
            foreach (string command in commands) Console.WriteLine(command);
            Console.Write($"{newLine}Type code: ");
            Console.Write($"{newLine}{newLine}[Console]");
            Console.Write($"{newLine}{newLine}{DateTime.Now.ToString("hh:mm:ss")} - {console}");
            Console.SetCursorPosition(11, 7 + commands.Length);
            string? stringTyped = Console.ReadLine();
            switch (stringTyped)
            {
                default:
                    console = "Invalid option, try again!";
                    InitialLogic();
                    break;
                case "0":
                    SwitchAPI();
                    break;
                case "1":
                    ProcessRecordedData();
                    break;
                case "2":
                    CompareJsonData();
                    break;
            }
        }

#if (DEBUGX86 || DEBUGX64)
        static void SwitchAPI()
        {
            try
            {
                string exePath = Path.Combine(Environment.CurrentDirectory, "scoring-analysis.exe");
                if (preset == "JDNOW API")
                {
                    Process.Start(exePath.Replace("x86", "x64"));
                }
                else
                {
                    Process.Start(exePath.Replace("x64", "x86"));
                }
            }
            catch (Win32Exception)
            {
                console = "Unable to open x86 version of this project, verify your files integrity!";
                InitialLogic();
            }            
        }
#elif (RELEASEX86 || RELEASEX64)
        static void SwitchAPI()
        {
            try
            {
                string exePath = Path.Combine(Environment.CurrentDirectory, "Assemblies", "scoring-analysis.exe");
                if (preset == "JDNOW API")
                {
                    Process.Start(exePath.Replace(@"Assemblies\", ""));
                }
                else
                {
                    Process.Start(exePath);
                }
            }
            catch (Win32Exception)
            {
                console = "Unable to open x64 version of this project, verify your files integrity!";
                InitialLogic();
            }            
        }
#endif
#if (DEBUGX86 || RELEASEX86)
        static void ProcessRecordedData()
        {
            WriteStaticHeader(true, "Select a file...", 1);
            DialogResult dialogResult = Dialog.FileOpen("json");
            if (dialogResult.IsCancelled) { console = "Operation cancelled..."; InitialLogic(); }
            ScoringRecorder recordedData = JsonConvert.DeserializeObject<ScoringRecorder>(File.ReadAllText(dialogResult.Path));
            WriteStaticHeader(true, "Verifying file...", 1);
            if (string.IsNullOrEmpty(recordedData.mapName) || recordedData.moves == null || recordedData.recordedAccData == null || recordedData.recordedScore == null)
            {
                console = "Error: Seems like you have selected an incorrect file, verify your file structure or select a valid one!";
                InitialLogic();
            }
            else
            {
                GenerateJDNEXTJSON(recordedData);
                GenerateJDNOWJSON(recordedData);
                console = "Successfully created JDNEXT-JSON and JDNOW-JSON on comparatives directory!";
                InitialLogic();
            }
        }        

        static void GenerateJDNEXTJSON(ScoringRecorder recordedData)
        {
            WriteStaticHeader(false, "Generating JDNEXT-JSON...", 1);
            ComparativeJSON jdnextJSON = new()
            {
                mapName = recordedData.mapName,
                comparativeType = ComparativeType.JDNEXT,
                values = recordedData.recordedScore
            };
            File.WriteAllText(Path.Combine(GetOrCreateComparativesDirectory(recordedData), "jdnext.json"), JsonConvert.SerializeObject(jdnextJSON, Formatting.Indented));
        }        

        static void GenerateJDNOWJSON(ScoringRecorder recordedData)
        {
            WriteStaticHeader(true, "Initializing JDNOW score api...", 1);
            Scoring scoring = new();
            Move lastMove = recordedData.moves.Last();
            int classifiersSuccessCount = 0;
            int classifiersFailureCount = 0;
            foreach (Move move in recordedData.moves)
            {
                bool classifierLoaded = scoring.LoadClassifier(move.data, Convert.FromBase64String(move.data));
                bool moveLoaded = scoring.LoadMove(move.data, (int)(move.time * 1000), (int)(move.duration * 1000), Convert.ToBoolean(move.goldMove), move.Equals(lastMove));
                if (classifierLoaded && moveLoaded) { classifiersSuccessCount++; } else { classifiersFailureCount++; }
            }
            if (classifiersFailureCount != 0)
            {
                console = $"Error: At least one classifier failed to load!";
                InitialLogic();
            }
            else
            {
                Console.Clear();
                Console.WriteLine("Scoring...");
                List<RecordedScore> recordedValues = new();
                int moveID = 0; float lastScore = 0f;
                foreach (RecordedAccData accData in recordedData.recordedAccData)
                {
                    ScoreResult scoreResult = scoring.GetLastScore();
                    (int, float) scoreData = GetScoreData(scoreResult, moveID, lastScore, recordedValues);
                    moveID = scoreData.Item1; lastScore = scoreData.Item2;
                    scoring.AddSample(accData.accX, accData.accY, accData.accZ, accData.mapTime - 0.1f);
                }
                ComparativeJSON jdnowJSON = new()
                {
                    mapName = recordedData.mapName,
                    comparativeType = ComparativeType.JDNOW,
                    values = recordedValues
                };
                File.WriteAllText(Path.Combine(GetOrCreateComparativesDirectory(recordedData), "jdnow.json"), JsonConvert.SerializeObject(jdnowJSON, Formatting.Indented));
            }
        }

        static (int, float) GetScoreData(ScoreResult scoreResult, int moveID, float lastScore, List<RecordedScore> recordedValues)
        {            
            if (scoreResult.moveNum == moveID)
            {
                string feedback = string.Empty;
                switch (scoreResult.rating)
                {
                    case 0:
                        if (scoreResult.isGoldMove)
                        {
                            feedback = "MISSYEAH";
                        }
                        else
                        {
                            feedback = "MISS";
                        }
                        break;
                    case 1:
                        feedback = "OK";
                        break;
                    case 2:
                        feedback = "GOOD";
                        break;
                    case 3:
                        feedback = "PERFECT";
                        break;
                    case 4:
                        feedback = "YEAH";
                        break;
                }
                recordedValues.Add(new() { feedback = feedback, addedScore = scoreResult.totalScore - lastScore, totalScore = scoreResult.totalScore});
                moveID++; lastScore = scoreResult.totalScore;
            }
            return (moveID, lastScore);
        }
#elif (DEBUGX64 || RELEASEX64)
        static void ProcessRecordedData()
        {
            WriteStaticHeader(true, "Select a file...", 1);
            DialogResult dialogResult = Dialog.FileOpen("json");
            if (dialogResult.IsCancelled) { console = "Operation cancelled..."; InitialLogic(); }
            ScoringRecorder recordedData = JsonConvert.DeserializeObject<ScoringRecorder>(File.ReadAllText(dialogResult.Path));
            WriteStaticHeader(true, "Verifying file...", 1);
            if (string.IsNullOrEmpty(recordedData.mapName) || recordedData.moves == null || recordedData.recordedAccData == null || recordedData.recordedScore == null)
            {
                console = "Error: Seems like you have selected an incorrect file, verify your file structure or select a valid one!";
                InitialLogic();
            }
            else
            {
                GenerateUAFJSON(recordedData);
                console = "Successfully created UAF-JSON on comparatives directory!";
                InitialLogic();
            }
        }

        static void GenerateUAFJSON(ScoringRecorder recordedData)
        {
            WriteStaticHeader(true, $"Initializing UAF score api...{newLine}", 1);
            ScoreManager scoreManager = new(); 
            scoreManager.Init();            
            float moveScore = 0f; 
            float goldScore = 0f; 
            float finalScore = 0f;
            GetScoreValues(ref moveScore, ref goldScore, recordedData);
            List<RecordedScore> recordedValues = new();
            foreach (Move move in recordedData.moves)
            {
                ComputeAccelerometerData(move, Convert.FromBase64String(move.data), ref scoreManager, recordedData);
                float percentage = GetPercentage(scoreManager);
                string feedback = GetFeedback(move, percentage);
                float score = GetScore(move, moveScore, goldScore, percentage);
                finalScore += score;
                recordedValues.Add(new() { feedback = feedback, addedScore = score, totalScore = finalScore });
                Console.WriteLine($"Pointer: {percentage}");
            }
            ComparativeJSON uafJSON = new()
            {
                mapName = recordedData.mapName,
                comparativeType = ComparativeType.UAF,
                values = recordedValues
            };
            File.WriteAllText(Path.Combine(GetOrCreateComparativesDirectory(recordedData), "uaf.json"), JsonConvert.SerializeObject(uafJSON, Formatting.Indented));
        }

        static void GetScoreValues(ref float moveScore, ref float goldScore, ScoringRecorder recordedData)
        {
            float totalScore = 13333f;
            goldScore = 750f;
            moveScore = totalScore - goldScore;
            int goldCount = 0;
            int moveCount = 0;
            foreach (Move move in recordedData.moves)
            {
                if (move.goldMove == 1)
                {
                    goldCount++;
                }
                else
                {
                    moveCount++;
                }
            }
            goldScore = goldScore / goldCount;
            moveScore = moveScore / moveCount;
        }

        static void ComputeAccelerometerData(Move move, byte[] moveData, ref ScoreManager scoreManager, ScoringRecorder recordedData)
        {
            fixed (byte* movePointer = &moveData[0])
            {
                scoreManager.StartMoveAnalysis(movePointer, (uint)moveData.Length, move.duration);
                foreach (RecordedAccData accData in recordedData.recordedAccData)
                {
                    if (accData.mapTime >= move.time && accData.mapTime <= (move.time + move.duration))
                    {
                        float time = InverseLerp(accData.mapTime - 0.1f, move.time, move.time + move.duration);
                        scoreManager.bUpdateFromProgressRatioAndAccels(time, accData.accX, accData.accY, accData.accZ);
                    }
                }
                scoreManager.StopMoveAnalysis();
            }
        }

        static float GetPercentage(ScoreManager scoreManager)
        {
            float percentage = 0f;
            for (int i = 1; i < 20; i++)
            {
                float tempPercentage = scoreManager.GetSignalValue((byte)i);
                if (tempPercentage.ToString() != "4,2949673E+09" && tempPercentage > 0) percentage += tempPercentage;
            }
            percentage = percentage / 100;
            if (percentage > 0.1f) percentage = 0.1f;
            return percentage;
        }

        static string GetFeedback(Move move, float percentage)
        {
            if (move.goldMove == 1 && percentage > 0.025f)
            {
                return "YEAH";
            }
            else if (move.goldMove == 1 && percentage < 0.025f)
            {
                return "MISSYEAH";
            }
            if (percentage < 0.025f)
            {
                return "MISS";
            }
            else if (percentage < 0.04f)
            {
                return "OK";
            }
            else if (percentage < 0.06f)
            {
                return "GOOD";
            }
            else if (percentage < 0.08f)
            {
                return "SUPER";
            }
            else
            {
                return "PERFECT";
            }
        }

        static float GetScore(Move move, float moveScore, float goldScore, float percentage)
        {
            float score = 0f;
            if (move.goldMove == 1 && percentage > 0.025f)
            {
                score = goldScore;
            }
            else if (percentage > 0.025f)
            {
                score = Single.Lerp(0, moveScore, percentage / 2) * 10f;
            }
            return score;
        }

        static float InverseLerp(float value, float a, float b)
        {
            if (a == b) { return 0f; }
            return (value - a) / (b - a);
        }
#endif
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

        static string GetOrCreateComparativesDirectory(ScoringRecorder recordedData)
        {
            string comparativesDirectory = Path.Combine(Environment.CurrentDirectory, "Comparatives").Replace(@"Assemblies\", "");
            if (!Directory.Exists(comparativesDirectory)) Directory.CreateDirectory(comparativesDirectory);
            string mapComparativesDirectory = Path.Combine(comparativesDirectory, recordedData.mapName);
            if (!Directory.Exists(mapComparativesDirectory)) Directory.CreateDirectory(mapComparativesDirectory);
            return mapComparativesDirectory;
        }

        static void CompareJsonData()
        {
            WriteStaticHeader(true, "Select a valid folder...", 2);
            DialogResult dialogResult = Dialog.FolderPicker(Path.Combine(Environment.CurrentDirectory, "Comparatives").Replace(@"Assemblies\", ""));
            if (dialogResult.IsCancelled) { console = "Operation cancelled..."; InitialLogic(); }
            
            try
            {
                if (!File.Exists(Path.Combine(dialogResult.Path, "jdnext.json")) || !File.Exists(Path.Combine(dialogResult.Path, "jdnow.json")) || !File.Exists(Path.Combine(dialogResult.Path, "uaf.json")))
                {
                    console = $"Error: Seems like you have selected an incorrect folder, verify your folder structure or select a valid one!";
                    InitialLogic();
                }
                ComparativeJSON jdnext; ComparativeJSON jdnow; ComparativeJSON uaf;
                jdnext = JsonConvert.DeserializeObject<ComparativeJSON>(File.ReadAllText(Path.Combine(dialogResult.Path, "jdnext.json")));
                jdnow = JsonConvert.DeserializeObject<ComparativeJSON>(File.ReadAllText(Path.Combine(dialogResult.Path, "jdnow.json")));
                uaf = JsonConvert.DeserializeObject<ComparativeJSON>(File.ReadAllText(Path.Combine(dialogResult.Path, "uaf.json")));
                if (jdnow.values.Count != jdnext.values.Count || uaf.values.Count != jdnext.values.Count)
                {
                    console = "Error: Impossible to compare. These files don't have the same number of recorded moves!";
                    InitialLogic();
                }
                WriteStaticHeader(true, $"Generated comparative:{newLine}{newLine}", 2);
                Console.Write("JDNEXT".PadRight(39)); Console.Write("JDNOW".PadRight(39)); Console.Write("UAF");
                Console.Write($"{newLine}" + new string('=', 117));
                Console.Write($"{newLine}"); Console.Write("Added".PadRight(12)); Console.Write("Total".PadRight(11)); Console.Write("Feedback       |");
                Console.Write("Added".PadRight(12)); Console.Write("Total".PadRight(11)); Console.Write("Feedback       |");
                Console.Write("Added".PadRight(12)); Console.Write("Total".PadRight(11)); Console.Write("Feedback       |");
                for (int i = 0; i < jdnext.values.Count; i++)
                {
                    Console.Write($"{newLine}");
                    GenerateComparative(jdnext, i);
                    GenerateComparative(jdnow, i);
                    GenerateComparative(uaf, i);
                }
                Console.ReadLine();
            }
            catch (Exception)
            {
                console = "Error: Verify your files structure before try again!";
                InitialLogic();
            }
        }

        static void GenerateComparative(ComparativeJSON comparative, int index)
        {            
            string addedScore = comparative.values[index].addedScore.ToString();
            while (addedScore.Length != 12) addedScore += " ";
            Console.Write(addedScore);
            string addedTotalScore = comparative.values[index].totalScore.ToString();
            while (addedTotalScore.Length != 11) addedTotalScore += " ";
            Console.Write(addedTotalScore);
            string addedFeedback = comparative.values[index].feedback;
            while (addedFeedback.Length != 15) addedFeedback += " ";
            Console.Write(addedFeedback + "|");
        }
    }
}
