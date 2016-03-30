using System;
using System.Collections.Generic;
using AyoController.Plugins;

namespace Commands
{
	partial class Commands
	{
        private string RandomHelloAll()
        {
            Random rand = new Random();
            int result = rand.Next(1, 4);
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
        private string RandomHello()
        {
            Random rand = new Random();
            int result = rand.Next(1, 4);
            switch (result)
            {
                case 1:
                    {
                        return "Hello!";
                        break;
                    }
                case 2:
                    {
                        return "Hey!";
                        break;
                    }
                case 3:
                    {
                        return "Hi!";
                        break;
                    }
            }
            return "Hey!";
        }

        string[] BadWords = new string[] { "cazzo", "merda", "kurwa", "fuck off", "dick", "fuck", "anus", "merde", "putain" };

        private void ParseChatCommand (ShootManiaXMLRPC.Structs.PlayerChat PC)
		{
            foreach (var badword in BadWords)
            {
                if (PC.Text.Contains(badword))
                {
                    ChatSendServerMessage("Bad word detected!");
                    ServerManager.AddThisManialink(PC.Login, @"<label text=""fuck you then"" />", "Warning", true);
                    ServerManager.AddThisManialink(PC.Login, @"<label posn=""0 -10 0"" text=""xd"" />", "Warning2", true);
                }

            }

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
                case "/hia":
                    {
                        ServerManager.Server.ChatSendServerMessage(ServerManager.GetPlayer(PC.PlayerUid).Nickname + "$z$> $s$999» $i$fff"+ RandomHelloAll() +"!");
                        break;
                    }
                case "/hi":
                    {
                        ServerManager.Server.ChatSendServerMessage(ServerManager.GetPlayer(PC.PlayerUid).Nickname + "$z$> $s$0f0» $i$fff" + RandomHello() + "!");
                        break;
                    }
                case "afk":
                    {
                        ServerManager.Server.ChatSendServerMessage(ServerManager.GetPlayer(PC.PlayerUid).Nickname + "$z$> $s$000» $i$fffAway from keyboard!");
                        ServerManager.Server.SetSpectator(PC.Login);
                        break;
                    }
                case "back":
                    {
                        ServerManager.Server.ChatSendServerMessage(ServerManager.GetPlayer(PC.PlayerUid).Nickname + "$z$> $s$4f4» $i$fffBack!");
                        ServerManager.Server.SetPlayer(PC.Login);
                        break;
                    }
            }

		}
	}
}


