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

public class HoudiniObjectControl : HoudiniControl 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	
	// Please keep these in the same order and grouping as their initializations in HAPI_Control.reset().

	public int		prObjectId {			get { return myObjectId; }		set { myObjectId = value; } }
	public string	prObjectName {			get { return myObjectName; }	set { myObjectName = value; } }
	public bool		prObjectVisible {		get { return myObjectVisible; }	set { myObjectVisible = value; } }
	public HAPI_ObjectInfo prObjectInfo {	get { return myObjectInfo; }	set { myObjectInfo = value; } }

	public List< GameObject > prGeos {		get { return myGeos; }			set { myGeos = value; } }

	public HoudiniObjectControl() 
	{
		reset();
	}

	~HoudiniObjectControl()
	{

	}

	public override void reset()
	{
		base.reset();

		// Please keep these in the same order and grouping as their declarations at the top.
		
		myObjectId = -1;
		myObjectName = "object_name";
		myObjectVisible = false;
		myObjectInfo = new HAPI_ObjectInfo();
		myObjectInfo.nodeId = -1;

		myGeos = new List< GameObject >( 0 );
	}

	public void init( HoudiniObjectControl object_control )
	{
		init( (HoudiniControl) object_control );

		prObjectId		= object_control.prObjectId;
		prObjectName	= object_control.prObjectName;
		prObjectVisible = object_control.prObjectVisible;
	}

	public void init( 
		int asset_id, int node_id, HoudiniAsset asset, int object_id, string object_name, bool object_visible )
	{
		prAssetId		= asset_id;
		prNodeId		= node_id;
		prAsset			= asset;
		prObjectNodeId	= node_id;
		prObjectId		= object_id;
		prObjectName	= object_name;
		prObjectVisible = object_visible;
	}

	public override string getRelativePath()
	{
		string name = prObjectName;

		// This is necessary so we return consistent paths regardless of the 
		// name of the asset node which can be named differently for different
		// instantiations of the asset (ie geo1, geo2, etc.)
		if ( prAsset.prNodeId == prObjectNodeId )
		{
			name = prAsset.prAssetOpName;
		}

		return base.getRelativePath() + "/" + name;
	}

	public override string getAbsolutePath()
	{
		string asset_name = "";

		// If this object is NOT the asset node, we want to include the
		// asset node name in the path.
		if ( prAsset.prNodeId != prObjectNodeId )
		{
			asset_name = prAsset.prAssetName + "/";
		}

		return base.getAbsolutePath() + "/" + asset_name + prObjectName;
	}

	public bool refresh( bool reload_asset, HAPI_ObjectInfo object_info )
	{
		bool needs_recook = false;

		if ( reload_asset )
		{
			for ( int i = 0; i < myGeos.Count; ++i )
				if ( myGeos[ i ] )
					HoudiniAssetUtility.destroyGameObject( myGeos[ i ] );
			myGeos.Clear();
		}

		if ( reload_asset || object_info.haveGeosChanged )
		{
			// Get the GeoInfos of the display geo node
			HAPI_GeoInfo display_geo_info = HoudiniHost.getDisplayGeoInfo( prObjectId );
			int GeoCount = object_info.geoCount = 1;

			// Add new geos as needed.
			while ( myGeos.Count < GeoCount )
				myGeos.Add( createGeo( display_geo_info.nodeId ) );

			int node_id = object_info.nodeId;
			if ( prAsset.prNodeInfo.type == HAPI_NodeType.HAPI_NODETYPE_SOP )
				node_id = display_geo_info.nodeId;

			// Look for editable nodes inside the network/the object
			const bool recursive = true;
			int[] editable_networks = HoudiniHost.getChildNodeList(
				node_id,
				(int)HAPI_NodeType.HAPI_NODETYPE_SOP,
				(int)HAPI_NodeFlags.HAPI_NODEFLAGS_EDITABLE,
				recursive);

			// Add the editable nodes to it
			for ( int n = 0; n < editable_networks.Length; n++ )
			{
				// The editable node has to be cooked first
				HoudiniHost.cookNode( editable_networks[ n ] );

				HAPI_GeoInfo editGeoInfo = HoudiniHost.getGeoInfo( editable_networks[ n ] );
				myGeos.Add( createGeo( editGeoInfo.nodeId ) );
				GeoCount++;
			}

			// Remove stale geos.
			while ( myGeos.Count > GeoCount )
			{
				HoudiniAssetUtility.destroyGameObject( myGeos[ GeoCount ] );
				myGeos.RemoveAt( GeoCount );
			}

			// Refresh all geos.
			for ( int i = 0; i < myGeos.Count; ++i )
				needs_recook |= myGeos[ i ].GetComponent< HoudiniGeoControl >().refresh( reload_asset );
		}

		return needs_recook;
	}

	public void beginBakeAnimation()
	{
		myCurveCollection = new HoudiniCurvesCollection();
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
				
				quat = HoudiniAssetUtility.getQuaternion( local_mat );
				scale = HoudiniAssetUtility.getScale( local_mat );
				pos = HoudiniAssetUtility.getPosition( local_mat );
			}

			HoudiniCurvesCollection curves = myCurveCollection;
			
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
		catch ( HoudiniError error )
		{
			Debug.LogWarning( error.ToString() );
			return;
		}
	}

	public bool endBakeAnimation()
	{
		try
		{
			HoudiniCurvesCollection curves = myCurveCollection;
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
		catch ( HoudiniError error )
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

		HoudiniGeoControl control = child.AddComponent< HoudiniGeoControl >();
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
	[SerializeField] private HAPI_ObjectInfo myObjectInfo;

	[SerializeField] private List< GameObject > myGeos;
	
	private HoudiniCurvesCollection myCurveCollection = null;
}
