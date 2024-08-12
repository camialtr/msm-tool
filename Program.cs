using System.Text.Json;

#pragma warning disable CS8600
#pragma warning disable CS8601
#pragma warning disable CS8602
#pragma warning disable IDE0071
namespace jd_tools;

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
    #elif (DEBUGX64 || RELEASEX64)
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
                ScoreFunctions.ProcessRecordedDataLocal();
                break;
        }
    }
    #endif
    static void HandleBoot()
    {
        string middlePath = @"\";
        #if DEBUGX64
        middlePath = @"bin\x64\Debug\net8.0\";
        #elif DEBUGX86
        middlePath = @"bin\x86\Debug\net8.0\";
        #endif
        string settingsFilePath = BuildPath(middlePath, @"settings.json").Replace(@"Assemblies\", "");
        if (!File.Exists(settingsFilePath))
        {
            Settings defaultSettings = new()
            {
                mapsPath = @"D:\Just Dance Next\Just Dance Next_Data\Maps"
            };
            File.WriteAllText(settingsFilePath, JsonSerializer.Serialize(defaultSettings));
        }
        Settings settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(settingsFilePath).Replace("\\", @"\"));
        mapsPath = settings.mapsPath;
    }        
}
