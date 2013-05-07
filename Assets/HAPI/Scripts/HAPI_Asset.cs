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
																	protected set { myAssetValidationId = value; } }
 	public string					prAssetName {					get { return myAssetName; }
																	set { myAssetName = value; } }
	public AssetType				prAssetType {					get { return myAssetType; } 
																	set { myAssetType = value; } }
	public HAPI_AssetType			prHAPIAssetType {				get { return myHAPIAssetType; } 
																	set { myHAPIAssetType = value; } }
	public HAPI_AssetSubType		prAssetSubType {				get { return myAssetSubType; } 
																	set { myAssetSubType = value; } }
	public bool						prFullBuild {					get { return myFullBuild; } 
																	set { myFullBuild = value; } }
	public bool						prPartialBuild {				get { return myPartialBuild; }
																	set { myPartialBuild = value; } }
	public bool						prForceReconnectInFullBuild {	get { return myForceReconnectInFullBuild; }
																	set { myForceReconnectInFullBuild = value; } }
	public bool 					prReloadAssetInFullBuild {		get { return myReloadAssetInFullBuild; } 
																	set { myReloadAssetInFullBuild = value; } }
	public bool						prUseDelayForProgressBar {		get { return myUseDelayForProgressBar; }
																	set { myUseDelayForProgressBar = value; } }
	
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
	
	// Parameters ---------------------------------------------------------------------------------------------------
	
	public int 						prParmCount {					get { return myParmCount; } 
																	set { myParmCount = value; } }
	public int						prParmIntValueCount {			get { return myParmIntValueCount; } 
																	set { myParmIntValueCount = value; } }
	public int						prParmFloatValueCount {			get { return myParmFloatValueCount; } 
																	set { myParmFloatValueCount = value; } }
	public int						prParmStringValueCount {		get { return myprParmStringValueCoun; } 
																	set { myprParmStringValueCoun = value; } }
	public int						prParmChoiceCount {				get { return myprParmChoiceCount; } 
																	set { myprParmChoiceCount = value; } }
	
	public HAPI_ParmInfo[] 			prParms {						get { return myParms; } 
																	set { myParms = value; } }
	public int[]					prParmIntValues {				get { return myParmIntValues; } 
																	set { myParmIntValues = value; } }
	public float[]					prParmFloatValues {				get { return myParmFloatValues; } 
																	set { myParmFloatValues = value; } }
	public int[]					prParmStringValues {			get { return myParmStringValues; }
																	set { myParmStringValues = value; } } // SH
	public HAPI_ParmChoiceInfo[]	prParmChoiceLists {				get { return myParmChoiceLists; } 
																	set { myParmChoiceLists = value; } }
	
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
	
	public int						prBakeSamplesPerSecond {				get { return myBakeSamplesPerSecond; }
																	set { myBakeSamplesPerSecond = value; } }
	public GameObject				prBakeParentObject {			get { return myBakeParentObject; }
																	set { myBakeParentObject = value; } }
	
	// GUI ----------------------------------------------------------------------------------------------------------
	
	public bool 					prShowObjectControls {			get { return myShowObjectControls; } 
																	set { myShowObjectControls = value; } }
	public bool 					prShowAssetControls {			get { return myShowAssetControls; } 
																	set { myShowAssetControls = value; } }
	public bool 					prShowAssetOptions {			get { return myShowAssetOptions; } 
																	set { myShowAssetOptions = value; } }
	public bool 					prShowBakeOptions {				get { return myShowBakeOptions; } 
																	set { myShowBakeOptions = value; } }
	public bool						prShowInputControls {			get { return myShowInputControls; } 
																	set { myShowInputControls = value; } }
	public bool						prAutoSelectAssetNode {			get { return myAutoSelectAssetNode; } 
																	set { myAutoSelectAssetNode = value; } }
	public bool						prEnableLogging {				get { return myEnableLogging; } 
																	set { myEnableLogging = value; } }
	public bool						prSyncAssetTransform {			get { return mySyncAssetTransform; } 
																	set { mySyncAssetTransform = value; } }
	public bool						prLiveTransformPropagation {	get { return myLiveTransformPropagation; } 
																	set { myLiveTransformPropagation = value; } }
	public bool						prEnableCooking {				get { return myEnableCooking; }
																	set { myEnableCooking = value; } }
	public bool						prHideGeometryOnLinking {		get { return myHideWhenFedToOtherAsset; }
																	set { myHideWhenFedToOtherAsset = value; } }
	public bool						prShowVertexColours {			get { return myShowVertexColours; }
																	set { myShowVertexColours = value; } }
	public bool						prShowPinnedInstances {			get { return myShowPinnedInstances; }
																	set { myShowPinnedInstances = value; } }
	public bool						prLiveInGameCooking {			get { return myLiveInGameCooking; }
																	set { myLiveInGameCooking = value; } }

	public int						prLastChangedParmId {			get { return myLastChangedParmId; } 
																	set { myLastChangedParmId = value; } }

	public bool						prIsGeoVisible {				get { return myIsGeoVisible; }
																	set { myIsGeoVisible = value; } }
	
	/// <summary>
	/// 	Indices of the currently selected folders in the Inspector.
	/// 	A 1:1 mapping with myFolderListSelectionIds.
	/// </summary>
	public List< int > 				prFolderListSelections {		get { return myFolderListSelections; } 
																	set { myFolderListSelections = value; } }
	
	/// <summary>
	/// 	Parameter ids of the currently selected folders in the Inspector. 
	/// 	A 1:1 mapping with myFolderListSelections.
	/// </summary>
	public List< int > 				prFolderListSelectionIds {		get { return myFolderListSelectionIds; } 
																	set { myFolderListSelectionIds = value; } }

	public List< string >			prTransInputNames {				get { return myTransInputNames; }
																	set { myTransInputNames = value; } }
	public List< string >			prGeoInputNames {				get { return myGeoInputNames; }
																	set { myGeoInputNames = value; } }
	public List< HAPI_GeoInputFormat >	prGeoInputFormats {			get { return myGeoInputFormats; }
																	set { myGeoInputFormats = value; } }
	
	public List< HAPI_InstancerOverrideInfo > prOverriddenInstances { get { return myOverriddenInstances; }		
																	  set {	myOverriddenInstances = value; } }
	
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods
	
	public HAPI_Asset() 
	{
		if ( prEnableLogging )
			Debug.Log( "HAPI_Asset created!" );
		
		HAPI.HAPI_SetPath.setPath();
		
		reset();
	}
	
	~HAPI_Asset() 
	{
		if ( prEnableLogging )
			Debug.Log( "HAPI_Asset destroyed!" );
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
		HAPI_Host.connectAssetGeometry( asset.prAssetId, object_index, 0, prAssetId, index );
		asset.addDownstreamGeoAsset( this );

		if ( HAPI_Host.prHideGeometryOnLinking && asset.prHideGeometryOnLinking )
		{
			asset.prIsGeoVisible = false;
			HAPI_PartControl[] controls = asset.GetComponentsInChildren< HAPI_PartControl >();
			foreach ( HAPI_PartControl control in controls )
			{
				if ( control.prGeoType != HAPI_GeoType.HAPI_GEOTYPE_EXPOSED_EDIT
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

	public void addGeoAsGeoInput( GameObject asset, int index )
	{
		prUpStreamGeoAssets[ index ] = null;
		prUpStreamGeoAdded[ index ] = true;
		
		int object_id;
		int geo_id;
		HAPI_Host.createGeoInput( prAssetId, index, out object_id, out geo_id );
		
		MeshFilter asset_mesh_filter 	= asset.GetComponent< MeshFilter >();
		Mesh mesh 						= asset_mesh_filter.sharedMesh;
		HAPI_PartControl child_control  = asset.GetComponent< HAPI_PartControl >();
		
		// Write marshlled geo to External Input Asset ( having assetId '0' )
		Utility.setMesh( 0, object_id, geo_id, ref mesh, child_control );

		// Apply the input asset transform to the marshaled object in the Houdini scene.
		HAPI_TransformEuler trans = Utility.getHapiTransform( asset.transform.localToWorldMatrix );
		HAPI_Host.setObjectTransform( 0, object_id, trans );
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
			
			HAPI_Host.unloadOTL( prAssetId );
			prAssetId = -1;
		}
	}

	public virtual void OnEnable()
	{
		if ( prAssetId >= 0 )
		{
			if ( HAPI_Host.isAssetValid( prAssetId, prAssetValidationId ) )
			{
				// Reloading asset after mode change or script-reload.
				prPartialBuild					= true;
			}
			else
			{
				// Loading Scene (no Houdini scene exists yet)
				prUseDelayForProgressBar		= false;
				prFullBuild						= true;
				prForceReconnectInFullBuild		= true;
				prAssetId						= -1;
			}

			build();
		}
	}
	
	public override void reset()
	{
		base.reset();

		// Please keep these in the same order and grouping as their declarations at the top.
		
		// Assets ---------------------------------------------------------------------------------------------------
		
		prAssetInfo 				= new HAPI_AssetInfo();
		prPreset 					= null;
		prAssetValidationId			= -1;
		prAssetName					= "ASSET_NAME";
		prAssetType					= AssetType.TYPE_INVALID;
		prHAPIAssetType 			= HAPI_AssetType.HAPI_ASSETTYPE_INVALID;
		prAssetSubType 				= 0;
		prFullBuild					= true;
		prForceReconnectInFullBuild	= false;
		prReloadAssetInFullBuild	= true;
		prUseDelayForProgressBar	= true;
		
		// Inputs ---------------------------------------------------------------------------------------------------
		
		prMinTransInputCount 		= 0;
		prMaxTransInputCount 		= 0;
		prMinGeoInputCount 			= 0;
		prMaxGeoInputCount 			= 0;
		prFileInputs 				= new List< string >();
		
		prDownStreamTransformAssets = new List< HAPI_Asset >();
		prUpStreamTransformAssets 	= new List< HAPI_Asset >();
		prUpStreamTransformObjects 	= new List< GameObject >();
		
		prDownStreamGeoAssets 		= new List< HAPI_Asset >();
		prUpStreamGeoAssets 		= new List< HAPI_Asset >();
		prUpStreamGeoObjects 		= new List< GameObject >();
		prUpStreamGeoAdded 			= new List< bool >();
		
		// Parameters -----------------------------------------------------------------------------------------------
		
		prParmCount 				= 0;
		prParmIntValueCount 		= 0;
		prParmFloatValueCount 		= 0;
		prParmStringValueCount 		= 0;
		prParmChoiceCount 			= 0;
		
		prParms 					= null;
		prParmIntValues 			= new int[ 0 ];
		prParmFloatValues 			= new float[ 0 ];
		prParmStringValues 			= new int[ 0 ]; // string handles (SH)
		prParmChoiceLists 			= new HAPI_ParmChoiceInfo[ 0 ];
		
		// Objects --------------------------------------------------------------------------------------------------
		
		prObjectCount 				= 0;
		prHandleCount 				= 0;
		
		prObjects 					= new HAPI_ObjectInfo[ 0 ];
		
		prGameObjects 				= new GameObject[ 0 ];
		prObjectTransforms 			= new HAPI_Transform[ 0 ];
		
		// Baking ---------------------------------------------------------------------------------------------------
		prBakeStartTime				= 0.0f;
		prBakeEndTime				= 1.0f;
		prBakeSamplesPerSecond		= 30;
		prBakeParentObject			= null;
		
		// GUI ------------------------------------------------------------------------------------------------------
		
		prShowObjectControls 		= true;
		prShowAssetControls 		= true;
		prShowAssetOptions			= false;
		prShowBakeOptions			= false;
		prShowInputControls 		= true;
		prAutoSelectAssetNode 		= true;
		prEnableLogging				= false;
		prSyncAssetTransform		= true;
		prLiveTransformPropagation	= false;
		prEnableCooking				= true;
		myHideWhenFedToOtherAsset	= true;
		prShowVertexColours			= false;
		prShowPinnedInstances		= true;
		prLiveInGameCooking			= false;
		
		prLastChangedParmId 		= -1;

		myIsGeoVisible				= true;
		
		prFolderListSelections 		= new List< int >();
		prFolderListSelectionIds 	= new List< int >();
		prFolderListSelections.Add( 0 );
		prFolderListSelectionIds.Add( -1 );

		prTransInputNames			= new List< string >();
		prGeoInputNames				= new List< string >();
		prGeoInputFormats			= new List< HAPI_GeoInputFormat >();
		prOverriddenInstances 		= new List< HAPI_InstancerOverrideInfo >();
		
		myProgressBarJustUsed 		= false;
	}
	
	public virtual bool build() 
	{
		Debug.LogError( "Should not be calling this directly!" );
		return false;
	}

	public virtual bool build( bool unload_asset_first )
	{
		if ( !HAPI.HAPI_SetPath.prIsPathSet )
		{
			Debug.LogError( "Cannot build asset as Houdini dlls not found!" );
			return false;
		}

		if ( !prEnableCooking || !HAPI_Host.prEnableCooking )
			return false;

		HAPI_ProgressBar progress_bar	= new HAPI_ProgressBar();
		progress_bar.prUseDelay			= prUseDelayForProgressBar;
		progress_bar.prAsset			= this;

		try
		{
			progress_bar.prStartTime = System.DateTime.Now;
			
			if ( prFullBuild || prPartialBuild ) 
			{
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

					prReloadAssetInFullBuild = true; // The default.

					progress_bar.statusCheckLoop();

					prAssetInfo = HAPI_Host.getAssetInfo( asset_id );

					if ( !prPartialBuild )
						Debug.Log( "Asset Loaded - Path: " + prAssetInfo.instancePath + ", ID: " + prAssetInfo.id );
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
						prLiveInGameCooking = false;
					
					return false; // false for failed :(
				}
				
				// For convenience we copy some asset info properties locally (since they are constant anyway).
				// More imporantly, structs are not serialized and therefore putting them into their own
				// variables is required in order to maintain state between serialization cycles.
				prAssetId 				= prAssetInfo.id;
				prAssetValidationId		= prAssetInfo.validationId;
				prAssetName				= prAssetInfo.name;
				prHAPIAssetType			= (HAPI_AssetType) prAssetInfo.type;
				prMinTransInputCount	= prAssetInfo.minTransInputCount;
				prMaxTransInputCount	= prAssetInfo.maxTransInputCount;
				prMinGeoInputCount 		= prAssetInfo.minGeoInputCount;
				prMaxGeoInputCount		= prAssetInfo.maxGeoInputCount;
				prParmCount 			= prAssetInfo.parmCount;
				prParmIntValueCount		= prAssetInfo.parmIntValueCount;
				prParmFloatValueCount	= prAssetInfo.parmFloatValueCount;
				prParmStringValueCount	= prAssetInfo.parmStringValueCount;
				prParmChoiceCount		= prAssetInfo.parmChoiceCount;
				
				prObjectCount 			= prAssetInfo.objectCount;
				prHandleCount 			= prAssetInfo.handleCount;
				
				progress_bar.prCurrentValue			= 0;
				progress_bar.prTotal				= prParmCount
													  + prParmIntValueCount
													  + prParmFloatValueCount
													  + prParmStringValueCount
													  + prParmChoiceCount
													  + prObjectCount
													  + prHandleCount;
				
				// Try to load presets.
				loadPreset();
				
				progress_bar.displayProgressBar();
				myProgressBarJustUsed = true;
				
				progress_bar.prMessage = "Loading parameter information...";
				
				// Get all parameters.
				prParms = new HAPI_ParmInfo[ prParmCount ];
				Utility.getArray1Id( prAssetId, HAPI_Host.getParameters, prParms, prParmCount );
				progress_bar.incrementProgressBar( prParmCount );
				
				// Get parameter int values.
				prParmIntValues = new int[ prParmIntValueCount ];
				Utility.getArray1Id( prAssetId, HAPI_Host.getParmIntValues, prParmIntValues, prParmIntValueCount );
				progress_bar.incrementProgressBar( prParmIntValueCount );
				
				// Get parameter float values.
				prParmFloatValues = new float[ prParmFloatValueCount ];
				Utility.getArray1Id( prAssetId, HAPI_Host.getParmFloatValues, prParmFloatValues, prParmFloatValueCount );
				progress_bar.incrementProgressBar( prParmFloatValueCount );
				
				// Get parameter string (handle) values.
				prParmStringValues = new int[ prParmStringValueCount ];
				Utility.getArray1Id( prAssetId, HAPI_Host.getParmStringValues, prParmStringValues, 
									 prParmStringValueCount );
				progress_bar.incrementProgressBar( prParmStringValueCount );
				
				// Get parameter choice lists.
				prParmChoiceLists = new HAPI_ParmChoiceInfo[ prParmChoiceCount ];
				Utility.getArray1Id( prAssetId, HAPI_Host.getParmChoiceLists, prParmChoiceLists, prParmChoiceCount );
				progress_bar.incrementProgressBar( prParmChoiceCount );
				
				// Add input fields.
				if ( !prPartialBuild && !prForceReconnectInFullBuild )
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

				if ( !prPartialBuild )
				{
					// Clean up.
					destroyChildren( transform );
					prGameObjects = new GameObject[ prObjectCount ];
				}

				// Create local object info caches (transforms need to be stored in a parallel array).
				prObjects = new HAPI_ObjectInfo[ prObjectCount ];
				prObjectTransforms = new HAPI_Transform[ prObjectCount ];

				// Custom work during a full build (custom to each subclass).
				buildFullBuildCustomWork( ref progress_bar );
			}
			else
			{
				progress_bar.displayProgressBar();
				myProgressBarJustUsed = true;
				
				progress_bar.prTotal = prObjectCount + prParmIntValueCount 
									   + prParmFloatValueCount + prParmStringValueCount;

				HAPI_Host.cookAsset( prAssetId );
				progress_bar.statusCheckLoop();

				// We need to get the parameter values again because they could have been
				// changed by a script.

				// Get parameter int values.
				Utility.getArray1Id( prAssetId, HAPI_Host.getParmIntValues, prParmIntValues, prParmIntValueCount );
				progress_bar.incrementProgressBar( prParmIntValueCount );
				
				// Get parameter float values.
				Utility.getArray1Id( prAssetId, HAPI_Host.getParmFloatValues, prParmFloatValues, prParmFloatValueCount );
				progress_bar.incrementProgressBar( prParmFloatValueCount );
				
				// Get parameter string (handle) values.
				Utility.getArray1Id( prAssetId, HAPI_Host.getParmStringValues, prParmStringValues, 
									 prParmStringValueCount );
				progress_bar.incrementProgressBar( prParmStringValueCount );
			}

			if ( !prPartialBuild )
			{
				// Set asset's transform.
				if ( prSyncAssetTransform )
				{
					HAPI_TransformEuler hapi_transform;
					HAPI_Host.getAssetTransform( prAssetId, (int) HAPI_RSTOrder.SRT, 
												 (int) HAPI_XYZOrder.ZXY, out hapi_transform );
					if ( Mathf.Approximately( 0.0f, hapi_transform.scale[ 0 ] ) ||
						 Mathf.Approximately( 0.0f, hapi_transform.scale[ 1 ] ) ||
						 Mathf.Approximately( 0.0f, hapi_transform.scale[ 2 ] ) )
					{
						Debug.LogWarning( "Asset(id: " + prAssetId + ", name: " + prAssetName + "): Scale has a zero component!" );
					}

					Utility.applyTransform( hapi_transform, transform );
				}
			
				progress_bar.prMessage = "Loading and composing objects...";
			
				Utility.getArray1Id( prAssetId, HAPI_Host.getObjects, prObjects, prObjectCount );
				Utility.getArray2Id( prAssetId, (int) HAPI_RSTOrder.SRT, HAPI_Host.getObjectTransforms, 
						 			 prObjectTransforms, prObjectCount );

				// Custom way to load objects (custom to each subclass).
				buildCreateObjects( ref progress_bar );
			
				// Process dependent assets.
				processDependentAssets();
			}
		}
		catch ( HAPI_ErrorIgnorable ) {}
		catch ( HAPI_ErrorProgressCancelled error )
		{
			// If in play mode, disable live cooks.
			if ( EditorApplication.isPlaying )
				prLiveInGameCooking = false;

			Debug.LogError( error.ToString() );
		}
		catch ( HAPI_Error error )
		{
			Debug.LogError( error.ToString() );
		}
		catch ( System.Exception error )
		{
			Debug.LogError( error.ToString() );
		}
		finally
		{
			progress_bar.clearProgressBar();

			prFullBuild = false;
			prPartialBuild = false;
			prForceReconnectInFullBuild = false;

			myProgressBarJustUsed = false;
			prUseDelayForProgressBar = true;
		}
		
		return true;
	}

	public virtual void Update()
	{
		if ( !prSyncAssetTransform || prAssetId < 0 || EditorApplication.isPlayingOrWillChangePlaymode )
			return;

		try
		{
			Matrix4x4 local_to_world = transform.localToWorldMatrix;
			if ( local_to_world == myLastLocalToWorld )
				return;
			myLastLocalToWorld = local_to_world;

			HAPI_TransformEuler hapi_transform = Utility.getHapiTransform( local_to_world );
			HAPI_Host.setAssetTransform( prAssetId, ref hapi_transform );

			int parm = -1;
			float [] parm_data = new float[ 3 ];

			parm = findParm( "t" );
			if ( parm > 0 )
			{
				HAPI_Host.getParmFloatValues( prAssetId, parm_data, prParms[ parm ].floatValuesIndex, 3 );
				for ( int i = 0; i < 3; ++i )
					prParmFloatValues[ prParms[ parm ].floatValuesIndex + i ] = parm_data[ i ];
			}

			parm = findParm( "r" );
			if ( parm > 0 )
			{
				HAPI_Host.getParmFloatValues( prAssetId, parm_data, prParms[ parm ].floatValuesIndex, 3 );
				for ( int i = 0; i < 3; ++i )
					prParmFloatValues[ prParms[ parm ].floatValuesIndex + i ] = parm_data[ i ];
			}

			parm = findParm( "s" );
			if ( parm > 0 )
			{
				HAPI_Host.getParmFloatValues( prAssetId, parm_data, prParms[ parm ].floatValuesIndex, 3 );
				for ( int i = 0; i < 3; ++i )
					prParmFloatValues[ prParms[ parm ].floatValuesIndex + i ] = parm_data[ i ];
			}

			// Process dependent assets.
			// TODO: These steps here might be too slow for some assets and can grind Unity to
			// a halt. But if we are to support all the different effects of transform changes
			// then we do need to do a full build so I'm not sure how to do this more proper.
			// Do note that the build function is fairly conditional and should only build
			// the bare minimum.
			if ( prLiveTransformPropagation )
			{
				foreach ( HAPI_Asset downstream_asset in prDownStreamTransformAssets )
					downstream_asset.build();
			
				foreach ( HAPI_Asset downstream_asset in prDownStreamGeoAssets )
					downstream_asset.build();
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
			int num_objects = prObjects.Length;
			for( int ii = 0; ii < num_objects; ii++ )
			{
				GameObject game_object = prGameObjects[ ii ];
				HAPI_ObjectInfo obj_info = prObjects[ ii ];
				
				if( game_object != null )
				{
					if( !obj_info.isInstancer )					
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
				
				
				for( int ii = 0; ii < num_objects; ii++ )
				{
					GameObject game_object = prGameObjects[ ii ];
					HAPI_ObjectInfo obj_info = prObjects[ ii ];
					
					if( game_object != null )
					{
						if( !obj_info.isInstancer )					
						{
							HAPI_ObjectControl obj_control = game_object.GetComponent< HAPI_ObjectControl >();
							obj_control.bakeAnimation( curr_time, parent_object );
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
			
			
			for( int ii = 0; ii < num_objects; ii++ )
			{
				GameObject game_object = prGameObjects[ ii ];
				HAPI_ObjectInfo obj_info = prObjects[ ii ];
				
				if( game_object != null )
				{
					if( !obj_info.isInstancer )					
					{
						HAPI_ObjectControl obj_control = game_object.GetComponent< HAPI_ObjectControl >();
						obj_control.endBakeAnimation();
					}
					else
					{
						HAPI_Instancer instancer = game_object.GetComponent< HAPI_Instancer >();
						instancer.endBakeAnimation();
					}
				}
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
			if ( myPreset != null && myPreset.Length > 0 && prReloadAssetInFullBuild )
				HAPI_Host.setPreset( prAssetId, myPreset, myPreset.Length );
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
			myPreset = HAPI_Host.getPreset( prAssetId );
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
	protected abstract void buildCreateObjects( ref HAPI_ProgressBar progress_bar );

	// -------------------------------------------------------------------------------------------------------------

	protected virtual void destroyChildren( Transform trans ) 
	{
		List< GameObject > children = new List< GameObject >();
		
		foreach ( Transform child in trans )
			children.Add( child.gameObject );
		
		foreach ( GameObject child in children )
			DestroyImmediate( child );
	}
	
	protected virtual int findParm( string name )
	{
		if ( prParms == null )
			return -1;

		for ( int i = 0; i < prParms.Length; ++i )
		{
			if ( prParms[ i ].name == name )
				return i;
		}
		return -1;
	}

	protected virtual void processDependentAssets()
	{
		if ( !prPartialBuild && !prForceReconnectInFullBuild )
		{
			foreach ( HAPI_Asset downstream_asset in prDownStreamTransformAssets )
			{
				prEnableCooking = false;
				if ( !downstream_asset.isAssetValid() )
					downstream_asset.OnEnable();
				downstream_asset.prUseDelayForProgressBar = prUseDelayForProgressBar;
				downstream_asset.build();
				prEnableCooking = true;
			}
			
			foreach ( HAPI_Asset downstream_asset in prDownStreamGeoAssets )
			{
				prEnableCooking = false;
				if ( !downstream_asset.isAssetValid() )
					downstream_asset.OnEnable();
				downstream_asset.prUseDelayForProgressBar = prUseDelayForProgressBar;
				downstream_asset.build();
				prEnableCooking = true;
			}
		}
		else if ( prForceReconnectInFullBuild )
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
						}
						addAssetAsGeoInput( asset, object_index, i );
					}
					else
						addGeoAsGeoInput( new_obj, i );
				}
			}

			prForceReconnectInFullBuild = false;
			prFullBuild = false;
			prPartialBuild = false;
			build(); // Need to rebuild because now we're connected to other assets.
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
	[SerializeField] private AssetType				myAssetType;
	[SerializeField] private HAPI_AssetType			myHAPIAssetType;
	[SerializeField] private HAPI_AssetSubType		myAssetSubType;
	[SerializeField] private bool					myFullBuild;
	[SerializeField] private bool					myPartialBuild;
	[SerializeField] private bool					myForceReconnectInFullBuild;
	[SerializeField] private bool 					myReloadAssetInFullBuild;
	[SerializeField] private bool					myUseDelayForProgressBar;
	
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
	
	[SerializeField] private int 					myParmCount;
	[SerializeField] private int					myParmIntValueCount;
	[SerializeField] private int					myParmFloatValueCount;
	[SerializeField] private int					myprParmStringValueCoun;
	[SerializeField] private int					myprParmChoiceCount;
	
	[SerializeField] private HAPI_ParmInfo[] 		myParms;
	[SerializeField] private int[]					myParmIntValues;
	[SerializeField] private float[]				myParmFloatValues;
	[SerializeField] private int[]					myParmStringValues; // string handles (SH)
	[SerializeField] private HAPI_ParmChoiceInfo[]	myParmChoiceLists;
	
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
	[SerializeField] private GameObject				myBakeParentObject;

	
	// GUI ----------------------------------------------------------------------------------------------------------
	
	[SerializeField] private bool 					myShowObjectControls;
	[SerializeField] private bool 					myShowAssetControls;
	[SerializeField] private bool 					myShowAssetOptions;
	[SerializeField] private bool 					myShowBakeOptions;
	[SerializeField] private bool					myShowInputControls;
	[SerializeField] private bool					myAutoSelectAssetNode;
	[SerializeField] private bool					myEnableLogging;
	[SerializeField] private bool					mySyncAssetTransform;
	[SerializeField] private bool					myLiveTransformPropagation;
	[SerializeField] private bool					myEnableCooking;
	[SerializeField] private bool					myHideWhenFedToOtherAsset;
	[SerializeField] private bool					myShowVertexColours;
	[SerializeField] private bool					myShowPinnedInstances;	
	[SerializeField] private bool					myLiveInGameCooking;
	
	[SerializeField] private int					myLastChangedParmId;

	[SerializeField] private bool					myIsGeoVisible;
	
	/// <summary>
	/// 	Indices of the currently selected folders in the Inspector.
	/// 	A 1:1 mapping with myFolderListSelectionIds.
	/// </summary>
	[SerializeField] private List< int > 			myFolderListSelections;
	
	/// <summary>
	/// 	Parameter ids of the currently selected folders in the Inspector. 
	/// 	A 1:1 mapping with myFolderListSelections.
	/// </summary>
	[SerializeField] private List< int > 			myFolderListSelectionIds;

	[SerializeField] private List< string >			myTransInputNames;
	[SerializeField] private List< string >			myGeoInputNames;
	[SerializeField] 
	private List< HAPI_GeoInputFormat >				myGeoInputFormats;
	
	
	[SerializeField] private List< HAPI_InstancerOverrideInfo >		myOverriddenInstances;
	
	// Private Temporary Data
	[SerializeField] private Matrix4x4				myLastLocalToWorld;
}
