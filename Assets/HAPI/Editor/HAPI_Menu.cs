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
 * 		Contains HAPI_Menu which is added to the main Unity menu bar.
 * 
 */

using UnityEngine;
using UnityEditor;
using System.Collections;
using HAPI;

/// <summary>
/// 	Main HAPI menu which adds components to the main Unity menu bar.
/// </summary>
public class HAPI_Menu : MonoBehaviour 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private
	
	/// <summary>
	/// 	Prompts the user for a path to a .otl file and creates a HAPI game object from it.
	/// </summary>
	[ MenuItem( "HAPI/" + HAPI_GUIUtility.myLoadAssetLabel ) ]
	static private void createHAPIObject() 
	{
		string asset_file_path = HAPI_GUIUtility.promptForOTLPath();
		HAPI_GUIUtility.instantiateAsset( asset_file_path );
	}
	
}
