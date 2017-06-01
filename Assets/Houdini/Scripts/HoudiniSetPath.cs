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

// Master control for enabling runtime.
#if ( UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_LINUX )
	#define HAPI_ENABLE_RUNTIME
#endif

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

#if UNITY_EDITOR_WIN && HAPI_ENABLE_RUNTIME
using System;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
#endif // UNITY_EDITOR_WIN

#if UNITY_EDITOR
[ InitializeOnLoad ]
#endif // UNITY_EDITOR
public class HoudiniSetPath
{
	static HoudiniSetPath()
	{
		setPath();
	}

#if UNITY_EDITOR_WIN && HAPI_ENABLE_RUNTIME
	public enum RegSAM
	{
		QueryValue = 0x0001,
		SetValue = 0x0002,
		CreateSubKey = 0x0004,
		EnumerateSubKeys = 0x0008,
		Notify = 0x0010,
		CreateLink = 0x0020,
		WOW64_32Key = 0x0200,
		WOW64_64Key = 0x0100,
		WOW64_Res = 0x0300,
		Read = 0x00020019,
		Write = 0x00020006,
		Execute = 0x00020019,
		AllAccess = 0x000f003f
	}

	public static UIntPtr HKEY_LOCAL_MACHINE = new UIntPtr( 0x80000002u );
	public static UIntPtr HKEY_CURRENT_USER = new UIntPtr( 0x80000001u );

	[DllImport("Advapi32.dll")]
	static extern uint RegOpenKeyEx(
		UIntPtr hKey,
		string lpSubKey,
		uint ulOptions,
		int samDesired,
		out int phkResult);

	[DllImport("advapi32.dll", EntryPoint = "RegQueryValueEx")]
	public static extern int RegQueryValueEx(
		int hKey,
		string lpValueName,
		int lpReserved,
		ref uint lpType,
		System.Text.StringBuilder lpData,
		ref uint lpcbData );

	[DllImport("Advapi32.dll")]
	static extern uint RegCloseKey( int hKey );

	static public string getRegKeyValue_x64( UIntPtr root_key, String key_name, String inPropertyName )
	{
		return getRegKeyValue( root_key, key_name, RegSAM.WOW64_64Key, inPropertyName);
	}

	static public string getRegKeyValue_x86( UIntPtr root_key, String key_name, String inPropertyName )
	{
		return getRegKeyValue( root_key, key_name, RegSAM.WOW64_32Key, inPropertyName);
	}

	static public string getRegKeyValue( UIntPtr root_key, String key_name, RegSAM is32or64key, String inPropertyName)
	{
		int phkResult = 0;

		try
		{
			uint lResult = RegOpenKeyEx( root_key, key_name, 0, (int) RegSAM.QueryValue | (int) is32or64key, out phkResult );
			if ( lResult != 0 )
				return null;
			uint lpType = 0;
			uint lpcbData = 1024;
			StringBuilder value_buffer = new StringBuilder( 1024 );
			RegQueryValueEx( phkResult, inPropertyName, 0, ref lpType, value_buffer, ref lpcbData );
			string value = value_buffer.ToString();
			return value;
		}
		finally
		{
			if ( phkResult != 0 )
				RegCloseKey( phkResult );
		}
	}
#endif // UNITY_EDITOR_WIN && HAPI_ENABLE_RUNTIME

	public static string getHoudiniPath()
	{
		string houdini_app_path = "";
#if UNITY_EDITOR_WIN

		string hapi_path = "";
#if HAPI_ENABLE_RUNTIME
		hapi_path = System.Environment.GetEnvironmentVariable(
			"HAPI_PATH", System.EnvironmentVariableTarget.Machine );
		if ( hapi_path == null || hapi_path.Length == 0 )
			hapi_path = System.Environment.GetEnvironmentVariable(
				"HAPI_PATH", System.EnvironmentVariableTarget.User );
		if ( hapi_path == null || hapi_path.Length == 0 )
			hapi_path = System.Environment.GetEnvironmentVariable(
				"HAPI_PATH", System.EnvironmentVariableTarget.Process );
#endif // HAPI_ENABLE_RUNTIME

		if ( hapi_path != null && hapi_path.Length > 0 )
		{
			//Debug.Log( "Using Custom Houdini Path: " + hapi_path );
			houdini_app_path = hapi_path;
		}
		else
		{
			// HAPI_PATH variable not set therefore we must find another way to detect an installation of
			// Houdini. This step is platform dependant hence the #if's.

			string current_app_name = "Houdini Engine";
			while ( true )
			{
				try
				{
					houdini_app_path = getAppPath( current_app_name );
				}
				catch ( HoudiniError error )
				{
					if ( current_app_name == "Houdini" )
					{
						myLastError = error.ToString();
						break;
					}
					else
					{
						current_app_name = "Houdini";
						continue;
					}
				}
				break;
			}
		}
#elif ( UNITY_EDITOR_OSX || UNITY_STANDALONE_LINUX )
		houdini_app_path = HoudiniVersion.HAPI_LIBRARY;
#else
		myLastError =
			"Could not find the Houdini installation because this is an unsupported platform.";
#endif


		return houdini_app_path;
	}

