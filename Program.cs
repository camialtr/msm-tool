using System.Text;
using System.Text.Json;
using System.Security.Cryptography;

#pragma warning disable CS8600
#pragma warning disable CS8601
#pragma warning disable CS8602
#pragma warning disable IDE0071
namespace msm_tools;

internal unsafe class Program : Base
{
    #if (DEBUGX86 || RELEASEX86)
    static void Main(string[] args)
    {
        try
        {
            HandleBoot();
        }
        catch {}
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
            console = "...";
            #if DEBUGX64
            InitialLogic();
            #endif
            #if RELEASEX64
            Authenticate();
            #endif
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

    static void Authenticate()
    {
        Console.Clear();
        Console.WriteLine(header);
        Console.WriteLine("  You need permission to access this program");
        Console.Write($"{newLine}Type password: ");
        Console.Write($"{newLine}{newLine}[Console]");
        Console.Write($"{newLine}{newLine}{DateTime.Now.ToString("hh:mm:ss")} - {console}");
        Console.SetCursorPosition(15, 5);
        string password = string.Empty;
        ConsoleKey key;
        do
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
            key = keyInfo.Key;
            if (key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password.Substring(0, password.Length - 1);
                Console.Write("\b \b");
            }
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                password += keyInfo.KeyChar;
                Console.Write("*");
            }
        } while (key != ConsoleKey.Enter);
        console = "Checking credentials...";
        Console.Clear();
        Console.WriteLine(header);
        Console.WriteLine("  You need permission to access this program");
        Console.Write($"{newLine}[Console]");
        Console.Write($"{newLine}{newLine}{DateTime.Now.ToString("hh:mm:ss")} - {console}");
        HttpClient client = new();
        if (GetSHA256(password) == Encoding.UTF8.GetString(client.GetByteArrayAsync(checkin).Result))
        {
            console = "...";
            InitialLogic();
        }
        else
        {
            console = "Wrong password, try again!";
            Authenticate();
        }
    }

    static string GetSHA256(string input)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = SHA256.HashData(bytes);
        StringBuilder builder = new();
        foreach (byte b in hashBytes)
        {
            builder.Append(b.ToString("x2"));
        }
        return builder.ToString();
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
            case "1":
                ScoreFunctions.ProcessRecordedDataLocal();
                break;
            case "2":
                MoveSpaceFunctions.GenerateMSMsFromRecordedData();
                break;
            case "3":
                MoveSpaceFunctions.ExtractMSMsFromMapFolder();
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
        string settingsFilePath = BuildPath(middlePath, @"Settings.json").Replace(@"Assemblies\", "");
        if (!File.Exists(settingsFilePath))
        {
            JsonSerializerOptions options = new() { WriteIndented = true };
            Settings defaultSettings = new()
            {
                mapsPath = @"C:\Games\Just Dance Next\Just Dance Next_Data\Maps",
                defaultLowThreshold = 1.0f,
                defaultHighThreshold = 3.0f,
                defaultAutoCorrelationThreshold = 1.0f,
                defaultDirectionImpactFactor = -1.0f,
                defaultCustomizationBitField = 2
            };
            File.WriteAllText(settingsFilePath, JsonSerializer.Serialize(defaultSettings, options));
        }
        settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(settingsFilePath).Replace("\\", @"\"));
        mapsPath = settings.mapsPath;
    }        
}
