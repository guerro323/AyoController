using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using AyoController;
using AyoController.Plugins;
using ShootManiaXMLRPC.XmlRpc;

namespace IGAdmin
{
	public partial class IgAdmin : Plugin
	{

		private AyoController.Classes.ServerManager ServerManager { get; set; }
		private const string AdminsCfgFile = "IGAdmin__Admins.cfg";
		private readonly List<string> _admins = new List<string>();

		public override AyO.PluginFunction[] PluginFunction
		{
			get
			{
				return new AyO.PluginFunction[] {
					AyO.PluginFunction.Nothing
				};
			}
		}

        public override string Name {
			get {
				return "IGAdmin";
			}
		}

		public override string Author {
			get {
				return "JuJuBoSc";
			}
		}

        public string StructAd
        {
            get
            {
                return "$z$s$fff[$f70ADMIN$z$s$fff]$z$s";
            }
        }

		public override string Version {
			get {
				return "0.1";
			}
		}

        public override string[] ListofCommands
        {
            get
            {
                return new string[] { "/players","/admins" };
            }
        }

		public override void OnLoad ()
		{
			RefreshAdmins();
		}

        public override void OnLoop()
        {
        }

        public override void HandleEventGbxCallback(object o, GbxCallbackEventArgs e)
        {
        }


        public override void OnServerManagerInitialize (AyoController.Classes.ServerManager serverManager)
		{

			this.ServerManager = serverManager;
			this.ServerManager.OnConnectionSuccessful += HandleOnConnectionSuccessful;

		}

		public override void OnConsoleCommand (string command)
		{

			if (command == "igadmin reload") {
				RefreshAdmins();
			}

		}

	    public override void HandleOnConnectionSuccessful ()
		{
            ServerManager.Server.OnPlayerChat += HandleOnPlayerChat;
            ServerManager.Server.OnPlayerConnect += HandleOnPlayerConnect;;
		}

		void HandleOnPlayerConnect (ShootManiaXMLRPC.Structs.PlayerConnect pc)
		{
			/*if (Admins.Contains (PC.Login)) {
				ChatSendServerMessage("Admin connected : " + PC.Login);
			}*/
		}

		void HandleOnPlayerChat (ShootManiaXMLRPC.Structs.PlayerChat pc)
		{
			ParseChatCommand(pc);
		}

		private void RefreshAdmins ()
		{
            Console.WriteLine(" Load admins ...");

			_admins.Clear();

			string currentAssemblyDirectoryName = Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location);

			if (File.Exists (currentAssemblyDirectoryName + "/" + AdminsCfgFile)) {

				StreamReader sr = new StreamReader(currentAssemblyDirectoryName + "/" + AdminsCfgFile);

				string line = sr.ReadLine();

				while (line != null)
				{

					line = line.Trim();

					if (line != string.Empty &&
					    !line.StartsWith("#") &&
					    !_admins.Contains(line))
					{
						Console.WriteLine("[IGAdmin] Admin found : " + line);
						_admins.Add(line);
					}

					line = sr.ReadLine();
				}

				sr.Close();

			} else {
				Console.WriteLine("[IGAdmin] Unable to find : " + AdminsCfgFile);
			}

		}

		private void SaveAdmins ()
		{

			string currentAssemblyDirectoryName = Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location);

			if (File.Exists (currentAssemblyDirectoryName + "/" + AdminsCfgFile)) {
				try
				{
					File.Delete(currentAssemblyDirectoryName + "/" + AdminsCfgFile);
				}
				catch
				{
					Console.WriteLine("Error on deleting " + AdminsCfgFile + " !");
				}
			}

			StreamWriter sw = new StreamWriter(currentAssemblyDirectoryName + "/" + AdminsCfgFile);

			foreach (string admin in _admins)
				sw.WriteLine(admin);

			sw.Close();

		}

		public void ChatSendServerMessage(string message)
		{
			ServerManager.Server.ChatSendServerMessage(StructAd + " $fff "+ message);
		}

        public override void Unload()
        {
            ServerManager.Server.OnPlayerChat -= HandleOnPlayerChat;
            ServerManager.Server.OnPlayerConnect -= HandleOnPlayerConnect;
            ServerManager.OnConnectionSuccessful -= HandleOnConnectionSuccessful;
        }

    }
}

