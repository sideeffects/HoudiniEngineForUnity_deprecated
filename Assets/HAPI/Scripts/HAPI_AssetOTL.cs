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
#endif
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using HAPI;
using Utility = HAPI_AssetUtility;


[ ExecuteInEditMode ]
public class HAPI_AssetOTL : HAPI_Asset 
{	
#if UNITY_EDITOR
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	
	// Please keep these in the same order and grouping as their initializations in HAPI_Asset.reset().
	
	public string prAssetPath {							get { return myAssetPath; } 
														set { myAssetPath = value; } }
	public HAPI_AssetOTLUndoInfo prAssetOTLUndoInfo {	get { return myAssetOTLUndoInfo; }
														private set { } }
	public HAPI_HandleInfo[] prHandleInfos { get; set; }
	public List< HAPI_HandleBindingInfo[] > prHandleBindingInfos { get; set; }
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods
	
	public HAPI_AssetOTL() 
	{
		if ( prEnableLogging )
			Debug.Log( "HAPI_AssetOTL created - Instance Id: " + GetInstanceID() );
		
		// These variables need to keep between asset reloads.
		prAssetPath = "";
		
		reset();
	}
	
	~HAPI_AssetOTL() 
	{}
	
	public override void reset()
	{
		// Save the asset type so we can restore it after the reset.
		HAPI_Asset.AssetType asset_type = prAssetType;

		base.reset();
		
		// Please keep these in the same order and grouping as their declarations at the top.
		
		prHandleInfos 				= new HAPI_HandleInfo[ 0 ];
		prHandleBindingInfos 		= null;

		// Need to restore the asset type here.
		prAssetType = asset_type;

		// Set undo info
		if ( myAssetOTLUndoInfo == null )
			myAssetOTLUndoInfo = ScriptableObject.CreateInstance< HAPI_AssetOTLUndoInfo >();

		myAssetOTLUndoInfo.isGeoVisible 				= prIsGeoVisible;
		myAssetOTLUndoInfo.showPinnedInstances 			= prShowPinnedInstances;
		myAssetOTLUndoInfo.autoSelectAssetRootNode 		= prAutoSelectAssetRootNode;
		myAssetOTLUndoInfo.hideGeometryOnLinking 		= prHideGeometryOnLinking;
		myAssetOTLUndoInfo.materialShaderType 			= prMaterialShaderType;
		myAssetOTLUndoInfo.renderResolution 			= prRenderResolution;
		myAssetOTLUndoInfo.showOnlyVertexColours 		= prShowOnlyVertexColours;
		myAssetOTLUndoInfo.generateTangents 			= prGenerateTangents;
		myAssetOTLUndoInfo.enableCooking 				= prEnableCooking;
		myAssetOTLUndoInfo.cookingTriggersDownCooks 	= prCookingTriggersDownCooks;
		myAssetOTLUndoInfo.playmodePerFrameCooking 		= prPlaymodePerFrameCooking;
		myAssetOTLUndoInfo.pushUnityTransformToHoudini	= prPushUnityTransformToHoudini;
		myAssetOTLUndoInfo.transformChangeTriggersCooks = prTransformChangeTriggersCooks;
		myAssetOTLUndoInfo.importTemplatedGeos 			= prImportTemplatedGeos;
		myAssetOTLUndoInfo.bakeStartTime				= prBakeStartTime;
		myAssetOTLUndoInfo.bakeEndTime					= prBakeEndTime;
		myAssetOTLUndoInfo.bakeSamplesPerSecond			= prBakeSamplesPerSecond;
	}
	
	public override void Update()
	{
		base.Update();

		if ( EditorApplication.isPlaying && prPlaymodePerFrameCooking )
		{
			HAPI_Host.setTime( Time.time );
			buildClientSide();
		}
	}

