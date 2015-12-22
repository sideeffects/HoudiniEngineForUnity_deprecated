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
		if ( !HoudiniHost.isInstallationOk() )
		{
			HoudiniHost.displayHoudiniEngineInstallInfo();
			return;
		}

		string asset_file_path = HoudiniGUIUtility.promptForOTLPath();
		HoudiniAssetUtility.instantiateAsset( asset_file_path );
	}

	[ MenuItem( HoudiniConstants.HAPI_PRODUCT_NAME + "/" + HoudiniGUIUtility.myLoadAssetLabel, true, 0 ) ]
	static private bool validateCreateHAPIObject() 
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		return true;
#else
		return false;
#endif // ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
	}

	// Hidden intentionally for now.
	//[ MenuItem( HoudiniConstants.HAPI_PRODUCT_NAME + "/" + HoudiniGUIUtility.myLoadHipLabel, false, 1 ) ]
	static private void loadHipFile() 
	{
		if ( !HoudiniHost.isInstallationOk() )
		{
			HoudiniHost.displayHoudiniEngineInstallInfo();
			return;
		}

		string hip_file_path = HoudiniGUIUtility.promptForHIPPath();
		HoudiniAssetUtility.loadHipFile( hip_file_path );
	}

	[ MenuItem( HoudiniConstants.HAPI_PRODUCT_NAME + "/" + HoudiniGUIUtility.myLoadHipLabel, true, 1 ) ]
	static private bool validateLoadHipFile() 
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		return true;
#else
		return false;
#endif // ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
	}

	// Hidden intentionally for now.
	//[ MenuItem( HoudiniConstants.HAPI_PRODUCT_NAME + "/" + HoudiniGUIUtility.mySaveHipLabel, false, 1 ) ]
	static private void saveHipFile() 
	{
		string hip_file_path = EditorUtility.SaveFilePanel( "Save HIP File", "", "hscene.hip", "hip" );
		if ( hip_file_path != "" )
			HoudiniHost.saveScene( hip_file_path, false );
		else
			Debug.LogError( "Nothing to save." );
	}

	// -----------------------------------------------------------------------

	[ MenuItem( HoudiniConstants.HAPI_PRODUCT_NAME + "/" + HoudiniGUIUtility.myLaunchOrboltPage, false, 30 ) ]
	static private void launchOrboltPage() 
	{
		Application.OpenURL( "http://www.orbolt.com/unity" );
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

	[ MenuItem( HoudiniConstants.HAPI_PRODUCT_NAME + "/" + "Installation Info", false, 70 ) ]
	static private void about()
	{
		HoudiniHost.displayHoudiniEngineInstallInfo();
	}

	// -----------------------------------------------------------------------

	[ MenuItem( HoudiniConstants.HAPI_PRODUCT_NAME + "/" + HoudiniGUIUtility.myConvertToInputAssetLabel, false, 100 ) ]
	static private void convertToInputAsset()
	{
		if ( !HoudiniHost.isInstallationOk() )
		{
			HoudiniHost.displayHoudiniEngineInstallInfo();
			return;
		}

		foreach( GameObject obj in Selection.gameObjects )
		{
			bool not_an_asset = ( obj.GetComponent< HoudiniAsset >() == null );
			bool has_mesh_filter = ( obj.GetComponent< MeshFilter >() != null );
			if ( not_an_asset && has_mesh_filter )
			{
				obj.AddComponent< HoudiniAssetInput >();
			}
		}
	}

	[ MenuItem( HoudiniConstants.HAPI_PRODUCT_NAME + "/" + HoudiniGUIUtility.myConvertToInputAssetLabel, true, 100 ) ]
	static private bool validateConvertToInputAsset()
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		foreach( GameObject obj in Selection.gameObjects )
		{
			bool not_an_asset = ( obj.GetComponent< HoudiniAsset >() == null );
			bool has_mesh_filter = ( obj.GetComponent< MeshFilter >() != null );
			if ( not_an_asset && has_mesh_filter )
			{
				return true;
			}
		}
		return false;
#else
		return false;
#endif // ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
	}

	[ MenuItem( HoudiniConstants.HAPI_PRODUCT_NAME + "/" + HoudiniGUIUtility.myCreateMergerAsset, false, 101 ) ]
	static private void createMergerAsset()
	{
		if ( !HoudiniHost.isInstallationOk() )
		{
			HoudiniHost.displayHoudiniEngineInstallInfo();
			return;
		}

		// Create game object.
		GameObject game_object = new GameObject( "HoudiniMergerAsset" );

		// Add HAPI Object Control script component.
		HoudiniAssetMerger asset = game_object.AddComponent< HoudiniAssetMerger >();
		
		asset.prAssetSubType = HAPI_AssetSubType.HAPI_ASSETSUBTYPE_DEFAULT;
		
		// Do a test build to see if things are ok.
		bool build_result = asset.buildClientSide();
		if ( !build_result ) // Something is not right. Clean up and die.
		{
			DestroyImmediate( game_object );
			return;
		}

		// Select the new houdini asset.
		GameObject[] selection = new GameObject[ 1 ];
		selection[ 0 ] = game_object;
		Selection.objects = selection;
	}

	[ MenuItem( HoudiniConstants.HAPI_PRODUCT_NAME + "/" + HoudiniGUIUtility.myCreateMergerAsset, true, 101 ) ]
	static private bool validateCreateMergerAsset()
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		return true;
#else
		return false;
