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
using UnityEditor;
using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using HAPI;
using Utility = HAPI_AssetUtility;

[ ExecuteInEditMode ]
public abstract class HAPI_Asset : HAPI_Control
{
	public enum AssetType
	{
		TYPE_OTL = 0,
		TYPE_HIP,
		TYPE_CURVE,
		TYPE_INVALID
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	
	// Please keep these in the same order and grouping as their initializations in HAPI_Asset.reset().
	
	// Assets -------------------------------------------------------------------------------------------------------
	
	public HAPI_AssetInfo 			prAssetInfo {					get { return myAssetInfo; } 
																	set { myAssetInfo = value; } }
	public byte[]					prPreset {						get { return myPreset; } 
																	set { myPreset = value; } }
	public int						prAssetValidationId {			get { return myAssetValidationId; }
																	set { myAssetValidationId = value; } }
 	public string					prAssetName {					get { return myAssetName; }
																	set { myAssetName = value; } }
	public string					prAssetHelp {					get { return myAssetHelp; }
																	set { myAssetHelp = value; } }
	public AssetType				prAssetType {					get { return myAssetType; } 
																	set { myAssetType = value; } }
	public HAPI_AssetType			prHAPIAssetType {				get { return myHAPIAssetType; } 
																	set { myHAPIAssetType = value; } }
	public HAPI_AssetSubType		prAssetSubType {				get { return myAssetSubType; } 
																	set { myAssetSubType = value; } }

	// Parameters ---------------------------------------------------------------------------------------------------

	public HAPI_PresetMap prPresetsMap 
	{					
		get 
		{ 
			if ( myPresetsMap == null )
				myPresetsMap = ScriptableObject.CreateInstance< HAPI_PresetMap >();
			return myPresetsMap;
		}
		private set { }
	}

	// Inputs -------------------------------------------------------------------------------------------------------
	
	public int 						prMinTransInputCount {			get { return myMinTransInputCount; } 
																	set { myMinTransInputCount = value; } }
	public int 						prMaxTransInputCount {			get { return myMaxTransInputCount; } 
																	set { myMaxTransInputCount = value; } }
	public int 						prMinGeoInputCount {			get { return myMinGeoInputCount; } 
																	set { myMinGeoInputCount = value; } }
	public int						prMaxGeoInputCount {			get { return myMaxGeoInputCount; } 
																	set { myMaxGeoInputCount = value; } }
	public List< string >			prFileInputs {					get { return myFileInputs; } 
																	set { myFileInputs = value; } }
	
	public List< HAPI_Asset >		prDownStreamTransformAssets {	get { return myDownStreamTransformAssets; } 
																	set { myDownStreamTransformAssets = value; } }
	public List< HAPI_Asset >		prUpStreamTransformAssets {		get { return myUpStreamTransformAssets; } 
																	set { myUpStreamTransformAssets = value; } }
	public List< GameObject >		prUpStreamTransformObjects {	get { return myUpStreamTransformObjects; } 
																	set { myUpStreamTransformObjects = value; } }
	
	public List< HAPI_Asset >		prDownStreamGeoAssets {			get { return myDownStreamGeoAssets; } 
																	set { myDownStreamGeoAssets = value; } }
	public List< HAPI_Asset >		prUpStreamGeoAssets {			get { return myUpStreamGeoAssets; } 
																	set { myUpStreamGeoAssets = value; } }
	public List< GameObject >		prUpStreamGeoObjects {			get { return myUpStreamGeoObjects; } 
																	set { myUpStreamGeoObjects = value; } }
	public List< bool >				prUpStreamGeoAdded {			get { return myUpStreamGeoAdded; } 
																	set { myUpStreamGeoAdded = value; } }

	// Objects ------------------------------------------------------------------------------------------------------
	
	public int 						prObjectCount {					get { return myObjectCount; } 
																	set { myObjectCount = value; } }
	public int						prHandleCount {					get { return myHandleCount; } 
																	set { myHandleCount = value; } }
	
	public HAPI_ObjectInfo[] 		prObjects {						get { return myObjects; } 
																	set { myObjects = value; } }
	
	public GameObject[]				prGameObjects {					get { return myGameObjects; } 
																	set { myGameObjects = value; } }
	public HAPI_Transform[] 		prObjectTransforms {			get { return myObjectTransforms; } 
																	set { myObjectTransforms = value; } }
	
	// Baking ------------------------------------------------------------------------------------------------------

	public float					prBakeStartTime {				get { return myBakeStartTime; }
																	set { myBakeStartTime = value; } }

	public float					prBakeEndTime {					get { return myBakeEndTime; }
																	set { myBakeEndTime = value; } }

	public int						prBakeSamplesPerSecond {		get { return myBakeSamplesPerSecond; }
																	set { myBakeSamplesPerSecond = value; } }

	// GUI ----------------------------------------------------------------------------------------------------------
	
	public bool 					prShowHoudiniControls {			get { return myShowHoudiniControls; } 
																	set { myShowHoudiniControls = value; } }
	public bool 					prShowHelp {					get { return myShowHelp; } 
																	set { myShowHelp = value; } }
	public bool 					prShowAssetSettings {			get { return myShowAssetSettings; } 
																	set { myShowAssetSettings = value; } }
	public bool 					prShowBakeOptions {				get { return myShowBakeOptions; } 
																	set { myShowBakeOptions = value; } }
	public bool						prShowInputControls {			get { return myShowInputControls; } 
																	set { myShowInputControls = value; } }
	public int						prAssetSettingsCategory {		get { return myAssetOptionsCategory; }
																	set { myAssetOptionsCategory = value; } }

	public bool						prIsGeoVisible {				get { return myIsGeoVisible; }
																	set { myIsGeoVisible = value; } }
	public bool						prShowPinnedInstances {			get { return myShowPinnedInstances; }
																	set { myShowPinnedInstances = value; } }
	public bool	prAutoSelectAssetRootNode {		get { return (	myAutoSelectAssetRootNode && 
																HAPI_Host.isAutoSelectAssetRootNodeDefault() )
														  || (	HAPI_Host.prAutoSelectAssetRootNode &&
																!HAPI_Host.isAutoSelectAssetRootNodeDefault() ); } 
												set { myAutoSelectAssetRootNode = value; } }
	public bool	prHideGeometryOnLinking {		get { return (	myHideGeometryOnLinking && 
																HAPI_Host.isHideGeometryOnLinkingDefault() )
														  || (	HAPI_Host.prHideGeometryOnLinking &&
																!HAPI_Host.isHideGeometryOnLinkingDefault() ); } 
												set { myHideGeometryOnLinking = value; } }

	public HAPI_ShaderType			prMaterialShaderType {	get { return myMaterialShaderType; }
															set { myMaterialShaderType = value; } }
	public Vector2					prRenderResolution {	get { return myRenderResolution; }
															set { myRenderResolution = value; } }
	public bool						prShowOnlyVertexColours{get { return myShowOnlyVertexColours; }
															set { myShowOnlyVertexColours = value; } }
	public bool						prGenerateTangents {	get { return (	myGenerateTangents && 
																			HAPI_Host.isGenerateTangentsDefault() )
																	|| (	HAPI_Host.prGenerateTangents &&
																			!HAPI_Host.isGenerateTangentsDefault() ); } 
															set { myGenerateTangents = value; } }

