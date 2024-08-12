using ExodusVFX.Vfx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExodusVFX.Database
{
    public class MetroDatabase
    {
        public static MetroVFX vfx;

        public static void loadFromFile(string filePath)
        {
            MetroDatabase.vfx = MetroVFX.Read(filePath);
        }
    }
}
