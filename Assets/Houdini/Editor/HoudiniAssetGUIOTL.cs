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
using System.Collections.Generic;
using System.Reflection;

[ CustomEditor( typeof( HoudiniAssetOTL ) ) ]
public partial class HoudiniAssetGUIOTL : HoudiniAssetGUI 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	public override void OnEnable() 
	{
		base.OnEnable();
		myAssetOTL = myAsset as HoudiniAssetOTL;
	}

	public override void OnDisable()
	{
		base.OnDisable();

		if ( myGeoAttributeManager != null && myAssetOTL.prActiveAttributeManager != null )
		{
			myGeoAttributeManager.changeMode( HoudiniGeoAttributeManager.Mode.NONE );
			myGeoAttributeManagerGUI = null;
		}
	}
	
	public override void OnInspectorGUI() 
	{
		base.OnInspectorGUI();

		bool gui_enable = GUI.enabled;
		
		// We can only build or do anything if we can link to our libraries.
#if !( HAPI_ENABLE_RUNTIME )
		GUI.enabled = false;
#else
		if ( !HoudiniHost.isInstallationOk() )
			GUI.enabled = false;
#endif // !( HAPI_ENABLE_RUNTIME )

		///////////////////////////////////////////////////////////////////////
		// Draw Game Object Controls
		

		myAsset.prShowHoudiniControls
			= HoudiniGUI.foldout( "Houdini Controls", myAsset.prShowHoudiniControls, true );
		if ( myAsset.prShowHoudiniControls )
		{
			if ( GUILayout.Button( "Rebuild" ) )
			{
				myGeoAttributeManagerGUI = null;
				myAsset.buildAll();
				if ( myAsset.prGeoInputCount > 0 || myAsset.prTransformInputCount > 0 )
					myAsset.buildClientSide();
			}
	
			if ( GUILayout.Button( "Recook" ) )
				myAsset.buildClientSide();

			if ( GUILayout.Button( "Bake" ) )
				myAsset.bakeAsset();
		}

		// Draw Help Pane
		myAsset.prShowHelp = HoudiniGUI.foldout( "Asset Help", myAsset.prShowHelp, true );
		if ( myAsset.prShowHelp )
			drawHelpBox( myAsset.prAssetHelp );
		
		///////////////////////////////////////////////////////////////////////
		// Draw Asset Settings
		// These don't affect the asset directly so they don't trigger rebuilds.

		myAsset.prShowAssetSettings = HoudiniGUI.foldout( "Asset Settings", myAsset.prShowAssetSettings, true );
		if ( myAsset.prShowAssetSettings )
			generateAssetSettings();

		///////////////////////////////////////////////////////////////////////
		// Draw Baking Controls

		myAsset.prShowBakeOptions = HoudiniGUI.foldout( "Bake Animations", myAssetOTL.prShowBakeOptions, true );
		if ( myAsset.prShowBakeOptions )
			generateAssetBakeControls();

		GUI.enabled = gui_enable;
	}

	public override void OnSceneGUI()
	{
		base.OnSceneGUI();

		// If no active attribute manager is set yet, set it.
		if ( myAssetOTL.prEditPaintGeos.Count > 0 && myGeoAttributeManagerGUI == null )
		{
			myGeoAttributeManager = myAssetOTL.prActiveAttributeManager;
			myGeoAttributeManagerGUI = new HoudiniGeoAttributeManagerGUI( myGeoAttributeManager );
		}

		// If we have an active attribute manager then go ahead and draw its Scene GUI.
		if ( myGeoAttributeManagerGUI != null )
		{
			// First, get the current active paint geo index, then call the manager's
			// OnSceneGUI function to draw the GUI and get back the new active paint
			// geo index.
			int current_active_edit_paint_geo_index =
				myAssetOTL.prEditPaintGeos.FindIndex(
					delegate ( HoudiniGeoControl g ) {
						return System.Object.ReferenceEquals(
							g.prGeoAttributeManager, myAssetOTL.prActiveAttributeManager ); } );
			int new_active_edit_paint_geo_index =
				myGeoAttributeManagerGUI.OnSceneGUI(
					"Intermediate Result",
					current_active_edit_paint_geo_index,
					myAssetOTL.prEditPaintGeos.ConvertAll< string >( x => x.prGeoName ).ToArray() );

			// If the new active paint geo index is different than the old one we need to
			// switch attribute managers.
			if ( new_active_edit_paint_geo_index != current_active_edit_paint_geo_index )
			{
				// Save the current mode on the current attribute manager and restore
				// its mode to NONE so that it properly hides its geometry.
				HoudiniGeoAttributeManager.Mode current_mode =
					myAssetOTL.prActiveAttributeManager.prCurrentMode;
				myAssetOTL.prActiveAttributeManager.changeMode(
					HoudiniGeoAttributeManager.Mode.NONE );

				// Switch to the new attribute manager.
				myAssetOTL.prActiveAttributeManager =
					myAssetOTL.prEditPaintGeos[ new_active_edit_paint_geo_index ].prGeoAttributeManager;

				// Change the new attribute manager's mode to the previous attribute manager's mode.
				// This is important so that we have a smooth transition between attribute managers
				// and so that the new attribute manager's geo is unhidden.
				myAssetOTL.prActiveAttributeManager.changeMode( current_mode );

				// Update our local attribute manager pointer with the new attribute manager
				// and create a new attribute manager GUI for it.
				myGeoAttributeManager = myAssetOTL.prActiveAttributeManager; 
				myGeoAttributeManagerGUI = new HoudiniGeoAttributeManagerGUI( myGeoAttributeManager );
			}

			// If the value has changed (something was painted) and we have live updates enabled,
			// apply the modification on the mesh itself and cook the asset.
			if ( myGeoAttributeManager != null &&
				myGeoAttributeManager.prHasChanged &&
				myGeoAttributeManager.prLiveUpdates )
			{
				myGeoAttributeManager.prHasChanged = false;

				HoudiniPartControl part_control =
					myAssetOTL
						.prEditPaintGeos[ new_active_edit_paint_geo_index ]
						.prParts[ 0 ]
						.GetComponent< HoudiniPartControl >();
				Mesh mesh = part_control.GetComponent< MeshFilter >().sharedMesh;
				HoudiniAssetUtility.setMesh(
					part_control.prAssetId, part_control.prObjectId, part_control.prGeoId,
					ref mesh, part_control, myGeoAttributeManager );
				myAssetOTL.buildClientSide();
			}
		}

		handlesOnSceneGUI();
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private
	
	private void generateAssetBakeControls()
	{
		// Start Time
		{
			float value = myAsset.prBakeStartTime;
			bool changed = HoudiniGUI.floatField(
				"bake_start_time", "Start Time", ref value, 
				myUndoInfo, ref myUndoInfo.bakeStartTime );

			if ( changed )
				myAsset.prBakeStartTime = value;
		}
		
		// End Time
		{
			float value = myAsset.prBakeEndTime;
			bool changed = HoudiniGUI.floatField(
				"bake_end_time", "End Time", ref value, 
				myUndoInfo, ref myUndoInfo.bakeEndTime );
			if ( changed )
				myAsset.prBakeEndTime = value;
		}
		
		// Samples per second
		{
			int value = myAsset.prBakeSamplesPerSecond;
			bool changed = HoudiniGUI.intField(
				"bake_samples_per_second", "Samples Per Second", ref value,
				1, 120, myUndoInfo, ref myUndoInfo.bakeSamplesPerSecond );

			if ( changed )
				myAsset.prBakeSamplesPerSecond = value;
		}
		
		if ( GUILayout.Button( "Bake Animation" ) ) 
		{
			HoudiniProgressBar progress_bar = new HoudiniProgressBar();
			progress_bar.prUseDelay = false;
			myAsset.bakeAnimations(
				myAsset.prBakeStartTime, 
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
		createToggleForProperty(
			"show_geometries", "Show Geometries", "prIsGeoVisible", 
			ref myUndoInfo.isGeoVisible, () => myAsset.applyGeoVisibilityToParts() );
		
		// Show Pinned Instances
		createToggleForProperty(
			"show_pinned_instances", "Show Pinned Instances", "prShowPinnedInstances", 
			ref myUndoInfo.showPinnedInstances, null );
	}
	
	private void generateMaterialSettings()
	{	
		if ( GUILayout.Button( "Re-Render" ) ) 
		{
			HoudiniAssetUtility.reApplyMaterials( myAsset );
		}

		// Show Vertex Colours
		createToggleForProperty(
			"show_only_vertex_colours", "Show Only Vertex Colors", 
			"prShowOnlyVertexColours", ref myUndoInfo.showOnlyVertexColours,
			() => HoudiniAssetUtility.reApplyMaterials( myAsset ) );

		// Generate UVs
		createToggleForProperty(
			"generate_uvs", "Generate UVs", "prGenerateUVs",
			ref myUndoInfo.generateUVs,
			() => myAssetOTL.build(
				true,	// reload_asset
				false,	// unload_asset_first
				false,	// serialization_recovery_only
				true,	// force_reconnect
				false,	// is_duplication
				myAsset.prCookingTriggersDownCooks,
				true	// use_delay_for_progress_bar
			), false );

		// Generate Lightmap UV2s
		createToggleForProperty(
			"generate_lightmapuv2s", "Generate Lightmap UVs", "prGenerateLightmapUV2s",
			ref myUndoInfo.generateLightmapUV2s,
			() => myAssetOTL.build(
				true,	// reload_asset
				false,	// unload_asset_first
				false,	// serialization_recovery_only
				true,	// force_reconnect
				false,	// is_duplication
				myAsset.prCookingTriggersDownCooks,
				true	// use_delay_for_progress_bar
			), false );

		// Generate Tangents
		createToggleForProperty(
			"generate_tangents", "Generate Tangents", "prGenerateTangents",
			ref myUndoInfo.generateTangents,
			() => myAssetOTL.build(
				true,	// reload_asset
				false,	// unload_asset_first
				false,	// serialization_recovery_only
				true,	// force_reconnect
				false,	// is_duplication
				myAsset.prCookingTriggersDownCooks,
				true	// use_delay_for_progress_bar
			), false );
	}

	private void generateCookingSettings()
	{
		// Enable Cooking Toggle
		createToggleForProperty(
			"enable_cooking", "Enable Cooking", "prEnableCooking",
			ref myUndoInfo.enableCooking, null, !HoudiniHost.isEnableCookingDefault() );

		HoudiniGUI.separator();

		// Cooking Triggers Downstream Cooks Toggle
		createToggleForProperty(
			"cooking_triggers_downstream_cooks", "Cooking Triggers Downstream Cooks", 
			"prCookingTriggersDownCooks", ref myUndoInfo.cookingTriggersDownCooks,
			null, !HoudiniHost.isCookingTriggersDownCooksDefault(),
			!myAsset.prEnableCooking, " (all cooking is disabled)" );

		// Playmode Per-Frame Cooking Toggle
		createToggleForProperty(
			"playmode_per_frame_cooking", "Playmode Per-Frame Cooking", 
			"prPlaymodePerFrameCooking", ref myUndoInfo.playmodePerFrameCooking,
			null, !HoudiniHost.isPlaymodePerFrameCookingDefault(),
			!myAsset.prEnableCooking, " (all cooking is disabled)" );

		HoudiniGUI.separator();

		// Push Unity Transform To Houdini Engine Toggle
		createToggleForProperty(
			"push_unity_transform_to_houdini_engine", "Push Unity Transform To Houdini Engine", 
			"prPushUnityTransformToHoudini", ref myUndoInfo.pushUnityTransformToHoudini,
			null, !HoudiniHost.isPushUnityTransformToHoudiniDefault() );

		// Transform Change Triggers Cooks Toggle
		createToggleForProperty(
			"transform_change_triggers_cooks", "Transform Change Triggers Cooks", 
			"prTransformChangeTriggersCooks", ref myUndoInfo.transformChangeTriggersCooks,
			null, !HoudiniHost.isTransformChangeTriggersCooksDefault(),
			!myAsset.prEnableCooking, " (all cooking is disabled)" );

		HoudiniGUI.separator();

		// Import Templated Geos Toggle
		createToggleForProperty(
			"import_templated_geos", "Import Templated Geos", "prImportTemplatedGeos",
			ref myUndoInfo.importTemplatedGeos, null, !HoudiniHost.isImportTemplatedGeosDefault(),
			!myAsset.prEnableCooking, " (all cooking is disabled)" );

		HoudiniGUI.separator();

		// Split Geos by Group Toggle
		{
			createToggleForProperty(
				"split_geos_by_group_override", "Override Split Geos by Group", "prSplitGeosByGroupOverride",
				ref myUndoInfo.splitGeosByGroupOverride, null );
			createToggleForProperty(
				"split_geos_by_group", "Split Geos by Group", "prSplitGeosByGroup",
				ref myUndoInfo.splitGeosByGroup, () => EditorUtility.DisplayDialog(
					"Rebuild Required",
					"This change will take affect only after a full asset rebuild.",
					"Ok" ), false,
				!myAsset.prSplitGeosByGroupOverride, " (check the override checkbox to enable)" );
		}

		HoudiniGUI.separator();

		// Split Points By Vertex Attributes Toggle
		{
			createToggleForProperty(
				"split_points_by_vertex_attribute_override", "Override Split Points by Vertex Attributes", "prSplitPointsByVertexAttributeOverride",
				ref myUndoInfo.splitPointsByVertexAttributeOverride, null );
			createToggleForProperty(
				"split_points_by_vertex_attribute", "Split Points by Vertex Attributes", "prSplitPointsByVertexAttribute",
				ref myUndoInfo.splitPointsByVertexAttribute, () => EditorUtility.DisplayDialog(
					"Rebuild Required",
					"This change will take affect only after a full asset rebuild.",
					"Ok"), false,
				!myAsset.prSplitPointsByVertexAttributeOverride, " (check the override checkbox to enable)");
		}

		HoudiniGUI.separator();

		// Omit Part Name Enumeration
		createToggleForProperty(
			"omit_part_name_enumeration", "Omit Part Name Enumeration", 
			"prOmitPartNameEnumeration", ref myUndoInfo.omitPartNameEnumeration,
			null );
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

	private HoudiniAssetOTL myAssetOTL;
	private HoudiniGeoAttributeManager myGeoAttributeManager;
	private HoudiniGeoAttributeManagerGUI myGeoAttributeManagerGUI;
}
