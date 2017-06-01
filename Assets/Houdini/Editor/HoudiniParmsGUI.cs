/*
* Copyright (c) <2017> Side Effects Software Inc.
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*
* Produced by:
*      Side Effects Software Inc
*      123 Front Street West, Suite 1401
*      Toronto, Ontario
*      Canada   M5J 2M2
*      416-504-9876
*
*/

// Master control for enabling runtime.
#if ( UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_LINUX )
	#define HAPI_ENABLE_RUNTIME
#endif

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[ CustomEditor( typeof( HoudiniParms ) ) ]
public class HoudiniParmsGUI : Editor
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	public void OnEnable()
	{
		myParms = target as HoudiniParms;
		myAsset = myParms.prControl.prAsset;

		myParmChanges		= true;
		myUnbuiltChanges 	= false;
		myFocusChanged 		= true;

		HoudiniHost.myRepaintDelegate += this.refresh;
		HoudiniHost.myDeselectionDelegate += this.deselect;
		HoudiniHost.myPlaymodeStateChangeDelegate += this.playmodeStateChange;
		HoudiniHost.mySelectionTarget = myParms.gameObject;
	}

	public void OnDisable()
	{
		HoudiniHost.myRepaintDelegate -= this.refresh;
		HoudiniHost.myDeselectionDelegate -= this.deselect;
		HoudiniHost.myPlaymodeStateChangeDelegate -= this.playmodeStateChange;
		HoudiniHost.mySelectionTarget = null;

		myParmChanges = false;
		applyChanges( true, true );
	}

	public virtual void refresh()
	{
		Repaint();
		SceneView.lastActiveSceneView.Repaint();
	}

	public virtual void deselect()
	{
		if ( HoudiniHost.mySelectionTarget == myParms.gameObject )
			HoudiniHost.mySelectionTarget = null;
	}

	public void playmodeStateChange()
	{
		applyChanges( true, true );
	}

	public void applyChanges( bool commit_changes, bool record_undo )
	{
		if ( ( ( myParmChanges && !myDelayBuild ) || 
				( myUnbuiltChanges && ( commit_changes || myFocusChanged ) ) ) )
		{
			if ( record_undo )
			{
				string changed_parm_name = "Parameter Change";
				if ( myParms.prLastChangedParmId != HoudiniConstants.HAPI_INVALID_PARM_ID )
					changed_parm_name = myParms.findParmStrings( myParms.prLastChangedParmId ).label;

				Undo.RecordObject( myParms.prParmsUndoInfo, changed_parm_name );
			}

			myParms.prControl.onParmChange();
	
			myUnbuiltChanges	= false;
			myParmChanges		= false;
		}
		else if ( myParmChanges )
			myUnbuiltChanges = true;
	}
	
	public override void OnInspectorGUI() 
	{
		try
		{
			myDelayBuild = false;
			myParmChanges = false;
			myFocusChanged = false;

			Event current_event = Event.current;
			bool commit_changes = false;
			if ( current_event.isKey && current_event.type == EventType.KeyUp && current_event.keyCode == KeyCode.Return )
				commit_changes = true;
			else if (
				current_event.type == EventType.ValidateCommand && 
				current_event.commandName == "UndoRedoPerformed" )
			{
				performUndo();
			}

			///////////////////////////////////////////////////////////////////////
			// Draw Asset Controls

			bool is_editable = myParms.prEditable && HoudiniHost.isInstallationOk();

			// We can only build or do anything if we can link to our libraries.
#if !( HAPI_ENABLE_RUNTIME )
			is_editable = false;
			HoudiniGUI.help( HoudiniConstants.HAPI_UNSUPPORTED_PLATFORM_MSG, MessageType.Info );
#else
			if ( !myParms.prEditable )
				HoudiniGUI.help( "The parameters on this node are readonly.", MessageType.Info );
			else if ( !HoudiniHost.isInstallationOk() )
				HoudiniGUI.help( HoudiniHost.getMissingEngineInstallHelpString(), MessageType.Info );
#endif // !( HAPI_ENABLE_RUNTIME )

			bool gui_enable = GUI.enabled;
			GUI.enabled = is_editable;

			myParmChanges |= generateAssetControls();

			GUI.enabled = gui_enable;

			///////////////////////////////////////////////////////////////////////
			// Apply Changes

			// Only record undo info object if parameters are not being changed
			// due to an undo/redo event.
			bool record_undo = current_event.commandName != "UndoRedoPerformed";
			applyChanges( commit_changes, record_undo );
		}
		catch ( HoudiniErrorIgnorable ) {}
		catch ( HoudiniError error )
		{
			Debug.LogError( error.ToString() + "\nSource: " + error.Source );
		}
		catch ( System.Exception error )
		{
			Debug.LogError( error.ToString() + "\nSource: " + error.Source );
		}
	}

	public virtual void OnSceneGUI()
	{
		Event current_event = Event.current;
		if (
				current_event.type == EventType.ValidateCommand && 
				current_event.commandName == "UndoRedoPerformed" )
		{
			performUndo();
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private
	
	private void performUndo()
	{
		HoudiniParmsUndoInfo undo_info =
			ScriptableObject.Instantiate( myParms.prParmsUndoInfo ) as HoudiniParmsUndoInfo;

		// First find all multiparms and add/remove instances as necessary.
		foreach ( HAPI_ParmInfo parm in myParms.prParms )
		{
			if (
				parm.type == HAPI_ParmType.HAPI_PARMTYPE_MULTIPARMLIST &&
				undo_info.parmNames.Contains( parm.name ) )
			{
				// Get value of multiparm from undo info.
				int new_value_index = undo_info.parmIndices[ undo_info.parmNames.IndexOf( parm.name ) ];
				int new_value = undo_info.parmIntValues[ new_value_index ];

				// get current value of multiparm
				int current_value = myParms.prParmIntValues[ parm.intValuesIndex ];

				// Add/Remove number of instances from current parameters to match number
				// of parameters from undo info.
				int difference = new_value - current_value;
				if ( difference > 0 )
					myParms.appendMultiparmInstances( parm, difference );
				else if ( difference < 0 )
					myParms.removeMultiparmInstances( parm, -difference );
				else
					continue;

				myParms.getParameterValues();
				myParmChanges = true;
			}
		}

		// Next loop through all parameters and copy changed values over from undo info.
		foreach ( HAPI_ParmInfo parm in myParms.prParms )
		{
			if ( !undo_info.parmNames.Contains( parm.name ) )
				continue;

			if ( parm.isInt() )
			{
				int new_value_index = undo_info.parmIndices[ undo_info.parmNames.IndexOf( parm.name ) ];

				bool is_different = false;
				for ( int idx = 0; idx < parm.size; ++idx )
					is_different |=
						undo_info.parmIntValues[ new_value_index + idx ] !=
							myParms.prParmIntValues[ parm.intValuesIndex + idx ];

				if ( is_different && parm.type != HAPI_ParmType.HAPI_PARMTYPE_MULTIPARMLIST )
				{
					myParmChanges = true;

					int[] values = new int[ parm.size ];
					Array.Copy( undo_info.parmIntValues, new_value_index, values, 0, parm.size );
					HoudiniHost.setParmIntValues(
						myParms.prControl.prNodeId, values, 
						parm.intValuesIndex, parm.size );
				}
			}
			else if ( parm.isFloat() )
			{
				int new_value_index = undo_info.parmIndices[ undo_info.parmNames.IndexOf( parm.name ) ];

				bool is_different = false;
				for ( int idx = 0; idx < parm.size; ++idx )
					is_different |=
						undo_info.parmFloatValues[ new_value_index + idx ] !=
							myParms.prParmFloatValues[ parm.floatValuesIndex + idx ];

				if ( is_different )
				{
					myParmChanges = true;

					float[] values = new float[ parm.size ];
					Array.Copy( undo_info.parmFloatValues, new_value_index, values, 0, parm.size );
					HoudiniHost.setParmFloatValues(
						myParms.prControl.prNodeId, values, 
						parm.floatValuesIndex, parm.size );
				}
			}
			else if ( parm.isString() )
			{
				string[] current_string_values = new string[ parm.size ];
				current_string_values = myParms.getParmStrings( parm );
						
				for ( int i = 0; i < parm.size; i++ )
				{
					int new_value_index = undo_info.parmIndices[ undo_info.parmNames.IndexOf( parm.name ) ];
					string new_string_value = undo_info.parmStringValues[ new_value_index + i ];
					string current_string_value = current_string_values[ i ];

					if ( string.Compare( current_string_value, new_string_value ) != 0 )
					{
						myParmChanges = true;

						HoudiniHost.setParmStringValue(
							myParms.prControl.prNodeId, 
							new_string_value, parm.id, i );
					}
				}
			}
			else if ( parm.isNode() )
			{
				// TODO
			} // By type of parm.
		} // For all parms.
	}

	protected bool generateAssetControl( int index, ref bool join_last, ref bool no_label_toggle_last )
	{
		if ( myParms.prParms == null )
			return false;
		
		if ( myParms.prParms[ index ].invisible )
			return false;

		bool changed = false;
		
		HAPI_ParmInfo[] parms = myParms.prParms;
		HAPI_ParmInfo parm = parms[ index ];
		HAPI_ParmInfoStrings parm_strings = myParms.prParmInfoStrings[ index ];

		int[] parm_int_values = myParms.prParmIntValues;
		float[] parm_float_values = myParms.prParmFloatValues;

		HoudiniGUIParm gui_parm = new HoudiniGUIParm( parm, parm_strings );

		///////////////////////////////////////////////////////////////////////
		// Integer Parameter
		if ( parm.type == HAPI_ParmType.HAPI_PARMTYPE_MULTIPARMLIST )
		{
			changed = HoudiniGUI.multiparmField(
				ref gui_parm, ref myDelayBuild, ref parm_int_values,
				ref join_last, ref no_label_toggle_last );
		}
		else if ( parm.type == HAPI_ParmType.HAPI_PARMTYPE_INT )
		{
			if ( parm.choiceCount > 0 && parm.choiceIndex >= 0 )
			{
				// Draw popup (menu) field.
				List< string > 	labels = new List< string >();
				List< int >		values = new List< int >();
				
				// Go through our choices.
				for ( int i = 0; i < parm.choiceCount; ++i )
				{
					if ( myParms.prParmChoiceLists[ parm.choiceIndex + i ].parentParmId != parm.id )
						Debug.LogError( "Parm choice parent parm id (" 
										+ myParms.prParmChoiceLists[ parm.choiceIndex + i ].parentParmId 
										+ ") not matching current parm id (" + parm.id + ")!\n"
										+ "Choice index: " + ( parm.choiceIndex + i ) + ", "
										+ "Choice count: " + parm.choiceCount );
					
					labels.Add( myParms.prParmChoiceInfoStrings[ parm.choiceIndex + i ].label );
					values.Add( i );
				}
				
				changed = HoudiniGUI.dropdown(
					ref gui_parm, ref parm_int_values,
					labels.ToArray(), values.ToArray(),
					ref join_last, ref no_label_toggle_last,
					null, ref parm_int_values );
			}
			else
			{
				changed = HoudiniGUI.intField(
					ref gui_parm, ref myDelayBuild, ref parm_int_values,
					ref join_last, ref no_label_toggle_last, null, 
					ref parm_int_values );
			} // if parm.choiceCount
		} // if parm.type is INT
		///////////////////////////////////////////////////////////////////////
		// Float Parameter
		else if ( parm.type == HAPI_ParmType.HAPI_PARMTYPE_FLOAT )
		{
			changed = HoudiniGUI.floatField(
				ref gui_parm, ref myDelayBuild, ref parm_float_values, 
				ref join_last, ref no_label_toggle_last, null, 
				ref parm_float_values );
		} // if parm.type is FLOAT
		///////////////////////////////////////////////////////////////////////
		// String Parameter
		else if ( parm.type == HAPI_ParmType.HAPI_PARMTYPE_STRING )
		{
			if ( parm.choiceCount > 0 && parm.choiceIndex >= 0 )
			{
				// Draw popup (menu) field.
				List< string > labels = new List< string >();
				List< string > values = new List< string >();
				
				// Go through our choices.
				for ( int i = 0; i < parm.choiceCount; ++i )
				{
					if ( myParms.prParmChoiceLists[ parm.choiceIndex + i ].parentParmId != parm.id )
						Debug.LogError(
							"Parm choice parent parm id ("
							+ myParms.prParmChoiceLists[ parm.choiceIndex + i ].parentParmId
							+ ") not matching current parm id (" + parm.id + ")!\n"
							+ "Choice index: " + ( parm.choiceIndex + i ) + ", "
							+ "Choice count: " + parm.choiceCount );
					
					labels.Add( myParms.prParmChoiceInfoStrings[ parm.choiceIndex + i ].label );
					values.Add( myParms.prParmChoiceInfoStrings[ parm.choiceIndex + i ].value );
				}
				
				string[] values_temp = myParms.getParmStrings( parm );
				gui_parm.valuesIndex = 0; // Since we're piping a de-handled temp array.

				changed = HoudiniGUI.dropdown(
					ref gui_parm, ref values_temp,
					labels.ToArray(), values.ToArray(),
					ref join_last, ref no_label_toggle_last,
					null, ref values_temp );

				if ( changed ) 
					myParms.setParmStrings( parm, values_temp );
			}
			else
			{
				string[] values = myParms.getParmStrings( parm ); 
			
				// The given string array is only for this parm so we need to set the values index to 0.
				gui_parm.valuesIndex = 0;
			
				changed = HoudiniGUI.stringField(
					ref gui_parm, ref myDelayBuild, ref values,
					ref join_last, ref no_label_toggle_last, null,
					ref values );
			
				// Set the to be changed strings into the cache
				if ( changed )
					myParms.setParmStrings( parm, values );
			}
		}
		///////////////////////////////////////////////////////////////////////
		// File Path Field
		else if ( parm.isPath() )
		{
			string[] path = myParms.getParmStrings( parm );

			if ( parm.type == HAPI_ParmType.HAPI_PARMTYPE_PATH_FILE_GEO )
				gui_parm.label += " (geo)";
			else if ( parm.type == HAPI_ParmType.HAPI_PARMTYPE_PATH_FILE_IMAGE )
				gui_parm.label += " (image)";

			// Asset ID might still be invalid here so don't fail if it is.
			// It is invalid if the asset didn't cook yet after a scene load.
			try
			{
				string file_pattern = parm.typeInfo;
				if ( file_pattern != "" )
					gui_parm.label += " (" + file_pattern + ")";
			}
			catch {}

			HAPI_Permissions permissions = parm.permissions;
			if ( permissions == HAPI_Permissions.HAPI_PERMISSIONS_READ_ONLY )
			{
				gui_parm.label += " (read-only)";
				changed = HoudiniGUI.fileOpenField(
					ref gui_parm, ref myDelayBuild, ref path[ 0 ],
					ref join_last, ref no_label_toggle_last );
			}
			else
			{
				if ( permissions == HAPI_Permissions.HAPI_PERMISSIONS_WRITE_ONLY )
					gui_parm.label += " (write-only)";

				changed = HoudiniGUI.fileSaveField(
					ref gui_parm, ref myDelayBuild, ref path[ 0 ],
					ref join_last, ref no_label_toggle_last );
			}

			if ( changed )
				myParms.setParmStrings( parm, path );
		}
		///////////////////////////////////////////////////////////////////////
		// Node Path Field
		else if ( parm.isNode() )
		{
			// Asset ID might still be invalid here so don't fail if it is.
			// It is invalid if the asset didn't cook yet after a scene load.
			try
			{
				string op_filter = parm.typeInfo;
				if ( op_filter != "" )
					gui_parm.label += " (" + op_filter + ")";
			}
			catch {}

			HAPI_ParmInput parm_input = myParms.prParmInputs[ parm.id ];
			UnityEngine.Object temp_object = parm_input.inputObject;
			GameObject undo_object = null;
			changed = HoudiniGUI.objectField(
				ref gui_parm, ref temp_object, typeof( GameObject ),
				ref join_last, ref no_label_toggle_last, null, ref undo_object );

			if ( changed )
			{
				parm_input.newInputObject = (GameObject) temp_object;
				myParms.prParmInputs[ index ] = parm_input;
			}
		}
		///////////////////////////////////////////////////////////////////////
		// Toggle Parameter
		else if ( parm.type == HAPI_ParmType.HAPI_PARMTYPE_TOGGLE )
		{
			changed = HoudiniGUI.toggle(
				ref gui_parm, ref parm_int_values,
				ref join_last, ref no_label_toggle_last,
				null, ref parm_int_values );
		}
		///////////////////////////////////////////////////////////////////////
		// Color Parameter
		else if ( parm.type == HAPI_ParmType.HAPI_PARMTYPE_COLOR )
		{
			changed = HoudiniGUI.colourField(
				ref gui_parm, ref myDelayBuild, ref parm_float_values,
				ref join_last, ref no_label_toggle_last, null,
				ref parm_float_values );
		}
		///////////////////////////////////////////////////////////////////////
		// Button Parameter
		else if ( parm.type == HAPI_ParmType.HAPI_PARMTYPE_BUTTON )
		{
			if ( parm.choiceCount > 0 && parm.choiceIndex >= 0 )
			{
				// Draw popup (menu) field.
				List< string > 	labels = new List< string >();
				List< int>		values = new List< int >();
				
				// Go through our choices.
				for ( int i = 0; i < parm.choiceCount; ++i )
				{
					if ( myParms.prParmChoiceLists[ parm.choiceIndex + i ].parentParmId != parm.id )
						Debug.LogError(
							"Parm choice parent parm id (" 
							+ myParms.prParmChoiceLists[ parm.choiceIndex + i ].parentParmId 
							+ ") not matching current parm id (" + parm.id + ")!\n"
							+ "Choice index: " + ( parm.choiceIndex + i ) + ", "
							+ "Choice count: " + parm.choiceCount );
					
					labels.Add( myParms.prParmChoiceInfoStrings[ parm.choiceIndex + i ].label );
					values.Add( i );
				}
				
				changed = HoudiniGUI.dropdown(
					ref gui_parm, ref parm_int_values,
					labels.ToArray(), values.ToArray(),
					ref join_last, ref no_label_toggle_last,
					null, ref parm_int_values );
			}
			else
			{
				changed = HoudiniGUI.button( ref gui_parm, ref join_last, ref no_label_toggle_last );
			}
		}
		///////////////////////////////////////////////////////////////////////
		// Label
		else if ( parm.type == HAPI_ParmType.HAPI_PARMTYPE_LABEL )
		{
			HoudiniGUI.label( ref gui_parm, ref join_last, ref no_label_toggle_last );
		}
		///////////////////////////////////////////////////////////////////////
		// Separator
		else if ( parm.type == HAPI_ParmType.HAPI_PARMTYPE_SEPARATOR )
		{
			HoudiniGUI.separator();
		}
		
		if ( myAsset.hasProgressBarBeenUsed() && parm.id == myParms.prLastChangedParmId )
		{
			// TODO: Set the focus back to this control since the progress bar would have stolen it.	
		}


		if ( changed )
			myParms.prLastChangedParmId = parm.id;
		
		return changed;
	}
	
	protected bool generateAssetControls() 
	{
		if ( myParms.prParms == null )
			return false;
		
		bool changed 					= false;
		int current_index 				= 0;
		HAPI_ParmInfo[] parms 			= myParms.prParms;
		HAPI_ParmInfoStrings[] parm_strings = myParms.prParmInfoStrings;
				
		bool join_last 					= false;
		bool no_label_toggle_last 		= false;
		
		int folder_list_count 			= 0;
		
		// These stacks maintain the current folder depth, parent id, and how many more child 
		// parameters are still contained in the current folder.
		Stack< int > parent_id_stack 		= new Stack< int >();
		Stack< int > parent_count_stack 	= new Stack< int >();
		int instance_length = -1;
		
		// Loop through all the parameters.
		while ( current_index < myParms.prParmCount )
		{
			int current_parent_id = HoudiniConstants.HAPI_INVALID_PARM_ID; // The root has parent id -1.
			
			// If we're not at the root (empty parent stack), get the current parent id and parent 
			// count from the stack as well as decrement the parent count as we're about to parse 
			// another parameter.
			if ( parent_id_stack.Count != 0 )
			{
				current_parent_id = parent_id_stack.Peek();

				// Get the leftover parameter count of the current folder.
				int current_parent_count = parent_count_stack.Peek();

				// The only reason current_parent_count would be 0 at this point is if we start off
				// with an empty folder. Pop it and move on, but do not increment the current_index!
				if ( current_parent_count <= 0 )
				{
					parent_id_stack.Pop();
					parent_count_stack.Pop();
					continue;
				}

				// If the current parameter, whatever it may be, does not belong to the current active
				// parent then skip it. This check has to be done here because we do not want to
				// increment the top of the parent_count_stack as if we included a parameter in the
				// current folder.
				if ( parms[ current_index ].parentId != current_parent_id )
				{
					current_index++;
					continue;
				}

				// Decrement the leftover parameter count.
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

			if ( parms[ current_index ].type == HAPI_ParmType.HAPI_PARMTYPE_MULTIPARMLIST )
			{
				changed |= generateAssetControl( current_index, ref join_last, ref no_label_toggle_last );

				//int instance_count = myParms.prParmIntValues[ parms[ current_index ].intValuesIndex ];
				int instance_count = parms[ current_index ].instanceCount;
				instance_length = parms[ current_index ].instanceLength;
				if ( instance_length > 0 )
				{
					for (int i = 0; i < instance_count; i++)
					{
						parent_id_stack.Push( parms[ current_index ].id );
						parent_count_stack.Push( 1 );
					}
				}
				current_index++;
			}
			else if ( parms[ current_index ].type == HAPI_ParmType.HAPI_PARMTYPE_FOLDERLIST )
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
					if ( parms[ current_index ].type != HAPI_ParmType.HAPI_PARMTYPE_FOLDER )
					{
						Debug.LogError( "We should be iterating through folders only here!"
							+ "\nCurrent Index: " + current_index + ", folder_count: " + folder_count );
					}
					
					// Don't add this folder if it's invisible.
					if ( parms[ current_index ].invisible || folder_list_invisible )
						continue;
					else
						has_visible_folders = true;
					
					tab_ids.Add( parms[ current_index ].id );
					tab_labels.Add( parm_strings[ current_index ].label );
					tab_sizes.Add( parms[ current_index ].size );
				}
				current_index--; // We decrement the current_index as we incremented one too many in the for loop.
				
				// If there are no folders visible in this folder list, don't even append the folder stacks.
				if ( has_visible_folders )
				{
					folder_list_count++;
					
					// If myObjectControl.myFolderListSelections is smaller than our current depth it means this
					// is the first GUI generation for this asset (no previous folder selection data) so
					// increase the size of the selection arrays to accomodate the new depth.
					if ( myParms.prFolderListSelections.Count <= folder_list_count )
					{
						myParms.prFolderListSelections.Add( 0 );
						myParms.prFolderListSelectionIds.Add( HoudiniConstants.HAPI_INVALID_PARM_ID );
					}
					
					int selected_folder = myParms.prFolderListSelections[ folder_list_count ];
					selected_folder = Mathf.Clamp( selected_folder, 0, tab_labels.Count - 1 );
					selected_folder = GUILayout.Toolbar( selected_folder, tab_labels.ToArray() );
					myParms.prFolderListSelections[ folder_list_count ] = selected_folder;
					
					// Push only the selected folder info to the parent stacks since for this depth and this folder
					// list only the parameters of the selected folder need to be generated.
					parent_id_stack.Push( tab_ids[ selected_folder ] );
					parent_count_stack.Push( tab_sizes[ selected_folder ] );
				}
				current_index++;
			}
			else if ( parms[ current_index ].isChildOfMultiParm )
			{
				GUILayout.BeginHorizontal();

				// Create the add / remove buttons
				GUILayout.BeginHorizontal();
				if ( GUILayout.Button( "X" ) ) 
				{
					myParms.removeMultiparmInstance( parms[ current_index ] );
					changed = true;
				}
				if ( GUILayout.Button( "+" ) )
				{
					myParms.insertMultiparmInstance( parms[ current_index ] );
					changed = true;
				}
				GUILayout.EndHorizontal();

				// Create the parms within the multiparm instance
				GUILayout.BeginVertical();
				for ( int i = 0; i < instance_length; i++ )
				{
					changed |= generateAssetControl( current_index, ref join_last, ref no_label_toggle_last );
					current_index++;
				}
				GUILayout.EndVertical();

				GUILayout.EndHorizontal();
				if ( instance_length > 1 )
					HoudiniGUI.separator();
			}
			else
			{
				// The current parameter is a simple parameter so just draw it.
				if ( parms[ current_index ].type == HAPI_ParmType.HAPI_PARMTYPE_FOLDER )
					Debug.LogError( "All folders should have been parsed in the folder list if clause!" );

				changed |= generateAssetControl( current_index, ref join_last, ref no_label_toggle_last );

				current_index++;
			}
		}

		if ( myLastFocusedControl != GUI.GetNameOfFocusedControl() )
		{
			//Debug.Log( "Changed focus from " + myLastFocusedControl + " to " + GUI.GetNameOfFocusedControl() );
			// We changed focus. Signal a build
			myLastFocusedControl = GUI.GetNameOfFocusedControl();
			myFocusChanged = true;
			myDelayBuild = false;
		}

		return changed;
	}
	
	private HoudiniParms myParms;
	private HoudiniAsset myAsset;

	private bool myDelayBuild;
	private bool myParmChanges;
	private bool myUnbuiltChanges;
	private bool myFocusChanged;

	private string myLastFocusedControl;
}
