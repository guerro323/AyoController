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
		public override AyO.PluginFunction[] PluginFunction
		{
			get
			{
				return new AyO.PluginFunction[] {
					AyO.PluginFunction.Global
				};
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
        public override AyO.HelpCommands ListofCommands
        {
            get
            {
                return new AyO.HelpCommands()
                {
                    Path = "/ml",
                    Commands = new[] {new AyO._Commands {Command = "rebuild", Params = new [] { "v|name" }}}
                };
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

        public string ScriptMain = "";
        public string ScriptWhile = "";

        public string Construct()
        {
            List<Nodes> nextStage = new List<Nodes>();
            string toReturnString = "";
            ScriptMain = "<script>";
            ScriptMain += "<!-- \n";
            ScriptMain += "main() {\n";
            foreach (var node in CurrentNodes)
            {
                if (node == null) continue;
                else
                {
                    if (node.Autobuild) node.AutoBuild();
                    // Render the script
                    //
                    if (node.Usescriptevents)
                    {
                        ScriptMain += "declare CMl" + node.ToString().Replace("AyoController.", "") + " node_" + node.GetHashCode() + " <=> (Page.GetFirstChild(\"" + (node.GetHashCode().ToString() + node.Name) + "\") as CMl" + node.ToString().Replace("AyoController.", "") + ");\n";
                    }

                    if (node.Childs.Count > 0)
                    {
                        nextStage.Add(node);
                    }
                    else if (node.Parents.Count <= 0) {     
                        toReturnString += node.BuildResult;
                    }
                }
            }
            foreach (var node in nextStage)
            {
                var scale = "";
                if (node.Scale != 0.01456297) scale = "scale=\"" + scale + "\" ";
                toReturnString += "<frame posn=\"" + node.Position.X + " " + node.Position.Y + " " + node.Position.Z + "\" " + scale +">";

                foreach (var child in node.Childs)
                {
                    child.AutoBuild();
                    ScriptMain += "declare CMl" + child.ToString().Replace("AyoController.", "") + " node_" + child.GetHashCode() + " <=> (Page.GetFirstChild(\"" + (child.GetHashCode().ToString() + child.Name) + "\") as CMl" + child.ToString().Replace("AyoController.", "") + ");\n";
                    toReturnString += child.BuildResult;
                }
                toReturnString += "</frame>";
            }
            ScriptWhile = "while(True) { yield;\n ";
            ScriptWhile += "foreach (Event in PendingEvents) {\n";
            ScriptWhile += "if (Event.Type == CMlEvent::Type::MouseClick) TriggerPageAction(Event.ControlId);";
            ScriptWhile += "}\n}";
            ScriptMain += ScriptWhile;
            ScriptMain += "}\n";
            ScriptMain += "--></script>";
            CurrentBuild = toReturnString + ScriptMain;
            return CurrentBuild;
        }



        public AyO.ErrorLog Add(Nodes node, bool construct)
        {
            if (node != null)
            {
                CurrentNodes.Add(node);
                if (construct) Construct();
                return new AyO.ErrorLog { IsError = false, ErrorString = "No error.", ErrorCode = -1 };
            }
            else
                return new AyO.ErrorLog { IsError = true, ErrorString = "There is an error, but AyO can't know why. Check if the manialink is correct", ErrorCode = 0 };
        }

        public void Add(Nodes node)
        {
            Add(node, false);
        }

        public override void OnLoad()
        {

        }
        public override void OnServerManagerInitialize(ServerManager serverManager)
        {

        }
        public override void OnLoop()
        {

        }
        public override void HandleEventGbxCallback(object o, GbxCallbackEventArgs e)
        {

        }
        public override void OnConsoleCommand(string command)
        {

        }
    } 
}


















