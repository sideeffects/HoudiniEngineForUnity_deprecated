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
using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using HAPI;
using Utility = HAPI_AssetUtility;

// Typedefs
using HAPI_StringHandle = System.Int32;
using HAPI_NodeId = System.Int32;

[ RequireComponent( typeof( HAPI_Control ) ) ]
public class HAPI_Parms : MonoBehaviour
{
#if UNITY_EDITOR
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

	public HAPI_Control				prControl {						get { return myControl; } 
																	set { myControl = value; } }

	// Parameters ---------------------------------------------------------------------------------------------------

	public bool						prEditable {					get { return myEditable; }
																	set { myEditable = value; } }

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
	public HAPI_StringHandle[]		prParmStringValues {			get { return myParmStringValues; }
																	set { myParmStringValues = value; } }
	public HAPI_ParmChoiceInfo[]	prParmChoiceLists {				get { return myParmChoiceLists; } 
																	set { myParmChoiceLists = value; } }

	public int						prLastChangedParmId {			get { return myLastChangedParmId; } 
																	set { myLastChangedParmId = value; } }

	public HAPI_ParmsUndoInfo 		prParmsUndoInfo {				get { return myParmsUndoInfo; }
																	private set { } }				
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

	public bool						prPostSerialization {			get { return myPostSerialization; }
																	set { myPostSerialization = value; } }

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods
	
	public HAPI_Parms() 
	{
		reset();
	}
	
	~HAPI_Parms() 
	{}
	
	public void removeMultiparmInstance( HAPI_ParmInfo parm )
	{
		myMultiparmInstancePos = parm;
		myToRemoveInstance = true;
	}

	public void insertMultiparmInstance( HAPI_ParmInfo parm )
	{
		myMultiparmInstancePos = parm;
		myToInsertInstance = true;
	}

	// This will retrieve the cached copy of the string
	public string[] getParmStrings( HAPI_ParmInfo parm )
	{
		return myParmStrings[ parm.id ];
	}

	// Set into dictionary to later be set into the host
	public void setParmStrings( HAPI_ParmInfo parm, string[] strings )
	{
		myParmStrings[ parm.id ] = strings;
	}
	
	public virtual void OnDestroy()
	{

	}

	public virtual void OnEnable()
	{

	}
	
	public void reset()
	{
		// Please keep these in the same order and grouping as their declarations at the top.

		// Assets -------------------------------------------------------------------------------------------------------

		prControl						= null;

		// Parameters -----------------------------------------------------------------------------------------------

		prEditable						= true;

		prParmCount 					= 0;
		prParmIntValueCount 			= 0;
		prParmFloatValueCount 			= 0;
		prParmStringValueCount 			= 0;
		prParmChoiceCount 				= 0;

		prParms 						= null;
		prParmIntValues 				= new int[ 0 ];
		prParmFloatValues 				= new float[ 0 ];
		prParmStringValues 				= new HAPI_StringHandle[ 0 ];
		prParmChoiceLists 				= new HAPI_ParmChoiceInfo[ 0 ];

		prLastChangedParmId 			= -1;

		myParmsUndoInfo					= null;

		prFolderListSelections 			= new List< int >();
		prFolderListSelectionIds 		= new List< int >();
		prFolderListSelections.Add( 0 );
		prFolderListSelectionIds.Add( -1 );

		// Control -------------------------------------------------------------------------------------------------

		myPostSerialization				= true;
	}

	public virtual void Update()
	{}

	// -------------------------------------------------------------------------------------------------------------

	public virtual HAPI_ParmInfo findParm( int id )
	{
		return myParmMap[ id ];
	}

	public virtual HAPI_ParmInfo findParm( string name )
	{
		if ( prParms == null )
		{
			throw new HAPI_ErrorNotFound( "Parameters have not been initialized!" );
		}

		for ( int i = 0; i < prParms.Length; ++i )
			if ( prParms[ i ].name == name )
				return prParms[ i ];

		throw new HAPI_ErrorNotFound( "Parameter with the name " + name + " does not exist!" );
	}
	
	public bool isParmOverridden( int parm_id )
	{
		if ( myOverriddenParmsMap.ContainsKey( parm_id ) )
		{
			return myOverriddenParmsMap[ parm_id ];
		}
		return false;
	}

