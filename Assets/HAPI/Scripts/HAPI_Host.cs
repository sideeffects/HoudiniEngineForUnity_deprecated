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
 */

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

namespace HAPI {
	
	/// <summary>
	/// 	Singleton Houdini host object that maintains the singleton Houdini scene and all access to the
	/// 	Houdini runtime.
	/// </summary>
	public class HAPI_Host : MonoBehaviour 
	{
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Public
		
		/// <summary>
		/// 	Ask if the singleton Houdini scene has already been created.
		/// </summary>
		/// <returns>
		/// 	Whether the singleton Houdini scene has been created yet or not.
		/// </returns>
		static public bool hasScene() 
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
		static public HAPI_AssetInfo loadOTL( string path ) 
		{
			if ( myHoudiniSceneExists ) 
			{
				Debug.Log( "Loading OTL: hip already exists" );
			} 
			else 
			{
				Debug.Log( "Loading OTL: new hip created" );
								
				HAPI_Initialize();
							
				myHoudiniSceneExists = true;
			}
			
			HAPI_AssetInfo asset_info 			= new HAPI_AssetInfo();
			asset_info.otlFilePath 				= path;
			asset_info.minVerticesPerPrimitive 	= 3;
			asset_info.maxVerticesPerPrimitive 	= 3;
			
			int result = HAPI_LoadOTLFile( ref asset_info );
			
			if ( result > 0 ) 
			{
				Debug.LogError( "OTL File Failed to Load" );
				return asset_info;
			}			
			
			Debug.Log( "Asset Loaded - Path: " + asset_info.assetInstancePath + ", ID: " + asset_info.id );			
			return asset_info;
		}
		
		/// <summary>
		/// 	Delete the asset Houdini instance in the scene and unload the OTL definition.
		/// </summary>
		/// <param name="asset_id">
		/// 	Asset_id as returned by <see cref="LoadOTLFile"/>.
		/// </param>
		/// <returns>
		/// 	<c>true</c> if successfully unload the asset, <c>false</c> otherwise.
		/// </returns>
		static public bool unloadOTL( int asset_id ) 
		{
			if ( asset_id < 0 )
				return false;
			
			int result = HAPI_UnloadOTLFile( asset_id );
			
			if ( result > 0 ) 
			{
				Debug.LogError( "OTL File Failed to Unload" );
				return false;
			}
			
			return true;	
		}
		
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Public HAPI API
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		public static extern int HAPI_GetParameters( 		int asset_id, 
															[Out] HAPI_ParmInfo[] parm_infos, 
															int start, int end );
				
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		public static extern int HAPI_SetParameters( 		int asset_id, 
															[Out] HAPI_ParmInfo[] parm_infos, 
															int start, int end );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		public static extern int HAPI_GetObjects( 			int asset_id, 
															[Out] HAPI_ObjectInfo[] object_infos, 
															int start, int end );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		public static extern int HAPI_GetObjectTransforms( 	int asset_id, 
															[Out] HAPI_Transform[] transforms, 
															int start, int end );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		public static extern int HAPI_GetGeometryInfo( 		int asset_id, int object_id, 
															out HAPI_GeometryInfo geo );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		public static extern int HAPI_GetVertexArray( 		int asset_id, int object_id, 
															[Out] HAPI_RawVertex[] vertices, 
															int start, int end );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		public static extern int HAPI_GetPrimitveArray( 	int asset_id, int object_id, 
															[Out] HAPI_RawPrimitive[] primitives, 
															int start, int end );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		public static extern int HAPI_GetInstanceArray( 	int asset_id, int object_id, 
															[Out] HAPI_RawInstance[] instances, 
															int start, int end );
		
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Private
		
		static private bool myHoudiniSceneExists = false;
		
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Private HAPI API
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_Initialize();
				
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_LoadOTLFile( 		ref HAPI_AssetInfo asset_info );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_UnloadOTLFile( 		int asset_id );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_PrintNetwork( 		StringBuilder buffer );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_Cleanup();	
	
	}

}