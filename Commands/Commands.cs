using System;
using System.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using AyoController.Classes;
using AyoController.Plugins;
using AyoController;
using Newtonsoft.Json;
using LitJson;

namespace Commands
{
	public partial class Commands : Plugin
	{

		private AyoController.Classes.ServerManager ServerManager { get; set; }
		private const string AdminsCfgFile = "IGAdmin__Admins.cfg";
		private List<string> Admins = new List<string>();

        public override AyO.PluginFunction PluginFunction
        {
            get
            {
                return AyO.PluginFunction.Global;
            }
        }

		public override string Name {
			get {
				return "Commands";
			}
		}

		public override string Author {
			get {
				return "Guerro";
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
                return new string[] { "/help", "/hi", "/hia" };
            }
        }

        public override void OnLoad ()
		{
			RefreshAdmins();
            
        }

        public void Nothing() { }

		public override void OnServerManagerInitialize (AyoController.Classes.ServerManager ServerManager)
		{

			this.ServerManager = ServerManager;
			this.ServerManager.OnConnectionSuccessful += HandleOnConnectionSuccessful;

            ServerManager.CreateNewFile(Name, "config.txt", "", new Action(Nothing));
        }

        public long StartChat;

		public override void OnConsoleCommand (string Command)
		{

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

            /*foreach (var PC in ServerManager.GetPlayers()) {
                if (Records.Count == 0) UpdateInterface(PC.Login, new object[] { "00:00.00" }, new List<Management>(), new List<Management>());
                else if (!Records.ContainsKey(PC.Login)) UpdateInterface(PC.Login, new object[] { "00:00.00" }, Records.Values.ToList(), Records.Values.ToList());
                else UpdateInterface(PC.Login, new object[] { ToTime(Records[PC.Login].Time), Records[PC.Login].Rank }, Records.Values.ToList(), Records.Values.ToList());
            }*/
        }

		void HandleOnPlayerConnect (ShootManiaXMLRPC.Structs.PlayerConnect PC)
		{
            /*if (Admins.Contains (PC.Login)) {
				ChatSendServerMessage("Admin connected : " + PC.Login);
			}*/

            /*if (Records.Count == 0 ) UpdateInterface(PC.Login, new object[] { "00:00.00" }, new List<Management>(), new List<Management>());
            else if (!Records.ContainsKey(PC.Login)) UpdateInterface(PC.Login, new object[] { "00:00.00" }, Records.Values.ToList(), Records.Values.ToList());
            else UpdateInterface(PC.Login, new object[] { ToTime(Records[PC.Login].Time), Records[PC.Login].Rank }, Records.Values.ToList(), Records.Values.ToList());*/
        }

        class CHATCLASS
        {
            public string Login;
            public string Text;
            public int thisNow;
        }

        List<CHATCLASS> ChatList = new List<CHATCLASS>();

