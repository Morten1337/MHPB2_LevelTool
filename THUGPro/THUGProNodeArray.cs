using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace MHPB2_LevelTool
{
    class THUGProNodeArray
    {

        public class NodeObject
        {
            // actual node properies
            public Vec3 m__position;
            public Vec3 m__angles;
            public string m__name;
            public string m__class;
            public string m__collisionMode;
            public bool CreatedAtStart;
            public bool AbsentInNetGames;

            public List<NodeObject> m__links;
            public string m__clusterName;

            // properies only used by this tool
            public Vec3 m__lookVector;
            public Vec3 m__upVector;

            public Vec3 m__col_position;
            public Vec3 m__col_lookVector;
            public Vec3 m__col_upVector;

            public string m__collisionObjectFile;
            public string m__sceneObjectFile;

            public CollisionObject m__collisionObject;
            public SceneObject m__sceneObject;

            public QB.QBScript m__triggerScript;

            public NodeObject()
            {
                // actual node properies
                m__position = new Vec3(0.0f, 0.0f, 0.0f);
                m__angles = new Vec3(0.0f, 0.0f, 0.0f);
                m__name = "";

                m__class = "GenericNode";
                m__collisionMode = "None";

                CreatedAtStart = true;
                AbsentInNetGames = false;

                m__links = new List<NodeObject>();
                m__clusterName = "";

                // properies only used by this tool
                m__lookVector = new Vec3(0.0f, 0.0f, 1.0f);
                m__upVector = new Vec3(0.0f, 0.0f, 1.0f);

            }
        }

        public static NodeObject GetNodeByName(string name, List<NodeObject> nodes)
        {
            for (int i = 0; i < nodes.Count(); i++)
            {
                if (nodes[i].m__name.ToLower() == name.ToLower())
                    return nodes[i];
            }
            return null;
        }

        public static bool NodeNameExists(string name, List<NodeObject> nodes)
        {
            for (int i = 0; i < nodes.Count(); i++)
            {
                if (nodes[i].m__name.ToLower() == name.ToLower())
                    return true;
            }
            return false;
        }

        public static string GetUniqueName(string name, List<NodeObject> nodes)
        {
            string unique_name = "";
            int _newNameIndex = 0;

            if (!THUGProNodeArray.NodeNameExists(name, nodes))
            {
                unique_name = name;
            }
            else
            {
                while (THUGProNodeArray.NodeNameExists(String.Format("{0}_{1}", name, _newNameIndex.ToString("D2")), nodes))
                {
                    _newNameIndex++;
                }

                unique_name = String.Format("{0}_{1}", name, _newNameIndex.ToString("D2"));
            }
            return unique_name;
        }

        public static int GetNodeIndexByName(string name, List<NodeObject> nodes)
        {
            for (int i = 0; i < nodes.Count(); i++)
            {
                if (nodes[i].m__name.ToLower() == name.ToLower())
                    return i;
            }
            return -1;
        }

        public static void write_qb_test(string filename, List<NodeObject> nodes)
        {
            bool matchFound = false;
            Vec3 _RailNodePosition;

            // get rail node clusters
            /*
            for (int i = 0; i < nodes.Count(); i++)
            {
                if (nodes[i].m__class == "RailNode")
                {
                    // return if we already did this..
                    if (nodes[i].m__clusterName != String.Empty)
                        return;

                    _RailNodePosition = new Vec3(nodes[i].m__position.x, nodes[i].m__position.y, -nodes[i].m__position.z).Multiply(11.5f);

                    // reset this for each rail
                    matchFound = false;

                    // loop through all objects
                    for (int geom = 0; geom < nodes.Count(); geom++)
                    {
                        if (matchFound)
                            break;

                        // we only care about level geometry, and make sure it has visible geometry!!
                        if ((nodes[geom].m__class == "LevelGeometry") && (null != nodes[geom].m__sceneObject))
                        {
                            // get distance between object world position first
                            float distSquared = Vec3.DistSqr(nodes[i].m__position, nodes[geom].m__position);

                            // is close enough... still has to be pretty high tho, since some objects are quite big...
                            if (distSquared < 30000.0f)
                            {
                                // loop trough the object meshes
                                for (int mesh = 0; mesh < nodes[geom].m__sceneObject.m__meshes.Count(); mesh++)
                                {
                                    if (matchFound)
                                        break;

                                    // now loop trough the vertex positions
                                    for (int vert = 0; vert < nodes[geom].m__sceneObject.m__meshes[mesh].m__positions.Count(); vert++)
                                    {
                                        if (matchFound)
                                            break;

                                        distSquared = Vec3.DistSqr(_RailNodePosition, nodes[geom].m__sceneObject.m__meshes[mesh].m__positions[vert]) * 100.0f;
                                        if (distSquared < 10.0f)
                                        {
                                            nodes[i].m__clusterName = nodes[geom].m__sceneObject.m__name;
                                            matchFound = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (nodes[i].m__class == "LevelGeometry")
                {
                    // use name by default
                    if (nodes[i].m__sceneObject != null)
                        nodes[i].m__clusterName = nodes[i].m__sceneObject.m__name;

                    // has collision but no visible geom?
                    if ((nodes[i].m__sceneObject == null) && (nodes[i].m__collisionObject != null))
                    {
                        // object has "col" suffix
                        if (nodes[i].m__collisionObject.m__name.ToLower().EndsWith("col"))
                        {
                            // assume theres an existing scene object without the "col" suffix
                            nodes[i].m__clusterName = nodes[i].m__collisionObject.m__name.Remove(nodes[i].m__name.Length - 3);
                        }
                    }

                    // we dont want roads
                    if (nodes[i].m__name.Contains("road"))
                        nodes[i].m__clusterName = String.Empty;

                }
            }
            */

            using (BinaryWriter writer = new BinaryWriter(File.Create(filename)))
            {
                QB.qb_write_checksum(writer, String.Format("{0}_NodeArray", Path.GetFileNameWithoutExtension(filename)), "");
                writer.Write((byte)QB.EScriptToken.ESCRIPTTOKEN_STARTARRAY);


                for (int i = 0; i < nodes.Count(); i++)
                {
                    writer.Write((byte)QB.EScriptToken.ESCRIPTTOKEN_ENDOFLINE);
                    writer.Write((byte)QB.EScriptToken.ESCRIPTTOKEN_STARTSTRUCT);

                    QB.qb_write_checksum(writer, "Name", nodes[i].m__name);
                    QB.qb_write_vector(writer, "Position", nodes[i].m__position.Multiply(11.5f));
                    QB.qb_write_vector(writer, "Angles", nodes[i].m__angles);
                    QB.qb_write_checksum(writer, "Class", nodes[i].m__class);

                    if (nodes[i].CreatedAtStart)
                        QB.qb_write_checksum(writer, "", "CreatedAtStart");

                    if (nodes[i].AbsentInNetGames)
                        QB.qb_write_checksum(writer, "", "AbsentInNetGames");

                    switch (nodes[i].m__class)
                    {
                        case "LevelGeometry":
                            if (nodes[i].m__collisionObject != null)
                            {
                                QB.qb_write_checksum(writer, "CollisionMode", "Geometry");
                            }

                            break;
                        case "RailNode":
                            QB.qb_write_checksum(writer, "Type", "Metal");
                            QB.qb_write_checksum(writer, "TerrainType", "TERRAIN_DEFAULT");
                            break;
                        case "Restart":
                            QB.qb_write_checksum(writer, "Type", "Player1");
                            QB.qb_write_string(writer, "RestartName", "P1: Restart");
                            QB.qb_write_checksum(writer, "restart_types", "");
                            writer.Write((byte)QB.EScriptToken.ESCRIPTTOKEN_STARTARRAY);
                            QB.qb_write_checksum(writer, "", "Player1", false);
                            QB.qb_write_checksum(writer, "", "Multiplayer", false);
                            QB.qb_write_checksum(writer, "", "Horse", false);
                            writer.Write((byte)QB.EScriptToken.ESCRIPTTOKEN_ENDARRAY);
                            break;
                        default:
                            break;
                    }

                    if (nodes[i].m__clusterName != String.Empty)
                    {
                        QB.qb_write_checksum(writer, "", "TrickObject");
                        QB.qb_write_checksum(writer, "Cluster", nodes[i].m__clusterName);
                    }

                    if (nodes[i].m__class == "LevelGeometry")
                    {
                        if (nodes[i].m__triggerScript != null)
                        {
                            QB.qb_write_checksum(writer, "TriggerScript", nodes[i].m__triggerScript.name);
                        }
                    }

                    if (nodes[i].m__links.Count() > 0)
                    {
                        QB.qb_write_checksum(writer, "Links", "");

                        writer.Write((byte)QB.EScriptToken.ESCRIPTTOKEN_STARTARRAY);
                        for (int link = 0; link < nodes[i].m__links.Count(); link++)
                        {
                            if (link > 0)
                                writer.Write((byte)QB.EScriptToken.ESCRIPTTOKEN_COMMA);

                            QB.qb_write_int(writer, "", THUGProNodeArray.GetNodeIndexByName(nodes[i].m__links[link].m__name, nodes), false);
                        }
                        writer.Write((byte)QB.EScriptToken.ESCRIPTTOKEN_ENDARRAY);
                    }

                    writer.Write((byte)QB.EScriptToken.ESCRIPTTOKEN_ENDOFLINE);
                    writer.Write((byte)QB.EScriptToken.ESCRIPTTOKEN_ENDSTRUCT);
                }

                writer.Write((byte)QB.EScriptToken.ESCRIPTTOKEN_ENDOFLINE);
                writer.Write((byte)QB.EScriptToken.ESCRIPTTOKEN_ENDARRAY);

                // trigger scripts

                for (int i = 0; i < nodes.Count(); i++)
                {
                    if (nodes[i].m__triggerScript != null)
                    {
                        nodes[i].m__triggerScript.dump(writer);
                    }
                }

                writer.Write((byte)QB.EScriptToken.ESCRIPTTOKEN_ENDOFLINE);
                for (int i = 0; i < QB.gChecksumTable.Count(); i++)
                {
                    writer.Write((byte)QB.EScriptToken.ESCRIPTTOKEN_CHECKSUM_NAME);
                    writer.Write(QB.gChecksumTable.Keys[i]);
                    QB.qb_write_single_string(writer, QB.gChecksumTable.Values[i]);
                }

                writer.Write((byte)QB.EScriptToken.ESCRIPTTOKEN_ENDOFFILE);
                writer.Close();
            }
        }
    }
}
