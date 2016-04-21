using System;
using System.Configuration;
using System.Linq;

namespace AyoController
{
    /// <summary>
    /// Add AyO graphic chart to yours plugins
    /// </summary>
    static public class AyO
    {
        public struct Chatclass
        {
            public string Login;
            public string Text;
            public int ThisNow;
        }

        public struct _Commands
        {
            public string Command;
            /// <summary>
            /// Params of the command.
            /// </summary>
            /// <remarks>
            /// Use the same var name for the param! 'paramfieldname'
            /// If you want to create a normal param, use 'v|paramname'
            /// </remarks>
            public string[] Params;
        }

        public struct HelpCommands
        {
            public string FieldClassName;
            public string Path;
            public _Commands[] Commands;
            public HelpCommands[] ChildCommands;

            public bool Any()
            {
                if (Path != "") return true;
                if (Commands.Length > 0) return true;
                if (Commands.Length == 0) return false;
                if (Path == "") return false;
                return false;
            }
        }

       

        public class Settings : Attribute
        {
            public string NameToShow;
            public bool ShowByDefault;
            public string Description;

            public Settings(string nameToShow, string description, bool showByDefault)
            {
                NameToShow = nameToShow;
                ShowByDefault = showByDefault;
                Description = description;
            }

            public Settings(string nameToShow, string description)
            {
                NameToShow = nameToShow;
                Description = description;
                ShowByDefault = true;
            }

            public Settings(string nameToShow)
            {
                NameToShow = nameToShow;
            }
        }

        public enum BeautifyType
        {
            UpperCase,
            UnderScore,
            AutoUpperCase
        }

        static public string BeautifyString(BeautifyType type,string toBeautify)
        {
            if (type == BeautifyType.UpperCase)
                return string.Concat(toBeautify.Select(c => char.IsUpper(c) ? " " + c.ToString() : c.ToString()))
        .TrimStart();
            else if (type == BeautifyType.UnderScore)
                return toBeautify.Replace("_", " ");
            else return "";
        }

        /// <summary>
        /// Print a text
        /// </summary>
        /// <param name="text">Text to show</param>
        /// <param name="color">Color to show on the text</param>
        static public void print(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        /// <summary>
        /// Print a simple white text
        /// </summary>
        /// <param name="text"></param>
        static public void print(string text)
        {
            print(text, ConsoleColor.White);
        }

        /// <summary>
        /// Add a '»" to the start of the text with color
        /// </summary>
        /// <param name="signs"></param>
        /// <param name="text"></param>
        static public void print(bool signs, string text, ConsoleColor color)
        {
            if (signs)
                print("» " + text, color);
            else
                print("[COMMAND] " + text, color);
        }

        /// <summary>
        /// Add a '»" to the start of the text
        /// </summary>
        /// <param name="signs"></param>
        /// <param name="text"></param>
        static public void print(bool signs, string text)
        {
            if (signs)
                print("» " + text);
            else
                print("[COMMAND] " + text);
        }

        static public string ChatPrefix
        {
            get
            {
                return "$999AY$fffo $ff0» $fff";
            }
        }

        public struct ErrorLog
        {
            public string ErrorString;
            public int ErrorCode;
            public bool IsError;
        }

        public enum PluginFunction
        {
            Settings = 1,
            MapList = 2,
            ScriptSettings = 3,
            RecordsTa = 4,
            RecordsRound = 5,
            RecordsLive = 6,
            RecordsOnline = 7,
            RecordsLocal = 8,
            PlayersList1 = 9,
            PlayersList2 = 10,
            PlayersList3 = 11,
            PlayerInformation1 = 12,
            PlayerInformation2 = 13,
            PlayerInformation3 = 14,
            Global = 15,
            Nothing = 16
        }

        static public string CompressEnviroName(string enviro)
        {
            if (enviro == "Valley" || enviro == "Stadium" || enviro == "Lagoon" || enviro == "Canyon")
            {
                return "tm";
            }
            else if (enviro == "Storm" || enviro == "Cryo" || enviro == "Meteor")
            {
                return "sm";
            }
            else return "qm";
        }
    }
		
	class MainClass
	{

        public static void Main(string[] args)
        {
            while (true)
            {
				
				Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(@" Launching :
                 ____     _____            _             _ _           
     /\         / __ \   / ____|          | |           | | |          
    /  \  _   _| |  | | | |     ___  _ __ | |_ _ __ ___ | | | ___ _ __ 
   / /\ \| | | | |  | | | |    / _ \| '_ \| __| '__/ _ \| | |/ _ \ '__|
  / ____ \ |_| | |__| | | |___| (_) | | | | |_| | | (_) | | |  __/ |   
 /_/    \_\__, |\____/   \_____\___/|_| |_|\__|_|  \___/|_|_|\___|_|   
           __/ |                                                       
          |___/                                                        
");
				Console.ForegroundColor = ConsoleColor.White;
				string currentBuildname = System.Diagnostics.Process.GetCurrentProcess ().ProcessName;

				if (!System.IO.File.Exists(currentBuildname + ".exe.config"))
                {
					Console.WriteLine("Config file not found !\nRegenerating new File");
					string sb = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
<startup><supportedRuntime version=""v4.0"" sku="".NETFramework,Version=v4.5""/></startup>
  <appSettings>
    <add key=""IP"" value=""127.0.0.1""/>
    <add key=""XMLRPC Port"" value=""5000""/>
    <add key=""Reconnect TimeOut"" value=""5000""/>
    <add key=""SuperAdmin Login"" value=""SuperAdmin""/>
    <add key=""SuperAdmin Password"" value=""SuperAdmin""/>
  </appSettings>
  </configuration>
";

					System.IO.File.WriteAllText(String.Concat(currentBuildname, ".exe.config"), sb);
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine ("Done, please restart.");
					Console.ForegroundColor = ConsoleColor.White;
                    return;
                }

                /*string configIni = args[0];

                if (!System.IO.File.Exists(configIni))
                {
                    Console.WriteLine(configIni + " not found !");
                    return;
                }*/

				var ConfigManager = new Classes.Config();

				ConfigManager = new Classes.Config ();
					
				if (ConfigManager.ParseFromConfigFile())
                {
					Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Settings loaded !");
                }
                else
				{
					Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Unable to load settings !");
					Console.ForegroundColor = ConsoleColor.White;
                }

                Console.WriteLine("Loading plugins ...");

				Classes.ServerManager serverManager = new AyoController.Classes.ServerManager(ConfigManager);

                serverManager.Initialize();

                while (true)
                {

                    string command = Console.ReadLine();

                    foreach (Plugins.Plugin plugin in serverManager.LoadedPlugins)
                    {
                        plugin.OnConsoleCommand(command);
                    }

                    serverManager.OnConsoleCommand(command);

                    if (command == "unload")
                    {
                        serverManager.UnloadPlugins();
                    }

                    if (command == "load")
                    {
                        serverManager.LoadPlugins();
                    }

                    if (command == "quit")
                    {
                        Console.WriteLine("Shutting down ...");
                        System.Diagnostics.Process.GetCurrentProcess().Kill();
                    }

                }
                
            }
        }
	}
}
