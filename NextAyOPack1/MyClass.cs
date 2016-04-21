//
//  MyClass.cs
//
//  Author:
//       guerro <kaltobattle@gmail.com>
//
//  Copyright (c) 2016 guerro
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using AyOPlugins.DediManiaPl;
using AyoController;
using AyoController.Plugins;
using AyoController.Classes;
using System.Linq;
using System.IO;
using System.Reflection;
using ShootManiaXMLRPC.Structs;

namespace AyOPlugins.Pack.AyODefaultPack
{

    public partial class AyOPack : Plugin
	{

        /// <remarks>
        /// Created by 56ka;
        /// http://stackoverflow.com/questions/1892492/set-custom-path-to-referenced-dlls
        /// </remarks>
        /// <summary>
        /// Here is the list of authorized assemblies (DLL files)
        /// You HAVE TO specify each of them and call InitializeAssembly()
        /// </summary>
        private static string[] LOAD_ASSEMBLIES = { "Dedimania.dll" };

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

        public bool UseCustomChatFrame = false;
        public bool UseRelay = true;
        DediMania Dedi;

		public override string Name {
			get {
					return "AyONext_Plugin_Pack_1";
				}
			set {
					base.Name = value;
				}
		}

		public override string Author {
			get {
				return "Guerro";
			}
			set {
				base.Author = value;
			}
		}

        public override AyO.HelpCommands ListofCommands
        {
            get
            {
                // return new string[] { "/pack useRelay _bool", "/pack opensettings", "/pack customchat _bool" };
                return new AyO.HelpCommands()
                {
                    FieldClassName = "AyOPack",
                    Path = "/pack",
                    Commands =
                        new[]
                        {
                            new AyO._Commands {Command = "useRelay", Params = new [] { UseRelay.ToString() }},
                            new AyO._Commands {Command = "opensetting"},
                            new AyO._Commands {Command = "customchat", Params = new [] { UseCustomChatFrame.ToString() }}
                        }
                };
            }
        }

        public override AyO.PluginFunction[] PluginFunction {
			get {
				return new AyO.PluginFunction[] {
					AyO.PluginFunction.Nothing
				};
			}
			set {
				base.PluginFunction = value;
			}
		}

		public override string Version {
			get {
				return "PACK-0.1";
			}
			set {
				base.Version = value;
			}
		}
			
		public override void OnLoad ()
		{
            initializeAssembly();
        }

        public void OnUpdateInterface(string playerName, object[] Params)
        {
            var rankToShow = "";
            if (Params.Count() == 1) rankToShow = "?";
            else rankToShow = Params[1].ToString();
            int rank = 0;
            var toreturn = "";
            var frameLrecord = "";
            Dedi.LocalRecords.OrderBy(o => o.Value.Time).ToList();
            List<DediMania.Management> lRecords = Dedi.LocalRecords.Values.ToList();
            List<DediMania.Management> sortedLocalRecords = lRecords.OrderBy(ao => ao.Time).ToList();

            //first, sort the rank
            foreach (var record in sortedLocalRecords)
            {
                record.Rank = sortedLocalRecords.IndexOf(record) + 1;
            }
            var color = "";
            var I = 0F;
            foreach (var record in sortedLocalRecords)
            {
                Dedi.LocalRecords[record.Login] = record;
                Dedi.LocalRecords[record.Login].Rank = rank + 1;
                if ((rank % 2) == 0) color = "FFFA";
                else color = "FFFFFF00";
                if (rank < 8)
                {
                    frameLrecord += @"<frame posn=""-136.5 " + ((-I) + 67.25) + @" -6"" scale=""0.328"">
<quad id=""localrecord4_" + rank + @""" posn=""-70 10 -1"" sizen=""150 20"" bgcolor=""" + color + @"""/>
<label id=""localrecord1_" + rank + @""" posn=""45 0 0"" sizen=""70 20"" textprefix=""$s"" text=""" + record.Pseudo + @""" halign=""center"" valign=""center"" textsize=""6.5""/>
<label id=""localrecord2_" + rank + @""" posn=""-10 0 0"" sizen=""40 20"" text=""" + DediMania.ToTime(record.Time) + @""" halign=""center"" valign=""center"" textsize=""8""/>
<quad id=""localrecord5_" + rank + @"""  posn=""-70 0 1"" sizen=""20 20"" bgcolor=""FFFA"" halign=""left"" valign=""center""/>
<label id=""localrecord3_" + rank + @""" posn=""-40 0 0"" sizen=""20 20"" text=""" + record.Rank + @""" style=""TextButtonBig"" valign=""center2"" halign=""center"" textsize=""15""/></frame>";
                }
                I += 6.9F;
                rank++;
            }
            string streamWidget = ServerManager.ReadText(Name, 0, "interface");

            streamWidget = streamWidget.Replace("[.besttimelabel.]", Params[0].ToString());
            streamWidget = streamWidget.Replace("[.recordrank.]", rankToShow);
            streamWidget = streamWidget.Replace("<!-- LOCALRECORD -->", frameLrecord);
            streamWidget = streamWidget.Replace("->RANK", rank + 2.ToString());
            streamWidget = streamWidget.Replace("->CHATS", ServerManager.ChatList.Count.ToString());
            streamWidget = streamWidget.Replace("ShowChatFrame", UseCustomChatFrame.ToString());
            
            List<AyO.Chatclass> ChatSortedList = ServerManager.ChatList.OrderBy(ao => -ao.ThisNow).ToList();



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

            foreach (var server in ServerManager.Servers.Where(server => server.IsConnected && AyO.CompressEnviroName(server.GetCurrentMapInfo().Environnement) == "tm"))
            ServerManager.AddThisManialink(playerName, streamWidget, "AyoPack1", true, server);
        }

