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
using UnityEditor;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

namespace HAPI 
{
	public class HAPI_Error : System.Exception 
	{
		public HAPI_Error() 
		{
			myErrorMessage = "Unknown Error";
		}
		public HAPI_Error( string msg ) 
		{
			myErrorMessage = msg;
		}
		
		public void addMessagePrefix( string prefix )
		{
			myErrorMessage = prefix + ": " + myErrorMessage;
		}
		
		public void addMessageDetail( string detail )
		{
			myErrorMessage = myErrorMessage + "\n" + detail;	
		}
		
		public override string ToString()
		{
			return myErrorMessage;
		}
		
		private string myErrorMessage;
	}
	
	public class HAPI_ErrorProgressCancelled : HAPI_Error 
	{
		public override string ToString()
		{
			return "Load Cancelled by User";
		}
	}
	
	/// <summary>
	/// 	Singleton Houdini host object that maintains the singleton Houdini scene and all access to the
	/// 	Houdini runtime.
	/// </summary>
	[ InitializeOnLoad ]
	public static partial class HAPI_Host
	{
		static HAPI_Host()
		{
			EditorApplication.update += update;
			EditorApplication.playmodeStateChanged += playmodeStateChanged;

			if ( !isRuntimeInitialized() )
			{	
				prHoudiniSceneExists = false;
				prMidPlaymodeStateChange = false;

				initialize();
			}

			// Preferences
			setBool( "HAPI_EnableDragAndDrop", true, true );
			setBool( "HAPI_HideGeometryOnLinking", true, true );
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Public

		public static bool prHoudiniSceneExists {
												get { return getBool( "HAPI_HoudiniSceneExists" ); } 
												private set { setBool( "HAPI_HoudiniSceneExists", value ); } }
		public static bool prMidPlaymodeStateChange {	
												get { return getBool( "HAPI_MidPlaymodeStateChange" ); } 
												private set { setBool( "HAPI_MidPlaymodeStateChange", value ); } }

		public static bool prEnableDragAndDrop {
												get { return getBool( "HAPI_EnableDragAndDrop" ); } 
												set { setBool( "HAPI_EnableDragAndDrop", value ); } }
		public static bool prHideGeometryOnLinking {
												get { return getBool( "HAPI_HideGeometryOnLinking" ); } 
												set { setBool( "HAPI_HideGeometryOnLinking", value ); } }

		public static bool hasScene() 
		{
			return prHoudiniSceneExists;
		}
		
		public static void saveScene( string file_name )
		{
			HAPI_SaveHIPFile( file_name );
		}
		
		public static int loadOTL( string path ) 
		{
			initialize();

			string textures_path = Application.dataPath + "/Textures";
			
			int asset_id = -1;
			HAPI_Result status_code = (HAPI_Result) HAPI_LoadOTLFile( path, textures_path,
																	  HAPI_Constants.HAPI_MIN_VERTICES_PER_FACE,
																	  HAPI_Constants.HAPI_MAX_VERTICES_PER_FACE,
																	  ref asset_id );
			processStatusCode( status_code );

			return asset_id;
		}
		
		public static void loadHip( string path ) 
		{
			initialize();

			string textures_path = Application.dataPath + "/Textures";
																
			HAPI_Result status_code = (HAPI_Result) HAPI_LoadHIPFile( path, textures_path, 
																	  HAPI_Constants.HAPI_MIN_VERTICES_PER_FACE,
																	  HAPI_Constants.HAPI_MAX_VERTICES_PER_FACE );
			
			processStatusCode( status_code );						
						
		}
		
		public static int getAssetCountFromLoadHip() 
		{
			int num_assets = 0;
			
			HAPI_Result status_code = (HAPI_Result) HAPI_GetAssetCountFromLoadHIPFile( ref num_assets );
			
			processStatusCode( status_code );
			
			return num_assets;
						
		}
		
		public static void getAssetIdsFromLoadHIPFile( int [] assetIds )
		{
			HAPI_Result status_code = (HAPI_Result) HAPI_GetAssetIdsFromLoadHIPFile( assetIds );
			
			processStatusCode( status_code );
			
		}
		
		public static int createCurve()
		{
			initialize();

			int asset_id = -1;
			HAPI_Result status_code = (HAPI_Result) HAPI_CreateCurve( ref asset_id );
			processStatusCode( status_code );

			return asset_id;
		}
		
		public static bool unloadOTL( int asset_id ) 
		{
			if ( asset_id < 0 )
				return false;
			
			int result = HAPI_UnloadOTLFile( asset_id );
			
			try
			{
				processStatusCode( (HAPI_Result) result );
			}
			catch ( HAPI_Error error )
			{
				Debug.LogError( "Asset failed to unload: " + error.ToString() );
			}
			
			return true;
		}

		public static bool isRealDestroy()
		{
			return !EditorApplication.isPlayingOrWillChangePlaymode && !prMidPlaymodeStateChange;
		}

		public static void initialize()
		{
			string otls_path = Application.dataPath + "/OTLs/Scanned";

			if ( !prHoudiniSceneExists )
			{
				HAPI_Result status_code;
				status_code = (HAPI_Result) HAPI_Initialize( HAPI_SetPath.prHoudiniPath, otls_path,
															 true, -1 );

				if ( status_code != HAPI_Result.HAPI_RESULT_ALREADY_INITIALIZED )
					processStatusCode( status_code );

				prHoudiniSceneExists = true;
			}
		}
		
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Private

		private static void update()
		{

		}

		private static void playmodeStateChanged()
		{
			prMidPlaymodeStateChange = !prMidPlaymodeStateChange;
		}

		private static bool isRuntimeInitialized()
		{
			if ( !HAPI_SetPath.prIsPathSet )
				return false;
			else
			{
				try
				{
					getStatus( HAPI_StatusType.HAPI_STATUS_STATE );
					return true;
				}
				catch
				{
					return false;
				}
			}
		}

		private static bool hasCallFailed( HAPI_Result code )
		{
			return ( (int) code > 0 );
		}
		
		private static void processStatusCode( HAPI_Result code )
		{
			if ( hasCallFailed( code ) )
			{
				int buffer_size = 4000;
				HAPI_GetStatusStringBufLength( (int) HAPI_StatusType.HAPI_STATUS_RESULT, out buffer_size );
				StringBuilder error_str = new StringBuilder( buffer_size );
				HAPI_GetStatusString( (int) HAPI_StatusType.HAPI_STATUS_RESULT, error_str );
				throw new HAPI_Error( error_str.ToString() );
			}
		}

		private static int getInt( string name )
		{
			return EditorPrefs.GetInt( name );
		}

		private static void setInt( string name, int value )
		{
			setInt( name, value, false );
		}
		private static void setInt( string name, int value, bool only_if_new )
		{
			if ( !only_if_new || !EditorPrefs.HasKey( name ) )
				EditorPrefs.SetInt( name, value );
		}

		private static bool getBool( string name )
		{
			return EditorPrefs.GetInt( name ) == 0 ? false : true;
		}

		private static void setBool( string name, bool value )
		{
			setBool( name, value, false );
		}
		private static void setBool( string name, bool value, bool only_if_new )
		{
			if ( !only_if_new || !EditorPrefs.HasKey( name ) )
				EditorPrefs.SetInt( name, value ? 1 : 0 );
		}

		private static string getString( string name )
		{
			return EditorPrefs.GetString( name );
		}

		private static void setString( string name, string value )
		{
			setString( name, value, false );
		}
		private static void setString( string name, string value, bool only_if_new )
		{
			if ( !only_if_new || !EditorPrefs.HasKey( name ) )
				EditorPrefs.SetString( name, value );
		}
	}

} // namespace HAPI
