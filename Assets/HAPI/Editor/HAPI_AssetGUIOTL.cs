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
		myHelpScrollPosition = new Vector2( 0.0f, 0.0f );
	}
	
	public override void OnInspectorGUI() 
	{
		myParmChanges = false;
		myDelayBuild = false;
		myFocusChanged = false;

		Event curr_event = Event.current;
		bool commitChanges = false;
		if ( curr_event.isKey && curr_event.type == EventType.KeyUp && curr_event.keyCode == KeyCode.Return )
			commitChanges = true;

		base.OnInspectorGUI();

		if ( myAssetOTL.isPrefab() )
			return;

		///////////////////////////////////////////////////////////////////////
		// Draw Game Object Controls

		myAssetOTL.prShowHoudiniControls 
			= HAPI_GUI.foldout( "Houdini Controls", myAssetOTL.prShowHoudiniControls, true );
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
											true,	// force_reconnect
											myAsset.prCookingTriggersDownCooks,
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

		// Draw Help Pane
		myAssetOTL.prShowHelp = HAPI_GUI.foldout( "Asset Help", myAssetOTL.prShowHelp, true );
		if ( myAssetOTL.prShowHelp )
		{
			myHelpScrollPosition = EditorGUILayout.BeginScrollView(
				myHelpScrollPosition, GUILayout.Height( 200 ) );
			float height = GUI.skin.label.CalcHeight( 
				new GUIContent( myAssetOTL.prAssetHelp ), (float) Screen.width );
			GUIStyle sel_label = GUI.skin.label;
			sel_label.stretchWidth = true;
			sel_label.wordWrap = true;
			EditorGUILayout.SelectableLabel( 
				myAssetOTL.prAssetHelp, sel_label, GUILayout.Height( height ), 
				GUILayout.Width( Screen.width - 40 ) );
			EditorGUILayout.EndScrollView();
		}
		
		///////////////////////////////////////////////////////////////////////
		// Draw Asset Settings
		// These don't affect the asset directly so they don't trigger rebuilds.

		myAssetOTL.prShowAssetSettings = HAPI_GUI.foldout( "Asset Settings", myAssetOTL.prShowAssetSettings, true );
		if ( myAssetOTL.prShowAssetSettings )
			generateAssetSettings();

		///////////////////////////////////////////////////////////////////////
		// Draw Baking Controls

		myAssetOTL.prShowBakeOptions = HAPI_GUI.foldout( "Bake Animations", myAssetOTL.prShowBakeOptions, true );
		if ( myAssetOTL.prShowBakeOptions )
			generateAssetBakeControls();

		///////////////////////////////////////////////////////////////////////
		// Apply Changes

		if ( ( ( myParmChanges && !myDelayBuild ) || 
				( myUnbuiltChanges && ( commitChanges || myFocusChanged ) ) ) )
		{
			myAssetOTL.onParmChange( myReloadAsset );
	
			myUnbuiltChanges	= false;
			myParmChanges		= false;
			myReloadAsset		= false;
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
			if ( changed )
				myAsset.prBakeStartTime = value;
		}
		
		// End Time
		{
			float value = myAsset.prBakeEndTime;
			bool changed = HAPI_GUI.floatField( "bake_end_time", "End Time", ref value );
			if ( changed )
				myAsset.prBakeEndTime = value;
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

	private void generateViewSettings()
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
					if ( control.prGeoType != HAPI_GeoType.HAPI_GEOTYPE_INTERMEDIATE
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

		// Auto Select Asset Root Node Toggle
		{
			bool value		= myAsset.prAutoSelectAssetRootNode;
			string name		= "auto_select_asset_root_node";
			string label	= "Auto Select Asset Root Node";

			GUI.enabled = ( HAPI_Host.prAutoSelectAssetRootNode == HAPI_Host.myDefaultAutoSelectAssetRootNode );
			if ( !GUI.enabled ) 
				label += " (overwritted by global setting)";
			bool changed = HAPI_GUI.toggle( name, label, ref value );
			GUI.enabled = true;

			if ( changed )
				myAsset.prAutoSelectAssetRootNode = value;
		}
		
		// Hide When Fed to Other Asset
		{
			bool value		= myAsset.prHideGeometryOnLinking;
			string name		= "hide_geometry_on_linking";
			string label	= "Hide Geometry On Linking";

			GUI.enabled = ( HAPI_Host.prHideGeometryOnLinking == HAPI_Host.myDefaultHideGeometryOnLinking );
			if ( !GUI.enabled ) 
				label += " (overwritted by global setting)";
			bool changed = HAPI_GUI.toggle( name, label, ref value );
			GUI.enabled = true;

			if ( changed )
				myAsset.prHideGeometryOnLinking = value;
		}
	}
	
	private void generateMaterialSettings()
	{
		if ( GUILayout.Button( "Re-Render" ) ) 
		{
			HAPI_AssetUtility.reApplyMaterials( myAsset );
		}

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

		// Render Resolution
		{
			bool delay_build = false;
			int[] values = new int[ 2 ];
			values[ 0 ] = (int) myAsset.prRenderResolution[ 0 ];
			values[ 1 ] = (int) myAsset.prRenderResolution[ 1 ];
			HAPI_GUIParm gui_parm = new HAPI_GUIParm( "render_resolution", "Render Resolution", 2 );
			bool changed = HAPI_GUI.intField( ref gui_parm, ref delay_build, ref values );
			if ( changed )
			{
				Vector2 new_resolution = new Vector2( (float) values[ 0 ], (float) values[ 1 ] );
				myAsset.prRenderResolution = new_resolution;
			}
		}

		// Show Vertex Colours
		{
			bool value = myAsset.prShowOnlyVertexColours;
			bool changed = HAPI_GUI.toggle( "show_only_vertex_colours", "Show Only Vertex Colors", ref value );
			if ( changed )
			{
				myAsset.prShowOnlyVertexColours = value;
				HAPI_AssetUtility.reApplyMaterials( myAsset );
			}
		}

		// Generate Tangents
		{
			bool value		= myAsset.prGenerateTangents;
			string name		= "generate_tangents";
			string label	= "Generate Tangents";

			GUI.enabled = HAPI_Host.isGenerateTangentsDefault();
			if ( !GUI.enabled ) 
				label += " (overwritted by global setting)";
			bool changed = HAPI_GUI.toggle( name, label, ref value );
			GUI.enabled = true;

			if ( changed )
			{
				myAsset.prGenerateTangents = value;
				myAssetOTL.build( true, false, false, true, myAsset.prCookingTriggersDownCooks, true );
			}
		}
	}

	private void generateCookingSettings()
	{
		// Enable Cooking Toggle
		{
			bool value		= myAsset.prEnableCooking;
			string name		= "enable_cooking";
			string label	= "Enable Cooking";

			GUI.enabled = ( HAPI_Host.prEnableCooking == HAPI_Host.myDefaultEnableCooking );
			if ( !GUI.enabled ) 
				label += " (overwritted by global setting)";
			bool changed = HAPI_GUI.toggle( name, label, ref value );
			GUI.enabled = true;

			if ( changed )
				myAsset.prEnableCooking = value;
		}

		HAPI_GUI.separator();

		// Cooking Triggers Downstream Cooks Toggle
		{
			bool value		= myAsset.prCookingTriggersDownCooks;
			string name		= "cooking_triggers_downstream_cooks";
			string label	= "Cooking Triggers Downstream Cooks";

			bool global_overwrite = ( HAPI_Host.prCookingTriggersDownCooks != 
									  HAPI_Host.myDefaultCookingTriggersDownCooks );
			bool local_overwrite  = ( myAsset.prEnableCooking == false );

			GUI.enabled = ( !global_overwrite && !local_overwrite );
			if ( !GUI.enabled ) 
				if ( global_overwrite )
					label += " (overwritted by global setting)";
				else // local_overwrite
					label += " (all cooking is disabled)";
			bool changed = HAPI_GUI.toggle( name, label, ref value );
			GUI.enabled = true;

			if ( changed )
				myAsset.prCookingTriggersDownCooks = value;
		}

		// Playmode Per-Frame Cooking Toggle
		{
			bool value		= myAsset.prPlaymodePerFrameCooking;
			string name		= "playmode_per_frame_cooking";
			string label	= "Playmode Per-Frame Cooking";

			bool global_overwrite = ( HAPI_Host.prPlaymodePerFrameCooking != 
									  HAPI_Host.myDefaultPlaymodePerFrameCooking );
			bool local_overwrite  = ( myAsset.prEnableCooking == false );

			GUI.enabled = ( !global_overwrite && !local_overwrite );
			if ( !GUI.enabled ) 
				if ( global_overwrite )
					label += " (overwritted by global setting)";
				else // local_overwrite
					label += " (all cooking is disabled)";
			bool changed = HAPI_GUI.toggle( name, label, ref value );
			GUI.enabled = true;

			if ( changed )
				myAsset.prPlaymodePerFrameCooking = value;
		}

		HAPI_GUI.separator();

		// Push Unity Transform To Houdini Engine Toggle
		{
			bool value		= myAsset.prPushUnityTransformToHoudini;
			string name		= "push_unity_transform_to_houdini_engine";
			string label	= "Push Unity Transform To Houdini Engine";

			GUI.enabled = ( HAPI_Host.prPushUnityTransformToHoudini == 
							HAPI_Host.myDefaultPushUnityTransformToHoudini );
			if ( !GUI.enabled ) 
				label += " (overwritted by global setting)";
			bool changed = HAPI_GUI.toggle( name, label, ref value );
			GUI.enabled = true;

			if ( changed )
				myAsset.prPushUnityTransformToHoudini = value;
		}

		// Transform Change Triggers Cooks Toggle
		{
			bool value		= myAsset.prTransformChangeTriggersCooks;
			string name		= "transform_change_triggers_cooks";
			string label	= "Transform Change Triggers Cooks";

			bool global_overwrite = ( HAPI_Host.prTransformChangeTriggersCooks != 
									  HAPI_Host.myDefaultTransformChangeTriggersCooks );
			bool local_overwrite  = ( myAsset.prEnableCooking == false );

			GUI.enabled = ( !global_overwrite && !local_overwrite );
			if ( !GUI.enabled ) 
				if ( global_overwrite )
					label += " (overwritted by global setting)";
				else // local_overwrite
					label += " (all cooking is disabled)";
			bool changed = HAPI_GUI.toggle( name, label, ref value );
			GUI.enabled = true;

			if ( changed )
				myAsset.prTransformChangeTriggersCooks = value;
		}
	}

	private void generateAssetSettings()
	{
		GUIContent[] modes = new GUIContent[ 3 ];
		modes[ 0 ] = new GUIContent( "View" );
		modes[ 1 ] = new GUIContent( "Materials" );
		modes[ 2 ] = new GUIContent( "Cooking" );
		myAsset.prAssetSettingsCategory = GUILayout.Toolbar( myAsset.prAssetSettingsCategory, modes );

		switch ( myAsset.prAssetSettingsCategory )
		{
			case 0: generateViewSettings(); break;
			case 1: generateMaterialSettings(); break;
			case 2: generateCookingSettings(); break;
			default: Debug.LogError( "Invalid Asset Settings Tab." ); break;
		}
	}
	
	private HAPI_AssetOTL myAssetOTL;
	private Vector2 myHelpScrollPosition = new Vector2( 0.0f, 0.0f );
}
