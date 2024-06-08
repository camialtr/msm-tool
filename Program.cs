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
                    NotImplemented();
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
            Console.Clear();
            Console.WriteLine(header);
            Console.WriteLine($"{commands[1].Replace("[1] ", "")}{newLine}");
            Console.Write($"[Console]");
            console = "Select a file...";
            Thread.Sleep(500);
            Console.Write($"{newLine}{newLine}{DateTime.Now.ToString("hh:mm:ss")} - {console}");
            ScoringRecorder recordedData = JsonConvert.DeserializeObject<ScoringRecorder>(File.ReadAllText(Dialog.FileOpen("json", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)).Path));
            console = "Verifying file...";
            Console.Write($"{newLine}{DateTime.Now.ToString("hh:mm:ss")} - {console}");
            if (string.IsNullOrEmpty(recordedData.mapName) || recordedData.moves == null || recordedData.recordedAccData == null || recordedData.recordedScore == null)
            {
                console = "Error: Seems like you have selected an incorrect file, verify your file structure or select a valid one!";
                InitialLogic();
            }
            else
            {
                console = "Creating JDNEXT-JSON...";
                Console.Write($"{newLine}{DateTime.Now.ToString("hh:mm:ss")} - {console}");
                ComparativeJSON jdnextJSON = new()
                {
                    mapName = recordedData.mapName,
                    comparativeType = ComparativeType.JDNEXT,
                    values = recordedData.recordedScore
                };
                string comparativesDirectory = Path.Combine(Environment.CurrentDirectory, "Comparatives");
                if (!Directory.Exists(comparativesDirectory)) Directory.CreateDirectory(comparativesDirectory);
                string mapComparativesDirectory = Path.Combine(comparativesDirectory, recordedData.mapName);
                if (!Directory.Exists(mapComparativesDirectory)) Directory.CreateDirectory(mapComparativesDirectory);
                console = "Saving JDNEXT-JSON...";
                Console.Write($"{newLine}{DateTime.Now.ToString("hh:mm:ss")} - {console}");
                File.WriteAllText(Path.Combine(mapComparativesDirectory, "jdnext.json"), JsonConvert.SerializeObject(jdnextJSON, Formatting.Indented));
                console = "Starting JDNOW API...";
                Console.Write($"{newLine}{DateTime.Now.ToString("hh:mm:ss")} - {console}");
                Scoring scoring = new();
                console = "Successfully initialized with ID: " + scoring.GetScoringID();
                Console.Write($"{newLine}{DateTime.Now.ToString("hh:mm:ss")} - {console}");
                console = "Loading classifiers...";
                Console.Write($"{newLine}{DateTime.Now.ToString("hh:mm:ss")} - {console}");
                Thread.Sleep(500);
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
                    console = $"Successfully loaded {classifiersSuccessCount} classifiers!";
                    Console.Write($"{newLine}{DateTime.Now.ToString("hh:mm:ss")} - {console}");
                    //to-do SCORING
                }
            }
        }
#elif (DEBUGX64 || RELEASEX64)
        static void ProcessRecordedData()
        {

        }
#endif
    }
}
