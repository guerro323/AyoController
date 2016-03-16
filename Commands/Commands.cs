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
                return new string[] { "/help", "/hi", "/hia" };
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
                foreach (ShootManiaXMLRPC.Structs.PlayerList Player in ServerManager.Server.GetPlayerList(100, 0))
                    ServerManager.Server.SendData(Player.Login, "test");
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
            /*if (Admins.Contains (PC.Login)) {
				ChatSendServerMessage("Admin connected : " + PC.Login);
			}*/
            if (Records.Count == 0 ) UpdateInterface(PC.Login, new object[] { "00:00.00" }, new List<Management>(), new List<Management>());
            else if (!Records.ContainsKey(PC.Login)) UpdateInterface(PC.Login, new object[] { "00:00.00" }, Records.Values.ToList(), Records.Values.ToList());
            else UpdateInterface(PC.Login, new object[] { ToTime(Records[PC.Login].Time), Records[PC.Login].Rank }, Records.Values.ToList(), Records.Values.ToList());
        }

		void HandleOnPlayerChat (ShootManiaXMLRPC.Structs.PlayerChat PC)
		{
			ParseChatCommand(PC);
            Console.WriteLine(ServerManager.Admins.GetGroupForLogin(PC.Login));
            if (Records.Count == 0) UpdateInterface(PC.Login, new object[] { "00:00.00" }, new List<Management>(), new List<Management>());
            else if (!Records.ContainsKey(PC.Login)) UpdateInterface(PC.Login, new object[] { "00:00.00" }, Records.Values.ToList(), Records.Values.ToList());
            else UpdateInterface(PC.Login, new object[] { ToTime(Records[PC.Login].Time), Records[PC.Login].Rank }, Records.Values.ToList(), Records.Values.ToList());
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
            //if (refreshTime > Now) return;
            var rankToShow = "";
            if (Params.Count() == 1) rankToShow = "?";
            else rankToShow = Params[1].ToString();

            /*var toreturn = @"<label posn=""-148 83 0"" style=""TextButtonBig"" sizen=""25 14"" text=" + Params[0].ToString() + @" valign=""center2"" halign=""center"" textsize=""3""/>
		<label posn=""-123 83 0"" style=""TextButtonBig"" sizen=""25 14"" text=""#" + rankToShow + @""" valign=""center2"" halign=""center"" textsize=""6""/>
		<quad posn=""-160 70.5 1"" sizen=""25 0.5"" bgcolor=""FFFFFFFF"" id=""quad""/>
		<label posn=""-148 73 0"" sizen=""25 6"" text=""LOCAL"" valign=""center2"" halign=""center"" textsize=""2""/>
		<label posn=""-123 73 0"" sizen=""25 6"" text=""DEDIMANIA"" valign=""center2"" halign=""center"" textsize=""1""/>
		<quad posn=""-160 90 0"" sizen=""50 20"" bgcolor=""000000AA""/>
		<quad posn=""-160 70 0"" sizen=""50 60"" bgcolor=""000000AA""/>
		<quad posn=""-160 71 -1"" sizen=""50 10"" bgcolor=""000000AA"" style=""Bgs1InRace"" substyle=""BgGradTop""/>
		<quad posn=""-160 20 -1"" sizen=""50 10"" bgcolor=""0000006D"" style=""Bgs1InRace"" substyle=""BgGradBottom"" modulatecolor=""00000087"" opacity=""0.5""/>
		<quad posn=""-160 76 0"" sizen=""50 6"" bgcolor=""000000AA""/>
		<quad posn=""-160 76 0"" sizen=""25 6"" bgcolor=""000000AA""/>

        <script><!--
            main() {
                declare persistent MaVar = 0;
                declare Quad_Global <=> (Page.GetFirstChild(""quad"") as CMlQuad);
                Quad_Global.BgColor = LocalUser.Color;
                while (True) {
                    yield;
                    foreach (Event in PendingEvents) {
                        if (Event != Null) {
            
                        }
                    }
                }
            }
        --></script>";*/
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
            toreturn = @"<frame>
			<quad id=""header_collider"" posn=""-160 90 15"" sizen=""50 20"" opacity=""0"" scriptevents=""true""/>
			<quad id=""showsetting_collider"" posn=""-162 100 16"" bgcolor=""000"" sizen=""20 30"" opacity=""0."" scriptevents=""true"" />
			<quad id=""showinfo_collider"" posn=""-142 100 16"" bgcolor=""500"" sizen=""32 30"" opacity=""0."" scriptevents=""true"" />
			<quad id=""hide[show]"" posn=""-160 70 18"" sizen=""50 30"" bgcolor=""000"" opacity=""0."" scriptevents=""true"" />
			<quad id=""hide[show]"" posn=""-110 105 18"" sizen=""30 60"" bgcolor=""000"" opacity=""0."" scriptevents=""true"" />
		</frame>

        <frame name=""options"" id=""framesetting"">
<quad posn=""-151 83 2"" sizen=""10 10"" bgcolor=""FFFA"" halign=""center"" valign=""center"" style=""ManiaPlanetMainMenu"" substyle=""IconSettings""/>
<label posn=""-142 83 0"" sizen=""29 14"" text=""SETTINGS"" halign=""left"" valign=""center2"" textsize=""4""/>
<quad id=""tohide"" posn=""-160 90 0"" sizen=""50 90"" bgcolor=""000000AA""/>
<quad posn=""-160 90 0"" sizen=""50 14"" bgcolor=""000000AA""/>
        </frame>
		
		<frame id=""mapframe"" posn=""0 0 10"">
			<quad id=""headermapquad2"" posn=""-162 90 0"" sizen=""20 20"" bgcolor=""000"" opacity=""1"" scriptevents=""true""/>
			<quad id=""headermapquad"" posn=""-142 90 -1"" sizen=""32 20"" bgcolor=""000"" opacity=""1."" scriptevents=""true""/>
			<quad id=""mapquad"" posn=""-152 83 1"" halign=""center"" valign=""center"" sizen=""10.5 10.5"" image=""https://raw.githubusercontent.com/guerro323/AyoController/master/stadium3_blurred.png"" />
			<quad posn=""-138 86 1"" halign=""center"" valign=""center"" sizen=""4.5 4.5"" style=""Icons64x64_1"" substyle=""Finish"" />
			<quad posn=""-138 80 1"" halign=""center"" valign=""center"" sizen=""4 4"" image=""https://raw.githubusercontent.com/guerro323/AyoController/master/creator.png"" />
			
			<label id=""mapname"" posn=""-134 86 1"" halign=""left"" valign=""center"" textsize=""1.2"" sizen=""22 10"" textprefix=""$s""/>
			<label id=""creatorname"" posn=""-134 80 1"" halign=""left"" valign=""center"" textsize=""1.2"" sizen=""22 10"" textprefix=""$s""/>
			<quad id=""headermapquad2"" posn=""-160 76 2"" sizen=""50 6"" bgcolor=""111"" opacity=""0.9"" scriptevents=""true""/>
			
			<frame name=""tip"">
				<frame id=""headerbuttonsetting"">
					<quad id=""settingquad"" posn=""-152 73.5 2"" halign=""center"" valign=""center"" sizen=""5 5"" style=""ManiaPlanetMainMenu"" substyle=""IconSettings"" scriptevents=""true"" />
					<label id=""settinglabel"" posn=""-152 73.5 2"" halign=""center"" valign=""center"" textsize=""1.2"" sizen=""25 10"" text=""SETTINGS""/>
				</frame>
				<frame id=""headerbuttonmoreinfo"">	
					<quad id=""moreinfoquad"" posn=""-126 73.5 2"" halign=""center"" valign=""center"" sizen=""5 5"" style=""ManiaPlanetMainMenu"" substyle=""IconStore"" scriptevents=""true"" />
					<label id=""moreinfolabel"" posn=""-126 73.5 2"" halign=""center"" valign=""center"" textsize=""1"" sizen=""28 10"" text=""MORE INFORMATIONS"" />
				</frame>
			</frame>
		</frame>
        <frame id=""headernormal"">
		<label id=""bestlabel"" posn=""-148 87.5 0"" style=""TextButtonBig"" sizen=""25 7"" text=""$wBest"" valign=""center2"" halign=""center"" textsize=""1"" opacity=""0""/>
		<label id=""besttimelabel"" posn=""-148 83 0"" style=""TextButtonBig"" sizen=""25 14"" text=""" + Params[0].ToString() + @""" valign=""center2"" halign=""center"" textsize=""3"" opacity=""0""/>
		<label id=""timelabel"" posn=""-148 83 0"" style=""TextButtonBig"" sizen=""25 14"" text=""" + Params[0].ToString() + @""" valign=""center2"" halign=""center"" textsize=""3""/>
		<quad id=""headerbgquad"" posn=""-160 90 0"" sizen=""50 20"" bgcolor=""000000AA"" scriptevents=""true""/>
		<label posn=""-123 83 0"" style=""TextButtonBig"" sizen=""25 14"" text=""#" + Records[playerName].Rank + @""" valign=""center2"" halign=""center"" textsize=""6""/>
		<frame name=""Records"">
			<quad posn=""-160 70.5 1"" sizen=""25 0.5"" bgcolor=""FFFFFFFF""/>
			<label posn=""-148 73 0"" sizen=""25 6"" text=""LOCAL"" valign=""center2"" halign=""center"" textsize=""2""/>
			<label posn=""-123 73 0"" sizen=""25 6"" text=""DEDIMANIA"" valign=""center2"" halign=""center"" textsize=""1""/>
			
			<quad id=""buttonsbgquad"" posn=""-160 76 0"" sizen=""50 6"" bgcolor=""000000AA""/>
			<quad id=""localbuttonbgquad"" posn=""-160 76 0"" sizen=""25 6"" bgcolor=""000000AA""/>
			<quad id=""dedibuttonbgquad"" posn=""-135 76 0"" sizen=""25 6"" bgcolor=""000000AA"" opacity=""0""/>
			<frame id=""timesframe"">
				<quad id=""timesbgquad"" posn=""-160 70 0"" sizen=""50 60"" bgcolor=""000000AA""/>
				<quad id=""timesgradbgquad"" posn=""-160 70.5 -1"" sizen=""50 10"" bgcolor=""000000AA"" style=""Bgs1InRace"" substyle=""BgGradTop""/>
				<quad id=""timesgrad2bgquad"" posn=""-160 20 -1"" sizen=""50 10"" bgcolor=""0000006D"" style=""Bgs1InRace"" substyle=""BgGradBottom"" modulatecolor=""00000087"" opacity=""0.5""/>
                <frame id=""localrecord"">
                " + frameLrecord + @"
                </frame>
		</frame>
        </frame>
        </frame>
		<label id=""speedlabel"" posn=""0 -87 0"" style=""TextButtonBig"" scale=""0.5"" textsize=""10"" valign=""center"" halign=""center"" text=""00""/>
		

		<script><!--

			#Include ""TextLib"" as TL
			#Include ""MathLib"" as ML
			#Include ""AnimLib"" as AL

			#Const C_BestLabelAnimTime				200
			#Const C_TimeToWaitForBestTimeLabelAnim	1000
			#Const C_TimesMenuSlide					20
			
Text TextExt_CharAt(Text string, Integer offset)
{
	if (offset <= TL::Length(string)) {
		return TL::SubString(string, offset, 1);
	}
	return """";
}
Text TextExt_Lowercase(Text string)
{
	return TL::ToLowerCase(string);
}
			
Text TextExt_StripFormat(Text string, Boolean sc, Boolean sf, Boolean sl)
{
	declare Text result;
	declare Integer length = TL::Length(string);
	if (length < 2) return string;
	declare Text[] ft = [""w"", ""n"", ""o"", ""i"", ""s""];
	declare Text[] lt = [""l"", ""h"", ""p""];
	declare Text[] ct = [""0"", ""1"", ""2"", ""3"", ""4"", ""5"", ""6"", ""7"", ""8"", ""9"", ""a"", ""b"", ""c"", ""d"", ""e"", ""f""];
	declare Text char;
	declare Text ochar;
	declare Text state = ""0"";
	declare Text color = """";
	declare Text linktag = """";
	for (i, 0, length-1) {
		ochar = TextExt_CharAt(string, i);
		char = TextExt_Lowercase(ochar);
		switch (state) {
			case ""0"": {
				if (char == ""$"")
					state = ""tag"";
				else
					result ^= ochar;
			}
			case ""tag"": {
				if (ft.exists(char) && sf) {
					state = ""0"";
				} else if (ct.exists(char) && sc) {
					color = ochar;
					state = ""c1"";
				} else if (lt.exists(char) && sl) {
					linktag = char;
					state = ""link"";
				} else {
					result ^= ""$"" ^ ochar;
					state = ""0"";	
				}
			}
			case ""c1"": {
				if (ct.exists(char)) {
					color ^= char;
					state = ""c2"";
				} else {
					result ^= ochar;
					state = ""0"";
				}
			}
			case ""c2"": {
				if (ct.exists(char)) {
					color ^= char;
				} else {
					result ^= ochar;
				}
				state = ""0"";
			}
			case ""link"": {
				if (char == ""["") {
					state = ""linkurl"";
				} else if (char == "" "") {
					result ^= ochar;
					state = ""0"";
				} else {
					result ^= ochar;
				}
			}
			case ""linkurl"": {
				if (char == ""]"") {
					state = ""0"";
				}
			}
		}
	}
	return result;
}
Text TextExt_StripFormat(Text string)
{
	return TextExt_StripFormat(string, False, True, False);
}

			main() {
				declare CMlFrame TimesFrame = (Page.GetFirstChild(""timesframe"") as CMlFrame);
                declare CMlFrame FrameRecords = (Page.GetFirstChild(""localrecord"") as CMlFrame);
				declare CMlFrame FrameMapInfo = (Page.GetFirstChild(""mapframe"") as CMlFrame);
                declare CMlFrame FrameSettings = (Page.GetFirstChild(""framesetting"") as CMlFrame);
				declare CMlQuad TimesBgQuad = (Page.GetFirstChild(""timesbgquad"") as CMlQuad);
				declare CMlQuad TimesGradBgQuad = (Page.GetFirstChild(""timesgradbgquad"") as CMlQuad);
				declare CMlQuad TimesGrad2BgQuad = (Page.GetFirstChild(""timesgrad2bgquad"") as CMlQuad);
                declare CMlQuad SettingsBigQuad = (Page.GetFirstChild(""tohide"") as CMlQuad);
                declare CMlFrame FrameSetting = (Page.GetFirstChild(""framesetting"") as CMlFrame);
                declare CMlFrame MapFrame = (Page.GetFirstChild(""headernormal"") as CMlFrame);				

				declare CMlQuad HeaderMapQuad = (Page.GetFirstChild(""headermapquad"") as CMlQuad);
				
				declare CMlQuad MapQuad = (Page.GetFirstChild(""mapquad"") as CMlQuad);

				declare CMlLabel SpeedLabel = (Page.GetFirstChild(""speedlabel"") as CMlLabel);
				declare CMlLabel TimeLabel = (Page.GetFirstChild(""timelabel"") as CMlLabel);
				declare CMlLabel BestTimeLabel = (Page.GetFirstChild(""besttimelabel"") as CMlLabel);
				declare CMlLabel BestLabel = (Page.GetFirstChild(""bestlabel"") as CMlLabel);
				
				declare CMlLabel MapNameLabel = (Page.GetFirstChild(""mapname"") as CMlLabel);
				declare CMlLabel MapCreatorNameLabel = (Page.GetFirstChild(""creatorname"") as CMlLabel);
				
				declare CMlLabel SettingLabel = (Page.GetFirstChild(""settinglabel"") as CMlLabel);
				declare CMlQuad SettingQuad = (Page.GetFirstChild(""settingquad"") as CMlQuad);
				
				declare CMlLabel MoreInfoLabel = (Page.GetFirstChild(""moreinfolabel"") as CMlLabel);
				declare CMlQuad MoreInfoQuad = (Page.GetFirstChild(""moreinfoquad"") as CMlQuad);

                declare CMlLabel[] Label_LocalRecords;
                declare CMlQuad[] Quad_LocalRecords;

                for (I, 0, " + rank+5 + @") {
                    declare CMlLabel TempLabelRecord1 <=> (Page.GetFirstChild(""localrecord1_""^I) as CMlLabel);
                    declare CMlLabel TempLabelRecord2 <=> (Page.GetFirstChild(""localrecord2_""^I) as CMlLabel);
                    declare CMlLabel TempLabelRecord3 <=> (Page.GetFirstChild(""localrecord3_""^I) as CMlLabel);
                    if (TempLabelRecord1 == Null || TempLabelRecord2 == Null || TempLabelRecord3 == Null) continue;
                    { declare oPos for TempLabelRecord1 = TempLabelRecord1.RelativePosition; }
                    { declare oPos for TempLabelRecord2 = TempLabelRecord2.RelativePosition; }
                    { declare oPos for TempLabelRecord3 = TempLabelRecord3.RelativePosition; }
                    Label_LocalRecords.add(TempLabelRecord1);
                    Label_LocalRecords.add(TempLabelRecord2);
                    Label_LocalRecords.add(TempLabelRecord3);
                }
                for (I, 0, " + rank + 5 + @") {
                    declare CMlQuad TempLabelRecord1 <=> (Page.GetFirstChild(""localrecord4_""^I) as CMlQuad);
                    declare CMlQuad TempLabelRecord2 <=> (Page.GetFirstChild(""localrecord5_""^I) as CMlQuad);
                    if (TempLabelRecord1 == Null || TempLabelRecord2 == Null) continue;
                    { declare oPos for TempLabelRecord1 = TempLabelRecord1.RelativePosition; }
                    { declare oPos for TempLabelRecord2 = TempLabelRecord2.RelativePosition; }
                    Quad_LocalRecords.add(TempLabelRecord1);
                    Quad_LocalRecords.add(TempLabelRecord2);
                }

				declare Boolean PreviousBestLabelState for LocalUser = False;

				declare Integer TimeBestLabelStateChange for LocalUser = Now;
				
				declare Boolean PreviousMapInfoState for LocalUser = False;
				
				declare Integer TimeMapInfoState for LocalUser = Now;
				
				declare Boolean[Text] PreviousMoreSettingState for LocalUser = [""Setting"" => False, ""More"" => False];
				
				declare Integer[Text] TimeMoreSettingState for LocalUser = [""Setting"" => Now, ""More"" => Now];

                declare Boolean PreviousShowSettingState for LocalUser = False;
                declare Integer TimeShowSettingState for LocalUser = Now;

				declare Integer CurrentSpeed for LocalUser = 0;
				declare Integer RaceTime for LocalUser = 0;

				declare Boolean HeaderShowMapInfo for LocalUser = False;
				declare Boolean[Text] MoreSetting for LocalUser = [""Setting"" => False, ""More"" => False];
				
                declare ShowSetting for LocalUser = False;

				declare Text CurrentShowName for LocalUser = ""Setting"";

                declare Integer TimeSave = 0;

				TimesFrame.ClipWindowActive = True;
				TimesFrame.ClipWindowRelativePosition = <-135., 40.>;
				TimesFrame.ClipWindowSize = <50., 60.>;

                FrameRecords.ClipWindowActive = True;
                FrameRecords.ClipWindowRelativePosition = <-135., 40.>;
                FrameRecords.ClipWindowSize = <50., 60.>;


				while (True) {
					yield;
                    if (GUIPlayer == Null) return;
					CurrentSpeed = GUIPlayer.DisplaySpeed;
					RaceTime = GUIPlayer.CurRace.Time;
					
                    
                    
					foreach (Event in PendingEvents) {
                        if (Event.Type == CMlEvent::Type::MouseClick) {
                            if (Event.ControlId == ""showsetting_collider"") {
                                
                                if (!ShowSetting) {
                                    ShowSetting = True;
                                } else {
                                    ShowSetting = False;
                                }
							}
                        }
						if (Event.Type == CMlEvent::Type::MouseOver) {
							if (Event.ControlId == ""header_collider"" || (Event.ControlId == ""showsetting_collider""
							|| Event.ControlId == ""showinfo_collider"")) {
								HeaderShowMapInfo = True;

							}
							if (Event.ControlId == ""showsetting_collider"") {
								MoreSetting[""Setting""] = True;
								MoreSetting[""More""] = False;
								CurrentShowName = ""Setting"";
							}
							if (Event.ControlId == ""showinfo_collider"") {
								MoreSetting[""More""] = True;
								MoreSetting[""Setting""] = False;
								CurrentShowName = ""More"";
							}
							if (Event.ControlId == ""hide[show]"") {
								HeaderShowMapInfo = False;
								MoreSetting[""Setting""] = False;
								MoreSetting[""More""] = False;
							}
						}
						if (Event.Type == CMlEvent::Type::MouseOut) {
							if (Event.ControlId == ""header_collider"" || Event.ControlId == ""hide[show]"") {
								if (!MoreSetting[""Setting""]) {
									HeaderShowMapInfo = False;
								}
								if (!MoreSetting[""More""]) {
									HeaderShowMapInfo = False;
								}
							}
							if (Event.ControlId == ""showsetting_collider"") {
								MoreSetting[""Setting""] = False;
								MoreSetting[""More""] = False;
							}
							if (Event.ControlId == ""showinfo_collider"") {
								MoreSetting[""More""] = False;
								MoreSetting[""Setting""] = False;
							}
						}
					}
					if (MapNameLabel.Value != Map.MapName || MapCreatorNameLabel.Value != Map.AuthorNickName) {
						MapNameLabel.Value = ""$""^TL::ColorToText(LocalUser.Color)^""""^Map.MapName;
						MapCreatorNameLabel.Value = TextExt_StripFormat(""$""^TL::ColorToText(LocalUser.Color)^""by ""^Map.AuthorNickName);

                        if (Map.CollectionName == ""Canyon"") {
						    MapQuad.ModulateColor = <1., 0.1, 0.>;
						    HeaderMapQuad.BgColor = <0.4, 0.1, 0.>;
                        }
                        if (Map.CollectionName == ""Valley"") {
						    MapQuad.ModulateColor = <0.1, 0.9, 0.1>;
						    HeaderMapQuad.BgColor = <0.05, 0.5, 0.05>;
                        }
                        if (Map.CollectionName == ""Stadium"") {
						    MapQuad.ModulateColor = <0., 0.9, 0.>;
						    HeaderMapQuad.BgColor = <0., 0.5, 0.>;
                        }
					}
					
					if (HeaderShowMapInfo) {
						if (!PreviousMapInfoState) {
							TimeMapInfoState = Now + 10;
							PreviousMapInfoState = True;
						}
						declare Transition = AL::EaseInOutQuad(Now - TimeMapInfoState, 0., 1., C_BestLabelAnimTime);
						FrameMapInfo.RelativePosition[1] = 40 - C_TimesMenuSlide - Transition * C_TimesMenuSlide;
					} else {
						if (PreviousMapInfoState) {
							TimeMapInfoState = Now + 250;
							PreviousMapInfoState = False;
						}
						declare Transition = AL::EaseInOutQuad(Now - TimeMapInfoState, 1., -1., C_BestLabelAnimTime);
						FrameMapInfo.RelativePosition[1] = 40 - C_TimesMenuSlide - Transition * C_TimesMenuSlide;
					}

					if (ShowSetting) {
						if (!PreviousShowSettingState) {
							TimeShowSettingState = Now + 10;
							PreviousShowSettingState = True;
						}
						declare Transition = AL::EaseInOutQuad(Now - TimeShowSettingState, 0., 1., C_BestLabelAnimTime);
                        declare Transition2 = AL::EaseInOutQuad(Now - TimeShowSettingState, 0., 4., C_BestLabelAnimTime);
						FrameSettings.RelativePosition[1] = 40 - C_TimesMenuSlide - Transition * C_TimesMenuSlide;
                        MapFrame.RelativePosition[0] = 20 - C_TimesMenuSlide - Transition2 * C_TimesMenuSlide;
                        TimesFrame.RelativePosition[0] = 20 - C_TimesMenuSlide - Transition2 * C_TimesMenuSlide;
                        SettingsBigQuad.Opacity = Transition;
					} else {
						if (PreviousShowSettingState) {
							TimeShowSettingState = Now + 250;
							PreviousShowSettingState = False;
						}
						declare Transition = AL::EaseInOutQuad(Now - TimeShowSettingState, 1., -1., C_BestLabelAnimTime);
                        declare Transition2 = AL::EaseInOutQuad(Now - TimeShowSettingState, 1., -1., C_BestLabelAnimTime);
						FrameSettings.RelativePosition[1] = 40 - C_TimesMenuSlide - Transition * C_TimesMenuSlide;
                        TimesFrame.RelativePosition[0] = 20 - C_TimesMenuSlide - Transition2 * (C_TimesMenuSlide*2);
                        MapFrame.RelativePosition[0] = 20 - C_TimesMenuSlide - Transition2 * (C_TimesMenuSlide*2);
                        SettingsBigQuad.Opacity = Transition;
					}
					
					if (MoreSetting[CurrentShowName]) {
						if (!PreviousMoreSettingState[CurrentShowName]) {
							TimeMoreSettingState[CurrentShowName] = Now + 10;
							PreviousMoreSettingState[CurrentShowName] = True;
							if (CurrentShowName == ""Setting"") {
								TimeMoreSettingState[""More""] = Now + 250;
							}
							if (CurrentShowName == ""More"") {
								TimeMoreSettingState[""Setting""] = Now + 250;
							}
						}
						declare Transition = AL::EaseInOutQuad(Now - TimeMoreSettingState[CurrentShowName], 0., 1., C_BestLabelAnimTime);
						if (CurrentShowName == ""Setting"") {
							SettingLabel.Opacity = Transition;
							SettingLabel.RelativeScale = 1.5 - Transition / 2.;
							SettingQuad.Opacity = 1. - Transition;
							SettingQuad.RelativeScale = 1.5 - Transition / 2.;
							{
								declare Transition2 = AL::EaseInOutQuad(Now - TimeMoreSettingState[""More""], 1., -1., C_BestLabelAnimTime);
								MoreInfoLabel.Opacity = Transition2;
								MoreInfoLabel.RelativeScale = 1.5 - Transition2 / 2.;
								MoreInfoQuad.Opacity = 1. - Transition2;
								MoreInfoQuad.RelativeScale = 1.5 - Transition2 / 2.;							
							}
						}
						if (CurrentShowName == ""More"") {
							MoreInfoLabel.Opacity = Transition;
							MoreInfoLabel.RelativeScale = 1.5 - Transition / 2.;
							MoreInfoQuad.Opacity = 1. - Transition;
							MoreInfoQuad.RelativeScale = 1.5 - Transition / 2.;
							{
								declare Transition2 = AL::EaseInOutQuad(Now - TimeMoreSettingState[""Setting""], 1., -1., C_BestLabelAnimTime);
								SettingLabel.Opacity = Transition2;
								SettingLabel.RelativeScale = 1.5 - Transition2 / 2.;
								SettingQuad.Opacity = 1. - Transition2;
								SettingQuad.RelativeScale = 1 - Transition2 / 2.;							
							}
						}
					} else {
						if (PreviousMoreSettingState[CurrentShowName]) {
							TimeMoreSettingState[CurrentShowName] = Now + 250;
							PreviousMoreSettingState[CurrentShowName] = False;
							if (CurrentShowName == ""Setting"") {
								TimeMoreSettingState[""More""] = Now + 250;
							}
							if (CurrentShowName == ""More"") {
								TimeMoreSettingState[""Setting""] = Now + 250;
							}
						}
						declare Transition = AL::EaseInOutQuad(Now - TimeMoreSettingState[CurrentShowName], 1., -1., C_BestLabelAnimTime);
						if (CurrentShowName == ""Setting"") {
							SettingLabel.Opacity = Transition;
							SettingLabel.RelativeScale = 1.5 - Transition / 2.;
							SettingQuad.Opacity = 1. - Transition;
							SettingQuad.RelativeScale = 1 - Transition / 2.;
							{
								declare Transition2 = AL::EaseInOutQuad(Now - TimeMoreSettingState[""More""], 0., 1., C_BestLabelAnimTime);
								MoreInfoLabel.Opacity = Transition2;
								MoreInfoLabel.RelativeScale = 1.5 - Transition2 / 2.;
								MoreInfoQuad.Opacity = 1. - Transition2;
								MoreInfoQuad.RelativeScale = 1.5 - Transition2 / 2.;							
							}
						}
						if (CurrentShowName == ""More"") {
							MoreInfoLabel.Opacity = Transition;
							MoreInfoLabel.RelativeScale = 1.5 - Transition / 2.;
							MoreInfoQuad.Opacity = 1. - Transition;
							MoreInfoQuad.RelativeScale = 1.5 - Transition / 2.;
							{
								declare Transition2 = AL::EaseInOutQuad(Now - TimeMoreSettingState[""Setting""], 0., 1., C_BestLabelAnimTime);
								SettingLabel.Opacity = Transition2;
								SettingLabel.RelativeScale = 1.5 - Transition2 / 2.;
								SettingQuad.Opacity = 1. - Transition2;
								SettingQuad.RelativeScale = 1.5 - Transition2 / 2.;
							}
						}
					}
					
					if (CurrentSpeed < 10. && GUIPlayer.Login == LocalUser.Login) {
						if (!PreviousBestLabelState) {
							TimeBestLabelStateChange = Now + C_TimeToWaitForBestTimeLabelAnim;
							PreviousBestLabelState = True;
						}
                       // ClientUI.OverlayHideChat = False;
                        ClientUI.UISequence = CUIConfig::EUISequence::Playing;
						declare Transition = AL::EaseInOutQuad(Now - TimeBestLabelStateChange, 0., 1., C_BestLabelAnimTime);
						BestLabel.Opacity = Transition;
						BestTimeLabel.Opacity = Transition;
						TimeLabel.Opacity = 1. - Transition;

						TimesBgQuad.Opacity = Transition;
						TimesBgQuad.RelativePosition[1] = 110 - C_TimesMenuSlide - Transition * C_TimesMenuSlide;

						TimesGradBgQuad.Opacity = Transition;
						TimesGradBgQuad.RelativePosition[1] = 110.5 - C_TimesMenuSlide - Transition * C_TimesMenuSlide;

						TimesGrad2BgQuad.Opacity = Transition;
						TimesGrad2BgQuad.RelativePosition[1] = 60 - C_TimesMenuSlide - Transition * C_TimesMenuSlide;

                        FrameRecords.RelativePosition[1] = 40 - C_TimesMenuSlide - Transition * C_TimesMenuSlide;

                        foreach (RecordTime in Label_LocalRecords) {
                            if (RecordTime == Null) continue;
                            declare Vec3 oPos for RecordTime;
                            RecordTime.RelativePosition[1] = ( oPos[1] + 40 - C_TimesMenuSlide - Transition * C_TimesMenuSlide);
                            RecordTime.Opacity = Transition;
                        }
                        foreach (RecordTime in Quad_LocalRecords) {
                            if (RecordTime == Null) continue;
                            declare Vec3 oPos for RecordTime;
                            RecordTime.RelativePosition[1] = ( oPos[1] + 40 - C_TimesMenuSlide - Transition * C_TimesMenuSlide);
                            RecordTime.Opacity = Transition;
                        }


						TimeLabel.RelativeScale = 1 - Transition / 2.;
						BestTimeLabel.RelativeScale = 1.5 - Transition / 2.;
					} else if (GUIPlayer.Login == LocalUser.Login) {
                        if (Now + 1000 > TimeSave && TimeSave != 0) {
                            TimeSave = Now + 1000;
                        }
                        if (Now + 500 > TimeSave || TimeSave == 0) {
                            TimeBestLabelStateChange = Now;
                            TimeSave = Now + 50000;
                        }
						if (PreviousBestLabelState) {
							TimeBestLabelStateChange = Now;
							PreviousBestLabelState = False;
						}
                       // ClientUI.OverlayHideChat = True;
                        ClientUI.UISequence = CUIConfig::EUISequence::Playing;
						declare Transition = AL::EaseInOutQuad(Now - TimeBestLabelStateChange, 1., -1., C_BestLabelAnimTime);
						BestLabel.Opacity = Transition;
						BestTimeLabel.Opacity = Transition;
						TimeLabel.Opacity = 1. - Transition;

						TimesBgQuad.Opacity = Transition;
						TimesBgQuad.RelativePosition[1] = 110 - C_TimesMenuSlide - Transition * C_TimesMenuSlide;

						TimesGradBgQuad.Opacity = Transition;
						TimesGradBgQuad.RelativePosition[1] = 110.5 - C_TimesMenuSlide - Transition * C_TimesMenuSlide;
						
						TimesGrad2BgQuad.Opacity = Transition;
						TimesGrad2BgQuad.RelativePosition[1] = 60 - C_TimesMenuSlide - Transition * C_TimesMenuSlide;

                        FrameRecords.RelativePosition[1] = 40 - C_TimesMenuSlide - Transition * C_TimesMenuSlide;
                        
                        foreach (RecordTime in Label_LocalRecords) {
                            if (RecordTime == Null) continue;
                            declare Vec3 oPos for RecordTime;
                            RecordTime.RelativePosition[1] = ( oPos[1] + 40 - C_TimesMenuSlide - Transition * C_TimesMenuSlide);
                            RecordTime.Opacity = Transition;
                        }
                        foreach (RecordTime in Quad_LocalRecords) {
                            if (RecordTime == Null) continue;
                            declare Vec3 oPos for RecordTime;
                            RecordTime.RelativePosition[1] = ( oPos[1] + 40 - C_TimesMenuSlide - Transition * C_TimesMenuSlide);
                            RecordTime.Opacity = Transition;
                        }

						TimeLabel.RelativeScale = 1 - Transition / 2.;
						BestTimeLabel.RelativeScale = 1.5 - Transition / 2.;
					}
					
                    
					
					SpeedLabel.SetText(""$s"" ^ TL::ToText(CurrentSpeed));
					SpeedLabel.TextSize = 10;
					SpeedLabel.RelativeScale = AL::EaseInOutQuad(CurrentSpeed, 0.5, 5., 1000);
					SpeedLabel.RelativePosition[0] = ML::Rand(-1., 1.) * (CurrentSpeed / 1000.);
					SpeedLabel.RelativePosition[1] = -87 + AL::EaseInOutQuad(CurrentSpeed, 0., 25., 1000) + ML::Rand(-1., 1.) * (CurrentSpeed / 1000.);
					TimeLabel.SetText(TL::TimeToText(RaceTime, True));
				}
			}
		-->
		</script>
            ";
            ServerManager.AddThisManialink(playerName, toreturn, "Records", true);
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
                if (mapUID != ServerManager.Server.GetCurrentMapInfo().UId)
                {
                    LoadMapUID();
                    LoadRecord();
                }
            }
            // ChatSendServerMessage(Now.ToString());
            foreach (ShootManiaXMLRPC.Structs.PlayerList Player in ServerManager.Server.GetPlayerList(100, 0))
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
            if (milliseconds < 100) format += "0";
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
                    ChatSendServerMessage(result);

                    //string format = ToTime(Records[ID].Time);
                    //UpdateInterface(ID, new object[] { format, Records[ID].Rank }, SortedList, SortedList);

                    // Save to json
                    var stringJson = JsonMapper.ToJson(Records);
                    Console.WriteLine("1");
                    File.WriteAllText("Plugins/misc/records_" + mapUID + ".json", stringJson);
                    Console.WriteLine("2");
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
                }
                else ServerManager.Server.ChatSendToLogin(ID, "$fffProgress $999» $fff" + ServerManager.GetPlayer(e.Response.Params[1].ToString()).Nickname + "$z$s$> best :" + ToTime(Records[ID].Time) + " current :" + ToTime(Timez));
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

