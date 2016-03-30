using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using AyoController; 

namespace Manialink.System
{

    public class Nodes
    {
        public string name = "";
        public Vector3 position = new Vector3(0, 0, 0);
        public Vector3 size = new Vector3(0, 0, 0);
        public bool hidden = false;
        public int id
        {
            get
            {
                return name.GetHashCode();
            }
        }

        public bool autobuild = false;

        public List<Nodes> childs = new List<Nodes>();
        public List<Nodes> parents = new List<Nodes>();
    }

    public class NodesVisible : Nodes
    {
        public double opacity = 1;
        public string style = "";
        public string substyle = "";
        public string redirecturl = "";

        public void Style(string Style, string Substyle)
        {
            this.style = Style;
            this.substyle = Substyle;
        }
        public void Style(string Style)
        {
            this.style = Style;
        }
        public void Substyle(string Substyle)
        {
            this.substyle = Substyle;
        }
        public void Translate(double x, double y, double z)
        {
            this.position.X = this.position.X + x;
            this.position.Y = this.position.Y + y;
            this.position.Z = this.position.Z + z;
        }

        /// <summary>
        /// Did the object will autobuild himself when a propety is changed?
        /// </summary>
        /// <param name="i">True or False</param>
        public bool AutoBuild(bool i)
        {
            autobuild = i;
            return autobuild;
        }
        public bool AutoBuild()
        {
            return autobuild;
        }
    }

    public class childOf
    {
        public childOf(Nodes child, Nodes parent)
        {
            child.parents.Add(parent);
        }
        public void removeFrom(Nodes child, Nodes parent)
        {
            parent.childs.Remove(child);
        }
    }
    public class parentOf
    {
        public parentOf(Nodes parent, Nodes child)
        {
            parent.childs.Add(child);
        }
        public void removeFrom(Nodes parent, Nodes child)
        {
            child.parents.Remove(parent);
        }
    }
}
