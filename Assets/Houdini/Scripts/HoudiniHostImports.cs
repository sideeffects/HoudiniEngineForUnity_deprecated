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
using HAPI_AssetLibraryId = System.Int32;
using HAPI_NodeId = System.Int32;
using HAPI_NodeTypeBits = System.Int32;
using HAPI_NodeFlagsBits = System.Int32;
using HAPI_ParmId = System.Int32;
using HAPI_PartId = System.Int32;

/// <summary>
/// 	Singleton Houdini host object that maintains the singleton Houdini scene and all access to the
/// 	Houdini runtime.
/// </summary>
public static partial class HoudiniHost
{
#if ( HAPI_ENABLE_RUNTIME )
	// SESSIONS -------------------------------------------------------------------------------------------------

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_CreateInProcessSession( out HAPI_Session session );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_StartThriftSocketServer(
		ref HAPI_ThriftServerOptions options,
		int port,
		out int process_id );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_CreateThriftSocketSession(
		out HAPI_Session session, string host_name, int port );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_StartThriftNamedPipeServer(
		ref HAPI_ThriftServerOptions options,
		string pipe_name,
		out int process_id );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_CreateThriftNamedPipeSession(
		out HAPI_Session session, string pipe_name );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_BindCustomImplementation( HAPI_SessionType session_type, string dll_path );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_CreateCustomSession( HAPI_SessionType session_type, byte[] session_info, out HAPI_Session session );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_IsSessionValid( ref HAPI_Session session );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_CloseSession( ref HAPI_Session session );

	// INITIALIZATION / CLEANUP ---------------------------------------------------------------------------------

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_IsInitialized( ref HAPI_Session session );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_Initialize(
		ref HAPI_Session session,
		ref HAPI_CookOptions cook_options,
		[ MarshalAs( UnmanagedType.U1 ) ] bool use_cooking_thread,
		int cooking_thread_stack_size,
		string houdini_environment_files,
		string otl_search_path,
		string dso_search_path,
		string image_dso_search_path,
		string audio_dso_search_path );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_Cleanup( ref HAPI_Session session );

