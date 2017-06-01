/*
* Copyright (c) <2017> Side Effects Software Inc.
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*
* Produced by:
*      Side Effects Software Inc
*      123 Front Street West, Suite 1401
*      Toronto, Ontario
*      Canada   M5J 2M2
*      416-504-9876
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
#if UNITY_EDITOR_WIN || ( UNITY_METRO && UNITY_EDITOR )
		"opengl32.dll";
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_LINUX
		"/System/Library/Frameworks/OpenGL.framework/OpenGL";
#elif UNITY_STANDALONE_LINUX
		"libGL"; // Untested on Linux, this may not be correct.
#else
		null; // OpenGL ES platforms don't require this feature.
#endif

	private static bool myIsOpenGL;

#if ( UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_LINUX || ( UNITY_METRO && UNITY_EDITOR ) || UNITY_STANDALONE_LINUX )
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
