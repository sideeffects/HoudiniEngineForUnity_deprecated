# Houdini Engine for Unity (deprecated)

### Deprecated Repo
**Please note that this plugin version has been deprecated and the repo is now archived.**
**The repo for the latest version of the plugin is here: **
https://github.com/sideeffects/HoudiniEngineForUnity


Houdini Engine for Unity is a Unity plug-in that allows deep integration of
Houdini technology into Unity through the use of Houdini Engine.

This plug-in brings Houdini's powerful and flexible procedural workflow into
Unity through Houdini Digital Assets. Artists can interactively adjust the
asset's parameters inside Unity, and use Unity geometries as an asset's inputs.
Houdini's procedural engine will then "cook" the asset and the results will be
available right inside Unity.

The easiest way for artists to access the plug-in is to download the latest
production build of Houdini or the [FREE Houdini Apprentice Learning
Edition](https://www.sidefx.com/products/houdini-apprentice/)
and install the Unity plug-in along with the Houdini interactive software.
Houdini Digital Assets created in either Houdini or Apprentice can then be
loaded into Unity through the plug-in. A growing library of Digital Assets for
use in Unity will be available at the [Orbolt Smart 3D Asset
Store](http://www.orbolt.com/unity).

For more information:

* [Houdini Engine for Unity](https://www.sidefx.com/products/houdini-engine/unity-plug-in/)
* [FAQ](https://www.sidefx.com/faq/houdini-engine-faq/)

For support and reporting bugs:

* [SideFX Houdini Engine for Unity forum](https://www.sidefx.com/forum/50/)
* [Bug Submission](https://www.sidefx.com/bugs/submit/)

## Supported Unity versions
Currently, the supported Unity versions are:

* 4.6
* 5.6
* 2017.2

## Installing from Source
1. Fork this repository to your own Github account using the Fork button at the top.
1. Clone the forked repository where you'd normally save your Unity projects.
1. Download and install the correct build of Houdini. You must have the exact build number and version as HOUDINI_MAJOR, HOUDINI_MINOR, and HOUDINI_BUILD int values in Assets/Houdini/Scripts/HoudiniVersion.cs. You can get the correct build from: http://www.sidefx.com/download/daily-builds (you might need to wait for the build to finish and show up if you're updating to the very latest version of the plugin)
1. Open Unity and open a new project by selecting the folder containing your cloned repository (the folder containing the Assets folder).
1. Restart Unity.
1. Ensure Houdini Engine loaded successfully by going to the "Houdini Engine" top menu and selecting "Installation Info" and making sure all the versions match.
