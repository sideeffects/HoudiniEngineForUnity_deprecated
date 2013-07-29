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
#define ENABLE_PARTICLES

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
	
	public string 					prAssetPath {	get { return myAssetPath; } 
													set { myAssetPath = value; } }
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

		if ( EditorApplication.isPlaying && prPlaymodePerFrameCooking )
		{
			HAPI_Host.setTime( Time.time );
			buildClientSide();
		}
	}

	public override bool buildAll()
	{
		bool unload_asset_first = ( prAssetType == HAPI_Asset.AssetType.TYPE_OTL );

		return base.build(	true,	// reload_asset
							unload_asset_first,
							false,	// serializatin_recovery_only
							true,	// force_reconnect
							prCookingTriggersDownCooks,
							false	// use_delay_for_progress_bar
						);
	}

	public override bool build( bool reload_asset, bool unload_asset_first,
								bool serialization_recovery_only,
								bool force_reconnect,
								bool cook_downstream_assets,
								bool use_delay_for_progress_bar ) 
	{
		unload_asset_first = unload_asset_first 
							 && prAssetType == HAPI_Asset.AssetType.TYPE_OTL 
							 && !serialization_recovery_only;

		bool base_built = base.build( reload_asset, unload_asset_first, serialization_recovery_only, 
									  force_reconnect, cook_downstream_assets,
									  use_delay_for_progress_bar );
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

	protected override void buildCreateObjects( bool reload_asset, ref HAPI_ProgressBar progress_bar )
	{
		for ( int object_index = 0; object_index < prObjectCount; ++object_index )
		{
			progress_bar.incrementProgressBar();
			try
			{
				if ( !prObjects[ object_index ].isInstancer && 
						( reload_asset	|| prObjects[ object_index ].hasTransformChanged
										|| prObjects[ object_index ].haveGeosChanged ) )
				{
					createObject( object_index, reload_asset );
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
						createObject( object_info.objectToInstanceId, reload_asset );
						
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
		HAPI_ObjectInfo object_info		= prObjects[ object_id ];
		HAPI_Instancer instancer		= null;
		
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
		
		instancer.prAsset		= this;
		instancer.prObjectId	= object_id;
		
		instancer.instanceObjects( progress_bar );
	}

	private Material createSoftCircle( Color color )
	{
		Material mat = new Material( Shader.Find( "Particles/Additive (Soft)" ) );
		int width = 20;
		int length = 20;
		Texture2D tex = new Texture2D( width, length, TextureFormat.RGBA32, false );
		for ( int x = 0; x < width; ++x ) 
		{
			for ( int y = 0; y < length; ++y ) 
			{
				float dist = (x - 10) * (x-10) + (y-10) * (y-10);
				//dist = Mathf.Sqrt( dist );
				float alpha_f = 1.0f - dist / 100.0f;
				color.a = Mathf.Lerp( 0.0f, 1.0f, alpha_f );
				tex.SetPixel( x, y, color );
			}
		}
		tex.Apply();
		mat.mainTexture = tex;
		mat.color = new Color( 1.0f, 1.0f, 1.0f );
		return mat;
	}

	private Material createSoftSquare( Color color )
	{
		Material mat = new Material( Shader.Find( "Particles/Additive (Soft)" ) );
		int width = 20;
		int length = 20;
		Texture2D tex = new Texture2D( width, length, TextureFormat.RGBA32, false );
		for ( int x = 0; x < width; ++x ) 
		{
			for ( int y = 0; y < length; ++y ) 
			{
				Vector2 pos = new Vector2(x - width / 2, y - length / 2);
				float linear_dist = Mathf.Max( Mathf.Abs( pos.x ), Mathf.Abs( pos.y ) );
				float alpha =  linear_dist / ( width / 2 );
				alpha = 1.0f - alpha;
				color.a = alpha;
				tex.SetPixel( x, y, color );
			}
		}
		tex.Apply();
		mat.mainTexture = tex;
		mat.color = new Color( 1.0f, 1.0f, 1.0f );
		return mat;
	}

	private void createFogVolume( GameObject node, float[] data, HAPI_VolumeTile tile, HAPI_VolumeInfo volume )
	{
		// Create a particle with alpha = to the data format
		const float particle_epsilon = 0.0f;
		int nparticles = 0;
		for ( int i = 0; i < data.Length; ++i )
			if ( data[ i ] > particle_epsilon )
				nparticles++;
		if ( nparticles == 0 )
		{
			DestroyImmediate( node );
			return;
		}

		ParticleEmitter particle_emitter = node.AddComponent( "EllipsoidParticleEmitter" ) as ParticleEmitter;
		particle_emitter.emit = false;
		particle_emitter.maxSize = volume.transform.scale[0]*2;
		particle_emitter.minSize = volume.transform.scale[1]*2;
		particle_emitter.ClearParticles();
		particle_emitter.Emit( nparticles );

		Vector3 tileMin = new Vector3( tile.minX, tile.minY, tile.minZ );
		int part_index = 0;
		Particle[] particles = particle_emitter.particles;
		for ( int z = 0; z < volume.tileSize; ++z )
			for ( int y = 0; y < volume.tileSize; ++y )
				  for ( int x = 0; x < volume.tileSize; ++x )
				  {
					  int index = z * volume.tileSize * volume.tileSize + y * volume.tileSize + x;
					  if ( data[ index ] > particle_epsilon
						   && part_index < particles.Length )
					  {
						  particles[ part_index ].position =
							  node.transform.parent.TransformPoint( new Vector3( (float)x, (float)y, (float)z )
																	+ tileMin );
						  particles[ part_index ].color    =
							  new Color( data[ index ], data[ index ], data[ index ], data[ index ] );
						  part_index++;
					  }
				  }
		particle_emitter.particles = particles;

		ParticleRenderer renderer = node.GetComponent< ParticleRenderer >();
		if ( renderer == null ) 
			renderer = node.AddComponent< ParticleRenderer >();

		renderer.material = createSoftCircle( new Color( Random.Range( 0.5f, 1.0f ),
														 Random.Range( 0.5f, 1.0f ),
														 Random.Range( 0.5f, 1.0f ) ) );
	}


	private void createPart( GameObject part_node, bool reload_asset, bool has_geo_changed, 
							 bool has_material_changed )
	{
		HAPI_PartControl part_control = part_node.GetComponent< HAPI_PartControl >();
		if ( part_control == null )
			return;

		HAPI_PartInfo part_info = new HAPI_PartInfo();
		
		// Get Part info.
		HAPI_Host.getPartInfo( prAssetId, part_control.prObjectId, part_control.prGeoId, 
							   part_control.prPartId, out part_info );

		bool is_mesh = ( part_info.vertexCount > 0 );
		
		if ( prEnableLogging && ( reload_asset || has_geo_changed || has_material_changed ) )
			Debug.Log( "Obj #" + part_control.prObjectId + " (" + part_control.prObjectName + "): "
					   + "verts: " + part_info.vertexCount + " faces: " + part_info.faceCount );

		if ( reload_asset || has_geo_changed )
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
					Utility.getMesh( part_control, part_mesh, prGenerateTangents && HAPI_Host.prGenerateTangents );
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
#if ENABLE_PARTICLES // Particles can get a bit annoying right now since our filter for what is a particle system is too broad.
			else if ( HAPI_Host.prEnablePointsAsParticles && part_info.vertexCount <= 0 && part_info.pointCount > 0 ) // Particles?
			{
				// Get position attributes.
				HAPI_AttributeInfo pos_attr_info = new HAPI_AttributeInfo( HAPI_Constants.HAPI_ATTRIB_POSITION );
				float[] pos_attr = new float[ 0 ];
				Utility.getAttribute( prAssetId, part_control.prObjectId, part_control.prGeoId, 
									  part_control.prPartId, HAPI_Constants.HAPI_ATTRIB_POSITION, ref pos_attr_info, ref pos_attr, 
									  HAPI_Host.getAttributeFloatData );
				if ( !pos_attr_info.exists )
					throw new HAPI_Error( "No position attribute found." );
				else if ( pos_attr_info.owner != (int) HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
					throw new HAPI_ErrorIgnorable( "I only understand position as point attributes!" );

				// Get colour attributes.
				HAPI_AttributeInfo colour_attr_info = new HAPI_AttributeInfo( HAPI_Constants.HAPI_ATTRIB_COLOUR );
				float[] colour_attr = new float[ 0 ];
				Utility.getAttribute( prAssetId, part_control.prObjectId, part_control.prGeoId, 
									  part_control.prPartId, HAPI_Constants.HAPI_ATTRIB_COLOUR, 
									  ref colour_attr_info, ref colour_attr, HAPI_Host.getAttributeFloatData );

				ParticleEmitter particle_emitter = part_node.AddComponent( "EllipsoidParticleEmitter" ) as ParticleEmitter;
				particle_emitter.emit = false;
				particle_emitter.useWorldSpace = true;

				particle_emitter.maxSize = 0.06f;
				particle_emitter.minSize = 0.02f;
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

				if ( particle_emitter.particles.Length < part_info.pointCount )
					Debug.LogWarning( "Geo has too many particles. Expected less than "
									  + particle_emitter.particles.Length
									  + " but found " + part_info.pointCount + ". "
									  + " Only using the first "
									  + particle_emitter.particles.Length + ".");


				for ( int i = 0; i < particle_emitter.particles.Length; ++i )
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
#endif
			if ( part_info.hasVolume )
			{
				// If we have a volume, retrieve the volume info
				HAPI_VolumeInfo volume = new HAPI_VolumeInfo();
				HAPI_Host.getVolumeInfo( prAssetId, part_control.prObjectId, part_control.prGeoId, 
										 part_control.prPartId, ref volume );
				volume.transform.scale[0] *= 20;
				volume.transform.scale[1] *= 20;
				volume.transform.scale[2] *= 20;


				// Iterate through the voxels and print out the data,
				// for now.
				HAPI_VolumeTile tile = new HAPI_VolumeTile();
				HAPI_Host.getFirstVolumeTile( prAssetId, part_control.prObjectId, part_control.prGeoId,
											  part_control.prPartId, ref tile );
				float[] values = new float[ volume.tileSize * volume.tileSize * volume.tileSize ];
				int tile_num = 0;

				Utility.applyTransform( volume.transform, part_node.transform );

				while ( tile.isValid() )
				{
					for ( int i = 0; i < values.Length; ++i )
						values[i] = 0;
					HAPI_Host.getVolumeTileFloatData( prAssetId, part_control.prObjectId, part_control.prGeoId,
											  part_control.prPartId, ref tile, values );

					tile_num += 1;
					GameObject tile_node = new GameObject( "tile (" + tile.minX +
														   ", " + tile.minY +
														   ", " + tile.minZ + ")" );
					tile_node.transform.parent = part_node.transform;

					createFogVolume( tile_node, values, tile, volume );

					HAPI_Host.getNextVolumeTile( prAssetId, part_control.prObjectId, part_control.prGeoId,
												 part_control.prPartId, ref tile );
				}
			}
		}

		Utility.assignMaterial( part_control, this, 
								( reload_asset || has_material_changed ) );
	}
	
	private void createGeo( GameObject geo_node, bool reload_asset, bool first_time )
	{
		HAPI_GeoControl geo_control = geo_node.GetComponent< HAPI_GeoControl >();
		if ( geo_control == null )
			return;

		// Get Geo info.
		HAPI_GeoInfo geo_info = new HAPI_GeoInfo();
		HAPI_Host.getGeoInfo( prAssetId, geo_control.prObjectId, geo_control.prGeoId, out geo_info );

		if ( geo_info.type == (int) HAPI.HAPI_GeoType.HAPI_GEOTYPE_INPUT )
			return;

		if ( !first_time && !reload_asset && !geo_info.hasGeoChanged && !geo_info.hasMaterialChanged )
			return;

		if ( first_time || reload_asset || geo_node.transform.childCount == 0 || geo_info.hasGeoChanged )
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
				createPart( part_node, reload_asset, true, geo_info.hasMaterialChanged );
			}
			
			geo_info.hasGeoChanged = true;
		}
		else
			foreach ( Transform part_trans in geo_node.transform )
				createPart( part_trans.gameObject, reload_asset, geo_info.hasGeoChanged, 
							geo_info.hasMaterialChanged );
		
		geo_info.hasMaterialChanged = false;
		
		if( (first_time || reload_asset) && geo_info.partCount > 0 )
		{
			HAPI_AttributeInfo script_attr_info = new HAPI_AttributeInfo( "Unity_Script" );
			int[] script_attr = new int[ 0 ];
			
			Utility.getAttribute( prAssetId, geo_control.prObjectId, geo_control.prGeoId, 0, "Unity_Script",
								  ref script_attr_info, ref script_attr, HAPI_Host.getAttributeStrData );
			
			if ( script_attr_info.exists && script_attr_info.owner != (int) HAPI_AttributeOwner.HAPI_ATTROWNER_DETAIL )
				throw new HAPI_ErrorIgnorable( "I only understand Unity_Script as detail attributes!" );
			
			if( script_attr_info.exists && script_attr.Length > 0 )
			{
				string script_to_attach = HAPI_Host.getString( script_attr[ 0 ] );
				HAPI_AssetUtility.attachScript( geo_node, script_to_attach );
			}
		}
				
	}
	
	private void createObject( int object_id, bool reload_asset )
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
			if ( reload_asset || object_info.haveGeosChanged )
			{
				if ( reload_asset || object_info.geoCount != main_child.transform.childCount )
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

						createGeo( geo_child, reload_asset, true );
					}
				}
				else
					foreach ( Transform geo_trans in main_child.transform )
						createGeo( geo_trans.gameObject, reload_asset, false );
			}
			
			if ( reload_asset || object_info.hasTransformChanged )
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
