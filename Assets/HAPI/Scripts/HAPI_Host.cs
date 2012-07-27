using UnityEngine;
using System.Collections;

namespace HAPI {

	public class HAPI_Host : MonoBehaviour {		
		static public bool HasScene() {
			return myHoudiniSceneExists;
		}
			
		static public string LoadOTL( string path ) {
			if ( myHoudiniSceneExists ) {
				Debug.Log( "Loading OTL: hip already exists" );
			} else {
				Debug.Log( "Loading OTL: new hip created" );	
			}
			
			myHoudiniSceneExists = true;
			
			return "";
		}
		
		static public bool UnloadOTL( string innerPath ) {
			return true;	
		}
		
		static private bool myHoudiniSceneExists = false;
	}

}