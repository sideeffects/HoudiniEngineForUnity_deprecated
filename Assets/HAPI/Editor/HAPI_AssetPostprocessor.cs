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
			if ( asset_path.EndsWith( ".prefab" ) )
			{
				GameObject prefab = AssetDatabase.LoadAssetAtPath( asset_path, typeof( GameObject ) ) as GameObject;
				if ( prefab )
				{
					HAPI_Asset prefab_asset = prefab.GetComponent< HAPI_Asset >();
					if ( prPrefabToReplace && asset_path.Equals( AssetDatabase.GetAssetPath( prPrefabToReplace ) ) )
					{
						prPrefabToReplace = null;
					}
					else if ( prefab_asset && prefab_asset.isApplyingChangesToPrefab() )
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
								HAPI_Host.destroyAsset( prefab_asset.prBackupAssetId );
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
						GameObject new_prefab = PrefabUtility.ReplacePrefab( asset.gameObject, 
																			 prPrefabToReplace, 
																			 ReplacePrefabOptions.ConnectToPrefab ) as GameObject;
						
						// Set asset id of prefab to -1 since it has not been built yet.
						// Call SetDirty so changes to prefab will be saved to disk and
						// OnEnable will be called on original asset so that it can be
						// rebuilt in order to re-create its game objects.
						HAPI_Asset new_prefab_asset = new_prefab.GetComponent< HAPI_Asset >();
						new_prefab_asset.prAssetId = -1;
						new_prefab_asset.prAssetValidationId = -1;
						EditorUtility.SetDirty( new_prefab_asset );
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
