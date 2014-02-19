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

		myAssetInput.prShowAttributesTable = HAPI_GUI.foldout( "Custom Attributes", myAssetInput.prShowAttributesTable, true );
		if ( myAssetInput.prShowAttributesTable )
		{

			if ( GUILayout.Button( "Create Attribute" ) )
			{
				myGeoAttributeManager.createAttribute();
				myAssetInput.prHasAttributeChanges = true;
			}

			// Draw recook notice.
			if ( myAssetInput.prHasAttributeChanges )
				HAPI_GUI.help( "There are uncommitted attribute changes. Press 'Recook' when ready.", MessageType.Info );
			else
				HAPI_GUI.help( "Ready", MessageType.Info );

			HAPI_GUI.separator();

			string[] type_labels = new string[] { "bool", "int", "float" };
			int[] type_values = new int[] { 0, 1, 2 };

			string[] tuple_labels = new string[] { "1", "2", "3", "4", "5" };
			int[] tuple_values = new int[] { 1, 2, 3, 4, 5 };

			// Draw table header.
			{
				EditorGUILayout.BeginHorizontal();

				GUIStyle label_style = new GUIStyle( EditorStyles.label );
				label_style.padding.right = -5;
				label_style.margin.left = -5;
				label_style.border.right = -10;

				EditorGUILayout.LabelField( "Name", GUILayout.Width( 100 ) );
				EditorGUILayout.LabelField( "Type", GUILayout.Width( 40 ) );
				EditorGUILayout.LabelField( "Tuple", GUILayout.Width( 40 ) );
				EditorGUILayout.LabelField( "|", GUILayout.Width( 8 ) );
				EditorGUILayout.LabelField( "Range", label_style, GUILayout.MinWidth( 20 ) );

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
					(int) attrib.prType, type_labels, type_values, GUILayout.Width( 40 ) );
				if ( new_attrib_type != attrib.prType )
				{
					attrib.prType = new_attrib_type;
					myAssetInput.prHasAttributeChanges = true;
				}

				// Attribute Tuple Size
				int new_tuple_size = EditorGUILayout.IntPopup(
					attrib.prTupleSize, tuple_labels, tuple_values, GUILayout.Width( 40 ) );
				if ( new_tuple_size != attrib.prTupleSize )
				{
					attrib.prTupleSize = new_tuple_size;
					myAssetInput.prHasAttributeChanges = true;
				}

				EditorGUILayout.LabelField( "|", GUILayout.Width( 8 ) );

				{
					EditorGUILayout.BeginVertical();

					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField( "Min", GUILayout.Width( 26 ) );
					for ( int j = 0; j < attrib.prTupleSize; ++j )
					{
						if ( attrib.prType == HAPI_GeoAttribute.Type.BOOL || attrib.prType == HAPI_GeoAttribute.Type.INT )
							attrib.prIntMins[ j ] = EditorGUILayout.IntField(
								"", attrib.prIntMins[ j ],
								GUILayout.MinWidth( 20 ), GUILayout.MaxWidth( 120 ) );
						else if ( attrib.prType == HAPI_GeoAttribute.Type.FLOAT )
							attrib.prFloatMins[ j ] = EditorGUILayout.FloatField(
								"", attrib.prFloatMins[ j ],
								GUILayout.MinWidth( 20 ), GUILayout.MaxWidth( 120 ) );
					}
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.LabelField( "Max", GUILayout.Width( 26 ) );
					for ( int j = 0; j < attrib.prTupleSize; ++j )
					{
						if ( attrib.prType == HAPI_GeoAttribute.Type.BOOL || attrib.prType == HAPI_GeoAttribute.Type.INT )
							attrib.prIntMaxes[ j ] = EditorGUILayout.IntField(
								"", attrib.prIntMaxes[ j ],
								GUILayout.MinWidth( 20 ), GUILayout.MaxWidth( 120 ) );
						else if ( attrib.prType == HAPI_GeoAttribute.Type.FLOAT )
							attrib.prFloatMaxes[ j ] = EditorGUILayout.FloatField(
								"", attrib.prFloatMaxes[ j ],
								GUILayout.MinWidth( 20 ), GUILayout.MaxWidth( 120 ) );
					}
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.EndVertical();
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

				HAPI_GUI.separator();
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
