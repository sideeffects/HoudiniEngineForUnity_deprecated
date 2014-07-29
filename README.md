# Houdini Engine for Unity
Houdini Engine for Unity is a Unity plug-in that allows deep integration of
Houdini technology into Unity through the use of Houdini Engine.

This plug-in brings Houdini's powerful and flexible procedural workflow into
Unity through Houdini Digital Assets. Artists can interactively adjust the
asset's parameters inside Unity, and use Unity geometries as an asset's inputs.
Houdini's procedural engine will then "cook" the asset and the results will be
available right inside Unity.

The plug-in is currently released as a public preview. During the experimental
testing period, the Unity plug-in will be available freely.

The easiest way for artists to access the plug-in is to download the latest
production build of 32-bit Houdini or the [FREE Houdini Apprentice Learning
Edition](http://www.sidefx.com/index.php?option=com_download&task=apprentice&Itemid=208)
and install the Unity plug-in along with the Houdini interactive software.
Houdini Digital Assets created in either Houdini or Apprentice can then be
loaded into Unity through the plug-in. A growing library of Digital Assets for
use in Unity will be available at the [Orbolt Smart 3D Asset
Store](http://www.orbolt.com/unity).

For more information:

* [Houdini Engine for Unity](http://www.sidefx.com/unity)
* [FAQ](http://www.sidefx.com/index.php?option=com_content&task=view&id=2618&Itemid=393)
* [SideFX Labs](http://labs.sidefx.com)

For support and reporting bugs:

* [SideFX Labs forum](http://www.sidefx.com/index.php?option=com_forum&Itemid=172&page=viewforum&f=46)
* [Bug Submission](http://www.sidefx.com/index.php?option=com_content&task=view&id=768&Itemid=239)

## Supported Unity versions
Currently, the supported Unity versions are:

* 4.5

The plug-in is supported on Windows only for now.

## Installing from Source
1. Fork this repository to your own Github account using the Fork button at the top.
1. Clone the forked repository where you'd normally save your Unity projects.
1. Download and install the correct build of 32-bit Houdini. You must have the exact build number and version as HOUDINI_MAJOR, HOUDINI_MINOR, and HOUDINI_BUILD int values in Assets/HAPI/Scripts/HAPI_Version.cs. You can get the correct build from: http://www.sidefx.com/index.php?option=com_download&Itemid=208 (you might need to wait for the build to finish and show up if you're updating to the very latest version of the plugin)
1. Open Unity and open a new project by selecting the folder containing your cloned repository (the folder containing the Assets folder).
1. Restart Unity.
1. Open the console and ensure Houdini Engine loaded successfully by looking for an entry starting with "Linking Against Houdini Engine Version:". You should also see a menu at the top named _Houdini Engine_.
1. You are now ready to load Houdini Digital Assets into Unity.



