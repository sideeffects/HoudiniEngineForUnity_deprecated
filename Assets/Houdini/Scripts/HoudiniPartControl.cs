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

// Master control for enabling runtime.
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_STANDALONE_LINUX || ( UNITY_METRO && UNITY_EDITOR ) )
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

#if !( HAPI_ENABLE_RUNTIME )
#pragma warning disable 0414
#endif // ( HAPI_ENABLE_RUNTIME )

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

#if !( HAPI_ENABLE_RUNTIME )
#pragma warning restore 0414
#endif // ( HAPI_ENABLE_RUNTIME )
}
