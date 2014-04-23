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
using System.Collections.Generic;

/// <summary>
/// 	GUI companion to <see cref="HoudiniMeshToPrefab"/>. Responsible for generating the custom Inspector for
/// 	our mesh saving component
/// </summary>
[ CustomEditor( typeof( HoudiniMeshToPrefab ) ) ]
public class HoudiniMeshToPrefabGUI : Editor 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	public void OnEnable()
	{
		myMeshToPrefab = target as HoudiniMeshToPrefab;
	}

	public override void OnInspectorGUI() 
	{
		EditorGUILayout.LabelField( new GUIContent( "Prefab Path:" ) );
		EditorGUILayout.BeginHorizontal(); 
		{
			string old_prefab_path = myMeshToPrefab.prPathToPrefab;
			string new_prefab_path = "";
			new_prefab_path = EditorGUILayout.TextField( old_prefab_path );

			if ( GUILayout.Button( "...", GUILayout.Width( myFileChooserButtonWidth ) ) ) 
			{
				string prompt_result_path = HoudiniGUIUtility.promptForPrefabPath( old_prefab_path );
				if ( prompt_result_path.Length > 0 )
					new_prefab_path = prompt_result_path;
			}

			myMeshToPrefab.prPathToPrefab = new_prefab_path;
		}
		EditorGUILayout.EndHorizontal();

		if ( GUILayout.Button( "Save To Prefab" ) ) 
			myMeshToPrefab.SaveToPrefab();
	}

	private HoudiniMeshToPrefab myMeshToPrefab;
	private int myFileChooserButtonWidth = 50;
}
