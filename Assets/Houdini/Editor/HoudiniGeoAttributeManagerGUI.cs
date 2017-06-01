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
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

#if !( HAPI_ENABLE_RUNTIME )
#pragma warning disable 0162 // Unreachable Code
#pragma warning disable 0414 // Initialized but unused Private Member Variable
#endif // !( HAPI_ENABLE_RUNTIME )

public class HoudiniGeoAttributeManagerGUI
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties

	public Mesh prGeoMesh {
		get
		{ 
			if ( myManager && myManager.prEditableMesh )
				return myManager.prEditableMesh;
			else
				return null;
		}
		private set {}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	public HoudiniGeoAttributeManagerGUI( HoudiniGeoAttributeManager manager ) 
	{
		myManager					= manager;

		myForceInspectorRedraw		= false;
		myShowMinMax				= false;

		myIsMouseDown				= false;
		myMouseKey					= 0;
		myCurrentlyPressedKey		= KeyCode.None;
		myCurrentlyPressedSecondKey	= KeyCode.None;
		myCurrentlyPressedModeKey	= KeyCode.None;

		myFirstMousePosition		= new Vector3();

		mySelectionArea				= new Rect();
		mySelectionMeshColours		= null;
		mySelectionMesh				= null;
		mySelectionMaterial			= null;
		mySelectedPoints			= new List< int >();
		mySelectedPointsMask		= new List< bool >();

		myLastMode					= HoudiniGeoAttributeManager.Mode.NONE;

		HoudiniHost.myRepaintDelegate += this.refresh;
		HoudiniHost.myDeselectionDelegate += this.deselect;
		HoudiniHost.mySelectionTarget = myManager.prTransform.gameObject;
	}

	~HoudiniGeoAttributeManagerGUI()
	{
		// This is called after OnSceneGUI sometimes for some reason.
		HoudiniHost.myRepaintDelegate -= this.refresh;
		HoudiniHost.myDeselectionDelegate -= this.deselect;
		HoudiniHost.mySelectionTarget = null;
		myIsTransformHandleHidden = false;
	}

	public void refresh()
	{
		buildGuideGeometry();
	}

	public void deselect()
	{
		myIsTransformHandleHidden = false;
	}

	public void OnInspectorGUI()
	{
		
	}
	
	public void OnSceneGUI() 
	{
		OnSceneGUI( "Input Geo", 0, new string[]{ myManager.prTransform.name } );
	}

	public int OnSceneGUI( string tool_name, int selected_node_index, string[] node_list ) 
	{
		// We can only build or do anything if we can link to our libraries.
#if !( HAPI_ENABLE_RUNTIME )
		return selected_node_index;
#endif // !( HAPI_ENABLE_RUNTIME )

		if ( myManager == null || myManager.prMeshCollider == null )
			return selected_node_index;

		if ( prGeoMesh == null )
			return selected_node_index;

		// First Remake and Draw Guide Geometry if necessary.
		if ( mySelectionMesh == null )
			buildGuideGeometry();
		
		if ( !myTempCamera && Camera.current )
			myTempCamera = Camera.current;

		Event current_event = Event.current;
		Vector3 mouse_position = getMousePosition( ref current_event );

		// Determine key state.
		getKeyState( current_event );

		//if ( current_event.type != EventType.Layout && current_event.type != EventType.Repaint
		//	&& current_event.type != EventType.mouseMove )
			//Debug.Log( current_event.type );

		//if ( GUI.GetNameOfFocusedControl() != "" )
			//Debug.Log( GUI.GetNameOfFocusedControl() );

		// Decide modes.
		if ( current_event.type == EventType.Layout && mySceneWindowHasFocus )
			decideModes( ref current_event );

		// Draw scene UI.
		selected_node_index = drawSceneUI( tool_name, selected_node_index, node_list );

		if ( !current_event.alt && !( current_event.type == EventType.MouseDown && current_event.button == 1 ) )
		{
			if ( myManager.prIsPaintingPoints )
			{
				// Switch nodes or attributes via shortcut keys.
				if ( areKeysTheSame( myCurrentlyPressedSecondKey, HoudiniHost.prPaintingNodeSwitchHotKey ) &&
					areKeysTheSame( myCurrentlyPressedModeKey, HoudiniHost.prPaintingModeHotKey ) )
				{
					selected_node_index = ( selected_node_index + 1 ) % node_list.Length;
					myCurrentlyPressedSecondKey = KeyCode.None;
					myCurrentlyPressedKey = KeyCode.None;
					return selected_node_index;
				}
				if ( areKeysTheSame( myCurrentlyPressedSecondKey, HoudiniHost.prPaintingAttributeSwitchHotKey ) &&
					areKeysTheSame( myCurrentlyPressedModeKey, HoudiniHost.prPaintingModeHotKey ) )
				{
					int selected_attribute_index = 0;
					for ( int i = 0; i < myManager.prAttributes.Count; ++i )
						if ( myManager.prAttributes[ i ].prName == myManager.prActiveAttribute.prName )
							selected_attribute_index = i;

					selected_attribute_index = ( selected_attribute_index + 1 ) % myManager.prAttributes.Count;

					string selected_attribute_name = myManager.prAttributes[ selected_attribute_index ].prName;
					myManager.setActiveAttribute( selected_attribute_name );

					myCurrentlyPressedSecondKey = KeyCode.None;
					myCurrentlyPressedKey = KeyCode.None;
					return selected_node_index;
				}
				if ( areKeysTheSame( myCurrentlyPressedSecondKey, HoudiniHost.prPaintingValueChangeHotKey ) &&
					areKeysTheSame( myCurrentlyPressedModeKey, HoudiniHost.prPaintingModeHotKey ) &&
					current_event.type == EventType.ScrollWheel )
				{
					for ( int i = 0; i < myManager.prActiveAttribute.prTupleSize; ++i )
					{
						if ( myManager.prActiveAttribute.prType == HoudiniGeoAttribute.Type.BOOL
							|| myManager.prActiveAttribute.prType == HoudiniGeoAttribute.Type.INT )
							myManager.prActiveAttribute.prIntPaintValue[ i ] = Mathf.Clamp(
								myManager.prActiveAttribute.prIntPaintValue[ i ] +
									(int) ( -current_event.delta.y * ( HoudiniHost.prPaintBrushRate + 1.0f ) ),
								myManager.prActiveAttribute.prIntMin,
								myManager.prActiveAttribute.prIntMax );
						else if ( myManager.prActiveAttribute.prType == HoudiniGeoAttribute.Type.FLOAT )
							myManager.prActiveAttribute.prFloatPaintValue[ i ] = Mathf.Clamp(
								myManager.prActiveAttribute.prFloatPaintValue[ i ] +
									-current_event.delta.y * HoudiniHost.prPaintBrushRate,
								myManager.prActiveAttribute.prFloatMin,
								myManager.prActiveAttribute.prFloatMax );
						else if ( myManager.prActiveAttribute.prType == HoudiniGeoAttribute.Type.STRING )
						{
							// Makes no sense to change the string paint value via hotkey so do nothing.
						}
					}
					current_event.Use();

					myCurrentlyPressedSecondKey = KeyCode.None;
					myCurrentlyPressedKey = KeyCode.None;
					return selected_node_index;
				}

				if ( myManager.prModeChangeWait ||
					( myCurrentlyPressedKey != myMayaBrushResizeKey &&
						myCurrentlyPressedKey != myHoudiniBrushResizeKey ) ||
					( current_event.type == EventType.MouseDown ) &&
						( myCurrentlyPressedKey == myMayaBrushResizeKey ||
							myCurrentlyPressedKey == myHoudiniBrushResizeKey ) )
				{
					myFirstMousePosition = mouse_position;
				}

				// The right mouse button has special interpretation in the Unity viewport.
				// So much so, that simply Use()'ing the event is not enough. We need to actually
				// change the event's mouse button to the left mouse button (0) as soon as
				// we detect the right mouse button is pressed (in the Layout event). Of
				// course, WE need to know when the right mouse button is pressed so we 
				// first remember it and then change the current event. Hacks!
				if ( current_event.type == EventType.Layout && myMouseKey != current_event.button )
					myMouseKey = current_event.button;
				Event.current.button = 0;

				// Capture the mouse down here because the Handles.Button will use it after this.
				bool mouseDown = current_event.type == EventType.MouseDown;

				Vector3 position	= Vector3.zero;
				float handle_size 	= HandleUtility.GetHandleSize( position ) * myBigButtonHandleSizeMultiplier;
				Quaternion rotation = HoudiniAssetUtility.getQuaternion( myTempCamera.transform.localToWorldMatrix );
#if UNITY_5_5_OR_NEWER
				Handles.Button( position, rotation, handle_size, handle_size, Handles.RectangleHandleCap );
#else
				Handles.Button( position, rotation, handle_size, handle_size, Handles.RectangleCap );
#endif

				Ray ray = myTempCamera.ScreenPointToRay( myFirstMousePosition );
				ray.origin = myTempCamera.transform.position;

				MeshCollider mesh_collider  = myManager.prMeshCollider;
				if ( !mesh_collider.enabled )
					mesh_collider.enabled = true;
				RaycastHit hit_info;
				mesh_collider.Raycast( ray, out hit_info, myIntersectionRayLength );

				if ( hit_info.collider )
				{
					// Draw paint brush.
					Handles.DrawLine(
						hit_info.point,
						hit_info.point + ( Vector3.Normalize( hit_info.normal ) * myManager.prBrushRadius ) );
#if UNITY_5_5_OR_NEWER
					Handles.CircleHandleCap(
						0, hit_info.point,
						Quaternion.FromToRotation( Vector3.forward, hit_info.normal ),
						myManager.prBrushRadius, EventType.Repaint );
#else
					Handles.CircleCap(
						0, hit_info.point,
						Quaternion.FromToRotation( Vector3.forward, hit_info.normal ),
						myManager.prBrushRadius );
#endif

					// Consume scroll-wheel event.
					if ( current_event.type == EventType.ScrollWheel
						&& areKeysTheSame( myCurrentlyPressedModeKey, HoudiniHost.prPaintingModeHotKey ) )
					{
						myManager.prBrushRadius += current_event.delta.y * myMouseWheelBrushSizeMultiplier;
						current_event.Use();
					}

					// Change brush size via grow/shrink keys.
					if ( current_event.type == EventType.KeyDown )
						if ( current_event.keyCode == myPhotoshopBrushShrinkKey )
							myManager.prBrushRadius -= myMouseWheelBrushSizeMultiplier;
						else if ( current_event.keyCode == myPhotoshopBrushGrowKey )
							myManager.prBrushRadius += myMouseWheelBrushSizeMultiplier;

					if ( !myManager.prModeChangeWait &&
						( myCurrentlyPressedKey == myMayaBrushResizeKey || 
						myCurrentlyPressedKey == myHoudiniBrushResizeKey ) )
					{
						EditorGUIUtility.AddCursorRect( myTempCamera.pixelRect, MouseCursor.ResizeHorizontal );
						if ( current_event.type == EventType.MouseDrag )
							myManager.prBrushRadius += current_event.delta.x * myMouseWheelBrushSizeMultiplier;
					}
					else
					{
						// Paint attributes on left-click.
						if ( current_event.type == EventType.MouseDrag || mouseDown )
						{
							// Once we add a point we are no longer bound to the user holding down the add points key.
							// Add points mode is now fully activated.
							myManager.prModeChangeWait = false;

							// Paint.
							if ( myMouseKey == 0 )
								myManager.paint( hit_info, false );
							else
								myManager.paint( hit_info, true );
						}
					}
				}
			}
			else if ( myManager.prIsEditingPoints )
			{
				Handles.matrix = myManager.prTransform.localToWorldMatrix;

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
						Vector3 transformed_point = myManager.prTransform.TransformPoint( points[ i ] );
						Vector3 proj_pos = myTempCamera.WorldToScreenPoint( transformed_point );
						proj_pos.z = 0.0f;

						if ( Mathf.Abs( mouse_delta.x ) > 1.5f || Mathf.Abs( mouse_delta.y ) > 1.5f )
						{
							if ( proj_pos.x >= min_bounds.x && proj_pos.x <= max_bounds.x &&
								 proj_pos.y >= min_bounds.y && proj_pos.y <= max_bounds.y )
							{
								// Once we modify a point we are no longer bound to the user holding down 
								// the point edit key. Edit point mode is now fully activated.
								myManager.prModeChangeWait = false;
								togglePointSelection( i );
							}
						} // drag
						else
						{
							float distance = Vector3.Distance( mouse_position, proj_pos );
							if ( distance < HoudiniHost.prMinDistanceForPointSelection )
							{
								// Once we modify a point we are no longer bound to the user holding down 
								// the point edit key. Edit point mode is now fully activated.
								myManager.prModeChangeWait = false;
								togglePointSelection( i );
							} // if point hit
						} // single click
					} // for all points
				} // mouse up
				
				// Prevent click from being passed lower (this is so stupid!).
				Vector3 position	= Vector3.zero;
				float handle_size 	= HandleUtility.GetHandleSize( position ) * myBigButtonHandleSizeMultiplier;
				Quaternion rotation = HoudiniAssetUtility.getQuaternion( myTempCamera.transform.localToWorldMatrix );
#if UNITY_5_5_OR_NEWER
				Handles.Button(	position, rotation, handle_size, handle_size, Handles.RectangleHandleCap );
#else
				Handles.Button(	position, rotation, handle_size, handle_size, Handles.RectangleCap );
#endif

				// Prevent the delete key from deleting the mesh in this mode.
				if ( current_event.isKey && current_event.keyCode == KeyCode.Delete )
				{
					Event.current.Use();
				}

			} // Edit mode.
		} // Not currently pressing alt.
		
		if ( myForceInspectorRedraw )
		{
			//Repaint();
			myForceInspectorRedraw = false;
		}

		// Create selection area.
		if ( myManager.prIsEditingPoints && myIsMouseDown )
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
		myIsTransformHandleHidden = myManager.prIsPaintingPoints || myManager.prIsEditingPoints;

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
			Vector3 new_mid_pos = Handles.PositionHandle( mid_pos, Quaternion.identity );

			if ( new_mid_pos != mid_pos )
			{
				myIsMouseDown = false;
				Vector3 delta = new_mid_pos - mid_pos;
				for ( int i = 0; i < mySelectedPoints.Count; ++i )
				{
					int point_index = mySelectedPoints[ i ];
					Vector3 old_pos = prGeoMesh.vertices[ point_index ];
					Vector3 new_pos = old_pos + delta;
					myManager.updatePoint( point_index, new_pos );
				}

				// Remake and Draw Guide Geometry
				buildGuideGeometry();
			}
		}

		// Selection Mesh Draws
		if ( mySelectionMaterial != null && mySelectionMesh != null && myManager.prIsEditingPoints )
		{
			mySelectionMaterial.SetFloat( "_PointSize", HoudiniHost.prGuidePointSize );
			mySelectionMaterial.SetColor( "_Color", HoudiniHost.prGuideWireframeColour );
			if ( mySelectionMaterial.SetPass( 0 ) )
			{
				// TODO: Clean this up!

				Camera tempCamera = Camera.current;
				float s = ( HoudiniHost.prGuidePointSize ) / 2.0f;
				float w = tempCamera.pixelWidth;
				float h = tempCamera.pixelHeight;

				GL.PushMatrix();
				GL.Begin( GL.QUADS );
				GL.LoadOrtho();

				for ( int i = 0; i < mySelectionMesh.vertexCount; ++i )
				{
					Vector3 p = mySelectionMesh.vertices[ i ];
					p = myManager.prTransform.TransformPoint( p );

					p = tempCamera.WorldToScreenPoint( p );

					if ( p.x < 0.0f || p.x > w )
						continue;
					if ( p.y < 0.0f || p.y > h )
						continue;
					if ( p.z < 0.0f )
						continue;

					p.z = 0.0f;

					GL.Color( mySelectionMeshColours[ i ] );
					GL.Vertex3( ( p.x + s ) / w, ( p.y + s ) / h, p.z );
					GL.Vertex3( ( p.x + s ) / w, ( p.y - s ) / h, p.z );
					GL.Vertex3( ( p.x - s ) / w, ( p.y - s ) / h, p.z );
					GL.Vertex3( ( p.x - s ) / w, ( p.y + s ) / h, p.z );
				}

				GL.End();
				GL.PopMatrix();
			}

			mySelectionMaterial.SetFloat( "_PointSize", HoudiniHost.prGuidePointSize - myGuideBorderSize );
			mySelectionMaterial.SetColor( "_Color", Color.white );
			if ( mySelectionMaterial.SetPass( 1 ) )
			{
				// TODO: Clean this up!

				Camera tempCamera = Camera.current;
				float s = ( HoudiniHost.prGuidePointSize - myGuideBorderSize ) / 2.0f;
				float w = tempCamera.pixelWidth;
				float h = tempCamera.pixelHeight;

				GL.PushMatrix();
				GL.Begin( GL.QUADS );
				GL.LoadOrtho();

				for ( int i = 0; i < mySelectionMesh.vertexCount; ++i )
				{
					Vector3 p = mySelectionMesh.vertices[ i ];
					p = myManager.prTransform.TransformPoint( p );

					p = tempCamera.WorldToScreenPoint( p );

					if ( p.x < 0.0f || p.x > w )
						continue;
					if ( p.y < 0.0f || p.y > h )
						continue;
					if ( p.z < 0.0f )
						continue;

					p.z = 0.0f;

					GL.Color( mySelectionMeshColours[ i ] );
					GL.Vertex3( ( p.x + s ) / w, ( p.y + s ) / h, p.z );
					GL.Vertex3( ( p.x + s ) / w, ( p.y - s ) / h, p.z );
					GL.Vertex3( ( p.x - s ) / w, ( p.y - s ) / h, p.z );
					GL.Vertex3( ( p.x - s ) / w, ( p.y + s ) / h, p.z );
				}

				GL.End();
				GL.PopMatrix();
			}
		}

		return selected_node_index;

		// We can only build or do anything if we can link to our libraries.
