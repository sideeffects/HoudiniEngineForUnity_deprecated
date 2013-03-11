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

[ CustomEditor( typeof( HAPI_AssetOTL ) ) ]
public partial class HAPI_AssetGUIOTL : HAPI_AssetGUI 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	public override void OnEnable() 
	{
		base.OnEnable();
		myAssetOTL = myAsset as HAPI_AssetOTL;
	}
	
	public override void OnInspectorGUI() 
	{
		myParmChanges = false;
		myDelayBuild = false;
		
		base.OnInspectorGUI();
		
		Event curr_event = Event.current;
		bool commitChanges = false;
		if ( curr_event.isKey && curr_event.type == EventType.KeyUp && curr_event.keyCode == KeyCode.Return )
			commitChanges = true;
		
		///////////////////////////////////////////////////////////////////////
		// Draw Game Object Controls
		
		myAssetOTL.prShowObjectControls = 
			EditorGUILayout.Foldout( myAssetOTL.prShowObjectControls, new GUIContent( "Object Controls" ) );
		
		if ( myAssetOTL.prShowObjectControls ) 
		{
			if ( GUILayout.Button( "Rebuild" ) ) 
			{
				myAssetOTL.prFullBuild = true;
				myAssetOTL.build();
			}
			if ( GUILayout.Button( "Recook" ) ) 
			{
				myAssetOTL.build();
			}
			
			EditorGUILayout.BeginHorizontal(); 
			{
				if ( GUILayout.Button( "Export To Hip File..." ) ) 
				{
					string hip_file_path = EditorUtility.SaveFilePanel( "Save HIP File", "", "hscene.hip", "hip" );
					if ( hip_file_path != "" && HAPI_Host.hasScene() )
						HAPI_Host.exportAssetToHIPFile( myAssetOTL.prAssetId, hip_file_path );
					else
						Debug.LogError( "Nothing to save." );
				}
				
				if ( GUILayout.Button( "Replace From Hip File..." ) ) 
				{
					string hip_file_path = EditorUtility.OpenFilePanel( "Import HIP File", "", "hip" );
					if ( hip_file_path != "" && HAPI_Host.hasScene() )
					{
						try
						{
							HAPI_Host.replaceAssetFromHIPFile ( myAssetOTL.prAssetId, hip_file_path );
						}
						catch ( HAPI_Error error )
						{
							Debug.LogError( error.ToString() );
						}
						
						myAssetOTL.prFullBuild = true;
						myAssetOTL.prReloadAssetInFullBuild = false;
						myAssetOTL.build();
					}
					else
						Debug.LogError( "Nothing to save." );
				}
				
			} 
			EditorGUILayout.EndHorizontal();
			
			string path = myAssetOTL.prAssetPath;
			myParmChanges |= HAPI_GUI.fileField( "otl_path", "OTL Path", ref myDelayBuild, ref path );
			if ( myParmChanges )
				myAssetOTL.prAssetPath = path;
			
			// These don't affect the asset directly so they don't trigger rebuilds.
			
			// Show Geometries
			{
				bool value = myAsset.prIsGeoVisible;
				bool changed = HAPI_GUI.toggle( "show_geometries", "Show Geometries", ref value );
				if ( changed )
				{
					myAsset.prIsGeoVisible = value;
					HAPI_PartControl[] controls = 
						myAsset.GetComponentsInChildren< HAPI_PartControl >();
					foreach ( HAPI_PartControl control in controls )
					{
						if ( control.prGeoType != HAPI_GeoType.HAPI_GEOTYPE_EXPOSED_EDIT
								&& control.gameObject.GetComponent< MeshRenderer >() != null )
							control.gameObject.GetComponent< MeshRenderer >().enabled = myAsset.prIsGeoVisible;
					}
				}
			}

			// Show Vertex Colours
			{
				bool value = myAsset.prShowVertexColours;
				bool changed = HAPI_GUI.toggle( "show_vertex_colours", "Show Vertex Colors", ref value );
				if ( changed )
				{
					myAsset.prShowVertexColours = value;
					foreach ( MeshRenderer renderer in myAsset.GetComponentsInChildren< MeshRenderer >() )
					{
						// Set material.
						if ( renderer.sharedMaterial == null )
							renderer.sharedMaterial = new Material( Shader.Find( "HAPI/SpecularVertexColor" ) );

						if ( myAsset.prShowVertexColours )
						{
							renderer.sharedMaterial.mainTexture = null;
							renderer.sharedMaterial.shader = Shader.Find( "HAPI/SpecularVertexColor" );
						}
						else
						{
							Transform parent = renderer.transform;
							HAPI_PartControl control = parent.GetComponent< HAPI_PartControl >();
							
							if ( control.prMaterialId >= 0 )
							{
								try
								{
									HAPI_MaterialInfo material = HAPI_Host.getMaterial( myAsset.prAssetId, 
																						control.prMaterialId );

									if ( material.isTransparent() )
										renderer.sharedMaterial.shader = Shader.Find( "HAPI/AlphaSpecularVertexColor" );
									else if ( !material.isTransparent() )
										renderer.sharedMaterial.shader = Shader.Find( "HAPI/SpecularVertexColor" );

									Material mat = renderer.sharedMaterial;
									HAPI_AssetUtility.assignTexture( ref mat, material );
								}
								catch ( HAPI_Error error )
								{
									Debug.LogError( error.ToString() );
								}
							}
							else
							{
								renderer.sharedMaterial.shader = Shader.Find( "HAPI/SpecularVertexColor" );
							}
						}
					}
				}
			}

			HAPI_GUI.separator();

			// Enable Cooking Toggle
			{
				bool value = myAsset.prEnableCooking;
				if ( HAPI_Host.prEnableCooking == false )
				{
					GUI.enabled = false;
					HAPI_GUI.toggle( "enable_cooking", "Enable Cooking (overwritted by global setting)", ref value );
					GUI.enabled = true;
				}
				else
					HAPI_GUI.toggle( "enable_cooking", "Enable Cooking", ref value );
				myAsset.prEnableCooking = value;
			}

			// Auto Select Asset Node Toggle
			{
				bool value = myAsset.prAutoSelectAssetNode;
				if ( HAPI_Host.prAutoSelectParent == false )
				{
					GUI.enabled = false;
					HAPI_GUI.toggle( "auto_select_parent", "Auto Select Parent (overwritted by global setting)", ref value );
					GUI.enabled = true;
				}
				else
					HAPI_GUI.toggle( "auto_select_parent", "Auto Select Parent", ref value );
				myAsset.prAutoSelectAssetNode = value;
			}
			
			// Hide When Fed to Other Asset
			{
				bool value = myAsset.prHideGeometryOnLinking;
				if ( HAPI_Host.prHideGeometryOnLinking == false )
				{
					GUI.enabled = false;
					HAPI_GUI.toggle( "hide_geometry_on_linking", "Hide Geometry On Linking (overwritted by global setting)", ref value );
					GUI.enabled = true;
				}
				else
					HAPI_GUI.toggle( "hide_geometry_on_linking", "Hide Geometry On Linking", ref value );
				myAsset.prHideGeometryOnLinking = value;
			}

			HAPI_GUI.separator();
			
			/* Hide for now since it's not used a lot.
			// Logging Toggle
			{
				bool value = myAsset.prEnableLogging;
				HAPI_GUI.toggle( "enable_logging", "Enable Logging", ref value );
				myAsset.prEnableLogging = value;
			}
			*/

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

			// Live In-Game Cooking
			{
				bool value = myAsset.prLiveInGameCooking;
				HAPI_GUI.toggle( "live_ingame_cooking", "Live In-Game Cooking", ref value );
				myAsset.prLiveInGameCooking = value;
			}
		} // if
		
		///////////////////////////////////////////////////////////////////////
		// Draw Asset Controls
		
		EditorGUILayout.Separator();
		myAssetOTL.prShowAssetControls = 
			EditorGUILayout.Foldout( myAssetOTL.prShowAssetControls, new GUIContent( "Asset Controls" ) );
		
		if ( myAssetOTL.prShowAssetControls )
			myParmChanges |= generateAssetControls();
		
		if ( ( myParmChanges && !myDelayBuild ) || ( myUnbuiltChanges && commitChanges ) )
		{
			myAssetOTL.build();
			myUnbuiltChanges = false;
			myParmChanges = false;

			// To keep things consistent with Unity workflow, we should not save parameter changes
			// while in Play mode.
			if ( !EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode )
				myAssetOTL.savePreset();
		}
		else if ( myParmChanges )
			myUnbuiltChanges = true;
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private
	
	private HAPI_AssetOTL myAssetOTL;
}
