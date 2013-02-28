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
 * 		Only a test class. Not used currently.
 * 
 */

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using HAPI;

public class HAPI_WindowUtilities : EditorWindow 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public

	[ MenuItem( "Window/" + HAPI_Constants.HAPI_PRODUCT_SHORT_NAME + " Utilities" ) ]
	public static void ShowWindow() 
	{
		// Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow< HAPI_WindowUtilities >( false, HAPI_Constants.HAPI_PRODUCT_SHORT_NAME + " Utilities" );
		float time = 0.0f;

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

		HAPI_Host.getTime( out time );
		HAPI_WindowUtilities.myTime = time;
	}
	
	public void OnGUI() 
	{
		if ( !myEnableDraw )
			return;
		
		myScrollPosition = GUILayout.BeginScrollView( myScrollPosition );

		myShowUtility = EditorGUILayout.Foldout( myShowUtility, new GUIContent( "Utility" ) );

		if ( myShowUtility )
		{
			if ( GUILayout.Button( HAPI_GUIUtility.mySaveHoudiniSceneLabel ) )
			{
				string hip_file_path = EditorUtility.SaveFilePanel( "Save HIP File", "", "hscene.hip", "hip" );
				if ( hip_file_path != "" && HAPI_Host.hasScene() )
					HAPI_Host.saveScene( hip_file_path );
				else
					Debug.LogError( "Nothing to save." );
			}
			
			if ( GUILayout.Button( HAPI_GUIUtility.myLoadAssetLabel ) )
			{
				string asset_file_path = HAPI_GUIUtility.promptForOTLPath();
				HAPI_GUIUtility.instantiateAsset( asset_file_path );
			}

			if ( HAPI_GUI.floatField( "global_time", "Global Time", ref myTime ) )
			{
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
					HAPI_Host.setTime( myTime );
				}
				catch ( HAPI_Error error )
				{
					Debug.LogError( error.ToString() );
				}
			}
		}

		myShowSettings = EditorGUILayout.Foldout( myShowSettings, new GUIContent( "Settings" ) );

		if ( myShowSettings )
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
				bool changed = HAPI_GUI.stringField( "rendered_collision_group_name", "Rendered Colli. Grp.", ref value );
				if ( changed )
					HAPI_Host.prRenderedCollisionGroupName = value;
			}

			HAPI_GUI.separator();
			
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
		}
		
		GUILayout.EndScrollView();
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private
	
	private static bool				myEnableDraw			= true;

	private static bool				myShowUtility			= true;
	private static bool				myShowSettings			= true;
	
	private static Vector2 			myScrollPosition;

	private static float			myTime;
}
