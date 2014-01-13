using UnityEngine;
using System.Collections;

using HAPI;

[ ExecuteInEditMode ]
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

	public Mode				prCurrentMode {				get { return myCurrentMode; }
														set { myCurrentMode = value; } }
	public bool				prIsPaintingPoints {		get { return ( myCurrentMode == Mode.PAINT ); } 
														set { myCurrentMode = 
																( value ? Mode.PAINT 
																		: ( myCurrentMode == Mode.PAINT 
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

		myTouched = false;
		prCurrentMode = Mode.NONE;
		prModeChangeWait = false;
	}
	
	// Use this for initialization
	public void Start () 
	{
	
	}
	
	// Update is called once per frame
	public void Update () 
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

	public void updatePoint( int index, Vector3 position )
	{
		MeshFilter mesh_filter = gameObject.GetComponent< MeshFilter >();
		if ( !mesh_filter )
			return;

		if ( !myTouched )
		{
			if ( !mesh_filter.sharedMesh )
				return;
			else
			{
				Mesh mesh_copy = Mesh.Instantiate( mesh_filter.sharedMesh ) as Mesh;
				
				Color[] colours = new Color[ mesh_copy.vertexCount ];
				for ( int i = 0; i < mesh_copy.vertexCount; ++i )
					colours[ i ] = new Color( 1.0f, 0.0f, 0.0f );

				mesh_copy.colors = colours;
				mesh_filter.sharedMesh = mesh_copy;
			}

			myTouched = true;
		}

		Vector3[] verts = mesh_filter.sharedMesh.vertices;
		verts[ index ] = position;
		mesh_filter.sharedMesh.vertices = verts;
	}

	[SerializeField] private bool			mySyncAssetTransform;
	[SerializeField] private bool			myLiveTransformPropagation;
	[SerializeField] private bool			myEnableCooking;
	[SerializeField] private Matrix4x4		myLastLocalToWorld;
	[SerializeField] private int			myInputObjectId;

	[SerializeField] private bool			myTouched;
	[SerializeField] private Mode			myCurrentMode;
	[SerializeField] private bool			myModeChangeWait;

#endif // UNITY_EDITOR
}
