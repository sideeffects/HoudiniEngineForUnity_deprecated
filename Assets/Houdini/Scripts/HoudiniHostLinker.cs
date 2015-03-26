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
 * 		Contains HAPI_Host which is a singleton class that maintains the singleton Houdini scene and 
 * 		all access to the Houdini runtime.
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
using System.Text;
using System.IO;

public static partial class HoudiniHost
{
	public static string prLastInitializationError;

	// This is different from isRuntimeInitialized() because it does not rely
	// on the PATH variable being already set. While the setting of the PATH
	// variable is done in HoudiniSetPath, it does not seem to fix any
	// calls inside HoudiniHost. Only GameObjects loaded after HoudiniSetPath
	// did its thing get the new PATH and when THEY call through HoudiniHost
	// the HAPI calls properly link to the dll via the new PATH.
	public static bool isInstallationOk()
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		return prHoudiniSceneExists;
#else
		return false;
#endif // ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
	}

	public static bool isRuntimeInitialized()
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		if ( !HoudiniSetPath.prIsPathSet || !prHoudiniSceneExists )
			return false;
		else
		{
			try
			{
				processStatusCode( (HAPI_Result) HAPI_IsInitialized() );
				return true;
			}
			catch
			{
				return false;
			}
		}
#else
		return false;
#endif // ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
	}

	public static bool initialize()
	{
#if !( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		return false;
#else
		// During the batch creation of our .unitypackage file we don't want to actually
		// initialize  We use this environment variable to inhibit initialization.
		string no_init = System.Environment.GetEnvironmentVariable( "HAPI_UNITY_NO_INIT" );
		if ( no_init != null )
			return false;

		if ( !HoudiniSetPath.prIsPathSet )
		{
			prLastInitializationError = HoudiniSetPath.prLastError;
			return false;
		}

		if ( !prHoudiniSceneExists )
		{
			HAPI_Result status_code;
			try
			{
				string otls_path = getAllFoldersInPath( Application.dataPath + "/OTLs/Scanned" );
				string dsos_path = getAllFoldersInPath( Application.dataPath + "/DSOs" );

				// Check version match.
				int houdini_engine_major 	= getEnvInt( HAPI_EnvIntType.HAPI_ENVINT_VERSION_HOUDINI_ENGINE_MAJOR );
				int houdini_engine_minor 	= getEnvInt( HAPI_EnvIntType.HAPI_ENVINT_VERSION_HOUDINI_ENGINE_MINOR );
				int houdini_engine_api 		= getEnvInt( HAPI_EnvIntType.HAPI_ENVINT_VERSION_HOUDINI_ENGINE_API );

				/*
				Debug.Log(  "Running Houdini Engine Unity Plugin Version: " +
							HoudiniVersion.HOUDINI_ENGINE_MAJOR + "." + 
							HoudiniVersion.HOUDINI_ENGINE_MINOR +
							", API: " + HoudiniVersion.HOUDINI_ENGINE_API );
					
				Debug.Log(  "Linking Against Houdini Engine Version: " + 
							houdini_engine_major + "." + houdini_engine_minor +
							", API: " + houdini_engine_api );
					
				Debug.Log(  "Underlying Houdini Core Version: " + 
							houdini_major + "." + houdini_minor + "." + houdini_build
							+ ( houdini_patch > 0 ? "." + houdini_patch : "" ) );
				*/

				// Make sure we are linking against the expected Houdini Engine API version.
				// Note: We don't need to be so strict as to require the BUILD to match.
				if ( houdini_engine_major != HoudiniVersion.HOUDINI_ENGINE_MAJOR ||
						houdini_engine_minor != HoudiniVersion.HOUDINI_ENGINE_MINOR ||
						houdini_engine_api != HoudiniVersion.HOUDINI_ENGINE_API )
				{
					prLastInitializationError =
						"Houdini Engine version mis-match. Expected " +
						HoudiniVersion.HOUDINI_ENGINE_MAJOR + "." +
						HoudiniVersion.HOUDINI_ENGINE_MINOR + "." +
						HoudiniVersion.HOUDINI_ENGINE_API + ". Got " +
						houdini_engine_major + "." +
						houdini_engine_minor + "." +
						houdini_engine_api + ".";
					return false;
				}

				HAPI_CookOptions cook_options = new HAPI_CookOptions();
				cook_options.splitGeosByGroup = prSplitGeosByGroup;
				cook_options.maxVerticesPerPrimitive = HoudiniConstants.HAPI_MAX_VERTICES_PER_FACE;
				cook_options.refineCurveToLinear = HoudiniConstants.HAPI_CURVE_REFINE_TO_LINEAR;
				cook_options.curveRefineLOD = HoudiniConstants.HAPI_CURVE_LOD;
				cook_options.cookTemplatedGeos = HoudiniHost.prImportTemplatedGeos;

				// Initialize.
				status_code = HAPI_Initialize( otls_path, dsos_path, ref cook_options, true, -1 );
				if ( status_code != HAPI_Result.HAPI_RESULT_ALREADY_INITIALIZED )
					processStatusCode( status_code );
			}
			catch ( HoudiniError error )
			{
				prLastInitializationError = error.ToString();
				return false;
			}
			catch ( System.Exception error )
			{
				prLastInitializationError = error.ToString();
				return false;
			}

			prHoudiniSceneExists = true;
		}

		return true;
#endif // !( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
	}

	public static void displayHoudiniEngineInstallInfo()
	{
#if !UNITY_EDITOR
		#pragma warning disable 0219
#endif // !UNITY_EDITOR
		string installed_version_msg = "";
		string dialog_title = "";
		if ( isInstallationOk() )
		{
			int houdini_major 			= getEnvInt( HAPI_EnvIntType.HAPI_ENVINT_VERSION_HOUDINI_MAJOR );
			int houdini_minor 			= getEnvInt( HAPI_EnvIntType.HAPI_ENVINT_VERSION_HOUDINI_MINOR );
			int houdini_build 			= getEnvInt( HAPI_EnvIntType.HAPI_ENVINT_VERSION_HOUDINI_BUILD );
			int houdini_patch			= getEnvInt( HAPI_EnvIntType.HAPI_ENVINT_VERSION_HOUDINI_PATCH );
			int houdini_engine_major 	= getEnvInt( HAPI_EnvIntType.HAPI_ENVINT_VERSION_HOUDINI_ENGINE_MAJOR );
			int houdini_engine_minor 	= getEnvInt( HAPI_EnvIntType.HAPI_ENVINT_VERSION_HOUDINI_ENGINE_MINOR );
			int houdini_engine_api 		= getEnvInt( HAPI_EnvIntType.HAPI_ENVINT_VERSION_HOUDINI_ENGINE_API );
			installed_version_msg =
				"Installed Houdini Version: " +
				houdini_major + "." +
				houdini_minor + "." +
				houdini_build + 
				( houdini_patch > 0 ? "." + houdini_patch : "" ) + "\n" +
				"Installed Houdini Engine Version: " +
				houdini_engine_major + "." +
				houdini_engine_minor + "." +
				houdini_engine_api + "\n\n" +
				"Houdini Binaries Path: " + HoudiniSetPath.prHoudiniPath;
			dialog_title = "Houdini Engine Installation Info";
		}
		else
		{
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR )
			installed_version_msg =
				"Reason for Installation Detection Failure: " + prLastInitializationError;
#else
			installed_version_msg =
				"Reason for Installation Detection Failure: Unsupported Platform\n" +
				HoudiniConstants.HAPI_UNSUPPORTED_PLATFORM_MSG;
#endif // UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR )
			dialog_title = "No Houdini Engine Installed";
		}

		string license_info = "License Type Acquired: ";
		HAPI_License houdini_license_type = (HAPI_License) getEnvInt( HAPI_EnvIntType.HAPI_ENVINT_LICENSE );
		switch ( houdini_license_type )
		{
			case HAPI_License.HAPI_LICENSE_NONE: license_info += "None"; break;
			case HAPI_License.HAPI_LICENSE_HOUDINI_ENGINE: license_info += "Houdini Engine"; break;
			case HAPI_License.HAPI_LICENSE_HOUDINI: license_info += "Houdini (Escape)"; break;
			case HAPI_License.HAPI_LICENSE_HOUDINI_FX: license_info += "Houdini FX"; break;
			case HAPI_License.HAPI_LICENSE_HOUDINI_ENGINE_INDIE: license_info += "Houdini Engine Indie"; break;
			case HAPI_License.HAPI_LICENSE_HOUDINI_INDIE: license_info += "Houdini Indie"; break;
			default: license_info += "Unknown"; break;
		}

		string full_message = 
			"Required Houdini Version: " +
			HoudiniVersion.HOUDINI_MAJOR + "." +
			HoudiniVersion.HOUDINI_MINOR + "." +
			HoudiniVersion.HOUDINI_BUILD + "\n" +
			"Required Houdini Engine Version: " +
			HoudiniVersion.HOUDINI_ENGINE_MAJOR + "." +
			HoudiniVersion.HOUDINI_ENGINE_MINOR + "." +
			HoudiniVersion.HOUDINI_ENGINE_API + "\n\n" +
			installed_version_msg +
			"\n\n" +
			license_info +
