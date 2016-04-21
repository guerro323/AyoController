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

namespace CustomChat
{
    public class CustomChat : Plugin
    {
        public override string Name
        {
            get { return "CustomChat"; }
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

        public override AyO.HelpCommands ListofCommands
        {
            get
            {
                return new AyO.HelpCommands()
                {
                    Path = "/chat",
                    Commands = new[]
                    {
                        new AyO._Commands {Command = "enable", Params = new []{ "True" }},
                    }
                };
            }
        }

        public override AyO.PluginFunction[] PluginFunction
        {
            get
            {
                return new AyO.PluginFunction[] {
                    AyO.PluginFunction.Global
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

        public override void OnLoad()
        {

        }

        public override void OnLoop()
        {

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
           if (pc.Login != pc.Server.GetLogin() && !pc.IsRegisteredCmd)
                pc.Server.ChatSendServerMessage(ServerManager.GetPlayer(pc.Login).Nickname + "$z$s$>$z$s $999» $fff" + pc.Text);
        }

        public override void HandleOnPlayerConnect(PlayerConnect pc)
        {

        }

        public override void HandleFixedGbxCallBacks(GbxCallbackEventArgs reponse, string methodname, ShootManiaServer maniaServer)
        {

        }
    }
}
