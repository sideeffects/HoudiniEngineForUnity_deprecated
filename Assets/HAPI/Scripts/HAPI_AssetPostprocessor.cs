using UnityEngine;
using UnityEditor;
using System;
using HAPI;

[InitializeOnLoad]
class HAPI_AssetPostprocessor : AssetPostprocessor 
{

	static void OnPostprocessAllAssets ( String[] importedAssets,
										 String[] deletedAssets,
						 				 String[] movedAssets,
										 String[] movedFromAssetPaths ) 
	{
		foreach ( string asset_path in importedAssets )
		{
			if( asset_path.EndsWith( ".prefab" ) )
			{
				GameObject prefab = AssetDatabase.LoadAssetAtPath( asset_path, typeof( GameObject ) ) as GameObject;
				if( prefab )
				{
					HAPI_Asset prefab_asset = prefab.GetComponent< HAPI_Asset >();
					if( prPrefabToReplace && asset_path.Equals( AssetDatabase.GetAssetPath( prPrefabToReplace ) ) )
					{
						prPrefabToReplace = null;
					}
					else if( prefab_asset && prefab_asset.isApplyingChangesToPrefab() )
					{
						prPrefabToReplace = prefab;
						EditorApplication.delayCall += ReplacePrefab;
					}
				}
			}
		}
	}
	
	static void ReplacePrefab()
	{
		HAPI_Asset prefab_asset = prPrefabToReplace.GetComponent<HAPI_Asset>();
		if ( prPrefabToReplace && prefab_asset )
		{
			foreach ( GameObject obj in GameObject.FindObjectsOfType( typeof( GameObject ) ) )
			{
				HAPI_Asset asset = obj.GetComponent< HAPI_Asset >();
				GameObject prefab_parent = PrefabUtility.GetPrefabParent( obj ) as GameObject;
				if ( asset && PrefabUtility.Equals( prefab_parent, prPrefabToReplace ) )
				{
					// this is the asset the prefab is being created/changed from
					if ( asset.prAssetId == prefab_asset.prAssetId )
					{
						// if prefab's backup id refers to a valid asset then delete this asset
						// since we are re-creating the prefab
						if ( HAPI_Host.isAssetValid( prefab_asset.prBackupAssetId, 
													 prefab_asset.prBackupAssetValidationId ) )
						{
							try
							{
								HAPI_Host.unloadOTL( prefab_asset.prBackupAssetId );
							}
							catch ( HAPI_Error error )
							{
								Debug.LogError( "Asset failed to unload: " + error.ToString() );
							}
							
							prefab_asset.reset();
							prefab_asset.prParms.reset();
						}
						
						// replace prefab with original asset with all children game objects removed
						asset.destroyChildren( asset.transform );
						PrefabUtility.ReplacePrefab( asset.gameObject, prPrefabToReplace );
					
						// rebuild original asset so it re-creates game objects
						asset.build( true,	// reload_asset
									 false,	// unload_asset_first
									 false,	// serializatin_recovery_only
									 true,	// force_reconnect
									 false,	// cook_downstream_assets
									 false	// use_delay_for_progress_bar
									);
						
						// set asset id of prefab to -1 since it has not been built yet
						prefab_asset.prAssetId = -1;
					}
					// this is not the prefab instance this prefab is being created from 
					// so apply changes being made to prefab to this prefab instance 
					// NOTE: if this gets called before the prefab is replaced it is okay 
					// because the preset on the original prefab will be the same as the 
					// one it is replaced with
					else
					{
						asset.prPreset = prefab_asset.prPreset;
						asset.loadPreset();
						asset.buildClientSide();
					}
				}
			}
		}
	}
	
	public static GameObject prPrefabToReplace { get { return myPrefabToReplace; }
												 set { myPrefabToReplace = value; } }
	
	private static GameObject myPrefabToReplace = null;
};