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
using HAPI;

// Typedefs
using HAPI_NodeId = System.Int32;

[ ExecuteInEditMode ]
public class HAPI_PartControl : HAPI_GeoControl 
{	
#if UNITY_STANDALONE_WIN
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	public int			prPartId {		get { return myPartId; }		set { myPartId = value; } }
	public string		prPartName {	get { return myPartName; }		set { myPartName = value; } }
	public int			prMaterialId {	get { return myMaterialId; }	set { myMaterialId = value; } }
	public int[]		prVertexList {	get { return myVertexList; }	set { myVertexList = value; } }
	public bool			prShowPointNumbers { get { return myShowPointNumbers; }	set { myShowPointNumbers = value; } }

	public bool 		prTransformChanged { get { return myTransformChanged; } set { myTransformChanged = value; } }
	
	public HAPI_GeoControl prGeoControl { get { return myGeoControl; } set { myGeoControl = value; } }

	public HAPI_PartControl()
	{
		reset();
	}

	~HAPI_PartControl()
	{

	}
	
	public override void reset()
	{
		base.reset();

		// Please keep these in the same order and grouping as their declarations at the top.
		
		prPartId 		= -1;
		prPartName		= "part_name";
		prMaterialId 	= -1;
		prVertexList 	= new int[ 1 ];
		myTransformChanged = false;
		myShowPointNumbers = false;

		myGeoControl = null;
	}

	public void Awake()
	{
		myLastLocalToWorld = transform.localToWorldMatrix;
	}

	public void selectParent()
	{
#if UNITY_EDITOR
		if ( prAsset != null && prAsset.prAutoSelectAssetRootNode && HAPI_Host.prAutoSelectAssetRootNode )
		{
			GameObject[] selection 	= new GameObject[ 1 ];
			selection[ 0 ] 			= prAsset.gameObject;
			Selection.objects 		= selection;
		}
#endif // UNITY_EDITOR
	}

	public void init( HAPI_PartControl part_control )
	{
		init( (HAPI_GeoControl) part_control );

		prAsset			= part_control.prAsset;
		prPartId		= part_control.prPartId;
		prPartName		= part_control.prPartName;
		prMaterialId	= part_control.prMaterialId;
	}

	public void init( HAPI_NodeId node_id, int part_id, string part_name, int material_id )
	{
		prNodeId		= node_id;
		prPartId		= part_id;
		prPartName		= part_name;
		prMaterialId	= material_id;
	}

