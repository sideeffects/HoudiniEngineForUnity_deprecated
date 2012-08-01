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
		
		myObjectControl.myShowObjectControls = EditorGUILayout.Foldout( myObjectControl.myShowObjectControls, new GUIContent( "Object Controls" ) );
		
		if ( myObjectControl.myShowObjectControls ) {
			EditorGUILayout.LabelField( new GUIContent( "OTL Path:" ) );
			EditorGUILayout.BeginHorizontal(); {
				string oldAssetPath = myObjectControl.GetAssetPath();
				string newAssetPath = "";
				newAssetPath = EditorGUILayout.TextField( oldAssetPath );
		        
		        if ( GUILayout.Button( "...", GUILayout.Width( 40 ) ) ) {
					string promptResultPath = PromptForAssetPath( oldAssetPath );
					if ( promptResultPath.Length > 0 ) {
						newAssetPath = promptResultPath;	
					}
		        }
				
				myObjectControl.SetAssetPath( newAssetPath );
			} EditorGUILayout.EndHorizontal();
			
			if ( GUILayout.Button( "Rebuild" ) ) {
				myObjectControl.Build();	
			}
		}
		
		myObjectControl.myShowAssetControls = EditorGUILayout.Foldout( myObjectControl.myShowAssetControls, new GUIContent( "Asset Controls" ) );
		
		if ( myObjectControl.myShowAssetControls ) {
			GenerateAssetControls();
		}
					
		if ( GUI.changed ) {
			myObjectControl.Build();
		}
		
		myUndoManager.CheckDirty();
	}
	
	public void GenerateAssetControls() {
		EditorGUILayout.LabelField( new GUIContent( "Box Size" ) );
		myObjectControl.myBoxSize = EditorGUILayout.FloatField( myObjectControl.myBoxSize );
		
	}
	
	public static string PromptForAssetPath( string location = "" ) {
		string path = EditorUtility.OpenFilePanel(
			"Open Houdini OTL",
			location,
			"otl");
		return path;
	}
	
	private HOEditorUndoManager myUndoManager;
	private HAPI_ObjectControl myObjectControl;
}
