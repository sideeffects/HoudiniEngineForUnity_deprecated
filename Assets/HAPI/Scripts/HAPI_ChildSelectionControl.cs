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

/// <summary>
/// 	Controls selection behaviour of asset game object children.
/// </summary>
public class HAPI_ChildSelectionControl : MonoBehaviour 
{	
	public void setObjectControl( MonoBehaviour object_control )
	{
		myObjectControl = object_control as HAPI_Asset;
	}
	
	/// <summary>
	/// 	Selects the parent of the current game object.
	/// </summary>
	public void selectParent()
	{
		
		if ( myObjectControl != null && myObjectControl.myAutoSelectAssetNode )
		{
			GameObject[] selection 	= new GameObject[ 1 ];
			selection[ 0 ] 			= myObjectControl.gameObject;
			Selection.objects 		= selection;
		}
	}

	private HAPI_Asset	myObjectControl;
}
