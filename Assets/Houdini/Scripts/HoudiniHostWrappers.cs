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
		status_code = HAPI_GetStatusString( status_type, string_builder, buffer_size );
		processStatusCode( status_code );

		string string_value = string_builder.ToString();
			
		return string_value;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

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

	public static void cookAsset( HAPI_AssetId asset_id, bool split_geos_by_group, bool import_templated_geos )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_CookOptions cook_options = new HAPI_CookOptions();
		cook_options.splitGeosByGroup = split_geos_by_group;
		cook_options.maxVerticesPerPrimitive = HoudiniConstants.HAPI_MAX_VERTICES_PER_FACE;
		cook_options.refineCurveToLinear = HoudiniConstants.HAPI_CURVE_REFINE_TO_LINEAR;
		cook_options.curveRefineLOD = HoudiniConstants.HAPI_CURVE_LOD;
		cook_options.cookTemplatedGeos = import_templated_geos;
		HAPI_Result status_code = HAPI_CookAsset( asset_id, ref cook_options );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void interrupt()
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_Interrupt();
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

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

	public static int getParmIntValue(
		HAPI_NodeId node_id, string parm_name, int index )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		int value = 0;
		HAPI_Result status_code = HAPI_GetParmIntValue( node_id, parm_name, index, out value );
		processStatusCode( status_code );
		return value;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

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

	public static float getParmFloatValue(
		HAPI_NodeId node_id, string parm_name, int index )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		float value = 0.0f;
		HAPI_Result status_code = HAPI_GetParmFloatValue( node_id, parm_name, index, out value );
		processStatusCode( status_code );
		return value;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

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

	public static string getParmStringValue(
		HAPI_NodeId node_id, string parm_name, int index )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		// Can't turn this off yet. More testing is needed, especially for file paths.
		const bool evaluate = true;

		HAPI_StringHandle string_handle = 0;
		HAPI_Result status_code = HAPI_GetParmStringValue(
			node_id, parm_name, index, evaluate, out string_handle );
		processStatusCode( status_code );
		return getString( string_handle );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

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

	public static void setParmIntValue(
		HAPI_NodeId node_id, string parm_name, int index, int value )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_SetParmIntValue( node_id, parm_name, index, value );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

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

	public static void setParmFloatValue(
		HAPI_NodeId node_id, string parm_name, int index, float value )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_SetParmFloatValue( node_id, parm_name, index, value );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

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

	public static void commitGeo( HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_CommitGeo( asset_id, object_id, geo_id );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

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

	public static void connectAssetTransform( int asset_id_from, int asset_id_to, int input_idx )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_ConnectAssetTransform( asset_id_from, asset_id_to, input_idx );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void disconnectAssetTransform( HAPI_AssetId asset_id, int input_idx )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_DisconnectAssetTransform( asset_id, input_idx );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

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

	public static HAPI_MaterialInfo[] getMaterialsOnFaces(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id, HAPI_PartId part_id )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_PartInfo part_info = new HAPI_PartInfo();
		HAPI_Result status_code = HAPI_GetPartInfo(
			asset_id, object_id, geo_id, part_id, out part_info );
		processStatusCode( status_code );

		bool are_all_the_same = false;
		int[] material_ids = new int[ part_info.faceCount ];
		status_code = HAPI_GetMaterialIdsOnFaces(
			asset_id, object_id, geo_id, part_id,
			ref are_all_the_same, material_ids, 0, part_info.faceCount );
		processStatusCode( status_code );

		HAPI_MaterialInfo material_info = new HAPI_MaterialInfo();
		HAPI_MaterialInfo[] material_infos = new HAPI_MaterialInfo[ part_info.faceCount ];
		for ( int m = 0; m < part_info.faceCount; ++m )
		{
			status_code = HAPI_GetMaterialInfo( asset_id, material_ids[ m ], out material_info );
			processStatusCode( status_code );
			material_infos[ m ] = material_info;
		}

		return material_infos;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static HAPI_MaterialInfo getMaterialOnPart(
		HAPI_AssetId asset_id, HAPI_ObjectId object_id, HAPI_GeoId geo_id, HAPI_PartId part_id )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
#if true
		HAPI_MaterialInfo material_info = new HAPI_MaterialInfo();
		HAPI_Result status_code = HAPI_GetMaterialOnPart(
			asset_id, object_id, geo_id, part_id, out material_info );
		processStatusCode( status_code );
#else
		HAPI_PartInfo part_info = new HAPI_PartInfo();
		HAPI_Result status_code = HAPI_GetPartInfo(
			asset_id, object_id, geo_id, part_id, out part_info );
		processStatusCode( status_code );

		bool are_all_the_same = false;
		int[] material_ids = new int[ 1 ];
		status_code = HAPI_GetMaterialIdsOnFaces(
			asset_id, object_id, geo_id, part_id, ref are_all_the_same, material_ids, 0, 1 );
		processStatusCode( status_code );

		HAPI_MaterialInfo material_info = new HAPI_MaterialInfo();
		status_code = HAPI_GetMaterialInfo( asset_id, material_ids[ 0 ], out material_info );
		processStatusCode( status_code );
#endif
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

	public static void renderMaterialToImage( HAPI_AssetId asset_id, HAPI_MaterialId material_id, HAPI_ShaderType shader_type )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_RenderMaterialToImage( asset_id, material_id, shader_type );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void renderTextureToImage( HAPI_AssetId asset_id, HAPI_MaterialId material_id, HAPI_ParmId parm_id )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_Result status_code = HAPI_RenderTextureToImage( asset_id, material_id, parm_id );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

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
