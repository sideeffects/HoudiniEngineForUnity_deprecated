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
using System.Collections;
using System.Collections.Generic;

public class HoudiniInstancerPersistentData : ScriptableObject
{
	public string instancerName;
	public List< string > uniqueNames;
	public List< GameObject > objsToInstantiate;
	public List< int > numObjsToInstantiate;
	public List< int > variationChoice;
	public List< bool > recalculateVariations;
	public bool showInstancerGUI = true;

	public Vector3 rotationalOffset = new Vector3( 0, 0, 0 );
	public Vector3 scaleOffset = new Vector3( 1, 1, 1 );

	public List< HoudiniInstancerOverrideInfo > overriddenInstances; 

	public HoudiniInstancerPersistentData()
	{
		uniqueNames = new List< string >();
		objsToInstantiate = new List< GameObject > ();
		overriddenInstances = new List< HoudiniInstancerOverrideInfo >();
		numObjsToInstantiate = new List< int >();
		variationChoice = new List< int >();
		recalculateVariations = new List< bool >();
	}

	public int baseIndex( string name )
	{
		int index = 0;
		for ( int ii = 0; ii < uniqueNames.Count; ii++ )
		{
			if ( uniqueNames[ ii ] == name )
			{
				return index;
			}
			index += numObjsToInstantiate[ ii ];
		}
		return -1;
	}

	public int baseIndex( int logical_index )
	{
		if ( logical_index >= uniqueNames.Count )
			return -1;

		int index = 0;
		for ( int ii = 0; ii < logical_index; ii++ )
		{
			index += numObjsToInstantiate[ ii ];
		}
		return index;
	}

	public GameObject getUserObjToInstantiateFromName( string name, int point_index )
	{
		for ( int ii = 0; ii < uniqueNames.Count; ii++ )
		{
			if ( uniqueNames[ ii ] == name )
			{
				int base_index = baseIndex( name );
				if ( point_index >= variationChoice.Count )
				{
					Debug.LogError(
						"point_index out of range in " +
						"HAPI_InstancerPersistentData::getUserObjToInstantiateFromName" );
					return null;
				}

				if ( recalculateVariations[ ii ] || 
					variationChoice[ point_index ] < 0 ||
					variationChoice[ point_index ] >= numObjsToInstantiate[ ii ] )
				{
					int random_index = UnityEngine.Random.Range( 0, numObjsToInstantiate[ ii ] );
					variationChoice[ point_index ] = random_index;
				}

				int variation_choice = base_index + variationChoice[ point_index ];
				return objsToInstantiate[ variation_choice ];
			}
		}
		return null;
	}
}

