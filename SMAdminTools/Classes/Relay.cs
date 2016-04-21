using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using RelayServer;

namespace AyoController.Classes
{
    public class Relay
    {
        public RelayServer Server;
        private ServerManager _serverManager;

        public Relay(ServerManager currentServerManager)
        {
            // Get the relayServer
            Server = new RelayServer("", 0, false);
            _serverManager = currentServerManager;
        }

        public AyO.ErrorLog SetVar(object login, string var, string value)
        {
            if (login is string)
            {
                // To only one player

                return new AyO.ErrorLog
                {
                    ErrorCode = -1,
                    ErrorString = "No error.",
                    IsError = false
                };
            }
            else if (login is string[])
            {
                // To multiple players

                return new AyO.ErrorLog
                {
                    ErrorCode = -1,
                    ErrorString = "No error.",
                    IsError = false
                };
            }
            else
            {
                // To Everyone
                foreach (var @player in _serverManager.GetPlayers())
                {
                    SetVar(@player.Login, var, value);
                }
                return new AyO.ErrorLog
                {
                    ErrorCode = -1,
                    ErrorString = "No error.",
                    IsError = false
                };
            }

            return new AyO.ErrorLog
            {
                ErrorCode = 0,
                ErrorString = "There is a problem; but AyO can't know why",
                IsError = true
            };
        }
    }

    public class RelayServer
    {
        public string Domain;

        public RelayServer(string domain, int port, bool forced)
        {
            Domain = domain;
        }
    }
}
