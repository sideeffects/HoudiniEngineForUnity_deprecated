using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using HAPI;

[ ExecuteInEditMode ]
[ CustomEditor( typeof( HAPI_GeoInputControl ) ) ]
public class HAPI_GeoInputControlGUI : Editor
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties

	public Mesh prGeoMesh {
		get
		{ 
			if ( myGeo && myGeo.prEditableMesh )
				return myGeo.prEditableMesh;
			else
				return null;
		}
		private set {}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	public void OnEnable() 
	{
		myGeo					= target as HAPI_GeoInputControl;

		myForceInspectorRedraw	= false;

		myIsMouseDown			= false;
		myCurrentlyPressedKey	= KeyCode.None;
		myFirstMousePosition	= new Vector3();

		mySelectionArea			= new Rect();
		mySelectionMeshColours	= null;
		mySelectionMesh			= null;
		mySelectionMaterial		= null;
		mySelectedPoints		= new List< int >();
		mySelectedPointsMask	= new List< bool >();

		myLastMode				= HAPI_GeoInputControl.Mode.NONE;

		HAPI_Host.myRepaintDelegate += this.refresh;
		HAPI_Host.myDeselectionDelegate += this.deselect;
		HAPI_Host.mySelectionTarget = myGeo.gameObject;
	}

	public void OnDisable()
	{
		// This is called after OnSceneGUI sometimes for some reason.
		HAPI_Host.myRepaintDelegate -= this.refresh;
		HAPI_Host.myDeselectionDelegate -= this.deselect;
		HAPI_Host.mySelectionTarget = null;
	}

	public void refresh()
	{
		buildGuideGeometry();
	}

	public void deselect()
	{
		myIsTransformHandleHidden = false;
	}

	public override void OnInspectorGUI()
	{
		if ( myGeo == null )
			return;

		bool is_editable = myGeo.prEditable;

		// We can only build or do anything if we can link to our dll which
		// can only happen on the Windows x86 platform.
#if !UNITY_STANDALONE_WIN
		is_editable = false;
		HAPI_GUI.help( HAPI_GUIUtility.myPlatformUnsupportedMessage, MessageType.Info );
#else
		if ( !is_editable )
			HAPI_GUI.help( "This curve is not editable.", MessageType.Info );
#endif // !UNITY_STANDALONE_WIN

		bool gui_enable = GUI.enabled;
		GUI.enabled = is_editable;

		// STUFF

		GUI.enabled = gui_enable;
	}
	
	public void OnSceneGUI() 
	{
		// We can only build or do anything if we can link to our dll which
		// can only happen on the Windows x86 platform.
#if !UNITY_STANDALONE_WIN
		return;
		#pragma warning disable 0162
#endif // !UNITY_STANDALONE_WIN

		if ( myGeo == null )
			return;

		if ( prGeoMesh == null )
			return;

		// First Remake and Draw Guide Geometry if necessary.
		if ( mySelectionMesh == null )
			buildGuideGeometry();
		
		if ( !myTempCamera && Camera.current )
			myTempCamera = Camera.current;

		Event current_event 		= Event.current;
		Vector3 mouse_position		= getMousePosition( ref current_event );

		// Determine key state.
		getKeyState( current_event );

		// Decide modes.
		if ( current_event.type == EventType.Layout )
			decideModes( ref current_event );

		// Draw scene UI.
		drawSceneUI();

		if ( !current_event.alt )
		{
			if ( myGeo.prIsPaintingPoints )
			{
				Vector3 position	= Vector3.zero;
				float handle_size 	= HandleUtility.GetHandleSize( position ) * myBigButtonHandleSizeMultiplier;
				Quaternion rotation = HAPI_AssetUtility.getQuaternion( myTempCamera.transform.localToWorldMatrix );
				bool button_press 	= Handles.Button( 	position, 
														rotation,
														handle_size,
														handle_size,
														Handles.RectangleCap );

				Ray ray = myTempCamera.ScreenPointToRay( mouse_position );
				ray.origin = myTempCamera.transform.position;

				MeshCollider mesh_collider  = myGeo.getOrCreateComponent< MeshCollider >();
				RaycastHit hit_info;
				mesh_collider.Raycast( ray, out hit_info, myIntersectionRayLength );

				if ( hit_info.collider )
				{
					// Consume scroll-wheel event.
					if ( current_event.type == EventType.ScrollWheel
						&& areKeysTheSame( myCurrentlyPressedKey, HAPI_Host.prPaintingModeHotKey ) )
					{
						myGeo.prBrushSize += current_event.delta.y * myMouseWheelBrushSizeMultiplier;
						current_event.Use();
					}

					// Draw paint brush.
					Handles.DrawLine( hit_info.point, hit_info.point + hit_info.normal );
					Handles.CircleCap(
						0, hit_info.point, Quaternion.FromToRotation( Vector3.forward, hit_info.normal ),
						myGeo.prBrushSize );

					// Paint attributes on left-click.
					if ( button_press )
					{
						// Once we add a point we are no longer bound to the user holding down the add points key.
						// Add points mode is now fully activated.
						myGeo.prModeChangeWait = false;

						// Paint.
						myGeo.paint( hit_info, myGeo.prPaintAmount );
					}

					// Unpaint attributes on right-click.
					if ( current_event.isMouse && current_event.button == 1 )
					{
						// Once we add a point we are no longer bound to the user holding down the add points key.
						// Add points mode is now fully activated.
						myGeo.prModeChangeWait = false;

						// Paint.
						myGeo.paint( hit_info, -myGeo.prPaintAmount );
					}
				}
			}
			else if ( myGeo.prIsEditingPoints )
			{
				Handles.matrix = myGeo.transform.localToWorldMatrix;

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
					Vector3[] points = prGeoMesh.vertices;
					for ( int i = 0; points != null && i < points.Length; ++i )
					{
						Vector3 transformed_point = myGeo.transform.TransformPoint( points[ i ] );
						Vector3 proj_pos = myTempCamera.WorldToScreenPoint( transformed_point );
						proj_pos.z = 0.0f;

						if ( Mathf.Abs( mouse_delta.x ) > 1.5f || Mathf.Abs( mouse_delta.y ) > 1.5f )
						{
							if ( proj_pos.x >= min_bounds.x && proj_pos.x <= max_bounds.x &&
								 proj_pos.y >= min_bounds.y && proj_pos.y <= max_bounds.y )
							{
								// Once we modify a point we are no longer bound to the user holding down 
								// the point edit key. Edit point mode is now fully activated.
								myGeo.prModeChangeWait = false;
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
								myGeo.prModeChangeWait = false;
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
				if ( current_event.isKey && current_event.keyCode == KeyCode.Delete )
				{
					Event.current.Use();
				}

			} // Edit mode.
		} // Not currently pressing alt.
		
		if ( myForceInspectorRedraw )
		{
			Repaint();
			myForceInspectorRedraw = false;
		}

		// Create selection area.
		if ( myGeo.prIsEditingPoints && myIsMouseDown )
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
		myIsTransformHandleHidden = myGeo.prIsEditingPoints;

		// Update active control point.
		if ( mySelectedPoints.Count > 0 ) 
		{
			// Create midpoint for the handle.
			Vector3 max_bounds = prGeoMesh.vertices[ mySelectedPoints[ 0 ] ];
			Vector3 min_bounds = prGeoMesh.vertices[ mySelectedPoints[ 0 ] ];
			for ( int i = 1; i < mySelectedPoints.Count; ++i )
			{
				Vector3 current_pos = prGeoMesh.vertices[ mySelectedPoints[ i ] ];
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
					Vector3 old_pos = prGeoMesh.vertices[ point_index ];
					Vector3 new_pos = old_pos + delta;
					myGeo.updatePoint( point_index, new_pos );
				}

				// Remake and Draw Guide Geometry
				buildGuideGeometry();
			}
		}

		// Selection Mesh Draws
		if ( mySelectionMaterial != null && mySelectionMesh != null )
		{
			mySelectionMaterial.SetFloat( "_PointSize", HAPI_Host.prGuidePointSize );
			mySelectionMaterial.SetColor( "_Color", HAPI_Host.prGuideWireframeColour );
			if ( mySelectionMaterial.SetPass( 0 ) )
			{
				Graphics.DrawMeshNow( mySelectionMesh, myGeo.transform.localToWorldMatrix );
			}

			mySelectionMaterial.SetFloat( "_PointSize", HAPI_Host.prGuidePointSize - myGuideBorderSize );
			mySelectionMaterial.SetColor( "_Color", Color.white );
			if ( mySelectionMaterial.SetPass( 1 ) )
			{
				Graphics.DrawMeshNow( mySelectionMesh, myGeo.transform.localToWorldMatrix );
			}
		}

		// We can only build or do anything if we can link to our dll which
		// can only happen on the Windows x86 platform.
#if !UNITY_STANDALONE_WIN
		#pragma warning restore 0162
#endif // !UNITY_STANDALONE_WIN
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
			myCurrentlyPressedKey = current_event.keyCode;
		else if ( current_event.control )
			myCurrentlyPressedKey = KeyCode.LeftControl;
		else if ( current_event.shift )
			myCurrentlyPressedKey = KeyCode.LeftShift;
		else if ( current_event.type == EventType.KeyUp || current_event.type == EventType.Ignore )
			// I have to also interpret the Ignore event as the mouse up event because that's all I
			// get if the use lets go of the mouse button while over a different Unity window...
			myCurrentlyPressedKey = KeyCode.None;
		else if ( myCurrentlyPressedKey == KeyCode.LeftShift && !current_event.shift )
			myCurrentlyPressedKey = KeyCode.None;
	}

	private void clearSelection()
	{
		mySelectedPoints.Clear();
		for ( int i = 0; i < mySelectedPointsMask.Count; ++i )
			mySelectedPointsMask[ i ] = false;

		if ( mySelectionMeshColours != null )
			for ( int i = 0; i < mySelectionMeshColours.Length; ++i )
				if ( myGeo.prIsEditingPoints )
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
		if ( prGeoMesh == null )
			return;

		// Build Selection Mesh -------------------------------------------------------------------------------------

		if ( mySelectionMaterial == null )
		{
			mySelectionMaterial						= new Material( Shader.Find( "HAPI/MeshPoint" ) );
			mySelectionMaterial.hideFlags			= HideFlags.HideAndDontSave;
			mySelectionMaterial.shader.hideFlags	= HideFlags.HideAndDontSave;
		}

		mySelectionMesh								= new Mesh();
		mySelectionMesh.hideFlags					= HideFlags.HideAndDontSave;

		// Check if we need to resize the selection mask.
		while ( mySelectedPointsMask.Count < prGeoMesh.vertexCount )
			mySelectedPointsMask.Add( false );

		// Pretend we have two points if only one actually exists. Since DrawMesh won't draw
		// anything unless it has at least a line we need a dummy line to exist.
		// In this case we just create two points, both at the same position as 
		// the one real point.
		Vector3[] selection_vertices = null;
		if ( prGeoMesh.vertexCount == 1 )
		{
			selection_vertices = new Vector3[ 2 ];
			selection_vertices[ 0 ] = prGeoMesh.vertices[ 0 ];
			selection_vertices[ 1 ] = prGeoMesh.vertices[ 0 ];
		}
		else
			selection_vertices = prGeoMesh.vertices;

		// Create the selection indices.
		int[] selection_indices = new int[ selection_vertices.Length ];
		for ( int i = 0; i < selection_vertices.Length; ++i )
			selection_indices[ i ] = i;

		if ( mySelectionMeshColours == null || mySelectionMeshColours.Length != selection_vertices.Length )
		{
			mySelectionMeshColours = new Color[ selection_vertices.Length ];
			for ( int i = 0; i < selection_vertices.Length; ++i )
			{
				if ( !myGeo.prIsEditingPoints )
					mySelectionMeshColours[ i ] = HAPI_Host.prUnselectableGuideWireframeColour;
				else
					mySelectionMeshColours[ i ] = HAPI_Host.prUnselectedGuideWireframeColour;
			}
		}

		mySelectionMesh.vertices	= selection_vertices;
		mySelectionMesh.colors		= mySelectionMeshColours;
		mySelectionMesh.SetIndices( selection_indices, MeshTopology.Points, 0 );
	}

	private void changeModes( ref bool paint_mode, ref bool edit_points_mode, HAPI_GeoInputControl.Mode mode )
	{
		switch ( mode )
		{
			case HAPI_GeoInputControl.Mode.NONE: 
				{
					paint_mode = false;
					edit_points_mode = false;
					break;
				}
			case HAPI_GeoInputControl.Mode.PAINT:
				{
					paint_mode = true;
					edit_points_mode = false;
					break;
				}
			case HAPI_GeoInputControl.Mode.EDIT:
				{
					paint_mode = false;
					edit_points_mode = true;
					break;
				}
			default:
				Debug.LogError( "Invalid mode?" ); break;
		}
	}

	// Undistinguishes left and right shift and control.
	private bool areKeysTheSame( KeyCode key1, KeyCode key2 )
	{
		if ( key1 == key2 )
			return true;
		
		if ( ( key1 == KeyCode.LeftShift || key1 == KeyCode.RightShift )
			&& ( key2 == KeyCode.LeftShift || key2 == KeyCode.RightShift ) )
			return true;

		if ( ( key1 == KeyCode.LeftControl || key1 == KeyCode.RightControl )
			&& ( key2 == KeyCode.LeftControl || key2 == KeyCode.RightControl ) )
			return true;

		return false;
	}

	private void decideModes( ref Event current_event )
	{
		if ( !myGeo.prEditable )
		{
			myLastMode				= HAPI_GeoInputControl.Mode.NONE;
			myGeo.prCurrentMode		= HAPI_GeoInputControl.Mode.NONE;
			myGeo.prIsPaintingPoints	= false;
			myGeo.prIsEditingPoints	= false;
			myGeo.prModeChangeWait	= false;
			return;
		}

		bool paint_mode_key			= areKeysTheSame( myCurrentlyPressedKey, HAPI_Host.prPaintingModeHotKey );
		bool edit_points_mode_key	= areKeysTheSame( myCurrentlyPressedKey, HAPI_Host.prEditingPointsModeHotKey );

		bool paint_mode				= myGeo.prIsPaintingPoints;
		bool edit_points_mode		= myGeo.prIsEditingPoints;
		bool mode_change_wait		= myGeo.prModeChangeWait;

		if ( paint_mode )
		{
			if ( !mode_change_wait && edit_points_mode_key )
			{
				myLastMode			= HAPI_GeoInputControl.Mode.PAINT;

				paint_mode			= false;
				edit_points_mode	= true;
				mode_change_wait	= true;
			}
			else if ( mode_change_wait && !paint_mode_key )
			{
				changeModes( ref paint_mode, ref edit_points_mode, myLastMode );
				mode_change_wait	= false;
			}
		}
		else if ( edit_points_mode )
		{
			if ( !mode_change_wait && paint_mode_key )
			{
				myLastMode			= HAPI_GeoInputControl.Mode.EDIT;

				paint_mode			= true;
				edit_points_mode	= false;
				mode_change_wait	= true;
			}
			else if ( mode_change_wait && !edit_points_mode_key )
			{
				changeModes( ref paint_mode, ref edit_points_mode, myLastMode );
				mode_change_wait	= false;
			}
		}
		else
		{
			if ( paint_mode_key )
			{
				paint_mode			= true;
				mode_change_wait	= true;
				myLastMode			= HAPI_GeoInputControl.Mode.NONE;
			}
			else if ( edit_points_mode_key )
			{
				edit_points_mode	= true;
				mode_change_wait	= true;
				myLastMode			= HAPI_GeoInputControl.Mode.NONE;
			}
		}

		// Check if ENTER or ESC was pressed so we can exit the mode.
		if ( myCurrentlyPressedKey == KeyCode.Escape || myCurrentlyPressedKey == KeyCode.Return )
		{
			paint_mode				= false;
			edit_points_mode		= false;
			myForceInspectorRedraw	= true;
		}

		// Change the colours of the points if the edit points mode has changed.
		if ( edit_points_mode != myGeo.prIsEditingPoints )
		{
			// Must assign this earlier than normal because clearSelection() will
			// use the value to determine the colour of the control points.
			// (between unselected and unselectable)
			myGeo.prIsEditingPoints = edit_points_mode;
			clearSelection();
		}

		myGeo.prIsPaintingPoints	= paint_mode;
		myGeo.prIsEditingPoints		= edit_points_mode;
		myGeo.prModeChangeWait		= mode_change_wait;
	}

	private void drawSceneUI()
	{
		string title_text = HAPI_Constants.HAPI_PRODUCT_SHORT_NAME + " Input Geo";
		string paint_hotkey_string = HAPI_Host.prPaintingModeHotKey.ToString();
		string edit_hotkey_string = HAPI_Host.prEditingPointsModeHotKey.ToString();
		string help_text = "" + paint_hotkey_string + ": paint | " + 
						   edit_hotkey_string + ": edit points";

		int skin = EditorPrefs.GetInt( "UserSkin" );
		Color box_color = ( skin == 0 ? mySceneUILightColour : mySceneUIDarkColour );
		Color text_color = Color.white;

		if ( !myGeo.prEditable )
		{
			help_text = "This mesh is not editable.";
		}
		if ( myGeo.prIsPaintingPoints )
		{
			help_text = "Click on mesh: paint attribute | Mouse Scroll: change brush size | ESC or Enter: exit mode";
			box_color = HAPI_Host.prPaintingModeColour;
		}
		else if ( myGeo.prIsEditingPoints )
		{
			help_text = "Click or drag: select points | Hold Control: toggle-based selection | ESC or Enter: exit mode";
			box_color = HAPI_Host.prEditingPointsModeColour;
		}

		if ( !mySceneWindowHasFocus && myGeo.prEditable )
			help_text = "Scene window doesn't have focus. Hotkeys may not work. Right click anywhere in the scene to focus.";

		Color original_color		= GUI.color;
		
		float scene_width			= myTempCamera.pixelWidth;
		float scene_height			= myTempCamera.pixelHeight;
		float border_width			= myActiveBorderWidth;
		float border_padding		= mySceneUIBorderPadding;
		float border_total			= border_width + border_padding;
		float line_height			= mySceneUILineHeight;
		float line_padding			= mySceneUILinePadding;
		float double_line_padding	= 2.0f * line_padding;

		GUIStyle normal_text_style	= new GUIStyle( GUI.skin.label );
		normal_text_style.alignment	= TextAnchor.MiddleLeft;
		normal_text_style.fontStyle	= FontStyle.Normal;
		normal_text_style.fontSize	= (int) line_height - mySceneUIFontSizeFromLineHeightMod;

		GUIStyle bold_text_style	= new GUIStyle( GUI.skin.label );
		bold_text_style.alignment	= TextAnchor.MiddleLeft;
		bold_text_style.fontStyle	= FontStyle.Bold;
		bold_text_style.fontSize	= (int) line_height - mySceneUIFontSizeFromLineHeightMod;
		
		float box_height			= line_height;
		float box_top				= scene_height - border_total - box_height;

		float title_box_width		= bold_text_style.CalcSize( new GUIContent( title_text ) ).x;
		title_box_width				+= double_line_padding;

		// The mode box should be nothing if the curve is static since there are no options for static curves.
		float mode_box_width		= myGeo.prEditable ? mySceneUIModeIndicatorWidth : 0.0f;
		float help_box_width		= scene_width - title_box_width - mode_box_width - 
									  ( 2.0f * border_total ) - ( 2.0f * border_padding );
		
		float title_box_right		= border_total;
		float mode_box_right		= border_total + title_box_width + border_padding;
		float help_box_right		= mode_box_right + mode_box_width + border_padding;

		// Create background boxes texture.
		Texture2D box_texture		= new Texture2D( 1, 1 );
		box_texture.wrapMode		= TextureWrapMode.Repeat;
		box_texture.SetPixel( 0, 0, new Color( box_color.r - mySceneUIDarkeningFactor, 
											   box_color.g - mySceneUIDarkeningFactor, 
											   box_color.b - mySceneUIDarkeningFactor, 0.5f ) );
		box_texture.Apply();

		// Set up rectangles for the boxes and the labels.
		Rect title_box_rect  = new Rect( title_box_right, box_top, title_box_width, box_height );
		Rect mode_box_rect	 = new Rect( mode_box_right, box_top, mode_box_width, box_height );
		Rect help_box_rect	 = new Rect( help_box_right, box_top, help_box_width, box_height );
		Rect title_text_rect = new Rect( title_box_right + line_padding, box_top, 
										 title_box_width - double_line_padding, box_height - double_line_padding );
		Rect mode_text_rect  = new Rect( mode_box_right + line_padding, box_top, 
										 mode_box_width - double_line_padding, box_height - double_line_padding );
		Rect help_text_rect  = new Rect( help_box_right + line_padding, box_top, 
										 help_box_width - double_line_padding, box_height - double_line_padding );

		// Start Drawing --------------------------------------------------------------------------------------------
		Handles.BeginGUI();
		GUILayout.BeginArea( new Rect( 0, 0, Screen.width, Screen.height ) );

		// Draw the background boxes for the Scene UI.
		GUI.color = box_color;
		GUI.DrawTexture( title_box_rect, box_texture, ScaleMode.StretchToFill );
		if ( myGeo.prEditable )
			GUI.DrawTexture( mode_box_rect, box_texture, ScaleMode.StretchToFill );
		GUI.DrawTexture( help_box_rect, box_texture, ScaleMode.StretchToFill );

		// Draw the labels for the curve and the help.
		GUI.color = text_color;
		GUI.Label( title_text_rect, title_text, bold_text_style );
 		GUI.Label( help_text_rect, help_text, normal_text_style );

		if ( myGeo.prEditable )
		{
			// Set up mode selection toolbar.
			GUIStyle button_style	= new GUIStyle( GUI.skin.button );
			button_style.alignment	= TextAnchor.MiddleCenter;
			button_style.fontStyle	= FontStyle.Normal;
			button_style.fontSize	= (int) line_height - mySceneUIFontSizeFromLineHeightMod;
			Color toolbar_color		= box_color;
			toolbar_color.r			+= mySceneUIBrightningFactor;
			toolbar_color.g			+= mySceneUIBrightningFactor;
			toolbar_color.b			+= mySceneUIBrightningFactor;
			GUI.color				= toolbar_color;
			GUIContent[] modes		= new GUIContent[ 3 ];
			modes[ 0 ]				= new GUIContent( "View" );
			modes[ 1 ]				= new GUIContent( "Paint" );
			modes[ 2 ]				= new GUIContent( "Edit" );

			// Draw the mode selection toolbar.
			// Note: We want to disable the toolbar if a mode key is being held down because
			// if a button is pressed the current mode will imidiatly switch back to the mode
			// whos key is being held down...
			GUI.enabled				= ( myCurrentlyPressedKey != HAPI_Host.prPaintingModeHotKey ) &&
									  ( myCurrentlyPressedKey != HAPI_Host.prEditingPointsModeHotKey );
			HAPI_GeoInputControl.Mode last_mode = myGeo.prCurrentMode;
			myGeo.prCurrentMode = (HAPI_GeoInputControl.Mode) GUI.Toolbar( mode_text_rect, (int) last_mode, modes );
			if ( last_mode != myGeo.prCurrentMode )
				clearSelection();
			GUI.enabled = true;

			// Draw selection rectangle.
			if ( myGeo.prIsEditingPoints )
			{
				GUI.color = Color.white;
				GUI.Box( mySelectionArea, "" );
			}
		}

		// Draw yellow mode lines around the Scene view.
		if ( mySceneWindowHasFocus )
		{
			// Create texture.
			Texture2D border_texture	= new Texture2D( 1, 1 );
			border_texture.wrapMode		= TextureWrapMode.Repeat;
			border_texture.SetPixel( 0, 0, new Color( box_color.r, box_color.g, box_color.b, 0.6f ) );
			border_texture.Apply();

			float width					= myTempCamera.pixelWidth;
			float height				= myTempCamera.pixelHeight;

			if ( myGeo.prCurrentMode == HAPI_GeoInputControl.Mode.NONE )
			{
				border_texture.SetPixel( 0, 0, new Color( text_color.r, text_color.g, text_color.b, 0.6f ) );
				border_texture.Apply();
				border_width = myInactiveBorderWidth;
			}

			GUI.DrawTexture( new Rect( 0, 0, width, border_width ),								// Top
							 border_texture, ScaleMode.StretchToFill );
			GUI.DrawTexture( new Rect( 0, border_width, border_width, height - border_width ),	// Right
							 border_texture, ScaleMode.StretchToFill );
			GUI.DrawTexture( new Rect( border_width, height - border_width, width, height ),	// Bottom
							 border_texture, ScaleMode.StretchToFill );
			GUI.DrawTexture( new Rect( width - border_width, border_width,						// Left
									   width, height - border_width - border_width ), 
							 border_texture, ScaleMode.StretchToFill );
		}

		GUILayout.EndArea();
		Handles.EndGUI();
		// Stop Drawing ---------------------------------------------------------------------------------------------
	
		// Restore GUI colour.
		GUI.color = original_color;
	}

	public static bool mySceneWindowHasFocus {
		get
		{
			return EditorWindow.focusedWindow == SceneView.lastActiveSceneView;
		}
	}

	public static bool myIsTransformHandleHidden {
		get
		{
			System.Type type = typeof( Tools );
			FieldInfo field = type.GetField( "s_Hidden", BindingFlags.NonPublic | BindingFlags.Static );
			return ( (bool) field.GetValue( null ) );
		}
		set
		{
			System.Type type = typeof( Tools );
			FieldInfo field = type.GetField( "s_Hidden", BindingFlags.NonPublic | BindingFlags.Static );
			field.SetValue( null, value );
		}
	}

	private HAPI_GeoInputControl myGeo;

	private bool				myForceInspectorRedraw;

	private const float			myActiveBorderWidth = 5.0f;
	private const float			myInactiveBorderWidth = 2.0f;
	private Camera				myTempCamera;

	private bool				myIsMouseDown;
	private KeyCode				myCurrentlyPressedKey;

	private static Color		mySceneUIDarkColour					= new Color( 0.5f, 0.5f, 0.5f, 1.0f );
	private static Color		mySceneUILightColour				= new Color( 0.9f, 0.9f, 0.9f, 1.0f );
	private const float			mySceneUIBorderPadding				= 2.0f;
	private const float			mySceneUILineHeight					= 18.0f;
	private const float			mySceneUILinePadding				= 1.0f;
	private const int			mySceneUIFontSizeFromLineHeightMod	= 8;
	private const float			mySceneUIModeIndicatorWidth			= 160.0f;
	private const float			mySceneUIDarkeningFactor			= 0.5f;
	private const float			mySceneUIBrightningFactor			= 0.2f;

	private Vector3				myFirstMousePosition;
	private const float			myMouseWheelBrushSizeMultiplier		= 0.01f;
	private const float			myBigButtonHandleSizeMultiplier		= 1000000.0f;
	private const float			myIntersectionRayLength				= 5000.0f;

	private const float			myGuideBorderSize					= 4.0f;

	private Rect				mySelectionArea;
	private Color[]				mySelectionMeshColours;
	private Mesh				mySelectionMesh;
	private Material			mySelectionMaterial;

	[SerializeField] 
	private List< int >			mySelectedPoints;
	[SerializeField] 
	private List< bool >		mySelectedPointsMask;

	private HAPI_GeoInputControl.Mode myLastMode;
}
