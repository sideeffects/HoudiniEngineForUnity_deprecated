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
		// INITIALIZATION / CLEANUP ---------------------------------------------------------------------------------

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_IsInitialized();

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_Initialize( string houdini_install_path,
												   string otl_search_path,
												   string dso_search_path,
												   [MarshalAs( UnmanagedType.U1 )] bool use_cooking_thread,
												   int cooking_thread_stack_size );

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_Cleanup();

		// DIAGNOSTICS ----------------------------------------------------------------------------------------------

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_PrintNetwork( StringBuilder buffer );

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetStatus( int status_code, out int status );

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetStatusStringBufLength( int status_code, out int buffer_size );

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetStatusString( int status_code, StringBuilder buffer );

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetCookingTotalCount( out int count );

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetCookingCurrentCount( out int count );

		// UTILITY --------------------------------------------------------------------------------------------------
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_ConvertTransform( 	ref HAPI_TransformEuler transform_in_out, 
														 	int rst_order, int rot_order );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_ConvertMatrixToQuat(		float[] mat, int rst_order,
																ref HAPI_Transform transform_out );

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_ConvertMatrixToEuler(	float[] mat, int rst_order, int rot_order,
																ref HAPI_TransformEuler transform_out );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_ConvertTransformQuatToMatrix( HAPI_Transform transform, 
																	 [Out] float[] matrix );

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_ConvertTransformEulerToMatrix( HAPI_TransformEuler transform, 
																	  [Out] float[] matrix );
		
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

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_IsAssetValid( int asset_id, int asset_validation_id, out int answer );

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_LoadOTLFile( string file_path,
													int min_vertices_per_primitive,
													int max_vertices_per_primitive,
													ref int asset_id );

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_CreateCurve( ref int asset_id );

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_UnloadOTLFile( int asset_id );

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetAssetInfo( int asset_id, ref HAPI_AssetInfo asset_info );

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_CookAsset( int asset_id );

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_Interrupt();

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetAssetTransform( int asset_id, int rst_order, int rot_order,
														  out HAPI_TransformEuler transform );

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_SetAssetTransform( int asset_id, 
														  ref HAPI_TransformEuler transform );

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetInputName( int asset_id, int input_idx, int input_type,
													 ref int name_sh );

		// HIP FILES ------------------------------------------------------------------------------------------------

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_LoadHIPFile( string file_name,
													int min_vertices_per_primitive,
													int max_vertices_per_primitive );

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetAssetCountFromLoadHIPFile( ref int asset_count );

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetAssetIdsFromLoadHIPFile( [Out] int[] asset_ids );

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_SaveHIPFile( string file_name );

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_ExportAssetToHIPFile( int asset_id, string file_name );

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_ReplaceAssetFromHIPFile( int asset_id, string file_name );

		// NODES ----------------------------------------------------------------------------------------------------

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetNodeInfo( int node_id, ref HAPI_NodeInfo node_info );

		// PARAMETERS -----------------------------------------------------------------------------------------------
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetParameters( 		int node_id, [Out] HAPI_ParmInfo[] parm_infos, 
															int start, int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetParmIntValues( 	int node_id, [Out] int[] values, 
															int start, int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetParmFloatValues( 	int node_id, [Out] float[] values, 
															int start, int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetParmStringValues( int node_id, [Out] int[] values, 
															int start, int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetParmChoiceLists(	int node_id, [Out] HAPI_ParmChoiceInfo[] parm_choices, 
															int start, int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_SetParmIntValues( 	int node_id, int[] values, 
															int start, int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_SetParmFloatValues( 	int node_id, float[] values, 
															int start, int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_SetParmStringValue( 	int node_id, string value, 
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
		private static extern int HAPI_GetPresetBufLength( int asset_id, ref int buffer_length );

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetPreset( int asset_id, [Out] byte[] preset, int buffer_length );

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_SetPreset( int asset_id, byte[] preset, int buffer_length );
		
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

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_SetObjectTransform(	int asset_id, int object_id,
															HAPI_TransformEuler transform );
		
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
															out HAPI_GeoInputInfo geo_input_info );
		
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
		private static extern int HAPI_GetMaterial(		int asset_id, int material_id,
														out HAPI_MaterialInfo material_info );

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_ExtractTextureToFile(	int node_id, int parm_id,
																string destination_folder_path,
																ref int destination_file_path );

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetTextureSize(	int node_id, int parm_id,
														ref int texture_size );

		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetTexture(		int node_id, int parm_id,
														[Out] byte[] buffer );
	}

}
