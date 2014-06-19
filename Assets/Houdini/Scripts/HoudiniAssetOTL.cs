/*
 * PROPRIETARY INFORMATION.  This software is proprietary to
 * Side Effects Software Inc., and is not to be reproduced,
 * transmitted, or disclosed in any way without written permission.
 *
 * Produced by:
 *      Side Effects Software Inc
 *		123 Front Street West, Suite 1401
 *		Toronto, Ontario
 *		Canada   M5J 2M2
 *		416-504-9876
 *
 * COMMENTS:
 * 
 */
#define ENABLE_PARTICLES

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;

[ ExecuteInEditMode ]
public class HoudiniAssetOTL : HoudiniAsset 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	
	// Please keep these in the same order and grouping as their initializations in HAPI_Asset.reset().
	
	public string prAssetPath {							get { return myAssetPath; } 
														set { myAssetPath = value; } }
	public HAPI_HandleInfo[] prHandleInfos { get; set; }
	public List< HAPI_HandleBindingInfo[] > prHandleBindingInfos { get; set; }
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods
	
	public HoudiniAssetOTL() 
	{
		if ( prEnableLogging )
			Debug.Log( "HoudiniAssetOTL created - Instance Id: " + GetInstanceID() );
		
		// These variables need to keep between asset reloads.
		prAssetPath = "";
		
		reset();
	}
	
	~HoudiniAssetOTL() 
	{}
	
	public override void reset()
	{
		// Save the asset type so we can restore it after the reset.
		HoudiniAsset.AssetType asset_type = prAssetType;

		base.reset();
		
		// Please keep these in the same order and grouping as their declarations at the top.
		
		prHandleInfos 				= new HAPI_HandleInfo[ 0 ];
		prHandleBindingInfos 		= null;

		// Need to restore the asset type here.
		prAssetType = asset_type;
	}
	
	public override void Update()
	{
		base.Update();

		if (
#if UNITY_EDITOR
			EditorApplication.isPlaying &&
#endif // UNITY_EDITOR
			prPlaymodePerFrameCooking )
		{
			HoudiniHost.setTime( Time.time );
			buildClientSide();
		}
	}

	public override bool buildAll()
	{
		bool unload_asset_first = ( prAssetType == HoudiniAsset.AssetType.TYPE_OTL );

		return base.build(	true,	// reload_asset
							unload_asset_first,
							false,	// serializatin_recovery_only
							true,	// force_reconnect
							prCookingTriggersDownCooks,
							false	// use_delay_for_progress_bar
						);
	}

	public override bool build( bool reload_asset, bool unload_asset_first,
								bool serialization_recovery_only,
								bool force_reconnect,
								bool cook_downstream_assets,
								bool use_delay_for_progress_bar ) 
	{
		unload_asset_first = unload_asset_first 
							 && prAssetType == HoudiniAsset.AssetType.TYPE_OTL 
							 && ( !serialization_recovery_only || 
								  isPrefab() );

		bool base_built = base.build( reload_asset, unload_asset_first, serialization_recovery_only, 
									  force_reconnect, cook_downstream_assets, use_delay_for_progress_bar );
		if ( !base_built )
			return false;

		return true;
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Protected Methods

	protected override int buildCreateAsset( HoudiniProgressBar progress_bar )
	{
		return HoudiniHost.loadOTL( prAssetPath, prSplitGeosByGroup, progress_bar );
	}

	protected override void buildFullBuildCustomWork( ref HoudiniProgressBar progress_bar )
	{
		progress_bar.prMessage = "Loading handles...";
				
		// Get exposed handle information.
		prHandleInfos = new HAPI_HandleInfo[ prHandleCount ];
		HoudiniAssetUtility.getArray1Id( prAssetId, HoudiniHost.getHandleInfo, prHandleInfos, prHandleCount );
				
		// Get handles.
		prHandleBindingInfos = new List< HAPI_HandleBindingInfo[] >( prHandleCount );
		for ( int handle_index = 0; handle_index < prHandleCount; ++handle_index )
		{
			progress_bar.incrementProgressBar();
			HAPI_HandleInfo handle_info = prHandleInfos[ handle_index ];

#if UNITY_EDITOR
			if ( handle_info.typeName != "xform" && HoudiniHost.prEnableSupportWarnings )
				Debug.LogWarning( "Handle " + handle_info.name + " of type " 
								   	+ handle_info.typeName + " is unsupported at this time." );
#endif // UNITY_EDITOR

			HAPI_HandleBindingInfo[] binding_infos = new HAPI_HandleBindingInfo[ handle_info.bindingsCount ];
			HoudiniAssetUtility.getArray2Id( prAssetId, handle_index, HoudiniHost.getHandleBindingInfo, 
								 	binding_infos, handle_info.bindingsCount );

			prHandleBindingInfos.Add( binding_infos );
		}
	}

	protected override void buildCreateObjects( bool reload_asset, ref HoudiniProgressBar progress_bar )
	{
		for ( int object_index = 0; object_index < prObjectCount; ++object_index )
		{
			progress_bar.incrementProgressBar();
			try
			{
				if ( !prObjects[ object_index ].isInstancer && 
						( reload_asset	|| prObjects[ object_index ].hasTransformChanged
										|| prObjects[ object_index ].haveGeosChanged ) )
				{
					createObject( object_index, reload_asset );
				}
			}
			catch ( HoudiniError error )
			{
				// Per-object errors are not re-thrown so that the rest of the asset has a chance to load.
				Debug.LogWarning( error.ToString() );
			}
		}

		// Processing instancers.
		for ( int object_index = 0; object_index < prObjectCount; ++object_index )
		{
			HAPI_ObjectInfo object_info = prObjects[ object_index ];
			if ( object_info.isInstancer )
			{
				try
				{
					if ( object_info.objectToInstanceId >= 0 && 
							prGameObjects[ object_info.objectToInstanceId ] == null )
						createObject( object_info.objectToInstanceId, reload_asset );
						
					if( reload_asset || object_info.haveGeosChanged )
						instanceObjects( object_index, progress_bar );
				}
				catch ( HoudiniError error )
				{
					// Per-object errors are not re-thrown so that the rest of the asset has a chance to load.
					Debug.LogWarning( error.ToString() );
				}
			}
		}

	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private Methods
	
	private void instanceObjects( int object_id, HoudiniProgressBar progress_bar )
	{
		HAPI_ObjectInfo object_info		= prObjects[ object_id ];
		HoudiniInstancer instancer		= null;
		
		Transform old_instancer_transform = transform.Find( object_info.name );
		if ( old_instancer_transform && old_instancer_transform.gameObject.GetComponent< HoudiniInstancer >() )
		{
			instancer = old_instancer_transform.gameObject.GetComponent< HoudiniInstancer >();
		}
		else
		{

			if( gameObject.GetComponent< HoudiniInstancerManager >() == null )
				gameObject.AddComponent< HoudiniInstancerManager >();

			GameObject main_object = new GameObject( object_info.name );
			main_object.transform.parent = transform;

			main_object.AddComponent< HoudiniInstancer >();
			prGameObjects[ object_id ] = main_object;
			instancer = main_object.GetComponent< HoudiniInstancer >();

			HoudiniInstancerManager instancer_manager = gameObject.GetComponent< HoudiniInstancerManager >();
			instancer_manager.updateInstancerData( instancer );
		}
		
		instancer.prAsset		= this;
		instancer.prObjectId	= object_id;
		
		instancer.instanceObjects( progress_bar );
	}

	private void createObject( int object_id, bool reload_asset )
	{
		HoudiniObjectControl object_control = null;
		HAPI_ObjectInfo object_info = prObjects[ object_id ];
		
		// Create main underling.
		if ( prGameObjects[ object_id ] == null )
		{
			prGameObjects[ object_id ] = new GameObject( object_info.name );
			prGameObjects[ object_id ].transform.parent = transform;
			prGameObjects[ object_id ].isStatic = gameObject.isStatic;

			object_control = prGameObjects[ object_id ].AddComponent< HoudiniObjectControl >();
			object_control.init(
				prAssetId, object_info.nodeId, prAsset, object_id, object_info.name, object_info.isVisible );
		}
		else
		{
			object_control = prGameObjects[ object_id ].GetComponent< HoudiniObjectControl >();
		}
		GameObject main_child = prGameObjects[ object_id ];
		
		try
		{
			object_control.refresh( reload_asset, object_info );

			if ( reload_asset || object_info.hasTransformChanged )
			{
				// Get transforms.
				HAPI_Transform trans = prObjectTransforms[ object_id ];
				HoudiniAssetUtility.applyTransform( trans, main_child.transform );
			}
		}
		catch ( HoudiniError error )
		{
			DestroyImmediate( main_child );
			prGameObjects[ object_id ] = null;
			error.addMessagePrefix( "Obj(id: " + object_info.id + ", name: " + object_info.name + ")" );
			error.addMessageDetail( "Object Path: " + object_info.objectInstancePath );
			throw;
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Serialized Data
	
	[SerializeField] private string					myAssetPath;
}
