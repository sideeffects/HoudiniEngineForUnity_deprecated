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
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;

// Typedefs
using HAPI_NodeId = System.Int32;

public class HoudiniGeoControl : HoudiniObjectControl 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	
	// Please keep these in the same order and grouping as their initializations in reset().

	public int prGeoId {				get { return myGeoId; }		set { myGeoId = value; } }
	public string prGeoName {			get { return myGeoName; }	set { myGeoName = value; } }
	public HAPI_GeoType prGeoType {		get { return myGeoType; }	set { myGeoType = value; } }
	public bool prIsEditable {			get { return myIsEditable; }set { myIsEditable = value; } }
	public bool prIsDisplay {			get { return myIsDisplay; }	set { myIsDisplay = value; } }
	public HoudiniObjectControl prObjectControl { get { return myObjectControl; } set { myObjectControl = value; } }

	public HoudiniGeoAttributeManager prGeoAttributeManager { get { return myGeoAttributeManager; }
															private set {} }

	public List< GameObject > prParts {	get { return myParts; }		set { myParts = value; } }

	public HoudiniGeoControl() 
	{
		reset();
	}

	~HoudiniGeoControl()
	{

	}

	public override void reset()
	{
		base.reset();

		// Please keep these in the same order and grouping as their declarations at the top.
		
		prGeoId			= -1;
		prGeoName		= "geo_name";
		prGeoType		= HAPI_GeoType.HAPI_GEOTYPE_DEFAULT;
		prIsEditable	= false;
		prIsDisplay		= true;
		prObjectControl	= null;

		myGeoAttributeManager = null;

		myParts			= new List< GameObject >( 0 );
	}

	public void init( HoudiniGeoControl geo_control )
	{
		init( (HoudiniObjectControl) geo_control );

		prGeoId			= geo_control.prGeoId;
		prGeoName		= geo_control.prGeoName;
		prGeoType		= geo_control.prGeoType;
		prIsEditable	= geo_control.prIsEditable;
		prIsDisplay		= geo_control.prIsDisplay;
	}

	public void init( 
		HAPI_NodeId node_id, int geo_id, string geo_name, HAPI_GeoType geo_type, bool editable, bool display )
	{
		prNodeId		= node_id;
		prGeoId			= geo_id;
		prGeoName		= geo_name;
		prGeoType		= geo_type;
		prIsEditable	= editable;
		prIsDisplay		= display;
	}

	public override string getRelativePath()
	{
		return base.getRelativePath() + "/" + prGeoName;
	}

	public override string getAbsolutePath()
	{
		return base.getAbsolutePath() + "/" + prGeoName;
	}

	public bool refresh( bool reload_asset )
	{
		bool needs_recook = false;

		if ( prObjectControl == null )
		{
			Debug.LogError( "Why is my object control null on a refresh?" );
			return needs_recook;
		}

		GameObject geo_node = gameObject;

		// Get Geo info.
		HAPI_GeoInfo geo_info = new HAPI_GeoInfo();
		try
		{
			// If templated geos are off this will error out for templated
			// geos because they woudn't have cooked. But we still need to
			// get the geo info to see that this is a templated geo and skip it.
			geo_info = HoudiniHost.getGeoInfo( prGeoId );
		}
		catch ( HoudiniErrorInvalidArgument ) {}

		if ( geo_info.type == HAPI_GeoType.HAPI_GEOTYPE_INPUT )
			return needs_recook;

		if ( geo_info.isTemplated && !prAsset.prImportTemplatedGeos && !geo_info.isEditable )
			return needs_recook;

		if ( !reload_asset && !geo_info.hasGeoChanged && !geo_info.hasMaterialChanged )
			return needs_recook;

		if ( reload_asset || geo_info.type == HAPI_GeoType.HAPI_GEOTYPE_CURVE )
		{
			for ( int i = 0; i < myParts.Count; ++i )
				HoudiniAssetUtility.destroyGameObject( myParts[ i ] );
			myParts.Clear();
		}

		if ( reload_asset || geo_info.hasGeoChanged )
		{
			// Initialize our geo control.
			init( 
				geo_info.nodeId, prGeoId, geo_info.name, (HAPI_GeoType) geo_info.type, 
				geo_info.isEditable, geo_info.isDisplayGeo );

			// Set node name.
			geo_node.name = prGeoName;
		}

		if ( !geo_info.isDisplayGeo && 
				(	geo_info.type != HAPI_GeoType.HAPI_GEOTYPE_CURVE && 
					!myObjectControl.prAsset.prImportTemplatedGeos && 
					geo_info.isTemplated ) )
		{
			return needs_recook;
		}

		if ( geo_info.type == HAPI_GeoType.HAPI_GEOTYPE_CURVE )
		{
			createAndInitCurve( prNodeId, prObjectId, prGeoId, prIsEditable );
			//needs_recook = true;
		}
		else
		{
			if ( reload_asset || geo_info.hasGeoChanged )
			{
				// Add new geos as needed.
				while ( myParts.Count < geo_info.partCount )
					myParts.Add( createPart( myParts.Count ) );

				// Remove stale geos.
				while ( myParts.Count > geo_info.partCount )
				{
					HoudiniAssetUtility.destroyGameObject( myParts[ geo_info.partCount ] );
					myParts.RemoveAt( geo_info.partCount );
				}
			}
		
			// Refresh all geos.
			for ( int i = 0; i < myParts.Count; ++i )
				myParts[ i ].GetComponent< HoudiniPartControl >().refresh( 
					reload_asset, geo_info.hasGeoChanged, geo_info.hasMaterialChanged );

			// Handle Edit/Paint Nodes
			if ( geo_info.type == HAPI_GeoType.HAPI_GEOTYPE_INTERMEDIATE )
			{
				// Currently, we only support painting on the first part.
				const int part_id = 0;

				GameObject part_gameobject = myParts[ part_id ];
				HoudiniPartControl part_control = part_gameobject.GetComponent< HoudiniPartControl >();
				MeshFilter mesh_filter = part_control.getOrCreateComponent< MeshFilter >();
				MeshRenderer mesh_renderer = part_control.getOrCreateComponent< MeshRenderer >();
				MeshCollider mesh_collider = part_control.getOrCreateComponent< MeshCollider >();
				Mesh mesh = mesh_filter.sharedMesh;

				// We are limited to using the first part, always.
				if ( myGeoAttributeManager == null && myParts.Count > 0 )
				{
					if ( prAsset.prGeoAttributeManagerMap.contains( getRelativePath() ) )
					{
						myGeoAttributeManager = prAsset.prGeoAttributeManagerMap.get( getRelativePath() );
						myGeoAttributeManager.name = getAbsolutePath() + "/GeoAttributeManager";
						myGeoAttributeManager.reInit( mesh, mesh_renderer, mesh_collider, part_gameobject.transform );
					}
					else
					{
						myGeoAttributeManager = ScriptableObject.CreateInstance< HoudiniGeoAttributeManager >();
						myGeoAttributeManager.name = getAbsolutePath() + "/GeoAttributeManager";
						myGeoAttributeManager.init( mesh, mesh_renderer, mesh_collider, part_gameobject.transform );
						prAsset.prGeoAttributeManagerMap.add( getRelativePath(), myGeoAttributeManager );
					}

					// Sync the attributes and see if we need a recook.
					if ( myGeoAttributeManager.syncAttributes( prAssetId, prObjectId, prGeoId, part_id, mesh ) )
					{
						HoudiniAssetUtility.setMesh(
							prAssetId, prObjectId, prGeoId,
							ref mesh, part_control, myGeoAttributeManager );
						needs_recook = true;
					}
				}
				else
				{
					// Just sync the attributes but don't recook. Setting needs_recook to true here would
					// cause infinite cooking.
					myGeoAttributeManager.syncAttributes( prAssetId, prObjectId, prGeoId, part_id, mesh );
				}
			}

			// Handle script attaching.
			if ( reload_asset && geo_info.partCount > 0 )
			{
				HAPI_AttributeInfo script_attr_info = new HAPI_AttributeInfo( "Unity_Script" );
				int[] script_attr = new int[ 0 ];
			
				HoudiniAssetUtility.getAttribute( 
					prGeoId, 0, "Unity_Script",
					ref script_attr_info, ref script_attr, HoudiniHost.getAttributeStringData );
			
				if ( script_attr_info.exists && script_attr_info.owner != HAPI_AttributeOwner.HAPI_ATTROWNER_DETAIL )
					throw new HoudiniErrorIgnorable( "I only understand Unity_Script as detail attributes!" );
			
				if ( script_attr_info.exists && script_attr.Length > 0 )
				{
					string script_to_attach = HoudiniHost.getString( script_attr[ 0 ] );
					HoudiniAssetUtility.attachScript( geo_node, script_to_attach );
				}
			}
		}

		return needs_recook;
	}

	public override void onParmChange()
	{
		base.onParmChange();

		if ( prGeoType == HAPI_GeoType.HAPI_GEOTYPE_CURVE && prIsEditable )
		{
			prParms.setChangedParametersIntoHost();

			if ( prAsset )
				prAsset.build(
					false,			// reload_asset
					false,			// unload_asset_first
					false,			// serializatin_recovery_only
					false,			// force_reconnect
					false,			// is_duplication
					prAsset.prCookingTriggersDownCooks,
					true			// use_delay_for_progress_bar
				);

			createAndInitCurve( prNodeId, prObjectId, prGeoId, prIsEditable );

#if UNITY_EDITOR
			// To keep things consistent with Unity workflow, we should not save parameter changes
			// while in Play mode.
			if ( !EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode )
				prAsset.prPresetsMap.set( getRelativePath(), HoudiniHost.getPreset( prNodeId ) );
#endif // UNITY_EDITOR
		}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private Methods

	private void createAndInitCurve( int node_id, int object_id, int geo_id, bool editable )
	{
		if ( prParmsNeedInit )
		{
			prParms.prEditable = editable;
			
			if ( prAsset.prPresetsMap.contains( getRelativePath() ) )
			{
				HoudiniPresetMap map = prAsset.prPresetsMap;
				byte[] preset = map.get( getRelativePath() );
				HoudiniHost.setPreset( prNodeId, preset );

				// Unfortunately, we need to build everything again because we just changed
				// the parameters on our geo node.
				if ( prAsset )
					prAsset.buildClientSide();

				// The asset build will get the new parameter values of the asset, not our 
				// geo node parameter values. We must get them ourselves.
				prParms.getParameterValues();
			}
		}

		HoudiniCurve curve = gameObject.GetComponent< HoudiniCurve >();
		if ( curve == null )
		{
			curve = gameObject.AddComponent< HoudiniCurve >();
			curve.prControl = this;
			curve.prParms = prParms;
			curve.prEditable = editable;
			curve.prCurrentMode = HoudiniCurve.Mode.NONE;
		}

		try
		{
            curve.syncPointsWithParm();
			curve.createObject( object_id, geo_id );
			HoudiniHost.repaint();
		}
		catch ( HoudiniError )
		{
			// Per-object errors are not re-thrown so that the rest of the asset has a chance to load.
		}
	}

	private GameObject createPart( int part_id )
	{
		GameObject child = new GameObject( "uninitialized_part" );
		child.transform.parent = gameObject.transform;
		child.isStatic = gameObject.isStatic;

		// Need to reset position here because the assignment above will massage the child's
		// position in order to be in the same place it was in the global namespace.
		child.transform.localPosition	= new Vector3();
		child.transform.localRotation	= new Quaternion();
		child.transform.localScale		= new Vector3( 1.0f, 1.0f, 1.0f );

		HoudiniPartControl control = child.AddComponent< HoudiniPartControl >();
		control.init( this );
		control.prPartId = part_id;
		control.prGeoControl = this;
		control.prObjectControl = prObjectControl;

		return child;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Serialized Private Data

	[SerializeField] private int			myGeoId;
	[SerializeField] private string			myGeoName;
	[SerializeField] private HAPI_GeoType	myGeoType;
	[SerializeField] private bool			myIsEditable;
	[SerializeField] private bool			myIsDisplay;
	[SerializeField] private HoudiniObjectControl myObjectControl;

	[SerializeField] private HoudiniGeoAttributeManager myGeoAttributeManager;

	[SerializeField] private List< GameObject > myParts;
}
