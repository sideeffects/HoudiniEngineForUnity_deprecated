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
		
		myUndoManager = new HOEditorUndoManager( myObjectControl, "HAPI_ObjectControl" );
		
		if ( GUI.changed )
			myObjectControl.Build();
	}
	
	public override void OnInspectorGUI() 
	{
		myUndoManager.CheckUndo();
		
		myObjectControl.myShowObjectControls = 
			EditorGUILayout.Foldout( myObjectControl.myShowObjectControls, new GUIContent( "Object Controls" ) );
		
		if ( myObjectControl.myShowObjectControls ) 
		{
			EditorGUILayout.LabelField( new GUIContent( "OTL Path:" ) );
			EditorGUILayout.BeginHorizontal(); 
			{
				string oldAssetPath = myObjectControl.GetAssetPath();
				string newAssetPath = "";
				newAssetPath = EditorGUILayout.TextField( oldAssetPath );
		        
		        if ( GUILayout.Button( "...", GUILayout.Width( myFileChooserButtonWidth ) ) ) 
				{
					string promptResultPath = PromptForAssetPath( oldAssetPath );
					if ( promptResultPath.Length > 0 )
						newAssetPath = promptResultPath;
		        }
				
				myObjectControl.SetAssetPath( newAssetPath );
			} 
			EditorGUILayout.EndHorizontal();
			
			if ( GUILayout.Button( "Rebuild" ) ) {
#if DEBUG
				myObjectControl.myAssetPathChanged = true;
#endif
				myObjectControl.Build();
			}
		} // if
		
		EditorGUILayout.Separator();
		myObjectControl.myShowAssetControls = 
			EditorGUILayout.Foldout( myObjectControl.myShowAssetControls, new GUIContent( "Asset Controls" ) );
		
		bool hasAssetChanged = false;
		if ( myObjectControl.myShowAssetControls )
			hasAssetChanged |= GenerateAssetControls();
					
		if ( hasAssetChanged )
			myObjectControl.Build();
		
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
	
	private bool GenerateAssetControl( int id, ref bool joinLast, ref bool noLabelToggleLast ) 
	{
		if ( myObjectControl.myParms == null )
			return false;
		
		bool changed = false;
		HAPI_ParmInfo[] parms = myObjectControl.myParms;
		HAPI_ParmType parm_type = (HAPI_ParmType) parms[ id ].type;
				
		GUIStyle labelStyle = new GUIStyle( GUI.skin.label );
		labelStyle.alignment = TextAnchor.MiddleRight;
		GUIStyle sliderStyle = new GUIStyle( GUI.skin.horizontalSlider );
		GUIStyle sliderThumbStyle = new GUIStyle( GUI.skin.horizontalSliderThumb );
				
		if ( parms[ id ].invisible )
			return changed;
						
		// decide whether to join with the previous parameter on the same 
		// line or not
		if ( !joinLast )
			EditorGUILayout.BeginHorizontal();
		
		// add label first if we're not a toggle
		if ( parm_type != HAPI_ParmType.HAPI_PARMTYPE_TOGGLE
			&& parm_type != HAPI_ParmType.HAPI_PARMTYPE_FOLDER
			&& !parms[ id ].labelNone )
		{
			GUILayoutOption labelFinalWidth = myLabelWidthGUI;
			if ( joinLast && !noLabelToggleLast )
			{
				float minWidth;
				float maxWidth;
				labelStyle.CalcMinMaxWidth( new GUIContent( parms[ id ].label ), out minWidth, out maxWidth );
				labelFinalWidth = GUILayout.Width( minWidth );
			}
			else if ( !joinLast )
			{
				// add padding for the toggle column
				EditorGUILayout.LabelField( "", myToggleWidthGUI );
			}
			EditorGUILayout.SelectableLabel( parms[ id ].label, labelStyle, labelFinalWidth, myLineHeightGUI );
			noLabelToggleLast = false;
		}
		
		//////////////////////////////////////////////////////////////////////
		// Integer Parameter
		if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_INT )
		{
			for ( int p = 0; p < parms[ id ].size; ++p )
			{
				int new_value = EditorGUILayout.IntField( parms[ id ].intValue[ p ] );
				
				if ( parms[ id ].size == 1 
					&& !joinLast 
					&& !parms[ id ].joinNext )
				{
					float ui_min = ( parms[ id ].hasUIMin ? parms[ id ].UIMin : 0.0f );
					float ui_max = ( parms[ id ].hasUIMax ? parms[ id ].UIMax : 10.0f );
					Rect lastDoubleRect = GetLastDoubleRect();
					sliderStyle.stretchWidth = false;
					sliderStyle.fixedWidth = lastDoubleRect.width;
					new_value = (int) GUI.HorizontalSlider( lastDoubleRect, new_value, ui_min, ui_max, 
															sliderStyle, sliderThumbStyle );
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
		else if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_FLOAT )
		{
			for ( int p = 0; p < parms[ id ].size; ++p )
			{				
				float new_value = EditorGUILayout.FloatField( parms[ id ].floatValue[ p ] );
				
				if ( parms[ id ].size == 1 
					&& !joinLast 
					&& !parms[ id ].joinNext )
				{
					float ui_min = ( parms[ id ].hasUIMin ? parms[ id ].UIMin : 0.0f );
					float ui_max = ( parms[ id ].hasUIMax ? parms[ id ].UIMax : 10.0f );
					Rect lastDoubleRect = GetLastDoubleRect();
					sliderStyle.stretchWidth = false;
					sliderStyle.fixedWidth = lastDoubleRect.width;
					new_value = GUI.HorizontalSlider( lastDoubleRect, new_value, ui_min, ui_max, 
													  sliderStyle, sliderThumbStyle );
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
		else if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_STRING )
		{
			string new_value = EditorGUILayout.TextField( parms[ id ].stringValue );
		
			if ( parms[ id ].stringValue != new_value )
			{
				parms[ id ].stringValue = new_value;
				changed |= true;
			}
		}
		//////////////////////////////////////////////////////////////////////
		// Toggle Parameter
		else if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_TOGGLE )
		{
			if ( !parms[ id ].joinNext )
			{
				// add padding for the toggle column
				EditorGUILayout.LabelField( myNullContent, myToggleWidthGUI );
				// add empty space to align with fields
				EditorGUILayout.LabelField( myNullContent, myLabelWidthGUI );
			}
			
			bool toggle_result = EditorGUILayout.Toggle( parms[ id ].intValue[ 0 ] != 0, myToggleWidthGUI );
			int new_value = ( toggle_result ? 1 : 0 );
			if ( !parms[ id ].labelNone )
				EditorGUILayout.SelectableLabel( parms[ id ].label, myLineHeightGUI );
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
		else if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_COLOUR )
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
		else if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_SEPARATOR )
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
	
	private bool GenerateAssetControls() 
	{
		if ( myObjectControl.myParms == null )
			return false;
		
		bool changed = false;
		int currentIndex = 0;
		HAPI_ParmInfo[] parms = myObjectControl.myParms;
				
		bool joinLast = false;
		bool noLabelToggleLast = false;
		
		int folder_list_count = 0;
		Stack< int > parent_id_stack = new Stack< int >();
		Stack< int > parent_count_stack = new Stack< int >();
		while ( currentIndex < myObjectControl.myParmCount )
		{
			int current_parent_id = -1;
			if ( parent_id_stack.Count != 0 )
		    {
				current_parent_id = parent_id_stack.Peek();
				
				if ( parent_count_stack.Count == 0 ) Debug.LogError( "" );
				
				if ( parms[ currentIndex ].parentId != current_parent_id )
				{
					currentIndex++;
					continue;
				}				
				
				int current_parent_count = parent_count_stack.Peek();
				current_parent_count--;
				if ( current_parent_count <= 0 )
				{
					parent_id_stack.Pop();
					parent_count_stack.Pop();
				}
				else
				{
					parent_count_stack.Pop();
					parent_count_stack.Push( current_parent_count );
				}
		    }
			
			if ( parms[ currentIndex ].parentId != current_parent_id )
			{
				currentIndex++;
				continue;
			}
			
			HAPI_ParmType parm_type = (HAPI_ParmType) parms[ currentIndex ].type;
					
			if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_FOLDERLIST )
			{
				folder_list_count++;
				int folder_count = parms[ currentIndex ].size;
				int first_folder_index = currentIndex + 1;
				int last_folder_index = currentIndex + folder_count;
				
				if ( myObjectControl.myFolderListSelections.Count <= folder_list_count )
				{
					myObjectControl.myFolderListSelections.Add( 0 );
					myObjectControl.myFolderListSelectionIds.Add( -1 );
				}
				
				List< int > tab_ids = new List< int >();
				List< string > tab_labels = new List< string >();
				List< int > tab_sizes = new List< int >();
				for ( currentIndex = first_folder_index; currentIndex <= last_folder_index; ++currentIndex )
				{
					if ( parms[ currentIndex ].type != (int) HAPI_ParmType.HAPI_PARMTYPE_FOLDER )
					{
						Debug.LogError( "We should be iterating through folders only here!"
							+ "\nCurrent Index: " + currentIndex + ", folder_count: " + folder_count );
					}
					
					tab_ids.Add( parms[ currentIndex ].id );
					tab_labels.Add( parms[ currentIndex ].label );
					tab_sizes.Add( parms[ currentIndex ].size );
				}
				currentIndex--;
				
				int selected_folder = myObjectControl.myFolderListSelections[ folder_list_count ];
				selected_folder = GUILayout.Toolbar( selected_folder, tab_labels.ToArray() );
				myObjectControl.myFolderListSelections[ folder_list_count ] = selected_folder;
				
				parent_id_stack.Push( tab_ids[ selected_folder ] );
				parent_count_stack.Push( tab_sizes[ selected_folder ] );
			}
			else
			{	
				if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_FOLDER )
					Debug.LogError( "All folders should have been parsed in the folder list if clause!" );
				
				changed |= GenerateAssetControl( currentIndex, ref joinLast, ref noLabelToggleLast );
			}
			
			currentIndex++;
		}
				
		return changed;
	}
	
	public static string PromptForAssetPath( string location ) 
	{
		string path = EditorUtility.OpenFilePanel( "Open Houdini OTL", location, "otl" );
		return path;
	}
	
	private HOEditorUndoManager myUndoManager;
	private HAPI_ObjectControl myObjectControl;
}
