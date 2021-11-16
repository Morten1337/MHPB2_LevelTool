//#define TEMP_TEXTURE
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace MHPB2_LevelTool
{
    class THUGProScene
    {
        enum mesh_flags
        {
            TEXTURED = 0x00000001,
            COLORED = 0x00000002,
            NORMALS = 0x00000004
            // ...
        }

        // output scene .scn file for thugpro/thug2
        public static void write_scene(string filename, List<SceneObject> sceneObjects, List<Material> sceneMaterials, List<THUGProNodeArray.NodeObject> sceneNodes, List<Texture> sceneTextures)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Create(filename)))
            {
                writer.Write(1);
                writer.Write(1);
                writer.Write(1);

                writer.Write(sceneMaterials.Count());
                for (int i = 0; i < sceneMaterials.Count(); i++)
                {
                    // material name checksum
                    writer.Write(QB.GenerateCRC(sceneMaterials[i].m__name));

                    // material alias checksum
                    writer.Write(QB.GenerateCRC(sceneMaterials[i].m__name));

                    // number of textures
                    writer.Write(sceneMaterials[i].m__texture_passes.Count());

                    // alpha cutoff
                    writer.Write(sceneMaterials[i].m__texture_passes[0].m__alpha_ref);
                    //writer.Write(1);

                    // sorted
                    writer.Write(false);

                    // draw order
                    writer.Write(0);

                    // one sided
                    writer.Write(false);

                    // two sided
                    writer.Write(true);

                    // texture index
                    writer.Write(0);

                    // use grass effect
                    writer.Write(false);

                    // specular power
                    writer.Write(0.0f);
                    // only write spec color vector if power > 0

                    // textures
                    for (int tex = 0; tex < sceneMaterials[i].m__texture_passes.Count(); tex++)
                    {
                        // texture name checksum
#if TEMP_TEXTURE
                        writer.Write(0xEFBEADDE);
#else
                        // Find valid texture name
                        if (Texture.TextureNameExists(sceneMaterials[i].m__texture_passes[tex].m__textureMap, sceneTextures))
                        {
                            // texture exists! good...
                            writer.Write(QB.GenerateCRC(sceneMaterials[i].m__texture_passes[tex].m__textureMap));
                        }
                        else
                        {
                            // maybe its a NoTexture?
                            if (sceneMaterials[i].m__texture_passes[tex].m__textureMap == "NoTexture")
                            {
                                // yup! write temp texture for now...
                                writer.Write(QB.GenerateCRC(sceneTextures[0].m__name));
                            }
                            else if (sceneMaterials[i].m__texture_passes[tex].m__textureAnimation.m__type == Material.TextureAnimationType.UNIQUE)
                            {
                                // this material has animated textures! append init frame name!
                                writer.Write(QB.GenerateCRC(String.Format("{0}_frame00", sceneMaterials[i].m__texture_passes[tex].m__textureMap)));
                            }
                            else
                            {
                                // this texture does not exist at all! write temp texture...
                                writer.Write(QB.GenerateCRC(sceneTextures[0].m__name));
                                Console.WriteLine("Texture {0} no existo!", sceneMaterials[i].m__texture_passes[tex].m__textureMap);
                            }
                        }
#endif

                        int flags = 4 | 32 | 4096;
                        if (sceneMaterials[i].m__texture_passes[tex].m__transparent)
                        {
                            flags |= 0x0040;
                        }
                        if (sceneMaterials[i].m__texture_passes[tex].m__textureAnimation.m__type == Material.TextureAnimationType.UNIQUE)
                        {
                            flags |= 0x0800;
                        }
                        else if (sceneMaterials[i].m__texture_passes[tex].m__textureAnimation.m__type == Material.TextureAnimationType.UV)
                        {
                            flags |= 0x0001;
                        }

                        // texture flags
                        writer.Write(flags);

                        // use color
                        writer.Write(false);

                        // texture color
                        writer.Write(0.5f);
                        writer.Write(0.5f);
                        writer.Write(0.5f);

                        // blend mode
                        writer.Write((int)sceneMaterials[i].m__texture_passes[tex].m__blend_mode);

                        // blend fixed alpha
                        byte alphaBit = (byte)(BitConverter.GetBytes(sceneMaterials[i].m__texture_passes[tex].m__blend_color)[3]);
                        writer.Write(((int)255-alphaBit));

                        // uv addres mode
                        writer.Write(0.0f);
                        writer.Write(0.0f);

                        // uv env map tile
                        writer.Write(3.0f);
                        writer.Write(3.0f);

                        // filtering
                        writer.Write((ushort)4);
                        writer.Write((ushort)1);

                        // uv animation effect
                        if (sceneMaterials[i].m__texture_passes[tex].m__textureAnimation.m__type == Material.TextureAnimationType.UV)
                        {
                            writer.Write(-sceneMaterials[i].m__texture_passes[tex].m__textureAnimation.u_velocity);
                            writer.Write(-sceneMaterials[i].m__texture_passes[tex].m__textureAnimation.v_velocity);
                            writer.Write(0);
                            writer.Write(0);
                            writer.Write(0);
                            writer.Write(0);
                            writer.Write(0);
                            writer.Write(0);
                        }

                        
                        // texture animation effect
                        if (sceneMaterials[i].m__texture_passes[tex].m__textureAnimation.m__type == Material.TextureAnimationType.UNIQUE)
                        {
                            int fps = (int)sceneMaterials[i].m__texture_passes[tex].m__textureAnimation.animatedMapFramesPerSecond;
                            int numFrames = (int)sceneMaterials[i].m__texture_passes[tex].m__textureAnimation.m__num_frames;

                            writer.Write(sceneMaterials[i].m__texture_passes[tex].m__textureAnimation.m__num_frames); // num keyframes
                            writer.Write(fps * numFrames); // period
                            writer.Write(0); // iterations
                            writer.Write(0); // phase

                            int frameTime = 0;
                            float scale = 1000.0f / fps;
                            for (int frame = 0; frame < numFrames; frame++)
                            {

                                frameTime += (int)scale * 1;
                                writer.Write(frameTime);
                                writer.Write(QB.GenerateCRC(sceneMaterials[i].m__texture_passes[tex].m__textures[frame].m__name));
                            }
                        }
                        
                        // also filtering?
                        writer.Write(1);
                        writer.Write(4);

                        // mip map stuff
                        writer.Write(-8.0f);
                        writer.Write(-8.0f);
                    }
                }

                string[] crashing_objects = new string[] {"bo_wall10","bo_bldg14", "bo_wall08", "bo_wall09"};

                writer.Write(sceneObjects.Count());
                for (int i = 0; i < sceneObjects.Count(); i++)
                {
                    SceneObject temp = sceneObjects[i];
                    if (crashing_objects.Contains<string>(temp.m__name))
                    {
                        Console.WriteLine("object = {0}", temp.m__name);
                    }

                    // object name checksum
                    writer.Write(QB.GenerateCRC(sceneObjects[i].m__name));

                    // object transform index
                    writer.Write(0xffffffff);

                    // object flags
                    uint _tempFlag = (uint)mesh_flags.NORMALS | (uint)mesh_flags.TEXTURED | (uint)mesh_flags.COLORED;

                    writer.Write(_tempFlag);

                    // number of sub meshes
                    writer.Write(sceneObjects[i].m__meshes.Count());

                    // [...]

                    // setup bbox/sphere temp variables
                    float _minX = 0f, _minY = 0f, _minZ = 0f, _maxX = 0f, _maxY = 0f, _maxZ = 0f;
                    float[] _center = new float[3];
                    float[] _diag = new float[3];
                    float _diagLength = 0.0f;
                    float _radius = 0.0f;

                    // generate new bbox/sphere data for object
                    BBox.GetBoundingBoxData(sceneObjects[i], ref _minX, ref _minY, ref _minZ, ref _maxX, ref _maxY, ref _maxZ);
                    BBox.GetBoundingSphereData(ref _center, ref _diag, ref _diagLength, ref _radius, ref _minX, ref _minY, ref _minZ, ref _maxX, ref _maxY, ref _maxZ);

                    // object bounding box min
                    writer.Write(_minX);
                    writer.Write(_minY);
                    writer.Write(_minZ);

                    // object bounding box max
                    writer.Write(_maxX);
                    writer.Write(_maxY);
                    writer.Write(_maxZ);

                    // object bounding sphere 
                    writer.Write(_center[0]);
                    writer.Write(_center[1]);
                    writer.Write(_center[2]);
                    writer.Write(_radius);
                    
                    // object sub meshes
                    for (int fi = 0; fi < sceneObjects[i].m__meshes.Count(); fi++)
                    {
                        // generate new bbox/sphere data for sub mesh
                        BBox.GetBoundingBoxData(sceneObjects[i].m__meshes[fi].m__positions, ref _minX, ref _minY, ref _minZ, ref _maxX, ref _maxY, ref _maxZ);
                        BBox.GetBoundingSphereData(ref _center, ref _diag, ref _diagLength, ref _radius, ref _minX, ref _minY, ref _minZ, ref _maxX, ref _maxY, ref _maxZ);

                        // sub mesh bounding sphere 
                        writer.Write(_center[0]);
                        writer.Write(_center[1]);
                        writer.Write(_center[2]);
                        writer.Write(_radius);

                        // sub mesh bounding box min
                        writer.Write(_minX);
                        writer.Write(_minY);
                        writer.Write(_minZ);

                        // sub mesh bounding box max
                        writer.Write(_maxX);
                        writer.Write(_maxY);
                        writer.Write(_maxZ);

                        // flag 
                        writer.Write(0);

                        // material checksum
                        writer.Write(QB.GenerateCRC(sceneObjects[i].m__materials[fi]));
                        int num_texture_passes = sceneObjects[i].m__materials[fi].m__texture_passes.Count();

                        // number of lod levels
                        writer.Write(1);
                        // hack
                        writer.Write(0);

                        // number of face indices
                        writer.Write((ushort)sceneObjects[i].m__meshes[fi].m__indices.Count());
                        for (int f = 0; f < sceneObjects[i].m__meshes[fi].m__indices.Count(); f++)
                        {
                            writer.Write((ushort)sceneObjects[i].m__meshes[fi].m__indices[f]);
                        }

                        // padding
                        writer.Write(new byte[14]);

                        // generate new vertex stride
                        uint _tempVertexStride = 12;
                        if (Utils.is_flag_set(_tempFlag, (uint)mesh_flags.NORMALS))
                            _tempVertexStride += 12;
                        if (Utils.is_flag_set(_tempFlag, (uint)mesh_flags.COLORED))
                            _tempVertexStride += 4;
                        if (Utils.is_flag_set(_tempFlag, (uint)mesh_flags.TEXTURED))
                            _tempVertexStride += 8 * num_texture_passes;

                        // vertex stride
                        writer.Write((byte)_tempVertexStride);

                        // number of verts
                        writer.Write((ushort)sceneObjects[i].m__meshes[fi].m__positions.Count());

                        // number of vert buffers
                        writer.Write((ushort)1);

                        // vertex buffers
                        {
                            // buffer size
                            writer.Write((uint)(_tempVertexStride * sceneObjects[i].m__meshes[fi].m__positions.Count()));

                            // verts
                            for (int vi = 0; vi < sceneObjects[i].m__meshes[fi].m__positions.Count(); vi++)
                            {
                                // vertex position vector
                                writer.Write(sceneObjects[i].m__meshes[fi].m__positions[vi].x);
                                writer.Write(sceneObjects[i].m__meshes[fi].m__positions[vi].y);
                                writer.Write(sceneObjects[i].m__meshes[fi].m__positions[vi].z);

                                // vertex normal vector
                                writer.Write(sceneObjects[i].m__meshes[fi].m__normals[vi].x);
                                writer.Write(sceneObjects[i].m__meshes[fi].m__normals[vi].y);
                                writer.Write(sceneObjects[i].m__meshes[fi].m__normals[vi].z);

                                // vertex color rgba
                                if (Utils.is_flag_set(_tempFlag, (uint)mesh_flags.COLORED))
                                    writer.Write(sceneObjects[i].m__meshes[fi].m__colors[vi]);

                                // vertex uv coords
                                for (int ti = 0; ti < num_texture_passes; ti++)
                                {
                                    writer.Write(sceneObjects[i].m__meshes[fi].m__texcoords[vi].u);
                                    writer.Write(sceneObjects[i].m__meshes[fi].m__texcoords[vi].v);
                                }
                            }
                        }

                        // shader data bytes
                        byte[] shaderOverrideBytes = new byte[] { 0x52, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0C, 0x18, 0x1C, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x03, 0x94, 0x02, 0x46, 0xF0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xD1, 0xD9, 0xD1, 0xD8, 0x00, 0x00, 0x00, 0x00, 0x10, 0x10, 0xD4, 0xDA, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0E, 0x03, 0x0F, 0x80, 0x1C, 0x01, 0x33, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xCD, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0xA0, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC2, 0xC9, 0xC1, 0xC8, 0xC4, 0xC9, 0xC4, 0xC8, 0x00, 0x00, 0xC1, 0xCA, 0x00, 0x00, 0xC4, 0xCA, 0x3D, 0xCC, 0xCD, 0x1D, 0x3A, 0xCC, 0xCA, 0x1A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x89, 0x00, 0x00, 0x00, 0xCD, 0x00, 0x02, 0x00, 0xA0, 0x00, 0x00, 0x00, 0xA0, 0x00, 0x02, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x11, 0x01, 0x00, 0x21, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x40, 0xF2, 0xFF, 0xFF, 0xF1, 0xFF, 0xFF, 0xFF, 0xF4, 0x01, 0x00, 0x00 };
                        writer.Write(shaderOverrideBytes);
                    }
                }

                // number of object transforms
                writer.Write(0); 

                writer.Close();
            }
        }
    }
}
