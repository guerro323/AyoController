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
using ShootManiaXMLRPC;
using ShootManiaXMLRPC.XmlRpc;

namespace Commands
{
    public partial class Commands : Plugin
    {
        public override AyoController.Classes.ServerManager ServerManager { get; set; }
        private const string AdminsCfgFile = "IGAdmin__Admins.cfg";
        private readonly List<string> _admins = new List<string>();

        public override AyO.PluginFunction[] PluginFunction
        {
            get
            {
                return new AyO.PluginFunction[]
                {
                    AyO.PluginFunction.Global
                };
            }
        }

        public override string Name
        {
            get { return "Commands"; }
        }

        public override string Author
        {
            get { return "Guerro"; }
        }

        public string StructAd
        {
            get { return "$z$s$fff[$f70ADMIN$z$s$fff]$z$s"; }
        }

        public override string Version
        {
            get { return "0.1"; }
        }

        public override AyO.HelpCommands ListofCommands
        {
            get
            {
                return new AyO.HelpCommands()
                {
                    Path = "",
                    Commands = new[]
                    {
                        new AyO._Commands {Command = "/help"},
                        new AyO._Commands {Command = "/hi"},
                        new AyO._Commands {Command = "hia"}
                    }
                };
            }
        }

        public override void OnLoad()
        {
            RefreshAdmins();
        }

        public override void Nothing()
        {
        }

        public override void OnServerManagerInitialize(AyoController.Classes.ServerManager serverManager)
        {

            this.ServerManager = serverManager;
            this.ServerManager.OnConnectionSuccessful += HandleOnConnectionSuccessful;

            ServerManager.CreateNewFile(Name, "config.txt", "", new Action(Nothing));
        }

        public long StartChat;

        public override void OnConsoleCommand(string command)
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

        public override void HandleOnConnectionSuccessful()
        {
            ServerManager.Server.OnPlayerChat += HandleOnPlayerChat;
            ServerManager.Server.OnPlayerConnect += HandleOnPlayerConnect;
        }

        public override void HandleOnPlayerConnect(ShootManiaXMLRPC.Structs.PlayerConnect pc)
        {

        }

        class Chatclass
        {
            public string Login;
            public string Text;
            public int ThisNow;
        }

        readonly List<Chatclass> _chatList = new List<Chatclass>();

        public override void HandleOnPlayerChat(ShootManiaXMLRPC.Structs.PlayerChat pc)
        {
            if (pc.Login == null) return;
            if (pc.Login != ServerManager.Server.GetLogin())
                _chatList.Add(new Chatclass {Login = pc.Login, Text = pc.Text, ThisNow = Now});
            else _chatList.Add(new Chatclass {Login = pc.Login, Text = pc.Text, ThisNow = Now});
            ParseChatCommand(pc);
        }

        int _refreshTime;

