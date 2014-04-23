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

/// <summary>
/// 	Main HAPI menu which adds components to the main Unity menu bar.
/// </summary>
public class HoudiniMenu : MonoBehaviour 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private
	
	[ MenuItem( HoudiniConstants.HAPI_PRODUCT_NAME + "/" + HoudiniGUIUtility.myLoadAssetLabel, false, 0 ) ]
	static private void createHAPIObject() 
	{
		string asset_file_path = HoudiniGUIUtility.promptForOTLPath();
		HoudiniGUIUtility.instantiateAsset( asset_file_path );
	}

	[ MenuItem( HoudiniConstants.HAPI_PRODUCT_NAME + "/" + HoudiniGUIUtility.myLoadAssetLabel, true, 0 ) ]
	static private bool validateCreateHAPIObject() 
	{
#if UNITY_STANDALONE_WIN
		return true;
#else
		return false;
#endif // UNITY_STANDALONE_WIN
	}

	[ MenuItem( HoudiniConstants.HAPI_PRODUCT_NAME + "/" + HoudiniGUIUtility.myLaunchOrboltPage, false, 1 ) ]
	static private void launchOrboltPage() 
	{
		Application.OpenURL( "http://www.orbolt.com/unity" );
	}

	// Hidden intentionally for now.
	//[ MenuItem( HAPI_Constants.HAPI_PRODUCT_NAME + "/" + HAPI_GUIUtility.myLoadHipLabel, false, 1 ) ]
	static private void loadHipFile() 
	{
		string hip_file_path = HoudiniGUIUtility.promptForHIPPath();
		HoudiniGUIUtility.loadHipFile( hip_file_path );
	}

	// -----------------------------------------------------------------------
	
	[ MenuItem( HoudiniConstants.HAPI_PRODUCT_NAME + "/" + HoudiniGUIUtility.myDebugLabel + " Window", false, 50 ) ]
	static private void debugWindow()
	{
		HoudiniWindowDebug.ShowWindow();
	}

	[ MenuItem( HoudiniConstants.HAPI_PRODUCT_NAME + "/" + HoudiniGUIUtility.mySettingsLabel + " Window", false, 51 ) ]
	static private void settingsWindow()
	{
		HoudiniWindowSettings.ShowWindow();
	}

	// -----------------------------------------------------------------------

	[ MenuItem( HoudiniConstants.HAPI_PRODUCT_NAME + "/" + HoudiniGUIUtility.myCreateCurveLabel, false, 100 ) ]
	static private void createCurve()
	{
		// Create game object.
		GameObject game_object = new GameObject( "curve" );
		
		// Add HAPI Object Control script component.
		game_object.AddComponent( "HAPI_AssetCurve" );
		HoudiniAssetCurve asset = game_object.GetComponent< HoudiniAssetCurve >();
		
		asset.prAssetSubType = HAPI_AssetSubType.HAPI_ASSETSUBTYPE_CURVE;
		
		// Do first build.
		bool build_result = asset.buildAll();
		if ( !build_result ) // Something is not right. Clean up and die.
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

	[ MenuItem( HoudiniConstants.HAPI_PRODUCT_NAME + "/" + HoudiniGUIUtility.myCreateCurveLabel, true, 100 ) ]
	static private bool validateCreateCurve()
	{
#if UNITY_STANDALONE_WIN
		return true;
#else
		return false;
#endif // UNITY_STANDALONE_WIN
	}

	// -----------------------------------------------------------------------
	// Debug Menus (Hidden by Default)

	//[ MenuItem( HAPI_Constants.HAPI_PRODUCT_NAME + "/Create Simple Input Geo", false, 1000 ) ]
	static private void createSimpleInputGeo() 
	{
		int asset_id = HoudiniHost.createInputAsset( "simple_input_geo_test" );
		HoudiniHost.cookAsset( asset_id );

		HAPI_PartInfo new_part = new HAPI_PartInfo();
		new_part.vertexCount = 3;
		new_part.pointCount = 3;
		new_part.faceCount = 1;
		new_part.isCurve = false;
		HoudiniHost.setPartInfo( asset_id, 0, 0, ref new_part );

		HAPI_AttributeInfo attrib_info = new HAPI_AttributeInfo( "P" );
		attrib_info.exists = true;
		attrib_info.count = 3; // 3 points
		attrib_info.tupleSize = 3; // 3 floats per point (x, y, z)
		attrib_info.owner = HAPI_AttributeOwner.HAPI_ATTROWNER_POINT;
		attrib_info.storage = HAPI_StorageType.HAPI_STORAGETYPE_FLOAT;
		HoudiniHost.addAttribute( asset_id, 0, 0, "P", ref attrib_info );

		float[] positions = new float[ 9 ] { 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f };
		HoudiniHost.setAttributeFloatData( asset_id, 0, 0, "P", ref attrib_info, positions, 0, 3 );

		int[] vertices = new int[ 3 ] { 0, 1, 2 };
		HoudiniHost.setVertexList( asset_id, 0, 0, vertices, 0, 3 );

		int[] face_counts = new int[ 1 ] { 3 }; // 3 edges for the first face (the only face)
		HoudiniHost.setFaceCounts( asset_id, 0, 0, face_counts, 0, 1 );

		bool[] point_group_mem = new bool[ 3 ] { true, true, false };
		HoudiniHost.addGroup( asset_id, 0, 0, HAPI_GroupType.HAPI_GROUPTYPE_POINT, "test_pt_group" );
		HoudiniHost.setGroupMembership(
			asset_id, 0, 0, HAPI_GroupType.HAPI_GROUPTYPE_POINT, "test_pt_group", point_group_mem, 3 );

		bool[] prim_group_mem = new bool[ 1 ] { true };
		HoudiniHost.addGroup( asset_id, 0, 0, HAPI_GroupType.HAPI_GROUPTYPE_PRIM, "test_prim_group" );
		HoudiniHost.setGroupMembership(
			asset_id, 0, 0, HAPI_GroupType.HAPI_GROUPTYPE_PRIM, "test_prim_group", prim_group_mem, 1 );

		HoudiniHost.commitGeo( asset_id, 0, 0 );
	}

}
