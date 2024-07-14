#if (DEBUGX86 || RELEASEX86)
using JDNow;
#elif (DEBUGX64 || RELEASEX64)
using MoveSpaceWrapper;
#endif
using Newtonsoft.Json;
using System.Diagnostics;
using NativeFileDialogSharp;

#pragma warning disable CS8600
#pragma warning disable CS8602
#pragma warning disable CS8604
#pragma warning disable IDE0071
namespace jd_tools
{
    internal unsafe class Program : Base
    {
#if (DEBUGX86 || RELEASEX86)
        static void Main(string[] args)
        {
            Console.Clear();
            if (args.Length == 0)
            {
                Console.WriteLine("This program is a sub function from the original one and is not meant to be used alone!");
                Console.ReadLine();
                Environment.Exit(0);
            }
            switch (args[0])
            {
                default:
                    Console.WriteLine("This program is a sub function from the original one and is not meant to be used alone!");
                    Console.ReadLine();
                    break;
                case "accdata":
                    ProcessRecordedData(args[1].Replace("|SPACE|", " "));
                    break;
            }
        }

        static void ProcessRecordedData(string recordedAccDataPath)
        {
            List<NewRecordedAccData> recordedData = JsonConvert.DeserializeObject<List<NewRecordedAccData>>(File.ReadAllText(recordedAccDataPath));
            Scoring scoring = InitializeScoring(recordedAccDataPath.Replace($@"\{Path.GetFileName(recordedAccDataPath)}", "").Replace(@"accdata", ""), recordedData[0].coachID - 1);
            List<RecordedScore> recordedValues = new();
            int moveID = 0; float lastScore = 0f;
            foreach (NewRecordedAccData accData in recordedData)
            {
                ScoreResult scoreResult = scoring.GetLastScore();
                (int, float) scoreData = GetScoreData(scoreResult, moveID, lastScore, recordedValues);
                moveID = scoreData.Item1; lastScore = scoreData.Item2;
                scoring.AddSample(accData.accX, accData.accY, accData.accZ, accData.mapTime);
            }
            scoring.EndScore();
            ComparativeJSON json = new()
            {
                comparativeType = ComparativeType.jdScoring,
                values = recordedValues
            };
            File.WriteAllText(Path.Combine(GetOrCreateComparativesDirectory(), "jdScoring.json"), JsonConvert.SerializeObject(json, Formatting.Indented));
            ProceedToMainFunction();
        }

        static Scoring InitializeScoring(string rootPath, int coachID)
        {
            Scoring scoring = new();
            Timeline timeline = JsonConvert.DeserializeObject<Timeline>(File.ReadAllText(rootPath + "timeline.json"));
            List<MoveFile> moveFiles = new();
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
                MoveFile file = moveFiles.Find(x => x.name == move.name);
                scoring.LoadClassifier(move.name, file.data);
                scoring.LoadMove(move.name, (int)(move.time * 1000), (int)(move.duration * 1000), Convert.ToBoolean(move.goldMove), moveIndex.Equals(moves.Count));
            }
            return scoring;
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
                recordedValues.Add(new() { feedback = feedback, addedScore = scoreResult.totalScore - lastScore, totalScore = scoreResult.totalScore });
                moveID++; lastScore = scoreResult.totalScore;
            }
            return (moveID, lastScore);
        }
        
        static void ProceedToMainFunction()
        {
            ProcessStartInfo processStartInfo = new()
            {
                FileName = Path.Combine(Environment.CurrentDirectory, "jd-tools.exe").Replace("x86", "x64"),
                Arguments = $"compare"
            };
            Process.Start(processStartInfo);
        }
