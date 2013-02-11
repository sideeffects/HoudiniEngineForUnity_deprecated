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

[ ExecuteInEditMode ]
[ CustomEditor( typeof( HAPI_AssetCurve ) ) ]
public class HAPI_AssetGUICurve : HAPI_AssetGUI 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	public override void OnEnable() 
	{
		base.OnEnable();
		myAssetCurve			= myAsset as HAPI_AssetCurve;
		
		myCurrentlyActivePoint	= -1;

		myForceInstectorRedraw	= false;
		myAddPointButtonLabel	= "Add Points";
		myTarget				= null;
		
		if ( GUI.changed )
			myAssetCurve.build();
	}

	public override void OnDisable()
	{
		myAssetCurve.prIsAddingPoints = false;
	}
	
	public override void OnInspectorGUI() 
	{
		if ( myAssetCurve == null )
			return;
		
		myParmChanges = false;
		
		base.OnInspectorGUI();
		
		Event current_event = Event.current;
		bool commitChanges = false;
		if ( current_event.isKey && current_event.type == EventType.KeyUp && current_event.keyCode == KeyCode.Return )
			commitChanges = true;

		// Check if ENTER or ESC was pressed to we can exit curve mode.
		if ( myAssetCurve.prIsAddingPoints && current_event.isKey && current_event.type == EventType.KeyUp )
			if ( current_event.keyCode == KeyCode.Escape || current_event.keyCode == KeyCode.Return )
			{
				myAssetCurve.prIsAddingPoints	= false;
				myForceInstectorRedraw			= true;
			}
		
		///////////////////////////////////////////////////////////////////////
		// Draw Game Object Controls
		
		myAssetCurve.prShowObjectControls = 
			EditorGUILayout.Foldout( myAssetCurve.prShowObjectControls, new GUIContent( "Object Controls" ) );
		
		if ( myAssetCurve.prShowObjectControls ) 
		{	
			if ( GUILayout.Button( "Rebuild" ) ) 
			{
				myAssetCurve.prIsAddingPoints = false;
				myAssetCurve.prFullBuild = true;
				myAssetCurve.build();

				myAssetCurve.syncPointsWithParm();
				myAssetCurve.build();
			}

			// Draw Auto Select Asset Node Toggle
			{
				bool value = myAsset.prAutoSelectAssetNode;
				HAPI_GUI.toggle( "auto_select_parent", "Auto Select Parent", ref value );
				myAsset.prAutoSelectAssetNode = value;
			}

			// Sync Asset Transform Toggle
			{
				bool value = myAsset.prSyncAssetTransform;
				HAPI_GUI.toggle( "sync_asset_transform", "Sync Asset Transform", ref value );
				myAsset.prSyncAssetTransform = value;
			}

			// Live Transform Propagation Toggle
			{
				bool value = myAsset.prLiveTransformPropagation;
				HAPI_GUI.toggle( "live_transform_propagation", "Live Transform Propagation", ref value );
				myAsset.prLiveTransformPropagation = value;
			}
		} // if
		
		///////////////////////////////////////////////////////////////////////
		// Draw Asset Controls
		
		EditorGUILayout.Separator();
		myAssetCurve.prShowAssetControls = 
			EditorGUILayout.Foldout( myAssetCurve.prShowAssetControls, new GUIContent( "Asset Controls" ) );
		
		myDelayBuild = false;
		if ( myAssetCurve.prShowAssetControls )
		{
			if ( myAssetCurve.prIsAddingPoints )
				myAddPointButtonLabel = "Stop Adding Points";
			else
				myAddPointButtonLabel = "Add Points";

			if ( GUILayout.Button( myAddPointButtonLabel ) )
			{
				myAssetCurve.prIsAddingPoints = !myAssetCurve.prIsAddingPoints;
				OnSceneGUI();
			}

			GUI.enabled = !myAssetCurve.prIsAddingPoints;

			Object target = (Object) myTarget;
			if ( HAPI_GUI.objectField( "target", "Target", ref target, typeof( GameObject ) ) )
			{
				myTarget = (GameObject) target;
			}
			myParmChanges |= generateAssetControls();
			
			GUI.enabled = true;
		}

		if ( ( myParmChanges && !myDelayBuild ) || ( myUnbuiltChanges && commitChanges ) )
		{
			myAssetCurve.syncPointsWithParm();
			myAssetCurve.build();
			myUnbuiltChanges = false;
			
			// To keep things consistent with Unity workflow, we should not save parameter changes
			// while in Play mode.
			if ( !EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode )
				myAssetCurve.savePreset();
		}
		else if ( myParmChanges )
			myUnbuiltChanges = true;
	}
	
	public void OnSceneGUI() 
	{
		if ( myAssetCurve == null )
			return;
		
		if ( !myTempCamera && Camera.current )
			myTempCamera = Camera.current;

		Event current_event 		= Event.current;
		
		int point_count 			= myAssetCurve.prPoints.Count;
		int pressed_point_index 	= -1;
		Vector3 previous_position 	= Vector3.zero;

		// Rebuilding geometry here to make sure the dummy line geometry is visible at any zoom level.
		myAssetCurve.buildDummyMesh();

		// Set appropriate handles matrix.
		Handles.matrix = myAssetCurve.transform.localToWorldMatrix;
		
		// Check if ENTER or ESC was pressed to we can exit curve mode.
		if ( myAssetCurve.prIsAddingPoints && current_event.isKey && current_event.type == EventType.KeyUp )
			if ( current_event.keyCode == KeyCode.Escape || current_event.keyCode == KeyCode.Return )
			{
				myAssetCurve.prIsAddingPoints	= false;
				myForceInstectorRedraw			= true;
			}

		// Add points.
		if ( myAssetCurve.prIsAddingPoints )
		{
			Handles.BeginGUI();
			GUILayout.BeginArea( new Rect( 0, 0, Screen.width, Screen.height ) );

			// Draw description and exit button of curve editing mode.
			GUIStyle text_style		= new GUIStyle( GUI.skin.label );
			text_style.alignment	= TextAnchor.UpperLeft;
			text_style.fontStyle	= FontStyle.Bold;
			Color original_color	= GUI.color;
			GUI.color				= Color.yellow;
			GUI.Box( new Rect( myActiveBorderWidth + 4, myActiveBorderWidth + 4, 220, 130 ), "" );
			GUI.Label( new Rect( myActiveBorderWidth + 5, myActiveBorderWidth + 4, 200, 20 ), "Curve Editing Mode", text_style );
			
			text_style.fontStyle	= FontStyle.Normal;
			text_style.wordWrap		= true;
			GUI.Label( new Rect( myActiveBorderWidth + 5, myActiveBorderWidth + 21, 200, 80 ), "Click anywhere on the screen to add a new curve control point. You can also move existing points in this mode but you cannot select any other object. Press (ENTER) or (ESC) when done.", text_style );
			
			myAssetCurve.prIsAddingPoints = !GUI.Button( new Rect( myActiveBorderWidth + 100, myActiveBorderWidth + 108, 120, 20 ), "Exit Curve Mode" );
			GUI.color				= original_color;

			// Draw yellow mode lines around the Scene view.
			Texture2D box_texture	= new Texture2D( 1, 1 );
			box_texture.SetPixel( 0, 0, new Color( 1.0f, 1.0f, 0.0f, 0.6f ) );
			box_texture.wrapMode	= TextureWrapMode.Repeat;
			box_texture.Apply();
			float width				= myTempCamera.pixelWidth;
			float height			= myTempCamera.pixelHeight;
			float border_width		= myActiveBorderWidth;
			GUI.DrawTexture( new Rect( 0, 0, width, border_width ),								// Top
							 box_texture, ScaleMode.StretchToFill );
			GUI.DrawTexture( new Rect( 0, border_width, border_width, height - border_width ),	// Right
							 box_texture, ScaleMode.StretchToFill );
			GUI.DrawTexture( new Rect( border_width, height - border_width, width, height ),	// Bottom
							 box_texture, ScaleMode.StretchToFill );
			GUI.DrawTexture( new Rect( width - border_width, border_width,						// Left
							 width, height - border_width - border_width ), 
							 box_texture, ScaleMode.StretchToFill );

			GUILayout.EndArea();
			Handles.EndGUI();

			if ( !current_event.alt )
			{

				Vector3 position	= Vector3.zero;
				float handle_size 	= HandleUtility.GetHandleSize( position ) * 1000000.0f;
				Quaternion rotation = HAPI_AssetUtility.getQuaternion( myTempCamera.transform.localToWorldMatrix );
				bool button_press 	= Handles.Button( 	position, 
														rotation,
														handle_size,
														handle_size,
														Handles.RectangleCap );

				if ( button_press )
				{
					Vector3 mouse_position = current_event.mousePosition;
				
					// Camera.current.pixelHeight != Screen.height for some reason.
					mouse_position.y		= myTempCamera.pixelHeight - mouse_position.y;
					Ray ray					= myTempCamera.ScreenPointToRay( mouse_position );
					ray.origin				= myTempCamera.transform.position;
					Vector3 intersection	= new Vector3();

					if ( myTarget != null && myTarget.GetComponent< MeshCollider >() )
					{
						MeshCollider collider = myTarget.GetComponent< MeshCollider >();
						RaycastHit hit_info;
						collider.Raycast( ray, out hit_info, 1000.0f );
						intersection = hit_info.point;
					}
					else
					{
						Plane plane = new Plane();
						plane.SetNormalAndPosition( Vector3.up, myAssetCurve.transform.position );
						float enter = 0.0f;
						plane.Raycast( ray, out enter );
 						intersection = ray.origin + ray.direction * enter;
					}
				
					myAssetCurve.addPoint( intersection );
				}
			}
		}
		else if ( myForceInstectorRedraw )
		{
			Repaint();
			myForceInstectorRedraw = false;
		}

		// Draw current curve.
		Vector3[] vertices = myAssetCurve.prVertices;
		for ( int i = 0; vertices != null && i < vertices.Length; ++i )
		{
			Vector3 position = vertices[ i ];
			
			if ( i > 0 )
			{
				Handles.color = Color.black;
				Handles.DrawLine( previous_position, position );
			}

			previous_position = position;
		}
		
		// Determine which control point was pressed for modification.
		for ( int i = 0; i < point_count; ++i ) 
		{
			Vector3 position 	= myAssetCurve.prPoints[ i ];
			float handle_size 	= HandleUtility.GetHandleSize( position ) * 0.06f;
			
			Handles.color 		= Color.cyan;
			Quaternion rotation = HAPI_AssetUtility.getQuaternion( myTempCamera.transform.localToWorldMatrix );
			bool buttonPress 	= Handles.Button( 	position, 
													rotation,
													handle_size,
													handle_size,
													Handles.RectangleCap );
			
			if ( buttonPress )
				pressed_point_index = i;
			
			Handles.Label( position, new GUIContent( "p" + i ) );
			
			if ( i > 0 )
			{
				Handles.color = Color.grey;
				Handles.DrawLine( previous_position, position );
			}

			previous_position = position;
		}
		
		// Set active control point.
		if ( pressed_point_index >= 0 )
			myCurrentlyActivePoint = pressed_point_index;

		// Update active control point.
		if ( myCurrentlyActivePoint >= 0 ) 
		{
			Vector3 old_position = myAssetCurve.prPoints[ myCurrentlyActivePoint ];
			Vector3 new_position = Handles.PositionHandle( old_position, 
														   Quaternion.identity );
			
			if ( new_position != old_position )
				myAssetCurve.updatePoint( myCurrentlyActivePoint, new_position );
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private
	
	private bool generateAssetControl( int id, ref bool join_last, ref bool no_label_toggle_last )
	{
		if ( myAssetCurve.prParms == null )
			return false;
		
		if ( myAssetCurve.prParms[ id ].invisible )
			return false;
		
		bool changed 				= false;
		
		int asset_id				= myAssetCurve.prAssetId;
		
		HAPI_ParmInfo[] parms 		= myAssetCurve.prParms;
		HAPI_ParmInfo parm			= parms[ id ];
		
		int[] parm_int_values		= myAssetCurve.prParmIntValues;
		float[] parm_float_values	= myAssetCurve.prParmFloatValues;
		int[] parm_string_values	= myAssetCurve.prParmStringValues;
		
		HAPI_ParmType parm_type 	= (HAPI_ParmType) parm.type;
		int parm_size				= parm.size;
		
		HAPI_GUIParm gui_parm = new HAPI_GUIParm( parm );
		
		int values_index = -1;
		if ( parm.isInt() )
		{
			if ( parm.intValuesIndex < 0 || parm_int_values == null )
				return false;
			values_index = parm.intValuesIndex;
		}
		else if ( parm.isFloat() )
		{
			if ( parm.floatValuesIndex < 0 || parm_float_values == null )
				return false;
			values_index = parm.floatValuesIndex;
		}
		else if ( parms[ id ].isString() )
		{
			if ( parm.stringValuesIndex < 0 || parm_string_values == null )
				return false;
			values_index = parm.stringValuesIndex;
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
					if ( myAssetCurve.prParmChoiceLists[ parm.choiceIndex + i ].parentParmId != id )
						Debug.LogError( "Parm choice parent parm id (" 
										+ myAssetCurve.prParmChoiceLists[ parm.choiceIndex + i ].parentParmId 
										+ ") not matching current parm id (" + id + ")!\n"
										+ "Choice index: " + ( parm.choiceIndex + i ) + ", "
										+ "Choice count: " + parm.choiceCount );
					
					labels.Add( myAssetCurve.prParmChoiceLists[ parm.choiceIndex + i ].label );
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
			string[] values = new string[ parm_size ];
			for ( int p = 0; p < parm_size; ++p )
				values[ p ] = HAPI_Host.getString( parm_string_values[ values_index + p ] );
			
			// The given string array is only for this parm so we need to set the values index to 0.
			gui_parm.valuesIndex = 0;
			
			changed = HAPI_GUI.stringField( ref gui_parm, ref myDelayBuild, ref values,
											ref join_last, ref no_label_toggle_last );
			
			if ( changed )
				for ( int p = 0; p < parm_size; ++p )
					HAPI_Host.setParmStringValue( asset_id, values[ p ], id, p );
		}
		///////////////////////////////////////////////////////////////////////
		// File Field
		else if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_FILE )
		{
			string path = HAPI_Host.getString( parm_string_values[ values_index ] );
			
			changed = HAPI_GUI.fileField( ref gui_parm, ref myDelayBuild, ref path,
										  ref join_last, ref no_label_toggle_last );
			
			if ( changed )
				HAPI_Host.setParmStringValue( asset_id, path, id, 0 );
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
		// Separator
		else if ( parm_type == HAPI_ParmType.HAPI_PARMTYPE_SEPARATOR )
		{
			HAPI_GUI.separator();
		}
		
		if ( myAssetCurve.hasProgressBarBeenUsed() && id == myAssetCurve.prLastChangedParmId )
		{
			// TODO: Set the focus back to this control since the progress bar would have stolen it.	
		}
		
		if ( changed )
		{
			myAssetCurve.prLastChangedParmId = id;
		
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
		if ( myAssetCurve.prParms == null )
			return false;
		
		bool changed 					= false;
		int current_index 				= 0;
		HAPI_ParmInfo[] parms 			= myAssetCurve.prParms;
		
		bool join_last 					= false;
		bool no_label_toggle_last 		= false;
		
		int folder_list_count 			= 0;
		
		// These stacks maintain the current folder depth, parent id, and how many more child 
		// parameters are still contained in the current folder.
		Stack< int > parent_id_stack 		= new Stack< int >();
		Stack< int > parent_count_stack 	= new Stack< int >();
		
		
		// Loop through all the parameters.
		while ( current_index < myAssetCurve.prParmCount )
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
					if ( myAssetCurve.prFolderListSelections.Count <= folder_list_count )
					{
						myAssetCurve.prFolderListSelections.Add( 0 );
						myAssetCurve.prFolderListSelectionIds.Add( -1 );
					}
					
					int selected_folder 	= myAssetCurve.prFolderListSelections[ folder_list_count ];
					selected_folder 		= GUILayout.Toolbar( selected_folder, tab_labels.ToArray() );
					myAssetCurve.prFolderListSelections[ folder_list_count ] = selected_folder;
					
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

	private HAPI_AssetCurve		myAssetCurve;

	private int 				myCurrentlyActivePoint;
	private bool				myForceInstectorRedraw;
	private string				myAddPointButtonLabel;

	private const float			myActiveBorderWidth = 5;
	private Camera				myTempCamera;

	[SerializeField] 
	private GameObject			myTarget;
}
