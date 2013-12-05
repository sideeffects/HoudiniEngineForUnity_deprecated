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
		else if ( curr_event.commandName == "UndoRedoPerformed" )
		{
			HAPI_AssetOTLUndoInfo undo_info = ScriptableObject.Instantiate( myAssetOTL.prAssetOTLUndoInfo ) as HAPI_AssetOTLUndoInfo;

			myAsset.prShowPinnedInstances 			= undo_info.showPinnedInstances;
			myAsset.prAutoSelectAssetRootNode 		= undo_info.autoSelectAssetRootNode;
			myAsset.prHideGeometryOnLinking 		= undo_info.hideGeometryOnLinking;
			myAsset.prRenderResolution 				= undo_info.renderResolution;
			myAsset.prEnableCooking 				= undo_info.enableCooking;
			myAsset.prCookingTriggersDownCooks 		= undo_info.cookingTriggersDownCooks;
			myAsset.prPlaymodePerFrameCooking 		= undo_info.playmodePerFrameCooking;
			myAsset.prPushUnityTransformToHoudini 	= undo_info.pushUnityTransformToHoudini;
			myAsset.prTransformChangeTriggersCooks 	= undo_info.transformChangeTriggersCooks;
			myAsset.prImportTemplatedGeos 			= undo_info.importTemplatedGeos;

			if ( undo_info.isGeoVisible != myAsset.prIsGeoVisible )
			{
				myAsset.prIsGeoVisible = undo_info.isGeoVisible;

				HAPI_PartControl[] controls = 
					myAsset.GetComponentsInChildren< HAPI_PartControl >();
				foreach ( HAPI_PartControl control in controls )
				{
					if ( control.prGeoType != HAPI_GeoType.HAPI_GEOTYPE_INTERMEDIATE
					    && control.gameObject.GetComponent< MeshRenderer >() != null )
						control.gameObject.GetComponent< MeshRenderer >().enabled = myAsset.prIsGeoVisible;
				}
			}
			if ( undo_info.materialShaderType != myAsset.prMaterialShaderType || 
			     undo_info.showOnlyVertexColours != myAsset.prShowOnlyVertexColours )
			{
				myAsset.prMaterialShaderType = undo_info.materialShaderType;
				myAsset.prShowOnlyVertexColours = undo_info.showOnlyVertexColours;

				HAPI_AssetUtility.reApplyMaterials( myAsset );
			}
			if ( undo_info.generateTangents != myAsset.prGenerateTangents )
			{
				myAsset.prGenerateTangents = undo_info.generateTangents;
				myAssetOTL.build( true, false, false, true, myAsset.prCookingTriggersDownCooks, true );
			}
		}

		base.OnInspectorGUI();

		///////////////////////////////////////////////////////////////////////
		// Draw Game Object Controls
		

		myAssetOTL.prShowHoudiniControls 
			= HAPI_GUI.foldout( "Houdini Controls", myAssetOTL.prShowHoudiniControls, true );
		if ( myAssetOTL.prShowHoudiniControls ) 
		{
			if ( !myAssetOTL.isPrefab() )
			{
				if ( GUILayout.Button( "Rebuild" ) ) 
					myAssetOTL.buildAll();
	
				if ( GUILayout.Button( "Recook" ) )
					myAssetOTL.buildClientSide();
			}
			
			string path = myAssetOTL.prAssetPath;
			bool gui_enabled = GUI.enabled;
			GUI.enabled = gui_enabled && !myAssetOTL.isPrefab();
			bool file_path_changed = HAPI_GUI.fileField( "otl_path", "OTL Path", ref myDelayBuild, ref path );
			GUI.enabled = gui_enabled;
			if ( file_path_changed )
			{
				myParmChanges			|= file_path_changed;
				myAssetOTL.prAssetPath	 = path;
				myReloadAsset			 = true;
				
				// if asset is a prefab instance than break connection to prefab
				if ( myAssetOTL.isPrefabInstance() )
				{
					PrefabUtility.DisconnectPrefabInstance( myAssetOTL.gameObject );
				}
			}

			if ( !myAssetOTL.isPrefab() && GUILayout.Button( "Bake" ) )
				myAssetOTL.bakeAsset();
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
		
		if( !myAssetOTL.isPrefab() )
		{
			myAssetOTL.prShowBakeOptions = HAPI_GUI.foldout( "Bake Animations", myAssetOTL.prShowBakeOptions, true );
			if ( myAssetOTL.prShowBakeOptions )
				generateAssetBakeControls();
		}

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
		// If the asset is a prefab instance then get the prefab parent in order to determine
		// what view settings need to be bold
		HAPI_Asset prefab_asset = myAsset.getParentPrefabAsset();
		
		// Show Geometries
		{
			bool value = myAsset.prIsGeoVisible;
			bool is_bold = prefab_asset && prefab_asset.prIsGeoVisible != value;
			string label = "Show Geometries";
			bool changed = HAPI_GUI.toggle( "show_geometries", label, is_bold, ref value );
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

				// record undo info
				Undo.RecordObject( myAssetOTL.prAssetOTLUndoInfo, label );
				myAssetOTL.prAssetOTLUndoInfo.isGeoVisible = value;
			}
		}
		
		// Show Pinned Instances
		{
			bool value = myAsset.prShowPinnedInstances;
			bool is_bold = prefab_asset && prefab_asset.prShowPinnedInstances != value;
			string label = "Show Pinned Instances";
			bool changed = HAPI_GUI.toggle( "show_pinned_instances", label, is_bold, ref value );
			if ( changed )
			{
				myAsset.prShowPinnedInstances = value;

				// record undo info
				Undo.RecordObject( myAssetOTL.prAssetOTLUndoInfo, label );
				myAssetOTL.prAssetOTLUndoInfo.showPinnedInstances = value;
			}
		}

		// Auto Select Asset Root Node Toggle
		{
			bool value		= myAsset.prAutoSelectAssetRootNode;
			bool is_bold 	= prefab_asset && prefab_asset.prAutoSelectAssetRootNode != value;
			string name		= "auto_select_asset_root_node";
			string label	= "Auto Select Asset Root Node";

			GUI.enabled = ( HAPI_Host.prAutoSelectAssetRootNode == HAPI_Host.myDefaultAutoSelectAssetRootNode );
			if ( !GUI.enabled ) 
				label += " (overwritted by global setting)";
			bool changed = HAPI_GUI.toggle( name, label, is_bold, ref value );
			GUI.enabled = true;

			if ( changed )
			{
				myAsset.prAutoSelectAssetRootNode = value;

				// record undo info
				Undo.RecordObject( myAssetOTL.prAssetOTLUndoInfo, label );
				myAssetOTL.prAssetOTLUndoInfo.autoSelectAssetRootNode = value;
			}
		}
		
		// Hide When Fed to Other Asset
		{
			bool value		= myAsset.prHideGeometryOnLinking;
			bool is_bold 	= prefab_asset && prefab_asset.prHideGeometryOnLinking != value;
			string name		= "hide_geometry_on_linking";
			string label	= "Hide Geometry On Linking";

			GUI.enabled = ( HAPI_Host.prHideGeometryOnLinking == HAPI_Host.myDefaultHideGeometryOnLinking );
			if ( !GUI.enabled ) 
				label += " (overwritted by global setting)";
			bool changed = HAPI_GUI.toggle( name, label, is_bold, ref value );
			GUI.enabled = true;

			if ( changed )
			{
				myAsset.prHideGeometryOnLinking = value;

				// record undo info
				Undo.RecordObject( myAssetOTL.prAssetOTLUndoInfo, label );
				myAssetOTL.prAssetOTLUndoInfo.hideGeometryOnLinking = value;
			}
		}
	}
	
	private void generateMaterialSettings()
	{
		// If the asset is a prefab instance then get the prefab parent in order to determine
		// what material settings need to be bold
		HAPI_Asset prefab_asset = myAsset.getParentPrefabAsset();
		
		if ( GUILayout.Button( "Re-Render" ) ) 
		{
			HAPI_AssetUtility.reApplyMaterials( myAsset );
		}

		// Material Shader Type
		{
			int value = (int) myAsset.prMaterialShaderType;
			bool is_bold = prefab_asset && (int) prefab_asset.prMaterialShaderType != value;
			string label = "Material Renderer";
			string[] labels = { "OpenGL", "Houdini Mantra Renderer" };
			int[] values = { 0, 1 };
			bool changed = HAPI_GUI.dropdown( "material_renderer", label, 
											  ref value, labels, is_bold, values );
			if ( changed )
			{
				myAsset.prMaterialShaderType = (HAPI_ShaderType) value;
				HAPI_AssetUtility.reApplyMaterials( myAsset );

				// record undo info
				Undo.RecordObject( myAssetOTL.prAssetOTLUndoInfo, label );
				myAssetOTL.prAssetOTLUndoInfo.materialShaderType = (HAPI_ShaderType) value;
			}
		}

		// Render Resolution
		{
			bool delay_build = false;
			int[] values = new int[ 2 ];
			values[ 0 ] = (int) myAsset.prRenderResolution[ 0 ];
			values[ 1 ] = (int) myAsset.prRenderResolution[ 1 ];
			string label = "Render Resolution";
			HAPI_GUIParm gui_parm = new HAPI_GUIParm( "render_resolution", label, 2 );
			gui_parm.isBold = prefab_asset && 
							  (int) prefab_asset.prRenderResolution[ 0 ] != values[ 0 ] &&
							  (int) prefab_asset.prRenderResolution[ 1 ] != values[ 1 ];
			bool changed = HAPI_GUI.intField( ref gui_parm, ref delay_build, ref values );
			if ( changed )
			{
				Vector2 new_resolution = new Vector2( (float) values[ 0 ], (float) values[ 1 ] );
				myAsset.prRenderResolution = new_resolution;

				// record undo info
				Undo.RecordObject( myAssetOTL.prAssetOTLUndoInfo, label );
				myAssetOTL.prAssetOTLUndoInfo.renderResolution = new_resolution;
			}
		}

		// Show Vertex Colours
		{
			bool value = myAsset.prShowOnlyVertexColours;
			bool is_bold = prefab_asset && prefab_asset.prShowOnlyVertexColours != value;
			string label = "Show Only Vertex Colors";
			bool changed = HAPI_GUI.toggle( "show_only_vertex_colours", label, is_bold, ref value );
			if ( changed )
			{
				myAsset.prShowOnlyVertexColours = value;
				HAPI_AssetUtility.reApplyMaterials( myAsset );

				// record undo info
				Undo.RecordObject( myAssetOTL.prAssetOTLUndoInfo, label );
				myAssetOTL.prAssetOTLUndoInfo.showOnlyVertexColours = value;
			}
		}

		// Generate Tangents
		{
			bool value		= myAsset.prGenerateTangents;
			bool is_bold 	= prefab_asset && prefab_asset.prGenerateTangents != value;
			string name		= "generate_tangents";
			string label	= "Generate Tangents";

			GUI.enabled = HAPI_Host.isGenerateTangentsDefault();
			if ( !GUI.enabled ) 
				label += " (overwritted by global setting)";
			bool changed = HAPI_GUI.toggle( name, label, is_bold, ref value );
			GUI.enabled = true;

			if ( changed )
			{
				myAsset.prGenerateTangents = value;
				myAssetOTL.build( true, false, false, true, myAsset.prCookingTriggersDownCooks, true );

				// record undo info
				Undo.RecordObject( myAssetOTL.prAssetOTLUndoInfo, label );
				myAssetOTL.prAssetOTLUndoInfo.generateTangents = value;
			}
		}
	}

	private void generateCookingSettings()
	{
		// If the asset is a prefab instance then get the prefab parent in order to determine
		// what material settings need to be bold
		HAPI_Asset prefab_asset = myAsset.getParentPrefabAsset();
		
		// Enable Cooking Toggle
		{
			bool value		= myAsset.prEnableCooking;
			bool is_bold 	= prefab_asset && prefab_asset.prEnableCooking != value;
			string name		= "enable_cooking";
			string label	= "Enable Cooking";

			GUI.enabled = ( HAPI_Host.prEnableCooking == HAPI_Host.myDefaultEnableCooking );
			if ( !GUI.enabled ) 
				label += " (overwritted by global setting)";
			bool changed = HAPI_GUI.toggle( name, label, is_bold, ref value );
			GUI.enabled = true;

			if ( changed )
			{
				myAsset.prEnableCooking = value;

				// record undo info
				Undo.RecordObject( myAssetOTL.prAssetOTLUndoInfo, label );
				myAssetOTL.prAssetOTLUndoInfo.enableCooking = value;
			}
		}

		HAPI_GUI.separator();

		// Cooking Triggers Downstream Cooks Toggle
		{
			bool value		= myAsset.prCookingTriggersDownCooks;
			bool is_bold 	= prefab_asset && prefab_asset.prCookingTriggersDownCooks != value;
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
			bool changed = HAPI_GUI.toggle( name, label, is_bold, ref value );
			GUI.enabled = true;

			if ( changed )
			{
				myAsset.prCookingTriggersDownCooks = value;

				// record undo info
				Undo.RecordObject( myAssetOTL.prAssetOTLUndoInfo, label );
				myAssetOTL.prAssetOTLUndoInfo.cookingTriggersDownCooks = value;
			}
		}

		// Playmode Per-Frame Cooking Toggle
		{
			bool value		= myAsset.prPlaymodePerFrameCooking;
			bool is_bold 	= prefab_asset && prefab_asset.prPlaymodePerFrameCooking != value;
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
			bool changed = HAPI_GUI.toggle( name, label, is_bold, ref value );
			GUI.enabled = true;

			if ( changed )
			{
				myAsset.prPlaymodePerFrameCooking = value;

				// record undo info
				Undo.RecordObject( myAssetOTL.prAssetOTLUndoInfo, label );
				myAssetOTL.prAssetOTLUndoInfo.playmodePerFrameCooking = value;
			}
		}

		HAPI_GUI.separator();

		// Push Unity Transform To Houdini Engine Toggle
		{
			bool value		= myAsset.prPushUnityTransformToHoudini;
			bool is_bold 	= prefab_asset && prefab_asset.prPushUnityTransformToHoudini != value;
			string name		= "push_unity_transform_to_houdini_engine";
			string label	= "Push Unity Transform To Houdini Engine";

			GUI.enabled = ( HAPI_Host.prPushUnityTransformToHoudini == 
							HAPI_Host.myDefaultPushUnityTransformToHoudini );
			if ( !GUI.enabled ) 
				label += " (overwritted by global setting)";
			bool changed = HAPI_GUI.toggle( name, label, is_bold, ref value );
			GUI.enabled = true;

			if ( changed )
			{
				myAsset.prPushUnityTransformToHoudini = value;

				// record undo info
				Undo.RecordObject( myAssetOTL.prAssetOTLUndoInfo, label );
				myAssetOTL.prAssetOTLUndoInfo.pushUnityTransformToHoudini = value;
			}
		}

		// Transform Change Triggers Cooks Toggle
		{
			bool value		= myAsset.prTransformChangeTriggersCooks;
			bool is_bold 	= prefab_asset && prefab_asset.prTransformChangeTriggersCooks != value;
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
			bool changed = HAPI_GUI.toggle( name, label, is_bold, ref value );
			GUI.enabled = true;

			if ( changed )
			{
				myAsset.prTransformChangeTriggersCooks = value;

				// record undo info
				Undo.RecordObject( myAssetOTL.prAssetOTLUndoInfo, label );
				myAssetOTL.prAssetOTLUndoInfo.transformChangeTriggersCooks = value;
			}
		}

		// Import Templated Geos Toggle
		{
			bool value		= myAsset.prImportTemplatedGeos;
			bool is_bold 	= prefab_asset && prefab_asset.prImportTemplatedGeos != value;
			string name		= "import_templated_geos";
			string label	= "Import Templated Geos";
			
			bool global_overwrite = ( HAPI_Host.prImportTemplatedGeos != 
			                         HAPI_Host.myDefaultImportTemplatedGeos );
			bool local_overwrite  = ( myAsset.prEnableCooking == false );
			
			GUI.enabled = ( !global_overwrite && !local_overwrite );
			if ( !GUI.enabled ) 
				if ( global_overwrite )
					label += " (overwritted by global setting)";
			else // local_overwrite
				label += " (all cooking is disabled)";
			bool changed = HAPI_GUI.toggle( name, label, is_bold, ref value );
			GUI.enabled = true;
			
			if ( changed )
			{
				myAsset.prImportTemplatedGeos = value;

				// record undo info
				Undo.RecordObject( myAssetOTL.prAssetOTLUndoInfo, label );
				myAssetOTL.prAssetOTLUndoInfo.importTemplatedGeos = value;
			}
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
