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

// Master control for enabling runtime.
#if ( UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_LINUX )
	#define HAPI_ENABLE_RUNTIME
#endif

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
