//#define TESTING
//#define EXPORT_VISIBLE_TRIGGERS
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace MHPB2_LevelTool
{
    public partial class Form1 : Form
    {

        #region Globals

        // Global containers for object data
        List<SceneObject> gSceneObjects;
        List<CollisionObject> gCollisionObjects;
        List<Material> gMaterials;
        List<Texture> gTextures;
        List<THUGProNodeArray.NodeObject> gNodes;

        // App settings file
        string __SETTINGS_FILE = "settings.ini"; 

        // Scene Object nodes..
        string __SCN_FILE = "test.scn"; 

        // Trigger, Gap, Object Exclusion, Vehicle, Ped, Waypoint nodes..
        string __TOY_FILE = "test.toy"; 
        
        // Sound Emiters, Collision / Terrain Sounds...
        string __AUD_FILE = "test.aud"; 

        // Rails!
        string __SGR_FILE = "test.sgr"; 

        // Textures / Mateterials..
        string __TEXTURE_PATH = "\\textures\\";

        // Static Models and Collision
        string __GEOMETRY_PATH = "\\geometry\\";

        // Dynamic Models and Collision
        string __ANIM_PATH = "\\anim\\"; 

        #endregion

        public Form1()
        {
            // Init form elements
            InitializeComponent();

            // Fancy transparency effect for our form panels
            panel2.BackColor = Color.FromArgb(100, 100, 100, 120);
            panel3.BackColor = Color.FromArgb(100, 100, 100, 120);
            panel4.BackColor = Color.FromArgb(100, 100, 100, 120);
            flowLayoutPanel1.BackColor = Color.FromArgb(100, 100, 100, 120);

            // Load settings file
            LoadSettingsFromFile();

            /*
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(Form1_HandleDragEnter);
            this.DragDrop += new DragEventHandler(Form1_HandleDragDrop);
            */

            // Init global containers
            gSceneObjects = new List<SceneObject>();
            gCollisionObjects = new List<CollisionObject>();
            gMaterials = new List<Material>();
            gTextures = new List<Texture>();
            gNodes = new List<THUGProNodeArray.NodeObject>();

        }

        // Write settings file
        bool WriteSettingsToFile()
        {
            StreamWriter dump = new StreamWriter(__SETTINGS_FILE);
            dump.WriteLine("[SETTINGS]");
            dump.WriteLine("__SCN_FILE          ={0}", __SCN_FILE);
            dump.WriteLine("__GEOMETRY_PATH     ={0}", __GEOMETRY_PATH);
            dump.WriteLine("__TEXTURE_PATH      ={0}", __TEXTURE_PATH);
            dump.Close();
            return true;
        }

        // Load settings file
        bool LoadSettingsFromFile()
        {
            if (!File.Exists(__SETTINGS_FILE))
                return false;

            IniFile settings = new IniFile();
            settings.Load(__SETTINGS_FILE);

            IniFile.IniSection _settings;
            _settings = settings.GetSection("SETTINGS");
            __SCN_FILE = _settings.GetKey("__SCN_FILE").Value;
            __GEOMETRY_PATH = _settings.GetKey("__GEOMETRY_PATH").Value;
            __TEXTURE_PATH = _settings.GetKey("__TEXTURE_PATH").Value;

            UpdateTextBoxes();
            return true;
        }

        // Update form controls after loading settings
        void UpdateTextBoxes()
        {
            textBox2.Text = __SCN_FILE;
            textBox3.Text = __GEOMETRY_PATH;
            textBox4.Text = __TEXTURE_PATH;
        }

        // Read BMX2 SceneInfo file [*.SCN]
        void read_scene_info_file(string filename)
        {

            // Load SceneInfo file as IniFile
            IniFile _sceneInfo = new IniFile();
            _sceneInfo.Load(filename);

            // going to use this later
            THUGProNodeArray.NodeObject _tempNode = null;

            // Handle player restarts
            for (int i = 1; i <= 4; i++)
            {
                _tempNode = new THUGProNodeArray.NodeObject();

                Vec3 _Position = new Vec3(_sceneInfo.GetKeyValue("SceneInfo", String.Format("StartPositionPlayer{0}", i)));
                //Vec3 _Angles = new Vec3(_sceneInfo.GetKeyValue("SceneInfo", String.Format("StartDirectionPlayer{0}", i)));

                // If the position is null, then we have already gotten all the restarts for this level...
                if (_Position.isNull())
                    break;

                _tempNode.m__name = String.Format("TRG_Restart_Player0{0}", i);
                _tempNode.m__position = _Position;
                //_tempNode.m__angles = _Angles;
                _tempNode.m__class = "Restart";

                if (!gNodes.Contains(_tempNode))
                    gNodes.Add(_tempNode);
            }

            IniFile.IniSection _StaticModels = _sceneInfo.GetSection("StaticModels");

            // Found [StaticModels] section?
            if (null != _StaticModels)
            {
                int _NumberOfStaticModels = Int32.Parse(_StaticModels.GetKey("NumberOfStaticModels").Value);
                for (int i = 1; i <= _NumberOfStaticModels; i++)
                {
                    _tempNode = new THUGProNodeArray.NodeObject();

                    string _ModelSectionName = String.Format("Model{0}", i);

                    // Parse Model Name
                    string _ModelName = _sceneInfo.GetKeyValue(_ModelSectionName, "SLT").ToLower().Replace(".slt", String.Empty);

                    // Parse Position
                    Vec3 _Position = new Vec3(_sceneInfo.GetKeyValue(_ModelSectionName, "Position"));

                    // Parse LookVector
                    Vec3 _LookVector = new Vec3(_sceneInfo.GetKeyValue(_ModelSectionName, "LookVector"));

                    // Parse UpVector
                    Vec3 _UpVector = new Vec3(_sceneInfo.GetKeyValue(_ModelSectionName, "UpVector"));

                    // Get unique name. Static models can reference the same dxg file, so there can be name conflict.
                    _tempNode.m__name = THUGProNodeArray.GetUniqueName(_ModelName.ToLower(), gNodes); // this name is updated when loading the dxg file.

                    _tempNode.m__sceneObjectFile = String.Format("{0}.dxg", _ModelName);

                    _tempNode.m__class = "LevelGeometry";

                    if (!_Position.isNull())
                        _tempNode.m__position = _Position;

                    if (!_LookVector.isNull())
                        _tempNode.m__lookVector = _LookVector;
                    else
                        _tempNode.m__lookVector = new Vec3(0.0f, 0.0f, -1.0f);

                    if (!_UpVector.isNull())
                        _tempNode.m__upVector = _UpVector;
                    else
                        _tempNode.m__upVector = new Vec3(0.0f, 1.0f, 0.0f);


                    if (!gNodes.Contains(_tempNode))
                        gNodes.Add(_tempNode);
                }
            }

            IniFile.IniSection _CollisionObjects = _sceneInfo.GetSection("CollisionObjects");

            // Found [CollisionObjects] section?
            if (null != _CollisionObjects)
            {
                int _NumberOfCollisionObjects = Int32.Parse(_CollisionObjects.GetKey("NumberOfCollisionObjects").Value);
                for (int i = 1; i <= _NumberOfCollisionObjects; i++)
                {
                    string _ObjectSectionName = String.Format("Object{0}", i);

                    // Parse Model Name
                    string _CollisionModelName = _sceneInfo.GetKeyValue(_ObjectSectionName, "COL").ToLower();

                    // Check for existing node object
                    _tempNode = THUGProNodeArray.GetNodeByName(_CollisionModelName, gNodes);

                    // Maybe create new node object
                    if (null == _tempNode)
                        _tempNode = new THUGProNodeArray.NodeObject();
                    //else
                    //    Console.WriteLine("node exists '{0}'", _tempNode.m__name);

                    // Parse Position
                    Vec3 _Position = new Vec3(_sceneInfo.GetKeyValue(_ObjectSectionName, "Position"));

                    // Parse LookVector
                    Vec3 _LookVector = new Vec3(_sceneInfo.GetKeyValue(_ObjectSectionName, "LookVector"));

                    // Parse UpVector
                    Vec3 _UpVector = new Vec3(_sceneInfo.GetKeyValue(_ObjectSectionName, "UpVector"));

                    _tempNode.m__collisionObjectFile = String.Format("{0}.col", _CollisionModelName);
                    _tempNode.m__name = _CollisionModelName;
                    _tempNode.m__class = "LevelGeometry";

                    if (!_Position.isNull())
                        _tempNode.m__col_position = _Position;

                    if (!_LookVector.isNull())
                        _tempNode.m__col_lookVector = _LookVector;
                    else
                        _tempNode.m__col_lookVector = new Vec3(0.0f, 0.0f, -1.0f);

                    if (!_UpVector.isNull())
                        _tempNode.m__col_upVector = _UpVector;
                    else
                        _tempNode.m__col_upVector = new Vec3(0.0f, 1.0f, 0.0f);

                    if (!gNodes.Contains(_tempNode))
                        gNodes.Add(_tempNode);

                }
            }

            // Read level geometry files [*.DXG]
            for (int i = 0; i < gNodes.Count(); i++)
            {
                if (null != gNodes[i].m__sceneObjectFile)
                {
                    string filepath = String.Format("{0}\\{1}", __GEOMETRY_PATH, gNodes[i].m__sceneObjectFile);
                    if (File.Exists(filepath))
                    {
                        gNodes[i].m__sceneObject = read_geometry_file(filepath, gNodes[i]);

                        if (null == gNodes[i].m__sceneObject)
                        {
                            MessageBox.Show(String.Format("ERROROR!! failed to load scene object {0}", gNodes[i].m__sceneObjectFile), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                        // change node name to unique sceneobject name...
                        gNodes[i].m__name = gNodes[i].m__sceneObject.m__name;

                        // Read materials | .dxm files
                        for (int mi = 0; mi < gNodes[i].m__sceneObject.m__materials.Count(); mi++)
                        {
                            read_material_file(String.Format("{0}\\{1}.dxm", __TEXTURE_PATH, gNodes[i].m__sceneObject.m__materials[mi]));
                        }
                    }
                }
            }

            // Read textures [*.DXT] & [*.AXT]
            for (int i = 0; i < gMaterials.Count(); i++)
            {
                // shader/texture pass
                for (int tex = 0; tex < gMaterials[i].m__texture_passes.Count(); tex++)
                {
                    bool foundAnimatedTexture = false;
                    bool foundRegularTexture = false;

                    // maybe animated material?
                    if (gMaterials[i].m__texture_passes[tex].m__textureAnimation.m__type == Material.TextureAnimationType.UNIQUE)
                    {
                        // yup! then we're looking for an atx file...
                        foundAnimatedTexture = read_texture_file(String.Format("{0}\\{1}.atx", __TEXTURE_PATH, gMaterials[i].m__texture_passes[tex].m__textureMap), gMaterials[i].m__texture_passes[tex]);

                        if (!foundAnimatedTexture)
                        {
                            gMaterials[i].m__texture_passes[tex].m__textureAnimation.m__type = Material.TextureAnimationType.NONE;
                        }
                    }

                    if (foundAnimatedTexture)
                    {
                        gMaterials[i].m__texture_passes[tex].m__textureAnimation.m__num_frames = gMaterials[i].m__texture_passes[tex].m__textures.Count();
                    }
                    else
                    {
                        // nope, just a single texture... try to load dxt file
                        foundRegularTexture = read_texture_file(String.Format("{0}\\{1}.dxt", __TEXTURE_PATH, gMaterials[i].m__texture_passes[tex].m__textureMap), gMaterials[i].m__texture_passes[tex]);

                        if (!foundRegularTexture)
                        {
                            if (!gMaterials[i].m__texture_passes[tex].m__textureMap.Contains("NoTexture"))
                            {
                                // default texture fallback
                                read_texture_file(String.Format("{0}\\{1}.dxt", __TEXTURE_PATH, gMaterials[0].m__texture_passes[0].m__textureMap), gMaterials[i].m__texture_passes[tex]);
                            }
                        }
                    }
                }
            }

            // Read level collision files [*.COL]
            for (int i = 0; i < gNodes.Count(); i++)
            {
                if (null != gNodes[i].m__collisionObjectFile)
                {
                    string filepath = String.Format("{0}\\{1}", __GEOMETRY_PATH, gNodes[i].m__collisionObjectFile);
                    if (File.Exists(filepath))
                    {
                        gNodes[i].m__collisionObject = read_collision_file(filepath);

                        if (null == gNodes[i].m__collisionObject)
                        {
                            MessageBox.Show(String.Format("ERROROR!! failed to load collision object {0}", gNodes[i].m__collisionObjectFile), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }

        }

        float DegToRad(float deg)
        {
            return deg * ((float)Math.PI / 180.0f);
        }

        bool construct_box_stripped(Vec3 boxPos, Vec3 boxSize, float yaw, float pitch, float roll, float radius, ref List<Vec3> positions_out, ref List<ushort> indices_out)
        {

            positions_out.Add(new Vec3(-1.0f, -1.0f, 1.0f) * boxSize * radius);
            positions_out.Add(new Vec3(1.0f, -1.0f, 1.0f) * boxSize * radius);
            positions_out.Add(new Vec3(-1.0f, 1.0f, 1.0f) * boxSize * radius);
            positions_out.Add(new Vec3(1.0f, 1.0f, 1.0f) * boxSize * radius);
            positions_out.Add(new Vec3(-1.0f, -1.0f, -1.0f) * boxSize * radius);
            positions_out.Add(new Vec3(1.0f, -1.0f, -1.0f) * boxSize * radius);
            positions_out.Add(new Vec3(-1.0f, 1.0f, -1.0f) * boxSize * radius);
            positions_out.Add(new Vec3(1.0f, 1.0f, -1.0f) * boxSize * radius);

            // jeez..
            indices_out.Add(0);
            indices_out.Add(1);
            indices_out.Add(2);
            indices_out.Add(3);
            indices_out.Add(7);
            indices_out.Add(1);
            indices_out.Add(5);
            indices_out.Add(4);
            indices_out.Add(7);
            indices_out.Add(6);
            indices_out.Add(2);
            indices_out.Add(4);
            indices_out.Add(0);
            indices_out.Add(1);

            Vec3 LookVector = new Vec3(
                (float)(Math.Sin(DegToRad(yaw)) * Math.Cos(DegToRad(pitch))),
                (float)(Math.Sin(DegToRad(pitch))),
                -(float)(Math.Cos(DegToRad(yaw)) * Math.Cos(DegToRad(pitch))));

            Vec3 UpVector = new Vec3(
                (float)(Math.Sin(DegToRad(yaw)) * Math.Cos(DegToRad(pitch + 90))),
                (float)(Math.Sin(DegToRad(pitch + 90))),
                (float)(-Math.Cos(DegToRad(yaw)) * Math.Cos(DegToRad(pitch + 90))));

            float Scale = 11.5f;
            positions_out = TransformVertexPositions(positions_out, boxPos, LookVector, UpVector, Scale);

            return true;
        }

        bool construct_box(Vec3 boxPos, Vec3 boxSize, float yaw, float pitch, float roll, float radius, ref List<Vec3> positions_out, ref List<int> triangles_out)
        {
            positions_out.Add(new Vec3(-1.0f, -1.0f, 1.0f) * boxSize * radius);
            positions_out.Add(new Vec3(-1.0f, -1.0f, -1.0f) * boxSize * radius);
            positions_out.Add(new Vec3(1.0f, -1.0f, -1.0f) * boxSize * radius);
            positions_out.Add(new Vec3(1.0f, -1.0f, 1.0f) * boxSize * radius);
            positions_out.Add(new Vec3(-1.0f, 1.0f, 1.0f) * boxSize * radius);
            positions_out.Add(new Vec3(1.0f, 1.0f, 1.0f) * boxSize * radius);
            positions_out.Add(new Vec3(1.0f, 1.0f, -1.0f) * boxSize * radius);
            positions_out.Add(new Vec3(1.0f, -1.0f, 1.0f) * boxSize * radius);

            // jeez..
            triangles_out.Add(0); triangles_out.Add(1); triangles_out.Add(2);
            triangles_out.Add(2); triangles_out.Add(3); triangles_out.Add(0);
            triangles_out.Add(4); triangles_out.Add(5); triangles_out.Add(6);
            triangles_out.Add(6); triangles_out.Add(7); triangles_out.Add(4);
            triangles_out.Add(0); triangles_out.Add(3); triangles_out.Add(5);
            triangles_out.Add(5); triangles_out.Add(4); triangles_out.Add(0);
            triangles_out.Add(3); triangles_out.Add(2); triangles_out.Add(6);
            triangles_out.Add(6); triangles_out.Add(5); triangles_out.Add(3);
            triangles_out.Add(2); triangles_out.Add(1); triangles_out.Add(7);
            triangles_out.Add(7); triangles_out.Add(6); triangles_out.Add(2);
            triangles_out.Add(1); triangles_out.Add(0); triangles_out.Add(4);
            triangles_out.Add(4); triangles_out.Add(7); triangles_out.Add(1);
            
            Vec3 LookVector = new Vec3(
                (float)(Math.Sin(DegToRad(yaw)) * Math.Cos(DegToRad(pitch))),
                (float)(Math.Sin(DegToRad(pitch))),
                -(float)(Math.Cos(DegToRad(yaw)) * Math.Cos(DegToRad(pitch))));

            Vec3 UpVector = new Vec3(
                (float)(Math.Sin(DegToRad(yaw)) * Math.Cos(DegToRad(pitch + 90))),
                (float)(Math.Sin(DegToRad(pitch + 90))),
                (float)(-Math.Cos(DegToRad(yaw)) * Math.Cos(DegToRad(pitch + 90))));

            float Scale = 11.5f;
            positions_out = TransformVertexPositions(positions_out, boxPos, LookVector, UpVector, Scale);

            return true;
        }

        // Read BMX2 Trigger info file [*.TOY]
        void read_trigger_data_file(string filename)
        {
            String _TempValueString;

            IniFile _TriggerFile = new IniFile();
            _TriggerFile.Load(filename);

            // Trigger geometry info. BBox / Plane / Sphere?
            IniFile.IniSection _Triggers = _TriggerFile.GetSection("Triggers");

            int _NumberOfTriggers = Int32.Parse(_Triggers != null ? _Triggers.GetKey("NumberOfTriggers").Value : "0");
            for (int i = 1; i <= _NumberOfTriggers; i++)
            {
                String _TriggerName = String.Format("Trigger{0}", i);

                String _ParentName = _TriggerFile.GetKeyValue(_TriggerName, "ParentName");

                // Skip nested objects...
                if (_ParentName!= String.Empty)
                {
                    continue;
                }

                Vec3 _Position = new Vec3(_TriggerFile.GetKeyValue(_TriggerName, "Position"));

                Vec3 _Box = new Vec3(_TriggerFile.GetKeyValue(_TriggerName, "Box"));

                if (_Box.isNull())
                {
                    _Box = new Vec3(1.0f, 1.0f, 1.0f);
                }

                _Box.y *= 4.0f; // multiply the height, cause we can jump much higher in thps!
                _Box.x *= 1.02f;
                _Box.z *= 1.02f;

                float _Yaw = ParseFloat(_TriggerFile.GetKeyValue(_TriggerName, "Yaw"));
                float _Pitch = ParseFloat(_TriggerFile.GetKeyValue(_TriggerName, "Pitch"));
                float _Roll = ParseFloat(_TriggerFile.GetKeyValue(_TriggerName, "Roll"));
                float _Radius = ParseFloat(_TriggerFile.GetKeyValue(_TriggerName, "Radius"));

                if (_Radius == 0.0f)
                {
                    _Radius = 1.0f;
                }
                else
                {
                    _Radius /= 4.0f;
                }

                String _Name = _TriggerFile.GetKeyValue(_TriggerName, "Name");
                _Name = String.Format("TRGP_{0}", _Name.Replace(" ", String.Empty));

                Vec3 LookVector = new Vec3(
                    (float)(Math.Sin(DegToRad(_Yaw)) * Math.Cos(DegToRad(_Pitch))),
                    (float)(Math.Sin(DegToRad(_Pitch))),
                    -(float)(Math.Cos(DegToRad(_Yaw)) * Math.Cos(DegToRad(_Pitch))));

                Vec3 UpVector = new Vec3(
                    (float)(Math.Sin(DegToRad(_Yaw)) * Math.Cos(DegToRad(_Pitch + 90))),
                    (float)(Math.Sin(DegToRad(_Pitch + 90))),
                    (float)(-Math.Cos(DegToRad(_Yaw)) * Math.Cos(DegToRad(_Pitch + 90))));

                THUGProNodeArray.NodeObject tempNode = new THUGProNodeArray.NodeObject();
                tempNode.m__name = _Name;
                tempNode.m__class = "LevelGeometry";


                _TempValueString = _TriggerFile.GetKeyValue(_TriggerName, "NumberOfActions");
                int _NumberOfActions = Int32.Parse(_TempValueString != String.Empty ? _TempValueString : "0");
                if (_NumberOfActions > 0)
                {

                    tempNode.m__triggerScript = new QB.QBScript();
                    tempNode.m__triggerScript.name = String.Format("{0}_Script", _Name);

                    for (int j = 1; j <= _NumberOfActions; j++)
                    {
                        String _TriggerActionName = String.Format("Trigger{0}Action{1}", i, j);
                        String _ActionType = _TriggerFile.GetKeyValue(_TriggerActionName, "ActionType").Replace(" ", String.Empty);

                        String _Text1 = _TriggerFile.GetKeyValue(_TriggerActionName, "Text1");
                        String _Text2 = _TriggerFile.GetKeyValue(_TriggerActionName, "Text2");

                        Vec3 _ResetPoint = new Vec3(_TriggerFile.GetKeyValue(_TriggerActionName, "ResetPoint"));
                        String restartNodeName = "";

                        if (_Text1 != null)
                        {
                            if (_Text1.StartsWith(" "))
                                _Text1.Remove(0, 1);
                            _Text1 = _Text1.Replace("\"", "");
                        }

                        if (_Text2 != null)
                        {
                            if (_Text2 == String.Empty)
                                _Text2 = _Text1;

                            if (_Text2.StartsWith(" "))
                                _Text2.Remove(0, 1);
                            _Text2 = _Text2.Replace("\"", "");
                        }


                        QB.QBInstruction tempInstruction = new QB.QBInstruction();
                        switch(_ActionType)
                        {
                            case "BikeReset":
                                tempInstruction.functionCallName = "SK3_KillSkater";
                                if (_ResetPoint.isNull())
                                {
                                    restartNodeName = "TRG_Restart_Player01";
                                }
                                else
                                {
                                    restartNodeName = String.Format("TRG_Restart_{0}", _Name);
                                    CreateRestartNode(restartNodeName, _ResetPoint);
                                }
                                tempInstruction.args.Add(new QB.QBItem(QB.eQBItem.CHECKSUM, "nodename", restartNodeName, false));
                                tempInstruction.args.Add(new QB.QBItem(QB.eQBItem.STRING, "Message1", _Text1, false));
                                tempInstruction.args.Add(new QB.QBItem(QB.eQBItem.STRING, "Message2", _Text2, false));
                                tempInstruction.args.Add(new QB.QBItem(QB.eQBItem.STRING, "Message3", _Text1, false));
                                tempInstruction.args.Add(new QB.QBItem(QB.eQBItem.STRING, "Message4", _Text2, false));
                                // Text1
                                // Text2
                                // ResetPoint
                                break;
                            case "BikeWarning":
                                tempInstruction.functionCallName = "create_panel_message";
                                tempInstruction.args.Add(new QB.QBItem(QB.eQBItem.CHECKSUM, "id", "leaving_message", false));
                                tempInstruction.args.Add(new QB.QBItem(QB.eQBItem.STRING, "text", _Text1, false));
                                // Text1
                                break;
                            case "BikeWreck":
                                tempInstruction.functionCallName = "SK3_KillSkater";
                                if (_ResetPoint.isNull())
                                {
                                    restartNodeName = "TRG_Restart_Player01";
                                }
                                else
                                {
                                    restartNodeName = String.Format("TRG_Restart_{0}", _Name);
                                    CreateRestartNode(restartNodeName, _ResetPoint);
                                }
                                tempInstruction.args.Add(new QB.QBItem(QB.eQBItem.CHECKSUM, "nodename", restartNodeName, false));
                                tempInstruction.args.Add(new QB.QBItem(QB.eQBItem.CHECKSUM, "", "Bail", false));
                                tempInstruction.args.Add(new QB.QBItem(QB.eQBItem.STRING, "Message1", _Text1, false));
                                tempInstruction.args.Add(new QB.QBItem(QB.eQBItem.STRING, "Message2", _Text1, false));
                                tempInstruction.args.Add(new QB.QBItem(QB.eQBItem.STRING, "Message3", _Text1, false));
                                tempInstruction.args.Add(new QB.QBItem(QB.eQBItem.STRING, "Message4", _Text1, false));
                                // Text1
                                break;
                            case "Particles":
                                break;
                            case "Sound":
                                break;
                            case "ShakeController":
                                break;
                        };

                        tempNode.m__triggerScript.instructions.Add(tempInstruction);
                    }

                }

                tempNode.m__lookVector = LookVector;
                tempNode.m__col_lookVector = LookVector;
                tempNode.m__angles = LookVector;

                tempNode.m__upVector = UpVector;
                tempNode.m__col_upVector = UpVector;

                tempNode.m__position = _Position;
                tempNode.m__col_position = _Position;

                List<Vec3> col_verts = new List<Vec3>();
                List<int> triangles = new List<int>();
                if (construct_box(_Position, _Box, _Yaw, _Pitch, _Roll, _Radius, ref col_verts, ref triangles))
                {
                    CollisionObject tempCollisionObject = new CollisionObject();
                    tempCollisionObject.m__name = _Name;
                    tempCollisionObject.m__flags = COLLISON_FLAGS.TRIGGER;

                    tempCollisionObject.m__num_triangles = (uint)triangles.Count() / 3;

                    tempCollisionObject.m__triangles = new List<CollisionObject.Triangle>();
                    for (int j = 0; j < triangles.Count() - 2; )
                    {
                        CollisionObject.Triangle tempTriangle = new CollisionObject.Triangle();
                        tempTriangle.m__a = (ushort)triangles[j++];
                        tempTriangle.m__b = (ushort)triangles[j++];
                        tempTriangle.m__c = (ushort)triangles[j++];
                        tempCollisionObject.m__triangles.Add(tempTriangle);
                    }

                    tempCollisionObject.m__num_verts = (uint)col_verts.Count();
                    tempCollisionObject.m__positions = col_verts;

                    for (int j = 0; j < tempCollisionObject.m__num_verts; j++)
                        tempCollisionObject.m__positions[j].z *= -1.0f;

                    tempNode.m__collisionObject = tempCollisionObject;

                    if (!gCollisionObjects.Contains(tempCollisionObject))
                        gCollisionObjects.Add(tempCollisionObject);
                }

#if EXPORT_VISIBLE_TRIGGERS

                List<Vec3> scn_verts = new List<Vec3>();
                List<ushort> indices = new List<ushort>();
                if (construct_box_stripped(_Position, _Box, _Yaw, _Pitch, _Roll, _Radius, ref scn_verts, ref indices))
                {
                    SceneObject tempObject = new SceneObject();
                    tempObject.m__name = _Name;
                    tempObject.m__num_meshes = 1;
                    tempObject.m__num_materials = 1;

                    tempObject.m__materials = new List<String>();
                    tempObject.m__materials.Add(gSceneObjects[0].m__materials[0]);

                    SceneObject.Mesh tempMesh = new SceneObject.Mesh();

                    tempMesh.m__num_verts = (ushort)scn_verts.Count();
                    tempMesh.m__positions = scn_verts;

                    for (int j = 0; j < tempMesh.m__num_verts; j++)
                        tempMesh.m__positions[j].z *= -1.0f;

                    tempMesh.m__num_indices = (ushort)indices.Count(); ;
                    tempMesh.m__indices = indices;

                    tempMesh.m__normals = new List<Vec3>();
                    for (int j = 0; j < tempMesh.m__num_verts; j++)
                        tempMesh.m__normals.Add(new Vec3(0, 1, 0));

                    tempMesh.m__texcoords = new List<Vec2>();
                    for (int j = 0; j < tempMesh.m__num_verts; j++)
                        tempMesh.m__texcoords.Add(new Vec2(0.5f, 0.5f));

                    tempMesh.m__colors = new List<uint>();
                    for (int j = 0; j < tempMesh.m__num_verts; j++)
                        tempMesh.m__colors.Add(0x80804080);

                    tempObject.m__meshes.Add(tempMesh);

                    tempNode.m__sceneObject = tempObject;

                    if (!gSceneObjects.Contains(tempObject))
                        gSceneObjects.Add(tempObject);
                }
#endif

                if (!gNodes.Contains(tempNode))
                    gNodes.Add(tempNode);

            }

            // Rail Gaps.
            IniFile.IniSection _Grinds = _TriggerFile.GetSection("Grinds");

            // Gap info. A / B
            IniFile.IniSection _Gaps = _TriggerFile.GetSection("Gaps");
            int _NumberOfGaps = Int32.Parse(_Gaps != null ? _Gaps.GetKey("NumberOfGaps").Value : "0");
            for (int i = 1; i <= _NumberOfGaps; i++)
            {
                String _GapName = String.Format("Gap{0}", i);

                // Gap name text
                string _Name = _TriggerFile.GetKeyValue(_GapName, "Name");
                if (_Name != null)
                {
                    if (_Name.StartsWith(" "))
                        _Name.Remove(0, 1);
                    _Name = _Name.Replace("\"", "");
                }

                // Gap id
                string _NameRef = _TriggerFile.GetKeyValue(_GapName, "NameRef");
                _NameRef = _NameRef.Replace(" ", String.Empty);

                // GapType
                string _GapType = _TriggerFile.GetKeyValue(_GapName, "GapType");
                _GapType = _GapType.Replace(" ", String.Empty);

                // Gap object a
                string _GapTriggerA = _TriggerFile.GetKeyValue(_GapName, "GapTriggerA");
                _GapTriggerA = String.Format("TRGP_{0}", _GapTriggerA.Replace(" ", String.Empty));

                // Gap object b
                string _GapTriggerB = _TriggerFile.GetKeyValue(_GapName, "GapTriggerB");
                _GapTriggerB = String.Format("TRGP_{0}", _GapTriggerB.Replace(" ", String.Empty));

                // score
                int _Points = Int32.Parse(_TriggerFile.GetKeyValue(_GapName, "Points"));

                //----------------------------------------------------------------------------------------------------
                // Create triggerscripts

                QB.QBScript triggerScriptA = new QB.QBScript();
                triggerScriptA.name = String.Format("{0}_Script", _GapTriggerA);

                QB.QBInstruction tempInstr = new QB.QBInstruction();
                tempInstr.functionCallName = "edmsg2";
                tempInstr.args.Add(new QB.QBItem(QB.eQBItem.STRING, "", String.Format("\\c2TRIGGER: {0}", _GapTriggerA), false));
                triggerScriptA.instructions.Add(tempInstr);

                triggerScriptA.instructions.Add(CreateEndGap(String.Format("{0}_B", _NameRef), _Name, _Points));
                triggerScriptA.instructions.Add(CreateStartGap(String.Format("{0}_A", _NameRef), "vert"));

                QB.QBScript triggerScriptB = new QB.QBScript();
                triggerScriptB.name = String.Format("{0}_Script", _GapTriggerB);

                tempInstr = new QB.QBInstruction();
                tempInstr.functionCallName = "edmsg2";
                tempInstr.args.Add(new QB.QBItem(QB.eQBItem.STRING, "", String.Format("\\c2TRIGGER: {0}", _GapTriggerB), false));
                triggerScriptB.instructions.Add(tempInstr);

                triggerScriptB.instructions.Add(CreateEndGap(String.Format("{0}_A", _NameRef), _Name, _Points));
                triggerScriptB.instructions.Add(CreateStartGap(String.Format("{0}_B", _NameRef), "vert"));

                THUGProNodeArray.GetNodeByName(_GapTriggerA, gNodes).m__triggerScript = triggerScriptA;
                THUGProNodeArray.GetNodeByName(_GapTriggerB, gNodes).m__triggerScript = triggerScriptB;

                //----------------------------------------------------------------------------------------------------

                // Name
                // NameRef
                // Points
                // GapType
                // GapTriggerA
                // GapTriggerB
                // PointMultiplier

            }

            // Particles
            IniFile.IniSection _ParticleEmitters = _TriggerFile.GetSection("ParticleEmitters");

            // Vehicles
            IniFile.IniSection _CarSplines = _TriggerFile.GetSection("CarSplines");

            // Pedestrians
            IniFile.IniSection _Pedestrians = _TriggerFile.GetSection("Pedestrians");
            IniFile.IniSection _PedRoutes = _TriggerFile.GetSection("PedRoutes");
        }

        public void CreateRestartNode(string nodeName, Vec3 restartPosition)
        {

            THUGProNodeArray.NodeObject _tempNode = new THUGProNodeArray.NodeObject();

            _tempNode.m__name = nodeName;
            _tempNode.m__position = restartPosition;
            _tempNode.m__class = "Restart";

            if (!gNodes.Contains(_tempNode))
                gNodes.Add(_tempNode);
        }

        public QB.QBInstruction CreateStartGap(string gapID, string gapType)
        {
            QB.QBInstruction gapInstruction = new QB.QBInstruction();

            gapInstruction.functionCallName = "StartGap";
            gapInstruction.args.Add(new QB.QBItem(QB.eQBItem.CHECKSUM, "GapID", gapID, false));

            switch(gapType)
            {
                case "vert":
                    gapInstruction.args.Add(new QB.QBItem(QB.eQBItem.CHECKSUM, "Flags", "PURE_AIR", false));
                    break;
                default:
                    gapInstruction.args.Add(new QB.QBItem(QB.eQBItem.CHECKSUM, "Flags", "PURE_MANUAL", false));
                    break;
            }

            return gapInstruction;
        }

        public QB.QBInstruction CreateEndGap(string gapID, string gapText, int gapScore)
        {
            QB.QBInstruction gapInstruction = new QB.QBInstruction();
            gapInstruction.functionCallName = "EndGap";
            gapInstruction.args.Add(new QB.QBItem(QB.eQBItem.CHECKSUM, "GapID", gapID, false));
            gapInstruction.args.Add(new QB.QBItem(QB.eQBItem.STRING, "text", gapText, false));
            gapInstruction.args.Add(new QB.QBItem(QB.eQBItem.INTEGER, "score", gapScore, false));
            return gapInstruction;
        }

        // Read BMX2 GrindInfo file [*.SGR]
        void read_rail_data_file(string filename)
        {
            filename = filename.Replace(".scn", ".sgr");

            if (!File.Exists(filename))
                return;

            IniFile _SceneGrindInfo = new IniFile();
            _SceneGrindInfo.Load(filename);

            IniFile.IniSection _GrindLineInfo = _SceneGrindInfo.GetSection("Info");

            // Found [Info] section?
            if (null != _GrindLineInfo)
            {
                int _TotalGrindLines = Int32.Parse(_GrindLineInfo.GetKey("TotalGrindLines").Value);
                for (int i = 1; i <= _TotalGrindLines; i++)
                {
                    string _GrindSectionName = String.Format("Grind_{0}", i);

                    int _NumVertices = Int32.Parse(_SceneGrindInfo.GetKeyValue(_GrindSectionName, "NumVertices"));
                    string _GrindName = _SceneGrindInfo.GetKeyValue(_GrindSectionName, "GrindName");

                    THUGProNodeArray.NodeObject _tempNode = null;
                    int RailNodeIndex = 0;
                    for (int gi = 1; gi <= _NumVertices; gi++)
                    {
                        if (_tempNode == null)
                            _tempNode = new THUGProNodeArray.NodeObject();

                        _tempNode.m__position = new Vec3(_SceneGrindInfo.GetKeyValue(_GrindSectionName, String.Format("Vertex_{0}", gi)));
                        _tempNode.m__name = String.Format("TRG_{0}{1}", _GrindName, RailNodeIndex.ToString("D2"));
                        _tempNode.m__class = "RailNode";

                        bool add_node = true;

                        // do not add node if already exist
                        if (gNodes.Contains(_tempNode))
                        {
                            add_node = false;
                        }

                        // check if not last or first rail point
                        if ((gi != 1) && (gi != _NumVertices) && add_node)
                        {
                            float distSquared = Vec3.DistSqr(_tempNode.m__position, gNodes[gNodes.Count() - 1].m__position) * 10.0f;

                            // if check if distance between this and last node is greater than 20.0f
                            if (distSquared < 20.0f)
                            {
                                add_node = false;
                            }
                        }

                        // can add node?
                        if (add_node)
                        {
                            gNodes.Add(_tempNode);
                            RailNodeIndex++;
                        }

                        // init new node for next loop
                        _tempNode = new THUGProNodeArray.NodeObject();

                        // if node was added...
                        if (add_node)
                        {
                            // if not first rail point..
                            if (gi != 1)
                            {
                                // link previous rail point to this rail point!
                                gNodes[gNodes.Count() - 2].m__links.Clear();
                                gNodes[gNodes.Count() - 2].m__links.Add(gNodes[gNodes.Count() - 1]);
                            }
                        }
                    }
                }
            }
        }

        // Read BMX2 Geometry file [*.DXG]
        SceneObject read_geometry_file(string filename, THUGProNodeArray.NodeObject node)
        {

            // TODO:

            // Figure out unknowns ?

            // Load [*.DXG] from \anim\ folder.
            //      Some levels have dynamic objects, such as vehicles, doors, moving geometry etc.
            //      See NodeArray documentation for LevelObjects.

            if (!File.Exists(filename))
                return null;

            MemoryStream ms = new MemoryStream(File.ReadAllBytes(filename.ToLower()));
            using (BinaryReader br = new BinaryReader(ms))
            {
                if (br.ReadUInt32() != 0x00677864)
                    return null;

                SceneObject _tempObject = new SceneObject();

                br.BaseStream.Seek(4, SeekOrigin.Current);

                // number of objects?
                uint _tempNumAnims = br.ReadUInt32();

                // just use filename without extension
                _tempObject.m__name = SceneObject.GetUniqueName(Path.GetFileNameWithoutExtension(filename).ToLower(), gSceneObjects);

                for (int i = 0; i < _tempNumAnims; i++)
                {
                    byte[] _objNameBytes = br.ReadBytes(128);
                    Utils.GetStringFromByteArray(_objNameBytes);

                    br.BaseStream.Seek(64, SeekOrigin.Current);

                    // bbox data??
                    br.BaseStream.Seek(12, SeekOrigin.Current);
                    br.BaseStream.Seek(12, SeekOrigin.Current);
                    br.BaseStream.Seek(4, SeekOrigin.Current);

                    br.BaseStream.Seek(4, SeekOrigin.Current); // 0xFFFFFFFF
                }

                // num model material names
                _tempObject.m__num_materials = br.ReadUInt32();
                
                // read model material names
                for (int i = 0; i < _tempObject.m__num_materials; i++)
                {
                    byte[] _matNameBytes = br.ReadBytes(64);
                    _tempObject.m__materials.Add(Utils.GetStringFromByteArray(_matNameBytes));
                }

                br.BaseStream.Seek(4, SeekOrigin.Current);
                br.BaseStream.Seek(4, SeekOrigin.Current); // flags?

                // num meshes
                _tempObject.m__num_meshes = br.ReadUInt32();

                // read meshes
                for (int i = 0; i < _tempObject.m__num_meshes; i++)
                {
                    SceneObject.Mesh _tempMesh = new SceneObject.Mesh();

                    br.BaseStream.Seek(4, SeekOrigin.Current);

                    _tempMesh.m__flags = br.ReadUInt32();

                    br.BaseStream.Seek(4, SeekOrigin.Current);

                    _tempMesh.m__num_verts = br.ReadUInt16();
                    _tempMesh.m__num_indices = br.ReadUInt16();

                    // read face indices
                    for (int fi = 0; fi < _tempMesh.m__num_indices; fi++)
                        _tempMesh.m__indices.Add(br.ReadUInt16());
                    
                    // Vertex positions
                    for (int vi = 0; vi < _tempMesh.m__num_verts; vi++)
                        _tempMesh.m__positions.Add(new Vec3(br));

                    // Vertex normals
                    for (int vi = 0; vi < _tempMesh.m__num_verts; vi++)
                        _tempMesh.m__normals.Add(new Vec3(br));

                    // Vertex colors
                    if (Utils.is_flag_set(_tempMesh.m__flags, 0x08))
                    {
                        for (int vi = 0; vi < _tempMesh.m__num_verts; vi++)
                            _tempMesh.m__colors.Add(br.ReadUInt32());
                    }
                    else
                    {
                        for (int vi = 0; vi < _tempMesh.m__num_verts; vi++)
                            _tempMesh.m__colors.Add(0xffffffff);
                    }

                    // Texture UVs
                    for (int vi = 0; vi < _tempMesh.m__num_verts; vi++)
                        _tempMesh.m__texcoords.Add(new Vec2(br));

                    _tempObject.m__meshes.Add(_tempMesh);
                }

                // apply transforms to object verts
                for (int fi = 0; fi < _tempObject.m__meshes.Count(); fi++)
                {


                    _tempObject.m__meshes[fi].m__positions = TransformVertexPositions(
                        _tempObject.m__meshes[fi].m__positions,
                        node.m__position,
                        node.m__lookVector,
                        node.m__upVector,
                        11.5f);

                    for (int vi = 0; vi < _tempObject.m__meshes[fi].m__positions.Count(); vi++)
                    {
                        // negate z axis
                        _tempObject.m__meshes[fi].m__positions[vi].z *= -1;
                        _tempObject.m__meshes[fi].m__normals[vi].z *= -1;

                        // convert vertex color range from max 256 to max 128
                        byte[] rgba = BitConverter.GetBytes(_tempObject.m__meshes[fi].m__colors[vi]);
                        rgba[0] = (byte)(rgba[0] / 2);
                        rgba[1] = (byte)(rgba[1] / 2);
                        rgba[2] = (byte)(rgba[2] / 2);
                        rgba[3] = (byte)(rgba[3] / 2);
                        _tempObject.m__meshes[fi].m__colors[vi] = (uint)BitConverter.ToInt32(rgba, 0);

                    }
                }

                if (!gSceneObjects.Contains(_tempObject))
                    gSceneObjects.Add(_tempObject);

                return _tempObject.GetObjectByName(_tempObject.m__name, gSceneObjects);
            }
        }

        // Read BMX2 Collision file [*.COL]
        CollisionObject read_collision_file(string filename)
        {

            // TODO:

            // Figure out unknowns ?

            // Read Collision Object Materials in [*.AUD] files.
            //      Convert data to THUGPro TerrainType index.

            // Read Trigger data in [*.TOY] files.
            //      Create CollisionObjects using Name, Position and Box properties.
            //      NodeArray: Create ProximNode (See NodeArray documentation).

            if (!File.Exists(filename))
                return null;

            MemoryStream ms = new MemoryStream(File.ReadAllBytes(filename.ToLower()));
            using (BinaryReader br = new BinaryReader(ms))
            {
                br.BaseStream.Seek(4, SeekOrigin.Current);
                if (br.ReadUInt32() != 0x006C6F63)
                    return null;

                CollisionObject _tempObject = new CollisionObject();
                _tempObject.m__name = Path.GetFileNameWithoutExtension(filename).ToLower();

                // set collision flags based on filename!!
                if (_tempObject.m__name.Contains("wall"))
                    _tempObject.m__flags = COLLISON_FLAGS.WALL;
                else if (_tempObject.m__name.Contains("vert"))
                    _tempObject.m__flags = COLLISON_FLAGS.VERT;
                else if (_tempObject.m__name.Contains("spine"))
                    _tempObject.m__flags = COLLISON_FLAGS.SPINE;
                else if (_tempObject.m__name.Contains("cage"))
                    _tempObject.m__flags = COLLISON_FLAGS.CAGE;
                else if (_tempObject.m__name.Contains("ramp"))
                    _tempObject.m__flags = COLLISON_FLAGS.RAMP;

                br.BaseStream.Seek(4, SeekOrigin.Current);
                br.BaseStream.Seek(4, SeekOrigin.Current); // padding ?
                br.BaseStream.Seek(48, SeekOrigin.Current);
                br.BaseStream.Seek(4, SeekOrigin.Current); // padding ?

                _tempObject.m__num_verts = br.ReadUInt32();
                uint _unk_count = br.ReadUInt32();
                _tempObject.m__num_triangles = br.ReadUInt32();
                br.BaseStream.Seek(4, SeekOrigin.Current); // padding ?

                for (int i = 0; i < _tempObject.m__num_verts; i++)
                    _tempObject.m__positions.Add(new Vec3(br));

                uint _unk = br.ReadUInt32(); // dunno
                if (_unk == 1)
                {
                    for (int i = 0; i < _tempObject.m__num_verts; i++)
                        br.BaseStream.Seek(12, SeekOrigin.Current); // dunno
                }

                for (int i = 0; i < _unk_count; i++)
                    br.BaseStream.Seek(32, SeekOrigin.Current); // dunno

                for (int i = 0; i < _tempObject.m__num_triangles; i++)
                {
                    CollisionObject.Triangle _tempTriangle = new CollisionObject.Triangle();

                    br.BaseStream.Seek(16, SeekOrigin.Current); // dunno

                    // read triangles in reverse order
                    _tempTriangle.m__c = br.ReadUInt16();
                    _tempTriangle.m__b = br.ReadUInt16();
                    _tempTriangle.m__a = br.ReadUInt16();
                    _tempObject.m__triangles.Add(_tempTriangle);
                }

                // get object world position
                THUGProNodeArray.NodeObject _tempNode = THUGProNodeArray.GetNodeByName(_tempObject.m__name, gNodes);

                // apply transforms to object verts
                _tempObject.m__positions = TransformVertexPositions(
                    _tempObject.m__positions,
                    _tempNode.m__col_position,
                    _tempNode.m__col_lookVector,
                    _tempNode.m__col_upVector,
                    11.5f);

                for (int vi = 0; vi < _tempObject.m__positions.Count(); vi++)
                {
                    // negate z axis
                    _tempObject.m__positions[vi].z *= -1;
                }

                if (!gCollisionObjects.Contains(_tempObject))
                    gCollisionObjects.Add(_tempObject);

                return _tempObject.GetObjectByName(_tempObject.m__name, gCollisionObjects);
            }
        }

        public float ParseFloat(string _string)
        {
            float ret = 0.0f;

            if (_string != String.Empty)
            {
                ret = float.Parse(_string.Replace(" ", String.Empty), System.Globalization.CultureInfo.InvariantCulture);
            }

            return ret;
        }

        public static Matrix CreateWorld(Vec3 position, Vec3 forward, Vec3 up)
        {
            Matrix result;
            result = Matrix.IdentityMatrix(4, 4);

            Vec3 x, y, z;

            bool UseDefaultUp = false;

            if (up == null)
            {
                UseDefaultUp = true;
                up = new Vec3(0.0f, 1.0f, 0.0f);
            }

            z = Vec3.Normalize(forward);
            x = Vec3.Cross(forward, up);
            y = Vec3.Cross(x, forward);

            x.Normalize();
            y.Normalize();


            // Right
            if (UseDefaultUp)
                result.Right = x;
            else
                result.Left = x;

            // Up
            result.Up = y;

            // Forward
            if (UseDefaultUp)
                result.Forward = z;
            else
                result.Backward = z;

            // Translation
            result.Translation = position;

            // Scale
            //result.Scale = new Vec3(1.0f, 1.0f, 1.0f);
            return result;
        }

        // Process BMX2 vertex positions.
        List<Vec3> TransformVertexPositions(List<Vec3> _positions, Vec3 _worldPosition, Vec3 _lookVector, Vec3 _upVector, float _scale)
        {

            // TODO:

            Matrix worldTransform = CreateWorld(_worldPosition, _lookVector, _upVector);

            for (int vi = 0; vi < _positions.Count(); vi++)
            {
                // [...]

                // Apply model transform
                _positions[vi] = ((_positions[vi] * worldTransform) * _scale);

            }

            return _positions;
        }

        bool read_texture_file(string filename, Material.TexturePass texturePass)
        {
            if (!File.Exists(filename))
                return false;

            MemoryStream ms = new MemoryStream(File.ReadAllBytes(filename.ToLower()));
            using (BinaryReader br = new BinaryReader(ms))
            {
                Texture _tempTexture = new Texture();

                // check if animated texture
                if (filename.EndsWith(".atx"))
                {
                    br.ReadUInt32();
                    uint _numTextures = br.ReadUInt32();
                    br.ReadUInt32();

                    // atx files contain multiple dxt files
                    for (int tex = 0; tex < _numTextures; tex++)
                    {
                        // read dxt
                        _tempTexture = new Texture(br);

                        // append frame index to last read texture
                        _tempTexture.m__name = String.Format("{0}_frame{1}", Path.GetFileNameWithoutExtension(Path.GetFileName(filename)), tex.ToString("D2"));

                        if (!Texture.TextureNameExists(_tempTexture.m__name, gTextures))
                        {
                            // add texure to master texture list
                            gTextures.Add(_tempTexture);

                            // add texture reference to the material texture list
                            texturePass.m__textures.Add(gTextures[gTextures.Count() - 1]);
                        }
                        else
                        {
                            // add texture reference to the material texture list
                            texturePass.m__textures.Add(Texture.GetTextureByName(_tempTexture.m__name, gTextures));
                        }
                    }
                }
                else
                {
                    // read dxt
                    _tempTexture = new Texture(br);
                    
                    // single texture, just use filename
                    _tempTexture.m__name = String.Format("{0}", Path.GetFileNameWithoutExtension(Path.GetFileName(filename)));

                    if (!Texture.TextureNameExists(_tempTexture.m__name, gTextures))
                    {
                        // add texure to master texture list
                        gTextures.Add(_tempTexture);

                        // add texture reference to the material texture list
                        texturePass.m__textures.Add(gTextures[gTextures.Count() - 1]);
                    }
                    else
                    {
                        // add texture reference to the material texture list
                        texturePass.m__textures.Add(Texture.GetTextureByName(_tempTexture.m__name, gTextures));
                    }
                }
                br.Close();
            }
            return true;
        }

        // Read BMX2 Material file [*.DXM]
        void read_material_file(string filename)
        {

            // TODO:

            // Read other material properties.

            // Extend the Material class to support other material properties.

            // Convert material properties to THUGPro material properties.
            //      blend modes, uv animation, multiple shader passes

            if (!File.Exists(filename))
            {
                Console.WriteLine("OH SHIT! Missing material {0}", filename);

                Material _defaultMaterial = new Material();
                _defaultMaterial.m__name = Path.GetFileNameWithoutExtension(filename).ToLower();
                _defaultMaterial.m__num_texture_passes = 1;

                Material.TexturePass _defaultMaterialTexturePass = new Material.TexturePass();

                _defaultMaterialTexturePass.m__textureMap = "NoTexture";
                _defaultMaterialTexturePass.m__blend_mode = Material.TextureBlendMode.DIFFUSE;
                _defaultMaterial.m__texture_passes.Add(_defaultMaterialTexturePass);

                if (!gMaterials.Contains(_defaultMaterial))
                    gMaterials.Add(_defaultMaterial);

                return;
            }

            // Load material file as IniFile
            IniFile _materialInfo = new IniFile();
            _materialInfo.Load(filename);

            Material _tempMaterial = new Material();
            _tempMaterial.m__name = Path.GetFileNameWithoutExtension(filename).ToLower();

            uint TotalShaderPasses = UInt32.Parse(_materialInfo.GetKeyValue("General", "TotalShaderPasses"));
            _tempMaterial.m__num_texture_passes = TotalShaderPasses;

            // ShaderPasses
            for (int i = 0; i < TotalShaderPasses; i++)
            {
                // get shader pass section
                IniFile.IniSection _ShaderPass = _materialInfo.GetSection(String.Format("ShaderPass_{0}", i+1));

                // get texture name
                IniFile.IniSection.IniKey _TextureMap = _ShaderPass.GetKey("TextureMap_1");

                // maybe get animation type
                IniFile.IniSection.IniKey _TextureAnimation = _ShaderPass.GetKey("TextureAnimation_1");

                // maybe get animation fps
                IniFile.IniSection.IniKey _TextureAnimationFPS = _ShaderPass.GetKey("animatedMapFramesPerSecond_1");

                // get alpha ref / cutoff(?)
                IniFile.IniSection.IniKey _ALPHAREF = _ShaderPass.GetKey("ALPHAREF");

                // get blend src
                IniFile.IniSection.IniKey _SRCBLEND = _ShaderPass.GetKey("SRCBLEND");

                // get blend dest
                IniFile.IniSection.IniKey _DESTBLEND = _ShaderPass.GetKey("DESTBLEND");

                // get alpha blend enable
                IniFile.IniSection.IniKey _ALPHABLENDENABLE = _ShaderPass.GetKey("ALPHABLENDENABLE");

                // get blend color
                IniFile.IniSection.IniKey _BLENDCOLOR = _ShaderPass.GetKey("BLENDCOLOR");


                // maybe get animation type uvs
                IniFile.IniSection.IniKey _TextureUVAnimation = _ShaderPass.GetKey("uvs_animate_1");
                IniFile.IniSection.IniKey _TextureUVAnimation_U_Vel = _ShaderPass.GetKey("u_velocity_1");
                IniFile.IniSection.IniKey _TextureUVAnimation_V_Vel = _ShaderPass.GetKey("v_velocity_1");

                Material.TexturePass _tempTexturePass = new Material.TexturePass();

                if (null != _TextureMap)
                {
                    // got texture name
                    _tempTexturePass.m__textureMap = _TextureMap.Value.Replace(" ", String.Empty); // remove whitespace

                    /*
                    if (_TextureMap.Value.Contains("waterwake"))
                    {
                        _tempTexturePass.m__textureMap = "labrick01";
                    }
                    */

                }

                if (null != _TextureAnimation)
                {
                    // got texture animation type
                    if (_TextureAnimation.Value.ToLower().Contains("unique"))
                    {
                        // material uses keyframe texture animation
                        _tempTexturePass.m__textureAnimation.m__type = Material.TextureAnimationType.UNIQUE;
                    }

                    if (null != _TextureAnimationFPS)
                    {
                        _tempTexturePass.m__textureAnimation.animatedMapFramesPerSecond = Single.Parse(_TextureAnimationFPS.Value, System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        _tempTexturePass.m__textureAnimation.animatedMapFramesPerSecond = 30.0f;
                    }
                }

                if (null != _TextureUVAnimation)
                {
                    if (UInt32.Parse(_TextureUVAnimation.Value) == 1)
                    {
                        _tempTexturePass.m__textureAnimation.m__type = Material.TextureAnimationType.UV;

                        if (_TextureUVAnimation_U_Vel != null)
                            _tempTexturePass.m__textureAnimation.u_velocity = Single.Parse(_TextureUVAnimation_U_Vel.Value, System.Globalization.CultureInfo.InvariantCulture);
                        else
                            _tempTexturePass.m__textureAnimation.u_velocity = 1.0f;

                        if (_TextureUVAnimation_V_Vel != null)
                            _tempTexturePass.m__textureAnimation.v_velocity = Single.Parse(_TextureUVAnimation_V_Vel.Value, System.Globalization.CultureInfo.InvariantCulture);
                        else
                            _tempTexturePass.m__textureAnimation.v_velocity = 1.0f;

                    }
                }

                if (null != _ALPHAREF)
                {
                    // got alpha ref
                    _tempTexturePass.m__alpha_ref = UInt32.Parse(_ALPHAREF.Value);
                }

                if (null != _BLENDCOLOR)
                {
                    _tempTexturePass.m__blend_color = Convert.ToUInt32(_BLENDCOLOR.Value.Replace(" ", String.Empty), 16);
                }

                _tempTexturePass.m__blend_mode = Material.TextureBlendMode.DIFFUSE;

                if (null != _ALPHABLENDENABLE)
                {
                    if (_ALPHABLENDENABLE.Value.ToLower().Contains("true"))
                    {

                        if (null == _SRCBLEND)
                            _SRCBLEND.Value = "D3DBLEND_SRCALPHA";

                        if (null == _DESTBLEND)
                            _DESTBLEND.Value = "D3DBLEND_INVSRCALPHA";

                        String blendMode = String.Format("{0}|{1}", _SRCBLEND.Value.ToUpper().Replace(" ", String.Empty), _DESTBLEND.Value.ToUpper().Replace(" ", String.Empty));

                        switch (blendMode)
                        {

                            case "D3DBLEND_SRCALPHA|D3DBLEND_INVSRCALPHA":
                                _tempTexturePass.m__blend_mode = Material.TextureBlendMode.BLEND;
                                _tempTexturePass.m__transparent = true;
                                break;

                            case "D3DBLEND_CONSTANTALPHA|D3DBLEND_INVCONSTANTALPHA":
                                //_tempTexturePass.m__blend_mode = Material.TextureBlendMode.BLEND; // FIXED
                                _tempTexturePass.m__blend_mode = Material.TextureBlendMode.DIFFUSE;
                                _tempTexturePass.m__transparent = true;
                                break;

                            case "D3DBLEND_SRCALPHA|D3DBLEND_ONE":
                                _tempTexturePass.m__blend_mode = Material.TextureBlendMode.ADD;
                                _tempTexturePass.m__transparent = true;
                                break;

                            case "D3DBLEND_CONSTANTALPHA|D3DBLEND_ONE":
                                _tempTexturePass.m__blend_mode = Material.TextureBlendMode.ADD_FIXED; // or SUB_FIXED
                                _tempTexturePass.m__transparent = true;
                                break;

                            default:
                                Console.WriteLine("Unknown BlendMode Combo = '{0}'", blendMode);
                                _tempTexturePass.m__blend_mode = Material.TextureBlendMode.DIFFUSE;
                                break;

                        };
                    }
                }

                // add pass to temp material
                _tempMaterial.m__texture_passes.Add(_tempTexturePass);
            }

            // maybe add material to master list
            if (!gMaterials.Contains(_tempMaterial))
                gMaterials.Add(_tempMaterial);
        }

        // Write collision .obj file
        void write_obj_file(string filename, List<CollisionObject> meshes)
        {
            StreamWriter dump = new StreamWriter(filename);

            dump.WriteLine(String.Format("# dump file '{0}'", Path.GetFileName(filename).ToLower()));
            dump.WriteLine();

            uint _FACE_INDEX_OFFSET = 1;
            foreach (CollisionObject _object in meshes)
            {
                dump.WriteLine(String.Format("# {0}", _object.m__name));
                dump.WriteLine(String.Format("g {0}", _object.m__name));
                dump.WriteLine("# verts");
                for (int vi = 0; vi < _object.m__positions.Count(); vi++)
                {
                    string str_vert = string.Format(
                        "v {0} {1} {2}",
                        _object.m__positions[vi].x,
                        _object.m__positions[vi].y,
                        _object.m__positions[vi].z);

                    dump.WriteLine(str_vert.Replace(",", "."));
                }

                dump.WriteLine("# tris");
                for (int fi = 0; fi < _object.m__triangles.Count(); fi++)
                {
                    dump.WriteLine("f {0} {1} {2}", _object.m__triangles[fi].m__a + _FACE_INDEX_OFFSET, _object.m__triangles[fi].m__b + _FACE_INDEX_OFFSET, _object.m__triangles[fi].m__c + _FACE_INDEX_OFFSET);
                }

                _FACE_INDEX_OFFSET += (uint)_object.m__positions.Count();
            }
            dump.Close();
        }

        // Write scene .obj file
        void write_obj_file(string filename, List<SceneObject> meshes)
        {
            StreamWriter dump = new StreamWriter(filename);

            dump.WriteLine(String.Format("# dump file '{0}'", Path.GetFileName(filename).ToLower()));
            dump.WriteLine();

            uint _FACE_INDEX_OFFSET = 1;

            foreach (SceneObject _object in meshes)
            {
                dump.WriteLine(String.Format("# {0}", _object.m__name));
                dump.WriteLine(String.Format("g {0}", _object.m__name));

                for (int i = 0; i < _object.m__meshes.Count(); i++)
                {
                    dump.WriteLine();
                    dump.WriteLine(String.Format("# submesh {0}", i));

                    dump.WriteLine("# verts");

                    for (int vi = 0; vi < _object.m__meshes[i].m__positions.Count(); vi++)
                    {
                        string str_vert = string.Format(
                            "v {0} {1} {2}",
                            _object.m__meshes[i].m__positions[vi].x,
                            _object.m__meshes[i].m__positions[vi].y,
                            _object.m__meshes[i].m__positions[vi].z);

                        dump.WriteLine(str_vert.Replace(",", "."));
                    }

                    dump.WriteLine("# normals");
                    for (int vi = 0; vi < _object.m__meshes[i].m__normals.Count(); vi++)
                    {
                        string str_norm = string.Format("vn {0} {1} {2}", _object.m__meshes[i].m__normals[vi].x, _object.m__meshes[i].m__normals[vi].y, _object.m__meshes[i].m__normals[vi].z);
                        dump.WriteLine(str_norm.Replace(",", "."));
                    }

                    dump.WriteLine("# tex coords");
                    for (int vi = 0; vi < _object.m__meshes[i].m__texcoords.Count(); vi++)
                    {
                        string str_uv = string.Format("vt {0} {1} {2}", _object.m__meshes[i].m__texcoords[vi].u, _object.m__meshes[i].m__texcoords[vi].v, 0.5f);
                        dump.WriteLine(str_uv.Replace(",", "."));
                    }

                    dump.WriteLine(String.Format("# faces", i));

                    ushort v1, v2, v3;
                    int tmp = 1;

                    dump.WriteLine("# tris");
                    for (int lp3 = 0; lp3 < _object.m__meshes[i].m__indices.Count() - 2; lp3++)
                    {
                        if (tmp == 0)
                        {
                            v1 = _object.m__meshes[i].m__indices[lp3];
                            v2 = _object.m__meshes[i].m__indices[lp3 + 2];
                            v3 = _object.m__meshes[i].m__indices[lp3 + 1];
                            tmp = 1;
                        }
                        else
                        {
                            v1 = _object.m__meshes[i].m__indices[lp3];
                            v2 = _object.m__meshes[i].m__indices[lp3 + 1];
                            v3 = _object.m__meshes[i].m__indices[lp3 + 2];
                            tmp = 0;
                        }

                        if (v1 != v2 && v1 != v3 && v2 != v3)
                        {
                            dump.WriteLine("f {0} {1} {2}", v1 + _FACE_INDEX_OFFSET, v2 + _FACE_INDEX_OFFSET, v3 + _FACE_INDEX_OFFSET);
                        }
                    }
                    _FACE_INDEX_OFFSET += (uint)_object.m__meshes[i].m__positions.Count();
                }
                dump.WriteLine();
            }
            dump.Close();
        }

        // __SCN_FILE dialog
        private void button4_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "Scene info files (*.scn)|*.scn|All files (*.*)|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = openFileDialog1.FileName;
                __SCN_FILE = textBox2.Text;
            }
        }

        // __GEOMETRY_PATH dialog
        private void button5_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox3.Text = String.Format("{0}\\", folderBrowserDialog1.SelectedPath);
                __GEOMETRY_PATH = textBox3.Text;
            }
        }

        // __TEXTURE_PATH dialog
        private void button6_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox4.Text = String.Format("{0}\\", folderBrowserDialog1.SelectedPath);
                __TEXTURE_PATH = textBox4.Text;
            }
        }

        // __SCN_FILE texbox
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            __SCN_FILE = ((TextBox)sender).Text;
            WriteSettingsToFile();
        }

        // __GEOMETRY_PATH texbox
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            __GEOMETRY_PATH = ((TextBox)sender).Text;
            WriteSettingsToFile();
        }

        // __TEXTURE_PATH texbox
        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            __TEXTURE_PATH = ((TextBox)sender).Text;
            WriteSettingsToFile();
        }

        bool sceneLoaded = false;

        // Load / Process files
        private void button3_Click(object sender, EventArgs e)
        {
            if (!File.Exists(__SCN_FILE))
            {
                if (__SCN_FILE == string.Empty)
                    MessageBox.Show("No SCN file selected!");
                else
                    MessageBox.Show("SCN file does not exist!");

                return;
            }

            if (!Directory.Exists(__GEOMETRY_PATH))
            {
                if (__GEOMETRY_PATH == string.Empty)
                    MessageBox.Show("No geometry path file selected!");
                else
                    MessageBox.Show("Geometry path does not exist!");

                return;
            }

            if (!Directory.Exists(__TEXTURE_PATH))
            {
                if (__TEXTURE_PATH == string.Empty)
                    MessageBox.Show("No texture path file selected!");
                else
                    MessageBox.Show("Texture path does not exist!");

                return;
            }

            // Read main SceneInfo file
            read_scene_info_file(__SCN_FILE);

            // Read rail data file
            read_rail_data_file(__SCN_FILE);

            // Read trigger data file
            string __TOY_FILE = __SCN_FILE.ToLower().Replace(".scn", ".toy");
            if (File.Exists(__TOY_FILE))
            {
                read_trigger_data_file(__TOY_FILE);
            }

            // TODO: Read other level info files.
            //      [.TOY], [.AUD] and [.SGR] files...

            sceneLoaded = true;
        }

        // Export THUGPro scene .scn dialog
        private void button1_Click(object sender, EventArgs e)
        {
            if (!sceneLoaded)
            {
                MessageBox.Show("No scene loaded!!");
                return;
            }

            saveFileDialog1.Filter = "THUG2 Scene (*.scn.xbx)|*.scn.xbx|All files (*.*)|*.*";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    THUGProScene.write_scene(saveFileDialog1.FileName, gSceneObjects, gMaterials, gNodes, gTextures);
                }
                catch (FormatException ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.InnerException.StackTrace);
                }
            }
        }

        // Export Wavefront .obj dialog
        private void button2_Click(object sender, EventArgs e)
        {
            if (!sceneLoaded)
            {
                MessageBox.Show("No scene loaded!!");
                return;
            }

            saveFileDialog1.Filter = "Wavefront OBJ (*.obj)|*.obj|All files (*.*)|*.*";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    write_obj_file(saveFileDialog1.FileName, gSceneObjects);
                    write_obj_file(saveFileDialog1.FileName.Replace(".obj", "_col.obj"), gCollisionObjects);
                }
                catch (FormatException ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.InnerException.StackTrace);
                }
            }
        }

        // Export THUGPro collision .col dialog
        private void button8_Click(object sender, EventArgs e)
        {
            if (!sceneLoaded)
            {
                MessageBox.Show("No scene loaded!!");
                return;
            }

            saveFileDialog1.Filter = "THUG2 Collision (*.col.xbx)|*.col.xbx|All files (*.*)|*.*";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    THUGProCollision.write_collision(saveFileDialog1.FileName, gCollisionObjects, gNodes);
                }
                catch (FormatException ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.InnerException.StackTrace);
                }
            }
        }

        // Export THUGPro nodearray .qb dialog
        private void button7_Click(object sender, EventArgs e)
        {
            if (!sceneLoaded)
            {
                MessageBox.Show("No scene loaded!!");
                return;
            }

            saveFileDialog1.Filter = "NodeArray QB (*.qb)|*.qb|All files (*.*)|*.*";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    THUGProNodeArray.write_qb_test(saveFileDialog1.FileName, gNodes);
                }
                catch (FormatException ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.InnerException.StackTrace);
                }
            }
        }

        // Export textures
        private void button9_Click(object sender, EventArgs e)
        {
            if (!sceneLoaded)
            {
                MessageBox.Show("No scene loaded!!");
                return;
            }

            /*
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                // dump dds files to folder 
                Texture.dump_textures(folderBrowserDialog1.SelectedPath, gTextures);
            }
            */

            saveFileDialog1.Filter = "Texture Container TEX (*.tex.xbx)|*.tex.xbx|All files (*.*)|*.*";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // save texture container
                    Texture.write_texture_container(saveFileDialog1.FileName, gTextures);
                }
                catch (FormatException ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.InnerException.StackTrace);
                }
            }
            
        }
    }
}
