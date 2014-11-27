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

[ CustomEditor( typeof( HoudiniAsset ) ) ]
public class HoudiniAssetGUI : Editor 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	public virtual void OnEnable() 
	{
		myAsset 			= target as HoudiniAsset;
		myUndoInfo			= myAsset.prAssetUndoInfo;
		
		myParmChanges		= true;
		myUnbuiltChanges 	= false;
		myFocusChanged 		= true;

		myHelpScrollPosition = new Vector2( 0.0f, 0.0f );

		HoudiniHost.myRepaintDelegate += this.refresh;
		HoudiniHost.myDeselectionDelegate += this.deselect;
		HoudiniHost.mySelectionTarget = myAsset.gameObject;
		
		// If selection is a prefab build it ( parameters only ) to allow 
		// editing of parameters. Only need to build once, the first time
		// it is selected.
		if ( myAsset.isPrefab() )
		{
			// Reloading prefab after play mode change since OnEnable
			// never gets called for prefab. This only needs to be done
			// if prefab has already been built.
			if ( myAsset.prReloadPrefabOnPlaymodeChange &&
				 HoudiniHost.isAssetValid( myAsset.prAssetId, myAsset.prAssetValidationId ) )
			{
				myAsset.prReloadPrefabOnPlaymodeChange = false;
				myAsset.build(	false,	// reload_asset
								false,	// unload_asset_first
								true,	// serializatin_recovery_only
								false,	// force_reconnect
								false,	// is_duplication
								false,	// cook_downstream_assets
								false	// use_delay_for_progress_bar
					 		 );
			}
			else if (
				myAsset.prAssetId != myAsset.prBackupAssetId ||
				!HoudiniHost.isAssetValid( myAsset.prAssetId, myAsset.prAssetValidationId ) )
			{
				myAsset.prAssetId = -1;
				myAsset.build( true,	// reload_asset
							   true,	// unload_asset_first
							   true,	// serializatin_recovery_only
							   false,	// force_reconnect
							   false,	// is_duplication
							   false,	// cook_downstream_assets
							   false	// use_delay_for_progress_bar
					 		 );
				EditorUtility.SetDirty( myAsset );
			}
		}
	}

	public virtual void OnDisable()
	{
		HoudiniHost.myRepaintDelegate -= this.refresh;
		HoudiniHost.myDeselectionDelegate -= this.deselect;
		HoudiniHost.mySelectionTarget = null;
	}

	public virtual void refresh()
	{
		Repaint();
		SceneView.lastActiveSceneView.Repaint();
	}

	public virtual void deselect()
	{
		if ( HoudiniHost.mySelectionTarget == myAsset.gameObject )
			HoudiniHost.mySelectionTarget = null;
	}

	public override void OnInspectorGUI() 
	{
		bool gui_enable = GUI.enabled;

		// We can only build or do anything if we can link to our libraries.
#if !( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HoudiniGUI.help( HoudiniConstants.HAPI_UNSUPPORTED_PLATFORM_MSG, MessageType.Info );
		GUI.enabled = false;
#else
		if ( !HoudiniHost.isInstallationOk() )
		{
			HoudiniGUI.help( HoudiniHost.getMissingEngineInstallHelpString(), MessageType.Info );
			GUI.enabled = false;
		}
#endif // !( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )

		try
		{
			myDelayBuild	= false;
			myParmChanges	= false;

			///////////////////////////////////////////////////////////////////////
			// Draw Game Object Controls

			if ( HoudiniHost.isAssetValid( myAsset.prAssetId, myAsset.prAssetValidationId ) &&
				( myAsset.prTransformInputCount > 0 || myAsset.prGeoInputCount > 0 ) &&
				 myAsset.prAssetSubType != HAPI_AssetSubType.HAPI_ASSETSUBTYPE_CURVE &&
				 !myAsset.isPrefab() )
			{
				myAsset.prShowInputControls = HoudiniGUI.foldout( "Inputs", myAsset.prShowInputControls, true );
			
				if ( myAsset.prShowInputControls )
				{
					if ( myAsset.prHAPIAssetType == HAPI_AssetType.HAPI_ASSETTYPE_OBJ )
						for ( int ii = 0; ii < myAsset.prTransformInputCount; ++ii )
							myParmChanges |= setTransformInput( ii );
					
					if ( myAsset.prUpStreamGeoObjects == null || myAsset.prUpStreamGeoAssets == null ||
						 myAsset.prUpStreamGeoObjects.Count <= 0 || myAsset.prUpStreamGeoAssets.Count <= 0 )
						return;

					for ( int input_index = 0; input_index < myAsset.prGeoInputCount; ++input_index )
					{
						bool join_last = false;
						bool no_label_toggle_last = true;

						HoudiniGUIParm geo_input = new HoudiniGUIParm(
							"geo_input_" + input_index, myAsset.prGeoInputNames[ input_index ] );
						Object obj = (Object) myAsset.prUpStreamGeoObjects[ input_index ];
						myParmChanges |= HoudiniGUI.objectField(
							ref geo_input, ref obj, typeof( GameObject ), ref join_last, ref no_label_toggle_last );
					
						if ( myParmChanges || !myAsset.isGeoInputValid( input_index ) )
						{
							if ( !obj )
							{
								myAsset.removeGeoInput( input_index );
								myAsset.buildClientSide();
							}
							else
							{
								GameObject new_obj = (GameObject) obj;
								myAsset.prUpStreamGeoObjects[ input_index ] = new_obj;

								// Select the asset component (if it exists).
								HoudiniAsset asset = new_obj.GetComponent< HoudiniAsset >();

								// If we're selecting a specific object to input than try and
								// get the object id. Note that by getting the HAPI_ObjectControl
								// component we also cover the geo and part controls because
								// they all inherit from HAPI_ObjectControl. The user can therefore
								// drag any gameObject under the asset into another asset's
								// input and have it all work.
								int object_index = 0;
								HoudiniObjectControl obj_control = new_obj.GetComponent< HoudiniObjectControl >();
								if ( obj_control )
								{
									object_index = obj_control.prObjectId;
									asset = obj_control.prAsset;
								}

								if ( asset == null )
								{
									myAsset.addGeoAsGeoInput( new_obj, input_index );
									myAsset.buildClientSide();
								}
								else if ( myAsset.prUpStreamGeoAssets[ input_index ] != asset )
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
					} // for
				} // if
			} // if

			// Draw Cook Log Pane
			myAsset.prShowCookLog = HoudiniGUI.foldout( "Asset Cook Log", myAsset.prShowCookLog, true );
			if ( myAsset.prShowCookLog )
				drawCookLog();
		}
		catch ( HoudiniError e )
		{
			Debug.LogError( e.ToString() );
		}

		GUI.enabled = gui_enable;
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

		HoudiniGUIParm trans_input	= new HoudiniGUIParm( "trans_input_" + index, myAsset.prTransInputNames[ index ] );
		Object obj					= (Object) myAsset.prUpStreamTransformObjects[ index ];
		bool changed				= HoudiniGUI.objectField( ref trans_input, ref obj, typeof( GameObject ),
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
				HoudiniAsset input_asset = game_obj.GetComponent< HoudiniAsset >();
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

	protected void drawCookLog()
	{
		if ( HoudiniGUI.button( "get_cook_log", "Get Cook Log" ) )
		{
			myAsset.buildClientSide();
			myLastCookLog = HoudiniHost.getStatusString(
				HAPI_StatusType.HAPI_STATUS_COOK_RESULT,
				HAPI_StatusVerbosity.HAPI_STATUSVERBOSITY_MESSAGES );
		}

		float width = (float) Screen.width - 60;

		myCookLogScrollPosition = EditorGUILayout.BeginScrollView(
			myCookLogScrollPosition, GUILayout.Height( 200 ) );

		GUIStyle sel_label = new GUIStyle( GUI.skin.label );
		sel_label.stretchWidth = true;
		sel_label.wordWrap = true;

		float height = sel_label.CalcHeight( 
			new GUIContent( myLastCookLog ), width );

		EditorGUILayout.SelectableLabel( 
			myLastCookLog, sel_label, GUILayout.Width( width ),
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
			PropertyInfo property = typeof( HoudiniAsset ).GetProperty( property_name );
			if ( property == null )
				throw new HoudiniErrorInvalidArgument( property_name + " is not a valid property of HAPI_Asset!" );
			if ( property.PropertyType != typeof( bool ) )
				throw new HoudiniErrorInvalidArgument( property_name + " is not a boolean!" );

			GUI.enabled = !global_overwrite && !local_overwrite && GUI.enabled;

			if ( global_overwrite )
				label += " (overwritted by global setting)";
			else if ( local_overwrite )
				label += local_overwrite_message;

			bool value = ( bool ) property.GetValue( myAsset, null );
			bool is_bold = myParentPrefabAsset && ( bool ) property.GetValue( myParentPrefabAsset, null ) != value;
			bool changed = HoudiniGUI.toggle( name, label, is_bold, ref value, myUndoInfo, ref undo_info_value );

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

	protected HoudiniAsset 	myAsset;
	protected bool			myDelayBuild;
	protected bool			myParmChanges;
	protected bool			myUnbuiltChanges;
	protected bool 			myFocusChanged;

	protected Vector2 myCookLogScrollPosition = new Vector2( 0.0f, 0.0f );
	protected string myLastCookLog = "";
	
	protected Vector2 myHelpScrollPosition = new Vector2( 0.0f, 0.0f );

	protected HoudiniAssetUndoInfo myUndoInfo;
	protected HoudiniAsset myParentPrefabAsset;

	private const int		myInputFormatDropdownWidth = 62;
}
