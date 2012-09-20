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
	public partial class HAPI_Host
	{		
		// GENERICS -------------------------------------------------------------------------------------------------
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_ConvertTransform( 	ref HAPI_TransformEuler transform_in_out, 
                                     					 	int rst_order, int rot_order );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_ConvertMatrix( 		float[] mat,
                                     					 	int rst_order, int rot_order,
															ref HAPI_TransformEuler transform_out );
		
		// STRINGS --------------------------------------------------------------------------------------------------
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetStringLength( 	int string_handle, 
															out int string_length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetString(		 	int string_handle,
															StringBuilder string_value,
															int string_length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_SetString( 			ref int string_handle,
															StringBuilder string_value,
															int string_length );
		
		// PARAMETERS -----------------------------------------------------------------------------------------------
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetParameters( 		int asset_id, 
															[Out] HAPI_ParmInfo[] parm_infos, 
															int start, int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetParmExtraValues( 	int asset_id, 
															[Out] HAPI_ParmSingleValue[] parm_extra_values, 
															int start, int length );
		
	    [ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetParmChoiceLists( 	int asset_id, 
															[Out] HAPI_ParmChoiceInfo[] parm_choices, 
															int start, int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_SetParameters( 		int asset_id, 
															[Out] HAPI_ParmInfo[] parm_infos, 
															int start, int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_SetParmExtraValues( 	int asset_id, 
															[Out] HAPI_ParmSingleValue[] parm_extra_values, 
															int start, int length );
		
		// HANDLES --------------------------------------------------------------------------------------------------
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
	    private static extern int HAPI_GetHandleInfo(		int asset_id, 
															[Out] HAPI_HandleInfo[] handle_infos,
															int start, int length );		
		
	    [ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
	    private static extern int HAPI_GetHandleBindingInfo(int asset_id,
				                                         	int handle_index,
															[Out] HAPI_HandleBindingInfo[] handle_infos,
															int start, int length );
		
		// OBJECTS --------------------------------------------------------------------------------------------------
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetObjects( 			int asset_id, 
															[Out] HAPI_ObjectInfo[] object_infos, 
															int start, int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetObjectTransforms( int asset_id, int rst_order,
															[Out] HAPI_Transform[] transforms,
															int start, int length );
		
		// DETAILS --------------------------------------------------------------------------------------------------
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetDetailInfo(		int asset_id, int object_id,
															out HAPI_DetailInfo detail_info );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetFaceCounts(		int asset_id, int object_id,
															[Out] int[] face_counts,
															int start, int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetVertexList(		int asset_id, int object_id,
															[Out] int[] vertex_list,
															int start, int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetAttributeInfo(	int asset_id, int object_id,
															int attribute_type,
															ref HAPI_AttributeInfo attr_info );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetAttributeNames(	int asset_id, int object_id,
															int attribute_type,
															[Out] HAPI_AttributeStrValue[] data,
															int count );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetAttributeIntData( int asset_id, int object_id,
															ref HAPI_AttributeInfo attr_info,
															[Out] int[] data,
															int start, int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetAttributeFloatData( int asset_id, int object_id,
															  ref HAPI_AttributeInfo attr_info,
															  [Out] float[] data,
															  int start, int length );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetAttributeStrData( int asset_id, int object_id,
															ref HAPI_AttributeInfo attr_info,
															[Out] HAPI_AttributeStrValue[] data,
															int start, int length );
		
		// None-exposed API calls: ----------------------------------------------------------------------------------
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_Initialize();
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_Cleanup();
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_PrintNetwork( 		StringBuilder buffer );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_GetLastErrorString( 	StringBuilder buffer );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_LoadOTLFile( 		ref HAPI_AssetInfo asset_info );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_UnloadOTLFile( 		int asset_id );
		
	}

}