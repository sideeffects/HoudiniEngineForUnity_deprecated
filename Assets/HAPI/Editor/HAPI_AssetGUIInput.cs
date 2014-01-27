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

		if ( GUILayout.Button( "Build" ) )
			myAssetInput.buildAll();

		HAPI_GUI.separator();

		if ( GUILayout.Button( "Create Attribute" ) )
			myGeoAttributeManager.createAttribute();

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

			EditorGUILayout.LabelField( "", GUILayout.Width( 18 ) );
			EditorGUILayout.LabelField( "Name", GUILayout.Width( 100 ) );
			EditorGUILayout.LabelField( "Type", GUILayout.Width( 40 ) );
			EditorGUILayout.LabelField( "Tuple", GUILayout.Width( 42 ) );
			EditorGUILayout.LabelField( "|", GUILayout.Width( 8 ) );
			EditorGUILayout.LabelField( "Range", label_style, GUILayout.MinWidth( 20 ) );

			EditorGUILayout.EndHorizontal();
		}

		for ( int i = 0; i < myGeoAttributeManager.prAttributes.Count; ++i )
		{
			HAPI_GeoAttribute attrib = myGeoAttributeManager.prAttributes[ i ];

			EditorGUILayout.BeginHorizontal();

			// Draw toggle to control the active attribute.
			if ( EditorGUILayout.Toggle( 
				"", myGeoAttributeManager.prActiveAttribute && myGeoAttributeManager.prActiveAttribute.prName == attrib.prName,
				GUILayout.Width( 20 ) ) )
			{
				myGeoAttributeManager.setActiveAttribute( attrib.prName );
			}

			// Attribute Name
			string new_name = EditorGUILayout.TextField( attrib.prName, GUILayout.Width( 100 ) );
			attrib.prName = new_name;

			// Attribute Type
			attrib.prType = (HAPI_GeoAttribute.Type) EditorGUILayout.IntPopup(
				(int) attrib.prType, type_labels, type_values, GUILayout.Width( 40 ) );

			// Attribute Tuple Size
			attrib.prTupleSize = EditorGUILayout.IntPopup(
				attrib.prTupleSize, tuple_labels, tuple_values, GUILayout.Width( 40 ) );

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
				myGeoAttributeManager.deleteAttribute( attrib.prName );

			EditorGUILayout.EndHorizontal();

			HAPI_GUI.separator();
		}

		{
			EditorGUILayout.BeginHorizontal();
			GUIStyle label_style = new GUIStyle( EditorStyles.label );
			label_style.fontStyle = FontStyle.Bold;
			EditorGUILayout.LabelField( "↳", label_style, GUILayout.Width( 10 ) );
			EditorGUILayout.LabelField( "Active Attribute to be Painted", GUILayout.MinWidth( 40 ) );

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

		GUI.enabled = gui_enable;
	}

	public override void OnSceneGUI()
	{
		base.OnSceneGUI();
		myGeoAttributeManagerGUI.OnSceneGUI();
	}

	private HAPI_AssetInput myAssetInput;
	private HAPI_GeoAttributeManager myGeoAttributeManager;
	private HAPI_GeoAttributeManagerGUI myGeoAttributeManagerGUI;
}
