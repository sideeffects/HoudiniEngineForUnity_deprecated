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

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class HoudiniWindowSettings : EditorWindow 
{
#if !( HAPI_ENABLE_RUNTIME )
	#pragma warning disable 0414
#endif // !( HAPI_ENABLE_RUNTIME )

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	public static void ShowWindow() 
	{
		// Show existing window instance. If one doesn't exist, make one.
		EditorWindow window = EditorWindow.GetWindow< HoudiniWindowSettings >(
			false, HoudiniConstants.HAPI_PRODUCT_SHORT_NAME + " " + HoudiniGUIUtility.mySettingsLabel );

		window.autoRepaintOnSceneChange = true;
	}
	
	public void OnGUI() 
	{
		bool gui_enable = GUI.enabled;

#if !( HAPI_ENABLE_RUNTIME )
		HoudiniGUI.help( HoudiniConstants.HAPI_UNSUPPORTED_PLATFORM_MSG, MessageType.Info );
		GUI.enabled = false;
#else
		if ( !HoudiniHost.isInstallationOk() )
		{
			HoudiniGUI.help(
				HoudiniHost.getMissingEngineInstallHelpString(), MessageType.Info );
			GUI.enabled = false;
		}
#endif // !( HAPI_ENABLE_RUNTIME )

		myUndoInfo = HoudiniHost.prHostUndoInfo;
		myScrollPosition = GUILayout.BeginScrollView( myScrollPosition );

		if ( GUILayout.Button( HoudiniGUIUtility.myRevertAllSettingsLabel ) )
		{
			if ( EditorUtility.DisplayDialog(
				"Revert all settings?",
				"Are you sure you want to revert ALL Houdini plugin settings?", 
				"Yes", "No" ) )
			{
				HoudiniHost.revertAllSettingsToDefaults();
				HoudiniHost.repaint();
			}
		}

		HoudiniGUI.separator();

		GUIContent[] modes = new GUIContent[ 6 ];
		modes[ 0 ] = new GUIContent( "General" );
		modes[ 1 ] = new GUIContent( "Materials" );
		modes[ 2 ] = new GUIContent( "Cooking" );
		modes[ 3 ] = new GUIContent( "Geometry" );
		modes[ 4 ] = new GUIContent( "Curves" );
		modes[ 5 ] = new GUIContent( "Advanced" );
		mySettingsTabSelection = GUILayout.Toolbar( mySettingsTabSelection, modes );

		switch ( mySettingsTabSelection )
		{
			case 0: generateGeneralSettings(); break;
			case 1: generateMaterialSettings(); break;
			case 2: generateCookingSettings(); break;
			case 3: generateGeometrySettings(); break;
			case 4: generateCurveSettings(); break;
			case 5: generateAdvancedSettings(); break;
			default: Debug.LogError( "Invalid Settings Tab." ); break;
		}

		GUILayout.EndScrollView();

		GUI.enabled = gui_enable;
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private
	
	private static void generateGeneralSettings()
	{
		// Pin Size
		{
			float value = HoudiniHost.prPinSize;
			bool changed = HoudiniGUI.floatField(
				"pin_size", "Pin Size", ref value, 
				myUndoInfo, ref myUndoInfo.pinSize );
			if ( changed )
			{
				HoudiniHost.prPinSize = value;
				HoudiniHost.repaint();
			}
		}

		// Pin Colour
		{
			Color value = HoudiniHost.prPinColour;
			bool changed = HoudiniGUI.colourField(
				"pin_colour", "Pin Color", ref value,
				myUndoInfo, ref myUndoInfo.pinColour );
			if ( changed )
			{
				HoudiniHost.prPinColour = value;
				HoudiniHost.repaint();
			}
		}

		// Auto pin
		{
			bool value = HoudiniHost.prAutoPinInstances;
			bool changed = HoudiniGUI.toggle(
				"auto_pin_instances", "Auto Pin Instances", 
				ref value, myUndoInfo,
				ref myUndoInfo.autoPinInstances );
			if ( changed )
			{
				HoudiniHost.prAutoPinInstances = value;
				HoudiniHost.repaint();
			}
		}

		HoudiniGUI.separator();

		// Enable Support Warnings
		{
			bool value = HoudiniHost.prEnableSupportWarnings;
			bool changed = HoudiniGUI.toggle(
				"enable_support_warnings", "Enable Support Warnings", 
				ref value, myUndoInfo,
				ref myUndoInfo.enableSupportWarnings );
			if ( changed )
				HoudiniHost.prEnableSupportWarnings = value;
		}
	}

	private static void generateMaterialSettings()
	{
		// Gamma
		{
			float value = HoudiniHost.prGamma;
			bool changed = HoudiniGUI.floatField(
				"gamma", "Gamma", ref value, 
				myUndoInfo, ref myUndoInfo.gamma );
			if ( changed )
			{
				HoudiniHost.prGamma = value;
				HoudiniHost.repaint();
			}
		}

		HoudiniGUI.separator();

		// Don't Create Texture Files
		{
			bool value = HoudiniHost.prDontCreateTextureFiles;
			bool changed = HoudiniGUI.toggle(
				"dont_create_texture_files", 
				"Don't Create Texture Files (use in-memory textures)", 
				ref value, myUndoInfo, ref myUndoInfo.dontCreateTextureFiles );
			if ( changed )
			{
				HoudiniHost.prDontCreateTextureFiles = value;
				HoudiniHost.repaint();

				EditorUtility.DisplayDialog(
					"Rebuilds Required",
					"This change will take affect for new instantiations or rebuilds.\n" +
					"A full Unity restart is recommended.", 
					"Ok" );
			}
		}

		// Extract Textures In Raw Format
		{
			bool value = HoudiniHost.prExtractTexturesInRawFormat;
			bool was_gui_enabled = GUI.enabled;
			GUI.enabled = HoudiniHost.prDontCreateTextureFiles;
			bool changed = HoudiniGUI.toggle(
				"extract_textures_in_raw_format", 
				"Extract Textures In Raw Format (only works for in-memory textures)", 
				ref value, myUndoInfo, ref myUndoInfo.extractTexturesInRawFormat );
			if ( changed )
			{
				HoudiniHost.prExtractTexturesInRawFormat = value;
				HoudiniHost.repaint();
			}
			GUI.enabled = was_gui_enabled;
		}
	}

	private static void generateCookingSettings()
	{
		// Enable Cooking
		{
			bool value = HoudiniHost.prEnableCooking;
			bool changed = HoudiniGUI.toggle(
				"enable_cooking", "Enable Cooking", ref value,
				myUndoInfo, ref myUndoInfo.enableCooking );
			if ( changed )
			{
				HoudiniHost.prEnableCooking = value;
				HoudiniHost.repaint();
			}
		}

		HoudiniGUI.separator();

		// Cooking Triggers Downstream Cooks
		{
			bool value = HoudiniHost.prCookingTriggersDownCooks;
			bool changed = HoudiniGUI.toggle(
				"cooking_triggers_downstream_cooks", 
				"Cooking Triggers Downstream Cooks", 
				ref value, myUndoInfo,
				ref myUndoInfo.cookingTriggersDownCooks );
			if ( changed )
			{
				HoudiniHost.prCookingTriggersDownCooks = value;
				HoudiniHost.repaint();
			}
		}

		// Playmode Per-Frame Cooking
		{
			bool value = HoudiniHost.prPlaymodePerFrameCooking;
			bool changed = HoudiniGUI.toggle(
				"playmode_per_frame_cooking", 
				"Playmode Per-Frame Cooking", 
				ref value, myUndoInfo,
				ref myUndoInfo.playmodePerFrameCooking );
			if ( changed )
			{
				HoudiniHost.prPlaymodePerFrameCooking = value;
				HoudiniHost.repaint();
			}
		}

		HoudiniGUI.separator();

		// Push Unity Transform To Houdini Engine
		{
			bool value = HoudiniHost.prPushUnityTransformToHoudini;
			bool changed = HoudiniGUI.toggle(
				"push_unity_transform_to_houdini", 
				"Push Unity Transform to Houdini Engine", 
				ref value, myUndoInfo,
				ref myUndoInfo.pushUnityTransformToHoudini );
			if ( changed )
			{
				HoudiniHost.prPushUnityTransformToHoudini = value;
				HoudiniHost.repaint();
			}
		}

		// Transform Change Triggers Cooks
		{
			bool value = HoudiniHost.prTransformChangeTriggersCooks;
			bool changed = HoudiniGUI.toggle(
				"transform_change_triggers_cooks", 
				"Transform Change Triggers Cooks", 
				ref value, myUndoInfo,
				ref myUndoInfo.transformChangeTriggersCooks );
			if ( changed )
			{
				HoudiniHost.prTransformChangeTriggersCooks = value;
				HoudiniHost.repaint();
			}
		}

		HoudiniGUI.separator();

		// Import Templated Geos
		{
			bool value = HoudiniHost.prImportTemplatedGeos;
			bool changed = HoudiniGUI.toggle(
				"import_templated_geos", 
				"Import Templated Geos", 
				ref value, myUndoInfo,
				ref myUndoInfo.importTemplatedGeos );
			if ( changed )
			{
				HoudiniHost.prImportTemplatedGeos = value;
				HoudiniHost.repaint();
			}
		}

		HoudiniGUI.separator();

		// Split Geos by Group
		{
			bool value = HoudiniHost.prSplitGeosByGroup;
			bool changed = HoudiniGUI.toggle(
				"split_geos_by_group",
				"Split Geos by Group",
				ref value, myUndoInfo,
				ref myUndoInfo.splitGeosByGroup );
			if ( changed )
			{
				HoudiniHost.prSplitGeosByGroup = value;
				HoudiniHost.repaint();
			
				EditorUtility.DisplayDialog(
					"Rebuild Required",
					"This change will take affect only after a full asset rebuild.",
					"Ok" );
			}
		}

		// Split Points by Vertex Attributes
		{
			bool value = HoudiniHost.prSplitPointsByVertexAttributes;
			bool changed = HoudiniGUI.toggle(
				"split_points_by_vertex_attributes",
				"Split Points by Vertex Attributes",
				ref value, myUndoInfo,
				ref myUndoInfo.splitGeosByGroup );
			if ( changed )
			{
				HoudiniHost.prSplitPointsByVertexAttributes = value;
				HoudiniHost.repaint();
			
				EditorUtility.DisplayDialog(
					"Rebuild Required",
					"This change will take affect only after a full asset rebuild.",
					"Ok" );
			}
		}
	}

	private static void generateGeometrySettings()
	{
		// Paint Brush Rate
		{
			// Everything is opposite here because the higher the number the slower
			// the paint rate and we want the user to think the higher the number
			// the FASTER the rate - so we have to invert.
			float value = HoudiniHost.prPaintBrushRate;
			bool changed = HoudiniGUI.floatField(
				"paint_brush_rate", "Paint Brush Rate", 
				ref value, 0.0f, 1.0f,
				myUndoInfo,
				ref myUndoInfo.paintBrushRate );
			if ( changed )
			{
				HoudiniHost.prPaintBrushRate = value;
				HoudiniHost.repaint();
			}
		}

		// Painting Mode Hot Key
		{
			KeyCode value = HoudiniHost.prPaintingModeHotKey;
			string[] labels = 
				System.Enum.GetValues( typeof( KeyCode ) )
					.Cast< KeyCode >()
					.Select( v => v.ToString() )
					.ToArray();
			KeyCode[] values =
				System.Enum.GetValues( typeof( KeyCode ) )
					.Cast< KeyCode >()
					.ToArray();
			bool changed = HoudiniGUI.dropdown(
				"painting_mode_hot_key", "Painting Key",
				ref value, labels, values, myUndoInfo,
				ref myUndoInfo.paintingModeHotKey );

			if ( changed )
			{
				HoudiniHost.prPaintingModeHotKey = (KeyCode) value;
				HoudiniHost.repaint();
			}
		}

		// Painting Mode Colour
		{
			Color value = HoudiniHost.prPaintingModeColour;
			bool changed = HoudiniGUI.colourField(
				"painting_mode_colour", "Painting Mode", 
				ref value, myUndoInfo, 
				ref myUndoInfo.paintingModeColour );

			if ( changed )
			{
				HoudiniHost.prPaintingModeColour = value;
				HoudiniHost.repaint();
			}
		}

		// Painting Node Switch Hot Key
		{
			KeyCode value = HoudiniHost.prPaintingNodeSwitchHotKey;
			string[] labels = 
				System.Enum.GetValues( typeof( KeyCode ) )
					.Cast< KeyCode >()
					.Select( v => v.ToString() )
					.ToArray();
			KeyCode[] values =
				System.Enum.GetValues( typeof( KeyCode ) )
					.Cast< KeyCode >()
					.ToArray();
			bool changed = HoudiniGUI.dropdown(
				"painting_node_switch_hot_key", "Node Switch Key",
				ref value, labels, values, myUndoInfo,
				ref myUndoInfo.paintingNodeSwitchHotKey );

			if ( changed )
			{
				HoudiniHost.prPaintingNodeSwitchHotKey = (KeyCode) value;
				HoudiniHost.repaint();
			}
		}

		// Painting Attribute Switch Hot Key
		{
			KeyCode value = HoudiniHost.prPaintingAttributeSwitchHotKey;
			string[] labels = 
				System.Enum.GetValues( typeof( KeyCode ) )
					.Cast< KeyCode >()
					.Select( v => v.ToString() )
					.ToArray();
			KeyCode[] values =
				System.Enum.GetValues( typeof( KeyCode ) )
					.Cast< KeyCode >()
					.ToArray();
			bool changed = HoudiniGUI.dropdown(
				"painting_attribute_switch_hot_key", "Attr. Switch Key",
				ref value, labels, values, myUndoInfo,
				ref myUndoInfo.paintingAttributeSwitchHotKey );

			if ( changed )
			{
				HoudiniHost.prPaintingAttributeSwitchHotKey = (KeyCode) value;
				HoudiniHost.repaint();
			}
		}

		// Painting Value Change Hot Key
		{
			KeyCode value = HoudiniHost.prPaintingValueChangeHotKey;
			string[] labels = 
				System.Enum.GetValues( typeof( KeyCode ) )
					.Cast< KeyCode >()
					.Select( v => v.ToString() )
					.ToArray();
			KeyCode[] values =
				System.Enum.GetValues( typeof( KeyCode ) )
					.Cast< KeyCode >()
					.ToArray();
			bool changed = HoudiniGUI.dropdown(
				"painting_value_change_hot_key", "Value Change Key",
				ref value, labels, values, myUndoInfo,
				ref myUndoInfo.paintingValueChangeHotKey );

			if ( changed )
			{
				HoudiniHost.prPaintingValueChangeHotKey = (KeyCode) value;
				HoudiniHost.repaint();
			}
		}

		// Painting Falloff Change Hot Key
		{
			KeyCode value = HoudiniHost.prPaintingFalloffChangeHotKey;
			string[] labels = 
				System.Enum.GetValues( typeof( KeyCode ) )
					.Cast< KeyCode >()
					.Select( v => v.ToString() )
					.ToArray();
			KeyCode[] values =
				System.Enum.GetValues( typeof( KeyCode ) )
					.Cast< KeyCode >()
					.ToArray();
			bool changed = HoudiniGUI.dropdown(
				"painting_falloff_change_hot_key", "Falloff Change Key",
				ref value, labels, values, myUndoInfo,
				ref myUndoInfo.paintingFalloffChangeHotKey );

			if ( changed )
			{
				HoudiniHost.prPaintingFalloffChangeHotKey = (KeyCode) value;
				HoudiniHost.repaint();
			}
		}

		HoudiniGUI.separator();

		// Adding Points Mode Hot Key
		{
			KeyCode value = HoudiniHost.prAddingPointsModeHotKey;
			string[] labels =
				System.Enum.GetValues( typeof( KeyCode ) )
					.Cast< KeyCode >()
					.Select( v => v.ToString() )
					.ToArray();
			KeyCode[] values =
				System.Enum.GetValues( typeof( KeyCode ) )
					.Cast< KeyCode >()
					.ToArray();

			bool changed = HoudiniGUI.dropdown(
				"adding_points_mode_hot_key", "Adding Points Key",
				ref value, labels, values, myUndoInfo,
				ref myUndoInfo.addingPointsModeHotKey );

			if ( changed )
			{
				HoudiniHost.prAddingPointsModeHotKey = (KeyCode) value;
				HoudiniHost.repaint();
			}
		}

		// Adding Points Mode Colour
		{
			Color value = HoudiniHost.prAddingPointsModeColour;
			bool changed = HoudiniGUI.colourField(
				"adding_ponits_mode_colour", "Adding Points Mode", 
				ref value, myUndoInfo, 
				ref myUndoInfo.addingPointsModeColour );

			if ( changed )
			{
				HoudiniHost.prAddingPointsModeColour = value;
				HoudiniHost.repaint();
			}
		}

		HoudiniGUI.separator();

		// Editing Points Mode Hot Key
		{
			KeyCode value = HoudiniHost.prEditingPointsModeHotKey;
			string[] labels =
				System.Enum.GetValues( typeof( KeyCode ) )
					.Cast< KeyCode >()
					.Select( v => v.ToString() )
					.ToArray();
			KeyCode[] values =
				System.Enum.GetValues( typeof( KeyCode ) )
					.Cast< KeyCode >()
					.ToArray();

			bool changed = HoudiniGUI.dropdown(
				"editing_points_mode_hot_key", "Editing Points Key", 
				ref value, labels, values, myUndoInfo,
				ref myUndoInfo.editingPointsModeHotKey );

			if ( changed )
			{
				HoudiniHost.prEditingPointsModeHotKey = (KeyCode) value;
				HoudiniHost.repaint();
			}
		}

		// Editing Points Mode Colour
		{
			Color value = HoudiniHost.prEditingPointsModeColour;
			bool changed = HoudiniGUI.colourField(
				"editing_ponits_mode_colour", "Editing Points Mode", 
				ref value, myUndoInfo, 
				ref myUndoInfo.editingPointsModeColour );
			if ( changed )
			{
				HoudiniHost.prEditingPointsModeColour = value;
				HoudiniHost.repaint();
			}
		}

		HoudiniGUI.separator();

		// Wireframe Colour
		{
			Color value = HoudiniHost.prWireframeColour;
			bool changed = HoudiniGUI.colourField(
				"wireframe_colour", "Wireframe", ref value,
				myUndoInfo, ref myUndoInfo.wireframeColour );
			if ( changed )
			{
				HoudiniHost.prWireframeColour = value;
				HoudiniHost.repaint();
			}
		}

		// Guide Wireframe Colour
		{
			Color value = HoudiniHost.prGuideWireframeColour;
			bool changed = HoudiniGUI.colourField(
				"guide_wireframe_colour", "Guide Wireframe", 
				ref value, myUndoInfo,
				ref myUndoInfo.guideWireframeColour );
			if ( changed )
			{
				HoudiniHost.prGuideWireframeColour = value;
				HoudiniHost.repaint();
			}
		}

		// Unselectable Guide Wireframe Colour
		{
			Color value = HoudiniHost.prUnselectableGuideWireframeColour;
			bool changed = HoudiniGUI.colourField(
				"unselectable_guide_wireframe_colour",
				"Unselectable Guide",
				ref value, myUndoInfo,
				ref myUndoInfo.unselectableGuideWireframeColour );
			if ( changed )
			{
				HoudiniHost.prUnselectableGuideWireframeColour = value;
				HoudiniHost.repaint();
			}
		}

		// Unselected Guide Wireframe Colour
		{
			Color value = HoudiniHost.prUnselectedGuideWireframeColour;
			bool changed = HoudiniGUI.colourField(
				"unselected_guide_wireframe_colour",
				"Unselected Guide",
				ref value, myUndoInfo,
				ref myUndoInfo.unselectedGuideWireframeColour );
			if ( changed )
			{
				HoudiniHost.prUnselectedGuideWireframeColour = value;
				HoudiniHost.repaint();
			}
		}

		// Selected Guide Wireframe Colour
		{
			Color value = HoudiniHost.prSelectedGuideWireframeColour;
			bool changed = HoudiniGUI.colourField(
				"selected_guide_wireframe_colour",
				"Selected Guide",
				ref value, myUndoInfo,
				ref myUndoInfo.selectedGuideWireframeColour );
			if ( changed )
			{
				HoudiniHost.prSelectedGuideWireframeColour = value;
				HoudiniHost.repaint();
			}
		}

		HoudiniGUI.separator();

		// Guide Point Size
		{
			float value = HoudiniHost.prGuidePointSize;
			bool changed = HoudiniGUI.floatField(
				"guide_point_size", "Guide Point Size", 
				ref value, 4.0f, 40.0f,
				myUndoInfo,
				ref myUndoInfo.guidePointSize );
			if ( changed )
			{
				HoudiniHost.prGuidePointSize = value;
				HoudiniHost.repaint();
			}
		}

		// Min. Distance For Point Selection
		{
			float value = HoudiniHost.prMinDistanceForPointSelection;
			bool changed = HoudiniGUI.floatField(
				"min_distance_for_point_selection", 
				"Min. Distance For Point Selection", 
				ref value, 1.0f, 20.0f,
				myUndoInfo,
				ref myUndoInfo.minDistanceForPointSelection );
			if ( changed )
			{
				HoudiniHost.prMinDistanceForPointSelection = value;
				HoudiniHost.repaint();
			}
		}

		// Guide Min. Distance For Mid Point Insertion
		{
			float value = HoudiniHost.prGuideMinDistanceForMidPointInsertion;
			bool changed = HoudiniGUI.floatField(
				"guide_min_distance_for_mid_point_insertion", 
				"Guide Min. Distance For Mid Point Insertion", 
				ref value, 1.0f, 20.0f,
				myUndoInfo,
				ref myUndoInfo.guideMinDistanceForMidPointInsertion );
			if ( changed )
			{
				HoudiniHost.prGuideMinDistanceForMidPointInsertion = value;
				HoudiniHost.repaint();
			}
		}

		HoudiniGUI.separator();

		// Create Groups from Bool Attribute
		{
			bool value = HoudiniHost.prCreateGroupsFromBoolAttributes;
			bool changed = HoudiniGUI.toggle(
				"create_groups_from_bool_attributes", "Create Groups from Bool Attributes", 
				ref value, myUndoInfo,
				ref myUndoInfo.createGroupsFromBoolAttributes );
			if ( changed )
			{
				HoudiniHost.prCreateGroupsFromBoolAttributes = value;
				HoudiniHost.repaint();
			}
		}
	}

	private static void generateCurveSettings()
	{
		// Curve Primitive Type Default
		{
			int value = HoudiniHost.prCurvePrimitiveTypeDefault;
			string[] labels = { "Polygon", "NURBS", "Bezier" };
			int[] values = { 0, 1, 2 };
			bool changed = HoudiniGUI.dropdown(
				"curve_primitive_type_default", "Initial Type", 
				ref value, labels, values, myUndoInfo,
				ref myUndoInfo.curvePrimitiveTypeDefault );
			if ( changed )
				HoudiniHost.prCurvePrimitiveTypeDefault = value;
		}

		// Curve Method Default
		{
			int value = HoudiniHost.prCurveMethodDefault;
			string[] labels = { "CVs", "Breakpoints", "Freehand" };
			int[] values = { 0, 1, 2 };
			bool changed = HoudiniGUI.dropdown(
				"curve_method_default", "Initial Method", 
				ref value, labels, values, myUndoInfo,
				ref myUndoInfo.curveMethodDefault );
			if ( changed )
				HoudiniHost.prCurveMethodDefault = value;
		}
	}

	private static void generateAdvancedSettings()
	{
		if ( !myEnableAdvancedSettings )
		{
			if ( HoudiniGUI.button(
				"allow_advanced_settings", "Allow Editing of Advanced Settings" ) )
			{
				if ( EditorUtility.DisplayDialog(
					"Careful!",
					"Changing these settings can cause the Houdini Engine plugin to stop working. " +
					"Are you sure you want to edit them?", 
					"Yes", "No" ) )
				{
					myEnableAdvancedSettings = true;
				}
			}
		}
		else
		{
			if ( HoudiniGUI.button(
				"disallow_advanced_settings", "Disallow Editing of Advanced Settings" ) )
			{
				myEnableAdvancedSettings = false;
			}
		}

		HoudiniGUI.separator();

		bool gui_enabled = GUI.enabled;
		GUI.enabled = myEnableAdvancedSettings;

		// Collision Group Name
		{
			string value = HoudiniHost.prCollisionGroupName;
			bool changed = HoudiniGUI.stringField(
				"collision_group_name", "Colli. Grp.", ref value,
				myUndoInfo, ref myUndoInfo.collisionGroupName );
			if ( changed )
				HoudiniHost.prCollisionGroupName = value;
		}

		// Rendered Collision Group Name
		{
			string value = HoudiniHost.prRenderedCollisionGroupName;
			bool changed = HoudiniGUI.stringField(
				"rendered_collision_group_name", 
				"Rendered Colli. Grp.", ref value, myUndoInfo, 
				ref myUndoInfo.renderedCollisionGroupName );
			if ( changed )
				HoudiniHost.prRenderedCollisionGroupName = value;
		}

		HoudiniGUI.separator();

		// Unity Material Attrib Name
		{
			string value = HoudiniHost.prUnityMaterialAttribName;
			bool changed = HoudiniGUI.stringField(
				"unity_material_attrib_name", 
				"Unity Mat. Attrib.", 
				ref value, myUndoInfo, 
				ref myUndoInfo.unityMaterialAttribName );
			if ( changed )
				HoudiniHost.prUnityMaterialAttribName = value;
		}

		// Unity Sub Material Name Attrib Name
		{
			string value = HoudiniHost.prUnitySubMaterialNameAttribName;
			bool changed = HoudiniGUI.stringField(
				"unity_sub_material_name_attrib_name", 
				"Unity SubMat. Name Attrib.", 
				ref value, myUndoInfo,
				ref myUndoInfo.unitySubMaterialNameAttribName );
			if ( changed )
				HoudiniHost.prUnitySubMaterialNameAttribName = value;
		}

		// Unity Sub Material Index Attrib Name
		{
			string value = HoudiniHost.prUnitySubMaterialIndexAttribName;
			bool changed = HoudiniGUI.stringField(
				"unity_sub_material_index_attrib_name", 
				"Unity SubMat. Index Attrib.", 
				ref value, myUndoInfo,
				ref myUndoInfo.unitySubMaterialIndexAttribName );
			if ( changed )
				HoudiniHost.prUnitySubMaterialIndexAttribName = value;
		}

		HoudiniGUI.separator();

		// Unity Tag Attrib Name
		{
			string value = HoudiniHost.prUnityTagAttribName;
			bool changed = HoudiniGUI.stringField(
				"unity_tag_attrib_name", "Unity Tag Attrib.", 
				ref value, myUndoInfo, 
				ref myUndoInfo.unityTagAttribName );
			if ( changed )
				HoudiniHost.prUnityTagAttribName = value;
		}

		GUI.enabled = gui_enabled;
	}

	private static int mySettingsTabSelection = 0;
	private static Vector2 myScrollPosition;
	private static bool myEnableAdvancedSettings = false;
	private static HoudiniHostUndoInfo myUndoInfo;

#if !( HAPI_ENABLE_RUNTIME )
	#pragma warning restore 0414
#endif // !( HAPI_ENABLE_RUNTIME )
}