        void HandleOnPlayerChat (ShootManiaXMLRPC.Structs.PlayerChat PC)
		{
            if (PC.Login == null) return;
            if (PC.Login != ServerManager.Server.GetLogin()) ChatList.Add(new CHATCLASS { Login = PC.Login, Text = PC.Text, thisNow = Now });
            else ChatList.Add(new CHATCLASS { Login = PC.Login, Text = PC.Text, thisNow = Now });
           /* if (Records.Count == 0) UpdateInterface(PC.Login, new object[] { "00:00.00" }, new List<Management>(), new List<Management>());
            else if (!Records.ContainsKey(PC.Login)) UpdateInterface(PC.Login, new object[] { "00:00.00" }, Records.Values.ToList(), Records.Values.ToList());
            else UpdateInterface(PC.Login, new object[] { ToTime(Records[PC.Login].Time), Records[PC.Login].Rank }, Records.Values.ToList(), Records.Values.ToList());*/
            ParseChatCommand(PC);
            Console.WriteLine(ServerManager.Admins.GetGroupForLogin(PC.Login));
            Console.WriteLine("test2");
            ServerManager.Server.SetNoUI(@"<ui_properties>
  <!-- The map name and author displayed in the top right of the screen when viewing the scores table -->
  <map_info visible=""true"" />
  <!-- Only visible in solo modes, it hides the medal/ghost selection UI -->
  <opponents_info visible=""true"" />
  <!-- 
    The server chat displayed on the bottom right of the screen 
    The offset values range from 0. to -3.2 for x and from 0. to 1.8 for y
    The linecount property must be between 0 and 40
  -->
  <chat visible=""true"" offset=""0. 0."" linecount=""7"" />
  <!-- Time of the players at the current checkpoint displayed at the bottom of the screen -->
  <checkpoint_list visible=""true"" pos=""40. -90. 5."" />
  <!-- Small scores table displayed at the end of race of the round based modes (Rounds, Cup, ...) on the right of the screen -->
  <round_scores visible=""false"" pos=""104. 14. 5."" />
  <!-- Race time left displayed at the bottom right of the screen -->
  <countdown visible=""false"" pos=""154. -57. 5."" />
  <!-- 3, 2, 1, Go! message displayed on the middle of the screen when spawning -->
  <go visible=""true"" />
  <!-- Current race chrono displayed at the bottom center of the screen -->
  <chrono visible=""false"" pos=""0. -80. 5."" />
  <!-- Speed and distance raced displayed in the bottom right of the screen -->
  <speed_and_distance visible=""false"" pos=""158. -79.5 5."" />
  <!-- Previous and best times displayed at the bottom right of the screen -->
  <personal_best_and_rank visible=""true"" pos=""158. -61. 5."" />
  <!-- Current position in the map ranking displayed at the bottom right of the screen -->
  <position visible=""false"" />
  <!-- Checkpoint time information displayed in the middle of the screen when crossing a checkpoint -->
  <checkpoint_time visible=""true"" pos=""-8. 31.8 -10."" />
  <!-- The avatar of the last player speaking in the chat displayed above the chat -->
  <chat_avatar visible=""true"" />
  <!-- Warm-up progression displayed on the right of the screen during warm-up -->
  <warmup visible=""true"" pos=""170. 27. 0."" />
  <!-- Ladder progression box displayed on the top of the screen at the end of the map -->
  <endmap_ladder_recap visible=""true"" />
  <!-- Laps count displayed on the right of the screen on multilaps map -->
  <multilap_info visible=""true"" pos=""152. 49.5 5."" />
</ui_properties>");
        }

        int refreshTime;

        private void UpdateInterface(string playerName, object[] Params, List<Management> LocalRecords, List<Management> DedimaniaRecords)
        {
            var rankToShow = "";
            if (Params.Count() == 1) rankToShow = "?";
            else rankToShow = Params[1].ToString();
            int rank = 0;
            var toreturn = "";
            var frameLrecord = "";
            List<Management> SortedList = LocalRecords.OrderBy(o => o.Time).ToList();
            var color = "";
            var I = 0F;
            foreach (var record in SortedList)
            {
                /*Records[record.Login] = record;
                Records[record.Login].Rank = rank + 1;*/
                /*frameLrecord += @"<label id=""localrecord1_" + rank + @""" posn=""-157 " + ((-rank * 5) + 65) + @" 0"" sizen=""10 10"" textsize=""3"" text=""#" + record.Rank + @""" />";
                frameLrecord += @"<label id=""localrecord2_" + rank + @""" posn=""-150 " + ((-rank * 5) + 65) + @" 0"" sizen=""9 10"" textsize=""3"" text=""" + ToTime(record.Time) + @""" />";
                frameLrecord += @"<label id=""localrecord3_" + rank + @""" posn=""-138.5 " + ((-rank * 5) + 65) + @" 0"" sizen=""22 10"" textsize=""3"" text=""" + record.Pseudo + @""" />";*/
                if ((rank % 2) == 0) color = "FFFA";
                else color = "FFFFFF00";
                if (rank < 8)
                {
                    frameLrecord += @"<frame posn=""-136.5 " + ((-I) + 67.25) + @" -6"" scale=""0.328"">
<quad id=""localrecord4_" + rank + @""" posn=""-70 10 -1"" sizen=""150 20"" bgcolor=""" + color + @"""/>
<label id=""localrecord1_" + rank + @""" posn=""45 0 0"" sizen=""70 20"" textprefix=""$s"" text=""" + record.Pseudo + @""" halign=""center"" valign=""center"" textsize=""6.5""/>
<label id=""localrecord2_" + rank + @""" posn=""-10 0 0"" sizen=""40 20"" text=""" + ToTime(record.Time) + @""" halign=""center"" valign=""center"" textsize=""8""/>
<quad id=""localrecord5_" + rank + @"""  posn=""-70 0 1"" sizen=""20 20"" bgcolor=""FFFA"" halign=""left"" valign=""center""/>
<label id=""localrecord3_" + rank + @""" posn=""-40 0 0"" sizen=""20 20"" text=""" + record.Rank + @""" style=""TextButtonBig"" valign=""center2"" halign=""center"" textsize=""15""/></frame>";
                }
                I += 6.9F;
                rank++;
            }
            string streamWidget = ServerManager.ReadText(Name, 0, "I_Records");
            
            streamWidget = streamWidget.Replace("[.besttimelabel.]", Params[0].ToString());
            streamWidget = streamWidget.Replace("[.recordrank.]", rankToShow);
            streamWidget = streamWidget.Replace("<!-- LOCALRECORD -->", frameLrecord);
            streamWidget = streamWidget.Replace("->RANK", rank + 2.ToString());
            streamWidget = streamWidget.Replace("->CHATS", ChatList.Count.ToString());

            List<CHATCLASS> ChatSortedList = ChatList.OrderBy(ao => -ao.thisNow).ToList();



            var hey = 0;
            var maxindex = 0;
            foreach (var chat in ChatSortedList)
            {
                if (maxindex > 6) continue;
                if (chat.Login != "guerro323") streamWidget = streamWidget.Replace("<!--CHATREPLACE-->", "<label id='chatlabel_" + maxindex + "' posn='0 " + hey + "' sizen='90 10' halign='left' valign='center' textsize='2' text='$s[" + ServerManager.GetPlayer(chat.Login).Nickname + "$z$s$>]" + chat.Text + "' /><!--CHATREPLACE-->");
                else streamWidget = streamWidget.Replace("<!--CHATREPLACE-->", "<label id='chatlabel_" + maxindex + "' posn='0 " + hey + "' sizen='90 10' halign='left' valign='center' textsize='2' text='$s" + chat.Text + "' /><!--CHATREPLACE-->");
                hey += 3;
                if (maxindex < 7) maxindex++;
            }

            ServerManager.CreateNewFile(Name, "xml.xml", streamWidget, Nothing);

            ServerManager.AddThisManialink(playerName, streamWidget, "Records", true);
        }

        class Management
        {
            public int Time;
            public int Id;
            public string Login;
            public string Pseudo;
            public int Rank;
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
            if (refreshTime < Now)
            {
                refreshTime = Now + 1000;
                if (ServerManager.Server.GetCurrentMapInfo() != null && mapUID != ServerManager.Server.GetCurrentMapInfo().UId)
                {
                    LoadMapUID();
                }
            }
            // ChatSendServerMessage(Now.ToString());
            /*foreach (ShootManiaXMLRPC.Structs.PlayerList Player in ServerManager.GetPlayers())
            {
                // UpdateInterface(Player.Login, new object[] { 0 });
                if (Records.Count > 0)
                {
                    if (Records.ContainsKey(Player.Login))
                    {

                    }
                    else
                    {
                       // UpdateInterface(Player.Login, new object[] { "00:00.00" }, Records.Values.ToList(), Records.Values.ToList());
                    }
                } else if (Records.Count <= 0)
                {
                    {
                      //  UpdateInterface(Player.Login, new object[] { "00:00.00" }, new List<Management>(), new List<Management>());
                    }
                }
            }*/
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
            if (milliseconds < 100) format += "0";
            format += milliseconds / 10;

            return format;
        }

        string mapUID;

        void LoadMapUID()
        {
            mapUID = ServerManager.Server.GetCurrentMapInfo().UId;

        }

        public override void HandleEventGbxCallback(object o, ShootManiaXMLRPC.XmlRpc.GbxCallbackEventArgs e)
        {
            if (e == null) return;
            var Name = e.Response.MethodName;
            if (Name == "ManiaPlanet.PlayerManialinkPageAnswer")
            {
                ServerManager.Server.ChatEnableManualRouting();
                string response = e.Response.Params[2].ToString();
               
                if (response.StartsWith("Message:")) {
                    response = response.Replace("Message:", "");
                    ServerManager.Server.SendAsLogin(e.Response.Params[1].ToString(), response);
                }
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

