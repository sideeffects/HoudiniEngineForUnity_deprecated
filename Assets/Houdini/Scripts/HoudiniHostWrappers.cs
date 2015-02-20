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
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

// Typedefs (copy these from HAPI_Common.cs)
using HAPI_StringHandle = System.Int32;
using HAPI_AssetLibraryId = System.Int32;
using HAPI_AssetId = System.Int32;
using HAPI_NodeId = System.Int32;
using HAPI_ParmId = System.Int32;
using HAPI_ObjectId = System.Int32;
using HAPI_GeoId = System.Int32;
using HAPI_PartId = System.Int32;
using HAPI_MaterialId = System.Int32;

public static partial class HoudiniHost
{
	// DIAGNOSTICS ----------------------------------------------------------------------------------------------

	public static HAPI_License getCurrentLicense()
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		int value;
		HAPI_Result status_code = HAPI_GetEnvInt( HAPI_EnvIntType.HAPI_ENVINT_LICENSE, out value );
		processStatusCode( status_code );
		return (HAPI_License) value;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static int getEnvInt( HAPI_EnvIntType int_type )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		int value;
		HAPI_Result status_code = HAPI_GetEnvInt( int_type, out value );
		processStatusCode( status_code );
		return value;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static int getStatus( HAPI_StatusType status_type )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		int status;
		HAPI_Result status_code = HAPI_GetStatus( status_type, out status );
		processStatusCode( status_code );
		return status;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static string getStatusString( HAPI_StatusType status_type, HAPI_StatusVerbosity verbosity )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		int buffer_size = 0;
		HAPI_Result status_code = HAPI_GetStatusStringBufLength( status_type, verbosity, out buffer_size );
		processStatusCode( status_code );

		if ( buffer_size <= 0 )
			return "";

		StringBuilder string_builder = new StringBuilder( buffer_size );
		status_code = HAPI_GetStatusString( status_type, string_builder );
		processStatusCode( status_code );

		string string_value = string_builder.ToString();
			
		return string_value;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Get total number of nodes that need to cook in the current session.
	/// </summary>
	public static int getCookingTotalCount()
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		int count = 0;
		HAPI_Result status_code = HAPI_GetCookingTotalCount( out count );
		processStatusCode( status_code );
		return count;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Get current number of nodes that have already cooked in the current session. Note that this is a 
	/// 	very crude approximation of the cooking progress - it may never make it to 100% or it 
	///		might spend another hour at 100%. Use HAPI_GetStatusString to get a better idea of progress if 
	///		this number gets stuck.
	/// </summary>
	public static int getCookingCurrentCount()
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		int count = 0;
		HAPI_Result status_code = HAPI_GetCookingCurrentCount( out count );
		processStatusCode( status_code );
		return count;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	// UTILITY --------------------------------------------------------------------------------------------------

	/// <summary>
	/// 	Converts a transform to differing TRS order and Euler rotation order.
	/// </summary>
	/// <param name= "transform_in_out">
	/// 	Used for both input and output.
	/// </param>
	/// <param name="rst_order">
	/// 	The desired transform order of the output.
	/// </param>
	///	<param name="rot_order">
	/// 	The desired rotation order of the output.
	/// </param>
	public static void convertTransform(
		ref HAPI_TransformEuler transform_in_out,
		HAPI_RSTOrder rst_order, HAPI_XYZOrder rot_order )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_ConvertTransform( ref transform_in_out, rst_order, rot_order );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Converts a 4x4 matrix into its quaternion TRS form.
	/// </summary>
	/// <param name="mat">
	/// 	A 4x4 matrix expressed in a 16 element float array.
	/// </param>
	/// <param name="rst_order">
	/// 	The desired transform order of the output.
	/// </param>
	/// <return>
	///		The converted <see cref="HAPI_Transform"/>.
	/// </return>
	public static HAPI_Transform convertMatrixToQuat( Matrix4x4 matrix, HAPI_RSTOrder rst_order )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Transform transform = new HAPI_Transform();
		float[] raw_matrix = new float[ 16 ];

		for ( int i = 0; i < 16; ++i )
			raw_matrix[ i ] = matrix[ i ];

		HAPI_Result status_code = HAPI_ConvertMatrixToQuat( raw_matrix, rst_order, ref transform );
		processStatusCode( status_code );

		return transform;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Converts a 4x4 matrix into its euler TRS form.
	/// </summary>
	/// <param name="mat">
	/// 	A 4x4 matrix expressed in a 16 element float array.
	/// </param>
	/// <param name="rst_order">
	/// 	The desired transform order of the output.
	/// </param>
	///	<param name="rot_order">
	/// 	The desired rotation order of the output.
	/// </param>
	/// <return>
	///		The converted <see cref="HAPI_TransformEuler"/>.
	/// </return>
	public static HAPI_TransformEuler convertMatrixToEuler(
		Matrix4x4 matrix, HAPI_RSTOrder rst_order, HAPI_XYZOrder rot_order )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_TransformEuler transform = new HAPI_TransformEuler();
		float[] raw_matrix = new float[ 16 ];

		for ( int i = 0; i < 16; ++i )
			raw_matrix[ i ] = matrix[ i ];

		HAPI_Result status_code = HAPI_ConvertMatrixToEuler(
			raw_matrix, rst_order, rot_order, ref transform );
		processStatusCode( status_code );

		return transform;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	///		Converts a <see cref="HAPI_Transform"/> into a 4x4 matrix.
	/// </summary>
	/// <param name="transform">
	///		The <see cref="HAPI_Transform"/> that you wish to convert.
	/// </param>
	/// <returns>
	///		The converted 4x4 matrix.
	/// </returns>
	public static Matrix4x4 convertTransformQuatToMatrix( HAPI_Transform transform )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		float[] raw_matrix = new float[ 16 ];
		HAPI_Result status_code = HAPI_ConvertTransformQuatToMatrix( ref transform, raw_matrix );
		processStatusCode( status_code );

		Matrix4x4 output = new Matrix4x4();
			
		for ( int i = 0; i < 16; ++i )
			output[ i ] = raw_matrix[ i ];

		return output;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	///		Converts a <see cref="HAPI_TransformEuler"/> into a 4x4 matrix.
	/// </summary>
	/// <param name="transform">
	///		The <see cref="HAPI_TransformEuler"/> that you wish to convert.
	/// </param>
	/// <returns>
	///		The converted 4x4 matrix.
	/// </returns>
	public static Matrix4x4 convertTransformEulerToMatrix( HAPI_TransformEuler transform )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		float[] raw_matrix = new float[ 16 ];
		HAPI_Result status_code = HAPI_ConvertTransformEulerToMatrix( ref transform, raw_matrix );
		processStatusCode( status_code );

		Matrix4x4 output = new Matrix4x4();
			
		for ( int i = 0; i < 16; ++i )
			output[ i ] = raw_matrix[ i ];

		return output;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	// STRINGS --------------------------------------------------------------------------------------------------

	// NOTE: These are private as we want people to use the more abstract methods in HAPI_HostStrings.cs.

