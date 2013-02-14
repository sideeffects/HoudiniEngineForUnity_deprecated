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

public class HAPI_Window : EditorWindow 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public

	class OTLDirectory
	{
		public bool myExpanded;
		public string myDirectoryName;
		public string myDirectoryPath;
	}

	[ MenuItem( "Window/HAPI" ) ]
	public static void ShowWindow() 
	{
		// Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow< HAPI_Window >( false, "HAPI Debug" );
		float time = 0.0f;

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

		HAPI_Host.getTime( out time );
		HAPI_Window.myTime = time;
	}
	
	public void OnGUI() 
	{
		string path = Application.dataPath;
		DirectoryInfo di = new DirectoryInfo( path + "//OTLs" );
		
		myShowUtility = EditorGUILayout.Foldout( myShowUtility, new GUIContent( "Utility" ) );
		
		if ( myShowUtility )
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

			// Collision Group Name
			{
				string value = HAPI_Host.prCollisionGroupName;
				bool changed = HAPI_GUI.stringField( "collision_group_name", "Collision Grp. Name", ref value );
				if ( changed )
					HAPI_Host.prCollisionGroupName = value;
			}

			// Enable Drag-and-Drop
			{
				bool value = HAPI_Host.prEnableDragAndDrop;
				bool changed = HAPI_GUI.toggle( "enable_drag_and_drop", "Enable Drag-and-Drop", ref value );
				if ( changed )
				{
					HAPI_Host.prEnableDragAndDrop = value;
					HAPI_GUIUtility.setDragAndDrop( value );
				}
			}

			// Hide Geometry On Linking
			{
				bool value = HAPI_Host.prHideGeometryOnLinking;
				bool changed = HAPI_GUI.toggle( "hide_geometry_on_linking", "Hide Geometry On Linking", ref value );
				if ( changed )
					HAPI_Host.prHideGeometryOnLinking = value;
			}

			// Hide Geometry On Linking
			{
				bool value = HAPI_Host.prEnableGlobalCooking;
				bool changed = HAPI_GUI.toggle( "enable_global_cooking", "Enable Global Cooking", ref value );
				if ( changed )
					HAPI_Host.prEnableGlobalCooking = value;
			}

			// Enable Support Warnings
			{
				bool value = HAPI_Host.prEnableSupportWarnings;
				bool changed = HAPI_GUI.toggle( "enable_support_warnings", "Enable Support Warnings", ref value );
				if ( changed )
					HAPI_Host.prEnableSupportWarnings = value;
			}
		}
		
		myShowFileList = EditorGUILayout.Foldout( myShowFileList, new GUIContent( "File List" ) );
		
		if ( myShowFileList )
		{
			try
			{
				if ( !di.Exists )
					throw new HAPI_Error( "Project/Assets/OTLs directory does not exist!" );
				
				myScrollPosition = GUILayout.BeginScrollView( myScrollPosition );
				
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
				
				GUILayout.EndScrollView();
			}
			catch ( System.Exception e )
			{
				Debug.LogError( "Directory navigation failed: " + e.ToString() );	
			}
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
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
	
	private static bool				myShowUtility			= true;
	private static bool				myShowFileList			= true;
	
	private static List< OTLDirectory >	myOTLDirectories  	= new List< OTLDirectory >();	
	
	private static float 			myLineHeight 			= 16;
	private static GUILayoutOption 	myLineHeightGUI 		= GUILayout.Height( myLineHeight );
	private static Vector2 			myScrollPosition;

	private static float			myTime;
}
