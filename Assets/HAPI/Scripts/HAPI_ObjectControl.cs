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

public class HAPI_ObjectControl : HAPI_Control 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	
	// Please keep these in the same order and grouping as their initializations in HAPI_Control.reset().

	public int		prObjectId {		get { return myObjectId; }			set { myObjectId = value; } }
	public string	prObjectName {		get { return myObjectName; }		set { myObjectName = value; } }
	public bool		prObjectVisible {	get { return myObjectVisible; }		set { myObjectVisible = value; } }

	public List< GameObject > prGeos {	get { return myGeos; }				set { myGeos = value; } }

	public HAPI_ObjectControl() 
	{
		reset();
	}

	~HAPI_ObjectControl()
	{

	}

	public override void reset()
	{
		base.reset();

		// Please keep these in the same order and grouping as their declarations at the top.
		
		myObjectId		= -1;
		myObjectName	= "object_name";
		myObjectVisible	= false;

		myGeos = new List< GameObject >( 0 );
	}

	public void init( HAPI_ObjectControl object_control )
	{
		init( (HAPI_Control) object_control );

		prObjectId		= object_control.prObjectId;
		prObjectName	= object_control.prObjectName;
		prObjectVisible = object_control.prObjectVisible;
	}

	public void init( 
		int asset_id, int node_id, HAPI_Asset asset, int object_id, string object_name, bool object_visible )
	{
		prAssetId		= asset_id;
		prNodeId		= node_id;
		prAsset			= asset;
		prObjectId		= object_id;
		prObjectName	= object_name;
		prObjectVisible = object_visible;
	}

	public override string getFullControlNameAndPath()
	{
		return base.getFullControlNameAndPath() + "/" + prObjectName;
	}

	public void refresh( bool reload_asset, HAPI_ObjectInfo object_info )
	{
		if ( reload_asset )
		{
			for ( int i = 0; i < myGeos.Count; ++i )
				HAPI_AssetUtility.destroyGameObject( myGeos[ i ] );
			myGeos.Clear();
		}

		if ( reload_asset || object_info.haveGeosChanged )
		{
			// Add new geos as needed.
			while ( myGeos.Count < object_info.geoCount )
				myGeos.Add( createGeo( myGeos.Count ) );

			// Remove stale geos.
			while ( myGeos.Count > object_info.geoCount )
			{
				HAPI_AssetUtility.destroyGameObject( myGeos[ object_info.geoCount ] );
				myGeos.RemoveAt( object_info.geoCount );
			}

			// Refresh all geos.
			for ( int i = 0; i < myGeos.Count; ++i )
				myGeos[ i ].GetComponent< HAPI_GeoControl >().refresh( reload_asset );
		}
	}

	public void beginBakeAnimation()
	{
		myCurveCollection = new HAPI_CurvesCollection();
	}

	public void bakeAnimation( float curr_time, GameObject parent_object, HAPI_Transform hapi_transform )
	{
		try
		{
			Matrix4x4 parent_xform_inverse = Matrix4x4.identity;

			if ( parent_object != null )
				parent_xform_inverse = parent_object.transform.localToWorldMatrix.inverse;

			Vector3 pos = new Vector3();
			
			// Apply object transforms.
			//
			// Axis and Rotation conversions:
			// Note that Houdini's X axis points in the opposite direction that Unity's does.  Also, Houdini's 
			// rotation is right handed, whereas Unity is left handed.  To account for this, we need to invert
			// the x coordinate of the translation, and do the same for the rotations (except for the x rotation,
			// which doesn't need to be flipped because the change in handedness AND direction of the left x axis
			// causes a double negative - yeah, I know).
			
			pos[ 0 ] = -hapi_transform.position[ 0 ];
			pos[ 1 ] =  hapi_transform.position[ 1 ];
			pos[ 2 ] =  hapi_transform.position[ 2 ];
			
			Quaternion quat = new Quaternion( 	hapi_transform.rotationQuaternion[ 0 ],
												hapi_transform.rotationQuaternion[ 1 ],
												hapi_transform.rotationQuaternion[ 2 ],
												hapi_transform.rotationQuaternion[ 3 ] );
			
			Vector3 euler = quat.eulerAngles;
			euler.y = -euler.y;
			euler.z = -euler.z;
								
			quat = Quaternion.Euler( euler );
			
			Vector3 scale = new Vector3 ( hapi_transform.scale[ 0 ],
										  hapi_transform.scale[ 1 ],
										  hapi_transform.scale[ 2 ] );
			
			if( parent_object != null )
			{
				Matrix4x4 world_mat = Matrix4x4.identity;
				world_mat.SetTRS( pos, quat, scale );
				Matrix4x4 local_mat = parent_xform_inverse  * world_mat;
				
				quat = HAPI_AssetUtility.getQuaternion( local_mat );
				scale = HAPI_AssetUtility.getScale( local_mat );
				pos = HAPI_AssetUtility.getPosition( local_mat );
			}

			HAPI_CurvesCollection curves = myCurveCollection;
			
			addKeyToCurve( curr_time, pos[ 0 ], curves.tx );
			addKeyToCurve( curr_time, pos[ 1 ], curves.ty );
			addKeyToCurve( curr_time, pos[ 2 ], curves.tz );
			addKeyToCurve( curr_time, quat.x, curves.qx );
			addKeyToCurve( curr_time, quat.y, curves.qy );
			addKeyToCurve( curr_time, quat.z, curves.qz );
			addKeyToCurve( curr_time, quat.w, curves.qw );
			addKeyToCurve( curr_time, scale.x, curves.sx );
			addKeyToCurve( curr_time, scale.y, curves.sy );
			addKeyToCurve( curr_time, scale.z, curves.sz );
		}
		catch ( HAPI_Error error )
		{
			Debug.LogWarning( error.ToString() );
			return;
		}
	}

	public bool endBakeAnimation()
	{
		try
		{
			HAPI_CurvesCollection curves = myCurveCollection;
			AnimationClip clip = curves.assignCurvesToClip();

			if ( clip != null )
			{				
				
				Animation anim_component = gameObject.GetComponent< Animation >();
				if ( anim_component == null )
				{
					gameObject.AddComponent< Animation >();
					anim_component = gameObject.GetComponent< Animation >();
				}

				anim_component.clip = clip;
				return true;
			}
			
			return false;
		}
		catch ( HAPI_Error error )
		{
			Debug.LogWarning( error.ToString() );
			return false;
		}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private Methods

	private GameObject createGeo( int geo_id )
	{
		GameObject child = new GameObject( "uninitialized_geo" );
		child.transform.parent = gameObject.transform;
		child.isStatic = gameObject.isStatic;

		// Need to reset position here because the assignment above will massage the child's
		// position in order to be in the same place it was in the global namespace.
		child.transform.localPosition	= new Vector3();
		child.transform.localRotation	= new Quaternion();
		child.transform.localScale		= new Vector3( 1.0f, 1.0f, 1.0f );

		HAPI_GeoControl control = child.AddComponent< HAPI_GeoControl >();
		control.init( this );
		control.prGeoId = geo_id;
		control.prObjectControl = this;

		return child;
	}

	private void addKeyToCurve( float time, float val, AnimationCurve curve )
	{
		Keyframe curr_key = new Keyframe( time, val, 0, 0 );
		curve.AddKey( curr_key );
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Serialized Private Data

	[SerializeField] private int		myObjectId;
	[SerializeField] private string		myObjectName;
	[SerializeField] private bool		myObjectVisible;

	[SerializeField] private List< GameObject > myGeos;
	
	private HAPI_CurvesCollection myCurveCollection = null;
}
