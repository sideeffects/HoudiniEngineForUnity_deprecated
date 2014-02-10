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

using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using HAPI;
using Utility = HAPI_AssetUtility;

[ ExecuteInEditMode ]
[ RequireComponent( typeof( MeshFilter ) ) ]
public class HAPI_AssetInput : HAPI_Asset
{
#if UNITY_STANDALONE_WIN
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties

	// Please keep these in the same order and grouping as their initializations in HAPI_Asset.reset().

	public Mesh			prEditableMesh {				get { return myEditableMesh; }
														set { myEditableMesh = value; } }
	public Mesh			prOriginalMesh {				get { return myOriginalMesh; }
														set { myOriginalMesh = value; } }
	public HAPI_GeoAttributeManager prGeoAttributeManager { get { return myGeoAttributeManager; }
															private set {} }

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods
	
	public HAPI_AssetInput() 
	{
		if ( prEnableLogging )
			Debug.Log( "HAPI_Asset created!" );

		reset();
	}
	
	~HAPI_AssetInput()
	{}

	public override void reset()
	{
		base.reset();

		prEditableMesh = null;
		prOriginalMesh = null;
		
		// Overwrite some settings that should be different by default for input assets than other asset types.
		prAutoSelectAssetRootNode	= false;
		prHideGeometryOnLinking		= false;
		prAssetType					= AssetType.TYPE_INPUT;

		myGeoAttributeManager		= null;
	}

	public override void OnEnable()
	{
		/* TODO
		
		// If this curve asset is a duplicate then it will have the same mesh
		// filter and mesh renderer as the curve it was duplicated from. Since
		// we want it to have its own, delete the existing ones here and new
		// ones will be created in the build.
		if ( isDuplicatingAsset() )
		{
			MeshFilter mesh_filter = gameObject.GetComponent< MeshFilter >();
			if ( mesh_filter != null )
				DestroyImmediate( mesh_filter );

			MeshRenderer mesh_renderer = gameObject.GetComponent< MeshRenderer >();
			if ( mesh_renderer != null )
				DestroyImmediate( mesh_renderer );
		}
		*/
		base.OnEnable();

		if ( prAssetId < 0 )
			buildAll();
	}
	
	public override bool build( bool reload_asset, bool unload_asset_first,
								bool serialization_recovery_only,
								bool force_reconnect,
								bool cook_downstream_assets,
								bool use_delay_for_progress_bar ) 
	{
		unload_asset_first = unload_asset_first && ( !serialization_recovery_only || isPrefab() );

		bool base_built = base.build( reload_asset, unload_asset_first, serialization_recovery_only, 
									  force_reconnect, cook_downstream_assets, use_delay_for_progress_bar );
		if ( !base_built )
			return false;
		
		return true;
	}

	public void resetFull()
	{
		// Safe to assume these exist because of [RequiredComponent] attributes.
		MeshFilter mesh_filter = gameObject.GetComponent< MeshFilter >();
		if ( prOriginalMesh )
			mesh_filter.sharedMesh = prOriginalMesh;

		HAPI_Host.destroyAsset( prAssetId );

		reset();
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Protected Methods

	protected override int buildCreateAsset()
	{
		return HAPI_Host.createInputAsset( transform.name );
	}

	protected override void buildFullBuildCustomWork( ref HAPI_ProgressBar progress_bar )
	{
		cloneMesh();

		if ( myGeoAttributeManager == null )
		{
			MeshRenderer mesh_renderer = getOrCreateComponent< MeshRenderer >();
			MeshCollider mesh_collider = getOrCreateComponent< MeshCollider >();

			myGeoAttributeManager = ScriptableObject.CreateInstance< HAPI_GeoAttributeManager >();
			myGeoAttributeManager.init( prEditableMesh, mesh_renderer, mesh_collider, transform );
		}
	}

	protected override void buildCreateObjects( bool reload_asset, ref HAPI_ProgressBar progress_bar )
	{
		try
		{
			const int object_id = 0;
			const int geo_id = 0;

			// Write marshalled geo to Input Asset.
			HAPI_AssetUtility.setMesh(
				prAssetId, object_id, geo_id, ref myEditableMesh, null, myGeoAttributeManager );

			// Apply the input asset transform to the marshaled object in the Houdini scene.
			HAPI_TransformEuler trans = Utility.getHapiTransform( transform.localToWorldMatrix );
			HAPI_Host.setObjectTransform( prAssetId, object_id, trans );

			// Marshall in the animation.
			Animation anim_component = GetComponent< Animation >();
			if ( anim_component )
				if ( anim_component.clip != null )
					marshalCurvesFromClip( prObjectNodeId, anim_component.clip );
				else
					foreach ( AnimationState anim_state in anim_component )
					{
						AnimationClip clip = anim_component.GetClip( anim_state.name );
						if ( clip != null )
						{
							marshalCurvesFromClip( prObjectNodeId, clip );
							break;
						}
					}

			HAPI_Host.repaint();
		}
		catch ( HAPI_Error )
		{
			// Per-object errors are not re-thrown so that the rest of the asset has a chance to load.
			//Debug.LogWarning( error.ToString() );
		}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private

	private void cloneMesh()
	{
		// Safe to assume these exist because of [RequiredComponent] attributes.
		MeshFilter mesh_filter = gameObject.GetComponent< MeshFilter >();

		// Create the editable mesh from the original mesh. We don't want to
		// modify the original mesh because it is likely shared by many
		// instances.
		if ( prEditableMesh == null )
		{
			prOriginalMesh = mesh_filter.sharedMesh;
			prEditableMesh = Mesh.Instantiate( prOriginalMesh ) as Mesh;
			prEditableMesh.name = prOriginalMesh.name + " (Editable Copy)";

			Color[] colours = new Color[ prEditableMesh.vertexCount ];
			for ( int i = 0; i < prEditableMesh.vertexCount; ++i )
				colours[ i ] = new Color( 1.0f, 1.0f, 1.0f );
			prEditableMesh.colors = colours;

			mesh_filter.sharedMesh = prEditableMesh;
		}
	}

	[SerializeField] private Mesh			myEditableMesh;
	[SerializeField] private Mesh			myOriginalMesh;

	[SerializeField] private HAPI_GeoAttributeManager myGeoAttributeManager;

#endif // UNITY_STANDALONE_WIN
}
