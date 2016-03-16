using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShootManiaXMLRPC.Structs
{
    public class Management
    {
        public int Time;
        public int Id;
        public string Login;
        public string Pseudo;
        public int Rank;
    }

    public class PlayerList
    {
        public string Login;
        public string Nickname;
        public int PlayerId;
        public int TeamId;
        public int SpectatorStatus;
        public int LadderRanking;
        public int Flags;
        public Management Management;
    }
}
