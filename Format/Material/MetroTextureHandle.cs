using System.Numerics;

namespace ExodusVFX.Format.Material
{
    public class MetroTextureHandle : MetroHandleClass
    {
        public uint type;
        public byte texture_type;
        public string source_name;
        public Vector4 surf_xform;
        public uint format;
        public uint width;
        public uint height;
        public bool animated;
        public bool draft;
        public bool override_avg_color;
        public Vector4 avg_color;
        public string shader_name;
        public string gamemtl_name;
        public uint priority;
        public bool streamable;
        public float bump_height;
        public byte displ_type;
        public float displ_height;
        public float parallax_height_mul;
        public bool mipmapped;
        public float reflectivity;
        public bool treat_as_metal;
        public string det_name;
        public float det_scale_u;
        public float det_scale_v;
        public float det_intensity;
        public Vector4 aux_params;
        public Vector4 aux_params_1;
        public float[] sph_coefs; // not always avaiable
        public byte[] lum; // not always avaiable
        public string bump_name;
        public string aux0_name;
        public string aux1_name;
        public string aux2_name;
        public string aux3_name;
        public string aux4_name;
        public string aux5_name;
        public string aux6_name;
        public string aux7_name;

    }
}