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
using System.Collections;
using System.Collections.Generic;

public class HAPI_GeoAttributeManager : ScriptableObject {
#if UNITY_STANDALONE_WIN
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Enums
	
	public enum Mode
	{
		NONE,
		PAINT,
		EDIT
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties

	public bool			prEditable {					get { return true; }
														private set {} }
	public bool			prHasChanged {					get { return myHasChanged; }
														set { myHasChanged = value; } }
	public bool			prLiveUpdates {					get { return myLiveUpdates; }
														set { myLiveUpdates = value; } }

	public Mesh			prEditableMesh {				get { return myMesh; }
														private set {} }
	public MeshCollider prMeshCollider {				get { return myMeshCollider; }
														private set {} }
	public Transform	prTransform {					get { return myTransform; }
														private set {} }

	public Mode			prCurrentMode {					get { return myCurrentMode; }
														set { changeMode( value ); } }
	public bool			prIsPaintingPoints {			get { return ( myCurrentMode == Mode.PAINT ); } 
														set { changeMode( 
																( value ? Mode.PAINT 
																		: ( myCurrentMode == Mode.PAINT 
																			? Mode.NONE 
																			: myCurrentMode ) ) ); } }
	public bool			prIsEditingPoints {				get { return ( myCurrentMode == Mode.EDIT ); } 
														set { changeMode( 
																( value ? Mode.EDIT 
																		: ( myCurrentMode == Mode.EDIT 
																			? Mode.NONE 
																			: myCurrentMode ) ) ); } }
	public bool			prModeChangeWait {				get { return myModeChangeWait; } 
														set { myModeChangeWait = value; } }
	public float		prBrushRadius {					get { return myBrushSize; }
														set { myBrushSize = value; } }
	public float		prPaintAmount {					get { return myPaintAmount; }
														set { myPaintAmount = value; } }

	public Material		prEditableMaterial {			get { return myEditableMaterial; }
														set { myEditableMaterial = value; } }
	public Material		prOriginalMaterial {			get { return myOriginalMaterial; }
														set { myOriginalMaterial = value; } }

	public HAPI_GeoAttribute prActiveAttribute {		get { return myActiveAttribute; }
														private set {} }
	public List< HAPI_GeoAttribute > prAttributes {		get { return myAttributes; }
														set { myAttributes = value; } }

	public void reset()
	{
		// Please keep these in the same order and grouping as their declarations at the top.

		myHasChanged = true;
		myLiveUpdates = true;

		myMesh = null;
		myMeshRenderer = null;
		myMeshCollider = null;
		myTransform = null;

		// Should not set prCurrentMode here because it triggers a call to
		// changeMode() which cannot be called in a constructor.
		myCurrentMode = Mode.NONE;

		prModeChangeWait = false;
		prBrushRadius = 0.5f;
		prPaintAmount = 0.1f;

		prEditableMaterial = null;
		prOriginalMaterial = null;

		myActiveAttribute = null;
		prAttributes = new List< HAPI_GeoAttribute >();
	}

	public void init( Mesh mesh, MeshRenderer mesh_renderer, MeshCollider mesh_collider, Transform trans )
	{
		reset();

		myMesh = mesh;
		myMeshRenderer = mesh_renderer;
		myMeshCollider = mesh_collider;
		myTransform = trans;

		if ( myMesh.colors == null )
		{
			Color[] colours = new Color[ myMesh.vertexCount ];
			for ( int i = 0; i < myMesh.vertexCount; ++i )
				colours[ i ] = new Color( 1.0f, 1.0f, 1.0f );
			myMesh.colors = colours;
		}

		// Create the editable material that can display vertex colours.
		if ( prEditableMaterial == null )
		{
			prOriginalMaterial = mesh_renderer.sharedMaterial;
			if ( prOriginalMaterial )
			{
				prEditableMaterial = Material.Instantiate( prOriginalMaterial ) as Material;
				prEditableMaterial.name = prOriginalMaterial.name + " (Editable Copy)";
				prEditableMaterial.shader = Shader.Find( "HAPI/SpecularVertexColor" );
			}
			else
				prEditableMaterial = new Material( Shader.Find( "HAPI/SpecularVertexColor" ) );
		}
	}

	public void changeMode( Mode new_mode )
	{
		if ( !myMesh || !myMeshRenderer || !myMeshCollider )
			return;

		if ( new_mode != myCurrentMode )
		{
			if ( new_mode == Mode.NONE )
			{
				// Note that this assignment causes extra events to be 
				// generated which mess up the control key holding and can
				// cause mode flickering. We had to treat left and right
				// control as the same key for this reason.
				myMeshRenderer.sharedMaterial = prOriginalMaterial;

				myMeshCollider.sharedMesh = myMesh;
				myMeshCollider.enabled = false;
				myMeshCollider.enabled = true;
			}
			else
			{
				// Note that this assignment causes extra events to be 
				// generated which mess up the control key holding and can
				// cause mode flickering. We had to treat left and right
				// control as the same key for this reason.
				myMeshRenderer.sharedMaterial = prEditableMaterial;

				myMeshCollider.sharedMesh = myMesh;
				myMeshCollider.enabled = false;
				myMeshCollider.enabled = true;
			}
			myCurrentMode = new_mode;
		}
	}

