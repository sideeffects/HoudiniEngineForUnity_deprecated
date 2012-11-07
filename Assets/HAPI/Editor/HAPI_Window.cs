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
using System.IO;
using HAPI;

/// <summary>
/// 	Only a test class. Not used currently.
/// </summary>
public class HAPI_Window : EditorWindow 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	/// <summary>
	/// 	Add menu item named "My Window" to the Window menu.
	/// </summary>
	[ MenuItem( "Window/HAPI" ) ]
	public static void ShowWindow() 
	{
		//Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow( typeof( HAPI_Window ) );
	}
	
	public void OnGUI() 
	{
		string path = Application.dataPath;			
		DirectoryInfo di = new DirectoryInfo( path + "//OTLs" );
		
		myShowUtility = EditorGUILayout.Foldout( myShowUtility, new GUIContent( "Utility" ) );
		
		if ( myShowUtility )
		{
			if ( GUILayout.Button( "Save Current Houdini Scene" ) )
			{
				string hip_file_path = EditorUtility.SaveFilePanel( "Save HIP File", "", "hscene.hip", "hip" );
				if ( hip_file_path != "" && HAPI_Host.hasScene() )
					HAPI_Host.saveScene( hip_file_path );
				else
					Debug.LogError( "Nothing to save." );
			}
			
			if ( GUILayout.Button( "Load Houdini Asset" ) )
			{
				string asset_file_path = HAPI_GUIUtility.promptForOTLPath();
				HAPI_GUIUtility.instantiateAsset( asset_file_path );
			}
			
			if ( GUILayout.Button( "Instantiate All Assets" ) )
			{
				try
				{
					if ( !di.Exists )
						throw new HAPI_Error( "Project/Assets/OTLs directory does not exist!" );
					
					foreach ( FileInfo fi in di.GetFiles() )
						if ( fi.Extension == ".otl" )
							loadOTL( fi );
				}
				catch ( System.Exception e )
				{
					Debug.LogError( "Directory navigation failed: " + e.ToString() );	
				}
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
				
		        foreach ( FileInfo fi in di.GetFiles() )
					if ( fi.Extension == ".otl" )
						genOTLEntry( fi );
				
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
	
	private static float 			myLineHeight 			= 16;
	private static GUILayoutOption 	myLineHeightGUI 		= GUILayout.Height( myLineHeight );
	private static Vector2 			myScrollPosition;
}