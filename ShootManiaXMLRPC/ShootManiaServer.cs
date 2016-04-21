using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Text;
using System.Xml;
using ShootManiaXMLRPC.XmlRpc;

namespace ShootManiaXMLRPC
{
    public class ShootManiaServer
    {

        private readonly object _mLockCallback = new object();
        private string _friendlyLogin;
        private string _friendlyName;

        private delegate void SimpleDelegate();

        public delegate void OnPlayerChatEventHandler(Structs.PlayerChat pc);

        public event OnPlayerChatEventHandler OnPlayerChat;

        public delegate void OnPlayerDisconnectEventHandler(Structs.PlayerDisconnect pc);

        public event OnPlayerDisconnectEventHandler OnPlayerDisconnect;

        public delegate void OnPlayerConnectEventHandler(Structs.PlayerConnect pc);

        public event OnPlayerConnectEventHandler OnPlayerConnect;

        public delegate void OnVoteUpdatedEventHandler(Structs.VoteUpdated vu);

        public event OnVoteUpdatedEventHandler OnVoteUpdated;

        public delegate void ModeScriptCallbackEventHandler(Structs.ModeScriptCallback msc);

        public event ModeScriptCallbackEventHandler OnModeScriptCallback;

        public XmlRpc.XmlRpcClient Client { get; private set; }

        /// <summary>
        /// The first 'string' is for the plugin name.
        /// The secondary 'string' is for the json config.
        /// </summary>
        public Dictionary<string, string> PluginsConfig;

        public string FriendlyLogin
        {
            get
            {
                if (!string.IsNullOrEmpty(_friendlyLogin) && _friendlyLogin.Length > 1 &&
                    !_friendlyLogin.StartsWith("  ")) return _friendlyLogin;
                return GetLogin();
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                _friendlyLogin = value;
            }
        }

        public string FriendlyName
        {
            get
            {
                if (!string.IsNullOrEmpty(_friendlyName) && _friendlyName.Length > 1 && !_friendlyName.StartsWith("  ")) return _friendlyName;
                return GetServerName();
            }
            set { _friendlyName = value; }
        }

        public ShootManiaServer(string ip, Int32 port)
        {
            Client = new XmlRpc.XmlRpcClient(ip, port);
            Client.EventGbxCallback += new XmlRpc.GbxCallbackHandler(Client_EventGbxCallback);
        }

        void Client_EventGbxCallback(object o, XmlRpc.GbxCallbackEventArgs e)
        {
            lock (_mLockCallback)
            {
                ParseEventGbx(e);
            }

            return;
            SimpleDelegate d = delegate()
            {
                ParseEventGbx(e);
            };

            ThreadStart threadDelegate = new ThreadStart(d);

            new System.Threading.Thread(threadDelegate).Start();

        }

