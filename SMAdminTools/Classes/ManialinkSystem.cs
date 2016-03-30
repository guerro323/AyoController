using AyoController.Classes;
using AyoController.Plugins;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using ShootManiaXMLRPC.XmlRpc;

namespace AyoController
{
    public partial class ManialinkSystem : Plugin
    {
        public List<Nodes> CurrentNodes = new List<Nodes>();
        public override AyO.PluginFunction PluginFunction
        {
            get
            {
                return AyO.PluginFunction.Global;
            }
        }

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

        public string CurrentBuild;

        public enum Halign
        {
            Bottom,
            Top,
            Left,
            Right,
            Center,
            Center2,
            None
        }

        public enum Valign
        {
            Bottom,
            Top,
            Left,
            Right,
            Center,
            Center2,
            None,
        }

        public string Construct()
        {
            List<Nodes> NextStage = new List<Nodes>();
            string ToReturnString = "";
            foreach (var Node in CurrentNodes)
            {
                if (Node == null) continue;
                else
                {
                    if (Node.childs.Count > 0)
                    {
                        NextStage.Add(Node);
                    }
                    else if (Node.parents.Count <= 0) {
                        if (Node.autobuild) Node.AutoBuild();
                        ToReturnString += Node.BuildResult;
                        Console.WriteLine("test");
                    }
                }
            }
            foreach (var Node in NextStage)
            {
                ToReturnString += "<frame posn=\"" + Node.position.X + " " + Node.position.Y + " " + Node.position.Z + "\">";

                foreach (var child in Node.childs)
                {
                    ToReturnString += child.BuildResult;
                }
                ToReturnString += "</frame>";
            }
            CurrentBuild = ToReturnString;
            Console.WriteLine(CurrentBuild);
            return CurrentBuild;
        }

        public AyO.ErrorLog Add(Nodes _node, bool _Construct)
        {
            if (_node != null)
            {
                this.CurrentNodes.Add(_node);
                if (_Construct) Construct();
                return new AyO.ErrorLog { IsError = false, ErrorString = "No error.", ErrorCode = -1 };
            }
            else
                return new AyO.ErrorLog { IsError = true, ErrorString = "There is an error, but AyO can't know why. Check if the manialink is correct", ErrorCode = 0 };
        }

        public void Add(Nodes _node)
        {
            this.Add(_node, false);
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
