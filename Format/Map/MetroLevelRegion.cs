using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExodusVFX.Format.Mesh;

namespace ExodusVFX.Format.Map
{
    public class MetroLevelRegion
    {
        public string name;
        public List<MetroMesh> meshes = new List<MetroMesh>();
        public MetroLevelRegion(string name)
        {
            this.name = name;
        }
    }
}
