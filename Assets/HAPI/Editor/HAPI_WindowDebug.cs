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
 * 		Only a test class. Not used currently.
 * 
 */

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using HAPI;

public class HAPI_WindowDebug : EditorWindow 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public

	class OTLDirectory
	{
		public bool myExpanded;
		public string myDirectoryName;
		public string myDirectoryPath;
	}

	public static void ShowWindow() 
	{
		float time = 0.0f;
		// Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow< HAPI_WindowDebug >( false, HAPI_Constants.HAPI_PRODUCT_SHORT_NAME +
													" " + HAPI_GUIUtility.myDebugLabel );
		
		HAPI_Host.getTime( out time );
		HAPI_WindowDebug.myTime = time;
	}
	
	public void OnGUI() 
	{
		
		if ( GUILayout.Button( HAPI_GUIUtility.mySaveHoudiniSceneLabel ) )
		{
			string hip_file_path = EditorUtility.SaveFilePanel( "Save HIP File", "", "hscene.hip", "hip" );
			if ( hip_file_path != "" && HAPI_Host.hasScene() )
				HAPI_Host.saveScene( hip_file_path );
			else
				Debug.LogError( "Nothing to save." );
		}
		
		if ( GUILayout.Button( HAPI_GUIUtility.myLoadAssetLabel ) )
		{
			string asset_file_path = HAPI_GUIUtility.promptForOTLPath();
			HAPI_GUIUtility.instantiateAsset( asset_file_path );
		}

		if ( HAPI_GUI.floatField( "global_time", "Global Time", ref myTime ) )
		{
			try
			{
				if ( !HAPI.HAPI_SetPath.prIsPathSet )
				{
					HAPI.HAPI_SetPath.setPath();
					if ( !HAPI.HAPI_SetPath.prIsPathSet )
					{
						Debug.LogError( "Cannot build asset as Houdini dlls not found!" );
						return;
					}
					HAPI_Host.initialize();
				}
				HAPI_Host.setTime( myTime );
			}
			catch ( HAPI_Error error )
			{
				Debug.LogError( error.ToString() );
			}
		}

		HAPI_GUI.separator();
		
		
		string path = Application.dataPath;
		DirectoryInfo di = new DirectoryInfo( path + "//OTLs" );
		
		myScrollPosition = GUILayout.BeginScrollView( myScrollPosition );

		if ( GUILayout.Button( "Instantiate Core Assets" ) )
		{
			try
			{
				DirectoryInfo core = new DirectoryInfo( path + "//OTLs/Core" );
					
				if ( !core.Exists )
					throw new HAPI_Error( "Project/Assets/OTLs/Core directory does not exist!" );
					
				foreach ( FileInfo fi in core.GetFiles() )
					if ( fi.Extension == ".otl" )
						loadOTL( fi );
			}
			catch ( System.Exception e )
			{
				Debug.LogError( "Directory navigation failed: " + e.ToString() );
			}
		}

		try
		{
			if ( !di.Exists )
				throw new HAPI_Error( "Project/Assets/OTLs directory does not exist!" );
				
			foreach ( DirectoryInfo child_directory in di.GetDirectories() )
			{
				OTLDirectory otlDir = null;
					
				foreach ( OTLDirectory existingOTLDir in myOTLDirectories )
				{
					if ( existingOTLDir.myDirectoryName == child_directory.Name )
					{
						otlDir = existingOTLDir;
						break;
					}
				}
					
				if ( otlDir == null )
				{
					otlDir = new OTLDirectory();
					otlDir.myDirectoryName = child_directory.Name;
					otlDir.myDirectoryPath = child_directory.FullName;
					otlDir.myExpanded = false;
					myOTLDirectories.Add( otlDir );
				}
					
				otlDir.myExpanded = 
							EditorGUILayout.Foldout( otlDir.myExpanded, new GUIContent( otlDir.myDirectoryName ) );
					
				if ( otlDir.myDirectoryName == "Core" )
					otlDir.myExpanded = true;
					
				if ( otlDir.myExpanded )
				{
					DirectoryInfo dirContents = new DirectoryInfo( otlDir.myDirectoryPath );
						
					foreach ( FileInfo fi in dirContents.GetFiles() )
					if ( fi.Extension == ".otl" )
						genOTLEntry( fi );
				}
			}
		}
		catch ( System.Exception e )
		{
			Debug.LogError( "Directory navigation failed: " + e.ToString() );
		}

		GUILayout.EndScrollView();
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private

	private void genOTLEntry( FileInfo fi )
	{
		bool load_file = false;
		EditorGUILayout.BeginHorizontal(); 
		{
			load_file = GUILayout.Button( "Instantiate", GUILayout.Width( 100 ) );
			EditorGUILayout.SelectableLabel( fi.Name, myLineHeightGUI );
		}
		EditorGUILayout.EndHorizontal();
		
		if ( load_file )
			loadOTL( fi );
	}
	
	private void loadOTL( FileInfo fi )
	{
		HAPI_GUIUtility.instantiateAsset( fi.DirectoryName + "\\" + fi.Name );
	}
	
	private static List< OTLDirectory >	myOTLDirectories  	= new List< OTLDirectory >();
	
	private static float 			myLineHeight 			= 16;
	private static GUILayoutOption 	myLineHeightGUI 		= GUILayout.Height( myLineHeight );
	private static Vector2 			myScrollPosition;
	private static float			myTime;
}
