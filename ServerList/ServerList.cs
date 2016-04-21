using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AyoController;
using AyoController.Plugins;
using AyoController.Classes;
using ShootManiaXMLRPC;
using ShootManiaXMLRPC.Structs;
using ShootManiaXMLRPC.XmlRpc;

namespace ServersList
{
    public class ServerList : Plugin
    {
        public override string Name
        {
            get { return "ServerList"; }
        }

        public override string Author
        {
            get { return "Guerro"; }
        }

        public override string Version
        {
            get { return "0.1"; }
        }

        public override AyoController.Classes.ServerManager ServerManager { get; set; }
        string streamrod;

        public override AyO.HelpCommands ListofCommands
        {
            get
            {
                return new AyO.HelpCommands()
                {
                    Path = "/servers",
                    Commands = new[]
                    {
                        new AyO._Commands {Command = "show", Params = new []{ "TM" }},
                    }
                };
            }
        }

        public override AyO.PluginFunction[] PluginFunction
        {
            get
            {
                return new AyO.PluginFunction[] {
                    AyO.PluginFunction.Nothing
                };
            }
            set
            {
                base.PluginFunction = value;
            }
        }

        public override void OnServerManagerInitialize(AyoController.Classes.ServerManager serverManager)
        {

            this.ServerManager = serverManager;
            this.ServerManager.OnConnectionSuccessful += HandleOnConnectionSuccessful;
        }

        public override void OnLoop()
        {

        }

        public void OnUpdateInterface(string login)
        {
            streamrod = ServerManager.ReadText(Name, 0, "Widget");
            foreach (var server in ServerManager.Servers.Where(server => server.IsConnected))
            {
                streamrod = streamrod.Replace("ServerName", server.GetServerName());
                streamrod = streamrod.Replace("ServerLogin", server.GetPlayerList(255, 0).Count(player => player.Login != server.GetLogin()) + "/" + (server.GetMaxPlayers().CurrentValue + server.GetMaxSpectators().CurrentValue));
                var rank = 0;
                var affServerList = "";
                foreach (var aserver in ServerManager.Servers.Where(aserver => aserver.IsConnected && aserver != server && aserver.GetLogin() != server.GetLogin()))
                {
                    var tempList = ServerManager.ReadText(Name, 0, "Widget_ServerList_Frame");
                    tempList = tempList.Replace("(->)[mx]Map1()", "[mx]Map_" + rank);
                    if ((rank % 2) == 0) tempList = tempList.Replace("(->)color", "C2C2C2CA");
                    else tempList = tempList.Replace("(->)color", "D4DD4D4CA");
                    tempList = tempList.Replace("((0))", "" + -(rank * 16));
                    tempList = tempList.Replace("MXMAPNAME", aserver.FriendlyName);
                    tempList = tempList.Replace("MXMAPAUTHOR", aserver.GetPlayerList(255, 0).Count(player => player.Login != aserver.GetLogin()) + "/" + (aserver.GetMaxPlayers().CurrentValue + aserver.GetMaxSpectators().CurrentValue));
                    tempList = tempList.Replace("MXMAPFOR", "$fff$h[#join=" + aserver.GetLogin() + "]JOIN THIS SERVER");
                    affServerList += tempList;
                    rank++;
                }
                streamrod = streamrod.Replace("<!-- SERVER_LIST -->", affServerList);
                streamrod = streamrod.Replace("((y))", ((ServerManager.Servers.Count+1)/4).ToString());
                if (!string.IsNullOrEmpty(login)) ServerManager.AddThisManialink(login, streamrod, "ServerList", true, server);
                else ServerManager.AddThisManialink(streamrod, "ServerList", true, server);
            }
        }

        public override void OnCustomEvent(object[] responseObjects, string eventName)
        {
            switch (eventName)
            {
                case "OnLoadServerSuccess":
                {
                    OnUpdateInterface("");
                    break;
                }
                case "OnRemoveServer":
                {
                    OnUpdateInterface("");
                    break;
                }
            }
        }

        public override void OnEverythingLoaded()
        {
            base.OnEverythingLoaded();
            OnUpdateInterface("");
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
            streamrod = ServerManager.ReadText(Name, 0, "Widget");
            ServerManager.AddThisManialink(streamrod, "ServerList", true);
        }

        public override void HandleOnPlayerChat(PlayerChat pc)
        {

        }

        public override void HandleOnPlayerConnect(PlayerConnect pc)
        {
            OnUpdateInterface(pc.Login);
        }

        public override void HandleFixedGbxCallBacks(GbxCallbackEventArgs reponse, string methodname, ShootManiaServer maniaServer)
        {

        }
    }
}
