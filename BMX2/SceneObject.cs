using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MHPB2_LevelTool
{
    public class SceneObject
    {
        public string m__name;
        public uint m__num_materials;
        public List<string> m__materials;

        public uint m__num_meshes;
        public List<Mesh> m__meshes;

        public SceneObject()
        {
            m__materials = new List<string>();
            m__meshes = new List<Mesh>();
        }

        public class Mesh
        {
            public ushort m__num_verts;
            public ushort m__num_indices;
            public uint m__flags;

            public List<ushort> m__indices;

            public List<Vec3> m__positions;
            public List<Vec3> m__normals;
            public List<uint> m__colors;
            public List<Vec2> m__texcoords;

            public Mesh()
            {
                m__indices = new List<ushort>();
                m__positions = new List<Vec3>();
                m__normals = new List<Vec3>();
                m__colors = new List<uint>();
                m__texcoords = new List<Vec2>();
            }
        }

        public SceneObject GetObjectByName(string name, List<SceneObject> objects)
        {
            for (int i = 0; i < objects.Count(); i++)
            {
                if (objects[i].m__name.ToLower() == name.ToLower())
                    return objects[i];
            }
            return null;
        }

        static public bool ObjectNameExists(string name, List<SceneObject> objects)
        {
            for (int i = 0; i < objects.Count(); i++)
            {
                if (objects[i].m__name.ToLower() == name.ToLower())
                    return true;
            }
            return false;
        }

        public int GetObjectIndexByName(string name, List<SceneObject> objects)
        {
            for (int i = 0; i < objects.Count(); i++)
            {
                if (objects[i].m__name.ToLower() == name.ToLower())
                    return i;
            }
            return -1;
        }

        public static string GetUniqueName(string name, List<SceneObject> objects)
        {
            string unique_name = "";
            int _newNameIndex = 0;

            if (!SceneObject.ObjectNameExists(name, objects))
            {
                unique_name = name;
            }
            else
            {
                while (SceneObject.ObjectNameExists(String.Format("{0}_{1}", name, _newNameIndex.ToString("D2")), objects))
                {
                    _newNameIndex++;
                }

                unique_name = String.Format("{0}_{1}", name, _newNameIndex.ToString("D2"));
            }

            return unique_name;
        }
    }
}
