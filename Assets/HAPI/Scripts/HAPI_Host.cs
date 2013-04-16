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
	public delegate void RepaintDelegate();

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
		
		protected string myErrorMessage;
	}
	
	public class HAPI_ErrorProgressCancelled : HAPI_Error 
	{
		public override string ToString()
		{
			return "Load Cancelled by User";
		}
	}

	public class HAPI_ErrorIgnorable : HAPI_Error
	{
		public HAPI_ErrorIgnorable() 
		{
			myErrorMessage = "Ignorable Error";
		}
		public HAPI_ErrorIgnorable( string msg ) 
		{
			myErrorMessage = msg;
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
			EditorApplication.update				+= update;
			EditorApplication.playmodeStateChanged	+= playmodeStateChanged;

			if ( !isRuntimeInitialized() )
			{
				prHoudiniSceneExists		= false;
				prMidPlaymodeStateChange	= false;

				initialize();
			}

			// Preferences
			
			setString(	"HAPI_CollisionGroupName", "collision_geo", true );
			setString(	"HAPI_RenderedCollisionGroupName", "rendered_collision_geo", true );

			setInt(		"HAPI_CurvePrimitiveTypeDefault", 1, true );
			setInt(		"HAPI_CurveMethodDefault", 1, true );

			setBool(	"HAPI_EnableDragAndDrop", true, true );
			setBool(	"HAPI_EnableSupportWarnings", false, true );
			
			setBool(	"HAPI_EnableCooking", true, true );
			setBool(	"HAPI_AutoSelectParent", true, true );
			setBool(	"HAPI_HideGeometryOnLinking", true, true );
			setBool(	"HAPI_AutoPinInstances", true, true );

			myRepaintDelegate = null;
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Public

		public static bool prHoudiniSceneExists {
												get { return getBool( "HAPI_HoudiniSceneExists" ); } 
												private set { setBool( "HAPI_HoudiniSceneExists", value ); } }
		public static bool prMidPlaymodeStateChange {	
												get { return getBool( "HAPI_MidPlaymodeStateChange" ); } 
												private set { setBool( "HAPI_MidPlaymodeStateChange", value ); } }

		public static string prCollisionGroupName {
												get { return getString( "HAPI_CollisionGroupName" ); }
												set { setString( "HAPI_CollisionGroupName", value ); } }
		public static string prRenderedCollisionGroupName {
												get { return getString( "HAPI_RenderedCollisionGroupName" ); }
												set { setString( "HAPI_RenderedCollisionGroupName", value ); } }

		public static int prCurvePrimitiveTypeDefault {
												get { return getInt( "HAPI_CurvePrimitiveTypeDefault" ); }
												set { setInt( "HAPI_CurvePrimitiveTypeDefault", value ); } }
		public static int prCurveMethodDefault {
												get { return getInt( "HAPI_CurveMethodDefault" ); }
												set { setInt( "HAPI_CurveMethodDefault", value ); } }

		public static bool prEnableDragAndDrop {
												get { return getBool( "HAPI_EnableDragAndDrop" ); } 
												set { setBool( "HAPI_EnableDragAndDrop", value ); } }
		public static bool prEnableSupportWarnings {
												get { return getBool( "HAPI_EnableSupportWarnings" ); } 
												set { setBool( "HAPI_EnableSupportWarnings", value ); } }

		public static bool prEnableCooking {
												get { return getBool( "HAPI_EnableCooking" ); }
												set { setBool( "HAPI_EnableCooking", value ); } }
		public static bool prAutoSelectParent {
												get { return getBool( "HAPI_AutoSelectParent" ); } 
												set { setBool( "HAPI_AutoSelectParent", value ); } }
		public static bool prHideGeometryOnLinking {
												get { return getBool( "HAPI_HideGeometryOnLinking" ); } 
												set { setBool( "HAPI_HideGeometryOnLinking", value ); } }
		
		public static bool prAutoPinInstances {
												get { return getBool( "HAPI_AutopinInstances" ); } 
												set { setBool( "HAPI_AutopinInstances", value ); } }

		public static RepaintDelegate			myRepaintDelegate;

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
			if ( !initialize() )
				throw new HAPI_Error( "DLL Not Found." );

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
			if ( !initialize() )
				throw new HAPI_Error( "DLL Not Found." );

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
			if ( !initialize() )
				throw new HAPI_Error( "DLL Not Found." );

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

		public static bool initialize()
		{
			string otls_path = Application.dataPath + "/OTLs/Scanned";
			string dsos_path = "@/dso_^;@/dso;" + Application.dataPath + "/DSOs";

			if ( !prHoudiniSceneExists )
			{
				HAPI_Result status_code;
				try
				{
					status_code = (HAPI_Result) HAPI_Initialize( HAPI_SetPath.prHoudiniPath, otls_path,
																 dsos_path, true, -1 );
					if ( status_code != HAPI_Result.HAPI_RESULT_ALREADY_INITIALIZED )
						processStatusCode( status_code );
				}
				catch ( HAPI_Error error )
				{
					Debug.LogError( error.ToString() );
					return false;
				}
				catch ( System.Exception error )
				{
					Debug.LogError( error.ToString() );
					return false;
				}

				prHoudiniSceneExists = true;
			}

			return true;
		}

		public static void throwRuntimeError()
		{
			int buffer_size = 4000;
			HAPI_GetStatusStringBufLength( (int) HAPI_StatusType.HAPI_STATUS_RESULT, out buffer_size );
			StringBuilder error_str = new StringBuilder( buffer_size );
			HAPI_GetStatusString( (int) HAPI_StatusType.HAPI_STATUS_RESULT, error_str );
			throw new HAPI_Error( error_str.ToString() );
		}

		public static bool isRuntimeInitialized()
		{
			if ( !HAPI_SetPath.prIsPathSet )
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
		}
		
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Private

		private static void update()
		{

		}

		private static void playmodeStateChanged()
		{
			prMidPlaymodeStateChange = !prMidPlaymodeStateChange;
			setTime( 0.0f );
		}

		private static bool hasCallFailed( HAPI_Result code )
		{
			return ( (int) code > 0 );
		}
		
		private static void processStatusCode( HAPI_Result code )
		{
			if ( hasCallFailed( code ) )
				throwRuntimeError();
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
