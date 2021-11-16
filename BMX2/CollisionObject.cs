using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MHPB2_LevelTool
{
    public enum COLLISON_FLAGS
    {
        WALL,
        VERT,
        SPINE,
        CAGE,
        RAMP,
        TRIGGER,
        DEFAULT
    }

    public class CollisionObject
    {
        public COLLISON_FLAGS m__flags;
        public string m__name;
        public uint m__num_verts;
        public uint m__num_triangles;
        public List<Vec3> m__positions;
        public List<Triangle> m__triangles;

        public CollisionObject()
        {
            m__flags = COLLISON_FLAGS.DEFAULT;
            m__positions = new List<Vec3>();
            m__triangles = new List<Triangle>();
        }

        public class Triangle
        {
            public byte[] m__unknown; // 16 bytes
            public ushort m__a;
            public ushort m__b;
            public ushort m__c;

            public Triangle()
            {
            }
        }

        public CollisionObject GetObjectByName(string name, List<CollisionObject> objects)
        {
            for (int i = 0; i < objects.Count(); i++)
            {
                if (objects[i].m__name.ToLower() == name.ToLower())
                    return objects[i];
            }
            return null;
        }

        public int GetObjectIndexByName(string name, List<CollisionObject> objects)
        {
            for (int i = 0; i < objects.Count(); i++)
            {
                if (objects[i].m__name.ToLower() == name.ToLower())
                    return i;
            }
            return -1;
        }
    }

}
