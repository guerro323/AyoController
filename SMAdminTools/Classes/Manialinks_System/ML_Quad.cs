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
            this.name = name;
            this.position = posn;
            this.size = size;
            this.hidden = hidden;

            childs = new List<Nodes>();
            parents = new List<Nodes>();
        }

        public override void OnBuild()
        {
            this.BuildResult = "<quad ";
            if (this.substyle != "") this.BuildResult += "substyle=\"" + this.substyle + "\" ";
            if (this.style != "") this.BuildResult += "style=\"" + this.style + "\" ";
            if (this.Halign != ManialinkSystem.Halign.None) this.BuildResult += "halign=\"" + AlignToText(this.Halign) +  "\" ";
            if (this.Valign != ManialinkSystem.Valign.None) this.BuildResult += "valign=\"" + AlignToText(this.Valign) + "\" ";
            this.BuildResult += "id=\"" + (this.GetHashCode().ToString() + name) + "\" ";
            this.BuildResult += "posn=\"" + this.position.X + " " + this.position.Y + " " + this.position.Z + "\" ";
            this.BuildResult = this.BuildResult.Replace(",", ".");
            this.BuildResult += "sizen=\"" + this.size.X + " " + this.size.Y +  "\" ";
            this.BuildResult += "hidden=\"" + "false" + "\" ";
            this.BuildResult += "/>";
            return;
        }
    }
}