	public void cacheStringsFromHost()
	{
		myParmsUndoInfo.parmStringValues = new string[ prParmStringValueCount ];

		// For each string parameter, cache the string from the host
		foreach ( HAPI_ParmInfo parm in prParms )
		{
			if ( parm.isString() )
			{
				myParmStrings[ parm.id ] = new string[ parm.size ];
				for ( int p = 0; p < parm.size; ++p )
				{
					int values_index = parm.stringValuesIndex + p;
					string string_value = HAPI_Host.getString( prParmStringValues[ values_index ] );

					myParmStrings[ parm.id ][ p ] = string_value;
					myParmsUndoInfo.parmStringValues[ values_index ] = string_value;
				}
			}
		}
	}

	public void getParameterValues()
	{
		if ( prControl == null )
			return;
		if ( prControl.prAsset == null )
			return;
		if ( prControl.prAssetId < 0 )
			return;

		// Create undo info if it hasn't been created already
		if ( myParmsUndoInfo == null )
			myParmsUndoInfo = ScriptableObject.CreateInstance< HAPI_ParmsUndoInfo >();

		// Get the node info again
		HAPI_NodeInfo node_info	= HAPI_Host.getNodeInfo( prControl.prNodeId );

		prParmCount 			= node_info.parmCount;
		prParmIntValueCount		= node_info.parmIntValueCount;
		prParmFloatValueCount	= node_info.parmFloatValueCount;
		prParmStringValueCount	= node_info.parmStringValueCount;
		prParmChoiceCount		= node_info.parmChoiceCount;

		// We need to get the parameter values again because they could have been
		// changed by a script.

		// Get all parameters.
		prParms = new HAPI_ParmInfo[ prParmCount ];
		Utility.getArray1Id( prControl.prNodeId, HAPI_Host.getParameters, prParms, prParmCount );

		// Get parameter int values.
		prParmIntValues = new int[ prParmIntValueCount ];
		Utility.getArray1Id( 
			prControl.prNodeId, HAPI_Host.getParmIntValues, prParmIntValues, prParmIntValueCount );

		myParmsUndoInfo.parmIntValues = new int[ prParmIntValueCount ];
		Array.Copy( prParmIntValues, myParmsUndoInfo.parmIntValues, prParmIntValueCount );

		// Get parameter float values.
		prParmFloatValues = new float[ prParmFloatValueCount ];
		Utility.getArray1Id( 
			prControl.prNodeId, HAPI_Host.getParmFloatValues, prParmFloatValues, prParmFloatValueCount );

		myParmsUndoInfo.parmFloatValues = new float[ prParmFloatValueCount ];
		Array.Copy( prParmFloatValues, myParmsUndoInfo.parmFloatValues, prParmFloatValueCount );

		// Get parameter string (handle) values.
		prParmStringValues = new int[ prParmStringValueCount ];
		Utility.getArray1Id( 
			prControl.prNodeId, HAPI_Host.getParmStringValues, prParmStringValues, prParmStringValueCount );

		// Get parameter choice lists.
		prParmChoiceLists = new HAPI_ParmChoiceInfo[ prParmChoiceCount ];
		Utility.getArray1Id( 
			prControl.prNodeId, HAPI_Host.getParmChoiceLists, prParmChoiceLists, prParmChoiceCount );

		// Build the map of parm id -> parm
		for ( int i = 0; i < prParms.Length; ++i )
			myParmMap[ prParms[ i ].id ] = prParms[ i ];

		cacheStringsFromHost();
		
		// Set which parameter values have been overridden (only needed for a prefab instance)
		if ( prControl && prControl.isPrefabInstance() && gameObject.GetComponent< HAPI_Asset >() != null )
		{
			HAPI_Asset prefab_asset = prControl.prAsset.getParentPrefabAsset();
			if ( prefab_asset )
			{
				// if prefab has not been built yet then build it
				if ( !HAPI_Host.isAssetValid( prefab_asset.prAssetId, prefab_asset.prAssetValidationId ) )
				{
					prefab_asset.prAssetId = -1;
					prefab_asset.build( true,	// reload_asset
								   		true,	// unload_asset_first
								   		true,	// serializatin_recovery_only
								   		false,	// force_reconnect
								   		false,	// cook_downstream_assets
								   		false	// use_delay_for_progress_bar
						 		 	  );
					EditorUtility.SetDirty( prefab_asset );
				}
				// if prefab has not been reloaded after play mode change yet then 
				// reload it to get its parameters back
				else if ( prefab_asset.prReloadPrefabOnPlaymodeChange )
				{
					prefab_asset.prReloadPrefabOnPlaymodeChange = false;
					prefab_asset.build(	false,	// reload_asset
					              		false,	// unload_asset_first
					              		true,	// serializatin_recovery_only
					              		false,	// force_reconnect
					              		false,	// cook_downstream_assets
					              		false	// use_delay_for_progress_bar
					              	  );
				}
				
				if ( prefab_asset.prParms.prParms != null && !prefab_asset.isApplyingChangesToPrefab() )
				{
					// loop through parameter values and determine which ones have been
					// overridden (ie. changed from corresponding parameter value on prefab)
					for ( int i = 0; i < prParms.Length; ++i )
					{
						myOverriddenParmsMap[ prParms[ i ].id ] = !isParmSameInPrefab( prParms[ i ].id, prefab_asset.prParms );
					}
				}
			}
		}
	}
	
