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
 * 		GUI companion to HAPI_Curve.
 * 
 */

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 	GUI companion to <see cref="HAPI_Curve"/>.
/// </summary>
[ CustomEditor( typeof( HAPI_Curve ) ) ]
public class HAPI_CurveGUI : Editor 
{	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	public void OnEnable() 
	{		
		myCurve = target as HAPI_Curve;
		
		myUndoManager = new HOEditorUndoManager( myCurve, "HAPI_Curve" );
		
		myCurrentlyActivePoint = -1;
	}
	
	public void OnSceneGUI() 
	{
		Event current_event 		= Event.current;
		
		int point_count 			= myCurve.getPointCount();
		int pressed_point_index 	= -1;
		Vector3 previous_position 	= Vector3.zero;
				
		for ( int i = 0; i < point_count; ++i ) 
		{
			Vector3 position 	= myCurve.getPointPosition( i );
			float handle_size 	= HandleUtility.GetHandleSize( position ) * 0.2f;
			
			if ( i > 0 ) 
			{
				Handles.color = Color.grey;
				Handles.DrawLine( previous_position, position );
				
				// Handles.DrawBezier actually just considers 2 axis (so it's 2D) for some
				// reason. So it's useless.
				/*
				Handles.DrawBezier( previous_position, 
					position, 
					( previous_position - position ).normalized,
					( position - previous_position ).normalized,
					Color.white,
					null,
					handleSize * 0.1f );
				*/
				
				Vector3 p0 				= previous_position;
				Vector3 p3 				= position;
				Vector3 start_tangent 	= Vector3.up;
				Vector3 end_tangent 	= Vector3.down;
				
				Vector3 p1 				= p0 + start_tangent;
				Vector3 p2 				= p3 + end_tangent;
				
				Vector3 previous_pixel 	= Vector3.zero;
				
				Handles.color = Color.cyan;
				for ( int s = 0; s <= myCurve.mySegmentCount; ++s ) 
				{
					float t = s / (float) myCurve.mySegmentCount;
					Vector3 pixel = calculateBezierPoint( t, p0, p1, p2, p3 );
					
					if ( s > 0 )
						Handles.DrawLine( previous_pixel, pixel );
					
					previous_pixel = pixel;
				}
			}
			
			Handles.color 		= Color.cyan;
			bool buttonPress 	= Handles.Button( 	position, 
													Quaternion.LookRotation( Camera.current.transform.position ),
													handle_size,
									               	handle_size,
													Handles.CircleCap );
			
			if ( buttonPress )
				pressed_point_index = i;
			
			Handles.Label( position, new GUIContent( "p" + i ) );
			
			previous_position = position;
		}
		
		if ( pressed_point_index >= 0 )
			myCurrentlyActivePoint = pressed_point_index;
		
		if ( myCurrentlyActivePoint >= 0 ) 
		{
			Vector3 new_position = Handles.PositionHandle( myCurve.getPointPosition( myCurrentlyActivePoint ), 
														   Quaternion.identity );
			myCurve.setPointPosition( myCurrentlyActivePoint, new_position );
		}
		
		if ( current_event.isKey && current_event.keyCode == KeyCode.Escape )
			myCurrentlyActivePoint = -1;
	}
	
	public override void OnInspectorGUI() {
		myUndoManager.CheckUndo();
		
		if ( GUILayout.Button( "Add Point" ) )
			myCurve.addPoint( Vector3.zero );
		
		EditorGUILayout.LabelField( new GUIContent( "Segments:" ) );
		EditorGUILayout.BeginHorizontal(); 
		{
			int new_segment_count_from_field = EditorGUILayout.IntField( myCurve.mySegmentCount, 
																		 GUILayout.Width( 40 ) );
			int new_segment_count_from_slider = (int) GUILayout.HorizontalSlider( (float) myCurve.mySegmentCount, 
																			   	  1.0f, 60.0f );
			
			if ( new_segment_count_from_field != myCurve.mySegmentCount ) {
				myCurve.mySegmentCount = new_segment_count_from_field;
			} else if ( new_segment_count_from_slider != myCurve.mySegmentCount ) {
				myCurve.mySegmentCount = new_segment_count_from_slider;
			}
		} 
		EditorGUILayout.EndHorizontal();
					
		if ( GUI.changed )
			HandleUtility.Repaint();
		
		myUndoManager.CheckDirty();	
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private
	
	private Vector3 calculateBezierPoint( float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3 ) 
	{
		float u 	= 1 - t;
		float tt 	= t * t;
		float uu 	= u * u;
		float uuu 	= uu * u;
		float ttt 	= tt * t;
		
		Vector3 p 	= uuu * p0; // first term
		p 		   += 3 * uu * t * p1; // second term
		p 		   += 3 * u * tt * p2; // third term
		p 		   += ttt * p3; // fourth term
		
		return p;
	}
		
	private HOEditorUndoManager 	myUndoManager;
	private HAPI_Curve 				myCurve;
	
	private int 					myCurrentlyActivePoint;
	
}
