#define DEBUG // since Unity doesn't seem to define it itself

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using HAPI;

[ CustomEditor( typeof( HAPI_ObjectControl ) ) ]
public class HAPI_Inspector : Editor 
{
	private const float myFileChooserButtonWidth = 30;
	private const float myLineHeight = 16;
	private const float myLabelWidth = 120;
	private const float myToggleWidth = 12;
	
	private const float myDefaultUIMin = 0.0f;
	private const float myDefaultUIMax = 10.0f;
	
	private GUILayoutOption myLineHeightGUI = GUILayout.Height( myLineHeight );
	private GUILayoutOption myLabelWidthGUI = GUILayout.Width( myLabelWidth );
	private GUILayoutOption myToggleWidthGUI = GUILayout.Width( myToggleWidth );
	
	private GUIContent myNullContent = new GUIContent( "" );
	
	public void OnEnable() 
	{		
		myObjectControl = target as HAPI_ObjectControl;
		
		myUndoManager = new HOEditorUndoManager( myObjectControl, 
												 "HAPI_ObjectControl" );
		
		if ( GUI.changed )
			myObjectControl.Build();
	}
	
	public override void OnInspectorGUI() 
	{
		myUndoManager.CheckUndo();
		
		myObjectControl.myShowObjectControls = 
			EditorGUILayout.Foldout( myObjectControl.myShowObjectControls, 
									 new GUIContent( "Object Controls" ) );
		
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
		myObjectControl.myShowAssetControls = 
			EditorGUILayout.Foldout( myObjectControl.myShowAssetControls, 
									 new GUIContent( "Asset Controls" ) );
		
		bool hasAssetChanged = false;
		if ( myObjectControl.myShowAssetControls ) {
			// the root has ID -1, depth 0
			hasAssetChanged |= GenerateAssetControls( -1, 0 );
		}
					
		if ( hasAssetChanged ) {
			myObjectControl.Build();
		}
		
		myUndoManager.CheckDirty();
	}
	
	private Rect GetLastDoubleRect()
	{		
		// draw first empty label field 
		EditorGUILayout.LabelField( myNullContent );
		float xMin = GUILayoutUtility.GetLastRect().xMin;
		float yMin = GUILayoutUtility.GetLastRect().yMin;
		float width = GUILayoutUtility.GetLastRect().width;
		float height = GUILayoutUtility.GetLastRect().height;
		
		// draw second empty label field
		EditorGUILayout.LabelField( myNullContent );
		float width2 = GUILayoutUtility.GetLastRect().width;
		
		return new Rect( xMin, yMin, width + width2, height );
	}
	