        private void UpdateInterface(string playerName, object[] Params, List<Management> localRecords,
            List<Management> dedimaniaRecords)
        {
            var rankToShow = "";
            if (Params.Count() == 1) rankToShow = "?";
            else rankToShow = Params[1].ToString();
            int rank = 0;
            var frameLrecord = "";
            List<Management> sortedList = localRecords.OrderBy(o => o.Time).ToList();
            var I = 0F;
            foreach (var record in sortedList)
            {
                /*Records[record.Login] = record;
                Records[record.Login].Rank = rank + 1;*/
                /*frameLrecord += @"<label id=""localrecord1_" + rank + @""" posn=""-157 " + ((-rank * 5) + 65) + @" 0"" sizen=""10 10"" textsize=""3"" text=""#" + record.Rank + @""" />";
                frameLrecord += @"<label id=""localrecord2_" + rank + @""" posn=""-150 " + ((-rank * 5) + 65) + @" 0"" sizen=""9 10"" textsize=""3"" text=""" + ToTime(record.Time) + @""" />";
                frameLrecord += @"<label id=""localrecord3_" + rank + @""" posn=""-138.5 " + ((-rank * 5) + 65) + @" 0"" sizen=""22 10"" textsize=""3"" text=""" + record.Pseudo + @""" />";*/
                var color = "";
                if ((rank%2) == 0) color = "FFFA";
                else color = "FFFFFF00";
                if (rank < 8)
                {
                    frameLrecord += @"<frame posn=""-136.5 " + ((-I) + 67.25) + @" -6"" scale=""0.328"">
<quad id=""localrecord4_" + rank + @""" posn=""-70 10 -1"" sizen=""150 20"" bgcolor=""" + color + @"""/>
<label id=""localrecord1_" + rank + @""" posn=""45 0 0"" sizen=""70 20"" textprefix=""$s"" text=""" + record.Pseudo +
                                    @""" halign=""center"" valign=""center"" textsize=""6.5""/>
<label id=""localrecord2_" + rank + @""" posn=""-10 0 0"" sizen=""40 20"" text=""" + ToTime(record.Time) +
                                    @""" halign=""center"" valign=""center"" textsize=""8""/>
<quad id=""localrecord5_" + rank +
                                    @"""  posn=""-70 0 1"" sizen=""20 20"" bgcolor=""FFFA"" halign=""left"" valign=""center""/>
<label id=""localrecord3_" + rank + @""" posn=""-40 0 0"" sizen=""20 20"" text=""" + record.Rank +
                                    @""" style=""TextButtonBig"" valign=""center2"" halign=""center"" textsize=""15""/></frame>";
                }
                I += 6.9F;
                rank++;
            }
            string streamWidget = ServerManager.ReadText(Name, 0, "I_Records");

            streamWidget = streamWidget.Replace("[.besttimelabel.]", Params[0].ToString());
            streamWidget = streamWidget.Replace("[.recordrank.]", rankToShow);
            streamWidget = streamWidget.Replace("<!-- LOCALRECORD -->", frameLrecord);
            streamWidget = streamWidget.Replace("->RANK", rank + 2.ToString());
            streamWidget = streamWidget.Replace("->CHATS", _chatList.Count.ToString());

            List<Chatclass> chatSortedList = _chatList.OrderBy(ao => -ao.ThisNow).ToList();



            var hey = 0;
            var maxindex = 0;
            foreach (var chat in chatSortedList)
            {
                if (maxindex > 6) continue;
                if (chat.Login != "guerro323")
                    streamWidget = streamWidget.Replace("<!--CHATREPLACE-->",
                        "<label id='chatlabel_" + maxindex + "' posn='0 " + hey +
                        "' sizen='90 10' halign='left' valign='center' textsize='2' text='$s[" +
                        ServerManager.GetPlayer(chat.Login).Nickname + "$z$s$>]" + chat.Text + "' /><!--CHATREPLACE-->");
                else
                    streamWidget = streamWidget.Replace("<!--CHATREPLACE-->",
                        "<label id='chatlabel_" + maxindex + "' posn='0 " + hey +
                        "' sizen='90 10' halign='left' valign='center' textsize='2' text='$s" + chat.Text +
                        "' /><!--CHATREPLACE-->");
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


        public override void OnLoop()
        {
            if (ServerManager == null) return;
            if (ChangeTimeMessage < Now)
            {
                ChangeTimeMessage = Now + 75000;
                ChatSendServerMessage("$999» $i$fff" + RandomHelloServer());
            }
            if (_refreshTime < Now)
            {
                _refreshTime = Now + 1000;
            }
            Now = Environment.TickCount;
        }

        private static string ToTime(int time)
        {
            var milliseconds = time%1000;
            var seconds = time/1000;
            var minutes = seconds/60;
            var hours = minutes/60;
            minutes -= hours*60;
            seconds -= ((hours*60*60) + (minutes*60));
            var format = "";
            if (minutes < 10) format += "0";
            format += minutes + ":";
            if (seconds < 10) format += "0";
            format += seconds + ".";
            if (milliseconds < 100) format += "0";
            format += milliseconds/10;

            return format;
        }

        public override void HandleFixedGbxCallBacks(GbxCallbackEventArgs _response, string methodname, ShootManiaServer maniaServer)
        {
            var e = (GbxCallbackEventArgs) _response;
            if (e == null) return;
            if (methodname == "ManiaPlanet.PlayerManialinkPageAnswer")
            {
                ServerManager.Server.ChatEnableManualRouting();
                string response = e.Response.Params[2].ToString();

                if (response.StartsWith("Message:"))
                {
                    response = response.Replace("Message:", "");
                    ServerManager.Server.SendAsLogin(e.Response.Params[1].ToString(), response);
                }
            }
        }

        public int Now;
        public int ChangeTimeMessage;

        private void RefreshAdmins()
        {
            Console.WriteLine(" Load admins ...");

            _admins.Clear();

            string currentAssemblyDirectoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (File.Exists(currentAssemblyDirectoryName + "/" + AdminsCfgFile))
            {

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

            }
            else
            {
                Console.WriteLine("[IGAdmin] Unable to find : " + AdminsCfgFile);
            }
        }

        public void ChatSendServerMessage(string message)
        {
            foreach (var server in ServerManager.Servers.Where(server => server.IsConnected))
            server.ChatSendServerMessage(message);
        }

        public override void Unload()
        {
            ServerManager.Server.OnPlayerChat -= HandleOnPlayerChat;
            ServerManager.Server.OnPlayerConnect -= HandleOnPlayerConnect;
            ServerManager.OnConnectionSuccessful -= HandleOnConnectionSuccessful;
        }

    }
}

