using System;
using System.Threading;
using System.Collections.Generic;
using ShootManiaXMLRPC;
using AyoController.Plugins;

namespace AyoController.Classes
{
    public class ServerManager
    {

        public Config Config { get; private set; }
        public ShootManiaServer Server { get; private set; }
        private Thread Thread_HandleConnection;

        public delegate void OnConnectionSuccessfulEventHandler();
        public event OnConnectionSuccessfulEventHandler OnConnectionSuccessful;

        public string[][] Commands;

        public ServerManager(Config Config)
        {
            this.Config = Config;
            this.Server = new ShootManiaServer(this.Config.ShootMania__IP, this.Config.ShootMania__XML_RPC_Port);
        }

        public ShootManiaXMLRPC.Structs.PlayerList GetPlayer(object ID)
        {
            ShootManiaXMLRPC.Structs.PlayerList nullplayer = null; //< Because :/.

            foreach (ShootManiaXMLRPC.Structs.PlayerList player in Server.GetPlayerList(100, 0))
            {
                if (ID is int)
                {
                    if (player.PlayerId == (int)ID) return player;
                }
                else if (ID is string)
                {
                    if (player.Login == (string)ID) return player;
                }
            }

            return nullplayer;
        }

        public List<cManialink> Manialinks = new List<cManialink>();

        public class cManialink
        {
            public string Manialink;
            public string Name;

            public cManialink(string _Ml, string _Na)
            {
                Manialink = _Ml;
                Name = _Na;
            }
        }

        public void AddThisManialink(string playerName, string _Manialink, string _Name, bool Refresh)
        {
            // Ajout
            var AlreadyExist = false;
            if (Manialinks.Count > 0)
            {
                foreach (var Ml in Manialinks)
                {
                    if (Ml.Name == _Name || Refresh)
                    {
                        AlreadyExist = true;//< already exist
                        Ml.Manialink = _Manialink;//< refresh?
                    }
                }
            }
            if (!AlreadyExist) Manialinks.Add(new cManialink(_Manialink, _Name));
            // Organisation
            var FinalManialink = "";
            FinalManialink += "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\" ?>";
            foreach (var Ml in Manialinks)
            {
                FinalManialink += "\n<manialink version=\"2\">\n";
                FinalManialink += Ml.Manialink;
                FinalManialink += "\n</manialink>";
            }
            Server.SendManialink(playerName, FinalManialink, 0, false);
        }

        private void HandleConnection()
        {
            while (true)
            {

                if (!Server.IsConnected)
                {

                    this.Server = new ShootManiaServer(this.Config.ShootMania__IP, this.Config.ShootMania__XML_RPC_Port);

                    if (Server.Connect() == 0)
                    {

                        Console.WriteLine("Connected to server !");
                        Console.WriteLine("Authentication ...");

                        if (Server.Authenticate(Config.ShootMania__SuperAdmin_Login, Config.ShootMania__SuperAdmin_Password))
                        {

                            Console.WriteLine("Authentication success !");
                            Server.ChatSendServerMessage("$999AY$fffo $f00� $fffLoading . . .");

                            Console.WriteLine("Set API version : " + Settings.ShootManiaApiVersion + " ...");
                            Server.SetApiVersion(Settings.ShootManiaApiVersion);
                            Console.WriteLine("Ok ...");

                            Console.WriteLine("Enable callbacks ...");
                            Server.EnableCallback();
                            Console.WriteLine("Ok ...");

                            Console.WriteLine("Register events ...");
                            Server.Client.EventGbxCallback += HandleEventGbxCallback;
                            Server.OnPlayerConnect += HandleOnPlayerConnect;
                            Server.OnPlayerDisconnect += HandleOnPlayerDisconnect;
                            Console.WriteLine("Ok ...");

                            Console.WriteLine("Calling OnConnectionSuccessful ...");

                            if (OnConnectionSuccessful != null)
                                OnConnectionSuccessful();

                            Console.WriteLine("Ok ...");
                            Server.ChatSendServerMessage("$999AY$fffo $f60� $fffPlease wait . . .");

                            // Loads command
                            int rankCommand = 1;
                            Commands = new String[Plugins.Manager.LoadedPlugins.Count][];
                            foreach (Plugin Pl in Plugins.Manager.LoadedPlugins)
                            {
                                Commands[rankCommand - 1] = Pl.listofCommands;
                                rankCommand++;
                            }

                            foreach (ShootManiaXMLRPC.Structs.PlayerList Player in Server.GetPlayerList(5, 0))
                            {
                            }

                            Console.WriteLine("Everythings is up and running !");
                            Server.ChatSendServerMessage("$999AY$fffo $ff0� $0f0Sucessfully loaded!");

                            // Loop
                            while (true)
                            {
                                foreach (Plugins.Plugin plugin in Plugins.Manager.LoadedPlugins)
                                {
                                    plugin.OnLoop();
                                }
                            }

                        }
                        else
                        {
                            Console.WriteLine("Authentication failed ...");
                            Server.Disconnect();
                        }

                    }
                    else
                    {
                        Console.WriteLine("Unable to connect to server " + Config.ShootMania__IP + ":" + Config.ShootMania__XML_RPC_Port + " ...");
                    }

                }

                if (!Server.IsConnected)
                    Console.WriteLine("Retry in " + Config.ShootMania__ReconnectTimeout + "ms ...");

                Thread.Sleep(Config.ShootMania__ReconnectTimeout);
            }
        }

        void HandleEventGbxCallback(object o, ShootManiaXMLRPC.XmlRpc.GbxCallbackEventArgs e)
        {
            foreach (Plugins.Plugin plugin in Plugins.Manager.LoadedPlugins)
            {
                plugin.HandleEventGbxCallback(o, e);
            }
            if (Config.AyoController__Debug)
            {
                if (e.Response.MethodName == "ManiaPlanet.PlayerChat") return;
                Console.WriteLine("[DEBUG] Callback received : " + e.Response.MethodName + " (Params : " + e.Response.Params.Count + ")");
                for (int I = 0; I < e.Response.Params.Count; I++)
                {
                    Server.ChatSendServerMessage(e.Response.Params[I].ToString());
                }
            }

            if (e.Response.MethodName == "ManiaPlanet.ModeScriptCallback")
            {
                //Console.WriteLine(e.Response.Params[0].ToString() + " - " + e.Response.Params[1].ToString());
            }

        }

        void HandleOnPlayerDisconnect(ShootManiaXMLRPC.Structs.PlayerDisconnect PC)
        {
            Console.WriteLine("Player [" + PC.Login + "] disconnected");
        }

        void HandleOnPlayerConnect(ShootManiaXMLRPC.Structs.PlayerConnect PC)
        {

            string PlayerConnectText = "Player [" + PC.Login + "] connected";

            if (PC.IsSpectator)
            {
                PlayerConnectText += " (Spectator)";
            }

            Console.WriteLine(PlayerConnectText);

            Server.SendNoticeToLogin(PC.Login, "$fffWelcome to you " + PC.Login + "! $0f0:)");
            Server.ChatSendServerMessage("$fff" + PC.Login + " $z$> $999� $i$fffjoined the server!");

            Server.SetServerName("I'm on a server!");

        }

        public void Initialize()
        {

            foreach (Plugins.Plugin plugin in Plugins.Manager.LoadedPlugins)
            {
                plugin.OnServerManagerInitialize(this);
            }

            Thread_HandleConnection = new Thread(HandleConnection);
            Thread_HandleConnection.IsBackground = true;
            Thread_HandleConnection.Start();

        }


    }
}
