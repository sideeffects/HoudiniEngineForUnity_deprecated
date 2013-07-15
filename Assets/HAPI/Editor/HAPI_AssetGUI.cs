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

[ CustomEditor( typeof( HAPI_Asset ) ) ]
public class HAPI_AssetGUI : Editor 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	public virtual void OnEnable() 
	{
		myAsset 			= target as HAPI_Asset;
		
		myParmChanges		= true;
		myUnbuiltChanges 	= false;
		myReloadAsset		= false;
	}

	public virtual void OnDisable()
	{

	}

	public virtual void refresh()
	{
		Repaint();
		SceneView.lastActiveSceneView.Repaint();
	}

	public virtual void deselect()
	{
		// Nothing for now.
	}

	public override void OnInspectorGUI() 
	{
		try
		{
			HAPI_Host.myRepaintDelegate = this.refresh;
			HAPI_Host.myDeselectionDelegate = this.deselect;
			HAPI_Host.mySelectionTarget = myAsset;

			myDelayBuild	= false;
			myParmChanges	= false;
		
			///////////////////////////////////////////////////////////////////////
			// Draw Game Object Controls

			if ( ( myAsset.prMaxTransInputCount > 0 || myAsset.prMaxGeoInputCount > 0 ) &&
				 myAsset.prAssetSubType != HAPI_AssetSubType.HAPI_ASSETSUBTYPE_CURVE )
			{
				myAsset.prShowInputControls = 
					EditorGUILayout.Foldout( myAsset.prShowInputControls, new GUIContent( "Inputs" ) );
			
				if ( myAsset.prShowInputControls )
				{
					if ( myAsset.prHAPIAssetType == HAPI_AssetType.HAPI_ASSETTYPE_OBJ )
						for ( int ii = 0; ii < myAsset.prMaxTransInputCount; ++ii )
							myParmChanges |= setTransformInput( ii );
					
					if ( myAsset.prUpStreamGeoObjects == null || myAsset.prUpStreamGeoAssets == null ||
						 myAsset.prUpStreamGeoObjects.Count <= 0 || myAsset.prUpStreamGeoAssets.Count <= 0 )
						return;

					for ( int input_index = 0; input_index < myAsset.prMaxGeoInputCount; ++input_index )
					{
						bool join_last							= false;
						bool no_label_toggle_last				= true;
						HAPI_GUIParm input_format_dropdown		= new HAPI_GUIParm( "input_format_dropdown_" + input_index );
						input_format_dropdown.width				= myInputFormatDropdownWidth;
						input_format_dropdown.size				= 1;
						input_format_dropdown.choiceCount		= 2;
						input_format_dropdown.joinNext			= true;
						input_format_dropdown.labelNone			= true;
						int[] input_format_value				= new int[ 1 ] { 0 };
						int[] input_format_dropdown_values		= new int[ 2 ] { 0, 1 };
						string[] input_format_dropdown_labels	= new string[ 2 ] { "Object", "File" };
						input_format_value[ 0 ]					= (int) myAsset.prGeoInputFormats[ input_index ];

						HAPI_GUI.dropdown( ref input_format_dropdown, ref input_format_value, 
										   input_format_dropdown_labels, input_format_dropdown_values, 
										   ref join_last, ref no_label_toggle_last );

						HAPI_GeoInputFormat value				= (HAPI_GeoInputFormat) input_format_value[ 0 ];
						myAsset.prGeoInputFormats[ input_index ]= value;

						if ( value == HAPI_GeoInputFormat.HAPI_GEO_INPUT_FORMAT_OBJECT )
						{
							HAPI_GUIParm geo_input = new HAPI_GUIParm( "geo_input_" + input_index, 
																	   myAsset.prGeoInputNames[ input_index ] );
							Object obj = (Object) myAsset.prUpStreamGeoObjects[ input_index ];
							myParmChanges |= HAPI_GUI.objectField( ref geo_input, ref obj, 
																   typeof( GameObject ), ref join_last,
																   ref no_label_toggle_last );
					
							if ( myParmChanges )
							{
								if ( myAsset.prUpStreamGeoObjects[ input_index ] != null )
								{
									HAPI_GeoInputControl control = 
										myAsset.prUpStreamGeoObjects[ input_index ].GetComponent< HAPI_GeoInputControl>();
									
									if( control != null )
										DestroyImmediate( control );
								}
								
								if ( !obj )
								{
									myAsset.removeGeoInput( input_index );
									myAsset.prUpStreamGeoObjects[ input_index ] = null;
									myAsset.prUpStreamGeoAssets[ input_index ] = null;
									
									myAsset.prFileInputs[ input_index ] = "";
									myAsset.buildClientSide();
								}
								else
								{
									myAsset.prFileInputs[ input_index ] = "";
									
									GameObject new_obj = (GameObject) obj;
							
									myAsset.prUpStreamGeoObjects[ input_index ] = new_obj;
							
									HAPI_Asset asset = null;
									HAPI_PartControl part_control = new_obj.GetComponent< HAPI_PartControl >();
							
									int object_index = 0;
									if ( part_control )
									{
										object_index = part_control.prObjectId;
										asset = part_control.prAsset;
									}
									else
										asset = new_obj.GetComponent< HAPI_Asset >();
									
									if( asset == null )
									{
										new_obj.AddComponent< HAPI_GeoInputControl >();
										HAPI_GeoInputControl control = new_obj.GetComponent< HAPI_GeoInputControl >();
										control.prAsset = myAsset;
										control.prAssetId = myAsset.prAssetId;
									}
									
									if ( asset == null || myAsset.prUpStreamGeoAssets[ input_index ] != asset )
									{
										if ( asset )
										{
											if ( myAsset == asset )
												Debug.LogError( "Can't connect an asset to itself!" );
											else
											{
												myAsset.addAssetAsGeoInput( asset, object_index, input_index );
												myAsset.buildClientSide();
											}
										}
										else
										{
											myAsset.addGeoAsGeoInput( new_obj, input_index );
											myAsset.buildClientSide();
										}
									}
								}
							}
						}
						else
						{
							HAPI_GUIParm file_input = new HAPI_GUIParm( "file_input_" + input_index,
																		myAsset.prGeoInputNames[ input_index ] );
							string file_path = myAsset.prFileInputs[ input_index ];
							myParmChanges |= HAPI_GUI.fileField( ref file_input, ref myDelayBuild, ref file_path,
																 ref join_last, ref no_label_toggle_last );
							if ( myParmChanges )
							{
								if ( file_path.Equals("") )
								{
									myAsset.removeGeoInput( input_index );
									myAsset.prFileInputs[ input_index ] = "";
									
									myAsset.prUpStreamGeoObjects[ input_index ] = null;
									myAsset.prUpStreamGeoAssets[ input_index ] = null;
									myAsset.buildClientSide();
								}
								else
								{
									myAsset.prFileInputs[ input_index ] = file_path;
									myAsset.addFileAsGeoInput( file_path, input_index );
									
									myAsset.prUpStreamGeoObjects[ input_index ] = null;
									myAsset.prUpStreamGeoAssets[ input_index ] = null;
									myAsset.buildClientSide();
								}
							}
						} // if
					} // for
				} // if
			} // if
		}
		catch ( HAPI_Error e )
		{
			Debug.LogError( e.ToString() );
		}
	}

	public virtual void OnSceneGUI()
	{
		HAPI_Host.myRepaintDelegate			= this.refresh;
		HAPI_Host.myDeselectionDelegate		= this.deselect;
		HAPI_Host.mySelectionTarget			= myAsset;
	}
	
	protected bool setTransformInput( int index )
	{
		if ( myAsset.prUpStreamTransformObjects == null || myAsset.prUpStreamTransformObjects.Count <= 0 )
			return false;

		bool join_last				= false;
		bool no_label_toggle_last	= true;

		HAPI_GUI.label( "Transform", myInputFormatDropdownWidth, true, ref join_last );

		HAPI_GUIParm trans_input	= new HAPI_GUIParm( "trans_input_" + index, myAsset.prTransInputNames[ index ] );
		Object obj					= (Object) myAsset.prUpStreamTransformObjects[ index ];
		bool changed				= HAPI_GUI.objectField( ref trans_input, ref obj, typeof( GameObject ),
															ref join_last, ref no_label_toggle_last );
		
		if ( changed )
		{
			if ( !obj )
			{
				myAsset.prUpStreamTransformObjects[ index ] = null;
				myAsset.removeTransformInput( index );
			}
			else
			{
				GameObject game_obj = (GameObject) obj;
				myAsset.prUpStreamTransformObjects[ index ] = game_obj;
				HAPI_Asset input_asset = game_obj.GetComponent< HAPI_Asset >();
				if ( input_asset )
					myAsset.addAssetAsTransformInput( input_asset, index );
				else
					myAsset.removeTransformInput( index );
				myAsset.buildClientSide();
			}
		}
		
		return changed;
	}

	protected bool generateAssetControl( int index, ref bool join_last, ref bool no_label_toggle_last )
	{
		if ( myAsset.prParms == null )
			return false;
		
		if ( myAsset.prParms[ index ].invisible )
			return false;
		
		bool changed 				= false;
		
		int node_id					= myAsset.prAssetNodeId;
		
		HAPI_ParmInfo[] parms 		= myAsset.prParms;
		HAPI_ParmInfo parm			= parms[ index ];
		
		int[] parm_int_values		= myAsset.prParmIntValues;
		float[] parm_float_values	= myAsset.prParmFloatValues;
		
		HAPI_ParmType parm_type 	= (HAPI_ParmType) parm.type;
		
		HAPI_GUIParm gui_parm = new HAPI_GUIParm( parm );
		
		///////////////////////////////////////////////////////////////////////
		// Integer Parameter
		if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_MULTIPARMLIST )
		{
			changed = HAPI_GUI.multiparmField( ref gui_parm, ref myDelayBuild, ref parm_int_values,
											   ref join_last, ref no_label_toggle_last );
		}
		else if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_INT )
		{
			if ( parm.choiceCount > 0 && parm.choiceIndex >= 0 )
			{
				// Draw popup (menu) field.
				List< string > 	labels = new List< string >();
				List< int>		values = new List< int >();
				
				// Go through our choices.
				for ( int i = 0; i < parm.choiceCount; ++i )
				{
					if ( myAsset.prParmChoiceLists[ parm.choiceIndex + i ].parentParmId != parm.id )
						Debug.LogError( "Parm choice parent parm id (" 
										+ myAsset.prParmChoiceLists[ parm.choiceIndex + i ].parentParmId 
										+ ") not matching current parm id (" + parm.id + ")!\n"
										+ "Choice index: " + ( parm.choiceIndex + i ) + ", "
										+ "Choice count: " + parm.choiceCount );
					
					labels.Add( myAsset.prParmChoiceLists[ parm.choiceIndex + i ].label );
					values.Add( i );
				}
				
				changed = HAPI_GUI.dropdown( ref gui_parm, ref parm_int_values,
											 labels.ToArray(), values.ToArray(),
											 ref join_last, ref no_label_toggle_last );
			}
			else
			{
				changed = HAPI_GUI.intField( ref gui_parm, ref myDelayBuild, ref parm_int_values,
											 ref join_last, ref no_label_toggle_last );
			} // if parm.choiceCount
		} // if parm_type is INT
		///////////////////////////////////////////////////////////////////////
		// Float Parameter
		else if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_FLOAT )
		{
			changed = HAPI_GUI.floatField( ref gui_parm, ref myDelayBuild, ref parm_float_values, 
										   ref join_last, ref no_label_toggle_last );
		} // if parm_type is FLOAT
		///////////////////////////////////////////////////////////////////////
		// String Parameter
		else if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_STRING )
		{
			if ( parm.choiceCount > 0 && parm.choiceIndex >= 0 )
			{
				// Draw popup (menu) field.
				List< string > labels = new List< string >();
				List< string > values = new List< string >();
				
				// Go through our choices.
				for ( int i = 0; i < parm.choiceCount; ++i )
				{
					if ( myAsset.prParmChoiceLists[ parm.choiceIndex + i ].parentParmId != parm.id )
						Debug.LogError( "Parm choice parent parm id (" 
										+ myAsset.prParmChoiceLists[ parm.choiceIndex + i ].parentParmId 
										+ ") not matching current parm id (" + parm.id + ")!\n"
										+ "Choice index: " + ( parm.choiceIndex + i ) + ", "
										+ "Choice count: " + parm.choiceCount );
					
					labels.Add( myAsset.prParmChoiceLists[ parm.choiceIndex + i ].label );
					values.Add( myAsset.prParmChoiceLists[ parm.choiceIndex + i ].value );
				}
				
				string[] values_temp = myAsset.getParmStrings( parm );
				gui_parm.valuesIndex = 0; // Since we're piping a de-handled temp array.

				changed = HAPI_GUI.dropdown( ref gui_parm, ref values_temp,
											 labels.ToArray(), values.ToArray(),
											 ref join_last, ref no_label_toggle_last );

				if ( changed ) 
					myAsset.setParmStrings( parm, values_temp );
			}
			else
			{
				string[] values = myAsset.getParmStrings( parm ); 
			
				// The given string array is only for this parm so we need to set the values index to 0.
				gui_parm.valuesIndex = 0;
			
				changed = HAPI_GUI.stringField( ref gui_parm, ref myDelayBuild, ref values,
												ref join_last, ref no_label_toggle_last );
			
				// Set the to be changed strings into the cache
				if ( changed )
					myAsset.setParmStrings( parm, values );
			}
		}
		///////////////////////////////////////////////////////////////////////
		// File Field
		else if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_FILE )
		{
			string[] path = myAsset.getParmStrings( parm );
			
			changed = HAPI_GUI.fileField( ref gui_parm, ref myDelayBuild, ref path[ 0 ],
										  ref join_last, ref no_label_toggle_last );
			
			if ( changed )
				myAsset.setParmStrings( parm, path );
		}
		///////////////////////////////////////////////////////////////////////
		// Toggle Parameter
		else if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_TOGGLE )
		{
			changed = HAPI_GUI.toggle( ref gui_parm, ref parm_int_values,
									   ref join_last, ref no_label_toggle_last );
		}
		///////////////////////////////////////////////////////////////////////
		// Color Parameter
		else if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_COLOUR )
		{
			changed = HAPI_GUI.colourField( ref gui_parm, ref myDelayBuild, ref parm_float_values,
											ref join_last, ref no_label_toggle_last );
		}
		///////////////////////////////////////////////////////////////////////
		// Button Parameter
		else if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_BUTTON )
		{
			if ( parm.choiceCount > 0 && parm.choiceIndex >= 0 )
			{
				// Draw popup (menu) field.
				List< string > 	labels = new List< string >();
				List< int>		values = new List< int >();
				
				// Go through our choices.
				for ( int i = 0; i < parm.choiceCount; ++i )
				{
					if ( myAsset.prParmChoiceLists[ parm.choiceIndex + i ].parentParmId != parm.id )
						Debug.LogError( "Parm choice parent parm id (" 
										+ myAsset.prParmChoiceLists[ parm.choiceIndex + i ].parentParmId 
										+ ") not matching current parm id (" + parm.id + ")!\n"
										+ "Choice index: " + ( parm.choiceIndex + i ) + ", "
										+ "Choice count: " + parm.choiceCount );
					
					labels.Add( myAsset.prParmChoiceLists[ parm.choiceIndex + i ].label );
					values.Add( i );
				}
				
				changed = HAPI_GUI.dropdown( ref gui_parm, ref parm_int_values,
											 labels.ToArray(), values.ToArray(),
											 ref join_last, ref no_label_toggle_last );
			}
			else
			{
				changed = HAPI_GUI.button( ref gui_parm, ref join_last, ref no_label_toggle_last );
			}
		}
		///////////////////////////////////////////////////////////////////////
		// Separator
		else if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_SEPARATOR )
		{
			HAPI_GUI.separator();
		}
		
		if ( myAsset.hasProgressBarBeenUsed() && parm.id == myAsset.prLastChangedParmId )
		{
			// TODO: Set the focus back to this control since the progress bar would have stolen it.	
		}
		
		if ( changed )
			myAsset.appendChangedParm( parm.id );
		
		return changed;
	}
	
	protected bool generateAssetControls() 
	{
		if ( myAsset.prParms == null )
			return false;
		
		bool changed 					= false;
		int current_index 				= 0;
		HAPI_ParmInfo[] parms 			= myAsset.prParms;
				
		bool join_last 					= false;
		bool no_label_toggle_last 		= false;
		
		int folder_list_count 			= 0;
		
		// These stacks maintain the current folder depth, parent id, and how many more child 
		// parameters are still contained in the current folder.
		Stack< int > parent_id_stack 		= new Stack< int >();
		Stack< int > parent_count_stack 	= new Stack< int >();
		int 	     instance_length = -1;
		
		// Loop through all the parameters.
		while ( current_index < myAsset.prParmCount )
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

			if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_MULTIPARMLIST )
			{
				changed |= generateAssetControl( current_index, ref join_last, ref no_label_toggle_last );

				//int instance_count = myAsset.prParmIntValues[ parms[ current_index ].intValuesIndex ];
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
			else if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_FOLDERLIST )
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
					if ( myAsset.prFolderListSelections.Count <= folder_list_count )
					{
						myAsset.prFolderListSelections.Add( 0 );
						myAsset.prFolderListSelectionIds.Add( -1 );
					}
					
					int selected_folder 	= myAsset.prFolderListSelections[ folder_list_count ];
					selected_folder 		= GUILayout.Toolbar( selected_folder, tab_labels.ToArray() );
					myAsset.prFolderListSelections[ folder_list_count ] = selected_folder;
					
					// Push only the selected folder info to the parent stacks since for this depth and this folder
					// list only the parameters of the selected folder need to be generated.
					parent_id_stack.Push( 		tab_ids[ selected_folder ] );
					parent_count_stack.Push( 	tab_sizes[ selected_folder ] );
				}
				current_index++;
			}
			else if ( parms[ current_index ].isMultiParm )
			{
				GUILayout.BeginHorizontal();

				// Create the add / remove buttons
				bool removed_instance = false;
				
				GUILayout.BeginHorizontal();
				if ( GUILayout.Button( "X" ) ) 
				{
					myAsset.removeMultiparmInstance( parms[ current_index ] );
					changed = true;
				}
				if ( GUILayout.Button( "+" ) )
				{
					myAsset.insertMultiparmInstance( parms[ current_index ] );
					changed = true;
				}
				GUILayout.EndHorizontal();

				// Create the parms within the multiparm
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
				if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_FOLDER )
					Debug.LogError( "All folders should have been parsed in the folder list if clause!" );

				changed |= generateAssetControl( current_index, ref join_last, ref no_label_toggle_last );

				current_index++;
			}
		}

		return changed;
	}
	
	protected HAPI_Asset 	myAsset;
	protected bool			myDelayBuild;
	protected bool			myParmChanges;
	protected bool			myUnbuiltChanges;
	protected bool			myReloadAsset;

	private const int		myInputFormatDropdownWidth = 62;
}
