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
 * 		Contains HAPI_ChildSelectionControlGUI, the GUI companion to HAPI_ChildSelectionControl.
 * 
 */

using UnityEngine;
using UnityEditor;
using System.Collections;

/// <summary>
/// 	GUI companion to <see cref="HAPI_ChildSelectionControl"/> which actually triggers the selections.
/// </summary>
[ CustomEditor( typeof( HAPI_ChildSelectionControl ) ) ]
public class HAPI_ChildSelectionControlGUI : Editor 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	public void OnSceneGUI()
	{
		HAPI_ChildSelectionControl control = target as HAPI_ChildSelectionControl;		
		control.selectParent();
	}
}
