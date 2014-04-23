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
/// 	GUI companion to <see cref="HoudiniInstancer"/>. Responsible for generating the custom Inspector for
/// 	our instancer
/// </summary>
[ CustomEditor( typeof( HoudiniInstancer ) ) ]
public class HoudiniInstancerGUI : Editor 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	public void OnEnable() 
	{
		myInstancer = target as HoudiniInstancer;
	}
	
	public void OnSceneGUI()
	{
		myInstancer = target as HoudiniInstancer;
		
		if ( myInstancer != null )
			myInstancer.drawAllPins();
		
	}
	
	private HoudiniInstancer	 	myInstancer;
}
