using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[ CustomEditor( typeof( HAPI_Curve ) ) ]
public class HAPI_CurveGUI : Editor {
	
	public void OnEnable() {		
		myCurve = target as HAPI_Curve;
		
		myUndoManager = new HOEditorUndoManager( myCurve, "HAPI_Curve" );
		
		myCurrentlyActivePoint = -1;
	}
	
	public void OnSceneGUI() {
		Event currentEvent = Event.current;
		
		int pointCount = myCurve.GetPointCount();
		int pressedPointIndex = -1;
		Vector3 previousPosition = Vector3.zero;
				
		for ( int i = 0; i < pointCount; ++i ) {
			Vector3 position = myCurve.GetPointPosition( i );
			float handleSize = HandleUtility.GetHandleSize( position ) * 0.2f;
			
			if ( i > 0 ) {
				Handles.color = Color.grey;
				Handles.DrawLine( previousPosition, position );
				
				/*
				Handles.DrawBezier( previousPosition, 
					position, 
					( previousPosition - position ).normalized,
					( position - previousPosition ).normalized,
					Color.white,
					null,
					handleSize * 0.1f );
				*/
				
				Vector3 p0 = previousPosition;
				Vector3 p3 = position;
				Vector3 startTangent = Vector3.up;
				Vector3 endTangent = Vector3.down;
				
				Vector3 p1 = p0 + startTangent;
				Vector3 p2 = p3 + endTangent;
				
				Vector3 previousPixel = Vector3.zero;
				
				Handles.color = Color.cyan;
				for ( int s = 0; s <= myCurve.mySegmentCount; ++s ) {
					float t = s / (float) myCurve.mySegmentCount;
					Vector3 pixel = CalculateBezierPoint(t, p0, p1, p2, p3);
					
					if ( s > 0 ) {
						Handles.DrawLine( previousPixel, pixel );
					}
					
					previousPixel = pixel;
				}
			}
			
			Handles.color = Color.cyan;
			bool buttonPress = Handles.Button( position, 
				Quaternion.LookRotation( Camera.current.transform.position ),
				handleSize,
               	handleSize,
				Handles.CircleCap );
			
			if ( buttonPress ) {
				pressedPointIndex = i;
			}
			
			Handles.Label( position, new GUIContent( "p" + i ) );
			
			previousPosition = position;
		}
		
		if ( pressedPointIndex >= 0 ) {
			myCurrentlyActivePoint = pressedPointIndex;
		}
		
		if ( myCurrentlyActivePoint >= 0 ) {
			Vector3 newPosition =
				Handles.PositionHandle( myCurve.GetPointPosition( myCurrentlyActivePoint ), 
						Quaternion.identity );
			myCurve.SetPointPosition( myCurrentlyActivePoint, newPosition );
		}
		
		if ( currentEvent.isKey && currentEvent.keyCode == KeyCode.Escape ) {
			myCurrentlyActivePoint = -1;
		}
	}
	
	public override void OnInspectorGUI() {
		myUndoManager.CheckUndo();
		
		if ( GUILayout.Button( "Add Point" ) ) {
			myCurve.AddPoint( Vector3.zero );
        }
		
		EditorGUILayout.LabelField( new GUIContent( "Segments:" ) );
		EditorGUILayout.BeginHorizontal(); {
			int newSegmentCountFromField = EditorGUILayout.IntField( myCurve.mySegmentCount, GUILayout.Width( 40 ) );
			int newSegmentCountFromSlider = (int) GUILayout.HorizontalSlider( (float) myCurve.mySegmentCount, 1.0f, 60.0f );
			
			if ( newSegmentCountFromField != myCurve.mySegmentCount ) {
				myCurve.mySegmentCount = newSegmentCountFromField;
			} else if ( newSegmentCountFromSlider != myCurve.mySegmentCount ) {
				myCurve.mySegmentCount = newSegmentCountFromSlider;	
			}
		} EditorGUILayout.EndHorizontal();
					
		if ( GUI.changed ) {
			HandleUtility.Repaint();
		}
		
		myUndoManager.CheckDirty();	
	}
	
	// Private Methods
	
	private Vector3 CalculateBezierPoint( float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3 ) {
		float u = 1 - t;
		float tt = t*t;
		float uu = u*u;
		float uuu = uu * u;
		float ttt = tt * t;
		
		Vector3 p = uuu * p0; // first term
		p += 3 * uu * t * p1; // second term
		p += 3 * u * tt * p2; // third term
		p += ttt * p3; // fourth term
		
		return p;
	}
	
	// Private Members
	
	private HOEditorUndoManager myUndoManager;
	private HAPI_Curve myCurve;
	
	private int myCurrentlyActivePoint;
	
}
