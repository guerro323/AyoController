using System;
using System.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using AyoController;
using AyoController.Plugins;
using Newtonsoft.Json;
using LitJson;

namespace Commands
{
	public partial class Commands : Plugin
	{

		private AyoController.Classes.ServerManager ServerManager { get; set; }
		private const string AdminsCfgFile = "IGAdmin__Admins.cfg";
		private List<string> Admins = new List<string>();
        private Dictionary<string, Management> Records = new Dictionary<string, Management>();

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

        public string StructAD
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

        public override string[] listofCommands
        {
            get
            {
                return new string[] { "/help" };
            }
        }

        public override void OnLoad ()
		{
			RefreshAdmins();
            
        }

		public override void OnServerManagerInitialize (AyoController.Classes.ServerManager ServerManager)
		{

			this.ServerManager = ServerManager;
			this.ServerManager.OnConnectionSuccessful += HandleOnConnectionSuccessful;

            Records = new Dictionary<string, Management>();

            bool exists = Directory.Exists("Plugins/misc");

            if (!exists)
                Directory.CreateDirectory("Plugins/misc");

            Console.WriteLine(Records.Count);
            foreach (var Record in Records)
            {
                Console.WriteLine(Record.Value.Login);
            }
        }

		public override void OnConsoleCommand (string Command)
		{

			if (Command == "igadmin reload") {
				RefreshAdmins();
                ServerManager.Server.SetTATime(-1);
            }

		}

        string Format(TimeSpan obj)
        {
            StringBuilder sb = new StringBuilder();
            if (obj.Hours != 0)
            {
                sb.Append(obj.Hours);
                sb.Append(" ");
                sb.Append("hours");
                sb.Append(" ");
            }
            if (obj.Minutes != 0 || sb.Length != 0)
            {
                sb.Append("$6c0");
                sb.Append(obj.Minutes);
            }
            if (obj.Seconds != 0 || sb.Length != 0)
            {
                if (obj.Minutes == 0) sb.Append("$6c000");
                sb.Append("$fff:$6c0");
                if (obj.Seconds < 10) sb.Append(0);
                sb.Append(obj.Seconds);
            }
            if (obj.Milliseconds != 0 || sb.Length != 0)
            {
                if (obj.Milliseconds < 10) sb.Append(0);
                if (obj.Milliseconds < 100) sb.Append(0);
                sb.Append("$fff.$390"); //< seconds
                sb.Append(obj.Milliseconds);
            }
            if (sb.Length == 0)
            {
                /*sb.Append(0);
                sb.Append("");
                sb.Append("$fff.$390Milliseconds"); //< mili*/
            }
            return sb.ToString();
        }

    void HandleOnConnectionSuccessful ()
		{
			this.ServerManager.Server.OnPlayerChat += HandleOnPlayerChat;
			this.ServerManager.Server.OnPlayerConnect += HandleOnPlayerConnect;

        }

		void HandleOnPlayerConnect (ShootManiaXMLRPC.Structs.PlayerConnect PC)
		{
			if (Admins.Contains (PC.Login)) {
				ChatSendServerMessage("Admin connected : " + PC.Login);
			}


		}

		void HandleOnPlayerChat (ShootManiaXMLRPC.Structs.PlayerChat PC)
		{
			ParseChatCommand(PC);
		}

