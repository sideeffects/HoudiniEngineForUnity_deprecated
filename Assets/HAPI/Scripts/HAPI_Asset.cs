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
	
	public HAPI_AssetInfo 			prAssetInfo { get { return myAssetInfo; } set { myAssetInfo = value; } }
	public byte[]					prPreset { get { return myPreset; } set { myPreset = value; } }
	public int 						prAssetId { get; set; }	
	public AssetType				prAssetType { get; set; }
	public HAPI_AssetType			prHAPIAssetType { get; set; }
	public int						prAssetSubType { get { return myAssetSubType; } set { myAssetSubType = value; } }
	public bool						prFullBuild { get; set; }
	public bool 					prUnloadAssetInFullBuild { get; set; }
	
	// Inputs -------------------------------------------------------------------------------------------------------
	
	public int 						prMinInputCount { get; set; }
	public int 						prMaxInputCount { get; set; }
	public int 						prMinGeoInputCount { get; set; }
	public int						prMaxGeoInputCount { get; set; }
	public List< string >			prFileInputs { get; set; }
	
	public List< HAPI_Asset >		prDownStreamTransformAssets { get; set; }
	public List< HAPI_Asset >		prUpStreamTransformAssets { get; set; }
	public List< GameObject >		prUpStreamTransformObjects { get; set; }
	
	public List< HAPI_Asset >		prDownStreamGeoAssets { get; set; }
	public List< HAPI_Asset >		prUpStreamGeoAssets { get; set; }
	public List< GameObject >		prUpStreamGeoObjects { get; set; }
	public List< bool >				prUpStreamGeoAdded { get; set; }
	
	// Parameters ---------------------------------------------------------------------------------------------------
	
	public int 						prParmCount { get; set; }
	public int						prParmIntValueCount { get; set; }
	public int						prParmFloatValueCount { get; set; }
	public int						prParmStringValueCount { get; set; }
	public int						prParmChoiceCount { get; set; }
	
	public HAPI_ParmInfo[] 			prParms { get; set; }
	public int[]					prParmIntValues { get; set; }
	public float[]					prParmFloatValues { get; set; }
	public int[]					prParmStringValues { get; set; } // string handles (SH)
	public HAPI_ParmChoiceInfo[]	prParmChoiceLists { get; set; }
	
	// Objects ------------------------------------------------------------------------------------------------------
	
	public int 						prObjectCount { get; set; }
	public int						prHandleCount { get; set; }
	
	public HAPI_ObjectInfo[] 		prObjects { get; set; }
	
	public GameObject[]				prGameObjects {	get; set; }
	public HAPI_Transform[] 		prObjectTransforms { get; set; }
	
	// GUI ----------------------------------------------------------------------------------------------------------
	
	public bool 					prShowObjectControls { get; set; }
	public bool 					prShowAssetControls { get; set; }
	public bool						prShowInputControls { get; set; }
	public bool						prAutoSelectAssetNode { get; set; }
	public bool						prEnableLogging { get; set; }
	public bool						prSyncAssetTransform { get; set; }
	
	public int						prLastChangedParmId { get; set; }
	
	/// <summary>
	/// 	Indices of the currently selected folders in the Inspector.
	/// 	A 1:1 mapping with myFolderListSelectionIds.
	/// </summary>
	public List< int > 				prFolderListSelections { get; set; }
	
	/// <summary>
	/// 	Parameter ids of the currently selected folders in the Inspector. 
	/// 	A 1:1 mapping with myFolderListSelections.
	/// </summary>
	public List< int > 				prFolderListSelectionIds { get; set; }
	
	
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
		build();
		return;
	}
	
	public void removeTransformInput( int index )
	{
		if ( prUpStreamTransformAssets[ index ] != null )
		{
			prUpStreamTransformAssets[ index ].removeDownstreamTransformAsset( this );
			HAPI_Host.disconnectAssetTransform( prAssetId, index );
			prUpStreamTransformAssets[ index ] = null;
			build();
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
				build();
				return;
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
		
		//TODO: It's hard coded to be the 0th group - fix this!
		HAPI_Host.connectAssetGeometry( asset.prAssetId, object_index, 0, prAssetId, index );
		asset.addDownstreamGeoAsset( this );
		build();
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
		build();
	}
	
	public void removeGeoInput( int index )
	{
		try
		{
			if ( prUpStreamGeoAssets[ index ] )
			{
				prUpStreamGeoAssets[ index ].removeDownstreamGeoAsset( this );
				HAPI_Host.disconnectAssetGeometry( prAssetId, index );
				prUpStreamGeoAssets[ index ] = null;
				build();
			}
			
			if ( prUpStreamGeoAdded[ index ] )
			{
				HAPI_Host.disconnectAssetGeometry( prAssetId, index );
				prUpStreamGeoAdded[ index ] = false;
				build();
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
		if ( prAssetId >= 0 )
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
	
	public virtual void reset()
	{
		// Please keep these in the same order and grouping as their declarations at the top.
		
		// Assets ---------------------------------------------------------------------------------------------------
		
		prAssetInfo 				= new HAPI_AssetInfo();
		prPreset 					= null;
		prAssetId 					= -1;
		prAssetType					= AssetType.TYPE_INVALID;
		prHAPIAssetType 			= HAPI_AssetType.HAPI_ASSETTYPE_INVALID;
		prAssetSubType 				= 0;
		prFullBuild					= true;
		prUnloadAssetInFullBuild	= true;
		
		// Inputs ---------------------------------------------------------------------------------------------------
		
		prMinInputCount 			= 0;
		prMaxInputCount 			= 0;
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
		prAutoSelectAssetNode 		= true;
		prEnableLogging				= false;
		prSyncAssetTransform		= true;
		
		prLastChangedParmId 		= -1;
		
		prFolderListSelections 		= new List< int >();
		prFolderListSelectionIds 	= new List< int >();
		prFolderListSelections.Add( 0 );
		prFolderListSelectionIds.Add( -1 );
		
		myProgressBarJustUsed 		= false;
				
	}
	
	public virtual bool build() 
	{
		if ( !HAPI.HAPI_SetPath.prIsPathSet )
		{
			Debug.LogError( "Cannot build asset as Houdini dlls not found!" );
			return false;
		}
		
		return true;
	}

	public virtual void Update()
	{
		if ( !prSyncAssetTransform )
			return;

		Matrix4x4 local_to_world = transform.localToWorldMatrix;
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
		for ( int i = 0; i < prParms.Length; ++i )
		{
			if ( prParms[ i ].name == name )
				return i;
		}
		return -1;
	}	
	
	// PROGRESS BAR -------------------------------------------------------------------------------------------------
	public bool hasProgressBarBeenUsed()
	{
		return myProgressBarJustUsed;
	}
	
	protected bool				myProgressBarJustUsed;
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Serialized Data
	
	[SerializeField]
	protected byte[] 			myPreset;
	[SerializeField]
	protected HAPI_AssetInfo	myAssetInfo;
	[SerializeField]
	protected int				myAssetSubType;
}
