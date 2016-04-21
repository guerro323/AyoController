using System;
using System.Text;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using AyoController;
using AyoController.Classes;
using AyoController.Plugins;
using Newtonsoft.Json;
using ShootManiaXMLRPC;
using ShootManiaXMLRPC.XmlRpc;

namespace AyOPlugins.DediManiaPl
{
    public partial class DediMania : Plugin
    {
        /// <remarks>
        /// Created by 56ka;
        /// http://stackoverflow.com/questions/1892492/set-custom-path-to-referenced-dlls
        /// </remarks>
        /// <summary>
        /// Here is the list of authorized assemblies (DLL files)
        /// You HAVE TO specify each of them and call InitializeAssembly()
        /// </summary>
        private static string[] LOAD_ASSEMBLIES = { "Newtonsoft.Json.dll" };

        /// <summary>
        /// Call this method at the beginning of the program
        /// </summary>
        public static void initializeAssembly()
        {
            AppDomain.CurrentDomain.AssemblyResolve += delegate (object sender, ResolveEventArgs args)
            {
                string assemblyFile = (args.Name.Contains(','))
                    ? args.Name.Substring(0, args.Name.IndexOf(','))
                    : args.Name;

                assemblyFile += ".dll";

                // Forbid non handled dll's
                if (!LOAD_ASSEMBLIES.Contains(assemblyFile))
                {
                    return null;
                }

                string absoluteFolder = new FileInfo((new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath).Directory.FullName;
                string targetPath = Path.Combine(absoluteFolder, assemblyFile);

                try
                {
                    return Assembly.LoadFile(targetPath);
                }
                catch (Exception)
                {
                    return null;
                }
            };
        }

        private AyoController.Classes.ServerManager ServerManager { get; set; }
        public Dictionary<string, Management> LocalRecords = new Dictionary<string, Management>();
        public Dictionary<string, Management> DedimaniaRecords = new Dictionary<string, Management>();

        string _mapUid;

        [AyO.Settings("Use API Feature", "WARNING! Will disable some features like the interface", false)] public bool
            UseApiFeature = false;

        [AyO.Settings("Send messages to other servers.",
            "Enabling this option will permit to this plugin to send messages to the others servers", true)] public bool
            SendToOther = true;

        [AyO.Settings("Top 'N' to be send in other servers",
            "If a player do a record and if his rank is below 'N', this record will be shown in others servers (if the option is enabled)\nIf 'N' <param> is below than 0, anyway, the message will be send"
            )] public Dictionary<string, int> RankToBeShown = new Dictionary<string, int>()
            {
                {"DediMania", 5},
                {"Local", 5}
            };

        [AyO.Settings("Use AyoNext Feature", "Use AyONext Interface Pack", true)] public bool UseAyONext = false;

        [AyO.Settings("Maximum of records on the interface", "Maximum of Records to be shown on the interface", true)] public int RecordsMax = 20;

        [AyO.Settings("Maximum of the records", "Maximum of the records that can be saved on the server ( per map )",
            true)] public int RecordsMaxServer = 350;

        [AyO.Settings("On/Off DediMania", "Activate the DediMania's records", true)] public bool UseDediMania = true;

        [AyO.Settings("DediMania Key", "The current DediKey of this server (login)", true)] public string DediKey = "-1";

        public override AyO.PluginFunction[] PluginFunction
        {
            get
            {
                return new AyO.PluginFunction[]
                {
                    AyO.PluginFunction.RecordsLocal,
                    AyO.PluginFunction.RecordsOnline
                };
            }
        }

        public override string Version
        {
            get { return "0.1"; }
        }

        public override string Name
        {
            get { return "DediMania"; }
        }

        public override string Author
        {
            get { return "Guerro"; }
        }

        public override AyO.HelpCommands ListofCommands
        {
            get
            {
                return new AyO.HelpCommands()
                {
                    Path = "/dedi",
                    Commands = new[]
                    {
                        new AyO._Commands {Command = "showGUI"},
                        new AyO._Commands {Command = "top", Params = new []{ "Local", "25" }}
                    }
                };
            }
        }

        public override void OnLoad()
        {
            initializeAssembly();
            ListofCommands.Commands[1].Params[0] = RankToBeShown.ToString();
        }

        public static string ToTime(int time)
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
            format += milliseconds;

            return format;
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
            OnUpdateInterface(pc.Login);
        }

        public override void HandleOnPlayerChat(ShootManiaXMLRPC.Structs.PlayerChat pc)
        {
            if (pc.Login == null) return;
            OnUpdateInterface(pc.Login);
        }