	public bool	prEnableCooking {				get { return (	myEnableCooking && 
																HAPI_Host.isEnableCookingDefault() )
														  || (	HAPI_Host.prEnableCooking &&
																!HAPI_Host.isEnableCookingDefault() ); } 
												set { myEnableCooking = value; } }
	public bool	prCookingTriggersDownCooks {	get { return (	myCookingTriggersDownCooks && 
																HAPI_Host.isCookingTriggersDownCooksDefault() )
														  || (	HAPI_Host.prCookingTriggersDownCooks &&
																!HAPI_Host.isCookingTriggersDownCooksDefault() ); } 
												set { myCookingTriggersDownCooks = value; } }
	public bool	prPlaymodePerFrameCooking {		get { return (	myPlaymodePerFrameCooking && 
																HAPI_Host.isPlaymodePerFrameCookingDefault() )
														  || (	HAPI_Host.prPlaymodePerFrameCooking &&
																!HAPI_Host.isPlaymodePerFrameCookingDefault() ); } 
												set { myPlaymodePerFrameCooking = value; } }
	public bool	prPushUnityTransformToHoudini {	get { return (	myPushUnityTransformToHoudini && 
																HAPI_Host.isPushUnityTransformToHoudiniDefault() )
														  || (	HAPI_Host.prPushUnityTransformToHoudini &&
																!HAPI_Host.isPushUnityTransformToHoudiniDefault() ); } 
												set { myPushUnityTransformToHoudini = value; } }
	public bool	prTransformChangeTriggersCooks{ get { return (	myTransformChangeTriggersCooks && 
																HAPI_Host.isTransformChangeTriggersCooksDefault() )
														  || (	HAPI_Host.prTransformChangeTriggersCooks &&
																!HAPI_Host.isTransformChangeTriggersCooksDefault() ); } 
												set { myTransformChangeTriggersCooks = value; } }

	public bool						prEnableLogging {				get { return myEnableLogging; } 
																	set { myEnableLogging = value; } }

	public List< string >			prTransInputNames {				get { return myTransInputNames; }
																	set { myTransInputNames = value; } }
	public List< string >			prGeoInputNames {				get { return myGeoInputNames; }
																	set { myGeoInputNames = value; } }
	public List< HAPI_GeoInputFormat >	prGeoInputFormats {			get { return myGeoInputFormats; }
																	set { myGeoInputFormats = value; } }
	
	public List< HAPI_InstancerOverrideInfo > prOverriddenInstances { get { return myOverriddenInstances; }
																	  set {	myOverriddenInstances = value; } }
	
	// Prefabs ------------------------------------------------------------------------------------------------------
	
	public int prBackupAssetId {					get { return myBackupAssetId; }
													set { myBackupAssetId = value; } }
	public int prBackupAssetValidationId {			get { return myBackupAssetValidationId; }
													set { myBackupAssetValidationId = value; } }
	public bool prReloadPrefabOnPlaymodeChange {	get { return myReloadPrefabOnPlaymodeChange; }
													set { myReloadPrefabOnPlaymodeChange = value; } }
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods
	
	public HAPI_Asset() 
	{
		if ( prEnableLogging )
			Debug.Log( "HAPI_Asset created - Instance Id: " + GetInstanceID() );
		
		HAPI.HAPI_SetPath.setPath();

		reset();
	}
	
	~HAPI_Asset() 
	{
		if ( prEnableLogging )
			Debug.Log( "HAPI_Asset destroyed - Instance Id:" + GetInstanceID() );
		
		// This is the only place to unload OTL when prefab is being deleted. 
		// Do not delete prefab if this is being called due to entering play mode.
		if ( myCleanUpPrefabOTL &&
			 !prReloadPrefabOnPlaymodeChange &&
			 HAPI_Host.isAssetValid( prAssetId, prAssetValidationId ) )
		{
			try
			{
				HAPI_Host.unloadOTL( prAssetId );
			}
			catch ( HAPI_Error error )
			{
				Debug.LogError( "Asset failed to unload: " + error.ToString() );
			}
		}
	}
	
	public int findObjectByName( string object_name )
	{
		for ( int object_index = 0; object_index < prObjectCount; ++object_index )
		{
			HAPI_ObjectInfo object_info = prObjects[ object_index ];
			if ( object_info.name == object_name )
				return object_index;
		}
		return -1;
	}
	
	
	public GameObject findPartByName( string part_name, bool allow_partial_match )
	{
		for ( int object_index = 0; object_index < prObjectCount; ++object_index )
		{
			GameObject top_level_obj = prGameObjects[ object_index ];
			for( int geo_index = 0; geo_index < top_level_obj.transform.childCount; geo_index++ )
			{
				GameObject geo = top_level_obj.transform.GetChild( geo_index ).gameObject;
				for( int part_index = 0; part_index < geo.transform.childCount; part_index++ )
				{
					GameObject part = geo.transform.GetChild( part_index ).gameObject;
					if( allow_partial_match )
					{
						if( part.name.StartsWith( part_name ) )
							return part;
					}
					else
					{
						if( part.name == part_name )
							return part;
					}
				}
			}
			
		}
		return null;
	}

	// Transform related connection methods -------------------------------------------------------
	
	public void addAssetAsTransformInput( HAPI_Asset asset, int index )
	{
		if ( prUpStreamTransformAssets[ index ] == asset )
			return;
		
		prUpStreamTransformAssets[ index ] = asset;
		HAPI_Host.connectAssetTransform( asset.prAssetId, prAssetId, index );
		asset.addDownstreamTransformAsset( this );
	}
	
	public void removeTransformInput( int index )
	{
		if ( prUpStreamTransformAssets[ index ] != null )
		{
			prUpStreamTransformAssets[ index ].removeDownstreamTransformAsset( this );
			HAPI_Host.disconnectAssetTransform( prAssetId, index );
			prUpStreamTransformAssets[ index ] = null;
		}
		
	}
	
	public void removeAssetAsTransformInput( HAPI_Asset asset )
	{
		for ( int ii = 0; ii < prUpStreamTransformAssets.Count; ++ii )
		{
			if ( prUpStreamTransformAssets[ii] == asset )
			{
				prUpStreamTransformAssets[ ii ] = null;
				HAPI_Host.disconnectAssetTransform( prAssetId, ii );
				
				asset.removeDownstreamTransformAsset( this );
			}
		}
	}
	
	public int getAssetTransformConnectionIndex( HAPI_Asset asset )
	{
		for ( int ii = 0; ii < prUpStreamTransformAssets.Count; ii++ )
			if ( prUpStreamTransformAssets[ii] == asset )
				return ii;
		
		return -1;
	}
	
	public void addDownstreamTransformAsset( HAPI_Asset asset )
	{
		foreach ( HAPI_Asset downstream_asset in prDownStreamTransformAssets )
			if ( downstream_asset == asset )
				return;
		
		prDownStreamTransformAssets.Add( asset );
	}
	
	public void removeDownstreamTransformAsset( HAPI_Asset asset )
	{
		prDownStreamTransformAssets.Remove( asset );
	}
	
	// Geometry related connection methods -------------------------------------------------------
	
	public void addAssetAsGeoInput( HAPI_Asset asset, int object_index, int index )
	{
		prUpStreamGeoAssets[ index ] = asset;
		
		// TODO: It's hard coded to be the 0th group - fix this!
		HAPI_Host.connectAssetGeometry( asset.prAssetId, object_index, prAssetId, index );
		asset.addDownstreamGeoAsset( this );

		if ( HAPI_Host.prHideGeometryOnLinking && asset.prHideGeometryOnLinking )
		{
			asset.prIsGeoVisible = false;
			HAPI_PartControl[] controls = asset.GetComponentsInChildren< HAPI_PartControl >();
			foreach ( HAPI_PartControl control in controls )
			{
				if ( control.prGeoType != HAPI_GeoType.HAPI_GEOTYPE_INTERMEDIATE
					 && control.gameObject.GetComponent< MeshRenderer >() != null )
					control.gameObject.GetComponent< MeshRenderer >().enabled = false;
			}
		}
	}
	
	public void addFileAsGeoInput( string path, int index )
	{
		prUpStreamGeoAssets[ index ] = null;
		prUpStreamGeoAdded[ index ] = true;

		HAPI_Host.setFileInput( prAssetId, index, path );
	}
	
