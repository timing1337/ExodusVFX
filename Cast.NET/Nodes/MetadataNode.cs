using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cast.NET.Nodes
{
    public class MetadataNode : CastNode
    {
        public string Author => GetStringValueOrDefault("a", string.Empty);
        public string Software => GetStringValueOrDefault("s", string.Empty);
        public string UpAxis => GetStringValueOrDefault("up", string.Empty);

        public MetadataNode() : base(CastNodeIdentifier.Metadata) { }

    }
}
