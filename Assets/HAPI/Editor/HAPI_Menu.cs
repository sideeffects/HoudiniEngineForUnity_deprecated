using UnityEngine;
using UnityEditor;
using System.Collections;
using HAPI;

public class HAPI_Menu : MonoBehaviour {
	
	public const string myNewHoudiniObjectLabel = "HAPI Object";
	public const string myNewHoudiniPrefabLabel = "New Houdini Object";
	
	[ MenuItem( "HAPI/Create HAPI Object" ) ]
	static void CreateHAPIObject() {
		// create game object
		GameObject gameObject = new GameObject( myNewHoudiniObjectLabel );
		
		// add HAPI script
		gameObject.AddComponent( "HAPI_ObjectControl" );		
		HAPI_ObjectControl objectControl = gameObject.GetComponent< HAPI_ObjectControl >();
				
		// get asset path now
		string assetPath = HAPI_Inspector.PromptForAssetPath();
		objectControl.SetAssetPath( assetPath );
		
		// save as prefab
		//Object prefab = PrefabUtility.CreateEmptyPrefab( "Assets/" + myNewHoudiniPrefabLabel + ".prefab" );
		//PrefabUtility.ReplacePrefab( gameObject, prefab, ReplacePrefabOptions.ConnectToPrefab );
		
		// do first build
		objectControl.Build();
	}
	
}