	// DIAGNOSTICS ----------------------------------------------------------------------------------------------

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetEnvInt( HAPI_EnvIntType int_type, out int value );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetSessionEnvInt( ref HAPI_Session session, HAPI_SessionEnvIntType int_type, out int value );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetServerEnvInt( ref HAPI_Session session, string variable_name, out int value );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetServerEnvString( ref HAPI_Session session, string variable_name, out HAPI_StringHandle value );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SetServerEnvInt( ref HAPI_Session session, string variable_name, int value );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SetServerEnvString( ref HAPI_Session session, string variable_name, string value );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetStatus( ref HAPI_Session session, HAPI_StatusType status_code, out int status );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetStatusStringBufLength(
		ref HAPI_Session session,
		HAPI_StatusType status_code,
		HAPI_StatusVerbosity verbosity,
		out int buffer_size );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetStatusString(
		ref HAPI_Session session,
		HAPI_StatusType status_type,
		StringBuilder string_value,
		int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_ComposeNodeCookResult(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		HAPI_StatusVerbosity verbosity,
		out int buffer_size );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetComposedNodeCookResult(
		ref HAPI_Session session,
		StringBuilder string_value,
		int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_CheckForSpecificErrors(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		HAPI_ErrorCodeBits errors_to_look_for,
		out HAPI_ErrorCodeBits errors_found );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetCookingTotalCount( ref HAPI_Session session, out int count );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetCookingCurrentCount( ref HAPI_Session session, out int count );

	// UTILITY --------------------------------------------------------------------------------------------------
		
	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_ConvertTransform(
		ref HAPI_Session session,
		ref HAPI_TransformEuler transform_in,
		HAPI_RSTOrder rst_order,
		HAPI_XYZOrder rot_order,
		out HAPI_TransformEuler transform_out );
		
	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_ConvertMatrixToQuat(
		ref HAPI_Session session,
		float[] matrix,
		HAPI_RSTOrder rst_order,
		ref HAPI_Transform transform_out );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_ConvertMatrixToEuler(
		ref HAPI_Session session,
		float[] matrix,
		HAPI_RSTOrder rst_order,
		HAPI_XYZOrder rot_order,
		ref HAPI_TransformEuler transform_out );
		
	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_ConvertTransformQuatToMatrix(
		ref HAPI_Session session,
		ref HAPI_Transform transform,
		[Out] float[] matrix );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_ConvertTransformEulerToMatrix(
		ref HAPI_Session session,
		ref HAPI_TransformEuler transform,
		[Out] float[] matrix );
		
	// STRINGS --------------------------------------------------------------------------------------------------
		
	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetStringBufLength(
		ref HAPI_Session session,
		HAPI_StringHandle string_handle,
		out int buffer_length );
		
	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetString(
		ref HAPI_Session session,
		HAPI_StringHandle string_handle,
		StringBuilder string_value,
		int length );

	// TIME -----------------------------------------------------------------------------------------------------

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetTime( ref HAPI_Session session, out float time );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SetTime( ref HAPI_Session session, float time );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetTimelineOptions( ref HAPI_Session session, ref HAPI_TimelineOptions timeline_options );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SetTimelineOptions( ref HAPI_Session session, ref HAPI_TimelineOptions timeline_options );

	// ASSETS ---------------------------------------------------------------------------------------------------

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_LoadAssetLibraryFromFile(
		ref HAPI_Session session,
		string file_path,
		[ MarshalAs( UnmanagedType.U1 ) ] bool allow_overwrite,
		out HAPI_AssetLibraryId library_id );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_LoadAssetLibraryFromMemory(
		ref HAPI_Session session,
		byte[] library_buffer, int library_buffer_length,
		[ MarshalAs( UnmanagedType.U1 ) ] bool allow_overwrite,
		out HAPI_AssetLibraryId library_id );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetAvailableAssetCount(
		ref HAPI_Session session,
		HAPI_AssetLibraryId library_id,
		out int asset_count );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result 
	HAPI_GetAvailableAssets(
		ref HAPI_Session session,
		HAPI_AssetLibraryId library_id,
		[Out] HAPI_StringHandle[] asset_names_array,
		int asset_count );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetAssetInfo( ref HAPI_Session session, HAPI_NodeId node_id, ref HAPI_AssetInfo asset_info );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_Interrupt( ref HAPI_Session session );

	// HIP FILES ------------------------------------------------------------------------------------------------

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_LoadHIPFile(
		ref HAPI_Session session,
		string file_name,
		[ MarshalAs( UnmanagedType.U1 ) ] bool cook_on_load );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SaveHIPFile(
		ref HAPI_Session session,
		string file_name,
		[ MarshalAs( UnmanagedType.U1 ) ] bool lock_nodes );

	// NODES ----------------------------------------------------------------------------------------------------

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_IsNodeValid(
		ref HAPI_Session session,
		HAPI_NodeId node_id, int unique_node_id,
		[ MarshalAs( UnmanagedType.U1 ) ] ref bool answer );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetNodeInfo( ref HAPI_Session session, HAPI_NodeId node_id, out HAPI_NodeInfo node_info );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetNodePath(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		HAPI_NodeId relative_to_node_id,
		out HAPI_StringHandle path );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetManagerNodeId(
		ref HAPI_Session session,
		HAPI_NodeType node_type,
		out HAPI_NodeId node_id );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_ComposeChildNodeList(
		ref HAPI_Session session,
		HAPI_NodeId parent_node_id,
		HAPI_NodeTypeBits node_type_filter,
		HAPI_NodeFlagsBits node_flags_filter,
		[ MarshalAs( UnmanagedType.U1 ) ] bool recursive,
		out int count );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetComposedChildNodeList(
		ref HAPI_Session session,
		HAPI_NodeId parent_node_id,
		[Out] HAPI_NodeId[] child_node_ids_array,
		int count );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_CreateNode(
		ref HAPI_Session session,
		HAPI_NodeId parent_node_id,
		string operator_name,
		string node_label,
		[ MarshalAs( UnmanagedType.U1 ) ] bool cook_on_creation,
		out HAPI_NodeId new_node_id );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_CreateInputNode(
		ref HAPI_Session session,
		out HAPI_NodeId node_id,
		string name );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_CookNode(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		ref HAPI_CookOptions cook_options );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_DeleteNode( ref HAPI_Session session, HAPI_NodeId node_id );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_RenameNode( ref HAPI_Session session, HAPI_NodeId node_id, string new_name );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_ConnectNodeInput( ref HAPI_Session session, HAPI_NodeId node_id, int input_index, HAPI_NodeId node_id_to_connect );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_DisconnectNodeInput( ref HAPI_Session session, HAPI_NodeId node_id, int input_index );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_QueryNodeInput(
		ref HAPI_Session session,
		HAPI_NodeId node_to_query,
		int input_index,
		out HAPI_NodeId connected_node_id );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetNodeInputName(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		int input_idx,
		out HAPI_StringHandle name );

	// PARAMETERS -----------------------------------------------------------------------------------------------

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetParameters(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		[Out] HAPI_ParmInfo[] parm_infos_array,
		int start, int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetParmInfo(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		HAPI_ParmId parm_id,
		out HAPI_ParmInfo parm_info );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetParmIdFromName(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		string parm_name,
		out HAPI_ParmId parm_id );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetParmInfoFromName(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		string parm_name,
		out HAPI_ParmInfo parm_info );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetParmTagName(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		HAPI_ParmId parm_id,
		int tag_index,
		out HAPI_StringHandle tag_name );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetParmTagValue(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		HAPI_ParmId parm_id,
		string tag_name,
		out HAPI_StringHandle tag_value );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_ParmHasTag(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		HAPI_ParmId parm_id,
		string tag_name,
		[ MarshalAs( UnmanagedType.U1 ) ] ref bool has_tag );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetParmWithTag(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		string tag_name,
		ref HAPI_ParmId parm_id );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetParmIntValue(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		string parm_name,
		int index, out int value );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetParmIntValues(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		[Out] int[] values_array,
		int start, int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetParmFloatValue(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		string parm_name,
		int index, out float value );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetParmFloatValues(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		[Out] float[] values_array,
		int start, int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetParmStringValue(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		string parm_name,
		int index,
		[ MarshalAs( UnmanagedType.U1 ) ] bool evaluate,
		out HAPI_StringHandle value );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetParmStringValues(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		[ MarshalAs( UnmanagedType.U1 ) ] bool evaluate,
		[Out] HAPI_StringHandle[] values_array,
		int start, int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetParmNodeValue(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		string parm_name,
		out HAPI_NodeId value );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetParmFile(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		string parm_name,
		string destination_directory,
		string destination_file_name );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetParmChoiceLists(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		[Out] HAPI_ParmChoiceInfo[] parm_choices_array,
		int start, int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SetParmIntValue(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		string parm_name,
		int index, int value );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SetParmIntValues(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		int[] values_array,
		int start, int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SetParmFloatValue(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		string parm_name,
		int index, float value );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SetParmFloatValues(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		float[] values_array,
		int start, int length );
		
	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SetParmStringValue(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		string value,
		HAPI_ParmId parm_id,
		int index );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SetParmNodeValue(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		string parm_name,
		HAPI_NodeId value );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_InsertMultiparmInstance(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		HAPI_ParmId parm_id,
		int instance_position );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_RemoveMultiparmInstance(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		HAPI_ParmId parm_id,
		int instance_position );
		
	// HANDLES --------------------------------------------------------------------------------------------------

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetHandleInfo(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		[Out] HAPI_HandleInfo[] handle_infos_array,
		int start, int length );
		
	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetHandleBindingInfo(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		int handle_index,
		[Out] HAPI_HandleBindingInfo[] handle_binding_infos_array,
		int start, int length );
		
	// PRESETS --------------------------------------------------------------------------------------------------
		
	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetPresetBufLength(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		HAPI_PresetType preset_type,
		string preset_name,
		ref int buffer_length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetPreset(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		[Out] byte[] preset,
		int buffer_length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SetPreset(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		HAPI_PresetType preset_type,
		string preset_name,
		byte[] preset,
		int buffer_length );

	// OBJECTS --------------------------------------------------------------------------------------------------

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetObjectInfo(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		out HAPI_ObjectInfo object_info );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetObjectTransform(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		HAPI_NodeId relative_to_node_id,
		HAPI_RSTOrder rst_order,
		out HAPI_Transform transform );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_ComposeObjectList(
		ref HAPI_Session session,
		HAPI_NodeId parent_node_id,
		string categories,
		out int object_count );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetComposedObjectList(
		ref HAPI_Session session,
		HAPI_NodeId parent_node_id,
		[Out] HAPI_ObjectInfo[] object_infos_array,
		int start, int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetComposedObjectTransforms(
		ref HAPI_Session session,
		HAPI_NodeId parent_node_id,
		HAPI_RSTOrder rst_order,
		[Out] HAPI_Transform[] transform_array,
		int start, int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetInstancedObjectIds(
		ref HAPI_Session session,
		HAPI_NodeId object_node_id,
		[Out] HAPI_NodeId[] instanced_node_id_array,
		int start, int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetInstanceTransforms(
		ref HAPI_Session session,
		HAPI_NodeId object_node_id,
		HAPI_RSTOrder rst_order,
		[Out] HAPI_Transform[] transforms_array,
		int start, int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SetObjectTransform(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		ref HAPI_TransformEuler transform );

	// GEOMETRY GETTERS -----------------------------------------------------------------------------------------

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetDisplayGeoInfo(
		ref HAPI_Session session,
		HAPI_NodeId object_node_id,
		out HAPI_GeoInfo geo_info );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetGeoInfo(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		out HAPI_GeoInfo geo_info );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetPartInfo(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		out HAPI_PartInfo part_info );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetFaceCounts(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		[Out] int[] face_counts_array,
		int start, int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetVertexList(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		[Out] int[] vertex_list_array,
		int start, int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetAttributeInfo(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		string name, HAPI_AttributeOwner owner,
		ref HAPI_AttributeInfo attr_info );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetAttributeNames(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		HAPI_AttributeOwner owner,
		[Out] HAPI_StringHandle[] attribute_names_array,
		int count );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetAttributeIntData(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		string name,
		ref HAPI_AttributeInfo attr_info,
		int stride,
		[Out] int[] data,
		int start, int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetAttributeInt64Data(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		string name,
		ref HAPI_AttributeInfo attr_info,
		int stride,
		[Out] HAPI_Int64[] data,
		int start, int length );
		
	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetAttributeFloatData(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		string name,
		ref HAPI_AttributeInfo attr_info,
		int stride,
		[Out] float[] data_array,
		int start, int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetAttributeFloat64Data(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		string name,
		ref HAPI_AttributeInfo attr_info,
		int stride,
		[Out] double[] data_array,
		int start, int length );
		
	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetAttributeStringData(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		string name,
		ref HAPI_AttributeInfo attr_info,
		[Out] int[] data_array,
		int start, int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetGroupNames(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		HAPI_GroupType group_type,
		[Out] HAPI_StringHandle[] group_names_array,
		int group_count );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetGroupMembership(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		HAPI_GroupType group_type,
		string group_name,
		[ MarshalAs( UnmanagedType.U1 ) ] ref bool membership_array_all_equal,
		[Out] int[] membership_array,
		int start, int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetInstancedPartIds(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		[Out] HAPI_PartId[] instanced_parts_array,
		int start, int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetInstancerPartTransforms(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		HAPI_RSTOrder rst_order,
		[Out] HAPI_Transform[] instanced_parts_array,
		int start, int length );

	// GEOMETRY SETTERS -----------------------------------------------------------------------------------------

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SetPartInfo(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		ref HAPI_PartInfo part_info );
		
	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SetFaceCounts(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		int[] face_counts_array,
		int start, int length );
		
	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SetVertexList(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		int[] vertex_list_array,
		int start, int length );
		
	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_AddAttribute(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		string name,
		ref HAPI_AttributeInfo attr_info );
		
	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SetAttributeIntData(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		string name,
		ref HAPI_AttributeInfo attr_info,
		int[] data_array,
		int start, int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SetAttributeInt64Data(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		string name,
		ref HAPI_AttributeInfo attr_info,
		HAPI_Int64[] data_array,
		int start, int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SetAttributeFloatData(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		string name,
		ref HAPI_AttributeInfo attr_info,
		float[] data_array,
		int start, int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SetAttributeFloat64Data(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		string name,
		ref HAPI_AttributeInfo attr_info,
		double[] data_array,
		int start, int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SetAttributeStringData(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		string name,
		ref HAPI_AttributeInfo attr_info,
		string[] data_array,
		int start, int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_AddGroup(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		HAPI_GroupType group_type,
		string group_name );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SetGroupMembership(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		HAPI_GroupType group_type,
		string group_name,
		[Out] int[] membership_array,
		int start, int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_CommitGeo(
		ref HAPI_Session session,
		HAPI_NodeId node_id );
		
	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_RevertGeo(
		ref HAPI_Session session,
		HAPI_NodeId node_id );

	// MATERIALS ------------------------------------------------------------------------------------------------

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetMaterialNodeIdsOnFaces(
		ref HAPI_Session session,
		HAPI_NodeId geometry_node_id, HAPI_PartId part_id,
		[ MarshalAs( UnmanagedType.U1 ) ] ref bool are_all_the_same,
		[Out] HAPI_NodeId[] material_ids_array,
		int start, int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetMaterialInfo(
		ref HAPI_Session session,
		HAPI_NodeId material_node_id,
		out HAPI_MaterialInfo material_info );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_RenderCOPToImage(
		ref HAPI_Session session,
		HAPI_NodeId cop_node_id );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_RenderTextureToImage(
		ref HAPI_Session session,
		HAPI_NodeId material_node_id,
		HAPI_ParmId parm_id );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetImageInfo(
		ref HAPI_Session session,
		HAPI_NodeId material_node_id,
		out HAPI_ImageInfo image_info );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SetImageInfo(
		ref HAPI_Session session,
		HAPI_NodeId material_node_id,
		ref HAPI_ImageInfo image_info );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetImagePlaneCount(
		ref HAPI_Session session,
		HAPI_NodeId material_node_id,
		out int image_plane_count );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetImagePlanes(
		ref HAPI_Session session,
		HAPI_NodeId material_node_id,
		[Out] HAPI_StringHandle[] image_planes_array,
		int image_plane_count );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_ExtractImageToFile(
		ref HAPI_Session session,
		HAPI_NodeId material_node_id,
		string image_file_format_name,
		string image_planes,
		string destination_folder_path,
		string destination_file_name,
		out int destination_file_path );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_ExtractImageToMemory(
		ref HAPI_Session session,
		HAPI_NodeId material_node_id,
		string image_file_format_name,
		string image_planes,
		out int buffer_size );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetImageMemoryBuffer(
		ref HAPI_Session session,
		HAPI_NodeId material_node_id,
		[Out] byte[] buffer,
		int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetSupportedImageFileFormatCount( ref HAPI_Session session, out int file_format_count );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetSupportedImageFileFormats(
		ref HAPI_Session session,
		[Out] HAPI_ImageFileFormat[] formats_array,
		int file_format_count );

	// SIMULATION/ANIMATIONS ------------------------------------------------------------------------------------

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SetAnimCurve(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_ParmId parm_id,
		int parm_index,
		HAPI_Keyframe[] curve_keyframes_array,
		int keyframe_count );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SetTransformAnimCurve(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_TransformComponent transform_component,
		HAPI_Keyframe[] curve_keyframes_array,
		int keyframe_count );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_ResetSimulation( ref HAPI_Session session, HAPI_NodeId node_id );

	// VOLUMES --------------------------------------------------------------------------------------------------

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetVolumeInfo(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		ref HAPI_VolumeInfo volume_info );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetFirstVolumeTile(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		ref HAPI_VolumeTileInfo tile );
		
	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetNextVolumeTile(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		ref HAPI_VolumeTileInfo tile );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetVolumeVoxelFloatData(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		int x_index, int y_index, int z_index,
		[Out] float[] values_array,
		int value_count );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetVolumeTileFloatData(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		float fill_value, ref HAPI_VolumeTileInfo tile,
		[Out] float[] values_array,
		int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetVolumeVoxelIntData(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		int x_index, int y_index, int z_index,
		[Out] int[] values_array,
		int value_count );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetVolumeTileIntData(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		int fill_value, ref HAPI_VolumeTileInfo tile,
		[Out] int[] values_array,
		int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetHeightFieldData(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		[Out] float[] values_array,
		int start, int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SetVolumeInfo(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		HAPI_PartId part_id,
		ref HAPI_VolumeInfo volume_info );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SetVolumeTileFloatData(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		HAPI_PartId part_id,
		ref HAPI_VolumeTileInfo tile,
		float[] values_array,
		int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SetVolumeTileIntData(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		HAPI_PartId part_id,
		ref HAPI_VolumeTileInfo tile,
		int[] values_array,
		int length );

	// CURVES ---------------------------------------------------------------------------------------------------

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetCurveInfo(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		ref HAPI_CurveInfo curve_info );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetCurveCounts(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		[Out] int[] counts_array,
		int start, int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetCurveOrders(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		[Out] int[] orders_array,
		int start, int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetCurveKnots(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		[Out] float[] knots_array,
		int start, int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SetCurveInfo(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		ref HAPI_CurveInfo curve_info );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SetCurveCounts(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		int[] counts_array,
		int start, int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SetCurveOrders(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		int[] orders_array,
		int start, int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SetCurveKnots(
		ref HAPI_Session session,
		HAPI_NodeId node_id, HAPI_PartId part_id,
		float[] knots_array,
		int start, int length );

	// BASIC PRIMITIVES -----------------------------------------------------------------------------------------

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetBoxInfo(
		ref HAPI_Session session,
		HAPI_NodeId geo_node_id, HAPI_PartId part_id,
		ref HAPI_BoxInfo box_info );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetSphereInfo(
		ref HAPI_Session session,
		HAPI_NodeId geo_node_id, HAPI_PartId part_id,
		ref HAPI_SphereInfo sphere_info );

	// CACHING --------------------------------------------------------------------------------------------------

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetActiveCacheCount( ref HAPI_Session session, out int active_cache_count );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetActiveCacheNames(
		ref HAPI_Session session,
		[Out] HAPI_StringHandle[] cache_names_array,
		int active_cache_count );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetCacheProperty(
		ref HAPI_Session session,
		string cache_name,
		HAPI_CacheProperty cache_property,
		out int property_value );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SetCacheProperty(
		ref HAPI_Session session,
		string cache_name,
		HAPI_CacheProperty cache_property,
		int property_value );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SaveGeoToFile(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		string file_name );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_LoadGeoFromFile(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		string file_name );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_GetGeoSize(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		string format, out int size );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_SaveGeoToMemory(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		[Out] byte[] buffer, int length );

	[ DllImport( HoudiniVersion.HAPI_LIBRARY, CallingConvention = CallingConvention.Cdecl ) ]
	private static extern HAPI_Result
	HAPI_LoadGeoFromMemory(
		ref HAPI_Session session,
		HAPI_NodeId node_id,
		string format, byte[] buffer, int length );

#endif // ( HAPI_ENABLE_RUNTIME )
}

