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

		// Set up dropdown for selecting a parameter
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField( "Parameter Name" );
		exampleScript.parmIndex = EditorGUILayout.Popup( old_parm_index, exampleScript.parmNames );
		EditorGUILayout.EndHorizontal();

		// If selected parameter has changed then we need
		// to get value for new parameter
		if ( old_parm_index != exampleScript.parmIndex )
		{
			exampleScript.SetSelectedParameter();
		}

		// Set up field for parameter value
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField( "Value" );
		exampleScript.GetParameterGUI();
		EditorGUILayout.EndHorizontal();

		if ( GUILayout.Button( "Set Parameter" ) ) 
		{
			exampleScript.SetParameterValue();
		}
	}
}
