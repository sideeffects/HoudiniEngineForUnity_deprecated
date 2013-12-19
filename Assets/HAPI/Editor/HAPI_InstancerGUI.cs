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
	
	public void OnSceneGUI()
	{
		myInstancer = target as HAPI_Instancer;
		
		if ( myInstancer != null )
			myInstancer.drawAllPins();
		
	}
	
	private HAPI_Instancer	 	myInstancer;
}
