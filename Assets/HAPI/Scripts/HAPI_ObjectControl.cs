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

using HAPI;

public class HAPI_ObjectControl : HAPI_Control 
{

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	
	// Please keep these in the same order and grouping as their initializations in HAPI_Control.reset().

	public int		prObjectId {		get { return myObjectId; }			set { myObjectId = value; } }
	public string	prObjectName {		get { return myObjectName; }		set { myObjectName = value; } }
	public bool		prObjectVisible {	get { return myObjectVisible; }		set { myObjectVisible = value; } }

	public HAPI_ObjectControl() 
	{
		reset();
	}

	~HAPI_ObjectControl()
	{

	}

	public override void reset()
	{
		base.reset();

		// Please keep these in the same order and grouping as their declarations at the top.
		
		myObjectId		= -1;
		myObjectName	= "object_name";
		myObjectVisible	= false;
	}

	public void init( HAPI_ObjectControl object_control )
	{
		prAssetId		= object_control.prAssetId;
		prObjectId		= object_control.prObjectId;
		prObjectName	= object_control.prObjectName;
		prObjectVisible = object_control.prObjectVisible;
	}
	public void init( int asset_id, int object_id, string object_name, bool object_visible )
	{
		prAssetId		= asset_id;
		prObjectId		= object_id;
		prObjectName	= object_name;
		prObjectVisible = object_visible;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Serialized Private Data

	[SerializeField] private int		myObjectId;
	[SerializeField] private string		myObjectName;
	[SerializeField] private bool		myObjectVisible;
}
