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
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using HAPI;

[ ExecuteInEditMode ]
[ CustomEditor( typeof( HAPI_AssetInput ) ) ]
public class HAPI_AssetGUIInput : HAPI_AssetGUI 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public

	public override void OnEnable()
	{
		myAssetInput = target as HAPI_AssetInput;

		base.OnEnable();
	}

	public override void OnInspectorGUI()
	{
		if ( GUILayout.Button( "Build" ) )
			myAssetInput.buildAll();
	}

	private HAPI_AssetInput myAssetInput;
}
