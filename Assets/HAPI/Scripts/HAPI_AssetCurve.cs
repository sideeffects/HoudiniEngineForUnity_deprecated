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
public class HAPI_AssetCurve : HAPI_Asset
{	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	
	// Please keep these in the same order and grouping as their initializations in HAPI_Asset.reset().
	
	public List< Vector3 > 	prPoints {				get { return myPoints; } set { myPoints = value; } }
	public Vector3[]		prVertices {			get { return myVertices; } set { myVertices = value; } }
	public GameObject		prMainChild {			get { return myMainChild; } set { myMainChild = value; } }

	public bool				prIsAddingPoints {		get { return myIsAddingPoints; } 
													set { myIsAddingPoints = value; } }
	public bool				prEditModeChangeWait {	get { return myEditModeChangeWait; } 
													set { myEditModeChangeWait = value; } }
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods
	
	public HAPI_AssetCurve() 
	{
		if ( prEnableLogging )
			Debug.Log( "HAPI_Asset created!" );
		
		EditorApplication.playmodeStateChanged += playmodeStateChanged;

		reset();
	}
	
	~HAPI_AssetCurve()
	{
		EditorApplication.playmodeStateChanged -= playmodeStateChanged;
	}
	
	public void addPoint( Vector3 pos )
	{
		prPoints.Add( pos );
		
		updatePoints();
	}
	
	public void updatePoint( int index, Vector3 pos )
	{
		prPoints[ index ] = pos;
		
		updatePoints();
	}
	
	public void updatePoints()
	{
		string parm = "";
		for ( int i = 0; i < prPoints.Count; ++i )
		{
			parm += -prPoints[ i ][ 0 ];
			parm += ",";
			parm += prPoints[ i ][ 1 ];
			parm += ",";
			parm += prPoints[ i ][ 2 ];
			parm += " ";
		}
		
		HAPI_Host.setParmStringValue( prAssetId, parm, 2, 0 );
		
		build();

		savePreset();
	}

	public void syncPointsWithParm()
	{
		// Find the parm.
		int coords_parm_id = findParm( "coords" );
		if ( coords_parm_id < 0 )
			return;

		string point_list = 
			HAPI_Host.getString( prParmStringValues[ prParms[ coords_parm_id ].stringValuesIndex ] );

		if ( point_list == null )
			return;

		// Clear all existing points.
		prPoints.Clear();

		// Parse parm value for the points.
		string [] point_split = point_list.Split( new char [] { ' ' } );
		for ( int i = 0; i < point_split.Length; ++i )
		{
			string vec_str = point_split[ i ];
			string [] vec_split = vec_str.Split( new char [] { ',' } );

			if ( vec_split.Length == 3 )
			{
				Vector3 vec = new Vector3();

				vec.x = (float) -System.Convert.ToDouble( vec_split[ 0 ] );
				vec.y = (float)  System.Convert.ToDouble( vec_split[ 1 ] );
				vec.z = (float)  System.Convert.ToDouble( vec_split[ 2 ] );

				prPoints.Add( vec );
			}
		}
	}

	public override void reset()
	{
		base.reset();
		
		// Overwrite some settings that should be different by default for curves than other asset types.
		prAutoSelectAssetNode		= true;
		prHideGeometryOnLinking		= false;
		prAssetType					= AssetType.TYPE_CURVE;

		// Please keep these in the same order and grouping as their declarations at the top.
		
		prPoints 					= new List< Vector3 >();
		prVertices 					= new Vector3[ 0 ];
		prMainChild					= null;

		myIsAddingPoints			= true;
		myEditModeChangeWait		= false;
	}
	
	public override bool build() 
	{
		bool unload_asset_first = prReloadAssetInFullBuild && !prPartialBuild;

		bool base_built = base.build( unload_asset_first );
		if ( !base_built )
			return false;
		
		return true;
	}