	public void refreshMeshColours()
	{
		if ( prActiveAttribute )
			myMesh.colors = prActiveAttribute.getColorRepresentation();
		else
			myMesh.colors = new Color[ myMesh.vertexCount ];
	}

	public void paint( RaycastHit hit_info, float amount )
	{
		if ( !myMesh || !myMeshCollider || !myTransform )
			return;

		if ( !prActiveAttribute || prActiveAttribute.prType == HAPI_GeoAttribute.Type.UNDEFINED )
			return;

		Vector3[] verts = myMesh.vertices;

		Vector3 hit_point = myTransform.InverseTransformPoint( hit_info.point );

		double current_time = System.DateTime.Now.ToOADate();
		double time_delta = current_time - myLastPaintTime;
		myLastPaintTime = current_time;

		float paint_factor = Mathf.Sign( amount );
		double min_time_delta = (double) HAPI.HAPI_Host.prPaintBrushRate * myPaintTimeMinDelta;
		if ( time_delta < min_time_delta )
			paint_factor *= (float) ( time_delta / min_time_delta );

		for ( int i = 0; i < myMesh.vertexCount; ++i )
			if ( Vector3.Distance( hit_point, verts[ i ] ) <= prBrushRadius )
				prActiveAttribute.paint( i, paint_factor );

		myMesh.colors = prActiveAttribute.getColorRepresentation();

		myHasChanged = true;
	}

	public void updatePoint( int index, Vector3 position )
	{
		if ( !myMesh || !myMeshCollider )
			return;

		Vector3[] verts = myMesh.vertices;
		verts[ index ] = position;
		myMesh.vertices = verts;
		myMesh.RecalculateBounds();
		myMesh.RecalculateNormals();

		// Refresh MeshCollider's geometry.
		myMeshCollider.enabled = false;
		myMeshCollider.enabled = true;

		myHasChanged = true;
	}

	public void setActiveAttribute( string name )
	{
		if ( !myMesh )
			return;

		if ( myActiveAttribute && myActiveAttribute.prName == name )
			return;

		for ( int i = 0; i < myAttributes.Count; ++i )
			if ( myAttributes[ i ].prName == name )
			{
				myActiveAttribute = myAttributes[ i ];
				myMesh.colors = prActiveAttribute.getColorRepresentation();
				break;
			}
	}

	public HAPI_GeoAttribute createAttribute()
	{
		int temp_name_count = 0;
		string temp_name = "";
		while ( temp_name == "" )
		{
			temp_name = "new_attribute_" + temp_name_count;
			for ( int i = 0; i < myAttributes.Count; ++i )
				if ( myAttributes[ i ].prName == temp_name )
				{
					temp_name_count++;
					temp_name = "";
					break;
				}
		}

		HAPI_GeoAttribute new_attribute = ScriptableObject.CreateInstance< HAPI_GeoAttribute >();
		new_attribute.init( myMesh, temp_name, HAPI_GeoAttribute.Type.FLOAT, 3 );
		myAttributes.Add( new_attribute );

		if ( myActiveAttribute == null )
			myActiveAttribute = new_attribute;

		refreshMeshColours();

		return new_attribute;
	}

	public void deleteAttribute( string name )
	{
		for ( int i = 0; i < myAttributes.Count; ++i )
			if ( myAttributes[ i ].prName == name )
			{
				myAttributes.RemoveAt( i );
				break;
			}

		if ( myActiveAttribute && myActiveAttribute.prName == name )
			if ( myAttributes.Count > 0 )
				myActiveAttribute = myAttributes[ 0 ];
			else
				myActiveAttribute = null;

		refreshMeshColours();
	}

	[SerializeField] private bool			myHasChanged;
	[SerializeField] private bool			myLiveUpdates;

	[SerializeField] private Mesh			myMesh;
	[SerializeField] private MeshRenderer	myMeshRenderer;
	[SerializeField] private MeshCollider	myMeshCollider;
	[SerializeField] private Transform		myTransform;

	[SerializeField] private Mode			myCurrentMode;
	[SerializeField] private bool			myModeChangeWait;
	[SerializeField] private float			myBrushSize;
	[SerializeField] private float			myPaintAmount;
	[SerializeField] private double			myLastPaintTime;
	private const double					myPaintTimeMinDelta = 0.0000005;

	[SerializeField] private Material		myEditableMaterial;
	[SerializeField] private Material		myOriginalMaterial;

	[SerializeField] private HAPI_GeoAttribute myActiveAttribute;
	[SerializeField] private List< HAPI_GeoAttribute > myAttributes;

#endif // UNITY_STANDALONE_WIN
}
