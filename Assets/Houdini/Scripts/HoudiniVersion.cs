/*
 * PROPRIETARY INFORMATION.  This software is proprietary to
 * Side Effects Software Inc., and is not to be reproduced,
 * transmitted, or disclosed in any way without written permission.
 *
 * Produced by:
 *      Side Effects Software Inc
 *      123 Front Street West, Suite 1401
 *      Toronto, Ontario
 *      Canada   M5J 2M2
 *      416-504-9876
 *
 * COMMENTS:
 *      Generated version information to be used when linking for sanity checks.
 */

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// WARNING! This file is GENERATED by Make.
// DO NOT modify manually or commit to the repository!
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Versions

public struct HoudiniVersion
{
	public const int HOUDINI_MAJOR			= 13;
	public const int HOUDINI_MINOR			= 0;
	public const int HOUDINI_BUILD			= 475;

	public const int ORIG_HOUDINI_MAJOR		= 13;
	public const int ORIG_HOUDINI_MINOR		= 0;
	public const int ORIG_HOUDINI_BUILD		= 475;

	public const int HOUDINI_ENGINE_MAJOR	= 1;
	public const int HOUDINI_ENGINE_MINOR	= 7;
	
	public const int HOUDINI_ENGINE_API		= 24;
	
#if UNITY_STANDALONE_WIN
	public const string HAPI_LIBRARY		= "libHAPI";
#elif UNITY_STANDALONE_OSX
	public const string HAPI_LIBRARY		= "/Library/Frameworks/Houdini.framework/Versions/13.0.475/Houdini";
#else
	#error Unsupported platform!
#endif
};
