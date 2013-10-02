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
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

using HAPI;

public class HAPI_Control : MonoBehaviour 
{

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	
	// Please keep these in the same order and grouping as their initializations in HAPI_Control.reset().

	public int prAssetId { get { return myAssetId; } set { myAssetId = value; } }
	public HAPI_Asset prAsset { get { return myAsset; } set { myAsset = value; } }

	public HAPI_Control() 
	{
		reset();
	}

	~HAPI_Control()
	{

	}

	public virtual void reset()
	{
		// Please keep these in the same order and grouping as their declarations at the top.
		
		prAssetId = -1;
		prAsset = null;
	}
	
	public T getOrCreateComponent< T >() where T : Component {
		T component = gameObject.GetComponent< T >();
		if ( component == null )
			component = gameObject.AddComponent< T >();
		return component;
	}
	
	public void destroyChildren( Transform trans ) 
	{
		List< GameObject > children = new List< GameObject >();
		
		foreach ( Transform child in trans )
			children.Add( child.gameObject );
		
		foreach ( GameObject child in children )
			HAPI_AssetUtility.destroyGameObject( child );
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Serialized Private Data

	[SerializeField] private int myAssetId;
	[SerializeField] private HAPI_Asset myAsset;
}
