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
using System.Collections.Generic;

[ System.Serializable ]
public class HAPI_Map< KEY, VALUE > : ScriptableObject where KEY : IEquatable< KEY > {

	public HAPI_Map( string name )
	{
		myName = name;
		myKeys = new List< KEY >();
		myValues = new List< VALUE >();
	}

	public void set( KEY key, VALUE value )
	{
		int index = myKeys.FindIndex( delegate( KEY k ) { return k.Equals( key ); } );
		if ( index >= 0 )
			myValues[ index ] = value;
	}

	public void add( KEY key, VALUE value )
	{
		if ( myKeys.Contains( key ) )
			set( key, value );
		else
		{
			myKeys.Add( key );
			myValues.Add( value );
			if ( myKeys.Count != myValues.Count )
				Debug.LogError( myName + " dictionary has missmatched key/value pairs." );
		}
	}

	public void remove( KEY key )
	{
		int index = myKeys.FindIndex( delegate( KEY k ) { return k.Equals( key ); } );
		if ( index >= 0 )
		{
			myKeys.RemoveAt( index );
			myValues.RemoveAt( index );
			if ( myKeys.Count != myValues.Count )
				Debug.LogError( myName + " dictionary has missmatched key/value pairs." );
		}
	}

	public bool contains( KEY key )
	{
		return myKeys.Contains( key );
	}

	public VALUE get( KEY key )
	{
		int index = myKeys.FindIndex( delegate( KEY k ) { return k.Equals( key ); } );
		if ( index >= 0 )
			return myValues[ index ];
		else
			throw new HAPI.HAPI_ErrorNotFound();
	}

	[SerializeField] private string myName;
	[SerializeField] private List< KEY > myKeys;
	[SerializeField] private List< VALUE > myValues;

}