	private void marshalAnimCurve( int node_id, AnimationCurve curve, HAPI_TransformComponentType transform_component )
	{
		HAPI_Keyframe[] keys = new HAPI_Keyframe[ curve.length ];
		for( int ii = 0; ii < curve.length; ii++ )
		{
			Keyframe unity_key = curve.keys[ ii ];
			keys[ ii ].time = unity_key.time;
			keys[ ii ].value = unity_key.value;
			keys[ ii ].inTangent = unity_key.inTangent;
			keys[ ii ].outTangent = unity_key.outTangent;
			
			if( transform_component == HAPI_TransformComponentType.HAPI_TRANSFORM_TX )
			{
				keys[ ii ].value *= -1;
			}
			
			if( transform_component == HAPI_TransformComponentType.HAPI_TRANSFORM_RY ||
				transform_component == HAPI_TransformComponentType.HAPI_TRANSFORM_RZ )
			{
				keys[ ii ].value *= -1;
			}
			
		}
		
		HAPI_Host.setTransformAnimCurve( node_id, (int)transform_component, keys, curve.length );
		
	}
	
	private void marshalRotation( int node_id, AnimationClipCurveData[] curve_datas )
	{
		AnimationCurve qx = null, qy = null, qz = null, qw = null;
		
		foreach ( AnimationClipCurveData curve_data in curve_datas )
		{
			if( curve_data.propertyName == "m_LocalRotation.x" )
				qx = curve_data.curve;
			else if( curve_data.propertyName == "m_LocalRotation.y" )
				qy = curve_data.curve;
			else if( curve_data.propertyName == "m_LocalRotation.z" )
				qz = curve_data.curve;
			else if( curve_data.propertyName == "m_LocalRotation.w" )
				qw = curve_data.curve;

			if( qx != null && qy != null && qz != null && qw != null )
				break;
						
		}
		
		if( qx != null && qy != null && qz != null && qw != null )
		{
			AnimationCurve rx = new AnimationCurve();
			AnimationCurve ry = new AnimationCurve();
			AnimationCurve rz = new AnimationCurve();
			for( int ii = 0; ii < qx.length; ii++ )
			{
				Keyframe key_qx = qx.keys[ ii ];
				Keyframe key_qy = qy.keys[ ii ];
				Keyframe key_qz = qz.keys[ ii ];
				Keyframe key_qw = qw.keys[ ii ];
				
				Quaternion quat = new Quaternion( key_qx.value, key_qy.value, key_qz.value, key_qw.value );
				Vector3 eulerAngle = quat.eulerAngles;
				
				HAPI_AssetUtility.addKeyToCurve( key_qx.time, eulerAngle.x, rx );
				HAPI_AssetUtility.addKeyToCurve( key_qx.time, -eulerAngle.y, ry );
				HAPI_AssetUtility.addKeyToCurve( key_qx.time, -eulerAngle.z, rz );
								
			}
			
			marshalAnimCurve( node_id, rx, HAPI_TransformComponentType.HAPI_TRANSFORM_RX );
			marshalAnimCurve( node_id, ry, HAPI_TransformComponentType.HAPI_TRANSFORM_RY );
			marshalAnimCurve( node_id, rz, HAPI_TransformComponentType.HAPI_TRANSFORM_RZ );
			
		}
	}
	
	private void marshalCurvesFromClip( int node_id, AnimationClip clip )
	{
		AnimationClipCurveData[] curve_datas = AnimationUtility.GetAllCurves( clip );
		foreach ( AnimationClipCurveData curve_data in curve_datas )
		{
			if( curve_data.propertyName == "m_LocalPosition.x" )
				marshalAnimCurve( node_id, curve_data.curve, HAPI_TransformComponentType.HAPI_TRANSFORM_TX );
			else if( curve_data.propertyName == "m_LocalPosition.y" )
				marshalAnimCurve( node_id, curve_data.curve, HAPI_TransformComponentType.HAPI_TRANSFORM_TY );
			else if( curve_data.propertyName == "m_LocalPosition.z" )
				marshalAnimCurve( node_id, curve_data.curve, HAPI_TransformComponentType.HAPI_TRANSFORM_TZ );
			else if( curve_data.propertyName == "m_LocalScale.x" )
				marshalAnimCurve( node_id, curve_data.curve, HAPI_TransformComponentType.HAPI_TRANSFORM_SX );
			else if( curve_data.propertyName == "m_LocalScale.y" )
				marshalAnimCurve( node_id, curve_data.curve, HAPI_TransformComponentType.HAPI_TRANSFORM_SY );
			else if( curve_data.propertyName == "m_LocalScale.z" )
				marshalAnimCurve( node_id, curve_data.curve, HAPI_TransformComponentType.HAPI_TRANSFORM_SZ );
						
		}
		
		marshalRotation( node_id, curve_datas );
	}

	public void addGeoAsGeoInput( GameObject asset, int index )
	{
		prUpStreamGeoAssets[ index ] = null;
		prUpStreamGeoAdded[ index ] = true;

		int object_id;
		int geo_id;
		int node_id;
		HAPI_Host.createGeoInput( prAssetId, index, out object_id, out geo_id, out node_id );

		HAPI_GeoInputControl input_control = asset.GetComponent< HAPI_GeoInputControl >();
		input_control.prInputObjectId = object_id;

		MeshFilter asset_mesh_filter 	= asset.GetComponent< MeshFilter >();
		Mesh mesh 						= asset_mesh_filter.sharedMesh;
		HAPI_PartControl child_control  = asset.GetComponent< HAPI_PartControl >();

		// Write marshlled geo to External Input Asset ( having assetId '0' )
		Utility.setMesh( 0, object_id, geo_id, ref mesh, child_control );

		// Apply the input asset transform to the marshaled object in the Houdini scene.
		HAPI_TransformEuler trans = Utility.getHapiTransform( asset.transform.localToWorldMatrix );
		HAPI_Host.setObjectTransform( 0, object_id, trans );

		Animation anim_component = asset.GetComponent< Animation >();
		if ( anim_component == null )
			return;

		if ( anim_component.clip != null )
		{
			marshalCurvesFromClip( node_id, anim_component.clip );
		}
		else 
		{
			foreach ( AnimationState anim_state in anim_component )
			{
				AnimationClip clip = anim_component.GetClip( anim_state.name );
				if ( clip != null )
				{
					marshalCurvesFromClip( node_id, clip );
					break;
				}
			}
		}
	}

	public void removeGeoInput( int index )
	{
		try
		{
			if ( prUpStreamGeoAssets[ index ] != null )
			{
				prUpStreamGeoAssets[ index ].removeDownstreamGeoAsset( this );
				HAPI_Host.disconnectAssetGeometry( prAssetId, index );
				prUpStreamGeoAssets[ index ] = null;
				prUpStreamGeoAdded[ index ] = false;
			}
			else if ( prUpStreamGeoAdded[ index ] )
			{
				HAPI_Host.disconnectAssetGeometry( prAssetId, index );
				prUpStreamGeoAdded[ index ] = false;
			}
		}
		catch ( HAPI_Error error )
		{
			Debug.LogError( error.ToString() );
		}
	}
	
	public void removeDownstreamGeoAsset( HAPI_Asset asset )
	{
		prDownStreamGeoAssets.Remove( asset );
	}
	
	public void addDownstreamGeoAsset( HAPI_Asset asset )
	{		
		foreach ( HAPI_Asset downstream_asset in prDownStreamGeoAssets )
			if ( downstream_asset == asset )
				return;
		
		prDownStreamGeoAssets.Add( asset );
	}
	
