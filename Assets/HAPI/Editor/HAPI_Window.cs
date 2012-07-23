using UnityEditor;
using UnityEngine;
using System.Collections;

public class HAPI_Window : EditorWindow {
	

	// Add menu item named "My Window" to the Window menu
	[ MenuItem( "Window/HAPI" ) ]
	public static void ShowWindow() {
		//Show existing window instance. If one doesn't exist, make one.
		EditorWindow.GetWindow( typeof( HAPI_Window ) );
	}
	
	public void CreateHAPIObject() {
			
	}

	public void OnGUI() {
		
		GUILayout.Label( "HAPI Controls", EditorStyles.boldLabel );
		
		GUILayout.Button( "Create HAPI Object" );
		
		
		//GUILayout.BeginHorizontal(
		
		//myString = EditorGUILayout.TextField( "HAPI Creation Tools", myString );
				
		/*
		groupEnabled = EditorGUILayout.BeginToggleGroup( "Optional Settings", groupEnabled );
			myBool = EditorGUILayout.Toggle( "Toggle", myBool );
			myFloat = EditorGUILayout.Slider( "Slider", myFloat, -3, 3 );
		EditorGUILayout.EndToggleGroup();
		*/
		
	}
}