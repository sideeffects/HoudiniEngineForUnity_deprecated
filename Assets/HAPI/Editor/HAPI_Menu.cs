using UnityEngine;
using UnityEditor;
using System.Collections;

public class HAPI_Menu : MonoBehaviour {
	
	[ MenuItem( "HAPI/Create HAPI Object" ) ]
	static void CreateHAPIObject() {
		// create game object
		GameObject gameObject = new GameObject( "HAPI Object" );
				
		// add HAPI script
		gameObject.AddComponent( "HAPI_ObjectControl" );
		
		// save as prefab
		//Object prefab = PrefabUtility.CreateEmptyPrefab( "Assets/HAPI/HAPI_Object.prefab" );
		//PrefabUtility.ReplacePrefab( gameObject, prefab, ReplacePrefabOptions.ConnectToPrefab );
				
		// do initial geometry build
		//HAPI_ObjectControl objectControl = gameObject.GetComponent< HAPI_ObjectControl >();
		//objectControl.OnEnable();
	}
	
}
