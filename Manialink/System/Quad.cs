using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AyoController;

namespace Manialink.System
{
    public class Quad : NodesVisible
    {
        public Quad (string name, Vector3 posn, Vector3 size, bool hidden)
        {
            this.name = name;
            this.position = posn;
            this.size = size;
            this.hidden = hidden;

            childs = new List<Nodes>();
            parents = new List<Nodes>();
        }
    }
}
