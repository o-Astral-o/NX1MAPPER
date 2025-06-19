using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedFox.Graphics3D
{
    [DebuggerDisplay("Name = {Name}")]
    public abstract class Graphics3DObject(string name)
    {
        public string Name { get; set; } = name;
    }
}
