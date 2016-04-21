using System;
using AyoController.Plugins;

namespace IGAdmin
{
    partial class IgAdmin
    {
        readonly string _prfx = "/admin ";
        private void ParseChatCommand(ShootManiaXMLRPC.Structs.PlayerChat pc)
        {
            if (pc.Text == "/admins")
            {
                foreach (string admin in _admins)
                {
                    ServerManager.Server.ChatSendToLogin(pc.Login, "Admin : " + admin);
                }
            }
            else if (pc.Text == "/players")
            {
                foreach (var player in ServerManager.GetPlayers())
                {
                    if (player.PlayerId > 0)
                    {
                        ServerManager.Server.ChatSendToLogin(pc.Login,"[" + player.PlayerId + "] " + player.Nickname);
                    }
                }
            }
            else if (pc.Text.StartsWith(_prfx + "addadmin") &&
              _admins.Contains(pc.Login))
            {

                string admin = pc.Text.Replace(_prfx + "addadmin", string.Empty);

                if (!_admins.Contains(admin))
                {
                    _admins.Add(admin);
                    SaveAdmins();
                    ChatSendServerMessage("Admin added : " + admin);
                }
                else {
                    ChatSendServerMessage("Admin already exist : " + admin);
                }

            }
            else if (pc.Text.StartsWith(_prfx + "deladmin") &&
              _admins.Contains(pc.Login))
            {

                string admin = pc.Text.Replace(_prfx + "deladmin", string.Empty);

                if (admin != pc.Login)
                {

                    if (_admins.Contains(admin))
                    {
                        _admins.Remove(admin);
                        SaveAdmins();
                        ChatSendServerMessage("Admin removed : " + admin);
                    }
                    else {
                        ChatSendServerMessage("Admin not found : " + admin);
                    }

                }
                else {
                    ChatSendServerMessage("You can't remove yourself !");
                }

            }
            else if (pc.Text == "!restartmap" &&
              _admins.Contains(pc.Login))
            {
                ChatSendServerMessage("Restart map ...");
                ServerManager.Server.RestartMap(false);
            }
            else if (pc.Text == _prfx + "skip" &&
              _admins.Contains(pc.Login))
            {
                ChatSendServerMessage("Next map ...");
                //ServerManager.Server.NextMap(false);
            }
            else if (pc.Text.StartsWith("!map ") &&
              _admins.Contains(pc.Login))
            {

                string newMap = pc.Text.Replace("!map ", string.Empty);

                foreach (var map in ServerManager.Server.GetMapList(1000, 0))
                {

                    if (map.FileName.ToLower().Contains(newMap.ToLower()))
                    {
                        ChatSendServerMessage("Map found : " + map.Name);
                        ServerManager.Server.ChooseNextMap(map.FileName);
                        ServerManager.Server.NextMap(false);
                        return;
                    }

                }

                ChatSendServerMessage("No map found with the pattern : " + newMap);

            }
            else if (pc.Text.StartsWith("!kick ") &&
              _admins.Contains(pc.Login))
            {

                try
                {

                    int playerId = Convert.ToInt32(pc.Text.Replace("!kick ", string.Empty));

                    ServerManager.Server.KickId(playerId, "Kicked by admin");
                    ChatSendServerMessage("Played kicked");

                }
                catch
                {
                    ChatSendServerMessage("Invalid player id !");
                }

            }
            else if (pc.Text.StartsWith("!ban ") &&
              _admins.Contains(pc.Login))
            {

                try
                {

                    int playerId = Convert.ToInt32(pc.Text.Replace("!ban ", string.Empty));

                    ServerManager.Server.BanId(playerId, "Banned by admin");
                    ChatSendServerMessage("Played banned");

                }
                catch
                {
                    ChatSendServerMessage("Invalid player id !");
                }

            }
            else if (pc.Text.StartsWith("!password ") &&
              _admins.Contains(pc.Login))
            {

                string newPassword = pc.Text.Replace("!password ", string.Empty);

                ServerManager.Server.SetServerPassword(newPassword);
                ChatSendServerMessage("Password set to : " + newPassword);

            }


        }
	}
}


