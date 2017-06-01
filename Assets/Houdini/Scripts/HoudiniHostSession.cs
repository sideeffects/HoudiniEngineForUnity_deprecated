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
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

public static partial class HoudiniHost
{
	public static HAPI_Session mySession;

	public static string prPipeName;
	public static string prServerExecutableName;
	public static string prServerExecutablePath;
	public static string prLibraryPath;

#if HAPI_ENABLE_RUNTIME
	public static long prSessionID {
		get { return HoudiniDataFile.getLong( "ServerSessionID", -1 ); }
		private set { HoudiniDataFile.setLong( "ServerSessionID", value ); } }
	public static int prProcessID {
		get { return HoudiniDataFile.getInt( "ServerProcessID", -1 ); }
		private set { HoudiniDataFile.setInt( "ServerProcessID", value ); } }
#else
	public static long prSessionID {
		get { return 0; }
		private set {} }
	public static int prProcessID {
		get { return 0; }
		private set {} }
#endif // HAPI_ENABLE_RUNTIME

	private static bool initializeSession()
	{
#if ( HAPI_ENABLE_RUNTIME )
		prPipeName = System.Text.RegularExpressions.Regex.Replace( Application.dataPath, "[^\\w\\._]", "__" );
#if UNITY_EDITOR_WIN
		prServerExecutableName = "HARS.exe";
		prServerExecutablePath = HoudiniSetPath.prHoudiniPath + "/" + prServerExecutableName;
		prLibraryPath = HoudiniSetPath.prHoudiniPath + "/" + HoudiniVersion.HAPI_LIBRARY;
#else
		prServerExecutableName = "HARS";
		prServerExecutablePath = HoudiniVersion.HAPI_SERVER;
		prLibraryPath = HoudiniVersion.HAPI_LIBRARY;
#endif // UNITY_EDITOR_WIN

		if ( !prHoudiniSceneExists )
		{
			if ( !startProcess( prServerExecutablePath, prPipeName ) )
				return false;

			return getSession( out mySession, prPipeName );
		}
		else
		{
			// Try to restore from data file.
			mySession.id = prSessionID;
			mySession.type = HAPI_SessionType.HAPI_SESSION_THRIFT;

			Debug.Log(
				"Houdini Engine: Recovering RPC session info from file.\n" +
				"    Datafile Path: " + HoudiniDataFile.prDataFilePath + "\n" +
				"    Session Id: " + prSessionID );

			return true;
		}
#else
		return false;
#endif // ( HAPI_ENABLE_RUNTIME )
	}

	private static bool startProcess( string server_executable_path, string pipe_name )
	{
#if ( HAPI_ENABLE_RUNTIME )
		try
		{
			Debug.Log(
				"Houdini Engine: Starting HARS process.\n" +
				"    Executable: " + prServerExecutablePath + "\n" +
				"    Pipe Name: " + pipe_name );

			int process_id = 0;
			HAPI_ThriftServerOptions server_options = new HAPI_ThriftServerOptions();
			server_options.autoClose = true;
			server_options.timeoutMs = 2000.0f;
			HAPI_Result result = HAPI_StartThriftNamedPipeServer(
				ref server_options,
				pipe_name,
				out process_id );
			if ( result != HAPI_Result.HAPI_RESULT_SUCCESS )
			{
				prLastInitializationError =
					"Could not start the RPC server process.\n" +
					"Make sure " + prServerExecutablePath + " exists.";
				return false;
			}

			prProcessID = process_id;

			return true;
		}
		catch ( System.Exception e )
		{
			prLastInitializationError =
				"Could not start the RPC server process because: " + e.Message + "\n" +
				"Make sure " + prServerExecutablePath + " exists.";
			return false;
		}
#else
		return false;
#endif // ( HAPI_ENABLE_RUNTIME )
	}

	private static bool getSession( out HAPI_Session session, string pipe_name )
	{
#if ( HAPI_ENABLE_RUNTIME )
		HAPI_Result status_code = HAPI_CreateThriftNamedPipeSession( out session, prPipeName );
		if ( status_code != HAPI_Result.HAPI_RESULT_SUCCESS )
		{
			prLastInitializationError =
				"Could not create the RPC pipe session.\n" +
				"Make sure " + prServerExecutablePath + " exists.";
			return false;
		}
		prSessionID = session.id;
#else
		session.id = 0;
		session.type = HAPI_SessionType.HAPI_SESSION_INPROCESS;
#endif // UNITY_EDITOR
		return true;
	}
}