#if !(HAPI_ENABLE_RUNTIME)
	#pragma warning restore 0162
#endif // !( HAPI_ENABLE_RUNTIME )
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private

	private Vector3 getMousePosition( ref Event current_event )
	{
		Vector3 mouse_position = current_event.mousePosition;

		// We need to take PixelPerPoints into consideration for Retina displays
		float fPixelsPerPoints = HoudiniGUIUtility.getPixelsPerPoint();
		mouse_position *= fPixelsPerPoints;

		// Camera.current.pixelHeight != Screen.height for some reason.
		mouse_position.y = myTempCamera.pixelHeight - mouse_position.y;

		return mouse_position;
	}

	private bool isModeKey( KeyCode key )
	{
		if ( key == HoudiniHost.prPaintingModeHotKey || key == HoudiniHost.prEditingPointsModeHotKey )
			return true;
		return false;
	}

	private void pressKey( KeyCode key )
	{
		if ( isModeKey( key ) )
			myCurrentlyPressedModeKey = key;
		else
			myCurrentlyPressedSecondKey = key;

		myCurrentlyPressedKey = key;
	}

	private void depressKey( KeyCode key )
	{
		if ( isModeKey( key ) )
			myCurrentlyPressedModeKey = KeyCode.None;
		else
			myCurrentlyPressedSecondKey = KeyCode.None;

		myCurrentlyPressedKey = KeyCode.None;
	}

	private void getKeyState( Event current_event )
	{
		// So, for some odd reason, it is possible (and highly likely) to get a the
		// EventType.KeyDown event triggerd but with a keyCode of KeyCode.None. Lovely.
		if ( current_event.type == EventType.KeyDown && current_event.keyCode != KeyCode.None )
		{
			pressKey( current_event.keyCode );
		}
		else if ( current_event.control )
		{
			pressKey( KeyCode.LeftControl );
		}
		else if ( current_event.shift )
		{
			pressKey( KeyCode.LeftShift );
		}
		else if ( current_event.type == EventType.KeyUp || current_event.type == EventType.Ignore )
		{
			// I have to also interpret the Ignore event as the mouse up event because that's all I
			// get if the use lets go of the mouse button while over a different Unity window...
			depressKey( current_event.keyCode );
		}
		else if ( myCurrentlyPressedModeKey == KeyCode.LeftShift && !current_event.shift )
		{
			myCurrentlyPressedModeKey = KeyCode.None;
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
				if ( myManager.prIsEditingPoints )
					mySelectionMeshColours[ i ] = HoudiniHost.prUnselectedGuideWireframeColour;
				else
					mySelectionMeshColours[ i ] = HoudiniHost.prUnselectableGuideWireframeColour;

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
				mySelectionMeshColours[ point_index ] = HoudiniHost.prUnselectedGuideWireframeColour;
			}
			else
			{
				mySelectedPointsMask[ point_index ] = true;
				mySelectedPoints.Add( point_index );
				mySelectionMeshColours[ point_index ] = HoudiniHost.prSelectedGuideWireframeColour;
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
			mySelectionMaterial						= new Material( Shader.Find( "Houdini/MeshPoint" ) );
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
				if ( !myManager.prIsEditingPoints )
					mySelectionMeshColours[ i ] = HoudiniHost.prUnselectableGuideWireframeColour;
				else
					mySelectionMeshColours[ i ] = HoudiniHost.prUnselectedGuideWireframeColour;
			}
		}

		mySelectionMesh.vertices	= selection_vertices;
		mySelectionMesh.colors		= mySelectionMeshColours;
		mySelectionMesh.SetIndices( selection_indices, MeshTopology.Points, 0 );
	}

	private void changeModes( ref bool paint_mode, ref bool edit_points_mode, HoudiniGeoAttributeManager.Mode mode )
	{
		switch ( mode )
		{
			case HoudiniGeoAttributeManager.Mode.NONE: 
				{
					paint_mode = false;
					edit_points_mode = false;
					break;
				}
			case HoudiniGeoAttributeManager.Mode.PAINT:
				{
					paint_mode = true;
					edit_points_mode = false;
					break;
				}
			case HoudiniGeoAttributeManager.Mode.EDIT:
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
		if ( !myManager.prEditable )
		{
			myLastMode = HoudiniGeoAttributeManager.Mode.NONE;
			myManager.prCurrentMode = HoudiniGeoAttributeManager.Mode.NONE;
			myManager.prIsPaintingPoints = false;
			myManager.prIsEditingPoints = false;
			myManager.prModeChangeWait = false;
			return;
		}

		bool paint_mode_key			= areKeysTheSame( myCurrentlyPressedModeKey, HoudiniHost.prPaintingModeHotKey );
		bool edit_points_mode_key	= areKeysTheSame( myCurrentlyPressedModeKey, HoudiniHost.prEditingPointsModeHotKey );

		bool paint_mode				= myManager.prIsPaintingPoints;
		bool edit_points_mode		= myManager.prIsEditingPoints;
		bool mode_change_wait		= myManager.prModeChangeWait;

		if ( paint_mode )
		{
			if ( !mode_change_wait && edit_points_mode_key )
			{
				myLastMode			= HoudiniGeoAttributeManager.Mode.PAINT;

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
				myLastMode			= HoudiniGeoAttributeManager.Mode.EDIT;

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
				myLastMode			= HoudiniGeoAttributeManager.Mode.NONE;
			}
			else if ( edit_points_mode_key )
			{
				edit_points_mode	= true;
				mode_change_wait	= true;
				myLastMode			= HoudiniGeoAttributeManager.Mode.NONE;
			}
		}

		// Check if ENTER or ESC was pressed so we can exit the mode.
		if ( myCurrentlyPressedSecondKey == KeyCode.Escape || 
			( myCurrentlyPressedSecondKey == KeyCode.Return &&
				!GUI.GetNameOfFocusedControl().StartsWith( myPaintValuesFieldName ) ) )
		{
			paint_mode				= false;
			edit_points_mode		= false;
			myForceInspectorRedraw	= true;
		}

		// Change the colours of the points if the edit points mode has changed.
		if ( edit_points_mode != myManager.prIsEditingPoints )
		{
			// Must assign this earlier than normal because clearSelection() will
			// use the value to determine the colour of the control points.
			// (between unselected and unselectable)
			myManager.prIsEditingPoints = edit_points_mode;
			clearSelection();
		}

		myManager.prIsPaintingPoints	= paint_mode;
		myManager.prIsEditingPoints		= edit_points_mode;
		myManager.prModeChangeWait		= mode_change_wait;
	}

	private void drawToolSceneUI( string tool_name )
	{
		string title_text = HoudiniConstants.HAPI_PRODUCT_SHORT_NAME + " " + tool_name;
		string paint_hotkey_string = HoudiniHost.prPaintingModeHotKey.ToString();
		string edit_hotkey_string = HoudiniHost.prEditingPointsModeHotKey.ToString();
		string help_text = "" + paint_hotkey_string + ": paint | " + 
						   edit_hotkey_string + ": edit points";

		int skin = EditorPrefs.GetInt( "UserSkin" );
		Color box_color = ( skin == 0 ? mySceneUILightColour : mySceneUIDarkColour );
		Color text_color = Color.white;

		if ( !myManager.prEditable )
		{
			help_text = "This mesh is not editable.";
		}
		if ( myManager.prIsPaintingPoints )
		{
			help_text = "Click on mesh: paint attribute | " + paint_hotkey_string + " + Mouse Scroll: change brush size | ESC or Enter: exit mode";
			box_color = HoudiniHost.prPaintingModeColour;
		}
		else if ( myManager.prIsEditingPoints )
		{
			help_text = "Click or drag: select points | Hold Control: toggle-based selection | ESC or Enter: exit mode";
			box_color = HoudiniHost.prEditingPointsModeColour;
		}

		if ( !mySceneWindowHasFocus && myManager.prEditable )
			help_text = "Scene window doesn't have focus. Hotkeys may not work. Right click anywhere in the scene to focus.";

		Color original_color		= GUI.color;

		// We need to take PixelsPerPoint into consideration for Retina Displays
		float fPixelsPerPoints      = HoudiniGUIUtility.getPixelsPerPoint();

		float scene_width			= myTempCamera.pixelWidth / fPixelsPerPoints;
		float scene_height			= myTempCamera.pixelHeight / fPixelsPerPoints;
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

		// The mode box should be nothing if the mesh is static since there are no options for static meshes.
		float mode_box_width		= myManager.prEditable ? mySceneUIModeIndicatorWidth : 0.0f;
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

		// Set up rectangles for the tool boxes and the labels.
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

		float screenWidth = Screen.width / fPixelsPerPoints;
		float screenHeight = Screen.height / fPixelsPerPoints;
		GUILayout.BeginArea( new Rect( 0, 0, screenWidth, screenHeight ) );

		// Draw the background boxes for the Scene UI.
		GUI.color = box_color;
		GUI.DrawTexture( title_box_rect, box_texture, ScaleMode.StretchToFill );
		if ( myManager.prEditable )
			GUI.DrawTexture( mode_box_rect, box_texture, ScaleMode.StretchToFill );
		GUI.DrawTexture( help_box_rect, box_texture, ScaleMode.StretchToFill );

		// Draw the labels for the mesh and the help.
		GUI.color = text_color;
		GUI.Label( title_text_rect, title_text, bold_text_style );
 		GUI.Label( help_text_rect, help_text, normal_text_style );

		if ( myManager.prEditable )
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
			GUI.enabled = 
				!mySceneWindowHasFocus ||
				( ( myCurrentlyPressedModeKey != HoudiniHost.prPaintingModeHotKey ) &&
				  ( myCurrentlyPressedModeKey != HoudiniHost.prEditingPointsModeHotKey ) );
			HoudiniGeoAttributeManager.Mode last_mode = myManager.prCurrentMode;
			myManager.prCurrentMode = (HoudiniGeoAttributeManager.Mode) GUI.Toolbar( mode_text_rect, (int) last_mode, modes );
			if ( last_mode != myManager.prCurrentMode )
				clearSelection();
			GUI.enabled = true;

			// Draw selection rectangle.
			if ( myManager.prIsEditingPoints )
			{
				GUI.color = Color.white;
				GUI.Box( mySelectionArea, "" );
			}
		}

		// Draw mode lines around the Scene view.
		if ( mySceneWindowHasFocus )
		{
			// Create texture.
			Texture2D border_texture	= new Texture2D( 1, 1 );
			border_texture.wrapMode		= TextureWrapMode.Repeat;
			border_texture.SetPixel( 0, 0, new Color( box_color.r, box_color.g, box_color.b, 0.6f ) );
			border_texture.Apply();

			float width					= scene_width;
			float height				= scene_height;

			if ( myManager.prCurrentMode == HoudiniGeoAttributeManager.Mode.NONE )
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

	private int drawPaintingSceneUI( int selected_node_index, string[] node_list )
	{
		string node_text = "Node:";
		string attribute_text = "Attribute:";
		string mode_text = "Mode:";
		string value_text = "Value:";
		string rate_text = "Rate:";
		string liveup_text = "Live Updates";

		Color box_color = HoudiniHost.prPaintingModeColour;
		Color text_color = Color.white;

		Color original_color		= GUI.color;
		
		float scene_width			= myTempCamera.pixelWidth;
		float border_width			= myActiveBorderWidth;
		float border_padding		= mySceneUIBorderPadding;
		float border_total			= border_width + border_padding;
		float line_height			= mySceneUILineHeight;
		float line_padding			= mySceneUILinePadding;
		float double_line_padding	= 2.0f * line_padding;
		float dropdown_width		= 80.0f;
		float field_width			= 50.0f;
		float toggle_width			= 14.0f;

		GUIStyle normal_text_style	= new GUIStyle( GUI.skin.label );
		normal_text_style.alignment	= TextAnchor.MiddleLeft;
		normal_text_style.fontStyle	= FontStyle.Normal;
		normal_text_style.fontSize	= (int) line_height - mySceneUIFontSizeFromLineHeightMod;

		GUIStyle bold_text_style	= new GUIStyle( GUI.skin.label );
		bold_text_style.alignment	= TextAnchor.MiddleLeft;
		bold_text_style.fontStyle	= FontStyle.Bold;
		bold_text_style.fontSize	= (int) line_height - mySceneUIFontSizeFromLineHeightMod;
		
		float box_height			= line_height;
		float expanded_box_height	= myShowMinMax ? box_height + line_padding + box_height : box_height;
		float box_top				= border_total;

		// Get text widths.
		float node_text_width		= bold_text_style.CalcSize( new GUIContent( node_text ) ).x
									  + double_line_padding;
		float attribute_text_width	= normal_text_style.CalcSize( new GUIContent( attribute_text ) ).x
									  + double_line_padding;
		float mode_text_width		= normal_text_style.CalcSize( new GUIContent( mode_text ) ).x
									  + double_line_padding;
		float value_text_width		= normal_text_style.CalcSize( new GUIContent( value_text ) ).x
									  + double_line_padding;
		float rate_text_width		= normal_text_style.CalcSize( new GUIContent( rate_text ) ).x
									  + double_line_padding;
		float liveup_text_width		= normal_text_style.CalcSize( new GUIContent( liveup_text ) ).x
									  + double_line_padding;

		float node_box_width		= node_text_width + dropdown_width + line_padding;
		float attribute_box_width	= attribute_text_width + dropdown_width + line_padding;
		float mode_box_width		= mode_text_width + dropdown_width + line_padding;
		float rate_box_width		= rate_text_width + field_width + line_padding;
		float liveup_box_width		= liveup_text_width + toggle_width + line_padding;
		float value_box_width		= scene_width
									  - node_box_width - border_padding
									  - attribute_box_width - border_padding
									  - rate_box_width - border_padding
									  - liveup_box_width - border_padding
									  - mode_box_width - border_padding
									  - border_total - border_total;
		
		float node_box_right		= border_total;
		float node_dropdown_right	= node_box_right + node_text_width;
		float attribute_box_right	= border_total + node_box_width + border_padding;
		float attribute_dropdown_right = attribute_box_right + attribute_text_width;
		float mode_box_right		= attribute_box_right + attribute_box_width + border_padding;
		float mode_dropdown_right	= mode_box_right + mode_text_width;
		float value_box_right		= mode_box_right + mode_box_width + border_padding;
		float rate_box_right		= value_box_right + value_box_width + border_padding;
		float rate_field_right		= rate_box_right;
		float liveup_box_right		= rate_box_right + rate_box_width + border_padding;
		float liveup_toggle_right	= liveup_box_right + line_padding;

		// Create background boxes texture.
		Texture2D box_texture		= new Texture2D( 1, 1 );
		box_texture.wrapMode		= TextureWrapMode.Repeat;
		box_texture.SetPixel( 0, 0, new Color( box_color.r - mySceneUIDarkeningFactor, 
											   box_color.g - mySceneUIDarkeningFactor, 
											   box_color.b - mySceneUIDarkeningFactor, 0.5f ) );
		box_texture.Apply();

		// Set up rectangles for the boxes.
		Rect node_box_rect				= new Rect( node_box_right, box_top, node_box_width, box_height );
		Rect node_dropdown_rect			= new Rect( node_dropdown_right, box_top, dropdown_width, box_height );
		Rect attribute_box_rect			= new Rect( attribute_box_right, box_top, attribute_box_width, box_height );
		Rect attribute_dropdown_rect	= new Rect( attribute_dropdown_right, box_top, dropdown_width, box_height );
		Rect mode_box_rect				= new Rect( mode_box_right, box_top, mode_box_width, box_height );
		Rect mode_dropdown_rect			= new Rect( mode_dropdown_right, box_top, dropdown_width, box_height );
		Rect value_box_rect				= new Rect( value_box_right, box_top, value_box_width, expanded_box_height );
		Rect rate_box_rect				= new Rect( rate_box_right, box_top, rate_box_width, box_height );
		Rect rate_field_rect			= new Rect( rate_field_right, box_top, rate_text_width + field_width, box_height );
		Rect liveup_box_rect			= new Rect( liveup_box_right, box_top, liveup_box_width, box_height );
		Rect liveup_toggle_rect			= new Rect( liveup_toggle_right, box_top, liveup_text_width + toggle_width, box_height );

		// Label boxes.
		Rect node_text_rect	 = new Rect( node_box_right + line_padding, box_top,
										 node_text_width - double_line_padding,
										 box_height - double_line_padding );
		Rect attribute_text_rect = new Rect( attribute_box_right + line_padding, box_top,
											 attribute_text_width - double_line_padding,
											 box_height - double_line_padding );
		Rect mode_text_rect  = new Rect( mode_box_right + line_padding, box_top, 
										 mode_text_width - double_line_padding,
										 box_height - double_line_padding );

		// Start Drawing --------------------------------------------------------------------------------------------
		Handles.BeginGUI();
		GUILayout.BeginArea( new Rect( 0, 0, Screen.width, Screen.height ) );

		// Draw the background boxes for the Scene UI.
		GUI.color = box_color;
		GUI.DrawTexture( node_box_rect, box_texture, ScaleMode.StretchToFill );
		GUI.DrawTexture( attribute_box_rect, box_texture, ScaleMode.StretchToFill );
		GUI.DrawTexture( mode_box_rect, box_texture, ScaleMode.StretchToFill );
		GUI.DrawTexture( value_box_rect, box_texture, ScaleMode.StretchToFill );
		GUI.DrawTexture( rate_box_rect, box_texture, ScaleMode.StretchToFill );
		GUI.DrawTexture( liveup_box_rect, box_texture, ScaleMode.StretchToFill );

		// Draw the labels for the mesh and the help.
		GUI.color = text_color;
		GUI.Label( node_text_rect, node_text, normal_text_style );
		GUI.Label( attribute_text_rect, attribute_text, normal_text_style );
		GUI.Label( mode_text_rect, mode_text, normal_text_style );

		// Set controls colours.
		Color control_color		= box_color;
		control_color.r			+= mySceneUIBrightningFactor;
		control_color.g			+= mySceneUIBrightningFactor;
		control_color.b			+= mySceneUIBrightningFactor;
		GUI.color				= control_color;

		// Draw rate field.
		EditorGUIUtility.labelWidth = rate_text_width;
		EditorGUIUtility.fieldWidth = field_width;
		GUI.SetNextControlName( myPaintValuesFieldName + "RATE" );
		HoudiniHost.prPaintBrushRate = EditorGUI.FloatField( rate_field_rect, rate_text, HoudiniHost.prPaintBrushRate );

		// Draw live updates toggle.
		GUI.SetNextControlName( myPaintValuesFieldName + "LIVE_UPDATES" );
		myManager.prLiveUpdates = EditorGUI.ToggleLeft(
			liveup_toggle_rect, liveup_text, myManager.prLiveUpdates );

		// Draw node dropdown.
		{
			int[] node_indicies = new int[ node_list.Length ];
			for ( int i = 0; i < node_list.Length; ++i )
				node_indicies[ i ] = i;

			GUI.SetNextControlName( myPaintValuesFieldName + "NODE" );
			selected_node_index = EditorGUI.IntPopup(
				node_dropdown_rect, "", selected_node_index, node_list, node_indicies );
		}

		// Draw attribute dropdown.
		{
			if ( myManager.prActiveAttribute == null )
			{
				GUI.Label( attribute_dropdown_rect, "No attributes.", normal_text_style );
			}
			else
			{
				int[] attribute_indicies = new int[ myManager.prAttributes.Count ];
				for ( int i = 0; i < myManager.prAttributes.Count; ++i )
					attribute_indicies[ i ] = i;

				string[] attribute_names = new string[ myManager.prAttributes.Count ];
				int selected_attribute_index = 0;
				for ( int i = 0; i < myManager.prAttributes.Count; ++i )
				{
					if ( myManager.prAttributes[ i ].prName == myManager.prActiveAttribute.prName )
						selected_attribute_index = i;
					attribute_names[ i ] = myManager.prAttributes[ i ].prName;
				}

				GUI.SetNextControlName( myPaintValuesFieldName + "ATTRIBUTE" );
				selected_attribute_index = EditorGUI.IntPopup(
					attribute_dropdown_rect, "", selected_attribute_index, attribute_names, attribute_indicies );

				string selected_attribute_name = attribute_names[ selected_attribute_index ];
				myManager.setActiveAttribute( selected_attribute_name );
			}
		}

		// Draw mode dropdown.
		{
			if ( myManager.prActiveAttribute == null )
			{
				GUI.Label( mode_dropdown_rect, "N/A", normal_text_style );
			}
			else
			{
				int mode_count = myManager.prActiveAttribute.prTupleSize + 1; // +1 for Colour Mode.
				int[] mode_values = new int[ mode_count ];
				for ( int i = 0; i < mode_count; ++i )
					mode_values[ i ] = i;

				string[] mode_labels = new string[ mode_count ];
				mode_labels[ 0 ] = "Color (First 3 Components)";
				for ( int i = 1; i < mode_count; ++i )
					mode_labels[ i ] = "Comp. " + i + " Only (Grayscale)";

				GUI.SetNextControlName( myPaintValuesFieldName + "DRAW_MODE" );
				int new_paint_mode = EditorGUI.IntPopup(
					mode_dropdown_rect, "", myManager.prActiveAttribute.prPaintMode, mode_labels, mode_values );
				if ( new_paint_mode != myManager.prActiveAttribute.prPaintMode )
				{
					myManager.prActiveAttribute.prPaintMode = new_paint_mode;
					myManager.refreshMesh();
				}
			}
		}

		// Draw paint value fields.
		{
			if ( myManager.prActiveAttribute == null )
			{
				GUI.Label( value_box_rect, value_text + "N/A", normal_text_style );
			}
			else
			{
				GUILayout.BeginArea( value_box_rect );
				GUILayout.BeginHorizontal();

				EditorGUILayout.LabelField( value_text, GUILayout.Width( value_text_width ) );

				int drawn_field_start_index = 0;

				if ( myManager.prActiveAttribute.prName.StartsWith( "Cd" ) &&
					myManager.prActiveAttribute.prType == HoudiniGeoAttribute.Type.FLOAT &&
					myManager.prActiveAttribute.prTupleSize >= 3 )
				{
					Color old_colour = new Color( 
						myManager.prActiveAttribute.prFloatPaintValue[ 0 ],
						myManager.prActiveAttribute.prFloatPaintValue[ 1 ],
						myManager.prActiveAttribute.prFloatPaintValue[ 2 ],
						myManager.prActiveAttribute.prTupleSize > 3
							? myManager.prActiveAttribute.prFloatPaintValue[ 3 ] : 1.0f );
					GUI.SetNextControlName( myPaintValuesFieldName + "COLOR" );
					Color new_colour = EditorGUILayout.ColorField(
						"", old_colour,
						GUILayout.MinWidth( 120 ), GUILayout.MaxWidth( 240 ) );
					if ( new_colour != old_colour )
					{
						myManager.prActiveAttribute.prFloatPaintValue[ 0 ] = new_colour[ 0 ];
						myManager.prActiveAttribute.prFloatPaintValue[ 1 ] = new_colour[ 1 ];
						myManager.prActiveAttribute.prFloatPaintValue[ 2 ] = new_colour[ 2 ];
						if ( myManager.prActiveAttribute.prTupleSize > 3 )
							myManager.prActiveAttribute.prFloatPaintValue[ 3 ] = new_colour[ 3 ];
					}

					drawn_field_start_index = Mathf.Min( 4, myManager.prActiveAttribute.prTupleSize );
				}

				for ( int i = drawn_field_start_index; i < myManager.prActiveAttribute.prTupleSize; ++i )
				{
					GUI.SetNextControlName( myPaintValuesFieldName + i );
					if ( myManager.prActiveAttribute.prType == HoudiniGeoAttribute.Type.BOOL
						|| myManager.prActiveAttribute.prType == HoudiniGeoAttribute.Type.INT )
					{
						myManager.prActiveAttribute.prIntPaintValue[ i ] = Mathf.Clamp(
							EditorGUILayout.IntField(
								"", myManager.prActiveAttribute.prIntPaintValue[ i ],
								GUILayout.MinWidth( 20 ), GUILayout.MaxWidth( 120 ) ),
							myManager.prActiveAttribute.prIntMin,
							myManager.prActiveAttribute.prIntMax );
					}
					else if ( myManager.prActiveAttribute.prType == HoudiniGeoAttribute.Type.FLOAT )
					{
						myManager.prActiveAttribute.prFloatPaintValue[ i ] = Mathf.Clamp(
							EditorGUILayout.FloatField(
								"", myManager.prActiveAttribute.prFloatPaintValue[ i ],
								GUILayout.MinWidth( 20 ), GUILayout.MaxWidth( 120 ) ),
							myManager.prActiveAttribute.prFloatMin,
							myManager.prActiveAttribute.prFloatMax );
					}
					else if ( myManager.prActiveAttribute.prType == HoudiniGeoAttribute.Type.STRING )
					{
						string new_value = EditorGUILayout.TextField(
							"", myManager.prActiveAttribute.prStringPaintValue[ i ],
							GUILayout.MinWidth( 20 ), GUILayout.MaxWidth( 120 ) );
						if ( new_value != myManager.prActiveAttribute.prStringPaintValue[ i ] )
						{
							myManager.prActiveAttribute.prStringPaintValue[ i ] = new_value;
							myManager.refreshMesh();
						}
					}
				}

				GUI.SetNextControlName( myPaintValuesFieldName + "FILL" );
				if ( GUILayout.Button( "Fill", GUILayout.Width( 35 ), GUILayout.Height( 14 ) ) )
				{
					myManager.prActiveAttribute.fill();
					myManager.refreshMesh();
					myManager.prHasChanged = true;
				}

				{ // Show min/max button.
					string min_max_button_label = "+";
					if ( myShowMinMax )
						min_max_button_label = "-";
					GUIStyle label_style = new GUIStyle( EditorStyles.toolbar );
					label_style.fontStyle = FontStyle.Bold;
					label_style.fontSize = 14;
					label_style.fixedHeight = line_height;
					if ( GUILayout.Button(
							min_max_button_label, label_style,
							GUILayout.Width( 22 ),
							GUILayout.Height( line_height ) ) )
						myShowMinMax = !myShowMinMax;
				}

				GUILayout.EndHorizontal();

				if ( myShowMinMax )
				{
					float old_label_width = EditorGUIUtility.labelWidth;
					EditorGUIUtility.labelWidth = 30;
					GUILayout.BeginHorizontal();
					if ( myManager.prActiveAttribute.prType == HoudiniGeoAttribute.Type.INT )
					{
						GUI.SetNextControlName( myPaintValuesFieldName + "MIN" );
						myManager.prActiveAttribute.prIntMin =
							EditorGUILayout.IntField(
								"Min:", myManager.prActiveAttribute.prIntMin,
								GUILayout.MinWidth( 20 ), GUILayout.MaxWidth( 120 ) );

						GUI.SetNextControlName( myPaintValuesFieldName + "MAX" );
						myManager.prActiveAttribute.prIntMax =
							EditorGUILayout.IntField(
								"Max:", myManager.prActiveAttribute.prIntMax,
								GUILayout.MinWidth( 20 ), GUILayout.MaxWidth( 120 ) );
					}
					else if ( myManager.prActiveAttribute.prType == HoudiniGeoAttribute.Type.FLOAT )
					{
						GUI.SetNextControlName( myPaintValuesFieldName + "MIN" );
						myManager.prActiveAttribute.prFloatMin =
							EditorGUILayout.FloatField(
								"Min:", myManager.prActiveAttribute.prFloatMin,
								GUILayout.MinWidth( 20 ), GUILayout.MaxWidth( 120 ) );

						GUI.SetNextControlName( myPaintValuesFieldName + "MAX" );
						myManager.prActiveAttribute.prFloatMax =
							EditorGUILayout.FloatField(
								"Max:", myManager.prActiveAttribute.prFloatMax,
								GUILayout.MinWidth( 20 ), GUILayout.MaxWidth( 120 ) );
					}
					GUILayout.EndHorizontal();
					EditorGUIUtility.labelWidth = old_label_width;
				}

				GUILayout.EndArea();
			}
		}

		GUILayout.EndArea();
		Handles.EndGUI();
		// Stop Drawing ---------------------------------------------------------------------------------------------
	
		// Restore GUI colour.
		GUI.color = original_color;

		return selected_node_index;
	}

	private int drawSceneUI( string tool_name, int selected_node_index, string[] node_list )
	{
		drawToolSceneUI( tool_name );
		if ( myManager.prEditable && myManager.prIsPaintingPoints )
			selected_node_index = drawPaintingSceneUI( selected_node_index, node_list );
		return selected_node_index;
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

	private HoudiniGeoAttributeManager myManager;

	private bool				myForceInspectorRedraw;
	private bool				myShowMinMax;

	private const float			myActiveBorderWidth = 5.0f;
	private const float			myInactiveBorderWidth = 2.0f;
	private Camera				myTempCamera;

	private bool				myIsMouseDown;
	private int					myMouseKey;
	private KeyCode				myCurrentlyPressedKey;
	private KeyCode				myCurrentlyPressedSecondKey;
	private KeyCode				myCurrentlyPressedModeKey;

	private const KeyCode		myMayaBrushResizeKey				= KeyCode.B;
	private const KeyCode		myHoudiniBrushResizeKey				= KeyCode.LeftShift;
	private const KeyCode		myPhotoshopBrushGrowKey				= KeyCode.RightBracket;
	private const KeyCode		myPhotoshopBrushShrinkKey			= KeyCode.LeftBracket;

	private const string		myPaintValuesFieldName				= "__HAPI_PaintBrushPaintValue";

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
	private const float			myMouseWheelBrushSizeMultiplier		= 0.05f;
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

	private HoudiniGeoAttributeManager.Mode myLastMode;
}

#if !(HAPI_ENABLE_RUNTIME)
	#pragma warning restore 0162 // Unreachable Code
	#pragma warning restore 0414 // Initialized but unused Private Member Variable
#endif // !( HAPI_ENABLE_RUNTIME )

