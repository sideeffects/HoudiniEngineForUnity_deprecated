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

// Typedefs
using HAPI_NodeId = System.Int32;

[ ExecuteInEditMode ]
[ RequireComponent( typeof( HAPI_Control ) ) ]
[ RequireComponent( typeof( HAPI_Parms ) ) ]
public class HAPI_Curve : MonoBehaviour
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
	
	public HAPI_Control		prControl {					get { return myControl; } 
														set { myControl = value; } }
	public HAPI_Parms		prParms {					get { return myParms; }
														set { myParms = value; } }

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
	
	public HAPI_Curve() 
	{
		EditorApplication.playmodeStateChanged += playmodeStateChanged;
		reset();
	}
	
	~HAPI_Curve()
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
		if ( prControl == null )
			return;

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
		
		HAPI_Host.setParmStringValue( prControl.prNodeId, parm, 2, 0 );
		
		prControl.prAsset.buildClientSide();
		prControl.prAsset.savePreset();
		prParms.getParameterValues();
	}

	public void syncPointsWithParm()
	{
		// Find the parm.
		int coords_parm_id = prParms.findParm( "coords" );
		if ( coords_parm_id < 0 )
			return;

		string point_list = 
			HAPI_Host.getString( prParms.prParmStringValues[ prParms.findParm( coords_parm_id ).stringValuesIndex ] );

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

	public void reset()
	{
		// Please keep these in the same order and grouping as their declarations at the top.

		prControl			= null;
		prParms				= null;

		prPoints 			= new List< Vector3 >();
		prVertices 			= new Vector3[ 0 ];

		prCurrentMode		= Mode.ADD;
		myModeChangeWait	= false;
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

	public void createObject( int object_id, int geo_id )
	{
		HAPI_ObjectInfo object_info = prControl.prAsset.prObjects[ object_id ];
		
		try
		{
			// Get position attributes (this is all we get for the curve's geometry).
			HAPI_AttributeInfo pos_attr_info = new HAPI_AttributeInfo( HAPI_Constants.HAPI_ATTRIB_POSITION );
			float[] pos_attr = new float[ 0 ];
			Utility.getAttribute( prControl.prAsset.prAssetId, object_id, geo_id, 0, HAPI_Constants.HAPI_ATTRIB_POSITION, 
								  ref pos_attr_info, ref pos_attr, HAPI_Host.getAttributeFloatData );
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

	public void initDefaultParameters() 
	{
		// Set curve defaults.
		// TODO: Make the defaults editable.
		// TODO: Make generic update parm value functions.

		int primitive_type_parm				= prParms.findParm( "type" );
		int method_parm						= prParms.findParm( "method" );
		int primitive_type_parm_default		= HAPI_Host.prCurvePrimitiveTypeDefault;
		int method_parm_default				= HAPI_Host.prCurveMethodDefault;

		int primitive_type_parm_int_values	= prParms.findParm( primitive_type_parm ).intValuesIndex;
		int method_parm_int_values			= prParms.findParm( method_parm ).intValuesIndex;

		prParms.prParmIntValues[ primitive_type_parm_int_values ]	= primitive_type_parm_default;
		prParms.prParmIntValues[ method_parm_int_values ]			= method_parm_default;

		int[] temp_int_values = new int[ 1 ];

		temp_int_values[ 0 ] = primitive_type_parm_default;
		HAPI_Host.setParmIntValues( prControl.prNodeId, temp_int_values, prParms.findParm( primitive_type_parm ).intValuesIndex, 1 );
		
		temp_int_values[ 0 ] = method_parm_default;
		HAPI_Host.setParmIntValues( prControl.prNodeId, temp_int_values, prParms.findParm( method_parm ).intValuesIndex, 1 );
		
		HAPI_Host.cookAsset( prControl.prAsset.prAssetId );
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private Methods

	private void playmodeStateChanged()
	{
		// In certain situations, sepcifically after loading a scene, the gameObject "function"
		// may throw an exception here along the lines of:
		//
		// UnityEngine.MissingReferenceException: The object of type 'HAPI_AssetCurve' has been destroyed but you are still trying to access it.
		// Your script should either check if it is null or you should not destroy the object.
		//  at (wrapper managed-to-native) UnityEngine.Component:InternalGetGameObject ()
		//	  at UnityEngine.Component.get_gameObject () [0x00000] in C:\BuildAgent\work\7535de4ca26c26ac\Runtime\ExportGenerated\Editor\UnityEngineComponent.cs:170 
		//	  at HAPI_AssetCurve.playmodeStateChanged () [0x00000] in D:\Storage\Projects\Unity4EmptyProject\Assets\HAPI\Scripts\HAPI_AssetCurve.cs:369 
		//	Source: UnityEngine
		//	UnityEngine.Debug:Log(Object)
		//	HAPI_AssetCurve:playmodeStateChanged() (at Assets/HAPI/Scripts/HAPI_AssetCurve.cs:384)
		//	UnityEditor.Toolbar:OnGUI()
		//
		// This is not good since we don't know why the curve does not yet have an initialization.
		//
		// This is 100% reproducible like so:
		//		1. Create a simple Houdini curve (2-3 points).
		//		2. Instatiate a simple asset that takes a curve as an input (like the metaballworm asset).
		//		3. Save the Unity scene.
		//		4. Go to play mode and exit play mode.
		//		5. Reload the saved Unity scene whithin the same session of Unity.
		//		6. Go to play mode.
		// You should see a message (white speech bubble) about UnityEngine.MissingReferenceException. This is
		// triggered somewhere in this function. It's harmless but it was the cause of the stalled state
		// change callback queue calls. 
		//
		// For now we need to catch this exception because if we let it out it will stall
		// the entire callback chain bound to EditorApplication.playmodeStateChanged which
		// causes other bound functions in this callback list to never be called, leading to
		// bug: #56253.
		
		try
		{	
			if ( gameObject != null )
			{
				MeshRenderer renderer = gameObject.GetComponent< MeshRenderer >();
				if ( renderer != null )
					renderer.enabled = !EditorApplication.isPlaying;
				else
					Debug.LogError( "Why does your curve not have a mesh renderer?" );
			}
			else
			{
				Debug.Log( 
					"Why is this curve without a gameObject?\nName: " + 
					prControl.prAsset.prAssetName + "\nId: " + prControl.prAsset.prAssetId );
			}
		}
		catch ( System.Exception error )
		{
			Debug.Log( error.ToString() + "\nSource: " + error.Source );	
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Serialized Private Data

	[SerializeField] private HAPI_Control		myControl;
	[SerializeField] private HAPI_Parms			myParms;
	
	[SerializeField] private List< Vector3 >	myPoints;
	[SerializeField] private Vector3[]			myVertices;

	[SerializeField] private Mode				myCurrentMode;
	[SerializeField] private bool				myModeChangeWait;

}
