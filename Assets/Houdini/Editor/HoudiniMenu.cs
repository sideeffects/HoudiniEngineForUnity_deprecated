/*
* Copyright (c) <2017> Side Effects Software Inc.
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*
* Produced by:
*      Side Effects Software Inc
*      123 Front Street West, Suite 1401
*      Toronto, Ontario
*      Canada   M5J 2M2
*      416-504-9876
*
*/

// Master control for enabling runtime.
#if ( UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_LINUX )
	#define HAPI_ENABLE_RUNTIME
#endif

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
#if ( HAPI_ENABLE_RUNTIME )
		return true;
#else
		return false;
#endif // ( HAPI_ENABLE_RUNTIME )
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
#if ( HAPI_ENABLE_RUNTIME )
		return true;
#else
		return false;
#endif // ( HAPI_ENABLE_RUNTIME )
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

	[ MenuItem( HoudiniConstants.HAPI_PRODUCT_NAME + "/Debug/" + "Open Scene in Houdini", false, 71 ) ]
	static private void openSceneInHoudini()
	{
		string path_to_hip = Application.temporaryCachePath + "/hscene.hip";
		string path_to_houdini = HoudiniSetPath.prHoudiniPath + "/houdini";

		HoudiniHost.saveScene( path_to_hip, false );
		var houdini = new System.Diagnostics.Process();
		houdini.StartInfo.FileName = path_to_houdini;
		houdini.StartInfo.Arguments = "\"" + path_to_hip + "\"";
		houdini.Start();
	}

	[ MenuItem( HoudiniConstants.HAPI_PRODUCT_NAME + "/Debug/" + "Save Houdini Scene (.hip)", false, 72 ) ]
	static private void saveHoudiniScene()
	{
		HoudiniGUIUtility.saveHoudiniScene( false );
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
#if ( HAPI_ENABLE_RUNTIME )
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
#endif // ( HAPI_ENABLE_RUNTIME )
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
#if ( HAPI_ENABLE_RUNTIME )
		return true;
#else
		return false;
#endif // ( HAPI_ENABLE_RUNTIME )
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
#if ( HAPI_ENABLE_RUNTIME )
		return true;
#else
		return false;
#endif // ( HAPI_ENABLE_RUNTIME )
	}

	// -----------------------------------------------------------------------
	// Debug Menus (Hidden by Default)

	//[ MenuItem( HoudiniConstants.HAPI_PRODUCT_NAME + "/TEst", false, 1000 ) ]
	static private void blah2()
	{
		GameObject parent = null;
		for ( int i = 0; i < 10; i++ )
		{
			GameObject ob = new GameObject( "ob" + i );
			if ( parent )
				ob.transform.parent = parent.transform;
			parent = ob;
		}
	}

	//[ MenuItem( HoudiniConstants.HAPI_PRODUCT_NAME + "/Create Two Curves and a Merge", false, 1000 ) ]
	static private void blah()
	{
		HoudiniHost.createInputAsset( "curve1" );
		{
			HAPI_PartInfo new_part = new HAPI_PartInfo();
			new_part.vertexCount = 3;
			new_part.pointCount = 3;
			new_part.faceCount = 2;
			new_part.type = HAPI_PartType.HAPI_PARTTYPE_CURVE;
			HoudiniHost.setPartInfo( 0, 0, ref new_part );

			HAPI_AttributeInfo attrib_info = new HAPI_AttributeInfo( "P" );
			attrib_info.exists = true;
			attrib_info.count = 3; // 3 points
			attrib_info.tupleSize = 3; // 3 floats per point (x, y, z)
			attrib_info.owner = HAPI_AttributeOwner.HAPI_ATTROWNER_POINT;
			attrib_info.storage = HAPI_StorageType.HAPI_STORAGETYPE_FLOAT;
			HoudiniHost.addAttribute( 0, 0, "P", ref attrib_info );

			float[] positions = new float[ 9 ] { 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f };
			HoudiniHost.setAttributeFloatData( 0, 0, "P", ref attrib_info, positions, 0, 3 );

			int[] vertices = new int[ 3 ] { 0, 1, 2 };
			HoudiniHost.setVertexList( 0, 0, vertices, 0, 3 );

			int[] face_counts = new int[ 2 ] { 2, 2 }; // 3 edges for the first face (the only face)
			HoudiniHost.setFaceCounts( 0, 0, face_counts, 0, 2 );

			HoudiniHost.commitGeo( 0 );
		}

		HoudiniHost.createInputNode( "curve2" );

		int merge = HoudiniHost.createNode( -1, "SOP/merge", true );
		int convert = HoudiniHost.createNode( -1, "SOP/convert", true );

		//HoudiniHost.connectAssetGeometry( curve1, 0, merge, 0 );
		//HoudiniHost.connectAssetGeometry( curve2, 0, merge, 1 );
		//HoudiniHost.connectAssetGeometry( merge, 0, convert, 0 );

		HAPI_AssetInfo convert_info = HoudiniHost.getAssetInfo( convert );

		Debug.Log( HoudiniHost.getNodeInfo( HoudiniHost.getAssetInfo( merge ).nodeId ).parmCount );

		int convert_to_parm_id = HoudiniHost.getParmIdFromName( convert_info.nodeId, "totype" );
		//HAPI_ParmInfo[] convert_to_parm_info = new HAPI_ParmInfo[ 1 ];
		int[] value_arr = new int[ 1 ];
		value_arr[ 0 ] = 2;
		HoudiniHost.setParmIntValues( convert_info.nodeId, value_arr, convert_to_parm_id, 1 );
		HoudiniHost.cookNode( convert, true, true, false );
	}

	//[ MenuItem( HoudiniConstants.HAPI_PRODUCT_NAME + "/Create Simple Input Geo", false, 1000 ) ]
	static private void createSimpleInputGeo()
	{
		int asset_id = HoudiniHost.createInputNode( "simple_input_geo_test" );
		HoudiniHost.cookNode(
			asset_id,
			HoudiniHost.prSplitGeosByGroup,
			HoudiniHost.prSplitPointsByVertexAttributes,
			HoudiniHost.prImportTemplatedGeos );

		HAPI_PartInfo new_part = new HAPI_PartInfo();
		new_part.vertexCount = 3;
		new_part.pointCount = 3;
		new_part.faceCount = 1;
		new_part.type = HAPI_PartType.HAPI_PARTTYPE_MESH;
		HoudiniHost.setPartInfo( 0, 0, ref new_part );

		HAPI_AttributeInfo attrib_info = new HAPI_AttributeInfo( "P" );
		attrib_info.exists = true;
		attrib_info.count = 3; // 3 points
		attrib_info.tupleSize = 3; // 3 floats per point (x, y, z)
		attrib_info.owner = HAPI_AttributeOwner.HAPI_ATTROWNER_POINT;
		attrib_info.storage = HAPI_StorageType.HAPI_STORAGETYPE_FLOAT;
		HoudiniHost.addAttribute( 0, 0, "P", ref attrib_info );

		float[] positions = new float[ 9 ] { 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f };
		HoudiniHost.setAttributeFloatData( 0, 0, "P", ref attrib_info, positions, 0, 3 );

		int[] vertices = new int[ 3 ] { 0, 1, 2 };
		HoudiniHost.setVertexList( 0, 0, vertices, 0, 3 );

		int[] face_counts = new int[ 1 ] { 3 }; // 3 edges for the first face (the only face)
		HoudiniHost.setFaceCounts( 0, 0, face_counts, 0, 1 );

		bool[] point_group_mem = new bool[ 3 ] { true, true, false };
		HoudiniHost.addGroup( 0, 0, HAPI_GroupType.HAPI_GROUPTYPE_POINT, "test_pt_group" );
		HoudiniHost.setGroupMembership(
			0, 0, HAPI_GroupType.HAPI_GROUPTYPE_POINT, "test_pt_group", point_group_mem, 3 );

		bool[] prim_group_mem = new bool[ 1 ] { true };
		HoudiniHost.addGroup( 0, 0, HAPI_GroupType.HAPI_GROUPTYPE_PRIM, "test_prim_group" );
		HoudiniHost.setGroupMembership(
			0, 0, HAPI_GroupType.HAPI_GROUPTYPE_PRIM, "test_prim_group", prim_group_mem, 1 );

		HoudiniHost.commitGeo( 0 );
	}

}
