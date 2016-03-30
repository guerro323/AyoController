using System;
using System.Text;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using AyoController;
using AyoController.Classes;
using AyoController.Plugins;
using LitJson;


namespace Dedimania
{
    public partial class DediMania : Plugin
    {
        private AyoController.Classes.ServerManager ServerManager { get; set; }
        private Dictionary<string, Management> Records = new Dictionary<string, Management>();

        public override AyO.PluginFunction PluginFunction
        {
            get
            {
                return AyO.PluginFunction.MapList;
            }
        }
        public override string Version
        {
            get
            {
                return "0.1";
            }
        }
        public override string Name
        {
            get
            {
                return "DediMania Plugin";
            }
        }
        public override string Author
        {
            get
            {
                return "Guerro (plugin)";
            }
        }
        public override string[] listofCommands
        {
            get
            {
                return new string[] { "/dedi" };
            }
        }
        public override void OnLoad()
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

        void HandleOnConnectionSuccessful()
        {
            this.ServerManager.Server.OnPlayerChat += HandleOnPlayerChat;
            this.ServerManager.Server.OnPlayerConnect += HandleOnPlayerConnect;
        }

        void HandleOnPlayerConnect(ShootManiaXMLRPC.Structs.PlayerConnect PC)
        {

        }

        void HandleOnPlayerChat(ShootManiaXMLRPC.Structs.PlayerChat PC)
        {
            if (PC.Login == null) return;
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

        class Management
        {
            public int Time;
            public int Id;
            public string Login;
            public string Pseudo;
            public int Rank;
        }

        void Nothing()
        {

        }

        public override void OnServerManagerInitialize(ServerManager ServerManager)
        {
            this.ServerManager = ServerManager;
            this.ServerManager.OnConnectionSuccessful += HandleOnConnectionSuccessful;

            ServerManager.CreateNewFile(Name, "config.txt", "", new Action(Nothing));
            Records = new Dictionary<string, Management>();

            Console.WriteLine(Records.Count);
            foreach (var Record in Records)
            {
                Console.WriteLine(Record.Value.Login);
            }
        }
        public override void OnLoop()
        {
            if (ServerManager.RefreshTime < ServerManager.Now)
            {
                if (mapUID != ServerManager.Server.GetCurrentMapInfo().UId)
                {
                    LoadMapUID();
                    LoadRecord();
                    
                }
                OnUpdateInterface();
            }
        }

        void OnUpdateInterface()
        {
            ManialinkSystem DedimaniaManialink = new ManialinkSystem();
            Frame MainFrame = new Frame();
            Quad backGroundQuad = new Quad("", new Vector3(2, 4, 0), new Vector2(40, 70), false);
            MainFrame.position = new Vector3(6, -28, 0);
            backGroundQuad.Halign = ManialinkSystem.Halign.Center;
            backGroundQuad.Valign = ManialinkSystem.Valign.Center;
            backGroundQuad.Style("Bgs1", "BgDialogBlur");
            childOf bgQuadToMainFrame = new childOf(backGroundQuad, MainFrame);
            backGroundQuad.AutoBuild();
            DedimaniaManialink.Add(MainFrame, true);
            DedimaniaManialink.Add(backGroundQuad, true);
            Console.WriteLine(DedimaniaManialink.CurrentBuild);
            ServerManager.AddThisManialink("guerro", DedimaniaManialink.CurrentBuild, "DediManialink", true);
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
        void DoNothing() { }
        void LoadRecord()
        {
            // Load
            if (File.Exists("Plugins/misc/" + Name + "/records_" + mapUID + ".misc"))
            {
                string json = ServerManager.ReadText(Name, 1, "/records_" + mapUID);
                if (json != "") Records = JsonMapper.ToObject<Dictionary<string, Management>>(json);
            }
            else
            {
                ServerManager.CreateNewFile(Name, "/records_" + mapUID + ".misc", "", DoNothing);
                Records = new Dictionary<string, Management>();
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

        public override void HandleEventGbxCallback(object o, ShootManiaXMLRPC.XmlRpc.GbxCallbackEventArgs e)
        {
            if (e == null) return;
            var Name = e.Response.MethodName;
            if (Name == "ManiaPlanet.PlayerManialinkPageAnswer")
            {
                ServerManager.Server.ChatEnableManualRouting();
                string response = e.Response.Params[2].ToString();

                if (response.Contains("Message["))
                {
                    response = response.Replace("Message[", "");
                    response = response.Replace("]", "");
                    ServerManager.Server.SendAsLogin(e.Response.Params[1].ToString(), response);
                    Console.WriteLine("2");
                }
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
                    result += Format(TimeSpan.FromMilliseconds(Timez));
                    result += " $fffand took the ";
                    int rank = 0;
                    List<Management> lRecords = Records.Values.ToList();
                    List<Management> SortedList = lRecords.OrderBy(ao => ao.Time).ToList();
                    foreach (var record in SortedList)
                    {
                        if (record == null) continue;
                        Records[record.Login] = record;
                        Records[record.Login].Rank = rank + 1;
                        rank++;
                    }
                    result += Records[ID].Rank + " place!";
                    ServerManager.CreateNewFile(this.Name, "/records_" + mapUID + ".misc", JsonMapper.ToJson(Records), DoNothing);
                    ServerManager.Server.ChatSendServerMessage(result);
                }
                else ServerManager.Server.ChatSendToLogin(ID, "$fffProgress $999» $fff" + ServerManager.GetPlayer(e.Response.Params[1].ToString()).Nickname + "$z$s$> best :" + ToTime(Records[ID].Time) + " current :" + ToTime(Timez));
            }
        }
        public override void OnConsoleCommand(string Command)
        {
            if (Command.StartsWith("s"))
            {
                ServerManager.Server.ChatSendServerMessage(Command);
            }
        }
    }
}