	public virtual void OnDestroy()
	{
		if ( prAssetId >= 0 && HAPI_Host.isRealDestroy() )
		{
			foreach ( HAPI_Asset upstream_asset in prUpStreamTransformAssets )
				if ( upstream_asset != null )
					upstream_asset.removeDownstreamTransformAsset( this );
			
			List< HAPI_Asset > downstream_asset_list = new List< HAPI_Asset >();
			
			foreach ( HAPI_Asset downstream_asset in prDownStreamTransformAssets )
				downstream_asset_list.Add( downstream_asset );
			
			foreach ( HAPI_Asset downstream_asset in downstream_asset_list )
				downstream_asset.removeAssetAsTransformInput( this );
			
			prUpStreamTransformAssets.Clear();
			prDownStreamTransformAssets.Clear();
			
			try
			{
				HAPI_Host.unloadOTL( prAssetId );
			}
			catch ( HAPI_Error error )
			{
				Debug.LogError( "Asset failed to unload: " + error.ToString() );
			}

			prAssetId = -1;
		}
	}
	
	// Methods for determining where OnEnable is being called from -------------------------------
	
	public bool isRevertingPrefabInstance()
	{
		return ( isPrefabInstance() && 
				 prAssetId != prBackupAssetId &&
				 HAPI_Host.isAssetValid( prBackupAssetId, prBackupAssetValidationId ) );
	}
	
	public bool isInstantiatingPrefab()
	{
		if ( isPrefabInstance() && prBackupAssetId < 0 )
		{
			GameObject prefab = PrefabUtility.GetPrefabParent( gameObject ) as GameObject;
			HAPI_Asset prefab_asset = prefab.GetComponent<HAPI_Asset>();
			if ( prefab_asset )
			{
				return prAssetId == prefab_asset.prAssetId;
			}
		}
		return false;		 
	}
	
	public bool isApplyingChangesToPrefab()
	{
		return ( isPrefab() && 
			     prAssetId != prBackupAssetId && 
				 HAPI_Host.isAssetValid( prAssetId, prAssetValidationId ) );
	}

	public virtual void OnEnable()
	{
		// If this asset is a prefab instance that is being reverted 
		// reload the asset in order to restore it's asset id and 
		// asset validation id from the backup and to load the preset
		// from the prefab
		if( isRevertingPrefabInstance() )
		{
			build( true,	// reload_asset
			   	   false,	// unload_asset_first
			   	   false,	// serializatin_recovery_only
			   	   true,	// force_reconnect
			   	   false,	// cook_downstream_assets
			   	   false	// use_delay_for_progress_bar
			 );
		}
		else if ( prAssetId >= 0 || isInstantiatingPrefab() )
		{	
			if ( !isInstantiatingPrefab() &&
				 HAPI_Host.isAssetValid( prAssetId, prAssetValidationId ) )
			{
				// Reloading asset after mode change or script-reload.
				build(	false,	// reload_asset
						false,	// unload_asset_first
						true,	// serializatin_recovery_only
						false,	// force_reconnect
						true,	// cook_downstream_assets
						true	// use_delay_for_progress_bar
					);
			}
			else
			{
				// Loading Scene (no Houdini scene exists yet)
				prAssetId = -1;
				build(	true,	// reload_asset
						true,	// unload_asset_first
						false,	// serializatin_recovery_only
						true,	// force_reconnect
						true,	// cook_downstream_assets
						false	// use_delay_for_progress_bar
					);
			}
		}
	}
	
	public override void reset()
	{
		base.reset();

		// Please keep these in the same order and grouping as their declarations at the top.
		
		// Assets ---------------------------------------------------------------------------------------------------
		
		prAsset							= this;
		prAssetInfo 					= new HAPI_AssetInfo();
		prPreset 						= null;
		prAssetValidationId				= -1;
		prAssetName						= "ASSET_NAME";
		prAssetHelp						= "ASSET_HELP";
		prAssetType						= AssetType.TYPE_INVALID;
		prHAPIAssetType 				= HAPI_AssetType.HAPI_ASSETTYPE_INVALID;
		prAssetSubType 					= 0;

		// Parameters -----------------------------------------------------------------------------------------------

		prPresetsMap					= null;

		// Inputs ---------------------------------------------------------------------------------------------------
		
		prMinTransInputCount 			= 0;
		prMaxTransInputCount 			= 0;
		prMinGeoInputCount 				= 0;
		prMaxGeoInputCount 				= 0;
		prFileInputs 					= new List< string >();
		
		prDownStreamTransformAssets		= new List< HAPI_Asset >();
		prUpStreamTransformAssets 		= new List< HAPI_Asset >();
		prUpStreamTransformObjects 		= new List< GameObject >();
		
		prDownStreamGeoAssets 			= new List< HAPI_Asset >();
		prUpStreamGeoAssets 			= new List< HAPI_Asset >();
		prUpStreamGeoObjects 			= new List< GameObject >();
		prUpStreamGeoAdded 				= new List< bool >();

		// Objects --------------------------------------------------------------------------------------------------
		
		prObjectCount 					= 0;
		prHandleCount 					= 0;
		
		prObjects 						= new HAPI_ObjectInfo[ 0 ];
		
		prGameObjects 					= new GameObject[ 0 ];
		prObjectTransforms 				= new HAPI_Transform[ 0 ];
		
		// Baking ---------------------------------------------------------------------------------------------------
		
		prBakeStartTime					= 0.0f;
		prBakeEndTime					= 1.0f;
		prBakeSamplesPerSecond			= 30;
		
		// GUI ------------------------------------------------------------------------------------------------------
		
		prShowHoudiniControls 			= true;
		prShowHelp						= false;
		prShowAssetSettings				= true;
		prShowBakeOptions				= false;
		prShowInputControls 			= true;
		prAssetSettingsCategory			= 0;

		prIsGeoVisible					= true;
		prShowPinnedInstances			= true;
		prAutoSelectAssetRootNode 		= HAPI_Host.myDefaultAutoSelectAssetRootNode;
		prHideGeometryOnLinking			= HAPI_Host.myDefaultHideGeometryOnLinking;

		prMaterialShaderType			= HAPI_ShaderType.HAPI_SHADER_OPENGL;
		prRenderResolution				= new Vector2( 1000, 1000 );
		prShowOnlyVertexColours			= false;
		prGenerateTangents				= true;

		prEnableCooking					= HAPI_Host.myDefaultEnableCooking;
		prCookingTriggersDownCooks		= HAPI_Host.myDefaultCookingTriggersDownCooks;
		prPlaymodePerFrameCooking		= false;
		prPushUnityTransformToHoudini	= HAPI_Host.myDefaultPushUnityTransformToHoudini;
		prTransformChangeTriggersCooks	= HAPI_Host.myDefaultTransformChangeTriggersCooks;

		prEnableLogging					= false;

		prTransInputNames				= new List< string >();
		prGeoInputNames					= new List< string >();
		prGeoInputFormats				= new List< HAPI_GeoInputFormat >();
		prOverriddenInstances 			= new List< HAPI_InstancerOverrideInfo >();
		
		myProgressBarJustUsed 			= false;
		
		// Prefabs ------------------------------------------------------------------------------------------------------
		
		prBackupAssetId					= -1;
		prBackupAssetValidationId		= -1;
		prReloadPrefabOnPlaymodeChange 	= false;
		myCleanUpPrefabOTL 				= false;
	}
	
