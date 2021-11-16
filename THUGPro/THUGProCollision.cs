using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace MHPB2_LevelTool
{
    class THUGProCollision
    {
        public enum FaceFlags
        {
            SKATABLE = 0x00000001,
            NOT_SKATABLE = 0x00000002,
            WALL_RIDABLE = 0x00000004,
            VERT = 0x00000008,
            NON_COLLIDABLE = 0x00000010,
            DECAL = 0x00000020,
            TRIGGER = 0x00000040,
            CAMERA_COLLIDABLE = 0x00000080,
            NO_SKATER_SHADOW = 0x00000100,
            SKATER_SHADOW = 0x00000200,
            NO_SKATER_SHADOW_WALL = 0x00000400,
            UNDER_OK = 0x00000800
            // ...
        }

        /* TODO REFACTOR NS CODE*/
        /* TODO REFACTOR NS CODE*/
        /* TODO REFACTOR NS CODE*/
        public class BSPNode
        {
            public int split_axis;
            public float split_point;

            public List<BSPNode> less_branch;
            public List<BSPNode> greater_branch;

            public int less_branch_index;
            public int greater_branch_index;

            public int array_offset;

            public int node_size;

            public BSPNode()
            {
                less_branch = new List<BSPNode>();
                greater_branch = new List<BSPNode>();
            }
        }

        public class BSPLeaf : BSPNode
        {
            public int num_faces;
            public int[] face_index_array;

            public BSPLeaf()
            {
            }
        }

        const int s_max_face_per_leaf = 20;
        const int s_max_tree_levels = 8;
        const float COLLISION_SUB_INCH_PRECISION = 16.0f;
        const float COLLISION_RECIPROCAL_SUB_INCH_PRECISION = 0.0625f;
        static public BSPLeaf create_bsp_leaf(List<int> _indices, int _num_faces)
        {
            BSPLeaf bsp_leaf = new BSPLeaf();

            bsp_leaf.split_axis = 3;
            bsp_leaf.less_branch = null;
            bsp_leaf.greater_branch = null;

            bsp_leaf.num_faces = _num_faces;
            bsp_leaf.face_index_array = new int[_num_faces];

            for (int i = 0; i < _num_faces; i++)
            {
                bsp_leaf.face_index_array[i] = _indices[i];
            }

            return bsp_leaf;
        }

        static public BSPNode create_bsp_tree(List<Vec3> _positions, List<int> _indices, int _num_faces, int _level)
        {
            if ((_num_faces <= s_max_face_per_leaf) || (_level== s_max_tree_levels))
            {
                return create_bsp_leaf(_indices, _num_faces);
            }
            else
            {
                float _min_x = 0.0f, _min_y = 0.0f, _min_z = 0.0f, _max_x = 0.0f, _max_y = 0.0f, _max_z = 0.0f;
                BBox.GetBoundingBoxData(_positions, ref _min_x, ref _min_y, ref _min_z, ref _max_x, ref _max_y, ref _max_z);

		        // Find initial splits on the three axis
		        int[] i_mid_split = new int[3];
                float[] mid_width = new float[3];
                float[] mid_split = new float[3];
                mid_width[0] = ((_max_x - _min_x) * 0.5f);
                mid_width[1] = ((_max_y - _min_y) * 0.5f);
                mid_width[2] = ((_max_z - _min_z) * 0.5f);
                i_mid_split[0] = (int)(((mid_width[0] + _min_x) * COLLISION_SUB_INCH_PRECISION) + 0.5f);
                i_mid_split[1] = (int)(((mid_width[1] + _min_y) * COLLISION_SUB_INCH_PRECISION) + 0.5f);
                i_mid_split[2] = (int)(((mid_width[2] + _min_z) * COLLISION_SUB_INCH_PRECISION) + 0.5f);
		        mid_split[0] = i_mid_split[0] * COLLISION_RECIPROCAL_SUB_INCH_PRECISION;
		        mid_split[1] = i_mid_split[1] * COLLISION_RECIPROCAL_SUB_INCH_PRECISION;
		        mid_split[2] = i_mid_split[2] * COLLISION_RECIPROCAL_SUB_INCH_PRECISION;
                
		        int[] less_faces = new int[3];
		        int[] greater_faces = new int[3];
                calc_split_faces(0, mid_split[0], _positions, _indices, _num_faces, ref less_faces[0], ref greater_faces[0]);
                calc_split_faces(1, mid_split[1], _positions, _indices, _num_faces, ref less_faces[1], ref greater_faces[1]);
                calc_split_faces(2, mid_split[2], _positions, _indices, _num_faces, ref less_faces[2], ref greater_faces[2]);

                // TODO

		        BSPNode bsp_tree = new BSPNode();
                return bsp_tree;
            }
        }

        static public bool calc_split_faces(int _axis, float _axis_distance, List<Vec3> _positions, List<int> _indices, int _num_faces, ref int _less_faces, ref int _greater_faces)
        {

            // TODO
            return true;
        }
        /* TODO REFACTOR NS CODE*/
        /* TODO REFACTOR NS CODE*/
        /* TODO REFACTOR NS CODE*/

        static public Vec3 GetFaceNormal(List<Vec3> _positions, CollisionObject.Triangle _face)
        {
            Vec3 a, b, p0, p1, p2, faceNormal;
            p0 = new Vec3(_positions[_face.m__a].x, _positions[_face.m__a].y, _positions[_face.m__a].z);
            p1 = new Vec3(_positions[_face.m__b].x, _positions[_face.m__b].y, _positions[_face.m__b].z);
            p2 = new Vec3(_positions[_face.m__c].x, _positions[_face.m__c].y, _positions[_face.m__c].z);

            a = new Vec3(p1.x - p0.x, p1.y - p0.y, p1.z - p0.z);
            b = new Vec3(p2.x - p0.x, p2.y - p0.y, p2.z - p0.z);

            faceNormal = new Vec3(0, 0, 0);
            faceNormal = Vec3.Cross(a, b);
            faceNormal = faceNormal.Normalize();

            return faceNormal;
        }

        // output collision .COL file for thugpro/thug2
        public static void write_collision(string filename, List<CollisionObject> collisionObjects, List<THUGProNodeArray.NodeObject> nodes)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Create(filename)))
            {
                writer.Write(10);
                writer.Write(collisionObjects.Count());

                uint _numTotalVerts = 0;
                uint _numLargeFaces = 0;
                uint _numSmallFaces = 0;
                uint _numFloatVerts = 0;
                uint _numFixedVerts = 0;

                for (int i = 0; i < collisionObjects.Count(); i++)
                {
                    _numTotalVerts += (uint)collisionObjects[i].m__positions.Count();
                    _numFloatVerts += (uint)collisionObjects[i].m__positions.Count();
                    _numLargeFaces += (uint)collisionObjects[i].m__triangles.Count();
                }

                writer.Write(_numTotalVerts);
                writer.Write(_numLargeFaces);
                writer.Write(_numSmallFaces);
                writer.Write(_numFloatVerts);
                writer.Write(_numFixedVerts);
                writer.Write(0);

                uint _tempFaceOffset = 0;
                uint _tempVertOffset = 0;
                uint _tempNodeOffset = 0;
                uint _tempColorOffset = 0;

                // setup bbox/sphere temp variables
                float _minX = 0.0f, _minY = 0.0f, _minZ = 0.0f, _maxX = 0.0f, _maxY = 0.0f, _maxZ = 0.0f;
                float[] _center = new float[3];
                float[] _diag = new float[3];

                // write object headers
                for (int i = 0; i < collisionObjects.Count(); i++)
                {
                    writer.Write(QB.GenerateCRC(collisionObjects[i].m__name));
                    writer.Write((ushort)0);
                    writer.Write((ushort)collisionObjects[i].m__positions.Count());
                    writer.Write((ushort)collisionObjects[i].m__triangles.Count());
                    writer.Write(false);
                    writer.Write(false);
                    writer.Write(_tempFaceOffset);

                    // generate new bbox/sphere data for object
                    BBox.GetBoundingBoxData(collisionObjects[i].m__positions, ref _minX, ref _minY, ref _minZ, ref _maxX, ref _maxY, ref _maxZ);

                    // object bounding box min
                    writer.Write(_minX);
                    writer.Write(_minY);
                    writer.Write(_minZ);
                    writer.Write(1.0f);

                    // object bounding box max
                    writer.Write(_maxX);
                    writer.Write(_maxY);
                    writer.Write(_maxZ);
                    writer.Write(1.0f);

                    writer.Write(_tempVertOffset);
                    writer.Write(_tempNodeOffset);
                    writer.Write(_tempColorOffset);
                    writer.Write(0);

                    _tempFaceOffset += (uint)collisionObjects[i].m__triangles.Count() * 10;
                    _tempVertOffset += (uint)collisionObjects[i].m__positions.Count() * 12;
                    _tempNodeOffset += 8;
                    _tempColorOffset += (uint)collisionObjects[i].m__positions.Count();
                }

                // write object vertices
                for (int i = 0; i < collisionObjects.Count(); i++)
                {
                    for (int vi = 0; vi < collisionObjects[i].m__positions.Count(); vi++)
                    {
                        writer.Write(collisionObjects[i].m__positions[vi].x);
                        writer.Write(collisionObjects[i].m__positions[vi].y);
                        writer.Write(collisionObjects[i].m__positions[vi].z);
                    }
                }

                // write object colors
                for (int i = 0; i < collisionObjects.Count(); i++)
                {
                    for (int vi = 0; vi < collisionObjects[i].m__positions.Count(); vi++)
                    {
                        writer.Write((byte)0x80);
                    }
                }

                // going to use this later
                int rem = 0;

                // write padding
                rem = ((int)_tempVertOffset + (int)_numTotalVerts) % 4;
                if (rem != 0)
                {
                    for (int i = 0; i < (4 - rem); i++)
                    {
                        writer.Write((byte)0x69);
                    }
                }

                // write object faces
                for (int i = 0; i < collisionObjects.Count(); i++)
                {
                    for (int fi = 0; fi < collisionObjects[i].m__triangles.Count(); fi++)
                    {
                        short _tempFaceFlags = 0;
                        Vec3 _tempFaceNormal = GetFaceNormal(collisionObjects[i].m__positions, collisionObjects[i].m__triangles[fi]);
                        
                        // generate face flags based on BMX2 object types
                        switch (collisionObjects[i].m__flags)
                        {
                            case COLLISON_FLAGS.TRIGGER:
                                _tempFaceFlags |= (short)FaceFlags.NON_COLLIDABLE | (short)FaceFlags.TRIGGER;
                                break;
                            case COLLISON_FLAGS.CAGE:
                                    _tempFaceFlags |= (short)FaceFlags.NON_COLLIDABLE | (short)FaceFlags.VERT;
                                    break;
                            case COLLISON_FLAGS.SPINE:
                            case COLLISON_FLAGS.VERT:
                                    if (_tempFaceNormal.y > 0.30 && _tempFaceNormal.y < 0.90)
                                        _tempFaceFlags |= (short)FaceFlags.SKATABLE;

                                    if (_tempFaceNormal.y < 0.60 && _tempFaceNormal.y >= 0)
                                        _tempFaceFlags |= (short)FaceFlags.VERT;

                                    if (!Utils.is_flag_set((uint)_tempFaceFlags, (uint)FaceFlags.VERT))
                                    {
                                        if (((_tempFaceNormal.y < 0.50) && (_tempFaceNormal.y >= 0)) || ((_tempFaceNormal.y > -0.50) && (_tempFaceNormal.y <= 0)))
                                            _tempFaceFlags |= (short)FaceFlags.WALL_RIDABLE;
                                    }
                                    break;
                            case COLLISON_FLAGS.DEFAULT:
                            case COLLISON_FLAGS.RAMP:
                            case COLLISON_FLAGS.WALL:
                            default:
                                    if (!Utils.is_flag_set((uint)_tempFaceFlags, (uint)FaceFlags.VERT))
                                    {
                                        if (((_tempFaceNormal.y < 0.50) && (_tempFaceNormal.y >= 0)) || ((_tempFaceNormal.y > -0.50) && (_tempFaceNormal.y <= 0)))
                                            _tempFaceFlags |= (short)FaceFlags.WALL_RIDABLE;
                                    }
                                    break;
                        }

                        writer.Write((short)_tempFaceFlags);
                        writer.Write((short)0);
                        writer.Write((short)collisionObjects[i].m__triangles[fi].m__a);
                        writer.Write((short)collisionObjects[i].m__triangles[fi].m__b);
                        writer.Write((short)collisionObjects[i].m__triangles[fi].m__c);
                    }
                }

                // write padding
                //rem = ((int)_numFloatVerts) % 4;
                rem = ((int)writer.BaseStream.Position) % 4;
                if (rem != 0)
                {
                    for (int i = 0; i < (4 - rem); i++)
                    {
                        writer.Write((byte)0x69);
                    }
                }

                // write thug2 bytes
                for (int i = 0; i < collisionObjects.Count(); i++)
                {
                    for (int fi = 0; fi < collisionObjects[i].m__triangles.Count(); fi++)
                    {
                        writer.Write((byte)0x0);
                    }
                }

                // write padding
                rem = ((int)writer.BaseStream.Position) % 4;
                if (rem != 0)
                {
                    for (int i = 0; i < (4 - rem); i++)
                    {
                        writer.Write((byte)0x69);
                    }
                }

                // write nodes
                uint _nodeOffset = 0;

                writer.Write(((uint)collisionObjects.Count() * 8));

                for (int i = 0; i < collisionObjects.Count(); i++)
                {
                    writer.Write((short)0x03);
                    writer.Write((short)(collisionObjects[i].m__triangles.Count() * 3));
                    writer.Write((int)_nodeOffset);
                    _nodeOffset += (uint)(collisionObjects[i].m__triangles.Count() * 3);
                }

                for (int i = 0; i < collisionObjects.Count(); i++)
                {
                    for (int fi = 0; fi < collisionObjects[i].m__triangles.Count(); fi++)
                    {
                        writer.Write((short)collisionObjects[i].m__triangles[fi].m__a);
                        writer.Write((short)collisionObjects[i].m__triangles[fi].m__b);
                        writer.Write((short)collisionObjects[i].m__triangles[fi].m__c);
                    }
                }

                writer.Close();
            }
        }
    }
}
