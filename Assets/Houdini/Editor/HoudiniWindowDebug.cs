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

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class HoudiniWindowDebug : EditorWindow 
{
#if !( HAPI_ENABLE_RUNTIME )
	#pragma warning disable 0414
	#pragma warning disable 0649
#endif // !( HAPI_ENABLE_RUNTIME )

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

#if !( HAPI_ENABLE_RUNTIME )
		HoudiniGUI.help( HoudiniConstants.HAPI_UNSUPPORTED_PLATFORM_MSG, MessageType.Info );
		GUI.enabled = false;
#else
		if ( !HoudiniHost.isInstallationOk() )
		{
			HoudiniGUI.help(
				HoudiniHost.getMissingEngineInstallHelpString(), MessageType.Info );
			GUI.enabled = false;
		}
#endif // !( HAPI_ENABLE_RUNTIME )
		
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
			HoudiniGUIUtility.saveHoudiniScene( myLockNodesOnHipSave );
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

#if ( HAPI_ENABLE_RUNTIME )
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
#endif // ( HAPI_ENABLE_RUNTIME )

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

#if !( HAPI_ENABLE_RUNTIME )
	#pragma warning restore 0414
	#pragma warning restore 0649
#endif // !( HAPI_ENABLE_RUNTIME )
}
