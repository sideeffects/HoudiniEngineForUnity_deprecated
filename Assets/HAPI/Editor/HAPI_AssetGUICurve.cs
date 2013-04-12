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
	public enum Mode
	{
		NONE,
		ADD,
		EDIT
	}

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

		mySelectionMeshColours	= null;
		mySelectionMesh			= null;
		mySelectionMaterial		= null;

		myConnectionMesh		= null;
		myConnectionMaterial	= null;

		myLastMode				= Mode.NONE;
		
		if ( GUI.changed )
			myAssetCurve.build();
	}

	public override void OnDisable()
	{
		// This is called after OnSceneGUI sometimes for some reason.
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

		// Decide modes.
		decideModes( ref current_event );
		
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
		
		int pressed_point_index 	= -1;

		// TESTING
		//if ( current_event.type == EventType.MouseDown && current_event.button == 0 )
			//Debug.Log("Left-Mouse Down");
		//if ( current_event.type == EventType.MouseUp && current_event.button == 0 )
			//Debug.Log("Left-Mouse Up");

		// Rebuilding geometry here to make sure the dummy line geometry is visible at any zoom level.
		myAssetCurve.buildDummyMesh();

		// Set appropriate handles matrix.
		Handles.matrix = myAssetCurve.transform.localToWorldMatrix;

		// Decide modes.
		decideModes( ref current_event );

		// Add points.
		if ( myAssetCurve.prIsAddingPoints )
		{
			myAssetCurve.prIsAddingPoints = drawModeBorders( Color.yellow );

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

					// Once we add a point we are no longer bound to the user holding down the add points key.
					// Add points mode is now fully activated.
					myAssetCurve.prModeChangeWait = false;

					myAssetCurve.addPoint( intersection );
				}
			}
		}
		else if ( myAssetCurve.prIsEditingPoints )
		{
			myAssetCurve.prIsEditingPoints = drawModeBorders( new Color( 0.7f, 0.7f, 0.9f ) );

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
					// Deselect all.
					myCurrentlyActivePoint = -1;

					Vector3 mouse_position = current_event.mousePosition;
				
					// Camera.current.pixelHeight != Screen.height for some reason.
					mouse_position.y		= myTempCamera.pixelHeight - mouse_position.y;
					Ray ray					= myTempCamera.ScreenPointToRay( mouse_position );
					ray.origin				= myTempCamera.transform.position;

					// Get Picking Information
					Vector3[] points = myAssetCurve.prPoints.ToArray();
					for ( int i = 0; points != null && i < points.Length; ++i )
					{
						Vector3 transformed_point = myAssetCurve.transform.TransformPoint( points[ i ] );
						Vector3 proj_pos = myTempCamera.WorldToScreenPoint( transformed_point );
						proj_pos.z = 0.0f;
						
						float distance = Vector3.Distance( mouse_position, proj_pos );
						if ( distance < 8.0f )
						{
							// Once we modify a point we are no longer bound to the user holding down 
							// the point edit key. Edit point mode is now fully activated.
							myAssetCurve.prModeChangeWait = false;
							pressed_point_index = i;
							if ( mySelectionMesh != null && mySelectionMeshColours != null )
								mySelectionMeshColours[ i ] = Color.yellow;
						}
						else if ( mySelectionMesh != null && mySelectionMeshColours != null )
							mySelectionMeshColours[ i ] = Color.white;
					}
				}
			}
		}
		else if ( myForceInspectorRedraw )
		{
			Repaint();
			myForceInspectorRedraw = false;
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

		// Remake and Draw Guide Geometry
		buildGuideGeometry();

		// Connection Mesh Draws
		if ( myConnectionMaterial != null && myConnectionMesh != null )
		{
			myConnectionMaterial.SetPass( 0 );
			Graphics.DrawMeshNow( myConnectionMesh, myAssetCurve.transform.localToWorldMatrix );
		}

		// Selection Mesh Draws
		if ( mySelectionMaterial != null && mySelectionMesh != null )
		{
			mySelectionMaterial.SetPass( 0 );
			Graphics.DrawMeshNow( mySelectionMesh, myAssetCurve.transform.localToWorldMatrix );
			mySelectionMaterial.SetPass( 1 );
			Graphics.DrawMeshNow( mySelectionMesh, myAssetCurve.transform.localToWorldMatrix );
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private

	private void buildGuideGeometry()
	{
		// Build Selection Mesh -------------------------------------------------------------------------------------

		if ( mySelectionMaterial == null )
		{
			mySelectionMaterial						= new Material( Shader.Find( "HAPI/Point" ) );
			mySelectionMaterial.hideFlags			= HideFlags.HideAndDontSave;
			mySelectionMaterial.shader.hideFlags	= HideFlags.HideAndDontSave;
		}

		if ( mySelectionMesh == null )
		{
			mySelectionMesh							= new Mesh();
			mySelectionMesh.hideFlags				= HideFlags.HideAndDontSave;
		}

		// Pretend we have two points if only one actually exists. Since DrawMesh won't draw
		// anything unless it has at least a line we need a dummy line to exist.
		// In this case we just create two points, both at the same position as 
		// the one real point.
		Vector3[] selection_vertices = null;
		if ( myAssetCurve.prPoints.Count == 1 )
		{
			selection_vertices = new Vector3[ 2 ];
			selection_vertices[ 0 ] = myAssetCurve.prPoints[ 0 ];
			selection_vertices[ 1 ] = myAssetCurve.prPoints[ 0 ];
		}
		else
			selection_vertices = myAssetCurve.prPoints.ToArray();

		// Create the selection indices.
		int[] selection_indices = new int[ selection_vertices.Length ];
		for ( int i = 0; i < selection_vertices.Length; ++i )
			selection_indices[ i ] = i;

		if ( mySelectionMeshColours == null || mySelectionMeshColours.Length != selection_vertices.Length )
		{
			mySelectionMeshColours = new Color[ selection_vertices.Length ];
			for ( int i = 0; i < selection_vertices.Length; ++i )
			{
				if ( !myAssetCurve.prIsEditingPoints )
					mySelectionMeshColours[ i ] = new Color( 0.0f, 0.2f, 0.2f );
				else
					mySelectionMeshColours[ i ] = Color.white;
			}
		}

		mySelectionMesh.vertices	= selection_vertices;
		mySelectionMesh.colors		= mySelectionMeshColours;
		mySelectionMesh.SetIndices( selection_indices, MeshTopology.Points, 0 );

		//////////////////////////////////
		// Build Connection Mesh

		if ( myConnectionMaterial == null )
		{
			myConnectionMaterial					= new Material( Shader.Find( "HAPI/Line" ) );
			myConnectionMaterial.hideFlags			= HideFlags.HideAndDontSave;
			myConnectionMaterial.shader.hideFlags	= HideFlags.HideAndDontSave;
		}

		if ( myConnectionMesh == null )
		{
			myConnectionMesh						= new Mesh();
			myConnectionMesh.hideFlags				= HideFlags.HideAndDontSave;
		}

		Vector3[] connection_vertices = myAssetCurve.prPoints.ToArray();
		int[] connection_indices = new int[ connection_vertices.Length ];
		for ( int i = 0; i < connection_vertices.Length; ++i )
			connection_indices[ i ] = i;

		myConnectionMesh.vertices = connection_vertices;
		myConnectionMesh.SetIndices( connection_indices, MeshTopology.LineStrip, 0 );
	}

	private void changeModes( ref bool add_points_mode, ref bool edit_points_mode, Mode mode )
	{
		switch ( mode )
		{
			case Mode.NONE: 
				{
					add_points_mode = false;
					edit_points_mode = false;
					break;
				}
			case Mode.ADD:
				{
					add_points_mode = true;
					edit_points_mode = false;
					break;
				}
			case Mode.EDIT:
				{
					add_points_mode = false;
					edit_points_mode = true;
					break;
				}
			default:
				Debug.LogError( "Invalid mode?" ); break;
		}
	}

	private void decideModes( ref Event current_event )
	{
		bool add_points_mode_key	= current_event.shift;
		bool edit_points_mode_key	= current_event.control;

		bool add_points_mode		= myAssetCurve.prIsAddingPoints;
		bool edit_points_mode		= myAssetCurve.prIsEditingPoints;
		bool mode_change_wait		= myAssetCurve.prModeChangeWait;

		if ( add_points_mode )
		{
			if ( !mode_change_wait && edit_points_mode_key )
			{
				myLastMode			= Mode.ADD;

				add_points_mode		= false;
				edit_points_mode	= true;
				mode_change_wait	= true;
			}
			else if ( mode_change_wait && !add_points_mode_key )
			{
				changeModes( ref add_points_mode, ref edit_points_mode, myLastMode );
				mode_change_wait	= false;
			}
		}
		else if ( edit_points_mode )
		{
			if ( !mode_change_wait && add_points_mode_key )
			{
				myLastMode			= Mode.EDIT;

				add_points_mode		= true;
				edit_points_mode	= false;
				mode_change_wait	= true;
			}
			else if ( mode_change_wait && !edit_points_mode_key )
			{
				changeModes( ref add_points_mode, ref edit_points_mode, myLastMode );
				mode_change_wait	= false;
			}
		}
		else
		{
			if ( add_points_mode_key )
			{
				add_points_mode		= true;
				mode_change_wait	= true;
				myLastMode			= Mode.NONE;
			}
			else if ( edit_points_mode_key )
			{
				edit_points_mode	= true;
				mode_change_wait	= true;
				myLastMode			= Mode.NONE;
			}
		}

		// Check if ENTER or ESC was pressed so we can exit the mode.
		if ( current_event.isKey && current_event.type == EventType.KeyUp
			 && ( current_event.keyCode == KeyCode.Escape || current_event.keyCode == KeyCode.Return ) )
		{
			add_points_mode				= false;
			edit_points_mode			= false;
			myForceInspectorRedraw		= true;
		}

		// Change the colours of the points if the edit points mode has changed.
		if ( edit_points_mode != myAssetCurve.prIsEditingPoints )
		{
			mySelectionMeshColours = null;
			myCurrentlyActivePoint = -1;
		}

		myAssetCurve.prIsAddingPoints	= add_points_mode;
		myAssetCurve.prIsEditingPoints	= edit_points_mode;
		myAssetCurve.prModeChangeWait	= mode_change_wait;
	}

	private bool drawModeBorders( Color color )
	{
		Handles.BeginGUI();
		GUILayout.BeginArea( new Rect( 0, 0, Screen.width, Screen.height ) );

		// Draw description and exit button of curve editing mode.
		GUIStyle text_style		= new GUIStyle( GUI.skin.label );
		text_style.alignment	= TextAnchor.UpperLeft;
		text_style.fontStyle	= FontStyle.Bold;
		Color original_color	= GUI.color;
		GUI.color				= color;
		GUI.Box( new Rect( myActiveBorderWidth + 4, myActiveBorderWidth + 4, 220, 130 ), "" );
		GUI.Label( new Rect( myActiveBorderWidth + 5, myActiveBorderWidth + 4, 200, 20 ), "Curve Editing Mode", text_style );
			
		text_style.fontStyle	= FontStyle.Normal;
		text_style.wordWrap		= true;
		GUI.Label( new Rect( myActiveBorderWidth + 5, myActiveBorderWidth + 21, 200, 80 ), "Click anywhere on the screen to add a new curve control point. You can also move existing points in this mode but you cannot select any other object. Press (ENTER) or (ESC) when done.", text_style );
			
		bool cancel_mode		= !GUI.Button( new Rect( myActiveBorderWidth + 100, myActiveBorderWidth + 108, 120, 20 ), "Exit Curve Mode" );
		GUI.color				= original_color;

		// Draw yellow mode lines around the Scene view.
		Texture2D box_texture	= new Texture2D( 1, 1 );
		box_texture.SetPixel( 0, 0, new Color( color.r, color.g, color.b, 0.6f ) );
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

		return cancel_mode;
	}
	
	private HAPI_AssetCurve		myAssetCurve;

	private int 				myCurrentlyActivePoint;
	private bool				myForceInspectorRedraw;
	private string				myAddPointButtonLabel;

	private const float			myActiveBorderWidth = 5;
	private Camera				myTempCamera;

	[SerializeField] 
	private GameObject			myTarget;

	private Color[]				mySelectionMeshColours;
	private Mesh				mySelectionMesh;
	private Material			mySelectionMaterial;

	private Mesh				myConnectionMesh;
	private Material			myConnectionMaterial;

	private Mode				myLastMode;
}
