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
        public string Name = "";
        public Vector3 Position = new Vector3(0, 0, 0);
        public Vector2 Size = new Vector2(0, 0);
        public bool Hidden = false;
        public double Scale = 0.01456297;
        public bool Usescriptevents = false;
        public string BuildResult = "";
        public ManialinkSystem.Halign Halign = ManialinkSystem.Halign.None;
        public ManialinkSystem.Valign Valign = ManialinkSystem.Valign.None;
        public int Id
        {
            get
            {
                return Name.GetHashCode();
            }
        }

        public string AlignToText(object align)
        {
            if (align.GetType() == typeof(ManialinkSystem.Halign))
            {
                var _Align = (ManialinkSystem.Halign)align;
                if (_Align == ManialinkSystem.Halign.Bottom) return "bottom";
                if (_Align == ManialinkSystem.Halign.Top) return "top";
                if (_Align == ManialinkSystem.Halign.Left) return "left";
                if (_Align == ManialinkSystem.Halign.Right) return "right";
                if (_Align == ManialinkSystem.Halign.Center) return "center";
            }
            if (align.GetType() == typeof(ManialinkSystem.Valign))
            {
                var _Align = (ManialinkSystem.Valign)align;
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
            if (Parents.Count == 0)
            {
                Position = new Vector3(Position.X + x, Position.Y + y, Position.Z + z);
            }
            else
            {
                foreach (var par in Parents)
                {
                    Position = new Vector3(((par.Position.X) - (Position.X + x)), ((par.Position.Y) - (Position.Y + y)), ((par.Position.Z) - (Position.Z + z)));
                }
            }

            if (Autobuild) AutoBuild();
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
            Autobuild = i;
            OnBuild();
            return Autobuild;
        }
        public bool AutoBuild()
        {
            return AutoBuild(true);
        }

        public bool Autobuild = true;

        public List<Nodes> Childs = new List<Nodes>();
        public List<Nodes> Parents = new List<Nodes>();
    }

    public class NodesVisible : Nodes
    {
        public double Opacity = 1;
        public string style = "";
        public string substyle = "";
        public string Redirecturl = "";

        public void Style(string Style, string Substyle)
        {
            style = Style;
            substyle = Substyle;

            if (Autobuild) AutoBuild();
        }
        public void Style(string Style)
        {
            style = Style;

            if (Autobuild) AutoBuild();
        }
        public void Substyle(string Substyle)
        {
            substyle = Substyle;

            if (Autobuild) AutoBuild();
        }
    }

    public class ChildOf
    {
        public ChildOf(Nodes child, Nodes parent)
        {
            child.Parents.Add(parent);
            parent.Childs.Add(child);

            if (child.Autobuild) child.AutoBuild();
            if (parent.Autobuild) parent.AutoBuild();
        }
        public void RemoveFrom(Nodes child, Nodes parent)
        {
            parent.Childs.Remove(child);

            if (child.Autobuild) child.AutoBuild();
            if (parent.Autobuild) parent.AutoBuild();
        }
    }
    public class ParentOf
    {
        public ParentOf(Nodes parent, Nodes child)
        {
            parent.Childs.Add(child);

            if (child.Autobuild) child.AutoBuild();
            if (parent.Autobuild) parent.AutoBuild();
        }
        public void RemoveFrom(Nodes parent, Nodes child)
        {
            child.Parents.Remove(parent);

                        if (child.Autobuild) child.AutoBuild();
            if (parent.Autobuild) parent.AutoBuild();
        }
    }
}
