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
		
		public virtual string what()
		{
			return myErrorMessage;	
		}
		
		private string myErrorMessage;
	}
	
	public class HAPI_ErrorProgressCancelled : HAPI_Error 
	{
		public override string what()
		{
			return "Load Cancelled by User";	
		}
	}
	
	/// <summary>
	/// 	Singleton Houdini host object that maintains the singleton Houdini scene and all access to the
	/// 	Houdini runtime.
	/// </summary>
	public partial class HAPI_Host
	{
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Public
		
		/// <summary>
		/// 	Ask if the singleton Houdini scene has already been created.
		/// </summary>
		/// <returns>
		/// 	Whether the singleton Houdini scene has been created yet or not.
		/// </returns>
		public static bool hasScene() 
		{
			return myHoudiniSceneExists;
		}
		
		/// <summary>
		/// 	Load and instantiate an asset from a .otl file.
		/// </summary>
		/// <param name="path">
		/// 	Absolute path to the .otl file.
		/// </param>
		/// <returns>
		/// 	An initialized <see cref="HAPI_AssetInfo"/>.
		/// </returns>
		public static HAPI_AssetInfo loadOTL( string path ) 
		{
			HAPI_StatusCode status_code = 0;			
			
			if ( myHoudiniSceneExists )
				Debug.Log( "Loading OTL: Using Existing Scene" );
			else 
			{
				Debug.Log( "Loading OTL: Creating New Scene" );
								
				status_code = (HAPI_StatusCode) HAPI_Initialize();
				
				if ( status_code != HAPI_StatusCode.HAPI_STATUS_ALREADY_INITIALIZED )
					processStatusCode( status_code );
				
				myHoudiniSceneExists = true;
			}
			
			HAPI_AssetInfo asset_info 			= new HAPI_AssetInfo();
			asset_info.filePath 				= path;
			asset_info.minVerticesPerPrimitive 	= HAPI_Constants.HAPI_MIN_VERTICES_PER_FACE;
			asset_info.maxVerticesPerPrimitive 	= HAPI_Constants.HAPI_MAX_VERTICES_PER_FACE;
			
			status_code = (HAPI_StatusCode) HAPI_LoadOTLFile( ref asset_info );
			
			processStatusCode( status_code );
			
			Debug.Log( "Asset Loaded - Path: " + asset_info.instancePath + ", ID: " + asset_info.id );			
			return asset_info;
		}
		
		/// <summary>
		/// 	Delete the asset Houdini instance in the scene and unload the OTL definition.
		/// </summary>
		/// <param name="asset_id">
		/// 	Asset_id as returned by <see cref="HAPI_Host.LoadOTLFile"/>.
		/// </param>
		/// <returns>
		/// 	<c>true</c> if successfully unload the asset, <c>false</c> otherwise.
		/// </returns>
		public static bool unloadOTL( int asset_id ) 
		{
			if ( asset_id < 0 )
				return false;
			
			int result = HAPI_UnloadOTLFile( asset_id );
			
			try
			{
				processStatusCode( (HAPI_StatusCode) result );
			}
			catch ( HAPI_Error error ) 
			{				
				Debug.LogError( "Asset failed to unload: " + error.what() );
			}
			
			return true;	
		}
		
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Private
		
		private static bool hasCallFailed( HAPI_StatusCode code )
		{
			return ( (int) code > 0 );	
		}
		
		private static void processStatusCode( HAPI_StatusCode code )
		{
			if ( hasCallFailed( code ) )
			{
				StringBuilder error_str = new StringBuilder( 1024 );
				HAPI_GetLastErrorString( error_str );
				throw new HAPI_Error( error_str.ToString() );
			}
		}
		
		private static bool myHoudiniSceneExists = false;		
	}

}