	public void propagateParmChangeToPrefabInstances()
	{
		if ( isPrefab() )
		{
			foreach ( GameObject obj in GameObject.FindObjectsOfType( typeof( GameObject ) ) )
			{
				HAPI_Asset asset = obj.GetComponent< HAPI_Asset >();
				GameObject prefab_parent = PrefabUtility.GetPrefabParent( obj ) as GameObject;
				if ( asset && 
					 asset.isPrefabInstance() && 
					 PrefabUtility.Equals( prefab_parent, gameObject ) )
				{
					// loop through parameters on prefab and only change the ones on the
					// asset that have not been overridden
					HAPI_Parms asset_parms = asset.prParms;
					for ( int ii = 0; ii < asset_parms.prParmCount; ii++ )
					{
						HAPI_ParmInfo asset_parm_info = asset_parms.prParms[ ii ];
						
						// do not propogate transform changes
						if( asset_parm_info.name == "r" || 
							asset_parm_info.name == "s" ||
							asset_parm_info.name == "t" )
						{
							continue;
						}
						
						// if this parameter has not been overridden and it's 
						// value differs from the value of this parameter on 
						// the prefab the change the value on the asset to
						// match the value of the prefab
						if ( !asset_parms.prOverriddenParms[ ii ] && 
							 !asset_parms.isParmSameInPrefab( asset_parm_info.id, prParms ) )
						{
							HAPI_ParmInfo parm_info = prParms.findParm( asset_parm_info.id );
							
							if ( asset_parm_info.isFloat() )
							{
								for ( int jj = 0; jj < asset_parm_info.size; jj++ )
								{
									int index = parm_info.floatValuesIndex + jj;
									int asset_index = asset_parm_info.floatValuesIndex + jj;
									asset_parms.prParmFloatValues[ asset_index ] = prParms.prParmFloatValues[ index ];
								}
							}
							else if ( asset_parm_info.isInt() )
							{
								for ( int jj = 0; jj < asset_parm_info.size; jj++ )
								{
									int index = parm_info.intValuesIndex + jj;
									int asset_index = asset_parm_info.intValuesIndex + jj;
									asset_parms.prParmIntValues[ asset_index ] = prParms.prParmIntValues[ index ];
								}
							}
							else if ( asset_parm_info.isString() )
							{
								string[] values = prParms.getParmStrings( parm_info );
								asset_parms.setParmStrings( asset_parm_info, values );
							}
							asset_parms.setChangedParameterIntoHost( asset_parm_info.id );
							asset.buildClientSide();
						}
					}
				}
			}
		}
		
	}
	
	public override void onParmChange( bool reload_asset )
	{
		base.onParmChange( reload_asset );

		if ( isPrefab() )
		{
			HAPI_ProgressBar progress_bar = new HAPI_ProgressBar();
			try 
			{
				// only need to update parameters for prefab
				updateParameters( progress_bar );
			}
			catch {}
			finally 
			{
				progress_bar.clearProgressBar();
			}
		}
		else
		{
			build(	
				reload_asset,	// reload_asset
				true,			// unload_asset_first
				false,			// serializatin_recovery_only
				false,			// force_reconnect
				prCookingTriggersDownCooks,
				true			// use_delay_for_progress_bar
			);
		}

		// To keep things consistent with Unity workflow, we should not save parameter changes
		// while in Play mode.
		if ( !EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode )
			savePreset();

		// If asset is a prefab then propagate parameter change to all prefab instances
		if ( isPrefab() )
			propagateParmChangeToPrefabInstances();
	}

	public virtual bool buildAll()
	{
		return build(	true,	// reload_asset
						true,	// unload_asset_first
						false,	// serializatin_recovery_only
						true,	// force_reconnect
						prCookingTriggersDownCooks,	// cook_downstream_assets
						true	// use_delay_for_progress_bar
					);
	}

	public virtual bool buildClientSide()
	{
		return build(	false,	// reload_asset
						false,	// unload_asset_first
						false,	// serializatin_recovery_only
						false,	// force_reconnect
						prCookingTriggersDownCooks,	// cook_downstream_assets
						true	// use_delay_for_progress_bar
					);
	}

	public virtual bool build( bool reload_asset, bool unload_asset_first,
							   bool serialization_recovery_only,
							   bool force_reconnect,
							   bool cook_downstream_assets,
							   bool use_delay_for_progress_bar )
	{
		if ( !HAPI.HAPI_SetPath.prIsPathSet )
		{
			Debug.LogError( "Cannot build asset as Houdini dlls not found!" );
			return false;
		}

		if ( !prEnableCooking )
			return false;
		
		if ( isPrefab() )
			myCleanUpPrefabOTL = true;

		HAPI_ProgressBar progress_bar	= new HAPI_ProgressBar();
		progress_bar.prUseDelay			= use_delay_for_progress_bar;
		progress_bar.prAsset			= this;

		try
		{
			progress_bar.prStartTime = System.DateTime.Now;
			
			if ( reload_asset ) 
			{
				bool is_reverting_prefab_instance = isRevertingPrefabInstance();
				if ( is_reverting_prefab_instance )
				{
					// restore asset id and asset validation id from the backup
					prAssetId = prBackupAssetId;
					prAssetValidationId = prBackupAssetValidationId;
				}
				
				if ( unload_asset_first )
				{
					// There's no reason to abort the whole rebuild process because we can't unload
					// the asset first as that would leave the user with no options other than
					// to delete this HAPI asset and create a new one for this OTL.
					try
					{
						HAPI_Host.unloadOTL( prAssetId );
					}
					catch ( HAPI_Error ) {}

					// Once an asset is unloaded its id will is obviously no longer valid, so reset it here.
					prAssetId = -1;
					
					// Need to reset the parms as well.
					prParms.reset();
				}
				
				bool is_first_time_build = false;

				try
				{
					int asset_id = 0;

					if ( prAssetId < 0 )
						is_first_time_build = true;

					if ( unload_asset_first )
						asset_id = buildCreateAsset();
					else
						asset_id = prAssetId;

					// We need to update the prAssetId in case the cook is aborted/fails 
					// and we need to clean up (unload the asset) in the catch.
					prAssetId = asset_id;

					progress_bar.statusCheckLoop();

					prAssetInfo = HAPI_Host.getAssetInfo( asset_id );
					
					if ( reload_asset )
						Debug.Log( 
							"Asset Loaded - Path: " + prAssetInfo.instancePath + "\n" +
							"ID: " + prAssetInfo.id + "\n" +
							"Version: " + prAssetInfo.version + "\n" + 
							"Full Name: " + prAssetInfo.fullOpName + "\n" +
							"Fuller Name: " + prAssetInfo.definitionSource );
				}
				catch ( HAPI_Error error )
				{
					Debug.LogError( "Asset not loaded: " + error.ToString() );
					// Nothing to build since the load failed.

					// Try to unload the asset so it doesn't dangle.
					if ( is_first_time_build )
					{
						try
						{
							HAPI_Host.unloadOTL( prAssetId );
						}
						catch ( HAPI_Error ) {}
					}
					
					// Clean up.
					reset();

					// If in play mode, disable live cooks.
					if ( EditorApplication.isPlaying )
						prPlaymodePerFrameCooking = false;
					
					return false; // false for failed :(
				}
				
				// For convenience we copy some asset info properties locally (since they are constant anyway).
				// More imporantly, structs are not serialized and therefore putting them into their own
				// variables is required in order to maintain state between serialization cycles.
				prAssetId 					= prAssetInfo.id;
				prBackupAssetId				= prAssetId;
				prAssetValidationId			= prAssetInfo.validationId;
				prBackupAssetValidationId 	= prAssetValidationId;
				prNodeId					= prAssetInfo.nodeId;
				prObjectCount 				= prAssetInfo.objectCount;
				prHandleCount 				= prAssetInfo.handleCount;

				prAssetName					= prAssetInfo.name;
				prAssetHelp					= prAssetInfo.helpText;
				prHAPIAssetType				= (HAPI_AssetType) prAssetInfo.type;
				prMinTransInputCount		= prAssetInfo.minTransInputCount;
				prMaxTransInputCount		= prAssetInfo.maxTransInputCount;
				prMinGeoInputCount 			= prAssetInfo.minGeoInputCount;
				prMaxGeoInputCount			= prAssetInfo.maxGeoInputCount;

				// Try to load presets.
				if ( unload_asset_first || is_reverting_prefab_instance )
				{
					loadPreset();
					
					// Transform may not have been saved as part of the presets so we have to rely 
					// on the serialized value.
					if ( myLastLocalToWorld != Matrix4x4.zero && !isPrefab() )
					{
						// If this is a prefab instance being reverted we don't want to use the 
						// serialized value so don't change transform. 
						if ( !is_reverting_prefab_instance )
						{
							transform.localPosition = Utility.getPosition( myLastLocalToWorld );
							transform.localRotation = Utility.getQuaternion( myLastLocalToWorld );
	
							Vector3 scale = Utility.getScale( myLastLocalToWorld );
							if ( !( Mathf.Approximately( 0.0f, scale.x )
								&& Mathf.Approximately( 0.0f, scale.y )
								&& Mathf.Approximately( 0.0f, scale.z ) ) )
							{
								transform.localScale = Utility.getScale( myLastLocalToWorld );
							}
						}
						
						if ( prPushUnityTransformToHoudini )
						{
							pushAssetTransformToHoudini();
						}
					}
				}
				
				progress_bar.prCurrentValue			= 0;
				progress_bar.prTotal				= prObjectCount + prHandleCount;
				
				progress_bar.displayProgressBar();
				myProgressBarJustUsed = true;

				// Add input fields.
				if ( is_first_time_build || !force_reconnect )
					initAssetConnections();
				
				// Clean up.
				destroyChildren( transform );
				prGameObjects = new GameObject[ prObjectCount ];
			}
				
			if ( reload_asset || serialization_recovery_only )
			{
				// Need to re-acquire all the params for all the child controls that have parms exposed.
				prParms.getParameterValues();
				foreach ( HAPI_Parms parms in GetComponentsInChildren< HAPI_Parms >() )
					parms.getParameterValues();

				// Create local object info caches (transforms need to be stored in a parallel array).
				prObjects = new HAPI_ObjectInfo[ prObjectCount ];
				prObjectTransforms = new HAPI_Transform[ prObjectCount ];

				// Custom work during a full build (custom to each subclass).
				buildFullBuildCustomWork( ref progress_bar );
			}
			
			if ( !reload_asset && !serialization_recovery_only )
			{
				progress_bar.displayProgressBar();
				updateParameters( progress_bar );
			}
			
			// Refresh object info arrays as they are lost after serialization.
			Utility.getArray1Id( prAssetId, HAPI_Host.getObjects, prObjects, prObjectCount );
			Utility.getArray2Id( prAssetId, (int) HAPI_RSTOrder.HAPI_SRT, HAPI_Host.getObjectTransforms, 
					 			 prObjectTransforms, prObjectCount );
			
			if ( !serialization_recovery_only )
			{
				// Set asset's transform.
				if ( prPushUnityTransformToHoudini )
					Utility.getHoudiniTransformAndApply( prAssetId, prAssetName, transform );
			
				progress_bar.prMessage = "Loading and composing objects...";
			
				// Custom way to load objects (custom to each subclass).
				buildCreateObjects( reload_asset, ref progress_bar );
			
				// Process dependent assets.
				if ( cook_downstream_assets )
					processDependentAssets( serialization_recovery_only, force_reconnect, 
											use_delay_for_progress_bar );
			}

			// A bit of a hack (but not terrible). If we have presets for other child controls
			// they set their presets by now so we need to rebuild with the new presets.
			if ( reload_asset && !prPresetsMap.isEmpty() )
				build( false, false, false, false, false, true );
		}
		catch ( HAPI_ErrorIgnorable ) {}
		catch ( HAPI_ErrorProgressCancelled error )
		{
			// If in play mode, disable live cooks.
			if ( EditorApplication.isPlaying )
				prPlaymodePerFrameCooking = false;

			Debug.LogError( error.ToString() + "\nSource: " + error.Source );
		}
		catch ( HAPI_Error error )
		{
			Debug.LogError( error.ToString() + "\nSource: " + error.Source );
		}
		catch ( System.Exception error )
		{
			Debug.LogError( error.ToString() + "\nSource: " + error.Source );
		}
		finally
		{
			progress_bar.clearProgressBar();

			myProgressBarJustUsed = false;
		}
		
		return true;
	}
	
