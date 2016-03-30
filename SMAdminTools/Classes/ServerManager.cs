using System;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Text;
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

        public cAdmins Admins = new cAdmins();

        public string[][] Commands;

        public ServerManager(Config Config)
        {
            this.Config = Config;
            this.Server = new ShootManiaServer(this.Config.ShootMania__IP, this.Config.ShootMania__XML_RPC_Port);
        }

        public bool AddMXMap(ManiaExchange MX, int Name)
        {
            if (MX != null && Name != 0)
            {
                MX.AddMap(Name);
                if (File.Exists("Plugins/misc/mx/" + Name + ".Map.Gbx"))
                {
                    var booleable = Server.WriteFile("Ayo_MXMAPS/" + Name.ToString() + ".Map.Gbx", File.ReadAllText("Plugins/misc/mx/" + Name.ToString() + ".Map.Gbx"));
                    Server.InsertMap("Ayo_MXMAPS/" + Name.ToString() + ".Map.Gbx");
                    return booleable;
                }
                else return true;
            }
            return false;
        }

        public ShootManiaXMLRPC.Structs.PlayerList GetPlayer(object ID)
        {
            ShootManiaXMLRPC.Structs.PlayerList nullplayer = null; //< Because :/.

            foreach (ShootManiaXMLRPC.Structs.PlayerList player in Server.GetPlayerList(100, 0))
            {
                if (player != null)
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
            }

            return nullplayer;
        }

        static public void CreateNewFile(string pluginName, string name, string contents, Action AfterJson)
        {
            bool exists = Directory.Exists("Plugins/misc/" + pluginName);

            if (!exists)
                Directory.CreateDirectory("Plugins/misc/" + pluginName);

            File.WriteAllText("Plugins/misc/" + pluginName + "/" + name, contents);

            /*if (File.Exists("Plugins/misc/" + pluginName + "/" + name))
            {
                AfterJson();
                File.WriteAllText("Plugins/misc/" + pluginName + "/" + name, contents);
            }
            else
            {
                File.WriteAllText("Plugins/misc/" + pluginName + "/" + name, contents);
            }*/
        }



        public Dictionary<Dictionary<string, string>, cManialink> Manialinks = new Dictionary<Dictionary<string, string>, cManialink>();

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

        public ShootManiaXMLRPC.Structs.PlayerList[] GetPlayers()
        {
            List<ShootManiaXMLRPC.Structs.PlayerList> listPlayers = new List<ShootManiaXMLRPC.Structs.PlayerList>();
            foreach (var Player in Server.GetPlayerList(100, 0))
            {
                if (Player != null)
                {
                    listPlayers.Add(Player);
                }
            }
            return listPlayers.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pluginName">Name of the plugin</param>
        /// <param name="Type">Type of the file ---> 
        /// [0: xml, 
        /// 1: misc]</param>
        /// <param name="ToRead"></param>
        /// <returns></returns>
        public string ReadText(string pluginName, int Type, string ToRead)
        {
            if (Type == 0) {
                if (!File.Exists("Plugins/" + pluginName + "_xmlfiles/" + ToRead + ".xml")) return "The file don't exist";
                else return File.ReadAllText("Plugins/" + pluginName + "_xmlfiles/" + ToRead + ".xml");
            }
            if (Type == 1)
            {
                if (!File.Exists("Plugins/misc/" + pluginName + "/" + ToRead + ".misc")) return "The file don't exist";
                else return File.ReadAllText("Plugins/misc/" + pluginName + "/" + ToRead + ".misc");
            }
            return "";
        }

        public void AddThisManialink(string playerName, string _Manialink, string _Name, bool Refresh)
        {
            // Ajout
            var AlreadyExist = false;
            if (playerName == "" || playerName == null)
            {
                foreach (var Player in GetPlayers())
                {
                    Dictionary<string, string> Index = new Dictionary<string, string>();
                    Dictionary<string, string> iPlayer = new Dictionary<string, string>();
                    iPlayer[Player.Login] = _Name;
                    Console.WriteLine("Adding : " + _Name + ",  for : " + Player.Login);
                    if (Manialinks.Count > 0)
                    {
                        foreach (var Ml in Manialinks)
                        {
                            if (Ml.Value.Name == _Name || Refresh)
                            {
                                if (Ml.Value.Name == _Name)
                                {
                                    Index = Ml.Key;
                                    Ml.Value.Manialink = _Manialink;//< refresh?
                                    Console.WriteLine("removing " + Ml.Value.Name);
                                    AlreadyExist = false;

                                    if (Refresh)
                                    {
                                        Index = null;
                                        AlreadyExist = true;
                                        Ml.Value.Manialink = _Manialink;
                                    }
                                }
                            }
                        }
                    }
                    if (Index != null) Manialinks.Remove(Index);
                    if (!AlreadyExist) Manialinks.Add(iPlayer, new cManialink(_Manialink, _Name));
                    // Organisation
                    var FinalManialink = "";
                    FinalManialink += "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\" ?>";
                    foreach (var Ml in Manialinks)
                    {
                        Console.WriteLine(Ml.Value.Name);
                        FinalManialink += "\n<manialink name=\"" + Ml.Value.Name + "\" version=\"2\">\n";
                        FinalManialink += Ml.Value.Manialink;
                        FinalManialink += "\n</manialink>";
                    }
                    Server.SendManialink(Player.Login, FinalManialink, 0, false);
                }
            }
            else {
                Dictionary<string, string> Index = new Dictionary<string, string>();
                Dictionary<string, string> Player = new Dictionary<string, string>();
                Player[playerName] = _Name;
                Console.WriteLine("Adding : " + _Name + ",  for : " + playerName);
                if (Manialinks.Count > 0)
                {
                    foreach (var Ml in Manialinks)
                    {
                        if (Ml.Value.Name == _Name || Refresh)
                        {
                            if (Ml.Value.Name == _Name)
                            {
                                Index = Ml.Key;
                                Ml.Value.Manialink = _Manialink;//< refresh?
                                Console.WriteLine("removing " + Ml.Value.Name);
                                AlreadyExist = false;

                                if (Refresh)
                                {
                                    Index = null;
                                    AlreadyExist = true;
                                    Ml.Value.Manialink = _Manialink;
                                }
                            }
                        }
                    }
                }
                if (Index != null) Manialinks.Remove(Index);
                if (!AlreadyExist) Manialinks.Add(Player, new cManialink(_Manialink, _Name));
                // Organisation
                var FinalManialink = "";
                FinalManialink += "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\" ?>";
                foreach (var Ml in Manialinks)
                {
                    Console.WriteLine(Ml.Value.Name);
                    FinalManialink += "\n<manialink name=\"" + Ml.Value.Name + "\" version=\"2\">\n";
                    FinalManialink += Ml.Value.Manialink;
                    FinalManialink += "\n</manialink>";
                }
                Server.SendManialink(playerName, FinalManialink, 0, false);
            }
        }

        public void RemoveThisManialink(string playerName, string _Name)
        {
            foreach (var Ml in Manialinks)
            {
                if (Ml.Value.Name == _Name)
                {
                    Manialinks.Remove(Ml.Key);
                }
            }
            AddThisManialink(playerName, "", "", true);
        }

        public int RefreshTime;
        public int Now;

        private string Widget_Setting;

        private void HandleConnection()
        {
            ManialinkSystem ManiaLinkTest = new ManialinkSystem();
            /*Quad MyQuad = new Quad("test", Vector3.Zero, Vector3.Zero, false);
            Quad SecondQuad = new Quad("", new Vector3(0, 0, 4), new Vector3(10, 10, 10), false);
            Label MyLabel = new Label("", Vector3.Zero, Vector3.Zero, false);
            SecondQuad.style = "Bgs1";
            SecondQuad.substyle = "BgGlow2";
            SecondQuad.Halign = ManialinkSystem.Halign.Center;
            MyLabel.text = "MANIALINK GENERATOR :D";
            ManiaLinkTest.Add(MyQuad, true);
            ManiaLinkTest.Add(SecondQuad, true);
            ManiaLinkTest.Add(MyLabel, true);*/
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
                            Server.ChatSendServerMessage("$999AY$fffo $f00» $fffLoading KEK . . .");

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
                            Server.ChatSendServerMessage("$999AY$fffo $f60» $fffPlease wait . . .");

                            // Loads command
                            int rankCommand = 1;
                            Commands = new String[Plugins.Manager.LoadedPlugins.Count][];
                            foreach (Plugin Pl in Plugins.Manager.LoadedPlugins)
                            {
                                Commands[rankCommand - 1] = Pl.listofCommands;
                                rankCommand++;
                            }

                            Console.WriteLine("Everythings is up and running !");
                            Server.ChatSendServerMessage("$999AY$fffo $ff0» $0f0Sucessfully loaded!");

                            cAdmins.cPermissions MyGroup = Admins.AddGroup("Admins");
                            MyGroup.AddPlayer("guerro");

                            Widget_Setting = ReadText("serverManager", 0, "widget_settings");
                            Server.SetTATime(-1);

                            Dictionary<string, bool> RemoveLegacy = new Dictionary<string, bool>();
                            RemoveLegacy.Add("Settings", false);

                            Console.WriteLine(ManiaLinkTest.CurrentBuild);
                            AddThisManialink("", ManiaLinkTest.CurrentBuild, "ololololol", true);

                            // Loop
                            while (true)
                            {
                                foreach (Plugins.Plugin plugin in Plugins.Manager.LoadedPlugins)
                                {
                                    plugin.OnLoop();
                                    if (plugin.PluginFunction == AyO.PluginFunction.Settings) RemoveLegacy["Settings"] = true;
                                }
                                // manialink of ayo
                                if (RefreshTime < Now)
                                {
                                    RefreshTime = Now + 1000;
                                    if (!RemoveLegacy["Settings"]) AddThisManialink("", Widget_Setting, "[AYO]Widget_Settings", true);
                                    else
                                    {
                                        foreach (var Player in GetPlayers())
                                        {
                                            Dictionary<string, string> Tempkey = new Dictionary<string, string>();
                                            Tempkey[Player.Login] = "[AYO]Widget_Settings";
                                            if (Manialinks.ContainsKey(Tempkey))
                                            {
                                                RemoveThisManialink(Player.Login, "[AYO]Widget_Settings");
                                            }
                                        }
                                    }
                                }
                                Now = Environment.TickCount;
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
            Server.ChatSendServerMessage("$fff" + PC.Login + " $z$> $f00» $i$fffleft the server!");
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
            Server.ChatSendServerMessage("$fff" + Admins.GetGroupForLogin(PC.Login) + " : " + GetPlayer(PC.Login).Nickname + " $z$> $0f0» $i$fffjoined the server!");

        }

        public class cAdmins
        {
            public class cPermissions
            {
                public string Name = "";
                public List<string> playersIn = new List<string>();
                public Dictionary<string, bool> currentPermissions = new Dictionary<string, bool>();

                public cPermissions(string _Name, Dictionary<string, bool> _Permissions)
                {
                    Name = _Name;
                    currentPermissions = _Permissions;
                }

                public void AddPlayer(string Login)
                {
                    playersIn.Add(Login);
                }
            }

            public List<cPermissions> currentGroups = new List<cPermissions>();

            public string GetGroupForLogin(string Login)
            {
                foreach (var Group in this.currentGroups)
                {
                    if (Group != null)
                    {
                        foreach (var player in Group.playersIn)
                        {
                            if (Login == player)
                            {
                                return Group.Name;
                            }
                        }
                    }
                }
                return "Player";
            }

            public cPermissions GetGroup(string Name)
            {
                foreach (var Group in this.currentGroups)
                {
                    if (Group.Name == Name)
                    {
                        return Group;
                    }
                }
                return null;
            }

            public cPermissions AddGroup(string Name)
            {
                currentGroups.Add(new cPermissions(Name, new Dictionary<string, bool>()));
                return GetGroup(Name);
            }
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

