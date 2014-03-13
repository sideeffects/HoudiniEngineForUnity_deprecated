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
		HAPI_GUI.help( HAPI_Constants.HAPI_UNSUPPORTED_PLATFORM_MSG, MessageType.Info );
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
			drawHelpBox( myHelpText );

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
					"Create Point Attribute:", "Custom", "Color", "UV", "Normal", "Bool", "Int", "Float", "String" };
				int[] preset_values = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };

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

			// Draw table header.
			{
				EditorGUILayout.BeginHorizontal();

				GUIStyle label_style = new GUIStyle( EditorStyles.label );
				label_style.padding.right = -5;
				label_style.margin.left = -5;
				label_style.border.right = -10;

				EditorGUILayout.LabelField( "Name", GUILayout.MinWidth( 100 ) );
				EditorGUILayout.LabelField( "Type", GUILayout.Width( 50 ) );
				EditorGUILayout.LabelField( "Tuple", GUILayout.Width( 50 ) );
				EditorGUILayout.LabelField( "|", GUILayout.Width( 8 ) );
				EditorGUILayout.LabelField( "Min", GUILayout.Width( 50 ) );
				EditorGUILayout.LabelField( "Max", GUILayout.Width( 50 ) );
				EditorGUILayout.LabelField( "", GUILayout.Width( 27 ) );

				EditorGUILayout.EndHorizontal();
			}

			for ( int i = 0; i < myGeoAttributeManager.prAttributes.Count; ++i )
			{
				HAPI_GeoAttribute attrib = myGeoAttributeManager.prAttributes[ i ];

				EditorGUILayout.BeginHorizontal();

				// Attribute Name
				string new_name = EditorGUILayout.TextField( attrib.prName, GUILayout.MinWidth( 100 ) );
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
						attrib.prIntMin =
							EditorGUILayout.IntField( "", attrib.prIntMin, GUILayout.Width( 50 ) );
					else if ( attrib.prType == HAPI_GeoAttribute.Type.FLOAT )
						attrib.prFloatMin =
							EditorGUILayout.FloatField( "", attrib.prFloatMin, GUILayout.Width( 50 ) );

					if ( attrib.prType == HAPI_GeoAttribute.Type.BOOL || attrib.prType == HAPI_GeoAttribute.Type.INT )
						attrib.prIntMax =
							EditorGUILayout.IntField( "", attrib.prIntMax, GUILayout.Width( 50 ) );
					else if ( attrib.prType == HAPI_GeoAttribute.Type.FLOAT )
						attrib.prFloatMax =
							EditorGUILayout.FloatField( "", attrib.prFloatMax, GUILayout.Width( 50 ) );
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

	private const string myHelpText = 
		"- Instantiation -\n" +
		"\n" +
		"As soon as this component is attached to a valid GameObject (one " +
		"that has a MeshFilter with a valid mesh) a Houdini asset will be " +
		"instantiated that will contain the full mesh and you'll be able to " +
		"connect this asset as geometry or transfrom input to any other " +
		"Houdini Engine asset loaded via OTL.\n" +
		"\n" +
		"- Attributes -\n" +
		"\n" +
		"Add Attribute:\n" +
		"    1. Expand the 'Point Attributes' foldout.\n" +
		"    2. Click on 'Create Point Attribute:'.\n" +
		"    3. Choose a preset or choose 'Custom'.\n" +
		"\n" +
		"Modify Attributes:\n" +
		"    1. Change either the name, type, tuple, min, or max.\n" +
		"        - Note that you cannot have an attribute name P or two attributes with the same name.\n" +
		"    2. Click on the 'Recook' button under 'Houdini Controls'.\n" +
		"\n" +
		"Delete Attribute:\n" +
		"    1. Click on the little 'X' beside the attribute to delete.\n" +
		"    2. Click on the 'Recook' button under 'Houdini Controls'.\n" +
		"\n" +
		"Paint Attributes:\n" +
		"    1. In the Scene window, at the bottom, click the 'Paint' button.\n" +
		"    2. At the top of the Scene window, select which attribute to paint.\n" +
		"    3. Also at the top, you can choose between painting 3 components at once or just one component at a time.\n" +
		"    4. Furthermore, at the top is also where you choose the value to paint. To fill paint the whole surface with this value click the 'Fill' button to the right of the paint value fields.\n" +
		"    5. To the right of the value is the 'Rate' setting which is a percentage of the max-min that will be applied on each click/stroke until the paint value is reached.\n" +
		"    6. Finally, at the top right corner is the 'Live Updates' toggle which controls whether to cook the asset on every brush stroke or not.\n" +
		"    7. You can also resize the brush by holding down Shift and dragging the mouse horizontally (the Maya and Houdini brush resize hotkeys also work).\n" +
		"    8. Left-click drag on the mesh surface to paint the paint value.\n" +
		"    9. Right-click drag on the mesh surface to paint the inverse paint value, respective to the min/max settings.\n";
}
