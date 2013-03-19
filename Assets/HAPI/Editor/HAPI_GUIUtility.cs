using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

using HAPI;

[ InitializeOnLoad ]
public class HAPI_GUIUtility : Editor 
{
	static HAPI_GUIUtility()
	{
		EditorApplication.update += update;

		if ( HAPI_Host.prEnableDragAndDrop )
		{
			EditorApplication.hierarchyWindowChanged += hierarchyWindowChanged;
			EditorApplication.hierarchyWindowItemOnGUI += hierarchyWindowItemOnGUI;
		}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	public static void setDragAndDrop( bool state )
	{
		if ( state )
		{
			EditorApplication.hierarchyWindowChanged += hierarchyWindowChanged;
			EditorApplication.hierarchyWindowItemOnGUI += hierarchyWindowItemOnGUI;
			hierarchyWindowChanged();
		}
		else
		{
			EditorApplication.hierarchyWindowChanged -= hierarchyWindowChanged;
			EditorApplication.hierarchyWindowItemOnGUI -= hierarchyWindowItemOnGUI;
		}
	}

	public static string promptForOTLPath()
	{
		return promptForOTLPath( "" );
	}
	public static string promptForOTLPath( string old_path )
	{		
		// Prompt for the absolute path to the .otl file to use.
		string new_path = EditorUtility.OpenFilePanel( "Open Houdini OTL", old_path, "otl" );
		return new_path;
	}
	
	public static string promptForHIPPath()
	{
		return promptForHIPPath( "" );
	}
	public static string promptForHIPPath( string old_path )
	{		
		// Prompt for the absolute path to the .otl file to use.
		string new_path = EditorUtility.OpenFilePanel( "Open Houdini HIP", old_path, "hip" );
		return new_path;
	}
	
	public static string promptForPrefabPath( string old_path )
	{		
		// Prompt for the absolute path to the .otl file to use.
		string new_path = EditorUtility.OpenFilePanel( "Choose Prefab", old_path, "prefab" );
		return new_path;
	}
	
	public static string promptForFileInputPath( string old_path )
	{		
		// Prompt for the absolute path to the .otl file to use.
		string new_path = EditorUtility.OpenFilePanel( "Open File", old_path, "*.*" );
		return new_path;
	}
	
	public static void loadHipFile( string file_path )
	{
		try
		{
			if ( file_path.Length <= 0 )
				return;
			
			HAPI_Host.loadHip( file_path );
			
			HAPI_ProgressBar progressBar = new HAPI_ProgressBar();
			progressBar.statusCheckLoop();
			
			int num_assets = HAPI_Host.getAssetCountFromLoadHip();
			
			int [] asset_ids = new int[ num_assets ];
			HAPI_Host.getAssetIdsFromLoadHIPFile( asset_ids );
			
			foreach ( int asset_id in asset_ids )
			{
				// Create game object.
				GameObject game_object = new GameObject( myDefaultAssetLabel );
				
				// Add HAPI Object Control script component.
				game_object.AddComponent( "HAPI_AssetOTL" );
				HAPI_AssetOTL asset = game_object.GetComponent< HAPI_AssetOTL >();
				
				asset.prAssetType = HAPI_Asset.AssetType.TYPE_HIP;
				// Set that asset path.
				asset.prAssetPath = file_path;
				
				asset.prAssetId = asset_id;
				
				bool build_result = asset.build();
				if ( build_result == false ) // Something is not right. Clean up.
				{
					DestroyImmediate( game_object );
					return;
				}
				
				// Set new object name from asset name.
				string asset_name		= asset.prAssetInfo.name;
				game_object.name 		= asset_name;
			}
		}
		catch ( HAPI_Error error )
		{
			Debug.LogError( error.ToString() );
		}
	}
	
	public static void instantiateAsset( string file_path )
	{
		if ( file_path.Length <= 0 )
			return;

		// Create game object.
		GameObject game_object = new GameObject( myDefaultAssetLabel );
		
		// Add HAPI Object Control script component.
		game_object.AddComponent( "HAPI_AssetOTL" );
		HAPI_AssetOTL asset = game_object.GetComponent< HAPI_AssetOTL >();
		
		asset.prAssetType = HAPI_Asset.AssetType.TYPE_OTL;
		// Set that asset path.
		asset.prAssetPath = file_path;
		
		// Save as a prefab.
		//Object prefab = PrefabUtility.CreateEmptyPrefab( "Assets/" + myDefaultPrefabLabel + ".prefab" );
		//PrefabUtility.ReplacePrefab( game_object, prefab, ReplacePrefabOptions.ConnectToPrefab );
		
		// Do first build.
		bool build_result = asset.build();
		if ( build_result == false ) // Something is not right. Clean up.
		{
			DestroyImmediate( game_object );
			return;
		}
		
		// Set new object name from asset name.
		string asset_name		= asset.prAssetInfo.name;
		game_object.name 		= asset_name;
		
		// Select the new houdini asset.
		GameObject[] selection 	= new GameObject[ 1 ];
		selection[ 0 ] 			= game_object;
		Selection.objects 		= selection;
	}
	
	public const string mySaveHoudiniSceneLabel = "Save Current Houdini Scene...";
	public const string myLoadAssetLabel		= "Load Houdini Asset...";
	public const string myLoadHipLabel			= "Load .hip File...";
	public const string myCreateCurveLabel		= "Create Curve";
	public const string myDefaultAssetLabel 	= "Houdini Asset";
	public const string myDefaultPrefabLabel 	= "Houdini Asset";

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private

	private static void update()
	{
		
	}

	private static void hierarchyWindowChanged()
	{
		// We optimize here a bit by not explicitly checking the HAPI_Host.prEnableDragAndDrop flag
		// as we should be doing. This is because we unregsiter this method as a callback if the
		// setting is disable so we can be assured this method won't be called at all.

		// Count total number of root objects in the hierarchy.
		int hierarchy_count = 0;
		foreach ( GameObject obj in Object.FindObjectsOfType( typeof( GameObject ) ) )
		{
			if ( obj.transform.parent == null )
				hierarchy_count++;
		}
		
		// We need to create SOMETHING so that the hierarchy is not completely empty. It it does
		// become empty then the callbacks to hierarchyWindowItemOnGUI stop coming at all so it
		// becomes impossible to tell if something is being dragged. If we have to create something
		// we might as well create the Main Camera since you need this object in your game ANYWAY.
		if ( hierarchy_count <= 0 )
		{
			GameObject forced_camera = new GameObject( "Main Camera" );
			forced_camera.AddComponent< Camera >();
			forced_camera.tag = "MainCamera";
			Debug.Log( 
				"Sorry, but if you have nothing in the Hierarchy the drag-and-drop feature for .otl files " +
				"does not work. Until this is fixed, we've forcible created a Camera for you. You can " +
				"disable this 'feature' by unchecking the 'Enable Drag-and-Drop' option in the HAPI Window." );
		}
	}

	private static void hierarchyWindowItemOnGUI( int instanceID, Rect selectionRect )
	{
		// We optimize here a bit by not explicitly checking the HAPI_Host.prEnableDragAndDrop flag
		// as we should be doing. This is because we unregsiter this method as a callback if the
		// setting is disable so we can be assured this method won't be called at all.

		if ( !myDragFlag && Event.current != null &&
				( Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform ) )
		{
			// Show an instatiation icon on the drag.
			DragAndDrop.visualMode = DragAndDropVisualMode.Link;
				
			if ( Event.current.type == EventType.DragPerform )
			{
				for ( int i = 0; i < DragAndDrop.paths.Length; ++i )
				{
					string path = DragAndDrop.paths[ i ];
					if ( path.Contains( ".otl" ) )
					{
						instantiateAsset( path );
						myDragFlag = true;
					}
				}
			}
		}
		else if ( myDragFlag && Event.current != null && Event.current.type == EventType.DragExited )
		{
			myDragFlag = false;
		}
	}

	private static bool myDragFlag = false;
}
