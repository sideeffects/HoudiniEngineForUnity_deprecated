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
 * 		Contains HAPI_Menu which is added to the main Unity menu bar.
 * 
 */

using UnityEngine;
using UnityEditor;
using System.Collections;
using HAPI;

/// <summary>
/// 	Main HAPI menu which adds components to the main Unity menu bar.
/// </summary>
public class HAPI_Menu : MonoBehaviour 
{	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	public const string myMenuName				= "HAPI";
	public const string myMenuLoadAssetLabel	= "Load Houdini Asset";
	public const string myDefaultAssetLabel 	= "Houdini Asset";
	public const string myDefaultPrefabLabel 	= "Houdini Asset";
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private
	
	/// <summary>
	/// 	Prompts the user for a path to a .otl file and creates a HAPI game object from it.
	/// </summary>
	[ MenuItem( myMenuName + "/" + myMenuLoadAssetLabel ) ]
	static private void createHAPIObject() 
	{
		// Prompt for the absolute path to the .otl file to use.
		string asset_file_path = HAPI_Inspector.promptForAssetPath( "" );
		if ( asset_file_path.Length == 0 )
			return; // User pressed Cancel in the dialog so just return.
				
		// Create game object.
		GameObject game_object = new GameObject( myDefaultAssetLabel );
		
		// Add HAPI Object Control script component.
		game_object.AddComponent( "HAPI_ObjectControl" );		
		HAPI_ObjectControl object_control = game_object.GetComponent< HAPI_ObjectControl >();
		
		// Set that asset path.
		object_control.setAssetPath( asset_file_path );
		
		// Save as a prefab.
		//Object prefab = PrefabUtility.CreateEmptyPrefab( "Assets/" + myDefaultPrefabLabel + ".prefab" );
		//PrefabUtility.ReplacePrefab( game_object, prefab, ReplacePrefabOptions.ConnectToPrefab );
		
		// Do first build.
		bool build_result = object_control.build();
		if ( build_result == false ) // Something is not right. Clean up.
		{
			DestroyImmediate( game_object );
			return;
		}
		
		// Set new object name from asset name.
		string asset_name		= object_control.myAssetInfo.name;
		game_object.name 		= asset_name;
		
		// Select the new houdini asset.
		GameObject[] selection 	= new GameObject[ 1 ];
		selection[ 0 ] 			= game_object;
		Selection.objects 		= selection;
	}
	
}
