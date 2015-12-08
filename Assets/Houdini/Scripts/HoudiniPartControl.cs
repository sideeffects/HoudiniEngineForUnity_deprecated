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
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;

// Typedefs
using HAPI_NodeId = System.Int32;

[ ExecuteInEditMode ]
public class HoudiniPartControl : HoudiniGeoControl 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	public int		prPartId {							get { return myPartId; }
														set { myPartId = value; } }
	public string	prPartName {						get { return myPartName; }
														set { myPartName = value; } }
	public int		prMaterialId {						get { return myMaterialId; }
														set { myMaterialId = value; } }
	public int[]	prVertexList {						get { return myVertexList; }
														set { myVertexList = value; } }

	public bool		prShowDisplayOptions {				get { return myShowDisplayOptions; }
														set { myShowDisplayOptions = value; } }
	public bool		prShowPointGroup {					get { return myShowPointGroup; }
														set { myShowPointGroup = value; } }
	public string	prSelectedPointGroup {				get { return mySelectedPointGroup; }
														set { mySelectedPointGroup = value; } }
	public bool		prShowPrimitiveGroup {				get { return myShowPrimitiveGroup; }
														set { myShowPrimitiveGroup = value; } }
	public string	prSelectedPrimitiveGroup {			get { return mySelectedPrimitiveGroup; }
														set { mySelectedPrimitiveGroup = value; } }
	public bool		prShowPointNumbers {				get { return myShowPointNumbers; }
														set { myShowPointNumbers = value; } }

	public bool		prShowIntermediateResultControls {	get { return myShowIntermediateResultControls; }
														set { myShowIntermediateResultControls = value; } }
	public bool		prShowInfo {						get { return myShowInfo; }
														set { myShowInfo = value; } }

	public bool 	prTransformChanged {				get { return myTransformChanged; }
														set { myTransformChanged = value; } }
	
	public HoudiniGeoControl prGeoControl {				get { return myGeoControl; }
														set { myGeoControl = value; } }

	public HoudiniPartControl()
	{
		reset();
	}

	~HoudiniPartControl()
	{

	}
	
	public override void reset()
	{
		base.reset();

		// Please keep these in the same order and grouping as their declarations at the top.
		
		prPartId = -1;
		prPartName = "part_name";
		prMaterialId = -1;
		prVertexList = new int[ 1 ];
		myTransformChanged = false;

		myShowDisplayOptions = true;
		myShowPointGroup = false;
		mySelectedPointGroup = "";
		myShowPrimitiveGroup = false;
		mySelectedPrimitiveGroup = "";
		myShowPointNumbers = false;

		myShowIntermediateResultControls = false;
		myShowInfo = false;

		myGeoControl = null;
	}

	public void Awake()
	{
		myLastLocalToWorld = transform.localToWorldMatrix;
	}

	public void selectParent()
	{
#if UNITY_EDITOR
		if ( prAsset != null && prAsset.prAutoSelectAssetRootNode && HoudiniHost.prAutoSelectAssetRootNode )
		{
			GameObject[] selection 	= new GameObject[ 1 ];
			selection[ 0 ] 			= prAsset.gameObject;
			Selection.objects 		= selection;
		}
#endif // UNITY_EDITOR
	}

	public void init( HoudiniPartControl part_control )
	{
		init( (HoudiniGeoControl) part_control );

		prAsset			= part_control.prAsset;
		prPartId		= part_control.prPartId;
		prPartName		= part_control.prPartName;
	}

	public void init( HAPI_NodeId node_id, int part_id, string part_name )
	{
		prNodeId		= node_id;
		prPartId		= part_id;
		prPartName		= part_name;
	}

	public override string getRelativePath()
	{
		return base.getRelativePath() + "/" + prPartName;
	}

	public override string getAbsolutePath()
	{
		return base.getAbsolutePath() + "/" + prPartName;
	}

	public void refresh( bool reload_asset, bool has_geo_changed, bool has_material_changed )
	{
		if ( prGeoControl == null )
		{
			Debug.LogError( "Why is my geo control null on a refresh?" );
			return;
		}

		GameObject part_node = gameObject;

		// Get Part info.
		HAPI_PartInfo part_info = new HAPI_PartInfo();
		HoudiniHost.getPartInfo( prAssetId, prObjectId, prGeoId, prPartId, out part_info );

		bool is_empty = part_info.vertexCount <= 0 && part_info.pointCount <= 0;
		bool is_mesh = ( part_info.vertexCount > 0 );

		// For Debugging.
#if false
		Debug.Log( "ATTRIBS" );
			HoudiniAssetUtility.printAllAttributeNames( prAssetId, prObjectId, prGeoId, prPartId );
		Debug.Log( "GROUPS" );
			HoudiniAssetUtility.printAllGroups( prAssetId, prObjectId, prGeoId, prPartId );
		Debug.Log( "DONE" );
		Debug.Log( "" );
#endif

		// TODO: Make this info a permanent UI display.
		//if ( prEnableLogging && ( reload_asset || has_geo_changed || has_material_changed ) )
		//	Debug.Log( "Obj #" + part_control.prObjectId + " (" + part_control.prObjectName + "): "
		//			   + "verts: " + part_info.vertexCount + " faces: " + part_info.faceCount );

		if ( reload_asset || has_geo_changed )
		{
			// Initialize our part control.
			init( -1, part_info.id, part_info.name );

			// Overwrite name.
			part_node.name = part_info.name;

			if ( is_empty )
			{
				// Add required components.
				MeshFilter mesh_filter = getOrCreateComponent< MeshFilter >();

				// Get or create mesh.
				Mesh part_mesh 				= mesh_filter.sharedMesh;
				if ( part_mesh == null ) 
				{
					mesh_filter.mesh 		= new Mesh();
					part_mesh 				= mesh_filter.sharedMesh;
				}
				part_mesh.Clear();
			}
			else if ( is_mesh ) // Valid mesh.
			{
				// Add required components.
				MeshFilter mesh_filter = getOrCreateComponent< MeshFilter >();

				// Get or create mesh.
				Mesh part_mesh = mesh_filter.sharedMesh;
				if ( part_mesh == null ) 
				{
					mesh_filter.mesh = new Mesh();
					part_mesh = mesh_filter.sharedMesh;
					part_mesh.name = getAbsolutePath() + "/Mesh";
				}
				part_mesh.Clear();
		
				// Get mesh.
				try
				{
					HoudiniAssetUtility.getMesh(
						this, part_mesh,
						prAsset.prGenerateUVs,
						prAsset.prGenerateLightmapUV2s,
						prAsset.prGenerateTangents );
				}
				catch ( HoudiniErrorIgnorable ) {}
				catch ( HoudiniError error )
				{
					Debug.LogWarning( error.ToString() );
					return;
				}

				// Add collider if group name matches. (Should be added after the mesh is set so that it
				// picks up the mesh automagically)
				if ( part_info.name.Contains( HoudiniHost.prRenderedCollisionGroupName ) )
				{
					MeshCollider mesh_collider = getOrCreateComponent< MeshCollider >();
					getOrCreateComponent< MeshRenderer >();
					mesh_collider.enabled = false;
					mesh_collider.enabled = true;
				}
				else if ( part_info.name.Contains( HoudiniHost.prCollisionGroupName ) )
				{
					MeshCollider mesh_collider = getOrCreateComponent< MeshCollider >();
					mesh_collider.enabled = false;
					mesh_collider.enabled = true;
				}
				else
				{
					getOrCreateComponent< MeshRenderer >();
				}

				if ( myGeoControl.prGeoType == HAPI_GeoType.HAPI_GEOTYPE_INTERMEDIATE )
				{
					MeshRenderer mesh_renderer = getOrCreateComponent< MeshRenderer >();
					MeshCollider mesh_collider = getOrCreateComponent< MeshCollider >();
					if ( myGeoControl.prGeoAttributeManager )
					{
						myGeoControl.prGeoAttributeManager.reInit(
							part_mesh, mesh_renderer, mesh_collider, transform );
					}
					mesh_collider.enabled = false;
					mesh_collider.enabled = true;
				}

				// Add Mesh-to-Prefab component.
				HoudiniMeshToPrefab mesh_saver = getOrCreateComponent< HoudiniMeshToPrefab >();
				mesh_saver.prGameObject = part_node;
				mesh_saver.prMeshName = prAsset.prAssetName + "_" + part_node.name;
			}
			else if ( HoudiniHost.prEnablePointsAsParticles && part_info.vertexCount <= 0 && part_info.pointCount > 0 ) // Particles?
			{
				// Get position attributes.
				HAPI_AttributeInfo pos_attr_info = new HAPI_AttributeInfo( HoudiniConstants.HAPI_ATTRIB_POSITION );
				float[] pos_attr = new float[ 0 ];
				HoudiniAssetUtility.getAttribute(
					prAssetId, prObjectId, prGeoId, prPartId, HoudiniConstants.HAPI_ATTRIB_POSITION, 
					ref pos_attr_info, ref pos_attr, HoudiniHost.getAttributeFloatData );
				if ( !pos_attr_info.exists )
					throw new HoudiniError( "No position attribute found." );
				else if ( pos_attr_info.owner != HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
					throw new HoudiniErrorIgnorable( "I only understand position as point attributes!" );

				// Get colour attributes.
				HAPI_AttributeInfo colour_attr_info = new HAPI_AttributeInfo( HoudiniConstants.HAPI_ATTRIB_COLOR );
				float[] colour_attr = new float[ 0 ];
				HoudiniAssetUtility.getAttribute( 
					prAssetId, prObjectId, prGeoId, prPartId, HoudiniConstants.HAPI_ATTRIB_COLOR,
					ref colour_attr_info, ref colour_attr, HoudiniHost.getAttributeFloatData );

				ParticleEmitter particle_emitter = part_node.GetComponent< ParticleEmitter >();
				if ( particle_emitter == null )
				{
#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6
					particle_emitter = part_node.AddComponent( "EllipsoidParticleEmitter" ) as ParticleEmitter;
#else
					particle_emitter = part_node.AddComponent< EllipsoidParticleEmitter >() as ParticleEmitter;
#endif // UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6
				}
				particle_emitter.ClearParticles();
				
				particle_emitter.emit = false;
				particle_emitter.useWorldSpace = true;

				particle_emitter.maxSize = 0.06f;
				particle_emitter.minSize = 0.02f;

				ParticleRenderer renderer = getOrCreateComponent< ParticleRenderer >();
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
					if ( colour_attr_info.exists && colour_attr_info.owner == HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
					{
						float alpha =
							colour_attr_info.tupleSize == 4
								? colour_attr[ i * colour_attr_info.tupleSize + 3 ]
								: 1.0f;
						particles[ i ].color = new Color( colour_attr[ i * colour_attr_info.tupleSize + 0 ], 
														  colour_attr[ i * colour_attr_info.tupleSize + 1 ], 
														  colour_attr[ i * colour_attr_info.tupleSize + 2 ], 
														  alpha );
					}
				}

				particle_emitter.particles = particles;
			}
			
			if ( part_info.type == HAPI_PartType.HAPI_PARTTYPE_VOLUME )
			{
				// Clear previous volume tiles.
				destroyChildren( part_node.transform );

				// If we have a volume, retrieve the volume info
				HAPI_VolumeInfo volume = new HAPI_VolumeInfo();
				HoudiniHost.getVolumeInfo( prAssetId, prObjectId, prGeoId, prPartId, ref volume );

				// The volume.transform.scale is the voxel size. Both the particle
				// delta and the point size are affected by the voxel size.
				HoudiniAssetUtility.applyTransform( volume.transform, part_node.transform );
				float particle_delta = HoudiniConstants.HAPI_VOLUME_SURFACE_DELTA_MULT * Mathf.Max( Mathf.Max( 
					volume.transform.scale[ 0 ],
					volume.transform.scale[ 1 ] ),
					volume.transform.scale[ 2 ] );
				float point_size = HoudiniConstants.HAPI_VOLUME_SURFACE_PT_SIZE_MULT * particle_delta;

				List< Vector3 > acc_vertices = new List< Vector3 >();
				List< Vector3 > acc_normals = new List< Vector3 >();

				float[] values = new float[ volume.tileSize * volume.tileSize * volume.tileSize ];
				int tile_index = 0;
				int current_container_particle_index = 0;

				// Iterate through the voxels and print out the data, for now.
				HAPI_VolumeTileInfo tile = new HAPI_VolumeTileInfo();
				HoudiniHost.getFirstVolumeTile( prAssetId, prObjectId, prGeoId, prPartId, ref tile );
				while ( tile.isValid )
				{
					for ( int i = 0; i < values.Length; ++i )
						values[ i ] = 0;
					HoudiniHost.getVolumeTileFloatData( 
						prAssetId, prObjectId, prGeoId, prPartId, ref tile, values );

					Vector3 tileMin = new Vector3( tile.minX, tile.minY, tile.minZ );
					int part_index = 0;
					for ( int z = 0; z < volume.tileSize; ++z )
						for ( int y = 0; y < volume.tileSize; ++y )
							for ( int x = 0; x < volume.tileSize; ++x )
							{
								int index = z * volume.tileSize * volume.tileSize + y * volume.tileSize + x;
								if ( values[ index ] > -particle_delta && values[ index ] < particle_delta )
								{
									// Make sure we have enough room in our arrays.
									if ( current_container_particle_index
										> HoudiniConstants.HAPI_VOLUME_SURFACE_MAX_PT_PER_C )
									{
										createVolumeTilesObject(
											point_size, part_node.transform, acc_vertices, acc_normals );
										current_container_particle_index = 0;
										acc_vertices.Clear();
										acc_normals.Clear();
									}

									// Get particle position.
									Vector3 pos = new Vector3( (float) x, (float) y, (float) z );
									pos = HoudiniConstants.HAPI_VOLUME_POSITION_MULT * ( pos + tileMin );
									pos.x = -pos.x;
									acc_vertices.Add( part_node.transform.TransformPoint( pos ) );

									// Get particle normal.
									int amount = 1;
									int sample_count = 0;
									Vector3 average_normal = Vector3.zero;
									for ( int xi = -1; xi <= 1; ++xi )
										for ( int yi = -1; yi <= 1; ++yi )
											for ( int zi = -1; zi <= 1; ++zi )
											{
												if ( xi == 0 && yi == 0 && zi == 0 )
													continue;

												float result = getVolumeData(
													values, volume, particle_delta, x + xi * amount, y + yi * amount, z + zi * amount );

												Vector3 normal = Vector3.zero;
												if ( result < -0.5f )
													normal = new Vector3( -xi, -yi, -zi );
												else if ( result > 0.5f )
													normal = new Vector3( xi, yi, zi );
												else
													continue;

												average_normal += normal;
												sample_count++;
											}
									average_normal /= sample_count;
									average_normal.Normalize();
									acc_normals.Add( average_normal );

									part_index++;
									current_container_particle_index++;
								}
							}

					HoudiniHost.getNextVolumeTile( prAssetId, prObjectId, prGeoId, prPartId, ref tile );

					tile_index++;
				} // tile iteration

				// If we have left-over particles in our arrays we need another container.
				createVolumeTilesObject( point_size, part_node.transform, acc_vertices, acc_normals );

			} // if has volume
			else // Restore part node if previously used to store volume.
			{
				// Clear previous volume tiles.
				destroyChildren( part_node.transform );
				part_node.transform.localScale = Vector3.one;
				part_node.transform.localPosition = Vector3.zero;
				part_node.transform.localRotation = Quaternion.identity;
			}
		}

		// Refresh enabled flags.
		{
			bool is_visible = prObjectVisible;
			is_visible &= ( prAsset.prIsGeoVisible || prGeoType == HAPI_GeoType.HAPI_GEOTYPE_INTERMEDIATE );
			if ( prGeoType == HAPI_GeoType.HAPI_GEOTYPE_INTERMEDIATE &&
				myGeoControl.prGeoAttributeManager != null )
				is_visible &=
					myGeoControl.prGeoAttributeManager.prCurrentMode != HoudiniGeoAttributeManager.Mode.NONE;

			if ( gameObject.GetComponent< MeshCollider >() )
				gameObject.GetComponent< MeshCollider >().enabled = is_visible;
			if ( gameObject.GetComponent< MeshRenderer >() )
				gameObject.GetComponent< MeshRenderer >().enabled = is_visible;
		}

		// Assign materials.
		HoudiniAssetUtility.assignMaterial( this, prAsset, reload_asset );

		// Assign unity tag.
		assignUnityTag();
	}

	public void createVolumeTilesObject(
		float point_size, Transform parent, List< Vector3 > vertices, List< Vector3 > normals )
	{
		GameObject tiles_node = new GameObject( "VolumeTiles" );
		tiles_node.transform.parent = parent;

		MeshFilter mesh_filter = HoudiniControl.getOrCreateComponent< MeshFilter >( tiles_node );
		MeshRenderer mesh_renderer = HoudiniControl.getOrCreateComponent< MeshRenderer >( tiles_node );

		if ( !mesh_filter.sharedMesh )
			mesh_filter.sharedMesh = new Mesh();
		mesh_filter.sharedMesh.Clear();

		if ( !mesh_renderer.sharedMaterial )
			mesh_renderer.sharedMaterial = new Material( Shader.Find( "Houdini/VolumeSurface" ) );
		mesh_renderer.sharedMaterial.SetFloat( "_PointSize", point_size );
		mesh_renderer.sharedMaterial.SetColor( "_Color", new Color( 0.9f, 0.9f, 0.9f ) );

		Vector3[] mesh_vertices = vertices.ToArray();
		Vector3[] mesh_normals = normals.ToArray();
		int[] indices = new int[ vertices.Count ];

		for ( int i = 0; i < vertices.Count; ++i )
			indices[ i ] = i;

		mesh_filter.sharedMesh.vertices = mesh_vertices;
		mesh_filter.sharedMesh.normals = mesh_normals;
		mesh_filter.sharedMesh.SetIndices( indices, MeshTopology.Points, 0 );
	}

#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
	public virtual void Update()
	{
		Matrix4x4 local_to_world = transform.localToWorldMatrix;
		
		if ( local_to_world == myLastLocalToWorld )
			return;

		myLastLocalToWorld = local_to_world;
		myTransformChanged = true;
	}
#endif // ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private

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

	private Material createSquare( Color color )
	{
		Material mat = new Material( Shader.Find( "Houdini/VolumeSurface" ) );
		int width = 20;
		int length = 20;
		Texture2D tex = new Texture2D( width, length, TextureFormat.RGBA32, false );
		for ( int x = 0; x < width; ++x ) 
		{
			for ( int y = 0; y < length; ++y ) 
			{
				color.a = 1.0f;
				tex.SetPixel( x, y, color );
			}
		}
		tex.Apply();
		mat.mainTexture = tex;
		mat.color = new Color( 1.0f, 1.0f, 1.0f );
		return mat;
	}

	private float getVolumeData( float[] data, HAPI_VolumeInfo volume, float delta, int x, int y, int z )
	{
		if ( x < 0 || x >= volume.tileSize
			|| y < 0 || y >= volume.tileSize
			|| z < 0 || z >= volume.tileSize )
			return 0.0f;

		int index = z * volume.tileSize * volume.tileSize + y * volume.tileSize + x;

		if ( data[ index ] > delta )
			return 1.0f;
		else if ( data[ index ] < -delta )
			return -1.0f;
		else
			return 0.0f;
	}

	private void createFogVolume( GameObject node, float[] data, HAPI_VolumeTileInfo tile, HAPI_VolumeInfo volume )
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

#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6
		ParticleEmitter particle_emitter = node.AddComponent( "EllipsoidParticleEmitter" ) as ParticleEmitter;
#else
		ParticleEmitter particle_emitter = node.AddComponent< EllipsoidParticleEmitter >() as ParticleEmitter;
#endif // UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6
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
						Vector3 pos = new Vector3( (float)x, (float)y, (float)z );
						pos = 2 * ( ( pos + tileMin ) - new Vector3( 0.5f, 0.5f, 0.5f ) );

						particles[ part_index ].position =
							node.transform.parent.TransformPoint( pos );
						particles[ part_index ].color =
							new Color( data[ index ], data[ index ], data[ index ], data[ index ] );
						part_index++;
					}
				}
		particle_emitter.particles = particles;

		ParticleRenderer renderer = node.GetComponent< ParticleRenderer >();
		if ( renderer == null ) 
			renderer = node.AddComponent< ParticleRenderer >();

		renderer.material = createSoftCircle(
			new Color( Random.Range( 0.5f, 1.0f ), Random.Range( 0.5f, 1.0f ), Random.Range( 0.5f, 1.0f ) ) );
	}

	private void createSurfaceVolume( GameObject node, float[] data, HAPI_VolumeTileInfo tile, HAPI_VolumeInfo volume )
	{
		// Create a particle with alpha = to the data format
		const float particle_epsilon = 0.05f;
		int particle_count = 0;
		for ( int i = 0; i < data.Length; ++i )
			if ( data[ i ] > -particle_epsilon && data[ i ] < particle_epsilon )
				particle_count++;

		MeshFilter mesh_filter = HoudiniControl.getOrCreateComponent< MeshFilter >( node );
		MeshRenderer mesh_renderer = HoudiniControl.getOrCreateComponent< MeshRenderer >( node );

		if ( !mesh_filter.sharedMesh )
			mesh_filter.sharedMesh = new Mesh();
		mesh_filter.sharedMesh.Clear();

		if ( particle_count <= 0 )
			return;

		if ( !mesh_renderer.sharedMaterial )
			mesh_renderer.sharedMaterial = new Material( Shader.Find( "Houdini/VolumeSurface" ) );
		mesh_renderer.sharedMaterial.SetFloat( "_PointSize", 70.0f );
		mesh_renderer.sharedMaterial.SetColor( "_Color", new Color( 1.0f, 0.9f, 0.9f ) );

		Vector3[] vertices = new Vector3[ particle_count ];
		Color[] colors = new Color[ particle_count ];
		Vector3[] normals = new Vector3[ particle_count ];
		Vector2[] uvs = new Vector2[ particle_count ];

		// Create the selection indices.
		int[] indices = new int[ vertices.Length ];
		for ( int i = 0; i < vertices.Length; ++i )
			indices[ i ] = i;

		Vector3 tileMin = new Vector3( tile.minX, tile.minY, tile.minZ );
		//float volume_scale = volume.transform.scale[ 0 ] * 2.0f;
		int part_index = 0;
		for ( int z = 0; z < volume.tileSize; ++z )
			for ( int y = 0; y < volume.tileSize; ++y )
				for ( int x = 0; x < volume.tileSize; ++x )
				{
					int index = z * volume.tileSize * volume.tileSize + y * volume.tileSize + x;
					if ( data[ index ] > -particle_epsilon && data[ index ] < particle_epsilon
						&& part_index < particle_count )
					{
						// Get particle position.
						Vector3 pos = new Vector3( (float) x, (float) y, (float) z );
						pos = 1.2f * ( ( pos + tileMin ) - new Vector3( 0.5f, 0.5f, 0.5f ) );
						vertices[ part_index ] = node.transform.parent.TransformPoint( pos );

						// Get particle normal.
						int amount = 1;
						int sample_count = 0;
						Vector3 average_normal = Vector3.zero;
						for ( int xi = -1; xi <= 1; ++xi )
							for ( int yi = -1; yi <= 1; ++yi )
								for ( int zi = -1; zi <= 1; ++zi )
								{
									if ( xi == 0 && yi == 0 && zi == 0 )
										continue;

									float result = getVolumeData( data, volume, particle_epsilon, x + xi * amount, y + yi * amount, z + zi * amount );

									Vector3 normal = Vector3.zero;
									if ( result < -0.5f )
										normal = new Vector3( -xi, -yi, -zi );
									else if ( result > 0.5f )
										normal = new Vector3( xi, yi, zi );
									else
										continue;

									average_normal += normal;
									sample_count++;
								}
						average_normal /= sample_count;
						average_normal.Normalize();
						normals[ part_index ] = average_normal;

						// Get particle color.
						average_normal.x += 1.0f; average_normal.y += 1.0f; average_normal.z += 1.0f;
						average_normal /= 2.0f;
						colors[ part_index ] = new Color( average_normal.x, average_normal.y, average_normal.z, 1 );

						part_index++;
					}
				}

		mesh_filter.sharedMesh.vertices = vertices;
		mesh_filter.sharedMesh.colors = colors;
		mesh_filter.sharedMesh.normals = normals;
		mesh_filter.sharedMesh.uv = uvs;
		mesh_filter.sharedMesh.SetIndices( indices, MeshTopology.Points, 0 );
	}

	private void assignUnityTag()
	{
		HAPI_AttributeInfo tag_attr_info = new HAPI_AttributeInfo( HoudiniHost.prUnityTagAttribName );
		int[] tag_attr = new int[ 0 ];
		HoudiniAssetUtility.getAttribute(
			prAssetId, prObjectId, prGeoId, prPartId, HoudiniHost.prUnityTagAttribName,
			ref tag_attr_info, ref tag_attr, HoudiniHost.getAttributeStringData );

		if ( tag_attr_info.exists )
		{
			string tag = HoudiniHost.getString( tag_attr[ 0 ] );
			if ( tag != string.Empty )
			{
				try
				{
					gameObject.tag = tag;
				}
				catch
				{
					Debug.LogWarning( "Unity Tag " + tag + " is not defined!" );
				}
			}
		}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Serialized Private Data

#if !( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
#pragma warning disable 0414
#endif // ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )

	[SerializeField] private int			myPartId;
	[SerializeField] private string			myPartName;
	[SerializeField] private int			myMaterialId;
	[SerializeField] private int[]			myVertexList;
	[SerializeField] private Matrix4x4		myLastLocalToWorld;
	[SerializeField] private bool			myTransformChanged;

	[SerializeField] private bool			myShowDisplayOptions;
	[SerializeField] private bool			myShowPointNumbers;
	[SerializeField] private bool			myShowPointGroup;
	[SerializeField] private string			mySelectedPointGroup;
	[SerializeField] private bool			myShowPrimitiveGroup;
	[SerializeField] private string			mySelectedPrimitiveGroup;

	[SerializeField] private bool			myShowIntermediateResultControls;
	[SerializeField] private bool			myShowInfo;

	[SerializeField] private HoudiniGeoControl myGeoControl;

#if !( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
#pragma warning restore 0414
#endif // ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
}
