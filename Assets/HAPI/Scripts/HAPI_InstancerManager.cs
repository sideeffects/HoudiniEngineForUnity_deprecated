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

public class HAPI_InstancerManager : MonoBehaviour 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	public List< HAPI_InstancerPersistentData > prInstancerPersistentData 
	{ 
		get { return myInstancersPersistentData; }
	}


	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Methods
	public HAPI_InstancerManager() 
	{
		myInstancersPersistentData = new List< HAPI_InstancerPersistentData >();
	}
	
	~HAPI_InstancerManager()
	{
		
	}

	public HAPI_InstancerPersistentData getInstancerPersistentData( string instancer_name )
	{
		foreach( HAPI_InstancerPersistentData persistent_data in myInstancersPersistentData )
		{
			if( persistent_data.instancerName == instancer_name )
				return persistent_data;
		}
		return null;
	}


	public void updateInstancerData( HAPI_Instancer instancer )
	{
		bool foundInstancer = false;
		foreach( HAPI_InstancerPersistentData persistent_data in myInstancersPersistentData )
		{
			if( persistent_data.instancerName == instancer.name )
			{
				foundInstancer = true;
				break;
			}
		}

		if( !foundInstancer )
		{
			HAPI_InstancerPersistentData instancer_data 
				= ScriptableObject.CreateInstance< HAPI_InstancerPersistentData>();
			instancer_data.instancerName = instancer.name;
			myInstancersPersistentData.Add( instancer_data );
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Serialized Private Data

	[SerializeField] private List< HAPI_InstancerPersistentData > myInstancersPersistentData;
}
