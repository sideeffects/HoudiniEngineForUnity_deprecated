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
 * 		Contains HAPI_Inspector, the GUI companion to HAPI_ObjectControl and responsible for generating
 * 		the custom Inspector GUI for an asset game object.
 * 
 */

#define DEBUG // since Unity doesn't seem to define it itself

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using HAPI;

/// <summary>
/// 	GUI companion to <see cref="HAPI_Instancer"/>. Responsible for generating the custom Inspector for
/// 	our instancer
/// </summary>
[ CustomEditor( typeof( HAPI_Instancer ) ) ]
public class HAPI_InstancerGUI : Editor 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	public void OnEnable() 
	{		
		myInstancer = target as HAPI_Instancer;
		
	}
	
	public override void OnInspectorGUI() 
	{
		
		myInstancer.prOverrideInstances = GUILayout.Toggle(myInstancer.prOverrideInstances, "Override Instance Object");
				
		myInstancer.prObjToInstantiate = EditorGUILayout.ObjectField( "Object To Instantiate", 
										myInstancer.prObjToInstantiate, typeof(GameObject), true ) as GameObject; 
				
		if ( GUILayout.Button( "Recreate Instances" ) ) 
		{
			myInstancer.instanceObjects();
		}
		
	}
		
	private HAPI_Instancer	 	myInstancer;
}