	public override bool buildAll()
	{
		bool unload_asset_first = ( prAssetType == HAPI_Asset.AssetType.TYPE_OTL );

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
							 && prAssetType == HAPI_Asset.AssetType.TYPE_OTL 
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

	protected override int buildCreateAsset()
	{
		return HAPI_Host.loadOTL( prAssetPath );
	}

	protected override void buildFullBuildCustomWork( ref HAPI_ProgressBar progress_bar )
	{
		progress_bar.prMessage = "Loading handles...";
				
		// Get exposed handle information.
		prHandleInfos = new HAPI_HandleInfo[ prHandleCount ];
		Utility.getArray1Id( prAssetId, HAPI_Host.getHandleInfo, prHandleInfos, prHandleCount );
				
		// Get handles.
		prHandleBindingInfos = new List< HAPI_HandleBindingInfo[] >( prHandleCount );
		for ( int handle_index = 0; handle_index < prHandleCount; ++handle_index )
		{
			progress_bar.incrementProgressBar();
			HAPI_HandleInfo handle_info = prHandleInfos[ handle_index ];
					
			if ( handle_info.typeName != "xform" && HAPI_Host.prEnableSupportWarnings )
				Debug.LogWarning( "Handle " + handle_info.name + " of type " 
								   	+ handle_info.typeName + " is unsupported at this time." );
					
			HAPI_HandleBindingInfo[] binding_infos = new HAPI_HandleBindingInfo[ handle_info.bindingsCount ];
			Utility.getArray2Id( prAssetId, handle_index, HAPI_Host.getHandleBindingInfo, 
								 	binding_infos, handle_info.bindingsCount );
					
			prHandleBindingInfos.Add( binding_infos );
		}
	}

	protected override void buildCreateObjects( bool reload_asset, ref HAPI_ProgressBar progress_bar )
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
			catch ( HAPI_Error error )
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
						
					instanceObjects( object_index, progress_bar );
				}
				catch ( HAPI_Error error )
				{
					// Per-object errors are not re-thrown so that the rest of the asset has a chance to load.
					Debug.LogWarning( error.ToString() );
				}
			}
		}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private Methods
	
	private void instanceObjects( int object_id, HAPI_ProgressBar progress_bar )
	{
		HAPI_ObjectInfo object_info		= prObjects[ object_id ];
		HAPI_Instancer instancer		= null;
		
		Transform old_instancer_transform = transform.Find( object_info.name );
		if ( old_instancer_transform && old_instancer_transform.gameObject.GetComponent< HAPI_Instancer >() )
		{
			instancer = old_instancer_transform.gameObject.GetComponent< HAPI_Instancer >();
		}
		else
		{
			GameObject main_object = new GameObject( object_info.name );
			main_object.transform.parent = transform;

			main_object.AddComponent( "HAPI_Instancer" );
			prGameObjects[ object_id ] = main_object;
			instancer = main_object.GetComponent< HAPI_Instancer >();
		}
		
		instancer.prAsset		= this;
		instancer.prObjectId	= object_id;
		
		instancer.instanceObjects( progress_bar );
	}

	private void createObject( int object_id, bool reload_asset )
	{
		HAPI_ObjectControl object_control = null;
		HAPI_ObjectInfo object_info = prObjects[ object_id ];
		
		// Create main underling.
		if ( prGameObjects[ object_id ] == null )
		{
			prGameObjects[ object_id ] = new GameObject( object_info.name + "_obj" + object_id );
			prGameObjects[ object_id ].transform.parent = transform;

			object_control = prGameObjects[ object_id ].AddComponent< HAPI_ObjectControl >();
			object_control.init( prAssetId, object_info.nodeId, prAsset, object_id, object_info.name, object_info.isVisible );
		}
		else
		{
			object_control = prGameObjects[ object_id ].GetComponent< HAPI_ObjectControl >();
		}
		GameObject main_child = prGameObjects[ object_id ];
		
		try
		{
			object_control.refresh( reload_asset, object_info );

			if ( reload_asset || object_info.hasTransformChanged )
			{
				// Get transforms.
				HAPI_Transform trans = prObjectTransforms[ object_id ];
				Utility.applyTransform( trans, main_child.transform );
			}
		}
		catch ( HAPI_Error error )
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
	[SerializeField] private HAPI_AssetOTLUndoInfo	myAssetOTLUndoInfo;
#endif // UNITY_EDITOR
	
}
