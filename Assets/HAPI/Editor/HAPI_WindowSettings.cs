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
 * 
 */

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HAPI;

public class HAPI_WindowSettings : EditorWindow 
{
#if !UNITY_STANDALONE_WIN
	#pragma warning disable 0414
#endif // !UNITY_STANDALONE_WIN

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	public static void ShowWindow() 
	{
		// Show existing window instance. If one doesn't exist, make one.
		EditorWindow window = EditorWindow.GetWindow< HAPI_WindowSettings >( false, 
			HAPI_Constants.HAPI_PRODUCT_SHORT_NAME + " " + HAPI_GUIUtility.mySettingsLabel );

		window.autoRepaintOnSceneChange = true;

		if ( !HAPI.HAPI_SetPath.prIsPathSet )
		{
			HAPI.HAPI_SetPath.setPath();
			if ( !HAPI.HAPI_SetPath.prIsPathSet )
			{
				Debug.LogError( "Cannot build asset as Houdini dlls not found!" );
				return;
			}
			HAPI_Host.initialize();
		}
		
	}
	
	public void OnGUI() 
	{
#if UNITY_STANDALONE_WIN
		if ( !myEnableDraw )
			return;
		try
		{
			if ( !HAPI.HAPI_SetPath.prIsPathSet )
			{
				HAPI.HAPI_SetPath.setPath();
				if ( !HAPI.HAPI_SetPath.prIsPathSet )
				{
					Debug.LogError( "Cannot build asset as Houdini dlls not found!" );
					return;
				}
				myEnableDraw = HAPI_Host.initialize();
			}
		}
		catch ( HAPI_Error error )
		{
			Debug.LogError( error.ToString() );
		}
#endif // UNITY_STANDALONE_WIN

		bool gui_enable = GUI.enabled;

#if !UNITY_STANDALONE_WIN
		HAPI_GUI.help( HAPI_GUIUtility.myPlatformUnsupportedMessage, MessageType.Info );
		GUI.enabled = false;
#endif // !UNITY_STANDALONE_WIN

		myUndoInfo = HAPI_Host.prHostUndoInfo;
		myScrollPosition = GUILayout.BeginScrollView( myScrollPosition );

		if ( GUILayout.Button( HAPI_GUIUtility.myRevertAllSettingsLabel ) )
		{
			if ( EditorUtility.DisplayDialog(	"Revert all settings?",
												"Are you sure you want to revert ALL Houdini plugin settings?", 
												"Yes", "No" ) )
			{
				HAPI_Host.revertAllSettingsToDefaults();
				HAPI_Host.repaint();
			}
		}

		HAPI_GUI.separator();

		GUIContent[] modes = new GUIContent[ 5 ];
		modes[ 0 ] = new GUIContent( "General" );
		modes[ 1 ] = new GUIContent( "Materials" );
		modes[ 2 ] = new GUIContent( "Cooking" );
		modes[ 3 ] = new GUIContent( "Geometry" );
		modes[ 4 ] = new GUIContent( "Curves" );
		mySettingsTabSelection = GUILayout.Toolbar( mySettingsTabSelection, modes );

		switch ( mySettingsTabSelection )
		{
			case 0: generateGeneralSettings(); break;
			case 1: generateMaterialSettings(); break;
			case 2: generateCookingSettings(); break;
			case 3: generateGeometrySettings(); break;
			case 4: generateCurveSettings(); break;
			default: Debug.LogError( "Invalid Settings Tab." ); break;
		}

		GUILayout.EndScrollView();

		GUI.enabled = gui_enable;
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private
	
	private static void generateGeneralSettings()
	{
		// Collision Group Name
		{
			string value = HAPI_Host.prCollisionGroupName;
			bool changed = HAPI_GUI.stringField( "collision_group_name", "Colli. Grp.", ref value,
			                                     myUndoInfo, ref myUndoInfo.collisionGroupName );
			if ( changed )
				HAPI_Host.prCollisionGroupName = value;
		}

		// Rendered Collision Group Name
		{
			string value = HAPI_Host.prRenderedCollisionGroupName;
			bool changed = HAPI_GUI.stringField( "rendered_collision_group_name", 
												 "Rendered Colli. Grp.", ref value, myUndoInfo, 
			                                     ref myUndoInfo.renderedCollisionGroupName );
			if ( changed )
				HAPI_Host.prRenderedCollisionGroupName = value;
		}

		HAPI_GUI.separator();

		// Pin Size
		{
			float value = HAPI_Host.prPinSize;
			bool changed = HAPI_GUI.floatField( "pin_size", "Pin Size", ref value, 
			                                    myUndoInfo, ref myUndoInfo.pinSize );
			if ( changed )
			{
				HAPI_Host.prPinSize = value;
				HAPI_Host.repaint();
			}
		}

		// Pin Colour
		{
			Color value = HAPI_Host.prPinColour;
			bool changed = HAPI_GUI.colourField( "pin_colour", "Pin Color", ref value,
			                                     myUndoInfo, ref myUndoInfo.pinColour );
			if ( changed )
			{
				HAPI_Host.prPinColour = value;
				HAPI_Host.repaint();
			}
		}

		// Auto pin
		{
			bool value = HAPI_Host.prAutoPinInstances;
			bool changed = HAPI_GUI.toggle( "auto_pin_instances", "Auto Pin Instances", 
			                                ref value, myUndoInfo,
			                                ref myUndoInfo.autoPinInstances );
			if ( changed )
			{
				HAPI_Host.prAutoPinInstances = value;
				HAPI_Host.repaint();
			}
		}

		HAPI_GUI.separator();

		// Enable Drag-and-Drop
		{
			bool value = HAPI_Host.prEnableDragAndDrop;
			bool changed = HAPI_GUI.toggle( "enable_drag_and_drop", "Enable Drag-and-Drop", 
			                                ref value, myUndoInfo,
			                                ref myUndoInfo.enableDragAndDrop );
			if ( changed )
			{
				HAPI_Host.prEnableDragAndDrop = value;
				HAPI_GUIUtility.setDragAndDrop( value );
			}
		}

		// Enable Support Warnings
		{
			bool value = HAPI_Host.prEnableSupportWarnings;
			bool changed = HAPI_GUI.toggle( "enable_support_warnings", "Enable Support Warnings", 
			                                ref value, myUndoInfo,
			                                ref myUndoInfo.enableSupportWarnings );
			if ( changed )
				HAPI_Host.prEnableSupportWarnings = value;
		}

		HAPI_GUI.separator();

		// Auto Select Asset Root Node
		{
			bool value = HAPI_Host.prAutoSelectAssetRootNode;
			bool changed = HAPI_GUI.toggle( "auto_select_asset_root_node", 
											"Auto Select Asset Root Node", 
			                                ref value, myUndoInfo,
			                                ref myUndoInfo.autoSelectAssetRootNode );
			if ( changed )
			{
				HAPI_Host.prAutoSelectAssetRootNode = value;
				HAPI_Host.repaint();
			}
		}

		// Hide Geometry On Linking
		{
			bool value = HAPI_Host.prHideGeometryOnLinking;
			bool changed = HAPI_GUI.toggle( "hide_geometry_on_linking", 
			                                "Hide Geometry On Linking", 
			                                ref value, myUndoInfo, 
			                                ref myUndoInfo.hideGeometryOnLinking );
			if ( changed )
			{
				HAPI_Host.prHideGeometryOnLinking = value;
				HAPI_Host.repaint();
			}
		}

		HAPI_GUI.separator();

		// Enable particles
		{
			bool value = HAPI_Host.prEnablePointsAsParticles;
			bool changed = HAPI_GUI.toggle( "enable_points_as_particles",
											"Create Points as Particles", 
											ref value, myUndoInfo,
			                                ref myUndoInfo.enablePointsAsParticles );
			if ( changed )
			{
				HAPI_Host.prEnablePointsAsParticles = value;
			}
		}
	}

	private static void generateMaterialSettings()
	{
		// Unity Material Attrib Name
		{
			string value = HAPI_Host.prUnityMaterialAttribName;
			bool changed = HAPI_GUI.stringField( "unity_material_attrib_name", 
												 "Unity Mat. Attrib.", 
			                                     ref value, myUndoInfo, 
			                                     ref myUndoInfo.unityMaterialAttribName );
			if ( changed )
				HAPI_Host.prUnityMaterialAttribName = value;
		}

		// Unity Sub Material Name Attrib Name
		{
			string value = HAPI_Host.prUnitySubMaterialNameAttribName;
			bool changed = HAPI_GUI.stringField( "unity_sub_material_name_attrib_name", 
												 "Unity SubMat. Name Attrib.", 
			                                     ref value, myUndoInfo,
			                                     ref myUndoInfo.unitySubMaterialNameAttribName );
			if ( changed )
				HAPI_Host.prUnitySubMaterialNameAttribName = value;
		}

		// Unity Sub Material Index Attrib Name
		{
			string value = HAPI_Host.prUnitySubMaterialIndexAttribName;
			bool changed = HAPI_GUI.stringField( "unity_sub_material_index_attrib_name", 
												 "Unity SubMat. Index Attrib.", 
			                                     ref value, myUndoInfo,
			                                     ref myUndoInfo.unitySubMaterialIndexAttribName );
			if ( changed )
				HAPI_Host.prUnitySubMaterialIndexAttribName = value;
		}

		HAPI_GUI.separator();

		// Generate Tangents
		{
			bool value = HAPI_Host.prGenerateTangents;
			bool changed = HAPI_GUI.toggle( "generate_tangents", "Generate Tangents", 
			                                ref value, myUndoInfo,
			                                ref myUndoInfo.generateTangents );
			if ( changed )
			{
				HAPI_Host.prGenerateTangents = value;
				HAPI_Host.repaint();

				EditorUtility.DisplayDialog(	"Rebuilds Required",
												"You will need to rebuild existing assets for tangents " +
												"to be removed/generated.", 
												"Ok" );
			}
		}

		HAPI_GUI.separator();

		// Don't Create Texture Files
		{
			bool value = HAPI_Host.prDontCreateTextureFiles;
			bool changed = HAPI_GUI.toggle( "dont_create_texture_files", 
			                                "Don't Create Texture Files (use in-memory textures)", 
			                                ref value, myUndoInfo, ref myUndoInfo.dontCreateTextureFiles );
			if ( changed )
			{
				HAPI_Host.prDontCreateTextureFiles = value;
				HAPI_Host.repaint();

				EditorUtility.DisplayDialog(	"Rebuilds Required",
												"This change will take affect for new instantiations or rebuilds.\n" +
												"A full Unity restart is recommended.", 
												"Ok" );
			}
		}

		// Extract Textures In Raw Format
		{
			bool value = HAPI_Host.prExtractTexturesInRawFormat;
			bool was_gui_enabled = GUI.enabled;
			GUI.enabled = HAPI_Host.prDontCreateTextureFiles;
			bool changed = HAPI_GUI.toggle( "extract_textures_in_raw_format", 
			                                "Extract Textures In Raw Format (only works for in-memory textures)", 
			                                ref value, myUndoInfo, ref myUndoInfo.extractTexturesInRawFormat );
			if ( changed )
			{
				HAPI_Host.prExtractTexturesInRawFormat = value;
				HAPI_Host.repaint();
			}
			GUI.enabled = was_gui_enabled;
		}
	}

	private static void generateCookingSettings()
	{
		// Enable Cooking
		{
			bool value = HAPI_Host.prEnableCooking;
			bool changed = HAPI_GUI.toggle( "enable_cooking", "Enable Cooking", ref value,
			                                myUndoInfo, ref myUndoInfo.enableCooking );
			if ( changed )
			{
				HAPI_Host.prEnableCooking = value;
				HAPI_Host.repaint();
			}
		}

		HAPI_GUI.separator();

		// Cooking Triggers Downstream Cooks
		{
			bool value = HAPI_Host.prCookingTriggersDownCooks;
			bool changed = HAPI_GUI.toggle( "cooking_triggers_downstream_cooks", 
											"Cooking Triggers Downstream Cooks", 
			                                ref value, myUndoInfo,
			                                ref myUndoInfo.cookingTriggersDownCooks );
			if ( changed )
			{
				HAPI_Host.prCookingTriggersDownCooks = value;
				HAPI_Host.repaint();
			}
		}

		// Playmode Per-Frame Cooking
		{
			bool value = HAPI_Host.prPlaymodePerFrameCooking;
			bool changed = HAPI_GUI.toggle( "playmode_per_frame_cooking", 
											"Playmode Per-Frame Cooking", 
			                                ref value, myUndoInfo,
			                                ref myUndoInfo.playmodePerFrameCooking );
			if ( changed )
			{
				HAPI_Host.prPlaymodePerFrameCooking = value;
				HAPI_Host.repaint();
			}
		}

		HAPI_GUI.separator();

		// Push Unity Transform To Houdini Engine
		{
			bool value = HAPI_Host.prPushUnityTransformToHoudini;
			bool changed = HAPI_GUI.toggle( "push_unity_transform_to_houdini", 
											"Push Unity Transform to Houdini Engine", 
			                                ref value, myUndoInfo,
			                                ref myUndoInfo.pushUnityTransformToHoudini );
			if ( changed )
			{
				HAPI_Host.prPushUnityTransformToHoudini = value;
				HAPI_Host.repaint();
			}
		}

		// Transform Change Triggers Cooks
		{
			bool value = HAPI_Host.prTransformChangeTriggersCooks;
			bool changed = HAPI_GUI.toggle( "transform_change_triggers_cooks", 
											"Transform Change Triggers Cooks", 
			                                ref value, myUndoInfo,
			                                ref myUndoInfo.transformChangeTriggersCooks );
			if ( changed )
			{
				HAPI_Host.prTransformChangeTriggersCooks = value;
				HAPI_Host.repaint();
			}
		}

		// Import Templated Geos
		{
			bool value = HAPI_Host.prImportTemplatedGeos;
			bool changed = HAPI_GUI.toggle( "import_templated_geos", 
			                                "Import Templated Geos", 
			                                ref value, myUndoInfo,
			                                ref myUndoInfo.importTemplatedGeos );
			if ( changed )
			{
				HAPI_Host.prImportTemplatedGeos = value;
				HAPI_Host.repaint();
			}
		}


	}

	private static void generateGeometrySettings()
	{
		// Unity Tag Attrib Name
		{
			string value = HAPI_Host.prUnityTagAttribName;
			bool changed = HAPI_GUI.stringField( "unity_tag_attrib_name", "Unity Tag Attrib.", 
			                                     ref value, myUndoInfo, 
			                                     ref myUndoInfo.unityTagAttribName );
			if ( changed )
				HAPI_Host.prUnityTagAttribName = value;
		}

		HAPI_GUI.separator();

		// Paint Brush Rate
		{
			// Everything is opposite here because the higher the number the slower
			// the paint rate and we want the user to think the higher the number
			// the FASTER the rate - so we have to invert.
			float value = HAPI_Host.prPaintBrushRate;
			bool changed = HAPI_GUI.floatField( "paint_brush_rate", "Paint Brush Rate", 
												ref value, 0.0f, 1.0f,
												myUndoInfo,
												ref myUndoInfo.paintBrushRate );
			if ( changed )
			{
				HAPI_Host.prPaintBrushRate = value;
				HAPI_Host.repaint();
			}
		}

		// Painting Mode Hot Key
		{
			KeyCode value = HAPI_Host.prPaintingModeHotKey;
			string[] labels = System.Enum.GetValues(typeof(KeyCode))
								.Cast< KeyCode >()
								.Select(v => v.ToString())
								.ToArray();
			KeyCode[] values = System.Enum.GetValues(typeof(KeyCode))
								.Cast< KeyCode >()
								.ToArray();
			bool changed = HAPI_GUI.dropdown( "painting_mode_hot_key", "Painting Key", 
											  ref value, labels, values, myUndoInfo,
			                                  ref myUndoInfo.paintingModeHotKey );
			if ( changed )
			{
				HAPI_Host.prPaintingModeHotKey = (KeyCode) value;
				HAPI_Host.repaint();
			}
		}

		// Painting Mode Colour
		{
			Color value = HAPI_Host.prPaintingModeColour;
			bool changed = HAPI_GUI.colourField( "painting_mode_colour", "Painting Mode", 
			                                     ref value, myUndoInfo, 
			                                     ref myUndoInfo.paintingModeColour );
			if ( changed )
			{
				HAPI_Host.prPaintingModeColour = value;
				HAPI_Host.repaint();
			}
		}

		HAPI_GUI.separator();

		// Adding Points Mode Hot Key
		{
			KeyCode value = HAPI_Host.prAddingPointsModeHotKey;
			string[] labels = System.Enum.GetValues(typeof(KeyCode))
								.Cast< KeyCode >()
								.Select(v => v.ToString())
								.ToArray();
			KeyCode[] values = System.Enum.GetValues(typeof(KeyCode))
								.Cast< KeyCode >()
								.ToArray();
			bool changed = HAPI_GUI.dropdown( "adding_points_mode_hot_key", "Adding Points Key", 
											  ref value, labels, values, myUndoInfo,
			                                  ref myUndoInfo.addingPointsModeHotKey );
			if ( changed )
			{
				HAPI_Host.prAddingPointsModeHotKey = (KeyCode) value;
				HAPI_Host.repaint();
			}
		}

		// Adding Points Mode Colour
		{
			Color value = HAPI_Host.prAddingPointsModeColour;
			bool changed = HAPI_GUI.colourField( "adding_ponits_mode_colour", "Adding Points Mode", 
			                                     ref value, myUndoInfo, 
			                                     ref myUndoInfo.addingPointsModeColour );
			if ( changed )
			{
				HAPI_Host.prAddingPointsModeColour = value;
				HAPI_Host.repaint();
			}
		}

		HAPI_GUI.separator();

		// Editing Points Mode Hot Key
		{
			KeyCode value = HAPI_Host.prEditingPointsModeHotKey;
			string[] labels = System.Enum.GetValues(typeof(KeyCode))
								.Cast< KeyCode >()
								.Select(v => v.ToString())
								.ToArray();
			KeyCode[] values = System.Enum.GetValues(typeof(KeyCode))
								.Cast< KeyCode >()
								.ToArray();
			bool changed = HAPI_GUI.dropdown( "editing_points_mode_hot_key", "Editing Points Key", 
											  ref value, labels, values, myUndoInfo,
			                                  ref myUndoInfo.editingPointsModeHotKey );
			if ( changed )
			{
				HAPI_Host.prEditingPointsModeHotKey = (KeyCode) value;
				HAPI_Host.repaint();
			}
		}

		// Editing Points Mode Colour
		{
			Color value = HAPI_Host.prEditingPointsModeColour;
			bool changed = HAPI_GUI.colourField( "editing_ponits_mode_colour", "Editing Points Mode", 
			                                     ref value, myUndoInfo, 
			                                     ref myUndoInfo.editingPointsModeColour );
			if ( changed )
			{
				HAPI_Host.prEditingPointsModeColour = value;
				HAPI_Host.repaint();
			}
		}

		HAPI_GUI.separator();

		// Wireframe Colour
		{
			Color value = HAPI_Host.prWireframeColour;
			bool changed = HAPI_GUI.colourField( "wireframe_colour", "Wireframe", ref value,
			                                     myUndoInfo, ref myUndoInfo.wireframeColour );
			if ( changed )
			{
				HAPI_Host.prWireframeColour = value;
				HAPI_Host.repaint();
			}
		}

		// Guide Wireframe Colour
		{
			Color value = HAPI_Host.prGuideWireframeColour;
			bool changed = HAPI_GUI.colourField( "guide_wireframe_colour", "Guide Wireframe", 
			                                     ref value, myUndoInfo,
			                                     ref myUndoInfo.guideWireframeColour );
			if ( changed )
			{
				HAPI_Host.prGuideWireframeColour = value;
				HAPI_Host.repaint();
			}
		}

		// Unselectable Guide Wireframe Colour
		{
			Color value = HAPI_Host.prUnselectableGuideWireframeColour;
			bool changed = HAPI_GUI.colourField( "unselectable_guide_wireframe_colour", 
			                                     "Unselectable Guide", 
			                                     ref value, myUndoInfo, 
			                                     ref myUndoInfo.unselectableGuideWireframeColour );
			if ( changed )
			{
				HAPI_Host.prUnselectableGuideWireframeColour = value;
				HAPI_Host.repaint();
			}
		}

		// Unselected Guide Wireframe Colour
		{
			Color value = HAPI_Host.prUnselectedGuideWireframeColour;
			bool changed = HAPI_GUI.colourField( "unselected_guide_wireframe_colour", 
			                                     "Unselected Guide", 
			                                     ref value, myUndoInfo,
			                                     ref myUndoInfo.unselectedGuideWireframeColour );
			if ( changed )
			{
				HAPI_Host.prUnselectedGuideWireframeColour = value;
				HAPI_Host.repaint();
			}
		}

		// Selected Guide Wireframe Colour
		{
			Color value = HAPI_Host.prSelectedGuideWireframeColour;
			bool changed = HAPI_GUI.colourField( "selected_guide_wireframe_colour", 
			                                     "Selected Guide", 
			                                     ref value, myUndoInfo,
			                                     ref myUndoInfo.selectedGuideWireframeColour );
			if ( changed )
			{
				HAPI_Host.prSelectedGuideWireframeColour = value;
				HAPI_Host.repaint();
			}
		}

		HAPI_GUI.separator();

		// Guide Point Size
		{
			float value = HAPI_Host.prGuidePointSize;
			bool changed = HAPI_GUI.floatField( "guide_point_size", "Guide Point Size", 
												ref value, 4.0f, 40.0f,
			                                    myUndoInfo,
			                                    ref myUndoInfo.guidePointSize );
			if ( changed )
			{
				HAPI_Host.prGuidePointSize = value;
				HAPI_Host.repaint();
			}
		}

		// Min. Distance For Point Selection
		{
			float value = HAPI_Host.prMinDistanceForPointSelection;
			bool changed = HAPI_GUI.floatField( "min_distance_for_point_selection", 
												"Min. Distance For Point Selection", 
			                                    ref value, 1.0f, 20.0f,
			                                    myUndoInfo,
			                                    ref myUndoInfo.minDistanceForPointSelection );
			if ( changed )
			{
				HAPI_Host.prMinDistanceForPointSelection = value;
				HAPI_Host.repaint();
			}
		}

		// Guide Min. Distance For Mid Point Insertion
		{
			float value = HAPI_Host.prGuideMinDistanceForMidPointInsertion;
			bool changed = HAPI_GUI.floatField( "guide_min_distance_for_mid_point_insertion", 
												"Guide Min. Distance For Mid Point Insertion", 
			                                   	ref value, 1.0f, 20.0f,
			                                   	myUndoInfo,
			                                   	ref myUndoInfo.guideMinDistanceForMidPointInsertion );
			if ( changed )
			{
				HAPI_Host.prGuideMinDistanceForMidPointInsertion = value;
				HAPI_Host.repaint();
			}
		}
	}

	private static void generateCurveSettings()
	{
		// Curve Primitive Type Default
		{
			int value = HAPI_Host.prCurvePrimitiveTypeDefault;
			string[] labels = { "Polygon", "NURBS", "Bezier" };
			int[] values = { 0, 1, 2 };
			bool changed = HAPI_GUI.dropdown( "curve_primitive_type_default", "Initial Type", 
											  ref value, labels, values, myUndoInfo,
			                                  ref myUndoInfo.curvePrimitiveTypeDefault );
			if ( changed )
				HAPI_Host.prCurvePrimitiveTypeDefault = value;
		}

		// Curve Method Default
		{
			int value = HAPI_Host.prCurveMethodDefault;
			string[] labels = { "CVs", "Breakpoints", "Freehand" };
			int[] values = { 0, 1, 2 };
			bool changed = HAPI_GUI.dropdown( "curve_method_default", "Initial Method", 
											  ref value, labels, values, myUndoInfo,
			                                  ref myUndoInfo.curveMethodDefault );
			if ( changed )
				HAPI_Host.prCurveMethodDefault = value;
		}
	}

	private static bool			myEnableDraw			= true;
	private static int			mySettingsTabSelection	= 0;
	private static Vector2 		myScrollPosition;
	private static HAPI_HostUndoInfo myUndoInfo;

#if !UNITY_STANDALONE_WIN
	#pragma warning restore 0414
#endif // !UNITY_STANDALONE_WIN
}
