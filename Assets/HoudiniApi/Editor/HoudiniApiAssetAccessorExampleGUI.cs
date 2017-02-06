/*
* Copyright (c) <2017> Side Effects Software Inc.
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*
* Produced by:
*      Side Effects Software Inc
*      123 Front Street West, Suite 1401
*      Toronto, Ontario
*      Canada   M5J 2M2
*      416-504-9876
*
*/

using UnityEngine;
using UnityEditor;
using System.Collections;

[ CustomEditor( typeof( HoudiniApiAssetAccessorExample ) ) ]
public class HoudiniApiAssetAccessorExampleGUI : Editor {

	private HoudiniApiAssetAccessorExample exampleScript;

	void OnEnable()
	{
		exampleScript = target as HoudiniApiAssetAccessorExample;
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