        private void ParseEventGbx(XmlRpc.GbxCallbackEventArgs e)
        {
            if (e.Response.MethodName == "ManiaPlanet.PlayerChat" &&
                e.Response.Params.Count == 4)
            {
                try
                {

                    Structs.PlayerChat pc = new Structs.PlayerChat();
                    pc.PlayerUid = (int) e.Response.Params[0];
                    pc.Login = (string) e.Response.Params[1];
                    pc.Text = (string) e.Response.Params[2];
                    pc.IsRegisteredCmd = (bool) e.Response.Params[3];
                    pc.Server = this;

                    /*if (pc.Text.Contains("guerro") && pc.Login != "guerro")
                    {
                        ChatSend("Guerro Help => guerro, guerro date");
                    }
                    if (pc.Text.Contains("guerro date") && pc.Login != "guerro")
                    {
                        ChatSend("Voici une date précise : " + new Random().Next(0, 1000000000));
                    }
                    if (pc.Text.Contains("guerro spam") && pc.Login != "guerro")
                    {
                        for (int I = 0; I < 100; I++)
                        ChatSend("/quote");
                    }*/

                    if (OnPlayerChat != null)
                    {
                        OnPlayerChat(pc);
                    }
                }
                catch
                {
                }
            }
            else if (e.Response.MethodName == "ManiaPlanet.PlayerConnect" &&
                     e.Response.Params.Count == 2)
            {
                try
                {

                    Structs.PlayerConnect pc = new Structs.PlayerConnect();
                    pc.Login = (string) e.Response.Params[0];
                    pc.IsSpectator = (bool) e.Response.Params[1];

                    if (OnPlayerConnect != null)
                        OnPlayerConnect(pc);

                }
                catch
                {
                }
            }
            else if (e.Response.MethodName == "ManiaPlanet.PlayerDisconnect" &&
                     e.Response.Params.Count == 1)
            {
                try
                {

                    Structs.PlayerDisconnect pd = new Structs.PlayerDisconnect();
                    pd.Login = (string) e.Response.Params[0];


                    if (OnPlayerDisconnect != null)
                        OnPlayerDisconnect(pd);

                }
                catch
                {
                }
            }
            else if (e.Response.MethodName == "ManiaPlanet.VoteUpdated" &&
                     e.Response.Params.Count == 4)
            {
                try
                {

                    Structs.VoteUpdated vu = new Structs.VoteUpdated();
                    vu.StateName = (string) e.Response.Params[0];
                    vu.Login = (string) e.Response.Params[1];
                    vu.CmdName = (string) e.Response.Params[2];
                    vu.CmdParam = (string) e.Response.Params[3];

                    if (OnVoteUpdated != null)
                        OnVoteUpdated(vu);

                }
                catch
                {
                }
            }
            else if (e.Response.MethodName == "ManiaPlanet.ModeScriptCallback" &&
                     e.Response.Params.Count == 2)
            {
                try
                {

                    Structs.ModeScriptCallback msc = new Structs.ModeScriptCallback();
                    msc.Param1 = (string) e.Response.Params[0];
                    msc.Param2 = (string) e.Response.Params[1];

                    if (OnModeScriptCallback != null)
                        OnModeScriptCallback(msc);

                }
                catch
                {
                }
            }
            else
            {
                // Unhandled callback
            }
        }

        public int Connect()
        {
            return Client.Connect();
        }

        public void Disconnect()
        {
            try
            {
                Client.Disconnect();
            }
            catch
            {
            }
        }

        public bool IsConnected
        {
            get { return Client.IsConnected; }
        }

        public void EnableCallback()
        {
            Client.EnableCallbacks(true);
        }

        public void DisableCallback()
        {
            Client.EnableCallbacks(false);
        }

        public Boolean Authenticate(string username, string password)
        {
            Console.WriteLine("test");
            GbxCall request = Client.Request("Authenticate", new object[] {username, password});
            if (request == null) return false;
            GbxCall response = Client.GetResponse(request.Handle);

            if (response.Params.Count == 1)
            {
                return (bool) response.Params[0];
            }
            else
            {
                return false;
            }

        }

        public void ChatSend(string message)
        {

            GbxCall request = Client.Request("ChatSend", new object[] {message});

        }

        public string GetLogin()
        {
            GbxCall request = Client.Request("GetSystemInfo", new object[] {});
            GbxCall done = Client.GetResponse(request.Handle);
            Hashtable ht = (Hashtable) done.Params[0];
            return ((string) ht["ServerLogin"]);
        }

        public void SendAsLogin(string login, string message)
        {
            GbxCall request = Client.Request("ChatForwardToLogin", new object[] {message, login, ""});
            Structs.PlayerChat pc = new Structs.PlayerChat();

            pc.PlayerUid = GetPlayerListByPlayerLogin(login).PlayerId;
            pc.Login = login;
            pc.Text = message;

            /*if (PC.Text.Contains("guerro") && PC.Login != "guerro")
            {
                ChatSend("Guerro Help => guerro, guerro date");
            }
            if (PC.Text.Contains("guerro date") && PC.Login != "guerro")
            {
                ChatSend("Voici une date précise : " + new Random().Next(0, 1000000000));
            }
            if (PC.Text.Contains("guerro spam") && PC.Login != "guerro")
            {
                for (int I = 0; I < 100; I++)
                ChatSend("/quote");
            }*/

            if (OnPlayerChat != null)
            {
                OnPlayerChat(pc);
            }
        }

