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
using System.Reflection;

[ CustomEditor( typeof( HAPI_Asset ) ) ]
public class HAPI_AssetGUI : Editor 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	public virtual void OnEnable() 
	{
		myAsset 			= target as HAPI_Asset;
		myUndoInfo			= myAsset.prAssetUndoInfo;
		
		myParmChanges		= true;
		myUnbuiltChanges 	= false;
		myFocusChanged 		= true;

		myHelpScrollPosition = new Vector2( 0.0f, 0.0f );

		HAPI_Host.myRepaintDelegate += this.refresh;
		HAPI_Host.myDeselectionDelegate += this.deselect;
		HAPI_Host.mySelectionTarget = myAsset.gameObject;
		
		// If selection is a prefab build it ( parameters only ) to allow 
		// editing of parameters. Only need to build once, the first time
		// it is selected.
		if ( myAsset.isPrefab() )
		{
			// Reloading prefab after play mode change since OnEnable
			// never gets called for prefab. This only needs to be done
			// if prefab has already been built.
			if ( myAsset.prReloadPrefabOnPlaymodeChange &&
				 HAPI_Host.isAssetValid( myAsset.prAssetId, myAsset.prAssetValidationId ) )
			{
				myAsset.prReloadPrefabOnPlaymodeChange = false;
				myAsset.build(	false,	// reload_asset
								false,	// unload_asset_first
								true,	// serializatin_recovery_only
								false,	// force_reconnect
								false,	// cook_downstream_assets
								false	// use_delay_for_progress_bar
					 		 );
			}
			else if (
				myAsset.prAssetId != myAsset.prBackupAssetId ||
				!HAPI_Host.isAssetValid( myAsset.prAssetId, myAsset.prAssetValidationId ) )
			{
				myAsset.prAssetId = -1;
				myAsset.build( true,	// reload_asset
							   true,	// unload_asset_first
							   true,	// serializatin_recovery_only
							   false,	// force_reconnect
							   false,	// cook_downstream_assets
							   false	// use_delay_for_progress_bar
					 		 );
				EditorUtility.SetDirty( myAsset );
			}
		}
	}

	public virtual void OnDisable()
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
		if ( HAPI_Host.mySelectionTarget == myAsset.gameObject )
			HAPI_Host.mySelectionTarget = null;
	}

	public override void OnInspectorGUI() 
	{
		// We can only build or do anything if we can link to our dll which
		// can only happen on the Windows x86 platform.
#if !UNITY_STANDALONE_WIN
		bool gui_enable = GUI.enabled;
		HAPI_GUI.help( HAPI_Constants.HAPI_UNSUPPORTED_PLATFORM_MSG, MessageType.Info );
		GUI.enabled = false;
#endif // !UNITY_STANDALONE_WIN

		try
		{
			myDelayBuild	= false;
			myParmChanges	= false;

			///////////////////////////////////////////////////////////////////////
			// Draw Game Object Controls

			if ( ( myAsset.prMaxTransInputCount > 0 || myAsset.prMaxGeoInputCount > 0 ) &&
				 myAsset.prAssetSubType != HAPI_AssetSubType.HAPI_ASSETSUBTYPE_CURVE &&
				 !myAsset.isPrefab() )
			{
				myAsset.prShowInputControls = HAPI_GUI.foldout( "Inputs", myAsset.prShowInputControls, true );
			
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
						                   ref join_last, ref no_label_toggle_last, null, ref input_format_value );

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

									// Select the asset component (if it exists).
									HAPI_Asset asset = new_obj.GetComponent< HAPI_Asset >();

									// If we're selecting a specific object to input than try and
									// get the object id. Note that by getting the HAPI_ObjectControl
									// component we also cover the geo and part controls because
									// they all inherit from HAPI_ObjectControl. The user can therefore
									// drag any gameObject under the asset into another asset's
									// input and have it all work.
									int object_index = 0;
									HAPI_ObjectControl obj_control = new_obj.GetComponent< HAPI_ObjectControl >();
									if ( obj_control )
									{
										object_index = obj_control.prObjectId;
										asset = obj_control.prAsset;
									}

									// If we are connecting a non-HAPI game object than we need to 
									// assetize it first by converting it to an Input Asset.
									if ( asset == null )
										asset = new_obj.AddComponent< HAPI_AssetInput >();
									
									if ( myAsset.prUpStreamGeoAssets[ input_index ] != asset )
									{
										if ( myAsset == asset )
											Debug.LogError( "Can't connect an asset to itself!" );
										else
										{
											myAsset.addAssetAsGeoInput( asset, object_index, input_index );
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
		
#if !UNITY_STANDALONE_WIN
		GUI.enabled = gui_enable;
#endif // !UNITY_STANDALONE_WIN
	}

	public virtual void OnSceneGUI()
	{
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

	protected void drawHelpBox( string text )
	{
		float width = (float) Screen.width - 60;

		myHelpScrollPosition = EditorGUILayout.BeginScrollView(
			myHelpScrollPosition, GUILayout.Height( 200 ) );

		GUIStyle sel_label = new GUIStyle( GUI.skin.label );
		sel_label.stretchWidth = true;
		sel_label.wordWrap = true;

		float height = sel_label.CalcHeight( 
			new GUIContent( text ), width );

		EditorGUILayout.SelectableLabel( 
			text, sel_label, GUILayout.Width( width ),
			GUILayout.Height( height ) );

		EditorGUILayout.EndScrollView();
	}

	protected delegate void valueChangedFunc();

	protected void createToggleForProperty(
		string name, string label, string property_name, 
		ref bool undo_info_value, valueChangedFunc func )
	{
		createToggleForProperty( name, label, property_name, ref undo_info_value, func, false );
	}
	
	protected void createToggleForProperty(
		string name, string label, string property_name, 
		ref bool undo_info_value, valueChangedFunc func,
		bool global_overwrite )
	{
		createToggleForProperty(
			name, label, property_name, ref undo_info_value, 
			func, global_overwrite, false, "" );
	}

	protected void createToggleForProperty(
		string name, string label, string property_name, 
		ref bool undo_info_value, valueChangedFunc func,
		bool global_overwrite, bool local_overwrite, 
		string local_overwrite_message )
	{
		bool gui_enabled = GUI.enabled;
		try
		{
			PropertyInfo property = typeof( HAPI_Asset ).GetProperty( property_name );
			if ( property == null )
				throw new HAPI_ErrorInvalidArgument( property_name + " is not a valid property of HAPI_Asset!" );
			if ( property.PropertyType != typeof( bool ) )
				throw new HAPI_ErrorInvalidArgument( property_name + " is not a boolean!" );

			GUI.enabled = !global_overwrite && !local_overwrite && GUI.enabled;

			if ( global_overwrite )
				label += " (overwritted by global setting)";
			else if ( local_overwrite )
				label += local_overwrite_message;

			bool value = ( bool ) property.GetValue( myAsset, null );
			bool is_bold = myParentPrefabAsset && ( bool ) property.GetValue( myParentPrefabAsset, null ) != value;
			bool changed = HAPI_GUI.toggle( name, label, is_bold, ref value, myUndoInfo, ref undo_info_value );

			if ( changed )
			{
				property.SetValue( myAsset, value, null );

				if ( func != null )
					func();
			}
		}
		catch ( System.Exception error )
		{
			Debug.LogError(
				"Failed to create toggle for: " + label + "\n" +
				error.ToString() + "\nSource: " + error.Source );
		}
		GUI.enabled = gui_enabled;
	}

	protected HAPI_Asset 	myAsset;
	protected bool			myDelayBuild;
	protected bool			myParmChanges;
	protected bool			myUnbuiltChanges;
	protected bool 			myFocusChanged;

	protected Vector2 myHelpScrollPosition = new Vector2( 0.0f, 0.0f );

	protected HAPI_AssetUndoInfo myUndoInfo;
	protected HAPI_Asset myParentPrefabAsset;

	private const int		myInputFormatDropdownWidth = 62;
}
