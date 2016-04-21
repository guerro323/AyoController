using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AyoController;
using AyoController.Plugins;

namespace Commands
{
    partial class Commands
	{
        private static string RandomHelloAll()
        {
            Random rand = new Random();
            int result = rand.Next(1, 4);
            switch (result)
            {
                case 1:
                    {
                        return "Hello all!";
                    }
                case 2:
                    {
                        return "Hey all!";
                    }
                case 3:
                    {
                        return "Hi alll!";
                    }
            }
            return "Hey!";
        }
        private static string RandomHello()
        {
            Random rand = new Random();
            int result = rand.Next(1, 4);
            switch (result)
            {
                case 1:
                    {
                        return "Hello!";
                    }
                case 2:
                    {
                        return "Hey!";
                    }
                case 3:
                    {
                        return "Hi!";
                    }
            }
            return "Hey!";
        }

        private readonly string[] _badWords = new string[] { "cazzo", "merda", "kurwa", "fuck off", "dick", "fuck", "anus", "merde", "putain" };

        private void ParseChatCommand(ShootManiaXMLRPC.Structs.PlayerChat pc)
        {
            foreach (var badword in _badWords.Where(badword => pc.Text.Contains(badword)))
            {
                ChatSendServerMessage("Bad word detected!");
                ServerManager.AddThisManialink(pc.Login, @"<label text=""fuck you then"" />", "Warning", true);
                ServerManager.AddThisManialink(pc.Login, @"<label posn=""0 -10 0"" text=""xd"" />", "Warning2", true);
            }

            if (!string.IsNullOrEmpty(pc.Text))
            {
                if (pc.Text.StartsWith("/help"))
                {
                    var param = pc.Text.Replace("/help ", "");
                    if (param != "full")
                    {
                        var result = "";
                        var rankCommand = 1;
                        foreach (Plugin pl in ServerManager.LoadedPlugins)
                        {
                            if (rankCommand > ServerManager.LoadedPlugins.Count + 2) break;
                            if (!ServerManager.Commands.ContainsKey(pl.Name) ||
                                !ServerManager.Commands[pl.Name].Any()) break;
                            var PluginBetterName = AyoController.AyO.BeautifyString(AyoController.AyO.BeautifyType.UnderScore ,pl.Name);
                            result += "\n$z$>$s$f70» $i$ddd" + PluginBetterName + "$fff : \n$555$i|$i$fff    ";
                            if (ServerManager.Commands[pl.Name].Path != "")
                                result += "$o$i$n"+ ServerManager.Commands[pl.Name].Path + "$z$s$i$fff\n    $333$i|$i$fff   ";
                            foreach (var cmd in ServerManager.Commands[pl.Name].Commands)
                            {
                                result += cmd.Command;
                                if (cmd.Params != null && cmd.Params.Any())
                                {
                                    result += "$0f0";
                                    foreach (var _param in cmd.Params)
                                    {
                                        result += " '" + _param + "'";
                                    }
                                }
                                result += "$fff, ";
                            }
                            rankCommand++;
                        }
                        pc.Server.ChatSendToLogin(pc.Login, "$fff[$4f4Available Commands$fff]" + result);
                        pc.Server.ChatSendToLogin(pc.Login, "    $9f9» Open Chat History to see more.");
                        pc.Server.ChatSendToLogin(pc.Login, "    $4f4» Type /help full to see in normal format.");
                        pc.Server.ChatSendToLogin(pc.Login, "to you : " + pc.Login);
                        Console.WriteLine(pc.Server == ServerManager.Server);
                    }
                    else
                    {
                        var result = "";
                        var rankCommand = 1;
                        foreach (Plugin pl in ServerManager.LoadedPlugins)
                        {
                            if (rankCommand > ServerManager.LoadedPlugins.Count + 2) break;
                            if (!ServerManager.Commands.ContainsKey(pl.Name) ||
                                ServerManager.Commands[pl.Name].Any()) break;
                            result += "$z$> $s$f70» $i$ddd" + pl.Name + "$fff : $i";
                            if (ServerManager.Commands[pl.Name].Path != "")
                                result += ServerManager.Commands[pl.Name].Path + " $555-> ";
                            foreach (var cmd in ServerManager.Commands[pl.Name].Commands)
                            {
                                result += cmd.Command + ", ";

                            }
                            rankCommand++;
                        }
                        pc.Server.ChatSendToLogin(pc.Login, "$fff[$4f4Available Commands$fff]\n" + result);
                        pc.Server.ChatSendToLogin(pc.Login, "    $9f9» Open Chat History to see more.");
                        pc.Server.ChatSendToLogin(pc.Login, "    $4f4» Type /help to see in original format.");
                    }
                }
                if (pc.Text == "/hia")
                {
                    ServerManager.Server.ChatSendServerMessage(ServerManager.GetPlayer(pc.PlayerUid).Nickname + "$z$> $s$999» $i$fff" + RandomHelloAll() + "!");
                }
                if (pc.Text == "/hi")
                {
                    ServerManager.Server.ChatSendServerMessage(ServerManager.GetPlayer(pc.PlayerUid).Nickname + "$z$> $s$0f0» $i$fff" + RandomHello() + "!");
                }
                if (pc.Text == "afk")
                {
                    ServerManager.Server.ChatSendServerMessage(ServerManager.GetPlayer(pc.PlayerUid).Nickname + "$z$> $s$000» $i$fffAway from keyboard!");
                    ServerManager.Server.SetSpectator(pc.Login); ;
                }
                if (pc.Text == "back")
                {
                    ServerManager.Server.ChatSendServerMessage(ServerManager.GetPlayer(pc.PlayerUid).Nickname + "$z$> $s$4f4» $i$fffBack!");
                    ServerManager.Server.SetPlayer(pc.Login);
                }
            }
        }
	}
}


