using System;

namespace IRCBot.Classes
{
	public class IrcBotSettings
	{

		public string IrcServer = "irc.quakenet.org";
		public int IrcPort = 6667;
		public string IrcChannel = "#AyoController";
		public string IrcNickname = "SM|IRCBot";
		public int ShootManiaIrcChatToGame = 1;
		public int ShootManiaGameChatToIrc = 1;
		public int ShootManiaSayPlayerConnected = 1;
		public int ShootManiaSayPlayerDisconnected = 1;
		public int ShootManiaSayHit = 1;
		public int ShootManiaSayFrag = 1;
		public int ShootManiaSayBeginMap = 1;
		public int ShootManiaSayEndMap = 1;
		public int ShootManiaSayBeginRound = 1;
		public int ShootManiaSayEndRound = 1;

				public Boolean ParseFromIniFile (string fileName)
		{
			// Need to be converted to XMLConfig.
			return false;
			/*
			AyoController.Classes.IniFile ini = new AyoController.Classes.IniFile (fileName);

            IrcServer = ini.GetValue ("IRC", "Server", string.Empty);

			if (IrcServer == string.Empty) {
				Console.WriteLine ("[IRCBot] Invalid IRC Server !");
				return false;
			}

            IrcChannel = ini.GetValue ("IRC", "Channel", string.Empty);

			if (IrcChannel == string.Empty) {
				Console.WriteLine ("[IRCBot] Invalid IRC Channel !");
				return false;
			}

            IrcNickname = ini.GetValue ("IRC", "Nickname", string.Empty);

			if (IrcNickname == string.Empty) {
				Console.WriteLine ("[IRCBot] Invalid IRC Nickname !");
				return false;
			}

			if (!int.TryParse (ini.GetValue ("IRC", "Port", string.Empty), out IrcPort)) {
				Console.WriteLine ("[IRCBot] Invalid IRC Port !");
				return false;
			}

			if (!int.TryParse (ini.GetValue ("ShootMania", "GameChatToIRC", "1"), out ShootManiaGameChatToIrc)) {
				Console.WriteLine ("[IRCBot] Invalid ShootMania GameChatToIRC !");
				return false;
			}

			if (!int.TryParse (ini.GetValue ("ShootMania", "IRCChatToGame", "1"), out ShootManiaIrcChatToGame)) {
				Console.WriteLine ("[IRCBot] Invalid ShootMania IRCChatToGame !");
				return false;
			}

			if (!int.TryParse (ini.GetValue ("ShootMania", "SayPlayerConnected", "1"), out ShootManiaSayPlayerConnected)) {
				Console.WriteLine ("[IRCBot] Invalid ShootMania SayPlayerConnected !");
				return false;
			}

			if (!int.TryParse (ini.GetValue ("ShootMania", "SayPlayerDisconnected", "1"), out ShootManiaSayPlayerDisconnected)) {
				Console.WriteLine ("[IRCBot] Invalid ShootMania SayPlayerDisconnected !");
				return false;
			}

			if (!int.TryParse (ini.GetValue ("ShootMania", "SayHit", "1"), out ShootManiaSayHit)) {
				Console.WriteLine ("[IRCBot] Invalid ShootMania SayHit !");
				return false;
			}

			if (!int.TryParse (ini.GetValue ("ShootMania", "SayFrag", "1"), out ShootManiaSayFrag)) {
				Console.WriteLine ("[IRCBot] Invalid ShootMania SayFrag !");
				return false;
			}

			if (!int.TryParse (ini.GetValue ("ShootMania", "SayBeginMap", "1"), out ShootManiaSayBeginMap)) {
				Console.WriteLine ("[IRCBot] Invalid ShootMania SayBeginMap !");
				return false;
			}

			if (!int.TryParse (ini.GetValue ("ShootMania", "SayEndMap", "1"), out ShootManiaSayEndMap)) {
				Console.WriteLine ("[IRCBot] Invalid ShootMania SayEndMap !");
				return false;
			}

			if (!int.TryParse (ini.GetValue ("ShootMania", "SayBeginRound", "1"), out ShootManiaSayBeginRound)) {
				Console.WriteLine ("[IRCBot] Invalid ShootMania SayBeginRound !");
				return false;
			}

			if (!int.TryParse (ini.GetValue ("ShootMania", "SayEndRound", "1"), out ShootManiaSayEndRound)) {
				Console.WriteLine ("[IRCBot] Invalid ShootMania SayEndRound !");
				return false;
			}
			*/
			return true;

		}

	}
}