        public class Management
        {
            public int Time;
            public int Id;
            public string Login;
            public string Pseudo;
            public int Rank;
            public string MapuId;
            public Dictionary<string, int> CheckpointRecords = new Dictionary<string, int>();
            public Dictionary<string, int> TempCheckpoints = new Dictionary<string, int>();
        }

        public override void Nothing()
        {

        }

        public override void OnServerManagerInitialize(ServerManager serverManager)
        {
            this.ServerManager = serverManager;
            this.ServerManager.OnConnectionSuccessful += HandleOnConnectionSuccessful;
            ServerManager.CreateNewFile(Name, "config.txt", "", new Action(Nothing));
            LocalRecords = new Dictionary<string, Management>();
        }

        public override void OnLoop()
        {
            if (ServerManager.RefreshTime < ServerManager.Now)
            {
                foreach (var server in ServerManager.Servers.Where(server => server != null && server.IsConnected))
                if (_mapUid != server.GetCurrentMapInfo().UId)
                {
                    LoadMapUid();
                    LoadRecord();
                    OnUpdateInterface("");
                }
                OnUpdateInterface("");
            }
        }

        public override void OnEverythingLoaded()
        {
            base.OnEverythingLoaded();
            LoadMapUid();
            LoadRecord();
        }

        void OnUpdateInterface(string login)
        {
            if (UseApiFeature)
            {
                foreach (var server in ServerManager.Servers)
                {
                    foreach (var player in ServerManager.GetPlayers())
                    {
                        ServerManager.RemoveThisManialink(player.Login,
                        "DediMania",
                        server)
                        ;
                    }
                }
            }
            if (!UseApiFeature)
            {
                if (!UseAyONext)
                {
                    var streamrod = ServerManager.ReadText(Name, 0, "Widget");
                    var toReplaceLocalRecords = "";

                    List<Management> lRecords = LocalRecords.Values.ToList();
                    List<Management> sortedLocalRecords = lRecords.OrderBy(ao => ao.Time).ToList();

                    //first, sort the rank
                    foreach (var record in sortedLocalRecords)
                    {
                        record.Rank = sortedLocalRecords.IndexOf(record) + 1;
                    }

                    var rank = 0;
                    var ranko = 0;
                    foreach (var record in sortedLocalRecords)
                    {
                        if (record.Time < 2) continue;
                        var streamrecord = ServerManager.ReadText(Name, 0, "Widget_RecordBar");
                        streamrecord = streamrecord.Replace("->NAME", record.Pseudo);
                        streamrecord = streamrecord.Replace("->00:00.00", ToTime(record.Time));
                        streamrecord = streamrecord.Replace("->1", record.Rank.ToString());
                        streamrecord = streamrecord.Replace("->POS.Y", rank.ToString());
                        streamrecord = streamrecord.Replace("->POS.Z", (30 - ranko).ToString());
                        toReplaceLocalRecords += streamrecord;
                        rank -= 7;
                        ranko += 1;
                    }
                    streamrod = streamrod.Replace("<!-- Record_Local Frame -->",
                        toReplaceLocalRecords + "<!-- Record_Local Frame -->");
                    foreach (var server in ServerManager.Servers.Where(server => server.IsConnected && AyO.CompressEnviroName(server.GetCurrentMapInfo().Environnement) == "tm"))
                        ServerManager.AddThisManialink(streamrod, "DediMania", true, server);
                }
                else
                {
                    /*var rankToShow = "";
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
                    Records[record.Login] = record;
                    Records[record.Login].Rank = rank + 1;
                /*frameLrecord += @"<label id=""localrecord1_" + rank + @""" posn=""-157 " + ((-rank * 5) + 65) + @" 0"" sizen=""10 10"" textsize=""3"" text=""#" + record.Rank + @""" />";
                frameLrecord += @"<label id=""localrecord2_" + rank + @""" posn=""-150 " + ((-rank * 5) + 65) + @" 0"" sizen=""9 10"" textsize=""3"" text=""" + ToTime(record.Time) + @""" />";
                frameLrecord += @"<label id=""localrecord3_" + rank + @""" posn=""-138.5 " + ((-rank * 5) + 65) + @" 0"" sizen=""22 10"" textsize=""3"" text=""" + record.Pseudo + @""" />";
                if ((rank % 2) == 0) color = "FFFA";
                else color = "FFFFFF00";
                if (rank< 8)
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
                if (maxindex< 7) maxindex++;
            }

            ServerManager.CreateNewFile(Name, "xml.xml", streamWidget, Nothing);

            ServerManager.AddThisManialink(playerName, streamWidget, "Records", true);*/
                }
            }
        }