        private void UpdateInterface(string playerName, object[] Params, List<Management> LocalRecords, List<Management> DedimaniaRecords)
        {
            var toreturn = @"<label posn=""-148 83 0"" style=""TextButtonBig"" sizen=""25 14"" text=" + Params[0].ToString() + @" valign=""center2"" halign=""center"" textsize=""3""/>
		<label posn=""-123 83 0"" style=""TextButtonBig"" sizen=""25 14"" text=""#1"" valign=""center2"" halign=""center"" textsize=""6""/>
		<quad posn=""-160 70.5 1"" sizen=""25 0.5"" bgcolor=""FFFFFFFF""/>
		<label posn=""-148 73 0"" sizen=""25 6"" text=""LOCAL"" valign=""center2"" halign=""center"" textsize=""2""/>
		<label posn=""-123 73 0"" sizen=""25 6"" text=""DEDIMANIA"" valign=""center2"" halign=""center"" textsize=""1""/>
		<quad posn=""-160 90 0"" sizen=""50 20"" bgcolor=""000000AA""/>
		<quad posn=""-160 70 0"" sizen=""50 60"" bgcolor=""000000AA""/>
		<quad posn=""-160 71 -1"" sizen=""50 10"" bgcolor=""000000AA"" style=""Bgs1InRace"" substyle=""BgGradTop""/>
		<quad posn=""-160 20 -1"" sizen=""50 10"" bgcolor=""0000006D"" style=""Bgs1InRace"" substyle=""BgGradBottom"" modulatecolor=""00000087"" opacity=""0.5""/>
		<quad posn=""-160 76 0"" sizen=""50 6"" bgcolor=""000000AA""/>
		<quad posn=""-160 76 0"" sizen=""25 6"" bgcolor=""000000AA""/>";
            int rank = 0;
            List<Management> SortedList = LocalRecords.OrderBy(o => o.Time).ToList();
            foreach (var record in SortedList)
            {
                toreturn += @"<label posn=""0 " + -rank*10 + @" 0"" text=""#" + rank+1 + @" " + record.Time + @" by " + record.Pseudo + @""" />";
                Records[record.Login] = record;
                rank++;
            }
            
            ServerManager.AddThisManialink(playerName, toreturn, "Records", true);
        }

        class Management
        {
            public int Time;
            public int Id;
            public string Login;
            public string Pseudo;
        }

        private string RandomHelloServer()
        {
            Random rand = new Random();
            int result = rand.Next(1, 4);
            switch (result)
            {
                case 1:
                    {
                        return "Welcome everyone!";
                    }
                case 2:
                    {
                        return "Controller in work!";
                    }
                case 3:
                    {
                        return "Let's the kek be in you!";
                    }
            }
            return "Hey!";
        }


        public override void OnLoop ()
        {
            if (ChangeTimeMessage < Now)
            {
                ChangeTimeMessage = Now + 75000;
                ChatSendServerMessage("$999» $i$fff" + RandomHelloServer());
            }

            if (mapUID != ServerManager.Server.GetCurrentMapInfo().UId)
            {
                LoadMapUID();
                LoadRecord();
            }
            // ChatSendServerMessage(Now.ToString());
            foreach (ShootManiaXMLRPC.Structs.PlayerList Player in ServerManager.Server.GetPlayerList(100, 0))
            {
                // UpdateInterface(Player.Login, new object[] { 0 });
                if (Records.Count > 0)
                {
                    if (Records.ContainsKey(Player.Login))
                    {
                        string format = ToTime(Records[Player.Login].Time);
                        UpdateInterface(Player.Login, new object[] { format }, Records.Values.ToList(), Records.Values.ToList());
                    }
                    else
                    {
                        UpdateInterface(Player.Login, new object[] { "00:00.00" }, Records.Values.ToList(), Records.Values.ToList());
                    }
                } else if (Records.Count <= 0)
                {
                    {
                        UpdateInterface(Player.Login, new object[] { "00:00.00" }, new List<Management>(), new List<Management>());
                    }
                }
            }
            Now = Environment.TickCount;
        }

        private string ToTime(int time)
        {
            var milliseconds = time % 1000;
            var seconds = time / 1000;
            var minutes = seconds / 60;
            var hours = minutes / 60;
            minutes -= hours * 60;
            seconds -= ((hours * 60 * 60) + (minutes * 60));
            var format = "";
            if (minutes < 10) format += "0";
            format += minutes + ":";
            if (seconds < 10) format += "0";
            format += seconds + ".";
            format += milliseconds / 10;

            return format;
        }

        string mapUID;

        void LoadMapUID()
        {
            mapUID = ServerManager.Server.GetCurrentMapInfo().UId;
        }
        void LoadRecord()
        {
            // Load
            if (File.Exists("Plugins/misc/records_" + mapUID + ".json"))
            {
                string json = File.ReadAllText("Plugins/misc/records_" + mapUID + ".json");
                if (json != "") Records = JsonConvert.DeserializeObject<Dictionary<string, Management>>(json);
            }
            else
            {
                File.Create("Plugins/misc/records_" + mapUID + ".json");
                Records = new Dictionary<string, Management>();
            }
        }

        public override void HandleEventGbxCallback(object o, ShootManiaXMLRPC.XmlRpc.GbxCallbackEventArgs e)
        {
            if (o == null || e == null) return;
            var Name = e.Response.MethodName;
            Console.WriteLine(Name);
            if (Name == "ManiaPlanet.BeginMap" || Name == "ManiaPlanet.BeginRound")
            {
                LoadMapUID();
                LoadRecord();
            }
            if (Name == "TrackMania.PlayerFinish")
            {
                var Timez = int.Parse(e.Response.Params[2].ToString());
                if (Timez == 0) return;
                var ID = ServerManager.GetPlayer(e.Response.Params[1].ToString()).Login;
                if (!Records.ContainsKey(ID)) Records.Add(ID, new Management { Time = Timez + 5, Pseudo = ServerManager.GetPlayer(e.Response.Params[1].ToString()).Nickname, Login = ServerManager.GetPlayer(e.Response.Params[1].ToString()).Login });
                if (Records[ID].Time > Timez)
                {
                    Records[ID].Time = Timez;

                    var result = "$ff0Record! $999» ";
                    result += "$fff" + ServerManager.GetPlayer(e.Response.Params[1].ToString()).Nickname + "$z$s$> $fffset $0f0";
                    if (Timez == 0) return;
                    //var Seconds = Timez.Substring(Timez.Length - 5, 2);
                    result += Format(TimeSpan.FromMilliseconds(Timez));
                    result += " $fffand took the ";
                    result += "4th place!";
                    ChatSendServerMessage(result);

                    // Save to json
                    var stringJson = JsonMapper.ToJson(Records);
                    File.WriteAllText("Plugins/misc/records_" + mapUID + ".json", stringJson);

                    /*List<KeyValuePair<string, Management>> tempList = Records.ToList();
                    tempList.Sort();
                    Console.WriteLine("+1");
                    Records.Clear();
                    Console.WriteLine("+2");
                    foreach (var list in tempList)
                    {
                        Console.WriteLine("bug");
                        Records[list.Key] = list.Value;
                    }*/
                    Console.WriteLine("test");
                    Console.WriteLine(Records.Count);
                    Console.WriteLine(Records[ID].Login + " " + Records[ID].Time);
                    //UpdateInterface();
                }
                else ChatSendServerMessage("$fffProgress $999» $fff" + ServerManager.GetPlayer(e.Response.Params[1].ToString()).Nickname + "$z$s$> best :" + Records[ID].Time + " current :" + Timez);
            }
        }

        public int Now;
        public int ChangeTimeMessage;

		private void RefreshAdmins ()
		{
            Console.WriteLine(" Load admins ...");

			Admins.Clear();

			string currentAssemblyDirectoryName = Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location);

			if (File.Exists (currentAssemblyDirectoryName + "/" + AdminsCfgFile)) {

				StreamReader sr = new StreamReader(currentAssemblyDirectoryName + "/" + AdminsCfgFile);

				string line = sr.ReadLine();

				while (line != null)
				{

					line = line.Trim();

					if (line != string.Empty &&
					    !line.StartsWith("#") &&
					    !Admins.Contains(line))
					{
						Console.WriteLine("[IGAdmin] Admin found : " + line);
						Admins.Add(line);
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

			foreach (string admin in Admins)
				sw.WriteLine(admin);

			sw.Close();

		}

        

		public void ChatSendServerMessage(String Message)
		{
			ServerManager.Server.ChatSendServerMessage(Message);
		}

	}
}

