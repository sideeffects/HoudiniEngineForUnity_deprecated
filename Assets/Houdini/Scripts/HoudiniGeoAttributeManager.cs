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
using System.Collections;
using System.Collections.Generic;

public class HoudiniGeoAttributeManager : ScriptableObject {
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
														set { myMeshCollider = value; } }
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

	public Material		prEditableMaterial {			get { return myEditableMaterial; }
														set { myEditableMaterial = value; } }
	public Material		prOriginalMaterial {			get { return myOriginalMaterial; }
														set { myOriginalMaterial = value; } }

	public HoudiniGeoAttribute prActiveAttribute {		get { return myActiveAttribute; }
														private set {} }
	public List< HoudiniGeoAttribute > prAttributes {		get { return myAttributes; }
														set { myAttributes = value; } }

	public void reset()
	{
		if ( myMeshRenderer != null )
			myMeshRenderer.enabled = false;
		if ( myMeshCollider != null )
			myMeshCollider.enabled = false;

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

		prEditableMaterial = null;
		prOriginalMaterial = null;

		myActiveAttribute = null;
		prAttributes = new List< HoudiniGeoAttribute >();
	}

	public void init( Mesh mesh, MeshRenderer mesh_renderer, MeshCollider mesh_collider, Transform trans )
	{
		reset();

		myMesh = mesh;
		myMeshRenderer = mesh_renderer;
		myMeshCollider = mesh_collider;
		myTransform = trans;

		// Hide the mesh until being edited.
		myMeshRenderer.enabled = false;
		myMeshCollider.enabled = false;

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
				prEditableMaterial.shader = Shader.Find( "Houdini/SpecularVertexColor" );
			}
			else
				prEditableMaterial = new Material( Shader.Find( "Houdini/SpecularVertexColor" ) );
		}
	}

	public void reInit( Mesh mesh, MeshRenderer mesh_renderer, MeshCollider mesh_collider, Transform trans )
	{
		myMesh = mesh;
		myMeshRenderer = mesh_renderer;
		myMeshCollider = mesh_collider;
		myTransform = trans;

		// Hide the mesh until being edited.
		myMeshRenderer.enabled = false;
		myMeshCollider.enabled = false;

		if ( myMesh.colors == null )
		{
			Color[] colours = new Color[ myMesh.vertexCount ];
			for ( int i = 0; i < myMesh.vertexCount; ++i )
				colours[ i ] = new Color( 1.0f, 1.0f, 1.0f );
			myMesh.colors = colours;
		}
	}

	public HoudiniGeoAttributeManager copy()
	{
		HoudiniGeoAttributeManager new_manager =
			ScriptableObject.CreateInstance< HoudiniGeoAttributeManager >();

		// It's ok to init the new manager with the mesh, mesh renderer, and mesh
		// collider of the old manager because these things will be overwritten
		// on next cook. I think.
		new_manager.init( myMesh, myMeshRenderer, myMeshCollider, myTransform );

		for ( int i = 0; i < myAttributes.Count; ++i )
		{
			HoudiniGeoAttribute new_attribute = myAttributes[ i ].copy();
			new_manager.addAttribute( new_attribute );
		}

		return new_manager;
	}

	public void changeMode( Mode new_mode )
	{
		if ( new_mode != myCurrentMode )
		{
			if ( !myMesh )
				Debug.LogWarning( "Can't paint on mesh. No mesh found!" );
			if ( !myMeshRenderer )
				Debug.LogWarning( "Can't paint on mesh. No mesh renderer found!" );
			if ( !myMeshCollider )
				Debug.LogWarning( "Can't paint on mesh. No mesh collider found!" );

			if ( !myMesh || !myMeshRenderer || !myMeshCollider )
				return;

			if ( new_mode == Mode.NONE )
			{
				// Note that this assignment causes extra events to be 
				// generated which mess up the control key holding and can
				// cause mode flickering. We had to treat left and right
				// control as the same key for this reason.
				myMeshRenderer.sharedMaterial = prOriginalMaterial;

				myMeshRenderer.enabled = false;

				myMeshCollider.sharedMesh = myMesh;
				myMeshCollider.enabled = false;
			}
			else
			{
				// Note that this assignment causes extra events to be 
				// generated which mess up the control key holding and can
				// cause mode flickering. We had to treat left and right
				// control as the same key for this reason.
				myMeshRenderer.sharedMaterial = prEditableMaterial;

				myMeshRenderer.enabled = true;

				myMeshCollider.sharedMesh = myMesh;
				myMeshCollider.enabled = false;
				myMeshCollider.enabled = true;
			}
			myCurrentMode = new_mode;
		}
	}

	public void refreshMesh()
	{
		if ( prActiveAttribute )
		{
			myMesh.colors = prActiveAttribute.getColorRepresentation();
			if ( prActiveAttribute.prName == "N" &&
				prActiveAttribute.prType == HoudiniGeoAttribute.Type.FLOAT &&
				prActiveAttribute.prTupleSize == HoudiniConstants.HAPI_NORMAL_VECTOR_SIZE )
				myMesh.normals = prActiveAttribute.prFloatDataVec3;
			else if ( prActiveAttribute.prName == "uv" &&
				prActiveAttribute.prType == HoudiniGeoAttribute.Type.FLOAT &&
				prActiveAttribute.prTupleSize == HoudiniConstants.HAPI_UV_VECTOR_SIZE )
				myMesh.uv = prActiveAttribute.prFloatDataVec2;

#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6
			else if ( prActiveAttribute.prName == HoudiniConstants.HAPI_ATTRIB_UV2 &&
				prActiveAttribute.prType == HoudiniGeoAttribute.Type.FLOAT &&
				prActiveAttribute.prTupleSize == HoudiniConstants.HAPI_UV_VECTOR_SIZE )
				myMesh.uv1 = prActiveAttribute.prFloatDataVec2;
			else if ( prActiveAttribute.prName == HoudiniConstants.HAPI_ATTRIB_UV3 &&
				prActiveAttribute.prType == HoudiniGeoAttribute.Type.FLOAT &&
				prActiveAttribute.prTupleSize == HoudiniConstants.HAPI_UV_VECTOR_SIZE )
				myMesh.uv2 = prActiveAttribute.prFloatDataVec2;
#else
			else if ( prActiveAttribute.prName == HoudiniConstants.HAPI_ATTRIB_UV2 &&
				prActiveAttribute.prType == HoudiniGeoAttribute.Type.FLOAT &&
				prActiveAttribute.prTupleSize == HoudiniConstants.HAPI_UV_VECTOR_SIZE )
				myMesh.uv2 = prActiveAttribute.prFloatDataVec2;
			else if ( prActiveAttribute.prName == HoudiniConstants.HAPI_ATTRIB_UV3 &&
				prActiveAttribute.prType == HoudiniGeoAttribute.Type.FLOAT &&
				prActiveAttribute.prTupleSize == HoudiniConstants.HAPI_UV_VECTOR_SIZE )
				myMesh.uv3 = prActiveAttribute.prFloatDataVec2;
#endif // UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6
		}
		else
			myMesh.colors = new Color[ myMesh.vertexCount ];
	}

	public void paint( RaycastHit hit_info, bool inverse )
	{
#if UNITY_EDITOR
		if ( !myMesh || !myMeshCollider || !myTransform )
			return;

		if ( !prActiveAttribute || prActiveAttribute.prType == HoudiniGeoAttribute.Type.UNDEFINED )
			return;

		Vector3[] verts = myMesh.vertices;

		Vector3 hit_point = myTransform.InverseTransformPoint( hit_info.point );

		double current_time = System.DateTime.Now.ToOADate();
		double time_delta = current_time - myLastPaintTime;
		myLastPaintTime = current_time;

		float paint_factor = 1.0f;
		double min_time_delta = myPaintTimeMinDelta;
		if ( time_delta < min_time_delta )
			paint_factor *= (float) ( time_delta / min_time_delta );

		for ( int i = 0; i < myMesh.vertexCount; ++i )
			if ( Vector3.Distance( hit_point, verts[ i ] ) <= prBrushRadius )
				prActiveAttribute.paint( i, paint_factor, inverse );

		refreshMesh();

		myHasChanged = true;
#endif // UNITY_EDITOR
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

	public HoudiniGeoAttribute createAttribute()
	{
		return createAttribute( "new_attribute" );
	}
	public HoudiniGeoAttribute createAttribute( string suggested_name )
	{
		string temp_name = getUniqueAttributeName( suggested_name );
		HoudiniGeoAttribute new_attribute = ScriptableObject.CreateInstance< HoudiniGeoAttribute >();
		new_attribute.init( myMesh, temp_name, HoudiniGeoAttribute.Type.FLOAT, 3 );
		addAttribute( new_attribute );
		return new_attribute;
	}
	public HoudiniGeoAttribute createAttribute( HoudiniGeoAttribute.Preset preset )
	{
		HoudiniGeoAttribute new_attribute = ScriptableObject.CreateInstance< HoudiniGeoAttribute >();
		new_attribute.init( myMesh, preset );
		new_attribute.prName = getUniqueAttributeName( new_attribute.prName );
		addAttribute( new_attribute );
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

		refreshMesh();
	}

	public bool hasAttribute( string name )
	{
		for ( int i = 0; i < myAttributes.Count; ++i )
			if ( myAttributes[ i ].prName == name )
				return true;
		return false;
	}

	public HoudiniGeoAttribute getAttribute( string name )
	{
		for ( int i = 0; i < myAttributes.Count; ++i )
			if ( myAttributes[ i ].prName == name )
				return myAttributes[ i ];
		return null;
	}

	private void addAttribute( HoudiniGeoAttribute new_attribute )
	{
		myAttributes.Add( new_attribute );

		if ( myActiveAttribute == null )
			myActiveAttribute = new_attribute;

		refreshMesh();
	}

	private string getUniqueAttributeName( string given_name )
	{
		int temp_name_count = 0;
		string temp_name = "";
		while ( temp_name == "" )
		{
			temp_name = given_name;

			if ( temp_name_count > 0 )
				temp_name += temp_name_count;

			for ( int i = 0; i < myAttributes.Count; ++i )
				if ( myAttributes[ i ].prName == temp_name )
				{
					temp_name_count++;
					temp_name = "";
					break;
				}
		}

		return temp_name;
	}

	public bool syncAttributes( int asset_id, int object_id, int geo_id, int part_id, Mesh mesh )
	{
		bool needs_recook = false;

		// Fetch all point attributes.
		string[] point_attribute_names = HoudiniHost.getAttributeNames(
			geo_id, part_id, HAPI_AttributeOwner.HAPI_ATTROWNER_POINT );

		foreach ( string point_attribute_name in point_attribute_names )
		{
			if ( point_attribute_name == "P" )
				continue;

			HAPI_AttributeInfo point_attribute_info = HoudiniHost.getAttributeInfo(
				geo_id, part_id, point_attribute_name,
				HAPI_AttributeOwner.HAPI_ATTROWNER_POINT );

			HoudiniGeoAttribute attribute = null;

			// If we already have an attribute with the same name, sync it with
			// the attribute in Houdini - trying to salvage any data even if the 
			// attribute storage type, tuple size, or mesh point count has changed.
			if ( hasAttribute( point_attribute_name ) )
			{
				attribute = getAttribute( point_attribute_name );
			}
			else // Attribute not found.
			{
				HoudiniGeoAttribute.Type geo_attribute_type = HoudiniGeoAttribute.Type.UNDEFINED;
				switch ( point_attribute_info.storage )
				{
					case HAPI_StorageType.HAPI_STORAGETYPE_INT:
						geo_attribute_type = HoudiniGeoAttribute.Type.INT; break;
					case HAPI_StorageType.HAPI_STORAGETYPE_FLOAT:
						geo_attribute_type = HoudiniGeoAttribute.Type.FLOAT; break;
					case HAPI_StorageType.HAPI_STORAGETYPE_STRING:
						geo_attribute_type = HoudiniGeoAttribute.Type.STRING; break;
				}

				attribute = createAttribute( point_attribute_name );
				attribute.init(
					mesh, point_attribute_name, geo_attribute_type,
					point_attribute_info.tupleSize );
				attribute.prOriginalAttributeOwner = HAPI_AttributeOwner.HAPI_ATTROWNER_POINT;
			}

			// Sync the values of the Unity attribute with the Houdini attribute.
			needs_recook |= attribute.sync( geo_id, part_id, mesh, point_attribute_info );

			// If the sync updated the values from the asset we need to refresh our mesh.
			if ( needs_recook )
				refreshMesh();
		}

		return needs_recook;
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
	[SerializeField] private double			myLastPaintTime;
	private const double					myPaintTimeMinDelta = 0.000001;

	[SerializeField] private Material		myEditableMaterial;
	[SerializeField] private Material		myOriginalMaterial;

	[SerializeField] private HoudiniGeoAttribute myActiveAttribute;
	[SerializeField] private List< HoudiniGeoAttribute > myAttributes;

}
