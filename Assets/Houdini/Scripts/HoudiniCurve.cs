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

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;

// Typedefs
using HAPI_NodeId = System.Int32;

[ ExecuteInEditMode ]
[ RequireComponent( typeof( HoudiniControl ) ) ]
[ RequireComponent( typeof( HoudiniParms ) ) ]
public class HoudiniCurve : MonoBehaviour
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
	
	public HoudiniControl		prControl {					get { return myControl; } 
														set { myControl = value; } }
	public HoudiniParms		prParms {					get { return myParms; }
														set { myParms = value; } }

	public List< Vector3 > 	prPoints {					get { return myPoints; } 
														set { myPoints = value; } }
	public Vector3[]		prVertices {				get { return myVertices; } 
														set { myVertices = value; } }

	public bool				prEditable {				get { return myEditable; }
														set { myEditable = value; } }
	
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
	
	public HoudiniCurve() 
	{
#if UNITY_EDITOR
		EditorApplication.playmodeStateChanged += playmodeStateChanged;
#endif // UNITY_EDITOR
		reset();
	}
	
	~HoudiniCurve()
	{
#if UNITY_EDITOR
		EditorApplication.playmodeStateChanged -= playmodeStateChanged;
#endif // UNITY_EDITOR
	}

	void OnDrawGizmos() {
#if UNITY_EDITOR
		if ( gameObject.GetComponent< MeshFilter >() == null )
			return; //throw new MissingComponentException( "Missing MeshFilter." );
		if ( gameObject.GetComponent< MeshFilter >().sharedMesh == null )
			return; //throw new HAPI_Error( "Missing sharedMesh on curve object." );
		if ( gameObject.GetComponent< MeshRenderer >() == null )
			return; //throw new MissingComponentException( "Missing MeshRenderer." );

		Mesh mesh = gameObject.GetComponent< MeshFilter >().sharedMesh;

		Color old_gizmo_color = Gizmos.color;

		if ( Selection.Contains( gameObject ) )
			Gizmos.color = HoudiniHost.prSelectedGuideWireframeColour;
		else
			Gizmos.color = HoudiniHost.prWireframeColour;

		for ( int i = 0; i < mesh.vertexCount - 1; ++i )
		{
			Vector3 from = mesh.vertices[ i ];
			Vector3 to = mesh.vertices[ i + 1 ];

			from = transform.TransformPoint( from );
			to = transform.TransformPoint( to );

			Camera tempCamera = Camera.current;
			from = tempCamera.WorldToScreenPoint( from );
			to = tempCamera.WorldToScreenPoint( to );

			int thickness = 1;
			for ( int a = -thickness; a <= thickness; ++a )
				for ( int b = -thickness; b <= thickness; ++b )
				{
					Vector3 fromf = from;
					Vector3 tof = to;
					fromf.x += a;
					fromf.y += b;
					tof.x += a;
					tof.y += b;

					fromf = tempCamera.ScreenToWorldPoint( fromf );
					tof = tempCamera.ScreenToWorldPoint( tof );
					Gizmos.DrawLine( fromf, tof );
				}
		}

		Gizmos.color = new Color( 1.0f, 0.0f, 1.0f, 0.0f );

		for ( int i = 0; i < mesh.vertexCount - 1; ++i )
		{
			Vector3 from = mesh.vertices[ i ];
			Vector3 to = mesh.vertices[ i + 1 ];

			from = transform.TransformPoint( from );
			to = transform.TransformPoint( to );

			Camera tempCamera = Camera.current;
			from = tempCamera.WorldToScreenPoint( from );
			to = tempCamera.WorldToScreenPoint( to );

			if ( from.x < 0.0f || from.x > tempCamera.pixelWidth )
				continue;
			if ( from.y < 0.0f || from.y > tempCamera.pixelHeight )
				continue;
			if ( to.x < 0.0f || to.x > tempCamera.pixelWidth )
				continue;
			if ( to.y < 0.0f || to.y > tempCamera.pixelHeight )
				continue;

			if ( from.z < 0.0f || to.z < 0.0f )
				continue;

			from.z = 0.1f;
			to.z = 0.1f;

			int thickness = 3;
			for ( int a = -thickness; a <= thickness; ++a )
				for ( int b = -thickness; b <= thickness; ++b )
				{
					Vector3 fromf = from;
					Vector3 tof = to;
					fromf.x += a;
					fromf.y += b;
					tof.x += a;
					tof.y += b;

					fromf = tempCamera.ScreenToWorldPoint( fromf );
					tof = tempCamera.ScreenToWorldPoint( tof );
					Gizmos.DrawLine( fromf, tof );
				}
		}

		Gizmos.color = old_gizmo_color;
#endif // UNITY_EDITOR
	}

	public void addPoint( Vector3 pos )
	{
#if UNITY_EDITOR
		// record undo info
		Undo.RecordObject( prParms.prParmsUndoInfo, "Add Point" );
#endif // UNITY_EDITOR

		prPoints.Add( pos );
		updatePoints();
	}

	public void insertPoint( int index, Vector3 pos )
	{
#if UNITY_EDITOR
		// record undo info
		Undo.RecordObject( prParms.prParmsUndoInfo, "Insert Point" );
#endif // UNITY_EDITOR

		prPoints.Insert( index, pos );
		updatePoints();
	}

	public void deletePoint( int index )
	{
#if UNITY_EDITOR
		// record undo info
		Undo.RecordObject( prParms.prParmsUndoInfo, "Delete Point" );
#endif // UNITY_EDITOR

		prPoints.RemoveAt( index );
		updatePoints();
	}

	public void deleteLastPoint()
	{
#if UNITY_EDITOR
		// record undo info
		Undo.RecordObject( prParms.prParmsUndoInfo, "Delete Last Point" );
#endif // UNITY_EDITOR

		prPoints.RemoveAt( prPoints.Count - 1 );
		updatePoints();
	}

	public void deletePoints( int[] indicies )
	{
#if UNITY_EDITOR
		// record undo info
		Undo.RecordObject( prParms.prParmsUndoInfo, "Delete Point(s)" );
#endif // UNITY_EDITOR

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
#if UNITY_EDITOR
		// record undo info
		Undo.RecordObject( prParms.prParmsUndoInfo, "Move Point" );
#endif // UNITY_EDITOR

		prPoints[ index ] = pos;
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

		if ( prControl.prAssetId < 0 )
			prControl.prAsset.buildAll();

		HoudiniHost.setParmStringValue( prControl.prNodeId, parm, 2, 0 );

		prControl.prAsset.buildClientSide();
		prControl.prAsset.savePreset();
		prParms.getParameterValues();
		prControl.onParmChange();
	}

	public void syncPointsWithParm()
	{
		try
		{
			// Find the parm.
			HAPI_ParmInfo coords_parm_info = prParms.findParm( "coords" );

			string point_list = 
				HoudiniHost.getString( prParms.prParmStringHandles[ coords_parm_info.stringValuesIndex ] );

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
		catch {}
	}

	public void reset()
	{
		// Please keep these in the same order and grouping as their declarations at the top.

		prControl			= null;
		prParms				= null;

		prPoints 			= new List< Vector3 >();
		prVertices 			= new Vector3[ 0 ];

		prEditable			= true;

		prCurrentMode		= Mode.ADD;
		myModeChangeWait	= false;

		myIsFirstCook		= true;
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
				line_colours[ i ] = HoudiniHost.prWireframeColour;

			mesh.Clear();
		
			mesh.vertices = prVertices;
			mesh.colors = line_colours;
			mesh.SetIndices( line_indices, MeshTopology.LineStrip, 0 );
			mesh.RecalculateBounds();
		}
	}

	public void createObject( int object_id, int geo_id )
	{
		HAPI_ObjectInfo object_info = HoudiniHost.getObjectInfo( object_id );

		try
		{
			// Get position attributes (this is all we get for the curve's geometry).
			HAPI_AttributeInfo pos_attr_info = new HAPI_AttributeInfo( HoudiniConstants.HAPI_ATTRIB_POSITION );
			float[] pos_attr = new float[ 0 ];
			HoudiniAssetUtility.getAttribute(
				geo_id, 0, HoudiniConstants.HAPI_ATTRIB_POSITION, 
				ref pos_attr_info, ref pos_attr, HoudiniHost.getAttributeFloatData );
			if ( !pos_attr_info.exists )
				throw new HoudiniError( "No position attribute found." );

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
				// This prevents curve from appearing in game (applies to entire game object).
				// Need this because the HAPI line shader doesn't work when building.
				mesh_renderer.tag = "EditorOnly"; 
		
				// Set generic texture so it's not pink.
				Material line_material = new Material( Shader.Find( "Houdini/Line" ) );
				mesh_renderer.material = line_material;
			}

			// Create guide and selection meshes.
			buildDummyMesh();

#if UNITY_EDITOR
			AssetDatabase.Refresh();
#endif // UNITY_EDITOR
		}
		catch ( HoudiniError error )
		{
			error.addMessagePrefix( "Obj(id: " + object_info.nodeId + ", name: " + object_info.name + ")" );
			error.addMessageDetail( "Object Path: " + object_info.objectInstancePath );
			throw;
		}
	}

	public void initDefaultParameters() 
	{
		// Set curve defaults.

		try
		{
			if ( myIsFirstCook )
			{
				HoudiniHost.setParmIntValue( prControl.prNodeId, "type", 0, HoudiniHost.prCurvePrimitiveTypeDefault );
				HoudiniHost.setParmIntValue( prControl.prNodeId, "method", 0, HoudiniHost.prCurveMethodDefault );
				myIsFirstCook = false;
			}
			
			HoudiniHost.cookNode(
				prControl.prAsset.prAssetId,
				prControl.prAsset.prSplitGeosByGroup,
				prControl.prAsset.prSplitPointsByVertexAttribute,
				prControl.prAsset.prImportTemplatedGeos );
		}
		catch {}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private Methods

	private void playmodeStateChanged()
	{
		// In certain situations, sepcifically after loading a scene, the gameObject "function"
		// may throw an exception here along the lines of:
		//
		// UnityEngine.MissingReferenceException: The object of type 'HoudiniAssetCurve' has been destroyed but you are still trying to access it.
		// Your script should either check if it is null or you should not destroy the object.
		//  at (wrapper managed-to-native) UnityEngine.Component:InternalGetGameObject ()
		//	  at UnityEngine.Component.get_gameObject () [0x00000] in C:\BuildAgent\work\7535de4ca26c26ac\Runtime\ExportGenerated\Editor\UnityEngineComponent.cs:170 
		//	  at HoudiniAssetCurve.playmodeStateChanged () [0x00000] in D:\Storage\Projects\Unity4EmptyProject\Assets\Houdini\Scripts\HoudiniAssetCurve.cs:369 
		//	Source: UnityEngine
		//	UnityEngine.Debug:Log(Object)
		//	HoudiniAssetCurve:playmodeStateChanged() (at Assets/Houdini/Scripts/HoudiniAssetCurve.cs:384)
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
		
#if UNITY_EDITOR
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
		catch ( System.Exception )
		{
			// There's no reason to worry the user about this useless error.
			//Debug.Log( error.ToString() + "\nSource: " + error.Source );	
		}
#endif // UNITY_EDITOR
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Serialized Private Data

	[SerializeField] private HoudiniControl		myControl;
	[SerializeField] private HoudiniParms		myParms;
	
	[SerializeField] private List< Vector3 >	myPoints;
	[SerializeField] private Vector3[]			myVertices;

	[SerializeField] private bool				myEditable;

	[SerializeField] private Mode				myCurrentMode;
	[SerializeField] private bool				myModeChangeWait;

	[SerializeField] private bool				myIsFirstCook;

}
