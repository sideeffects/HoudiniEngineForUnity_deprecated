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
	// Public Enums
	
	public enum Mode
	{
		NONE,
		ADD,
		EDIT
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	
	// Please keep these in the same order and grouping as their initializations in HAPI_Asset.reset().
	
	public List< Vector3 > 	prPoints {					get { return myPoints; } 
														set { myPoints = value; } }
	public Vector3[]		prVertices {				get { return myVertices; } 
														set { myVertices = value; } }
	
	public Mode				prCurrentMode {				get { return myCurrentMode; }
														set { myCurrentMode = value; } }
	public bool				prIsAddingPoints {			get { return ( myCurrentMode == Mode.ADD ); } 
														set { myCurrentMode = 
																( value ? Mode.ADD 
																		: ( myCurrentMode == Mode.ADD 
																			? Mode.NONE 
																			: myCurrentMode ) ); } }
	public bool				prIsEditingPoints {			get { return ( myCurrentMode == Mode.EDIT ); } 
														set { myCurrentMode = 
																( value ? Mode.EDIT 
																		: ( myCurrentMode == Mode.EDIT 
																			? Mode.NONE 
																			: myCurrentMode ) ); } }
	public bool				prModeChangeWait {			get { return myModeChangeWait; } 
														set { myModeChangeWait = value; } }
	
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

	public void insertPoint( int index, Vector3 pos )
	{
		prPoints.Insert( index, pos );
		updatePoints();
	}

	public void deletePoint( int index )
	{
		prPoints.RemoveAt( index );
		updatePoints();
	}

	public void deleteLastPoint()
	{
		prPoints.RemoveAt( prPoints.Count - 1 );
		updatePoints();
	}

	public void deletePoints( int[] indicies )
	{
		List< Vector3 > new_points = new List< Vector3 >( prPoints.Count - indicies.Length );
		List< bool > point_status = new List< bool >( prPoints.Count );
		for ( int i = 0; i < prPoints.Count; ++i )
			point_status.Add( true );
		for ( int i = 0; i < indicies.Length; ++i )
			point_status[ indicies[ i ] ] = false;
		for ( int i = 0; i < point_status.Count; ++i )
			if ( point_status[ i ] )
				new_points.Add( prPoints[ i ] );

		prPoints = new_points;

		updatePoints();
		buildDummyMesh();
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

		prCurrentMode				= Mode.ADD;
		myModeChangeWait			= false;
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
		//////////////////////////////////
		// Line Mesh

		if ( gameObject.GetComponent< MeshFilter >() == null )
			return; //throw new MissingComponentException( "Missing MeshFilter." );
		if ( gameObject.GetComponent< MeshFilter >().sharedMesh == null )
			return; //throw new HAPI_Error( "Missing sharedMesh on curve object." );
		if ( gameObject.GetComponent< MeshRenderer >() == null )
			return; //throw new MissingComponentException( "Missing MeshRenderer." );

		Mesh mesh = gameObject.GetComponent< MeshFilter >().sharedMesh;

		if ( prPoints.Count <= 1 )
			gameObject.GetComponent< MeshFilter >().sharedMesh = null;
		else
		{
			int[] line_indices = new int[ prVertices.Length ];
			for ( int i = 0; i < prVertices.Length; ++i )
				line_indices[ i ] = i;

			Color[] line_colours = new Color[ prVertices.Length ];
			for ( int i = 0; i < prVertices.Length; ++i )
				line_colours[ i ] = HAPI_Host.prWireframeColour;

			mesh.Clear();
		
			mesh.vertices = prVertices;
			mesh.colors = line_colours;
			mesh.SetIndices( line_indices, MeshTopology.LineStrip, 0 );
			mesh.RecalculateBounds();
		}
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
		
		try
		{
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

			// Set the Mesh Filter.
			if ( gameObject.GetComponent< MeshFilter >() == null )
			{
				MeshFilter mesh_filter = gameObject.AddComponent< MeshFilter >();
				mesh_filter.sharedMesh = new Mesh();
			}

			// Set the Mesh Renderer.
			if ( gameObject.GetComponent< MeshRenderer >() == null )
			{
				MeshRenderer mesh_renderer = gameObject.AddComponent< MeshRenderer >();
		
				// Set generic texture so it's not pink.
				Material line_material = new Material( Shader.Find( "HAPI/Line" ) );
				mesh_renderer.material = line_material;
			}

			// Create guide and selection meshes.
			buildDummyMesh();

			AssetDatabase.Refresh();
		}
		catch ( HAPI_Error error )
		{
			error.addMessagePrefix( "Obj(id: " + object_info.id + ", name: " + object_info.name + ")" );
			error.addMessageDetail( "Object Path: " + object_info.objectInstancePath );
			throw;
		}
	}

	private void playmodeStateChanged()
	{
		MeshRenderer renderer = gameObject.GetComponent< MeshRenderer >();
		if ( renderer != null )
			renderer.enabled = !EditorApplication.isPlaying;
		else
			Debug.LogError( "Why does your curve not have a mesh renderer?" );
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Serialized Private Data
	
	[SerializeField] private List< Vector3 >	myPoints;
	[SerializeField] private Vector3[]			myVertices;

	[SerializeField] private Mode				myCurrentMode;
	[SerializeField] private bool				myModeChangeWait;

}
