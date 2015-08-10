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

	public static int prSessionID {
		get { return HoudiniDataFile.getInt( "ServerSessionID", -1 ); } 
		private set { HoudiniDataFile.setInt( "ServerSessionID", value ); } }
	public static int prProcessID {
		get { return HoudiniDataFile.getInt( "ServerProcessID", -1 ); } 
		private set { HoudiniDataFile.setInt( "ServerProcessID", value ); } }

	private static bool initializeSession()
	{
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

#if UNITY_STANDALONE_OSX // TODO: Add support HARS on OSX.
		mySession.id = 0;
		mySession.type = HAPI_SessionType.HAPI_SESSION_INPROCESS;
		return true;
#else
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
			return true;
		}
#endif // UNITY_STANDALONE_OSX
	}
	
	private static bool startProcess( string server_executable_path, string pipe_name )
	{
		try
		{
			System.Diagnostics.Process process = new System.Diagnostics.Process();
			
			// Stop the process from opening a new window.
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.CreateNoWindow = true;
			
			// Setup executable and parameters.
			process.StartInfo.FileName = server_executable_path;
			process.StartInfo.Arguments = "--named-pipe " + pipe_name + " --auto-close";
			
			// Go.
			process.Start();

			// Wait for the process to start.
			System.Threading.Thread.Sleep( 1000 );

			prProcessID = process.Id;
		
			return true;
		}
		catch ( System.Exception e )
		{
			prLastInitializationError =
				"Could not start the RPC server process because: " + e.Message + "\n" +
				"Make sure " + prServerExecutablePath + " exists and try to re-initialize again from: \n" +
				"Houdini Engine > Debug Window > Reinitialize";
			return false;
		}
	}

	private static bool getSession( out HAPI_Session session, string pipe_name )
	{
		HAPI_Result status_code = HAPI_CreateThriftNamedPipeSession( out session, prPipeName );
		if ( status_code != HAPI_Result.HAPI_RESULT_SUCCESS )
		{
			prLastInitializationError =
				"Could not create the RPC pipe session.\n" +
				"Make sure " + prServerExecutablePath + " exists and try to re-initialize again from: \n" +
				"Houdini Engine > Debug Window > Reinitialize";
			return false;
		}
		prSessionID = session.id;
		return true;
	}
}