	// Checks if the parameter with the given id parm_id represents the same parameter 
	// with the same value in this set of parameters and another set of parameters parmsB.
	public bool isParmSameInPrefab( int parm_id, HAPI_Parms parmsB )
	{
		HAPI_ParmInfo parm_infoA = findParm( parm_id );
		HAPI_ParmInfo parm_infoB = parmsB.findParm( parm_id );
		
		if ( parm_infoA.GetType() != parm_infoB.GetType() ||
			 parm_infoA.size != parm_infoB.size ||
			 parm_infoA.name != parm_infoB.name || 
			 parm_infoA.label != parm_infoB.label )
		{
			Debug.LogError( "Parameter structure is different from prefab" );
			return false;
		}
		
		// only need to check type and size of one because already checked that
		// parameter infos have  type and size
		if ( parm_infoA.isFloat() )
		{
			for ( int ii = 0; ii < parm_infoA.size; ii++ )
			{
				float valueA = prParmFloatValues[ parm_infoA.floatValuesIndex + ii ];
				float valueB = parmsB.prParmFloatValues[ parm_infoB.floatValuesIndex + ii ];
				if ( valueA != valueB )
				{
					return false;
				}
			}
		}
		else if ( parm_infoB.isInt() )
		{
			for ( int ii = 0; ii < parm_infoA.size; ii++ )
			{
				int valueA = prParmIntValues[ parm_infoA.intValuesIndex + ii ];
				int valueB = parmsB.prParmIntValues[ parm_infoB.intValuesIndex + ii ];
				if ( valueA != valueB )
				{
					return false;
				}
			}
		}
		else if ( parm_infoB.isString() )
		{
			string[] valuesA = getParmStrings( parm_infoA );
			string[] valuesB = parmsB.getParmStrings( parm_infoB );
			
			if ( valuesA.Length != valuesB.Length )
			{
				return false;
			}
			
			for ( int ii = 0; ii < valuesA.Length; ii++ )
			{
				if ( valuesA[ ii ] != valuesB[ ii ] )
				{
					return false;
				}
			}
		}
		
	    return true;
	}

	public void removeMultiparmInstances( HAPI_ParmInfo multiparm, int num_instances )
	{
		if ( prControl == null )
			return;
		if ( prControl.prAsset == null )
			return;

		int first_removed_instance = multiparm.instanceCount - num_instances + multiparm.instanceStartOffset;

		for ( int i = 0; i < num_instances; ++i )
			HAPI_Host.removeMultiparmInstance(
				prControl.prNodeId,
				multiparm.id, // The multiparm list
				first_removed_instance );
	}

	public void appendMultiparmInstances( HAPI_ParmInfo multiparm, int num_instances )
	{
		if ( prControl == null )
			return;
		if ( prControl.prAsset == null )
			return;

		int insert_position = multiparm.instanceCount + multiparm.instanceStartOffset;

		for ( int i = 0; i < num_instances; ++i )
			HAPI_Host.insertMultiparmInstance(
				prControl.prNodeId,
				multiparm.id, // The multiparm list
				insert_position );
	}

