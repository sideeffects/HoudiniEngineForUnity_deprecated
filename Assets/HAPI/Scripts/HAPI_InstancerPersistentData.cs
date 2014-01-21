using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using HAPI;


public class HAPI_InstancerPersistentData : ScriptableObject
{
	public string instancerName;
	public List< string > uniqueNames;
	public List< GameObject > objsToInstantiate;
	public List< int > numObjsToInstantiate;
	public bool showInstancerGUI = true;

	public List< HAPI_InstancerOverrideInfo > overriddenInstances; 

	public HAPI_InstancerPersistentData()
	{
		uniqueNames = new List< string >();
		objsToInstantiate = new List< GameObject > ();
		overriddenInstances = new List< HAPI_InstancerOverrideInfo >();
		numObjsToInstantiate = new List< int >();
	}

	public int baseIndex( string name )
	{
		int index = 0;
		for( int ii = 0; ii < uniqueNames.Count; ii++ )
		{
			if( uniqueNames[ ii ] == name )
			{
				return index;
			}
			index += numObjsToInstantiate[ ii ];
		}
		return -1;
	}

	public int baseIndex( int logical_index )
	{
		if( logical_index >= uniqueNames.Count )
			return -1;

		int index = 0;
		for( int ii = 0; ii < logical_index; ii++ )
		{
			index += numObjsToInstantiate[ ii ];
		}
		return index;
	}


	public GameObject getUserObjToInstantiateFromName( string name )
	{
		for( int ii = 0; ii < uniqueNames.Count; ii++ )
		{
			if( uniqueNames[ ii ] == name )
			{
				int base_index = baseIndex( name );
				int random_index = UnityEngine.Random.Range( base_index, base_index + numObjsToInstantiate[ ii ] );
				return objsToInstantiate[ random_index ];
			}
		}
		return null;
	}
}

