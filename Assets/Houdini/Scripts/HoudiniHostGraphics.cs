/*
 * PROPRIETARY INFORMATION.  This software is proprietary to
 * Side Effects Software Inc., and is not to be reproduced,
 * transmitted, or disclosed in any way without written permission.
 *
 * Produced by:
 *      Side Effects Software Inc
 *		123 Front Street West, Suite 1401
 *		Toronto, Ontario
 *		Canada   M5J 2M2
 *		416-504-9876
 *
 * COMMENTS:
 * 		Utility functions for graphics hacks, mostly.
 * 		
 *		OpenGL point size trick inspired by:
 *		http://answers.unity3d.com/questions/519670/shader-inconsistency-between-opengl-and-directx.html
 * 
 */

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public static partial class HoudiniHost
{
	const uint GL_VERTEX_PROGRAM_POINT_SIZE = 0x8642;
	const string LibGLPath =
#if UNITY_STANDALONE_WIN || ( UNITY_METRO && UNITY_EDITOR )
		"opengl32.dll";
#elif UNITY_STANDALONE_OSX
		"/System/Library/Frameworks/OpenGL.framework/OpenGL";
#elif UNITY_STANDALONE_LINUX
		"libGL"; // Untested on Linux, this may not be correct.
#else
		null; // OpenGL ES platforms don't require this feature.
#endif

	private static bool myIsOpenGL;

#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) || UNITY_STANDALONE_LINUX )
	[ DllImport( LibGLPath ) ]
	private static extern void glEnable( uint cap );

	public static void preDrawSetup()
	{
		myIsOpenGL = SystemInfo.graphicsDeviceVersion.Contains( "OpenGL" );

		if ( myIsOpenGL )
			glEnable( GL_VERTEX_PROGRAM_POINT_SIZE );
	}
#else
	public static void preDrawSetup()
	{
		// This is purely to avoid the unused variable warning. Sigh indeed.
		myIsOpenGL = !!myIsOpenGL;
	}
#endif
}
