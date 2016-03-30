using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AyoController
{
    public class Label : NodesVisible
    {
        public string text;
        public Label(string name, Vector3 posn, Vector2 size, bool hidden)
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
            this.BuildResult = "<label ";
            if (this.substyle != "") this.BuildResult += "substyle=\"" + this.substyle + "\" ";
            if (this.style != "") this.BuildResult += "style=\"" + this.style + "\" ";
            this.BuildResult += "id=\"" + (this.GetHashCode().ToString() + name) + "\" ";
            this.BuildResult += "posn=\"" + this.position.X + " " + this.position.Y + " " + this.position.X + "\" ";
            this.BuildResult += "sizen=\"" + this.size.X + " " + this.size.Y + " " + this.size.X + "\" ";
            this.BuildResult += "text=\"" + this.text + "\" ";
            this.BuildResult += "hidden=\"" + hidden + "\" ";
            this.BuildResult += "/>";
            return;
        }
    }
}