        public bool WriteFile(string fileName, string toWrite)
        {
            var Base = new Base64(toWrite);
            GbxCall request = Client.Request("WriteFile", new object[] {fileName, Encoding.Default.GetBytes(toWrite)});
            GbxCall done = Client.GetResponse(request.Handle);
            if (done.Error)
            {
                Console.WriteLine("ERROR::WriteFile > " + done.ErrorCode + " | " + done.ErrorString);
                return true;
            }
            return false;
        }


        public class Base64
        {
            public byte[] Data;

            public Base64(string _data)
            {
                var tempdata = _data;
                Data = Encoding.Default.GetBytes(tempdata);

            }

            public string GetXml()
            {
                return "<base64>" + Encoding.Default.GetString(Data) + "</base64>";
            }
        }


        public void InsertMap(string mapName)
        {
            GbxCall request = Client.Request("InsertMap", new object[] {mapName});
            GbxCall done = Client.GetResponse(request.Handle);
            if (done.Error)
            {
                Console.WriteLine("ERROR::InsertMap > " + done.ErrorCode + " | " + done.ErrorString);
            }
        }

        public void ChatSendServerMessage(string message)
        {

            GbxCall request = Client.Request("ChatSendServerMessage", new object[] {message});

        }

        public void SetServerPassword(string password)
        {

            GbxCall request = Client.Request("SetServerPassword", new object[] {password});

        }

        public void ChatEnableManualRouting()
        {
            GbxCall request = Client.Request("ChatEnableManualRouting", new object[] {true, false});
            Console.WriteLine("---------------------" + Client.GetResponse(request.Handle).Error);
        }

        public string GetServerPassword()
        {

            GbxCall request = Client.Request("GetServerPassword", new object[] {});

            GbxCall response = Client.GetResponse(request.Handle);

            if (response.Params.Count == 1)
            {
                return response.Params[0].ToString();
            }
            else
            {
                return string.Empty;
            }

        }

        public void SetServerPasswordForSpectator(string password)
        {

            GbxCall request = Client.Request("SetServerPasswordForSpectator", new object[] {password});

        }

        public string GetServerPasswordForSpectator()
        {

            GbxCall request = Client.Request("GetServerPasswordForSpectator", new object[] {});

            GbxCall response = Client.GetResponse(request.Handle);

            if (response.Params.Count == 1)
            {
                return response.Params[0].ToString();
            }
            else
            {
                return string.Empty;
            }

        }

        public void SetServerComment(string comment)
        {

            GbxCall request = Client.Request("SetServerComment", new object[] {comment});

        }

        public string GetServerComment()
        {

            GbxCall request = Client.Request("GetServerComment", new object[] {});

            GbxCall response = Client.GetResponse(request.Handle);

            if (response.Params.Count == 1)
            {
                return response.Params[0].ToString();
            }
            else
            {
                return string.Empty;
            }

        }

        public void SetServerName(string comment)
        {

            GbxCall request = Client.Request("SetServerName", new object[] {comment});

        }

        public void NextMap(Boolean dontClearCupScores)
        {

            GbxCall request = Client.Request("NextMap", new object[] {dontClearCupScores});

        }

        public void RestartMap(Boolean dontClearCupScores)
        {

            GbxCall request = Client.Request("RestartMap", new object[] {dontClearCupScores});

        }

        public string GetServerName()
        {

            GbxCall request = Client.Request("GetServerName", new object[] {});

            GbxCall response = Client.GetResponse(request.Handle);

            if (response.Params.Count == 1)
            {
                return response.Params[0].ToString();
            }
            else
            {
                return string.Empty;
            }

        }

