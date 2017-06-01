/*
* Copyright (c) <2017> Side Effects Software Inc.
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*
* Produced by:
*      Side Effects Software Inc
*      123 Front Street West, Suite 1401
*      Toronto, Ontario
*      Canada   M5J 2M2
*      416-504-9876
*
*/

// Master control for enabling runtime.
#if ( UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_LINUX )
	#define HAPI_ENABLE_RUNTIME
#endif

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

	public virtual void OnDestroy()
	{
		destoryChildrenWithComponent< BoxCollider >();
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

		// Clean up first.
		destoryChildrenWithComponent< BoxCollider >();
		destoryChildrenWithComponent< SphereCollider >();

		GameObject part_node = gameObject;

		// Get Part info.
		HAPI_PartInfo part_info = HoudiniHost.getPartInfo( prGeoId, prPartId );
		myPartType = part_info.type;

		bool is_empty = part_info.vertexCount <= 0 && part_info.pointCount <= 0;
		bool is_mesh = ( part_info.vertexCount > 0 );

		bool is_collision_geo = ( part_info.name.Contains( HoudiniHost.prCollisionGroupName ) );
		bool is_rendered_collision_geo = ( part_info.name.Contains( HoudiniHost.prRenderedCollisionGroupName ) );
		if ( is_rendered_collision_geo )
			is_collision_geo = false;

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
			var part_name = part_info.name;
			if ( prAsset.prOmitPartNameEnumeration )
				part_name = part_name.Substring( 0, part_name.LastIndexOf( '_' ) );
			part_node.name = part_name;

			if ( is_empty )
			{
				// Add required components.
				MeshFilter mesh_filter = getOrCreateComponent< MeshFilter >();

				// Get or create mesh.
				Mesh part_mesh = mesh_filter.sharedMesh;
				if ( part_mesh == null )
				{
					mesh_filter.mesh = new Mesh();
					part_mesh = mesh_filter.sharedMesh;
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
				bool has_visible_geometry = false;
				try
				{
					has_visible_geometry = HoudiniAssetUtility.getMesh(
						this, part_mesh,
						prAsset.prGenerateUVs,
						prAsset.prGenerateLightmapUV2s,
						prAsset.prGenerateTangents,
						prAsset.prSplitPointsByVertexAttribute );
				}
				catch ( HoudiniErrorIgnorable ) {}
				catch ( HoudiniError error )
				{
					Debug.LogWarning( error.ToString() );
					return;
				}

				// Add collider if group name matches. (Should be added after the mesh is set so that it
				// picks up the mesh automagically)
				if ( prAsset.prSplitGeosByGroup )
				{
					if ( is_rendered_collision_geo || is_collision_geo )
					{
						getOrCreateComponent<MeshRenderer>();

						// For simple prim collider (Box / Sphere), recreating the Box/Sphere collider
						// will use the mesh renderer to position/size it properly
						// Unfortunately for now, the returned part infos can have transform issue for box/sphere prim

						// Create the box collider if one exists.
						if ( myPartType == HAPI_PartType.HAPI_PARTTYPE_BOX )
						{
							//createBoxCollider( part_info );
							removeComponent<BoxCollider>();
							BoxCollider mesh_collider = getOrCreateComponent<BoxCollider>();
							mesh_collider.enabled = false;
							mesh_collider.enabled = true;
						}
						else if ( myPartType == HAPI_PartType.HAPI_PARTTYPE_SPHERE )
						{
							//createSphereCollider( part_info );
							removeComponent<SphereCollider>();
							SphereCollider mesh_collider = getOrCreateComponent<SphereCollider>();
							mesh_collider.enabled = false;
							mesh_collider.enabled = true;
						}
						else
						{
							MeshCollider mesh_collider = getOrCreateComponent< MeshCollider >();
							mesh_collider.enabled = false;
							mesh_collider.enabled = true;
						}

						if ( is_collision_geo )
						{
							// We're not a rendered collision geo so we dont need a renderer
							removeComponent<MeshRenderer>();
						}
					}
					else
					{
						getOrCreateComponent< MeshRenderer >();

						// We're not a collision geo so we dont need a collider
						removeComponent< MeshCollider >();
						removeComponent< BoxCollider >();
						removeComponent< SphereCollider >();
					}
				}
				else if ( !prAsset.prSplitGeosByGroup && has_visible_geometry )
				{
					getOrCreateComponent< MeshRenderer >();
				}

				// TODO: Intermediate mesh editing currently not supported when not splitting by group.
				if ( prAsset.prSplitGeosByGroup )
				{
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
				}
			}

			/*
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
			*/
		}

		// Refresh enabled flags.
		{
			bool is_visible = prObjectVisible;
			is_visible &= ( prAsset.prIsGeoVisible || prGeoType == HAPI_GeoType.HAPI_GEOTYPE_INTERMEDIATE );
			if ( prGeoType == HAPI_GeoType.HAPI_GEOTYPE_INTERMEDIATE && myGeoControl.prGeoAttributeManager != null )
				is_visible &= myGeoControl.prGeoAttributeManager.prCurrentMode != HoudiniGeoAttributeManager.Mode.NONE;

			// Do we need to enable the colliders?
			bool enable_colliders = is_visible && ( is_collision_geo || is_rendered_collision_geo );

			if ( gameObject.GetComponent<MeshCollider>() )
				gameObject.GetComponent<MeshCollider>().enabled = enable_colliders;
			if ( gameObject.GetComponent<BoxCollider>() )
				gameObject.GetComponent<BoxCollider>().enabled = enable_colliders;
			if ( gameObject.GetComponent<SphereCollider>() )
				gameObject.GetComponent<SphereCollider>().enabled = enable_colliders;

			// Do we need to enable the mesh renderer ?
			bool enable_renderers = is_visible && !is_collision_geo;
			if ( gameObject.GetComponent< MeshRenderer >() )
				gameObject.GetComponent< MeshRenderer >().enabled = enable_renderers;
		}

		// Assign materials.
		HoudiniAssetUtility.assignMaterial( this, prAsset, reload_asset );

		// Assign unity tag.
		assignUnityTag();
	}

	public void createBoxCollider( HAPI_PartInfo part_info )
	{
		GameObject box_collider_obj =
			new GameObject( part_info.name + "_box_collider_" + prGeoControl.prNodeId );
		box_collider_obj.transform.SetParent( gameObject.transform );
		box_collider_obj.isStatic = gameObject.isStatic;

		// Need to reset position here because the assignment above will massage the child's
		// position in order to be in the same place it was in the global namespace.
		box_collider_obj.transform.localPosition = new Vector3();
		box_collider_obj.transform.localRotation = new Quaternion();
		box_collider_obj.transform.localScale = new Vector3( 1.0f, 1.0f, 1.0f );

		box_collider_obj.AddComponent< BoxCollider >();

		HAPI_BoxInfo box_info = HoudiniHost.getBoxInfo( prGeoControl.prNodeId, part_info.id );
		BoxCollider box_collider = box_collider_obj.GetComponent< BoxCollider >();
		box_collider.center = new Vector3(
			-box_info.center[ 0 ],
			box_info.center[ 1 ],
			box_info.center[ 2 ] );
		box_collider.size = new Vector3(
			box_info.size[ 0 ] * 2,
			box_info.size[ 1 ] * 2,
			box_info.size[ 2 ] * 2 );

		box_collider.transform.rotation = Quaternion.Euler(
			box_info.rotation[ 2 ],
			-box_info.rotation[ 1 ],
			-box_info.rotation[ 0 ] );

		box_collider.enabled = false;
		box_collider.enabled = true;

		removeComponent< MeshCollider >();
	}

	public void createSphereCollider( HAPI_PartInfo part_info )
	{
		GameObject sphere_collider_obj =
			new GameObject( part_info.name + "_sphere_collider_" + prGeoControl.prNodeId );
		sphere_collider_obj.transform.SetParent( gameObject.transform );
		sphere_collider_obj.isStatic = gameObject.isStatic;

		// Need to reset position here because the assignment above will massage the child's
		// position in order to be in the same place it was in the global namespace.
		sphere_collider_obj.transform.localPosition = new Vector3();
		sphere_collider_obj.transform.localRotation = new Quaternion();
		sphere_collider_obj.transform.localScale = new Vector3( 1.0f, 1.0f, 1.0f );

		sphere_collider_obj.AddComponent< SphereCollider >();

		HAPI_SphereInfo sphere_info = HoudiniHost.getSphereInfo( prGeoControl.prNodeId, part_info.id );
		SphereCollider sphere_collider = sphere_collider_obj.GetComponent< SphereCollider >();
		sphere_collider.center = new Vector3(
			-sphere_info.center[ 0 ],
			sphere_info.center[ 1 ],
			sphere_info.center[ 2 ] );
		sphere_collider.radius = sphere_info.radius;

		sphere_collider.enabled = false;
		sphere_collider.enabled = true;

		removeComponent< MeshCollider >();
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

#if ( HAPI_ENABLE_RUNTIME )
	public virtual void Update()
	{
		Matrix4x4 local_to_world = transform.localToWorldMatrix;
		
		if ( local_to_world == myLastLocalToWorld )
			return;

		myLastLocalToWorld = local_to_world;
		myTransformChanged = true;
	}
#endif // ( HAPI_ENABLE_RUNTIME )

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private

	private void assignUnityTag()
	{
		HAPI_AttributeInfo tag_attr_info = new HAPI_AttributeInfo( HoudiniHost.prUnityTagAttribName );
		int[] tag_attr = new int[ 0 ];
		HoudiniAssetUtility.getAttribute(
			prGeoId, prPartId, HoudiniHost.prUnityTagAttribName,
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

#if !( HAPI_ENABLE_RUNTIME )
#pragma warning disable 0414
#endif // ( HAPI_ENABLE_RUNTIME )

	[SerializeField] private int			myPartId;
	[SerializeField] private string			myPartName;
	[SerializeField] private HAPI_PartType	myPartType;
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

#if !( HAPI_ENABLE_RUNTIME )
#pragma warning restore 0414
#endif // ( HAPI_ENABLE_RUNTIME )
}
