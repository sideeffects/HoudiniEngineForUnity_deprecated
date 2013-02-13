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
 * 		Contains HAPI_ChildSelectionControl which controls selection behaviour of asset game object children.
 * 
 */

using UnityEngine;
using UnityEditor;
using System.Collections;

using HAPI;

/// <summary>
/// 	Controls selection behaviour of asset game object children.
/// </summary>
public class HAPI_ChildSelectionControl : MonoBehaviour 
{	
	public int prObjectId		{ get { return myObjectId; }	set { myObjectId = value; } }
	public int prGeoId			{ get { return myGeoId; }		set { myGeoId = value; } }
	public int prGeoType		{ get { return myGeoType; }		set { myGeoType = value; } }
	public int prPartId			{ get { return myPartId; }		set { myPartId = value; } }
	public int prMaterialId		{ get { return myMaterialId; }	set { myMaterialId = value; } }
	public int[] prVertexList	{ get { return myVertexList; }	set { myVertexList = value; } }
	
	public HAPI_AttributeInfo prNormalAttrInfo { get; set; }
	
	public HAPI_AttributeInfo prUVAttrInfo { get; set; }
	
	public HAPI_Asset prAsset 
	{ 
		get
		{
			return myAsset;
		}
	}
	
	public HAPI_ChildSelectionControl()
	{
		prVertexList = null;
	}
	
	public void setAsset( MonoBehaviour asset )
	{
		myAsset = asset as HAPI_Asset;
	}
	
	/// <summary>
	/// 	Selects the parent of the current game object.
	/// </summary>
	public void selectParent()
	{
		
		if ( myAsset != null && myAsset.prAutoSelectAssetNode )
		{
			GameObject[] selection 	= new GameObject[ 1 ];
			selection[ 0 ] 			= myAsset.gameObject;
			Selection.objects 		= selection;
		}
	}

	// Private

	[SerializeField] private HAPI_Asset		myAsset;
	[SerializeField] private int			myObjectId;
	[SerializeField] private int			myGeoId;
	[SerializeField] private int			myGeoType;
	[SerializeField] private int			myPartId;
	[SerializeField] private int			myMaterialId;
	[SerializeField] private int[]			myVertexList;
}
