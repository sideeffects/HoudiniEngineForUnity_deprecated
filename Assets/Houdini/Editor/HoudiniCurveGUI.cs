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

[ ExecuteInEditMode ]
[ CustomEditor( typeof( HoudiniCurve ) ) ]
public class HoudiniCurveGUI : Editor 
{
#if !( HAPI_ENABLE_RUNTIME )
	#pragma warning disable 0414
#endif // !( HAPI_ENABLE_RUNTIME )

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	public void OnEnable() 
	{
		myCurve					= target as HoudiniCurve;

		myForceInspectorRedraw	= false;
		myTarget				= null;

		myIsMouseDown			= false;
		myCurrentlyPressedKey	= KeyCode.None;
		myFirstMousePosition	= new Vector3();

		myGuideLinesMaterial	= null;
		myGuideLinesTexture		= null;
		myGuideLinesMesh		= null;

		mySelectionArea			= new Rect();
		mySelectionMeshColours	= null;
		mySelectionMaterial		= null;
		mySelectedPoints		= new List< int >();
		mySelectedPointsMask	= new List< bool >();

		myConnectionMesh		= null;
		myConnectionMaterial	= null;

		myLastMode				= HoudiniCurve.Mode.NONE;

		HoudiniHost.myRepaintDelegate += this.refresh;
		HoudiniHost.myDeselectionDelegate += this.deselect;
		HoudiniHost.mySelectionTarget = myCurve.gameObject;
	}

	public void OnDisable()
	{
		// This is called after OnSceneGUI sometimes for some reason.
		HoudiniHost.myRepaintDelegate -= this.refresh;
		HoudiniHost.myDeselectionDelegate -= this.deselect;
		HoudiniHost.mySelectionTarget = null;
		myIsTransformHandleHidden = false;
	}

	public void refresh()
	{
		myCurve.buildDummyMesh();
		buildGuideGeometry();
	}

	public void deselect()
	{
		myIsTransformHandleHidden = false;
	}

	public override void OnInspectorGUI()
	{
		if ( myCurve == null )
			return;

		bool is_editable = myCurve.prEditable && HoudiniHost.isInstallationOk();

		// We can only build or do anything if we can link to our libraries.
#if !( HAPI_ENABLE_RUNTIME )
		is_editable = false;
		HoudiniGUI.help( HoudiniConstants.HAPI_UNSUPPORTED_PLATFORM_MSG, MessageType.Info );
#else
		if ( !myCurve.prEditable )
			HoudiniGUI.help( "This curve is not editable.", MessageType.Info );
		else if ( !HoudiniHost.isInstallationOk() )
			HoudiniGUI.help( HoudiniHost.getMissingEngineInstallHelpString(), MessageType.Info );
#endif // !( HAPI_ENABLE_RUNTIME )

		bool gui_enable = GUI.enabled;
		GUI.enabled = is_editable;

		Object target = (Object) myTarget;
		GameObject temp_obj = null;
		if ( HoudiniGUI.objectField( "target", "Target", ref target, typeof( GameObject ), null, ref temp_obj ) )
			myTarget = (GameObject) target;

		GUI.enabled = gui_enable;
	}
	
