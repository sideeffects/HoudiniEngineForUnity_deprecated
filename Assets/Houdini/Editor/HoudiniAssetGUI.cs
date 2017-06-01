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
#if !( HAPI_ENABLE_RUNTIME )
		HoudiniGUI.help( HoudiniConstants.HAPI_UNSUPPORTED_PLATFORM_MSG, MessageType.Info );
		GUI.enabled = false;
#else
		if ( !HoudiniHost.isInstallationOk() )
		{
			HoudiniGUI.help( HoudiniHost.getMissingEngineInstallHelpString(), MessageType.Info );
			GUI.enabled = false;
		}
#endif // !( HAPI_ENABLE_RUNTIME )

		try
		{
			myDelayBuild	= false;
			myParmChanges	= false;

			///////////////////////////////////////////////////////////////////////
			// Draw License/Logo Thingy
#if ( HAPI_ENABLE_RUNTIME )
			drawLicenseLogo();
#endif

			///////////////////////////////////////////////////////////////////////
			// Draw Game Object Controls

			if ( HoudiniHost.isNodeValid( myAsset.prAssetId, myAsset.prNodeInfo.uniqueHoudiniNodeId ) &&
				( myAsset.prTransformInputCount > 0 || myAsset.prGeoInputCount > 0 ) &&
				 myAsset.prAssetType != HoudiniAsset.AssetType.TYPE_CURVE )
			{
				myAsset.prShowInputControls = HoudiniGUI.foldout( "Inputs", myAsset.prShowInputControls, true );
			
				if ( myAsset.prShowInputControls )
				{
					if ( myAsset.prNodeInfo.type == HAPI_NodeType.HAPI_NODETYPE_OBJ )
						for ( int ii = 0; ii < myAsset.prTransformInputCount; ++ii )
							myParmChanges |= setTransformInput( ii );
					
					if ( myAsset.prUpStreamGeoObjects == null || myAsset.prUpStreamGeoAssets == null ||
						 myAsset.prUpStreamGeoObjects.Count <= 0 || myAsset.prUpStreamGeoAssets.Count <= 0 )
						myAsset.prGeoInputCount = 0;

					for ( int input_index = 0; input_index < myAsset.prGeoInputCount; ++input_index )
					{
						bool join_last = false;
						bool no_label_toggle_last = true;

                        GameObject temp_obj = null;
                        HoudiniGUIParm geo_input = new HoudiniGUIParm(
							"geo_input_" + input_index, myAsset.prGeoInputNames[ input_index ] );
						Object obj = (Object) myAsset.prUpStreamGeoObjects[ input_index ];
						myParmChanges |= HoudiniGUI.objectField(
							ref geo_input, ref obj, typeof( GameObject ), ref join_last, ref no_label_toggle_last, null, ref temp_obj );

						HoudiniGUIParm geo_input_transform_type = new HoudiniGUIParm(
							"geo_input_transform_type_" + input_index, "Keep world transform" );

						bool transform_type = myAsset.prGeoInputsTransformTypes[input_index] != 0;
						bool TransformTypeUpdate = HoudiniGUI.toggle(ref geo_input_transform_type, ref transform_type);

						bool need_build_client_side = false;
						if ( myParmChanges || !myAsset.isGeoInputValid( input_index ) )
						{
							if ( !obj )
							{
								myAsset.removeGeoInput( input_index );
								//myAsset.buildClientSide();
								need_build_client_side = true;
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
									// Connecting a new game object
									myAsset.addGeoAsGeoInput( new_obj, input_index );
									myAsset.updateGeoInputTransformType( input_index, transform_type ? 1 : 0 );
									need_build_client_side = true;
									//myAsset.buildClientSide();
								}
								else if ( myAsset.prUpStreamGeoAssets[ input_index ] != asset )
								{
									// Connecting a new asset
									if ( myAsset == asset )
										Debug.LogError( "Can't connect an asset to itself!" );
									else
									{
										myAsset.addAssetAsGeoInput( asset, object_index, input_index );
										myAsset.updateGeoInputTransformType( input_index, transform_type ? 1 : 0 );
										need_build_client_side = true;
										//myAsset.buildClientSide();
									}
								}
							}
						}

						if ( TransformTypeUpdate )
						{
							myAsset.updateGeoInputTransformType(input_index, transform_type ? 1 : 0);
							need_build_client_side = true;
						}

						if ( need_build_client_side )
							myAsset.buildClientSide();
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
		GameObject temp_obj         = null;

		HoudiniGUIParm trans_input	= new HoudiniGUIParm( "trans_input_" + index, myAsset.prTransInputNames[ index ] );
		Object obj					= (Object) myAsset.prUpStreamTransformObjects[ index ];
		bool changed				= HoudiniGUI.objectField( ref trans_input, ref obj, typeof( GameObject ),
															ref join_last, ref no_label_toggle_last, null, ref temp_obj );

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

	protected void drawLicenseLogo()
	{
		if ( HoudiniHost.getCurrentLicense() != HAPI_License.HAPI_LICENSE_HOUDINI_ENGINE_INDIE )
			return;

		HoudiniGUI.separator();

		int skin = EditorPrefs.GetInt( "UserSkin" );
		bool is_light_skin = skin == 0;

#if false
		if ( myDarkSkinLogo == null && is_dark_skin )
			myDarkSkinLogo = Resources.Load< Texture2D >( "hEngine_white_color" );
		if ( myLightSkinLogo == null && is_light_skin )
			myLightSkinLogo = Resources.Load< Texture2D >( "hEngine_black_color" );
		Texture2D logo = ( is_light_skin ? myLightSkinLogo : myDarkSkinLogo );

		float pane_width = (float) Screen.width - 60;
		float ratio = Mathf.Min( 0.2f, pane_width / logo.width );

		GUIStyle image_style = new GUIStyle( GUI.skin.label );
		image_style.normal.background = logo;
		image_style.imagePosition = ImagePosition.ImageAbove;
		EditorGUILayout.LabelField(
			"", image_style,
			GUILayout.Height( logo.height * ratio ),
			GUILayout.Width( logo.width * ratio ) );
#endif

		GUIStyle label_style = new GUIStyle( GUI.skin.label );
		label_style.fontStyle = FontStyle.Bold;
		label_style.normal.textColor = is_light_skin ? Color.red : Color.yellow;
		EditorGUILayout.LabelField( "Houdini Engine Indie - For Limited Commercial Use Only", label_style );

		HoudiniGUI.separator();
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
			bool changed = HoudiniGUI.toggle( name, label, false, ref value, myUndoInfo, ref undo_info_value );

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

#if false
	private static Texture2D myLightSkinLogo = null;
	private static Texture2D myDarkSkinLogo = null;
#endif
}
