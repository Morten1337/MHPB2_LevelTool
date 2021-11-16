MHPB2 Level Tool
====================

This is an old tool I built for converting level file formats from **Mat Hoffman's Pro BMX 2** to **THUG Pro**.
It's a GUI application, and most of the code is found in the Form.cs file... *sigh*

----

##### Supported games
- Mat Hoffman's Pro BMX 2 `(Xbox)`

##### Notes

- Currently there is some data loss going from **mhpb2→thugpro→blender.**
Might be worth looking into making this a native blender importer, as the levels require some manual work anyways.

- Trick Objects / Graffiti nodes needs to be set up. (Difficult to fully automate this process).

- Import dynamic objects, lights, particle effects etc...

- Improve automated collision flag detection. (Produces bad results in many cases).

- Improve generation of supporting geometry and script for gaps and killskater-areas.

----

##### Images
![image](https://github.com/Morten1337/MHPB2_LevelTool/raw/master/Docs/Images/image_collision_flags.png "image_collision_flags")
![image](https://github.com/Morten1337/MHPB2_LevelTool/raw/master/Docs/Images/IMG_06052016_024655.png "IMG_06052016_024655")
![image](https://github.com/Morten1337/MHPB2_LevelTool/raw/master/Docs/Images/IMG_08042016_014118.png "IMG_08042016_014118")
![image](https://github.com/Morten1337/MHPB2_LevelTool/raw/master/Docs/Images/IMG_09042016_031027.png "IMG_09042016_031027")
![image](https://github.com/Morten1337/MHPB2_LevelTool/raw/master/Docs/Images/IMG_22042016_190604.png "IMG_22042016_190604")
![image](https://github.com/Morten1337/MHPB2_LevelTool/raw/master/Docs/Images/IMG_22042016_192821.png "IMG_22042016_192821")
![image](https://github.com/Morten1337/MHPB2_LevelTool/raw/master/Docs/Images/IMG_22042016_230907.png "IMG_22042016_230907")
![image](https://github.com/Morten1337/MHPB2_LevelTool/raw/master/Docs/Images/IMG_23042016_000028.png "IMG_23042016_000028")
![image](https://github.com/Morten1337/MHPB2_LevelTool/raw/master/Docs/Images/IMG_31032016_030655.png "IMG_31032016_030655")
![image](https://github.com/Morten1337/MHPB2_LevelTool/raw/master/Docs/Images/IMG_31032016_032832.png "IMG_31032016_032832")
![image](https://github.com/Morten1337/MHPB2_LevelTool/raw/master/Docs/Images/IMG_31032016_043331.png "IMG_31032016_043331")
![image](https://github.com/Morten1337/MHPB2_LevelTool/raw/master/Docs/Images/IMG_31032016_043452.png "IMG_31032016_043452")