	public void OnSceneGUI() 
	{
		// We can only build or do anything if we can link to our libraries.
#if !( HAPI_ENABLE_RUNTIME )
		return;
		#pragma warning disable 0162
#endif // !( HAPI_ENABLE_RUNTIME )

		if ( myCurve == null )
			return;

		// Enable point size on OpenGL.
		HoudiniHost.preDrawSetup();

		// First Remake and Draw Guide Geometry if necessary.
		if ( mySelectionMeshColours == null )
			buildGuideGeometry();
		
		if ( !myTempCamera && Camera.current )
			myTempCamera = Camera.current;

		Event current_event 		= Event.current;
		Vector3 mouse_position		= getMousePosition( ref current_event );

		// Set appropriate handles matrix.
		// TODO: Fix.
		/*
		Vector3 handle_pos = HAPI_AssetUtility.getPosition( myCurve.transform.localToWorldMatrix );
		Quaternion handle_rot = HAPI_AssetUtility.getQuaternion( myCurve.transform.localToWorldMatrix );
		Matrix4x4 handle_matrix = Matrix4x4.identity;
		handle_matrix.SetTRS( handle_pos, handle_rot, new Vector3( 1.0f, 1.0f, 1.0f ) );
		//Debug.Log( handle_pos );
		//Debug.Log( handle_rot );
		//Debug.Log( handle_matrix );
		Handles.matrix = handle_matrix;
		 */
		Handles.matrix = myCurve.transform.localToWorldMatrix;

		// Determine key state.
		getKeyState( current_event );

		// Decide modes.
		if ( current_event.type == EventType.Layout && mySceneWindowHasFocus )
			decideModes( ref current_event );

		// Draw scene UI.
		drawSceneUI();

		// Add points.
		if ( !current_event.alt && !( current_event.type == EventType.MouseDown && current_event.button == 1 ) )
		{
			if ( myCurve.prIsAddingPoints )
			{
				Vector3 position	= Vector3.zero;
				float handle_size 	= HandleUtility.GetHandleSize( position ) * myBigButtonHandleSizeMultiplier;
				Quaternion rotation = HoudiniAssetUtility.getQuaternion( myTempCamera.transform.localToWorldMatrix );
#if UNITY_5_5_OR_NEWER
				bool button_press = Handles.Button(position, rotation, handle_size, handle_size, Handles.RectangleHandleCap );
#else
				bool button_press = Handles.Button(position, rotation, handle_size, handle_size, Handles.RectangleCap );
#endif
				Ray ray = myTempCamera.ScreenPointToRay( mouse_position );
#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6
				if ( !myTempCamera.isOrthoGraphic )
#else
				if ( !myTempCamera.orthographic )
#endif // UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6
					ray.origin = myTempCamera.transform.position;

				Vector3 intersection = new Vector3();

				if ( myTarget != null && myTarget.GetComponent< Collider >() )
				{
					Collider collider = myTarget.GetComponent< Collider >();
					RaycastHit hit_info;
					collider.Raycast( ray, out hit_info, myIntersectionRayLength );
					intersection = hit_info.point;
				}
				else
				{
					Plane plane = new Plane();
					plane.SetNormalAndPosition( Vector3.up, Vector3.zero );
					float enter = 0.0f;
					plane.Raycast( ray, out enter );
					enter = Mathf.Clamp( enter, myTempCamera.nearClipPlane, myTempCamera.farClipPlane );
 					intersection = ray.origin + ray.direction * enter;
				}

				bool is_mid_point			= false;
				int insert_index			= -1;
				Vector3 new_point_location	= intersection;
				
				// Draw guide line.
				if ( myCurve.prPoints.Count > 0 )
				{
					Vector3 anchor1 = myCurve.transform.TransformPoint(
						myCurve.prPoints[ myCurve.prPoints.Count - 1 ] );
					Vector3 anchor2 = Vector3.zero;
					insert_index = myCurve.prPoints.Count;

					// See if we're close to another segment.
					for ( int i = 1; i < myCurve.prPoints.Count; ++i )
					{
						Vector3 p0 = myCurve.transform.TransformPoint( myCurve.prPoints[ i - 1 ] );
						Vector3 p1 = myCurve.transform.TransformPoint( myCurve.prPoints[ i ] );

						Vector3 closest_point = new Vector3();
						float distance = HoudiniGUIUtility.closestDistanceBetweenLineAndLineSegment(
							p0, p1,
							ray, out closest_point );
						
						if ( distance < 
								HandleUtility.GetHandleSize( closest_point ) / 
								HoudiniHost.prGuideMinDistanceForMidPointInsertion )
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

					line_vertices[ 0 ]	= HoudiniGUIUtility.getCameraNearPlanePoint( anchor1, myTempCamera );
					line_vertices[ 1 ]	= HoudiniGUIUtility.getCameraNearPlanePoint( new_point_location, myTempCamera );
					float length		= Vector3.Distance( line_vertices[ 0 ], line_vertices[ 1 ] ) * 
										  myGuideLinesDashTilingMultiplier;
					line_indices[ 0 ]	= 0; 
					line_indices[ 1 ]	= 1;
					uvs[ 0 ]			= new Vector2(); 
					uvs[ 1 ]			= new Vector2( length, length );

					if ( is_mid_point )
					{
						line_vertices[ 2 ]	= HoudiniGUIUtility.getCameraNearPlanePoint( anchor2, myTempCamera );
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
					myGuideLinesMaterial.SetColor( "_Color", HoudiniHost.prGuideWireframeColour );
					myGuideLinesMaterial.SetTextureScale( "_MainTex", new Vector2( 1.0f, 1.0f ) );
					Graphics.DrawMeshNow( myGuideLinesMesh, Matrix4x4.identity );
				}

				// Add points on click.
				if ( button_press )
				{
					// Once we add a point we are no longer bound to the user holding down the add points key.
					// Add points mode is now fully activated.
					myCurve.prModeChangeWait = false;

					// Need to inverse transform the new point because it is in world space
					// and we want it to stay in the same location as it is in world space
					// when it is parented to the curve's transform.
					new_point_location = myCurve.transform.InverseTransformPoint( new_point_location );

					if ( is_mid_point )
						myCurve.insertPoint( insert_index, new_point_location );
					else
						myCurve.addPoint( new_point_location );

					// Remake and Draw Guide Geometry
					buildGuideGeometry();
				}

				// Delete last point on backspace.
				if ( current_event.isKey && current_event.type == EventType.KeyUp &&
					 ( current_event.keyCode == KeyCode.Delete || current_event.keyCode == KeyCode.Backspace ) )
				{
					// Once we add a point we are no longer bound to the user holding down the add points key.
					// Add points mode is now fully activated.
					myCurve.prModeChangeWait = false;

					myCurve.deleteLastPoint();

					// Remake and Draw Guide Geometry
					buildGuideGeometry();
				}
				if ( current_event.isKey && current_event.keyCode == KeyCode.Delete 
					 || current_event.keyCode == KeyCode.Backspace )
				{
					Event.current.Use();
				}

			} // Add mode.
			else if ( myCurve.prIsEditingPoints )
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
					Vector3[] points = myCurve.prPoints.ToArray();
					for ( int i = 0; points != null && i < points.Length; ++i )
					{
						Vector3 transformed_point = myCurve.transform.TransformPoint( points[ i ] );
						Vector3 proj_pos = myTempCamera.WorldToScreenPoint( transformed_point );
						proj_pos.z = 0.0f;

						if ( Mathf.Abs( mouse_delta.x ) > 1.5f || Mathf.Abs( mouse_delta.y ) > 1.5f )
						{
							if ( proj_pos.x >= min_bounds.x && proj_pos.x <= max_bounds.x &&
								 proj_pos.y >= min_bounds.y && proj_pos.y <= max_bounds.y )
							{
								// Once we modify a point we are no longer bound to the user holding down 
								// the point edit key. Edit point mode is now fully activated.
								myCurve.prModeChangeWait = false;
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
								myCurve.prModeChangeWait = false;
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
		if ( myCurve.prIsEditingPoints && myIsMouseDown )
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
		myIsTransformHandleHidden = myCurve.prIsEditingPoints;

		// Update active control point.
		if ( mySelectedPoints.Count > 0 ) 
		{
			if ( myCurrentlyPressedKey == KeyCode.Delete )
			{ // Handle deletions.
				myCurve.deletePoints( mySelectedPoints.ToArray() );
				clearSelection();
				Event.current.Use();
			}
			else
			{ // Create midpoint for the handle.
				Vector3 max_bounds = myCurve.prPoints[ mySelectedPoints[ 0 ] ];
				Vector3 min_bounds = myCurve.prPoints[ mySelectedPoints[ 0 ] ];
				for ( int i = 1; i < mySelectedPoints.Count; ++i )
				{
					Vector3 current_pos = myCurve.prPoints[ mySelectedPoints[ i ] ];
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
						Vector3 old_pos = myCurve.prPoints[ point_index ];
						Vector3 new_pos = old_pos + delta;
						myCurve.updatePoint( point_index, new_pos );
					}

					// Remake and Draw Guide Geometry
					myCurve.updatePoints();
					buildGuideGeometry();
				}
			} // Delete?
		}

		// Connection Mesh Draws
		if ( myConnectionMaterial != null && myConnectionMesh != null )
		{
			myConnectionMaterial.SetPass( 0 );
			Graphics.DrawMeshNow( myConnectionMesh, myCurve.transform.localToWorldMatrix );
		}

		// Selection Mesh Draws
		if ( mySelectionMaterial != null )
		{
			mySelectionMaterial.SetFloat( "_PointSize", HoudiniHost.prGuidePointSize );
			mySelectionMaterial.SetColor( "_Color", HoudiniHost.prGuideWireframeColour );
			if ( mySelectionMaterial.SetPass( 0 ) )
			{
				// TODO: Clean this up!
				Camera tempCamera = Camera.current;
				float s = HoudiniHost.prGuidePointSize / 2.0f;
				float w = tempCamera.pixelWidth;
				float h = tempCamera.pixelHeight;

				GL.PushMatrix();
				GL.Begin( GL.QUADS );
				GL.LoadOrtho();

				for ( int i = 0; i < myCurve.prPoints.Count; ++i )
				{
					Vector3 p = myCurve.prPoints[ i ];
					p = myCurve.transform.TransformPoint( p );

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

				for ( int i = 0; i < myCurve.prPoints.Count; ++i )
				{
					Vector3 p = myCurve.prPoints[ i ];
					p = myCurve.transform.TransformPoint( p );

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

#if !( HAPI_ENABLE_RUNTIME )
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
				if ( myCurve.prIsEditingPoints )
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
		// Build Guide Lines Mesh -----------------------------------------------------------------------------------

		if ( myGuideLinesMaterial == null || myGuideLinesTexture == null || myGuideLinesMaterial == null )
		{
			myGuideLinesMaterial					= new Material( Shader.Find( "Houdini/DottedLine" ) );
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
			mySelectionMaterial						= new Material( Shader.Find( "Houdini/CurvePoint" ) );
			mySelectionMaterial.hideFlags			= HideFlags.HideAndDontSave;
			mySelectionMaterial.shader.hideFlags	= HideFlags.HideAndDontSave;
		}

		// Check if we need to resize the selection mask.
		while ( mySelectedPointsMask.Count < myCurve.prPoints.Count )
			mySelectedPointsMask.Add( false );

		// Pretend we have two points if only one actually exists. Since DrawMesh won't draw
		// anything unless it has at least a line we need a dummy line to exist.
		// In this case we just create two points, both at the same position as 
		// the one real point.
		Vector3[] selection_vertices = null;
		if ( myCurve.prPoints.Count == 1 )
		{
			selection_vertices = new Vector3[ 2 ];
			selection_vertices[ 0 ] = myCurve.prPoints[ 0 ];
			selection_vertices[ 1 ] = myCurve.prPoints[ 0 ];
		}
		else
			selection_vertices = myCurve.prPoints.ToArray();

		// Create the selection indices.
		int[] selection_indices = new int[ selection_vertices.Length ];
		for ( int i = 0; i < selection_vertices.Length; ++i )
			selection_indices[ i ] = i;

		if ( mySelectionMeshColours == null || mySelectionMeshColours.Length != selection_vertices.Length )
		{
			mySelectionMeshColours = new Color[ selection_vertices.Length ];
			for ( int i = 0; i < selection_vertices.Length; ++i )
			{
				if ( !myCurve.prIsEditingPoints )
					mySelectionMeshColours[ i ] = HoudiniHost.prUnselectableGuideWireframeColour;
				else
					mySelectionMeshColours[ i ] = HoudiniHost.prUnselectedGuideWireframeColour;
			}
		}

		// Build Connection Mesh ------------------------------------------------------------------------------------

		if ( myConnectionMaterial == null )
		{
			myConnectionMaterial					= new Material( Shader.Find( "Houdini/Line" ) );
			myConnectionMaterial.hideFlags			= HideFlags.HideAndDontSave;
			myConnectionMaterial.shader.hideFlags	= HideFlags.HideAndDontSave;
		}

		myConnectionMesh							= new Mesh();
		myConnectionMesh.hideFlags					= HideFlags.HideAndDontSave;

		Vector3[] connection_vertices = myCurve.prPoints.ToArray();
		int[] connection_indices = new int[ connection_vertices.Length ];
		for ( int i = 0; i < connection_vertices.Length; ++i )
			connection_indices[ i ] = i;

		Color[] connection_colours = new Color[ connection_vertices.Length ];
		for ( int i = 0; i < connection_vertices.Length; ++i )
			connection_colours[ i ] = HoudiniHost.prGuideWireframeColour;

		myConnectionMesh.vertices = connection_vertices;
		myConnectionMesh.colors = connection_colours;
		myConnectionMesh.SetIndices( connection_indices, MeshTopology.LineStrip, 0 );
	}

	private void changeModes( ref bool add_points_mode, ref bool edit_points_mode, HoudiniCurve.Mode mode )
	{
		switch ( mode )
		{
			case HoudiniCurve.Mode.NONE: 
				{
					add_points_mode = false;
					edit_points_mode = false;
					break;
				}
			case HoudiniCurve.Mode.ADD:
				{
					add_points_mode = true;
					edit_points_mode = false;
					break;
				}
			case HoudiniCurve.Mode.EDIT:
				{
					add_points_mode = false;
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
		if ( !myCurve.prEditable )
		{
			myLastMode					= HoudiniCurve.Mode.NONE;
			myCurve.prCurrentMode		= HoudiniCurve.Mode.NONE;
			myCurve.prIsAddingPoints	= false;
			myCurve.prIsEditingPoints	= false;
			myCurve.prModeChangeWait	= false;
			return;
		}

		bool add_points_mode_key	= areKeysTheSame( myCurrentlyPressedKey, HoudiniHost.prAddingPointsModeHotKey );
		bool edit_points_mode_key	= areKeysTheSame( myCurrentlyPressedKey, HoudiniHost.prEditingPointsModeHotKey );

		bool add_points_mode		= myCurve.prIsAddingPoints;
		bool edit_points_mode		= myCurve.prIsEditingPoints;
		bool mode_change_wait		= myCurve.prModeChangeWait;

		if ( add_points_mode )
		{
			if ( !mode_change_wait && edit_points_mode_key )
			{
				myLastMode			= HoudiniCurve.Mode.ADD;

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
				myLastMode			= HoudiniCurve.Mode.EDIT;

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
				myLastMode			= HoudiniCurve.Mode.NONE;
			}
			else if ( edit_points_mode_key )
			{
				edit_points_mode	= true;
				mode_change_wait	= true;
				myLastMode			= HoudiniCurve.Mode.NONE;
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
		if ( edit_points_mode != myCurve.prIsEditingPoints )
		{
			// Must assign this earlier than normal because clearSelection() will
			// use the value to determine the colour of the control points.
			// (between unselected and unselectable)
			myCurve.prIsEditingPoints	= edit_points_mode;
			clearSelection();
		}

		myCurve.prIsAddingPoints	= add_points_mode;
		myCurve.prIsEditingPoints	= edit_points_mode;
		myCurve.prModeChangeWait	= mode_change_wait;
	}

	private void drawSceneUI()
	{
		string title_text = HoudiniConstants.HAPI_PRODUCT_SHORT_NAME + " Curve";
		string add_hotkey_string = HoudiniHost.prAddingPointsModeHotKey.ToString();
		string edit_hotkey_string = HoudiniHost.prEditingPointsModeHotKey.ToString();
		string help_text = "" + add_hotkey_string + ": add points | " + 
						   edit_hotkey_string + ": edit points";

		int skin = EditorPrefs.GetInt( "UserSkin" );
		Color box_color = ( skin == 0 ? mySceneUILightColour : mySceneUIDarkColour );
		Color text_color = Color.white;

		if ( !myCurve.prEditable )
		{
			help_text = "This curve is not editable.";
		}
		if ( myCurve.prIsAddingPoints )
		{
			help_text = "Click in space: add next point | Click a line segment: add midpoint | Backspace: delete last point | ESC or Enter: exit mode";
			box_color = HoudiniHost.prAddingPointsModeColour;
		}
		else if ( myCurve.prIsEditingPoints )
		{
			help_text = "Click or drag: select points | Delete: delete selected | Hold Control: toggle-based selection | ESC or Enter: exit mode";
			box_color = HoudiniHost.prEditingPointsModeColour;
		}

		if ( !mySceneWindowHasFocus && myCurve.prEditable )
			help_text = "Scene window doesn't have focus. Hotkeys may not work. Right click anywhere in the scene to focus.";

		// We need to take PixelsPerPoint into consideration for Retina Displays
		float fPixelsPerPoints = HoudiniGUIUtility.getPixelsPerPoint();

		Color original_color		= GUI.color;
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

		// The mode box should be nothing if the curve is static since there are no options for static curves.
		float mode_box_width		= myCurve.prEditable ? mySceneUIModeIndicatorWidth : 0.0f;
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
		float screenWidth = Screen.width / fPixelsPerPoints;
		float screenHeight = Screen.height / fPixelsPerPoints;
		GUILayout.BeginArea( new Rect( 0, 0, screenWidth, screenHeight ) );

		// Draw the background boxes for the Scene UI.
		GUI.color = box_color;
		GUI.DrawTexture( title_box_rect, box_texture, ScaleMode.StretchToFill );
		if ( myCurve.prEditable )
			GUI.DrawTexture( mode_box_rect, box_texture, ScaleMode.StretchToFill );
		GUI.DrawTexture( help_box_rect, box_texture, ScaleMode.StretchToFill );

		// Draw the labels for the curve and the help.
		GUI.color = text_color;
		GUI.Label( title_text_rect, title_text, bold_text_style );
 		GUI.Label( help_text_rect, help_text, normal_text_style );

		if ( myCurve.prEditable )
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
			modes[ 1 ]				= new GUIContent( "Add" );
			modes[ 2 ]				= new GUIContent( "Edit" );

			// Draw the mode selection toolbar.
			// Note: We want to disable the toolbar if a mode key is being held down because
			// if a button is pressed the current mode will imidiatly switch back to the mode
			// whos key is being held down...
			GUI.enabled =
				!mySceneWindowHasFocus ||
				( ( myCurrentlyPressedKey != HoudiniHost.prAddingPointsModeHotKey ) &&
				  ( myCurrentlyPressedKey != HoudiniHost.prEditingPointsModeHotKey ) );
			HoudiniCurve.Mode last_mode = myCurve.prCurrentMode;
			myCurve.prCurrentMode = (HoudiniCurve.Mode) GUI.Toolbar( mode_text_rect, (int) last_mode, modes );
			if ( last_mode != myCurve.prCurrentMode )
				clearSelection();
			GUI.enabled = true;

			// Draw selection rectangle.
			if ( myCurve.prIsEditingPoints )
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

			float width					= scene_width;
			float height				= scene_height;

			if ( myCurve.prCurrentMode == HoudiniCurve.Mode.NONE )
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

	private HoudiniCurve			myCurve;

	private bool				myForceInspectorRedraw;

	private const float			myActiveBorderWidth = 5.0f;
	private const float			myInactiveBorderWidth = 2.0f;
	private Camera				myTempCamera;

	[SerializeField] 
	private GameObject			myTarget;

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
	private const float			myBigButtonHandleSizeMultiplier		= 1000000.0f;
	private const float			myIntersectionRayLength				= 5000.0f;

	private const float			myGuideBorderSize					= 4.0f;
	private const float			myGuideLinesDashTilingMultiplier	= 4.0f;
	private Material			myGuideLinesMaterial;
	private Texture2D			myGuideLinesTexture;
	private Mesh				myGuideLinesMesh;

	private Rect				mySelectionArea;
	private Color[]				mySelectionMeshColours;
	private Material			mySelectionMaterial;

	[SerializeField] 
	private List< int >			mySelectedPoints;
	[SerializeField] 
	private List< bool >		mySelectedPointsMask;

	private Mesh				myConnectionMesh;
	private Material			myConnectionMaterial;

	private HoudiniCurve.Mode myLastMode;

#if !( HAPI_ENABLE_RUNTIME )
	#pragma warning restore 0414
#endif // !( HAPI_ENABLE_RUNTIME )
}