#if UNITY_EDITOR && ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
			"\n\n" +
			"PATH Variable: \n" +
			System.Environment.GetEnvironmentVariable( "PATH", System.EnvironmentVariableTarget.Process ) +
#endif // UNITY_EDITOR && ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
			"";

		Debug.Log( full_message );
#if UNITY_EDITOR
		EditorUtility.DisplayDialog( dialog_title, full_message, "Ok" );
#endif // UNITY_EDITOR

#if !UNITY_EDITOR
		#pragma warning restore 0219
#endif // !UNITY_EDITOR
	}

	public static string getMissingEngineInstallHelpString()
	{
		return
			"You are missing the correct Houdini (" +
			HoudiniVersion.HOUDINI_MAJOR + "." +
			HoudiniVersion.HOUDINI_MINOR + "." +
			HoudiniVersion.HOUDINI_BUILD +
			") with Houdini Engine (" +
			HoudiniVersion.HOUDINI_ENGINE_MAJOR + "." +
			HoudiniVersion.HOUDINI_ENGINE_MINOR + "." +
			HoudiniVersion.HOUDINI_ENGINE_API + ") installation.";
	}

	private static string getAllFoldersInPath( string path )
	{
		string paths = "";
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		if ( !Directory.Exists( path ) )
			return "";

		DirectoryInfo di = new DirectoryInfo( path );
		foreach ( DirectoryInfo child_directory in di.GetDirectories() )
			paths += ";" + getAllFoldersInPath( child_directory.FullName );
#endif // ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )

		return path + paths;
	}
}
