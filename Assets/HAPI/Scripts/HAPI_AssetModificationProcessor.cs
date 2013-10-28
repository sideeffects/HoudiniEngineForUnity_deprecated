using UnityEngine;
using UnityEditor;
using System;
using HAPI;

[InitializeOnLoad]
class HAPI_AssetModificationProcessor : UnityEditor.AssetModificationProcessor 
{
	
	static void OnWillCreateAsset( string path )
	{
		//Debug.Log( "Will create asset" );
	}
	
	static AssetDeleteResult OnWillDeleteAsset( string assetPath, RemoveAssetOptions option )
	{
		//Debug.Log ( "Will delete asset" );
		/*if ( assetPath.EndsWith( ".prefab" ) )
		{
			GameObject prefab = AssetDatabase.LoadAssetAtPath( assetPath, typeof( GameObject ) ) as GameObject;
			if ( prefab )
			{
				HAPI_Asset prefab_asset = prefab.GetComponent< HAPI_Asset >();
				if ( prefab_asset )
				{
					try
					{
						HAPI_Host.unloadOTL( prefab_asset.prAssetId );
					}
					catch ( HAPI_Error error )
					{
						Debug.LogError( "Asset failed to unload: " + error.ToString() );
					}
				}
			}
		}*/
		return AssetDeleteResult.DidNotDelete;
	}
}

