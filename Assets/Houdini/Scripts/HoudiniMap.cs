/*
* Copyright (c) <2017> Side Effects Software Inc.
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*
* Produced by:
*      Side Effects Software Inc
*      123 Front Street West, Suite 1401
*      Toronto, Ontario
*      Canada   M5J 2M2
*      416-504-9876
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
