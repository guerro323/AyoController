using System;
using ShootManiaXMLRPC;
using ShootManiaXMLRPC.XmlRpc;

namespace AyoController.Plugins
{
    public partial class Plugin
    {
        public virtual Classes.ServerManager ServerManager { get; set; }

        public virtual AyO.PluginFunction[] PluginFunction { get; set; }

        public virtual string Name { get; set; }

        public virtual string Author { get; set; }

        public virtual string Version { get; set; }

        public virtual AyO.HelpCommands ListofCommands
        {
            get { return new AyO.HelpCommands(); }
        }

        public bool Loaded;

        public virtual void Nothing()
        {

        }

        public virtual void OnLoad()
        {
        }

        public virtual void OnEverythingLoaded()
        {

        }

        public virtual void OnServerManagerInitialize(Classes.ServerManager serverManager)
        {
            ServerManager = serverManager;
            ServerManager.OnConnectionSuccessful += HandleOnConnectionSuccessful;

            ServerManager.Instance_CreateNewFile(Name, "config.txt", "", Nothing);
        }

        public virtual void HandleOnConnectionSuccessful()
        {
            ServerManager.Server.OnPlayerChat += HandleOnPlayerChat;
            ServerManager.Server.OnPlayerConnect += HandleOnPlayerConnect;
        }

        public virtual void HandleOnPlayerChat(ShootManiaXMLRPC.Structs.PlayerChat pc)
        {

        }

        public virtual void HandleOnPlayerConnect(ShootManiaXMLRPC.Structs.PlayerConnect pc)
        {

        }

        public virtual void HandleOnModeScriptCallback(ShootManiaXMLRPC.Structs.ModeScriptCallback msc)
        {

        }

        public virtual void OnConsoleCommand(string command)
        {

        }

        public virtual void OnLoop()
        {
        }

        public virtual void HandleEventGbxCallback(object o, ShootManiaXMLRPC.XmlRpc.GbxCallbackEventArgs e)
        {

        }

        public virtual void HandleFixedGbxCallBacks(GbxCallbackEventArgs reponse, string methodname,
            ShootManiaServer maniaServer)
        {
        }

        public virtual void OnCustomEvent(object[] responseObjects, string eventName)
        {
        }

        public virtual void Unload()
        {

        }
    }
}

