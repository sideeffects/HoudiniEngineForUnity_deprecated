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
	
	// Please keep these in the same order and grouping as their initializations in HAPI_Control.reset().

	public int prGeoId {				get { return myGeoId; }		set { myGeoId = value; } }
	public string prGeoName {			get { return myGeoName; }	set { myGeoName = value; } }
	public HAPI_GeoType prGeoType {		get { return myGeoType; }	set { myGeoType = value; } }
	public bool prIsEditable {			get { return myIsEditable; }set { myIsEditable = value; } }
	public bool prIsDisplay {			get { return myIsDisplay; }	set { myIsDisplay = value; } }
	public HoudiniObjectControl prObjectControl { get { return myObjectControl; } set { myObjectControl = value; } }

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

	public override string getFullControlNameAndPath()
	{
		return base.getFullControlNameAndPath() + "/" + prGeoName;
	}

	public void refresh( bool reload_asset )
	{
		if ( prObjectControl == null )
		{
			Debug.LogError( "Why is my object control null on a refresh?" );
			return;
		}

		GameObject geo_node = gameObject;

		// Get Geo info.
		HAPI_GeoInfo geo_info = new HAPI_GeoInfo();
		HoudiniHost.getGeoInfo( prAssetId, prObjectId, prGeoId, out geo_info );

		if ( geo_info.type == HAPI_GeoType.HAPI_GEOTYPE_INPUT )
			return;

		if ( !reload_asset && !geo_info.hasGeoChanged && !geo_info.hasMaterialChanged )
			return;

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
			return;
		}

		if ( geo_info.type == HAPI_GeoType.HAPI_GEOTYPE_CURVE )
		{
			createAndInitCurve( prNodeId, prObjectId, prGeoId, prIsEditable );
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

			if ( reload_asset && geo_info.partCount > 0 )
			{
				HAPI_AttributeInfo script_attr_info = new HAPI_AttributeInfo( "Unity_Script" );
				int[] script_attr = new int[ 0 ];
			
				HoudiniAssetUtility.getAttribute( 
					prAssetId, prObjectId, prGeoId, 0, "Unity_Script",
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
					prAsset.prCookingTriggersDownCooks,
					true			// use_delay_for_progress_bar
				);

			createAndInitCurve( prNodeId, prObjectId, prGeoId, prIsEditable );

#if UNITY_EDITOR
			// To keep things consistent with Unity workflow, we should not save parameter changes
			// while in Play mode.
			if ( !EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode )
				prAsset.prPresetsMap.set( getFullControlNameAndPath(), HoudiniHost.getPreset( prNodeId ) );
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
			
			if ( prAsset.prPresetsMap.contains( getFullControlNameAndPath() ) )
			{
				HoudiniPresetMap map = prAsset.prPresetsMap;
				byte[] preset = map.get( getFullControlNameAndPath() );
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

	[SerializeField] private List< GameObject > myParts;
}
