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

[ CustomEditor( typeof( HAPI_AssetOTL ) ) ]
public partial class HAPI_AssetGUIOTL : HAPI_AssetGUI 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	public override void OnEnable() 
	{
		base.OnEnable();
		myAssetOTL = myAsset as HAPI_AssetOTL;
	}
	
	public override void OnInspectorGUI() 
	{
		myParmChanges = false;
		myDelayBuild = false;
		
		base.OnInspectorGUI();
		
		Event curr_event = Event.current;
		bool commitChanges = false;
		if ( curr_event.isKey && curr_event.type == EventType.KeyUp && curr_event.keyCode == KeyCode.Return )
			commitChanges = true;
		
		///////////////////////////////////////////////////////////////////////
		// Draw Game Object Controls
		
		myAssetOTL.prShowHoudiniControls = HAPI_GUI.foldout( "Houdini Controls", 
															 myAssetOTL.prShowHoudiniControls, true );
		if ( myAssetOTL.prShowHoudiniControls ) 
		{
			if ( GUILayout.Button( "Rebuild" ) ) 
				myAssetOTL.buildAll();

			if ( GUILayout.Button( "Recook" ) )
				myAssetOTL.buildClientSide();
			
			EditorGUILayout.BeginHorizontal(); 
			{
				if ( GUILayout.Button( "Export To Hip File..." ) ) 
				{
					string hip_file_path = EditorUtility.SaveFilePanel( "Save HIP File", "", "hscene.hip", "hip" );
					if ( hip_file_path != "" && HAPI_Host.hasScene() )
						HAPI_Host.exportAssetToHIPFile( myAssetOTL.prAssetId, hip_file_path );
					else
						Debug.LogError( "Nothing to save." );
				}
				
				if ( GUILayout.Button( "Replace From Hip File..." ) ) 
				{
					string hip_file_path = EditorUtility.OpenFilePanel( "Import HIP File", "", "hip" );
					if ( hip_file_path != "" && HAPI_Host.hasScene() )
					{
						try
						{
							HAPI_Host.replaceAssetFromHIPFile( myAssetOTL.prAssetId, hip_file_path );
						}
						catch ( HAPI_Error error )
						{
							Debug.LogError( error.ToString() );
						}
						
						myAssetOTL.build(	true,	// reload_asset
											false,	// unload_asset_first
											false,	// serialization_recovery_only
											false,	// force_reconnect
											true	// use_progress_bar_delay
										);
					}
					else
						Debug.LogError( "Nothing to save." );
				}
				
			} 
			EditorGUILayout.EndHorizontal();
			
			string path = myAssetOTL.prAssetPath;
			bool file_path_changed = HAPI_GUI.fileField( "otl_path", "OTL Path", ref myDelayBuild, ref path );
			if ( file_path_changed )
			{
				myParmChanges			|= file_path_changed;
				myAssetOTL.prAssetPath	 = path;
				myReloadAsset			 = true;
			}
		} // if
		
		///////////////////////////////////////////////////////////////////////
		// Draw Asset Options
		//	These don't affect the asset directly so they don't 
		//	trigger rebuilds.
		//

		myAssetOTL.prShowAssetOptions = HAPI_GUI.foldout( "Asset Options", myAssetOTL.prShowAssetOptions, true );
		if ( myAssetOTL.prShowAssetOptions )
			generateAssetOptions();

		///////////////////////////////////////////////////////////////////////
		// Draw Baking Controls

		myAssetOTL.prShowBakeOptions = HAPI_GUI.foldout( "Bake Animations", myAssetOTL.prShowBakeOptions, true );
		if ( myAssetOTL.prShowBakeOptions )
			generateAssetBakeControls();

		///////////////////////////////////////////////////////////////////////
		// Draw Asset Controls
		
		EditorGUILayout.Separator();
		myAssetOTL.prShowAssetControls = HAPI_GUI.foldout( "Asset Controls", 
														   myAssetOTL.prShowAssetControls, true );
		
		if ( myAssetOTL.prShowAssetControls )
			myParmChanges |= generateAssetControls();
		
		if ( ( myParmChanges && !myDelayBuild ) || ( myUnbuiltChanges && commitChanges ) )
		{
			myAssetOTL.build(	myReloadAsset,	// reload_asset
								true,			// unload_asset_first
								false,			// serializatin_recovery_only
								false,			// force_reconnect
								true			// use_delay_for_progress_bar
							);

			myUnbuiltChanges	= false;
			myParmChanges		= false;
			myReloadAsset		= false;

			// To keep things consistent with Unity workflow, we should not save parameter changes
			// while in Play mode.
			if ( !EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode )
				myAssetOTL.savePreset();
		}
		else if ( myParmChanges )
			myUnbuiltChanges = true;
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private
	
	private void generateAssetBakeControls()
	{
		// Start Time
		{
			float value = myAsset.prBakeStartTime;
			bool changed = HAPI_GUI.floatField( "bake_start_time", "Start Time", ref value );
			if( changed )
			{
				myAsset.prBakeStartTime = value;
			}
		}
		
		// End Time
		{
			float value = myAsset.prBakeEndTime;
			bool changed = HAPI_GUI.floatField( "bake_end_time", "End Time", ref value );
			if( changed )
			{
				myAsset.prBakeEndTime = value;
			}
		}
		
		// Samples per second
		{
			HAPI_GUIParm gui_parm	= new HAPI_GUIParm( "bake_samples_per_second", "Samples Per Second" );
			gui_parm.hasMin			= true;
			gui_parm.hasMax			= true;
			gui_parm.hasUIMin		= true;
			gui_parm.hasUIMax		= true;
			gui_parm.min			= 1;
			gui_parm.max			= 120;
			gui_parm.UIMin			= 1;
			gui_parm.UIMax			= 120;
			
			bool delay_build		= false;
			int[] values			= new int[1];
			values[0]				= myAsset.prBakeSamplesPerSecond;
			bool changed			= HAPI_GUI.intField( ref gui_parm, ref delay_build,
														 ref values );

			if ( changed )
				myAsset.prBakeSamplesPerSecond = values[ 0 ];
		}
		
		if ( GUILayout.Button( "Bake" ) ) 
		{
			HAPI_ProgressBar progress_bar = new HAPI_ProgressBar();
			myAsset.bakeAnimations( myAsset.prBakeStartTime, 
									myAsset.prBakeEndTime, 
									myAsset.prBakeSamplesPerSecond, 
									myAsset.gameObject,
									progress_bar );
			progress_bar.clearProgressBar();
		}
	}

	private void generateViewOptions()
	{
		// Show Geometries
		{
			bool value = myAsset.prIsGeoVisible;
			bool changed = HAPI_GUI.toggle( "show_geometries", "Show Geometries", ref value );
			if ( changed )
			{
				myAsset.prIsGeoVisible = value;
				HAPI_PartControl[] controls = 
					myAsset.GetComponentsInChildren< HAPI_PartControl >();
				foreach ( HAPI_PartControl control in controls )
				{
					if ( control.prGeoType != HAPI_GeoType.HAPI_GEOTYPE_EXPOSED_EDIT
							&& control.gameObject.GetComponent< MeshRenderer >() != null )
						control.gameObject.GetComponent< MeshRenderer >().enabled = myAsset.prIsGeoVisible;
				}
			}
		}
		
		// Show Pinned Instances
		{
			bool value = myAsset.prShowPinnedInstances;
			bool changed = HAPI_GUI.toggle( "show_pinned_instances", "Show Pinned Instances", ref value );
			if ( changed )
			{
				myAsset.prShowPinnedInstances = value;
			}
		}

		// Auto Select Asset Node Toggle
		{
			bool value = myAsset.prAutoSelectAssetNode;
			if ( HAPI_Host.prAutoSelectParent == false )
			{
				GUI.enabled = false;
				HAPI_GUI.toggle( "auto_select_parent", "Auto Select Parent (overwritted by global setting)", ref value );
				GUI.enabled = true;
			}
			else
				HAPI_GUI.toggle( "auto_select_parent", "Auto Select Parent", ref value );
			myAsset.prAutoSelectAssetNode = value;
		}
		
		// Hide When Fed to Other Asset
		{
			bool value = myAsset.prHideGeometryOnLinking;
			if ( HAPI_Host.prHideGeometryOnLinking == false )
			{
				GUI.enabled = false;
				HAPI_GUI.toggle( "hide_geometry_on_linking", "Hide Geometry On Linking (overwritted by global setting)", ref value );
				GUI.enabled = true;
			}
			else
				HAPI_GUI.toggle( "hide_geometry_on_linking", "Hide Geometry On Linking", ref value );
			myAsset.prHideGeometryOnLinking = value;
		}
	}
	
	private void generateMaterialOptions()
	{
		// Material Shader Type
		{
			int value = (int) myAsset.prMaterialShaderType;
			string[] labels = { "OpenGL", "Houdini Mantra Renderer" };
			int[] values = { 0, 1 };
			bool changed = HAPI_GUI.dropdown( "material_renderer", "Material Renderer", 
											  ref value, labels, values );
			if ( changed )
			{
				myAsset.prMaterialShaderType = (HAPI_ShaderType) value;
				HAPI_AssetUtility.reApplyMaterials( myAsset );
			}
		}

		// Show Vertex Colours
		{
			bool value = myAsset.prShowVertexColours;
			bool changed = HAPI_GUI.toggle( "show_vertex_colours", "Show Vertex Colors", ref value );
			if ( changed )
			{
				myAsset.prShowVertexColours = value;
				HAPI_AssetUtility.reApplyMaterials( myAsset );
			}
		}
	}

	private void generateCookingOptions()
	{
		// Enable Cooking Toggle
		{
			bool value = myAsset.prEnableCooking;
			if ( HAPI_Host.prEnableCooking == false )
			{
				GUI.enabled = false;
				HAPI_GUI.toggle( "enable_cooking", "Enable Cooking (overwritted by global setting)", ref value );
				GUI.enabled = true;
			}
			else
				HAPI_GUI.toggle( "enable_cooking", "Enable Cooking", ref value );
			myAsset.prEnableCooking = value;
		}

		// Playmode Per-Frame Cooking Toggle
		{
			bool value = myAsset.prPlaymodePerFrameCooking;
			HAPI_GUI.toggle( "playmode_per_frame_cooking", "Playmode Per-Frame Cooking", ref value );
			myAsset.prPlaymodePerFrameCooking = value;
		}

		// Push Unity Transform To Houdini Engine Toggle
		{
			bool value = myAsset.prPushUnityTransformToHoudini;
			HAPI_GUI.toggle( "push_unity_transform_to_houdini_engine", 
							 "Push Unity Transform To Houdini Engine", ref value );
			myAsset.prPushUnityTransformToHoudini = value;
		}

		// Transform Change Triggers Cooks Toggle
		{
			bool value = myAsset.prTransformChangeTriggersCooks;
			HAPI_GUI.toggle( "transform_change_triggers_cooks", "Transform Change Triggers Cooks", ref value );
			myAsset.prTransformChangeTriggersCooks = value;
		}
	}

	private void generateAssetOptions()
	{
		GUIContent[] modes = new GUIContent[ 3 ];
		modes[ 0 ] = new GUIContent( "View" );
		modes[ 1 ] = new GUIContent( "Materials" );
		modes[ 2 ] = new GUIContent( "Cooking" );
		myAsset.prAssetOptionsCategory = GUILayout.Toolbar( myAsset.prAssetOptionsCategory, modes );

		switch ( myAsset.prAssetOptionsCategory )
		{
			case 0: generateViewOptions(); break;
			case 1: generateMaterialOptions(); break;
			case 2: generateCookingOptions(); break;
			default: Debug.LogError( "Invalid Asset Options Category." ); break;
		}
	}
	
	private HAPI_AssetOTL myAssetOTL;
}