	private bool GenerateAssetControl( int id, int depth, ref bool joinLast, 
									   ref bool noLabelToggleLast ) 
	{
		if ( myObjectControl.myParameters == null )
			return false;
		
		bool changed = false;
		HAPI_Parameter[] parms = myObjectControl.myParameters;
		int parentId = myObjectControl.mySelectedAssetTabIds[ depth ];
		HAPI_ParameterType parm_type = (HAPI_ParameterType) parms[ id ].type;
				
		GUIStyle labelStyle = new GUIStyle( GUI.skin.label );
		labelStyle.alignment = TextAnchor.MiddleRight;
		
		if ( parms[ id ].invisible )
			return changed;
			
		if ( parms[ id ].parentId != parentId )
			return changed;
						
		// decide whether to join with the previous parameter on the same 
		// line or not
		if ( !joinLast )
			EditorGUILayout.BeginHorizontal();
		
		// add label first if we're not a toggle
		if ( parm_type != HAPI_ParameterType.HAPI_PARMTYPE_TOGGLE
			&& parm_type != HAPI_ParameterType.HAPI_PARMTYPE_FOLDER
			&& !parms[ id ].labelNone )
		{
			GUILayoutOption labelFinalWidth = myLabelWidthGUI;
			if ( joinLast && !noLabelToggleLast )
			{
				float minWidth;
				float maxWidth;
				labelStyle.CalcMinMaxWidth( new GUIContent( parms[ id ].label ), 
											out minWidth, out maxWidth );
				labelFinalWidth = GUILayout.Width( minWidth );
			}
			else if ( !joinLast )
			{
				// add padding for the toggle column
				EditorGUILayout.LabelField( "", myToggleWidthGUI );
			}
			EditorGUILayout.SelectableLabel( parms[ id ].label, labelStyle, 
											 labelFinalWidth, myLineHeightGUI );
			noLabelToggleLast = false;
		}
		
		//////////////////////////////////////////////////////////////////////
		// Integer Parameter
		if ( parm_type == HAPI_ParameterType.HAPI_PARMTYPE_INT )
		{
			for ( int p = 0; p < parms[ id ].size; ++p )
			{
				int new_value = 
					EditorGUILayout.IntField( parms[ id ].intValue[ p ] );
				
				if ( parms[ id ].size == 1 
					&& !joinLast 
					&& !parms[ id ].joinNext )
				{
					float ui_min = ( parms[ id ].hasUIMin ? parms[ id ].UIMin 
														  : 0.0f );
					float ui_max = ( parms[ id ].hasUIMax ? parms[ id ].UIMax 
														  : 10.0f );
					new_value = 
						(int) GUI.HorizontalSlider( GetLastDoubleRect(), 
											  		new_value, 
											  		ui_min, 
													ui_max );
				}
				
				if ( parms[ id ].hasMin && new_value < (int) parms[ id ].min )
					new_value = (int) parms[ id ].min;
				if ( parms[ id ].hasMax && new_value > (int) parms[ id ].max )
					new_value = (int) parms[ id ].max;
				
				if ( parms[ id ].intValue[ p ] != new_value )
				{
					parms[ id ].intValue[ p ] = new_value;
					changed |= true;
				}
			}			
		}		
		//////////////////////////////////////////////////////////////////////
		// Float Parameter
		else if ( parm_type == HAPI_ParameterType.HAPI_PARMTYPE_FLOAT )
		{
			for ( int p = 0; p < parms[ id ].size; ++p )
			{				
				float new_value = 
					EditorGUILayout.FloatField( parms[ id ].floatValue[ p ] );
				
				if ( parms[ id ].size == 1 
					&& !joinLast 
					&& !parms[ id ].joinNext )
				{
					float ui_min = ( parms[ id ].hasUIMin ? parms[ id ].UIMin 
														  : 0.0f );
					float ui_max = ( parms[ id ].hasUIMax ? parms[ id ].UIMax 
														  : 10.0f );
					new_value = GUI.HorizontalSlider( GetLastDoubleRect(), 
											  		  new_value, 
											  		  ui_min, ui_max );
				}
				
				if ( parms[ id ].hasMin && new_value < parms[ id ].min )
					new_value = parms[ id ].min;
				if ( parms[ id ].hasMax && new_value > parms[ id ].max )
					new_value = parms[ id ].max;
				
				if ( parms[ id ].floatValue[ p ] != new_value )
				{
					parms[ id ].floatValue[ p ] = new_value;
					changed |= true;
				}
			}
		}		
		//////////////////////////////////////////////////////////////////////
		// String Parameter
		else if ( parm_type == HAPI_ParameterType.HAPI_PARMTYPE_STRING )
		{
			string new_value = 
				EditorGUILayout.TextField( parms[ id ].stringValue );
		
			if ( parms[ id ].stringValue != new_value )
			{
				parms[ id ].stringValue = new_value;
				changed |= true;
			}
		}
		//////////////////////////////////////////////////////////////////////
		// Toggle Parameter
		else if ( parm_type == HAPI_ParameterType.HAPI_PARMTYPE_TOGGLE )
		{
			if ( !parms[ id ].joinNext )
			{
				// add padding for the toggle column
				EditorGUILayout.LabelField( myNullContent, myToggleWidthGUI );
				// add empty space to align with fields
				EditorGUILayout.LabelField( myNullContent, myLabelWidthGUI );
			}
				
			int new_value = 
				( EditorGUILayout.Toggle( parms[ id ].intValue[ 0 ] != 0, 
										  myToggleWidthGUI ) 
				  ? 1 : 0 );
			if ( !parms[ id ].labelNone )
				EditorGUILayout.SelectableLabel( parms[ id ].label, 
												 myLineHeightGUI );
			else
				noLabelToggleLast = true;
			
			if ( parms[ id ].intValue[ 0 ] != new_value )
			{
				parms[ id ].intValue[ 0 ] = new_value;
				changed |= true;
			}
		}		
		//////////////////////////////////////////////////////////////////////
		// Color Parameter
		else if ( parm_type == HAPI_ParameterType.HAPI_PARMTYPE_COLOUR )
		{
			Color color = new Color( parms[ id ].floatValue[ 0 ], 
									 parms[ id ].floatValue[ 1 ], 
									 parms[ id ].floatValue[ 2 ] );
			if ( parms[ id ].size > 3 )
				color.a = parms[ id ].floatValue[ 3 ];
			
			Color new_color = EditorGUILayout.ColorField( color );
			
			if ( color != new_color )
			{
				parms[ id ].floatValue[ 0 ] = new_color.r;
				parms[ id ].floatValue[ 1 ] = new_color.g;
				parms[ id ].floatValue[ 2 ] = new_color.b;
				
				if ( parms[ id ].size > 3 )
					parms[ id ].floatValue[ 3 ] = new_color.a;
			
				changed |= true;
			}
		}		
		//////////////////////////////////////////////////////////////////////
		// Separator
		else if ( parm_type == HAPI_ParameterType.HAPI_PARMTYPE_SEPARATOR )
		{
			EditorGUILayout.Separator();
		}
		
		// decide whether to join with the next parameter on the same line or not
		// but also save our status for the next parameter
		joinLast = parms[ id ].joinNext;
		if ( !parms[ id ].joinNext )
			EditorGUILayout.EndHorizontal();
		
		return changed;
	}
	
