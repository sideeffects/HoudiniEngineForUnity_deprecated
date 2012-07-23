using UnityEngine;
using UnityEditor;
using System.Collections;

[ CustomEditor( typeof( HAPI_ObjectControl ) ) ]
public class HAPI_Inspector : Editor {
	public void OnEnable() {		
		myObjectControl = target as HAPI_ObjectControl;
		
		myUndoManager = new HOEditorUndoManager( myObjectControl, "HAPI_ObjectControl" );
		
		if ( GUI.changed ) {
			myObjectControl.Build();
		}
	}
	
	public override void OnInspectorGUI() {
		myUndoManager.CheckUndo();
		
		EditorGUILayout.LabelField( new GUIContent( "Box Size" ) );
		myObjectControl.myBoxSize = EditorGUILayout.FloatField( myObjectControl.myBoxSize );
		
		//GUI.Button( new Rect( 10, 10, 100, 50 ), "Test" );
			
		if ( GUI.changed ) {
			myObjectControl.Build();
		}
		
		myUndoManager.CheckDirty();
	}
	
	private HOEditorUndoManager myUndoManager;
	private HAPI_ObjectControl myObjectControl;
}
