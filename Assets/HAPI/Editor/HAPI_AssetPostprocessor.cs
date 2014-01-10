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
		foreach ( string asset_path in deletedAssets )
		{
			if ( HAPI_Host.myCleanUpPrefabAssets.ContainsKey( asset_path ) )
			{
				int asset_id = HAPI_Host.myCleanUpPrefabAssets[ asset_path ];
				HAPI_Host.destroyAsset( asset_id );
				HAPI_Host.myCleanUpPrefabAssets.Remove( asset_path );
			}
		}

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

						// TODO: Used to be (why?):
						// EditorApplication.delayCall += ReplacePrefab;
						ReplacePrefab();
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
						// If the prefab's backup id refers to a valid asset then this signifies the
						// prefab is being changed due to an "Apply" from a prefab instance. We need
						// to delete the prefab asset since we are re-creating the prefab. 
						bool is_applying_changes = HAPI_Host.isAssetValid( prefab_asset.prBackupAssetId, 
						                                                   prefab_asset.prBackupAssetValidationId );
						if ( is_applying_changes )
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
						HAPI_Asset new_prefab_asset = new_prefab.GetComponent< HAPI_Asset >();

						// If applying changes to prefab from prefab instance we also need to retrieve 
						// all the overriden parameters on the prefab instance so that we can apply 
						// these changes properly to all the other instances. This must be done after
						// prefab has been replaced.
						if ( is_applying_changes )
						{
							foreach ( HAPI_ParmInfo parm_info in asset.prParms.prParms )
							{
								if ( asset.prParms.isParmOverridden( parm_info.id ) )
								{
									new_prefab_asset.prUpdatePrefabInstanceParmNames.Add( parm_info.name );
								}
							}
						}
						
						// Set asset id of prefab to -1 since it has not been built yet.
						// Call SetDirty so changes to prefab will be saved to disk and
						// OnEnable will be called on original asset so that it can be
						// rebuilt in order to re-create its game objects.
						new_prefab_asset.prAssetId = -1;
						new_prefab_asset.prAssetValidationId = -1;
						EditorUtility.SetDirty( new_prefab_asset );
						
						break;
					}
				}
			}
		}
	}
	
	public static GameObject prPrefabToReplace { get { return myPrefabToReplace; }
												 set { myPrefabToReplace = value; } }
	
	private static GameObject myPrefabToReplace = null;
};
