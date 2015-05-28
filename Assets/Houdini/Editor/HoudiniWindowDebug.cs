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
#if !( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
	#pragma warning disable 0414
	#pragma warning disable 0649
#endif // !( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )

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
		// Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow< HoudiniWindowDebug >(
			false, HoudiniConstants.HAPI_PRODUCT_SHORT_NAME + " " + HoudiniGUIUtility.myDebugLabel );
		
		float time = HoudiniHost.getTime();
		HoudiniWindowDebug.myTime = time;
		HoudiniWindowDebug.myLockNodesOnHipSave = false;
	}
	
	public void OnGUI() 
	{
		bool gui_enable = GUI.enabled;

#if !( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HoudiniGUI.help( HoudiniConstants.HAPI_UNSUPPORTED_PLATFORM_MSG, MessageType.Info );
		GUI.enabled = false;
#else
		if ( !HoudiniHost.isInstallationOk() )
		{
			HoudiniGUI.help(
				HoudiniHost.getMissingEngineInstallHelpString(), MessageType.Info );
			GUI.enabled = false;
		}
#endif // !( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		
		if ( GUILayout.Button( HoudiniGUIUtility.myLoadAssetLabel ) )
		{
			string asset_file_path = HoudiniGUIUtility.promptForOTLPath();
			HoudiniAssetUtility.instantiateAsset( asset_file_path );
		}

		if ( GUILayout.Button( HoudiniGUIUtility.myLoadHipLabel ) )
		{
			string hip_file_path = HoudiniGUIUtility.promptForHIPPath();
			HoudiniAssetUtility.loadHipFile( hip_file_path );
		}

		HoudiniGUI.separator();

		if ( GUILayout.Button( HoudiniGUIUtility.mySaveHoudiniSceneLabel ) )
		{
			HAPI_License license = HoudiniHost.getCurrentLicense();
			string ext = "hip";

			if ( license == HAPI_License.HAPI_LICENSE_HOUDINI_ENGINE_INDIE
				|| license == HAPI_License.HAPI_LICENSE_HOUDINI_INDIE )
			{
				ext = "hiplc";
			}

			string hip_file_path = EditorUtility.SaveFilePanel( "Save HIP File", "", "hscene", ext );
			if ( hip_file_path != "" )
				HoudiniHost.saveScene( hip_file_path, myLockNodesOnHipSave );
		}
		{
			bool undo_value = myLockNodesOnHipSave;
			HoudiniGUI.toggle(
				"lock_nodes", "Lock Nodes On Scene Save",
				ref myLockNodesOnHipSave, null, ref undo_value );
		}

		HoudiniGUI.separator();

		if ( GUILayout.Button( "Check for New Untracked Asset Nodes" ) )
		{
			HoudiniAssetUtility.checkForNewAssets();
		}

		HoudiniGUI.separator();

		if ( HoudiniGUI.floatField( "global_time", "Global Time", ref myTime, null, ref myTime ) )
		{
			HoudiniHost.setTime( myTime );
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

#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
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
#endif // ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )

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
		HoudiniAssetUtility.instantiateAsset( fi.DirectoryName + "\\" + fi.Name );
	}

	private static List< OTLDirectory >	myOTLDirectories  	= new List< OTLDirectory >();
	
	private static float 			myLineHeight 			= 16;
	private static GUILayoutOption 	myLineHeightGUI 		= GUILayout.Height( myLineHeight );
	private static Vector2 			myScrollPosition;
	private static float			myTime;

	private static bool myLockNodesOnHipSave;

#if !( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
	#pragma warning restore 0414
	#pragma warning restore 0649
#endif // !( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
}
