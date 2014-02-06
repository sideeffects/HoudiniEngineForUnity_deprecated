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
public class HAPI_AssetCurve : HAPI_Asset
{
#if UNITY_STANDALONE_WIN
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	
	// Please keep these in the same order and grouping as their initializations in HAPI_Asset.reset().

	public HAPI_Curve				prCurve {	get { 
													HAPI_Curve curve = gameObject.GetComponent< HAPI_Curve >();
													if ( curve == null )
													{
														curve = gameObject.AddComponent< HAPI_Curve >();
														curve.prControl = this;
														curve.prParms = this.prParms;
													}
													return curve;
												}
												private set { } }
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods
	
	public HAPI_AssetCurve() 
	{
		if ( prEnableLogging )
			Debug.Log( "HAPI_Asset created!" );

		reset();
	}
	
	~HAPI_AssetCurve()
	{}

	public override void reset()
	{
		base.reset();
		
		// Overwrite some settings that should be different by default for curves than other asset types.
		prAutoSelectAssetRootNode	= true;
		prHideGeometryOnLinking		= false;
		prAssetType					= AssetType.TYPE_CURVE;
	}

	public override void OnEnable()
	{
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

		base.OnEnable();
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

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Protected Methods

	protected override int buildCreateAsset()
	{
		return HAPI_Host.createCurve();
	}

	protected override void buildFullBuildCustomWork( ref HAPI_ProgressBar progress_bar )
	{
		prCurve.initDefaultParameters();
	}

	protected override void buildCreateObjects( bool reload_asset, ref HAPI_ProgressBar progress_bar )
	{
		try
		{
			prCurve.syncPointsWithParm();
			prCurve.createObject( 0, 0 );
			HAPI_Host.repaint();
		}
		catch ( HAPI_Error )
		{
			// Per-object errors are not re-thrown so that the rest of the asset has a chance to load.
			//Debug.LogWarning( error.ToString() );
		}
	}
#endif // UNITY_STANDALONE_WIN

}
