using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShootManiaXMLRPC.Structs
{
    public struct PlayerChat
    {
        public int PlayerUid;
        public string Login;
        public string Text;
        /// <summary>
        /// Access to the server of where the player is located
        /// Useful for CrossServers or Multiples servers
        /// </summary>
        public ShootManiaServer Server;
        public bool IsRegisteredCmd;
    }
}
