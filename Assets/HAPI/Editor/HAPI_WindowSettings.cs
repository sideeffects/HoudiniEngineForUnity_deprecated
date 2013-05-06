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
		
		myScrollPosition = GUILayout.BeginScrollView( myScrollPosition );

		if ( GUILayout.Button( HAPI_GUIUtility.myRevertAllSettingsLabel ) )
		{
			if ( EditorUtility.DisplayDialog( "Revert all settings?",
												"Are you sure you want to revert ALL Houdini plugin settings?", 
												"Yes", "No" ) )
			{
				HAPI_Host.revertAllSettingsToDefaults();
				HAPI_Host.myRepaintDelegate();
			}
		}

		myShowGeneralSettings = EditorGUILayout.Foldout( myShowGeneralSettings, new GUIContent( "General" ) );
		if ( myShowGeneralSettings )
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

			// Auto Select Parent
			{
				bool value = HAPI_Host.prAutoSelectParent;
				bool changed = HAPI_GUI.toggle( "auto_select_parent", "Auto Select Parent", ref value );
				if ( changed )
				{
					HAPI_Host.prAutoSelectParent = value;
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
		}

		myShowGeometryToolsSettings = EditorGUILayout.Foldout( myShowGeometryToolsSettings, 
															   new GUIContent( "Geometry Tools" ) );
		if ( myShowGeometryToolsSettings )
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

		myShowCurveSettings = EditorGUILayout.Foldout( myShowCurveSettings, new GUIContent( "Curves" ) );
		if ( myShowCurveSettings )
		{
			// Curve Primitive Type Default
			{
				int value = HAPI_Host.prCurvePrimitiveTypeDefault;
				string[] labels = { "Polygon", "NURBS", "Bezier" };
				int[] values = { 0, 1, 2 };
				bool changed = HAPI_GUI.dropdown( "curve_primitive_type_default", "Default Curve Type", 
												  ref value, labels, values );
				if ( changed )
					HAPI_Host.prCurvePrimitiveTypeDefault = value;
			}

			// Curve Method Default
			{
				int value = HAPI_Host.prCurveMethodDefault;
				string[] labels = { "CVs", "Breakpoints", "Freehand" };
				int[] values = { 0, 1, 2 };
				bool changed = HAPI_GUI.dropdown( "curve_method_default", "Default Curve Method", 
												  ref value, labels, values );
				if ( changed )
					HAPI_Host.prCurveMethodDefault = value;
			}
		}
		
		GUILayout.EndScrollView();
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private
	
	private static bool				myEnableDraw					= true;

	private static bool				myShowGeneralSettings			= false;
	private static bool				myShowGeometryToolsSettings		= false;
	private static bool				myShowCurveSettings				= false;
	
	private static Vector2 			myScrollPosition;
	
}
