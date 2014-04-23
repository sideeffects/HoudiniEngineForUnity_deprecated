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
public class HoudiniMap< KEY, VALUE > : ScriptableObject where KEY : IEquatable< KEY >
{

	public HoudiniMap( string name )
	{
		myName = name;
		myKeys = new List< KEY >();
		myValues = new List< VALUE >();
	}

	public virtual void set( KEY key, VALUE value )
	{
		int index = myKeys.FindIndex( delegate( KEY k ) { return k.Equals( key ); } );
		if ( index >= 0 )
			myValues[ index ] = value;
		else
			add( key, value );
	}

	public virtual void add( KEY key, VALUE value )
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

	public virtual void remove( KEY key )
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

	public virtual bool contains( KEY key )
	{
		return myKeys.Contains( key );
	}

	public virtual bool isEmpty()
	{
		if ( myKeys.Count != myValues.Count )
			Debug.LogError( myName + " dictionary has missmatched key/value pairs." );
		return myKeys.Count == 0;
	}

	public virtual VALUE get( KEY key )
	{
		if ( myKeys.Count != myValues.Count )
		{
			Debug.LogError( myName + " dictionary has unserializable values." );
			throw new HoudiniErrorNotFound();
		}
		int index = myKeys.FindIndex( delegate( KEY k ) { return k.Equals( key ); } );
		if ( index >= 0 )
			return myValues[ index ];
		else
			throw new HoudiniErrorNotFound();
	}

	[SerializeField] protected string myName;
	[SerializeField] protected List< KEY > myKeys;
	[SerializeField] protected List< VALUE > myValues;

}
