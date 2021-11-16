using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MHPB2_LevelTool
{

    public class Texture
    {
        public string m__name;
        public uint m__version;
        public uint m__dxt_type;
        public uint m__num_mips;
        public uint m__unknown;
        public uint m__width;
        public uint m__height;
        public List<Mips> m__mips;

        public Texture()
        {
            m__name = "";
            m__mips = new List<Mips>();
        }

        public Texture(BinaryReader br)
        {
            m__name = "";
            m__mips = new List<Mips>();
            m__version = br.ReadUInt32();
            m__dxt_type = br.ReadUInt32();
            m__num_mips = br.ReadUInt32();
            m__unknown = br.ReadUInt32();
            m__width = br.ReadUInt32();
            m__height = br.ReadUInt32();

            for (int i = 0; i < m__num_mips; i++)
            {
                m__mips.Add(new Mips(br));
            }

            // maybe mip level 0 ?
            if ((m__num_mips == 0) && (br.BaseStream.Position + 12 < br.BaseStream.Length))
            {
                m__num_mips = 1;
                m__mips.Add(new Mips(br));
            }
        }

        public class Mips
        {
            public uint m__width;
            public uint m__height;
            public int m__buffer_size;
            public byte[] m__data;

            public Mips()
            {
            }

            public Mips(BinaryReader br)
            {
                m__width = br.ReadUInt32();
                m__height = br.ReadUInt32();
                m__buffer_size = br.ReadInt32();

                if ((br.BaseStream.Position + m__buffer_size) > br.BaseStream.Length)
                    return;

                m__data = br.ReadBytes(m__buffer_size);
            }
        }

        static public Texture GetTextureByName(string name, List<Texture> textures)
        {
            for (int i = 0; i < textures.Count(); i++)
            {
                if (textures[i].m__name.ToLower() == name.ToLower())
                    return textures[i];
            }
            return null;
        }

        static public bool TextureNameExists(string name, List<Texture> textures)
        {
            for (int i = 0; i < textures.Count(); i++)
            {
                if (textures[i].m__name.ToLower() == name.ToLower())
                    return true;
            }
            return false;
        }

        // dump texture container
        public static void write_texture_container(string filename, List<Texture> textures)
        {
            // HACK!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // HACK!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // HACK!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            using (BinaryWriter writer = new BinaryWriter(File.Create(filename)))
            {
                writer.Write(1);
                writer.Write(textures.Count());

                for (int i = 0; i < textures.Count(); i++)
                {
                    writer.Write(QB.GenerateCRC(textures[i].m__name));
                    writer.Write(textures[i].m__width);
                    writer.Write(textures[i].m__height);

                    /*
                    writer.Write(textures[i].m__mips.Count());
                    */
                    writer.Write(1);

                    writer.Write(0x20);
                    writer.Write(0x20);

                    if (textures[i].m__dxt_type == 2)
                    {
                        writer.Write(5);
                    }
                    else
                    {
                        writer.Write(1);
                    }

                    writer.Write(0);

                    /*
                    for (int m = 0; m < textures[i].m__mips.Count(); m++)
                    {
                        writer.Write(textures[i].m__mips[m].m__buffer_size);
                        writer.Write(textures[i].m__mips[m].m__data);
                    }
                    */

                    writer.Write(textures[i].m__mips[textures[i].m__mips.Count()-1].m__buffer_size);
                    writer.Write(textures[i].m__mips[textures[i].m__mips.Count()-1].m__data);
                }
                writer.Close();
            }
        }

        // dump dds textures to folder
        public static void dump_textures(string path, List<Texture> textures)
        {
            // HACK!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // HACK!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            // HACK!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            for (int i = 0; i < textures.Count(); i++)
            {
                using (BinaryWriter writer = new BinaryWriter(File.Create(Path.Combine(path, String.Format("{0}.dds", textures[i].m__name)))))
                {
                    writer.Write(0x20534444); // "DDS "
                    writer.Write(0x0000007c);
                    writer.Write(0x00a01007);
                    writer.Write(textures[i].m__height);
                    writer.Write(textures[i].m__width);
                    writer.Write(textures[i].m__mips[textures[i].m__mips.Count() - 1].m__buffer_size);
                    writer.Write(0x00000000);
                    writer.Write(0x00000007);
                    writer.Write(new byte[44]);
                    writer.Write(0x00000020);
                    writer.Write(0x00000004);

                    if (textures[i].m__dxt_type == 2)
                        writer.Write(0x32545844);
                    else
                        writer.Write(0x31545844);

                    writer.Write(new byte[20]);

                    writer.Write(0x00401008);

                    writer.Write(new byte[16]);

                    writer.Write(textures[i].m__mips[textures[i].m__mips.Count() - 1].m__data);

                    writer.Close();
                }
            }
        }
    }

}
