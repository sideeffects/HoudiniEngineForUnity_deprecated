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
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using HAPI;
using Utility = HAPI_AssetUtility;

[ ExecuteInEditMode ]
public class HAPI_AssetOTL : HAPI_Asset 
{	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	
	// Please keep these in the same order and grouping as their initializations in HAPI_Asset.reset().
	
	public string 					prAssetPath { 
										get { return myAssetPath; } 
										set 
										{
											if ( value != prAssetPath ) 
											{
												myAssetPath = value;
												prFullBuild = true;
											}
										}
									}
	public HAPI_HandleInfo[]		prHandleInfos { get; set; }
	public List< HAPI_HandleBindingInfo[] > prHandleBindingInfos { get; set; }
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods
	
	public HAPI_AssetOTL() 
	{
		if ( prEnableLogging )
			Debug.Log( "HAPI_Asset created!" );
		
		// These variables need to keep between asset reloads.
		prAssetPath = "";
		
		reset();
	}
	
	~HAPI_AssetOTL() 
	{}
	
	public override void reset()
	{
		base.reset();
		
		// Please keep these in the same order and grouping as their declarations at the top.
		
		prHandleInfos 				= new HAPI_HandleInfo[ 0 ];
		prHandleBindingInfos 		= null;
	}
	
