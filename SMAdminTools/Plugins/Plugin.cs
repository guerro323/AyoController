using System;

namespace AyoController.Plugins
{
    public abstract class Plugin
    {

        public abstract String Name { get; }

        public abstract String Author { get; }

        public abstract String Version { get; }

        public abstract String[] listofCommands { get; }

        public abstract void OnLoad();

        public abstract void OnServerManagerInitialize(Classes.ServerManager ServerManager);

		public abstract void OnConsoleCommand(string Command);

        public abstract void OnLoop();

        public abstract void HandleEventGbxCallback(object o, ShootManiaXMLRPC.XmlRpc.GbxCallbackEventArgs e);

    }
}

