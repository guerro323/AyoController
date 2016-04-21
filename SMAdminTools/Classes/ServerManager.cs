using AyoController.Plugins;
using ShootManiaXMLRPC;
using ShootManiaXMLRPC.Structs;
using ShootManiaXMLRPC.XmlRpc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using LitJson;

namespace AyoController.Classes
{
    class Product
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public string Category { get; set; }
    }

    class Program
    {

        public async Task RunAsync()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:54089/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // HTTP GET
                HttpResponseMessage response = await client.GetAsync("api/products/1");
                if (response.IsSuccessStatusCode)
                {
                    Product product = await response.Content.ReadAsAsync<Product>();
                    Console.WriteLine("{0}\t${1}\t{2}", product.Name, product.Price, product.Category);
                }

                // HTTP POST
                var gizmo = new Product() {Name = "Gizmo", Price = 100, Category = "Widget"};
                response = await client.PostAsJsonAsync("api/products", gizmo);
                if (response.IsSuccessStatusCode)
                {
                    Uri gizmoUrl = response.Headers.Location;

                    // HTTP PUT
                    gizmo.Price = 80; // Update price
                    response = await client.PutAsJsonAsync(gizmoUrl, gizmo);

                    // HTTP DELETE
                    response = await client.DeleteAsync(gizmoUrl);
                }
            }
        }
    }

    public class ServerManager
    {
        public delegate void OnConnectionSuccessfulEventHandler();

        // private variables
        private CurrentMapInfo _currentMapInfo = new CurrentMapInfo() {Null = true};

        /// <summary>
        /// Basic class for creating manialinks
        /// </summary>
        public class CManialink
        {
            public string Manialink;

            public string Name;

            public string ToLogin;

            public CManialink(string ml, string na, string login)
            {
                this.Manialink = ml;
                this.Name = na;
                this.ToLogin = login;
            }
        }

        public enum ParamEnum
        {
            ShowSetting,
            ShowPluginsList,
            ShowPluginInfo,
            ShowPluginSettings
        }

        /// <summary>
        /// Basic class for the creation or modification of groups.
        /// </summary>
        public class CAdmins
        {
            public class CPermissions
            {
                public string Name = "";

                public List<string> PlayersIn = new List<string>();

                public Dictionary<string, bool> CurrentPermissions = new Dictionary<string, bool>();

                public CPermissions(string name, Dictionary<string, bool> permissions)
                {
                    this.Name = name;
                    this.CurrentPermissions = permissions;
                }

                public void AddPlayer(string login)
                {
                    this.PlayersIn.Add(login);
                }
            }

            public List<CPermissions> CurrentGroups = new List<CPermissions>();

            public string GetGroupForLogin(string login)
            {
                foreach (CPermissions current in this.CurrentGroups)
                {
                    if (current != null)
                    {
                        foreach (string current2 in current.PlayersIn)
                        {
                            if (login == current2)
                            {
                                return current.Name;
                            }
                        }
                    }
                }
                return "Player";
            }

            public ServerManager.CAdmins.CPermissions GetGroup(string name)
            {
                foreach (ServerManager.CAdmins.CPermissions current in this.CurrentGroups)
                {
                    if (current.Name == name)
                    {
                        return current;
                    }
                }
                return null;
            }

            public ServerManager.CAdmins.CPermissions AddGroup(string name)
            {
                this.CurrentGroups.Add(new ServerManager.CAdmins.CPermissions(name, new Dictionary<string, bool>()));
                return this.GetGroup(name);
            }
        }

        private Thread _threadHandleConnection;

        public ServerManager.CAdmins Admins = new ServerManager.CAdmins();

        /// <summary>
        /// Current list of commands
        /// <param>The plugin name
        ///     <name>string</name>
        /// </param>
        /// <param>Commands
        ///     <name>AyO.HelpCommands</name>
        /// </param>
        /// </summary>
        public Dictionary<string, AyO.HelpCommands> Commands;

        public bool PluginsNotLoaded;

        private readonly Dictionary<string, bool> _removeLegacy = new Dictionary<string, bool>();

        public List<Plugin> LoadedPlugins = new List<Plugin>();

        public List<FileInfo> Dlls = new List<FileInfo>();

        public Dictionary<Dictionary<string, string>, ServerManager.CManialink> Manialinks =
            new Dictionary<Dictionary<string, string>, ServerManager.CManialink>();

        public int RefreshTime;

        public int Now;

        private string _widgetSetting;

        [method: CompilerGenerated]
        [CompilerGenerated]
        public event ServerManager.OnConnectionSuccessfulEventHandler OnConnectionSuccessful;

        public Config Config { get; private set; }

        public ShootManiaServer Server { get; private set; }

        /// <summary>
        /// This is useful when you run a lot of servers on the same application.
        /// Some plugins don't use this variable, so launching another application is more safe.
        /// </summary>
        public List<ShootManiaServer> Servers;


        public ServerManager(Config config)
        {
            this.Config = config;
            this.Server = new ShootManiaServer(this.Config.ShootManiaIp, this.Config.ShootManiaXmlRpcPort);
            this.Servers = new List<ShootManiaServer>();
            Servers.Add(this.Server);
        }

        public void UnloadPlugins()
        {
            Console.WriteLine("Unloading all plugins...");
            using (List<Plugin>.Enumerator enumerator = this.LoadedPlugins.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Unload();
                }
            }
            this.Dlls = new List<FileInfo>();
            this.LoadedPlugins = new List<Plugin>();
            this.PluginsNotLoaded = true;
        }

        public void LoadPlugins()
        {
            if (!Directory.Exists("Plugins"))
            {
                Console.WriteLine("Creating the Plugins folder.");
                Directory.CreateDirectory("Plugins");
            }
            FileInfo[] files = new DirectoryInfo("Plugins").GetFiles();
            for (int i = 0; i < files.Length; i++)
            {
                FileInfo fileInfo = files[i];
                if (fileInfo.Extension.ToLower() == ".dll")
                {
                    this.Dlls.Add(fileInfo);
                }
            }
            List<Plugin> list = new List<Plugin>();
            this.PluginsNotLoaded = true;
            foreach (FileInfo current in this.Dlls)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFile(current.FullName);
                    AyO.print(true, "Plugin found... ||| Assembly Name : " + assembly.GetName(), ConsoleColor.Gray);
                    Type[] types = assembly.GetTypes();
                    for (int i = 0; i < types.Length; i++)
                    {
                        Type type = types[i];
                        if (type.BaseType == typeof (Plugin))
                        {
                            Plugin plugin = (Plugin) Activator.CreateInstance(type);
                            AyO.print(true, "   Name    :   " + plugin.Name, ConsoleColor.Green);
                            AyO.print(true, "   Author  :   " + plugin.Author, ConsoleColor.White);
                            AyO.print(true, "   Version :   " + plugin.Version, ConsoleColor.Green);
                            plugin.Loaded = true;
                            plugin.OnLoad();
                            this.InitializePlugin(plugin);
                            list.Add(plugin);
                        }
                    }
                }
                catch
                {
                }
            }
            this.LoadedPlugins = list;
            this.PluginsNotLoaded = false;
            int num = 1;
            this.Commands = new Dictionary<string, AyO.HelpCommands>();
            foreach (Plugin current2 in this.LoadedPlugins)
            {
                if (current2.ListofCommands.Any())
                {
                    this.Commands[current2.Name] = current2.ListofCommands;
                    num++;
                    current2.OnEverythingLoaded();
                }
            }
        }

        public string GetMlAnimLib
        {
            get
            {
                var streamrod = ReadText("ManialinkLib", 1, "mlAnimlib");
                return streamrod;
            }
        }

        public string GetRelayLib
        {
            get
            {
                var streamrod = ReadText("ManialinkLib", 1, "mlRelaylib");
                return streamrod;
            }
        }


        public bool AddMxMap(ManiaExchange mx, int name)
        {
            if (mx == null || name == 0)
            {
                return false;
            }
            mx.AddMap(name);
            if (!File.Exists("Plugins/misc/mx/" + name + ".Map.Gbx")) return false;
            bool boolReturn = this.Server.WriteFile("Ayo_MXMAPS/" + name.ToString() + ".Map.Gbx",
                File.ReadAllText("Plugins/misc/mx/" + name.ToString() + ".Map.Gbx"));
            this.Server.InsertMap("Ayo_MXMAPS/" + name.ToString() + ".Map.Gbx");
            return boolReturn;
        }

        public PlayerList GetPlayer(object id)
        {
            PlayerList result = new PlayerList();
            foreach (var server in Servers)
            {
                if (!server.IsConnected) continue;
                foreach (PlayerList current in server.GetPlayerList(100, 0))
                {
                    if (!current.Null)
                    {
                        if (id is int)
                        {
                            if (current.PlayerId == (int) id)
                            {
                                PlayerList result2 = current;
                                return result2;
                            }
                        }
                        else if (id is string && current.Login == (string) id)
                        {
                            PlayerList result2 = current;
                            return result2;
                        }
                    }
                }
            }
            return result;
        }

        public static void CreateNewFile(string pluginName, string name, string contents, Action afterJson)
        {
            if (!Directory.Exists("Plugins/misc/" + pluginName))
            {
                Directory.CreateDirectory("Plugins/misc/" + pluginName);
            }
            File.WriteAllText("Plugins/misc/" + pluginName + "/" + name, contents);
        }

        public void Instance_CreateNewFile(string plname, string n, string c, Action aj)
        {
            if (!Directory.Exists("Plugins/misc/" + plname))
            {
                Directory.CreateDirectory("Plugins/misc/" + plname);
            }
            File.WriteAllText("Plugins/misc/" + plname + "/" + n, c);
        }

        public PlayerList[] GetPlayers()
        {
            List<PlayerList> list = new List<PlayerList>();
            if (!this.Server.IsConnected) return list.ToArray();
            foreach (var server in Servers)
            {
                if (!server.IsConnected) continue;
                foreach (PlayerList current in server.GetPlayerList(100, 0))
                {
                    if (!current.Null)
                    {
                        list.Add(current);
                    }
                }
            }
            return list.ToArray();
        }

        public CurrentMapInfo GetCurrentMapInfo
        {
            get { return _currentMapInfo; }
        }

        public string ReadText(string pluginName, int type, string toRead)
        {

            if (type == 0)
            {
                if (!File.Exists(string.Concat(new string[]
                {
                    "Plugins/",
                    pluginName,
                    "_xmlfiles/",
                    toRead,
                    ".xml"
                })))
                {
                    return "The file don't exist";
                }
                return File.ReadAllText(string.Concat(new string[]
                {
                    "Plugins/",
                    pluginName,
                    "_xmlfiles/",
                    toRead,
                    ".xml"
                }));
            }
            else
            {
                if (type != 1)
                {
                    return "";
                }
                if (!File.Exists(string.Concat(new string[]
                {
                    "Plugins/misc/",
                    pluginName,
                    "/",
                    toRead,
                    ".misc"
                })))
                {
                    return "The file don't exist";
                }
                return File.ReadAllText(string.Concat(new string[]
                {
                    "Plugins/misc/",
                    pluginName,
                    "/",
                    toRead,
                    ".misc"
                }));
            }
        }

        public void AddThisManialink(string _manialink, string name, bool refresh)
            => AddThisManialink(_manialink, name, refresh, null);

        public void AddThisManialink(string manialink, string name, bool refresh, ShootManiaServer _server)
        {
            if (!Server.IsConnected) return;
            PlayerList[] players = this.GetPlayers();
            foreach (PlayerList playerList in players.Where(playerList => !string.IsNullOrEmpty(playerList.Login)))
            {
                if (_server != null) this.AddThisManialink(playerList.Login, manialink, name, refresh, _server);
                else
                    foreach (var server in Servers.Where(server => server.IsConnected))
                        this.AddThisManialink(playerList.Login, manialink, name, refresh, server);
            }
        }

        public void AddThisManialink(string playerName, string _manialink, string name, bool refresh)
            => AddThisManialink(playerName, _manialink, name, refresh, null);

        public void AddThisManialink(string playerName, string _manialink, string name, bool refresh,
            ShootManiaServer _server)
        {
            if (!Server.IsConnected) return;
            bool flag = false;
            var manialink = _manialink.Replace("<libraryanim/>", GetMlAnimLib).Replace("<libraryrelay/>", GetRelayLib);
            if (string.IsNullOrEmpty(playerName))
            {
                PlayerList[] players = this.GetPlayers();
                for (int i = 0; i < players.Length; i++)
                {
                    PlayerList playerList = players[i];
                    if (!string.IsNullOrEmpty(playerList.Login))
                    {
                        flag = false;
                        Dictionary<string, string> dictionary = new Dictionary<string, string>();
                        Dictionary<string, string> dictionary2 = new Dictionary<string, string>();
                        dictionary2[playerList.Login] = name;
                        if (this.Manialinks.Count > 0)
                        {
                            foreach (
                                KeyValuePair<Dictionary<string, string>, ServerManager.CManialink> current in
                                    this.Manialinks)
                            {
                                if ((current.Value.Name == name | refresh) && current.Value.Name == name &&
                                    current.Key == dictionary2)
                                {
                                    dictionary = current.Key;
                                    current.Value.Manialink = manialink;
                                    flag = false;
                                    current.Value.ToLogin = playerList.Login;
                                    if (refresh)
                                    {
                                        dictionary = null;
                                        flag = true;
                                        current.Value.Manialink = manialink;
                                    }
                                }
                            }
                        }
                        if (dictionary != null)
                        {
                            this.Manialinks.Remove(dictionary);
                        }
                        if (!flag)
                        {
                            this.Manialinks.Add(dictionary2,
                                new ServerManager.CManialink(manialink, name, playerList.Login));
                        }
                        string text = "";
                        text += "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\" ?>";
                        foreach (
                            KeyValuePair<Dictionary<string, string>, ServerManager.CManialink> current2 in
                                this.Manialinks)
                        {
                            if (current2.Value.ToLogin == playerList.Login)
                            {
                                text = text + "\n<manialink name=\"" + current2.Value.Name + "\" version=\"2\">\n";
                                text += current2.Value.Manialink;
                                text += "\n</manialink>";
                            }
                        }
                        if (_server != null) _server.SendManialink(playerList.Login, text);
                        else
                            foreach (var server in Servers.Where(server => server != null && server.IsConnected))
                                server.SendManialink(playerList.Login, text);
                    }
                }
                return;
            }
            Dictionary<string, string> dictionary3 = new Dictionary<string, string>();
            Dictionary<string, string> dictionary4 = new Dictionary<string, string>();
            dictionary4[playerName] = name;
            foreach (KeyValuePair<Dictionary<string, string>, ServerManager.CManialink> current3 in this.Manialinks)
            {
                if ((current3.Value.Name == name | refresh) && current3.Key.ContainsKey(playerName) &&
                    current3.Key[playerName] == name)
                {
                    dictionary3 = current3.Key;
                    current3.Value.Manialink = manialink;
                    current3.Value.ToLogin = playerName;
                    flag = false;
                    if (refresh)
                    {
                        dictionary3 = null;
                        flag = true;
                        current3.Value.Manialink = manialink;
                    }
                }
            }
            if (dictionary3 != null)
            {
                this.Manialinks.Remove(dictionary3);
            }
            if (!flag)
            {
                this.Manialinks.Add(dictionary4, new ServerManager.CManialink(manialink, name, playerName));
            }
            string text2 = "";
            text2 += "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\" ?>";
            foreach (KeyValuePair<Dictionary<string, string>, ServerManager.CManialink> current4 in this.Manialinks)
            {
                if (current4.Value.ToLogin == playerName)
                {
                    text2 = text2 + "\n<manialink name=\"" + current4.Value.Name + "\" version=\"2\">\n";
                    text2 += current4.Value.Manialink;
                    text2 += "\n</manialink>";
                }
            }
            if (_server != null) _server.SendManialink(playerName, text2);
            else
                foreach (var server in Servers.Where(server => server != null && server.IsConnected))
                    server.SendManialink(playerName, text2);
        }

        public void RemoveThisManialink(string playerName, string name) => RemoveThisManialink(playerName, name, null);

        public void RemoveThisManialink(string playerName, string name, ShootManiaServer _server)
        {
            Dictionary<string, string> key = new Dictionary<string, string>();
            foreach (KeyValuePair<Dictionary<string, string>, ServerManager.CManialink> current in this.Manialinks)
            {
                if (current.Key.ContainsKey(playerName) && current.Key[playerName] == name)
                {
                    key = current.Key;
                }
            }
            this.Manialinks.Remove(key);
            if (_server != null) this.AddThisManialink(playerName, "", "", true, _server);
            else this.AddThisManialink(playerName, "", "", true, null);
        }

        private void ShowParam(string login, ServerManager.ParamEnum type)
        {
            ManialinkSystem manialinkSystem = new ManialinkSystem();
            Frame frame = new Frame();
            Quad quad = new Quad("", Vector3.Zero, new Vector2(100.0, 80.0), false);
            Quad quad2 = new Quad("Legacy_CloseSettings", new Vector3(30.0, 35.0, 1.0), new Vector2(10.0, 10.0), false);
            Label argE90 = new Label("", new Vector3(0.0, 35.0, 1.0), Vector2.Zero, false);
            frame.Position = new Vector3(0.0, 0.0, 30.0);
            quad.Halign = ManialinkSystem.Halign.Center;
            quad.Valign = ManialinkSystem.Valign.Center;
            quad2.Halign = ManialinkSystem.Halign.Center;
            quad2.Valign = ManialinkSystem.Valign.Center;
            argE90.Halign = ManialinkSystem.Halign.Center;
            argE90.Valign = ManialinkSystem.Valign.Center;
            argE90.Scale = 1.5;
            quad.Style("EnergyBar", "BgText");
            argE90.Style("TextButtonBig");
            quad2.Style("UIConstruction_Buttons", "Up");
            argE90.Text = "SETTINGS";
            quad2.Usescriptevents = true;
            new ChildOf(quad, frame);
            new ChildOf(argE90, frame);
            new ChildOf(quad2, frame);
            if (type == ServerManager.ParamEnum.ShowPluginsList)
            {
                int num = 25;
                foreach (Plugin current in this.LoadedPlugins.Where(current => current != null))
                {
                    Frame frame2 = new Frame();
                    frame2.Name = current.Name;
                    frame2.Position = new Vector3(0.0, 0.0, 31.0);
                    Quad quad3 = new Quad("Legacy_ShowPluginSettings_" + current.Name,
                        new Vector3(0.0, (double) num, 0.0), new Vector2(50.0, 10.0), false);
                    quad3.Style("BgsPlayerCard", "BgPlayerCard");
                    quad3.Usescriptevents = true;
                    quad3.Halign = ManialinkSystem.Halign.Center;
                    quad3.Valign = ManialinkSystem.Valign.Center;
                    Label expr26B = new Label("Plugin", new Vector3(0.0, (double) num, 6.0), new Vector2(50.0, 10.0),
                        false);
                    expr26B.Text = current.Name;
                    expr26B.Halign = ManialinkSystem.Halign.Center;
                    expr26B.Valign = ManialinkSystem.Valign.Center;
                    new ChildOf(quad3, frame2);
                    new ChildOf(expr26B, frame2);
                    new ChildOf(frame2, frame);
                    manialinkSystem.Add(frame2, false);
                    num -= 12;
                   /* BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                               BindingFlags.NonPublic;
                    FieldInfo[] fields = current.GetType().GetFields(bindingAttr);
                    for (int i = 0; i < fields.Length; i++)
                    {
                        AyO.Settings arg_2E40 =
                            (AyO.Settings) Attribute.GetCustomAttribute(fields[i], typeof (AyO.Settings));
                    }*/
                }
            }
            if (type == ServerManager.ParamEnum.ShowSetting)
            {
                Quad expr357 = new Quad("Legacy_ShowPluginList", new Vector3(0.0, 25.0, 0.0), new Vector2(50.0, 10.0),
                    false);
                expr357.Style("BgsPlayerCard", "BgPlayerCard");
                expr357.Usescriptevents = true;
                expr357.Valign = ManialinkSystem.Valign.Center;
                expr357.Halign = ManialinkSystem.Halign.Center;
                new ChildOf(new Label("", new Vector3(0.0, 25.0, 6.0), new Vector2(50.0, 10.0), false)
                {
                    Halign = ManialinkSystem.Halign.Center,
                    Valign = ManialinkSystem.Valign.Center,
                    Text = "Show Plugins List"
                }, frame);
                new ChildOf(expr357, frame);
            }
            manialinkSystem.Add(frame, true);
            this.AddThisManialink(login, manialinkSystem.CurrentBuild, "Legacy_ShowParamTo", true);
        }

        public readonly List<AyO.Chatclass> ChatList = new List<AyO.Chatclass>();

        private void HandleConnection()
        {
            while (true)
            {
                if (!this.Server.IsConnected)
                {
                    this.Server = new ShootManiaServer(this.Config.ShootManiaIp, this.Config.ShootManiaXmlRpcPort);
                    if (this.Server.Connect() == 0)
                    {
                        AyO.print("Connected to server!", ConsoleColor.Green);
                        AyO.print("Authentication ...", ConsoleColor.White);
                        if (this.Server.Authenticate(this.Config.ShootManiaSuperAdminLogin,
                            this.Config.ShootManiaSuperAdminPassword))
                        {
                            break;
                        }
                        AyO.print("Authentication failed!", ConsoleColor.Red);
                        this.Server.Disconnect();
                    }
                    else
                    {
                        AyO.print(string.Concat(new object[]
                        {
                            "Unable to connect to the server! : IP[",
                            this.Config.ShootManiaIp,
                            "] PORT[",
                            this.Config.ShootManiaXmlRpcPort,
                            "]"
                        }), ConsoleColor.Red);
                    }
                }
                if (!this.Server.IsConnected)
                {
                    AyO.print("Unable to connect, retry in : " + this.Config.ShootManiaReconnectTimeout + "ms ...",
                        ConsoleColor.Red);
                }
                Thread.Sleep(this.Config.ShootManiaReconnectTimeout);
            }
            AyO.print("Succes!", ConsoleColor.Green);
            this.Server.ChatSendServerMessage("$999AY$fffo $f00» $fffLoading. . .");
            AyO.print("Set API version...", ConsoleColor.White);
            this.Server.SetApiVersion("2015-02-10");
            AyO.print("Set API version -> Done!", ConsoleColor.Green);
            AyO.print("Enabling Callbacks...", ConsoleColor.White);
            this.Server.EnableCallback();
            AyO.print("Enabling Callbacks -> Done!", ConsoleColor.Green);
            AyO.print("Register events...", ConsoleColor.White);
            this.Server.Client.EventGbxCallback += new GbxCallbackHandler(this.HandleEventGbxCallback);
            this.Server.OnPlayerChat += new ShootManiaServer.OnPlayerChatEventHandler(this.OnPlayerChat);
            this.Server.OnModeScriptCallback +=
                new ShootManiaServer.ModeScriptCallbackEventHandler(this.OnModeScriptCallback);
            this.Server.OnPlayerConnect += new ShootManiaServer.OnPlayerConnectEventHandler(this.HandleOnPlayerConnect);
            this.Server.OnPlayerDisconnect +=
                new ShootManiaServer.OnPlayerDisconnectEventHandler(this.HandleOnPlayerDisconnect);
            AyO.print("Register events -> Done!", ConsoleColor.Green);
            AyO.print("Calling OnConnectionSuccessful...", ConsoleColor.White);
            ServerManager.OnConnectionSuccessfulEventHandler expr18E = this.OnConnectionSuccessful;
            if (expr18E != null)
            {
                expr18E();
            }
            AyO.print("Calling OnConnectionSuccessful -> Done!", ConsoleColor.Green);
            this.Server.ChatSendServerMessage("$999AY$fffo $f60» $fffPlease wait . . .");
            AyO.print("=-=-=-=-=-=-=-=-=-=-=", ConsoleColor.DarkGreen);
            AyO.print("     SUCCESS!        ", ConsoleColor.Green);
            AyO.print("=-=-=-=-=-=-=-=-=-=-=", ConsoleColor.DarkGreen);
            AyO.print("");
            AyO.print(true, "Type 'help' for list of commands");
            AyO.print(true, "Type 'about' for the credits", ConsoleColor.Gray);
            AyO.print(true, "Type 'load' to load plugins");
            this.Server.ChatSendServerMessage("$999AY$fffo $ff0» $0f0Sucessfully loaded!");
            this.Admins.AddGroup("SuperMegaFunGroup XDD").AddPlayer("guerro");
            this._widgetSetting = this.ReadText("serverManager", 0, "widget_settings");
            this.Server.SetTaTime(-1);
            this._removeLegacy.Add("Settings", false);
            int needRefreshLegacy = this.Now;
            this.RefreshTime = -1;
            var testprogam = new Program();
            if (Servers.Count > 0)
            {
                Server.ChatSendServerMessage("$999AY$fffo $06c・ $fffAvailable servers on network : $fff" + Servers.Count);
                foreach (var server in Servers.Where(server => server.IsConnected))
                {
                    Server.ChatSendServerMessage("$06c・$fc3 (" + server.FriendlyLogin + ")$06c | Name : $fc3" +
                                                 server.GetServerName() + "$>$z$s");
                    Server.ChatSendServerMessage("$fc3 Current Map : $fff" + server.GetCurrentMapInfo().Name +
                                                 "$z$s$>$z$s Game : " +
                                                 AyO.CompressEnviroName(Server.GetCurrentMapInfo().Environnement)
                                                     .ToUpper());
                }
            }
            Server.ChatEnableManualRouting();
            Servers.Add(Server);

            while (true)
            {
                if (!this.PluginsNotLoaded)
                {
                    foreach (Plugin plugin in this.LoadedPlugins)
                    {
                        if (plugin == null) continue;
                        plugin.OnLoop();
                        ;
                        foreach (var func in plugin.PluginFunction)
                        {

                            if (func == AyO.PluginFunction.Settings && needRefreshLegacy == -1)
                            {
                                this._removeLegacy["Settings"] = true;
                                needRefreshLegacy = this.Now;
                            }
                        }
                    }
                }
                if (GetCurrentMapInfo.Null)
                {
                    _currentMapInfo = Server.GetCurrentMapInfo();
                }
                if (this.RefreshTime < this.Now)
                {
                    this.AddThisManialink(this._widgetSetting, "[AYO]Widget_Settings",
                        true);
                    this.RefreshTime = this.Now + 15000;
                    /*if (true)
                    {
                        PlayerList[] players = this.GetPlayers();
                        needRefreshLegacy = Now + 15000;
                        for (int i = 0; i < players.Length; i++)
                        {
                            PlayerList playerList = players[i];
                            Dictionary<string, string> dictionary = new Dictionary<string, string>();
                            dictionary[playerList.Login] = "[AYO]Widget_Settings";
                            if (this.Manialinks.ContainsKey(dictionary))
                            {
                                this.RemoveThisManialink(playerList.Login, "[AYO]Widget_Settings");
                            }
                            if (!this._removeLegacy["Settings"])
                            {
                                this.AddThisManialink(playerList.Login, this._widgetSetting, "[AYO]Widget_Settings",
                                    true);
                            }
                            
                        }
                    }*/
                    /*    if (!this.Server.IsConnected)
				{
					this.UnloadPlugins();
					this.Server.Authenticate(this.Config.ShootManiaSuperAdminLogin, this.Config.ShootManiaSuperAdminPassword);
					this.Server = new ShootManiaServer(this.Config.ShootManiaIp, this.Config.ShootManiaXmlRpcPort);
					//this.LoadPlugins();
					Console.ForegroundColor = ConsoleColor.Red;
					this.Server.ChatSend("The XMLRPC server has an problem, and has been restarted");
					Console.ForegroundColor = ConsoleColor.White;
				}*/
                    this.Now = Environment.TickCount;
                }
            }
        }

        private void OnPlayerChat(PlayerChat pc)
        {
            foreach (var plu in LoadedPlugins)
            {
                plu.HandleOnPlayerChat(pc);
            }
            foreach (var server in Servers)
            {
                var isContinue = false;
                if (pc.Text.StartsWith("/")) break; //< Don't send commands

                foreach (var aserv in Servers.Where(aserv => aserv.IsConnected && pc.Login == aserv.GetLogin()))
                {
                    isContinue = true;
                }

                if (server.IsConnected && !isContinue)
                {
                    if (server.IsConnected && pc.Server.GetLogin() != server.GetLogin())
                        server.ChatSendServerMessage("$z$s[" + pc.Server.GetServerName() + "]$z$s(" + pc.Login + ") => " +
                                                     pc.Text);
                }
            }
            Console.WriteLine(pc.Text);
            if (pc.Login != Server.GetLogin())
                ChatList.Add(new AyO.Chatclass {Login = pc.Login, Text = pc.Text, ThisNow = Now});
            else ChatList.Add(new AyO.Chatclass {Login = pc.Login, Text = pc.Text, ThisNow = Now});
        }

        private void OnModeScriptCallback(ModeScriptCallback msc)
        {
            foreach (var plu in LoadedPlugins)
            {
                plu.HandleOnModeScriptCallback(msc);
            }
        }

        private void HandleEventGbxCallback(object o, GbxCallbackEventArgs e)
        {

            if (e.Response.MethodName == "ManiaPlanet.BeginMap" && Server.IsConnected)
            {
                Thread.Sleep(500);
                _currentMapInfo = Server.GetCurrentMapInfo();

            }

            if (e.Response.MethodName == "ManiaPlanet.PlayerManialinkPageAnswer")
            {
                Console.WriteLine(e.Response.Params[2].ToString());
                if ((string) e.Response.Params[2] == "Legacy_ShowParam")
                {
                    this.ShowParam(e.Response.Params[1].ToString(), ServerManager.ParamEnum.ShowSetting);
                }
                if ((string) e.Response.Params[2] == "Legacy_CloseSettings")
                {
                    this.RemoveThisManialink(e.Response.Params[1].ToString(), "Legacy_ShowParamTo");
                }
                if ((string) e.Response.Params[2] == "Legacy_ShowPluginList")
                {
                    this.ShowParam(e.Response.Params[1].ToString(), ServerManager.ParamEnum.ShowPluginsList);
                }
            }

            foreach (var plu in LoadedPlugins)
            {
                if (plu != null && o != null && e != null)
                {
                    if (Server.IsConnected)
                    {
                        plu.HandleFixedGbxCallBacks(e, e.Response.MethodName, GetServerFromRpcClient((XmlRpcClient)o));
                    }
                }

            }
        }

        public ShootManiaServer GetServerFromRpcClient(XmlRpcClient o)
        {
            if (o == null) return null;
            foreach (var server in Servers.Where(server => server.IsConnected))
            {
                if (server.Client == o) return server;
            }
            return null;
        }

        private void HandleOnPlayerDisconnect(PlayerDisconnect pc)
        {
            Console.WriteLine("Player [" + pc.Login + "] disconnected");
            this.Server.ChatSendServerMessage("$fff" + pc.Login + " $z$> $f00» $i$fffleft the server!");
        }

        private void HandleOnPlayerConnect(PlayerConnect pc)
        {
            string str = "Player [" + pc.Login + "] connected";
            if (pc.IsSpectator)
            {
                str += " (Spectator)";
            }
            PlayerList[] players = this.GetPlayers();
            for (int i = 0; i < players.Length; i++)
            {
                PlayerList playerList = players[i];
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary[playerList.Login] = "[AYO]Widget_Settings";
                if (this.Manialinks.ContainsKey(dictionary))
                {
                    this.RemoveThisManialink(playerList.Login, "[AYO]Widget_Settings");
                }
                if (!this._removeLegacy["Settings"])
                {
                    this.AddThisManialink(playerList.Login, "[AYO]Widget_Settings", true);
                }
            }
            this.Server.SendNoticeToLogin(pc.Login, "$fffWelcome to you " + pc.Login + "! $0f0:)");
            this.Server.ChatSendServerMessage(string.Concat(new string[]
            {
                "$fff",
                this.Admins.GetGroupForLogin(pc.Login),
                " : ",
                this.GetPlayer(pc.Login).Nickname,
                " $z$> $0f0» $i$fffjoined the server!"
            }));
        }

        public void InitializePlugin(Plugin pl)
        {
            pl.OnServerManagerInitialize(this);
        }

        public void Initialize()
        {
            foreach (var pl in LoadedPlugins)
            {
                pl.OnServerManagerInitialize(this);
            }
            this._threadHandleConnection = new Thread(new ThreadStart(this.HandleConnection));
            this._threadHandleConnection.IsBackground = true;
            this._threadHandleConnection.Start();
        }

        /// <summary>
        /// Add a simple server without custom friendlylogin and friendlyname
        /// </summary>
        /// <param name="ip">The IP of the server to add (only numbers)</param>
        /// <param name="port">The PORT of the server to add</param>
        public void AddServer(string ip, int port) => AddServer(ip, port, "", "");

        /// <summary>
        /// Add a simple server with custom friendlylogin and without friendlyname
        /// </summary>
        /// <param name="ip">The IP of the server to add (only numbers)</param>
        /// <param name="port">The PORT of the server to add</param>
        /// <param name="friendlyLogin">The friendlyLogin</param>
        public void AddServer(string ip, int port, string friendlyLogin) => AddServer(ip, port, friendlyLogin, "");

        /// <summary>
        /// Add a simple server with custom friendlylogin and friendlyname
        /// </summary>
        /// <param name="ip">The IP of the server to add (only numbers)</param>
        /// <param name="port">The PORT of the server to add</param>
        /// <param name="friendlyLogin">The friendlyLogin</param>
        /// <param name="friendlyName">The friendlyName</param>
        public void AddServer(string ip, int port, string friendlyLogin, string friendlyName)
        {
            ShootManiaServer _server = new ShootManiaServer(ip, port);
            foreach(var pl in LoadedPlugins) pl.OnCustomEvent(new object[] {_server}, "OnLoadServer");
            while (true)
            {
                if (!_server.IsConnected)
                {
                    _server = new ShootManiaServer(ip, port);
                    if (_server.Connect() == 0)
                    {
                        AyO.print("Connected to server!", ConsoleColor.Green);
                        AyO.print("Authentication ...", ConsoleColor.White);
                        if (_server.Authenticate(this.Config.ShootManiaSuperAdminLogin,
                            this.Config.ShootManiaSuperAdminPassword))
                        {
                            break;
                        }
                        AyO.print("Authentication failed!", ConsoleColor.Red);
                        _server.Disconnect();
                    }
                    else
                    {
                        AyO.print(string.Concat(new object[]
                        {
                            "Unable to connect to the server! : IP["
                            + ip +
                            "] PORT["
                            + port +
                            "]"
                        }), ConsoleColor.Red);
                    }
                }
                if (!_server.IsConnected)
                {
                    AyO.print("Unable to connect, retry in : " + this.Config.ShootManiaReconnectTimeout + "ms ...",
                        ConsoleColor.Red);
                }
            }
            _server.FriendlyLogin = friendlyLogin;
            _server.FriendlyName = friendlyName;
            AyO.print("Succes!", ConsoleColor.Green);
            _server.ChatSendServerMessage("$999AY$fffo $f00» $fffLoading. . .");
            AyO.print("Set API version...", ConsoleColor.White);
            _server.SetApiVersion("2015-02-10");
            AyO.print("Set API version -> Done!", ConsoleColor.Green);
            AyO.print("Enabling Callbacks...", ConsoleColor.White);
            _server.EnableCallback();
            AyO.print("Enabling Callbacks -> Done!", ConsoleColor.Green);
            AyO.print("Register events...", ConsoleColor.White);
            _server.ChatEnableManualRouting();
            _server.Client.EventGbxCallback += new GbxCallbackHandler(this.HandleEventGbxCallback);
            _server.OnPlayerChat += new ShootManiaServer.OnPlayerChatEventHandler(this.OnPlayerChat);
            _server.OnModeScriptCallback +=
                new ShootManiaServer.ModeScriptCallbackEventHandler(this.OnModeScriptCallback);
            _server.OnPlayerConnect += new ShootManiaServer.OnPlayerConnectEventHandler(this.HandleOnPlayerConnect);
            _server.OnPlayerDisconnect +=
                new ShootManiaServer.OnPlayerDisconnectEventHandler(this.HandleOnPlayerDisconnect);
            AyO.print("Register events -> Done!", ConsoleColor.Green);
            AyO.print("Calling OnConnectionSuccessful...", ConsoleColor.White);
            ServerManager.OnConnectionSuccessfulEventHandler expr18E = this.OnConnectionSuccessful;
            if (expr18E != null)
            {

                expr18E();
            }
            AyO.print("Calling OnConnectionSuccessful -> Done!", ConsoleColor.Green);

            _server.ChatSendServerMessage("$999AY$fffo $f60» $fffPlease wait . . .");
            AyO.print("=-=-=-=-=-=-=-=-=-=-=", ConsoleColor.DarkGreen);
            AyO.print("     SUCCESS!        ", ConsoleColor.Green);
            AyO.print("=-=-=-=-=-=-=-=-=-=-=", ConsoleColor.DarkGreen);

            if (Servers.Count > 0)
            {
                _server.ChatSendServerMessage("$999AY$fffo $06c・ $fffAvailable servers on network : $fff" +
                                              Servers.Count);
                foreach (var server in Servers.Where(server => server.IsConnected))
                {
                    _server.ChatSendServerMessage("$39f・$fc3 (" + server.GetLogin() + ")$39f | Name : $fc3" +
                                                  server.FriendlyName + "$>$z$s\n     $fc3 Current Map : $fff" +
                                                  server.GetCurrentMapInfo().Name + "$z$s$>\n      $z$s$39f | Game : $fff" +
                                                  AyO.CompressEnviroName(Server.GetCurrentMapInfo().Environnement)
                                                      .ToUpper() + "");
                }
            }
            Servers.Add(_server);

            foreach (var pl in LoadedPlugins) pl.OnCustomEvent(new object[] { _server }, "OnLoadServerSuccess");
        }

        public void RemoveServer(string frienLogin)
        {
            ShootManiaServer index = null;
            foreach (var server in Servers.Where(server => server.IsConnected))
            {
                if (server.FriendlyLogin == frienLogin) index = server;
            }
            if (index != null)
            {
                index.Client.EventGbxCallback += HandleEventGbxCallback;
                index.OnPlayerConnect += HandleOnPlayerConnect;
                index.OnPlayerDisconnect += HandleOnPlayerDisconnect;
                foreach (var pl in LoadedPlugins) pl.OnCustomEvent(new object[] { index }, "OnRemoveServer");
                Servers.Remove(index);
                AyO.print(false, " Server : " + frienLogin + " ,sucessfuly removed!");
            } else AyO.print(false, "Wrong friendlylogin or server don't exist");
        }

        public void OnConsoleCommand(string c)
        {
            if (c == "")
            {
                AyO.print(true, "Type 'help' for list of commands");
                AyO.print(true, "Type 'about' for the credits", ConsoleColor.Gray);
                AyO.print(true, "Type 'load' to load plugins");
            }
            if (c == "help")
            {
                AyO.print(false, " --- HELP --- ", ConsoleColor.Red);
                AyO.print(true, "   'load' to load all plugins");
                AyO.print(true, "   'unload' to unload all plugins");
                AyO.print(true,
                    "   'addserver {ip:\"127.0.0.7\", port:5000, login:'friendlyLogin', name:'friendlyName'}' to add an maniaplanet server.\n" +
                    "|      @param ip => obligated, @param port => obligated, @param login && name => not obligated : those params are only useful if you want to set a custom friendlyLogin or friendlyName, it will not change the login or the name of the server",
                    ConsoleColor.Gray);
            }
            if (c == "about")
            {
                AyO.print(false, "Credits : ", ConsoleColor.Blue);
                AyO.print(
                    "Original Creator : JuJuBoSc\r\nAyO Creator : Guerro ( ManiaPlanetForum : 'Guerro323'; ManiaPlanet : 'Guerro' )");
                AyO.print("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=",
                    ConsoleColor.Blue);
                AyO.print(
                    "AyO was originally based on SMAdminTools by JuJuBoSc.\r\nAyO is now a lot different from SMAdminTools, with his new core and new plugins\r\nAyO introduce a different point of view of the plugin management.\r\nPlugins can be based on another(s) plugin(s)\r\nAyO introduce RelayServer to communicate between clients and server(s)\r\nRelayServer is usefull when you got a lot of manialinks to refresh\r\nRelayServer can send ManialinkVars to the client without refreshing the manialink.\r\n\r\nAbout page is not finished.");
                Thread.Sleep(20);
                AyO.print("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=",
                    ConsoleColor.Blue);
            }
            if (c.StartsWith("addserver"))
            {
                var ipandport =
                    c.Replace("addserver ", "")
                        .Replace("ip", "\"ip\"")
                        .Replace("port", "\"port\"")
                        .Replace("login", "\"login\"")
                        .Replace("name", "\"name\"");
                var config = JsonMapper.ToObject<Private_Config>(ipandport);
                Console.WriteLine(config.ip + "  " + config.port);
                if (string.IsNullOrEmpty(config.login)) AddServer(config.ip, config.port);
                else if (string.IsNullOrEmpty(config.name)) AddServer(config.ip, config.port, config.login);
                else AddServer(config.ip, config.port, config.login, config.name);
            }
            if (c.StartsWith("removeserver"))
            {
                var removeserv = c.Replace("removeserver ", "");
                RemoveServer(removeserv);
            }
        }

        private class Private_Config
        {
            public string ip;
            public int port;
            public string login;
            public string name;
        }
    }
}