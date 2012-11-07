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
 * 		Continuation of HAPI_Host class definition. Here we include all public wrappers for the dll imports defined
 * 		in HAPI_HostImports.cs.
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

		/// <summary>
		/// 	Converts a transform to differing TRS order and Euler rotation order.
		/// </summary>
		/// <param name= "transform_in_out">
		/// 	Used for both input and output.
		/// </param>
		/// <param name="rst_order">
		/// 	The desired transform order of the output.
		/// 	TRS = 0, TSR = 1, RTS = 2, RST = 3, STR = 4, SRT = 5
		/// </param>
		///	<param name="rot_order">
		/// 	The desired rotation order of the output.
		/// 	XYZ = 0, XZY = 1, YXZ = 2, YZX = 3, ZXY = 4, ZYX = 5
		/// </param>
		public static void convertTransform(	ref HAPI_TransformEuler transform_in_out,
												int rst_order, int rot_order )
		{
			int status_code = HAPI_ConvertTransform( ref transform_in_out, rst_order, rot_order );
			processStatusCode( (HAPI_StatusCode) status_code );
		}


		/// <summary>
		/// 	Converts a 4x4 matrix into its TRS form.
		/// </summary>
		/// <param name="mat">
		/// 	A 4x4 matrix expressed in a 16 element float array.
		/// </param>
		/// <param name="rst_order">
		/// 	The desired transform order of the output.
		/// 	TRS = 0, TSR = 1, RTS = 2, RST = 3, STR = 4, SRT = 5
		/// </param>
		///	<param name="rot_order">
		/// 	The desired rotation order of the output.
		/// 	XYZ = 0, XZY = 1, YXZ = 2, YZX = 3, ZXY = 4, ZYX = 5
		/// </param>
		/// <param name="transform_out">
		/// 	Used for the output.
		/// </param>
		public static void convertMatrix(	float[] mat,
											int rst_order, int rot_order,
											ref HAPI_TransformEuler transform_out )
		{
			int status_code = HAPI_ConvertMatrix( mat, rst_order, rot_order, ref transform_out );
			processStatusCode( (HAPI_StatusCode) status_code );
		}


		// STRINGS --------------------------------------------------------------------------------------------------

		// NOTE: These are private as we want people to use the more abstract methods in HAPI_HostStrings.cs.

		/// <summary>
		/// 	Gives back the string length of the string with the given handle.
		/// </summary>
		/// <param name="string_handle">
		/// 	Handle of the string to query.
		/// </param>
		/// <param name="string_length">
		/// 	Length of the queried string.
		/// </param>
		private static void getStringLength( 	int string_handle,
												out int string_length )
		{
			int status_code = HAPI_GetStringLength( string_handle, out string_length );
			processStatusCode( (HAPI_StatusCode) status_code );
		}

		/// <summary>
		/// 	Gives back the string value of the string with the given handle.
		/// </summary>
		/// <param name="string_handle">
		/// 	Handle of the string to query.
		/// </param>
		/// <param name="string_value">
		/// 	Actual string value.
		/// </param>
		/// <param name="string_length">
		/// 	Length of the queried string (must match size of <paramref name="string_value">).
		/// </param>
		private static void getString(		 	int string_handle,
												StringBuilder string_value,
												int string_length )
		{
			int status_code = HAPI_GetString( string_handle, string_value, string_length );
			processStatusCode( (HAPI_StatusCode) status_code );
		}

		// PARAMETERS -----------------------------------------------------------------------------------------------

		/// <summary>
		/// 	Fill an array of HAPI_ParmInfo structs with parameter information from the asset instance node.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="parm_infos">
		/// 	Array of <see cref="HAPI_ParmInfo"/> at least the size of
		/// 	<paramref name="length"/>.
		/// </param>
		/// <param name="start">
		/// 	First index of range. Must be at least 0 and at most <see cref="HAPI_AssetInfo.parmCount"/> - 1.
		/// </param>
		/// <param name="length">
		/// 	Must be at least 0 and at most <see cref="HAPI_AssetInfo.parmCount"/> - <paramref name="start"/>.
		/// </param>
		public static void getParameters( 	int asset_id,
											[Out] HAPI_ParmInfo[] parm_infos,
											int start, int length )
		{
			int status_code = HAPI_GetParameters( asset_id, parm_infos, start, length );
			processStatusCode( (HAPI_StatusCode) status_code );
		}

		/// <summary>
		/// 	Fill an array of parameter int values.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="values">
		/// 	Array of ints at least the size of <paramref name="length"/>.
		/// </param>
		/// <param name="start">
		/// 	First index of range. Must be at least 0 and at most
		/// 	<see cref="HAPI_AssetInfo.parmIntValueCount"/> - 1.
		/// </param>
		/// <param name="length">
		/// 	Must be at least 0 and at most
		/// 	<see cref="HAPI_AssetInfo.parmIntValueCount"/> - <paramref name="start"/>.
		/// </param>
		public static void getParmIntValues(	int asset_id,
												[Out] int[] values,
												int start, int length )
		{
			int status_code = HAPI_GetParmIntValues( asset_id, values, start, length );
			processStatusCode( (HAPI_StatusCode) status_code );
		}

		/// <summary>
		/// 	Fill an array of parameter float values.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="values">
		/// 	Array of floats at least the size of <paramref name="length"/>.
		/// </param>
		/// <param name="start">
		/// 	First index of range. Must be at least 0 and at most
		/// 	<see cref="HAPI_AssetInfo.parmFloatValueCount"/> - 1.
		/// </param>
		/// <param name="length">
		/// 	Must be at least 0 and at most
		/// 	<see cref="HAPI_AssetInfo.parmFloatValueCount"/> - <paramref name="start"/>.
		/// </param>
		public static void getParmFloatValues(	int asset_id,
												[Out] float[] values,
												int start, int length )
		{
			int status_code = HAPI_GetParmFloatValues( asset_id, values, start, length );
			processStatusCode( (HAPI_StatusCode) status_code );
		}

		/// <summary>
		/// 	Fill an array of parameter string handles. These handles must be used in conjunction with
		/// 	<see cref="HAPI_GetString"> to get the actual string values.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="values">
		/// 	Array of ints at least the size of <paramref name="length"/>.
		/// </param>
		/// <param name="start">
		/// 	First index of range. Must be at least 0 and at most
		/// 	<see cref="HAPI_AssetInfo.parmStringValueCount"/> - 1.
		/// </param>
		/// <param name="length">
		/// 	Must be at least 0 and at most
		/// 	<see cref="HAPI_AssetInfo.parmStringValueCount"/> - <paramref name="start"/>.
		/// </param>
		public static void getParmStringValues(	int asset_id,
												[Out] int[] values,
												int start, int length )
		{
			int status_code = HAPI_GetParmStringValues( asset_id, values, start, length );
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
		/// 	Array of <see cref="HAPI_ParmChoiceInfo"/> exactly the size of <paramref name="length"/>.
		/// </param>
		/// <param name="start">
		/// 	First index of range. Must be at least 0 and at most <see cref="HAPI_AssetInfo.parmChoiceCount"/> - 1.
		/// </param>
		/// <param name="length">
		/// 	Must be at least 0 and at most <see cref="HAPI_AssetInfo.parmChoiceCount"/> - <paramref name="start"/>.
		/// </param>
		public static void getParmChoiceLists( 	int asset_id,
												[Out] HAPI_ParmChoiceInfo[] parm_choices,
												int start, int length )
		{
			int status_code = HAPI_GetParmChoiceLists( asset_id, parm_choices, start, length );
			processStatusCode( (HAPI_StatusCode) status_code );
		}

		/// <summary>
		/// 	Set (push) an array of parameter int values.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="values">
		/// 	Array of ints at least the size of <paramref name="length"/>.
		/// </param>
		/// <param name="start">
		/// 	First index of range. Must be at least 0 and at most
		/// 	<see cref="HAPI_AssetInfo.parmIntValueCount"/> - 1.
		/// </param>
		/// <param name="length">
		/// 	Must be at least 0 and at most
		/// 	<see cref="HAPI_AssetInfo.parmIntValueCount"/> - <paramref name="start"/>.
		/// </param>
		public static void setParmIntValues(	int asset_id,
												int[] values,
												int start, int length )
		{
			int status_code = HAPI_SetParmIntValues( asset_id, values, start, length );
			processStatusCode( (HAPI_StatusCode) status_code );
		}

		/// <summary>
		/// 	Set (push) an array of parameter float values.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="values">
		/// 	Array of floats at least the size of <paramref name="length"/>.
		/// </param>
		/// <param name="start">
		/// 	First index of range. Must be at least 0 and at most
		/// 	<see cref="HAPI_AssetInfo.parmFloatValueCount"/> - 1.
		/// </param>
		/// <param name="length">
		/// 	Must be at least 0 and at most
		/// 	<see cref="HAPI_AssetInfo.parmFloatValueCount"/> - <paramref name="start"/>.
		/// </param>
		public static void setParmFloatValues(	int asset_id,
												float[] values,
												int start, int length )
		{
			int status_code = HAPI_SetParmFloatValues( asset_id, values, start, length );
			processStatusCode( (HAPI_StatusCode) status_code );
		}

		/// <summary>
		/// 	Set (push) a string value. We can only set a single value at a time because we want to avoid
		/// 	fixed size string buffers.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="value">
		/// 	The string value.
		/// </param>
		/// <param name="parm_id">
		/// 	Parameter id of the parameter being updated.
		/// </param>
		/// <param name="index">
		/// 	Index within the parameter's values tuple.
		/// </param>
		public static void setParmStringValue(	int asset_id,
												string value,
												int parm_id, int index )
		{
			int status_code = HAPI_SetParmStringValue( asset_id, value, parm_id, index );
			processStatusCode( (HAPI_StatusCode) status_code );
		}

		// HANDLES --------------------------------------------------------------------------------------------------

		/// <summary>
		/// 	Fill an array of <see cref="HAPI_HandleInfo"/> structs with information about every exposed
		/// 	user manipulation handle on the asset.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name ="handle_infos">
		/// 	Array of <see cref="HAPI_HandleInfo"/> exactly the size of <paramref name="length"/>.
		/// </param>
		/// <param name="start">
		/// 	First index of range. Must be at least 0 and at most <see cref="HAPI_AssetInfo.handleCount"/> - 1.
		/// </param>
		/// <param name="length">
		/// 	Must be at least 0 and at most <see cref="HAPI_AssetInfo.handleCount"/> - <paramref name="start"/>.
		/// </param>
		public static void getHandleInfo(	int asset_id,
											[Out] HAPI_HandleInfo[] handle_infos,
											int start, int length )
		{
			int status_code = HAPI_GetHandleInfo( asset_id, handle_infos, start, length );
			processStatusCode( (HAPI_StatusCode) status_code );
		}

		/// <summary>
		/// 	Fill an array of <see cref="HAPI_HandleBindingInfo"/> structs with information about how each
		/// 	handle parameter maps to each asset parameter.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="handle index">
		/// 	The index of the handle, from 0 to handleCount - 1 from the call to <see cref="HAPI_Host.loadOTLFile"/>
		/// </param>
		/// <param name ="handle_infos">
		/// 	Array of <see cref="HAPI_HandleBindingInfo"/> exactly the size of <paramref name="length"/>.
		/// </param>
		/// <param name="start">
		/// 	First index of range. Must be at least 0 and at most <see cref="HAPI_HandleInfo.bindingsCount"/> - 1.
		/// </param>
		/// <param name="length">
		/// 	Must be at least 0 and at most <see cref="HAPI_HandleInfo.bindingsCount"/> - <paramref name="start"/>.
		/// </param>
		public static void getHandleBindingInfo(	int asset_id,
													int handle_index,
													[Out] HAPI_HandleBindingInfo[] handle_infos,
													int start, int length )
		{
			int status_code = HAPI_GetHandleBindingInfo( asset_id, handle_index, handle_infos, start, length );
			processStatusCode( (HAPI_StatusCode) status_code );
		}

		// PRESETS --------------------------------------------------------------------------------------------------
		
		/// <summary>
		/// 	Generates a preset for the given asset.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by HAPI_LoadOTLFile().
		/// </param>
		/// <param name="buf">
		/// 	Buffer to hold the preset data.
		/// </param>
		/// <param name="length">
		/// 	Size of the buffer.  If this value is 0, the required length of the buffer will be returned.
		/// </param>
		public static void getPreset( int asset_id, byte[] preset, ref int length )
		{
			int status_code = HAPI_GetPreset( asset_id, preset, ref length );
			processStatusCode( (HAPI_StatusCode) status_code );
		}

		/// <summary>
		/// 	Sets a particular asset to a given preset.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by HAPI_LoadOTLFile().
		/// </param>
		/// <param name="buf">
		/// 	Buffer to hold the preset data.
		/// </param>
		/// <param ="length">
		/// 	Size of the buffer.
		/// </param>
		public static void setPreset( int asset_id, byte[] preset, int length )
		{
			int status_code = HAPI_SetPreset( asset_id, preset, length );
			processStatusCode( (HAPI_StatusCode) status_code );
		}

		// OBJECTS --------------------------------------------------------------------------------------------------

		/// <summary>
		/// 	Fill an array of <see cref="HAPI_ObjectInfo"/> structs with information on each visible object
		/// 	in the scene that has a SOP network (is not a sub-network).
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="object_infos">
		/// 	Array of <see cref="HAPI_ObjectInfo"/> at least the size of <paramref name="length"/>.
		/// </param>
		/// <param name="start">
		/// 	First index of range. Must be at least 0 and at most <see cref="HAPI_AssetInfo.objectCount"/> - 1.
		/// </param>
		/// <param name="length">
		/// 	Must be at least 0 and at most <see cref="HAPI_AssetInfo.objectCount"/> - <paramref name="start"/>.
		/// </param>
		public static void getObjects( 			int asset_id,
												[Out] HAPI_ObjectInfo[] object_infos,
												int start, int length )
		{
			int status_code = HAPI_GetObjects( asset_id, object_infos, start, length );
			processStatusCode( (HAPI_StatusCode) status_code );
		}

		/// <summary>
		/// 	Fill an array of <see cref="HAPI_ObjectInfo"/> structs with information on each visible object
		/// 	in the scene that has a SOP network (is not a sub-network).
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="rst_order">
		/// 	The order of application of translation, rotation and
		/// 	scale:
		/// 	TRS = 0, TSR = 1, RTS = 2, RST = 3, STR = 4, SRT = 5
		/// </param>
		/// <param name="transforms">
		/// 	Array of <see cref="HAPI_Transform"/> at least the size of
		/// 	<paramref name="length"/>. The <see cref="HAPI_Transform.id"/> of each will be
		/// 	set to the object id as given by <see cref="HAPI_Host.HAPI_GetObjects"/>.
		/// </param>
		/// <param name="start">
		/// 	First index of range. Must be at least 0 and at most <see cref="HAPI_AssetInfo.objectCount"/> - 1.
		/// </param>
		/// <param name="length">
		/// 	Must be at least 0 and at most <see cref="HAPI_AssetInfo.objectCount"/> - <paramref name="start"/>.
		/// </param>
		public static void getObjectTransforms(	int asset_id,
												int rst_order,
												[Out] HAPI_Transform[] transforms,
												int start, int length )
		{
			int status_code = HAPI_GetObjectTransforms( asset_id, rst_order, transforms, start, length );
			processStatusCode( (HAPI_StatusCode) status_code );
		}
		
		// GEOMETRY GETTERS -----------------------------------------------------------------------------------------

		/// <summary>
		/// 	Get the main geometry info struct (<see cref="HAPI_GeoInfo"/>).
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="object_id">
		/// 	The object id returned by <see cref="HAPI_Host.getObjects"/>.
		/// </param>
		/// <param name="geo_id">
		/// 	The geometry id.
		/// </param>
		/// <param name="geo_info">
		/// 	<see cref="HAPI_GeoInfo"/> out parameter.
		/// </param>
		public static void getGeoInfo(			int asset_id, int object_id, int geo_id,
												out HAPI_GeoInfo geo_info )
		{
			int status_code = HAPI_GetGeoInfo( asset_id, object_id, geo_id, out geo_info );
			processStatusCode( (HAPI_StatusCode) status_code );
		}

		/// <summary>
		/// 	Get the array of faces where the nth integer in the array is the number of vertices
		/// 	the nth face has.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="object_id">
		/// 	The object id returned by <see cref="HAPI_Host.getObjects"/>.
		/// </param>
		/// <param name="geo_id">
		/// 	The geometry id.
		/// </param>
		/// <param name="face_counts">
		/// 	An integer array at least the size of <paramref name="length"/>.
		/// </param>
		/// <param name="start">
		/// 	First index of range. Must be at least 0 and at most <see cref="HAPI_GeoInfo.faceCount"/> - 1.
		/// </param>
		/// <param name="length">
		/// 	Must be at least 0 and at most <see cref="HAPI_GeoInfo.faceCount"/> - <paramref name="start"/>.
		/// </param>
		public static void getFaceCounts(		int asset_id, int object_id, int geo_id,
												[Out] int[] face_counts,
												int start, int length )
		{
			int status_code = HAPI_GetFaceCounts( asset_id, object_id, geo_id, face_counts, start, length );
			processStatusCode( (HAPI_StatusCode) status_code );
		}

		/// <summary>
		/// 	Get array containing the vertex-point associations where the ith element in the array is
		/// 	the point index the ith vertex associates with.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="object_id">
		/// 	The object id returned by <see cref="HAPI_Host.getObjects"/>.
		/// </param>
		/// <param name="geo_id">
		/// 	The geometry id.
		/// </param>
		/// <param name="vertex_list">
		/// 	An integer array at least the size of <paramref name="length"/>.
		/// </param>
		/// <param name="start">
		/// 	First index of range. Must be at least 0 and at most <see cref="HAPI_GeoInfo.vertexCount"/> - 1.
		/// </param>
		/// <param name="length">
		/// 	Must be at least 0 and at most <see cref="HAPI_GeoInfo.vertexCount"/> - <paramref name="start"/>.
		/// </param>
		public static void getVertexList(		int asset_id, int object_id, int geo_id,
												[Out] int[] vertex_list,
												int start, int length )
		{
			int status_code = HAPI_GetVertexList( asset_id, object_id, geo_id, vertex_list, start, length );
			processStatusCode( (HAPI_StatusCode) status_code );
		}

		/// <summary>
		/// 	Get attribute information; fill a <see cref="HAPI_AttributeInfo"/>.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="object_id">
		/// 	The object id returned by <see cref="HAPI_Host.getObjects"/>.
		/// </param>
		/// <param name="geo_id">
		/// 	The geometry id.
		/// </param>
		/// <param name="name">
		/// 	Attribute name.
		/// </param>
		/// <param name="attr_info">
		/// 	<see cref="HAPI_AttributeInfo"/> used as input for owner type and as output for the rest of 
		/// 	the information.
		/// </param>
		public static void getAttributeInfo(	int asset_id, int object_id, int geo_id, string name,
												ref HAPI_AttributeInfo attr_info )
		{
			int status_code = HAPI_GetAttributeInfo( asset_id, object_id, geo_id, name, ref attr_info );
			processStatusCode( (HAPI_StatusCode) status_code );
		}

		/// <summary>
		/// 	Get list of attribute names by attribute owner. Note that the name string handles are only valid 
		/// 	until the next time this function is called.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="object_id">
		/// 	The object id returned by <see cref="HAPI_Host.getObjects"/>.
		/// </param>
		/// <param name="geo_id">
		/// 	The geometry id.
		/// </param>
		/// <param name="attribute_type"/>
		/// 	The <see cref="HAPI_AttributeType"/> enum value specifying the owner of the attribute.
		/// </param>
		/// <param name="data">
		/// 	Array of ints (string handles) to house the attribute names.
		/// 	Should be exactly the size of the appropriate attribute owner type count
		/// 	in <see cref="HAPI_GeoInfo"/>.
		/// </param>
		/// <param name="count">
		/// 	Sanity check count. Must be equal to the appropriate attribute owner type count
		/// 	in <see cref="HAPI_GeoInfo"/>.
		/// </param>
		public static void getAttributeNames(	int asset_id, int object_id, int geo_id,
												int attribute_type,
												[Out] int[] data,
												int count )
		{
			int status_code = HAPI_GetAttributeNames( asset_id, object_id, geo_id, attribute_type, data, count );
			processStatusCode( (HAPI_StatusCode) status_code );
		}

		/// <summary>
		/// 	Get attribute integer data.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="object_id">
		/// 	The object id returned by <see cref="HAPI_Host.getObjects"/>.
		/// </param>
		/// <param name="geo_id">
		/// 	The geometry id.
		/// </param>
		/// <param name="name">
		/// 	Attribute name.
		/// </param>
		/// <param name="attr_info">
		/// 	<see cref="HAPI_AttributeInfo"/> used as input for tuple size. Also contains some sanity checks 
		/// 	like data type. Generally should be the same struct returned by 
		/// 	<see cref="HAPI_Host.getAttributeInfo"/>.
		/// <param name="data">
		/// 	An integer array at least the size of <paramref name="length"/>.
		/// </param>
		/// <param name="start">
		/// 	First index of range. Must be at least 0 and at most <see cref="HAPI_AttributeInfo.count"/> - 1.
		/// </param>
		/// <param name="length">
		/// 	Must be at least 0 and at most <see cref="HAPI_AttributeInfo.count"/> - <paramref name="start"/>.
		/// </param>
		public static void getAttributeIntData( int asset_id, int object_id, int geo_id, string name,
												ref HAPI_AttributeInfo attr_info,
												[Out] int[] data,
												int start, int length )
		{
			int status_code = HAPI_GetAttributeIntData( asset_id, object_id, geo_id, name, ref attr_info, data, 
														start, length );
			processStatusCode( (HAPI_StatusCode) status_code );
		}

		/// <summary>
		/// 	Get attribute float data.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="object_id">
		/// 	The object id returned by <see cref="HAPI_Host.getObjects"/>.
		/// </param>
		/// <param name="geo_id">
		/// 	The geometry id.
		/// </param>
		/// <param name="name">
		/// 	Attribute name.
		/// </param>
		/// <param name="attr_info">
		/// 	<see cref="HAPI_AttributeInfo"/> used as input for tuple size. Also contains some sanity checks 
		/// 	like data type. Generally should be the same struct returned by 
		/// 	<see cref="HAPI_Host.getAttributeInfo"/>.
		/// <param name="data">
		/// 	An float array at least the size of <paramref name="length"/>.
		/// </param>
		/// <param name="start">
		/// 	First index of range. Must be at least 0 and at most <see cref="HAPI_AttributeInfo.count"/> - 1.
		/// </param>
		/// <param name="length">
		/// 	Must be at least 0 and at most <see cref="HAPI_AttributeInfo.count"/> - <paramref name="start"/>.
		/// </param>
		public static void getAttributeFloatData(	int asset_id, int object_id, int geo_id, string name,
													ref HAPI_AttributeInfo attr_info,
													[Out] float[] data,
													int start, int length )
		{
			int status_code = HAPI_GetAttributeFloatData( asset_id, object_id, geo_id, name, ref attr_info, 
														  data, start, length );
			processStatusCode( (HAPI_StatusCode) status_code );
		}

		/// <summary>
		/// 	Get attribute string data. Note that the string handles returned are only valid until the next 
		/// 	time this function is called.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="object_id">
		/// 	The object id returned by <see cref="HAPI_Host.getObjects"/>.
		/// </param>
		/// <param name="geo_id">
		/// 	The geometry id.
		/// </param>
		/// <param name="name">
		/// 	Attribute name.
		/// </param>
		/// <param name="attr_info">
		/// 	<see cref="HAPI_AttributeInfo"/> used as input for tuple size. Also contains some sanity checks 
		/// 	like data type. Generally should be the same struct returned by 
		/// 	<see cref="HAPI_Host.getAttributeInfo"/>.
		/// </param>
		/// <param name="data">
		/// 	An int (string handles) array at least the size of <paramref name="length"/>.
		/// </param>
		/// <param name="start">
		/// 	First index of range. Must be at least 0 and at most <see cref="HAPI_AttributeInfo.count"/> - 1.
		/// </param>
		/// <param name="length">
		/// 	Must be at least 0 and at most <see cref="HAPI_AttributeInfo.count"/> - <paramref name="start"/>.
		/// </param>
		public static void getAttributeStrData( int asset_id, int object_id, int geo_id, string name,
												ref HAPI_AttributeInfo attr_info,
												[Out] int[] data,
												int start, int length )
		{
			int status_code = HAPI_GetAttributeStrData( asset_id, object_id, geo_id, name, ref attr_info, data, 
														start, length );
			processStatusCode( (HAPI_StatusCode) status_code );
		}
		
		// GEOMETRY SETTERS -----------------------------------------------------------------------------------------

		/// <summary>
		/// 	Set the main geometry info struct (<see cref="HAPI_GeoInfo"/>).
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="object_id">
		/// 	The object id returned by <see cref="HAPI_Host.getObjects"/>.
		/// </param>
		/// <param name="geo_id">
		/// 	The geometry id.
		/// </param>
		/// <param name="geo_info">
		/// 	<see cref="HAPI_GeoInfo"/> out parameter.
		/// </param>
		public static void setGeoInfo(	int asset_id, int object_id, int geo_id,
										ref HAPI_GeoInfo geo_info )
		{
			int status_code = HAPI_SetGeoInfo( asset_id, object_id, geo_id, ref geo_info );
			processStatusCode( (HAPI_StatusCode) status_code );
		}

		/// <summary>
		/// 	Set the array of faces where the nth integer in the array is the number of vertices
		/// 	the nth face has.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="object_id">
		/// 	The object id returned by <see cref="HAPI_Host.getObjects"/>.
		/// </param>
		/// <param name="geo_id">
		/// 	The geometry id.
		/// </param>
		/// <param name="face_counts">
		/// 	An integer array at least the size of <paramref name="length"/>.
		/// </param>
		/// <param name="start">
		/// 	First index of range. Must be at least 0 and at most <see cref="HAPI_GeoInfo.faceCount"/> - 1.
		/// </param>
		/// <param name="length">
		/// 	Must be at least 0 and at most <see cref="HAPI_GeoInfo.faceCount"/> - <paramref name="start"/>.
		/// </param>
		public static void setFaceCounts(		int asset_id, int object_id, int geo_id,
												int[] face_counts,
												int start, int length )
		{
			int status_code = HAPI_SetFaceCounts( asset_id, object_id, geo_id, face_counts, start, length );
			processStatusCode( (HAPI_StatusCode) status_code );
		}

		/// <summary>
		/// 	Set array containing the vertex-point associations where the ith element in the array is
		/// 	the point index the ith vertex associates with.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="object_id">
		/// 	The object id returned by <see cref="HAPI_Host.getObjects"/>.
		/// </param>
		/// <param name="geo_id">
		/// 	The geometry id.
		/// </param>
		/// <param name="vertex_list">
		/// 	An integer array at least the size of <paramref name="length"/>.
		/// </param>
		/// <param name="start">
		/// 	First index of range. Must be at least 0 and at most <see cref="HAPI_GeoInfo.vertexCount"/> - 1.
		/// </param>
		/// <param name="length">
		/// 	Must be at least 0 and at most <see cref="HAPI_GeoInfo.vertexCount"/> - <paramref name="start"/>.
		/// </param>
		public static void setVertexList(		int asset_id, int object_id, int geo_id,
												int[] vertex_list,
												int start, int length )
		{
			int status_code = HAPI_SetVertexList( asset_id, object_id, geo_id, vertex_list, start, length );
			processStatusCode( (HAPI_StatusCode) status_code );
		}

		/// <summary>
		/// 	Add an attribute.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="object_id">
		/// 	The object id returned by <see cref="HAPI_Host.getObjects"/>.
		/// </param>
		/// <param name="geo_id">
		/// 	The geometry id.
		/// </param>
		/// <param name="name">
		/// 	Attribute name.
		/// </param>
		/// <param name="attr_info">
		/// 	<see cref="HAPI_AttributeInfo"/> used as input for owner type and as output for the rest of 
		/// 	the information.
		/// </param>
		public static void addAttribute(	int asset_id, int object_id, int geo_id, string name,
											ref HAPI_AttributeInfo attr_info )
		{
			int status_code = HAPI_AddAttribute( asset_id, object_id, geo_id, name, ref attr_info );
			processStatusCode( (HAPI_StatusCode) status_code );
		}

		/// <summary>
		/// 	Set attribute integer data.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="object_id">
		/// 	The object id returned by <see cref="HAPI_Host.getObjects"/>.
		/// </param>
		/// <param name="geo_id">
		/// 	The geometry id.
		/// </param>
		/// <param name="name">
		/// 	Attribute name.
		/// </param>
		/// <param name="attr_info">
		/// 	<see cref="HAPI_AttributeInfo"/> used as input for tuple size. Also contains some sanity checks 
		/// 	like data type. Generally should be the same struct returned by 
		/// 	<see cref="HAPI_Host.getAttributeInfo"/>.
		/// <param name="data">
		/// 	An integer array at least the size of <paramref name="length"/>.
		/// </param>
		/// <param name="start">
		/// 	First index of range. Must be at least 0 and at most <see cref="HAPI_AttributeInfo.count"/> - 1.
		/// </param>
		/// <param name="length">
		/// 	Must be at least 0 and at most <see cref="HAPI_AttributeInfo.count"/> - <paramref name="start"/>.
		/// </param>
		public static void setAttributeIntData( int asset_id, int object_id, int geo_id, string name,
												ref HAPI_AttributeInfo attr_info,
												int[] data,
												int start, int length )
		{
			int status_code = HAPI_SetAttributeIntData( asset_id, object_id, geo_id, name, ref attr_info, data, 
														start, length );
			processStatusCode( (HAPI_StatusCode) status_code );
		}

		/// <summary>
		/// 	Set attribute float data.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="object_id">
		/// 	The object id returned by <see cref="HAPI_Host.getObjects"/>.
		/// </param>
		/// <param name="geo_id">
		/// 	The geometry id.
		/// </param>
		/// <param name="name">
		/// 	Attribute name.
		/// </param>
		/// <param name="attr_info">
		/// 	<see cref="HAPI_AttributeInfo"/> used as input for tuple size. Also contains some sanity checks 
		/// 	like data type. Generally should be the same struct returned by 
		/// 	<see cref="HAPI_Host.getAttributeInfo"/>.
		/// <param name="data">
		/// 	An float array at least the size of <paramref name="length"/>.
		/// </param>
		/// <param name="start">
		/// 	First index of range. Must be at least 0 and at most <see cref="HAPI_AttributeInfo.count"/> - 1.
		/// </param>
		/// <param name="length">
		/// 	Must be at least 0 and at most <see cref="HAPI_AttributeInfo.count"/> - <paramref name="start"/>.
		/// </param>
		public static void setAttributeFloatData(	int asset_id, int object_id, int geo_id, string name,
													ref HAPI_AttributeInfo attr_info,
													float[] data,
													int start, int length )
		{
			int status_code = HAPI_SetAttributeFloatData( asset_id, object_id, geo_id, name, ref attr_info, 
														  data, start, length );
			processStatusCode( (HAPI_StatusCode) status_code );
		}
		
		
		/// <summary>	
		/// 	Fill an array of HAPI_Transform structs with the transforms
		///		of each instance of this instancer object
		/// </summary>
		/// <param="asset_id">
		///			The asset id returned by HAPI_LoadOTLFile().
		/// </param>
		/// <param="object_id">
		///			The object id returned by HAPI_GetObjects().
		/// </param>
		/// <param name="geo_id">
		///			The geometry id.
		///	</param>
		/// <param name="rstOrder">
		///			The order of application of translation, rotation and
		///			scale:
		///			TRS = 0, TSR = 1, RTS = 2, RST = 3, STR = 4, SRT = 5  
		/// </param>
		/// <param="transforms">
		///			Array of HAPI_Transform at least the size of
		///			#length.
		/// </param>
		///	<param="start">
		///			First index of range. Must be at least 0 and at 
		///			most HAPI_GeoInfo::pointCount - 1.
		/// </param>
		/// <param="length">
		///			Must be at least 0 and at most 
		///			HAPI_GeoInfo::pointCount - #start.  
		/// </param>
		public static void getInstanceTransforms( int asset_id, int object_id, int geo_id,
												  int rstOrder, [Out] HAPI_Transform[] transforms,
												  int start, int length )
		{
			int status_code = HAPI_GetInstanceTransforms( asset_id, object_id, geo_id, rstOrder, transforms, 
														  start, length );
			processStatusCode( (HAPI_StatusCode) status_code );
		}
		
		/// <summary>
		/// 	Commit the current input geometry to the cook engine. Nodes that use this geometry node will 
		/// 	re-cook using the input geometry given through the geometry setter API calls.
		/// </summary>
		/// <param name="asset_id">
		/// 	The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
		/// <param name="object_id">
		/// 	The object id returned by <see cref="HAPI_Host.getObjects"/>.
		/// </param>
		/// <param name="geo_id">
		/// 	The geometry id.
		/// </param>
		public static void commitGeo( int asset_id, int object_id, int geo_id )
		{
			int status_code = HAPI_CommitGeo( asset_id, object_id, geo_id );
			processStatusCode( (HAPI_StatusCode) status_code );
		}
		
		// GEOMETRY INPUT -------------------------------------------------------------------------------------------
		
		/// <summary> 
		/// 	Set a file as input to an asset.
		/// </summary>
		/// <param name="asset_id">
		///		The asset id returned by HAPI_LoadOTLFile().
		/// </param>
		/// <param name="input_idx">
		///		The index of the input - 0 for an asset that has just
		///	</param>		
		/// <param name="file_name">
		///		Name of the file to use as input to the given asset
		/// </param>
		public static void setFileInput( int asset_id, int input_idx, string file_name )
		{
			int status_code = HAPI_SetFileInput( asset_id, input_idx, file_name );
			processStatusCode( (HAPI_StatusCode) status_code );
		}
		
		/// <summary> 
		/// 	Create a geometry input node.
		/// </summary>
		/// <param name="asset_id">
		///		The asset id returned by HAPI_LoadOTLFile().
		/// </param>
		/// <param name="input_idx">
		///		The index of the input - 0 for an asset that has just one input.
		///	</param>
		/// <param name="object_id">
		///		Object id corresponding to created input node. Use this id to push geometry to this input. Note 
		///		that this id will usually be 0 if geometry is piped to a geometry type asset but will could be 
		///		non-zero if geometry is piped to an object type asset.
		/// </param>
		/// <param name="geo_id">
		///		Geometry id corresponding to created input node. Use this id to push geometry to this input.
		/// </param>
		public static void createGeoInput( int asset_id, int input_idx, out int object_id, out int geo_id )
		{
			int status_code = HAPI_CreateGeoInput( asset_id, input_idx, out object_id, out geo_id );
			processStatusCode( (HAPI_StatusCode) status_code );
		}
		
		// Inter Asset ---------------------------------------------------------------------------------------------
		
		/// <summary>	Connect two assets of compatible types together.  For 
		///		example we can connect two object level assets or
		///		two sop level assets together.  Note this method does
		///		not give you the fine grained control over the exact
		///		piece of geometry to connect in the case of SOP assets,
		///		it will connect the entire geometry from 1 SOP to another.
		///		For fine grained control please use HAPI_ConnectAssetGeometry
		///		which allows you to specify groups.
		/// </summary>
		///
		/// <param name="asset_id_from">
		///			The asset id of the source asset
		/// </param>
		/// <param name="asset_id_to">
		///			The asset id of the destination asset
		/// </param>
		/// <param name="input_idx">
		///			The index on the destination asset where the connection
		///			should be made.
		/// </param>		
		
		public static void connectAssetTransform( int asset_id_from, int asset_id_to, int input_idx )
		{
			int status_code = HAPI_ConnectAssetTransform( asset_id_from, asset_id_to, input_idx );
			processStatusCode( (HAPI_StatusCode) status_code );
		}
		
		
		/// <summary> 
		/// 	Break an existing connection on transforms
		/// </summary>
		/// <param name ="asset_id">
		///			The asset id of the asset
		/// </param>
		/// <param name="input_idx">
		///			The index on the asset where the connection
		///			should be broken.
		///	</param>
		
		public static void disconnectAssetTransform( int asset_id, int input_idx )
		{
			int status_code = HAPI_DisconnectAssetTransform( asset_id, input_idx );
			processStatusCode( (HAPI_StatusCode) status_code );
		}

		/// <summary>	Connect the geometry of two assets together.  For 
		///		example we can connect a particular piece of geometry from
		///		an object level asset to a sop level asset or even another
		///		object level asset.  This method gives you the fine grained 
		///		control over the exact piece of geometry to connect by allowing
		///		you to specify the exact object and group of the geometry you
		///		are trying to connect.
		/// </summary>
		/// <param name="asset_id_from">
		///			The asset id of the source asset
		/// </param>
		/// <param name="object_id_from">
		///			The object within the asset that contains the geometry
		///			to send.
		/// </param>
		/// <param name="group_id_from">
		///			The id of the group within the object 
		/// </param>
		/// <param name="asset_id_to">
		///			The asset id of the destination asset
		/// </param>
		/// <param name="input_idx">
		///			The index on the destination asset where the connection
		///			should be made.
		/// </param>		
		public static void connectAssetGeometry( int asset_id_from, int object_id_from,	int group_id_from,
													int asset_id_to, int input_idx )
		{
			int status_code = HAPI_ConnectAssetGeometry( asset_id_from, object_id_from, group_id_from, 
														 asset_id_to, input_idx );
			processStatusCode( (HAPI_StatusCode) status_code );
		}
		
		/// <summary> 
		/// 	Break an existing connection on geometry
		/// </summary>	
		///
		/// <param name ="asset_id">
		///			The asset id of the asset
		/// </param>
		/// <param name="input_idx">
		///			The index on the asset where the connection
		///			should be broken.
		///	</param>
		
		public static void disconnectAssetGeometry( int asset_id, int input_idx )
		{
			int status_code = HAPI_DisconnectAssetGeometry( asset_id, input_idx );
			processStatusCode( (HAPI_StatusCode) status_code );
		}

		// MATERIALS ------------------------------------------------------------------------------------------------

		/// <summary>
		/// 	Fill an array of <see cref="HAPI_MaterialInfo"> structs with information about the materials
		/// 	used and stored in an asset.
		/// </summary>
	    /// <param name="asset_id">
	    ///		The asset id returned by <see cref="HAPI_Host.loadOTLFile"/>.
		/// </param>
	    /// <param name ="material_infos">
	    ///		Array of <see cref="HAPI_MaterialInfo"/> exactly the size of <paramref name="length"/>.
		/// </param>
		/// <param name="start">
		/// 	First index of range. Must be at least 0 and at most <see cref="HAPI_AssetInfo.materialCount"/> - 1.
		/// </param>
		/// <param name="length">
		/// 	Must be at least 0 and at most <see cref="HAPI_AssetInfo.materialCount"/> - <paramref name="start"/>.
		/// </param>
		public static void getMaterials( int asset_id, [Out] HAPI_MaterialInfo[] material_infos,
										 int start, int length )
		{
			int status_code = HAPI_GetMaterials( asset_id, material_infos, start, length );
			processStatusCode( (HAPI_StatusCode) status_code );
		}
	}

}