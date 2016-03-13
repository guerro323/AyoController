using System;
using System.Collections.Generic;
using AyoController.Plugins;

namespace Commands
{
	partial class Commands
	{
        private string RandomHello()
        {
            Random rand = new Random();
            int result = rand.Next(0, 3);
            switch (result)
            {
                case 1:
                    {
                        return "Hello all!";
                        break;
                    }
                case 2:
                    {
                        return "Hey all!";
                        break;
                    }
                case 3:
                    {
                        return "Hi alll!";
                        break;
                    }
            }
            return "Hey!";
        }

		private void ParseChatCommand (ShootManiaXMLRPC.Structs.PlayerChat PC)
		{
            switch (PC.Text)
            {
                case "/help":
                    {
                        var result = "";
                        var rankCommand = 1;
                        foreach (Plugin Pl in Manager.LoadedPlugins)
                        {
                            foreach (string Cmd in ServerManager.Commands[rankCommand-1])
                            {
                                result += Cmd + ", ";
                            }
                            rankCommand++;
                        }
                        ServerManager.Server.ChatSendToLogin(PC.Login, "Commands : " + result);

                        break;
                    }
                case "/hi":
                    {
                        ServerManager.Server.ChatSendServerMessage(ServerManager.GetPlayer(PC.PlayerUid).Nickname + "$z$> $s$999» $i$fff"+ RandomHello() +"!");
                        break;
                    }
            }

		}
	}
}


