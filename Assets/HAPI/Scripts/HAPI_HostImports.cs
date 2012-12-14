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
 * 		Continuation of HAPI_Host class definition. Here we include all libHAPI.dll dll imports.
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
	public static partial class HAPI_Host
	{		
		// GENERICS -------------------------------------------------------------------------------------------------
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_ConvertTransform( 	ref HAPI_TransformEuler transform_in_out, 
														 	int rst_order, int rot_order );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_ConvertMatrix(	float[] mat,
														int rst_order, int rot_order,
														ref HAPI_TransformEuler transform_out );
		
		
		// STRINGS --------------------------------------------------------------------------------------------------
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetStringBufLength( 	int string_handle, 
															out int buffer_length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetString(		 	int string_handle,
															StringBuilder string_value,
															int buffer_length );

		// TIME -----------------------------------------------------------------------------------------------------

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetTime( 			out float time );

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_SetTime( 			float time );

		// ASSETS ---------------------------------------------------------------------------------------------------

		[DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl )]
		private static extern int HAPI_CookAsset(			int asset_id );
		
		// PARAMETERS -----------------------------------------------------------------------------------------------
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetParameters( 		int asset_id, 
															[Out] HAPI_ParmInfo[] parm_infos, 
															int start, int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetParmIntValues( 	int asset_id, 
															[Out] int[] values, 
															int start, int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetParmFloatValues( 	int asset_id, 
															[Out] float[] values, 
															int start, int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetParmStringValues( int asset_id, 
															[Out] int[] values, 
															int start, int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetParmChoiceLists(	int asset_id, 
															[Out] HAPI_ParmChoiceInfo[] parm_choices, 
															int start, int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_SetParmIntValues( 	int asset_id, 
															int[] values, 
															int start, int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_SetParmFloatValues( 	int asset_id, 
															float[] values, 
															int start, int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_SetParmStringValue( 	int asset_id, 
															string value, 
															int parm_id, int index );
		
		// HANDLES --------------------------------------------------------------------------------------------------
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetHandleInfo(		int asset_id, 
															[Out] HAPI_HandleInfo[] handle_infos,
															int start, int length );		
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetHandleBindingInfo(	int asset_id,
																int handle_index,
																[Out] HAPI_HandleBindingInfo[] handle_infos,
																int start, int length );
		
		// PRESETS --------------------------------------------------------------------------------------------------
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetPreset( int asset_id, 
												  byte[] preset,
												  ref int length );


		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_SetPreset(	int asset_id, 
				 									byte[] preset,
													int length );
		
		
		// OBJECTS --------------------------------------------------------------------------------------------------
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetObjects(			int asset_id, 
															[Out] HAPI_ObjectInfo[] object_infos, 
															int start, int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetObjectTransforms( int asset_id, int rst_order,
															[Out] HAPI_Transform[] transforms,
															int start, int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetInstanceTransforms( int asset_id, int object_id, int geo_id,
															  int rstOrder, [Out] HAPI_Transform[] transforms,
															  int start, int length );
		
		// GEOMETRY GETTERS -----------------------------------------------------------------------------------------
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetGeoInfo(			int asset_id, int object_id, int geo_id,
															out HAPI_GeoInfo geo_info );

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetPartInfo(			int asset_id, int object_id, int geo_id, int part_id,
															out HAPI_PartInfo part_info );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetFaceCounts(		int asset_id, int object_id, int geo_id, int part_id,
															[Out] int[] face_counts,
															int start, int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetVertexList(		int asset_id, int object_id, int geo_id, int part_id,
															[Out] int[] vertex_list,
															int start, int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetAttributeInfo(	int asset_id, int object_id, int geo_id, int part_id,
															string name,
															ref HAPI_AttributeInfo attr_info );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetAttributeNames(	int asset_id, int object_id, int geo_id, int part_id,
															int attribute_type,
															[Out] int[] data,
															int count );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetAttributeIntData( int asset_id, int object_id, int geo_id, int part_id,
															string name,
															ref HAPI_AttributeInfo attr_info,
															[Out] int[] data,
															int start, int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetAttributeFloatData( int asset_id, int object_id, int geo_id, int part_id,
															  string name,
															  ref HAPI_AttributeInfo attr_info,
															  [Out] float[] data,
															  int start, int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetAttributeStrData( int asset_id, int object_id, int geo_id, int part_id,
															string name,
															ref HAPI_AttributeInfo attr_info,
															[Out] int[] data,
															int start, int length );
		
		// GEOMETRY SETTERS -----------------------------------------------------------------------------------------
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_SetGeoInfo(			int asset_id, int object_id, int geo_id,
															ref HAPI_GeoInfo geo_info );

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_SetPartInfo(			int asset_id, int object_id, int geo_id,
															ref HAPI_PartInfo part_info );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_SetFaceCounts(		int asset_id, int object_id, int geo_id,
															int[] face_counts,
															int start, int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_SetVertexList(		int asset_id, int object_id, int geo_id,
															int[] vertex_list,
															int start, int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_AddAttribute(		int asset_id, int object_id, int geo_id,
															string name,
															ref HAPI_AttributeInfo attr_info );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_SetAttributeIntData( int asset_id, int object_id, int geo_id,
															string name,
															ref HAPI_AttributeInfo attr_info,
															int[] data,
															int start, int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_SetAttributeFloatData( int asset_id, int object_id, int geo_id,
															  string name,
															  ref HAPI_AttributeInfo attr_info,
															  float[] data,
															  int start, int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_CommitGeo( int asset_id, int object_id, int geo_id );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_RevertGeo( int asset_id, int object_id, int geo_id );
		
		// GEOMETRY INPUT -------------------------------------------------------------------------------------------
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_SetFileInput( 		int asset_id, int input_idx, 
															string file_name );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_CreateGeoInput( 		int asset_id, int input_idx, 
															out int object_id, out int geo_id );
		
		// INTER ASSET ----------------------------------------------------------------------------------------------
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_ConnectAssetTransform( int asset_id_from, int asset_id_to, int input_idx );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_DisconnectAssetTransform( int asset_id, int input_idx );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_ConnectAssetGeometry( int asset_id_from, 
															 int object_id_from,
															 int group_id_from,
															 int asset_id_to, 
															 int input_idx );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_DisconnectAssetGeometry( int asset_id, int input_idx );
		
		// MATERIALS ------------------------------------------------------------------------------------------------
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetMaterials(	int asset_id, 
														[Out] HAPI_MaterialInfo[] material_infos,
														int start, int length );
		
		// HIP FILES ------------------------------------------------------------------------------------------------
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_ExportAssetToHIPFile( int asset_id, string file_name );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_ReplaceAssetFromHIPFile( int asset_id, string file_name );
				
		
		// None-exposed API calls: ----------------------------------------------------------------------------------
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_Initialize(	string houdini_install_path,
													string otl_search_path,
													[ MarshalAs( UnmanagedType.U1 ) ] bool use_cooking_thread,
													int cooking_thread_stack_size );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_Cleanup();
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_PrintNetwork( 		StringBuilder buffer );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetLastErrorStringLength( out int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetLastErrorString( 	StringBuilder buffer );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_SaveHIPFile( string file_name );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_LoadOTLFile(	string file_path, 
													string textures_path,
													int min_vertices_per_primitive,
													int max_vertices_per_primitive,
													ref int asset_id );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_CreateCurve( ref int asset_id );

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_LoadHIPFile(	string file_name,
													string textures_path,
													ref HAPI_AssetInfo asset_info );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_UnloadOTLFile( int asset_id );

		[DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl )]
		private static extern int HAPI_GetAssetInfo( int asset_id, ref HAPI_AssetInfo asset_info );
		
	}

}
