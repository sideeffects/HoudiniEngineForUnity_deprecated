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
public class HAPI_Asset : MonoBehaviour 
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
	public int 						prAssetId {						get { return myAssetId; } 
																	set { myAssetId = value; } }
	public int						prAssetValidationId {			get { return myAssetValidationId; }
																	protected set { myAssetValidationId = value; } } 
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
	
	// GUI ----------------------------------------------------------------------------------------------------------
	
	public bool 					prShowObjectControls {			get { return myShowObjectControls; } 
																	set { myShowObjectControls = value; } }
	public bool 					prShowAssetControls {			get { return myShowAssetControls; } 
																	set { myShowAssetControls = value; } }
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
	public bool						prHideWhenFedToOtherAsset {		get { return myHideWhenFedToOtherAsset; }
																	set { myHideWhenFedToOtherAsset = value; } }
	public bool						prShowVertexColours {			get { return myShowVertexColours; }
																	set { myShowVertexColours = value; } }

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

		if ( HAPI_Host.prHideGeometryOnLinking && asset.prHideWhenFedToOtherAsset )
		{
			asset.prIsGeoVisible = false;
			HAPI_ChildSelectionControl[] controls = asset.GetComponentsInChildren< HAPI_ChildSelectionControl >();
			foreach ( HAPI_ChildSelectionControl control in controls )
			{
				if ( control.prGeoType != (int) HAPI_GeoType.HAPI_GEOTYPE_EXPOSED_EDIT
					 && control.gameObject.GetComponent< MeshRenderer >() != null )
					control.gameObject.GetComponent< MeshRenderer >().enabled = false;
			}
		}
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
		HAPI_ChildSelectionControl child_control = asset.GetComponent< HAPI_ChildSelectionControl >();
		
		Utility.setMesh( prAssetId, object_id, geo_id, ref mesh, child_control );
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
				prPartialBuild = true;
			}
			else
			{
				// Loading Scene (no Houdini scene exists yet)
				prFullBuild = true;
				prForceReconnectInFullBuild = true;
				prAssetId = -1;
			}

			build();
		}
	}
	
	public virtual void reset()
	{
		// Please keep these in the same order and grouping as their declarations at the top.
		
		// Assets ---------------------------------------------------------------------------------------------------
		
		prAssetInfo 				= new HAPI_AssetInfo();
		prPreset 					= null;
		prAssetId 					= -1;
		prAssetValidationId			= -1;
		prAssetType					= AssetType.TYPE_INVALID;
		prHAPIAssetType 			= HAPI_AssetType.HAPI_ASSETTYPE_INVALID;
		prAssetSubType 				= 0;
		prFullBuild					= true;
		prForceReconnectInFullBuild	= false;
		prReloadAssetInFullBuild	= true;
		
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
		
		// GUI ------------------------------------------------------------------------------------------------------
		
		prShowObjectControls 		= true;
		prShowAssetControls 		= true;
		prShowInputControls 		= true;
		prAutoSelectAssetNode 		= false; // Should be turned back on for production build.
		prEnableLogging				= false;
		prSyncAssetTransform		= true;
		prLiveTransformPropagation	= false;
		prEnableCooking				= true;
		myHideWhenFedToOtherAsset	= true;
		prShowVertexColours			= false;
		
		prLastChangedParmId 		= -1;

		myIsGeoVisible				= true;
		
		prFolderListSelections 		= new List< int >();
		prFolderListSelectionIds 	= new List< int >();
		prFolderListSelections.Add( 0 );
		prFolderListSelectionIds.Add( -1 );

		prTransInputNames			= new List< string >();
		prGeoInputNames				= new List< string >();
		prGeoInputFormats			= new List< HAPI_GeoInputFormat >();
		
		myProgressBarJustUsed 		= false;
				
	}
	
	public virtual bool build()
	{
		return build( -1 );
	}
	public virtual bool build( int source ) 
	{
		if ( !HAPI.HAPI_SetPath.prIsPathSet )
		{
			Debug.LogError( "Cannot build asset as Houdini dlls not found!" );
			return false;
		}

		if ( !prEnableCooking )
			return false;
		
		if ( source == prAssetId && !( prFullBuild || prPartialBuild ) )
			return false;

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
			int buf_length = 0;
			HAPI_Host.getPreset( prAssetId, myPreset, ref buf_length );
				
			myPreset = new byte[ buf_length ];
				
			HAPI_Host.getPreset( prAssetId, myPreset, ref buf_length );
		}
		catch {} // Just catch them here but don't report them because we would just get a huge stream of errors.
	}

	public bool isAssetValid()
	{
		return HAPI_Host.isAssetValid( prAssetId, prAssetInfo.validationId );
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Protected Methods

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

	protected virtual void processDependentAssets( int source )
	{
		// If we're the source, set the source id to our asset id.
		if ( source < 0 )
			source = prAssetId;

		if ( !prPartialBuild && !prForceReconnectInFullBuild )
		{
			foreach ( HAPI_Asset downstream_asset in prDownStreamTransformAssets )
			{
				if ( !downstream_asset.isAssetValid() )
					downstream_asset.OnEnable();
				downstream_asset.build( source );
			}
			
			foreach ( HAPI_Asset downstream_asset in prDownStreamGeoAssets )
			{
				if ( !downstream_asset.isAssetValid() )
					downstream_asset.OnEnable();
				downstream_asset.build( source );
			}
		}
		else if ( prForceReconnectInFullBuild )
		{
			for ( int i = 0; i < prUpStreamTransformObjects.Count; ++i )
			{
				if ( prUpStreamTransformObjects[ i ] != null )
				{
					GameObject game_obj = prUpStreamTransformObjects[ i ];

					HAPI_Asset asset_component = game_obj.GetComponent< HAPI_Asset >();
					if ( asset_component )
					{
						if ( !asset_component.isAssetValid() )
							asset_component.OnEnable();
						addAssetAsTransformInput( asset_component, i );
					}
				}
			}
			
			for ( int i = 0; i < prUpStreamGeoObjects.Count; ++i )
			{
				if ( prUpStreamGeoObjects[ i ] != null )
				{
					GameObject new_obj = prUpStreamGeoObjects[ i ];

					HAPI_Asset asset_component = null;
					HAPI_ChildSelectionControl 
						child_selection_control = new_obj.GetComponent< HAPI_ChildSelectionControl >();
							
					int object_index = 0;
					if ( child_selection_control )
					{
						object_index = child_selection_control.prObjectId;
						asset_component = child_selection_control.prAsset;
					}
					else
						asset_component = new_obj.GetComponent< HAPI_Asset >();
							
					if ( asset_component )
					{
						if ( !asset_component.isAssetValid() )
							asset_component.OnEnable();
						addAssetAsGeoInput( asset_component, object_index, i );
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
	[SerializeField] private int					myAssetId;
	[SerializeField] private int					myAssetValidationId;
	[SerializeField] private AssetType				myAssetType;
	[SerializeField] private HAPI_AssetType			myHAPIAssetType;
	[SerializeField] private HAPI_AssetSubType		myAssetSubType;
	[SerializeField] private bool					myFullBuild;
	[SerializeField] private bool					myPartialBuild;
	[SerializeField] private bool					myForceReconnectInFullBuild;
	[SerializeField] private bool 					myReloadAssetInFullBuild;
	
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
	
	// GUI ----------------------------------------------------------------------------------------------------------
	
	[SerializeField] private bool 					myShowObjectControls;
	[SerializeField] private bool 					myShowAssetControls;
	[SerializeField] private bool					myShowInputControls;
	[SerializeField] private bool					myAutoSelectAssetNode;
	[SerializeField] private bool					myEnableLogging;
	[SerializeField] private bool					mySyncAssetTransform;
	[SerializeField] private bool					myLiveTransformPropagation;
	[SerializeField] private bool					myEnableCooking;
	[SerializeField] private bool					myHideWhenFedToOtherAsset;
	[SerializeField] private bool					myShowVertexColours;
	
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

	// Private Temporary Data
	[SerializeField] private Matrix4x4				myLastLocalToWorld;
}
