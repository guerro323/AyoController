using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AyoController.Plugins;
using AyoController.Classes;
using ShootManiaXMLRPC.XmlRpc;
using AyoController;

namespace MapInformation
{
    public partial class MapInformation : Plugin
    {
        private AyoController.Classes.ServerManager ServerManager;
        ManiaExchange MyManiaExchange = new ManiaExchange();

        public override AyO.PluginFunction PluginFunction
        {
            get
            {
                return AyO.PluginFunction.Nothing;
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
                return "MapInformation";
            }
        }
        public override string Author
        {
            get
            {
                return "Guerro";
            }
        }
        public override string[] listofCommands
        {
            get
            {
                return new string[] { "[/map => info, mxlink]" };
            }
        }
        public override void OnLoad()
        {

        }

        string streamrod;
        string Widget_MXList_Frame;

        public override void OnServerManagerInitialize(ServerManager ServerManager)
        {
            this.ServerManager = ServerManager;
            this.ServerManager.OnConnectionSuccessful += HandleOnConnectionSuccessful;

            // Load Xml...
            streamrod = ServerManager.ReadText(Name, 0, "Widget");
            Widget_MXList_Frame = ServerManager.ReadText(Name, 0, "Widget_MXList_Frame");
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
            Console.WriteLine(PC.Login);
            if (PC.Text != "" && PC.Login != "guerro")
            {
                
            }
        }

        string mapUID;
        public int Now;
        public int RefreshTime;

        public override void OnLoop()
        {
            if (RefreshTime < Now && ServerManager.Server.GetCurrentMapInfo() != null)
            {
                MapInfo[] Maps;
                //Maps = MyManiaExchange.ToResults(MyManiaExchange.Request(MXAPIType.Site,new MapArgument { argEnviro = "tm", argAuthor="guerro" }));

                Widget_MXList_Frame = ServerManager.ReadText(Name, 0, "Widget_MXList_Frame");

                RefreshTime = Now + 5000;
                var MXMapResult = "";
                var rank = 0;
                var TempList = Widget_MXList_Frame;
                /*if (Maps.Count() > 0)
                {
                    foreach (var Map in Maps)
                    {
                        if (Map.Name == "" || Map.Username == "" || Map.UserID < 0) continue;
                        TempList = TempList.Replace("(->)[mx]Map1()", "[mx]Map_" + rank);
                        if ((rank % 2) == 0) TempList = TempList.Replace("(->)color", "C2C2C2CA");
                        else TempList = TempList.Replace("(->)color", "D4DD4D4CA");
                        TempList = TempList.Replace("((0))", "" + -(rank * 16));
                        TempList = TempList.Replace("MXMAPNAME", Map.Name);
                        TempList = TempList.Replace("MXMAPAUTHOR", Map.Username);
                        MXMapResult += TempList;
                        TempList = Widget_MXList_Frame;
                        rank++;
                    }
                }*/

                streamrod = ServerManager.ReadText(Name, 0, "Widget");
                streamrod = streamrod.Replace("MapName", ServerManager.Server.GetCurrentMapInfo().Name);
                streamrod = streamrod.Replace("MapAuthor", ServerManager.Server.GetCurrentMapInfo().Author);
                /*streamrod = streamrod.Replace("<!-- MX_LIST -->", MXMapResult);
                Console.WriteLine(ServerManager.Server.GetCurrentMapInfo().Environnement);
                streamrod = streamrod.Replace("$f750 AWARDS", "$f75" + MyManiaExchange.GetMapInformation(AyO.CompressEnviroName(ServerManager.Server.GetCurrentMapInfo().Environnement),ServerManager.Server.GetCurrentMapInfo().UId).AwardCount + " AWARDS");
                streamrod = streamrod.Replace("Picture", "https://" + AyO.CompressEnviroName(ServerManager.Server.GetCurrentMapInfo().Environnement) + ".mania-exchange.com/tracks/screenshot/normal/" + MyManiaExchange.GetMapInformation(AyO.CompressEnviroName(ServerManager.Server.GetCurrentMapInfo().Environnement), ServerManager.Server.GetCurrentMapInfo().UId).TrackID);*/
                ServerManager.AddThisManialink("", streamrod, "MapInformationPL", true);
            }

            if (ServerManager.Server.GetCurrentMapInfo().UId != null && mapUID != ServerManager.Server.GetCurrentMapInfo().UId)
            {
                mapUID = ServerManager.Server.GetCurrentMapInfo().UId;
            }
            Now = Environment.TickCount;
        }


        public override void HandleEventGbxCallback(object o, GbxCallbackEventArgs e)
        {
            if (e != null)
            {
                
                var Name = e.Response.MethodName;
                Console.WriteLine(Name);
                if (Name == "ManiaPlanet.BeginMap" || Name == "ManiaPlanet.BeginRound")
                {
                    // Load . . .
                    MapInfo[] Maps;
                    Maps = MyManiaExchange.ToResults(MyManiaExchange.Request(MXAPIType.Site, new MapArgument { argEnviro = "tm" }));

                    Widget_MXList_Frame = ServerManager.ReadText(Name, 0, "Widget_MXList_Frame");

                    RefreshTime = Now + 5000;
                    var MXMapResult = "";
                    var rank = 0;
                    var TempList = Widget_MXList_Frame;
                    foreach (var Map in Maps)
                    {
                        TempList = TempList.Replace("(->)[mx]Map1()", "[mx]Map_" + rank);
                        if ((rank % 2) == 0) TempList = TempList.Replace("(->)color", "C2C2C2CA");
                        else TempList = TempList.Replace("(->)color", "D4DD4D4CA");
                        TempList = TempList.Replace("((0))", "" + -(rank * 16));
                        TempList = TempList.Replace("MXMAPNAME", Map.Name);
                        TempList = TempList.Replace("MXMAPAUTHOR", Map.Username);
                        MXMapResult += TempList;
                        TempList = Widget_MXList_Frame;
                        rank++;
                    }

                    streamrod = ServerManager.ReadText(Name, 0, "Widget");
                    streamrod = streamrod.Replace("MapName", ServerManager.Server.GetCurrentMapInfo().Name);
                    streamrod = streamrod.Replace("MapAuthor", ServerManager.Server.GetCurrentMapInfo().Author);
                    streamrod = streamrod.Replace("<!-- MX_LIST -->", MXMapResult);
                    ServerManager.AddThisManialink(null, streamrod, "MapInformationPL", true);
                }
            }
        }
        public override void OnConsoleCommand(string Command)
        {
            if (Command == "reload_ml")
            {

            }
            if (Command.StartsWith("addmap")) {
                string ID = Command.Replace("addmap", string.Empty);
                ServerManager.Server.ChatSendServerMessage(AyO.ChatPrefix + " [$29fMANIA-EXCHANGE$fff] Adding map : $999" + ID);
                ServerManager.AddMXMap(MyManiaExchange, int.Parse(ID));
            }
        }
    }
}
