#if (DEBUGX86 || RELEASEX86)
using JDNow;
#elif (DEBUGX64 || RELEASEX64)
using MoveSpaceWrapper;
#endif
using System.Diagnostics;

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
                    NotImplemented();
                    break;
                case "2":
                    NotImplemented();
                    break;
            }
        }

#if (DEBUGX86 || DEBUGX64)
        static void SwitchAPI()
        {
            string exePath = Path.Combine(Environment.CurrentDirectory, "scoring-analysis.exe");
            if (preset == "JDN API")
            {
                Process.Start(exePath.Replace("x86", "x64"));
            }
            else
            {
                Process.Start(exePath.Replace("x64", "x86"));
            }
        }
#elif (RELEASEX86 || RELEASEX64)
        static void SwitchAPI()
        {
            string exePath = Path.Combine(Environment.CurrentDirectory, "Assemblies", "scoring-analysis.exe");
            if (preset == "JDN API")
            {
                Process.Start(exePath.Replace(@"Assemblies\", ""));
            }
            else
            {
                Process.Start(exePath);
            }
        }
#endif
    }
}
