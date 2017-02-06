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
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;

// Typedefs
using HAPI_NodeId = System.Int32;

public class HoudiniControl : MonoBehaviour 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	
	// Please keep these in the same order and grouping as their initializations in HAPI_Control.reset().

	public HAPI_NodeId prAssetId { get { return myAssetId; } set { myAssetId = value; } }
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

	public bool hasComponent< T >() where T : Component
	{
		return GetComponent< T >() != null;
	}

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

	public void removeComponent< T >() where T : Component
	{
		removeComponent< T >( gameObject );
	}
	static public void removeComponent< T >( GameObject game_object ) where T : Component
	{
		T component = game_object.GetComponent< T >();
		if ( component != null )
			DestroyImmediate( component );
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

	public void destoryChildrenWithComponent< T >() where T : Component
	{
		destoryChildrenWithComponent< T >( gameObject.transform );
	}
	static public void destoryChildrenWithComponent< T >( Transform trans ) where T : Component
	{
		List< GameObject > children = new List< GameObject >();

		foreach ( Transform child in trans )
			children.Add( child.gameObject );
		
		foreach ( GameObject child in children )
			if ( child.GetComponent< T >() != null )
				HoudiniAssetUtility.destroyGameObject( child );
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

	[SerializeField] private HAPI_NodeId myAssetId;
	[SerializeField] private HAPI_NodeId myNodeId;
	[SerializeField] private HAPI_NodeId myObjectNodeId;
	[SerializeField] private HoudiniAsset myAsset;

}
