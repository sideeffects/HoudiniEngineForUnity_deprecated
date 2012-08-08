using UnityEngine;
using UnityEditor;
using System.Collections;
using HAPI;

[ CustomEditor( typeof( HAPI_ObjectControl ) ) ]
public class HAPI_Inspector : Editor {
	private const float myFileChooserButtonWidth = 35;
	private const float myLineHeight = 16;
	private const float myLabelWidth = 120;
	private const float myToggleWidth = 12;
	
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
		        
		        if ( GUILayout.Button( "...", GUILayout.Width( myFileChooserButtonWidth ) ) ) {
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
		} // if
		
		EditorGUILayout.Separator();
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
		if ( myObjectControl.myParameters != null )
		{
			GUILayoutOption lineHeight = GUILayout.Height( myLineHeight );
			GUILayoutOption labelWidth = GUILayout.Width( myLabelWidth );
			GUILayoutOption toggleWidth = GUILayout.Width( myToggleWidth );			
			
			for ( int i = 0; i < myObjectControl.myParameterCount; ++i )
			{
				HAPI_Parameter parm = myObjectControl.myParameters[ i ];
				if ( parm.type == (int) HAPI_ParameterType.HAPI_PARMTYPE_FOLDER )
					continue;
				
				EditorGUILayout.BeginHorizontal(); {
					if ( parm.type == (int) HAPI_ParameterType.HAPI_PARMTYPE_INT )
					{
						EditorGUILayout.SelectableLabel( parm.label, labelWidth, lineHeight );
						for ( int p = 0; p < parm.size; ++p )
							parm.intValue[ p ] = EditorGUILayout.IntField( parm.intValue[ p ] );
					}
					else if ( parm.type == (int) HAPI_ParameterType.HAPI_PARMTYPE_FLOAT )
					{
						EditorGUILayout.SelectableLabel( parm.label, labelWidth, lineHeight );
						for ( int p = 0; p < parm.size; ++p )
							parm.floatValue[ p ] = EditorGUILayout.FloatField( parm.floatValue[ p ] );
					}
					else if ( parm.type == (int) HAPI_ParameterType.HAPI_PARMTYPE_STRING )
					{
						EditorGUILayout.SelectableLabel( parm.label, labelWidth, lineHeight );
						parm.stringValue = EditorGUILayout.TextField( parm.stringValue );
					}
					else if ( parm.type == (int) HAPI_ParameterType.HAPI_PARMTYPE_TOGGLE )
					{
						EditorGUILayout.LabelField( new GUIContent( "" ), labelWidth );
						parm.intValue[ 0 ] = ( EditorGUILayout.Toggle( parm.intValue[ 0 ] != 0, toggleWidth ) ? 1 : 0 );
						EditorGUILayout.SelectableLabel( parm.label, lineHeight );
					}
					else if ( parm.type == (int) HAPI_ParameterType.HAPI_PARMTYPE_COLOUR )
					{
						EditorGUILayout.SelectableLabel( parm.label, labelWidth, lineHeight );
						Color color = new Color( parm.floatValue[ 0 ], 
												 parm.floatValue[ 1 ], 
												 parm.floatValue[ 2 ] );
						if ( parm.size > 3 )
							color.a = parm.floatValue[ 3 ];
						
						color = EditorGUILayout.ColorField( color );
						
						parm.floatValue[ 0 ] = color.r;
						parm.floatValue[ 1 ] = color.g;
						parm.floatValue[ 2 ] = color.b;
						
						if ( parm.size > 3 )
							parm.floatValue[ 3 ] = color.a;
					}
				} EditorGUILayout.EndHorizontal();
			} // for
		} // if
	}
	
	public static string PromptForAssetPath( string location ) {
		string path = EditorUtility.OpenFilePanel(
			"Open Houdini OTL",
			location,
			"otl");
		return path;
	}
	
	private HOEditorUndoManager myUndoManager;
	private HAPI_ObjectControl myObjectControl;
}
