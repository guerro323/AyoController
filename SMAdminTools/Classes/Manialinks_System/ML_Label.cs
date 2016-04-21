using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AyoController
{
    public class Label : NodesVisible
    {
        public string Text;
        public double Textsize = 0.01456297;
        public Label(string name, Vector3 posn, Vector2 size, bool hidden)
        {
            this.Name = name;
            Position = posn;
            this.Size = size;
            this.Hidden = hidden;

            Childs = new List<Nodes>();
            Parents = new List<Nodes>();
        }

        public override void OnBuild()
        {
            BuildResult = "<label ";
            if (substyle != "") BuildResult += "substyle=\"" + substyle + "\" ";
            if (style != "") BuildResult += "style=\"" + style + "\" ";
            if (Halign != ManialinkSystem.Halign.None) BuildResult += "halign=\"" + AlignToText(Halign) + "\" ";
            if (Valign != ManialinkSystem.Valign.None) BuildResult += "valign=\"" + AlignToText(Valign) + "\" ";
            if (Textsize != 0.01456297) BuildResult += "textsize=\"" + Textsize + "\" ";
            if (Scale != 0.01456297) BuildResult += "scale=\"" + Scale + "\" ";
            BuildResult += "id=\"" + (GetHashCode().ToString() + Name) + "\" ";
            BuildResult += "posn=\"" + Position.X + " " + Position.Y + " " + Position.X + "\" ";
            BuildResult += "sizen=\"" + Size.X + " " + Size.Y + " " + Size.X + "\" ";
            BuildResult += "text=\"" + Text + "\" ";
            BuildResult += "hidden=\"" + Hidden + "\" ";
            BuildResult += "/>";
            BuildResult = BuildResult.Replace(",", ".");
            return;
        }
    }
}
