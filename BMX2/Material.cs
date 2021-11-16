using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MHPB2_LevelTool
{

    public class Material
    {
        public string m__name;
        public uint m__num_texture_passes;
        public List<TexturePass> m__texture_passes;

        // TODO: add blend params
        // TODO: add uv animation params
        // TODO: support multiple shader / texture passes

        public Material()
        {
            m__name = "DefaultMaterial";
            m__texture_passes = new List<TexturePass>();
        }

        public class TexturePass
        {
            public string m__textureMap;
            public uint m__alpha_ref;
            public uint m__blend_color;
            public TextureBlendMode m__blend_mode;
            public bool m__transparent;
            public TextureAnimation m__textureAnimation;
            public List<Texture> m__textures;

            public TexturePass()
            {
                m__textureMap = "NoTexture";
                m__alpha_ref = 1;
                m__blend_color = 0;
                m__transparent = false;
                m__blend_mode = TextureBlendMode.DIFFUSE;
                m__textureAnimation = new TextureAnimation();
                m__textures = new List<Texture>();
            }
        }

        public enum TextureBlendMode
        {
            DIFFUSE,
            ADD,
            ADD_FIXED,
            SUBTRACT,
            SUB_FIXED,
            BLEND,	
            BLEND_FIXED,
            MODULATE,
            MODULATE_FIXED,
            BRIGHTEN,
            BRIGHTEN_FIXED,
            GLOSS_MAP,
            BLEND_PREVIOUS_MASK,
            BLEND_INVERSE_PREVIOUS_MASK,
            ADD_PREVIOUS_ALPHA
        };

        public enum TextureAnimationType
        {
            NONE = 0,
            UNIQUE,
            UV
        }

        public class TextureAnimation
        {
            // TODO: Convert params to thps format

            public TextureAnimationType m__type;
            public int m__num_frames;
            public float animatedMapInitialFrame;
            public float animatedMapFramesPerSecond;
            public int animatedMapLooping;
            public int animatedMapReverseLoopOnEnd;
            public int animatedMapLoopOnEndToFrame;

            public float u_velocity;
            public float v_velocity;

            public TextureAnimation()
            {
                m__type = TextureAnimationType.NONE;
            }
        }

        public Material GetMaterialByName(string name, List<Material> materials)
        {
            for (int i = 0; i < materials.Count(); i++)
            {
                if (materials[i].m__name.ToLower() == name.ToLower())
                    return materials[i];
            }
            return null;
        }

        public int GetMaterialIndexByName(string name, List<Material> materials)
        {
            for (int i = 0; i < materials.Count(); i++)
            {
                if (materials[i].m__name.ToLower() == name.ToLower())
                    return i;
            }
            return -1;
        }
    }
}
