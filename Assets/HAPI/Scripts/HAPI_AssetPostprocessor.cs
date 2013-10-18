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
					else if( prefab_asset )
					{
						// prefab is being created for the first time
						if( prefab_asset.prBackupAssetId < 0 )
						{
							prPrefabToReplace = prefab;
							EditorApplication.delayCall += ReplacePrefab;
						}
						// prefab has already been created and this is called due to an Apply on
						// one of the prefab instances so we need to restore the prefab's asset
						// id and asset validation id so it's not replaced by the instance's values
						else if( prefab_asset.prAssetId != prefab_asset.prBackupAssetId &&
								 HAPI_Host.isAssetValid( prefab_asset.prBackupAssetId, 
														 prefab_asset.prBackupAssetValidationId ) )
						{
							prefab_asset.prAssetId = prefab_asset.prBackupAssetId;
							prefab_asset.prAssetValidationId = prefab_asset.prBackupAssetValidationId;
						}
					}
				}
			}
		}
	}
	
	static void ReplacePrefab()
	{
		if( prPrefabToReplace )
		{
			foreach ( GameObject obj in GameObject.FindObjectsOfType( typeof( GameObject ) ) )
			{
				HAPI_Asset asset = obj.GetComponent< HAPI_Asset >();
				GameObject prefab_parent = PrefabUtility.GetPrefabParent( obj ) as GameObject;
				if( asset &&  PrefabUtility.Equals( prefab_parent, prPrefabToReplace ) )
				{
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
					break;
				}
			}
		}
	}
	
	public static GameObject prPrefabToReplace { get { return myPrefabToReplace; }
												 set { myPrefabToReplace = value; } }
	
	private static GameObject myPrefabToReplace = null;
};