	private bool GenerateAssetControls( int parentId, int depth ) {		
		if ( myObjectControl.myParameters == null )
			return false;		
		
		bool changed = false;
		HAPI_Parameter[] parms = myObjectControl.myParameters;
		int childDepth = depth + 1;
		
		// make sure the list of tab selections is big enough
		if ( myObjectControl.mySelectedAssetTabs.Count <= childDepth )
			myObjectControl.mySelectedAssetTabs.Add( 0 );
		if ( myObjectControl.mySelectedAssetTabIds.Count <= childDepth )
			myObjectControl.mySelectedAssetTabIds.Add( parentId );
		
		bool joinLast = false;
		bool noLabelToggleLast = false;
				
		int tabCount = -1;
		List< string > tabLabels = new List< string >();
		List< int > tabIds = new List< int >();
		for ( int i = 0; i < myObjectControl.myParameterCount; ++i )
		{
			if ( parms[ i ].invisible ) 
				continue;
			if ( parms[ i ].parentId != parentId ) 
				continue;
			
			if ( parms[ i ].type == (int) HAPI_ParameterType.HAPI_PARMTYPE_FOLDER )
			{	
				// increment first so that all elements afterwards get 
				// this folder as their parent
				tabCount++;
				
				tabLabels.Add( parms[ i ].label );
				tabIds.Add( parms[ i ].id );
				parms[ i ].intValue[ 0 ] = tabCount;
			}
			else if ( depth == 0 )
			{
				changed |= GenerateAssetControl( i, depth, ref joinLast, 
												 ref noLabelToggleLast );
			} // if
		} // for
		
		myObjectControl.mySelectedAssetTabs[ childDepth ] = 
			GUILayout.Toolbar( myObjectControl.mySelectedAssetTabs[ childDepth ], 
							   tabLabels.ToArray() );
		
		int childParentId = 
			tabIds[ myObjectControl.mySelectedAssetTabs[ childDepth ] ];
		myObjectControl.mySelectedAssetTabIds[ childDepth ] = childParentId;
		
		joinLast = false;
		noLabelToggleLast = false;
		for ( int i = 0; i < myObjectControl.myParameterCount; ++i )
		{
			if ( parms[ i ].type == (int) HAPI_ParameterType.HAPI_PARMTYPE_FOLDER
				&& parms[ i ].parentId == childParentId )
				changed |= GenerateAssetControls( childParentId, childDepth );
			else
				changed |= GenerateAssetControl( i, childDepth, 
												 ref joinLast, 
												 ref noLabelToggleLast );
		} // for
		
		return changed;
	}
	
	public static string PromptForAssetPath( string location ) 
	{
		string path = EditorUtility.OpenFilePanel( "Open Houdini OTL",
												   location,
												   "otl" );
		return path;
	}
	
	private HOEditorUndoManager myUndoManager;
	private HAPI_ObjectControl myObjectControl;
}
