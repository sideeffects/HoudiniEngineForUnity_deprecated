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

public class HAPI_PartControl : HAPI_GeoControl 
{	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public

	public HAPI_Asset	prAsset {		get { return myAsset; }			set { myAsset = value; } }
	public int			prPartId {		get { return myPartId; }		set { myPartId = value; } }
	public string		prPartName {	get { return myPartName; }		set { myPartName = value; } }
	public int			prMaterialId {	get { return myMaterialId; }	set { myMaterialId = value; } }
	public int[]		prVertexList {	get { return myVertexList; }	set { myVertexList = value; } }
	
	public int 			prInstancePointNumber { get { return myInstancePointNumber; } set { myInstancePointNumber = value; } }
	public GameObject 	prObjectToInstantiate { get { return myObjectToInstantiate; } set { myObjectToInstantiate = value; } }
	
	public HAPI_PartControl()
	{
		reset();
	}

	~HAPI_PartControl()
	{

	}
	
	public override void reset()
	{
		base.reset();

		// Please keep these in the same order and grouping as their declarations at the top.
		
		prPartId 		= -1;
		prPartName		= "part_name";
		prMaterialId 	= -1;
		prVertexList 	= new int[ 1 ];
		myInstancePointNumber = -1;
	}

	public void selectParent()
	{
		if ( myAsset != null && myAsset.prAutoSelectAssetNode && HAPI_Host.prAutoSelectParent )
		{
			GameObject[] selection 	= new GameObject[ 1 ];
			selection[ 0 ] 			= myAsset.gameObject;
			Selection.objects 		= selection;
		}
	}

	public void init( HAPI_PartControl part_control )
	{
		init( (HAPI_GeoControl) part_control );

		prAsset			= part_control.prAsset;
		prPartId		= part_control.prPartId;
		prPartName		= part_control.prPartName;
		prMaterialId	= part_control.prMaterialId;
	}
	public void init( HAPI_Asset asset, int part_id, string part_name, int material_id )
	{
		prAsset			= asset;
		prPartId		= part_id;
		prPartName		= part_name;
		prMaterialId	= material_id;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private

	[SerializeField] private HAPI_Asset		myAsset;
	[SerializeField] private int			myPartId;
	[SerializeField] private string			myPartName;
	[SerializeField] private int			myMaterialId;
	[SerializeField] private int[]			myVertexList;
	[SerializeField] private int			myInstancePointNumber;
	[SerializeField] private GameObject		myObjectToInstantiate;
}
