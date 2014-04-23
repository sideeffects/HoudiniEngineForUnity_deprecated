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
using System;
using System.Collections;

[Serializable]
public class HoudiniAssetAccessor
{
	public enum ParmType
	{
		INVALID = -1,
		INT,
		FLOAT,
		STRING,
		IMMUTABLE
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties

	public string prName {	get { return myAsset.prAssetName; }
							private set {} }

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods

	// Static methods used to get HAPI_AssetAccessor(s) -------------------------------------------------------------

	public static HoudiniAssetAccessor[] getAllAssetAccessors()
	{
		HoudiniAsset[] assets = UnityEngine.Object.FindObjectsOfType( typeof( HoudiniAsset ) ) as HoudiniAsset[];
		HoudiniAssetAccessor[] accessors = new HoudiniAssetAccessor[ assets.Length ];

		for ( int i = 0; i < assets.Length; i++ )
		{
			accessors[ i ] = new HoudiniAssetAccessor( assets[ i ] );
		}

		return accessors;
	}

	public static HoudiniAssetAccessor getAssetAccessor( GameObject gameObject )
	{
		HoudiniAsset asset = gameObject.GetComponent< HoudiniAsset >();
		if ( asset )
		{
			return new HoudiniAssetAccessor( asset );
		}

		return null;
	}

	// Parameter related methods ------------------------------------------------------------------------------------

	public string[] getParameters()
	{
		HAPI_ParmInfo[] parm_infos = myAssetParms.prParms;
		string[] names = new string[ parm_infos.Length ];
		
		for ( int i = 0; i < parm_infos.Length; i++ )
		{
			names[ i ] = parm_infos[ i ].name;
		}
		
		return names;
	}

	public ParmType getParmType( string name )
	{
		HAPI_ParmInfo parm_info = myAssetParms.findParm( name );

		if ( parm_info.isInt() )
			return ParmType.INT;
		if ( parm_info.isFloat() )
			return ParmType.FLOAT;
		if ( parm_info.isString() )
			return ParmType.STRING;
		if ( parm_info.isNonValue() )
			return ParmType.IMMUTABLE;

		return ParmType.INVALID;
	}

	public int getParmSize( string name )
	{
		HAPI_ParmInfo parm_info = myAssetParms.findParm( name );

		return parm_info.size;
	}

	public int getParmIntValue( string name, int index )
	{

		HAPI_ParmInfo parm_info = myAssetParms.findParm( name );
		
		if ( parm_info.isInt() )
			return myAssetParms.prParmIntValues[ parm_info.intValuesIndex + index ];
		
		throw new HoudiniErrorInvalidArgument( name + " is not an int!" );
	}
	
	public float getParmFloatValue( string name, int index )
	{
		HAPI_ParmInfo parm_info = myAssetParms.findParm( name );
		
		if ( parm_info.isFloat() )
			return myAssetParms.prParmFloatValues[ parm_info.floatValuesIndex + index ];

		throw new HoudiniErrorInvalidArgument( name + " is not a float!" );
	}
	
	public string getParmStringValue( string name, int index )
	{
		HAPI_ParmInfo parm_info = myAssetParms.findParm( name );
		
		if ( parm_info.isString() )
			return myAssetParms.getParmStrings( parm_info )[ index ];

		throw new HoudiniErrorInvalidArgument( name + " is not a string!" );
	}
	
	public void setParmIntValue( string name, int index, int value )
	{
		HAPI_ParmInfo parm_info = myAssetParms.findParm( name );
		
		if ( !parm_info.isInt() )
			throw new HoudiniErrorInvalidArgument( name + " is not an int!" );
		
		int values_index = parm_info.intValuesIndex + index;
		int[] int_value = { value };
		
		HoudiniHost.setParmIntValues( myAsset.prNodeId, int_value, values_index, 1 );
		
		myAsset.buildClientSide();
	}
	
	public void setParmFloatValue( string name, int index, float value )
	{
		HAPI_ParmInfo parm_info = myAssetParms.findParm( name );
		
		if ( !parm_info.isFloat() )
			throw new HoudiniErrorInvalidArgument( name + " is not a float!" );
		
		int values_index = parm_info.floatValuesIndex + index;
		float[] float_value = { value };
		
		HoudiniHost.setParmFloatValues( myAsset.prNodeId, float_value, values_index, 1 );
		
		myAsset.buildClientSide();
	}
	
	public void setParmStringValue( string name, int index, string value )
	{
		HAPI_ParmInfo parm_info = myAssetParms.findParm( name );
		
		if ( !parm_info.isString() )
			throw new HoudiniErrorInvalidArgument( name + " is not a string!" );
		
		HoudiniHost.setParmStringValue( myAsset.prNodeId, value, parm_info.id, index );
		
		myAsset.buildClientSide();
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private Methods

	private HoudiniAssetAccessor()
	{
	}

	private HoudiniAssetAccessor( HoudiniAsset asset )
	{
		myAsset = asset;
		myAssetParms = asset.prParms;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Serialized Private Data

	[SerializeField] private HoudiniAsset myAsset;
	[SerializeField] private HoudiniParms myAssetParms;

}

