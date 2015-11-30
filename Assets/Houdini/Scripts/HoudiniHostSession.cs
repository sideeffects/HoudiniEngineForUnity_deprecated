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

#if UNITY_EDITOR
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
#endif // UNITY_EDITOR

	private static bool initializeSession()
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
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
#if UNITY_EDITOR
			Debug.Log(
				"Houdini Engine: Recovering RPC session info from file.\n" +
				"    Datafile Path: " + HoudiniDataFile.prDataFilePath + "\n" +
				"    Session Id: " + prSessionID );
#endif // UNITY_EDITOR
			return true;
		}
#else
		return false;
#endif // ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
	}

	private static bool startProcess( string server_executable_path, string pipe_name )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		try
		{
#if UNITY_EDITOR
			Debug.Log(
				"Houdini Engine: Starting HARS process.\n" +
				"    Executable: " + prServerExecutablePath + "\n" +
				"    Pipe Name: " + pipe_name );
#endif // UNITY_EDITOR
			int process_id = 0;
			HAPI_Result result = HAPI_StartThriftNamedPipeServer( true, pipe_name, 2000.0f, out process_id );
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
#endif // ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
	}

	private static bool getSession( out HAPI_Session session, string pipe_name )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
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