        public void SetMaxPlayers(Int32 maxPlayers)
        {

            GbxCall request = Client.Request("SetMaxPlayers", new object[] {maxPlayers});

        }

        public Structs.MaxPlayers GetMaxPlayers()
        {

            GbxCall request = Client.Request("GetMaxPlayers", new object[] {});

            GbxCall response = Client.GetResponse(request.Handle);

            if (response.Params.Count == 1 &&
                response.Params[0].GetType() == typeof (Hashtable))
            {
                Hashtable ht = (Hashtable) response.Params[0];
                Structs.MaxPlayers mp = new Structs.MaxPlayers();
                mp.CurrentValue = (int) ht["CurrentValue"];
                mp.NextValue = (int) ht["NextValue"];
                return mp;
            }
            else
            {
                return new Structs.MaxPlayers();
            }

        }

        public void SetMaxSpectators(Int32 maxSpectators)
        {

            GbxCall request = Client.Request("SetMaxSpectators", new object[] {maxSpectators});

        }

        public Structs.MaxSpectators GetMaxSpectators()
        {

            GbxCall request = Client.Request("GetMaxSpectators", new object[] {});

            GbxCall response = Client.GetResponse(request.Handle);

            if (response.Params.Count == 1 &&
                response.Params[0].GetType() == typeof (Hashtable))
            {
                Hashtable ht = (Hashtable) response.Params[0];
                Structs.MaxSpectators ms = new Structs.MaxSpectators();
                ms.CurrentValue = (int) ht["CurrentValue"];
                ms.NextValue = (int) ht["NextValue"];
                return ms;
            }
            else
            {
                return new Structs.MaxSpectators();
            }

        }

        public List<string> GetChatLines()
        {

            List<string> result = new List<string>();

            GbxCall request = Client.Request("GetChatLines", new object[] {});

            GbxCall response = Client.GetResponse(request.Handle);

            foreach (object o in response.Params)
            {
                if (o.GetType() == typeof (ArrayList))
                {
                    ArrayList lines = (ArrayList) o;
                    foreach (var line in lines)
                    {
                        result.Add(line.ToString());
                    }
                }
            }

            return result;

        }

        public List<Structs.MapList> GetMapList(int maxResults, int startIndex)
        {

            List<Structs.MapList> result = new List<Structs.MapList>();

            GbxCall request = Client.Request("GetMapList", new object[] {maxResults, startIndex});

            GbxCall response = Client.GetResponse(request.Handle);

            if (response.Params.Count == 1 &&
                response.Params[0].GetType() == typeof (ArrayList))
            {
                foreach (Hashtable ht in (ArrayList) response.Params[0])
                {
                    Structs.MapList ml = new Structs.MapList();
                    ml.Author = (string) ht["Author"];
                    ml.Environnement = (string) ht["Environnement"];
                    ml.FileName = (string) ht["FileName"];
                    ml.MapStyle = (string) ht["MapStyle"];
                    ml.Name = (string) ht["Name"];
                    ml.UId = (string) ht["UId"];
                    ml.GoldTime = (int) ht["GoldTime"];
                    ml.CopperPrice = (int) ht["CopperPrice"];
                    result.Add(ml);
                }
            }

            return result;

        }

        public List<Structs.PlayerList> GetPlayerList(int maxResults, int startIndex)
        {

            List<Structs.PlayerList> result = new List<Structs.PlayerList>();

            GbxCall request = Client.Request("GetPlayerList", new object[] {maxResults, startIndex});

            GbxCall response = Client.GetResponse(request.Handle);

            if (response.Params.Count == 1 &&
                response.Params[0].GetType() == typeof (ArrayList))
            {
                foreach (Hashtable ht in (ArrayList) response.Params[0])
                {
                    Structs.PlayerList pl = new Structs.PlayerList();
                    pl.Null = false;
                    pl.SpectatorStatus = (int) ht["SpectatorStatus"];
                    pl.Flags = (int) ht["Flags"];
                    pl.LadderRanking = (int) ht["LadderRanking"];
                    pl.PlayerId = (int) ht["PlayerId"];
                    pl.TeamId = (int) ht["TeamId"];
                    pl.Login = (string) ht["Login"];
                    pl.Nickname = (string) ht["NickName"];
                    pl.Server = this;
                    result.Add(pl);
                }
            }

            return result;

        }