        void LoadMapUid()
        {
            _mapUid = ServerManager.GetCurrentMapInfo.UId;

        }

        void DoNothing()
        {
        }

        void LoadRecord()
        {

            // Load
            if (File.Exists("Plugins/misc/" + Name + "/records_" + _mapUid + ".misc"))
            {
                string json = ServerManager.ReadText(Name, 1, "/records_" + _mapUid);
                if (json != "") LocalRecords = JsonConvert.DeserializeObject<Dictionary<string, Management>>(json);
            }
            else
            {
                ServerManager.CreateNewFile(Name, "/records_" + _mapUid + ".misc", "", DoNothing);
                LocalRecords = new Dictionary<string, Management>();
            }
        }

        /* void () {
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
                Records[record.Login] = record;
                Records[record.Login].Rank = rank + 1;*/
        /*frameLrecord += @"<label id=""localrecord1_" + rank + @""" posn=""-157 " + ((-rank * 5) + 65) + @" 0"" sizen=""10 10"" textsize=""3"" text=""#" + record.Rank + @""" />";
                frameLrecord += @"<label id=""localrecord2_" + rank + @""" posn=""-150 " + ((-rank * 5) + 65) + @" 0"" sizen=""9 10"" textsize=""3"" text=""" + ToTime(record.Time) + @""" />";
                frameLrecord += @"<label id=""localrecord3_" + rank + @""" posn=""-138.5 " + ((-rank * 5) + 65) + @" 0"" sizen=""22 10"" textsize=""3"" text=""" + record.Pseudo + @""" />";
                if ((rank % 2) == 0) color = "FFFA";
                else color = "FFFFFF00";
                if (rank< 8)
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
                if (maxindex< 7) maxindex++;
            }

            ServerManager.CreateNewFile(Name, "xml.xml", streamWidget, Nothing);

            ServerManager.AddThisManialink(playerName, streamWidget, "Records", true);
    */

