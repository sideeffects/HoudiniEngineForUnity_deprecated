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

int blah = 0;
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
	public static partial class HAPI_Host
	{
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Public
		
		public static bool hasScene() 
		{
			return myHoudiniSceneExists;
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

		public static void initialize()
		{
			string otls_path = Application.dataPath + "/OTLs/Scanned";

			if ( !myHoudiniSceneExists )
			{
				Debug.Log( "Loading OTL: Creating New Scene" );

				HAPI_Result status_code;
				status_code = (HAPI_Result) HAPI_Initialize( HAPI.HAPI_SetPath.prHoudiniPath, otls_path,
															 true, -1 );

				if ( status_code != HAPI_Result.HAPI_RESULT_ALREADY_INITIALIZED )
					processStatusCode( status_code );

				myHoudiniSceneExists = true;
			}
		}

		// There is no way to save static variables by themselves between Unity modes
		// (so serialization/deserialization steps). Therefore, we need to save "state"
		// inside a component which will then restore the state after it was 
		// deserialized. It doesn't matter which HAPI component does this.
		public static void forceInitialize()
		{
			myHoudiniSceneExists = true;
		}
		
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Private
		
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
		
		private static bool myHoudiniSceneExists = false;
	}

} // namespace HAPI
