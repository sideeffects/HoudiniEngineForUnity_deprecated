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
 */

using UnityEngine;
using UnityEditor;
using System.Collections;
using HAPI;

public class HAPI_Menu : MonoBehaviour {
	
	public const string myNewHoudiniAssetLabel = "Houdini Asset";
	public const string myNewHoudiniPrefabLabel = "Houdini Asset";
	
	[ MenuItem( "HAPI/Load Houdini Asset" ) ]
	static void createHAPIObject() {
		// Create game object.
		GameObject game_object = new GameObject( myNewHoudiniAssetLabel );
		
		// Add HAPI Object Control script component.
		game_object.AddComponent( "HAPI_ObjectControl" );		
		HAPI_ObjectControl object_control = game_object.GetComponent< HAPI_ObjectControl >();
				
		// Get asset path now.
		string asset_file_path = HAPI_Inspector.promptForAssetPath( "" );
		object_control.setAssetPath( asset_file_path );
		
		// Save as a prefab.
		//Object prefab = PrefabUtility.CreateEmptyPrefab( "Assets/" + myNewHoudiniPrefabLabel + ".prefab" );
		//PrefabUtility.ReplacePrefab( game_object, prefab, ReplacePrefabOptions.ConnectToPrefab );
		
		// Do first build.
		object_control.build();
		
		// Set new object name from asset name.
		string asset_name = object_control.myAssetInfo.assetName;
		game_object.name = asset_name;
		
		// Select the new houdini asset.
		GameObject[] selection = new GameObject[ 1 ];
		selection[ 0 ] = game_object;
		Selection.objects = selection;
	}
	
}
