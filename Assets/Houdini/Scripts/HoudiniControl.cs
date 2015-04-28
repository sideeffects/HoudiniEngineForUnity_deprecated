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

// Typedefs
using HAPI_NodeId = System.Int32;
using HAPI_AssetId = System.Int32;

public class HoudiniControl : MonoBehaviour 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	
	// Please keep these in the same order and grouping as their initializations in HAPI_Control.reset().

	public HAPI_AssetId prAssetId { get { return myAssetId; } set { myAssetId = value; } }
	public HAPI_NodeId prNodeId { get { return myNodeId; } set { myNodeId = value; } }
	public HAPI_NodeId prObjectNodeId { get { return myObjectNodeId; } set { myObjectNodeId = value; } }
	public HoudiniAsset prAsset { get { return myAsset; } set { myAsset = value; } }
	public bool prParmsNeedInit
	{
		get
		{
			HoudiniParms parms = gameObject.GetComponent< HoudiniParms >();
			return ( parms == null ) || ( parms.prPostSerialization );
		}
		private set {}
	}
	public HoudiniParms prParms
	{
		get
		{
			HoudiniParms parms = getOrCreateComponent< HoudiniParms >();

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

#if UNITY_EDITOR
	public bool isPrefab() 			{ return PrefabUtility.GetPrefabType( gameObject ) == PrefabType.Prefab; }
	public bool isPrefabInstance()	{ return PrefabUtility.GetPrefabType( gameObject ) == PrefabType.PrefabInstance; }
#else
	public bool isPrefab()			{ return false; }
	public bool isPrefabInstance()	{ return false; }
#endif // UNITY_EDITOR

	public HoudiniControl() 
	{
		reset();
	}

	~HoudiniControl()
	{

	}

	public virtual void reset()
	{
		// Please keep these in the same order and grouping as their declarations at the top.

		prAssetId = -1;
		prNodeId = -1;
		prObjectNodeId = -1;
		prAsset = null;
	}

	public void init( HoudiniControl control )
	{
		prAssetId	= control.prAssetId;
		prNodeId	= control.prNodeId;
		prObjectNodeId = control.prObjectNodeId;
		prAsset		= control.prAsset;
	}

	public virtual string getRelativePath()
	{
		return "CONTROL";
	}

	public virtual string getAbsolutePath()
	{
		return "CONTROL";
	}

	public virtual void onParmChange() {}

	public T getOrCreateComponent< T >() where T : Component
	{
		return getOrCreateComponent< T >( gameObject );
	}
	static public T getOrCreateComponent< T >( GameObject game_object ) where T : Component
	{
		T component = game_object.GetComponent< T >();
		if ( component == null )
			component = game_object.AddComponent< T >();
		return component;
	}

	public void removeComponentsFromChildren< T >() where T : Component
	{
		removeComponentsFromChildren< T >( gameObject );
	}
	static public void removeComponentsFromChildren< T >( GameObject game_object ) where T : Component
	{
		foreach ( T comp in game_object.GetComponentsInChildren< T >() )
			DestroyImmediate( comp );
	}
	
	public void destroyChildren()
	{
		destroyChildren( gameObject.transform );
	}
	static public void destroyChildren( Transform trans ) 
	{
		List< GameObject > children = new List< GameObject >();
		
		foreach ( Transform child in trans )
			children.Add( child.gameObject );
		
		foreach ( GameObject child in children )
			HoudiniAssetUtility.destroyGameObject( child );
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Serialized Private Data

	[SerializeField] private HAPI_AssetId myAssetId;
	[SerializeField] private HAPI_NodeId myNodeId;
	[SerializeField] private HAPI_NodeId myObjectNodeId;
	[SerializeField] private HoudiniAsset myAsset;

}
