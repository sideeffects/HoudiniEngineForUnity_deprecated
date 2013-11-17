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
		EditorWindow.GetWindow< HAPI_WindowSettings >( false, HAPI_Constants.HAPI_PRODUCT_SHORT_NAME + 
													   " " + HAPI_GUIUtility.mySettingsLabel );

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

		myScrollPosition = GUILayout.BeginScrollView( myScrollPosition );

		if ( GUILayout.Button( HAPI_GUIUtility.myRevertAllSettingsLabel ) )
		{
			if ( EditorUtility.DisplayDialog(	"Revert all settings?",
												"Are you sure you want to revert ALL Houdini plugin settings?", 
												"Yes", "No" ) )
			{
				HAPI_Host.revertAllSettingsToDefaults();
				HAPI_Host.myRepaintDelegate();
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
			bool changed = HAPI_GUI.stringField( "collision_group_name", "Colli. Grp.", ref value );
			if ( changed )
				HAPI_Host.prCollisionGroupName = value;
		}

		// Rendered Collision Group Name
		{
			string value = HAPI_Host.prRenderedCollisionGroupName;
			bool changed = HAPI_GUI.stringField( "rendered_collision_group_name", 
													"Rendered Colli. Grp.", ref value );
			if ( changed )
				HAPI_Host.prRenderedCollisionGroupName = value;
		}

		HAPI_GUI.separator();

		// Pin Size
		{
			float value = HAPI_Host.prPinSize;
			bool changed = HAPI_GUI.floatField( "pin_size", "Pin Size", ref value );
			if ( changed )
			{
				HAPI_Host.prPinSize = value;
				if ( HAPI_Host.myRepaintDelegate != null )
					HAPI_Host.myRepaintDelegate();
			}
		}

		// Pin Colour
		{
			Color value = HAPI_Host.prPinColour;
			bool changed = HAPI_GUI.colourField( "pin_colour", "Pin Color", ref value );
			if ( changed )
			{
				HAPI_Host.prPinColour = value;
				if ( HAPI_Host.myRepaintDelegate != null )
					HAPI_Host.myRepaintDelegate();
			}
		}

		// Auto pin
		{
			bool value = HAPI_Host.prAutoPinInstances;
			bool changed = HAPI_GUI.toggle( "auto_pin_instances", "Auto Pin Instances", ref value );
			if ( changed )
			{
				HAPI_Host.prAutoPinInstances = value;
				if ( HAPI_Host.myRepaintDelegate != null )
					HAPI_Host.myRepaintDelegate();
			}
		}

		HAPI_GUI.separator();

		// Enable Drag-and-Drop
		{
			bool value = HAPI_Host.prEnableDragAndDrop;
			bool changed = HAPI_GUI.toggle( "enable_drag_and_drop", "Enable Drag-and-Drop", ref value );
			if ( changed )
			{
				HAPI_Host.prEnableDragAndDrop = value;
				HAPI_GUIUtility.setDragAndDrop( value );
			}
		}

		// Enable Support Warnings
		{
			bool value = HAPI_Host.prEnableSupportWarnings;
			bool changed = HAPI_GUI.toggle( "enable_support_warnings", "Enable Support Warnings", ref value );
			if ( changed )
				HAPI_Host.prEnableSupportWarnings = value;
		}

		HAPI_GUI.separator();

		// Auto Select Asset Root Node
		{
			bool value = HAPI_Host.prAutoSelectAssetRootNode;
			bool changed = HAPI_GUI.toggle( "auto_select_asset_root_node", 
											"Auto Select Asset Root Node", ref value );
			if ( changed )
			{
				HAPI_Host.prAutoSelectAssetRootNode = value;
				if ( HAPI_Host.myRepaintDelegate != null )
					HAPI_Host.myRepaintDelegate();
			}
		}

		// Hide Geometry On Linking
		{
			bool value = HAPI_Host.prHideGeometryOnLinking;
			bool changed = HAPI_GUI.toggle( "hide_geometry_on_linking", "Hide Geometry On Linking", ref value );
			if ( changed )
			{
				HAPI_Host.prHideGeometryOnLinking = value;
				if ( HAPI_Host.myRepaintDelegate != null )
					HAPI_Host.myRepaintDelegate();
			}
		}

		HAPI_GUI.separator();

		// Enable particles
		{
			bool value = HAPI_Host.prEnablePointsAsParticles;
			bool changed = HAPI_GUI.toggle( "enable_points_as_particles",
											"Create Points as Particles", 
											ref value );
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
												 "Unity Mat. Attrib.", ref value );
			if ( changed )
				HAPI_Host.prUnityMaterialAttribName = value;
		}

		// Unity Sub Material Name Attrib Name
		{
			string value = HAPI_Host.prUnitySubMaterialNameAttribName;
			bool changed = HAPI_GUI.stringField( "unity_sub_material_name_attrib_name", 
												 "Unity SubMat. Name Attrib.", ref value );
			if ( changed )
				HAPI_Host.prUnitySubMaterialNameAttribName = value;
		}

		// Unity Sub Material Index Attrib Name
		{
			string value = HAPI_Host.prUnitySubMaterialIndexAttribName;
			bool changed = HAPI_GUI.stringField( "unity_sub_material_index_attrib_name", 
												 "Unity SubMat. Index Attrib.", ref value );
			if ( changed )
				HAPI_Host.prUnitySubMaterialIndexAttribName = value;
		}

		HAPI_GUI.separator();

		// Generate Tangents
		{
			bool value = HAPI_Host.prGenerateTangents;
			bool changed = HAPI_GUI.toggle( "generate_tangents", "Generate Tangents", ref value );
			if ( changed )
			{
				HAPI_Host.prGenerateTangents = value;
				if ( HAPI_Host.myRepaintDelegate != null )
					HAPI_Host.myRepaintDelegate();

				EditorUtility.DisplayDialog(	"Rebuilds Required",
												"You will need to rebuild existing assets for tangents "+
												"to be removed/generated.", 
												"Ok" );
			}
		}

		HAPI_GUI.separator();

		// Don't Create Texture Files
		{
			bool value = HAPI_Host.prDontCreateTextureFiles;
			bool changed = HAPI_GUI.toggle( "dont_create_texture_files", "Don't Create Texture Files (use in-memory textures)", ref value );
			if ( changed )
			{
				HAPI_Host.prDontCreateTextureFiles = value;
				if ( HAPI_Host.myRepaintDelegate != null )
					HAPI_Host.myRepaintDelegate();

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
			bool changed = HAPI_GUI.toggle( "extract_textures_in_raw_format", "Extract Textures In Raw Format (only works for in-memory textures)", ref value );
			if ( changed )
			{
				HAPI_Host.prExtractTexturesInRawFormat = value;
				if ( HAPI_Host.myRepaintDelegate != null )
					HAPI_Host.myRepaintDelegate();
			}
			GUI.enabled = was_gui_enabled;
		}
	}

	private static void generateCookingSettings()
	{
		// Enable Cooking
		{
			bool value = HAPI_Host.prEnableCooking;
			bool changed = HAPI_GUI.toggle( "enable_cooking", "Enable Cooking", ref value );
			if ( changed )
			{
				HAPI_Host.prEnableCooking = value;
				if ( HAPI_Host.myRepaintDelegate != null )
					HAPI_Host.myRepaintDelegate();
			}
		}

		HAPI_GUI.separator();

		// Cooking Triggers Downstream Cooks
		{
			bool value = HAPI_Host.prCookingTriggersDownCooks;
			bool changed = HAPI_GUI.toggle( "cooking_triggers_downstream_cooks", 
											"Cooking Triggers Downstream Cooks", ref value );
			if ( changed )
			{
				HAPI_Host.prCookingTriggersDownCooks = value;
				if ( HAPI_Host.myRepaintDelegate != null )
					HAPI_Host.myRepaintDelegate();
			}
		}

		// Playmode Per-Frame Cooking
		{
			bool value = HAPI_Host.prPlaymodePerFrameCooking;
			bool changed = HAPI_GUI.toggle( "playmode_per_frame_cooking", 
											"Playmode Per-Frame Cooking", ref value );
			if ( changed )
			{
				HAPI_Host.prPlaymodePerFrameCooking = value;
				if ( HAPI_Host.myRepaintDelegate != null )
					HAPI_Host.myRepaintDelegate();
			}
		}

		HAPI_GUI.separator();

		// Push Unity Transform To Houdini Engine
		{
			bool value = HAPI_Host.prPushUnityTransformToHoudini;
			bool changed = HAPI_GUI.toggle( "push_unity_transform_to_houdini", 
											"Push Unity Transform to Houdini Engine", ref value );
			if ( changed )
			{
				HAPI_Host.prPushUnityTransformToHoudini = value;
				if ( HAPI_Host.myRepaintDelegate != null )
					HAPI_Host.myRepaintDelegate();
			}
		}

		// Transform Change Triggers Cooks
		{
			bool value = HAPI_Host.prTransformChangeTriggersCooks;
			bool changed = HAPI_GUI.toggle( "transform_change_triggers_cooks", 
											"Transform Change Triggers Cooks", ref value );
			if ( changed )
			{
				HAPI_Host.prTransformChangeTriggersCooks = value;
				if ( HAPI_Host.myRepaintDelegate != null )
					HAPI_Host.myRepaintDelegate();
			}
		}

		// Import Templated Geos
		{
			bool value = HAPI_Host.prImportTemplatedGeos;
			bool changed = HAPI_GUI.toggle( "import_templated_geos", 
			                               "Import Templated Geos", ref value );
			if ( changed )
			{
				HAPI_Host.prImportTemplatedGeos = value;
				if ( HAPI_Host.myRepaintDelegate != null )
					HAPI_Host.myRepaintDelegate();
			}
		}


	}

	private static void generateGeometrySettings()
	{
		// Adding Points Mode Hot Key
		{
			int value = (int) HAPI_Host.prAddingPointsModeHotKey;
			string[] labels = System.Enum.GetValues(typeof(KeyCode))
								.Cast< KeyCode >()
								.Select(v => v.ToString())
								.ToArray();
			int[] values = System.Enum.GetValues(typeof(KeyCode))
								.Cast< KeyCode >()
								.Select(v => (int) v)
								.ToArray();
			bool changed = HAPI_GUI.dropdown( "adding_points_mode_hot_key", "Adding Points Key", 
												ref value, labels, values );
			if ( changed )
			{
				HAPI_Host.prAddingPointsModeHotKey = (KeyCode) value;
				if ( HAPI_Host.myRepaintDelegate != null )
					HAPI_Host.myRepaintDelegate();
			}
		}

		// Adding Points Mode Colour
		{
			Color value = HAPI_Host.prAddingPointsModeColour;
			bool changed = HAPI_GUI.colourField( "adding_ponits_mode_colour", "Adding Points Mode", ref value );
			if ( changed )
			{
				HAPI_Host.prAddingPointsModeColour = value;
				if ( HAPI_Host.myRepaintDelegate != null )
					HAPI_Host.myRepaintDelegate();
			}
		}

		HAPI_GUI.separator();

		// Editing Points Mode Hot Key
		{
			int value = (int) HAPI_Host.prEditingPointsModeHotKey;
			string[] labels = System.Enum.GetValues(typeof(KeyCode))
								.Cast< KeyCode >()
								.Select(v => v.ToString())
								.ToArray();
			int[] values = System.Enum.GetValues(typeof(KeyCode))
								.Cast< KeyCode >()
								.Select(v => (int) v)
								.ToArray();
			bool changed = HAPI_GUI.dropdown( "editing_points_mode_hot_key", "Editing Points Key", 
												ref value, labels, values );
			if ( changed )
			{
				HAPI_Host.prEditingPointsModeHotKey = (KeyCode) value;
				if ( HAPI_Host.myRepaintDelegate != null )
					HAPI_Host.myRepaintDelegate();
			}
		}

		// Editing Points Mode Colour
		{
			Color value = HAPI_Host.prEditingPointsModeColour;
			bool changed = HAPI_GUI.colourField( "editing_ponits_mode_colour", "Editing Points Mode", ref value );
			if ( changed )
			{
				HAPI_Host.prEditingPointsModeColour = value;
				if ( HAPI_Host.myRepaintDelegate != null )
					HAPI_Host.myRepaintDelegate();
			}
		}

		HAPI_GUI.separator();

		// Wireframe Colour
		{
			Color value = HAPI_Host.prWireframeColour;
			bool changed = HAPI_GUI.colourField( "wireframe_colour", "Wireframe", ref value );
			if ( changed )
			{
				HAPI_Host.prWireframeColour = value;
				if ( HAPI_Host.myRepaintDelegate != null )
					HAPI_Host.myRepaintDelegate();
			}
		}

		// Guide Wireframe Colour
		{
			Color value = HAPI_Host.prGuideWireframeColour;
			bool changed = HAPI_GUI.colourField( "guide_wireframe_colour", "Guide Wireframe", ref value );
			if ( changed )
			{
				HAPI_Host.prGuideWireframeColour = value;
				if ( HAPI_Host.myRepaintDelegate != null )
					HAPI_Host.myRepaintDelegate();
			}
		}

		// Unselectable Guide Wireframe Colour
		{
			Color value = HAPI_Host.prUnselectableGuideWireframeColour;
			bool changed = HAPI_GUI.colourField( "unselectable_guide_wireframe_colour", "Unselectable Guide", ref value );
			if ( changed )
			{
				HAPI_Host.prUnselectableGuideWireframeColour = value;
				if ( HAPI_Host.myRepaintDelegate != null )
					HAPI_Host.myRepaintDelegate();
			}
		}

		// Unselected Guide Wireframe Colour
		{
			Color value = HAPI_Host.prUnselectedGuideWireframeColour;
			bool changed = HAPI_GUI.colourField( "unselected_guide_wireframe_colour", "Unselected Guide", ref value );
			if ( changed )
			{
				HAPI_Host.prUnselectedGuideWireframeColour = value;
				if ( HAPI_Host.myRepaintDelegate != null )
					HAPI_Host.myRepaintDelegate();
			}
		}

		// Selected Guide Wireframe Colour
		{
			Color value = HAPI_Host.prSelectedGuideWireframeColour;
			bool changed = HAPI_GUI.colourField( "selected_guide_wireframe_colour", "Selected Guide", ref value );
			if ( changed )
			{
				HAPI_Host.prSelectedGuideWireframeColour = value;
				if ( HAPI_Host.myRepaintDelegate != null )
					HAPI_Host.myRepaintDelegate();
			}
		}

		HAPI_GUI.separator();

		// Guide Point Size
		{
			float value = HAPI_Host.prGuidePointSize;
			bool changed = HAPI_GUI.floatField( "guide_point_size", "Guide Point Size", 
												ref value, 4.0f, 40.0f );
			if ( changed )
			{
				HAPI_Host.prGuidePointSize = value;
				if ( HAPI_Host.myRepaintDelegate != null )
					HAPI_Host.myRepaintDelegate();
			}
		}

		// Min. Distance For Point Selection
		{
			float value = HAPI_Host.prMinDistanceForPointSelection;
			bool changed = HAPI_GUI.floatField( "min_distance_for_point_selection", 
												"Min. Distance For Point Selection", 
												ref value, 1.0f, 20.0f );
			if ( changed )
			{
				HAPI_Host.prMinDistanceForPointSelection = value;
				if ( HAPI_Host.myRepaintDelegate != null )
					HAPI_Host.myRepaintDelegate();
			}
		}

		// Guide Min. Distance For Mid Point Insertion
		{
			float value = HAPI_Host.prGuideMinDistanceForMidPointInsertion;
			bool changed = HAPI_GUI.floatField( "guide_min_distance_for_mid_point_insertion", 
												"Guide Min. Distance For Mid Point Insertion", 
												ref value, 1.0f, 20.0f );
			if ( changed )
			{
				HAPI_Host.prGuideMinDistanceForMidPointInsertion = value;
				if ( HAPI_Host.myRepaintDelegate != null )
					HAPI_Host.myRepaintDelegate();
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
												ref value, labels, values );
			if ( changed )
				HAPI_Host.prCurvePrimitiveTypeDefault = value;
		}

		// Curve Method Default
		{
			int value = HAPI_Host.prCurveMethodDefault;
			string[] labels = { "CVs", "Breakpoints", "Freehand" };
			int[] values = { 0, 1, 2 };
			bool changed = HAPI_GUI.dropdown( "curve_method_default", "Initial Method", 
												ref value, labels, values );
			if ( changed )
				HAPI_Host.prCurveMethodDefault = value;
		}
	}

	private static bool			myEnableDraw			= true;
	private static int			mySettingsTabSelection	= 0;
	private static Vector2 		myScrollPosition;

#if !UNITY_STANDALONE_WIN
	#pragma warning restore 0414
#endif // !UNITY_STANDALONE_WIN
}