	public override string getFullControlNameAndPath()
	{
		return base.getFullControlNameAndPath() + "/" + prPartName;
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
		HAPI_Host.getPartInfo( prAssetId, prObjectId, prGeoId, prPartId, out part_info );

		bool is_mesh = ( part_info.vertexCount > 0 );

		// TODO: Make this info a permanent UI display.
		//if ( prEnableLogging && ( reload_asset || has_geo_changed || has_material_changed ) )
		//	Debug.Log( "Obj #" + part_control.prObjectId + " (" + part_control.prObjectName + "): "
		//			   + "verts: " + part_info.vertexCount + " faces: " + part_info.faceCount );

		if ( reload_asset || has_geo_changed )
		{
			// Initialize our part control.
			init( -1, part_info.id, part_info.name, part_info.materialId );

			// Overwrite name.
			part_node.name = part_info.name;

			if ( is_mesh ) // Valid mesh.
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
		
				// Get mesh.
				try
				{
					HAPI_AssetUtility.getMesh( 
						this, part_mesh, prAsset.prGenerateTangents && HAPI_Host.prGenerateTangents );
				}
				catch ( HAPI_ErrorIgnorable ) {}
				catch ( HAPI_Error error )
				{
					Debug.LogWarning( error.ToString() );
					return;
				}

				// Add collider if group name matches. (Should be added after the mesh is set so that it
				// picks up the mesh automagically)
				if ( part_info.name.Contains( HAPI_Host.prRenderedCollisionGroupName ) )
				{
					MeshCollider mesh_collider = getOrCreateComponent< MeshCollider >();
					getOrCreateComponent< MeshRenderer >();
					mesh_collider.enabled = false;
					mesh_collider.enabled = true;
				}
				else if ( part_info.name.Contains( HAPI_Host.prCollisionGroupName ) )
				{
					MeshCollider mesh_collider = getOrCreateComponent< MeshCollider >();
					mesh_collider.enabled = false;
					mesh_collider.enabled = true;
				}
				else
					getOrCreateComponent< MeshRenderer >();

				// Add Mesh-to-Prefab component.
				HAPI_MeshToPrefab mesh_saver = getOrCreateComponent< HAPI_MeshToPrefab >();
				mesh_saver.prGameObject = part_node;
				mesh_saver.prMeshName = prAsset.prAssetName + "_" + part_node.name;
			}
			else if ( HAPI_Host.prEnablePointsAsParticles && part_info.vertexCount <= 0 && part_info.pointCount > 0 ) // Particles?
			{
				// Get position attributes.
				HAPI_AttributeInfo pos_attr_info = new HAPI_AttributeInfo( HAPI_Constants.HAPI_ATTRIB_POSITION );
				float[] pos_attr = new float[ 0 ];
				HAPI_AssetUtility.getAttribute(
					prAssetId, prObjectId, prGeoId, prPartId, HAPI_Constants.HAPI_ATTRIB_POSITION, 
					ref pos_attr_info, ref pos_attr, HAPI_Host.getAttributeFloatData );
				if ( !pos_attr_info.exists )
					throw new HAPI_Error( "No position attribute found." );
				else if ( pos_attr_info.owner != HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
					throw new HAPI_ErrorIgnorable( "I only understand position as point attributes!" );

				// Get colour attributes.
				HAPI_AttributeInfo colour_attr_info = new HAPI_AttributeInfo( HAPI_Constants.HAPI_ATTRIB_COLOR );
				float[] colour_attr = new float[ 0 ];
				HAPI_AssetUtility.getAttribute( 
					prAssetId, prObjectId, prGeoId, prPartId, HAPI_Constants.HAPI_ATTRIB_COLOR,
					ref colour_attr_info, ref colour_attr, HAPI_Host.getAttributeFloatData );

				ParticleEmitter particle_emitter = part_node.GetComponent< ParticleEmitter >();
				if ( particle_emitter == null )
					particle_emitter = part_node.AddComponent( "EllipsoidParticleEmitter" ) as ParticleEmitter;
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
					if ( colour_attr_info.exists )
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
			
			if ( part_info.hasVolume )
			{
				// If we have a volume, retrieve the volume info
				HAPI_VolumeInfo volume = new HAPI_VolumeInfo();
				HAPI_Host.getVolumeInfo( prAssetId, prObjectId, prGeoId, prPartId, ref volume );

				// Iterate through the voxels and print out the data, for now.
				HAPI_VolumeTileInfo tile = new HAPI_VolumeTileInfo();
				HAPI_Host.getFirstVolumeTile( prAssetId, prObjectId, prGeoId, prPartId, ref tile );
				float[] values = new float[ volume.tileSize * volume.tileSize * volume.tileSize ];
				int tile_num = 0;
				
				HAPI_AssetUtility.applyTransform( volume.transform, part_node.transform );
				
				// Clear previous volume tiles.
				destroyChildren( part_node.transform );
				
				while ( tile.isValid )
				{
					for ( int i = 0; i < values.Length; ++i )
						values[ i ] = 0;
					HAPI_Host.getVolumeTileFloatData( 
						prAssetId, prObjectId, prGeoId, prPartId, ref tile, values );

					tile_num += 1;
					GameObject tile_node = new GameObject( "tile (" + tile.minX +
														   ", " + tile.minY +
														   ", " + tile.minZ + ")" );
					tile_node.transform.parent = part_node.transform;

					createFogVolume( tile_node, values, tile, volume );

					HAPI_Host.getNextVolumeTile( prAssetId, prObjectId, prGeoId, prPartId, ref tile );
				}
			}
		}

		// Refresh enabled flags.
		if ( gameObject.GetComponent< MeshCollider >() )
			gameObject.GetComponent< MeshCollider >().enabled = 
				prObjectVisible && 
					( prAsset.prIsGeoVisible || prGeoType == HAPI_GeoType.HAPI_GEOTYPE_INTERMEDIATE );
		if ( gameObject.GetComponent< MeshRenderer >() )
			gameObject.GetComponent< MeshRenderer >().enabled = 
				prObjectVisible && 
					( prAsset.prIsGeoVisible || prGeoType == HAPI_GeoType.HAPI_GEOTYPE_INTERMEDIATE );

		// Assign materials.
		HAPI_AssetUtility.assignMaterial( this, prAsset, ( reload_asset || has_material_changed ) );

		// Assign unity tag.
		assignUnityTag();
	}
	
	public virtual void Update()
	{
		
		Matrix4x4 local_to_world = transform.localToWorldMatrix;
		
		if ( local_to_world == myLastLocalToWorld )
			return;
						
		myLastLocalToWorld = local_to_world;
		myTransformChanged = true;
	}

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
						  Vector3 pos = new Vector3( (float)x, (float)y, (float)z );
						  pos = 2 * ( ( pos + tileMin ) - new Vector3( 0.5f, 0.5f, 0.5f ) );

						  particles[ part_index ].position =
							  node.transform.parent.TransformPoint( pos );
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

	private void assignUnityTag()
	{
		HAPI_AttributeInfo tag_attr_info = new HAPI_AttributeInfo( HAPI_Host.prUnityTagAttribName );
		int[] tag_attr = new int[ 0 ];
		HAPI_AssetUtility.getAttribute( prAssetId, prObjectId, prGeoId, prPartId, HAPI_Host.prUnityTagAttribName, 
		             					ref tag_attr_info, ref tag_attr, HAPI_Host.getAttributeStrData );

		if ( tag_attr_info.exists )
		{
			string tag = HAPI_Host.getString( tag_attr[ 0 ] );
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

	[SerializeField] private int			myPartId;
	[SerializeField] private string			myPartName;
	[SerializeField] private int			myMaterialId;
	[SerializeField] private int[]			myVertexList;
	[SerializeField] private Matrix4x4		myLastLocalToWorld;
	[SerializeField] private bool			myTransformChanged;
	[SerializeField] private bool			myShowPointNumbers;

	[SerializeField] private HAPI_GeoControl myGeoControl;
#endif // UNITY_STANDALONE_WIN
}
