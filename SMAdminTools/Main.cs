using System;
using ShootManiaXMLRPC;

namespace AyoController
{
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
