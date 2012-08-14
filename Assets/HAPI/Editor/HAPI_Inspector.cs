#define DEBUG // since Unity doesn't seem to define it itself

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using HAPI;

[ CustomEditor( typeof( HAPI_ObjectControl ) ) ]
public class HAPI_Inspector : Editor {
	private const float myFileChooserButtonWidth = 30;
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
#if DEBUG
				myObjectControl.myAssetPathChanged = true;
#endif
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
			HAPI_Parameter[] parms = myObjectControl.myParameters;
			
			GUILayoutOption lineHeight = GUILayout.Height( myLineHeight );
			GUILayoutOption labelWidth = GUILayout.Width( myLabelWidth );
			GUILayoutOption toggleWidth = GUILayout.Width( myToggleWidth );
			
			GUIContent nullContent = new GUIContent( "" );
			
			GUIStyle labelStyle = new GUIStyle( GUI.skin.label );
			labelStyle.alignment = TextAnchor.MiddleRight;
			
			int tabCount = -1;
			List< string > tabLabels = new List< string >();
			for ( int i = 0; i < myObjectControl.myParameterCount; ++i )
			{
				if ( parms[ i ].id < 0 ) continue; // TODO: this is how we hide parms for now
				
				if ( parms[ i ].type == (int) HAPI_ParameterType.HAPI_PARMTYPE_FOLDER )
				{	
					// increment first so that all elements afterwards get this folder
					// as their parent
					tabCount++;
					
					tabLabels.Add( parms[ i ].label );
					parms[ i ].intValue[ 0 ] = tabCount;
				}
				else
				{
					parms[ i ].parentId = tabCount;
				}
			}
			
			myObjectControl.myAssetTabSelectedIndex = 
				GUILayout.Toolbar( myObjectControl.myAssetTabSelectedIndex, tabLabels.ToArray() );
			
			bool joinLast = false;
			bool noLabelToggleLast = false;
			for ( int i = 0; i < myObjectControl.myParameterCount; ++i )
			{
				if ( parms[ i ].invisible ) continue;
				
				if ( parms[ i ].type == (int) HAPI_ParameterType.HAPI_PARMTYPE_FOLDER )
					continue;
				else if ( parms[ i ].parentId != myObjectControl.myAssetTabSelectedIndex )
					continue;
				
				// decide whether to join with the previous parameter on the same line or not
				if ( !joinLast )
					EditorGUILayout.BeginHorizontal();
				
				// add label first if we're not a toggle
				if ( parms[ i ].type != (int) HAPI_ParameterType.HAPI_PARMTYPE_TOGGLE
					&& !parms[ i ].labelNone )
				{
					GUILayoutOption labelFinalWidth = labelWidth;
					if ( joinLast && !noLabelToggleLast )
					{
						float minWidth;
						float maxWidth;
						labelStyle.CalcMinMaxWidth( new GUIContent( parms[ i ].label ), out minWidth, out maxWidth );
						labelFinalWidth = GUILayout.Width( minWidth );
					}
					else if ( !joinLast )
					{
						EditorGUILayout.LabelField( "", toggleWidth ); // add padding for the toggle column
					}
					EditorGUILayout.SelectableLabel( parms[ i ].label, labelStyle, labelFinalWidth, lineHeight );
					noLabelToggleLast = false;
				}
				
				if ( parms[ i ].type == (int) HAPI_ParameterType.HAPI_PARMTYPE_INT )
					for ( int p = 0; p < parms[ i ].size; ++p )
						parms[ i ].intValue[ p ] = EditorGUILayout.IntField( parms[ i ].intValue[ p ] );
				else if ( parms[ i ].type == (int) HAPI_ParameterType.HAPI_PARMTYPE_FLOAT )
				{
					if ( parms[ i ].size == 1 )
					{						
						parms[ i ].floatValue[ 0 ] = EditorGUILayout.FloatField( parms[ i ].floatValue[ 0 ] );
						EditorGUILayout.LabelField( nullContent );
						parms[ i ].floatValue[ 0 ] = GUI.HorizontalSlider( GUILayoutUtility.GetLastRect(), parms[ i ].floatValue[ 0 ], 0.0f, 10.0f );
					}
					else
						for ( int p = 0; p < parms[ i ].size; ++p )
							parms[ i ].floatValue[ p ] = EditorGUILayout.FloatField( parms[ i ].floatValue[ p ] );
				}
				else if ( parms[ i ].type == (int) HAPI_ParameterType.HAPI_PARMTYPE_STRING )
					parms[ i ].stringValue = EditorGUILayout.TextField( parms[ i ].stringValue );
				else if ( parms[ i ].type == (int) HAPI_ParameterType.HAPI_PARMTYPE_TOGGLE )
				{
					if ( !parms[ i ].joinNext )
					{
						EditorGUILayout.LabelField( nullContent, toggleWidth ); // add padding for the toggle column
						EditorGUILayout.LabelField( nullContent, labelWidth ); // add empty space to align with fields
					}
						
					parms[ i ].intValue[ 0 ] = ( EditorGUILayout.Toggle( parms[ i ].intValue[ 0 ] != 0, toggleWidth ) ? 1 : 0 );
					if ( !parms[ i ].labelNone )
						EditorGUILayout.SelectableLabel( parms[ i ].label, lineHeight );
					else
						noLabelToggleLast = true;
				}
				else if ( parms[ i ].type == (int) HAPI_ParameterType.HAPI_PARMTYPE_COLOUR )
				{
					Color color = new Color( parms[ i ].floatValue[ 0 ], 
											 parms[ i ].floatValue[ 1 ], 
											 parms[ i ].floatValue[ 2 ] );
					if ( parms[ i ].size > 3 )
						color.a = parms[ i ].floatValue[ 3 ];
					
					color = EditorGUILayout.ColorField( color );
					
					parms[ i ].floatValue[ 0 ] = color.r;
					parms[ i ].floatValue[ 1 ] = color.g;
					parms[ i ].floatValue[ 2 ] = color.b;
					
					if ( parms[ i ].size > 3 )
						parms[ i ].floatValue[ 3 ] = color.a;
				}
				else if ( parms[ i ].type == (int) HAPI_ParameterType.HAPI_PARMTYPE_SEPARATOR )
				{
					EditorGUILayout.Separator();
				}
				
				// decide whether to join with the next parameter on the same line or not
				// but also save our status for the next parameter
				joinLast = parms[ i ].joinNext;
				if ( !parms[ i ].joinNext )
					EditorGUILayout.EndHorizontal();
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
