using AyoController.Classes;
using AyoController.Plugins;
using ShootManiaXMLRPC.XmlRpc;

namespace Manialink
{
    public partial class Manialink : Plugin
    {
        public override string Version
        {
            get
            {
                return "0.1";
            }
        }
        public override string Name
        {
            get
            {
                return "Manialinks";
            }
        }
        public override string Author
        {
            get
            {
                return "Guerro";
            }
        }
        public override string[] listofCommands
        {
            get
            {
                return new string[] { "/reload" };
            }
        }
        public override void OnLoad()
        {

        }
        public override void OnServerManagerInitialize(ServerManager ServerManager)
        {

        }
        public override void OnLoop()
        {

        }
        public override void HandleEventGbxCallback(object o, GbxCallbackEventArgs e)
        {

        }
        public override void OnConsoleCommand(string Command)
        {

        }
    }
}
