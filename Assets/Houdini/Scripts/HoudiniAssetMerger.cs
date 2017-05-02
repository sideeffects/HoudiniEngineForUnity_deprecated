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
using System.Collections.Generic;

public class HoudiniAssetMerger : HoudiniAsset
{

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties

	// Please keep these in the same order and grouping as their initializations in HAPI_Asset.reset().

	public bool prShowInputSelection {
		get { return myShowInputSelection; }
		set { myShowInputSelection = value; } }
	
	public GameObject prInputObject { get { return myInputObject; } set { myInputObject = value; } }

	public bool prUseLayerMask { get { return myUseLayerMask; } set { myUseLayerMask = value; } }
	public LayerMask prLayerMask { get { return myLayerMask; } set { myLayerMask = value; } }

	public bool prUseTag { get { return myUseTag; } set { myUseTag = value; } }
	public string prTag { get { return myTag; } set { myTag = value; } }

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods

	public HoudiniAssetMerger() 
	{
		if ( prEnableLogging )
			Debug.Log( "HoudiniAssetMerger created!" );

		reset();
	}
	
	~HoudiniAssetMerger()
	{}

	public override void reset()
	{
		base.reset();
		
		myShowInputSelection = true;

		myUseLayerMask = false;

		myUseTag = false;
		myTag = "";

		myInputs = new List< GameObject >();
	}

	public override void OnEnable()
	{
		base.OnEnable();

		// We want to preserve the transform of the original mesh as we
		// assetize it.
		myLastLocalToWorld = transform.localToWorldMatrix;
	}
	
	public override bool build(
		bool reload_asset, bool unload_asset_first,
		bool serialization_recovery_only,
		bool force_reconnect,
		bool is_duplication,
		bool cook_downstream_assets,
		bool use_delay_for_progress_bar )
	{
		if ( prAssetId < 0 )
			base.build(
				true,	// reload_asset
				true,	// unload_asset_first
				false,	// serializatin_recovery_only
				true,	// force_reconnect
				false,	// is_duplication
				true,	// cook_downstream_assets
				true	// use_delay_for_progress_bar
			);

		int last_input_count = myInputs.Count;
		myInputs.Clear();

		// Add by input object.
		if ( myInputObject != null )
		{
			MeshFilter[] child_mesh_filters = myInputObject.GetComponentsInChildren< MeshFilter >();
			foreach ( MeshFilter mf in child_mesh_filters )
				myInputs.Add( mf.gameObject );
		}

		// Add by layer.
		if ( prUseLayerMask && myLayerMask > 0 )
		{
			if ( myInputs.Count == 0 )
			{
				GameObject[] layer_objects = FindObjectsOfType< GameObject >();
				foreach ( GameObject obj in layer_objects )
					if ( obj.layer == myLayerMask )
						myInputs.Add( obj );
			}
			else
			{
				myInputs.RemoveAll( s => s.layer != myLayerMask );
			}
		}

		// Add by tag.
		if ( prUseTag && myTag != "" && myTag != "Untagged" )
		{
			if ( myInputs.Count == 0 )
			{
				GameObject [] tag_objects = GameObject.FindGameObjectsWithTag( myTag );
				foreach ( GameObject obj in tag_objects )
					if ( !myInputs.Contains( obj ) )
						myInputs.Add( obj );
			}
			else
			{
				myInputs.RemoveAll( s => s.tag != myTag );
			}
		}

		// Add all input objects as asset inputs.
		for ( int i = 0; i < myInputs.Count; ++i )
		{
			GameObject obj = myInputs[ i ];

			if ( !obj || !obj.GetComponent< MeshFilter >() )
				continue;

			addGeoAsGeoInput( obj, i );
		}

		// Cleanup inputs.
		while ( last_input_count > myInputs.Count )
		{
			removeGeoInput( last_input_count - 1 );
			last_input_count--;
		}

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

	public void resetFull()
	{
		HoudiniHost.destroyAsset( prAssetId );

		reset();
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Protected Methods

	protected override int buildCreateAsset( HoudiniProgressBar progress_bar )
	{
		//return HoudiniHost.createInputNode( transform.name );
		return HoudiniHost.createNode( -1, "SOP/merge", true );
	}

	protected override void buildFullBuildCustomWork( ref HoudiniProgressBar progress_bar, bool is_duplication )
	{

	}

	protected override bool buildCreateObjects( bool reload_asset, ref HoudiniProgressBar progress_bar )
	{
		

		return false;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private

	[SerializeField] private bool myShowInputSelection;

	[SerializeField] private GameObject myInputObject;


	[SerializeField] private bool myUseLayerMask;
	[SerializeField] private LayerMask myLayerMask;

	[SerializeField] private bool myUseTag;
	[SerializeField] private string myTag;

	[SerializeField] private List< GameObject > myInputs;
}