	/// <summary>
	/// 	Gives back the string length of the string with the given handle.
	/// </summary>
	/// <param name="string_handle">
	/// 	Handle of the string to query.
	/// </param>
	/// <param name="buffer_length">
	/// 	Buffer length of the queried string (including NULL terminator).
	/// </param>
	private static void getStringBufLength(
		HAPI_StringHandle string_handle, out int buffer_length )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_GetStringBufLength( string_handle, out buffer_length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
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
	/// <param name="buffer_length">
	/// 	Length of the string buffer (must match size of <paramref name="string_value" - 
	/// 	so including NULL terminator).
	/// </param>
	private static void getString(
		HAPI_StringHandle string_handle,
		StringBuilder string_value,
		int buffer_length )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_GetString( string_handle, string_value, buffer_length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	// TIME -----------------------------------------------------------------------------------------------------

	/// <summary>
	/// 	Gets the global time of the scene. All API calls deal with this time to cook.
	/// </summary>
	/// <param name="time">
	/// 	Time as a float in seconds.
	/// </param>
	public static float getTime()
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		float time;
		HAPI_Result status_code = HAPI_GetTime( out time );
		processStatusCode( status_code );
		return time;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Sets the global time of the scene. All API calls will deal with this time to cook.
	/// </summary>
	/// <param name="time">
	/// 	Time as a float in seconds.
	/// </param>
	public static void setTime( float time )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_SetTime( time );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static HAPI_TimelineOptions getTimelineOptions()
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_TimelineOptions timeline_options = new HAPI_TimelineOptions();
		HAPI_Result status_code = HAPI_GetTimelineOptions( ref timeline_options );
		processStatusCode( status_code );
		return timeline_options;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void setTimelineOptions( HAPI_TimelineOptions timeline_options )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_SetTimelineOptions( ref timeline_options );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	// ASSETS -------------------------------------------------------------------------------------------------------

	/// <summary>
	/// 	Determine if your instance of the asset actually still exists inside the Houdini scene. 
	/// 	This is what can be used to determine when the Houdini scene needs to be re-populated 
	/// 	using the host application's instances of the assets.
	/// </summary>
	/// <param name="asset_id">
	/// 	The asset id.
	/// </param>
	/// <param name="asset_validation_id">
	///		The asset validation id that's found in the <see cref="HAPI_AssetInfo"/> struct 
	///		returned by <see cref="HAPI_GetAssetInfo"/>.
	/// </param>
	public static bool isAssetValid( HAPI_AssetId asset_id, int asset_validation_id )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		if ( !isInstallationOk() )
			return false;

		int answer = 0;
			
		// No need to process return code because this function is guaranteed to 
		// always return HAPI_STATUS_SUCCESS.
		HAPI_IsAssetValid( asset_id, asset_validation_id, out answer );
			
		if ( answer > 0 )
			return true;
		else
			return false;
#else
		return false;
#endif
	}

	public static HAPI_AssetId instantiateAsset( string asset_name, bool cook_on_load )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_AssetId asset_id = 0;
		HAPI_Result status_code = HAPI_InstantiateAsset( asset_name, cook_on_load, out asset_id );
		processStatusCode( status_code );
		return asset_id;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Get an asset_info struct.
	/// </summary>
	/// <param name="asset_id">
	/// 	The asset id.
	/// </param>
	public static HAPI_AssetInfo getAssetInfo( HAPI_AssetId asset_id )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_AssetInfo asset_info = new HAPI_AssetInfo();
		HAPI_Result status_code = HAPI_GetAssetInfo( asset_id, ref asset_info );
		processStatusCode( status_code );
		return asset_info;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void cookAsset( HAPI_AssetId asset_id, bool split_geos_by_group )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_CookOptions cook_options = new HAPI_CookOptions();
		cook_options.splitGeosByGroup = split_geos_by_group;
		cook_options.maxVerticesPerPrimitive = HoudiniConstants.HAPI_MAX_VERTICES_PER_FACE;
		cook_options.refineCurveToLinear = HoudiniConstants.HAPI_CURVE_REFINE_TO_LINEAR;
		cook_options.curveRefineLOD = HoudiniConstants.HAPI_CURVE_LOD;
		HAPI_Result status_code = HAPI_CookAsset( asset_id, ref cook_options );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	///		Interrupt a cook or load operation.
	/// </summary>
	public static void interrupt()
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_Interrupt();
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Get the transform of an asset to match the transform of the asset on the client side.
	/// </summary>
	/// <param name="asset_id">
	/// 	The asset id.
	/// </param>
	/// <param name="rst_order">
	/// 	The order of application of translation, rotation and scale.
	/// </param>
	///	<param name="rot_order">
	/// 	The desired rotation order of the output.
	/// </param>
	/// <param name="transform">
	/// 	The actual transform struct.
	/// </param>
	public static void getAssetTransform(
		HAPI_AssetId asset_id, HAPI_RSTOrder rst_order, HAPI_XYZOrder rot_order,
		out HAPI_TransformEuler transform )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_GetAssetTransform( asset_id, rst_order, rot_order, out transform );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Set the transform of an asset to match the transform of the asset on the client side.
	/// </summary>
	/// <param name="asset_id">
	/// 	The asset id.
	/// </param>
	/// <param name="transform">
	/// 	The actual transform struct.
	/// </param>
	public static void setAssetTransform(
		HAPI_AssetId asset_id, ref HAPI_TransformEuler transform )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_SetAssetTransform( asset_id, ref transform );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Get the name of an asset's input.
	/// </summary>
	/// <param name="asset_id">
	/// 	The asset id.
	/// </param>
	/// <param name="input_idx">
	/// 	Input index of the asset.
	/// </param>
	/// <param name="input_type">
	/// 	Input type (<see cref="HAPI_InputType"/>).
	/// </param>
	/// <returns>
	///		The input name.
	/// </returns>
	public static string getInputName(
		HAPI_AssetId asset_id, int input_idx, HAPI_InputType input_type )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_StringHandle name_sh = 0;
		HAPI_Result status_code = HAPI_GetInputName( asset_id, input_idx, (int) input_type, out name_sh );
		processStatusCode( status_code );
		string name = getString( name_sh );
		return name;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	// NODES ----------------------------------------------------------------------------------------------------

	/// <summary>
	/// 	Get an node info struct.
	/// </summary>
	/// <param name="node_id">
	/// 	The node id given by either a <see cref="HAPI_AssetInfo"/> struct or 
	/// 	a <see cref="HAPI_MaterialInfo"/> struct.
	/// </param>
	public static HAPI_NodeInfo getNodeInfo( HAPI_NodeId node_id )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_NodeInfo node_info = new HAPI_NodeInfo();
		HAPI_Result status_code = HAPI_GetNodeInfo( node_id, ref node_info );
		processStatusCode( status_code );
		return node_info;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Get an global nodes struct.
	/// </summary>
	/// <returns>
	///		The struct containing the global nodes by node id.
	/// </returns>
	public static HAPI_GlobalNodes getGlobalNodes()
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_GlobalNodes global_nodes = new HAPI_GlobalNodes();
		HAPI_Result status_code = HAPI_GetGlobalNodes( out global_nodes );
		processStatusCode( status_code );
		return global_nodes;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	// PARAMETERS -----------------------------------------------------------------------------------------------

	/// <summary>
	/// 	Fill an array of HAPI_ParmInfo structs with parameter information from the asset instance node.
	/// </summary>
	/// <param name="node_id">
	/// 	The node id.
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
	public static void getParameters(
		HAPI_NodeId node_id, [Out] HAPI_ParmInfo[] parm_infos,
		int start, int length )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_GetParameters( node_id, parm_infos, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	All parameter APIs require a HAPI_ParmId but if you know the parameter you wish
	/// 	to operate on by name than you can use this function to get its HAPI_ParmId.
	/// 	If the parameter with the given name is not found the parameter id returned
	/// 	will be -1.
	/// </summary>
	/// <param name="node_id">
	/// 	The node id.
	/// </param>
	/// <param name="name">
	/// 	The name of the parameter you wish to get the id for.
	/// </param>
	/// <returns>
	///		The parameter id. -1 if parameter not found.
	/// </returns>
	public static HAPI_ParmId getParmIdFromName( HAPI_NodeId node_id, string name )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_ParmId parm_id = -1;
		HAPI_Result status_code = HAPI_GetParmIdFromName( node_id, name, out parm_id );
		processStatusCode( status_code );
		return parm_id;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Fill an array of parameter int values.
	/// </summary>
	/// <param name="node_id">
	/// 	The node id given by either a <see cref="HAPI_AssetInfo"/> struct or 
	/// 	a <see cref="HAPI_MaterialInfo"/> struct.
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
	public static void getParmIntValues(
		HAPI_NodeId node_id, [Out] int[] values,
		int start, int length )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_GetParmIntValues( node_id, values, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Fill an array of parameter float values.
	/// </summary>
	/// <param name="node_id">
	/// 	The node id given by either a <see cref="HAPI_AssetInfo"/> struct or 
	/// 	a <see cref="HAPI_MaterialInfo"/> struct.
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
	public static void getParmFloatValues(
		HAPI_NodeId node_id, [Out] float[] values,
		int start, int length )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_GetParmFloatValues( node_id, values, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Fill an array of parameter string handles. These handles must be used in conjunction with
	/// 	<see cref="HAPI_GetString"> to get the actual string values.
	/// </summary>
	/// <param name="node_id">
	/// 	The node id given by either a <see cref="HAPI_AssetInfo"/> struct or 
	/// 	a <see cref="HAPI_MaterialInfo"/> struct.
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
	public static void getParmStringValues(
		HAPI_NodeId node_id, [Out] HAPI_StringHandle[] values,
		int start, int length )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		// Can't turn this off yet. More testing is needed, especially for file paths.
		const bool evaluate = true;

		HAPI_Result status_code = HAPI_GetParmStringValues( node_id, evaluate, values, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Fill an array of <see cref="HAPI_ParmChoiceInfo"/> structs with parameter choice list information
	/// 	from the asset instance node.
	/// </summary>
	/// <param name="node_id">
	/// 	The node id given by either a <see cref="HAPI_AssetInfo"/> struct or 
	/// 	a <see cref="HAPI_MaterialInfo"/> struct.
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
	public static void getParmChoiceLists(
		HAPI_NodeId node_id, [Out] HAPI_ParmChoiceInfo[] parm_choices,
		int start, int length )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_GetParmChoiceLists( node_id, parm_choices, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Set (push) an array of parameter int values.
	/// </summary>
	/// <param name="node_id">
	/// 	The node id given by either a <see cref="HAPI_AssetInfo"/> struct or 
	/// 	a <see cref="HAPI_MaterialInfo"/> struct.
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
	public static void setParmIntValues(
		HAPI_NodeId node_id, int[] values,
		int start, int length )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_SetParmIntValues( node_id, values, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Set (push) an array of parameter float values.
	/// </summary>
	/// <param name="node_id">
	/// 	The node id given by either a <see cref="HAPI_AssetInfo"/> struct or 
	/// 	a <see cref="HAPI_MaterialInfo"/> struct.
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
	public static void setParmFloatValues(
		HAPI_NodeId node_id, float[] values,
		int start, int length )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_SetParmFloatValues( node_id, values, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Set (push) a string value. We can only set a single value at a time because we want to avoid
	/// 	fixed size string buffers.
	/// </summary>
	/// <param name="node_id">
	/// 	The node id given by either a <see cref="HAPI_AssetInfo"/> struct or 
	/// 	a <see cref="HAPI_MaterialInfo"/> struct.
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
	public static void setParmStringValue(
		HAPI_NodeId node_id, string value,
		HAPI_ParmId parm_id, int index )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_SetParmStringValue( node_id, value, parm_id, index );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Insert an instance of a multiparm after instance_position.
	/// </summary>
	///
	/// <param name="node_id">
	/// 	The node id given by either a HAPI_AssetInfo
	/// 	struct or a HAPI_MaterialInfo struct
	/// </param>
	/// <param name="parm_id">
	/// 	A parm id given by a HAPI_ParmInfo struct that
	/// 	has type HAPI_PARMTYPE_MULTIPARMLIST
	/// </param>
	/// <param name="instance_position">
	/// 	The new instance will be inserted one after this
	/// 	instance_position. The first instance has position 1.
	/// </param>
	public static void insertMultiparmInstance(
		HAPI_NodeId node_id, HAPI_ParmId parm_id,
		int instance_position )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_InsertMultiparmInstance( node_id, parm_id, instance_position );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Remove the instance of a multiparm given by instance_position.
	/// </summary>
	///
	/// <param name="node_id">
	/// 	The node id given by either a HAPI_AssetInfo
	/// 	struct or a HAPI_MaterialInfo struct
	/// </param>
	/// <param name="parm_id">
	/// 	A parm id given by a HAPI_ParmInfo struct that
	/// 	has type HAPI_PARMTYPE_MULTIPARMLIST
	/// </param>
	/// <param name="instance_position">
	/// 	The new instance will be inserted one after this
	/// 	instance_position. The first instance has position 1.
	/// </param>
	public static void removeMultiparmInstance(
		HAPI_NodeId node_id, HAPI_ParmId parm_id,
		int instance_position )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_RemoveMultiparmInstance( node_id, parm_id, instance_position );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}
		
	// HANDLES --------------------------------------------------------------------------------------------------

	/// <summary>
	/// 	Fill an array of <see cref="HAPI_HandleInfo"/> structs with information about every exposed
	/// 	user manipulation handle on the asset.
	/// </summary>
	/// <param name="asset_id">
	/// 	The asset id.
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
	public static void getHandleInfo(
		HAPI_AssetId asset_id,
		[Out] HAPI_HandleInfo[] handle_infos,
		int start, int length )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_GetHandleInfo( asset_id, handle_infos, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Fill an array of <see cref="HAPI_HandleBindingInfo"/> structs with information about how each
	/// 	handle parameter maps to each asset parameter.
	/// </summary>
	/// <param name="asset_id">
	/// 	The asset id.
	/// </param>
	/// <param name="handle_index">
	/// 	The index of the handle, from 0 to handleCount - 1 from the call to <see cref="HoudiniHost.loadOTLFile"/>
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
	public static void getHandleBindingInfo(
		HAPI_AssetId asset_id,
		int handle_index,
		[Out] HAPI_HandleBindingInfo[] handle_infos,
		int start, int length )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_GetHandleBindingInfo( asset_id, handle_index, handle_infos, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	// PRESETS --------------------------------------------------------------------------------------------------
		
	/// <summary>
	/// 	Generates a preset for the given asset.
	/// </summary>
	/// <param name="node_id">
	/// 	The node id given by either a <see cref="HAPI_AssetInfo"/> struct or 
	/// 	a <see cref="HAPI_MaterialInfo"/> struct.
	/// </param>
	/// <returns>
	///		The preset byte string.
	/// </returns>
	public static byte[] getPreset( HAPI_NodeId node_id )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		int buffer_length = 0;
		HAPI_Result status_code = HAPI_GetPresetBufLength(
			node_id, HAPI_PresetType.HAPI_PRESETTYPE_BINARY, null, ref buffer_length );
		processStatusCode( status_code );

		byte[] preset = new byte[ buffer_length ];
		status_code = HAPI_GetPreset( node_id, preset, buffer_length );
		processStatusCode( status_code );

		return preset;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Sets a particular asset to a given preset.
	/// </summary>
	/// <param name="node_id">
	/// 	The node id given by either a <see cref="HAPI_AssetInfo"/> struct or 
	/// 	a <see cref="HAPI_MaterialInfo"/> struct.
	/// </param>
	/// <param name="preset">
	/// 	Buffer to hold the preset data.
	/// </param>
	public static void setPreset( HAPI_NodeId node_id, byte[] preset )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_SetPreset(
			node_id, HAPI_PresetType.HAPI_PRESETTYPE_BINARY, null, preset, preset.Length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	// OBJECTS --------------------------------------------------------------------------------------------------

	/// <summary>
	/// 	Fill an array of <see cref="HAPI_ObjectInfo"/> structs with information on each visible object
	/// 	in the scene that has a SOP network (is not a sub-network).
	/// 	Note that this function will reset all the objects' <see cref="HAPI_ObjectInfo.haveGeosChanged"/>
	/// 	flags to false after it returns the original flag values.
	/// </summary>
	/// <param name="asset_id">
	/// 	The asset id.
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
	public static void getObjects(
		HAPI_AssetId asset_id,
		[Out] HAPI_ObjectInfo[] object_infos,
		int start, int length )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_GetObjects( asset_id, object_infos, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Fill an array of <see cref="HAPI_ObjectInfo"/> structs with information on each visible object
	/// 	in the scene that has a SOP network (is not a sub-network).
	/// 	Note that this function will reset all the objects' <see cref="HAPI_ObjectInfo.hasTransformChanged"/>
	/// 	flags to false after it returns the original flag values.
	/// </summary>
	/// <param name="asset_id">
	/// 	The asset id.
	/// </param>
	/// <param name="rst_order">
	/// 	The order of application of translation, rotation and scale.
	/// </param>
	/// <param name="transforms">
	/// 	Array of <see cref="HAPI_Transform"/> at least the size of
	/// 	<paramref name="length"/>. The <see cref="HAPI_Transform.id"/> of each will be
	/// 	set to the object id as given by <see cref="HoudiniHost.HAPI_GetObjects"/>.
	/// </param>
	/// <param name="start">
	/// 	First index of range. Must be at least 0 and at most <see cref="HAPI_AssetInfo.objectCount"/> - 1.
	/// </param>
	/// <param name="length">
	/// 	Must be at least 0 and at most <see cref="HAPI_AssetInfo.objectCount"/> - <paramref name="start"/>.
	/// </param>
	public static void getObjectTransforms(
		HAPI_AssetId asset_id,
		HAPI_RSTOrder rst_order,
		[Out] HAPI_Transform[] transforms,
		int start, int length )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_GetObjectTransforms( asset_id, rst_order, transforms, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
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
	///			The order of application of translation, rotation and scale.
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
	public static void getInstanceTransforms(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id,
		HAPI_RSTOrder rst_order, [Out] HAPI_Transform[] transforms,
		int start, int length )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_GetInstanceTransforms(
			asset_id, object_id, geo_id, rst_order, transforms, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Set the transform of an individual object. This is mostly used with marshaled 
	/// 	geometry objects. Trying to modify the transform of an object belonging to an 
	/// 	asset other than the special External Input Asset with object id 0 will most
	/// 	likely fail, unless the transforms are exposed as editable via exposed parameters.
	/// </summary>
	/// <param name="asset_id">
	/// 	The asset id.
	/// </param>
	/// <param name="object_id">
	/// 	The object id returned by <see cref="HoudiniHost.HAPI_GetObjects"/>.
	/// </param>
	/// <param name="transform">
	/// 	A <see cref="HAPI_TransformEuler"/> that stores the transform.
	/// </param>
	public static void setObjectTransform(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id,
		ref HAPI_TransformEuler transform )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_SetObjectTransform( asset_id, object_id, ref transform );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}
		
	// GEOMETRY GETTERS -----------------------------------------------------------------------------------------

	/// <summary>
	/// 	Get the main geometry info struct (<see cref="HAPI_GeoInfo"/>).
	/// 	Note that this function will reset all the geos' <see cref="HAPI_GeoInfo.hasGeoChanged"/>
	/// 	flags to false after it returns the original flag values.
	/// </summary>
	/// <param name="asset_id">
	/// 	The asset id.
	/// </param>
	/// <param name="object_id">
	/// 	The object id.
	/// </param>
	/// <param name="geo_id">
	/// 	The geometry id.
	/// </param>
	/// <param name="geo_info">
	/// 	<see cref="HAPI_GeoInfo"/> out parameter.
	/// </param>
	public static void getGeoInfo(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id,
		out HAPI_GeoInfo geo_info )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_GetGeoInfo( asset_id, object_id, geo_id, out geo_info );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Get the part info struct (<see cref="HAPI_PartInfo"/>).
	/// </summary>
	/// <param name="asset_id">
	/// 	The asset id.
	/// </param>
	/// <param name="object_id">
	/// 	The object id.
	/// </param>
	/// <param name="geo_id">
	/// 	The geometry id.
	/// </param>
	/// <param name="part_id">
	/// 	The part id.
	/// </param>
	/// <param name="part_info">
	/// 	<see cref="HAPI_PartInfo"/> out parameter.
	/// </param>
	public static void getPartInfo(	HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id, HAPI_PartId part_id,
									out HAPI_PartInfo part_info )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_GetPartInfo( asset_id, object_id, geo_id, part_id, out part_info );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Get the array of faces where the nth integer in the array is the number of vertices
	/// 	the nth face has.
	/// </summary>
	/// <param name="asset_id">
	/// 	The asset id.
	/// </param>
	/// <param name="object_id">
	/// 	The object id.
	/// </param>
	/// <param name="geo_id">
	/// 	The geometry id.
	/// </param>
	/// <param name="part_id">
	/// 	The part id.
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
	public static void getFaceCounts(		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id, HAPI_PartId part_id,
											[Out] int[] face_counts,
											int start, int length )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_GetFaceCounts( asset_id, object_id, geo_id, part_id, face_counts, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Get array containing the vertex-point associations where the ith element in the array is
	/// 	the point index the ith vertex associates with.
	/// </summary>
	/// <param name="asset_id">
	/// 	The asset id.
	/// </param>
	/// <param name="object_id">
	/// 	The object id.
	/// </param>
	/// <param name="geo_id">
	/// 	The geometry id.
	/// </param>
	/// <param name="part_id">
	/// 	The part id.
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
	public static void getVertexList(		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id, HAPI_PartId part_id,
											[Out] int[] vertex_list,
											int start, int length )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_GetVertexList( asset_id, object_id, geo_id, part_id, vertex_list, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Get attribute information; fill a <see cref="HAPI_AttributeInfo"/>.
	/// </summary>
	/// <param name="asset_id">
	/// 	The asset id.
	/// </param>
	/// <param name="object_id">
	/// 	The object id.
	/// </param>
	/// <param name="geo_id">
	/// 	The geometry id.
	/// </param>
	/// <param name="part_id">
	/// 	The part id.
	/// </param>
	/// <param name="name">
	/// 	Attribute name.
	/// </param>
	/// <param name="owner">
	/// 	Attribute owner.
	/// </param>
	/// <returns>
	/// 	A filled <see cref="HAPI_AttributeInfo"/>. Check HAPI_AttributeInfo.exists to see if 
	/// 	this attribute exists.
	/// </returns>
	public static HAPI_AttributeInfo getAttributeInfo(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id, HAPI_PartId part_id, string name, HAPI_AttributeOwner owner )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_AttributeInfo info = new HAPI_AttributeInfo();
		HAPI_Result status_code = HAPI_GetAttributeInfo( asset_id, object_id, geo_id, part_id, name, owner, ref info );
		processStatusCode( status_code );
		return info;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Get list of attribute names by attribute owner.
	/// </summary>
	/// <param name="asset_id">
	/// 	The asset id.
	/// </param>
	/// <param name="object_id">
	/// 	The object id.
	/// </param>
	/// <param name="geo_id">
	/// 	The geometry id.
	/// </param>
	/// <param name="part_id">
	/// 	The part id.
	/// </param>
	/// <param name="owner"/>
	/// 	The <see cref="HAPI_AttributeOwner"/> enum value specifying the owner of the attribute.
	/// </param>
	/// <returns>
	/// 	List of attribute names.
	/// </returns>
	public static string[] getAttributeNames(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id, HAPI_PartId part_id,
		HAPI_AttributeOwner owner )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_PartInfo part_info = new HAPI_PartInfo();
		HAPI_Result status_code = HAPI_GetPartInfo( asset_id, object_id, geo_id, part_id, out part_info );
		processStatusCode( status_code );

		int count = part_info.getAttributeCountByOwner( owner );

		int[] names = new int[ count ];
		status_code = HAPI_GetAttributeNames(
			asset_id, object_id, geo_id, part_id, owner, names, count );
		processStatusCode( status_code );

		string[] name_strings = new string[ count ];
		for ( int i = 0; i < count; ++i )
			name_strings[ i ] = getString( names[ i ] );

		return name_strings;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Get attribute integer data.
	/// </summary>
	/// <param name="asset_id">
	/// 	The asset id.
	/// </param>
	/// <param name="object_id">
	/// 	The object id.
	/// </param>
	/// <param name="geo_id">
	/// 	The geometry id.
	/// </param>
	/// <param name="part_id">
	/// 	The part id.
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
	public static void getAttributeIntData(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id, HAPI_PartId part_id, string name,
		ref HAPI_AttributeInfo attr_info,
		[Out] int[] data,
		int start, int length )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_GetAttributeIntData( asset_id, object_id, geo_id, part_id, name, 
													ref attr_info, data, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Get attribute float data.
	/// </summary>
	/// <param name="asset_id">
	/// 	The asset id.
	/// </param>
	/// <param name="object_id">
	/// 	The object id.
	/// </param>
	/// <param name="geo_id">
	/// 	The geometry id.
	/// </param>
	/// <param name="part_id">
	/// 	The part id.
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
	public static void getAttributeFloatData(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id, HAPI_PartId part_id, string name,
		ref HAPI_AttributeInfo attr_info,
		[Out] float[] data,
		int start, int length )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_GetAttributeFloatData(
			asset_id, object_id, geo_id, part_id, name, ref attr_info, data, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Get attribute string data. Note that the string handles returned are only valid until the next 
	/// 	time this function is called.
	/// </summary>
	/// <param name="asset_id">
	/// 	The asset id.
	/// </param>
	/// <param name="object_id">
	/// 	The object id.
	/// </param>
	/// <param name="geo_id">
	/// 	The geometry id.
	/// </param>
	/// <param name="part_id">
	/// 	The part id.
	/// </param>
	/// <param name="name">
	/// 	Attribute name.
	/// </param>
	/// <param name="attr_info">
	/// 	<see cref="HAPI_AttributeInfo"/> used as input for tuple size. Also contains some sanity checks 
	/// 	like data type. Generally should be the same struct returned by 
	/// 	<see cref="HoudiniHost.getAttributeInfo"/>.
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
	public static void getAttributeStringData(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id, HAPI_PartId part_id, string name,
		ref HAPI_AttributeInfo attr_info,
		[Out] int[] data,
		int start, int length )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_GetAttributeStringData(
			asset_id, object_id, geo_id, part_id, name, ref attr_info, data, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Get list of group names by group type.
	/// </summary>
	/// <param name="asset_id">
	/// 	The asset id.
	/// </param>
	/// <param name="object_id">
	/// 	The object id.
	/// </param>
	/// <param name="geo_id">
	/// 	The geometry id.
	/// </param>
	/// <param name="group_type">
	/// 	The group type.
	/// </param>
	/// <returns>
	/// 	List of group names.
	/// </returns>
	public static string[] getGroupNames(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id,
		HAPI_GroupType group_type )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_GeoInfo geo_info = new HAPI_GeoInfo();
		HAPI_Result status_code = HAPI_GetGeoInfo( asset_id, object_id, geo_id, out geo_info );
		processStatusCode( status_code );

		int count = geo_info.getGroupCountByType( group_type );

		int[] names = new int[ count ];
		status_code = HAPI_GetGroupNames(
			asset_id, object_id, geo_id, group_type, names, count );
		processStatusCode( status_code );

		string[] name_strings = new string[ count ];
		for ( int i = 0; i < count; ++i )
			name_strings[ i ] = getString( names[ i ] );

		return name_strings;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Get group membership.
	/// </summary>
	/// <param name="asset_id">
	/// 	The asset id.
	/// </param>
	/// <param name="object_id">
	/// 	The object id.
	/// </param>
	/// <param name="geo_id">
	/// 	The geometry id.
	/// </param>
	/// <param name="part_id">
	/// 	The part id.
	/// </param>
	/// <param name="group_type">
	/// 	The group type.
	/// </param>
	/// <returns>
	/// 	List of group names.
	/// </returns>
	public static bool[] getGroupMembership(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id, HAPI_PartId part_id,
		HAPI_GroupType group_type,
		string group_name )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_PartInfo part_info = new HAPI_PartInfo();
		HAPI_Result status_code = HAPI_GetPartInfo( asset_id, object_id, geo_id, part_id, out part_info );
		processStatusCode( status_code );

		int count = part_info.getElementCountByGroupType( group_type );

		int[] membership = new int[ count ];

		if ( count > 0 )
		{
			status_code = HAPI_GetGroupMembership(
				asset_id, object_id, geo_id, part_id, group_type, group_name, membership, 0, count );
			processStatusCode( status_code );
		}

		bool[] membership_bools = new bool[ count ];
		for ( int i = 0; i < count; ++i )
			membership_bools[ i ] = membership[ i ] > 0;

		return membership_bools;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	// GEOMETRY SETTERS -----------------------------------------------------------------------------------------

	/// <summary>
	/// 	Set the main geometry info struct (<see cref="HAPI_GeoInfo"/>).
	/// </summary>
	/// <param name="asset_id">
	/// 	The asset id.
	/// </param>
	/// <param name="object_id">
	/// 	The object id.
	/// </param>
	/// <param name="geo_id">
	/// 	The geometry id.
	/// </param>
	/// <param name="geo_info">
	/// 	<see cref="HAPI_GeoInfo"/> out parameter.
	/// </param>
	public static void setGeoInfo(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id,
		ref HAPI_GeoInfo geo_info )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_SetGeoInfo( asset_id, object_id, geo_id, ref geo_info );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Set the main part info struct (<see cref="HAPI_PartInfo"/>).
	/// </summary>
	/// <param name="asset_id">
	/// 	The asset id.
	/// </param>
	/// <param name="object_id">
	/// 	The object id.
	/// </param>
	/// <param name="geo_id">
	/// 	The geometry id.
	/// </param>
	/// <param name="part_info">
	/// 	<see cref="HAPI_PartInfo"/> out parameter.
	/// </param>
	public static void setPartInfo(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id,
		ref HAPI_PartInfo part_info )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_SetPartInfo( asset_id, object_id, geo_id, ref part_info );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Set the array of faces where the nth integer in the array is the number of vertices
	/// 	the nth face has.
	/// </summary>
	/// <param name="asset_id">
	/// 	The asset id.
	/// </param>
	/// <param name="object_id">
	/// 	The object id.
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
	public static void setFaceCounts(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id,
		int[] face_counts,
		int start, int length )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_SetFaceCounts( asset_id, object_id, geo_id, face_counts, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Set array containing the vertex-point associations where the ith element in the array is
	/// 	the point index the ith vertex associates with.
	/// </summary>
	/// <param name="asset_id">
	/// 	The asset id.
	/// </param>
	/// <param name="object_id">
	/// 	The object id.
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
	public static void setVertexList(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id,
		int[] vertex_list,
		int start, int length )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_SetVertexList( asset_id, object_id, geo_id, vertex_list, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Add an attribute.
	/// </summary>
	/// <param name="asset_id">
	/// 	The asset id.
	/// </param>
	/// <param name="object_id">
	/// 	The object id.
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
	public static void addAttribute(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id, string name,
		ref HAPI_AttributeInfo attr_info )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_AddAttribute( asset_id, object_id, geo_id, name, ref attr_info );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Set attribute integer data.
	/// </summary>
	/// <param name="asset_id">
	/// 	The asset id.
	/// </param>
	/// <param name="object_id">
	/// 	The object id.
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
	public static void setAttributeIntData(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id, string name,
		ref HAPI_AttributeInfo attr_info,
		int[] data,
		int start, int length )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_SetAttributeIntData(
			asset_id, object_id, geo_id, name, ref attr_info, data, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Set attribute float data.
	/// </summary>
	/// <param name="asset_id">
	/// 	The asset id.
	/// </param>
	/// <param name="object_id">
	/// 	The object id.
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
	public static void setAttributeFloatData(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id, string name,
		ref HAPI_AttributeInfo attr_info,
		float[] data,
		int start, int length )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_SetAttributeFloatData(
			asset_id, object_id, geo_id, name, ref attr_info, data, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Set attribute string data.
	/// </summary>
	/// <param name="asset_id">
	/// 	The asset id.
	/// </param>
	/// <param name="object_id">
	/// 	The object id.
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
	/// 	A strings array at least the size of #length.
	/// </param>
	/// <param name="start">
	/// 	First index of range. Must be at least 0 and at most <see cref="HAPI_AttributeInfo.count"/> - 1.
	/// </param>
	/// <param name="length">
	/// 	Must be at least 0 and at most <see cref="HAPI_AttributeInfo.count"/> - <paramref name="start"/>.
	/// </param>
	public static void setAttributeStringData(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id, string name,
		ref HAPI_AttributeInfo attr_info,
		string[] data,
		int start, int length )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_SetAttributeStringData(
			asset_id, object_id, geo_id, name, ref attr_info, data, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Add a group.
	/// </summary>
	/// <param name="asset_id">
	/// 	The asset id.
	/// </param>
	/// <param name="object_id">
	/// 	The object id.
	/// </param>
	/// <param name="geo_id">
	/// 	The geometry id.
	/// </param>
	/// <param name="group_type">
	/// 	The group type.
	/// </param>
	/// <param name="group_name">
	/// 	The group name.
	/// </param>
	public static void addGroup(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id,
		HAPI_GroupType group_type, string group_name )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_AddGroup(
			asset_id, object_id, geo_id, group_type, group_name );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Set group membership.
	/// </summary>
	/// <param name="asset_id">
	/// 	The asset id.
	/// </param>
	/// <param name="object_id">
	/// 	The object id.
	/// </param>
	/// <param name="geo_id">
	/// 	The geometry id.
	/// </param>
	/// <param name="part_id">
	/// 	The part id.
	/// </param>
	/// <param name="group_type">
	/// 	The group type.
	/// </param>
	/// <param name="group_name">
	/// 	The group name.
	/// </param>
	/// <param name="membership">
	/// 	The group membership.
	/// </param>
	/// <param name="count">
	/// 	The group owner element count.
	/// </param>
	public static void setGroupMembership(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id,
		HAPI_GroupType group_type,
		string group_name,
		bool[] membership,
		int count )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		if ( count != membership.Length )
			throw new HoudiniErrorInvalidArgument( "Membership array not same size as count argument!" );

		int[] membership_int = new int[ count ];
		for ( int i = 0; i < count; ++i )
			membership_int[ i ] = membership[ i ] ? 1 : 0;

		HAPI_Result status_code = HAPI_SetGroupMembership(
			asset_id, object_id, geo_id, group_type, group_name, membership_int, 0, count );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Commit the current input geometry to the cook engine. Nodes that use this geometry node will 
	/// 	re-cook using the input geometry given through the geometry setter API calls.
	/// </summary>
	/// <param name="asset_id">
	/// 	The asset id.
	/// </param>
	/// <param name="object_id">
	/// 	The object id.
	/// </param>
	/// <param name="geo_id">
	/// 	The geometry id.
	/// </param>
	public static void commitGeo( HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_CommitGeo( asset_id, object_id, geo_id );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}
		
	/// <summary>	
	/// 	Remove all changes that have been committed to this
	///		geometry.  Only applies to geometry nodes that are
	///		exposed edit nodes.
	/// </summary>
	/// <param name="asset_id">
	///			The asset id returned by HAPI_LoadOTLFile().
	/// </param>
	/// <param name="object_id">
	///			The object id returned by HAPI_GetObjects().
	/// </param>
	/// <param name="geo_id">
	///			The geometry id.
	/// </param>
	/// 
	public static void revertGeo( HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_RevertGeo( asset_id, object_id, geo_id );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	// INTER-ASSET ----------------------------------------------------------------------------------------------

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
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_ConnectAssetTransform( asset_id_from, asset_id_to, input_idx );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
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
		
	public static void disconnectAssetTransform( HAPI_AssetId asset_id, int input_idx )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_DisconnectAssetTransform( asset_id, input_idx );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
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
	/// <param name="asset_id_to">
	///			The asset id of the destination asset
	/// </param>
	/// <param name="input_idx">
	///			The index on the destination asset where the connection
	///			should be made.
	/// </param>		
	public static void connectAssetGeometry(
		HAPI_AssetId asset_id_from, HAPI_ObjectId object_id_from,
		HAPI_AssetId asset_id_to, int input_idx )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_ConnectAssetGeometry(
			asset_id_from, object_id_from, asset_id_to, input_idx );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
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
	public static void disconnectAssetGeometry( HAPI_AssetId asset_id, int input_idx )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_DisconnectAssetGeometry( asset_id, input_idx );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	// MATERIALS ------------------------------------------------------------------------------------------------

	public static HAPI_MaterialInfo getMaterialOnPart(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id, HAPI_PartId part_id )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_MaterialInfo material_info = new HAPI_MaterialInfo();
		HAPI_Result status_code = HAPI_GetMaterialOnPart(
			asset_id, object_id, geo_id, part_id, out material_info );
		processStatusCode( status_code );
		return material_info;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static HAPI_MaterialInfo getMaterialOnGroup(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id, string group_name )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_MaterialInfo material_info = new HAPI_MaterialInfo();
		HAPI_Result status_code = HAPI_GetMaterialOnGroup(
			asset_id, object_id, geo_id, group_name, out material_info );
		processStatusCode( status_code );
		return material_info;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Render the entire material to an image for later extraction. This process will use the 
	/// 	shader specified to render the object assigned with the material you specified (by id) in
	///		UV space, flattening the material into an image that can be later mapped back onto the object.
	/// </summary>
	/// <param name="asset_id">
	///		The asset id.
	/// </param>
	/// <param name="material_id">
	///		The material id from a <see cref="HAPI_PartInfo"/> struct.
	/// </param>
	/// <param name="shader_type">
	///		The shader that will be used to bake this material.
	/// </param>
	public static void renderMaterialToImage( HAPI_AssetId asset_id, HAPI_MaterialId material_id, HAPI_ShaderType shader_type )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_RenderMaterialToImage( asset_id, material_id, shader_type );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Render only a single texture to an image for later extraction. An example use of this method 
	/// 	might be to render the diffuse, normal, and bump texture maps of a material to individual
	///		texture files for use within the client application.
	/// </summary>
	/// <param name="asset_id">
	///		The asset id.
	/// </param>
	/// <param name="material_id">
	///		The material id from a <see cref="HAPI_PartInfo"/> struct.
	/// </param>
	/// <param name="parm_id">
	///		This is the index in the parameter list of the material_id's node of the parameter 
	///		containing the texture map file path.
	/// </param>
	public static void renderTextureToImage( HAPI_AssetId asset_id, HAPI_MaterialId material_id, HAPI_ParmId parm_id )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_RenderTextureToImage( asset_id, material_id, parm_id );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Get the number of supported texture file formats.
	/// </summary>
	/// <returns>
	///		The number of supported image file formats.
	/// </returns>
	public static int getSupportedImageFileFormatCount()
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		int format_count = 0;
		HAPI_Result status_code = HAPI_GetSupportedImageFileFormatCount( out format_count );
		processStatusCode( status_code );
		return format_count;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	///		Get the names, descriptions, and default extensions of all supported file formats.
	/// </summary>
	/// <returns>
	///		An array of <see cref="HAPI_ImageFileFormat"/>.
	/// </returns>
	public static HAPI_ImageFileFormat[] getSupportedImageFileFormats()
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		int format_count = getSupportedImageFileFormatCount();
		HAPI_ImageFileFormat[] formats = new HAPI_ImageFileFormat[ format_count ];
		HAPI_Result status_code = HAPI_GetSupportedImageFileFormats( formats, format_count );
		processStatusCode( status_code );
		return formats;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Get information about the image that was just rendered, like resolution and default file 
	/// 	format. This information will be used when extracting planes to an image.
	/// </summary>
	/// <param name="asset_id">
	///		The asset id.
	/// </param>
	/// <param name="material_id">
	///		The material id from a <see cref="HAPI_PartInfo"/> struct.
	/// </param>
	/// <returns>
	///		A <see cref="HAPI_ImageInfo"/> with the image's information.
	/// </returns>
	public static HAPI_ImageInfo getImageInfo( HAPI_AssetId asset_id, HAPI_MaterialId material_id )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_ImageInfo image_info = new HAPI_ImageInfo();
		HAPI_Result status_code = HAPI_GetImageInfo( asset_id, material_id, out image_info );
		processStatusCode( status_code );
		return image_info;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Set image information like resolution and file format. This information will be used when 
	/// 	extracting planes to an image.
	/// </summary>
	/// <param name="asset_id">
	///		The asset id.
	/// </param>
	/// <param name="material_id">
	///		The material id from a <see cref="HAPI_PartInfo"/> struct.
	/// </param>
	/// <param name="image_info">
	///		A <see cref="HAPI_ImageInfo"/> with the new image information.
	/// </param>
	public static void setImageInfo(
		HAPI_AssetId asset_id, HAPI_MaterialId material_id, ref HAPI_ImageInfo image_info )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_SetImageInfo( asset_id, material_id, ref image_info );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Get the names of the image planes of the just rendered image.
	/// </summary>
	/// <param name="asset_id">
	///		The asset id.
	/// </param>
	/// <param name="material_id">
	///		The material id from a <see cref="HAPI_PartInfo"/> struct.
	/// </param>
	/// <returns>
	///		A list of image plane names.
	/// </returns>
	public static List< string > getImagePlanes( HAPI_AssetId asset_id, HAPI_MaterialId material_id )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = (int) HAPI_Result.HAPI_RESULT_SUCCESS;

		int image_plane_count = 0;
		status_code = HAPI_GetImagePlaneCount( asset_id, material_id, out image_plane_count );
		processStatusCode( status_code );

		int[] image_plane_names_array = new int[ image_plane_count ];
		status_code = HAPI_GetImagePlanes( 
			asset_id, material_id, image_plane_names_array, image_plane_count );
		processStatusCode( status_code );

		List< string > image_plane_names = new List< string >( image_plane_count );
		for ( int i = 0; i < image_plane_count; ++i )
			image_plane_names.Add( getString( image_plane_names_array[ i ] ) );

		return image_plane_names;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Extract a rendered image to a file.
	/// </summary>
	/// <param name="asset_id">
	///		The asset id.
	/// </param>
	/// <param name="material_id">
	///		The material id from a <see cref="HAPI_PartInfo"/> struct.
	/// </param>
	/// <param name="image_file_format_name">
	///		The image file format name you wish the image to be
	///		extracted as. You can leave this parameter (null) to
	///		get the image in the original format if it comes from
	///		another texture file or in the default HAPI format,
	///		which is HAPI_Constants.HAPI_DEFAULT_IMAGE_FORMAT_NAME, 
	///		if the image is generated.
	///
	///		You can get some of the very common standard image
	///		file format names from HAPI_Common.cs under the 
	///		HAPI_Constants enum (ie. HAPI_PNG_FORMAT_NAME).
	///
	///		You can also get a list of all supported file formats
	///		(and the exact names this parameter expects)
	///		by using getSupportedImageFileFormats(). This
	///		list will include custom file formats you created via
	///		custom DSOs (see HDK docs about IMG_Format). You 
	///		will get back a list of HAPI_ImageFileFormat(s). 
	///		This parameter expects the HAPI_ImageFileFormat::name
	///		of a given image file format.
	/// </param>
	/// <param name="image_planes">
	///		The image planes you wish to extract into the file. Multiple image planes should be 
	///		separated by spaces.
	/// </param>
	/// <param name="destination_folder_path">
	///		The folder where the image file should be created.
	/// </param>
	public static string extractImageToFile( 
		HAPI_AssetId asset_id, HAPI_MaterialId material_id, string image_file_format_name, 
		string image_planes, string destination_folder_path )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		int destination_file_path_sh = 0;

		HAPI_Result status_code = HAPI_ExtractImageToFile( 
			asset_id, material_id, image_file_format_name, image_planes, 
			destination_folder_path, null, out destination_file_path_sh );
		processStatusCode( status_code );
			
		string destination_file_path = getString( destination_file_path_sh );
		return destination_file_path;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	/// <summary>
	/// 	Extract a rendered image to memory.
	/// </summary>
	/// <param name="asset_id">
	///		The asset id.
	/// </param>
	/// <param name="material_id">
	///		The material id from a <see cref="HAPI_PartInfo"/> struct.
	/// </param>
	/// <param name="image_file_format_name">
	///		The image file format name you wish the image to be
	///		extracted as. You can leave this parameter (null) to
	///		get the image in the original format if it comes from
	///		another texture file or in the default HAPI format,
	///		which is HAPI_Constants.HAPI_DEFAULT_IMAGE_FORMAT_NAME, 
	///		if the image is generated.
	///
	///		You can get some of the very common standard image
	///		file format names from HAPI_Common.cs under the 
	///		HAPI_Constants enum (ie. HAPI_PNG_FORMAT_NAME).
	///
	///		You can also get a list of all supported file formats
	///		(and the exact names this parameter expects)
	///		by using getSupportedImageFileFormats(). This
	///		list will include custom file formats you created via
	///		custom DSOs (see HDK docs about IMG_Format). You 
	///		will get back a list of HAPI_ImageFileFormat(s). 
	///		This parameter expects the HAPI_ImageFileFormat::name
	///		of a given image file format.
	/// </param>
	/// <param name="image_planes">
	///		The image planes you wish to extract into the file. Multiple image planes should be 
	///		separated by spaces.
	/// </param>
	/// <returns>
	///		The byte stream with the extracted image binary data.
	/// </returns>
	public static byte[] extractImageToMemory( 
		HAPI_AssetId asset_id, HAPI_MaterialId material_id, string image_file_format_name, string image_planes )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		int buffer_size = 0;

		HAPI_Result status_code = HAPI_ExtractImageToMemory( 
			asset_id, material_id, image_file_format_name, image_planes, out buffer_size );
		processStatusCode( status_code );

		byte[] buffer = new byte[ buffer_size ];
		status_code = HAPI_GetImageMemoryBuffer( asset_id, material_id, buffer, buffer_size );
		processStatusCode( status_code );

		return buffer;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	// SIMULATION/ANIMATIONS ------------------------------------------------------------------------------------

	public static void setAnimCurve(
		HAPI_NodeId node_id, HAPI_ParmId parm_id, int parm_index,
		HAPI_Keyframe[] curve_keyframes,
		int keyframe_count )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_SetAnimCurve(
			node_id, parm_id, parm_index, curve_keyframes, keyframe_count );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void setTransformAnimCurve(
		HAPI_NodeId node_id, HAPI_TransformComponent transform_component,
		HAPI_Keyframe[] curve_keyframes,
		int keyframe_count )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_SetTransformAnimCurve( node_id, transform_component, curve_keyframes, keyframe_count );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void resetSimulation( HAPI_AssetId asset_id )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_ResetSimulation( asset_id );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	// VOLUMES --------------------------------------------------------------------------------------------------

	public static void getVolumeInfo(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id, HAPI_PartId part_id,
		ref HAPI_VolumeInfo volume_info )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_GetVolumeInfo( asset_id, object_id, geo_id, part_id, ref volume_info );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void  getFirstVolumeTile(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id, HAPI_PartId part_id,
		ref HAPI_VolumeTileInfo tile )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_GetFirstVolumeTile( asset_id, object_id, geo_id, part_id, ref tile );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void  getNextVolumeTile(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id, HAPI_PartId part_id,
		ref HAPI_VolumeTileInfo next )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_GetNextVolumeTile( asset_id, object_id, geo_id, part_id, ref next );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void  getVolumeTileFloatData (
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id, HAPI_PartId part_id,
		ref HAPI_VolumeTileInfo tile, [Out] float[] values )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_GetVolumeTileFloatData( asset_id, object_id, geo_id, part_id, ref tile, values );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	// CURVES ---------------------------------------------------------------------------------------------------

	public static HAPI_CurveInfo getCurveInfo(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id, HAPI_PartId part_id,
		ref HAPI_VolumeInfo volume_info )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_CurveInfo curve_info = new HAPI_CurveInfo();
		HAPI_Result status_code = HAPI_GetCurveInfo( asset_id, object_id, geo_id, part_id, ref curve_info );
		processStatusCode( status_code );
		return curve_info;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static Vector3[] getCurveVertices(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id, HAPI_PartId part_id,
		int start, int length )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		float[] raw_values = new float[ length ];
		HAPI_Result status_code = HAPI_GetCurveVertices(
			asset_id, object_id, geo_id, part_id, raw_values, start, length );
		processStatusCode( status_code );

		Vector3[] values = new Vector3[ length / HoudiniConstants.HAPI_CV_VECTOR_SIZE ];
		for ( int i = 0; i < values.Length; ++i )
		{
			values[ i ].x = raw_values[ i * HoudiniConstants.HAPI_CV_VECTOR_SIZE + 0 ];
			values[ i ].y = raw_values[ i * HoudiniConstants.HAPI_CV_VECTOR_SIZE + 1 ];
			values[ i ].z = raw_values[ i * HoudiniConstants.HAPI_CV_VECTOR_SIZE + 2 ];
		}

		return values;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static int[] getCurveCounts(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id, HAPI_PartId part_id,
		int start, int length )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		int[] counts = new int[ length ];
		HAPI_Result status_code = HAPI_GetCurveCounts(
			asset_id, object_id, geo_id, part_id, counts, start, length );
		processStatusCode( status_code );
		return counts;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static int[] getCurveOrders(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id, HAPI_PartId part_id,
		int start, int length )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		int[] orders = new int[ length ];
		HAPI_Result status_code = HAPI_GetCurveOrders(
			asset_id, object_id, geo_id, part_id, orders, start, length );
		processStatusCode( status_code );
		return orders;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static float[] getCurveKnots(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id, HAPI_PartId part_id,
		int start, int length )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		float[] knots = new float[ length ];
		HAPI_Result status_code = HAPI_GetCurveKnots(
			asset_id, object_id, geo_id, part_id, knots, start, length );
		processStatusCode( status_code );
		return knots;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	// CACHING --------------------------------------------------------------------------------------------------

	public static void saveGeoToFile(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id, 
		string file_name )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_SaveGeoToFile(
			asset_id, object_id, geo_id, file_name );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void loadGeoFromFile(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id, 
		string file_name )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_LoadGeoFromFile(
			asset_id, object_id, geo_id, file_name );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static byte[] saveGeoToMemory(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id, 
		string format )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		int size;
		HAPI_Result status_code = HAPI_GetGeoSize(
			asset_id, object_id, geo_id, format, out size );
		processStatusCode( status_code );

		byte[] memory = new byte[ size ];
		status_code = HAPI_SaveGeoToMemory(
			asset_id, object_id, geo_id, memory, size );
		processStatusCode( status_code );

		return memory;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void loadGeoFromMemory(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id, 
		string format, byte[] buffer )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_LoadGeoFromMemory(
			asset_id, object_id, geo_id, format, buffer, buffer.Length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}
}
