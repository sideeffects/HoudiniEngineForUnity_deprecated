using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
using Microsoft.Win32;
#endif // ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )

#if UNITY_EDITOR
[ InitializeOnLoad ]
#endif // UNITY_EDITOR
public class HoudiniSetPath
{
	static HoudiniSetPath()
	{
		setPath();
	}

	public static string getHoudiniPath()
	{
		string houdini_app_path = "";
#if UNITY_STANDALONE_WIN || ( UNITY_METRO && UNITY_EDITOR )
		string hapi_path = System.Environment.GetEnvironmentVariable(
			"HAPI_PATH", System.EnvironmentVariableTarget.Machine );
		if ( hapi_path == null || hapi_path.Length == 0 )
			hapi_path = System.Environment.GetEnvironmentVariable(
				"HAPI_PATH", System.EnvironmentVariableTarget.User );
		if ( hapi_path == null || hapi_path.Length == 0 )
			hapi_path = System.Environment.GetEnvironmentVariable(
				"HAPI_PATH", System.EnvironmentVariableTarget.Process );
			
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
#elif UNITY_STANDALONE_OSX
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

#if UNITY_STANDALONE_WIN || ( UNITY_METRO && UNITY_EDITOR )
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

			if ( System.IO.File.Exists( path + "/libHAPI.dll" ) )
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

#elif UNITY_STANDALONE_OSX
		string houdini_app_path = getHoudiniPath();
		if ( !System.IO.File.Exists( houdini_app_path ) )
		{
			myLastError =
				"Could not find Houdini Engine dll at: " +
				houdini_app_path;
			return;
		}
		prHoudiniPath = houdini_app_path;
		myIsPathSet = true;

#endif // UNITY_STANDALONE_WIN || ( UNITY_METRO && UNITY_EDITOR )
	}
		
	public static bool prIsPathSet { get { return myIsPathSet; } private set {} }
	public static string prHoudiniPath { get; private set; }
	public static string prLastError { get { return myLastError; } private set {} }

	private static string getAppPath( string app_name )
	{
		string app_path = "";
			
#if UNITY_EDITOR
		if ( BuildPipeline.isBuildingPlayer )
			return app_path;
#endif // UNITY_EDITOR

#if ( UNITY_STANDALONE_WIN || ( UNITY_METRO && UNITY_EDITOR ) )
		// For Windows, we look at the registry entries made by the Houdini installer. We look for the 
		// "active version" key which gives us the most recently installed Houdini version. Using the
		// active version we find the registry made by that particular installer and find the install
		// path.

		RegistryKey local_machine = Registry.LocalMachine;

		RegistryKey sesi_key = local_machine.OpenSubKey( "Software\\Side Effects Software\\" + app_name );
		if ( sesi_key == null )
			throw new HoudiniError( "No " + app_name + " installation found in the registry!" );

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

		string[] sesi_key_value_names = sesi_key.GetValueNames();
		string matched_correct_version_key = correct_version_key;
		foreach ( string value_name in sesi_key_value_names )
		{
			if ( value_name.StartsWith( correct_version_key ) )
			{
				matched_correct_version_key = value_name;
				break;
			}
		}

		app_path = (string) sesi_key.GetValue( matched_correct_version_key );
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

#endif // ( UNITY_STANDALONE_WIN || ( UNITY_METRO && UNITY_EDITOR ) )

		return app_path;
	}

	private static bool myIsPathSet = false;
	private static bool myAttemptedPathSetting = false;
	private static string myLastError = "";

}
