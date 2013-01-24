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
		
		HAPI_ProgressBar progressBar = new HAPI_ProgressBar();
		try
		{
			progressBar.prProgressBarStartTime = System.DateTime.Now;
			
			if ( prFullBuild || prPartialBuild ) 
			{
				if ( prUnloadAssetInFullBuild && prAssetType == HAPI_Asset.AssetType.TYPE_OTL && !prPartialBuild )
					HAPI_Host.unloadOTL( prAssetId );
				
				try
				{
					int asset_id = 0;
					if ( prUnloadAssetInFullBuild && prAssetType == HAPI_Asset.AssetType.TYPE_OTL && !prPartialBuild )
						asset_id = HAPI_Host.loadOTL( prAssetPath );
					else
						asset_id = prAssetId;

					progressBar.statusCheckLoop();

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
				prAssetId 				= prAssetInfo.id;
				prHAPIAssetType			= (HAPI_AssetType) prAssetInfo.type;
				prMinInputCount			= prAssetInfo.minInputCount;
				prMaxInputCount			= prAssetInfo.maxInputCount;
				prMinGeoInputCount 		= prAssetInfo.minGeoInputCount;
				prMaxGeoInputCount		= prAssetInfo.maxGeoInputCount;
				prParmCount 			= prAssetInfo.parmCount;
				prParmIntValueCount		= prAssetInfo.parmIntValueCount;
				prParmFloatValueCount	= prAssetInfo.parmFloatValueCount;
				prParmStringValueCount	= prAssetInfo.parmStringValueCount;
				prParmChoiceCount		= prAssetInfo.parmChoiceCount;
				
				prObjectCount 			= prAssetInfo.objectCount;
				prHandleCount 			= prAssetInfo.handleCount;
				
				progressBar.prProgressBarCurrent	= 0;
				progressBar.prProgressBarTotal		= prParmCount
													  + prParmIntValueCount
													  + prParmFloatValueCount
													  + prParmStringValueCount
													  + prParmChoiceCount
													  + prObjectCount
													  + prHandleCount;
				
				// Try to load presets.
				loadPreset();
				
				progressBar.displayProgressBar();
				myProgressBarJustUsed = true;
				
				progressBar.prProgressBarMsg = "Loading parameter information...";
				
				// Get all parameters.
				prParms = new HAPI_ParmInfo[ prParmCount ];
				Utility.getArray1Id( prAssetId, HAPI_Host.getParameters, prParms, prParmCount );
				progressBar.incrementProgressBar( prParmCount );
				
				// Get parameter int values.
				prParmIntValues = new int[ prParmIntValueCount ];
				Utility.getArray1Id( prAssetId, HAPI_Host.getParmIntValues, prParmIntValues, prParmIntValueCount );
				progressBar.incrementProgressBar( prParmIntValueCount );
				
				// Get parameter float values.
				prParmFloatValues = new float[ prParmFloatValueCount ];
				Utility.getArray1Id( prAssetId, HAPI_Host.getParmFloatValues, prParmFloatValues, prParmFloatValueCount );
				progressBar.incrementProgressBar( prParmFloatValueCount );
				
				// Get parameter string (handle) values.
				prParmStringValues = new int[ prParmStringValueCount ];
				Utility.getArray1Id( prAssetId, HAPI_Host.getParmStringValues, prParmStringValues, 
									 prParmStringValueCount );
				progressBar.incrementProgressBar( prParmStringValueCount );
				
				// Get parameter choice lists.
				prParmChoiceLists = new HAPI_ParmChoiceInfo[ prParmChoiceCount ];
				Utility.getArray1Id( prAssetId, HAPI_Host.getParmChoiceLists, prParmChoiceLists, prParmChoiceCount );
				progressBar.incrementProgressBar( prParmChoiceCount );
				
				progressBar.prProgressBarMsg = "Loading handles...";
				
				// Get exposed handle information.
				prHandleInfos = new HAPI_HandleInfo[ prHandleCount ];
				Utility.getArray1Id( prAssetId, HAPI_Host.getHandleInfo, prHandleInfos, prHandleCount );
				
				// Get handles.
				prHandleBindingInfos = new List< HAPI_HandleBindingInfo[] >( prHandleCount );
				for ( int handle_index = 0; handle_index < prHandleCount; ++handle_index )
				{
					progressBar.incrementProgressBar();
					HAPI_HandleInfo handle_info = prHandleInfos[ handle_index ];
					
					if ( handle_info.typeName != "xform" )
						Debug.LogWarning( "Handle " + handle_info.name + " of type " 
								   		  + handle_info.typeName + " is unsupported at this time." );
					
					HAPI_HandleBindingInfo[] binding_infos = new HAPI_HandleBindingInfo[ handle_info.bindingsCount ];
					Utility.getArray2Id( prAssetId, handle_index, HAPI_Host.getHandleBindingInfo, 
								 		 binding_infos, handle_info.bindingsCount );
					
					prHandleBindingInfos.Add( binding_infos );
				}
				
				// Add input fields.
				if ( !prPartialBuild )
				{
					if ( prAssetInfo.type == (int) HAPI_AssetType.HAPI_ASSETTYPE_OBJ )
					{
						if ( prMaxInputCount > 0 && prUpStreamTransformAssets.Count <= 0 )
							for ( int ii = 0; ii < prMaxInputCount ; ++ii )
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
					if ( prAssetInfo.type == (int) HAPI_AssetType.HAPI_ASSETTYPE_OBJ )
					{
						int numValidTransformInputs = 0;
						for ( int ii = 0; ii < prMaxInputCount ; ++ii )
							if ( prUpStreamTransformAssets[ ii ] )
								numValidTransformInputs++;
					
						if ( numValidTransformInputs < prMinInputCount )
							Debug.LogWarning( "Insufficent Transform Inputs to Asset. " +
											  "Please provide inputs in the Inputs section." );
					}
				
					int numValidGeoInputs = 0;
					for ( int ii = 0; ii < prMaxGeoInputCount ; ++ii )
						if ( prFileInputs[ ii ] != "" )
							numValidGeoInputs++;
				
					if ( numValidGeoInputs < prMinGeoInputCount )
						Debug.LogWarning( "Insufficent Geo Inputs to Asset. Please provide inputs in the Inputs section." );
				
					if ( prAssetInfo.type == (int) HAPI_AssetType.HAPI_ASSETTYPE_OBJ )
						for ( int ii = 0; ii < prMaxInputCount ; ++ii )
							if ( prUpStreamTransformAssets[ ii ] )
								HAPI_Host.connectAssetTransform( prUpStreamTransformAssets[ ii ].prAssetId, prAssetId, ii );
				
					for ( int ii = 0; ii < prMaxGeoInputCount ; ++ii )
					{
						if ( prFileInputs[ ii ] != "" )
							HAPI_Host.setFileInput( prAssetId, ii, prFileInputs[ ii ] );
					
						//TODO: handle restoring geometry connections
						//if ( prUpStreamGeoAssets[ ii ] != null )
						//{
						//	HAPI_Host.connectAsset( prUpStreamAssets[ ii ].prAssetId, prAssetId, ii );
						//}
					}
				
					foreach ( HAPI_Asset downstream_asset in prDownStreamTransformAssets )
					{
						int index = downstream_asset.getAssetTransformConnectionIndex( this );
						if ( index >= 0 )
							HAPI_Host.connectAssetTransform( prAssetId, downstream_asset.prAssetId, index );
					}

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
				progressBar.displayProgressBar();
				myProgressBarJustUsed = true;
				
				progressBar.prProgressBarTotal = prObjectCount;

				HAPI_Host.cookAsset( prAssetId );

				progressBar.statusCheckLoop();
			}

			// Set asset's transform.
			if ( prSyncAssetTransform )
			{
				HAPI_TransformEuler hapi_transform;
				HAPI_Host.getAssetTransform( prAssetId, (int) HAPI_RSTOrder.SRT, 
											 (int) HAPI_XYZOrder.ZXY, out hapi_transform );
				Utility.applyTransform( hapi_transform, transform );
			}
			
			progressBar.prProgressBarMsg = "Loading and composing objects...";
			
			Utility.getArray1Id( prAssetId, HAPI_Host.getObjects, prObjects, prObjectCount );
			Utility.getArray2Id( prAssetId, (int) HAPI_RSTOrder.SRT, HAPI_Host.getObjectTransforms, 
						 		 prObjectTransforms, prObjectCount );
			
			for ( int object_index = 0; object_index < prObjectCount; ++object_index )
			{
				progressBar.incrementProgressBar();
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
						
						instanceObjects( object_index );
					}
					catch ( HAPI_Error error )
					{
						// Per-object errors are not re-thrown so that the rest of the asset has a chance to load.
						Debug.LogWarning( error.ToString() );
					}
				}
			}
			
			// Process dependent assets.
			if ( !prPartialBuild )
			{
				foreach ( HAPI_Asset downstream_asset in prDownStreamTransformAssets )
					downstream_asset.build();
			
				foreach ( HAPI_Asset downstream_asset in prDownStreamGeoAssets )
					downstream_asset.build();
			}
			
			prFullBuild = false;
			prPartialBuild = false;
		}
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
			progressBar.clearProgressBar();
			myProgressBarJustUsed = false;
		}
		
		return true;
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private Methods
	
	private void instanceObjects( int object_id )
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
		
		instancer.instanceObjects();
	}

	private void createPart( int object_id, int geo_id, int part_id, ref HAPI_GeoInfo geo_info, GameObject part_node )
	{
		HAPI_ObjectInfo object_info = prObjects[ object_id ];
		HAPI_PartInfo part_info = new HAPI_PartInfo();
		
		if ( prFullBuild || geo_info.hasGeoChanged || geo_info.hasMaterialChanged )
		{
			// Get Part info.
			HAPI_Host.getPartInfo( prAssetId, object_id, geo_id, part_id, out part_info );
			if ( prEnableLogging )
			Debug.Log( "Obj #" + object_id + " (" + object_info.name + "): "
					   + "verts: " + part_info.vertexCount + " faces: " + part_info.faceCount );
		}

		if ( prFullBuild || geo_info.hasGeoChanged )
		{
			// Add required components.
			part_node.AddComponent( "MeshFilter" );
			part_node.AddComponent( "MeshRenderer" );
			part_node.AddComponent( "HAPI_ChildSelectionControl" );
			MeshFilter mesh_filter							= part_node.GetComponent< MeshFilter >();
			HAPI_ChildSelectionControl child_control		= part_node.GetComponent< HAPI_ChildSelectionControl >();
			
			// Set Object Control on child selection control so it can read settings from here.
			child_control.setAsset( this );
			child_control.prObjectId	= object_id;
			child_control.prGeoId		= geo_id;
			child_control.prGeoType		= geo_info.type;
			child_control.prPartId		= part_id;
		
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
				Utility.getMesh( prAssetId, object_id, geo_id, part_id, part_mesh, child_control );
			}
			catch ( HAPI_Error error )
			{
				Debug.LogWarning( error.ToString() );
				return;
			}
		
			// Add Mesh-to-Prefab component.
			part_node.AddComponent( "HAPI_MeshToPrefab" );
			HAPI_MeshToPrefab mesh_saver = part_node.GetComponent< HAPI_MeshToPrefab >();
			mesh_saver.prObjectControl = this;
			mesh_saver.prGameObject = part_node;
			mesh_saver.prMeshName = this.prAssetInfo.name + "_" + part_node.name;
		}

		// Set visibility.
		MeshRenderer mesh_renderer = part_node.GetComponent< MeshRenderer >();
		mesh_renderer.enabled = object_info.isVisible;
		
		// Set material.
		if ( mesh_renderer.sharedMaterial == null )
			mesh_renderer.sharedMaterial = new Material( Shader.Find( "Specular" ) );
		if ( part_info.materialId >= 0 && ( prFullBuild || geo_info.hasMaterialChanged ) )
		{
			HAPI_MaterialInfo[] materials = new HAPI_MaterialInfo[ 1 ];
			HAPI_Host.getMaterials( prAssetId, materials, part_info.materialId, 1 );
			HAPI_MaterialInfo material = materials[ 0 ];

			// Assign the transparency shader if this material is transparent or unassign it otherwise.
			if ( material.isTransparent() && 
				 mesh_renderer.sharedMaterial.name == "Specular" )
				mesh_renderer.sharedMaterial = 
					new Material( Shader.Find( "Transparent/Specular" ) );
			else if ( !material.isTransparent() &&
					  mesh_renderer.sharedMaterial.name == "Transparent/Specular" )
				mesh_renderer.sharedMaterial =
					new Material( Shader.Find( "Specular" ) );

			Material mat = mesh_renderer.sharedMaterial;
			Utility.assignTexture( ref mat, material );
		}
	}
	
	private void createGeo( int object_id, int geo_id, GameObject geo_node )
	{
		// Get Geo info.
		HAPI_GeoInfo geo_info = new HAPI_GeoInfo();
		HAPI_Host.getGeoInfo( prAssetId, object_id, geo_id, out geo_info );

		if ( geo_info.type == (int) HAPI.HAPI_GeoType.HAPI_GEOTYPE_INPUT )
			return;

		if ( !prFullBuild && !geo_info.hasGeoChanged && !geo_info.hasMaterialChanged )
			return;

		if ( prFullBuild || geo_node.transform.childCount == 0 || geo_info.hasGeoChanged )
		{
			destroyChildren( geo_node.transform );
			for ( int ii = 0; ii < geo_info.partCount; ii++ )
			{
				GameObject part_node = new GameObject( "part" + ii );
				part_node.transform.parent = geo_node.transform;

				// Need to reset position here because the assignment above will massage the child's
				// position in order to be in the same place it was in the global namespace.
				part_node.transform.localPosition = new Vector3( 0.0f, 0.0f, 0.0f );
			}
			
			geo_info.hasGeoChanged = true;
		}

		// I'm assuming here the object order is maintained and will match their ids.
		int part_id = 0;
		foreach ( Transform part_trans in geo_node.transform )
		{
			part_trans.localPosition = new Vector3( 0.0f, 0.0f, 0.0f );
			createPart( object_id, geo_id, part_id, ref geo_info, part_trans.gameObject );
			part_id++;
		}

		geo_info.hasMaterialChanged = false;
	}
	
	private void createObject( int object_id )
	{
		HAPI_ObjectInfo object_info = prObjects[ object_id ];
		
		// Create main underling.
		if ( prGameObjects[ object_id ] == null )
		{
			prGameObjects[ object_id ] = new GameObject( object_info.name + "_obj" );
			prGameObjects[ object_id ].transform.parent = transform;
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
						GameObject geo_child = new GameObject( object_info.name + "_geo" + ii );
						geo_child.transform.parent = main_child.transform;
						
						// Need to reset position here because the assignment above will massage the child's
						// position in order to be in the same place it was in the global namespace.
						geo_child.transform.localPosition = new Vector3( 0.0f, 0.0f, 0.0f );
					}
				}
	
				// I'm assuming here the object order is maintained and will match their ids.
				int geo_id = 0;
				foreach ( Transform geo_trans in main_child.transform )
				{
					geo_trans.localPosition = new Vector3( 0.0f, 0.0f, 0.0f );
					createGeo( object_id, geo_id, geo_trans.gameObject );
					geo_id++;
				}
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