	public void buildDummyMesh()
	{
		if ( prMainChild == null )
			return;

		int vertex_count = prVertices.Length;

		// Set mesh counts.
		int mesh_vertex_count	= vertex_count * 2;
		int mesh_uv_count		= mesh_vertex_count;
		int mesh_triangle_count = ( vertex_count - 1 ) * 4; // Two triangles per side (double-sided).

		// Create mesh arrays.
		Vector3[] vertices 	= new Vector3[ 	mesh_vertex_count ];
		Vector2[] uvs		= new Vector2[  mesh_uv_count ];
		int[] triangles 	= new int[ 		mesh_triangle_count * 3 ];

		for ( int i = 0; i < vertex_count; ++i )
		{
			vertices[ i * 2 ] = prVertices[ i ];
			vertices[ i * 2 + 1 ] = prVertices[ i ];
			vertices[ i * 2 + 1 ].y += HandleUtility.GetHandleSize( prVertices[ i ] ) * myDummyLineWidthMod;

			uvs[ i * 2 + 0 ][ 0 ] = 0.0f;
			uvs[ i * 2 + 1 ][ 1 ] = 0.0f;

			if ( i < vertex_count - 1 )
			{
				// Side one.
					
				// Triangle 1.
				triangles[ i * 12 + 0  ] = i * 2 + 0;
				triangles[ i * 12 + 1  ] = i * 2 + 1;
				triangles[ i * 12 + 2  ] = i * 2 + 2;

				// Triangle 2.
				triangles[ i * 12 + 3  ] = i * 2 + 1;
				triangles[ i * 12 + 4  ] = i * 2 + 3;
				triangles[ i * 12 + 5  ] = i * 2 + 2;

				// Side two.

				// Triangle 3.
				triangles[ i * 12 + 6  ] = i * 2 + 2;
				triangles[ i * 12 + 7  ] = i * 2 + 1;
				triangles[ i * 12 + 8  ] = i * 2 + 0;

				// Triangle 4.
				triangles[ i * 12 + 9  ] = i * 2 + 2;
				triangles[ i * 12 + 10 ] = i * 2 + 3;
				triangles[ i * 12 + 11 ] = i * 2 + 1;
			}
		}

		// Load into vertices and face into mesh.
		// Get or create mesh.
		MeshFilter main_child_mesh_filter 	= prMainChild.GetComponent< MeshFilter >();
		Mesh main_child_mesh 				= main_child_mesh_filter.sharedMesh;
		if ( main_child_mesh == null ) 
		{
			main_child_mesh_filter.mesh 	= new Mesh();
			main_child_mesh 				= main_child_mesh_filter.sharedMesh;
		}
		main_child_mesh.Clear();
		
		main_child_mesh.vertices	= vertices;
		main_child_mesh.uv			= uvs;
		main_child_mesh.triangles	= triangles;

		main_child_mesh.RecalculateBounds();
		main_child_mesh.RecalculateNormals();
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Protected Methods

	protected override int buildCreateAsset()
	{
		return HAPI_Host.createCurve();
	}

	protected override void buildFullBuildCustomWork( ref HAPI_ProgressBar progress_bar )
	{
		// Set curve defaults.
		// TODO: Make the defaults editable.
		// TODO: Make generic update parm value functions.

		int primitive_type_parm				= findParm( "type" );
		int method_parm						= findParm( "method" );
		int primitive_type_parm_default		= HAPI_Host.prCurvePrimitiveTypeDefault;
		int method_parm_default				= HAPI_Host.prCurveMethodDefault;

		int primitive_type_parm_int_values	= prParms[ primitive_type_parm ].intValuesIndex;
		int method_parm_int_values			= prParms[ method_parm ].intValuesIndex;

		prParmIntValues[ primitive_type_parm_int_values ]	= primitive_type_parm_default;
		prParmIntValues[ method_parm_int_values ]			= method_parm_default;

		int[] temp_int_values = new int[ 1 ];

		temp_int_values[ 0 ] = primitive_type_parm_default;
		HAPI_Host.setParmIntValues( prAssetId, temp_int_values, prParms[ primitive_type_parm ].intValuesIndex, 1 );
		
		temp_int_values[ 0 ] = method_parm_default;
		HAPI_Host.setParmIntValues( prAssetId, temp_int_values, prParms[ method_parm ].intValuesIndex, 1 );
		
		HAPI_Host.cookAsset( prAssetId );
	}

	protected override void buildCreateObjects( ref HAPI_ProgressBar progress_bar )
	{
		try
		{
			createObject( 0 );
		}
		catch ( HAPI_Error )
		{
			// Per-object errors are not re-thrown so that the rest of the asset has a chance to load.
			//Debug.LogWarning( error.ToString() );
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private Methods
	
	private void createObject( int object_id )
	{
		HAPI_ObjectInfo object_info = prObjects[ object_id ];
		
		// Create main underling.
		if ( prMainChild == null )
		{
			prMainChild = new GameObject( object_info.name + "_obj" );
			prMainChild.transform.parent = transform;

			// Add required components.
			prMainChild.AddComponent< MeshFilter >();
			prMainChild.AddComponent< MeshRenderer >();
			prMainChild.AddComponent< HAPI_PartControl >();
		}
		
		try
		{
			// Set Object Control on child selection control so it can read settings from here.
			prMainChild.GetComponent< HAPI_PartControl >().prAsset		= this;
			prMainChild.GetComponent< HAPI_PartControl >().prObjectId	= object_id;

			// Set generic texture so it's not pink.
			Material decal = new Material( Shader.Find( "Decal" ) );
			prMainChild.GetComponent< MeshRenderer >().material = decal;
			decal.SetColor( "_Color", new Color( 0.0f, 0.0f, 0.0f ) );

			// Get or create mesh.
			MeshFilter main_child_mesh_filter 	= prMainChild.GetComponent< MeshFilter >();
			Mesh main_child_mesh 				= main_child_mesh_filter.sharedMesh;
			if ( main_child_mesh == null ) 
			{
				main_child_mesh_filter.mesh 	= new Mesh();
				main_child_mesh 				= main_child_mesh_filter.sharedMesh;
			}
			main_child_mesh.Clear();
			
			// Print attribute names.
			//printAllAttributeNames( asset_id, object_id, geo_id, geo_info );
			
			// Get position attributes (this is all we get for the curve's geometry).
			HAPI_AttributeInfo pos_attr_info = new HAPI_AttributeInfo( "P" );
			float[] pos_attr = new float[ 0 ];
			Utility.getAttribute( prAssetId, object_id, 0, 0, "P", ref pos_attr_info, ref pos_attr, 
								  HAPI_Host.getAttributeFloatData );
			if ( !pos_attr_info.exists )
				throw new HAPI_Error( "No position attribute found." );

			int vertex_count = pos_attr_info.count;
			
			// Add vertices to the vertices array for guides.
			prVertices = new Vector3[ vertex_count ];
			for ( int i = 0; i < vertex_count; ++i )
			{
				for ( int j = 0; j < 3; ++j )
					prVertices[ i ][ j ] = pos_attr[ i * 3 + j ];
				prVertices[ i ].x = -prVertices[ i ].x;
			}
			
			// Get transforms.
			HAPI_Transform trans = prObjectTransforms[ object_id ];
			
			// Apply object transforms.		
			//
			// Axis and Rotation conversions:
			// Note that Houdini's X axis points in the opposite direction that Unity's does.  Also, Houdini's 
			// rotation is right handed, whereas Unity is left handed.  To account for this, we need to invert
			// the x coordinate of the translation, and do the same for the rotations (except for the x rotation,
			// which doesn't need to be flipped because the change in handedness AND direction of the left x axis
			// causes a double negative - yeah, I know).
			
			prMainChild.transform.localPosition = new Vector3( -trans.position[ 0 ], 
																trans.position[ 1 ],
																trans.position[ 2 ] );
			
			Quaternion quat = new Quaternion(	trans.rotationQuaternion[ 0 ],
												trans.rotationQuaternion[ 1 ],
												trans.rotationQuaternion[ 2 ],
												trans.rotationQuaternion[ 3 ] );
			
			Vector3 euler = quat.eulerAngles;
			euler.y = -euler.y;
			euler.z = -euler.z;
			
			prMainChild.transform.localRotation = Quaternion.Euler( euler );
			prMainChild.transform.localScale = new Vector3( trans.scale[ 0 ], trans.scale[ 1 ], trans.scale[ 2 ] );

			// Create guide mesh (for when the asset is not selected).
			buildDummyMesh();

			AssetDatabase.Refresh();
		}
		catch ( HAPI_Error error )
		{
			DestroyImmediate( prMainChild );
			error.addMessagePrefix( "Obj(id: " + object_info.id + ", name: " + object_info.name + ")" );
			error.addMessageDetail( "Object Path: " + object_info.objectInstancePath );
			throw;
		}
	}

	private void playmodeStateChanged()
	{
		if ( prMainChild != null )
		{
			MeshRenderer renderer = prMainChild.GetComponent< MeshRenderer >();
			if ( renderer != null )
				renderer.enabled = !EditorApplication.isPlaying;
			else
				Debug.LogError( "Why does your curve not have a mesh renderer?" );
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Serialized Data
	
	[SerializeField] private List< Vector3 >	myPoints;
	[SerializeField] private Vector3[]			myVertices;
	[SerializeField] private GameObject			myMainChild;

	[SerializeField] private bool				myIsAddingPoints;
	[SerializeField] private bool				myEditModeChangeWait;

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private Data

	private static float myDummyLineWidthMod = 0.05f;
}