#endif // ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
	}

	[ MenuItem( HoudiniConstants.HAPI_PRODUCT_NAME + "/" + HoudiniGUIUtility.myCreateCurveLabel, false, 102 ) ]
	static private void createCurve()
	{
		if ( !HoudiniHost.isInstallationOk() )
		{
			HoudiniHost.displayHoudiniEngineInstallInfo();
			return;
		}

		// Create game object.
		GameObject game_object = new GameObject( "curve" );
		
		// Add HAPI Object Control script component.
		HoudiniAssetCurve asset = game_object.AddComponent< HoudiniAssetCurve >();
		
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

	[ MenuItem( HoudiniConstants.HAPI_PRODUCT_NAME + "/" + HoudiniGUIUtility.myCreateCurveLabel, true, 102 ) ]
	static private bool validateCreateCurve()
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		return true;
#else
		return false;
#endif // ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
	}

	// -----------------------------------------------------------------------
	// Debug Menus (Hidden by Default)

	//[ MenuItem( HoudiniConstants.HAPI_PRODUCT_NAME + "/TEst", false, 1000 ) ]
	static private void blah2()
	{
		HoudiniDataFile.reset();

		HoudiniDataFile.load();

		HoudiniDataFile.setInt( "Int", 12 );
		HoudiniDataFile.setFloat( "Float", 12.0f );
		HoudiniDataFile.setString( "String", "string" );
		HoudiniDataFile.setInt( "Int", 13 );

		HoudiniDataFile.save();
		HoudiniDataFile.load();

		Debug.Log( "Int: " + HoudiniDataFile.getInt( "Int", 9 ) );
		Debug.Log( "Float: " + HoudiniDataFile.getFloat( "Float", 9.0f ) );
		Debug.Log( "String: " + HoudiniDataFile.getString( "String", "blah" ) );
	}

	//[ MenuItem( HoudiniConstants.HAPI_PRODUCT_NAME + "/Create Two Curves and a Merge", false, 1000 ) ]
	static private void blah()
	{
		int curve1 = HoudiniHost.createInputAsset( "curve1" );
		{
			HAPI_PartInfo new_part = new HAPI_PartInfo();
			new_part.vertexCount = 3;
			new_part.pointCount = 3;
			new_part.faceCount = 2;
			new_part.type = HAPI_PartType.HAPI_PARTTYPE_CURVE;
			HoudiniHost.setPartInfo( curve1, 0, 0, ref new_part );

			HAPI_AttributeInfo attrib_info = new HAPI_AttributeInfo( "P" );
			attrib_info.exists = true;
			attrib_info.count = 3; // 3 points
			attrib_info.tupleSize = 3; // 3 floats per point (x, y, z)
			attrib_info.owner = HAPI_AttributeOwner.HAPI_ATTROWNER_POINT;
			attrib_info.storage = HAPI_StorageType.HAPI_STORAGETYPE_FLOAT;
			HoudiniHost.addAttribute( curve1, 0, 0, "P", ref attrib_info );

			float[] positions = new float[ 9 ] { 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f };
			HoudiniHost.setAttributeFloatData( curve1, 0, 0, "P", ref attrib_info, positions, 0, 3 );

			int[] vertices = new int[ 3 ] { 0, 1, 2 };
			HoudiniHost.setVertexList( curve1, 0, 0, vertices, 0, 3 );

			int[] face_counts = new int[ 2 ] { 2, 2 }; // 3 edges for the first face (the only face)
			HoudiniHost.setFaceCounts( curve1, 0, 0, face_counts, 0, 2 );

			HoudiniHost.commitGeo( curve1, 0, 0 );
		}

		int curve2 = HoudiniHost.createInputAsset( "curve2" );

		int merge = HoudiniHost.instantiateAsset( "SOP/merge", true );
		int convert = HoudiniHost.instantiateAsset( "SOP/convert", true );

		HoudiniHost.connectAssetGeometry( curve1, 0, merge, 0 );
		HoudiniHost.connectAssetGeometry( curve2, 0, merge, 1 );
		HoudiniHost.connectAssetGeometry( merge, 0, convert, 0 );

		HAPI_AssetInfo convert_info = HoudiniHost.getAssetInfo( convert );

		Debug.Log( HoudiniHost.getNodeInfo( HoudiniHost.getAssetInfo( merge ).nodeId ).parmCount );

		int convert_to_parm_id = HoudiniHost.getParmIdFromName( convert_info.nodeId, "totype" );
		//HAPI_ParmInfo[] convert_to_parm_info = new HAPI_ParmInfo[ 1 ];
		int[] value_arr = new int[ 1 ];
		value_arr[ 0 ] = 2;
		HoudiniHost.setParmIntValues( convert_info.nodeId, value_arr, convert_to_parm_id, 1 );
		HoudiniHost.cookAsset( convert, true, false );
	}

	//[ MenuItem( HoudiniConstants.HAPI_PRODUCT_NAME + "/Create Simple Input Geo", false, 1000 ) ]
	static private void createSimpleInputGeo() 
	{
		int asset_id = HoudiniHost.createInputAsset( "simple_input_geo_test" );
		HoudiniHost.cookAsset( asset_id, HoudiniHost.prSplitGeosByGroup, HoudiniHost.prImportTemplatedGeos );

		HAPI_PartInfo new_part = new HAPI_PartInfo();
		new_part.vertexCount = 3;
		new_part.pointCount = 3;
		new_part.faceCount = 1;
		new_part.type = HAPI_PartType.HAPI_PARTTYPE_MESH;
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
