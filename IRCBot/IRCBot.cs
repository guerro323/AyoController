using System;
using System.Threading;
using System.IO;
using System.Reflection;
using AyoController;
using AyoController.Plugins;
using ShootManiaXMLRPC.XmlRpc;
using Meebey.SmartIrc4net;

namespace IRCBot
{
	public partial class IrcBot : Plugin
	{

		private const string IrcBotSettingsFile = "IRCBot__Settings.ini";

		private readonly Classes.IrcBotSettings _ircBotSettings = new Classes.IrcBotSettings();
		private readonly IrcClient _irc = new IrcClient();
		private bool _isLoaded = false;
		private Thread _threadIrcListen;
		private AyoController.Classes.ServerManager _server;

		public override AyO.PluginFunction[] PluginFunction
		{
			get
			{
				return new AyO.PluginFunction[] {
					AyO.PluginFunction.Nothing
				};
			}
		}

        public override string Name {
			get {
				return "IRCBot";
			}
		}

		public override string Author {
			get {
				return "JuJuBoSc";
			}
		}

		public override string Version {
			get {
				return "0.1";
			}
		}

        public override string[] ListofCommands
        {
            get
            {
                return new string[] { "???" };
            }
        }

        public override void OnLoop()
        {
        }

        public override void HandleEventGbxCallback(object o, GbxCallbackEventArgs e)
        {
        }

        public override void OnLoad ()
		{

			string currentAssemblyDirectoryName = Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location);

			if (File.Exists (currentAssemblyDirectoryName + "/" + IrcBotSettingsFile)) {
				if (!_ircBotSettings.ParseFromIniFile(currentAssemblyDirectoryName + "/" + IrcBotSettingsFile))
				    {
						Console.WriteLine("[IRCBot] Unable to load settings !");
					return;
				}
			} else {
				Console.WriteLine("[IRCBot] " + IrcBotSettingsFile + " not found !");
			}

			_irc.Encoding = System.Text.Encoding.UTF8;
			_irc.SendDelay = 200;
			_irc.ActiveChannelSyncing = true;
			_irc.AutoReconnect = true;
			_irc.AutoRejoin = true;
			_irc.AutoRetry = true;

			_irc.OnRawMessage += HandleOnRawMessage;
			_irc.OnChannelMessage += HandleOnChannelMessage;

			Console.WriteLine("[IRCBot] Connecting to IRC server ...");

			try {
				_irc.Connect (_ircBotSettings.IrcServer, _ircBotSettings.IrcPort);
				Console.WriteLine("[IRCBot] Connected to IRC server !");
			} catch {
				Console.WriteLine("[IRCBot] Cannot connect to IRC server !");
				return;
			}


			_irc.Login(_ircBotSettings.IrcNickname, "AyoController IRC Plugin", 0, "AyoController");
			_irc.RfcJoin(_ircBotSettings.IrcChannel);

			_threadIrcListen = new Thread(_irc.Listen);
			_threadIrcListen.IsBackground = true;
			_threadIrcListen.Start();

			_isLoaded = true;

		}

		void HandleOnChannelMessage (object sender, IrcEventArgs e)
		{
			if (e.Data.Channel == _ircBotSettings.IrcChannel &&
			    _ircBotSettings.ShootManiaIrcChatToGame == 1 &&
			    _server != null) {
				_server.Server.ChatSendServerMessage("$08F$o[IRC]$z <" + e.Data.Nick + "> " + e.Data.Message);
			}
		}

		void HandleOnRawMessage (object sender, IrcEventArgs e)
		{

		}

		public override void OnServerManagerInitialize (AyoController.Classes.ServerManager serverManager)
		{
			if (_isLoaded) {

                _server = serverManager;
                _server.OnConnectionSuccessful += HandleOnConnectionSuccessful;

			}
		}

        public override void HandleOnPlayerConnect (ShootManiaXMLRPC.Structs.PlayerConnect pc)
		{
			if (_ircBotSettings.ShootManiaSayPlayerConnected == 1) {
				_irc.SendMessage(SendType.Message, _ircBotSettings.IrcChannel, "[ShootMania] Player connected : " + pc.Login);
			}
		}

		void HandleOnPlayerDisconnect (ShootManiaXMLRPC.Structs.PlayerDisconnect pc)
		{
			if (_ircBotSettings.ShootManiaSayPlayerDisconnected == 1) {
				_irc.SendMessage(SendType.Message, _ircBotSettings.IrcChannel, "[ShootMania] Player disconnected : " + pc.Login);
			}
		}

        public override void HandleOnConnectionSuccessful ()
		{
            _server.Server.OnPlayerConnect += HandleOnPlayerConnect;
            _server.Server.OnPlayerDisconnect += HandleOnPlayerDisconnect;
            _server.Server.OnPlayerChat += HandleOnPlayerChat;
            _server.Server.OnModeScriptCallback += HandleOnModeScriptCallback;
			_irc.SendMessage(SendType.Message, _ircBotSettings.IrcChannel, "Connected to ShootMania server !");
		}

		public override void HandleOnPlayerChat (ShootManiaXMLRPC.Structs.PlayerChat pc)
		{
			if (_ircBotSettings.ShootManiaGameChatToIrc == 1)
			{
				foreach (var player in _server.GetPlayers())
				{
					if (player.Login == pc.Login &&
					    player.TeamId >= 0)
					{
						_irc.SendMessage(SendType.Message, _ircBotSettings.IrcChannel, "[ShootMania] [Chat] <" + AyoController.Functions.Mania.StripNadeoColours(player.Nickname) + "> " + AyoController.Functions.Mania.StripNadeoColours(pc.Text));
					}
				}
			}
		}

		public override void OnConsoleCommand (string command)
		{
			if (_isLoaded) {

			}
		}

        public override void Unload()
        {
            _server.Server.OnPlayerChat -= HandleOnPlayerChat;
            _server.Server.OnPlayerConnect -= HandleOnPlayerConnect;
            _server.OnConnectionSuccessful -= HandleOnConnectionSuccessful;
        }

    }
}

