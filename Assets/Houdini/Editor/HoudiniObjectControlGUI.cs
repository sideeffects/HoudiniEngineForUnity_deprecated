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

[ CustomEditor( typeof( HoudiniObjectControl ) ) ]
[ CanEditMultipleObjects ]
public class HoudiniObjectControlGUI : Editor 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public

	public void OnSceneGUI()
	{

	}

	public virtual void OnEnable()
	{

	}

	public override void OnInspectorGUI()
	{
		HoudiniGUI.help( "Values here are for debugging only and should not be modified directly.", MessageType.Info );

		bool gui_enabled = GUI.enabled;
		GUI.enabled = false;
		DrawDefaultInspector();
		GUI.enabled = gui_enabled;
	}
}
