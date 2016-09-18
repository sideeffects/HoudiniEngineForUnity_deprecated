using UnityEngine;

public class HoudiniApiAssetHook : MonoBehaviour {

	// Called before the asset cooks.
	virtual public void preCook( HoudiniAsset asset ) {
		Debug.Log( asset.name + " is about to cook." );
	}
	
	// Called after a *successful* cook.
	virtual public void postCook( HoudiniAsset asset ) {
		Debug.Log( asset.name + " just cooked." );
	}
}

