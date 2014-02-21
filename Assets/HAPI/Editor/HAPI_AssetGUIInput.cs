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
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using HAPI;

[ ExecuteInEditMode ]
[ CustomEditor( typeof( HAPI_AssetInput ) ) ]
public class HAPI_AssetGUIInput : HAPI_AssetGUI 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public

	public override void OnEnable()
	{
		myAssetInput = target as HAPI_AssetInput;
		myGeoAttributeManager = myAssetInput.prGeoAttributeManager;
		myGeoAttributeManagerGUI = new HAPI_GeoAttributeManagerGUI( myGeoAttributeManager );

		base.OnEnable();
	}

	public override void OnInspectorGUI()
	{
		if ( myGeoAttributeManager == null )
			return;

		bool is_editable = true;

		// We can only build or do anything if we can link to our dll which
		// can only happen on the Windows x86 platform.
#if !UNITY_STANDALONE_WIN
		is_editable = false;
		HAPI_GUI.help( HAPI_GUIUtility.myPlatformUnsupportedMessage, MessageType.Info );
#else
		if ( !is_editable )
			HAPI_GUI.help( "This mesh is not editable.", MessageType.Info );
#endif // !UNITY_STANDALONE_WIN

		bool gui_enable = GUI.enabled;
		GUI.enabled = is_editable;

		myAssetInput.prShowHoudiniControls 
			= HAPI_GUI.foldout( "Houdini Controls", myAssetInput.prShowHoudiniControls, true );
		if ( myAssetInput.prShowHoudiniControls ) 
		{
			if ( !myAssetInput.isPrefab() )
			{
				if ( GUILayout.Button( "Rebuild" ) ) 
					myAssetInput.buildAll();
	
				if ( GUILayout.Button( "Recook" ) )
					myAssetInput.buildClientSide();
			}
		}

		// Draw Help Pane
		myAssetInput.prShowHelp = HAPI_GUI.foldout( "Asset Help", myAssetInput.prShowHelp, true );
		if ( myAssetInput.prShowHelp )
			drawHelpBox( "No help yet. TODO!" );

		///////////////////////////////////////////////////////////////////////
		// Draw Asset Settings
		// These don't affect the asset directly so they don't trigger rebuilds.

		myAssetInput.prShowAssetSettings = HAPI_GUI.foldout( "Asset Settings", myAssetInput.prShowAssetSettings, true );
		if ( myAssetInput.prShowAssetSettings )
		{
			// Enable Cooking Toggle
			createToggleForProperty(
				"enable_cooking", "Enable Cooking", "prEnableCooking",
				ref myUndoInfo.enableCooking, null, !HAPI_Host.isEnableCookingDefault() );

			HAPI_GUI.separator();

			// Cooking Triggers Downstream Cooks Toggle
			createToggleForProperty(
				"cooking_triggers_downstream_cooks", "Cooking Triggers Downstream Cooks", 
				"prCookingTriggersDownCooks", ref myUndoInfo.cookingTriggersDownCooks,
				null, !HAPI_Host.isCookingTriggersDownCooksDefault(),
				!myAssetInput.prEnableCooking, " (all cooking is disabled)" );

			HAPI_GUI.separator();

			// Push Unity Transform To Houdini Engine Toggle
			createToggleForProperty(
				"push_unity_transform_to_houdini_engine", "Push Unity Transform To Houdini Engine", 
				"prPushUnityTransformToHoudini", ref myUndoInfo.pushUnityTransformToHoudini,
				null, !HAPI_Host.isPushUnityTransformToHoudiniDefault() );

			// Transform Change Triggers Cooks Toggle
			createToggleForProperty(
				"transform_change_triggers_cooks", "Transform Change Triggers Cooks", 
				"prTransformChangeTriggersCooks", ref myUndoInfo.transformChangeTriggersCooks,
				null, !HAPI_Host.isTransformChangeTriggersCooksDefault(),
				!myAssetInput.prEnableCooking, " (all cooking is disabled)" );
		}

		///////////////////////////////////////////////////////////////////////
		// Draw Point Attributes

		myAssetInput.prShowAttributesTable = HAPI_GUI.foldout( "Point Attributes", myAssetInput.prShowAttributesTable, true );
		if ( myAssetInput.prShowAttributesTable )
		{
			// Draw Create Point Attributes Action Bar
			{
				string[] preset_labels = new string[] {
					"Create Point Attribute:", "Custom", "Color", "UV", "Normal" };
				int[] preset_values = new int[] { 0, 1, 2, 3, 4 };

				GUIStyle style = new GUIStyle( EditorStyles.popup );
				style.fixedHeight = 18;
				style.margin.bottom = 6;
				int preset_selected = EditorGUILayout.IntPopup(
					0, preset_labels, preset_values, style );

				if ( preset_selected == 1 )
				{
					myGeoAttributeManager.createAttribute();
					myAssetInput.prHasAttributeChanges = true;
				}
				else if ( preset_selected > 1 )
				{
					HAPI_GeoAttribute.Preset preset = (HAPI_GeoAttribute.Preset) ( preset_selected - 2 );
					myGeoAttributeManager.createAttribute( preset );
					myAssetInput.prHasAttributeChanges = true;
				}
			}

			// Draw recook notice.
			string help_msg = "There are uncommitted attribute changes. Press 'Recook' when ready.";
			if ( myAssetInput.prHasError )
				help_msg += "\nError: " + myAssetInput.prErrorMsg;
			if ( myAssetInput.prHasAttributeChanges )
				HAPI_GUI.help( help_msg, MessageType.Info );
			else
				HAPI_GUI.help( "Ready", MessageType.Info );

			HAPI_GUI.separator();

			string[] type_labels = new string[] { "bool", "int", "float", "string" };
			int[] type_values = new int[] { 0, 1, 2, 3 };

			string[] tuple_labels = new string[] { "1", "2", "3", "4", "5" };
			int[] tuple_values = new int[] { 1, 2, 3, 4, 5 };

			string[] do_action_labels = new string[] {
				"Do:",
				"Fill (set all values to the paint value)",
				"Fit (recalculate the range to encompass all values)" };
			int[] do_action_values = new int[] { 0, 1, 2 };

			// Draw table header.
			{
				EditorGUILayout.BeginHorizontal();

				GUIStyle label_style = new GUIStyle( EditorStyles.label );
				label_style.padding.right = -5;
				label_style.margin.left = -5;
				label_style.border.right = -10;

				EditorGUILayout.LabelField( "Name", GUILayout.Width( 100 ) );
				EditorGUILayout.LabelField( "Type", GUILayout.Width( 50 ) );
				EditorGUILayout.LabelField( "Tuple", GUILayout.Width( 50 ) );
				EditorGUILayout.LabelField( "|", GUILayout.Width( 8 ) );
				EditorGUILayout.LabelField( "Min", GUILayout.MinWidth( 20 ) );
				EditorGUILayout.LabelField( "Max", GUILayout.MinWidth( 20 ) );
				EditorGUILayout.LabelField( "|", GUILayout.Width( 8 ) );
				EditorGUILayout.LabelField( "Action", GUILayout.Width( 81 ) );

				EditorGUILayout.EndHorizontal();
			}

			for ( int i = 0; i < myGeoAttributeManager.prAttributes.Count; ++i )
			{
				HAPI_GeoAttribute attrib = myGeoAttributeManager.prAttributes[ i ];

				EditorGUILayout.BeginHorizontal();

				// Attribute Name
				string new_name = EditorGUILayout.TextField( attrib.prName, GUILayout.Width( 100 ) );
				if ( new_name != attrib.prName )
				{
					attrib.prName = new_name;
					myAssetInput.prHasAttributeChanges = true;
				}

				// Attribute Type
				HAPI_GeoAttribute.Type new_attrib_type = (HAPI_GeoAttribute.Type) EditorGUILayout.IntPopup(
					(int) attrib.prType, type_labels, type_values, GUILayout.Width( 50 ) );
				if ( new_attrib_type != attrib.prType )
				{
					attrib.prType = new_attrib_type;
					myAssetInput.prHasAttributeChanges = true;
				}

				// Attribute Tuple Size
				int new_tuple_size = EditorGUILayout.IntPopup(
					attrib.prTupleSize, tuple_labels, tuple_values, GUILayout.Width( 50 ) );
				if ( new_tuple_size != attrib.prTupleSize )
				{
					attrib.prTupleSize = new_tuple_size;
					myAssetInput.prHasAttributeChanges = true;
				}

				EditorGUILayout.LabelField( "|", GUILayout.Width( 8 ) );

				// Range
				if ( attrib.prType == HAPI_GeoAttribute.Type.STRING )
				{
					EditorGUILayout.LabelField( "N/A", GUILayout.MinWidth( 20 ) );
				}
				else
				{
					if ( attrib.prType == HAPI_GeoAttribute.Type.BOOL || attrib.prType == HAPI_GeoAttribute.Type.INT )
						attrib.prIntMin = Mathf.Min(
							EditorGUILayout.IntField( "", attrib.prIntMin, GUILayout.MinWidth( 20 ) ),
							attrib.prIntMax );
					else if ( attrib.prType == HAPI_GeoAttribute.Type.FLOAT )
						attrib.prFloatMin = Mathf.Min(
							EditorGUILayout.FloatField( "", attrib.prFloatMin, GUILayout.MinWidth( 20 ) ),
							attrib.prFloatMax );

					if ( attrib.prType == HAPI_GeoAttribute.Type.BOOL || attrib.prType == HAPI_GeoAttribute.Type.INT )
						attrib.prIntMax = Mathf.Max(
							EditorGUILayout.IntField( "", attrib.prIntMax, GUILayout.MinWidth( 20 ) ),
							attrib.prIntMin );
					else if ( attrib.prType == HAPI_GeoAttribute.Type.FLOAT )
						attrib.prFloatMax = Mathf.Max(
							EditorGUILayout.FloatField( "", attrib.prFloatMax, GUILayout.MinWidth( 20 ) ),
							attrib.prFloatMin );
				}

				EditorGUILayout.LabelField( "|", GUILayout.Width( 8 ) );

				// Do Actions
				{
					int action = EditorGUILayout.IntPopup( 0, do_action_labels, do_action_values, GUILayout.Width( 50 ) );
					if ( action > 0 )
					{
						if ( action == 1 )
							attrib.fill();
						else if ( action == 2 )
							attrib.fit();
						myAssetInput.prGeoAttributeManager.refreshMesh();
					}
				}

				EditorGUILayout.LabelField( "|", GUILayout.Width( 8 ) );

				GUIStyle label_style = new GUIStyle( EditorStyles.label );
				label_style.fontStyle = FontStyle.Bold;
				if ( GUILayout.Button( "X", label_style, GUILayout.Width( 15 ), GUILayout.Height( 15 ) ) )
				{
					myGeoAttributeManager.deleteAttribute( attrib.prName );
					myAssetInput.prHasAttributeChanges = true;
				}

				EditorGUILayout.EndHorizontal();
			}

			{
				EditorGUILayout.BeginHorizontal();
				GUIStyle label_style = new GUIStyle( EditorStyles.label );
				label_style.fontStyle = FontStyle.Normal;
				label_style.alignment = TextAnchor.MiddleRight;
				label_style.padding.left = 0;
				label_style.margin.left = 0;
				label_style.padding.right = 0;
				label_style.margin.right = 0;
				EditorGUILayout.LabelField( "Delete Attribute", label_style, GUILayout.MinWidth( 40 ) );

				label_style.fontStyle = FontStyle.Bold;
				label_style.padding.left = 0;
				label_style.margin.left = 6;
				label_style.padding.right = 5;
				label_style.margin.right = 5;
				EditorGUILayout.LabelField( "↲", label_style, GUILayout.Width( 10 ) );

				EditorGUILayout.EndHorizontal();
			}
		} // Show Attributes Table

		GUI.enabled = gui_enable;
	}

	public override void OnSceneGUI()
	{
		base.OnSceneGUI();
		myGeoAttributeManagerGUI.OnSceneGUI();
		if ( myAssetInput.prGeoAttributeManager.prHasChanged && myAssetInput.prGeoAttributeManager.prLiveUpdates )
		{
			myAssetInput.prGeoAttributeManager.prHasChanged = false;
			myAssetInput.buildClientSide();
		}
	}

	private HAPI_AssetInput myAssetInput;
	private HAPI_GeoAttributeManager myGeoAttributeManager;
	private HAPI_GeoAttributeManagerGUI myGeoAttributeManagerGUI;
}
