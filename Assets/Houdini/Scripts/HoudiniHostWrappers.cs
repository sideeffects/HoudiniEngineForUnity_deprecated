/*
* Copyright (c) <2017> Side Effects Software Inc.
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*
* Produced by:
*      Side Effects Software Inc
*      123 Front Street West, Suite 1401
*      Toronto, Ontario
*      Canada   M5J 2M2
*      416-504-9876
*
*/

// Master control for enabling runtime.
#if ( UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_LINUX )
	#define HAPI_ENABLE_RUNTIME
#endif

using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

// Typedefs (copy these from HAPI_Common.cs)
using HAPI_Int64 = System.Int64;
using HAPI_StringHandle = System.Int32;
using HAPI_ErrorCodeBits = System.Int32;
using HAPI_NodeId = System.Int32;
using HAPI_NodeTypeBits = System.Int32;
using HAPI_NodeFlagsBits = System.Int32;
using HAPI_ParmId = System.Int32;
using HAPI_PartId = System.Int32;

public static partial class HoudiniHost
{
	// DIAGNOSTICS ----------------------------------------------------------------------------------------------

	public static HAPI_License getCurrentLicense()
	{
		if ( myCurrentHoudiniLicense != HAPI_License.HAPI_LICENSE_NONE )
			return myCurrentHoudiniLicense;

#if ( HAPI_ENABLE_RUNTIME )
		int value;
		HAPI_Result status_code = HAPI_GetSessionEnvInt( ref mySession, HAPI_SessionEnvIntType.HAPI_SESSIONENVINT_LICENSE, out value );
		processStatusCode( status_code );
		myCurrentHoudiniLicense = (HAPI_License) value;
		return myCurrentHoudiniLicense;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static int getEnvInt( HAPI_EnvIntType int_type )
	{
#if ( HAPI_ENABLE_RUNTIME )
		int value;
		HAPI_Result status_code = HAPI_GetEnvInt( int_type, out value );
		processStatusCode( status_code );
		return value;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static int getServerEnvInt( string variable_name )
	{
#if ( HAPI_ENABLE_RUNTIME )
		int value;
		HAPI_Result status_code = HAPI_GetServerEnvInt( ref mySession, variable_name, out value );
		processStatusCode( status_code );
		return value;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static string getServerEnvString( string variable_name )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_StringHandle value;
		HAPI_Result status_code = HAPI_GetServerEnvString( ref mySession, variable_name, out value );
		processStatusCode( status_code );
		string str_value = getString( value );
		return str_value;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void setServerEnvInt( string variable_name, int value )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_SetServerEnvInt( ref mySession, variable_name, value );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void setServerEnvString( string variable_name, string value )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_SetServerEnvString( ref mySession, variable_name, value );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static int getStatus( HAPI_StatusType status_type )
	{
#if ( HAPI_ENABLE_RUNTIME )
		int status;
		HAPI_Result status_code = HAPI_GetStatus( ref mySession, status_type, out status );
		processStatusCode( status_code );
		return status;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static string getStatusString( HAPI_StatusType status_type, HAPI_StatusVerbosity verbosity )
	{
#if ( HAPI_ENABLE_RUNTIME )
		int buffer_length = 0;
		HAPI_Result status_code = HAPI_GetStatusStringBufLength( ref mySession, status_type, verbosity, out buffer_length );
		processStatusCode( status_code );

		if ( buffer_length <= 0 )
			return "";

		StringBuilder string_builder = new StringBuilder( buffer_length );
		status_code = HAPI_GetStatusString( ref mySession, status_type, string_builder, buffer_length );
		processStatusCode( status_code );

		string string_value = string_builder.ToString();

		return string_value;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static HAPI_ErrorCodeBits checkForSpecificErrors(
		HAPI_NodeId node_id, HAPI_ErrorCodeBits errors_to_look_for )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_ErrorCodeBits errors_found;
		HAPI_Result status_code = HAPI_CheckForSpecificErrors(
			ref mySession, node_id, errors_to_look_for, out errors_found );
		processStatusCode( status_code );

		return errors_found;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static string getNodeCookResult( HAPI_NodeId node_id, HAPI_StatusVerbosity verbosity )
	{
#if ( HAPI_ENABLE_RUNTIME )
		int buffer_length = 0;
		HAPI_Result status_code = HAPI_ComposeNodeCookResult( ref mySession, node_id, verbosity, out buffer_length );
		processStatusCode( status_code );

		if ( buffer_length <= 0 )
			return "";

		StringBuilder string_builder = new StringBuilder( buffer_length );
		status_code = HAPI_GetComposedNodeCookResult( ref mySession, string_builder, buffer_length );
		processStatusCode( status_code );

		string string_value = string_builder.ToString();

		return string_value;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static string getStatusStringNoExceptions( HAPI_StatusType status_type, HAPI_StatusVerbosity verbosity )
	{
#if ( HAPI_ENABLE_RUNTIME )
		int buffer_length = 0;
		HAPI_Result status_code = HAPI_GetStatusStringBufLength( ref mySession, status_type, verbosity, out buffer_length );
		if ( status_code != HAPI_Result.HAPI_RESULT_SUCCESS )
		{
			return "Status string getter failed. Likely the session is invalid.";
		}

		if ( buffer_length <= 0 )
			return "";

		StringBuilder string_builder = new StringBuilder( buffer_length );
		status_code = HAPI_GetStatusString( ref mySession, status_type, string_builder, buffer_length );
		if ( status_code != HAPI_Result.HAPI_RESULT_SUCCESS )
		{
			return "Status string getter failed. Likely the session is invalid.";
		}
		else
		{
			string string_value = string_builder.ToString();

			return string_value;
		}
#else
		return "Unsupported platform!";
#endif
	}

	public static int getCookingTotalCount()
	{
#if ( HAPI_ENABLE_RUNTIME )
		int count = 0;
		HAPI_Result status_code = HAPI_GetCookingTotalCount( ref mySession, out count );
		processStatusCode( status_code );
		return count;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static int getCookingCurrentCount()
	{
#if ( HAPI_ENABLE_RUNTIME )
		int count = 0;
		HAPI_Result status_code = HAPI_GetCookingCurrentCount( ref mySession, out count );
		processStatusCode( status_code );
		return count;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	// UTILITY --------------------------------------------------------------------------------------------------

	public static HAPI_TransformEuler convertTransform(
		HAPI_TransformEuler transform,
		HAPI_RSTOrder rst_order, HAPI_XYZOrder rot_order )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_TransformEuler transform_out = new HAPI_TransformEuler();
		HAPI_Result status_code = HAPI_ConvertTransform(
			ref mySession, ref transform, rst_order, rot_order, out transform_out );
		processStatusCode( status_code );
		return transform_out;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static HAPI_Transform convertMatrixToQuat( Matrix4x4 matrix, HAPI_RSTOrder rst_order )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Transform transform = new HAPI_Transform();
		float[] raw_matrix = new float[ 16 ];

		for ( int i = 0; i < 16; ++i )
			raw_matrix[ i ] = matrix[ i ];

		HAPI_Result status_code = HAPI_ConvertMatrixToQuat( ref mySession, raw_matrix, rst_order, ref transform );
		processStatusCode( status_code );

		return transform;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static HAPI_TransformEuler convertMatrixToEuler(
		Matrix4x4 matrix, HAPI_RSTOrder rst_order, HAPI_XYZOrder rot_order )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_TransformEuler transform = new HAPI_TransformEuler();
		float[] raw_matrix = new float[ 16 ];

		for ( int i = 0; i < 16; ++i )
			raw_matrix[ i ] = matrix[ i ];

		HAPI_Result status_code = HAPI_ConvertMatrixToEuler(
			ref mySession, raw_matrix, rst_order, rot_order, ref transform );
		processStatusCode( status_code );

		return transform;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static Matrix4x4 convertTransformQuatToMatrix( HAPI_Transform transform )
	{
#if ( HAPI_ENABLE_RUNTIME )
		float[] raw_matrix = new float[ 16 ];
		HAPI_Result status_code = HAPI_ConvertTransformQuatToMatrix( ref mySession, ref transform, raw_matrix );
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
#if ( HAPI_ENABLE_RUNTIME )
		float[] raw_matrix = new float[ 16 ];
		HAPI_Result status_code = HAPI_ConvertTransformEulerToMatrix( ref mySession, ref transform, raw_matrix );
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
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_GetStringBufLength( ref mySession, string_handle, out buffer_length );
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
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_GetString( ref mySession, string_handle, string_value, buffer_length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	// TIME -----------------------------------------------------------------------------------------------------

	public static float getTime()
	{
#if ( HAPI_ENABLE_RUNTIME )
		float time;
		HAPI_Result status_code = HAPI_GetTime( ref mySession, out time );
		processStatusCode( status_code );
		return time;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void setTime( float time )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_SetTime( ref mySession, time );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static HAPI_TimelineOptions getTimelineOptions()
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_TimelineOptions timeline_options = new HAPI_TimelineOptions();
		HAPI_Result status_code = HAPI_GetTimelineOptions( ref mySession, ref timeline_options );
		processStatusCode( status_code );
		return timeline_options;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void setTimelineOptions( HAPI_TimelineOptions timeline_options )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_SetTimelineOptions( ref mySession, ref timeline_options );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	// ASSETS -------------------------------------------------------------------------------------------------------

	public static HAPI_AssetInfo getAssetInfo( HAPI_NodeId node_id )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_AssetInfo asset_info = new HAPI_AssetInfo();
		HAPI_Result status_code = HAPI_GetAssetInfo( ref mySession, node_id, ref asset_info );
		processStatusCode( status_code );
		return asset_info;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void interrupt()
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_Interrupt( ref mySession );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	// NODES ----------------------------------------------------------------------------------------------------

	public static bool isNodeValid( HAPI_NodeId node_id, int unique_node_id )
	{
#if ( HAPI_ENABLE_RUNTIME )
		if ( !isInstallationOk() )
			return false;

		if ( node_id < 0 )
			return false;

		bool answer = true;

		// No need to process return code because this function is guaranteed to 
		// always return HAPI_STATUS_SUCCESS.
		HAPI_IsNodeValid( ref mySession, node_id, unique_node_id, ref answer );

		return answer;
#else
		return false;
#endif
	}

	public static HAPI_NodeInfo getNodeInfo( HAPI_NodeId node_id )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_NodeInfo node_info;
		HAPI_Result status_code = HAPI_GetNodeInfo( ref mySession, node_id, out node_info );
		processStatusCode( status_code );
		return node_info;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static string getNodePath( HAPI_NodeId node_id, HAPI_NodeId relative_to_node_id )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_StringHandle path_sh;
		HAPI_Result status_code = HAPI_GetNodePath( ref mySession, node_id, relative_to_node_id, out path_sh );
		processStatusCode( status_code );

		string path = getString( path_sh );

		return path;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static HAPI_NodeId getManagerNodeId( HAPI_NodeType node_type )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_NodeId node_id = -1;
		HAPI_Result status_code = HAPI_GetManagerNodeId( ref mySession, node_type, out node_id );
		processStatusCode( status_code );
		return node_id;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static HAPI_NodeId[] getChildNodeList(
		HAPI_NodeId parent_node_id,
		HAPI_NodeTypeBits node_type_filter,
		HAPI_NodeFlagsBits node_flags_filter,
		bool recursive )
	{
#if ( HAPI_ENABLE_RUNTIME )
		int count = -1;
		HAPI_Result status_code = HAPI_ComposeChildNodeList(
			ref mySession, parent_node_id, node_type_filter,
			node_flags_filter, recursive, out count );
		processStatusCode( status_code );

		HAPI_NodeId[] child_node_ids = new HAPI_NodeId[ count ];
		status_code = HAPI_GetComposedChildNodeList(
			ref mySession, parent_node_id, child_node_ids, count );
		processStatusCode( status_code );

		return child_node_ids;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static HAPI_NodeId createNode( HAPI_NodeId parent_node_id, string operator_name, bool cook_on_creation )
	{
		return createNode( parent_node_id, operator_name, cook_on_creation, "" );
	}

	public static HAPI_NodeId createNode( HAPI_NodeId parent_node_id, string operator_name, bool cook_on_creation, string node_label )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_NodeId new_node_id;
		HAPI_Result status_code = HAPI_CreateNode(
			ref mySession, parent_node_id, operator_name, node_label, cook_on_creation, out new_node_id );
		processStatusCode( status_code );

		return new_node_id;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static HAPI_NodeId createInputNode( string name )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_NodeId node_id = -1;
		HAPI_Result status_code = HAPI_CreateInputNode( ref mySession, out node_id, name );
		processStatusCode( status_code );
		return node_id;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void cookNode( HAPI_NodeId node_id )
	{
		cookNode( node_id, prSplitGeosByGroup, prSplitPointsByVertexAttributes, prImportTemplatedGeos );
	}

	public static void cookNode(
		HAPI_NodeId node_id,
		bool split_geos_by_group,
		bool split_points_by_vertex_attributes,
		bool import_templated_geos )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_CookOptions cook_options =
			getCookOptions( split_geos_by_group, split_points_by_vertex_attributes, import_templated_geos );
		HAPI_Result status_code = HAPI_CookNode(
			ref mySession, node_id, ref cook_options );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void deleteNode( HAPI_NodeId node_id )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_DeleteNode( ref mySession, node_id );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void renameNode( HAPI_NodeId node_id, string new_name )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_RenameNode( ref mySession, node_id, new_name );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void connectNodeInput(
		HAPI_NodeId node_id, int input_index, HAPI_NodeId node_id_to_connect )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_ConnectNodeInput(
			ref mySession, node_id, input_index, node_id_to_connect );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void disconnectNodeInput( HAPI_NodeId node_id, int input_index )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_DisconnectNodeInput( ref mySession, node_id, input_index );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static HAPI_NodeId queryNodeInput( HAPI_NodeId node_id, int input_index )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_NodeId connected_node_id;
		HAPI_Result status_code = HAPI_QueryNodeInput( ref mySession, node_id, input_index, out connected_node_id );
		processStatusCode( status_code );

		return connected_node_id;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static string getNodeInputName(
		HAPI_NodeId node_id, int input_idx )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_StringHandle name_sh = 0;
		HAPI_Result status_code = HAPI_GetNodeInputName( ref mySession, node_id, input_idx, out name_sh );
		processStatusCode( status_code );
		string name = getString( name_sh );
		return name;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	// PARAMETERS -----------------------------------------------------------------------------------------------

	public static void getParameters(
		HAPI_NodeId node_id, [Out] HAPI_ParmInfo[] parm_infos,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_GetParameters( ref mySession, node_id, parm_infos, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static HAPI_ParmInfo getParmInfo( HAPI_NodeId node_id, HAPI_ParmId parm_id )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_ParmInfo parm_info = new HAPI_ParmInfo();
		HAPI_Result status_code = HAPI_GetParmInfo( ref mySession, node_id, parm_id, out parm_info );
		processStatusCode( status_code );
		return parm_info;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static HAPI_ParmId getParmIdFromName( HAPI_NodeId node_id, string name )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_ParmId parm_id = -1;
		HAPI_Result status_code = HAPI_GetParmIdFromName( ref mySession, node_id, name, out parm_id );
		processStatusCode( status_code );
		return parm_id;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static string getParmTagName( HAPI_NodeId node_id, HAPI_ParmId parm_id, int tag_index )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_StringHandle tag_name_sh = -1;
		HAPI_Result status_code = HAPI_GetParmTagName( ref mySession, node_id, parm_id, tag_index, out tag_name_sh );
		processStatusCode( status_code );

		string tag_name = getString( tag_name_sh );
		return tag_name;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static string getParmTagValue( HAPI_NodeId node_id, HAPI_ParmId parm_id, string tag_name )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_StringHandle tag_value_sh = -1;
		HAPI_Result status_code = HAPI_GetParmTagValue( ref mySession, node_id, parm_id, tag_name, out tag_value_sh );
		processStatusCode( status_code );

		string tag_value = getString( tag_value_sh );
		return tag_value;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static bool parmHasTag( HAPI_NodeId node_id, HAPI_ParmId parm_id, string tag_name )
	{
#if ( HAPI_ENABLE_RUNTIME )
		bool has_tag = false;
		HAPI_Result status_code = HAPI_ParmHasTag( ref mySession, node_id, parm_id, tag_name, ref has_tag );
		processStatusCode( status_code );
		return has_tag;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static HAPI_ParmId getParmWithTag( HAPI_NodeId node_id, string tag_name )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_ParmId parm_id = -1;
		HAPI_Result status_code = HAPI_GetParmWithTag( ref mySession, node_id, tag_name, ref parm_id );
		processStatusCode( status_code );
		return parm_id;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static int getParmIntValue(
		HAPI_NodeId node_id, string parm_name, int index )
	{
#if ( HAPI_ENABLE_RUNTIME )
		int value = 0;
		HAPI_Result status_code = HAPI_GetParmIntValue( ref mySession, node_id, parm_name, index, out value );
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
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_GetParmIntValues( ref mySession, node_id, values, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static float getParmFloatValue(
		HAPI_NodeId node_id, string parm_name, int index )
	{
#if ( HAPI_ENABLE_RUNTIME )
		float value = 0.0f;
		HAPI_Result status_code = HAPI_GetParmFloatValue( ref mySession, node_id, parm_name, index, out value );
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
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_GetParmFloatValues( ref mySession, node_id, values, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static string getParmStringValue(
		HAPI_NodeId node_id, string parm_name, int index )
	{
#if ( HAPI_ENABLE_RUNTIME )
		// Can't turn this off yet. More testing is needed, especially for file paths.
		const bool evaluate = true;

		HAPI_StringHandle string_handle = 0;
		HAPI_Result status_code = HAPI_GetParmStringValue(
			ref mySession, node_id, parm_name, index, evaluate, out string_handle );
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
#if ( HAPI_ENABLE_RUNTIME )
		// Can't turn this off yet. More testing is needed, especially for file paths.
		const bool evaluate = true;

		HAPI_Result status_code = HAPI_GetParmStringValues( ref mySession, node_id, evaluate, values, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static HAPI_NodeId getParmNodeValue(
		HAPI_NodeId node_id, string parm_name )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_NodeId value = -1;
		HAPI_Result status_code = HAPI_GetParmNodeValue( ref mySession, node_id, parm_name, out value );
		processStatusCode( status_code );
		return value;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void getParmFile(
		HAPI_NodeId node_id,
		string parm_name,
		string destination_directory,
		string destination_file_name )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_GetParmFile(
			ref mySession, node_id, parm_name, destination_directory, destination_file_name );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void getParmChoiceLists(
		HAPI_NodeId node_id, [Out] HAPI_ParmChoiceInfo[] parm_choices,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_GetParmChoiceLists( ref mySession, node_id, parm_choices, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void setParmIntValue(
		HAPI_NodeId node_id, string parm_name, int index, int value )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_SetParmIntValue( ref mySession, node_id, parm_name, index, value );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void setParmIntValues(
		HAPI_NodeId node_id, int[] values,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_SetParmIntValues( ref mySession, node_id, values, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void setParmFloatValue(
		HAPI_NodeId node_id, string parm_name, int index, float value )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_SetParmFloatValue( ref mySession, node_id, parm_name, index, value );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void setParmFloatValues(
		HAPI_NodeId node_id, float[] values,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_SetParmFloatValues( ref mySession, node_id, values, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void setParmStringValue(
		HAPI_NodeId node_id, string value,
		HAPI_ParmId parm_id, int index )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_SetParmStringValue( ref mySession, node_id, value, parm_id, index );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void setParmNodeValue(
		HAPI_NodeId node_id, string parm_name, HAPI_NodeId value )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_SetParmNodeValue( ref mySession, node_id, parm_name, value );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void insertMultiparmInstance(
		HAPI_NodeId node_id, HAPI_ParmId parm_id,
		int instance_position )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_InsertMultiparmInstance( ref mySession, node_id, parm_id, instance_position );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void removeMultiparmInstance(
		HAPI_NodeId node_id, HAPI_ParmId parm_id,
		int instance_position )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_RemoveMultiparmInstance( ref mySession, node_id, parm_id, instance_position );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	// HANDLES --------------------------------------------------------------------------------------------------

	public static void getHandleInfo(
		HAPI_NodeId node_id,
		[Out] HAPI_HandleInfo[] handle_infos,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_GetHandleInfo( ref mySession, node_id, handle_infos, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void getHandleBindingInfo(
		HAPI_NodeId node_id,
		int handle_index,
		[Out] HAPI_HandleBindingInfo[] handle_infos,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_GetHandleBindingInfo( ref mySession, node_id, handle_index, handle_infos, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	// PRESETS --------------------------------------------------------------------------------------------------

	public static byte[] getPreset( HAPI_NodeId node_id )
	{
#if ( HAPI_ENABLE_RUNTIME )
		int buffer_length = 0;
		HAPI_Result status_code = HAPI_GetPresetBufLength(
			ref mySession, node_id, HAPI_PresetType.HAPI_PRESETTYPE_BINARY, null, ref buffer_length );
		processStatusCode( status_code );

		byte[] preset = new byte[ buffer_length ];
		status_code = HAPI_GetPreset( ref mySession, node_id, preset, buffer_length );
		processStatusCode( status_code );

		return preset;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void setPreset( HAPI_NodeId node_id, byte[] preset )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_SetPreset(
			ref mySession, node_id, HAPI_PresetType.HAPI_PRESETTYPE_BINARY, null, preset, preset.Length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	// OBJECTS --------------------------------------------------------------------------------------------------

	public static HAPI_ObjectInfo getObjectInfo( HAPI_NodeId node_id )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_ObjectInfo object_info;
		HAPI_Result status_code = HAPI_GetObjectInfo( ref mySession, node_id, out object_info );
		processStatusCode( status_code );
		return object_info;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static HAPI_Transform getObjectTransform(
		HAPI_NodeId node_id, HAPI_NodeId relative_to_node_id, HAPI_RSTOrder rst_order )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Transform transform;
		HAPI_Result status_code = HAPI_GetObjectTransform(
			ref mySession, node_id, relative_to_node_id, rst_order, out transform );
		processStatusCode( status_code );
		return transform;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

    /*
	public static HAPI_ObjectInfo[] getObjectInfos( HAPI_NodeId node_id )
	{
#if ( HAPI_ENABLE_RUNTIME )
		int object_count = 0;
		HAPI_Result status_code = HAPI_ComposeObjectList(
			ref mySession, node_id, "", out object_count );
		processStatusCode( status_code );

		HAPI_ObjectInfo[] object_infos = new HAPI_ObjectInfo[ object_count ];
		status_code = HAPI_GetComposedObjectList(
			ref mySession, node_id, object_infos, 0, object_count );
		processStatusCode( status_code );

		return object_infos;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}
    */

    public static HAPI_ObjectInfo[] getObjectInfos(HAPI_NodeId node_id)
    {
#if (HAPI_ENABLE_RUNTIME)
        HAPI_NodeInfo LocalAssetNodeInfo = getNodeInfo( node_id );

        int object_count = 0;
        HAPI_ObjectInfo[] object_infos = new HAPI_ObjectInfo[0];
        if ( LocalAssetNodeInfo.type == HAPI_NodeType.HAPI_NODETYPE_SOP )
        {
            object_count = 1;

            object_infos = new HAPI_ObjectInfo[object_count];
            getObjectInfo( LocalAssetNodeInfo.parentId );
        }
        else if ( LocalAssetNodeInfo.type == HAPI_NodeType.HAPI_NODETYPE_OBJ )
        {
            HAPI_Result status_code = HAPI_ComposeObjectList(
            ref mySession, node_id, "", out object_count );
            processStatusCode(status_code);

            if ( object_count <= 0)
            {
                object_count = 1;

                object_infos = new HAPI_ObjectInfo[object_count];
                getObjectInfo( node_id );
            }
            else
            {
                object_infos = new HAPI_ObjectInfo[object_count];
                status_code = HAPI_GetComposedObjectList(
                    ref mySession, node_id, object_infos, 0, object_count);
                processStatusCode(status_code);
            }            
        }

        return object_infos;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
    }
    
    /*
    public static HAPI_Transform[] getObjectTransforms( HAPI_NodeId node_id, HAPI_RSTOrder rst_order )
	{
#if ( HAPI_ENABLE_RUNTIME )
		int object_count = 0;
		HAPI_Result status_code = HAPI_ComposeObjectList(
			ref mySession, node_id, "", out object_count );
		processStatusCode( status_code );

		HAPI_Transform[] transforms = new HAPI_Transform[ object_count ];
		status_code = HAPI_GetComposedObjectTransforms(
			ref mySession, node_id, rst_order, transforms, 0, object_count );
		processStatusCode( status_code );

		return transforms;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}
    */
    
    public static HAPI_Transform[] getObjectTransforms(HAPI_NodeId node_id, HAPI_RSTOrder rst_order)
    {
#if (HAPI_ENABLE_RUNTIME)
        HAPI_NodeInfo LocalAssetNodeInfo = getNodeInfo(node_id);

        HAPI_Transform identity = new HAPI_Transform(true);

        int object_count = 1;
        HAPI_Transform[] object_transforms = new HAPI_Transform[1];
        object_transforms[0] = identity;

        if (LocalAssetNodeInfo.type == HAPI_NodeType.HAPI_NODETYPE_OBJ)
        {
            HAPI_Result status_code = HAPI_ComposeObjectList(
                ref mySession, node_id, "", out object_count);
            processStatusCode(status_code);

            if (object_count > 0)
            {
                object_transforms = new HAPI_Transform[ object_count ];
                status_code = HAPI_GetComposedObjectTransforms(
                    ref mySession, node_id, rst_order, object_transforms, 0, object_count);
                processStatusCode(status_code);
            }
        }

        return object_transforms;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
    }
    
    public static int composeObjectList( HAPI_NodeId node_id )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_NodeInfo node_info;
		HAPI_Result status_code = HAPI_GetNodeInfo( ref mySession, node_id, out node_info );
		
		int obj_node_id = node_id;
		if ( node_info.type == HAPI_NodeType.HAPI_NODETYPE_SOP )
			obj_node_id = node_info.parentId;

		int object_count = 0;
		status_code = HAPI_ComposeObjectList( ref mySession, obj_node_id, "", out object_count );
		processStatusCode( status_code );

		return object_count;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

    public static void getComposedObjectList(
		HAPI_NodeId node_id,
		[Out] HAPI_ObjectInfo[] object_infos,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_GetComposedObjectList( ref mySession, node_id, object_infos, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void getComposedObjectTransforms(
		HAPI_NodeId node_id,
		HAPI_RSTOrder rst_order,
		[Out] HAPI_Transform[] transforms,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_GetComposedObjectTransforms( ref mySession, node_id, rst_order, transforms, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void getInstancedObjectIds(
		HAPI_NodeId node_id,
		[Out] HAPI_NodeId[] instanced_node_id_array,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_GetInstancedObjectIds(
			ref mySession, node_id, instanced_node_id_array, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void getInstanceTransforms(
		HAPI_NodeId node_id,
		HAPI_RSTOrder rst_order,
		[Out] HAPI_Transform[] transforms,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_GetInstanceTransforms(
			ref mySession, node_id, rst_order, transforms, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void setObjectTransform(
		HAPI_NodeId node_id,
		ref HAPI_TransformEuler transform )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_SetObjectTransform( ref mySession, node_id, ref transform );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	// GEOMETRY GETTERS -----------------------------------------------------------------------------------------

	public static HAPI_GeoInfo getDisplayGeoInfo( HAPI_NodeId object_node_id )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_GeoInfo geo_info;
		HAPI_Result status_code = HAPI_GetDisplayGeoInfo( ref mySession, object_node_id, out geo_info );
		processStatusCode( status_code );
		return geo_info;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static HAPI_GeoInfo getGeoInfo( HAPI_NodeId node_id )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_GeoInfo geo_info;
		HAPI_Result status_code = HAPI_GetGeoInfo( ref mySession, node_id, out geo_info );
		processStatusCode( status_code );
		return geo_info;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static HAPI_PartInfo getPartInfo( HAPI_NodeId node_id, HAPI_PartId part_id )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_PartInfo part_info;
		HAPI_Result status_code = HAPI_GetPartInfo( ref mySession, node_id, part_id, out part_info );
		processStatusCode( status_code );
		return part_info;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void getFaceCounts(
		HAPI_NodeId node_id, HAPI_PartId part_id, [Out] int[] face_counts,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_GetFaceCounts( ref mySession, node_id, part_id, face_counts, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static int[] getFaceCounts(
		HAPI_NodeId node_id, HAPI_PartId part_id,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		int[] face_counts = new int[ length ];
		HAPI_Result status_code = HAPI_GetFaceCounts( ref mySession, node_id, part_id, face_counts, start, length );
		processStatusCode( status_code );
		return face_counts;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void getVertexList(
		HAPI_NodeId node_id, HAPI_PartId part_id, [Out] int[] vertex_list,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_GetVertexList( ref mySession, node_id, part_id, vertex_list, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static int[] getVertexList(
		HAPI_NodeId node_id, HAPI_PartId part_id,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		int[] vertex_list = new int[ length ];
		HAPI_Result status_code = HAPI_GetVertexList( ref mySession, node_id, part_id, vertex_list, start, length );
		processStatusCode( status_code );
		return vertex_list;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static HAPI_AttributeInfo getAttributeInfo(
		HAPI_NodeId node_id, HAPI_PartId part_id, string name, HAPI_AttributeOwner owner )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_AttributeInfo info = new HAPI_AttributeInfo();
		HAPI_Result status_code = HAPI_GetAttributeInfo( ref mySession, node_id, part_id, name, owner, ref info );
		processStatusCode( status_code );
		return info;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static string[] getAttributeNames(
		HAPI_NodeId node_id, HAPI_PartId part_id,
		HAPI_AttributeOwner owner )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_PartInfo part_info = new HAPI_PartInfo();
		HAPI_Result status_code = HAPI_GetPartInfo( ref mySession, node_id, part_id, out part_info );
		processStatusCode( status_code );

		int count = part_info.attributeCounts[ (int) owner ];

		int[] names = new int[ count ];
		status_code = HAPI_GetAttributeNames(
			ref mySession, node_id, part_id, owner, names, count );
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
		HAPI_NodeId node_id, HAPI_PartId part_id, string name,
		ref HAPI_AttributeInfo attr_info, [Out] int[] data,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_GetAttributeIntData(
			ref mySession, node_id, part_id, name,
			ref attr_info, -1, data, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static int[] getAttributeIntData(
		HAPI_NodeId node_id, HAPI_PartId part_id, string name,
		ref HAPI_AttributeInfo attr_info,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		int[] data = new int[ length * attr_info.tupleSize ];
		HAPI_Result status_code = HAPI_GetAttributeIntData(
			ref mySession, node_id, part_id, name,
			ref attr_info, -1, data, start, length );
		processStatusCode( status_code );
		return data;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void getAttributeInt64Data(
		HAPI_NodeId node_id, HAPI_PartId part_id, string name,
		ref HAPI_AttributeInfo attr_info, [Out] HAPI_Int64[] data,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_GetAttributeInt64Data(
			ref mySession, node_id, part_id, name,
			ref attr_info, -1, data, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static HAPI_Int64[] getAttributeInt64Data(
		HAPI_NodeId node_id, HAPI_PartId part_id, string name,
		ref HAPI_AttributeInfo attr_info,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Int64[] data = new HAPI_Int64[ length * attr_info.tupleSize ];
		HAPI_Result status_code = HAPI_GetAttributeInt64Data(
			ref mySession, node_id, part_id, name,
			ref attr_info, -1, data, start, length );
		processStatusCode( status_code );
		return data;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void getAttributeFloatData(
		HAPI_NodeId node_id, HAPI_PartId part_id, string name,
		ref HAPI_AttributeInfo attr_info, [Out] float[] data,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_GetAttributeFloatData(
			ref mySession, node_id, part_id, name,
			ref attr_info, -1, data, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static float[] getAttributeFloatData(
		HAPI_NodeId node_id, HAPI_PartId part_id, string name,
		ref HAPI_AttributeInfo attr_info,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		float[] data = new float[ length * attr_info.tupleSize ];
		HAPI_Result status_code = HAPI_GetAttributeFloatData(
			ref mySession, node_id, part_id, name,
			ref attr_info, -1, data, start, length );
		processStatusCode( status_code );
		return data;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void getAttributeFloat64Data(
		HAPI_NodeId node_id, HAPI_PartId part_id, string name,
		ref HAPI_AttributeInfo attr_info, [Out] double[] data,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_GetAttributeFloat64Data(
			ref mySession, node_id, part_id, name,
			ref attr_info, -1, data, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static double[] getAttributeFloat64Data(
		HAPI_NodeId node_id, HAPI_PartId part_id, string name,
		ref HAPI_AttributeInfo attr_info,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		double[] data = new double[ length * attr_info.tupleSize ];
		HAPI_Result status_code = HAPI_GetAttributeFloat64Data(
			ref mySession, node_id, part_id, name,
			ref attr_info, -1, data, start, length );
		processStatusCode( status_code );
		return data;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void getAttributeStringData(
		HAPI_NodeId node_id, HAPI_PartId part_id, string name,
		ref HAPI_AttributeInfo attr_info, [Out] int[] data,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_GetAttributeStringData(
			ref mySession, node_id, part_id, name, ref attr_info, data, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static string[] getAttributeStringData(
		HAPI_NodeId node_id, HAPI_PartId part_id, string name,
		ref HAPI_AttributeInfo attr_info,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		int[] data = new int[ length * attr_info.tupleSize ];
		HAPI_Result status_code = HAPI_GetAttributeStringData(
			ref mySession, node_id, part_id, name, ref attr_info, data, start, length );
		processStatusCode( status_code );
		
		string[] strings = new string[ length * attr_info.tupleSize ];
		for ( int i = 0; i < strings.Length; ++i )
			strings[ i ] = getString( data[ i ] );

		return strings;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static string[] getGroupNames( HAPI_NodeId node_id, HAPI_GroupType group_type )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_GeoInfo geo_info = new HAPI_GeoInfo();
		HAPI_Result status_code = HAPI_GetGeoInfo( ref mySession, node_id, out geo_info );
		processStatusCode( status_code );

		int count = geo_info.getGroupCountByType( group_type );

		int[] names = new int[ count ];
		status_code = HAPI_GetGroupNames(
			ref mySession, node_id, group_type, names, count );
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
		HAPI_NodeId node_id, HAPI_PartId part_id,
		HAPI_GroupType group_type,
		string group_name )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_PartInfo part_info = new HAPI_PartInfo();
		HAPI_Result status_code = HAPI_GetPartInfo( ref mySession, node_id, part_id, out part_info );
		processStatusCode( status_code );

		int count = part_info.getElementCountByGroupType( group_type );

		int[] membership = new int[ count ];

		if ( count > 0 )
		{
			bool membership_array_all_equal = false;
			status_code = HAPI_GetGroupMembership(
				ref mySession, node_id, part_id,
				group_type, group_name, 
				ref membership_array_all_equal,
				membership, 0, count );
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

	public static HAPI_PartId[] getInstancedPartIds(
		HAPI_NodeId node_id, HAPI_PartId part_id )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_PartInfo part_info = new HAPI_PartInfo();
		HAPI_Result status_code = HAPI_GetPartInfo( ref mySession, node_id, part_id, out part_info );
		processStatusCode( status_code );

		int count = part_info.instancedPartCount;

		HAPI_PartId[] part_ids = new HAPI_PartId[ count ];

		if ( count > 0 )
		{
			status_code = HAPI_GetInstancedPartIds(
				ref mySession, node_id, part_id, part_ids, 0, count );
			processStatusCode( status_code );
		}

		return part_ids;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static HAPI_Transform[] getInstancerPartTransforms(
		HAPI_NodeId node_id, HAPI_PartId part_id,
		HAPI_RSTOrder rst_order )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_PartInfo part_info = new HAPI_PartInfo();
		HAPI_Result status_code = HAPI_GetPartInfo( ref mySession, node_id, part_id, out part_info );
		processStatusCode( status_code );

		int count = part_info.instanceCount;

		HAPI_Transform[] transforms = new HAPI_Transform[ count ];

		if ( count > 0 )
		{
			status_code = HAPI_GetInstancerPartTransforms(
				ref mySession, node_id, part_id, rst_order, transforms, 0, count );
			processStatusCode( status_code );
		}

		return transforms;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	// GEOMETRY SETTERS -----------------------------------------------------------------------------------------

	public static void setPartInfo(
		HAPI_NodeId node_id, HAPI_PartId part_id,
		ref HAPI_PartInfo part_info )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_SetPartInfo( ref mySession, node_id, part_id, ref part_info );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void setFaceCounts(
		HAPI_NodeId node_id, HAPI_PartId part_id,
		int[] face_counts,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_SetFaceCounts( ref mySession, node_id, part_id, face_counts, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void setVertexList(
		HAPI_NodeId node_id, HAPI_PartId part_id,
		int[] vertex_list,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_SetVertexList( ref mySession, node_id, part_id, vertex_list, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void addAttribute(
		HAPI_NodeId node_id, HAPI_PartId part_id,
		string name,
		ref HAPI_AttributeInfo attr_info )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_AddAttribute( ref mySession, node_id, part_id, name, ref attr_info );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void setAttributeIntData(
		HAPI_NodeId node_id, HAPI_PartId part_id,
		string name,
		ref HAPI_AttributeInfo attr_info,
		int[] data,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_SetAttributeIntData(
			ref mySession, node_id, part_id, name, ref attr_info, data, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void setAttributeInt64Data(
		HAPI_NodeId node_id, HAPI_PartId part_id,
		string name,
		ref HAPI_AttributeInfo attr_info,
		HAPI_Int64[] data,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_SetAttributeInt64Data(
			ref mySession, node_id, part_id, name, ref attr_info, data, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void setAttributeFloatData(
		HAPI_NodeId node_id, HAPI_PartId part_id,
		string name,
		ref HAPI_AttributeInfo attr_info,
		float[] data,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_SetAttributeFloatData(
			ref mySession, node_id, part_id, name, ref attr_info, data, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void setAttributeFloat64Data(
		HAPI_NodeId node_id, HAPI_PartId part_id,
		string name,
		ref HAPI_AttributeInfo attr_info,
		double[] data,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_SetAttributeFloat64Data(
			ref mySession, node_id, part_id, name, ref attr_info, data, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void setAttributeStringData(
		HAPI_NodeId node_id, HAPI_PartId part_id,
		string name,
		ref HAPI_AttributeInfo attr_info,
		string[] data,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_SetAttributeStringData(
			ref mySession, node_id, part_id, name, ref attr_info, data, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void addGroup(
		HAPI_NodeId node_id, HAPI_PartId part_id,
		HAPI_GroupType group_type, string group_name )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_AddGroup(
			ref mySession, node_id, part_id, group_type, group_name );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void setGroupMembership(
		HAPI_NodeId node_id, HAPI_PartId part_id,
		HAPI_GroupType group_type,
		string group_name,
		bool[] membership,
		int count )
	{
#if ( HAPI_ENABLE_RUNTIME )
		if ( count != membership.Length )
			throw new HoudiniErrorInvalidArgument( "Membership array not same size as count argument!" );

		int[] membership_int = new int[ count ];
		for ( int i = 0; i < count; ++i )
			membership_int[ i ] = membership[ i ] ? 1 : 0;

		HAPI_Result status_code = HAPI_SetGroupMembership(
			ref mySession, node_id, part_id, group_type, group_name, membership_int, 0, count );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void commitGeo( HAPI_NodeId node_id )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_CommitGeo( ref mySession, node_id );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void revertGeo( HAPI_NodeId node_id )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_RevertGeo( ref mySession, node_id );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	// INTER-ASSET ----------------------------------------------------------------------------------------------

//	public static void connectAssetTransform( int asset_id_from, int asset_id_to, int input_idx )
//	{
//#if ( HAPI_ENABLE_RUNTIME )
//		HAPI_Result status_code = HAPI_ConnectAssetTransform( ref mySession, asset_id_from, asset_id_to, input_idx );
//		processStatusCode( status_code );
//#else
//		throw new HoudiniErrorUnsupportedPlatform();
//#endif
//	}

//	public static void disconnectAssetTransform( HAPI_NodeId asset_id, int input_idx )
//	{
//#if ( HAPI_ENABLE_RUNTIME )
//		HAPI_Result status_code = HAPI_DisconnectAssetTransform( ref mySession, asset_id, input_idx );
//		processStatusCode( status_code );
//#else
//		throw new HoudiniErrorUnsupportedPlatform();
//#endif
//	}

//	public static void connectAssetGeometry(
//		HAPI_NodeId asset_id_from, HAPI_NodeId object_id_from,
//		HAPI_NodeId asset_id_to, int input_idx )
//	{
//#if ( HAPI_ENABLE_RUNTIME )
//		HAPI_Result status_code = HAPI_ConnectAssetGeometry(
//			ref mySession, asset_id_from, object_id_from, asset_id_to, input_idx );
//		processStatusCode( status_code );
//#else
//		throw new HoudiniErrorUnsupportedPlatform();
//#endif
//	}

//	public static void disconnectAssetGeometry( HAPI_NodeId asset_id, int input_idx )
//	{
//#if ( HAPI_ENABLE_RUNTIME )
//		HAPI_Result status_code = HAPI_DisconnectAssetGeometry( ref mySession, asset_id, input_idx );
//		processStatusCode( status_code );
//#else
//		throw new HoudiniErrorUnsupportedPlatform();
//#endif
//	}

	// MATERIALS ------------------------------------------------------------------------------------------------

	public static HAPI_MaterialInfo[] getMaterialsOnFaces(
		HAPI_NodeId geometry_node_id, HAPI_PartId part_id )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_PartInfo part_info = new HAPI_PartInfo();
		HAPI_Result status_code = HAPI_GetPartInfo(
			ref mySession, geometry_node_id, part_id, out part_info );
		processStatusCode( status_code );

		bool are_all_the_same = false;
		int[] material_ids = new int[ part_info.faceCount ];
		status_code = HAPI_GetMaterialNodeIdsOnFaces(
			ref mySession, geometry_node_id, part_id,
			ref are_all_the_same, material_ids, 0, part_info.faceCount );
		processStatusCode( status_code );

		HAPI_MaterialInfo material_info = new HAPI_MaterialInfo();
		HAPI_MaterialInfo[] material_infos = new HAPI_MaterialInfo[ part_info.faceCount ];
		for ( int m = 0; m < part_info.faceCount; ++m )
		{
			status_code = HAPI_GetMaterialInfo( ref mySession, material_ids[ m ], out material_info );
			processStatusCode( status_code );
			material_infos[ m ] = material_info;
		}

		return material_infos;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static HAPI_MaterialInfo getMaterialOnPart(
		HAPI_NodeId geometry_node_id, HAPI_PartId part_id )
	{
#if ( HAPI_ENABLE_RUNTIME )
		bool are_all_the_same = false;
		HAPI_NodeId[] material_ids = new HAPI_NodeId[ 1 ];
		HAPI_Result status_code = HAPI_GetMaterialNodeIdsOnFaces(
			ref mySession, geometry_node_id, part_id, ref are_all_the_same, material_ids, 0, 1 );
		processStatusCode( status_code );

		HAPI_MaterialInfo material_info = new HAPI_MaterialInfo();
		status_code = HAPI_GetMaterialInfo(
			ref mySession, material_ids[ 0 ], out material_info );
		processStatusCode( status_code );

		return material_info;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static HAPI_MaterialInfo getMaterialOnGroup( HAPI_NodeId geometry_node_id, string group_name )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_MaterialInfo material_info = new HAPI_MaterialInfo();

		// This method is no longer supported.
		// TODO: Sweep!
		material_info.exists = false;

		return material_info;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void renderCOPToImage( HAPI_NodeId cop_node_id )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_RenderCOPToImage( ref mySession, cop_node_id );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void renderTextureToImage( HAPI_NodeId material_node_id, HAPI_ParmId parm_id )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_RenderTextureToImage( ref mySession, material_node_id, parm_id );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static HAPI_ImageInfo getImageInfo( HAPI_NodeId material_node_id )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_ImageInfo image_info = new HAPI_ImageInfo();
		HAPI_Result status_code = HAPI_GetImageInfo( ref mySession, material_node_id, out image_info );
		processStatusCode( status_code );
		return image_info;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void setImageInfo( HAPI_NodeId material_node_id, ref HAPI_ImageInfo image_info )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_SetImageInfo( ref mySession, material_node_id, ref image_info );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static List< string > getImagePlanes( HAPI_NodeId material_node_id )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = (int) HAPI_Result.HAPI_RESULT_SUCCESS;

		int image_plane_count = 0;
		status_code = HAPI_GetImagePlaneCount( ref mySession, material_node_id, out image_plane_count );
		processStatusCode( status_code );

		int[] image_plane_names_array = new int[ image_plane_count ];
		status_code = HAPI_GetImagePlanes( 
			ref mySession, material_node_id, image_plane_names_array, image_plane_count );
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
		HAPI_NodeId material_node_id, string image_file_format_name,
		string image_planes, string destination_folder_path )
	{
#if ( HAPI_ENABLE_RUNTIME )
		int destination_file_path_sh = 0;

		HAPI_Result status_code = HAPI_ExtractImageToFile(
			ref mySession, material_node_id, image_file_format_name, image_planes, 
			destination_folder_path, null, out destination_file_path_sh );
		processStatusCode( status_code );
			
		string destination_file_path = getString( destination_file_path_sh );
		return destination_file_path;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static byte[] extractImageToMemory( 
		HAPI_NodeId material_node_id, string image_file_format_name, string image_planes )
	{
#if ( HAPI_ENABLE_RUNTIME )
		int buffer_size = 0;

		HAPI_Result status_code = HAPI_ExtractImageToMemory(
			ref mySession, material_node_id, image_file_format_name, image_planes, out buffer_size );
		processStatusCode( status_code );

		byte[] buffer = new byte[ buffer_size ];
		status_code = HAPI_GetImageMemoryBuffer( ref mySession, material_node_id, buffer, buffer_size );
		processStatusCode( status_code );

		return buffer;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static int getSupportedImageFileFormatCount()
	{
#if ( HAPI_ENABLE_RUNTIME )
		int format_count = 0;
		HAPI_Result status_code = HAPI_GetSupportedImageFileFormatCount( ref mySession, out format_count );
		processStatusCode( status_code );
		return format_count;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static HAPI_ImageFileFormat[] getSupportedImageFileFormats()
	{
#if ( HAPI_ENABLE_RUNTIME )
		int format_count = getSupportedImageFileFormatCount();
		HAPI_ImageFileFormat[] formats = new HAPI_ImageFileFormat[ format_count ];
		HAPI_Result status_code = HAPI_GetSupportedImageFileFormats( ref mySession, formats, format_count );
		processStatusCode( status_code );
		return formats;
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
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_SetAnimCurve(
			ref mySession, node_id, parm_id, parm_index, curve_keyframes, keyframe_count );
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
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_SetTransformAnimCurve(
			ref mySession, node_id, transform_component, curve_keyframes, keyframe_count );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void resetSimulation( HAPI_NodeId node_id )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_ResetSimulation( ref mySession, node_id );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	// VOLUMES --------------------------------------------------------------------------------------------------

	public static HAPI_VolumeInfo getVolumeInfo( HAPI_NodeId node_id, HAPI_PartId part_id )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_VolumeInfo volume_info = new HAPI_VolumeInfo();
		HAPI_Result status_code = HAPI_GetVolumeInfo( ref mySession, node_id, part_id, ref volume_info );
		processStatusCode( status_code );
		return volume_info;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static HAPI_VolumeTileInfo getFirstVolumeTile( HAPI_NodeId node_id, HAPI_PartId part_id )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_VolumeTileInfo tile = new HAPI_VolumeTileInfo();
		HAPI_Result status_code = HAPI_GetFirstVolumeTile( ref mySession, node_id, part_id, ref tile );
		processStatusCode( status_code );
		return tile;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static HAPI_VolumeTileInfo getNextVolumeTile(
		HAPI_NodeId node_id, HAPI_PartId part_id )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_VolumeTileInfo next = new HAPI_VolumeTileInfo();
		HAPI_Result status_code = HAPI_GetNextVolumeTile( ref mySession, node_id, part_id, ref next );
		processStatusCode( status_code );
		return next;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void getVolumeTileFloatData(
		HAPI_NodeId node_id, HAPI_PartId part_id,
		ref HAPI_VolumeTileInfo tile, [Out] float[] values )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_GetVolumeTileFloatData(
			ref mySession, node_id, part_id, 0.0f, ref tile, values, values.Length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void getHeightFieldData(
		HAPI_NodeId node_id, HAPI_PartId part_id,
		[Out] float[] values, int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_GetHeightFieldData(
			ref mySession, node_id, part_id, values, start, length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	// CURVES ---------------------------------------------------------------------------------------------------

	public static HAPI_CurveInfo getCurveInfo( HAPI_NodeId node_id, HAPI_PartId part_id )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_CurveInfo curve_info = new HAPI_CurveInfo();
		HAPI_Result status_code = HAPI_GetCurveInfo( ref mySession, node_id, part_id, ref curve_info );
		processStatusCode( status_code );
		return curve_info;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static int[] getCurveCounts(
		HAPI_NodeId node_id, HAPI_PartId part_id,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		int[] counts = new int[ length ];
		HAPI_Result status_code = HAPI_GetCurveCounts(
			ref mySession, node_id, part_id, counts, start, length );
		processStatusCode( status_code );
		return counts;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static int[] getCurveOrders(
		HAPI_NodeId node_id, HAPI_PartId part_id,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		int[] orders = new int[ length ];
		HAPI_Result status_code = HAPI_GetCurveOrders(
			ref mySession, node_id, part_id, orders, start, length );
		processStatusCode( status_code );
		return orders;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static float[] getCurveKnots(
		HAPI_NodeId node_id, HAPI_PartId part_id,
		int start, int length )
	{
#if ( HAPI_ENABLE_RUNTIME )
		float[] knots = new float[ length ];
		HAPI_Result status_code = HAPI_GetCurveKnots(
			ref mySession, node_id, part_id, knots, start, length );
		processStatusCode( status_code );
		return knots;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	// BASIC PRIMITIVES -----------------------------------------------------------------------------------------

	public static HAPI_BoxInfo
	getBoxInfo( HAPI_NodeId geo_node_id, HAPI_PartId part_id )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_BoxInfo box_info = new HAPI_BoxInfo( true );
		HAPI_Result status_code = HAPI_GetBoxInfo(
			ref mySession, geo_node_id, part_id, ref box_info );
		processStatusCode( status_code );
		return box_info;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static HAPI_SphereInfo
	getSphereInfo( HAPI_NodeId geo_node_id, HAPI_PartId part_id )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_SphereInfo sphere_info = new HAPI_SphereInfo( true );
		HAPI_Result status_code = HAPI_GetSphereInfo(
			ref mySession, geo_node_id, part_id, ref sphere_info );
		processStatusCode( status_code );
		return sphere_info;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	// CACHING --------------------------------------------------------------------------------------------------

	public static string[] getActiveCacheNames()
	{
#if ( HAPI_ENABLE_RUNTIME )
		int cache_count = 0;
		HAPI_Result status_code = HAPI_GetActiveCacheCount(
			ref mySession, out cache_count );
		processStatusCode( status_code );

		int[] cache_names = new int[ cache_count ];
		status_code = HAPI_GetActiveCacheNames(
			ref mySession, cache_names, cache_count );
		processStatusCode( status_code );

		string[] name_strings = new string[ cache_count ];
		for ( int i = 0; i < cache_count; ++i )
			name_strings[ i ] = getString( cache_names[ i ] );

		return name_strings;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static int getCacheProperty(
		string cache_name, HAPI_CacheProperty cache_property )
	{
#if ( HAPI_ENABLE_RUNTIME )
		int property_value = 0;
		HAPI_Result status_code = HAPI_GetCacheProperty(
			ref mySession, cache_name, cache_property, out property_value );
		processStatusCode( status_code );

		return property_value;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void setCacheProperty(
		string cache_name, HAPI_CacheProperty cache_property, int property_value )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_SetCacheProperty(
			ref mySession, cache_name, cache_property, property_value );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void saveGeoToFile(
		HAPI_NodeId node_id, 
		string file_name )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_SaveGeoToFile(
			ref mySession, node_id, file_name );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void loadGeoFromFile(
		HAPI_NodeId node_id, 
		string file_name )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_LoadGeoFromFile(
			ref mySession, node_id, file_name );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static byte[] saveGeoToMemory(
		HAPI_NodeId node_id, 
		string format )
	{
#if ( HAPI_ENABLE_RUNTIME )
		int size;
		HAPI_Result status_code = HAPI_GetGeoSize(
			ref mySession, node_id, format, out size );
		processStatusCode( status_code );

		byte[] memory = new byte[ size ];
		status_code = HAPI_SaveGeoToMemory(
			ref mySession, node_id, memory, size );
		processStatusCode( status_code );

		return memory;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}

	public static void loadGeoFromMemory(
		HAPI_NodeId node_id,
		string format, byte[] buffer )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_LoadGeoFromMemory(
			ref mySession, node_id, format, buffer, buffer.Length );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif
	}
}
