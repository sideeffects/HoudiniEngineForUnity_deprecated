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
		/// 	Asset_id as returned by <see cref="HAPI_Host.LoadOTLFile"/>.
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
		
		/// <summary>
		/// 	Fill an array of HAPI_ParmInfo structs with parameter information from the asset instance node.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.HAPI_LoadOTLFile"/>.
		/// </param>
		/// <param name="parm_infos">
		/// 	Array of <see cref="HAPI_ParmInfo"/> at least the size of 
		/// 	<paramref name="end"/> - <paramref name="start"/>.
		/// </param>
		/// <param name="start">
		/// 	First parameter index to begin with. Must be at least <see cref="HAPI_AssetInfo.parmCount"/>
		/// 	and at most <paramref name="end"/>.
		/// </param>
		/// <param name="end">
		/// 	Last parameter index to use. Must be at least <see cref="HAPI_AssetInfo.parmCount"/> and 
		/// 	more than or equal to <paramref name="start"/>.
		/// </param>
		/// <returns>
		/// 	A HAPI_StatusCode.
		/// </returns>
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		public static extern int HAPI_GetParameters( 		int asset_id, 
															[Out] HAPI_ParmInfo[] parm_infos, 
															int start, int end );
		
		/// <summary>
		/// 	Set a subset of parameter values using the given array of <see cref="HAPI_ParmInfo"/>.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.HAPI_LoadOTLFile"/>.
		/// </param>
		/// <param name="parm_infos">
		/// 	Array of <see cref="HAPI_ParmInfo"/> at least the size of 
		/// 	<paramref name="end"/> - <paramref name="start"/> containing the new parameter values.
		/// </param>
		/// <param name="start">
		/// 	First parameter index to begin with. Must be at least <see cref="HAPI_AssetInfo.parmCount"/>
		/// 	and at most <paramref name="end"/>.
		/// </param>
		/// <param name="end">
		/// 	Last parameter index to use. Must be at least <see cref="HAPI_AssetInfo.parmCount"/> and 
		/// 	more than or equal to <paramref name="start"/>.
		/// </param>
		/// <returns>
		/// 	A HAPI_StatusCode.
		/// </returns>
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		public static extern int HAPI_SetParameters( 		int asset_id, 
															[Out] HAPI_ParmInfo[] parm_infos, 
															int start, int end );
		
		/// <summary>
		/// 	Fill an array of <see cref="HAPI_ObjectInfo"/> structs with information on each visible object 
		/// 	in the scene that has a SOP network (is not a sub-network).
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.HAPI_LoadOTLFile"/>.
		/// </param>
		/// <param name="object_infos">
		/// 	Array of <see cref="HAPI_ObjectInfo"/> at least the size of 
		/// 	<paramref name="end"/> - <paramref name="start"/>.
		/// </param>
		/// <param name="start">
		/// 	First object index to begin with. Must be at least <see cref="HAPI_AssetInfo.objectCount"/>
		/// 	and at most <paramref name="end"/>.
		/// </param>
		/// <param name="end">
		/// 	Last object index to use. Must be at least <see cref="HAPI_AssetInfo.objectCount"/> and 
		/// 	more than or equal to <paramref name="start"/>.
		/// </param>
		/// <returns>
		/// 	A HAPI_StatusCode.
		/// </returns>
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		public static extern int HAPI_GetObjects( 			int asset_id, 
															[Out] HAPI_ObjectInfo[] object_infos, 
															int start, int end );
		
		/// <summary>
		/// 	Fill an array of <see cref="HAPI_ObjectInfo"/> structs with information on each visible object 
		/// 	in the scene that has a SOP network (is not a sub-network).
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.HAPI_LoadOTLFile"/>.
		/// </param>
		/// <param name="transforms">
		/// 	Array of <see cref="HAPI_Transform"/> at least the size of 
		/// 	<paramref name="end"/> - <paramref name="start"/>. The <see cref="HAPI_Transform.id"/> of each will be 
    	/// 	set to the object id as given by <see cref="HAPI_Host.HAPI_GetObjects"/>.
		/// </param>
		/// <param name="start">
		/// 	First object index to begin with. Must be at least <see cref="HAPI_AssetInfo.objectCount"/>
		/// 	and at most <paramref name="end"/>.
		/// </param>
		/// <param name="end">
		/// 	Last object index to use. Must be at least <see cref="HAPI_AssetInfo.objectCount"/> and 
		/// 	more than or equal to <paramref name="start"/>.
		/// </param>
		/// <returns>
		/// 	A HAPI_StatusCode.
		/// </returns>
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		public static extern int HAPI_GetObjectTransforms( 	int asset_id, 
															[Out] HAPI_Transform[] transforms, 
															int start, int end );
		
		/// <summary>
		/// 	Fill a <see cref="HAPI_GeometryInfo"/> with geometry information of an object.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.HAPI_LoadOTLFile"/>.
		/// </param>
		/// <param name="object_id">
		/// 	The object id returned by <see cref="HAPI_Host.HAPI_GetObjects"/>.
		/// </param>
		/// <param name="geometry_info">
		/// 	A <see cref="HAPI_GeometryInfo"/> to be filled with geometry information of the specified 
		/// 	object and asset instance.
		/// </param>
		/// <returns>
		/// 	A HAPI_StatusCode.
		/// </returns>
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		public static extern int HAPI_GetGeometryInfo( 		int asset_id, int object_id, 
															out HAPI_GeometryInfo geometry_info );
		
		/// <summary>
		/// 	Fill an array of <see cref="HAPI_RawVertex"/> structs with vertex information from 
		/// 	an object's geometry.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.HAPI_LoadOTLFile"/>.
		/// </param>
		/// <param name="object_id">
		/// 	The object id returned by <see cref="HAPI_Host.HAPI_GetObjects"/>.
		/// </param>
		/// <param name="vertices">
		/// 	Array of <see cref="HAPI_RawVertex"/> at least the size of 
		/// 	<paramref name="end"/> - <paramref name="start"/>.
		/// </param>
		/// <param name="start">
		/// 	First vertex index to begin with. Must be at least <see cref="HAPI_GeometryInfo.vertexCount"/>
		/// 	and at most <paramref name="end"/>.
		/// </param>
		/// <param name="end">
		/// 	Last vertex index to use. Must be at least <see cref="HAPI_GeometryInfo.vertexCount"/> and 
		/// 	more than or equal to <paramref name="start"/>.
		/// </param>
		/// <returns>
		/// 	A HAPI_StatusCode.
		/// </returns>
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		public static extern int HAPI_GetVertexArray( 		int asset_id, int object_id, 
															[Out] HAPI_RawVertex[] vertices, 
															int start, int end );
		
		/// <summary>
		/// 	Fill an array of <see cref="HAPI_RawPrimitive"/> structs with primitive information from 
		/// 	an object's geometry.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.HAPI_LoadOTLFile"/>.
		/// </param>
		/// <param name="object_id">
		/// 	The object id returned by <see cref="HAPI_Host.HAPI_GetObjects"/>.
		/// </param>
		/// <param name="primitives">
		/// 	Array of <see cref="HAPI_RawPrimitive"/> at least the size of 
		/// 	<paramref name="end"/> - <paramref name="start"/>.
		/// </param>
		/// <param name="start">
		/// 	First primitive index to begin with. Must be at least <see cref="HAPI_GeometryInfo.primCount"/>
		/// 	and at most <paramref name="end"/>.
		/// </param>
		/// <param name="end">
		/// 	Last primitive index to use. Must be at least <see cref="HAPI_GeometryInfo.primCount"/> and 
		/// 	more than or equal to <paramref name="start"/>.
		/// </param>
		/// <returns>
		/// 	A HAPI_StatusCode.
		/// </returns>
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		public static extern int HAPI_GetPrimitveArray( 	int asset_id, int object_id, 
															[Out] HAPI_RawPrimitive[] primitives, 
															int start, int end );
		
		/// <summary>
		/// 	Fill an array of <see cref="HAPI_RawInstance"/> structs with instance information from 
		/// 	an object's geometry.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.HAPI_LoadOTLFile"/>.
		/// </param>
		/// <param name="object_id">
		/// 	The object id returned by <see cref="HAPI_Host.HAPI_GetObjects"/>.
		/// </param>
		/// <param name="instances">
		/// 	Array of <see cref="HAPI_RawInstance"/> at least the size of 
		/// 	<paramref name="end"/> - <paramref name="start"/>.
		/// </param>
		/// <param name="start">
		/// 	First instance index to begin with. Must be at least <see cref="HAPI_GeometryInfo.instanceCount"/>
		/// 	and at most <paramref name="end"/>.
		/// </param>
		/// <param name="end">
		/// 	Last instance index to use. Must be at least <see cref="HAPI_GeometryInfo.instanceCount"/> and 
		/// 	more than or equal to <paramref name="start"/>.
		/// </param>
		/// <returns>
		/// 	A HAPI_StatusCode.
		/// </returns>
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		public static extern int HAPI_GetInstanceArray( 	int asset_id, int object_id, 
															[Out] HAPI_RawInstance[] instances, 
															int start, int end );
		
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Private
		
		static private bool myHoudiniSceneExists = false;
		
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Private HAPI API
		
		/// <summary>
		/// 	Create the asset manager, set up environment variables, and initialize the main Houdini scene.
		/// </summary>
		/// <returns>
		/// 	A HAPI_StatusCode.
		/// </returns>
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_Initialize();
		
		/// <summary>
		/// 	This does nothing for now. Placeholder.
		/// </summary>
		/// <returns>
		/// 	A HAPI_StatusCode.
		/// </returns>
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_Cleanup();
		
		/// <summary>
		/// 	Debug helper function that prints the contents of the main Houdini scene.
		/// </summary>
		/// <param name="buffer">
		/// 	Buffer char array ready to be filled.
		/// </param>
		/// <returns>
		/// 	A HAPI_StatusCode.
		/// </returns>
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_PrintNetwork( 		StringBuilder buffer );
		
		/// <summary>
		/// 	Loads a .otl file, adds the definition to the library and instantiates the asset into the 
		/// 	main Houdini scene.
		/// </summary>
		/// <param name="asset_info">
		/// 	A <see cref="HAPI_AssetInfo"/> that has at least <see cref="HAPI_AssetInfo.otlFilePath"/>, 
		/// 	<see cref="HAPI_AssetInfo.minVerticesPerPrimitive"/>, and 
    	///		<see cref="HAPI_AssetInfo.maxVerticesPerPrimitive"/> properly initialized. The 
		///		<see cref="HAPI_AssetInfo.otlFilePath"/> should contain the absolute path to the .otl file. The 
    	///		function will initialize the rest of the struct with appropriate data so this argument 
		///		serves as a return value as well.
		/// </param>
		/// <returns>
		/// 	A HAPI_StatusCode.
		/// </returns>
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_LoadOTLFile( 		ref HAPI_AssetInfo asset_info );
		
		/// <summary>
		/// 	Destroy the asset instance and unload the OTL definition.
		/// </summary>
		/// <param name="asset_info">
		/// 	The asset id returned by <see cref="HAPI_Host.HAPI_LoadOTLFile"/>.
		/// </param>
		/// <returns>
		/// 	A HAPI_StatusCode.
		/// </returns>
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_UnloadOTLFile( 		int asset_id );			
	
	}

}