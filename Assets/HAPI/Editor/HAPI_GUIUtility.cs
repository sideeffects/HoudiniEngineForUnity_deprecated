using UnityEngine;
using UnityEditor;
using System.Collections;

using HAPI;

public class HAPI_GUIUtility : Editor 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
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
		if ( file_path.Length <= 0 )
			return;			
		
		HAPI_Host.loadHip( file_path );
		
		HAPI_State state = HAPI_State.HAPI_STATE_STARTING_LOAD;
		
		while ( state != HAPI_State.HAPI_STATE_READY )
		{
			state = (HAPI_State) HAPI_Host.getStatus( HAPI_StatusType.HAPI_STATUS_STATE );
			
		}
		
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
	
	public const string myMenuName				= "HAPI";
	public const string myLoadAssetLabel		= "Load Houdini Asset";
	public const string myDefaultAssetLabel 	= "Houdini Asset";
	public const string myDefaultPrefabLabel 	= "Houdini Asset";
}
