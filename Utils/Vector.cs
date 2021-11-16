using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace MHPB2_LevelTool
{
    public class Vec4
    {
        private float[] items;

        public bool isNull()
        {
            return (null == items);
        }

        public Vec4()
        {
            items = new float[4];
        }

        public Vec4(BinaryReader br)
        {
            items = new float[4];
            items[0] = br.ReadSingle();
            items[1] = br.ReadSingle();
            items[2] = br.ReadSingle();
            items[3] = br.ReadSingle();
        }

        public Vec4(float _x, float _y, float _z, float _w)
        {
            items = new float[4];
            items[0] = _x;
            items[1] = _y;
            items[2] = _z;
            items[3] = _w;
        }

        public Vec4(string _string)
        {
            if (_string != String.Empty)
            {
                _string = _string.Replace(" ", String.Empty);
                string[] _splitString = _string.Split(',');
                items = new float[4];
                items[0] = float.Parse(_splitString[0], System.Globalization.CultureInfo.InvariantCulture);
                items[1] = float.Parse(_splitString[1], System.Globalization.CultureInfo.InvariantCulture);
                items[2] = float.Parse(_splitString[2], System.Globalization.CultureInfo.InvariantCulture);
                items[3] = float.Parse(_splitString[3], System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        public Vec4(string[] _splitString)
        {
            items = new float[4];
            items[0] = float.Parse(_splitString[0], System.Globalization.CultureInfo.InvariantCulture);
            items[1] = float.Parse(_splitString[1], System.Globalization.CultureInfo.InvariantCulture);
            items[2] = float.Parse(_splitString[2], System.Globalization.CultureInfo.InvariantCulture);
            items[3] = float.Parse(_splitString[3], System.Globalization.CultureInfo.InvariantCulture);
        }

        public float x
        {
            get { return items[0]; }
            set { items[0] = value; }
        }

        public float y
        {
            get { return items[1]; }
            set { items[1] = value; }
        }

        public float z
        {
            get { return items[2]; }
            set { items[2] = value; }
        }

        public float w
        {
            get { return items[3]; }
            set { items[3] = value; }
        }

        public float this[int a]
        {
            get { return items[a]; }
            set { items[a] = value; }
        }

        public static implicit operator float[](Vec4 vec)
        {
            return vec.items;
        }

        public static implicit operator Vec4(float[] values)
        {
            var v = new Vec4();
            v.items = values;
            return v;
        }
    }

    public class Vec3
    {
        private float[] items;

        public bool isNull()
        {
            return (null == items);
        }

        public Vec3()
        {
            items = new float[3];
        }

        public Vec3(BinaryReader br)
        {
            items = new float[3];
            items[0] = br.ReadSingle();
            items[1] = br.ReadSingle();
            items[2] = br.ReadSingle();
        }

        public Vec3(float _x, float _y, float _z)
        {
            items = new float[3];
            items[0] = _x;
            items[1] = _y;
            items[2] = _z;
        }

        public Vec3(string _string)
        {
            if (_string != String.Empty)
            {
                _string = _string.Replace(" ", String.Empty);
                string[] _splitString = _string.Split(',');
                items = new float[3];
                items[0] = float.Parse(_splitString[0], System.Globalization.CultureInfo.InvariantCulture);
                items[1] = float.Parse(_splitString[1], System.Globalization.CultureInfo.InvariantCulture);
                items[2] = float.Parse(_splitString[2], System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        public Vec3(string[] _splitString)
        {
            items = new float[3];
            items[0] = float.Parse(_splitString[0], System.Globalization.CultureInfo.InvariantCulture);
            items[1] = float.Parse(_splitString[1], System.Globalization.CultureInfo.InvariantCulture);
            items[2] = float.Parse(_splitString[2], System.Globalization.CultureInfo.InvariantCulture);
        }

        public float x
        {
            get { return items[0]; }
            set { items[0] = value; }
        }

        public float y
        {
            get { return items[1]; }
            set { items[1] = value; }
        }

        public float z
        {
            get { return items[2]; }
            set { items[2] = value; }
        }

        public float this[int a]
        {
            get { return items[a]; }
            set { items[a] = value; }
        }

        public static implicit operator float[](Vec3 vec)
        {
            return vec.items;
        }

        public static implicit operator Vec3(float[] values)
        {
            var v = new Vec3();
            v.items = values;
            return v;
        }

        static public Vec3 Cross(Vec3 a, Vec3 b)
        {
            Vec3 retVec = new Vec3(0, 0, 0);
            retVec.x = a.y * b.z - a.z * b.y;
            retVec.y = a.z * b.x - a.x * b.z;
            retVec.z = a.x * b.y - a.y * b.x;
            return retVec;
        }

        static public float LengthSqr(Vec3 a)
        {
            return a.x * a.x + a.y * a.y + a.z * a.z;
        }

        static public float Length(Vec3 a)
        {
            return (float)Math.Sqrt(LengthSqr(a));
        }

        static public float DistSqr(Vec3 a, Vec3 b)
        {
            Vec3 c = new Vec3(0, 0, 0);
            c.x = a.x - b.x;
            c.y = a.y - b.y;
            c.z = a.z - b.z;
            return LengthSqr(c);
        }

        public Vec3 Multiply(float m)
        {
            Vec3 retVec = new Vec3(0, 0, 0);
            retVec.x = items[0] * m;
            retVec.y = items[1] * m;
            retVec.z = items[2] * m;
            return retVec;
        }

        public static float Dot(Vec3 vec1, Vec3 vec2) 
        {
            return (vec1.items[0] * vec2.items[0] +
                    vec1.items[1] * vec2.items[1] +
                    vec1.items[2] * vec2.items[2]);
        }

        public static Vec3 Sub(Vec3 vec1, Vec3 vec2)
        {
            Vec3 retVec = new Vec3(vec1.items[0]-vec2.items[0], vec1.items[1] - vec2.items[1], vec1.items[2] - vec2.items[2]);
            return retVec;
        }

        public static Vec3 Normalize(Vec3 vec)
        {
            double k = vec.items[0] * vec.items[0] + vec.items[1] * vec.items[1] + vec.items[2] * vec.items[2];
            k = Math.Sqrt(k);
            if (k < 0.0000000000001)
            {
                k = 1;
            }

            Vec3 retVec = new Vec3(0, 0, 0);
            retVec.x = vec.items[0] / (float)k;
            retVec.y = vec.items[1] / (float)k;
            retVec.z = vec.items[2] / (float)k;

            return retVec;
        }

        public Vec3 Reverse()
        {
            Vec3 retVec = new Vec3(-1.0f * items[0], -1.0f * items[1], -1.0f * items[2]);
            return retVec;
        }

        public Vec3 Normalize()
        {
            double k = items[0] * items[0] + items[1] * items[1] + items[2] * items[2];
            k = Math.Sqrt(k);
            if (k < 0.0000000000001)
            {
                k = 1;
            }

            Vec3 retVec = new Vec3(0, 0, 0);
            retVec.x = items[0] / (float)k;
            retVec.y = items[1] / (float)k;
            retVec.z = items[2] / (float)k;

            return retVec;
        }

        public static Vec3 operator +(Vec3 vec1, Vec3 vec2)
        {
            vec1.items[0] += vec2.items[0];
            vec1.items[1] += vec2.items[1];
            vec1.items[2] += vec2.items[2];
            return vec1;
        }

        public static Vec3 operator *(Vec3 vec1, Vec3 vec2)
        {
            vec1.items[0] *= vec2.items[0];
            vec1.items[1] *= vec2.items[1];
            vec1.items[2] *= vec2.items[2];
            return vec1;
        }

        public static Vec3 operator *(Vec3 vec, float f)
        {
            Vec3 ret = new Vec3(0.0f, 0.0f, 0.0f);
            ret.x = vec.x * f;
            ret.y = vec.y * f;
            ret.z = vec.z * f;
            return ret;
        }

        public static Vec3 operator /(Vec3 vec, float f)
        {
            Vec3 ret = new Vec3(0.0f, 0.0f, 0.0f);
            ret.x = vec.x / f;
            ret.y = vec.y / f;
            ret.z = vec.z / f;
            return ret;
        }

        public static Vec3 operator *(Vec3 vec, Matrix mat)
        {
            Vec3 ret = new Vec3(0.0f, 0.0f, 0.0f);
            float d;
            for (int i = 0; i < 3; i++)
            {
                d = (float)mat.mat[0, i] * vec.x + (float)mat.mat[1, i] * vec.y +
                    (float)mat.mat[2, i] * vec.z + (float)mat.mat[3, i] * 1.0f;

                switch (i)
                {
                    case 0: ret.x = d; break;
                    case 1: ret.y = d; break;
                    case 2: ret.z = d; break;
                }
            }

            //ret.w = w;

            return ret;
        }


        public static Vec3 operator -(Vec3 vec1, Vec3 vec2)
        {
            vec1.items[0] -= vec2.items[0];
            vec1.items[1] -= vec2.items[1];
            vec1.items[2] -= vec2.items[2];
            return vec1;
        }

    }

    public class Vec2
    {
        private float[] items;

        public bool isNull()
        {
            return (null == items);
        }

        public Vec2()
        {
            items = new float[2];
        }

        public Vec2(BinaryReader br)
        {
            items = new float[2];
            items[0] = br.ReadSingle();
            items[1] = br.ReadSingle();
        }

        public Vec2(float _x, float _y)
        {
            items = new float[2];
            items[0] = _x;
            items[1] = _y;
        }

        public Vec2(string _string)
        {
            if (_string != String.Empty)
            {
                _string = _string.Replace(" ", String.Empty);
                string[] _splitString = _string.Split(',');
                items = new float[2];
                items[0] = float.Parse(_splitString[0], System.Globalization.CultureInfo.InvariantCulture);
                items[1] = float.Parse(_splitString[1], System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        public Vec2(string[] _splitString)
        {
            items = new float[2];
            items[0] = float.Parse(_splitString[0], System.Globalization.CultureInfo.InvariantCulture);
            items[1] = float.Parse(_splitString[1], System.Globalization.CultureInfo.InvariantCulture);
        }

        public float x
        {
            get { return items[0]; }
            set { items[0] = value; }
        }

        public float y
        {
            get { return items[1]; }
            set { items[1] = value; }
        }

        public float u
        {
            get { return items[0]; }
            set { items[0] = value; }
        }

        public float v
        {
            get { return items[1]; }
            set { items[1] = value; }
        }

        public float this[int a]
        {
            get { return items[a]; }
            set { items[a] = value; }
        }

        public static implicit operator float[](Vec2 vec)
        {
            return vec.items;
        }

        public static implicit operator Vec2(float[] values)
        {
            var v = new Vec2();
            v.items = values;
            return v;
        }
    }
}
