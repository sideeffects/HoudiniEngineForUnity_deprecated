/*
* Copyright (c) <2017> Side Effects Software Inc.
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*
* Produced by:
*      Side Effects Software Inc
*      123 Front Street West, Suite 1401
*      Toronto, Ontario
*      Canada   M5J 2M2
*      416-504-9876
*
*/

// Master control for enabling runtime.
#if ( UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_LINUX )
	#define HAPI_ENABLE_RUNTIME
#endif

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
	
	public string prAssetPath {									get { return myAssetPath; } 
																set { myAssetPath = value; } }

	public HoudiniGeoAttributeManager prActiveAttributeManager {get { return myActiveAttributeManager; }
																set { myActiveAttributeManager = value; } }
	public List< HoudiniGeoControl > prEditPaintGeos {			get { return myEditPaintGeos; }
																set { myEditPaintGeos = value; } }
	public HAPI_HandleInfo[] prHandleInfos { get { return myHandleInfos; } set { myHandleInfos = value; } }
	public List< HAPI_HandleBindingInfo[] > prHandleBindingInfos { get { return myHandleBindingInfos; }
																   set { myHandleBindingInfos = value; } }
	
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

		prAssetPath					= "";

		prActiveAttributeManager	= null;
		prEditPaintGeos				= new List< HoudiniGeoControl >();

		prHandleInfos 				= new HAPI_HandleInfo[ 0 ];
		prHandleBindingInfos 		= null;

		// Need to restore the asset type here.
		prAssetType = asset_type;
	}

