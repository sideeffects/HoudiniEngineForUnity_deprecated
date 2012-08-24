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
 */

using UnityEngine;
using UnityEditor;
using System.Collections;

[ CustomEditor( typeof( HAPI_ChildSelectionControl ) ) ]
public class HAPI_ChildSelectionControlGUI : Editor 
{
	public void OnSceneGUI()
	{
		HAPI_ChildSelectionControl control = target as HAPI_ChildSelectionControl;		
		control.SelectParent();
	}
}