	public static void setPath()
	{
		if ( myAttemptedPathSetting )
			return;
		myAttemptedPathSetting = true;
		myIsPathSet = false;
#if HAPI_ENABLE_RUNTIME

#if UNITY_EDITOR_WIN
		string houdini_app_path = getHoudiniPath();
		string paths = System.Environment.GetEnvironmentVariable(
			"PATH", System.EnvironmentVariableTarget.Machine );
		if ( houdini_app_path != "" && !paths.Contains( houdini_app_path + "/bin" ) )
		{
			string houdini_bin_path = houdini_app_path + "/bin";
			if ( !paths.Contains( houdini_bin_path ) && houdini_bin_path != "" )
				if ( paths != "" )
					paths = houdini_bin_path + ";" + paths;
				else
					paths = houdini_bin_path;
			System.Environment.SetEnvironmentVariable(
				"PATH", paths, System.EnvironmentVariableTarget.Process );
		}

		bool found = false;
		foreach( string path in paths.Split( ';' ) )
		{
			if ( !System.IO.Directory.Exists( path ) )
				continue;

			if ( System.IO.File.Exists( path + "/" + HoudiniVersion.HAPI_LIBRARY + ".dll" ) )
			{
				prHoudiniPath = path;
				found = true;
				break;
			}
		}
		if ( !found )
		{
			if ( houdini_app_path != "" )
				myLastError =
					"Could not find Houdini Engine dll in the PATH or at: " +
					houdini_app_path;
			else
				myLastError =
					"Could not find Houdini Engine dll in the PATH.";
			return;
		}
		myIsPathSet = true;

#elif ( UNITY_EDITOR_OSX || UNITY_STANDALONE_LINUX )
		string houdini_app_path = getHoudiniPath();
		if ( !System.IO.File.Exists( houdini_app_path ) )
		{
			myLastError =
				"Could not find Houdini Engine dll at: " +
				houdini_app_path;
			return;
		}
		prHoudiniPath = houdini_app_path;

		// Set the path so that HARS executable is found.
		// TODO: Remove this when the HAPI_StartServer... functions get the path parameter.
		string paths = System.Environment.GetEnvironmentVariable(
			"PATH", System.EnvironmentVariableTarget.Machine );
		paths = HoudiniVersion.HAPI_SERVER.Replace( "/HARS", "" ) + ":" + paths;
		System.Environment.SetEnvironmentVariable(
			"PATH", paths, System.EnvironmentVariableTarget.Process );

		myIsPathSet = true;
#endif

#endif // HAPI_ENABLE_RUNTIME
	}

	public static bool prIsPathSet { get { return myIsPathSet; } private set {} }
	public static string prHoudiniPath { get; private set; }
	public static string prLastError { get { return myLastError; } private set {} }

	private static string getAppPath( string app_name )
	{
		string app_path = "";

#if UNITY_EDITOR_WIN
		// For Windows, we look at the registry entries made by the Houdini installer. We look for the
		// "active version" key which gives us the most recently installed Houdini version. Using the
		// active version we find the registry made by that particular installer and find the install
		// path.

		string correct_version =
			HoudiniVersion.HOUDINI_MAJOR + "." +
			HoudiniVersion.HOUDINI_MINOR + "." +
			HoudiniVersion.HOUDINI_BUILD;

		// Note the extra 0 for the "minor-minor" version that's needed here.
		string correct_version_key =
			HoudiniVersion.HOUDINI_MAJOR + "." +
			HoudiniVersion.HOUDINI_MINOR + "." +
			"0" + "." +
			HoudiniVersion.HOUDINI_BUILD;

		// HOUDINI_PATCH is a const variable, hence the 'unreachable code' warning.
		// However, it's a const variable in HoudiniVersion.cs which is a generated
		// file and could have the value be non-zero for certain builds, like
		// stub builds.
#pragma warning disable 162
		if ( HoudiniVersion.HOUDINI_PATCH != 0 )
			correct_version_key += "." + HoudiniVersion.HOUDINI_PATCH;
#pragma warning restore 162

		app_path = getRegKeyValue_x64( HKEY_LOCAL_MACHINE, "SOFTWARE\\Side Effects Software\\" + app_name, correct_version_key );

		if ( app_path == null || app_path.Length == 0 )
			throw new HoudiniError(
				"The correct version (" + correct_version + ") of " + app_name +
				" was not found in the registry!" );
		else if ( app_path.EndsWith( "\\" ) || app_path.EndsWith( "/" ) )
			app_path = app_path.Remove( app_path.Length - 1 );

		//Debug.Log( "Linked-To " + app_name + " Install Path: " + app_path );
#else
		// TODO: Add support for other platforms (only whichever platforms the Unity Editor supports).

		//#error "Your current platform is not yet fully supported. Binaries search path not set."
		//Debug.LogError( "Your current platform is not yet full support. Binaries search path not set." );

#endif // UNITY_EDITOR_WIN

		return app_path;
	}

	private static bool myIsPathSet = false;
	private static bool myAttemptedPathSetting = false;
	private static string myLastError = "";

}