        public void KickId(Int32 playerId, string message)
        {

            GbxCall request = Client.Request("KickId", new object[] {playerId, message});

        }

        public void BanId(Int32 playerId, string message)
        {

            GbxCall request = Client.Request("BanId", new object[] {playerId, message});

        }

        public void ChooseNextMap(string filename)
        {

            GbxCall request = Client.Request("ChooseNextMap", new object[] {filename});

        }

        public void SetApiVersion(string version)
        {

            GbxCall request = Client.Request("SetApiVersion", new object[] {version});

        }

        public void UnBan(string clientName)
        {

            GbxCall request = Client.Request("UnBan", new object[] {clientName});

        }

        public ShootManiaXMLRPC.Structs.ServerStatus GetStatus()
        {


            GbxCall request = Client.Request("GetStatus", new object[] {});

            GbxCall response = Client.GetResponse(request.Handle);

            if (response.Params.Count == 1 &&
                response.Params[0].GetType() == typeof (Hashtable))
            {
                Hashtable ht = (Hashtable) response.Params[0];
                Structs.ServerStatus ss = new Structs.ServerStatus();
                ss.Code = (int) ht["Code"];
                ss.Name = (string) ht["Name"];
                return ss;
            }

            return new Structs.ServerStatus();

        }



        public List<Structs.BanList> GetBanList(int maxResults, int startIndex)
        {

            List<Structs.BanList> result = new List<Structs.BanList>();

            GbxCall request = Client.Request("GetBanList", new object[] {maxResults, startIndex});

            GbxCall response = Client.GetResponse(request.Handle);

            if (response.Params.Count == 1 &&
                response.Params[0].GetType() == typeof (ArrayList))
            {
                foreach (Hashtable ht in (ArrayList) response.Params[0])
                {
                    Structs.BanList bl = new Structs.BanList();
                    bl.Login = (string) ht["Login"];
                    bl.ClientName = (string) ht["ClientName"];
                    bl.IpAddress = (string) ht["IPAddress"];
                    result.Add(bl);
                }
            }

            return result;

        }

        public ShootManiaXMLRPC.Structs.ScriptName GetScriptName()
        {


            GbxCall request = Client.Request("GetScriptName", new object[] {});

            GbxCall response = Client.GetResponse(request.Handle);

            if (response.Params.Count == 1 &&
                response.Params[0].GetType() == typeof (Hashtable))
            {
                Hashtable ht = (Hashtable) response.Params[0];
                Structs.ScriptName sn = new Structs.ScriptName();
                sn.CurrentValue = (string) ht["CurrentValue"];
                sn.NextValue = (string) ht["NextValue"];
                return sn;
            }

            return new Structs.ScriptName();

        }

        public void SetScriptName(string scriptName)
        {

            GbxCall request = Client.Request("SetScriptName", new object[] {scriptName});

        }

