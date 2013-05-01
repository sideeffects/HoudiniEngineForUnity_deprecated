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
	
	[ MenuItem( HAPI_Constants.HAPI_PRODUCT_NAME + "/" + HAPI_GUIUtility.myLoadAssetLabel, false, 0 ) ]
	static private void createHAPIObject() 
	{
		string asset_file_path = HAPI_GUIUtility.promptForOTLPath();
		HAPI_GUIUtility.instantiateAsset( asset_file_path );
	}
	
	[ MenuItem( HAPI_Constants.HAPI_PRODUCT_NAME + "/" + HAPI_GUIUtility.myLoadHipLabel, false, 1 ) ]
	static private void loadHipFile() 
	{
		string hip_file_path = HAPI_GUIUtility.promptForHIPPath();
		HAPI_GUIUtility.loadHipFile( hip_file_path );
	}

	// -----------------------------------------------------------------------
	
	[ MenuItem( HAPI_Constants.HAPI_PRODUCT_NAME + "/" + HAPI_GUIUtility.myDebugLabel + " Window", false, 50 ) ]
	static private void debugWindow()
	{
		HAPI_WindowDebug.ShowWindow();
	}

	[ MenuItem( HAPI_Constants.HAPI_PRODUCT_NAME + "/" + HAPI_GUIUtility.mySettingsLabel + " Window", false, 51 ) ]
	static private void settingsWindow()
	{
		HAPI_WindowSettings.ShowWindow();
	}

	// -----------------------------------------------------------------------

	[ MenuItem( HAPI_Constants.HAPI_PRODUCT_NAME + "/" + HAPI_GUIUtility.myCreateCurveLabel, false, 100 ) ]
	static private void createCurve()
	{
		// Create game object.
		GameObject game_object = new GameObject( "curve" );
		
		// Add HAPI Object Control script component.
		game_object.AddComponent( "HAPI_AssetCurve" );
		HAPI_AssetCurve asset = game_object.GetComponent< HAPI_AssetCurve >();
		
		asset.prAssetSubType = HAPI_AssetSubType.HAPI_ASSETSUBTYPE_CURVE;
		
		// Do first build.
		bool build_result = asset.build();
		if ( !build_result ) // Something is not right. Clean up.
		{
			DestroyImmediate( game_object );
			return;
		}
		
		// Set new object name from asset name.
		string asset_name		= asset.prAssetInfo.name;
		game_object.name 		= asset_name;
		
		// Select the new houdini asset.
		GameObject[] selection 	= new GameObject[ 1 ];
		selection[ 0 ] 			= game_object;
		Selection.objects 		= selection;
	}
}
