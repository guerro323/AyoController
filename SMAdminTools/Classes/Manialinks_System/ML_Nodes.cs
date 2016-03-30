using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AyoController;

namespace AyoController
{

    public class Nodes
    {
        public string name = "";
        public Vector3 position = new Vector3(0, 0, 0);
        public Vector2 size = new Vector2(0, 0);
        public bool hidden = false;
        public string BuildResult = "";
        public ManialinkSystem.Halign Halign = ManialinkSystem.Halign.None;
        public ManialinkSystem.Valign Valign = ManialinkSystem.Valign.None;
        public int id
        {
            get
            {
                return name.GetHashCode();
            }
        }

        public string AlignToText(object Align)
        {
            if (Align.GetType() == typeof(ManialinkSystem.Halign))
            {
                var _Align = (ManialinkSystem.Halign)Align;
                if (_Align == ManialinkSystem.Halign.Bottom) return "bottom";
                if (_Align == ManialinkSystem.Halign.Top) return "top";
                if (_Align == ManialinkSystem.Halign.Left) return "left";
                if (_Align == ManialinkSystem.Halign.Right) return "right";
                if (_Align == ManialinkSystem.Halign.Center) return "center";
            }
            if (Align.GetType() == typeof(ManialinkSystem.Valign))
            {
                var _Align = (ManialinkSystem.Valign)Align;
                if (_Align == ManialinkSystem.Valign.Bottom) return "bottom";
                if (_Align == ManialinkSystem.Valign.Top) return "top";
                if (_Align == ManialinkSystem.Valign.Left) return "left";
                if (_Align == ManialinkSystem.Valign.Right) return "right";
                if (_Align == ManialinkSystem.Valign.Center) return "center";
            }
            return "";
        }

        public void Translate(double x, double y, double z)
        {
            if (parents.Count == 0)
            {
                this.position = new Vector3(this.position.X + x, this.position.Y + y, this.position.Z + z);
            }
            else
            {
                foreach (var par in parents)
                {
                    this.position = new Vector3(((par.position.X) - (this.position.X + x)), ((par.position.Y) - (this.position.Y + y)), ((par.position.Z) - (this.position.Z + z)));
                }
            }

            if (autobuild) AutoBuild();
        }

        public virtual void OnBuild()
        {

        }

        /// <summary>
        /// Did the object will autobuild himself when a propety is changed?
        /// </summary>
        /// <param name="i">True or False</param>
        public bool AutoBuild(bool i)
        {
            autobuild = i;
            OnBuild();
            return autobuild;
        }
        public bool AutoBuild()
        {
            return this.AutoBuild(true);
        }

        public bool autobuild = true;

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

            if (autobuild) AutoBuild();
        }
        public void Style(string Style)
        {
            this.style = Style;

            if (autobuild) AutoBuild();
        }
        public void Substyle(string Substyle)
        {
            this.substyle = Substyle;

            if (autobuild) AutoBuild();
        }
    }

    public class childOf
    {
        public childOf(Nodes child, Nodes parent)
        {
            child.parents.Add(parent);
            parent.childs.Add(child);

            if (child.autobuild) child.AutoBuild();
            if (parent.autobuild) parent.AutoBuild();
        }
        public void removeFrom(Nodes child, Nodes parent)
        {
            parent.childs.Remove(child);

            if (child.autobuild) child.AutoBuild();
            if (parent.autobuild) parent.AutoBuild();
        }
    }
    public class parentOf
    {
        public parentOf(Nodes parent, Nodes child)
        {
            parent.childs.Add(child);

            if (child.autobuild) child.AutoBuild();
            if (parent.autobuild) parent.AutoBuild();
        }
        public void removeFrom(Nodes parent, Nodes child)
        {
            child.parents.Remove(parent);

                        if (child.autobuild) child.AutoBuild();
            if (parent.autobuild) parent.AutoBuild();
        }
    }
}
