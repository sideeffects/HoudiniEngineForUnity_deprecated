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

using HAPI;

[ ExecuteInEditMode ]
[ RequireComponent( typeof( MeshFilter ) ) ]
[ RequireComponent( typeof( MeshRenderer ) ) ]
public class HAPI_GeoInputControl : HAPI_Control 
{
#if UNITY_EDITOR
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

	public bool			prEnableCooking {				get { return myEnableCooking; }
														set { myEnableCooking = value; } }
	public bool			prSyncAssetTransform {			get { return mySyncAssetTransform; } 
														set { mySyncAssetTransform = value; } }
	public bool			prLiveTransformPropagation {	get { return myLiveTransformPropagation; } 
														set { myLiveTransformPropagation = value; } }
	public int			prInputObjectId {				get { return myInputObjectId; }
														set { myInputObjectId = value; } }

	public bool			prEditable {					get { return true; }
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

	public Mesh			prEditableMesh {				get { return myEditableMesh; }
														set { myEditableMesh = value; } }
	public Material		prEditableMaterial {			get { return myEditableMaterial; }
														set { myEditableMaterial = value; } }
	public Mesh			prOriginalMesh {				get { return myOriginalMesh; }
														set { myOriginalMesh = value; } }
	public Material		prOriginalMaterial {			get { return myOriginalMaterial; }
														set { myOriginalMaterial = value; } }

	public HAPI_GeoAttribute prActiveAttribute {		get { return myActiveAttribute; }
														private set {} }
	public List< HAPI_GeoAttribute > prAttributes {		get { return myAttributes; }
														set { myAttributes = value; } }

	public void Awake()
	{
		myLastLocalToWorld = transform.localToWorldMatrix;
	}
	
	public override void reset()
	{
		base.reset();

		// Please keep these in the same order and grouping as their declarations at the top.
		
		prEnableCooking = true;
		prSyncAssetTransform = true;
		prLiveTransformPropagation = false;
		prInputObjectId = -1;

		// Should not set prCurrentMode here because it triggers a call to
		// changeMode() which cannot be called in a constructor.
		myCurrentMode = Mode.NONE;

		prModeChangeWait = false;
		prBrushRadius = 0.5f;
		prPaintAmount = 0.1f;

		prEditableMesh = null;
		prEditableMaterial = null;
		prOriginalMesh = null;
		prOriginalMaterial = null;

		myActiveAttribute = null;
		prAttributes = new List<HAPI_GeoAttribute>();
	}

	// Update is called once per frame.
	public void Update() 
	{
		if ( prInputObjectId < 0 )
			return;

		Matrix4x4 local_to_world = transform.localToWorldMatrix;
		
		if ( local_to_world == myLastLocalToWorld )
			return;

		myLastLocalToWorld = local_to_world;

		if ( prEnableCooking )
		{
			//TODO: detect geometry changes...
			
			if( prSyncAssetTransform )
			{
				HAPI_TransformEuler trans =
					HAPI_AssetUtility.getHapiTransform( transform.localToWorldMatrix );
				HAPI_Host.setObjectTransform( 0, prInputObjectId, trans );
				
				if( prLiveTransformPropagation )
				{
					prAsset.buildClientSide();
				}
			}
		}
	}

	public void OnEnable()
	{
		initialSetup();
	}

	public void initialSetup()
	{
		// Safe to assume these exist because of [RequiredComponent] attributes.
		MeshFilter mesh_filter = gameObject.GetComponent< MeshFilter >();
		MeshRenderer mesh_renderer = gameObject.GetComponent< MeshRenderer >();

		// Create the editable mesh from the original mesh. We don't want to
		// modify the original mesh because it is likely shared by many
		// instances.
		if ( prEditableMesh == null )
		{
			prOriginalMesh = mesh_filter.sharedMesh;
			prEditableMesh = Mesh.Instantiate( prOriginalMesh ) as Mesh;
			prEditableMesh.name = prOriginalMesh.name + " (Editable Copy)";

			Color[] colours = new Color[ prEditableMesh.vertexCount ];
			for ( int i = 0; i < prEditableMesh.vertexCount; ++i )
				colours[ i ] = new Color( 1.0f, 1.0f, 1.0f );
			prEditableMesh.colors = colours;
		}

		// Create the editable material that can display vertex colours.
		if ( prEditableMaterial == null )
		{
			prOriginalMaterial = mesh_renderer.sharedMaterial;
			prEditableMaterial = Material.Instantiate( prOriginalMaterial ) as Material;
			prEditableMaterial.name = prOriginalMaterial.name + " (Editable Copy)";

			prEditableMaterial.shader = Shader.Find( "HAPI/SpecularVertexColor" );
		}

		// Create MeshCollider for the paint brush.
		getOrCreateComponent< MeshCollider >();
	}

	public void changeMode( Mode new_mode )
	{
		initialSetup();

		// Safe to assume these exist because of [RequiredComponent] attributes.
		MeshFilter mesh_filter = gameObject.GetComponent< MeshFilter >();
		MeshRenderer mesh_renderer = gameObject.GetComponent< MeshRenderer >();

		if ( new_mode != myCurrentMode )
		{
			if ( new_mode == Mode.NONE )
			{
				mesh_filter.sharedMesh = prOriginalMesh;

				// Note that this assignment causes extra events to be 
				// generated which mess up the control key holding and can
				// cause mode flickering. We had to treat left and right
				// control as the same key for this reason.
				mesh_renderer.sharedMaterial = prOriginalMaterial;

				getOrCreateComponent< MeshCollider >().sharedMesh = prOriginalMesh;
				getOrCreateComponent< MeshCollider >().enabled = false;
				getOrCreateComponent< MeshCollider >().enabled = true;
			}
			else
			{
				mesh_filter.sharedMesh = prEditableMesh;

				// Note that this assignment causes extra events to be 
				// generated which mess up the control key holding and can
				// cause mode flickering. We had to treat left and right
				// control as the same key for this reason.
				mesh_renderer.sharedMaterial = prEditableMaterial;

				getOrCreateComponent< MeshCollider >().sharedMesh = prEditableMesh;
				getOrCreateComponent< MeshCollider >().enabled = false;
				getOrCreateComponent< MeshCollider >().enabled = true;
			}
			myCurrentMode = new_mode;
		}
	}

	public void refreshMeshColours()
	{
		if ( prActiveAttribute )
			prEditableMesh.colors = prActiveAttribute.getColorRepresentation();
		else
			prEditableMesh.colors = new Color[ prEditableMesh.vertexCount ];
	}

	public void paint( RaycastHit hit_info, float amount )
	{
		if ( !prEditableMesh )
			return;

		if ( !prActiveAttribute || prActiveAttribute.prType == HAPI_GeoAttribute.Type.UNDEFINED )
			return;

		Vector3[] verts = prEditableMesh.vertices;

		Vector3 hit_point = transform.InverseTransformPoint( hit_info.point );

		double current_time = System.DateTime.Now.ToOADate();
		double time_delta = current_time - myLastPaintTime;
		myLastPaintTime = current_time;

		float paint_factor = Mathf.Sign( amount );
		double min_time_delta = (double) HAPI_Host.prPaintBrushRate * myPaintTimeMinDelta;
		if ( time_delta < min_time_delta )
			paint_factor *= (float) ( time_delta / min_time_delta );

		for ( int i = 0; i < prEditableMesh.vertexCount; ++i )
			if ( Vector3.Distance( hit_point, verts[ i ] ) <= prBrushRadius )
				prActiveAttribute.paint( i, paint_factor );

		prEditableMesh.colors = prActiveAttribute.getColorRepresentation();
	}

	public void updatePoint( int index, Vector3 position )
	{
		if ( !prEditableMesh )
			return;

		Vector3[] verts = prEditableMesh.vertices;
		verts[ index ] = position;
		prEditableMesh.vertices = verts;
		prEditableMesh.RecalculateBounds();
		prEditableMesh.RecalculateNormals();

		// Refresh MeshCollider's geometry.
		getOrCreateComponent< MeshCollider >().enabled = false;
		getOrCreateComponent< MeshCollider >().enabled = true;
	}

	public void setActiveAttribute( string name )
	{
		if ( myActiveAttribute && myActiveAttribute.prName == name )
			return;

		for ( int i = 0; i < myAttributes.Count; ++i )
			if ( myAttributes[ i ].prName == name )
			{
				myActiveAttribute = myAttributes[ i ];
				prEditableMesh.colors = prActiveAttribute.getColorRepresentation();
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
		new_attribute.init( myEditableMesh, temp_name, HAPI_GeoAttribute.Type.FLOAT, 3 );
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

	[SerializeField] private bool			mySyncAssetTransform;
	[SerializeField] private bool			myLiveTransformPropagation;
	[SerializeField] private bool			myEnableCooking;
	[SerializeField] private Matrix4x4		myLastLocalToWorld;
	[SerializeField] private int			myInputObjectId;

	[SerializeField] private Mode			myCurrentMode;
	[SerializeField] private bool			myModeChangeWait;
	[SerializeField] private float			myBrushSize;
	[SerializeField] private float			myPaintAmount;
	[SerializeField] private double			myLastPaintTime;
	private const double					myPaintTimeMinDelta = 0.0000005;

	[SerializeField] private Mesh			myEditableMesh;
	[SerializeField] private Material		myEditableMaterial;
	[SerializeField] private Mesh			myOriginalMesh;
	[SerializeField] private Material		myOriginalMaterial;

	[SerializeField] private HAPI_GeoAttribute myActiveAttribute;
	[SerializeField] private List< HAPI_GeoAttribute > myAttributes;

#endif // UNITY_EDITOR
}
