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

#define DEBUG // since Unity doesn't seem to define it itself

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using HAPI;

/// <summary>
/// 	GUI companion to <see cref="HAPI_MeshToPrefab"/>. Responsible for generating the custom Inspector for
/// 	our mesh saving component
/// </summary>
[ CustomEditor( typeof( HAPI_MeshToPrefab ) ) ]
public class HAPI_MeshToPrefabGUI : Editor 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	public void OnEnable() 
	{		
		myMeshToPrefab = target as HAPI_MeshToPrefab;
		
		
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
				string prompt_result_path = HAPI_GUIUtility.promptForPrefabPath( old_prefab_path );
				if ( prompt_result_path.Length > 0 )
					new_prefab_path = prompt_result_path;
	        }
			
			myMeshToPrefab.prPathToPrefab = new_prefab_path;
		} 
		EditorGUILayout.EndHorizontal();
		
		
		if ( GUILayout.Button( "Save To Prefab" ) ) 
		{
			myMeshToPrefab.SaveToPrefab();
		}
		
	}
		
	private HAPI_MeshToPrefab	 	myMeshToPrefab;
	private int myFileChooserButtonWidth = 50;
}
