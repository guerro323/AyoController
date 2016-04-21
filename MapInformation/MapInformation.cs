using AyoController;
using AyoController.Classes;
using AyoController.Plugins;
using ShootManiaXMLRPC;
using ShootManiaXMLRPC.Structs;
using ShootManiaXMLRPC.XmlRpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MapInformation
{

    public class MapInformation : Plugin
    {
        private string _mapUid;
        private readonly ManiaExchange _myManiaExchange = new ManiaExchange();
        public override ServerManager ServerManager { get; set; }
        private string _streamrod;
        private string _widgetMxListFrame;
        public int Now;
        public int RefreshTime;

        Dictionary<string, MapArgument> _mapArgument = new Dictionary<string, MapArgument>(); //< of players

        public void OnUpdateInterface(string playerLogin)
        {
            if (!string.IsNullOrEmpty(playerLogin))
            {
                if (!_mapArgument.ContainsKey(playerLogin))
                    _mapArgument.Add(playerLogin,
                        new MapArgument()
                        {
                            ArgEnviro =
                                AyO.CompressEnviroName(
                                    ServerManager.GetPlayer(playerLogin).Server.GetCurrentMapInfo().Environnement)
                        });
                List<MapInfo> Maps;
                Maps =
                    _myManiaExchange.ToResults(_myManiaExchange.Request(MxapiType.Site,
                        _mapArgument[playerLogin])).ToList();
                _widgetMxListFrame = ServerManager.ReadText(Name, 0, "Widget_MXList_Frame");
                RefreshTime = Now + 0x1388;
                var MXMapResult = "";
                var rank = 0;
                var TempList = _widgetMxListFrame;
                if (Maps.Count > 0)
                {
                    foreach (var Map in Maps)
                    {
                        if (Map.Name == "" || Map.Username == "" || Map.UserID < 0) continue;
                        TempList = TempList.Replace("(->)[mx]Map1()", "[mx]Map_" + rank);
                        if ((rank%2) == 0) TempList = TempList.Replace("(->)color", "C2C2C2CA");
                        else TempList = TempList.Replace("(->)color", "D4DD4D4CA");
                        TempList = TempList.Replace("((0))", "" + -(rank*16));
                        TempList = TempList.Replace("MXMAPNAME", Map.Name);
                        TempList = TempList.Replace("MXMAPAUTHOR", Map.Username);
                        TempList = TempList.Replace("((index))", (Maps.IndexOf(Map) + 1).ToString());
                        MXMapResult += TempList;
                        TempList = _widgetMxListFrame;
                        rank++;
                    }
                }
                _streamrod = ServerManager.ReadText(Name, 0, "Widget");
                _streamrod = _streamrod.Replace("((y))", (Maps.Count/4).ToString());
                _streamrod = _streamrod.Replace("MapName", ServerManager.GetCurrentMapInfo.Name);
                _streamrod = _streamrod.Replace("MapAuthor", ServerManager.GetCurrentMapInfo.Author);
                _streamrod = _streamrod.Replace("<!-- MX_LIST -->", MXMapResult);
                ServerManager.AddThisManialink(playerLogin, _streamrod, "MapInformationPL", true);
                return;
            }
            foreach (var player in ServerManager.GetPlayers().Where(player => !player.Null && player.Server != null && player.Server.IsConnected))
            {
                if (!_mapArgument.ContainsKey(player.Login))
                    _mapArgument.Add(player.Login,
                        new MapArgument()
                        {
                            ArgEnviro = AyO.CompressEnviroName(player.Server.GetCurrentMapInfo().Environnement)
                        });
                List<MapInfo> Maps;
                Maps =
                    _myManiaExchange.ToResults(_myManiaExchange.Request(MxapiType.Site,
                        _mapArgument[player.Login])).ToList();
                _widgetMxListFrame = ServerManager.ReadText(Name, 0, "Widget_MXList_Frame");
                RefreshTime = Now + 0x1388;
                var MXMapResult = "";
                var rank = 0;
                var TempList = _widgetMxListFrame;
                if (Maps.Count > 0)
                {
                    foreach (var Map in Maps)
                    {
                        if (Map.Name == "" || Map.Username == "" || Map.UserID < 0) continue;
                        TempList = TempList.Replace("(->)[mx]Map1()", "[mx]Map_" + rank);
                        if ((rank%2) == 0) TempList = TempList.Replace("(->)color", "C2C2C2CA");
                        else TempList = TempList.Replace("(->)color", "D4DD4D4CA");
                        TempList = TempList.Replace("((0))", "" + -(rank*16));
                        TempList = TempList.Replace("MXMAPNAME", Map.Name);
                        TempList = TempList.Replace("MXMAPAUTHOR", Map.Username);
                        TempList = TempList.Replace("((index))", (Maps.IndexOf(Map) + 1).ToString());
                        MXMapResult += TempList;
                        TempList = _widgetMxListFrame;
                        rank++;
                    }
                }
                _streamrod = ServerManager.ReadText(Name, 0, "Widget");
                _streamrod = _streamrod.Replace("((y))", (Maps.Count/4).ToString());
                var MapInfoName = player.Server.GetCurrentMapInfo().Name;
                var MapInfoCreator = player.Server.GetCurrentMapInfo().Author;
                _streamrod = _streamrod.Replace("MapName", MapInfoName);
                _streamrod = _streamrod.Replace("MapAuthor", MapInfoCreator);
                if (!string.IsNullOrEmpty(MXMapResult)) _streamrod = _streamrod.Replace("<!-- MX_LIST -->", MXMapResult);
                ServerManager.AddThisManialink(player.Login, _streamrod, "MapInformationPL", true, player.Server);
            }
        }

        public override void HandleFixedGbxCallBacks(GbxCallbackEventArgs response, string methodname, ShootManiaServer maniaServer)
        {
            var e = (GbxCallbackEventArgs) response;
            if (e == null) return;
            if (methodname == null)
                return;
            if (methodname == "")
                return;
            if (methodname == "ManiaPlanet.BeginRound")
            {
                OnUpdateInterface("");
            }
            /*if (e != null)
            {
                switch (methodname)
                {
                    case "ManiaPlanet.BeginRound":
                    {

                        MapArgument param = new MapArgument();
                        param.ArgEnviro = "tm";
                        this._widgetMxListFrame = this._serverManager.ReadText(Name, 0, "Widget_MXList_Frame");
                        this.RefreshTime = this.Now + 0x1388;
                        string newValue = "";
                        int num = 0;
                        string str3 = this._widgetMxListFrame;
                        foreach (MapInfo info in this._myManiaExchange.ToResults(this._myManiaExchange.Request(MxapiType.Site, param)))
                        {
                            str3 = str3.Replace("(->)[mx]Map1()", "[mx]Map_" + num);
                            if ((num % 2) == 0)
                            {
                                str3 = str3.Replace("(->)color", "C2C2C2CA");
                            }
                            else
                            {
                                str3 = str3.Replace("(->)color", "D4DD4D4CA");
                            }
                            str3 = str3.Replace("((0))", (num * 10).ToString()).Replace("MXMAPNAME", info.Name).Replace("MXMAPAUTHOR", info.Username);

                            newValue = newValue + str3;
                            str3 = this._widgetMxListFrame;
                            num++;
                        }
                        this._streamrod = this._serverManager.ReadText(Name, 0, "Widget");
                        this._streamrod = this._streamrod.Replace("MapName", this._serverManager.GetCurrentMapInfo.Name);
                        this._streamrod = this._streamrod.Replace("MapAuthor", this._serverManager.GetCurrentMapInfo.Author);
                        this._streamrod = this._streamrod.Replace("<!-- MX_LIST -->", newValue);
                        this._serverManager.AddThisManialink(this._streamrod, "MapInformationPL", true);
                        ServerManager.Server.ChatSendServerMessage(("hello"));
                        break;
                    }
                }
            }*/
        }

        public override void HandleOnConnectionSuccessful()
        {
            foreach (var server in ServerManager.Servers.Where(server => server.IsConnected))
            {
                server.OnPlayerChat +=
                new ShootManiaServer.OnPlayerChatEventHandler(HandleOnPlayerChat);
                server.OnPlayerConnect +=
                    new ShootManiaServer.OnPlayerConnectEventHandler(HandleOnPlayerConnect);
            }
        }

        public override void HandleOnPlayerChat(PlayerChat pc)
        {
            if (pc.Text != "")
            {
                bool flag1 = pc.Login != "guerro";
            }
        }

        public override void HandleOnPlayerConnect(PlayerConnect pc)
        {
        }

        public override void OnConsoleCommand(string command)
        {
            bool flag1 = command == "reload_ml";
            if (command.StartsWith("addmap"))
            {
                string s = command.Replace("addmap", string.Empty);
                ServerManager.Server.ChatSendServerMessage(AyO.ChatPrefix +
                                                           " [$29fMANIA-EXCHANGE$fff] Adding map : $999" + s);
                ServerManager.AddMxMap(_myManiaExchange, int.Parse(s));
            }
        }

        public override void OnEverythingLoaded()
        {
            OnUpdateInterface("");
        }

        public override void OnLoop()
        {
            /*if ((this.RefreshTime < this.Now) && !this._serverManager.GetCurrentMapInfo.Null)
            {
                this._widgetMxListFrame = this._serverManager.ReadText(this.Name, 0, "Widget_MXList_Frame");
                this.RefreshTime = this.Now + 0x1388;
                string text1 = this._widgetMxListFrame;
                this._streamrod = this._serverManager.ReadText(this.Name, 0, "Widget");
                this._streamrod = this._streamrod.Replace("MapName", this._serverManager.GetCurrentMapInfo.Name);
                this._streamrod = this._streamrod.Replace("MapAuthor", this._serverManager.GetCurrentMapInfo.Author);
                this._serverManager.AddThisManialink(this._streamrod, "MapInformationPL", true);
            }
            if ((this._serverManager.GetCurrentMapInfo.UId != null) && (this._mapUid != this._serverManager.GetCurrentMapInfo.UId))
            {
                this._mapUid = this._serverManager.GetCurrentMapInfo.UId;
            }*/
            Now = Environment.TickCount;
        }

        public override void OnServerManagerInitialize(ServerManager serverManager)
        {
            ServerManager = serverManager;
            ServerManager.OnConnectionSuccessful +=
                new ServerManager.OnConnectionSuccessfulEventHandler(HandleOnConnectionSuccessful);
            _streamrod = serverManager.ReadText(Name, 0, "Widget");
            _widgetMxListFrame = serverManager.ReadText(Name, 0, "Widget_MXList_Frame");
        }

        public override void Unload()
        {
            ServerManager.Server.OnPlayerChat -= new ShootManiaServer.OnPlayerChatEventHandler(HandleOnPlayerChat);
            ServerManager.Server.OnPlayerConnect -=
                new ShootManiaServer.OnPlayerConnectEventHandler(HandleOnPlayerConnect);
            ServerManager.OnConnectionSuccessful -=
                new ServerManager.OnConnectionSuccessfulEventHandler(HandleOnConnectionSuccessful);
        }

        public override string Author =>
            "Guerro";

        public override AyO.HelpCommands ListofCommands =>
            new AyO.HelpCommands()
            {
                Path = "/map",
                Commands = new[]
                {
                    new AyO._Commands {Command = "map"},
                    new AyO._Commands {Command = "mx"}
                }
            };



        public override string Name =>
            "Map_Information";

        public override AyO.PluginFunction[] PluginFunction =>
            new[] {AyO.PluginFunction.MapList};

        public override string Version =>
            "0.1";
    }
}
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       