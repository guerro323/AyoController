using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AyoController;

namespace AyoController
{
    public class Quad : NodesVisible
    {
        public Quad(string name, Vector3 posn, Vector2 size, bool hidden)
        {
            this.Name = name;
            Position = posn;
            this.Size = size;
            this.Hidden = hidden;
            Opacity = 1.0;

            Childs = new List<Nodes>();
            Parents = new List<Nodes>();
        }

        public override void OnBuild()
        {
            var scriptevents = 1;
            if (Usescriptevents) scriptevents = 1;
            else scriptevents = 0;

            BuildResult = "<quad ";
            if (substyle != "") BuildResult += "substyle=\"" + substyle + "\" ";
            if (style != "") BuildResult += "style=\"" + style + "\" ";
            if (Halign != ManialinkSystem.Halign.None) BuildResult += "halign=\"" + AlignToText(Halign) +  "\" ";
            if (Valign != ManialinkSystem.Valign.None) BuildResult += "valign=\"" + AlignToText(Valign) + "\" ";
            if (Scale != 0.01456297) BuildResult += "scale=\"" + Scale + "\" ";
            if (scriptevents == 1) BuildResult += "scriptevents=\"" + scriptevents + "\" ";
            if (Name == "") BuildResult += "id=\"" + (GetHashCode().ToString()) + "\" ";
            else BuildResult += "id=\"" + (Name) + "\" ";
            BuildResult += "posn=\"" + Position.X + " " + Position.Y + " " + Position.Z + "\"";
            BuildResult += "opacity=\"" + Opacity + "\" ";
            BuildResult += "sizen=\"" + Size.X + " " + Size.Y +  "\" ";
            BuildResult += "hidden=\"" + "false" + "\" ";
            BuildResult += "/>";

            BuildResult = BuildResult.Replace(",", ".");
            return;
        }
    }
}
