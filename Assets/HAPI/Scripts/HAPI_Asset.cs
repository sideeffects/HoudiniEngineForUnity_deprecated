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
		
		prLastChangedParmId 		= -1;
		
		prFolderListSelections 		= new List< int >();
		prFolderListSelectionIds 	= new List< int >();
		prFolderListSelections.Add( 0 );
		prFolderListSelectionIds.Add( -1 );
		
		myProgressBarJustUsed 		= false;
		myProgressBarTotal			= 0;
		myProgressBarCurrent		= 0;
		myProgressBarTitle			= "Building Houdini Asset";
		myProgressBarMsg			= "";

		myProgressBarLastValue		= -1;
		myProgressBarLastMsg		= "";
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
		int t = findParm( "t" );
		if ( t < 0 )
			return;


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
	
	protected void statusCheckLoop()
	{
		HAPI_State state = HAPI_State.HAPI_STATE_STARTING_LOAD;
		myProgressBarCurrent = 0;
		myProgressBarTotal = 100;
		while ( state != HAPI_State.HAPI_STATE_READY )
		{
			state = (HAPI_State) HAPI_Host.getStatus( HAPI_StatusType.HAPI_STATUS_STATE );

			if ( state == HAPI_State.HAPI_STATE_COOKING )
			{
				myProgressBarCurrent = HAPI_Host.getCookingCurrentCount();
				myProgressBarTotal = HAPI_Host.getCookingTotalCount();
			}
			else
			{
				myProgressBarCurrent = ( System.DateTime.Now - myProgressBarStartTime ).Seconds;
				myProgressBarTotal = 100;
			}

			myProgressBarMsg = HAPI_Host.getStatusString( HAPI_StatusType.HAPI_STATUS_STATE );
			displayProgressBar();
		}
	}

	protected void incrementProgressBar()
	{
		incrementProgressBar( 1 );
	}

	protected void incrementProgressBar( int increment )
	{
		myProgressBarCurrent += increment;
		displayProgressBar( myProgressBarCurrent );
	}
	
	protected void displayProgressBar()
	{
		displayProgressBar( myProgressBarCurrent );
	}

	protected void displayProgressBar( int value )
	{
		// If there are no changes to the progress bar value or message don't re-display it again.
		if ( value == myProgressBarLastValue && myProgressBarMsg == myProgressBarLastMsg )
			return;

		System.DateTime current = System.DateTime.Now;
		System.TimeSpan delta = current - myProgressBarStartTime;
		
		// This delay for displaying the progress bar is so the bar won't flicker for really quick updates
		// (less than a few seconds). Also, when we do show the progress bar the focus of the current 
		// inspector control is lost.
		if ( delta.TotalSeconds < HAPI_Constants.HAPI_SEC_BEFORE_PROGRESS_BAR_SHOW )
		{
			EditorUtility.ClearProgressBar();
			return;
		}
		
		myProgressBarJustUsed = true;
		
		myProgressBarCurrent = value;
		string message = "";
		if ( delta.Hours > 0 )
			message = delta.Hours + "h " + delta.Minutes + "m " + delta.Seconds + "s - " + myProgressBarMsg;
		else if ( delta.Minutes > 0 )
			message = delta.Minutes + "m " + delta.Seconds + "s - " + myProgressBarMsg;
		else if ( delta.Seconds > 0 )
			message = delta.Seconds + "s - " + myProgressBarMsg;
		else
			message = myProgressBarMsg;

		bool result = 
			!EditorUtility.DisplayCancelableProgressBar( 
				myProgressBarTitle, message, Mathf.InverseLerp( 0, myProgressBarTotal, myProgressBarCurrent ) );
		
		if ( !result )
		{
			myProgressBarLastValue = -1;
			myProgressBarLastMsg = "";
			HAPI_Host.interrupt();
			throw new HAPI_ErrorProgressCancelled();
		}
		else
		{
			myProgressBarLastValue = value;
			myProgressBarLastMsg = myProgressBarMsg;
		}
	}
	
	public bool hasProgressBarBeenUsed()
	{
		return myProgressBarJustUsed;
	}
	
	protected void clearProgressBar()
	{
		myProgressBarJustUsed = false;
		myProgressBarCurrent = 0;
		EditorUtility.ClearProgressBar();
	}
	
	protected bool				myProgressBarJustUsed;
	protected System.DateTime	myProgressBarStartTime;
	protected int				myProgressBarTotal; // Used for the progress bar.
	protected int				myProgressBarCurrent;
	protected string			myProgressBarTitle;
	protected string			myProgressBarMsg;
	
	// Used to reduce the update frequency of the progress bar so it doesn't flicker.
	private int					myProgressBarLastValue;
	private string				myProgressBarLastMsg;
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Serialized Data
	
	[SerializeField]
	protected byte[] 			myPreset;
	[SerializeField]
	protected HAPI_AssetInfo	myAssetInfo;
	[SerializeField]
	protected int				myAssetSubType;
}
