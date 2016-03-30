using System;

namespace AyoController
{
    /// <summary>
    /// Add AyO graphic chart to yours plugins
    /// </summary>
    static public class AyO
    {
        static public string ChatPrefix
        {
            get
            {
                return "$999AY$fffo $ff0» $fff";
            }
        }

        public struct ErrorLog
        {
            public string    ErrorString;
            public int       ErrorCode;
            public bool      IsError;
        }

        public enum PluginFunction
        {
            Settings = 1,
            MapList = 2,
            ScriptSettings = 3,
            RecordsTA = 4,
            RecordsROUND = 5,
            RecordsLIVE = 6,
            RecordsONLINE = 7,
            RecordsLOCAL = 8,
            PlayersList1 = 9,
            PlayersList2 = 10,
            PlayersList3 = 11,
            PlayerInformation1 = 12,
            PlayerInformation2 = 13,
            PlayerInformation3 = 14,
            Global = 15,
            Nothing = 16   
        }

        static public string CompressEnviroName(string _enviro)
        {
            if (_enviro == "Valley" || _enviro == "Stadium" || _enviro == "Lagoon" || _enviro == "Canyon")
            {
                return "tm";
            }
            else if (_enviro == "Storm" || _enviro == "Cryo" || _enviro == "Meteor")
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

                Console.WriteLine(@" Launching :
               ___     __   __     
              /   \    \ \ / /   
             /  O  \    \ V /
            /   __  \    \ /    / _ \
           /   /__\  \   | |    |(_)|
           \_________/   \_/    \___/ 
");


                if (args.Length != 1)
                {
                    Console.WriteLine("Invalid argments !");
                    Console.WriteLine("AyoController Config.ini");
                    return;
                }

                string ConfigINI = args[0];

                if (!System.IO.File.Exists(ConfigINI))
                {
                    Console.WriteLine(ConfigINI + " not found !");
                    return;
                }

                Classes.Config config = new AyoController.Classes.Config();

                if (config.ParseFromIniFile(ConfigINI))
                {
                    Console.WriteLine("Settings loaded !");
                }
                else {
                    Console.WriteLine("Unable to load settings !");
                }

                Console.WriteLine("Loading plugins ...");
                Plugins.Manager.LoadPlugins();
                Console.WriteLine("Done, " + Plugins.Manager.LoadedPlugins.Count + " plugins found !");

                Classes.ServerManager serverManager = new AyoController.Classes.ServerManager(config);

                serverManager.Initialize();

                while (true)
                {

                    string command = Console.ReadLine();

                    foreach (Plugins.Plugin plugin in Plugins.Manager.LoadedPlugins)
                    {
                        plugin.OnConsoleCommand(command);
                    }

                    if (command == "reloadplugins")
                    {
                        Plugins.Manager.LoadPlugins();
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
