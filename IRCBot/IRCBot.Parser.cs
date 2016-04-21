using System;
using Meebey.SmartIrc4net;

namespace IRCBot
{
	partial class IrcBot
	{

		void HandleOnModeScriptCallback (ShootManiaXMLRPC.Structs.ModeScriptCallback msc)
		{

			if (msc.Param1 == "startRound" &&
				_ircBotSettings.ShootManiaSayBeginRound == 1) {

				_irc.SendMessage(SendType.Message, _ircBotSettings.IrcChannel, "[ShootMania] Round " + msc.Param2 + " started !");

			}
			else if (msc.Param1 == "endRound" &&
				_ircBotSettings.ShootManiaSayEndRound == 1) {

				string[] Params = msc.Param2.Split(';');

				if (Params.Length == 3)
				{
					
					string unk1 = Params[0];
					string winner = Params[1];
					string unk2 = Params[2];

					_irc.SendMessage(SendType.Message, _ircBotSettings.IrcChannel, "[ShootMania] " + winner + " win the round !");

				}

			}
			else if (msc.Param1 == "hit" &&
				_ircBotSettings.ShootManiaSayHit == 1) {

				string[] Params = msc.Param2.Split(';');

				if (Params.Length == 3)
				{

					string shooter = Params[0];
					string unk1 = Params[1];
					string victim = Params[2];
					
					var playerShooter = _server.Server.GetPlayerListByPlayerLogin(shooter);
					var playerVictim = _server.Server.GetPlayerListByPlayerLogin(victim);

					if (playerShooter.Nickname.Length > 0 &&
					    playerVictim.Nickname.Length > 0)
					{
						_irc.SendMessage(SendType.Message, _ircBotSettings.IrcChannel, "[ShootMania] " + AyoController.Functions.Mania.StripNadeoColours(playerShooter.Nickname) + " hit " + AyoController.Functions.Mania.StripNadeoColours(playerVictim.Nickname) + " !");
					}

				}

			}
			else if (msc.Param1 == "frag" &&
				_ircBotSettings.ShootManiaSayFrag == 1) {

				string[] Params = msc.Param2.Split(';');

				if (Params.Length == 3)
				{

					string shooter = Params[0];
					string unk1 = Params[1];
					string victim = Params[2];
					
					var playerShooter = _server.Server.GetPlayerListByPlayerLogin(shooter);
					var playerVictim = _server.Server.GetPlayerListByPlayerLogin(victim);

					if (playerShooter.Nickname.Length > 0 &&
					    playerVictim.Nickname.Length > 0)
					{
						_irc.SendMessage(SendType.Message, _ircBotSettings.IrcChannel, "[ShootMania] " + AyoController.Functions.Mania.StripNadeoColours(playerShooter.Nickname) + " killed " + AyoController.Functions.Mania.StripNadeoColours(playerVictim.Nickname) + " !");
					}

				}

			}
			else if (msc.Param1 == "endMap" &&
				_ircBotSettings.ShootManiaSayEndMap == 1) {

				_irc.SendMessage(SendType.Message, _ircBotSettings.IrcChannel, "[ShootMania] Map ended !");

			}
			else if (msc.Param1 == "beginMap" &&
				_ircBotSettings.ShootManiaSayBeginMap == 1) {

				_irc.SendMessage(SendType.Message, _ircBotSettings.IrcChannel, "[ShootMania] Map started : " + AyoController.Functions.Mania.StripNadeoColours(msc.Param2) + " !");

			}

		}

	}
}

