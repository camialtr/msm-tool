using Newtonsoft.Json;

#pragma warning disable IDE0071
namespace jd_tools
{
    internal unsafe class Program : Base
    {
        #if (DEBUGX86 || RELEASEX86)
        static void Main(string[] args)
        {
            HandleBoot();
            if (args.Length == 0)
            {
                Console.WriteLine("This program is a sub function from the original one and is not meant to be used alone!");
                Console.ReadKey();
                Environment.Exit(0);
            }
            switch (args[0])
            {
                default:
                    Console.WriteLine("This program is a sub function from the original one and is not meant to be used alone!");
                    Console.ReadKey();
                    break;
                case "processrecordeddatalocal":
                    ScoreFunctions.ProcessRecordedDataLocal(args[1].Replace("|SPACE|", " "));
                    break;
            }
        }
        #elif (DEBUGX64 || RELEASEX64 || DEBUGANYCPU || RELEASEANYCPU)
        static void Main(string[] args)
        {
            HandleBoot();
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
                    ScoreFunctions.Compare();
                    break;
            }
        }

        public static void InitialLogic()
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
                    PickCompareCommand();
                    break;
            }
        }

        static void PickCompareCommand()
        {
            Console.Clear();
            Console.WriteLine(header);
            Console.WriteLine($"Select an option below: {newLine}");
            foreach (string command in compareCommands) Console.WriteLine(command);
            Console.Write($"{newLine}Type code: ");
            Console.Write($"{newLine}{newLine}[Console]");
            Console.Write($"{newLine}{newLine}{DateTime.Now.ToString("hh:mm:ss")} - {console}");
            Console.SetCursorPosition(11, 6 + compareCommands.Length);
            string? stringTyped = Console.ReadLine();
            switch (stringTyped)
            {
                default:
                    console = "Invalid option, try again!";
                    PickCompareCommand();
                    break;
                case "0":
                    console = "...";
                    InitialLogic();
                    break;
                case "1":
                    ScoreFunctions.ProcessRecordedDataLocal();
                    break;
                case "2":
                    ScoreFunctions.ProcessRecordedDataOnline();
                    break;
            } 
        }
        #endif
        static void HandleBoot()
        {
            string settingsFilePath = Environment.CurrentDirectory.Replace(@"\Assemblies", "") + @"\settings.json";
            if (!File.Exists(settingsFilePath))
            {
                Settings defaultSettings = new()
                {
                    mapsPath = @"D:\Just Dance Next\Just Dance Next_Data\Maps",
                    apiLink = ""
                };
                File.WriteAllText(settingsFilePath, JsonConvert.SerializeObject(defaultSettings, Formatting.Indented));
            }
            Settings settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingsFilePath).Replace("\\", @"\"));
            mapsPath = settings.mapsPath;
            apiLink = settings.apiLink;
        }        
    }
}
