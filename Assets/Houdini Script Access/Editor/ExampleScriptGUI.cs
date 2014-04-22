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

[ CustomEditor( typeof( ExampleScript ) ) ]
public class ExampleScriptGUI : Editor {

	private ExampleScript exampleScript;

	void OnEnable()
	{
		exampleScript = target as ExampleScript;
	}

	public override void OnInspectorGUI()
	{
		int old_parm_index = exampleScript.parmIndex;

		if ( exampleScript.hasAsset() )
		{
			// Set up dropdown for selecting a parameter
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField( "Parameter Name" );
			exampleScript.parmIndex = EditorGUILayout.Popup( old_parm_index, exampleScript.parmNames );
			EditorGUILayout.EndHorizontal();

			// If selected parameter has changed then we need
			// to get value for new parameter
			if ( old_parm_index != exampleScript.parmIndex )
			{
				exampleScript.setSelectedParameter();
			}

			// Set up field for parameter value
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField( "Value" );
			exampleScript.getParameterGUI();
			EditorGUILayout.EndHorizontal();

			if ( GUILayout.Button( "Set Parameter" ) ) 
			{
				exampleScript.setParameterValue();
			}
		}
	}
}
