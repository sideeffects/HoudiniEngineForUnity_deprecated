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
 * 
 */


using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using HAPI;

/// <summary>
/// 	GUI companion to <see cref="HAPI_Asset"/>. Responsible for generating the custom Inspector GUI 
/// 	for an asset game object.
/// </summary>
[ CustomEditor( typeof( HAPI_Asset ) ) ]
public partial class HAPI_AssetGUI : Editor 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	public void OnEnable() 
	{		
		myObjectControl = target as HAPI_Asset;
		
		if ( GUI.changed )
			myObjectControl.build();
	}
	
	public override void OnInspectorGUI() 
	{
		myLabelStyle = new GUIStyle( GUI.skin.label );
		myLabelStyle.alignment = TextAnchor.MiddleRight;
		
		
		bool isMouseUp = false;
		Event curr_event = Event.current;
		if( curr_event.isMouse && curr_event.type == EventType.MouseUp  )
		{
			isMouseUp = true;
		}
		///////////////////////////////////////////////////////////////////////
		// Draw Game Object Controls
		
		myObjectControl.prShowObjectControls = 
			EditorGUILayout.Foldout( myObjectControl.prShowObjectControls, new GUIContent( "Object Controls" ) );
		
		if ( myObjectControl.prShowObjectControls ) 
		{
			EditorGUILayout.LabelField( new GUIContent( "OTL Path:" ) );
			EditorGUILayout.BeginHorizontal(); 
			{
				string old_asset_path = myObjectControl.getAssetPath();
				string new_asset_path = "";
				new_asset_path = EditorGUILayout.TextField( old_asset_path );
		        
		        if ( GUILayout.Button( "...", GUILayout.Width( myFileChooserButtonWidth ) ) ) 
				{
					string prompt_result_path = HAPI_GUIUtility.promptForOTLPath( old_asset_path );
					if ( prompt_result_path.Length > 0 )
						new_asset_path = prompt_result_path;
		        }
				
				myObjectControl.setAssetPath( new_asset_path );
			} 
			EditorGUILayout.EndHorizontal();
			
			if ( GUILayout.Button( "Rebuild" ) ) 
			{
				myObjectControl.prAssetPathChanged = true;
				myObjectControl.build();
			}
			
			// Draw Auto Select Asset Node Toggle
			EditorGUILayout.BeginHorizontal(); 
			{
				// Add padding for the toggle column.
				EditorGUILayout.LabelField( myNullContent, myToggleWidthGUI );
				// Add empty space to align with fields.
				EditorGUILayout.LabelField( myNullContent, myLabelWidthGUI );
				
				// Draw toggle with its label.
				bool old_value = myObjectControl.prAutoSelectAssetNode;
				myObjectControl.prAutoSelectAssetNode = EditorGUILayout.Toggle( old_value, myToggleWidthGUI );
				EditorGUILayout.SelectableLabel( "Auto Select Parent", myLineHeightGUI );		
			}
			EditorGUILayout.EndHorizontal();
			
			// Draw Logging Toggle
			EditorGUILayout.BeginHorizontal(); 
			{
				// Add padding for the toggle column.
				EditorGUILayout.LabelField( myNullContent, myToggleWidthGUI );
				// Add empty space to align with fields.
				EditorGUILayout.LabelField( myNullContent, myLabelWidthGUI );
				
				// Draw toggle with its label.
				bool old_value = myObjectControl.prEnableLogging;
				myObjectControl.prEnableLogging = EditorGUILayout.Toggle( old_value, myToggleWidthGUI );
				EditorGUILayout.SelectableLabel( "Enable Logging", myLineHeightGUI );		
			}
			EditorGUILayout.EndHorizontal();
			
		} // if
		
		if ( myObjectControl.prMaxInputCount > 0 || myObjectControl.prMaxGeoInputCount > 0 )
		{
			myObjectControl.prShowInputControls = 
				EditorGUILayout.Foldout( myObjectControl.prShowInputControls, new GUIContent( "Inputs" ) );
			
			if ( myObjectControl.prShowInputControls )
			{
				if ( myObjectControl.prAssetType == HAPI_AssetType.HAPI_ASSETTYPE_OBJ )
					for ( int ii = 0; ii < myObjectControl.prMaxInputCount; ++ii )
						setTransformInput( ii );
				
				for ( int ii = 0; ii < myObjectControl.prMaxGeoInputCount; ++ii )
				{
					GameObject old_object = myObjectControl.prUpStreamGeoObjects[ ii ];
					GameObject new_object = EditorGUILayout.ObjectField( "Geometry Input", old_object, 
																		 typeof( GameObject ), true ) as GameObject;
					
					if ( new_object != old_object )
					{
						myObjectControl.prUpStreamGeoObjects[ ii ] = new_object;
						if ( !new_object )
							myObjectControl.removeGeoInput( ii );
						else
						{
							HAPI_Asset asset_component = new_object.GetComponent< HAPI_Asset >();
							if ( asset_component )
								myObjectControl.addAssetAsGeoInput( asset_component, ii );
							else
								myObjectControl.addGeoAsGeoInput( new_object, ii );
						}
					}
					
					EditorGUILayout.LabelField( new GUIContent( "File Input " + ii + ":" ) );
					EditorGUILayout.BeginHorizontal(); 
					{
						string old_file_path = myObjectControl.prFileInputs[ ii ];
						string new_file_path = "";
						new_file_path = EditorGUILayout.TextField( old_file_path );
						
						if ( GUILayout.Button( "...", GUILayout.Width( myFileChooserButtonWidth ) ) ) 
						{
							string prompt_result_path = HAPI_GUIUtility.promptForFileInputPath( old_file_path );
							if ( prompt_result_path.Length > 0 )
								new_file_path = prompt_result_path;
						}
						
						myObjectControl.prFileInputs[ ii ] = new_file_path;
					} 
					EditorGUILayout.EndHorizontal();
					
				} // for
			} // if
		} // if
		
		///////////////////////////////////////////////////////////////////////
		// Draw Asset Controls
		
		EditorGUILayout.Separator();
		myObjectControl.prShowAssetControls = 
			EditorGUILayout.Foldout( myObjectControl.prShowAssetControls, new GUIContent( "Asset Controls" ) );
		
		bool hasAssetChanged = false;
		if ( myObjectControl.prShowAssetControls )
			hasAssetChanged |= generateAssetControls();
		
		if ( hasAssetChanged )
			myObjectControl.build();
		
		if ( isMouseUp )
		{
			try
			{
				int bufLength = 0;
				HAPI_Host.getPreset( myObjectControl.prAssetId, myObjectControl.prPreset, ref bufLength );
				
				myObjectControl.prPreset = new byte[bufLength];
				
				HAPI_Host.getPreset( myObjectControl.prAssetId, myObjectControl.prPreset, ref bufLength );
			}
			catch {} // Just catch them here but don't report them because we would just get a huge stream of errors.
		}
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
	
	private void setTransformInput( int index )
	{
		myObjectControl.prUpStreamTransformObjects[index] = EditorGUILayout.ObjectField( "Transform Input", 
						myObjectControl.prUpStreamTransformObjects[index], typeof(GameObject), true ) as GameObject;
												
		if( myObjectControl.prUpStreamTransformObjects[index] != null )
		{
			HAPI_Asset inputAsset = (HAPI_Asset) 
									myObjectControl.prUpStreamTransformObjects[index].GetComponent("HAPI_Asset");
			if( inputAsset != null )
			{
				
				myObjectControl.addAssetAsTransformInput( inputAsset, index );
			}
			else
			{
				myObjectControl.removeTransformInput( index );
			}
		}
		else
		{
			myObjectControl.removeTransformInput( index );
		}
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
	private bool generateAssetControl( 	int id, ref bool join_last, ref bool no_label_toggle_last ) 
	{
		if ( myObjectControl.prParms == null )
			return false;
		
		bool changed 				= false;
		
		int asset_id				= myObjectControl.prAssetId;
		
		HAPI_ParmInfo[] parms 		= myObjectControl.prParms;
		HAPI_ParmInfo parm			= parms[ id ];
		
		int[] parm_int_values		= myObjectControl.prParmIntValues;
		float[] parm_float_values	= myObjectControl.prParmFloatValues;
		int[] parm_string_values	= myObjectControl.prParmStringValues;
		
		HAPI_ParmType parm_type 	= (HAPI_ParmType) parm.type;
		int parm_size				= parm.size;
		
		int values_index = -1;
		if ( parm.isInt() )
		{
			if ( parm.intValuesIndex < 0 || parm_int_values == null )
				return false;
			values_index 			= parm.intValuesIndex;
		}
		else if ( parm.isFloat() )
		{
			if ( parm.floatValuesIndex < 0 || parm_float_values == null )
				return false;
			values_index			= parm.floatValuesIndex;
		}
		else if ( parms[ id ].isString() )
		{
			if ( parm.stringValuesIndex < 0 || parm_string_values == null )
				return false;
			values_index			= parm.stringValuesIndex;
		}
		
		GUIStyle slider_style 		= new GUIStyle( GUI.skin.horizontalSlider );
		GUIStyle slider_thumb_style	= new GUIStyle( GUI.skin.horizontalSliderThumb );
				
		if ( parms[ id ].invisible )
			return changed;
						
		// Decide whether to join with the previous parameter on the same 
		// line or not.
		if ( !join_last || parm_size > 1 )
			EditorGUILayout.BeginHorizontal();
		
		// Add label first if we're not a toggle.
		if ( parm_type != HAPI_ParmType.HAPI_PARMTYPE_TOGGLE
			&& parm_type != HAPI_ParmType.HAPI_PARMTYPE_FOLDER
			&& !parm.labelNone )
		{
			GUILayoutOption label_final_width = myLabelWidthGUI;
			if ( join_last && !no_label_toggle_last )
			{
				float min_width;
				float max_width;
				myLabelStyle.CalcMinMaxWidth( new GUIContent( parm.label ), out min_width, out max_width );
				label_final_width = GUILayout.Width( min_width );
			}
			else if ( !join_last )
			{
				// Add padding for the toggle column.
				EditorGUILayout.LabelField( "", myToggleWidthGUI );
			}
			EditorGUILayout.SelectableLabel( parm.label, myLabelStyle, label_final_width, myLineHeightGUI );
			no_label_toggle_last = false;
		}
		
		///////////////////////////////////////////////////////////////////////
		// Integer Parameter
		if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_INT )
		{
			if ( parm.choiceCount > 0 && parm.choiceIndex >= 0 )
			{
				// Draw popup (menu) field.
				List< string > 	labels = new List< string >();
				List< int>		values = new List< int >();
				
				// Go through our choices.
				for ( int i = 0; i < parm.choiceCount; ++i )
				{
					if ( myObjectControl.prParmChoiceLists[ parm.choiceIndex + i ].parentParmId != id )
						Debug.LogError( "Parm choice parent parm id (" 
										+ myObjectControl.prParmChoiceLists[ parm.choiceIndex + i ].parentParmId 
										+ ") not matching current parm id (" + id + ")!\n"
										+ "Choice index: " + ( parm.choiceIndex + i ) + ", "
										+ "Choice count: " + parm.choiceCount );
					
					labels.Add( myObjectControl.prParmChoiceLists[ parm.choiceIndex + i ].label );
					values.Add( i );
				}
				
				// Get old value.
				int old_value = parm_int_values[ values_index ];
				
				// Draw popup.
				int new_value = EditorGUILayout.IntPopup( old_value, labels.ToArray(), values.ToArray() );
				
				// Determine if value changed and update parameter value.
				if ( new_value != old_value )
				{
					parm_int_values[ values_index ] = new_value;
					changed |= true;
				}
			}
			else
			{
				int per_line = 0;
				for ( int p = 0; p < parm_size; ++p, ++per_line )
				{
					if ( per_line >= myMaxFieldCountPerLine )
					{
						EditorGUILayout.EndHorizontal();
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField( "", myToggleWidthGUI );
						EditorGUILayout.LabelField( "", myLabelWidthGUI );
						per_line = 0;
					}
					
					// Get old value.
					int old_value = parm_int_values[ values_index + p ];
					
					// Draw field.
					int new_value = EditorGUILayout.IntField( old_value );
					
					// Draw the slider.
					if ( parm_size == 1 && !join_last && !parm.joinNext )
					{
						float ui_min = ( parm.hasUIMin ? parm.UIMin : 0.0f );
						float ui_max = ( parm.hasUIMax ? parm.UIMax : 10.0f );
						Rect lastDoubleRect = getLastDoubleRect();
						slider_style.stretchWidth = false;
						slider_style.fixedWidth = lastDoubleRect.width;
						new_value = (int) GUI.HorizontalSlider( lastDoubleRect, new_value, ui_min, ui_max, 
																slider_style, slider_thumb_style );
					}
					
					// Enforce min/max bounds.
					if ( parm.hasMin && new_value < (int) parm.min )
						new_value = (int) parm.min;
					if ( parm.hasMax && new_value > (int) parm.max )
						new_value = (int) parm.max;
					
					// Determine if value changed and update parameter value.
					if ( new_value != old_value )
					{
						parm_int_values[ values_index + p ] = new_value;
						changed |= true;
					} // if
				} // for
			} // if parm.choiceCount
		} // if parm_type is INT
		///////////////////////////////////////////////////////////////////////
		// Float Parameter
		else if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_FLOAT )
		{
			int per_line = 0;
			for ( int p = 0; p < parm_size; ++p, ++per_line )
			{
				if ( per_line >= myMaxFieldCountPerLine )
				{
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField( "", myToggleWidthGUI );
					EditorGUILayout.LabelField( "", myLabelWidthGUI );
					per_line = 0;
				}
				
				// Get old value.
				float old_value = parm_float_values[ values_index + p ];
				
				// Draw field.
				float new_value = EditorGUILayout.FloatField( old_value );
				
				// Draw the slider.
				if ( parm_size == 1 && !join_last && !parm.joinNext )
				{
					float ui_min = ( parm.hasUIMin ? parm.UIMin : 0.0f );
					float ui_max = ( parm.hasUIMax ? parm.UIMax : 10.0f );
					Rect lastDoubleRect = getLastDoubleRect();
					slider_style.stretchWidth = false;
					slider_style.fixedWidth = lastDoubleRect.width;
					new_value = GUI.HorizontalSlider( lastDoubleRect, new_value, ui_min, ui_max, 
													  slider_style, slider_thumb_style );
				}
				
				// Enforce min/max bounds.
				if ( parm.hasMin && new_value < parm.min )
					new_value = parm.min;
				if ( parm.hasMax && new_value > parm.max )
					new_value = parm.max;
				
				// Determine if value changed and update parameter value.
				if ( new_value != old_value )
				{					
					parm_float_values[ values_index + p ] = new_value;
					changed |= true;
				} // if
			} // for
		} // if parm_type is FLOAT
		///////////////////////////////////////////////////////////////////////
		// String Parameter
		else if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_STRING )
		{			
			int per_line = 0;
			for ( int p = 0; p < parm_size; ++p, ++per_line )
			{
				if ( per_line >= myMaxFieldCountPerLine )
				{
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField( "", myToggleWidthGUI );
					EditorGUILayout.LabelField( "", myLabelWidthGUI );
					per_line = 0;
				}
				
				// Get old value.
				string old_value = HAPI_Host.getString( parm_string_values[ values_index + p ] );
				
				// Draw field.
				string new_value = EditorGUILayout.TextField( old_value );
				
				// Determine if value changed and update parameter value. 
				if ( new_value != old_value )
				{
					HAPI_Host.setParmStringValue( asset_id, new_value, id, p );
					changed |= true;
				}
			}
		}
		///////////////////////////////////////////////////////////////////////
		// File Field
		else if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_FILE )
		{
			string old_path = HAPI_Host.getString( parm_string_values[ values_index ] );
			string new_path = EditorGUILayout.TextField( old_path );
		        
	        if ( GUILayout.Button( "...", GUILayout.Width( myFileChooserButtonWidth ) ) ) 
			{
				string prompt_path = EditorUtility.OpenFilePanel( "Select File", old_path, "*" );;
				if ( prompt_path.Length > 0 )
					new_path = prompt_path;
	        }
			
			if ( new_path != old_path )
			{
				HAPI_Host.setParmStringValue( asset_id, new_path, id, 0 );
				changed |= true;	
			}
		}
		///////////////////////////////////////////////////////////////////////
		// Toggle Parameter
		else if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_TOGGLE )
		{
			if ( !parm.joinNext )
			{
				// Add padding for the toggle column.
				EditorGUILayout.LabelField( myNullContent, myToggleWidthGUI );
				// Add empty space to align with fields.
				EditorGUILayout.LabelField( myNullContent, myLabelWidthGUI );
			}
			
			// Get old value.
			int old_value = parm_int_values[ values_index ];
			
			// Draw toggle with its label.
			bool toggle_result = EditorGUILayout.Toggle( old_value != 0, myToggleWidthGUI );
			int new_value = ( toggle_result ? 1 : 0 );
			if ( !parms[ id ].labelNone )
				EditorGUILayout.SelectableLabel( parms[ id ].label, myLineHeightGUI );
			else
				no_label_toggle_last = true;
			
			// Determine if value changed and update parameter value.
			if ( new_value != old_value )
			{
				parm_int_values[ values_index ] = new_value;
				changed |= true;
			}
		}		
		///////////////////////////////////////////////////////////////////////
		// Color Parameter
		else if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_COLOUR )
		{
			Color old_color = new Color( parm_float_values[ values_index + 0 ], 
									 	 parm_float_values[ values_index + 1 ], 
									 	 parm_float_values[ values_index + 2 ] );
			if ( parm_size > 3 )
				old_color.a = parm_float_values[ values_index + 3 ];
			
			// Draw control.
			Color new_color = EditorGUILayout.ColorField( old_color );
			
			// Determine if value changed and update parameter value.
			if ( new_color != old_color )
			{
				parm_float_values[ values_index + 0 ] = new_color.r;
				parm_float_values[ values_index + 1 ] = new_color.g;
				parm_float_values[ values_index + 2 ] = new_color.b;
				
				if ( parm_size > 3 )
					parm_float_values[ values_index + 3 ] = new_color.a;
			
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
		join_last = ( parm.joinNext && parm_size <= 1 );
		if ( !parm.joinNext || parm_size > 1 )
			EditorGUILayout.EndHorizontal();
		
		if ( myObjectControl.hasProgressBarBeenUsed() && id == myObjectControl.prLastChangedParmId )
		{
			// TODO: Set the focus back to this control since the progress bar would have stolen it.	
		}
		
		if ( changed )
		{
			myObjectControl.prLastChangedParmId = id;
		
			if ( parm.isInt() )
			{
				int[] temp_int_values = new int[ parm_size ];
				for ( int p = 0; p < parm_size; ++p )
					temp_int_values[ p ] = parm_int_values[ values_index + p ];
				HAPI_Host.setParmIntValues( asset_id, temp_int_values, values_index, parm_size );
			}
			else if ( parm.isFloat() )
			{
				float[] temp_float_values = new float[ parm_size ];
				for ( int p = 0; p < parm_size; ++p )
					temp_float_values[ p ] = parm_float_values[ values_index + p ];
				HAPI_Host.setParmFloatValues( asset_id, temp_float_values, values_index, parm_size );
			}
			
			// Note: String parameters update their values themselves so no need to do anything here.
		}
		
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
		if ( myObjectControl.prParms == null )
			return false;
		
		bool changed 					= false;
		int current_index 				= 0;
		HAPI_ParmInfo[] parms 			= myObjectControl.prParms;
				
		bool join_last 					= false;
		bool no_label_toggle_last 		= false;
		
		int folder_list_count 			= 0;
		
		// These stacks maintain the current folder depth, parent id, and how many more child 
		// parameters are still contained in the current folder.
		Stack< int > parent_id_stack 		= new Stack< int >();
		Stack< int > parent_count_stack 	= new Stack< int >();
		
		
		// Loop through all the parameters.
		while ( current_index < myObjectControl.prParmCount )
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
				// The current parameter is a folder list which means the next parms[ current_index ].size
				// parameters will be folders belonging to this folder list. Push to the stack a new
				// folder depth, ready to eat the next few parameters belonging to the folder list's 
				// selected folder.
				
				bool folder_list_invisible	= parms[ current_index ].invisible;
				int folder_count 			= parms[ current_index ].size;
				int first_folder_index 		= current_index + 1;
				int last_folder_index 		= current_index + folder_count;
				
				// Generate the list of folders which will be passed to the GUILayout.Toolbar() method.
				List< int > 	tab_ids 	= new List< int >();
				List< string > 	tab_labels 	= new List< string >();
				List< int > 	tab_sizes 	= new List< int >();
				bool has_visible_folders	= false;
				for ( current_index = first_folder_index; current_index <= last_folder_index; ++current_index )
				{
					if ( parms[ current_index ].type != (int) HAPI_ParmType.HAPI_PARMTYPE_FOLDER )
					{
						Debug.LogError( "We should be iterating through folders only here!"
							+ "\nCurrent Index: " + current_index + ", folder_count: " + folder_count );
					}
					
					// Don't add this folder if it's invisible.
					if ( parms[ current_index ].invisible || folder_list_invisible )
						continue;
					else
						has_visible_folders = true;
					
					tab_ids.Add( 		parms[ current_index ].id );
					tab_labels.Add( 	parms[ current_index ].label );
					tab_sizes.Add( 		parms[ current_index ].size );
				}
				current_index--; // We decrement the current_index as we incremented one too many in the for loop.
				
				// If there are no folders visible in this folder list, don't even append the folder stacks.
				if ( has_visible_folders )
				{
					folder_list_count++;
					
					// If myObjectControl.myFolderListSelections is smaller than our current depth it means this
					// is the first GUI generation for this asset (no previous folder selection data) so
					// increase the size of the selection arrays to accomodate the new depth.
					if ( myObjectControl.prFolderListSelections.Count <= folder_list_count )
					{
						myObjectControl.prFolderListSelections.Add( 0 );
						myObjectControl.prFolderListSelectionIds.Add( -1 );
					}
					
					int selected_folder 	= myObjectControl.prFolderListSelections[ folder_list_count ];
					selected_folder 		= GUILayout.Toolbar( selected_folder, tab_labels.ToArray() );
					myObjectControl.prFolderListSelections[ folder_list_count ] = selected_folder;
					
					// Push only the selected folder info to the parent stacks since for this depth and this folder
					// list only the parameters of the selected folder need to be generated.
					parent_id_stack.Push( 		tab_ids[ selected_folder ] );
					parent_count_stack.Push( 	tab_sizes[ selected_folder ] );
				}
			}
			else
			{
				// The current parameter is a simple parameter so just draw it.
				
				if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_FOLDER )
					Debug.LogError( "All folders should have been parsed in the folder list if clause!" );
				
				changed |= generateAssetControl( current_index, ref join_last, ref no_label_toggle_last );
			}
			
			current_index++;
		}
				
		return changed;
	}
		
	private const float			myMaxFieldCountPerLine		= 4;
	
	private const float 		myFileChooserButtonWidth 	= 30;
	private const float 		myLineHeight 				= 16;
	private const float 		myLabelWidth 				= 120;
	private const float 		myToggleWidth 				= 12;
	private const float			myDummyLabelMinWidth		= 20;
	
	private const float 		myDefaultUIMin 				= 0.0f;
	private const float 		myDefaultUIMax 				= 10.0f;
	
	private GUILayoutOption 	myLineHeightGUI 			= GUILayout.Height( myLineHeight );
	private GUILayoutOption 	myLabelWidthGUI 			= GUILayout.Width( myLabelWidth );
	private GUILayoutOption 	myToggleWidthGUI 			= GUILayout.Width( myToggleWidth );
	private GUILayoutOption		myDummyLabelMinWidthGUI		= GUILayout.MinWidth( myDummyLabelMinWidth );
	
	private GUIContent 			myNullContent 				= new GUIContent( "" );
	
	private GUIStyle			myLabelStyle;
	
	private HAPI_Asset 			myObjectControl;
}
