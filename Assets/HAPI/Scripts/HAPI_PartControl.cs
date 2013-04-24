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

[ ExecuteInEditMode ]
public class HAPI_PartControl : HAPI_GeoControl 
{	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public

	public HAPI_Asset	prAsset {		get { return myAsset; }			set { myAsset = value; } }
	public int			prPartId {		get { return myPartId; }		set { myPartId = value; } }
	public string		prPartName {	get { return myPartName; }		set { myPartName = value; } }
	public int			prMaterialId {	get { return myMaterialId; }	set { myMaterialId = value; } }
	public int[]		prVertexList {	get { return myVertexList; }	set { myVertexList = value; } }
	public bool			prShowPointNumbers { get { return myShowPointNumbers; }	set { myShowPointNumbers = value; } }
	
	public int 			prInstancePointNumber { get { return myInstancePointNumber; } set { myInstancePointNumber = value; } }
	public GameObject 	prObjectToInstantiate { get { return myObjectToInstantiate; } set { myObjectToInstantiate = value; } }
	public bool 		prTransformChanged { get { return myTransformChanged; } set { myTransformChanged = value; } }
	
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
		myTransformChanged = false;
		myShowPointNumbers = false;
	}
	
	public void Awake()
	{
		myLastLocalToWorld = transform.localToWorldMatrix;
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
	
	
	public virtual void Update()
	{
		
		Matrix4x4 local_to_world = transform.localToWorldMatrix;
		
		if ( local_to_world == myLastLocalToWorld )
			return;
						
		myLastLocalToWorld = local_to_world;
		myTransformChanged = true;
		
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
	[SerializeField] private Matrix4x4		myLastLocalToWorld;
	[SerializeField] private bool			myTransformChanged;
	[SerializeField] private bool			myShowPointNumbers;
}
