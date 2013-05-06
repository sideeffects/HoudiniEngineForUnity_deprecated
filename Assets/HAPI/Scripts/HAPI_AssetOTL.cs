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
		// Save the asset type so we can restore it after the reset.
		HAPI_Asset.AssetType asset_type = prAssetType;

		base.reset();
		
		// Please keep these in the same order and grouping as their declarations at the top.
		
		prHandleInfos 				= new HAPI_HandleInfo[ 0 ];
		prHandleBindingInfos 		= null;

		// Need to restore the asset type here.
		prAssetType = asset_type;
	}
	
	public override void Update()
	{
		base.Update();

		if ( EditorApplication.isPlaying && prLiveInGameCooking )
		{
			HAPI_Host.setTime( Time.time );
			build();
		}
	}

	public override bool build() 
	{
		bool unload_asset_first = prReloadAssetInFullBuild 
								  && prAssetType == HAPI_Asset.AssetType.TYPE_OTL 
								  && !prPartialBuild;

		bool base_built = base.build( unload_asset_first );
		if ( !base_built )
			return false;

		return true;
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Protected Methods

	protected override int buildCreateAsset()
	{
		return HAPI_Host.loadOTL( prAssetPath );
	}

	protected override void buildFullBuildCustomWork( ref HAPI_ProgressBar progress_bar )
	{
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
	}

	protected override void buildCreateObjects( ref HAPI_ProgressBar progress_bar )
	{
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
			prGameObjects[ object_id ] = main_object;
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

		bool is_mesh = ( part_info.vertexCount > 0 );
		
		if ( prEnableLogging && ( prFullBuild || has_geo_changed || has_material_changed ) )
			Debug.Log( "Obj #" + part_control.prObjectId + " (" + part_control.prObjectName + "): "
					   + "verts: " + part_info.vertexCount + " faces: " + part_info.faceCount );

		if ( prFullBuild || has_geo_changed )
		{
			// Initialize our part control.
			part_control.init( this, part_info.id, part_info.name, part_info.materialId );

			// Overwrite name.
			part_node.name = part_info.name + "_part" + part_control.prPartId;

			if ( is_mesh ) // Valid mesh.
			{
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
			else if ( part_info.vertexCount <= 0 && part_info.pointCount > 0 ) // Particles?
			{
				// Get position attributes.
				HAPI_AttributeInfo pos_attr_info = new HAPI_AttributeInfo( "P" );
				float[] pos_attr = new float[ 0 ];
				Utility.getAttribute( prAssetId, part_control.prObjectId, part_control.prGeoId, 
									  part_control.prPartId, "P", ref pos_attr_info, ref pos_attr, 
									  HAPI_Host.getAttributeFloatData );
				if ( !pos_attr_info.exists )
					throw new HAPI_Error( "No position attribute found." );
				else if ( pos_attr_info.owner != (int) HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
					throw new HAPI_ErrorIgnorable( "I only understand position as point attributes!" );

				// Get colour attributes.
				HAPI_AttributeInfo colour_attr_info = new HAPI_AttributeInfo( "Cd" );
				float[] colour_attr = new float[ 0 ];
				Utility.getAttribute( prAssetId, part_control.prObjectId, part_control.prGeoId, 
									  part_control.prPartId, "Cd", ref colour_attr_info, ref colour_attr, 
									  HAPI_Host.getAttributeFloatData );

				ParticleEmitter particle_emitter = part_node.AddComponent( "EllipsoidParticleEmitter" ) as ParticleEmitter;
				particle_emitter.emit = false;
				particle_emitter.useWorldSpace = true;

				particle_emitter.maxSize = 0.6f;
				particle_emitter.minSize = 0.2f;
				//particle_emitter.maxSize = 0.06f;
				//particle_emitter.minSize = 0.02f;

				ParticleRenderer renderer = part_node.AddComponent< ParticleRenderer >();
				Material mat = new Material( Shader.Find( "Particles/Additive (Soft)" ) );
				int width = 20;
				int length = 20;
				Texture2D tex = new Texture2D( width, length, TextureFormat.RGBA32, false );
				for ( int x = 0; x < width; ++x ) 
				{
					for ( int y = 0; y < length; ++y ) 
					{
						float dist = (x - 10) * (x-10) + (y-10) * (y-10);
						dist = Mathf.Sqrt( dist );
						float alpha_f = 1.0f - dist / 10.0f;
						//Color col = new Color( 0.1f, 0.08f, 0.03f, alpha_f );
						Color col = new Color( 0.8f, 0.8f, 0.8f, alpha_f );
						tex.SetPixel( x, y, col );
					}
				}
				tex.Apply();
				mat.mainTexture = tex;
				mat.color = new Color( 1.0f, 1.0f, 0.5f );
				renderer.material = mat;

				particle_emitter.Emit( part_info.pointCount );

				Particle[] particles = particle_emitter.particles;

				for ( int i = 0; i < part_info.pointCount; ++i )
				{
					particles[ i ].position = new Vector3( pos_attr[ i * 3 + 0 ], 
														   pos_attr[ i * 3 + 1 ], 
														   pos_attr[ i * 3 + 2 ] );
					if ( colour_attr_info.exists )
						particles[ i ].color = new Color( colour_attr[ i * 4 + 0 ], 
														  colour_attr[ i * 4 + 1 ], 
														  colour_attr[ i * 4 + 2 ], 
														  colour_attr[ i * 4 + 3 ] );
				}

				particle_emitter.particles = particles;
			}
		}

		// Set visibility.
		if ( part_control.prPartName != HAPI_Host.prCollisionGroupName && is_mesh )
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
				part_node.transform.localPosition	= new Vector3();
				part_node.transform.localRotation	= new Quaternion();
				part_node.transform.localScale		= new Vector3( 1.0f, 1.0f, 1.0f );

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
						geo_child.transform.localPosition	= new Vector3();
						geo_child.transform.localRotation	= new Quaternion();
						geo_child.transform.localScale		= new Vector3( 1.0f, 1.0f, 1.0f );

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
