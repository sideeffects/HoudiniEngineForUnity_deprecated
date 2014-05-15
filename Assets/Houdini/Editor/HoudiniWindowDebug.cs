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

public class HoudiniWindowDebug : EditorWindow 
{
#if !( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX )
	#pragma warning disable 0414
	#pragma warning disable 0649
#endif // !( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX )

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
		EditorWindow.GetWindow< HoudiniWindowDebug >( false, HoudiniConstants.HAPI_PRODUCT_SHORT_NAME +
													" " + HoudiniGUIUtility.myDebugLabel );
		
		HoudiniHost.getTime( out time );
		HoudiniWindowDebug.myTime = time;
	}
	
	public void OnGUI() 
	{
		bool gui_enable = GUI.enabled;

#if !( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX )
		HAPI_GUI.help( HAPI_Constants.HAPI_UNSUPPORTED_PLATFORM_MSG, MessageType.Info );
		GUI.enabled = false;
#endif // !( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX )

		if ( GUILayout.Button( HoudiniGUIUtility.mySaveHoudiniSceneLabel ) )
		{
			string hip_file_path = EditorUtility.SaveFilePanel( "Save HIP File", "", "hscene.hip", "hip" );
			if ( hip_file_path != "" && HoudiniHost.hasScene() )
				HoudiniHost.saveScene( hip_file_path );
			else
				Debug.LogError( "Nothing to save." );
		}
		
		if ( GUILayout.Button( HoudiniGUIUtility.myLoadAssetLabel ) )
		{
			string asset_file_path = HoudiniGUIUtility.promptForOTLPath();
			HoudiniGUIUtility.instantiateAsset( asset_file_path );
		}

		if ( HoudiniGUI.floatField( "global_time", "Global Time", ref myTime, null, ref myTime ) )
		{
			try
			{
				if ( !HoudiniSetPath.prIsPathSet )
				{
					HoudiniSetPath.setPath();
					if ( !HoudiniSetPath.prIsPathSet )
					{
						Debug.LogError( "Cannot build asset as Houdini dlls not found!" );
						return;
					}
					HoudiniHost.initialize();
				}
				HoudiniHost.setTime( myTime );
			}
			catch ( HoudiniError error )
			{
				Debug.LogError( error.ToString() );
			}
		}

		HoudiniGUI.separator();
		
		
		string path = Application.dataPath;
		
		myScrollPosition = GUILayout.BeginScrollView( myScrollPosition );

		if ( GUILayout.Button( "Instantiate Core Assets" ) )
		{
			try
			{
				DirectoryInfo core = new DirectoryInfo( path + "//OTLs/Core" );
					
				if ( !core.Exists )
					throw new HoudiniError( "Project/Assets/OTLs/Core directory does not exist!" );
					
				foreach ( FileInfo fi in core.GetFiles() )
					if ( fi.Extension == ".otl" )
						loadOTL( fi );
			}
			catch ( System.Exception e )
			{
				Debug.LogError( "Directory navigation failed: " + e.ToString() );
			}
		}

#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX )
		DirectoryInfo di = new DirectoryInfo( path + "//OTLs" );

		try
		{
			if ( !di.Exists )
				throw new HoudiniError( "Project/Assets/OTLs directory does not exist!" );
				
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
#endif // ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX )

		GUILayout.EndScrollView();

		GUI.enabled = gui_enable;
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
		HoudiniGUIUtility.instantiateAsset( fi.DirectoryName + "\\" + fi.Name );
	}

	private static List< OTLDirectory >	myOTLDirectories  	= new List< OTLDirectory >();
	
	private static float 			myLineHeight 			= 16;
	private static GUILayoutOption 	myLineHeightGUI 		= GUILayout.Height( myLineHeight );
	private static Vector2 			myScrollPosition;
	private static float			myTime;

#if !( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX )
	#pragma warning restore 0414
	#pragma warning restore 0649
#endif // !( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX )
}
