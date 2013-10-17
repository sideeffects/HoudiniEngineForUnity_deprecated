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

// Typedefs
using HAPI_StringHandle = System.Int32;
using HAPI_NodeId = System.Int32;

[ RequireComponent( typeof( HAPI_Control ) ) ]
public class HAPI_Parms : MonoBehaviour
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

	public HAPI_Control				prControl {						get { return myControl; } 
																	set { myControl = value; } }

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
	public HAPI_StringHandle[]		prParmStringValues {			get { return myParmStringValues; }
																	set { myParmStringValues = value; } }
	public HAPI_ParmChoiceInfo[]	prParmChoiceLists {				get { return myParmChoiceLists; } 
																	set { myParmChoiceLists = value; } }

	public int						prLastChangedParmId {			get { return myLastChangedParmId; } 
																	set { myLastChangedParmId = value; } }

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

		prFolderListSelections 			= new List< int >();
		prFolderListSelectionIds 		= new List< int >();
		prFolderListSelections.Add( 0 );
		prFolderListSelectionIds.Add( -1 );
	}

	public virtual void Update()
	{}

	// -------------------------------------------------------------------------------------------------------------

	public virtual HAPI_ParmInfo findParm( int id )
	{
		return myParmMap[ id ];
	}

	public virtual int findParm( string name )
	{
		if ( prParms == null )
			return -1;

		for ( int i = 0; i < prParms.Length; ++i )
			if ( prParms[ i ].name == name )
				return prParms[ i ].id;

		return -1;
	}

	public void cacheStringsFromHost()
	{
		// For each string parameter, cache the string from the host
		foreach ( HAPI_ParmInfo parm in prParms )
		{
			if ( parm.isString() )
			{
				myParmStrings[ parm.id ] = new string[ parm.size ];
				for ( int p = 0; p < parm.size; ++p )
					myParmStrings[ parm.id ][ p ] =
						HAPI_Host.getString( prParmStringValues[ parm.stringValuesIndex + p ] );
			}
		}
	}

	public void getParameterValues()
	{
		if ( prControl == null )
			return;
		if ( prControl.prAsset == null )
			return;

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

		// Get parameter float values.
		prParmFloatValues = new float[ prParmFloatValueCount ];
		Utility.getArray1Id( 
			prControl.prNodeId, HAPI_Host.getParmFloatValues, prParmFloatValues, prParmFloatValueCount );

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
	}

	public void removeMultiparmInstances( HAPI_ParmInfo multiparm, int num_instances )
	{
		if ( prControl == null )
			return;
		if ( prControl.prAsset == null )
			return;

		int[] values = new int[ 1 ];
		HAPI_Host.getParmIntValues( prControl.prNodeId, values, multiparm.intValuesIndex, 1);

		int last_instance = values[ 0 ];

		for ( int i = 0; i < num_instances; ++i )
			HAPI_Host.removeMultiparmInstance(
				prControl.prNodeId,
				multiparm.id, // The multiparm list
				last_instance - i );
	}

	public void appendMultiparmInstances( HAPI_ParmInfo multiparm, int num_instances )
	{
		if ( prControl == null )
			return;
		if ( prControl.prAsset == null )
			return;

		int[] values = new int[1];
		HAPI_Host.getParmIntValues( prControl.prNodeId, values, multiparm.intValuesIndex, 1 );

		int last_instance = values[ 0 ];

		for ( int i = 0; i < num_instances; ++i )
			HAPI_Host.insertMultiparmInstance(
				prControl.prNodeId,
				multiparm.id, // The multiparm list
				last_instance + i );
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

	private HAPI_ParmInfo 							myMultiparmInstancePos;
	private bool 									myToInsertInstance = false;
	private bool 									myToRemoveInstance = false;

	[SerializeField] private int					myLastChangedParmId;
	
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
}