#if ( HAPI_ENABLE_RUNTIME )
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
#endif // ( HAPI_ENABLE_RUNTIME )

	public override bool buildAll()
	{
		bool unload_asset_first = ( prAssetType == HoudiniAsset.AssetType.TYPE_OTL );

		return base.build(	true,	// reload_asset
							unload_asset_first,
							false,	// serializatin_recovery_only
							true,	// force_reconnect
							false,	// is_duplication
							prCookingTriggersDownCooks,
							false	// use_delay_for_progress_bar
						);
	}

	public override bool build( bool reload_asset, bool unload_asset_first,
								bool serialization_recovery_only,
								bool force_reconnect,
								bool is_duplication,
								bool cook_downstream_assets,
								bool use_delay_for_progress_bar ) 
	{
		unload_asset_first = unload_asset_first 
							 && prAssetType == HoudiniAsset.AssetType.TYPE_OTL 
							 && !serialization_recovery_only;

		bool base_built = base.build(
			reload_asset,
			unload_asset_first,
			serialization_recovery_only,
			force_reconnect,
			is_duplication,
			cook_downstream_assets,
			use_delay_for_progress_bar );
		if ( !base_built )
			return false;

		return true;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Protected Methods

	protected override int buildCreateAsset( HoudiniProgressBar progress_bar )
	{
		// Try to find the otl if it has moved.
#if UNITY_EDITOR
#if UNITY_4_3 || UNITY_4_4
		// Finding moved assets is not supported in Unity 4.4 and earlier versions.
#else
		if ( !System.IO.File.Exists( prAssetPath ) )
		{
			string file_name = System.IO.Path.GetFileNameWithoutExtension( prAssetPath );
			string[] guids = AssetDatabase.FindAssets( file_name );
			if ( guids.Length == 0 )
			{
				throw new HoudiniError(
					"Houdini asset file has moved from last location: " + prAssetPath );
			}
			string new_path = AssetDatabase.GUIDToAssetPath( guids[ 0 ] );
			if ( new_path != null && new_path != "" )
			{
				Debug.Log( "Changing asset path for " + name + " to: " + new_path );
				prAssetPath = new_path;
			}
		}
#endif // UNITY_4_3 || UNITY_4_4
#endif // UNITY_EDITOR

		return HoudiniHost.loadOTL(
			prAssetPath, prSplitGeosByGroup, prSplitPointsByVertexAttribute,
			prImportTemplatedGeos, progress_bar, false );
	}

	protected override void buildFullBuildCustomWork( ref HoudiniProgressBar progress_bar, bool is_duplication )
	{
		// Initialize (or leave alone!) the pain structs.
		if ( prEditPaintGeos == null )
		{
			prEditPaintGeos = new List< HoudiniGeoControl >();
			prActiveAttributeManager = null;
		}
		else if ( is_duplication )
		{
			prActiveAttributeManager = null;
		}

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

			HAPI_HandleBindingInfo[] binding_infos = new HAPI_HandleBindingInfo[ handle_info.bindingsCount ];
			HoudiniAssetUtility.getArray2Id(
				prAssetId, handle_index, HoudiniHost.getHandleBindingInfo, 
				binding_infos, handle_info.bindingsCount );

			prHandleBindingInfos.Add( binding_infos );
		}
	}

	protected override bool buildCreateObjects( bool reload_asset, ref HoudiniProgressBar progress_bar )
	{
		bool needs_recook = false;

		for ( int object_index = 0; object_index < prObjectCount; ++object_index )
		{
			progress_bar.incrementProgressBar();
			try
			{
				if ( !prObjects[ object_index ].isInstancer && 
						( reload_asset	|| prObjects[ object_index ].hasTransformChanged
										|| prObjects[ object_index ].haveGeosChanged ) )
				{
					needs_recook |= createObject( object_index, reload_asset );
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
					if ( object_info.objectToInstanceId >= 0 )
					{
						int instanced_object_index = findObjectByNodeId( object_info.objectToInstanceId );
						if ( instanced_object_index >= 0 )
							needs_recook |= createObject( instanced_object_index, reload_asset );
					}

					if ( reload_asset || object_info.haveGeosChanged )
						instanceObjects( object_info.nodeId, object_index, progress_bar );
				}
				catch ( HoudiniError error )
				{
					// Per-object errors are not re-thrown so that the rest of the asset has a chance to load.
					Debug.LogWarning( error.ToString() );
				}
			}
		}

		if ( prObjectCount <= 0 )
		{
			bool needs_init = prObjectInfo.nodeId < 0 || reload_asset;

			if ( prNodeInfo.type == HAPI_NodeType.HAPI_NODETYPE_OBJ )
				prObjectInfo = HoudiniHost.getObjectInfo( prAssetId );
			else if ( prNodeInfo.type == HAPI_NodeType.HAPI_NODETYPE_SOP )
				prObjectInfo = HoudiniHost.getObjectInfo( prNodeInfo.parentId );
			else
			{
				Debug.LogError( "Unsupported asset type!" );
				return needs_recook;
			}

			if ( needs_init )
			{
				init(
					prAssetId, prNodeId, prAsset,
					prObjectInfo.nodeId, prObjectInfo.name, prObjectInfo.isVisible );
			}

			needs_recook = refresh( reload_asset, prObjectInfo );
		}

		// Enumerate edit and paint geos.
		HoudiniGeoControl[] geo_controls = gameObject.GetComponentsInChildren< HoudiniGeoControl >();
		prEditPaintGeos.Clear();
		foreach ( HoudiniGeoControl geo_control in geo_controls )
		{
			if ( geo_control.prGeoType == HAPI_GeoType.HAPI_GEOTYPE_INTERMEDIATE
				&& geo_control.GetType() == typeof( HoudiniGeoControl ) )
			{
				prEditPaintGeos.Add( geo_control );
			}
		}
		if ( prEditPaintGeos.Count > 0 && prActiveAttributeManager == null )
			prActiveAttributeManager = prEditPaintGeos[ 0 ].prGeoAttributeManager;

		return needs_recook;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private Methods
	
	private void instanceObjects( int object_id, int object_idx, HoudiniProgressBar progress_bar )
	{
		HAPI_ObjectInfo object_info = prObjects[ object_idx ];
		HoudiniInstancer instancer = null;
		
		Transform old_instancer_transform = transform.Find( object_info.name );
		if ( old_instancer_transform && old_instancer_transform.gameObject.GetComponent< HoudiniInstancer >() )
		{
			instancer = old_instancer_transform.gameObject.GetComponent< HoudiniInstancer >();
		}
		else
		{

			if ( gameObject.GetComponent< HoudiniInstancerManager >() == null )
				gameObject.AddComponent< HoudiniInstancerManager >();

			GameObject main_object = new GameObject( object_info.name );
			main_object.transform.parent = transform;

			main_object.AddComponent< HoudiniInstancer >();
			prGameObjects[ object_idx ] = main_object;
			instancer = main_object.GetComponent< HoudiniInstancer >();

			HoudiniInstancerManager instancer_manager = gameObject.GetComponent< HoudiniInstancerManager >();
			instancer_manager.updateInstancerData( instancer );
		}
		
		instancer.prAsset = this;
		instancer.prObjectId = object_id;
		
		instancer.instanceObjects( progress_bar );
	}

	private bool createObject( int object_id, bool reload_asset )
	{
		bool needs_recook = false;
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
				prAssetId, object_info.nodeId, prAsset, object_info.nodeId, object_info.name, object_info.isVisible );
		}
		else
		{
			object_control = prGameObjects[ object_id ].GetComponent< HoudiniObjectControl >();
		}
		GameObject main_child = prGameObjects[ object_id ];
		
		try
		{
			needs_recook |= object_control.refresh( reload_asset, object_info );

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
			error.addMessagePrefix( "Obj(id: " + object_info.nodeId + ", name: " + object_info.name + ")" );
			error.addMessageDetail( "Object Path: " + object_info.objectInstancePath );
			throw;
		}

		return needs_recook;
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Serialized Data
	
	[SerializeField] private string myAssetPath;

	[SerializeField] private HoudiniGeoAttributeManager myActiveAttributeManager;
	[SerializeField] private List< HoudiniGeoControl > myEditPaintGeos;
	[SerializeField] private HAPI_HandleInfo[] myHandleInfos;
	[SerializeField] private List< HAPI_HandleBindingInfo[] > myHandleBindingInfos;
}