#elif (DEBUGX64 || RELEASEX64)
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                InitialLogic();
                Environment.Exit(0);
            }
            switch (args[0])
            {
                default:
                    InitialLogic();
                    break;
                case "compare":
                    Compare();
                    break;
            }
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
            Console.SetCursorPosition(11, 6 + commands.Length);
            string? stringTyped = Console.ReadLine();
            switch (stringTyped)
            {
                default:
                    console = "Invalid option, try again!";
                    InitialLogic();
                    break;
                case "0":
                    ProcessRecordedData();
                    break;
            }
        }

        static void ProcessRecordedData()
        {
            WriteStaticHeader(true, $"Select a file...", 0);
            DialogResult dialogResult = Dialog.FileOpen("json", mapsPath);
            if (dialogResult.IsCancelled) { console = "Operation cancelled..."; InitialLogic(); }
            List<NewRecordedAccData> recordedAccData = new();
            try
            {
                recordedAccData = JsonConvert.DeserializeObject<List<NewRecordedAccData>>(File.ReadAllText(dialogResult.Path));
            }
            catch
            {
                console = "Error: Seems like you have selected an incorrect file, verify your file structure or select a valid one!";
                InitialLogic();
            }
            string rootPath = dialogResult.Path.Replace($@"\{Path.GetFileName(dialogResult.Path)}", "").Replace(@"accdata", "");
            Timeline timeline = JsonConvert.DeserializeObject<Timeline>(File.ReadAllText(rootPath + "timeline.json"));
            List<Move> moves = timeline.moves.FindAll(x => x.coachID == recordedAccData[0].coachID - 1);
            List<RecordedScore> recordedValues = new();
            float moveScoreValue = 0f;
            float goldScoreValue = 0f;
            float totalScore = 0f;
            GetScoreValues(ref moveScoreValue, ref goldScoreValue, moves);
            ScoreManager scoreManager = new();
            scoreManager.Init(1f, 1f, 1f, 1f, 60f);
            foreach (Move move in moves)
            {
                float percentage = ComputeMoveSpace(scoreManager, move, recordedAccData, rootPath);
                float score = GetScore(move, moveScoreValue, goldScoreValue, percentage);
                totalScore += score;
                string feedback = GetFeedback(move, percentage);
                recordedValues.Add(new() { addedScore = score, totalScore = totalScore, feedback = feedback });
            }
            Console.ReadKey();
            scoreManager.Dispose();
            ComparativeJSON json = new()
            {
                comparativeType = ComparativeType.MoveSpaceWrapper,
                values = recordedValues
            };
            File.WriteAllText(Path.Combine(GetOrCreateComparativesDirectory(), "MoveSpaceWrapper.json"), JsonConvert.SerializeObject(json, Formatting.Indented));
            ProceedToSubFunction(dialogResult.Path);
        }

        static void GetScoreValues(ref float moveScore, ref float goldScore, List<Move> moves)
        {
            float totalScore = 13333f;
            goldScore = 750f;
            moveScore = totalScore - goldScore;
            int goldCount = 0;
            int moveCount = 0;
            foreach (Move move in moves)
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

        static float ComputeMoveSpace(ScoreManager scoreManager, Move move, List<NewRecordedAccData> recordedAccData, string rootPath)
        {            
            MoveSpaceFileHandler file = MoveSpaceFileHandler.GetFile(rootPath + $@"moves\{move.name}.msm");
            scoreManager.StartMoveAnalysis((void*)file.FileContent, (uint)file.Length, move.duration);
            List<NewRecordedAccData> samples = GetSampleDataFromTimeRange(recordedAccData, move.time, move.duration);
            for (int sID = 0; sID < samples.Count; ++sID)
            {
                NewRecordedAccData sample = samples[sID];
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
            float scorePercentage = scoreManager.GetLastMovePercentageScore();
            file.Dispose();
            return scorePercentage;
        }

        static List<NewRecordedAccData> GetSampleDataFromTimeRange(List<NewRecordedAccData> recordedAccData, float time, float duration)
        {
            List<NewRecordedAccData> toReturn = new();
            foreach (NewRecordedAccData accData in recordedAccData)
            {
                if (accData.mapTime >= time && accData.mapTime <= (time + duration))
                {
                    toReturn.Add(accData);
                }
            }
            return toReturn;
        }

        static float Clamp(float value, float min, float max) 
        {
            float toReturn = value;
            if (value <= min) toReturn = min;
            if (value >= max) toReturn = max;
            return toReturn;
        }

        static float GetScore(Move move, float moveScoreValue, float goldScoreValue, float percentage)
        {
            float score = 0f;
            if (move.goldMove == 1)
            {
                score = Single.Lerp(0f, goldScoreValue, percentage / 100);
            }
            else
            {
                score = Single.Lerp(0f, moveScoreValue, percentage / 100);
            }
            return score;
        }

        static string GetFeedback(Move move, float percentage)
        {
            if (move.goldMove == 1 && percentage > 70.0f)
            {
                return "YEAH";
            }
            else if (move.goldMove == 1 && percentage < 70.0f)
            {
                return "MISSYEAH";
            }
            if (percentage < 25.0f)
            {
                return "MISS";
            }
            else if (percentage < 50.0f)
            {
                return "OK";
            }
            else if (percentage < 70.0f)
            {
                return "GOOD";
            }
            else if (percentage < 80.0f)
            {
                return "SUPER";
            }
            else
            {
                return "PERFECT";
            }
        }

        static void ProceedToSubFunction(string path)
        {
            ProcessStartInfo processStartInfo = new()
            {
                FileName = Path.Combine(Environment.CurrentDirectory, "Assemblies", "jd-tools.exe").Replace("x64", "x86"),
                Arguments = $"accdata {path.Replace(" ", "|SPACE|")}"
            };
#if DEBUGX64
            processStartInfo.FileName = processStartInfo.FileName.Replace(@"Assemblies\", "");
#endif
            Process.Start(processStartInfo);
        }

        static void Compare()
        {
            string comparativesDirectory = Path.Combine(Environment.CurrentDirectory, "Comparatives");
            if (!Directory.Exists(comparativesDirectory) || !File.Exists(Path.Combine(comparativesDirectory, "jdScoring.json")) || !File.Exists(Path.Combine(comparativesDirectory, "MoveSpaceWrapper.json")))
            {
                console = "Error: Incorrect structure or missing files at comparatives directory!";
                Directory.Delete(comparativesDirectory, true);
                InitialLogic();
            }
            ComparativeJSON jdScoring = JsonConvert.DeserializeObject<ComparativeJSON>(File.ReadAllText(Path.Combine(comparativesDirectory, "jdScoring.json")));            
            ComparativeJSON moveSpaceWrapper = JsonConvert.DeserializeObject<ComparativeJSON>(File.ReadAllText(Path.Combine(comparativesDirectory, "MoveSpaceWrapper.json")));
            WriteStaticHeader(false, $"Generated comparative:{newLine}{newLine}", 0);
            Console.Write("jdScoring".PadRight(37) + "MoveSpaceWrapper");
            Console.Write($"{newLine}" + new string('=', 74));
            Console.Write($"{newLine}"); Console.Write("Added".PadRight(12) + "Total".PadRight(12) + "Feedback".PadRight(12) + "|");
            Console.Write("Added".PadRight(12) + "Total".PadRight(12) + "Feedback".PadRight(12) + "|");
            for (int i = 0; i < jdScoring.values.Count; i++)
            {
                Console.Write($"{newLine}");
                GenerateComparative(jdScoring, i);
                GenerateComparative(moveSpaceWrapper, i);
            }
            Directory.Delete(comparativesDirectory, true);
            Console.Write($"{newLine}" + new string('=', 74));
            Console.Write($"{newLine}Type any key to exit...");
            Console.Write($"{newLine}" + new string('=', 74));
            Console.CursorVisible = false;
            Console.ReadKey();
            Console.CursorVisible = true;
            console = "...";
            InitialLogic();
        }
#endif
        static void GenerateComparative(ComparativeJSON comparative, int index)
        {
            Console.Write(comparative.values[index].addedScore.ToString().PadRight(12) + comparative.values[index].totalScore.ToString().PadRight(12) + comparative.values[index].feedback.ToString().PadRight(12) + "|");
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
            string comparativesDirectory = Path.Combine(Environment.CurrentDirectory, "Comparatives").Replace(@"Assemblies\", "").Replace("x86", "x64");
            if (!Directory.Exists(comparativesDirectory)) Directory.CreateDirectory(comparativesDirectory);
            return comparativesDirectory;
        }
    }
}
