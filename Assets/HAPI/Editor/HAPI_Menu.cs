using UnityEngine;
using UnityEditor;
using System.Collections;
using HAPI;

public class HAPI_Menu : MonoBehaviour {
	
	public const string myNewHoudiniAssetLabel = "Houdini Asset";
	public const string myNewHoudiniPrefabLabel = "Houdini Asset";
	
	[ MenuItem( "HAPI/Load Houdini Asset" ) ]
	static void CreateHAPIObject() {
		// create game object
		GameObject gameObject = new GameObject( myNewHoudiniAssetLabel );
		
		// add HAPI script
		gameObject.AddComponent( "HAPI_ObjectControl" );		
		HAPI_ObjectControl objectControl = gameObject.GetComponent< HAPI_ObjectControl >();
				
		// get asset path now
		string assetPath = HAPI_Inspector.PromptForAssetPath( "" );
		objectControl.SetAssetPath( assetPath );
		
		// save as prefab
		//Object prefab = PrefabUtility.CreateEmptyPrefab( "Assets/" + myNewHoudiniPrefabLabel + ".prefab" );
		//PrefabUtility.ReplacePrefab( gameObject, prefab, ReplacePrefabOptions.ConnectToPrefab );
		
		// do first build
		objectControl.Build();
		
		// select the new houdini asset
		GameObject[] selection = new GameObject[ 1 ];
		selection[ 0 ] = gameObject;
		Selection.objects = selection;
	}
	
}
