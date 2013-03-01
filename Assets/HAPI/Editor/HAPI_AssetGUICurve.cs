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

[ ExecuteInEditMode ]
[ CustomEditor( typeof( HAPI_AssetCurve ) ) ]
public class HAPI_AssetGUICurve : HAPI_AssetGUI 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	public override void OnEnable() 
	{
		base.OnEnable();
		myAssetCurve			= myAsset as HAPI_AssetCurve;
		
		myCurrentlyActivePoint	= -1;

		myForceInspectorRedraw	= false;
		myAddPointButtonLabel	= "Add Points";
		myTarget				= null;
		
		if ( GUI.changed )
			myAssetCurve.build();
	}

	public override void OnDisable()
	{
		myAssetCurve.prIsAddingPoints = false;
	}
	
	public override void OnInspectorGUI() 
	{
		if ( myAssetCurve == null )
			return;
		
		myParmChanges = false;
		
		base.OnInspectorGUI();
		
		Event current_event = Event.current;
		bool commitChanges = false;
		if ( current_event.isKey && current_event.type == EventType.KeyUp && current_event.keyCode == KeyCode.Return )
			commitChanges = true;

		// Check if Shift is pressed so we can enable curve edit mode.
		if ( !myAssetCurve.prIsAddingPoints && current_event.shift )
		{
			// Set curve edit mode to true but add the condition that it will stay
			// true only if they continue to hold down Shift or they add at least
			// one point.
			myAssetCurve.prIsAddingPoints		= true;
			myAssetCurve.prEditModeChangeWait	= true;
		}
		else if ( myAssetCurve.prEditModeChangeWait && !current_event.shift )
		{
			// If no points were added before Shift was let go then revert the curve edit mode
			// change.
			myAssetCurve.prIsAddingPoints		= false;
			myAssetCurve.prEditModeChangeWait	= false;
		}

		// Check if ENTER or ESC was pressed to we can exit curve mode.
		if ( myAssetCurve.prIsAddingPoints && current_event.isKey && current_event.type == EventType.KeyUp )
			if ( current_event.keyCode == KeyCode.Escape || current_event.keyCode == KeyCode.Return )
			{
				myAssetCurve.prIsAddingPoints	= false;
				myForceInspectorRedraw			= true;
			}
		
		///////////////////////////////////////////////////////////////////////
		// Draw Game Object Controls
		
		myAssetCurve.prShowObjectControls = 
			EditorGUILayout.Foldout( myAssetCurve.prShowObjectControls, new GUIContent( "Object Controls" ) );
		
		if ( myAssetCurve.prShowObjectControls ) 
		{	
			if ( GUILayout.Button( "Rebuild" ) ) 
			{
				myAssetCurve.prIsAddingPoints = false;
				myAssetCurve.prFullBuild = true;
				myAssetCurve.build();

				myAssetCurve.syncPointsWithParm();
				myAssetCurve.build();
			}

			// Draw Auto Select Asset Node Toggle
			{
				bool value = myAsset.prAutoSelectAssetNode;
				HAPI_GUI.toggle( "auto_select_parent", "Auto Select Parent", ref value );
				myAsset.prAutoSelectAssetNode = value;
			}

			// Sync Asset Transform Toggle
			{
				bool value = myAsset.prSyncAssetTransform;
				HAPI_GUI.toggle( "sync_asset_transform", "Sync Asset Transform", ref value );
				myAsset.prSyncAssetTransform = value;
			}

			// Live Transform Propagation Toggle
			{
				bool value = myAsset.prLiveTransformPropagation;
				HAPI_GUI.toggle( "live_transform_propagation", "Live Transform Propagation", ref value );
				myAsset.prLiveTransformPropagation = value;
			}
		} // if
		
		///////////////////////////////////////////////////////////////////////
		// Draw Asset Controls
		
		EditorGUILayout.Separator();
		myAssetCurve.prShowAssetControls = 
			EditorGUILayout.Foldout( myAssetCurve.prShowAssetControls, new GUIContent( "Asset Controls" ) );
		
		myDelayBuild = false;
		if ( myAssetCurve.prShowAssetControls )
		{
			if ( myAssetCurve.prIsAddingPoints )
				myAddPointButtonLabel = "Stop Adding Points";
			else
				myAddPointButtonLabel = "Add Points";

			if ( GUILayout.Button( myAddPointButtonLabel ) )
			{
				myAssetCurve.prIsAddingPoints = !myAssetCurve.prIsAddingPoints;
				OnSceneGUI();
			}

			GUI.enabled = !myAssetCurve.prIsAddingPoints;

			Object target = (Object) myTarget;
			if ( HAPI_GUI.objectField( "target", "Target", ref target, typeof( GameObject ) ) )
			{
				myTarget = (GameObject) target;
			}
			myParmChanges |= generateAssetControls();
			
			GUI.enabled = true;
		}

		if ( ( myParmChanges && !myDelayBuild ) || ( myUnbuiltChanges && commitChanges ) )
		{
			myAssetCurve.syncPointsWithParm();
			myAssetCurve.build();
			myUnbuiltChanges = false;
			
			// To keep things consistent with Unity workflow, we should not save parameter changes
			// while in Play mode.
			if ( !EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode )
				myAssetCurve.savePreset();
		}
		else if ( myParmChanges )
			myUnbuiltChanges = true;
	}
	
	public void OnSceneGUI() 
	{
		if ( myAssetCurve == null )
			return;
		
		if ( !myTempCamera && Camera.current )
			myTempCamera = Camera.current;

		Event current_event 		= Event.current;
		
		int point_count 			= myAssetCurve.prPoints.Count;
		int pressed_point_index 	= -1;
		Vector3 previous_position 	= Vector3.zero;

		// Rebuilding geometry here to make sure the dummy line geometry is visible at any zoom level.
		myAssetCurve.buildDummyMesh();

		// Set appropriate handles matrix.
		Handles.matrix = myAssetCurve.transform.localToWorldMatrix;

		// Check if Shift is pressed so we can enable curve edit mode.
		if ( !myAssetCurve.prIsAddingPoints && current_event.shift )
		{
			// Set curve edit mode to true but add the condition that it will stay
			// true only if they continue to hold down Shift or they add at least
			// one point.
			myAssetCurve.prIsAddingPoints		= true;
			myAssetCurve.prEditModeChangeWait	= true;
		}
		else if ( myAssetCurve.prEditModeChangeWait && !current_event.shift )
		{
			// If no points were added before Shift was let go then revert the curve edit mode
			// change.
			myAssetCurve.prIsAddingPoints		= false;
			myAssetCurve.prEditModeChangeWait	= false;
		}

		// Check if ENTER or ESC was pressed to we can exit curve mode.
		if ( myAssetCurve.prIsAddingPoints && current_event.isKey && current_event.type == EventType.KeyUp )
			if ( current_event.keyCode == KeyCode.Escape || current_event.keyCode == KeyCode.Return )
			{
				myAssetCurve.prIsAddingPoints	= false;
				myForceInspectorRedraw			= true;
			}

		// Add points.
		if ( myAssetCurve.prIsAddingPoints )
		{
			Handles.BeginGUI();
			GUILayout.BeginArea( new Rect( 0, 0, Screen.width, Screen.height ) );

			// Draw description and exit button of curve editing mode.
			GUIStyle text_style		= new GUIStyle( GUI.skin.label );
			text_style.alignment	= TextAnchor.UpperLeft;
			text_style.fontStyle	= FontStyle.Bold;
			Color original_color	= GUI.color;
			GUI.color				= Color.yellow;
			GUI.Box( new Rect( myActiveBorderWidth + 4, myActiveBorderWidth + 4, 220, 130 ), "" );
			GUI.Label( new Rect( myActiveBorderWidth + 5, myActiveBorderWidth + 4, 200, 20 ), "Curve Editing Mode", text_style );
			
			text_style.fontStyle	= FontStyle.Normal;
			text_style.wordWrap		= true;
			GUI.Label( new Rect( myActiveBorderWidth + 5, myActiveBorderWidth + 21, 200, 80 ), "Click anywhere on the screen to add a new curve control point. You can also move existing points in this mode but you cannot select any other object. Press (ENTER) or (ESC) when done.", text_style );
			
			myAssetCurve.prIsAddingPoints = !GUI.Button( new Rect( myActiveBorderWidth + 100, myActiveBorderWidth + 108, 120, 20 ), "Exit Curve Mode" );
			GUI.color				= original_color;

			// Draw yellow mode lines around the Scene view.
			Texture2D box_texture	= new Texture2D( 1, 1 );
			box_texture.SetPixel( 0, 0, new Color( 1.0f, 1.0f, 0.0f, 0.6f ) );
			box_texture.wrapMode	= TextureWrapMode.Repeat;
			box_texture.Apply();
			float width				= myTempCamera.pixelWidth;
			float height			= myTempCamera.pixelHeight;
			float border_width		= myActiveBorderWidth;
			GUI.DrawTexture( new Rect( 0, 0, width, border_width ),								// Top
							 box_texture, ScaleMode.StretchToFill );
			GUI.DrawTexture( new Rect( 0, border_width, border_width, height - border_width ),	// Right
							 box_texture, ScaleMode.StretchToFill );
			GUI.DrawTexture( new Rect( border_width, height - border_width, width, height ),	// Bottom
							 box_texture, ScaleMode.StretchToFill );
			GUI.DrawTexture( new Rect( width - border_width, border_width,						// Left
							 width, height - border_width - border_width ), 
							 box_texture, ScaleMode.StretchToFill );

			GUILayout.EndArea();
			Handles.EndGUI();

			if ( !current_event.alt )
			{

				Vector3 position	= Vector3.zero;
				float handle_size 	= HandleUtility.GetHandleSize( position ) * 1000000.0f;
				Quaternion rotation = HAPI_AssetUtility.getQuaternion( myTempCamera.transform.localToWorldMatrix );
				bool button_press 	= Handles.Button( 	position, 
														rotation,
														handle_size,
														handle_size,
														Handles.RectangleCap );

				if ( button_press )
				{
					Vector3 mouse_position = current_event.mousePosition;
				
					// Camera.current.pixelHeight != Screen.height for some reason.
					mouse_position.y		= myTempCamera.pixelHeight - mouse_position.y;
					Ray ray					= myTempCamera.ScreenPointToRay( mouse_position );
					ray.origin				= myTempCamera.transform.position;
					Vector3 intersection	= new Vector3();

					if ( myTarget != null && myTarget.GetComponent< MeshCollider >() )
					{
						MeshCollider collider = myTarget.GetComponent< MeshCollider >();
						RaycastHit hit_info;
						collider.Raycast( ray, out hit_info, 1000.0f );
						intersection = hit_info.point;
					}
					else
					{
						Plane plane = new Plane();
						plane.SetNormalAndPosition( Vector3.up, myAssetCurve.transform.position );
						float enter = 0.0f;
						plane.Raycast( ray, out enter );
 						intersection = ray.origin + ray.direction * enter;
					}

					// Once we add a point we are no longer bound to the user holding down Shift.
					// Curve edit mode is not full activated.
					myAssetCurve.prEditModeChangeWait = false;

					myAssetCurve.addPoint( intersection );
				}
			}
		}
		else if ( myForceInspectorRedraw )
		{
			Repaint();
			myForceInspectorRedraw = false;
		}

		// Draw current curve.
		Vector3[] vertices = myAssetCurve.prVertices;
		for ( int i = 0; vertices != null && i < vertices.Length; ++i )
		{
			Vector3 position = vertices[ i ];
			
			if ( i > 0 )
			{
				Handles.color = Color.black;
				Handles.DrawLine( previous_position, position );
			}

			previous_position = position;
		}
		
		// Determine which control point was pressed for modification.
		for ( int i = 0; i < point_count; ++i ) 
		{
			Vector3 position 	= myAssetCurve.prPoints[ i ];
			float handle_size 	= HandleUtility.GetHandleSize( position ) * 0.06f;
			
			Handles.color 		= Color.cyan;
			Quaternion rotation = HAPI_AssetUtility.getQuaternion( myTempCamera.transform.localToWorldMatrix );
			bool buttonPress 	= Handles.Button( 	position, 
													rotation,
													handle_size,
													handle_size,
													Handles.RectangleCap );
			
			if ( buttonPress )
				pressed_point_index = i;
			
			Handles.Label( position, new GUIContent( "p" + i ) );
			
			if ( i > 0 )
			{
				Handles.color = Color.grey;
				Handles.DrawLine( previous_position, position );
			}

			previous_position = position;
		}
		
		// Set active control point.
		if ( pressed_point_index >= 0 )
			myCurrentlyActivePoint = pressed_point_index;

		// Update active control point.
		if ( myCurrentlyActivePoint >= 0 ) 
		{
			Vector3 old_position = myAssetCurve.prPoints[ myCurrentlyActivePoint ];
			Vector3 new_position = Handles.PositionHandle( old_position, 
														   Quaternion.identity );
			
			if ( new_position != old_position )
				myAssetCurve.updatePoint( myCurrentlyActivePoint, new_position );
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private
	
	private HAPI_AssetCurve		myAssetCurve;

	private int 				myCurrentlyActivePoint;
	private bool				myForceInspectorRedraw;
	private string				myAddPointButtonLabel;

	private const float			myActiveBorderWidth = 5;
	private Camera				myTempCamera;

	[SerializeField] 
	private GameObject			myTarget;
}