	public override bool build() 
	{
		bool base_built = base.build();
		if ( !base_built )
			return false;
		
		HAPI_ProgressBar progress_bar	= new HAPI_ProgressBar();
		progress_bar.prUseDelay			= prUseDelayForProgressBar;
		progress_bar.prAsset			= this;

		try
		{
			progress_bar.prStartTime = System.DateTime.Now;
			
			if ( prFullBuild || prPartialBuild ) 
			{
				if ( prReloadAssetInFullBuild && prAssetType == HAPI_Asset.AssetType.TYPE_OTL && !prPartialBuild )
					HAPI_Host.unloadOTL( prAssetId );
				
				try
				{
					int asset_id = 0;
					if ( prReloadAssetInFullBuild && prAssetType == HAPI_Asset.AssetType.TYPE_OTL && !prPartialBuild )
						asset_id = HAPI_Host.loadOTL( prAssetPath );
					else
						asset_id = prAssetId;

					prReloadAssetInFullBuild = true; // The default.

					progress_bar.statusCheckLoop();

					prAssetInfo = HAPI_Host.getAssetInfo( asset_id );

					if ( !prPartialBuild )
						Debug.Log( "Asset Loaded - Path: " + prAssetInfo.instancePath + ", ID: " + prAssetInfo.id );
				}
				catch ( HAPI_Error error )
				{
					Debug.LogError( "Asset not loaded: " + error.ToString() );
					// Nothing to build since the load failed.
					
					// Clean up.
					reset();
					
					return false; // false for failed :(
				}
				
				// For convenience we copy some asset info properties locally (since they are constant anyway).
				// More imporantly, structs are not serialized and therefore putting them into their own
				// variables is required in order to maintain state between serialization cycles.
				prAssetId 				= prAssetInfo.id;
				prAssetValidationId		= prAssetInfo.validationId;
				prAssetName				= prAssetInfo.name;
				prHAPIAssetType			= (HAPI_AssetType) prAssetInfo.type;
				prMinTransInputCount	= prAssetInfo.minTransInputCount;
				prMaxTransInputCount	= prAssetInfo.maxTransInputCount;
				prMinGeoInputCount 		= prAssetInfo.minGeoInputCount;
				prMaxGeoInputCount		= prAssetInfo.maxGeoInputCount;
				prParmCount 			= prAssetInfo.parmCount;
				prParmIntValueCount		= prAssetInfo.parmIntValueCount;
				prParmFloatValueCount	= prAssetInfo.parmFloatValueCount;
				prParmStringValueCount	= prAssetInfo.parmStringValueCount;
				prParmChoiceCount		= prAssetInfo.parmChoiceCount;
				
				prObjectCount 			= prAssetInfo.objectCount;
				prHandleCount 			= prAssetInfo.handleCount;
				
				progress_bar.prCurrentValue			= 0;
				progress_bar.prTotal				= prParmCount
													  + prParmIntValueCount
													  + prParmFloatValueCount
													  + prParmStringValueCount
													  + prParmChoiceCount
													  + prObjectCount
													  + prHandleCount;
				
				// Try to load presets.
				loadPreset();
				
				progress_bar.displayProgressBar();
				myProgressBarJustUsed = true;
				
				progress_bar.prMessage = "Loading parameter information...";
				
				// Get all parameters.
				prParms = new HAPI_ParmInfo[ prParmCount ];
				Utility.getArray1Id( prAssetId, HAPI_Host.getParameters, prParms, prParmCount );
				progress_bar.incrementProgressBar( prParmCount );
				
				// Get parameter int values.
				prParmIntValues = new int[ prParmIntValueCount ];
				Utility.getArray1Id( prAssetId, HAPI_Host.getParmIntValues, prParmIntValues, prParmIntValueCount );
				progress_bar.incrementProgressBar( prParmIntValueCount );
				
				// Get parameter float values.
				prParmFloatValues = new float[ prParmFloatValueCount ];
				Utility.getArray1Id( prAssetId, HAPI_Host.getParmFloatValues, prParmFloatValues, prParmFloatValueCount );
				progress_bar.incrementProgressBar( prParmFloatValueCount );
				
				// Get parameter string (handle) values.
				prParmStringValues = new int[ prParmStringValueCount ];
				Utility.getArray1Id( prAssetId, HAPI_Host.getParmStringValues, prParmStringValues, 
									 prParmStringValueCount );
				progress_bar.incrementProgressBar( prParmStringValueCount );
				
				// Get parameter choice lists.
				prParmChoiceLists = new HAPI_ParmChoiceInfo[ prParmChoiceCount ];
				Utility.getArray1Id( prAssetId, HAPI_Host.getParmChoiceLists, prParmChoiceLists, prParmChoiceCount );
				progress_bar.incrementProgressBar( prParmChoiceCount );
				
				progress_bar.prMessage = "Loading handles...";
				
				// Get exposed handle information.
				prHandleInfos = new HAPI_HandleInfo[ prHandleCount ];
				Utility.getArray1Id( prAssetId, HAPI_Host.getHandleInfo, prHandleInfos, prHandleCount );
				
				// Get handles.
				prHandleBindingInfos = new List< HAPI_HandleBindingInfo[] >( prHandleCount );
				for ( int handle_index = 0; handle_index < prHandleCount; ++handle_index )
				{
					progress_bar.incrementProgressBar();
					HAPI_HandleInfo handle_info = prHandleInfos[ handle_index ];
					
					if ( handle_info.typeName != "xform" && HAPI_Host.prEnableSupportWarnings )
						Debug.LogWarning( "Handle " + handle_info.name + " of type " 
								   		  + handle_info.typeName + " is unsupported at this time." );
					
					HAPI_HandleBindingInfo[] binding_infos = new HAPI_HandleBindingInfo[ handle_info.bindingsCount ];
					Utility.getArray2Id( prAssetId, handle_index, HAPI_Host.getHandleBindingInfo, 
								 		 binding_infos, handle_info.bindingsCount );
					
					prHandleBindingInfos.Add( binding_infos );
				}
				
				// Add input fields.
				if ( !prPartialBuild && !prForceReconnectInFullBuild )
				{
					if ( prHAPIAssetType == HAPI_AssetType.HAPI_ASSETTYPE_OBJ )
					{
						if ( prMaxTransInputCount > 0 && prUpStreamTransformAssets.Count <= 0 )
							for ( int ii = 0; ii < prMaxTransInputCount ; ++ii )
							{
								prUpStreamTransformAssets.Add( null );
								prUpStreamTransformObjects.Add( null );
							}
					}
				
					if ( prMaxGeoInputCount > 0 && prFileInputs.Count <= 0 )
						for ( int ii = 0; ii < prMaxGeoInputCount ; ++ii )
						{
							prFileInputs.Add( "" );
							prUpStreamGeoAssets.Add( null );
							prUpStreamGeoObjects.Add( null );
							prUpStreamGeoAdded.Add( false );
						}
				
					// Check for min input fields set.
					if ( prHAPIAssetType == HAPI_AssetType.HAPI_ASSETTYPE_OBJ )
					{
						int numValidTransformInputs = 0;
						for ( int ii = 0; ii < prMaxTransInputCount ; ++ii )
							if ( prUpStreamTransformAssets[ ii ] )
								numValidTransformInputs++;
					
						if ( numValidTransformInputs < prMinTransInputCount )
							Debug.LogWarning( "Insufficent Transform Inputs to Asset. " +
											  "Please provide inputs in the Inputs section." );
					}
				
					int numValidGeoInputs = 0;
					for ( int ii = 0; ii < prMaxGeoInputCount ; ++ii )
						if ( prFileInputs[ ii ] != "" )
							numValidGeoInputs++;
				
					if ( numValidGeoInputs < prMinGeoInputCount )
						Debug.LogWarning( "Insufficent Geo Inputs to Asset. " +
										  "Please provide inputs in the Inputs section." );
				
					if ( prHAPIAssetType == HAPI_AssetType.HAPI_ASSETTYPE_OBJ )
						for ( int ii = 0; ii < prMaxTransInputCount ; ++ii )
							if ( prUpStreamTransformAssets[ ii ] )
								HAPI_Host.connectAssetTransform( prUpStreamTransformAssets[ ii ].prAssetId, prAssetId, ii );

					foreach ( HAPI_Asset downstream_asset in prDownStreamTransformAssets )
					{
						int index = downstream_asset.getAssetTransformConnectionIndex( this );
						if ( index >= 0 )
							HAPI_Host.connectAssetTransform( prAssetId, downstream_asset.prAssetId, index );
					}
					
					// Fill input names.
					for ( int i = 0; i < prMaxTransInputCount; ++i )
					{
						string trans_input_name = HAPI_Host.getInputName( prAssetId, i, 
																		  HAPI_InputType.HAPI_INPUT_TRANSFORM );
						if ( trans_input_name == "" )
							trans_input_name = "Transform Input #" + ( i + 1 );
						prTransInputNames.Add( trans_input_name );
					}
					for ( int i = 0; i < prMaxGeoInputCount; ++i )
					{
						string geo_input_name = HAPI_Host.getInputName( prAssetId, i, 
																		HAPI_InputType.HAPI_INPUT_GEOMETRY );
						if ( geo_input_name == "" )
							geo_input_name = "Geometry Input #" + ( i + 1 );
						prGeoInputNames.Add( geo_input_name );
						prGeoInputFormats.Add( HAPI_GeoInputFormat.HAPI_GEO_INPUT_FORMAT_DEFAULT );
					}
				}

				if ( !prPartialBuild )
				{
					// Clean up.
					destroyChildren( transform );
					prGameObjects = new GameObject[ prObjectCount ];
				}

				// Create local object info caches (transforms need to be stored in a parallel array).
				prObjects = new HAPI_ObjectInfo[ prObjectCount ];
				prObjectTransforms = new HAPI_Transform[ prObjectCount ];
			}
			else
			{
				progress_bar.displayProgressBar();
				myProgressBarJustUsed = true;
				
				progress_bar.prTotal = prObjectCount + prParmIntValueCount 
									   + prParmFloatValueCount + prParmStringValueCount;

				HAPI_Host.cookAsset( prAssetId );
				progress_bar.statusCheckLoop();

				// We need to get the parameter values again because they could have been
				// changed by a script.

				// Get parameter int values.
				Utility.getArray1Id( prAssetId, HAPI_Host.getParmIntValues, prParmIntValues, prParmIntValueCount );
				progress_bar.incrementProgressBar( prParmIntValueCount );
				
				// Get parameter float values.
				Utility.getArray1Id( prAssetId, HAPI_Host.getParmFloatValues, prParmFloatValues, prParmFloatValueCount );
				progress_bar.incrementProgressBar( prParmFloatValueCount );
				
				// Get parameter string (handle) values.
				Utility.getArray1Id( prAssetId, HAPI_Host.getParmStringValues, prParmStringValues, 
									 prParmStringValueCount );
				progress_bar.incrementProgressBar( prParmStringValueCount );
			}

			if ( !prPartialBuild )
			{
				// Set asset's transform.
				if ( prSyncAssetTransform )
				{
					HAPI_TransformEuler hapi_transform;
					HAPI_Host.getAssetTransform( prAssetId, (int) HAPI_RSTOrder.SRT, 
												 (int) HAPI_XYZOrder.ZXY, out hapi_transform );
					if ( Mathf.Approximately( 0.0f, hapi_transform.scale[ 0 ] ) ||
						 Mathf.Approximately( 0.0f, hapi_transform.scale[ 1 ] ) ||
						 Mathf.Approximately( 0.0f, hapi_transform.scale[ 2 ] ) )
					{
						Debug.LogWarning( "Asset(id: " + prAssetId + ", name: " + prAssetName + "): Scale has a zero component!" );
					}

					Utility.applyTransform( hapi_transform, transform );
				}
			
				progress_bar.prMessage = "Loading and composing objects...";
			
				Utility.getArray1Id( prAssetId, HAPI_Host.getObjects, prObjects, prObjectCount );
				Utility.getArray2Id( prAssetId, (int) HAPI_RSTOrder.SRT, HAPI_Host.getObjectTransforms, 
						 			 prObjectTransforms, prObjectCount );
			
				for ( int object_index = 0; object_index < prObjectCount; ++object_index )
				{
					progress_bar.incrementProgressBar();
					try
					{
						if ( !prObjects[ object_index ].isInstancer && 
							 ( prFullBuild || prObjects[ object_index ].hasTransformChanged
										   || prObjects[ object_index ].haveGeosChanged ) )
						{
							createObject( object_index );
						}
					}
					catch ( HAPI_Error error )
					{
						// Per-object errors are not re-thrown so that the rest of the asset has a chance to load.
						Debug.LogWarning( error.ToString() );
					}
				}
			
				// Processing instancers.
				for ( int object_index = 0; object_index < prObjectCount; ++object_index )
				{
					HAPI_ObjectInfo object_info = prObjects[ object_index ];
					if ( object_info.isInstancer )
					{
						try
						{
							if ( object_info.objectToInstanceId >= 0 && 
								 prGameObjects[ object_info.objectToInstanceId ] == null )
								createObject( object_info.objectToInstanceId );
						
							instanceObjects( object_index, progress_bar );
						}
						catch ( HAPI_Error error )
						{
							// Per-object errors are not re-thrown so that the rest of the asset has a chance to load.
							Debug.LogWarning( error.ToString() );
						}
					}
				}
			
				// Process dependent assets.
				processDependentAssets();
			}
		}
		catch ( HAPI_ErrorIgnorable ) {}
		catch ( HAPI_Error error )
		{
			Debug.LogError( error.ToString() );
		}
		catch ( System.Exception error )
		{
			Debug.LogError( error.ToString() );
		}
		finally
		{
			progress_bar.clearProgressBar();

			prFullBuild = false;
			prPartialBuild = false;
			prForceReconnectInFullBuild = false;

			myProgressBarJustUsed = false;
			prUseDelayForProgressBar = true;
		}
		
		return true;
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private Methods
	
	private void instanceObjects( int object_id, HAPI_ProgressBar progress_bar )
	{
		HAPI_ObjectInfo object_info = prObjects[ object_id ];
		HAPI_Instancer instancer = null;
		
		Transform old_instancer_transform = transform.Find( object_info.name );
		if ( old_instancer_transform && old_instancer_transform.gameObject.GetComponent< HAPI_Instancer >() )
		{
			instancer = old_instancer_transform.gameObject.GetComponent< HAPI_Instancer >();
		}
		else
		{
			GameObject main_object = new GameObject( object_info.name );
			main_object.transform.parent = transform;

			main_object.AddComponent( "HAPI_Instancer" );
			instancer = main_object.GetComponent< HAPI_Instancer >();
		}
		
		instancer.prAsset = this;
		instancer.prObjectId = object_id;
		
		instancer.instanceObjects( progress_bar );
	}

	private void createPart( GameObject part_node, bool has_geo_changed, bool has_material_changed )
	{
		HAPI_PartControl part_control = part_node.GetComponent< HAPI_PartControl >();
		if ( part_control == null )
			return;

		HAPI_PartInfo part_info = new HAPI_PartInfo();
		
		// Get Part info.
		HAPI_Host.getPartInfo( prAssetId, part_control.prObjectId, part_control.prGeoId, 
							   part_control.prPartId, out part_info );
		
		if ( prEnableLogging && ( prFullBuild || has_geo_changed || has_material_changed ) )
			Debug.Log( "Obj #" + part_control.prObjectId + " (" + part_control.prObjectName + "): "
					   + "verts: " + part_info.vertexCount + " faces: " + part_info.faceCount );

		if ( prFullBuild || has_geo_changed )
		{
			// Initialize our part control.
			part_control.init( this, part_info.id, part_info.name, part_info.materialId );

			// Overwrite name.
			part_node.name = part_info.name + "_part" + part_control.prPartId;

			// Add required components.
			MeshFilter mesh_filter = part_node.AddComponent< MeshFilter >();

			// Get or create mesh.
			Mesh part_mesh 				= mesh_filter.sharedMesh;
			if ( part_mesh == null ) 
			{
				mesh_filter.mesh 		= new Mesh();
				part_mesh 				= mesh_filter.sharedMesh;
			}
			part_mesh.Clear();
		
			// Get mesh.
			try
			{
				Utility.getMesh( part_control, part_mesh );
			}
			catch ( HAPI_ErrorIgnorable ) {}
			catch ( HAPI_Error error )
			{
				Debug.LogWarning( error.ToString() );
				return;
			}

			// Add collider if group name matches. (Should be added after the mesh is set so that it
			// picks up the mesh automagically)
			if ( part_info.name.EndsWith( HAPI_Host.prRenderedCollisionGroupName ) )
			{
				part_node.AddComponent< MeshCollider >();
				part_node.AddComponent< MeshRenderer >();
			}
			else if ( part_info.name == HAPI_Host.prCollisionGroupName )
				part_node.AddComponent< MeshCollider >();
			else
				part_node.AddComponent< MeshRenderer >();
		
			// Add Mesh-to-Prefab component.
			part_node.AddComponent( "HAPI_MeshToPrefab" );
			HAPI_MeshToPrefab mesh_saver = part_node.GetComponent< HAPI_MeshToPrefab >();
			mesh_saver.prObjectControl = this;
			mesh_saver.prGameObject = part_node;
			mesh_saver.prMeshName = prAssetName + "_" + part_node.name;
		}

		// Set visibility.
		if ( part_control.prPartName != HAPI_Host.prCollisionGroupName )
		{
			part_control.prMaterialId = part_info.materialId;
			
			MeshRenderer mesh_renderer = part_node.GetComponent< MeshRenderer >();
			if ( !mesh_renderer )
				throw new HAPI_Error( "No mesh renderer!" );

			mesh_renderer.enabled = 
				part_control.prObjectVisible && 
					( prIsGeoVisible || part_control.prGeoType == HAPI_GeoType.HAPI_GEOTYPE_EXPOSED_EDIT );
		
			// Set material.
			if ( mesh_renderer.sharedMaterial == null )
				mesh_renderer.sharedMaterial = new Material( Shader.Find( "HAPI/SpecularVertexColor" ) );

			if ( ( prFullBuild || has_material_changed || mesh_renderer.sharedMaterial.mainTexture == null ) 
				 && part_info.materialId >= 0 )
			{
				HAPI_MaterialInfo material = HAPI_Host.getMaterial( prAssetId, part_info.materialId );

				// Assign vertex color shader if the flag says so.
				if ( prShowVertexColours )
				{
					mesh_renderer.sharedMaterial.shader = Shader.Find( "HAPI/SpecularVertexColor" );
				}
				else
				{
					// Assign the transparency shader if this material is transparent or unassign it otherwise.
					if ( material.isTransparent() )
						mesh_renderer.sharedMaterial.shader = Shader.Find( "HAPI/AlphaSpecularVertexColor" );
					else
						mesh_renderer.sharedMaterial.shader = Shader.Find( "HAPI/SpecularVertexColor" );
				
					Material mat = mesh_renderer.sharedMaterial;
					Utility.assignTexture( ref mat, material );
				}
			}
		}
	}
	
	private void createGeo( GameObject geo_node, bool first_time )
	{
		HAPI_GeoControl geo_control = geo_node.GetComponent< HAPI_GeoControl >();
		if ( geo_control == null )
			return;

		// Get Geo info.
		HAPI_GeoInfo geo_info = new HAPI_GeoInfo();
		HAPI_Host.getGeoInfo( prAssetId, geo_control.prObjectId, geo_control.prGeoId, out geo_info );

		if ( geo_info.type == (int) HAPI.HAPI_GeoType.HAPI_GEOTYPE_INPUT )
			return;

		if ( !first_time && !prFullBuild && !geo_info.hasGeoChanged && !geo_info.hasMaterialChanged )
			return;

		if ( first_time || prFullBuild || geo_node.transform.childCount == 0 || geo_info.hasGeoChanged )
		{
			// Initialize our geo control.
			geo_control.init( geo_control.prGeoId, geo_info.name, (HAPI_GeoType) geo_info.type );

			// Set node name.
			geo_node.name = geo_control.prGeoName + "_geo" + geo_control.prGeoId;

			destroyChildren( geo_node.transform );
			for ( int ii = 0; ii < geo_info.partCount; ii++ )
			{
				GameObject part_node = new GameObject( "uninitialized_part" );
				part_node.transform.parent = geo_node.transform;

				// Need to reset position here because the assignment above will massage the child's
				// position in order to be in the same place it was in the global namespace.
				part_node.transform.localPosition = new Vector3( 0.0f, 0.0f, 0.0f );

				HAPI_PartControl part_control = part_node.AddComponent< HAPI_PartControl >();
				part_control.init( geo_control );
				part_control.prPartId = ii;

				// Force geo re-build so for "has_geo_changed" to true.
				createPart( part_node, true, geo_info.hasMaterialChanged );
			}
			
			geo_info.hasGeoChanged = true;
		}
		else
			foreach ( Transform part_trans in geo_node.transform )
				createPart( part_trans.gameObject, geo_info.hasGeoChanged, geo_info.hasMaterialChanged );

		geo_info.hasMaterialChanged = false;
	}
	
	private void createObject( int object_id )
	{
		HAPI_ObjectInfo object_info = prObjects[ object_id ];
		
		// Create main underling.
		if ( prGameObjects[ object_id ] == null )
		{
			prGameObjects[ object_id ] = new GameObject( object_info.name + "_obj" + object_id );
			prGameObjects[ object_id ].transform.parent = transform;

			HAPI_ObjectControl object_control = prGameObjects[ object_id ].AddComponent< HAPI_ObjectControl >();
			object_control.init( prAssetId, object_id, object_info.name, object_info.isVisible );
		}
		GameObject main_child = prGameObjects[ object_id ];
		
		try
		{
			if ( prFullBuild || object_info.haveGeosChanged )
			{
				if ( prFullBuild || object_info.geoCount != main_child.transform.childCount )
				{
					destroyChildren( main_child.transform );
					for ( int ii = 0; ii < object_info.geoCount; ii++ )
					{
						GameObject geo_child = new GameObject( "uninitialized_geo" );
						geo_child.transform.parent = main_child.transform;
						
						// Need to reset position here because the assignment above will massage the child's
						// position in order to be in the same place it was in the global namespace.
						geo_child.transform.localPosition = new Vector3( 0.0f, 0.0f, 0.0f );

						HAPI_GeoControl geo_control = geo_child.AddComponent< HAPI_GeoControl >();
						geo_control.init( main_child.GetComponent< HAPI_ObjectControl >() );
						geo_control.prGeoId = ii;

						createGeo( geo_child, true );
					}
				}
				else
					foreach ( Transform geo_trans in main_child.transform )
						createGeo( geo_trans.gameObject, false );
			}
			
			if ( prFullBuild || object_info.hasTransformChanged )
			{
				// Get transforms.
				HAPI_Transform trans = prObjectTransforms[ object_id ];
				Utility.applyTransform( trans, main_child.transform );
			}
		}
		catch ( HAPI_Error error )
		{
			DestroyImmediate( main_child );
			prGameObjects[ object_id ] = null;
			error.addMessagePrefix( "Obj(id: " + object_info.id + ", name: " + object_info.name + ")" );
			error.addMessageDetail( "Object Path: " + object_info.objectInstancePath );
			throw;
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Serialized Data
	
	[SerializeField]
	private string			myAssetPath;
	
}