	public void updateParameters( HAPI_ProgressBar progress_bar )
	{
		prParms.setChangedParametersIntoHost();

		HAPI_Host.cookAsset( prAssetId );
		progress_bar.statusCheckLoop();

		myProgressBarJustUsed = true;
		
		progress_bar.prTotal = prObjectCount;

		prParms.getParameterValues();
	}

	public virtual void Update()
	{
		if ( !prPushUnityTransformToHoudini || prAssetId < 0 || EditorApplication.isPlayingOrWillChangePlaymode )
			return;

		try
		{
			Matrix4x4 local_to_world = transform.localToWorldMatrix;
			if ( local_to_world == myLastLocalToWorld )
				return;
			myLastLocalToWorld = local_to_world;

			pushAssetTransformToHoudini();
			savePreset();

			// Process dependent assets.
			// TODO: These steps here might be too slow for some assets and can grind Unity to
			// a halt. But if we are to support all the different effects of transform changes
			// then we do need to do a full build so I'm not sure how to do this more proper.
			// Do note that the build function is fairly conditional and should only build
			// the bare minimum.
			if ( prTransformChangeTriggersCooks )
			{
				foreach ( HAPI_Asset downstream_asset in prDownStreamTransformAssets )
					downstream_asset.buildClientSide();
			
				foreach ( HAPI_Asset downstream_asset in prDownStreamGeoAssets )
					downstream_asset.buildClientSide();
			}
		}
		catch ( HAPI_Error err )
		{
			Debug.LogError( err.ToString() );
		}
	}		
	
	public void bakeAnimations( float start_time, 
								float end_time, 
								int samples_per_second,
								GameObject parent_object,
								HAPI_ProgressBar progress_bar )
	{
		try
		{
			//This build is needed in case the user changed a geometry input to the asset.  
			//The geometry to be instanced needs to be re-brought in.
			buildClientSide();
			
			int num_objects = prObjects.Length;
			for ( int ii = 0; ii < num_objects; ii++ )
			{
				GameObject game_object = prGameObjects[ ii ];
				HAPI_ObjectInfo obj_info = prObjects[ ii ];
				
				if ( game_object != null )
				{
					if ( !obj_info.isInstancer )
					{
						HAPI_ObjectControl obj_control = game_object.GetComponent< HAPI_ObjectControl >();
						obj_control.beginBakeAnimation();
					}
					else
					{
						HAPI_Instancer instancer = game_object.GetComponent< HAPI_Instancer >();
						instancer.beginBakeAnimation();
					}
				}
			}
			
			int num_samples = Mathf.CeilToInt(samples_per_second*( end_time - start_time ));
				
			float total_sim_time = ( end_time - start_time );
			float delta_time = total_sim_time / (float) num_samples;
			
			progress_bar.prTotal = num_samples;
			progress_bar.prCurrentValue = 0;
			for ( float curr_time = start_time; curr_time <= end_time; curr_time += delta_time )
			{
				HAPI_Host.setTime( curr_time );
				
				HAPI_Host.cookAsset( prAssetId );
				
				HAPI_State state = HAPI_State.HAPI_STATE_STARTING_LOAD;
					
				while ( state != HAPI_State.HAPI_STATE_READY && state != HAPI_State.HAPI_STATE_READY_WITH_ERRORS )
				{
					state = (HAPI_State) HAPI_Host.getStatus( HAPI_StatusType.HAPI_STATUS_STATE );
				}

				if ( state == HAPI_State.HAPI_STATE_READY_WITH_ERRORS )
				{
					state = HAPI_State.HAPI_STATE_READY;
					HAPI_Host.throwRuntimeError();
				}
				
				HAPI_Transform[] object_transforms = new HAPI_Transform[ prObjectCount ];
				Utility.getArray2Id( prAssetId, (int) HAPI_RSTOrder.HAPI_SRT, HAPI_Host.getObjectTransforms, 
						 			 object_transforms, prObjectCount );
				
				for ( int ii = 0; ii < num_objects; ii++ )
				{
					GameObject game_object = prGameObjects[ ii ];
					HAPI_ObjectInfo obj_info = prObjects[ ii ];
					
					if ( game_object != null )
					{
						if ( !obj_info.isInstancer )
						{
							HAPI_ObjectControl obj_control = game_object.GetComponent< HAPI_ObjectControl >();
							obj_control.bakeAnimation( curr_time, parent_object, object_transforms[ ii ] );
						}
						else
						{
							HAPI_Instancer instancer = game_object.GetComponent< HAPI_Instancer >();
							instancer.bakeAnimation( curr_time, parent_object );
						}
					}
				}			
				
				// Set progress bar information.
				progress_bar.prCurrentValue++;
				progress_bar.prMessage = "Baking: " + progress_bar.prCurrentValue + " of " + num_samples;
				progress_bar.displayProgressBar();
			}
						
			bool found_anim = false;
			for ( int ii = 0; ii < num_objects; ii++ )
			{
				GameObject game_object = prGameObjects[ ii ];
				HAPI_ObjectInfo obj_info = prObjects[ ii ];
				
				if ( game_object != null )
				{
					if ( !obj_info.isInstancer )
					{
						HAPI_ObjectControl obj_control = game_object.GetComponent< HAPI_ObjectControl >();
						if ( obj_control.endBakeAnimation() )
							found_anim = true;
					}
					else
					{
						HAPI_Instancer instancer = game_object.GetComponent< HAPI_Instancer >();
						if ( instancer.endBakeAnimation( parent_object ) )
							found_anim = true;
					}
				}
			}
			
			if ( !found_anim )
			{
				EditorUtility.DisplayDialog( "Bake Error", "No animation was found to bake", "OK" );
			}
		}
		catch ( HAPI_Error error )
		{
			Debug.LogWarning( error.ToString() );
		}
		
	}

