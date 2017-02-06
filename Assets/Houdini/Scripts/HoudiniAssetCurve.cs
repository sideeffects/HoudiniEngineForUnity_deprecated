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

using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;

[ ExecuteInEditMode ]
public class HoudiniAssetCurve : HoudiniAsset
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	
	// Please keep these in the same order and grouping as their initializations in HAPI_Asset.reset().

	public HoudiniCurve				prCurve {	get { 
													HoudiniCurve curve = gameObject.GetComponent< HoudiniCurve >();
													if ( curve == null )
													{
														curve = gameObject.AddComponent< HoudiniCurve >();
														curve.prControl = this;
														curve.prParms = this.prParms;
													}
													return curve;
												}
												private set { } }
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods
	
	public HoudiniAssetCurve() 
	{
		if ( prEnableLogging )
			Debug.Log( "HoudiniAssetCurve created!" );

		reset();
	}
	
	~HoudiniAssetCurve()
	{}

	public override void reset()
	{
		base.reset();
		
		// Overwrite some settings that should be different by default for curves than other asset types.
		prCookingTriggersDownCooks = true;
		prPushUnityTransformToHoudini = true;
		prTransformChangeTriggersCooks = true;
		prAssetType = AssetType.TYPE_CURVE;
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
								bool is_duplication,
								bool cook_downstream_assets,
								bool use_delay_for_progress_bar ) 
	{
		unload_asset_first = unload_asset_first && !serialization_recovery_only;

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
		return HoudiniHost.createCurve();
	}

	protected override void buildFullBuildCustomWork( ref HoudiniProgressBar progress_bar, bool is_duplication )
	{
		prCurve.initDefaultParameters();
	}

	protected override bool buildCreateObjects( bool reload_asset, ref HoudiniProgressBar progress_bar )
	{
		try
		{
			prCurve.syncPointsWithParm();

			HAPI_GeoInfo geo_info = HoudiniHost.getDisplayGeoInfo( prCurve.prControl.prAssetId );
			HAPI_NodeInfo geo_node_info = HoudiniHost.getNodeInfo( geo_info.nodeId );
			prCurve.createObject( geo_node_info.parentId, geo_info.nodeId );

			HoudiniHost.repaint();
		}
		catch ( HoudiniError )
		{
			// Per-object errors are not re-thrown so that the rest of the asset has a chance to load.
			//Debug.LogWarning( error.ToString() );
		}

		return false;
	}

}
