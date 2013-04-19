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
using System.Reflection;
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

		myForceInspectorRedraw	= false;
		myAddPointButtonLabel	= "Add Points";
		myTarget				= null;

		myIsMouseDown			= false;
		myCurrentlyPressedKey	= KeyCode.None;
		myFirstMousePosition	= new Vector3();

		myGuideLinesMaterial	= null;
		myGuideLinesTexture		= null;
		myGuideLinesMesh		= null;

		mySelectionArea			= new Rect();
		mySelectionMeshColours	= null;
		mySelectionMesh			= null;
		mySelectionMaterial		= null;
		mySelectedPoints		= new List< int >();
		mySelectedPointsMask	= new List< bool >();

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

	public override void refresh()
	{
		buildGuideGeometry();
		//OnSceneGUI();

		base.refresh();
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
				refresh();
			}

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
	
	public override void OnSceneGUI() 
	{
		if ( myAssetCurve == null )
			return;

		base.OnSceneGUI();

		// First Remake and Draw Guide Geometry if necessary.
		if ( mySelectionMesh == null )
			buildGuideGeometry();
		
		if ( !myTempCamera && Camera.current )
			myTempCamera = Camera.current;

		Event current_event 		= Event.current;
		Vector3 mouse_position		= getMousePosition( ref current_event ); 

		// Set appropriate handles matrix.
		Handles.matrix = myAssetCurve.transform.localToWorldMatrix;

		// Determine key state.
		getKeyState( current_event );

		// Decide modes.
		if ( current_event.type == EventType.Layout )
			decideModes( ref current_event );

		// Add points.
		if ( myAssetCurve.prIsAddingPoints )
		{
			myAssetCurve.prIsAddingPoints = drawModeBorders( HAPI_Host.prAddingPointsModeColour, mouse_position );
			
			if ( !current_event.alt )
			{
				Vector3 position	= Vector3.zero;
				float handle_size 	= HandleUtility.GetHandleSize( position ) * myBigButtonHandleSizeMultiplier;
				Quaternion rotation = HAPI_AssetUtility.getQuaternion( myTempCamera.transform.localToWorldMatrix );
				bool button_press 	= Handles.Button( 	position, 
														rotation,
														handle_size,
														handle_size,
														Handles.RectangleCap );

				Ray ray						= myTempCamera.ScreenPointToRay( mouse_position );
				ray.origin					= myTempCamera.transform.position;
				Vector3 intersection		= new Vector3();

				if ( myTarget != null && myTarget.GetComponent< MeshCollider >() )
				{
					MeshCollider collider = myTarget.GetComponent< MeshCollider >();
					RaycastHit hit_info;
					collider.Raycast( ray, out hit_info, myIntersectionRayLength );
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

				bool is_mid_point			= false;
				int insert_index			= -1;
				Vector3 new_point_location	= intersection;
				
				// Draw guide line.
				if ( myAssetCurve.prPoints.Count > 0 )
				{
					Vector3 anchor1				= myAssetCurve.prPoints[ myAssetCurve.prPoints.Count - 1 ];
					Vector3 anchor2				= Vector3.zero;
					insert_index				= myAssetCurve.prPoints.Count;

					// See if we're close to another segment.
					for ( int i = 1; i < myAssetCurve.prPoints.Count; ++i )
					{
						Vector3 p0 = myAssetCurve.prPoints[ i - 1 ];
						Vector3 p1 = myAssetCurve.prPoints[ i ];
						
						Vector3 closest_point = new Vector3();
						float distance = HAPI_GUIUtility.closestDistanceBetweenLineAndLineSegment(
											p0, p1, ray, out closest_point );
						
						if ( distance < 
								HandleUtility.GetHandleSize( closest_point ) / 
								HAPI_Host.prGuideMinDistanceForMidPointInsertion )
						{
							anchor1 = p0;
							anchor2 = p1;
							new_point_location = closest_point;
							insert_index = i;
							is_mid_point = true;
						}
					}

					int point_count = ( is_mid_point ? 3 : 2 );

					Vector3[] line_vertices = new Vector3[ point_count ];
					int[] line_indices = new int[ point_count ];
					Vector2[] uvs = new Vector2[ point_count ];

					line_vertices[ 0 ]	= HAPI_GUIUtility.getCameraNearPlanePoint( anchor1, myTempCamera );
					line_vertices[ 1 ]	= HAPI_GUIUtility.getCameraNearPlanePoint( new_point_location, myTempCamera );
					float length		= Vector3.Distance( line_vertices[ 0 ], line_vertices[ 1 ] ) * 
										  myGuideLinesDashTilingMultiplier;
					line_indices[ 0 ]	= 0; 
					line_indices[ 1 ]	= 1;
					uvs[ 0 ]			= new Vector2(); 
					uvs[ 1 ]			= new Vector2( length, length );

					if ( is_mid_point )
					{
						line_vertices[ 2 ]	= HAPI_GUIUtility.getCameraNearPlanePoint( anchor2, myTempCamera );
						line_indices[ 2 ]	= 2;
						length				+= Vector3.Distance( line_vertices[ 1 ], line_vertices[ 2 ] ) * 
											   myGuideLinesDashTilingMultiplier;
						uvs[ 2 ]			= new Vector2( length, length );
					}

					myGuideLinesMesh.Clear();
					myGuideLinesMesh.vertices = line_vertices;
					myGuideLinesMesh.uv = uvs;
					myGuideLinesMesh.SetIndices( line_indices, MeshTopology.LineStrip, 0 );

					myGuideLinesMaterial.SetPass( 0 );
					myGuideLinesMaterial.SetColor( "_Color", HAPI_Host.prGuideWireframeColour );
					myGuideLinesMaterial.SetTextureScale( "_MainTex", new Vector2( 1.0f, 1.0f ) );
					Graphics.DrawMeshNow( myGuideLinesMesh, myAssetCurve.transform.localToWorldMatrix );
				}

				if ( button_press )
				{
					// Once we add a point we are no longer bound to the user holding down the add points key.
					// Add points mode is now fully activated.
					myAssetCurve.prModeChangeWait = false;

					if ( is_mid_point )
						myAssetCurve.insertPoint( insert_index, new_point_location );
					else
						myAssetCurve.addPoint( new_point_location );

					// Remake and Draw Guide Geometry
					buildGuideGeometry();
				}
			}
		}
		else if ( myAssetCurve.prIsEditingPoints )
		{
			myAssetCurve.prIsEditingPoints = drawModeBorders( HAPI_Host.prEditingPointsModeColour, mouse_position );
			if ( !myAssetCurve.prIsEditingPoints )
				clearSelection();

			if ( !current_event.alt )
			{
				// Track mouse dragging.
				if ( current_event.type == EventType.MouseDown && current_event.button == 0 && !myIsMouseDown )
				{
					myIsMouseDown = true;
					myFirstMousePosition = mouse_position;
				} 
				// I have to also interpret the Ignore event as the mouse up event because that's all I
				// get if the use lets go of the mouse button while over a different Unity window...
				else if ( ( ( current_event.type == EventType.MouseUp && current_event.button == 0 ) ||
							( current_event.type == EventType.Ignore ) ) 
						  && myIsMouseDown )
				{
					myIsMouseDown = false;
					
					// Deselect all.
					if ( !current_event.control )
						clearSelection();

					Ray ray					= myTempCamera.ScreenPointToRay( mouse_position );
					ray.origin				= myTempCamera.transform.position;

					Vector3 mouse_delta		= myFirstMousePosition - mouse_position;
					Vector3 max_bounds		= mouse_position;
					Vector3 min_bounds		= mouse_position;
					max_bounds.x			= Mathf.Max( max_bounds.x, myFirstMousePosition.x );
					max_bounds.y			= Mathf.Max( max_bounds.y, myFirstMousePosition.y );
					min_bounds.x			= Mathf.Min( min_bounds.x, myFirstMousePosition.x );
					min_bounds.y			= Mathf.Min( min_bounds.y, myFirstMousePosition.y );

					// Get Picking Information
					Vector3[] points = myAssetCurve.prPoints.ToArray();
					for ( int i = 0; points != null && i < points.Length; ++i )
					{
						Vector3 transformed_point = myAssetCurve.transform.TransformPoint( points[ i ] );
						Vector3 proj_pos = myTempCamera.WorldToScreenPoint( transformed_point );
						proj_pos.z = 0.0f;

						if ( Mathf.Abs( mouse_delta.x ) > 1.5f || Mathf.Abs( mouse_delta.y ) > 1.5f )
						{
							if ( proj_pos.x >= min_bounds.x && proj_pos.x <= max_bounds.x &&
								 proj_pos.y >= min_bounds.y && proj_pos.y <= max_bounds.y )
							{
								// Once we modify a point we are no longer bound to the user holding down 
								// the point edit key. Edit point mode is now fully activated.
								myAssetCurve.prModeChangeWait = false;
								togglePointSelection( i );
							}
						} // drag
						else
						{
							float distance = Vector3.Distance( mouse_position, proj_pos );
							if ( distance < HAPI_Host.prMinDistanceForPointSelection )
							{
								// Once we modify a point we are no longer bound to the user holding down 
								// the point edit key. Edit point mode is now fully activated.
								myAssetCurve.prModeChangeWait = false;
								togglePointSelection( i );
							} // if point hit
						} // single click
					} // for all points
				} // mouse up
				
				// Prevent click from being passed lower (this is so stupid!).
				Vector3 position	= Vector3.zero;
				float handle_size 	= HandleUtility.GetHandleSize( position ) * myBigButtonHandleSizeMultiplier;
				Quaternion rotation = HAPI_AssetUtility.getQuaternion( myTempCamera.transform.localToWorldMatrix );
				Handles.Button(	position, rotation, handle_size, handle_size, Handles.RectangleCap );

				// Prevent the delete key from deleting the curve in this mode.
				if ( current_event.isKey && current_event.keyCode == KeyCode.Delete && mySelectedPoints.Count == 0 )
					Event.current.Use();
			}
		}
		else if ( myForceInspectorRedraw )
		{
			Repaint();
			myForceInspectorRedraw = false;
		}

		// Create selection area.
		if ( myAssetCurve.prIsEditingPoints && myIsMouseDown )
		{
			float sel_left			= Mathf.Min( myFirstMousePosition.x, mouse_position.x );
			float sel_top			= myTempCamera.pixelHeight - Mathf.Max( myFirstMousePosition.y, mouse_position.y );
			float sel_width			= Mathf.Abs( myFirstMousePosition.x - mouse_position.x );
			float sel_height		= Mathf.Abs( myFirstMousePosition.y - mouse_position.y );
			mySelectionArea			= new Rect( sel_left, sel_top, sel_width, sel_height );
		}
		else
			mySelectionArea			= new Rect();

		// Hide default transform handles.
		myIsTransformHandleHidden = myAssetCurve.prIsEditingPoints;

		// Update active control point.
		if ( mySelectedPoints.Count > 0 ) 
		{
			if ( current_event.isKey && current_event.keyCode == KeyCode.Delete )
			{ // Handle deletions.
				myAssetCurve.deletePoints( mySelectedPoints.ToArray() );
				clearSelection();
				Event.current.Use();
			}
			else
			{ // Create midpoint for the handle.
				Vector3 max_bounds = myAssetCurve.prPoints[ mySelectedPoints[ 0 ] ];
				Vector3 min_bounds = myAssetCurve.prPoints[ mySelectedPoints[ 0 ] ];
				for ( int i = 1; i < mySelectedPoints.Count; ++i )
				{
					Vector3 current_pos = myAssetCurve.prPoints[ mySelectedPoints[ i ] ];
					max_bounds.x = Mathf.Max( max_bounds.x, current_pos.x );
					max_bounds.y = Mathf.Max( max_bounds.y, current_pos.y );
					max_bounds.z = Mathf.Max( max_bounds.z, current_pos.z );
					min_bounds.x = Mathf.Min( min_bounds.x, current_pos.x );
					min_bounds.y = Mathf.Min( min_bounds.y, current_pos.y );
					min_bounds.z = Mathf.Min( min_bounds.z, current_pos.z );
				}
				Vector3 mid_pos = ( max_bounds + min_bounds ) / 2.0f;

				Vector3 new_mid_pos = Handles.PositionHandle( mid_pos, 
															  Quaternion.identity );
			
				if ( new_mid_pos != mid_pos )
				{
					myIsMouseDown = false;
					Vector3 delta = new_mid_pos - mid_pos;
					for ( int i = 0; i < mySelectedPoints.Count; ++i )
					{
						int point_index = mySelectedPoints[ i ];
						Vector3 old_pos = myAssetCurve.prPoints[ point_index ];
						Vector3 new_pos = old_pos + delta;
						myAssetCurve.updatePoint( point_index, new_pos );
					}

					// Remake and Draw Guide Geometry
					buildGuideGeometry();
				}
			} // Delete?
		}

		// Connection Mesh Draws
		if ( myConnectionMaterial != null && myConnectionMesh != null )
		{
			myConnectionMaterial.SetPass( 0 );
			Graphics.DrawMeshNow( myConnectionMesh, myAssetCurve.transform.localToWorldMatrix );
		}

		// Selection Mesh Draws
		if ( mySelectionMaterial != null && mySelectionMesh != null )
		{
			mySelectionMaterial.SetFloat( "_PointSize", HAPI_Host.prGuidePointSize );
			mySelectionMaterial.SetColor( "_Color", HAPI_Host.prGuideWireframeColour );
			if ( mySelectionMaterial.SetPass( 0 ) )
			{
				Graphics.DrawMeshNow( mySelectionMesh, myAssetCurve.transform.localToWorldMatrix );
			}

			mySelectionMaterial.SetFloat( "_PointSize", HAPI_Host.prGuidePointSize - myGuideBorderSize );
			mySelectionMaterial.SetColor( "_Color", Color.white );
			if ( mySelectionMaterial.SetPass( 1 ) )
			{
				Graphics.DrawMeshNow( mySelectionMesh, myAssetCurve.transform.localToWorldMatrix );
			}
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private

	private Vector3 getMousePosition( ref Event current_event )
	{
		Vector3 mouse_position = current_event.mousePosition;
		
		// Camera.current.pixelHeight != Screen.height for some reason.
		mouse_position.y = myTempCamera.pixelHeight - mouse_position.y;

		return mouse_position;
	}

	private void getKeyState( Event current_event )
	{
		// So, for some odd reason, it is possible (and highly likely) to get a the
		// EventType.KeyDown event triggerd but with a keyCode of KeyCode.None. Lovely.
		if ( current_event.type == EventType.KeyDown && current_event.keyCode != KeyCode.None )
		{
			myCurrentlyPressedKey = current_event.keyCode;
		}
		else if ( current_event.shift )
		{
			myCurrentlyPressedKey = KeyCode.LeftShift;
		}
		// I have to also interpret the Ignore event as the mouse up event because that's all I
		// get if the use lets go of the mouse button while over a different Unity window...
		else if ( current_event.type == EventType.KeyUp || current_event.type == EventType.Ignore )
		{
			myCurrentlyPressedKey = KeyCode.None;
		}
		else if ( myCurrentlyPressedKey == KeyCode.LeftShift && !current_event.shift )
		{
			myCurrentlyPressedKey = KeyCode.None;
		}
	}

	private void clearSelection()
	{
		mySelectedPoints.Clear();
		for ( int i = 0; i < mySelectedPointsMask.Count; ++i )
			mySelectedPointsMask[ i ] = false;

		if ( mySelectionMeshColours != null )
			for ( int i = 0; i < mySelectionMeshColours.Length; ++i )
				if ( myAssetCurve.prIsEditingPoints )
					mySelectionMeshColours[ i ] = HAPI_Host.prUnselectedGuideWireframeColour;
				else
					mySelectionMeshColours[ i ] = HAPI_Host.prUnselectableGuideWireframeColour;

		buildGuideGeometry();
	}

	private void togglePointSelection( int point_index )
	{
		if ( mySelectedPointsMask == null ||
			 mySelectedPoints == null ||
			 mySelectionMeshColours == null )
			return;

		if ( point_index < mySelectedPointsMask.Count )
		{
			if ( mySelectedPointsMask[ point_index ] )
			{
				mySelectedPointsMask[ point_index ] = false;
				mySelectedPoints.Remove( point_index );
				mySelectionMeshColours[ point_index ] = HAPI_Host.prUnselectedGuideWireframeColour;
			}
			else
			{
				mySelectedPointsMask[ point_index ] = true;
				mySelectedPoints.Add( point_index );
				mySelectionMeshColours[ point_index ] = HAPI_Host.prSelectedGuideWireframeColour;
			}
		}

		buildGuideGeometry();
	}

	private void buildGuideGeometry()
	{
		// Build Guide Lines Mesh -----------------------------------------------------------------------------------

		if ( myGuideLinesMaterial == null || myGuideLinesTexture == null || myGuideLinesMaterial == null )
		{
			myGuideLinesMaterial					= new Material( Shader.Find( "HAPI/DottedLine" ) );
			myGuideLinesMaterial.hideFlags			= HideFlags.HideAndDontSave;

			myGuideLinesTexture						= new Texture2D( 4, 1, TextureFormat.RGBA32, false );
			myGuideLinesTexture.hideFlags			= HideFlags.HideAndDontSave;
			myGuideLinesTexture.wrapMode			= TextureWrapMode.Repeat;

			// We only set 2 pixels to pure black and 2 to pure white. This way, we can change the
			// dashed line colour via a global colour shader property which is multiplied by
			// this texture.
			myGuideLinesTexture.SetPixel( 0, 0, new Color( 0.0f, 0.0f, 0.0f, 0.0f ) );
			myGuideLinesTexture.SetPixel( 1, 0, new Color( 0.0f, 0.0f, 0.0f, 0.0f ) );
			myGuideLinesTexture.SetPixel( 2, 0, new Color( 1.0f, 1.0f, 1.0f, 1.0f ) );
			myGuideLinesTexture.SetPixel( 3, 0, new Color( 1.0f, 1.0f, 1.0f, 1.0f ) );
			myGuideLinesTexture.Apply();
			
			myGuideLinesMaterial.mainTexture = myGuideLinesTexture;
			myGuideLinesMaterial.SetTexture( "_MainTex", myGuideLinesTexture );
			myGuideLinesMesh = new Mesh();
		}

		// Build Selection Mesh -------------------------------------------------------------------------------------

		if ( mySelectionMaterial == null )
		{
			mySelectionMaterial						= new Material( Shader.Find( "HAPI/Point" ) );
			mySelectionMaterial.hideFlags			= HideFlags.HideAndDontSave;
			mySelectionMaterial.shader.hideFlags	= HideFlags.HideAndDontSave;
		}

		mySelectionMesh								= new Mesh();
		mySelectionMesh.hideFlags					= HideFlags.HideAndDontSave;

		// Check if we need to resize the selection mask.
		while ( mySelectedPointsMask.Count < myAssetCurve.prPoints.Count )
			mySelectedPointsMask.Add( false );

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
					mySelectionMeshColours[ i ] = HAPI_Host.prUnselectableGuideWireframeColour;
				else
					mySelectionMeshColours[ i ] = HAPI_Host.prUnselectedGuideWireframeColour;
			}
		}

		mySelectionMesh.vertices	= selection_vertices;
		mySelectionMesh.colors		= mySelectionMeshColours;
		mySelectionMesh.SetIndices( selection_indices, MeshTopology.Points, 0 );

		// Build Connection Mesh ------------------------------------------------------------------------------------

		if ( myConnectionMaterial == null )
		{
			myConnectionMaterial					= new Material( Shader.Find( "HAPI/Line" ) );
			myConnectionMaterial.hideFlags			= HideFlags.HideAndDontSave;
			myConnectionMaterial.shader.hideFlags	= HideFlags.HideAndDontSave;
		}

		myConnectionMesh							= new Mesh();
		myConnectionMesh.hideFlags					= HideFlags.HideAndDontSave;

		Vector3[] connection_vertices = myAssetCurve.prPoints.ToArray();
		int[] connection_indices = new int[ connection_vertices.Length ];
		for ( int i = 0; i < connection_vertices.Length; ++i )
			connection_indices[ i ] = i;

		Color[] connection_colours = new Color[ connection_vertices.Length ];
		for ( int i = 0; i < connection_vertices.Length; ++i )
			connection_colours[ i ] = HAPI_Host.prGuideWireframeColour;

		myConnectionMesh.vertices = connection_vertices;
		myConnectionMesh.colors = connection_colours;
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
		bool add_points_mode_key	= myCurrentlyPressedKey == HAPI_Host.prAddingPointsModeHotKey;
		bool edit_points_mode_key	= myCurrentlyPressedKey == HAPI_Host.prEditingPointsModeHotKey;

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
		if ( myCurrentlyPressedKey == KeyCode.Escape || myCurrentlyPressedKey == KeyCode.Return )
		{
			add_points_mode				= false;
			edit_points_mode			= false;
			myForceInspectorRedraw		= true;
		}

		// Change the colours of the points if the edit points mode has changed.
		if ( edit_points_mode != myAssetCurve.prIsEditingPoints )
		{
			// Must assign this earlier than normal because clearSelection() will
			// use the value to determine the colour of the control points.
			// (between unselected and unselectable)
			myAssetCurve.prIsEditingPoints	= edit_points_mode;
			clearSelection();
		}

		myAssetCurve.prIsAddingPoints	= add_points_mode;
		myAssetCurve.prIsEditingPoints	= edit_points_mode;
		myAssetCurve.prModeChangeWait	= mode_change_wait;
	}

	private bool drawModeBorders( Color color, Vector3 mouse_position )
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

		// Draw selection rectangle.
		// NOTE: If we must ALWAYS draw this rectangle, even if no selection is being made.
		// If we add a decision statement here it will affect the drawing order and render
		// the full-screen button, used for preventing deselection of the curve, useless.
		GUI.color				= Color.white;
		GUI.Box( mySelectionArea, "" );
		GUI.color				= original_color;

		GUILayout.EndArea();
		Handles.EndGUI();

		return cancel_mode;
	}

	public static bool myIsTransformHandleHidden {
		get {
			System.Type type = typeof (Tools);
			FieldInfo field = type.GetField ("s_Hidden", BindingFlags.NonPublic | BindingFlags.Static);
			return ((bool) field.GetValue (null));
		}
		set {
			System.Type type = typeof (Tools);
			FieldInfo field = type.GetField ("s_Hidden", BindingFlags.NonPublic | BindingFlags.Static);
			field.SetValue (null, value);
		}
	}
	
	private HAPI_AssetCurve		myAssetCurve;

	private bool				myForceInspectorRedraw;
	private string				myAddPointButtonLabel;

	private const float			myActiveBorderWidth = 5;
	private Camera				myTempCamera;

	[SerializeField] 
	private GameObject			myTarget;

	private bool				myIsMouseDown;
	private KeyCode				myCurrentlyPressedKey;
	private Vector3				myFirstMousePosition;
	private const float			myBigButtonHandleSizeMultiplier = 1000000.0f;
	private const float			myIntersectionRayLength = 5000.0f;

	private const float			myGuideBorderSize = 4.0f;
	private const float			myGuideLinesDashTilingMultiplier = 4.0f;
	private Material			myGuideLinesMaterial;
	private Texture2D			myGuideLinesTexture;
	private Mesh				myGuideLinesMesh;

	private Rect				mySelectionArea;
	private Color[]				mySelectionMeshColours;
	private Mesh				mySelectionMesh;
	private Material			mySelectionMaterial;

	[SerializeField] 
	private List< int >			mySelectedPoints;
	[SerializeField] 
	private List< bool >		mySelectedPointsMask;

	private Mesh				myConnectionMesh;
	private Material			myConnectionMaterial;

	private Mode				myLastMode;
}
