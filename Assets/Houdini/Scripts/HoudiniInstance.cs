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

// Master control for enabling runtime.
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX || ( UNITY_METRO && UNITY_EDITOR ) )
	#define HAPI_ENABLE_RUNTIME
#endif

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;

[ ExecuteInEditMode ]
public class HoudiniInstance : MonoBehaviour
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	public int 			prInstancePointNumber { get { return myInstancePointNumber; } set { myInstancePointNumber = value; } }
	public GameObject 	prObjectToInstantiate { get { return myObjectToInstantiate; } set { myObjectToInstantiate = value; } }
	public bool 		prTransformChanged { get { return myTransformChanged; } set { myTransformChanged = value; } }

	public HoudiniInstancer prInstancer { get { return myInstancer; } set { myInstancer = value; } }


	public HoudiniInstance()
	{
		reset();
	}

	~HoudiniInstance()
	{

	}
	
	public void reset()
	{
		myInstancePointNumber = -1;
		myTransformChanged = false;
		myInstancer = null;
	}
	
	public void Awake()
	{
		myLastLocalToWorld = transform.localToWorldMatrix;
	}

#if ( HAPI_ENABLE_RUNTIME )
	public virtual void Update()
	{
		Matrix4x4 local_to_world = transform.localToWorldMatrix;
		
		if ( local_to_world == myLastLocalToWorld )
			return;
						
		myLastLocalToWorld = local_to_world;
		myTransformChanged = true;
	}
#endif // ( HAPI_ENABLE_RUNTIME )

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private

	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Serialized Private Data

#if !( HAPI_ENABLE_RUNTIME )
#pragma warning disable 0414
#endif // ( HAPI_ENABLE_RUNTIME )

	[SerializeField] private int			myInstancePointNumber;
	[SerializeField] private GameObject		myObjectToInstantiate;
	[SerializeField] private Matrix4x4		myLastLocalToWorld;
	[SerializeField] private bool			myTransformChanged;
	[SerializeField] private HoudiniInstancer myInstancer;

#if !( HAPI_ENABLE_RUNTIME )
#pragma warning restore 0414
#endif // ( HAPI_ENABLE_RUNTIME )

}
