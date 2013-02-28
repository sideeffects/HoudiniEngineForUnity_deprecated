using UnityEngine;
using UnityEditor;

#if UNITY_STANDALONE_WIN
using Microsoft.Win32;
#endif // UNITY_STANDALONE_WIN

namespace HAPI 
{
	[ InitializeOnLoad ]
	public class HAPI_SetPath {
	
		static HAPI_SetPath()
		{
			setPath();
		}

		public static string getHoudiniPath()
		{
			string houdini_app_path = "";
			string hapi_path = System.Environment.GetEnvironmentVariable( "HAPI_PATH", 
																		  System.EnvironmentVariableTarget.Machine );
			if ( hapi_path == null || hapi_path.Length == 0 )
				hapi_path = System.Environment.GetEnvironmentVariable( "HAPI_PATH", 
																	   System.EnvironmentVariableTarget.User );
			if ( hapi_path == null || hapi_path.Length == 0 )
				hapi_path = System.Environment.GetEnvironmentVariable( "HAPI_PATH", 
																	   System.EnvironmentVariableTarget.Process );
			
			if ( hapi_path != null && hapi_path.Length > 0 )
			{
				Debug.Log( "Using Custom Houdini Path: " + hapi_path );
				houdini_app_path = hapi_path;
			}
			else
			{
				// HAPI_PATH variable not set therefore we must find another way to detect an installation of
				// Houdini. This step is platform dependant hence the #if's.
				
				string current_active_version_key_name = "ActiveEngineVersion";
				string current_app_name = "Houdini Engine";
				while ( true )
				{
					try
					{
						houdini_app_path = getAppPath( current_active_version_key_name, current_app_name );
					}
					catch ( HAPI_Error error )
					{
						if ( current_app_name == "Houdini" )
						{
							Debug.LogError( error.ToString() );
						}
						else
						{
							current_active_version_key_name = "ActiveVersion";
							current_app_name = "Houdini";
							continue;
						}
					}
					break;
				}
			}

			return houdini_app_path;
		}

		public static void setPath()
		{
			if ( prIsPathSet )
				return;
			
			string houdini_app_path = getHoudiniPath();
			string houdini_bin_path = houdini_app_path + "/bin";

			string path = System.Environment.GetEnvironmentVariable( "PATH", System.EnvironmentVariableTarget.Machine );
			
			if ( !path.Contains( houdini_bin_path ) )
				if ( path != "" )
					path = houdini_bin_path + ";" + path;
				else
					path = houdini_bin_path;
			
			System.Environment.SetEnvironmentVariable( "PATH", path, System.EnvironmentVariableTarget.Process );
			Debug.Log( "DLL search path set to: " + path );
			
			prHoudiniPath = houdini_app_path;
			myIsPathSet = true;
		}
		
		public static bool prIsPathSet { get { return myIsPathSet; } private set {} }
		public static string prHoudiniPath { get; private set; }

		private static string getAppPath( string active_version_key_name, string app_name )
		{
			string install_path = "";

#if UNITY_STANDALONE_WIN
			// For Windows, we look at the registry entries made by the Houdini installer. We look for the 
			// "active version" key which gives us the most recently installed Houdini version. Using the
			// active version we find the registry made by that particular installer and find the install
			// path.

			RegistryKey local_machine = Registry.LocalMachine;

			RegistryKey sesi_key = local_machine.OpenSubKey( "Software\\Side Effects Software" );
			if ( sesi_key == null )
				throw new HAPI_Error( "No 32-bit Houdini installation found!" );

			string active_version = (string) sesi_key.GetValue( active_version_key_name );
			if ( active_version == null )
				throw new HAPI_Error( "No 32-bit Houdini active version registry found!" );

			RegistryKey active_houdini_key = sesi_key.OpenSubKey( app_name + " " + active_version );
			if ( active_houdini_key == null )
				throw new HAPI_Error( "Specified active 32-bit Houdini version is not installed!" );

			install_path = (string) active_houdini_key.GetValue( "InstallPath" );
			if ( install_path == null || install_path.Length == 0 )
				throw new HAPI_Error( "Specified active 32-bit Houdini install path not valid!" );

			Debug.Log( "Active " + app_name + " Version: " + active_version );
			Debug.Log( "Active " + app_name + " Install Path: " + install_path );
#else
			// TODO: Add support for other platforms (only whichever platforms the Unity Editor supports).
				
			#error "Your current platform is not yet fully supported. Binaries search path not set."
			Debug.LogError( "Your current platform is not yet full support. Binaries search path not set." );
			return;

#endif // UNITY_STANDALONE_WIN

			return install_path;
		}

		private static bool myIsPathSet = false;
	}
	
} // namespace HAPI
