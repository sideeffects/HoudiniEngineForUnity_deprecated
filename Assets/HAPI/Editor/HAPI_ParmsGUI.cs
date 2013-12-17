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
using System;
using System.Collections;
using System.Collections.Generic;
using HAPI;

[ CustomEditor( typeof( HAPI_Parms ) ) ]
public class HAPI_ParmsGUI : Editor
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	public void OnEnable()
	{
		myParms = target as HAPI_Parms;
		myAsset = myParms.prControl.prAsset;

		myParmChanges		= true;
		myUnbuiltChanges 	= false;
		myFocusChanged 		= true;

		HAPI_Host.myRepaintDelegate += this.refresh;
		HAPI_Host.myDeselectionDelegate += this.deselect;
		HAPI_Host.mySelectionTarget = myParms.gameObject;
	}

	public void OnDisable()
	{
		HAPI_Host.myRepaintDelegate -= this.refresh;
		HAPI_Host.myDeselectionDelegate -= this.deselect;
		HAPI_Host.mySelectionTarget = null;
	}

	public virtual void refresh()
	{
		Repaint();
		SceneView.lastActiveSceneView.Repaint();
	}

	public virtual void deselect()
	{
		if ( HAPI_Host.mySelectionTarget == myParms.gameObject )
			HAPI_Host.mySelectionTarget = null;
	}
	
	public override void OnInspectorGUI() 
	{
		try
		{
			myDelayBuild = false;
			myParmChanges = false;
			myFocusChanged = false;

			Event curr_event = Event.current;
			bool commitChanges = false;
			if ( curr_event.isKey && curr_event.type == EventType.KeyUp && curr_event.keyCode == KeyCode.Return )
				commitChanges = true;
			else if ( curr_event.type == EventType.ValidateCommand && 
			          curr_event.commandName == "UndoRedoPerformed" )
			{
				HAPI_ParmsUndoInfo undo_info = ScriptableObject.Instantiate( myParms.prParmsUndoInfo ) as HAPI_ParmsUndoInfo;
				bool update_prefab_instance = myAsset.isPrefab() && myParms.gameObject.GetComponent< HAPI_Asset >() != null;
						
						// First find all multiparms and add/remove instances as necessary
				foreach ( HAPI_ParmInfo parm in myParms.prParms )
				{
					if ( parm.type == HAPI_ParmType.HAPI_PARMTYPE_MULTIPARMLIST &&
					     undo_info.parmNames.Contains( parm.name ) )
					{
						// get value of multiparm from undo info
						int new_value_index = undo_info.parmIndices[ undo_info.parmNames.IndexOf( parm.name ) ];
						int new_value = undo_info.parmIntValues[ new_value_index ];

						// get current value of multiparm
						int current_value = myParms.prParmIntValues[ parm.intValuesIndex ];

						// add/remove number of instances from current parameters to match number
						// of parameters from undo info
						int difference = new_value - current_value;
						if ( difference > 0 )
							myParms.appendMultiparmInstances( parm, difference );
						else if ( difference < 0 )
							myParms.removeMultiparmInstances( parm, -difference );
						else
							continue;

						myParms.getParameterValues();
						myParmChanges = true;

						if ( update_prefab_instance )
						{
							myAsset.prUpdatePrefabInstanceParmNames.Add( parm.name );
						}
					}
				}
				
				// Next loop through all parameters and copy changed values over from undo info
				foreach ( HAPI_ParmInfo parm in myParms.prParms )
				{
					if ( !undo_info.parmNames.Contains( parm.name ) )
						continue;

					if ( parm.isInt() )
					{
						int new_value_index = undo_info.parmIndices[ undo_info.parmNames.IndexOf( parm.name ) ];
						int new_int_value = undo_info.parmIntValues[ new_value_index ];
						int current_int_value = myParms.prParmIntValues[ parm.intValuesIndex ];

						if ( new_int_value != current_int_value &&
						    parm.type != HAPI_ParmType.HAPI_PARMTYPE_MULTIPARMLIST )
						{
							myParmChanges = true;

							int[] values = new int[ parm.size ];
							Array.Copy( undo_info.parmIntValues, new_value_index, values, 0, parm.size );
							HAPI_Host.setParmIntValues( myParms.prControl.prNodeId, values, 
							                            parm.intValuesIndex, parm.size );

							if ( update_prefab_instance )
							{
								myAsset.prUpdatePrefabInstanceParmNames.Add( parm.name );
							}
						}
					}
					else if ( parm.isFloat() )
					{
						int new_value_index = undo_info.parmIndices[ undo_info.parmNames.IndexOf( parm.name ) ];
						float new_float_value = undo_info.parmFloatValues[ new_value_index ];
						float current_float_value = myParms.prParmFloatValues[ parm.floatValuesIndex ];

						if ( new_float_value != current_float_value )
						{
							myParmChanges = true;

							float[] values = new float[ parm.size ];
							Array.Copy( undo_info.parmFloatValues, new_value_index, values, 0, parm.size );
							HAPI_Host.setParmFloatValues( myParms.prControl.prNodeId, values, 
							                              parm.floatValuesIndex, parm.size );

							if ( update_prefab_instance )
							{
								myAsset.prUpdatePrefabInstanceParmNames.Add( parm.name );
							}
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

								HAPI_Host.setParmStringValue( myParms.prControl.prNodeId, 
								                             new_string_value, parm.id, i );

								if ( update_prefab_instance )
								{
									myAsset.prUpdatePrefabInstanceParmNames.Add( parm.name );
								}
							}
						}
					}
				}
			}

			///////////////////////////////////////////////////////////////////////
			// Draw Asset Controls

			bool is_editable = myParms.prEditable;

			// We can only build or do anything if we can link to our dll which
			// can only happen on the Windows x86 platform.
#if !UNITY_STANDALONE_WIN
			is_editable = false;
			HAPI_GUI.help( HAPI_GUIUtility.myPlatformUnsupportedMessage, MessageType.Info );
#else
			if ( !is_editable )
				HAPI_GUI.help( "The parameters on this node are readonly.", MessageType.Info );
#endif // !UNITY_STANDALONE_WIN

			bool gui_enable = GUI.enabled;
			GUI.enabled = is_editable;

			myParmChanges |= generateAssetControls();

			GUI.enabled = gui_enable;

			///////////////////////////////////////////////////////////////////////
			// Apply Changes

			if ( ( ( myParmChanges && !myDelayBuild ) || 
				 ( myUnbuiltChanges && ( commitChanges || myFocusChanged ) ) ) )
			{
				// Only record undo info object if parameters are not being changed
				// due to an undo/redo event
				if ( curr_event.commandName != "UndoRedoPerformed" )
				{
					string changed_parm_name = "Parameter Change";
					if ( myParms.prLastChangedParmId != HAPI_Constants.HAPI_INVALID_PARM_ID )
						changed_parm_name = myParms.findParm( myParms.prLastChangedParmId ).label;

					Undo.RecordObject( myParms.prParmsUndoInfo, changed_parm_name );
				}

				myParms.prControl.onParmChange();
	
				myUnbuiltChanges	= false;
				myParmChanges		= false;
			}
			else if ( myParmChanges )
				myUnbuiltChanges = true;
		}
		catch ( HAPI_ErrorIgnorable ) {}
		catch ( HAPI_Error error )
		{
			Debug.LogError( error.ToString() + "\nSource: " + error.Source );
		}
		catch ( System.Exception error )
		{
			Debug.LogError( error.ToString() + "\nSource: " + error.Source );
		}
	}

	public virtual void OnSceneGUI()
	{}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private

	protected bool generateAssetControl( int index, ref bool join_last, ref bool no_label_toggle_last )
	{
		if ( myParms.prParms == null )
			return false;
		
		if ( myParms.prParms[ index ].invisible )
			return false;

		bool changed 				= false;
		
		HAPI_ParmInfo[] parms 		= myParms.prParms;
		HAPI_ParmInfo parm			= parms[ index ];

		int[] parm_int_values		= myParms.prParmIntValues;
		float[] parm_float_values	= myParms.prParmFloatValues;

		HAPI_GUIParm gui_parm 		= new HAPI_GUIParm( parm );

		// overridden parameters should not be bold in play mode
		gui_parm.isBold				= myParms.isParmOverridden( parm.id ) && 
									  !EditorApplication.isPlayingOrWillChangePlaymode;

		///////////////////////////////////////////////////////////////////////
		// Integer Parameter
		if ( parm.type == HAPI_ParmType.HAPI_PARMTYPE_MULTIPARMLIST )
		{
			changed = HAPI_GUI.multiparmField( ref gui_parm, ref myDelayBuild, ref parm_int_values,
											   ref join_last, ref no_label_toggle_last );
		}
		else if ( parm.type == HAPI_ParmType.HAPI_PARMTYPE_INT )
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
						Debug.LogError( "Parm choice parent parm id (" 
										+ myParms.prParmChoiceLists[ parm.choiceIndex + i ].parentParmId 
										+ ") not matching current parm id (" + parm.id + ")!\n"
										+ "Choice index: " + ( parm.choiceIndex + i ) + ", "
										+ "Choice count: " + parm.choiceCount );
					
					labels.Add( myParms.prParmChoiceLists[ parm.choiceIndex + i ].label );
					values.Add( i );
				}
				
				changed = HAPI_GUI.dropdown( ref gui_parm, ref parm_int_values,
											 labels.ToArray(), values.ToArray(),
											 ref join_last, ref no_label_toggle_last,
				                             null, ref parm_int_values );
			}
			else
			{
				changed = HAPI_GUI.intField( ref gui_parm, ref myDelayBuild, ref parm_int_values,
											 ref join_last, ref no_label_toggle_last, null, 
				                             ref parm_int_values );
			} // if parm.choiceCount
		} // if parm.type is INT
		///////////////////////////////////////////////////////////////////////
		// Float Parameter
		else if ( parm.type == HAPI_ParmType.HAPI_PARMTYPE_FLOAT )
		{
			changed = HAPI_GUI.floatField( ref gui_parm, ref myDelayBuild, ref parm_float_values, 
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
						Debug.LogError( "Parm choice parent parm id (" 
										+ myParms.prParmChoiceLists[ parm.choiceIndex + i ].parentParmId 
										+ ") not matching current parm id (" + parm.id + ")!\n"
										+ "Choice index: " + ( parm.choiceIndex + i ) + ", "
										+ "Choice count: " + parm.choiceCount );
					
					labels.Add( myParms.prParmChoiceLists[ parm.choiceIndex + i ].label );
					values.Add( myParms.prParmChoiceLists[ parm.choiceIndex + i ].value );
				}
				
				string[] values_temp = myParms.getParmStrings( parm );
				gui_parm.valuesIndex = 0; // Since we're piping a de-handled temp array.

				changed = HAPI_GUI.dropdown( ref gui_parm, ref values_temp,
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
			
				changed = HAPI_GUI.stringField( ref gui_parm, ref myDelayBuild, ref values,
												ref join_last, ref no_label_toggle_last, null,
				                                ref values );
			
				// Set the to be changed strings into the cache
				if ( changed )
					myParms.setParmStrings( parm, values );
			}
		}
		///////////////////////////////////////////////////////////////////////
		// File Field
		else if ( parm.type == HAPI_ParmType.HAPI_PARMTYPE_FILE )
		{
			string[] path = myParms.getParmStrings( parm );
			
			changed = HAPI_GUI.fileField( ref gui_parm, ref myDelayBuild, ref path[ 0 ],
										  ref join_last, ref no_label_toggle_last );
			
			if ( changed )
				myParms.setParmStrings( parm, path );
		}
		///////////////////////////////////////////////////////////////////////
		// Toggle Parameter
		else if ( parm.type == HAPI_ParmType.HAPI_PARMTYPE_TOGGLE )
		{
			changed = HAPI_GUI.toggle( ref gui_parm, ref parm_int_values,
									   ref join_last, ref no_label_toggle_last,
			                           null, ref parm_int_values );
		}
		///////////////////////////////////////////////////////////////////////
		// Color Parameter
		else if ( parm.type == HAPI_ParmType.HAPI_PARMTYPE_COLOUR )
		{
			changed = HAPI_GUI.colourField( ref gui_parm, ref myDelayBuild, ref parm_float_values,
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
						Debug.LogError( "Parm choice parent parm id (" 
										+ myParms.prParmChoiceLists[ parm.choiceIndex + i ].parentParmId 
										+ ") not matching current parm id (" + parm.id + ")!\n"
										+ "Choice index: " + ( parm.choiceIndex + i ) + ", "
										+ "Choice count: " + parm.choiceCount );
					
					labels.Add( myParms.prParmChoiceLists[ parm.choiceIndex + i ].label );
					values.Add( i );
				}
				
				changed = HAPI_GUI.dropdown( ref gui_parm, ref parm_int_values,
											 labels.ToArray(), values.ToArray(),
											 ref join_last, ref no_label_toggle_last,
				                             null, ref parm_int_values );
			}
			else
			{
				changed = HAPI_GUI.button( ref gui_parm, ref join_last, ref no_label_toggle_last );
			}
		}
		///////////////////////////////////////////////////////////////////////
		// Separator
		else if ( parm.type == HAPI_ParmType.HAPI_PARMTYPE_SEPARATOR )
		{
			HAPI_GUI.separator();
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
				
		bool join_last 					= false;
		bool no_label_toggle_last 		= false;
		
		int folder_list_count 			= 0;
		
		// These stacks maintain the current folder depth, parent id, and how many more child 
		// parameters are still contained in the current folder.
		Stack< int > parent_id_stack 		= new Stack< int >();
		Stack< int > parent_count_stack 	= new Stack< int >();
		int 	     instance_length = -1;
		
		// Loop through all the parameters.
		while ( current_index < myParms.prParmCount )
		{
			int current_parent_id = HAPI_Constants.HAPI_INVALID_PARM_ID; // The root has parent id -1.
			
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
					if ( myParms.prFolderListSelections.Count <= folder_list_count )
					{
						myParms.prFolderListSelections.Add( 0 );
						myParms.prFolderListSelectionIds.Add( HAPI_Constants.HAPI_INVALID_PARM_ID );
					}
					
					int selected_folder 	= myParms.prFolderListSelections[ folder_list_count ];
					selected_folder 		= GUILayout.Toolbar( selected_folder, tab_labels.ToArray() );
					myParms.prFolderListSelections[ folder_list_count ] = selected_folder;
					
					// Push only the selected folder info to the parent stacks since for this depth and this folder
					// list only the parameters of the selected folder need to be generated.
					parent_id_stack.Push( 		tab_ids[ selected_folder ] );
					parent_count_stack.Push( 	tab_sizes[ selected_folder ] );
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
					HAPI_GUI.separator();
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
	
	private HAPI_Parms myParms;
	private HAPI_Asset myAsset;

	private bool myDelayBuild;
	private bool myParmChanges;
	private bool myUnbuiltChanges;
	private bool myFocusChanged;

	private string myLastFocusedControl;
}
