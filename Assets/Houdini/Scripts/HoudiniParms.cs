/*
 * PROPRIETARY INFORMATION.  This software is proprietary to
 * Side Effects Software Inc., and is not to be reproduced,
 * transmitted, or disclosed in any way without written permission.
 *
 * Produced by:
 *      Side Effects Software Inc
 *      123 Front Street West, Suite 1401
 *      Toronto, Ontario
 *      Canada   M5J 2M2
 *      416-504-9876
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

// Typedefs
using HAPI_StringHandle = System.Int32;
using HAPI_NodeId = System.Int32;

[ RequireComponent( typeof( HoudiniControl ) ) ]
public class HoudiniParms : MonoBehaviour
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
	
	// Please keep these in the same order and grouping as their initializations reset().

	// Assets -------------------------------------------------------------------------------------------------------

	public HoudiniControl				prControl {					get { return myControl; } 
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

	public HoudiniParmsUndoInfo 		prParmsUndoInfo {				get { return myParmsUndoInfo; }
																	private set { } }

	public List< int > 				prFolderListSelections {		get { return myFolderListSelections; } 
																	set { myFolderListSelections = value; } }
	public List< int > 				prFolderListSelectionIds {		get { return myFolderListSelectionIds; } 
																	set { myFolderListSelectionIds = value; } }

	public bool						prPostSerialization {			get { return myPostSerialization; }
																	set { myPostSerialization = value; } }
	public bool						prValuesEqualToHoudini {		get { return myValuesEqualToHoudini; }
																	set { myValuesEqualToHoudini = value; } }

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods
	
	public HoudiniParms() 
	{
		reset();
	}
	
	~HoudiniParms() 
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
	{}
	
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

		myPostSerialization = true;
		myValuesEqualToHoudini = false;
	}

#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
	public virtual void Update()
	{}
#endif // ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )

	// -------------------------------------------------------------------------------------------------------------

	public virtual HAPI_ParmInfo findParm( int id )
	{
		return myParmMap[ id ];
	}

	public virtual HAPI_ParmInfo findParm( string name )
	{
		if ( prParms == null )
		{
			throw new HoudiniErrorNotFound( "Parameters have not been initialized!" );
		}

		for ( int i = 0; i < prParms.Length; ++i )
			if ( prParms[ i ].name == name )
				return prParms[ i ];

		throw new HoudiniErrorNotFound( "Parameter with the name " + name + " does not exist!" );
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
					string string_value = HoudiniHost.getString( prParmStringValues[ values_index ] );

					myParmStrings[ parm.id ][ p ] = string_value;
					myParmsUndoInfo.parmStringValues[ values_index ] = string_value;
				}
			}
		}
	}

	public bool areValuesEqualToHoudini()
	{
		if ( prControl == null )
			return false;
		if ( prControl.prAsset == null )
			return false;
		if ( prControl.prAssetId < 0 )
			return false;

		// Get the node info again
		HAPI_NodeInfo node_info	= HoudiniHost.getNodeInfo( prControl.prNodeId );

		if ( prParmCount != node_info.parmCount )
			return false;
		if ( prParmIntValueCount != node_info.parmIntValueCount )
			return false;
		if ( prParmFloatValueCount != node_info.parmFloatValueCount )
			return false;
		if ( prParmStringValueCount != node_info.parmStringValueCount )
			return false;
		if ( prParmChoiceCount != node_info.parmChoiceCount )
			return false;

		// Get parameter int values.
		int[] houdini_int_values = new int[ prParmIntValueCount ];
		HoudiniAssetUtility.getArray1Id( 
			prControl.prNodeId, HoudiniHost.getParmIntValues, houdini_int_values, prParmIntValueCount );
		if ( prParmIntValues.Length != houdini_int_values.Length )
			return false;
		for ( int i = 0; i < prParmIntValueCount; ++i )
			if ( !prParmIntValues[ i ].Equals( houdini_int_values[ i ] ) )
				return false;

		// Get parameter float values.
		float[] houdini_float_values = new float[ prParmFloatValueCount ];
		HoudiniAssetUtility.getArray1Id( 
			prControl.prNodeId, HoudiniHost.getParmFloatValues, houdini_float_values, prParmFloatValueCount );
		if ( prParmFloatValues.Length != houdini_float_values.Length )
			return false;
		for ( int i = 0; i < prParmFloatValueCount; ++i )
			if ( !prParmFloatValues[ i ].Equals( houdini_float_values[ i ] ) )
				return false;

		// Get parameter string (handle) values.
		int[] houdini_string_values = new int[ prParmStringValueCount ];
		HoudiniAssetUtility.getArray1Id( 
			prControl.prNodeId, HoudiniHost.getParmStringValues, houdini_string_values, prParmStringValueCount );
		if ( prParmStringValues.Length != houdini_string_values.Length )
			return false;
		for ( int i = 0; i < prParmStringValueCount; ++i )
			if ( !HoudiniHost.getString( prParmStringValues[ i ] ).Equals(
				HoudiniHost.getString( houdini_string_values[ i ] ) ) )
				return false;

		return true;
	}

	public void getParameterValues()
	{
		if ( prControl == null )
			return;
		if ( prControl.prAsset == null )
			return;
		if ( prControl.prAssetId < 0 )
			return;

		if ( myPostSerialization )
			myValuesEqualToHoudini = areValuesEqualToHoudini();
		else
			myValuesEqualToHoudini = true;

		// Create undo info if it hasn't been created already
		if ( myParmsUndoInfo == null )
			myParmsUndoInfo = ScriptableObject.CreateInstance< HoudiniParmsUndoInfo >();

		// Get the node info again
		HAPI_NodeInfo node_info	= HoudiniHost.getNodeInfo( prControl.prNodeId );

		prParmCount 			= node_info.parmCount;
		prParmIntValueCount		= node_info.parmIntValueCount;
		prParmFloatValueCount	= node_info.parmFloatValueCount;
		prParmStringValueCount	= node_info.parmStringValueCount;
		prParmChoiceCount		= node_info.parmChoiceCount;

		// Get all parameters.
		prParms = new HAPI_ParmInfo[ prParmCount ];
		HoudiniAssetUtility.getArray1Id( prControl.prNodeId, HoudiniHost.getParameters, prParms, prParmCount );

		// Get parameter int values.
		prParmIntValues = new int[ prParmIntValueCount ];
		HoudiniAssetUtility.getArray1Id( 
			prControl.prNodeId, HoudiniHost.getParmIntValues, prParmIntValues, prParmIntValueCount );

		myParmsUndoInfo.parmIntValues = new int[ prParmIntValueCount ];
		Array.Copy( prParmIntValues, myParmsUndoInfo.parmIntValues, prParmIntValueCount );

		// Get parameter float values.
		prParmFloatValues = new float[ prParmFloatValueCount ];
		HoudiniAssetUtility.getArray1Id( 
			prControl.prNodeId, HoudiniHost.getParmFloatValues, prParmFloatValues, prParmFloatValueCount );

		myParmsUndoInfo.parmFloatValues = new float[ prParmFloatValueCount ];
		Array.Copy( prParmFloatValues, myParmsUndoInfo.parmFloatValues, prParmFloatValueCount );

		// Get parameter string (handle) values.
		prParmStringValues = new int[ prParmStringValueCount ];
		HoudiniAssetUtility.getArray1Id( 
			prControl.prNodeId, HoudiniHost.getParmStringValues, prParmStringValues, prParmStringValueCount );

		// Get parameter choice lists.
		prParmChoiceLists = new HAPI_ParmChoiceInfo[ prParmChoiceCount ];
		HoudiniAssetUtility.getArray1Id( 
			prControl.prNodeId, HoudiniHost.getParmChoiceLists, prParmChoiceLists, prParmChoiceCount );

		// Build the map of parm id -> parm
		for ( int i = 0; i < prParms.Length; ++i )
			myParmMap[ prParms[ i ].id ] = prParms[ i ];

		cacheStringsFromHost();

		// Go through parameters and set index map and multiparm map for undo info
		myParmsUndoInfo.parmNames.Clear();
		myParmsUndoInfo.parmIndices.Clear();

		foreach ( HAPI_ParmInfo parm in prParms )
		{
			// Need to check the index values are greater or equal to 0
			// for now because there is a bug where some parameters are
			// being set to have an integer parameter type, a size of 
			// zero, and an index value of -1
			if ( parm.isInt() && parm.intValuesIndex >= 0 )
				myParmsUndoInfo.parmIndices.Add( parm.intValuesIndex );
			else if ( parm.isFloat() && parm.floatValuesIndex >= 0 )
				myParmsUndoInfo.parmIndices.Add( parm.floatValuesIndex );
			else if ( parm.isString() && parm.stringValuesIndex >= 0 )
				myParmsUndoInfo.parmIndices.Add( parm.stringValuesIndex );
			else
				continue;

			myParmsUndoInfo.parmNames.Add( parm.name );
		}

#if UNITY_EDITOR
		// Set which parameter values have been overridden (only needed for a prefab instance)
		if ( prControl && prControl.isPrefabInstance() && gameObject.GetComponent< HoudiniAsset >() != null )
		{
			HoudiniAsset prefab_asset = prControl.prAsset.getParentPrefabAsset();
			if ( prefab_asset && prefab_asset.prParms != null && 
				prefab_asset.prParms.prParms != null && 
				!prefab_asset.isApplyingChangesToPrefab() )
			{
				// loop through parameter values and determine which ones have been
				// overridden (ie. changed from corresponding parameter value on prefab)
				for ( int i = 0; i < prParms.Length; ++i )
				{
					myOverriddenParmsMap[ prParms[ i ].id ] = !isParmSameInPrefab( prParms[ i ].id, prefab_asset.prParms );
				}
			}

			// This tells Unity that parameter values have been overridden for this prefab instance
			PrefabUtility.RecordPrefabInstancePropertyModifications( this );
		}
#endif // UNITY_EDITOR
	}
	
	// Checks if the parameter with the given id parm_id represents the same parameter 
	// with the same value in this set of parameters and another set of parameters parmsB.
	public bool isParmSameInPrefab( int parm_id, HoudiniParms parmsB )
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
			HoudiniHost.removeMultiparmInstance(
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
			HoudiniHost.insertMultiparmInstance(
				prControl.prNodeId,
				multiparm.id, // The multiparm list
				insert_position + i );
	}

	public void setChangedParametersIntoHost()
	{
		if ( prControl == null )
			return;
		if ( prControl.prAsset == null )
			return;

		setChangedParameterIntoHost( prLastChangedParmId );

		if ( myToInsertInstance )
			HoudiniHost.insertMultiparmInstance(
				prControl.prNodeId,
				myMultiparmInstancePos.parentId, // The multiparm list
				myMultiparmInstancePos.instanceNum );

		if ( myToRemoveInstance )
			HoudiniHost.removeMultiparmInstance(
				prControl.prNodeId,
				myMultiparmInstancePos.parentId, // The multiparm list
				myMultiparmInstancePos.instanceNum );

		if ( myToRemoveInstance || myToInsertInstance )
			getParameterValues();

		myToInsertInstance = false;
		myToRemoveInstance = false;
		prLastChangedParmId = HoudiniConstants.HAPI_INVALID_PARM_ID;
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
			HoudiniHost.getParmIntValues( prControl.prNodeId, values, parm.intValuesIndex, 1);

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
			HoudiniHost.setParmFloatValues( prControl.prNodeId, values, parm.floatValuesIndex, parm.size );
		}
		else if ( parm.isInt() && (HAPI_ParmType) parm.type != HAPI_ParmType.HAPI_PARMTYPE_MULTIPARMLIST )
		{
			int[] values = new int[ parm.size ];
			Array.Copy( prParmIntValues, parm.intValuesIndex, values, 0, parm.size );
			HoudiniHost.setParmIntValues( prControl.prNodeId, values, parm.intValuesIndex, parm.size );
		}
		else if ( parm.isString() )
		{
			for ( int p = 0; p < myParmStrings[ parm.id ].Length; ++p )
				HoudiniHost.setParmStringValue( prControl.prNodeId, myParmStrings[ parm.id ][ p ], parm.id, p );
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Serialized Private Data
	
	// Assets -------------------------------------------------------------------------------------------------------

	[SerializeField] private HoudiniControl			myControl;

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

	// A mapping from parm id to a boolean indicating whether the value 
	// of the parameter with that id has been changed from the value of 
	// the same parameter in the associated prefab. These values are only 
	// used if these parameters are the parameters of a prefab instance.
	private Dictionary< int, bool > 				myOverriddenParmsMap = new Dictionary< int, bool >();

	private HAPI_ParmInfo 							myMultiparmInstancePos;
	private bool 									myToInsertInstance = false;
	private bool 									myToRemoveInstance = false;

	[SerializeField] private int					myLastChangedParmId;

	[SerializeField] private HoudiniParmsUndoInfo		myParmsUndoInfo;

	// Indices of the currently selected folders in the Inspector.
	// A 1:1 mapping with myFolderListSelectionIds.
	[SerializeField] private List< int > 			myFolderListSelections;

	// Parameter ids of the currently selected folders in the Inspector.
	// A 1:1 mapping with myFolderListSelections.
	[SerializeField] private List< int > 			myFolderListSelectionIds;

	// Control ------------------------------------------------------------------------------------------------------

	private bool myPostSerialization;

	// This variable keeps track of whether the parameter values were the same
	// as those in the Houdini instantiate when the parms were deserialized
	// or if the values were different. The only time there will be a difference
	// is when going out of playmode having made changes to parameters during
	// playmode. This is because Unity restores the parameter values from
	// before playmode by design (so what you do during playmode SHOULD be
	// lost). In this case we need to do an extra cook when we go out 
	// of playmode with the "old" parameter values. In all other cases this
	// value is true.
	private bool myValuesEqualToHoudini;
}
