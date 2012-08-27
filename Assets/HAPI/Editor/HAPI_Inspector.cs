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
 * 		Contains HAPI_Inspector, the GUI companion to HAPI_ObjectControl and responsible for generating
 * 		the custom Inspector GUI for an asset game object.
 * 
 */

#define DEBUG // since Unity doesn't seem to define it itself

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using HAPI;

/// <summary>
/// 	GUI companion to <see cref="HAPI_ObjectControl"/>. Responsible for generating the custom Inspector GUI 
/// 	for an asset game object.
/// </summary>
[ CustomEditor( typeof( HAPI_ObjectControl ) ) ]
public class HAPI_Inspector : Editor 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	public void OnEnable() 
	{		
		myObjectControl = target as HAPI_ObjectControl;
		
		myUndoManager = new HOEditorUndoManager( myObjectControl, "HAPI_ObjectControl" );
		
		if ( GUI.changed )
			myObjectControl.build();
	}
	
	public override void OnInspectorGUI() 
	{
		myUndoManager.CheckUndo();
		
		///////////////////////////////////////////////////////////////////////
		// Draw Game Object Controls
		
		myObjectControl.myShowObjectControls = 
			EditorGUILayout.Foldout( myObjectControl.myShowObjectControls, new GUIContent( "Object Controls" ) );
		
		if ( myObjectControl.myShowObjectControls ) 
		{
			EditorGUILayout.LabelField( new GUIContent( "OTL Path:" ) );
			EditorGUILayout.BeginHorizontal(); 
			{
				string oldAssetPath = myObjectControl.getAssetPath();
				string newAssetPath = "";
				newAssetPath = EditorGUILayout.TextField( oldAssetPath );
		        
		        if ( GUILayout.Button( "...", GUILayout.Width( myFileChooserButtonWidth ) ) ) 
				{
					string promptResultPath = promptForAssetPath( oldAssetPath );
					if ( promptResultPath.Length > 0 )
						newAssetPath = promptResultPath;
		        }
				
				myObjectControl.setAssetPath( newAssetPath );
			} 
			EditorGUILayout.EndHorizontal();
			
			if ( GUILayout.Button( "Rebuild" ) ) {
#if DEBUG
				myObjectControl.myAssetPathChanged = true;
#endif
				myObjectControl.build();
			}
		} // if
		
		///////////////////////////////////////////////////////////////////////
		// Draw Asset Controls
		
		EditorGUILayout.Separator();
		myObjectControl.myShowAssetControls = 
			EditorGUILayout.Foldout( myObjectControl.myShowAssetControls, new GUIContent( "Asset Controls" ) );
		
		bool hasAssetChanged = false;
		if ( myObjectControl.myShowAssetControls )
			hasAssetChanged |= generateAssetControls();
					
		if ( hasAssetChanged )
			myObjectControl.build();
		
		myUndoManager.CheckDirty();
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private
	
	/// <summary>
	/// 	Creates two empty label fields, gets the rectangles from each, and combines it to create
	/// 	the last double rectangle. This is used for <see cref="GUI.HorizontalSlider"/> which
	/// 	uses absolute positioning and needs a rectangle to know it's size and position.
	/// 	This way, we can insert sliders within the relative positioning of the Inspector GUI elements.
	/// </summary>
	/// <returns>
	/// 	The last double rectangle.
	/// </returns>
	private Rect getLastDoubleRect()
	{		
		// Draw first empty label field. 
		EditorGUILayout.LabelField( myNullContent, myDummyLabelMinWidthGUI );
		float xMin = GUILayoutUtility.GetLastRect().xMin;
		float yMin = GUILayoutUtility.GetLastRect().yMin;
		float width = GUILayoutUtility.GetLastRect().width;
		float height = GUILayoutUtility.GetLastRect().height;
		
		// Draw second empty label field.
		EditorGUILayout.LabelField( myNullContent, myDummyLabelMinWidthGUI );
		float width2 = GUILayoutUtility.GetLastRect().width;
		
		// Create the double rectangle from the two above.
		Rect last_double_rect = new Rect( xMin, yMin, width + width2, height );
		
		return last_double_rect;
	}
	
	/// <summary>
	/// 	Draws a single asset control.
	/// </summary>
	/// <param name="id">
	/// 	Corresponding parameter id as given by <see cref="HAPI_Host.GetParameters"/>.
	/// </param>
	/// <param name="join_last">
	/// 	Determines if the current control should be put on the same line as the previous one.
	/// 	Also serves as a return value to be used with the next control.
	/// </param>
	/// <param name="no_label_toggle_last">
	/// 	Determines if the current control should not have its label drawn.
	/// 	Also serves as a return value to be used with the next control.
	/// </param>
	/// <returns>
	/// 	<c>true</c> if the parameter value corresponding to this control has changed, <c>false</c> otherwise.
	/// </returns>
	private bool generateAssetControl( int id, ref bool join_last, ref bool no_label_toggle_last ) 
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
						
		// Decide whether to join with the previous parameter on the same 
		// line or not.
		if ( !join_last )
			EditorGUILayout.BeginHorizontal();
		
		// Add label first if we're not a toggle.
		if ( parm_type != HAPI_ParmType.HAPI_PARMTYPE_TOGGLE
			&& parm_type != HAPI_ParmType.HAPI_PARMTYPE_FOLDER
			&& !parms[ id ].labelNone )
		{
			GUILayoutOption labelFinalWidth = myLabelWidthGUI;
			if ( join_last && !no_label_toggle_last )
			{
				float minWidth;
				float maxWidth;
				labelStyle.CalcMinMaxWidth( new GUIContent( parms[ id ].label ), out minWidth, out maxWidth );
				labelFinalWidth = GUILayout.Width( minWidth );
			}
			else if ( !join_last )
			{
				// add padding for the toggle column
				EditorGUILayout.LabelField( "", myToggleWidthGUI );
			}
			EditorGUILayout.SelectableLabel( parms[ id ].label, labelStyle, labelFinalWidth, myLineHeightGUI );
			no_label_toggle_last = false;
		}
		
		///////////////////////////////////////////////////////////////////////
		// Integer Parameter
		if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_INT )
		{
			for ( int p = 0; p < parms[ id ].size; ++p )
			{
				// Draw field.
				int new_value = EditorGUILayout.IntField( parms[ id ].intValue[ p ] );
				
				// Draw the slider.
				if ( parms[ id ].size == 1 
					&& !join_last 
					&& !parms[ id ].joinNext )
				{
					float ui_min = ( parms[ id ].hasUIMin ? parms[ id ].UIMin : 0.0f );
					float ui_max = ( parms[ id ].hasUIMax ? parms[ id ].UIMax : 10.0f );
					Rect lastDoubleRect = getLastDoubleRect();
					sliderStyle.stretchWidth = false;
					sliderStyle.fixedWidth = lastDoubleRect.width;
					new_value = (int) GUI.HorizontalSlider( lastDoubleRect, new_value, ui_min, ui_max, 
															sliderStyle, sliderThumbStyle );
				}
				
				// Enforce min/max bounds.
				if ( parms[ id ].hasMin && new_value < (int) parms[ id ].min )
					new_value = (int) parms[ id ].min;
				if ( parms[ id ].hasMax && new_value > (int) parms[ id ].max )
					new_value = (int) parms[ id ].max;
				
				// Determine if value changed and update parameter value.
				if ( parms[ id ].intValue[ p ] != new_value )
				{
					parms[ id ].intValue[ p ] = new_value;
					changed |= true;
				}
			}			
		}		
		///////////////////////////////////////////////////////////////////////
		// Float Parameter
		else if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_FLOAT )
		{
			for ( int p = 0; p < parms[ id ].size; ++p )
			{	
				// Draw field.
				float new_value = EditorGUILayout.FloatField( parms[ id ].floatValue[ p ] );
				
				// Draw the slider.
				if ( parms[ id ].size == 1 
					&& !join_last 
					&& !parms[ id ].joinNext )
				{
					float ui_min = ( parms[ id ].hasUIMin ? parms[ id ].UIMin : 0.0f );
					float ui_max = ( parms[ id ].hasUIMax ? parms[ id ].UIMax : 10.0f );
					Rect lastDoubleRect = getLastDoubleRect();
					sliderStyle.stretchWidth = false;
					sliderStyle.fixedWidth = lastDoubleRect.width;
					new_value = GUI.HorizontalSlider( lastDoubleRect, new_value, ui_min, ui_max, 
													  sliderStyle, sliderThumbStyle );
				}
				
				// Enforce min/max bounds.
				if ( parms[ id ].hasMin && new_value < parms[ id ].min )
					new_value = parms[ id ].min;
				if ( parms[ id ].hasMax && new_value > parms[ id ].max )
					new_value = parms[ id ].max;
				
				// Determine if value changed and update parameter value.
				if ( parms[ id ].floatValue[ p ] != new_value )
				{
					parms[ id ].floatValue[ p ] = new_value;
					changed |= true;
				}
			}
		}		
		///////////////////////////////////////////////////////////////////////
		// String Parameter
		else if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_STRING )
		{
			string new_value = EditorGUILayout.TextField( parms[ id ].stringValue );
			
			// Determine if value changed and update parameter value.
			if ( parms[ id ].stringValue != new_value )
			{
				parms[ id ].stringValue = new_value;
				changed |= true;
			}
		}
		///////////////////////////////////////////////////////////////////////
		// Toggle Parameter
		else if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_TOGGLE )
		{
			if ( !parms[ id ].joinNext )
			{
				// Add padding for the toggle column.
				EditorGUILayout.LabelField( myNullContent, myToggleWidthGUI );
				// Add empty space to align with fields.
				EditorGUILayout.LabelField( myNullContent, myLabelWidthGUI );
			}
			
			// Draw toggle with its label.
			bool toggle_result = EditorGUILayout.Toggle( parms[ id ].intValue[ 0 ] != 0, myToggleWidthGUI );
			int new_value = ( toggle_result ? 1 : 0 );
			if ( !parms[ id ].labelNone )
				EditorGUILayout.SelectableLabel( parms[ id ].label, myLineHeightGUI );
			else
				no_label_toggle_last = true;
			
			// Determine if value changed and update parameter value.
			if ( parms[ id ].intValue[ 0 ] != new_value )
			{
				parms[ id ].intValue[ 0 ] = new_value;
				changed |= true;
			}
		}		
		///////////////////////////////////////////////////////////////////////
		// Color Parameter
		else if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_COLOUR )
		{
			Color color = new Color( parms[ id ].floatValue[ 0 ], 
									 parms[ id ].floatValue[ 1 ], 
									 parms[ id ].floatValue[ 2 ] );
			if ( parms[ id ].size > 3 )
				color.a = parms[ id ].floatValue[ 3 ];
			
			// Draw control.
			Color new_color = EditorGUILayout.ColorField( color );
			
			// Determine if value changed and update parameter value.
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
		///////////////////////////////////////////////////////////////////////
		// Separator
		else if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_SEPARATOR )
		{
			EditorGUILayout.Separator();
		}
		
		// Decide whether to join with the next parameter on the same line or not
		// but also save our status for the next parameter.
		join_last = parms[ id ].joinNext;
		if ( !parms[ id ].joinNext )
			EditorGUILayout.EndHorizontal();
		
		return changed;
	}
	
	/// <summary>
	/// 	Draws all asset controls.
	/// </summary>
	/// <returns>
	/// 	<c>true</c> if any of the control values have changed from the corresponding cached parameter
	/// 	values, <c>false</c> otherwise.
	/// </returns>
	private bool generateAssetControls() 
	{
		if ( myObjectControl.myParms == null )
			return false;
		
		bool changed 				= false;
		int current_index 			= 0;
		HAPI_ParmInfo[] parms 		= myObjectControl.myParms;
				
		bool join_last 				= false;
		bool no_label_toggle_last 	= false;
		
		int folder_list_count 		= 0;
		
		// These stacks maintain the current folder depth, parent id, and how many more child 
		// parameters are still contained in the current folder.
		Stack< int > parent_id_stack 		= new Stack< int >();
		Stack< int > parent_count_stack 	= new Stack< int >();
		
		// Loop through all the parameters.
		while ( current_index < myObjectControl.myParmCount )
		{
			int current_parent_id = -1; // The root has parent id -1.
			
			// If we're not at the root (empty parent stack), get the current parent id and parent 
			// count from the stack as well as decrement the parent count as we're about to parse 
			// another parameter.
			if ( parent_id_stack.Count != 0 )
		    {
				current_parent_id = parent_id_stack.Peek();
				
				if ( parent_count_stack.Count == 0 ) Debug.LogError( "" );
				
				// If the current parameter, whatever it may be, does not belong to the current active
				// parent then skip it. This check has to be done here because we do not want to
				// increment the top of the parent_count_stack as if we included a parameter in the
				// current folder.
				if ( parms[ current_index ].parentId != current_parent_id )
				{
					current_index++;
					continue;
				}				
				
				int current_parent_count = parent_count_stack.Peek();
				current_parent_count--;
				
				// If we've reached the last parameter in the current folder we need to pop the parent 
				// stacks (we're done with the current folder). Otherwise, update the top of the 
				// parent_count_stack.
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
			else if ( parms[ current_index ].parentId != current_parent_id )
			{
				// If the current parameter does not belong to the current active parent then skip it.
				current_index++;
				continue;
			}
			
			HAPI_ParmType parm_type = (HAPI_ParmType) parms[ current_index ].type;
					
			if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_FOLDERLIST )
			{
				// The current parameter is a folder list which means the next parms[ currentIndex ].size
				// parameters will be folders belonging to this folder list. Push to the stack a new
				// folder depth, ready to eat the next few parameters belonging to the folder list's 
				// selected folder.
				
				folder_list_count++;
				int folder_count 		= parms[ current_index ].size;
				int first_folder_index 	= current_index + 1;
				int last_folder_index 	= current_index + folder_count;
				
				// If myObjectControl.myFolderListSelections is smaller than our current depth it means this
				// is the first GUI generation for this asset (no previous folder selection data) so
				// increase the size of the selection arrays to accomodate the new depth.
				if ( myObjectControl.myFolderListSelections.Count <= folder_list_count )
				{
					myObjectControl.myFolderListSelections.Add( 0 );
					myObjectControl.myFolderListSelectionIds.Add( -1 );
				}
				
				// Generate the list of folders which will be passed to the GUILayout.Toolbar() method.
				List< int > 	tab_ids 	= new List< int >();
				List< string > 	tab_labels 	= new List< string >();
				List< int > 	tab_sizes 	= new List< int >();
				for ( current_index = first_folder_index; current_index <= last_folder_index; ++current_index )
				{
					if ( parms[ current_index ].type != (int) HAPI_ParmType.HAPI_PARMTYPE_FOLDER )
					{
						Debug.LogError( "We should be iterating through folders only here!"
							+ "\nCurrent Index: " + current_index + ", folder_count: " + folder_count );
					}
					
					tab_ids.Add( 		parms[ current_index ].id );
					tab_labels.Add( 	parms[ current_index ].label );
					tab_sizes.Add( 		parms[ current_index ].size );
				}
				current_index--; // We decrement the current_index as we incremented one too many in the for loop.
				
				int selected_folder 	= myObjectControl.myFolderListSelections[ folder_list_count ];
				selected_folder 		= GUILayout.Toolbar( selected_folder, tab_labels.ToArray() );
				myObjectControl.myFolderListSelections[ folder_list_count ] = selected_folder;
				
				// Push only the selected folder info to the parent stacks since for this depth and this folder
				// list only the parameters of the selected folder need to be generated.
				parent_id_stack.Push( 		tab_ids[ selected_folder ] );
				parent_count_stack.Push( 	tab_sizes[ selected_folder ] );
			}
			else
			{
				// The current parameter is a simple parameters so just draw it.
				
				if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_FOLDER )
					Debug.LogError( "All folders should have been parsed in the folder list if clause!" );
				
				changed |= generateAssetControl( current_index, ref join_last, ref no_label_toggle_last );
			}
			
			current_index++;
		}
				
		return changed;
	}
	
	/// <summary>
	/// 	Opens the file chooser dialog window set to open .otl files only.
	/// </summary>
	/// <param name="location">
	/// 	Starting location.
	/// </param>
	/// <returns>
	/// 	The absolute path to the .otl file selected.
	/// </returns>
	public static string promptForAssetPath( string location ) 
	{
		string path = EditorUtility.OpenFilePanel( "Open Houdini OTL", location, "otl" );
		return path;
	}
	
	private const float 		myFileChooserButtonWidth 	= 30;
	private const float 		myLineHeight 				= 16;
	private const float 		myLabelWidth 				= 120;
	private const float 		myToggleWidth 				= 12;
	private const float			myDummyLabelMinWidth		= 60;
	
	private const float 		myDefaultUIMin 				= 0.0f;
	private const float 		myDefaultUIMax 				= 10.0f;
	
	private GUILayoutOption 	myLineHeightGUI 			= GUILayout.Height( myLineHeight );
	private GUILayoutOption 	myLabelWidthGUI 			= GUILayout.Width( myLabelWidth );
	private GUILayoutOption 	myToggleWidthGUI 			= GUILayout.Width( myToggleWidth );
	private GUILayoutOption		myDummyLabelMinWidthGUI		= GUILayout.MinWidth( myDummyLabelMinWidth );
	
	private GUIContent 			myNullContent 				= new GUIContent( "" );
	
	private HOEditorUndoManager myUndoManager;
	private HAPI_ObjectControl 	myObjectControl;
}
