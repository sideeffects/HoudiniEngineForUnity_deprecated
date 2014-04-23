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
using System.Collections;
using System.Collections.Generic;

public class HoudiniInstancerManager : MonoBehaviour 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	public List< HoudiniInstancerPersistentData > prInstancerPersistentData 
	{ 
		get { return myInstancersPersistentData; }
	}


	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Methods
	public HoudiniInstancerManager() 
	{
		myInstancersPersistentData = new List< HoudiniInstancerPersistentData >();
	}
	
	~HoudiniInstancerManager()
	{
		
	}

	public HoudiniInstancerPersistentData getInstancerPersistentData( string instancer_name )
	{
		foreach( HoudiniInstancerPersistentData persistent_data in myInstancersPersistentData )
		{
			if( persistent_data.instancerName == instancer_name )
				return persistent_data;
		}
		return null;
	}


	public void updateInstancerData( HoudiniInstancer instancer )
	{
		bool foundInstancer = false;
		foreach( HoudiniInstancerPersistentData persistent_data in myInstancersPersistentData )
		{
			if( persistent_data.instancerName == instancer.name )
			{
				foundInstancer = true;
				break;
			}
		}

		if( !foundInstancer )
		{
			HoudiniInstancerPersistentData instancer_data 
				= ScriptableObject.CreateInstance< HoudiniInstancerPersistentData>();
			instancer_data.instancerName = instancer.name;
			myInstancersPersistentData.Add( instancer_data );
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Serialized Private Data

	[SerializeField] private List< HoudiniInstancerPersistentData > myInstancersPersistentData;
}