        public override void HandleFixedGbxCallBacks(GbxCallbackEventArgs _response, string gName, ShootManiaServer maniaServer)
        {
            var e = (GbxCallbackEventArgs) _response;
            if (e == null) return;
            if (gName == null)
                return;
            if (gName == "")
                return;
            if (gName == "ManiaPlanet.PlayerManialinkPageAnswer")
            {
                ServerManager.Server.ChatEnableManualRouting();
                string response = e.Response.Params[2].ToString();

                if (response.Contains("Message["))
                {
                    response = response.Replace("Message[", "");
                    response = response.Replace("]", "");
                    ServerManager.Server.SendAsLogin(e.Response.Params[1].ToString(), response);
                }
            }
            if (gName == "TrackMania.PlayerCheckpoint")
            {
                var timez = int.Parse(e.Response.Params[2].ToString());
                if (timez == 0) return;
                var id = ServerManager.GetPlayer(e.Response.Params[1].ToString()).Login;
                if (!LocalRecords.ContainsKey(id))
                    LocalRecords.Add(id,
                        new Management
                        {
                            Pseudo = ServerManager.GetPlayer(e.Response.Params[1].ToString()).Nickname,
                            Login = ServerManager.GetPlayer(e.Response.Params[1].ToString()).Login,
                            MapuId = maniaServer.GetCurrentMapInfo().UId
                        });
                if (!LocalRecords[id].TempCheckpoints.ContainsKey(e.Response.Params[4].ToString()))
                    LocalRecords[id].TempCheckpoints.Add(e.Response.Params[4].ToString(), timez + 5);
                else LocalRecords[id].TempCheckpoints[e.Response.Params[4].ToString()] = timez + 5;

                if (LocalRecords[id].CheckpointRecords.ContainsKey(e.Response.Params[4].ToString()))
                {
                    if (LocalRecords[id].TempCheckpoints[e.Response.Params[4].ToString()] <
                        LocalRecords[id].CheckpointRecords[e.Response.Params[4].ToString()])
                    {
                        //ServerManager.Relay.SetVar("Net_Checkpoint", new object[] { "test" });
                    }
                    else ServerManager.Server.ChatSend("$f00--");
                }
                //else ServerManager.Server.ChatSendToLogin(ID, "$fffProgress $999» $fff" + ServerManager.GetPlayer(e.Response.Params[1].ToString()).Nickname + "$z$s$> best :" + ToTime(LocalRecords[ID].CheckpointRecords[e.Response.Params[4].ToString()]) + " current :" + ToTime(Timez));
            }

            if (gName == "TrackMania.PlayerFinish")
            {
                var timez = int.Parse(e.Response.Params[2].ToString());
                if (timez == 0) return;
                Console.WriteLine("-1");
                var id = ServerManager.GetPlayer(e.Response.Params[1].ToString()).Login;
                Console.WriteLine("0");
                if (!LocalRecords.ContainsKey(id))
                    LocalRecords.Add(id,
                        new Management
                        {
                            Rank = 0,
                            Time = timez + 5,
                            Pseudo = ServerManager.GetPlayer(e.Response.Params[1].ToString()).Nickname,
                            Login = ServerManager.GetPlayer(e.Response.Params[1].ToString()).Login,
                            MapuId = maniaServer.GetCurrentMapInfo().UId
                        });
                else if (LocalRecords[id].Time < 1)
                    LocalRecords[id].Time = timez + 5;
                Console.WriteLine("1");
                maniaServer.ChatSendServerMessage(LocalRecords[id].Time + "  " + timez);
                if (LocalRecords[id].Time > timez)
                {
                    Console.WriteLine("2");
                    LocalRecords[id].Time = timez;

                    var result = "$ff0Record! $999» ";
                    var resultToOther = "$06cRecord ・$fc3$<" +
                                        ServerManager.GetPlayer(e.Response.Params[1].ToString()).Server.GetServerName() +
                                        "$>$06c・$z$s$fc3$<" +
                                        ServerManager.GetPlayer(e.Response.Params[1].ToString()).Nickname + "$> $06c";
                    result += "$fff" + ServerManager.GetPlayer(e.Response.Params[1].ToString()).Nickname +
                              "$z$s$> $fffset $0f0";
                    Console.WriteLine("3");
                    if (timez == 0) return;
                    result += Format(TimeSpan.FromMilliseconds(timez));
                    resultToOther += "set " + Format(TimeSpan.FromMilliseconds(timez)) +
                                     " $fff| $06cRank : ";
                    result += " $fffand took the ";
                    int rank = 0;
                    Console.WriteLine("4");
                    // Clean the non records
                    List<Management> lRecords = LocalRecords.Values.ToList();
                    List<Management> sortedList = lRecords.OrderBy(ao => ao.Time).ToList();
                    foreach (var record in sortedList)
                    {
                        if (record == null) continue;
                        LocalRecords[record.Login] = record;
                        LocalRecords[record.Login].Rank = rank + 1;
                        if (LocalRecords[id].TempCheckpoints.Count > 0)
                            LocalRecords[id].CheckpointRecords = LocalRecords[id].TempCheckpoints;
                        rank++;
                    }
                    Console.WriteLine("5");
                    result += LocalRecords[id].Rank + " place!";
                    resultToOther += LocalRecords[id].Rank + " on " +
                                     ServerManager.GetPlayer(e.Response.Params[1].ToString())
                                         .Server.GetCurrentMapInfo()
                                         .Name;
                    Console.WriteLine("6");
                    ServerManager.CreateNewFile(Name, "/records_" + _mapUid + ".misc",
                        JsonHelper.FormatJson(JsonConvert.SerializeObject(LocalRecords)), DoNothing);
                    Console.WriteLine("7");
                    foreach (var server in ServerManager.Servers.Where(server => server.IsConnected))
                    {
                        if (server.GetLogin() ==
                            ServerManager.GetPlayer(e.Response.Params[1].ToString()).Server.GetLogin())
                        {
                            server.ChatSendServerMessage(result);
                        }
                        if (SendToOther && server.GetLogin() !=
                            ServerManager.GetPlayer(e.Response.Params[1].ToString()).Server.GetLogin())
                        {
                            server.ChatSendServerMessage(resultToOther);
                        }
                    }
                    Console.WriteLine("8");
                }
                else
                    ServerManager.Server.ChatSendToLogin(id,
                        "$fffProgress $999» $fff" + ServerManager.GetPlayer(e.Response.Params[1].ToString()).Nickname +
                        "$z$s$> best :" + ToTime(LocalRecords[id].Time) + " current :" + ToTime(timez));
            }
        }

        public override void OnConsoleCommand(string command)
        {
            if (command == "s")
            {
                ServerManager.Server.ChatSendServerMessage("ssss");
            }
        }

        public override void Unload()
        {
            ServerManager.Server.OnPlayerChat -= HandleOnPlayerChat;
            ServerManager.Server.OnPlayerConnect -= HandleOnPlayerConnect;
            ServerManager.OnConnectionSuccessful -= HandleOnConnectionSuccessful;
        }
    }
}