	public void loadPreset()
	{
		try
		{
			if ( myPreset != null && myPreset.Length > 0 )
			{
				HAPI_Host.setPreset( prNodeId, myPreset );
				HAPI_Host.cookAsset( prAssetId );
			}
		}
		catch ( HAPI_Error error )
		{
			Debug.LogWarning( error.ToString() );
		}
		catch
		{
			Debug.LogWarning( "Unable to load presets." );
		}
	}

	public void savePreset()
	{
		try
		{
			myPreset = HAPI_Host.getPreset( prNodeId );
		}
		catch {} // Just catch them here but don't report them because we would just get a huge stream of errors.
	}

	public bool isAssetValid()
	{
		return HAPI_Host.isAssetValid( prAssetId, prAssetInfo.validationId );
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Protected Methods

	// Build Custom Work Methods -----------------------------------------------------------------------------------
	
	// Inherited classes should override this with their specific call to the HAPI_Host asset create method.
	// For example: OTLs need to call HAPI_Host.loadOTL( path ), curves need to call HAPI_Host.createCurve().
	protected abstract int buildCreateAsset();

	// Inherited classes should override this for work they need done during the full build step only. (Optional)
	protected virtual void buildFullBuildCustomWork( ref HAPI_ProgressBar progress_bar ) {}

	// Inherited classes should override this with however they wish to load objects in the prObjects array.
	protected abstract void buildCreateObjects( bool reload_asset, ref HAPI_ProgressBar progress_bar );

	// -------------------------------------------------------------------------------------------------------------

	protected void pushAssetTransformToHoudini()
	{
		Matrix4x4 local_to_world = transform.localToWorldMatrix;

		HAPI_TransformEuler hapi_transform = Utility.getHapiTransform( local_to_world );
		HAPI_Host.setAssetTransform( prAssetId, ref hapi_transform );

		int parm = -1;
		float [] parm_data = new float[ 3 ];

		parm = prParms.findParm( "t" );
		if ( parm > 0 )
		{
			HAPI_Host.getParmFloatValues( prNodeId, parm_data, prParms.findParm( parm ).floatValuesIndex, 3 );
			for ( int i = 0; i < 3; ++i )
				prParms.prParmFloatValues[ prParms.findParm( parm ).floatValuesIndex + i ] = parm_data[ i ];
		}

		parm = prParms.findParm( "r" );
		if ( parm > 0 )
		{
			HAPI_Host.getParmFloatValues( prNodeId, parm_data, prParms.findParm( parm ).floatValuesIndex, 3 );
			for ( int i = 0; i < 3; ++i )
				prParms.prParmFloatValues[ prParms.findParm( parm ).floatValuesIndex + i ] = parm_data[ i ];
		}

		parm = prParms.findParm( "s" );
		if ( parm > 0 )
		{
			HAPI_Host.getParmFloatValues( prNodeId, parm_data, prParms.findParm( parm ).floatValuesIndex, 3 );
			for ( int i = 0; i < 3; ++i )
				prParms.prParmFloatValues[ prParms.findParm( parm ).floatValuesIndex + i ] = parm_data[ i ];
		}
	}
	
	protected void initAssetConnections()
	{
		if ( prHAPIAssetType == HAPI_AssetType.HAPI_ASSETTYPE_OBJ )
		{
			if ( prMaxTransInputCount > 0 && prUpStreamTransformAssets.Count <= 0 )
				for ( int ii = 0; ii < prMaxTransInputCount ; ++ii )
				{
					prUpStreamTransformAssets.Add( null );
					prUpStreamTransformObjects.Add( null );
				}
		}
	
		if ( prMaxGeoInputCount > 0 && prFileInputs.Count <= 0 )
			for ( int ii = 0; ii < prMaxGeoInputCount ; ++ii )
			{
				prFileInputs.Add( "" );
				prUpStreamGeoAssets.Add( null );
				prUpStreamGeoObjects.Add( null );
				prUpStreamGeoAdded.Add( false );
			}
	
		// Check for min input fields set.
		if ( prHAPIAssetType == HAPI_AssetType.HAPI_ASSETTYPE_OBJ )
		{
			int numValidTransformInputs = 0;
			for ( int ii = 0; ii < prMaxTransInputCount ; ++ii )
				if ( prUpStreamTransformAssets[ ii ] )
					numValidTransformInputs++;
		
			if ( numValidTransformInputs < prMinTransInputCount )
				Debug.LogWarning( "Insufficent Transform Inputs to Asset. " +
								  "Please provide inputs in the Inputs section." );
		}
	
		int numValidGeoInputs = 0;
		for ( int ii = 0; ii < prMaxGeoInputCount ; ++ii )
			if ( prFileInputs[ ii ] != "" )
				numValidGeoInputs++;
	
		if ( numValidGeoInputs < prMinGeoInputCount )
			Debug.LogWarning( "Insufficent Geo Inputs to Asset. " +
							  "Please provide inputs in the Inputs section." );
	
		if ( prHAPIAssetType == HAPI_AssetType.HAPI_ASSETTYPE_OBJ )
			for ( int ii = 0; ii < prMaxTransInputCount ; ++ii )
				if ( prUpStreamTransformAssets[ ii ] )
					HAPI_Host.connectAssetTransform( prUpStreamTransformAssets[ ii ].prAssetId, prAssetId, ii );

		foreach ( HAPI_Asset downstream_asset in prDownStreamTransformAssets )
		{
			int index = downstream_asset.getAssetTransformConnectionIndex( this );
			if ( index >= 0 )
				HAPI_Host.connectAssetTransform( prAssetId, downstream_asset.prAssetId, index );
		}
		
		// Fill input names.
		for ( int i = 0; i < prMaxTransInputCount; ++i )
		{
			string trans_input_name = HAPI_Host.getInputName( prAssetId, i, 
															  HAPI_InputType.HAPI_INPUT_TRANSFORM );
			if ( trans_input_name == "" )
				trans_input_name = "Transform Input #" + ( i + 1 );
			prTransInputNames.Add( trans_input_name );
		}
		for ( int i = 0; i < prMaxGeoInputCount; ++i )
		{
			string geo_input_name = HAPI_Host.getInputName( prAssetId, i, 
															HAPI_InputType.HAPI_INPUT_GEOMETRY );
			if ( geo_input_name == "" )
				geo_input_name = "Geometry Input #" + ( i + 1 );
			prGeoInputNames.Add( geo_input_name );
			prGeoInputFormats.Add( HAPI_GeoInputFormat.HAPI_GEO_INPUT_FORMAT_DEFAULT );
		}
	}

	protected virtual void processDependentAssets( bool serialization_recovery_only, bool force_reconnect, 
												   bool use_delay_for_progress_bar )
	{
		bool has_built_something_upstream = false;
		
		if ( !serialization_recovery_only && !force_reconnect )
		{
			foreach ( HAPI_Asset downstream_asset in prDownStreamTransformAssets )
			{
				prEnableCooking = false;
				if ( !downstream_asset.isAssetValid() )
					downstream_asset.OnEnable();
				downstream_asset.build( false, // reload_asset
										false, // unload_asset_first
										false, // serialization_recovery_only
										false, // force_reconnect
										downstream_asset.prCookingTriggersDownCooks,
										use_delay_for_progress_bar );
				prEnableCooking = true;
			}
			
			foreach ( HAPI_Asset downstream_asset in prDownStreamGeoAssets )
			{
				prEnableCooking = false;
				if ( !downstream_asset.isAssetValid() )
					downstream_asset.OnEnable();
				downstream_asset.build( false, // reload_asset
										false, // unload_asset_first
										false, // serialization_recovery_only
										false, // force_reconnect
										downstream_asset.prCookingTriggersDownCooks,
										use_delay_for_progress_bar );
				prEnableCooking = true;
			}
		}
		else if ( force_reconnect )
		{
			for ( int i = 0; i < prUpStreamTransformObjects.Count; ++i )
			{
				if ( prUpStreamTransformObjects[ i ] != null )
				{
					GameObject game_obj = prUpStreamTransformObjects[ i ];

					HAPI_Asset asset = game_obj.GetComponent< HAPI_Asset >();
					if ( asset )
					{
						if ( !asset.isAssetValid() )
						{
							// No need to cache because since we're in here it means prEnableCooking == true.
							prEnableCooking = false;
							asset.OnEnable();
							prEnableCooking = true;
							has_built_something_upstream = true;
						}
						addAssetAsTransformInput( asset, i );
					}
				}
			}
			
			for ( int i = 0; i < prUpStreamGeoObjects.Count; ++i )
			{
				if ( prUpStreamGeoObjects[ i ] != null )
				{
					GameObject new_obj = prUpStreamGeoObjects[ i ];

					HAPI_Asset asset = null;
					HAPI_PartControl part_control = new_obj.GetComponent< HAPI_PartControl >();
							
					int object_index = 0;
					if ( part_control )
					{
						object_index = part_control.prObjectId;
						asset = part_control.prAsset;
					}
					else
						asset = new_obj.GetComponent< HAPI_Asset >();
							
					if ( asset )
					{
						if ( !asset.isAssetValid() )
						{
							// No need to cache because since we're in here it means prEnableCooking == true.
							prEnableCooking = false;
							asset.OnEnable();
							prEnableCooking = true;
							has_built_something_upstream = true;
						}
						addAssetAsGeoInput( asset, object_index, i );
					}
					else
						addGeoAsGeoInput( new_obj, i );
				}
			}

			// Need to rebuild because now we're connected to other assets.
			if ( has_built_something_upstream )
				build(	false, // reload_asset
						false, // unload_asset_first
						false, // serialization_recovery_only
						false, // force_reconnect
						true,  // cook_downstream_assets
						use_delay_for_progress_bar );
		}
	}
	
	// PROGRESS BAR -------------------------------------------------------------------------------------------------
	
	public bool hasProgressBarBeenUsed()
	{
		return myProgressBarJustUsed;
	}
	
	protected bool				myProgressBarJustUsed;
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Serialized Private Data
	
	// Assets -------------------------------------------------------------------------------------------------------
	
	[SerializeField] private HAPI_AssetInfo			myAssetInfo;
	[SerializeField] private byte[]					myPreset;
	[SerializeField] private int					myAssetValidationId;
	[SerializeField] private string					myAssetName;
	[SerializeField] private string					myAssetHelp;
	[SerializeField] private AssetType				myAssetType;
	[SerializeField] private HAPI_AssetType			myHAPIAssetType;
	[SerializeField] private HAPI_AssetSubType		myAssetSubType;
	
	// Inputs -------------------------------------------------------------------------------------------------------
	
	[SerializeField] private int 					myMinTransInputCount;
	[SerializeField] private int 					myMaxTransInputCount;
	[SerializeField] private int 					myMinGeoInputCount;
	[SerializeField] private int					myMaxGeoInputCount;
	[SerializeField] private List< string >			myFileInputs;
	
	[SerializeField] private List< HAPI_Asset >		myDownStreamTransformAssets;
	[SerializeField] private List< HAPI_Asset >		myUpStreamTransformAssets;
	[SerializeField] private List< GameObject >		myUpStreamTransformObjects;
	
	[SerializeField] private List< HAPI_Asset >		myDownStreamGeoAssets;
	[SerializeField] private List< HAPI_Asset >		myUpStreamGeoAssets;
	[SerializeField] private List< GameObject >		myUpStreamGeoObjects;
	[SerializeField] private List< bool >			myUpStreamGeoAdded;

	// Parameters ---------------------------------------------------------------------------------------------------

	[SerializeField] private HAPI_PresetMap			myPresetsMap;

	// Objects ------------------------------------------------------------------------------------------------------
	
	[SerializeField] private int 					myObjectCount;
	[SerializeField] private int					myHandleCount;
	
	[SerializeField] private GameObject[]			myGameObjects;

					 private HAPI_ObjectInfo[] 		myObjects;
					 private HAPI_Transform[] 		myObjectTransforms;
	
	// Baking -------------------------------------------------------------------------------------------------------
	
	[SerializeField] private float 					myBakeStartTime;
	[SerializeField] private float					myBakeEndTime;
	[SerializeField] private int 					myBakeSamplesPerSecond;

	// GUI ----------------------------------------------------------------------------------------------------------
	
	[SerializeField] private bool 					myShowHoudiniControls;
	[SerializeField] private bool					myShowHelp;
	[SerializeField] private bool 					myShowAssetSettings;
	[SerializeField] private bool 					myShowBakeOptions;
	[SerializeField] private bool					myShowInputControls;
	[SerializeField] private int					myAssetOptionsCategory;

	[SerializeField] private bool					myIsGeoVisible;
	[SerializeField] private bool					myShowPinnedInstances;
	[SerializeField] private bool					myAutoSelectAssetRootNode;
	[SerializeField] private bool					myHideGeometryOnLinking;

	[SerializeField] private HAPI_ShaderType		myMaterialShaderType;
	[SerializeField] private Vector2				myRenderResolution;
	[SerializeField] private bool					myShowOnlyVertexColours;
	[SerializeField] private bool					myGenerateTangents;

	[SerializeField] private bool					myEnableCooking;
	[SerializeField] private bool					myCookingTriggersDownCooks;
	[SerializeField] private bool					myPlaymodePerFrameCooking;
	[SerializeField] private bool					myPushUnityTransformToHoudini;
	[SerializeField] private bool					myTransformChangeTriggersCooks;
	
	[SerializeField] private bool					myEnableLogging;

	[SerializeField] private List< string >			myTransInputNames;
	[SerializeField] private List< string >			myGeoInputNames;
	[SerializeField] 
	private List< HAPI_GeoInputFormat >				myGeoInputFormats;
	
	
	[SerializeField] private List< HAPI_InstancerOverrideInfo >	myOverriddenInstances;
	
	// Private Temporary Data
	[SerializeField] private Matrix4x4				myLastLocalToWorld;
	
	// Prefabs ------------------------------------------------------------------------------------------------------
	
	private int myBackupAssetId;
	private int myBackupAssetValidationId;
	private bool myReloadPrefabOnPlaymodeChange;
	private bool myCleanUpPrefabOTL;
}
