using UnityEngine;
using UnityEditor;
using System;

[InitializeOnLoad]
class HoudiniAssetModificationProcessor : UnityEditor.AssetModificationProcessor 
{
	
	static void OnWillCreateAsset( string assetPath )
	{
		//Debug.Log( "Will create asset" );
	}
	
	static AssetDeleteResult OnWillDeleteAsset( string assetPath, RemoveAssetOptions option )
	{
		//Debug.Log ( "Will delete asset" );
		return AssetDeleteResult.DidNotDelete;
	}
	
	static string[] OnWillSaveAssets( string[] assetPaths )
	{
		//Debug.Log( "Will save assets" );
		return assetPaths;
	}
}

