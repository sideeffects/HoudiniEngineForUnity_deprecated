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

using HAPI;

// Typedefs
using HAPI_NodeId = System.Int32;
using HAPI_AssetId = System.Int32;

public class HAPI_Control : MonoBehaviour 
{
#if UNITY_EDITOR

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	
	// Please keep these in the same order and grouping as their initializations in HAPI_Control.reset().

	public HAPI_AssetId prAssetId { get { return myAssetId; } set { myAssetId = value; } }
	public HAPI_NodeId prNodeId { get { return myNodeId; } set { myNodeId = value; } }
	public HAPI_Asset prAsset { get { return myAsset; } set { myAsset = value; } }
	public bool prParmsNeedInit
	{
		get
		{
			HAPI_Parms parms = gameObject.GetComponent< HAPI_Parms >();
			return ( parms == null ) || ( parms.prPostSerialization );
		}
		private set {}
	}
	public HAPI_Parms prParms
	{
		get
		{
			HAPI_Parms parms = getOrCreateComponent< HAPI_Parms >();

			if ( parms.prControl == false )
				parms.prControl = this;

			if ( parms.prPostSerialization )
			{
				parms.getParameterValues();
				parms.prPostSerialization = false;
			}

			return parms;
		}
		private set {}
	}
	
	public bool isPrefab() 			{ return PrefabUtility.GetPrefabType( gameObject ) == PrefabType.Prefab; }
	public bool isPrefabInstance()	{ return PrefabUtility.GetPrefabType( gameObject ) == PrefabType.PrefabInstance; }

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
		prNodeId = -1;
		prAsset = null;
	}

	public void init( HAPI_Control control )
	{
		prAssetId	= control.prAssetId;
		prNodeId	= control.prNodeId;
		prAsset		= control.prAsset;
	}

	public virtual string getFullControlNameAndPath()
	{
		return "CONTROL";
	}

	public virtual void onParmChange() {}

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

	[SerializeField] private HAPI_AssetId myAssetId;
	[SerializeField] private HAPI_NodeId myNodeId;
	[SerializeField] private HAPI_Asset myAsset;
#endif // UNITY_EDITOR
}