        public ShootManiaXMLRPC.Structs.CurrentMapInfo GetCurrentMapInfo()
        {

            if (Client == null
                || !Client.IsConnected) return new Structs.CurrentMapInfo() {Null = true};
            GbxCall request = Client.Request("GetCurrentMapInfo", new object[] {});

            GbxCall response = Client.GetResponse(request.Handle);

            if (response.Params.Count == 1 &&
                response.Params[0].GetType() == typeof (Hashtable))
            {
                Hashtable ht = (Hashtable) response.Params[0];
                Structs.CurrentMapInfo cmi = new Structs.CurrentMapInfo();
                cmi.Author = (string) ht["Author"];
                cmi.Environnement = (string) ht["Environnement"];
                cmi.FileName = (string) ht["FileName"];
                cmi.MapStyle = (string) ht["MapStyle"];
                cmi.Name = (string) ht["Name"];
                cmi.UId = (string) ht["UId"];
                cmi.GoldTime = (int) ht["GoldTime"];
                cmi.CopperPrice = (int) ht["CopperPrice"];
                cmi.NbCheckpoints = (int) ht["NbCheckpoints"];
                cmi.Null = false;
                return cmi;
            }

            return new Structs.CurrentMapInfo() {Null = true};

        }

        public GbxCall RequestCall(string methodName, object[] paramObjects)
            => Client.GetResponse(Client.Request(methodName, paramObjects).Handle);

        public void SendNoticeToLogin(string login, string text)
        {

            GbxCall request = Client.Request("SendNoticeToLogin", new object[] {text, login});

        }

        public void ChatSendToLogin(string login, string text)
        {

            GbxCall request = Client.Request("ChatSendServerMessageToLogin", new object[] {text, login});
            GbxCall done = Client.GetResponse(request.Handle);

        }

        public void Test(string login, string text)
        {

            GbxCall request = Client.Request("ChatSendToLogin", new object[] {text, login});
            GbxCall request2 = Client.Request("ChatSendToLogin", new object[] {login, login});
            GbxCall done = Client.GetResponse(request.Handle);
            GbxCall done2 = Client.GetResponse(request2.Handle);
            ChatSend(done.ErrorString);
            ChatSend(done2.ErrorString);

        }

        public void SetTaTime(int time)
        {
            GbxCall request = Client.Request("SetTimeAttackLimit", new object[] {time});
            GbxCall done = Client.GetResponse(request.Handle);
        }

        public void SendData(string playerName, object toSend)
        {
            GbxCall request = Client.Request("GetModeScriptVariables", new object[] {});
            GbxCall done = Client.GetResponse(request.Handle);
        }

        public void SetSpectator(string login)
        {
            GbxCall request = Client.Request("ForceSpectator", new object[] {login, 3});
        }

        public void SetPlayer(string login)
        {
            GbxCall request = Client.Request("ForceSpectator", new object[] {login, 2});
        }

        public void SetNoUi(string ui)
        {
            GbxCall request = Client.Request("TriggerModeScriptEvent", new object[] {"UI_SetProperties", ui});
        }


        public string[] GetAllXmlMethod()
        {
            GbxCall request = Client.Request("system.listMethods", new object[] {});
            GbxCall response = Client.GetResponse(request.Handle);
            List<string> final = new List<string>();
            ArrayList ht = (ArrayList) response.Params[0];
            foreach (object resp in ht)
            {
                final.Add(resp.ToString());
            }
            Console.WriteLine(response.Params[0]);
            return final.ToArray();
        }

        public void SendManialink(string playerName, string manialinkToDisplay, int timeOut = 0,
            Boolean hideWhenClicked = false)
        {
            if (playerName != null && playerName != "")
            {
                GbxCall request = Client.Request("SendDisplayManialinkPageToLogin",
                    new object[] {playerName, manialinkToDisplay, timeOut, hideWhenClicked});
                GbxCall done = Client.GetResponse(request.Handle);
                Console.WriteLine(done.Error);
            }
            else
            {
                GbxCall request = Client.Request("SendDisplayManialinkPage",
                    new object[] {manialinkToDisplay, timeOut, hideWhenClicked});
                GbxCall done = Client.GetResponse(request.Handle);
                ChatSendServerMessage("Manialink sent to EVERYONE");
            }
        }

        public Structs.PlayerList GetPlayerListByPlayerLogin(string login)
        {

            foreach (var player in GetPlayerList(100, 0))
                if (!player.Null && player.Login == login)
                    return player;

            return new ShootManiaXMLRPC.Structs.PlayerList();

        }

    }
}
