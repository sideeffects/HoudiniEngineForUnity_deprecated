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
			
	}
	
	public class HAPI_ErrorInitFailed : HAPI_Error
	{
			
	}
	
	public class HAPI_ErrorFileLoadFailed : HAPI_Error
	{
		
	}
	
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
			HAPI_StatusCode status_code = 0;			
			
			if ( myHoudiniSceneExists ) 
			{
				Debug.Log( "Loading OTL: hip already exists" );
			} 
			else 
			{
				Debug.Log( "Loading OTL: new hip created" );
								
				status_code = (HAPI_StatusCode) HAPI_Initialize();
				
				if ( status_code != HAPI_StatusCode.HAPI_STATUS_ALREADY_INITIALIZED )
				{				
					processStatusCode( status_code );
					
					if ( hasCallFailed( status_code ) )
						throw new HAPI_ErrorInitFailed();
				}
				
				myHoudiniSceneExists = true;
			}
			
			HAPI_AssetInfo asset_info 			= new HAPI_AssetInfo();
			asset_info.otlFilePath 				= path;
			asset_info.minVerticesPerPrimitive 	= 3;
			asset_info.maxVerticesPerPrimitive 	= 3;
			
			status_code = (HAPI_StatusCode) HAPI_LoadOTLFile( ref asset_info );
			
			processStatusCode( status_code );
			
			if ( hasCallFailed( status_code ) )
				throw new HAPI_ErrorFileLoadFailed();
			
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
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="parm_infos">
		/// 	Array of <see cref="HAPI_ParmInfo"/> at least the size of 
		/// 	<paramref name="end"/> - <paramref name="start"/>.
		/// </param>
		/// <param name="start">
		/// 	First parameter index to begin with. Must be at least 0
		/// 	and at most <paramref name="end"/> - 1.
		/// </param>
		/// <param name="end">
		/// 	Last parameter index to use. Must be at least <paramref name="start"/> + 1 and 
		/// 	at most <see cref="HAPI_AssetInfo.parmCount"/>
		/// </param>
		public static void getParameters( 	int asset_id, 
											[Out] HAPI_ParmInfo[] parm_infos, 
											int start, int end )
		{
			int status_code = HAPI_GetParameters( asset_id, parm_infos, start, end );
			processStatusCode( (HAPI_StatusCode) status_code );	
		}
		
		/// <summary>
		/// 	Fill an array of <see cref="HAPI_ParmSingleValue"/> structs with extra parameter vector fields.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="parm_extra_values">
		/// 	Array of <see cref="HAPI_ParmSingleValue"/> exactly the size of 
		/// 	<see cref="HAPI_AssetInfo.parmExtraValueCount"/>.
		/// </param>
		/// <param name="count">
		/// 	Sanity check. Must be equal to <see cref="HAPI_AssetInfo.parmExtraValueCount"/>.
		/// </param>
		public static void getParmExtraValues( 	int asset_id, 
												[Out] HAPI_ParmSingleValue[] parm_extra_values, 
												int count )
		{
			int status_code = HAPI_GetParmExtraValues( asset_id, parm_extra_values, count );
			processStatusCode( (HAPI_StatusCode) status_code );	
		}
		
		/// <summary>
		/// 	Fill an array of <see cref="HAPI_ParmChoiceInfo"/> structs with parameter choice list information 
		/// 	from the asset instance node.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="parm_choices">
		/// 	Array of <see cref="HAPI_ParmChoiceInfo"/> exactly the size of 
		/// 	<see cref="HAPI_AssetInfo.parmChoiceCount"/>.
		/// </param>
		/// <param name="count">
		/// 	Sanity check. Must be equal to <see cref="HAPI_AssetInfo.parmChoiceCount"/>.
		/// </param>
		public static void getParmChoiceLists( 	int asset_id, 
												[Out] HAPI_ParmChoiceInfo[] parm_choices, 
												int count )
		{
			int status_code = HAPI_GetParmChoiceLists( asset_id, parm_choices, count );
			processStatusCode( (HAPI_StatusCode) status_code );	
		}
		
		/// <summary>
		/// 	Set a subset of parameter values using the given array of <see cref="HAPI_ParmInfo"/>.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="parm_infos">
		/// 	Array of <see cref="HAPI_ParmInfo"/> at least the size of 
		/// 	<paramref name="end"/> - <paramref name="start"/> containing the new parameter values.
		/// </param>
		/// <param name="start">
		/// 	First parameter index to begin with. Must be at least 0
		/// 	and at most <paramref name="end"/> - 1.
		/// </param>
		/// <param name="end">
		/// 	Last parameter index to use. Must be at least <paramref name="start"/> + 1 and 
		/// 	at most <see cref="HAPI_AssetInfo.parmCount"/> 
		/// </param>
		public static void setParameters( 		int asset_id, 
												[Out] HAPI_ParmInfo[] parm_infos, 
												int start, int end )
		{
			int status_code = HAPI_SetParameters( asset_id, parm_infos, start, end );
			processStatusCode( (HAPI_StatusCode) status_code );
		}
		
		/// <summary>
		/// 	Set an array of <see cref="HAPI_ParmSingleValue"/> structs with extra parameter vector fields.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="parm_extra_values">
		/// 	Array of <see cref="HAPI_ParmSingleValue"/> exactly the size of 
		/// 	<see cref="HAPI_AssetInfo.parmExtraValueCount"/>.
		/// </param>
		/// <param name="count">
		/// 	Sanity check. Must be equal to <see cref="HAPI_AssetInfo.parmExtraValueCount"/>.
		/// </param>
		public static void setParmExtraValues( 	int asset_id, 
												[Out] HAPI_ParmSingleValue[] parm_extra_values, 
												int count )
		{
			int status_code = HAPI_SetParmExtraValues( asset_id, parm_extra_values, count );
			processStatusCode( (HAPI_StatusCode) status_code );	
		}
		
		
		/// <summary>	
		/// 	Fill an array of HAPI_HandleInfo structs with information
	    ///		about every exposed user manipulation handle on the asset    
		/// </summary>
	    ///
	    /// <param name="asset_id">
	    ///			The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
	    /// <param name ="handle_infos">
	    ///			Array of <see cref="HAPI_HandleInfo"/> exactly the size of
	    ///			<see cref="HAPI_AssetInfo.handleCount"/>.
		/// </param>
	    ///	<param name="start">
	    ///			First object index to begin fill. Must be at least 
	    ///			0 and at most <paramref name="end"/> - 1.
		/// </param>
	    /// <param name ="end">
	    ///			Last object index to fill up to. Must be at least <paramref name="start"/> + 1 and
	    ///         at most <see cref="HAPI_AssetInfo.handleCount"/>
	    /// </param>
	    public static void getHandleInfo(	int asset_id, 
											[Out] HAPI_HandleInfo[] handle_infos,
											int start, int end )
		{
			int status_code = HAPI_GetHandleInfo( asset_id, handle_infos, start, end );
			processStatusCode( (HAPI_StatusCode) status_code );				
		}
	
		
		/// <summary>	
		/// 	Fill an array of HAPI_HandleBindingInfo structs with information
	    ///		about how each handle parameter maps to each asset parameter
		/// </summary>
	    ///
	    /// <param name="asset_id">
	    ///			The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="handle index">
	    ///			The index of the handle, from 0 to handleCount - 1 from the call to <see cref="HAPI_Host.loadOTLFile"/>
		/// </param>		
		/// <param name ="handle_infos">
	    ///			Array of <see cref="HAPI_HandleBindingInfo"/> exactly the size of
	    ///			<see cref="HAPI_HandleInfo.bindingsCount"/>.
		/// </param>
	    ///	<param name="start">
	    ///			First object index to begin fill. Must be at least 
	    ///			0 and at most <paramref name="end"/> - 1.
		/// </param>
	    /// <param name ="end">
	    ///			Last object index to fill up to. Must be at least <paramref name="start"/> + 1 and
	    ///         at most <see cref="HAPI_HandleInfo.bindingsCount"/>
	    /// </param>
	    public static void getHandleBindingInfo (	int asset_id,
	                                         		int handle_index,
													[Out] HAPI_HandleBindingInfo[] handle_infos,
													int start, int end )
		{
			int status_code = HAPI_GetHandleBindingInfo( asset_id, handle_index, handle_infos, start, end );
			processStatusCode( (HAPI_StatusCode) status_code );	
		}
		
		/// <summary>
		/// 	Fill an array of <see cref="HAPI_ObjectInfo"/> structs with information on each visible object 
		/// 	in the scene that has a SOP network (is not a sub-network).
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="object_infos">
		/// 	Array of <see cref="HAPI_ObjectInfo"/> at least the size of 
		/// 	<paramref name="end"/> - <paramref name="start"/>.
		/// </param>
		/// <param name="start">
		/// 	First object index to begin with. Must be at least 0
		/// 	and at most <paramref name="end"/> - 1.
		/// </param>
		/// <param name="end">
		/// 	Last object index to use. Must be at least <paramref name="start"/> + 1 and
		/// 	at most <see cref="HAPI_AssetInfo.objectCount"/>
		/// </param>
		public static void getObjects( 			int asset_id, 
												[Out] HAPI_ObjectInfo[] object_infos, 
												int start, int end )
		{
			int status_code = HAPI_GetObjects( asset_id, object_infos, start, end );
			processStatusCode( (HAPI_StatusCode) status_code );	
		}
		
		/// <summary>
		/// 	Fill an array of <see cref="HAPI_ObjectInfo"/> structs with information on each visible object 
		/// 	in the scene that has a SOP network (is not a sub-network).
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="transforms">
		/// 	Array of <see cref="HAPI_Transform"/> at least the size of 
		/// 	<paramref name="end"/> - <paramref name="start"/>. The <see cref="HAPI_Transform.id"/> of each will be 
    	/// 	set to the object id as given by <see cref="HAPI_Host.HAPI_GetObjects"/>.
		/// </param>
		/// <param name="start">
		/// 	First object index to begin with. Must be at least 0
		/// 	and at most <paramref name="end"/> - 1.
		/// </param>
		/// <param name="end">
		/// 	Last object index to use. Must be at least <paramref name="start"/> + 1 and 
		/// 	at most <see cref="HAPI_AssetInfo.objectCount"/>
		/// </param>
		public static void getObjectTransforms( 		int asset_id, 
													[Out] HAPI_Transform[] transforms, 
													int start, int end )
		{
			int status_code = HAPI_GetObjectTransforms( asset_id, transforms, start, end );
			processStatusCode( (HAPI_StatusCode) status_code );	
		}
		
		/// <summary>
		/// 	Fill a <see cref="HAPI_GeometryInfo"/> with geometry information of an object.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="object_id">
		/// 	The object id returned by <see cref="HAPI_Host.getObjects"/>.
		/// </param>
		/// <param name="geometry_info">
		/// 	A <see cref="HAPI_GeometryInfo"/> to be filled with geometry information of the specified 
		/// 	object and asset instance.
		/// </param>
		public static void getGeometryInfo( 	int asset_id, int object_id, 
												out HAPI_GeometryInfo geometry_info )
		{
			int status_code = HAPI_GetGeometryInfo( asset_id, object_id, out geometry_info );
			processStatusCode( (HAPI_StatusCode) status_code );
		}
		
		/// <summary>
		/// 	Fill an array of <see cref="HAPI_RawVertex"/> structs with vertex information from 
		/// 	an object's geometry.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="object_id">
		/// 	The object id returned by <see cref="HAPI_Host.getObjects"/>.
		/// </param>
		/// <param name="vertices">
		/// 	Array of <see cref="HAPI_RawVertex"/> at least the size of 
		/// 	<paramref name="end"/> - <paramref name="start"/>.
		/// </param>
		/// <param name="start">
		/// 	First vertex index to begin with. Must be at least 0
		/// 	and at most <paramref name="end"/> - 1.
		/// </param>
		/// <param name="end">
		/// 	Last vertex index to use. Must be at least <paramref name="start"/> + 1 and
		/// 	at most <see cref="HAPI_GeometryInfo.vertexCount"/>
		/// </param>
		static public void getVertexArray( 		int asset_id, int object_id, 
												[Out] HAPI_RawVertex[] vertices, 
												int start, int end )
		{
			int status_code = HAPI_GetVertexArray( asset_id, object_id, vertices, start, end );
			processStatusCode( (HAPI_StatusCode) status_code );
		}
		
		/// <summary>
		/// 	Fill an array of <see cref="HAPI_RawPrimitive"/> structs with primitive information from 
		/// 	an object's geometry.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="object_id">
		/// 	The object id returned by <see cref="HAPI_Host.getObjects"/>.
		/// </param>
		/// <param name="primitives">
		/// 	Array of <see cref="HAPI_RawPrimitive"/> at least the size of 
		/// 	<paramref name="end"/> - <paramref name="start"/>.
		/// </param>
		/// <param name="start">
		/// 	First primitive index to begin with. Must be at least 0
		/// 	and at most <paramref name="end"/> - 1.
		/// </param>
		/// <param name="end">
		/// 	Last primitive index to use. Must be at least <paramref name="start"/> and 
		/// 	at most <see cref="HAPI_GeometryInfo.primCount"/>
		/// </param>
		static public void getPrimitveArray( 	int asset_id, int object_id, 
												[Out] HAPI_RawPrimitive[] primitives, 
												int start, int end )
		{
			int status_code = HAPI_GetPrimitveArray( asset_id, object_id, primitives, start, end );
			processStatusCode( (HAPI_StatusCode) status_code );
		}
		
		/// <summary>
		/// 	Fill an array of <see cref="HAPI_RawInstance"/> structs with instance information from 
		/// 	an object's geometry.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="object_id">
		/// 	The object id returned by <see cref="HAPI_Host.getObjects"/>.
		/// </param>
		/// <param name="instances">
		/// 	Array of <see cref="HAPI_RawInstance"/> at least the size of 
		/// 	<paramref name="end"/> - <paramref name="start"/>.
		/// </param>
		/// <param name="start">
		/// 	First instance index to begin with. Must be at least 0
		/// 	and at most <paramref name="end"/> - 1.
		/// </param>
		/// <param name="end">
		/// 	Last instance index to use. Must be at least <paramref name="start"/> and
		/// 	at most <see cref="HAPI_GeometryInfo.instanceCount"/>
		/// </param>
		static public void getInstanceArray( 	int asset_id, int object_id, 
												[Out] HAPI_RawInstance[] instances, 
												int start, int end )
		{
			int status_code = HAPI_GetInstanceArray( asset_id, object_id, instances, start, end );
			processStatusCode( (HAPI_StatusCode) status_code );
		}
		
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Private
		
		static private bool hasCallFailed( HAPI_StatusCode code )
		{
			return ( (int) code > 0 );	
		}
		
		static private void processStatusCode( HAPI_StatusCode code )
		{
			if ( hasCallFailed( code ) )
			{
				StringBuilder error_str = new StringBuilder( 200 );
				HAPI_GetLastErrorString( error_str );
				Debug.LogError( error_str );
			}
		}
		
		static private bool myHoudiniSceneExists = false;
		
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Private HAPI API
		
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
		/// 	First parameter index to begin with. Must be at least 0
		/// 	and at most <paramref name="end"/> - 1.
		/// </param>
		/// <param name="end">
		/// 	Last parameter index to use. Must be at least <paramref name="start"/> + 1 and 
		/// 	at most <see cref="HAPI_AssetInfo.parmCount"/>
		/// </param>
		/// <returns>
		/// 	A HAPI_StatusCode.
		/// </returns>
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetParameters( 		int asset_id, 
															[Out] HAPI_ParmInfo[] parm_infos, 
															int start, int end );
		
		/// <summary>
		/// 	Fill an array of <see cref="HAPI_ParmSingleValue"/> structs with extra parameter vector fields.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="parm_extra_values">
		/// 	Array of <see cref="HAPI_ParmSingleValue"/> exactly the size of 
		/// 	<see cref="HAPI_AssetInfo.parmExtraValueCount"/>.
		/// </param>
		/// <param name="count">
		/// 	Sanity check. Must be equal to <see cref="HAPI_AssetInfo.parmExtraValueCount"/>.
		/// </param>
		/// <returns>
		/// 	A HAPI_StatusCode.
		/// </returns>
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetParmExtraValues( 	int asset_id, 
															[Out] HAPI_ParmSingleValue[] parm_extra_values, 
															int count );
		
	    [ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
	    private static extern int HAPI_GetHandleInfo(	int asset_id, 
											[Out] HAPI_HandleInfo[] handle_infos,
											int start, int end );
		
		
		
	    [ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
	    private static extern int HAPI_GetHandleBindingInfo (	int asset_id,
	                                         		int handle_index,
													[Out] HAPI_HandleBindingInfo[] handle_infos,
													int start, int end );
			
		/// <summary>
		/// 	Fill an array of <see cref="HAPI_ParmChoiceInfo"/> structs with parameter choice list information 
		/// 	from the asset instance node.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.HAPI_LoadOTLFile"/>.
		/// </param>
		/// <param name="parm_choices">
		/// 	Array of <see cref="HAPI_ParmChoiceInfo"/> exactly the size of 
		/// 	<see cref="HAPI_AssetInfo.parmChoiceCount"/>.
		/// </param>
		/// <param name="count">
		/// 	Sanity check. Must be equal to <see cref="HAPI_AssetInfo.parmChoiceCount"/>.
		/// </param>
		/// <returns>
		/// 	A HAPI_StatusCode.
		/// </returns>
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetParmChoiceLists( 	int asset_id, 
															[Out] HAPI_ParmChoiceInfo[] parm_choices, 
															int count );
		
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
		/// 	First parameter index to begin with. Must be at least 0
		/// 	and at most <paramref name="end"/> - 1.
		/// </param>
		/// <param name="end">
		/// 	Last parameter index to use. Must be at least <paramref name="start"/> + 1 and 
		/// 	at most <see cref="HAPI_AssetInfo.parmCount"/>
		/// </param>
		/// <returns>
		/// 	A HAPI_StatusCode.
		/// </returns>
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_SetParameters( 		int asset_id, 
															[Out] HAPI_ParmInfo[] parm_infos, 
															int start, int end );
		
		/// <summary>
		/// 	Set an array of <see cref="HAPI_ParmSingleValue"/> structs with extra parameter vector fields.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="parm_extra_values">
		/// 	Array of <see cref="HAPI_ParmSingleValue"/> exactly the size of 
		/// 	<see cref="HAPI_AssetInfo.parmExtraValueCount"/>.
		/// </param>
		/// <param name="count">
		/// 	Sanity check. Must be equal to <see cref="HAPI_AssetInfo.parmExtraValueCount"/>.
		/// </param>
		/// <returns>
		/// 	A HAPI_StatusCode.
		/// </returns>
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_SetParmExtraValues( 	int asset_id, 
															[Out] HAPI_ParmSingleValue[] parm_extra_values, 
															int count );
		
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
		/// 	First object index to begin with. Must be at least 0
		/// 	and at most <paramref name="end"/> - 1.
		/// </param>
		/// <param name="end">
		/// 	Last object index to use. Must be at least <paramref name="start"/> + 1 and 
		/// 	at most <see cref="HAPI_AssetInfo.objectCount"/>
		/// </param>
		/// <returns>
		/// 	A HAPI_StatusCode.
		/// </returns>
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetObjects( 			int asset_id, 
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
		/// 	First object index to begin with. Must be at least 0
		/// 	and at most <paramref name="end"/> - 1.
		/// </param>
		/// <param name="end">
		/// 	Last object index to use. Must be at least <paramref name="start"/> + 1 and 
		/// 	at most <see cref="HAPI_AssetInfo.objectCount"/>
		/// </param>
		/// <returns>
		/// 	A HAPI_StatusCode.
		/// </returns>
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetObjectTransforms( 	int asset_id, 
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
		private static extern int HAPI_GetGeometryInfo( 		int asset_id, int object_id, 
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
		/// 	First vertex index to begin with. Must be at least 0
		/// 	and at most <paramref name="end"/> - 1.
		/// </param>
		/// <param name="end">
		/// 	Last vertex index to use. Must be at least <paramref name="start"/> + 1 and 
		/// 	at most <see cref="HAPI_GeometryInfo.vertexCount"/>
		/// </param>
		/// <returns>
		/// 	A HAPI_StatusCode.
		/// </returns>
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetVertexArray( 		int asset_id, int object_id, 
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
		/// 	First primitive index to begin with. Must be at least 0
		/// 	and at most <paramref name="end"/> - 1.
		/// </param>
		/// <param name="end">
		/// 	Last primitive index to use. Must be at least <paramref name="start"/> + 1 and 
		/// 	at most <see cref="HAPI_GeometryInfo.primCount"/>
		/// </param>
		/// <returns>
		/// 	A HAPI_StatusCode.
		/// </returns>
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetPrimitveArray( 	int asset_id, int object_id, 
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
		/// 	First instance index to begin with. Must be at least 0
		/// 	and at most <paramref name="end"/> - 1.
		/// </param>
		/// <param name="end">
		/// 	Last instance index to use. Must be at least <paramref name="start"/> and 
		/// 	at most <see cref="HAPI_GeometryInfo.instanceCount"/>
		/// </param>
		/// <returns>
		/// 	A HAPI_StatusCode.
		/// </returns>
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetInstanceArray( 	int asset_id, int object_id, 
															[Out] HAPI_RawInstance[] instances, 
															int start, int end );
		
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
		/// 	Debug helper function that returns more detailed text for a failed status code.
		/// </summary>
		/// <param name="buffer">
		/// 	Buffer char array ready to be filled.
		/// </param>
		/// <returns>
		/// 	A HAPI_StatusCode.
		/// </returns>
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetLastErrorString( 	StringBuilder buffer );
		
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