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
using HAPI;

[ CustomEditor( typeof( HAPI_AssetOTL ) ) ]
public partial class HAPI_AssetGUIOTL : HAPI_AssetGUI 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	public override void OnEnable() 
	{
		base.OnEnable();
		myAssetOTL = myAsset as HAPI_AssetOTL;
		myUndoInfo = myAssetOTL.prAssetOTLUndoInfo;
		myHelpScrollPosition = new Vector2( 0.0f, 0.0f );
	}
	
	public override void OnInspectorGUI() 
	{
		base.OnInspectorGUI();

		if ( myAssetOTL.isPrefabInstance() )
			myParentPrefabAsset = myAsset.getParentPrefabAsset();

		///////////////////////////////////////////////////////////////////////
		// Draw Game Object Controls
		

		myAssetOTL.prShowHoudiniControls 
			= HAPI_GUI.foldout( "Houdini Controls", myAssetOTL.prShowHoudiniControls, true );
		if ( myAssetOTL.prShowHoudiniControls ) 
		{
			if ( !myAssetOTL.isPrefab() )
			{
				if ( GUILayout.Button( "Rebuild" ) ) 
					myAssetOTL.buildAll();
	
				if ( GUILayout.Button( "Recook" ) )
					myAssetOTL.buildClientSide();

				if ( GUILayout.Button( "Bake" ) )
					myAssetOTL.bakeAsset();
			}
		} // if

		// Draw Help Pane
		myAssetOTL.prShowHelp = HAPI_GUI.foldout( "Asset Help", myAssetOTL.prShowHelp, true );
		if ( myAssetOTL.prShowHelp )
		{
			myHelpScrollPosition = EditorGUILayout.BeginScrollView(
				myHelpScrollPosition, GUILayout.Height( 200 ) );
			float height = GUI.skin.label.CalcHeight( 
				new GUIContent( myAssetOTL.prAssetHelp ), (float) Screen.width );
			GUIStyle sel_label = GUI.skin.label;
			sel_label.stretchWidth = true;
			sel_label.wordWrap = true;
			EditorGUILayout.SelectableLabel( 
				myAssetOTL.prAssetHelp, sel_label, GUILayout.Height( height ), 
				GUILayout.Width( Screen.width - 40 ) );
			EditorGUILayout.EndScrollView();
		}
		
		///////////////////////////////////////////////////////////////////////
		// Draw Asset Settings
		// These don't affect the asset directly so they don't trigger rebuilds.

		myAssetOTL.prShowAssetSettings = HAPI_GUI.foldout( "Asset Settings", myAssetOTL.prShowAssetSettings, true );
		if ( myAssetOTL.prShowAssetSettings )
			generateAssetSettings();

		///////////////////////////////////////////////////////////////////////
		// Draw Baking Controls
		
		if( !myAssetOTL.isPrefab() )
		{
			myAssetOTL.prShowBakeOptions = HAPI_GUI.foldout( "Bake Animations", myAssetOTL.prShowBakeOptions, true );
			if ( myAssetOTL.prShowBakeOptions )
				generateAssetBakeControls();
		}

		HAPI_Instancer[] instancers = myAssetOTL.gameObject.GetComponentsInChildren< HAPI_Instancer >();
		if( !myAssetOTL.isPrefab() && instancers.Length > 0 )
		{
			myAssetOTL.prShowInstanceControls = HAPI_GUI.foldout( "Instancing Controls", 
			                                                      myAssetOTL.prShowInstanceControls, true );
			if ( myAssetOTL.prShowInstanceControls )
				generateAssetInstanceControls();
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private
	
	private void generateAssetBakeControls()
	{
		// Start Time
		{
			float value = myAsset.prBakeStartTime;
			bool changed = HAPI_GUI.floatField( "bake_start_time", "Start Time", ref value, 
			                                    myUndoInfo, ref myUndoInfo.bakeStartTime );
			if ( changed )
				myAsset.prBakeStartTime = value;
		}
		
		// End Time
		{
			float value = myAsset.prBakeEndTime;
			bool changed = HAPI_GUI.floatField( "bake_end_time", "End Time", ref value, 
			                                    myUndoInfo, ref myUndoInfo.bakeEndTime );
			if ( changed )
				myAsset.prBakeEndTime = value;
		}
		
		// Samples per second
		{
			int value = myAsset.prBakeSamplesPerSecond;
			bool changed = HAPI_GUI.intField( "bake_samples_per_second", "Samples Per Second", ref value,
			                                  1, 120, myUndoInfo, ref myUndoInfo.bakeSamplesPerSecond );

			if ( changed )
				myAsset.prBakeSamplesPerSecond = value;
		}
		
		if ( GUILayout.Button( "Bake" ) ) 
		{
			HAPI_ProgressBar progress_bar = new HAPI_ProgressBar();
			myAsset.bakeAnimations( myAsset.prBakeStartTime, 
									myAsset.prBakeEndTime, 
									myAsset.prBakeSamplesPerSecond, 
									myAsset.gameObject,
									progress_bar );
			progress_bar.clearProgressBar();
		}
	}

	private void generateAssetInstanceControls()
	{
		HAPI_Instancer[] instancers = myAssetOTL.gameObject.GetComponentsInChildren< HAPI_Instancer >();

		foreach( HAPI_Instancer instancer in instancers )
		{
			bool changed = false;
			
			{
				List< string > unique_names = instancer.prUniqueInstantiatedNames;
				
				for( int ii = 0; ii < unique_names.Count; ii++ )
				{
					string instanced_name = unique_names[ ii ];
					
					Object obj = (Object) instancer.getUserObjToInstantiateFromName( instanced_name );
					changed |= HAPI_GUI.objectField( "object_to_instantiate", "Replace " + instanced_name, 
					                                ref obj, typeof( GameObject ) );
					if( changed )
					{
						instancer.prObjsToInstantiate[ ii ] = (GameObject) obj;
					}
				}

			}
			
			
			if ( instancer.hasOverriddenInstances() )
			{
				if ( GUILayout.Button( "UnPin All Instances" ) ) 
				{
					instancer.unPinAllInstances();
					changed = true;
				}
			}
			
			if ( changed )
			{
				HAPI_ProgressBar progress_bar = new HAPI_ProgressBar();
				instancer.instanceObjects( progress_bar );
				progress_bar.clearProgressBar();
			}

			EditorGUILayout.Separator();
		}
	}

	private void generateViewSettings()
	{	
		// Show Geometries
		createToggleForProperty( "show_geometries", "Show Geometries", "prIsGeoVisible", 
		                         ref myUndoInfo.isGeoVisible, () => myAsset.applyGeoVisibilityToParts() );
		
		// Show Pinned Instances
		createToggleForProperty( "show_pinned_instances", "Show Pinned Instances", "prShowPinnedInstances", 
		                         ref myUndoInfo.showPinnedInstances, null );

		// Auto Select Asset Root Node Toggle
		createToggleForProperty( "auto_select_asset_root_node", "Auto Select Asset Root Node", 
			                     "prAutoSelectAssetRootNode", ref myUndoInfo.autoSelectAssetRootNode,
		                         null, !HAPI_Host.isAutoSelectAssetRootNodeDefault() );
		
		// Hide When Fed to Other Asset
		createToggleForProperty( "hide_geometry_on_linking", "Hide Geometry On Linking", "prHideGeometryOnLinking",
		                         ref myUndoInfo.hideGeometryOnLinking, null, !HAPI_Host.isHideGeometryOnLinkingDefault() );
	}
	
	private void generateMaterialSettings()
	{	
		if ( GUILayout.Button( "Re-Render" ) ) 
		{
			HAPI_AssetUtility.reApplyMaterials( myAsset );
		}

		// Material Shader Type
		{
			HAPI_ShaderType value = myAsset.prMaterialShaderType;
			bool is_bold = myParentPrefabAsset && myParentPrefabAsset.prMaterialShaderType != value;
			string[] labels = { "OpenGL", "Houdini Mantra Renderer" };
			HAPI_ShaderType[] values = { HAPI_ShaderType.HAPI_SHADER_OPENGL, HAPI_ShaderType.HAPI_SHADER_MANTRA };
			bool changed = HAPI_GUI.dropdown( "material_renderer", "Material Renderer", ref value, 
			                                  is_bold, labels, values, myUndoInfo, 
			                                  ref myUndoInfo.materialShaderType );
			if ( changed )
			{
				myAsset.prMaterialShaderType = (HAPI_ShaderType) value;
				HAPI_AssetUtility.reApplyMaterials( myAsset );
			}
		}

		// Render Resolution
		{
			bool delay_build = false;
			int[] values 			= new int[ 2 ];
			values[ 0 ] 			= (int) myAsset.prRenderResolution[ 0 ];
			values[ 1 ] 			= (int) myAsset.prRenderResolution[ 1 ];
			int[] undo_values 		= new int[ 2 ];
			undo_values[ 0 ] 		= (int) myUndoInfo.renderResolution[ 0 ];
			undo_values[ 1 ] 		= (int) myUndoInfo.renderResolution[ 1 ];
			HAPI_GUIParm gui_parm 	= new HAPI_GUIParm( "render_resolution", "Render Resolution", 2 );
			gui_parm.isBold 		= myParentPrefabAsset && 
							  		  (int) myParentPrefabAsset.prRenderResolution[ 0 ] != values[ 0 ] &&
							  		  (int) myParentPrefabAsset.prRenderResolution[ 1 ] != values[ 1 ];
			bool changed = HAPI_GUI.intField( ref gui_parm, ref delay_build, ref values, 
			                                  myUndoInfo, ref undo_values );
			if ( changed )
			{
				Vector2 new_resolution = new Vector2( (float) values[ 0 ], (float) values[ 1 ] );
				myAsset.prRenderResolution = new_resolution;
				myUndoInfo.renderResolution = new_resolution;
			}
		}

		// Show Vertex Colours
		createToggleForProperty( "show_only_vertex_colours", "Show Only Vertex Colors", 
                                 "prShowOnlyVertexColours", ref myUndoInfo.showOnlyVertexColours,
                                 () => HAPI_AssetUtility.reApplyMaterials( myAsset ) );

		// Generate Tangents
		createToggleForProperty( "generate_tangents", "Generate Tangents", "prGenerateTangents",
		                         ref myUndoInfo.generateTangents,
		                         () => myAssetOTL.build( true, false, false, true, myAsset.prCookingTriggersDownCooks, true ),
		                         !HAPI_Host.isGenerateTangentsDefault() );
	}

	private void generateCookingSettings()
	{	
		// Enable Cooking Toggle
		createToggleForProperty( "enable_cooking", "Enable Cooking", "prEnableCooking",
		                         ref myUndoInfo.enableCooking, null, !HAPI_Host.isEnableCookingDefault() );

		HAPI_GUI.separator();

		// Cooking Triggers Downstream Cooks Toggle
		createToggleForProperty( "cooking_triggers_downstream_cooks", "Cooking Triggers Downstream Cooks", 
		                         "prCookingTriggersDownCooks", ref myUndoInfo.cookingTriggersDownCooks,
		                         null, !HAPI_Host.isCookingTriggersDownCooksDefault(),
		                         !myAsset.prEnableCooking, " (all cooking is disabled)" );

		// Playmode Per-Frame Cooking Toggle
		createToggleForProperty( "playmode_per_frame_cooking", "Playmode Per-Frame Cooking", 
		                         "prPlaymodePerFrameCooking", ref myUndoInfo.playmodePerFrameCooking,
		                         null, !HAPI_Host.isPlaymodePerFrameCookingDefault(),
		                         !myAsset.prEnableCooking, " (all cooking is disabled)" );

		HAPI_GUI.separator();

		// Push Unity Transform To Houdini Engine Toggle
		createToggleForProperty( "push_unity_transform_to_houdini_engine", "Push Unity Transform To Houdini Engine", 
		                         "prPushUnityTransformToHoudini", ref myUndoInfo.pushUnityTransformToHoudini,
		                         null, !HAPI_Host.isPushUnityTransformToHoudiniDefault() );

		// Transform Change Triggers Cooks Toggle
		createToggleForProperty( "transform_change_triggers_cooks", "Transform Change Triggers Cooks", 
		                         "prTransformChangeTriggersCooks", ref myUndoInfo.transformChangeTriggersCooks,
		                         null, !HAPI_Host.isTransformChangeTriggersCooksDefault(),
		                         !myAsset.prEnableCooking, " (all cooking is disabled)" );

		// Import Templated Geos Toggle
		createToggleForProperty( "import_templated_geos", "Import Templated Geos", "prImportTemplatedGeos",
		                         ref myUndoInfo.importTemplatedGeos, null, !HAPI_Host.isImportTemplatedGeosDefault(),
		                         !myAsset.prEnableCooking, " (all cooking is disabled)" );
	}

	private void generateAssetSettings()
	{
		GUIContent[] modes = new GUIContent[ 3 ];
		modes[ 0 ] = new GUIContent( "View" );
		modes[ 1 ] = new GUIContent( "Materials" );
		modes[ 2 ] = new GUIContent( "Cooking" );
		myAsset.prAssetSettingsCategory = GUILayout.Toolbar( myAsset.prAssetSettingsCategory, modes );

		switch ( myAsset.prAssetSettingsCategory )
		{
			case 0: generateViewSettings(); break;
			case 1: generateMaterialSettings(); break;
			case 2: generateCookingSettings(); break;
			default: Debug.LogError( "Invalid Asset Settings Tab." ); break;
		}
	}

	private delegate void valueChangedFunc();

	private void createToggleForProperty( string name, string label, string property_name, 
	                                      ref bool undo_info_value, valueChangedFunc func )
	{
		createToggleForProperty( name, label, property_name, ref undo_info_value, func, false );
	}
	
	private void createToggleForProperty( string name, string label, string property_name, 
	                                      ref bool undo_info_value, valueChangedFunc func,
	                                      bool global_overwrite )
	{
		createToggleForProperty( name, label, property_name, ref undo_info_value, 
		                         func, global_overwrite, false, "" );
	}

	private void createToggleForProperty( string name, string label, string property_name, 
	                                      ref bool undo_info_value, valueChangedFunc func,
	                                      bool global_overwrite, bool local_overwrite, 
	                                      string local_overwrite_message )
	{
		try
		{
			PropertyInfo property = typeof( HAPI_Asset ).GetProperty( property_name );
		if ( property == null )
		{
			throw new HAPI_ErrorInvalidArgument( property_name + " is not a valid property of HAPI_Asset!" );
		}
		if ( property.PropertyType != typeof( bool ) )
		{
			throw new HAPI_ErrorInvalidArgument( property_name + " is not a boolean!" );
		}

		GUI.enabled = !global_overwrite && !local_overwrite;
		if ( !GUI.enabled )
		{
			if ( global_overwrite )
				label += " (overwritted by global setting)";
			else
				label += local_overwrite_message;
			}
			
			bool value = ( bool ) property.GetValue( myAsset, null );
			bool is_bold = myParentPrefabAsset && ( bool ) property.GetValue( myParentPrefabAsset, null ) != value;
			bool changed = HAPI_GUI.toggle( name, label, is_bold, ref value, myUndoInfo, ref undo_info_value );
			GUI.enabled = true;

			if ( changed )
			{
				property.SetValue( myAsset, value, null );

				if ( func != null )
					func();
			}
		}
		catch ( System.Exception error )
		{
			Debug.LogError( "Failed to create toggle for: " + label + "\n" +
			                error.ToString() + "\nSource: " + error.Source );
		}
	}
	
	private HAPI_AssetOTL myAssetOTL;
	private HAPI_AssetOTLUndoInfo myUndoInfo;
	private HAPI_Asset myParentPrefabAsset;
	private Vector2 myHelpScrollPosition = new Vector2( 0.0f, 0.0f );
}