        public override void OnEverythingLoaded()
		{
            foreach (var Plugin in ServerManager.LoadedPlugins)
            {
                if (Plugin.GetType() == typeof(AyOPlugins.DediManiaPl.DediMania))
                {
                    Dedi = (DediMania)Plugin;
                    // Activate the API mode;
                    Dedi.UseApiFeature = true;
                }
            }
            // Load interface
            foreach (var Player in ServerManager.GetPlayers())
            {
                if (Dedi.LocalRecords.Count == 0) OnUpdateInterface(Player.Login, new object[] { "00:00.00" });
                else if (!Dedi.LocalRecords.ContainsKey(Player.Login)) OnUpdateInterface(Player.Login, new object[] { "00:00.00" });
                else OnUpdateInterface(Player.Login, new object[] { DediMania.ToTime(Dedi.LocalRecords[Player.Login].Time), Dedi.LocalRecords[Player.Login].Rank });
            }
            /*var streamrod = ServerManager.ReadText(this.Name, 0, "interface");
            ServerManager.AddThisManialink(streamrod, "AyOPack1", true);*/
        }

		public override void OnLoop ()
		{
            if (this.ServerManager.RefreshTime < this.ServerManager.Now)
            {
                foreach (var Player in ServerManager.GetPlayers())
                {
                    if (Dedi.LocalRecords.Count == 0) OnUpdateInterface(Player.Login, new object[] { "00:00.00" });
                    else if (!Dedi.LocalRecords.ContainsKey(Player.Login)) OnUpdateInterface(Player.Login, new object[] { "00:00.00" });
                    else OnUpdateInterface(Player.Login, new object[] { DediMania.ToTime(Dedi.LocalRecords[Player.Login].Time), Dedi.LocalRecords[Player.Login].Rank });
                }
            }
		}

        public override void HandleOnPlayerChat(PlayerChat pc)
        {
            /*if (ServerManager.Admins.GetGroupForLogin(pc.Login) == "Admins")
            {
                if (pc.Text == "/pack customchat true") UseCustomChatFrame = true;
            }*/
        }

        public override void HandleEventGbxCallback(object o, ShootManiaXMLRPC.XmlRpc.GbxCallbackEventArgs e)
        {
            /*if (e == null) return;
            var name = e.Response.MethodName;
            if (name == "ManiaPlanet.PlayerManialinkPageAnswer")
            {
                ServerManager.Server.ChatEnableManualRouting();
                string response = e.Response.Params[2].ToString();

                if (response.StartsWith("PackMessage:"))
                {
                    response = response.Replace("PackMessage:", "");
                    if (ServerManager.Admins.GetGroupForLogin(e.Response.Params[1].ToString()) == "Admins")
                    {
                        if (response == "/pack customchat false")
                        {
                            UseCustomChatFrame = false;
                        }
                    }
                    ServerManager.Server.SendAsLogin(e.Response.Params[1].ToString(), response);
                    foreach (var Player in ServerManager.GetPlayers())
                    {
                        if (Dedi.LocalRecords.Count == 0) OnUpdateInterface(Player.Login, new object[] { "00:00.00" });
                        else if (!Dedi.LocalRecords.ContainsKey(Player.Login)) OnUpdateInterface(Player.Login, new object[] { "00:00.00" });
                        else OnUpdateInterface(Player.Login, new object[] { DediMania.ToTime(Dedi.LocalRecords[Player.Login].Time), Dedi.LocalRecords[Player.Login].Rank });
                    }
                }
            }*/
        }
    }
}