	public void setChangedParametersIntoHost()
	{
		if ( prControl == null )
			return;
		if ( prControl.prAsset == null )
			return;

		setChangedParameterIntoHost( prLastChangedParmId );

		if ( myToInsertInstance )
			HAPI_Host.insertMultiparmInstance(
				prControl.prNodeId,
				myMultiparmInstancePos.parentId, // The multiparm list
				myMultiparmInstancePos.instanceNum );

		if ( myToRemoveInstance )
			HAPI_Host.removeMultiparmInstance(
				prControl.prNodeId,
				myMultiparmInstancePos.parentId, // The multiparm list
				myMultiparmInstancePos.instanceNum );

		if ( myToRemoveInstance || myToInsertInstance )
			getParameterValues();

		myToInsertInstance = false;
		myToRemoveInstance = false;
		prLastChangedParmId = HAPI_Constants.HAPI_INVALID_PARM_ID;
	}

	public void setChangedParameterIntoHost( int id )
	{
		if ( prControl == null )
			return;
		if ( prControl.prAsset == null )
			return;

		if ( id == -1 )
			return;

		HAPI_ParmInfo parm = myParmMap[ id ];
		if ( (HAPI_ParmType) parm.type == HAPI_ParmType.HAPI_PARMTYPE_MULTIPARMLIST )
		{
			int[] values = new int[ 1 ];
			HAPI_Host.getParmIntValues( prControl.prNodeId, values, parm.intValuesIndex, 1);

			int difference = prParmIntValues[ parm.intValuesIndex ] - values[ 0 ];
			if ( difference > 0 )
				appendMultiparmInstances( parm, difference );
			else if ( difference < 0 )
				removeMultiparmInstances( parm, -difference );

			getParameterValues();
		}
		else if ( parm.isFloat() )
		{
			float[] values = new float[ parm.size ];
			Array.Copy( prParmFloatValues, parm.floatValuesIndex, values, 0, parm.size );
			HAPI_Host.setParmFloatValues( prControl.prNodeId, values, parm.floatValuesIndex, parm.size );
		}
		else if ( parm.isInt() && (HAPI_ParmType) parm.type != HAPI_ParmType.HAPI_PARMTYPE_MULTIPARMLIST )
		{
			int[] values = new int[ parm.size ];
			Array.Copy( prParmIntValues, parm.intValuesIndex, values, 0, parm.size );
			HAPI_Host.setParmIntValues( prControl.prNodeId, values, parm.intValuesIndex, parm.size );
		}
		else if ( parm.isString() )
		{
			for ( int p = 0; p < myParmStrings[ parm.id ].Length; ++p )
				HAPI_Host.setParmStringValue( prControl.prNodeId, myParmStrings[ parm.id ][ p ], parm.id, p );
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Serialized Private Data
	
	// Assets -------------------------------------------------------------------------------------------------------

	[SerializeField] private HAPI_Control			myControl;

	// Parameters ---------------------------------------------------------------------------------------------------

	[SerializeField] private bool					myEditable;

	[SerializeField] private int 					myParmCount;
	[SerializeField] private int					myParmIntValueCount;
	[SerializeField] private int					myParmFloatValueCount;
	[SerializeField] private int					myprParmStringValueCoun;
	[SerializeField] private int					myprParmChoiceCount;
	
	[SerializeField] private HAPI_ParmInfo[]  		myParms;
	[SerializeField] private int[]					myParmIntValues;
	[SerializeField] private float[]				myParmFloatValues;
	[SerializeField] private HAPI_StringHandle[]	myParmStringValues;
	[SerializeField] private HAPI_ParmChoiceInfo[]	myParmChoiceLists;

	// A mapping from parm id to the parm's string values
	private Dictionary< int, string[] >  			myParmStrings = new Dictionary< int, string[] >();
	private Dictionary< int, HAPI_ParmInfo >		myParmMap = new Dictionary< int, HAPI_ParmInfo >();
	
	/// <summary>
	/// 	A mapping from parm id to a boolean indicating whether the value 
	/// 	of the parameter with that id has been changed from the value of 
	/// 	the same parameter in the associated prefab. These values are only 
	///     used if these parameters are the parameters of a prefab instance.
	/// </summary>
	private Dictionary< int, bool > 				myOverriddenParmsMap = new Dictionary< int, bool >();

	private HAPI_ParmInfo 							myMultiparmInstancePos;
	private bool 									myToInsertInstance = false;
	private bool 									myToRemoveInstance = false;

	[SerializeField] private int					myLastChangedParmId;

	[SerializeField] private HAPI_ParmsUndoInfo		myParmsUndoInfo;

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

	// Control ------------------------------------------------------------------------------------------------------

	private bool myPostSerialization;
#endif // UNITY_EDITOR
}
