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

// Master control for enabling runtime.
#if ( UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_LINUX )
	#define HAPI_ENABLE_RUNTIME
#endif

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using Utility = HoudiniAssetUtility;
using System;

public class HoudiniCurvesCollection
{
	public AnimationCurve tx = new AnimationCurve();
	public AnimationCurve ty = new AnimationCurve();
	public AnimationCurve tz = new AnimationCurve();
	public AnimationCurve qx = new AnimationCurve();
	public AnimationCurve qy = new AnimationCurve();
	public AnimationCurve qz = new AnimationCurve();
	public AnimationCurve qw = new AnimationCurve();
	public AnimationCurve sx = new AnimationCurve();
	public AnimationCurve sy = new AnimationCurve();
	public AnimationCurve sz = new AnimationCurve();
	
	
	private bool isConstantAnimCurve( AnimationCurve curve )
	{
		if( curve.length < 1 )
			return true;
		
		Keyframe key = curve.keys[ 0 ];
		for ( int ii=1; ii < curve.keys.Length; ii++ )
		{
			Keyframe curr_key = curve.keys[ ii ];
			if( Mathf.Abs( curr_key.value - key.value ) > 0.0001f )
			{
				return false;
			}			
		}
		
		return true;
	}
	
	public AnimationClip assignCurvesToClip()
	{
		HoudiniCurvesCollection curves = this;

		AnimationClip clip = new AnimationClip();
		clip.legacy = true;

		bool found_nonconst_curve = false;
		
		if( !curves.isConstantAnimCurve( curves.tx ) )
		{
			clip.SetCurve( "", typeof(Transform), "localPosition.x", curves.tx );
			found_nonconst_curve = true;
		}
		
		if( !curves.isConstantAnimCurve( curves.ty ) )
		{
			clip.SetCurve( "", typeof(Transform), "localPosition.y", curves.ty );
			found_nonconst_curve = true;
		}
		
		if( !curves.isConstantAnimCurve( curves.tz ) )
		{				
			clip.SetCurve( "", typeof(Transform), "localPosition.z", curves.tz );
			found_nonconst_curve = true;
		}
		
		if( !curves.isConstantAnimCurve( curves.qx ) || 
			!curves.isConstantAnimCurve( curves.qy ) ||
			!curves.isConstantAnimCurve( curves.qz ) ||
			!curves.isConstantAnimCurve( curves.qw ) )
		{
			clip.SetCurve( "", typeof(Transform), "localRotation.x", curves.qx );
			clip.SetCurve( "", typeof(Transform), "localRotation.y", curves.qy );
			clip.SetCurve( "", typeof(Transform), "localRotation.z", curves.qz );
			clip.SetCurve( "", typeof(Transform), "localRotation.w", curves.qw );
			found_nonconst_curve = true;
		}		
		
		if( !curves.isConstantAnimCurve( curves.sx ) )
		{
			clip.SetCurve( "", typeof(Transform), "localScale.x", curves.sx );
			found_nonconst_curve = true;
		}	
		
		if( !curves.isConstantAnimCurve( curves.sy ) )
		{
			clip.SetCurve( "", typeof(Transform), "localScale.y", curves.sy );
			found_nonconst_curve = true;
		}
		
		if( !curves.isConstantAnimCurve( curves.sz ) )
		{
			clip.SetCurve( "", typeof(Transform), "localScale.z", curves.sz );
			found_nonconst_curve = true;
		}	
		
		if( found_nonconst_curve )
		{
			clip.EnsureQuaternionContinuity();
			return clip;
		}
		return null;
	}
	
}

public class HoudiniInstancer : MonoBehaviour
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties

	public HoudiniAsset 	prAsset { get { return myAsset; } set { myAsset = value; } }
	public int 			prObjectId { get { return myObjectId; } set { myObjectId = value; } }

	public HoudiniInstancerPersistentData prPersistentData
	{
		get
		{
			HoudiniInstancerManager instancer_manager = prAsset.GetComponent< HoudiniInstancerManager >();
			if( instancer_manager == null )
				return null;

			HoudiniInstancerPersistentData data = instancer_manager.getInstancerPersistentData( this.name );
			if( data == null )
				Debug.LogError("Unable to retrieve persistent data for instancer " + this.name );

			return data;
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods
	
	public HoudiniInstancer () 
	{
		prAsset = null;
		prObjectId = -1;
		myNumInstances = 0;
		myCurvesCollection = null;
	}
	
	
	private void instanceOverriddenObjects( int total_points, List< int > exclusion_list )
	{		
		int current_max_point_index = total_points - 1;

		HoudiniInstancerPersistentData data = prPersistentData;
		for ( int ii = 0; ii < data.overriddenInstances.Count; ii++ )
		{
			HoudiniInstancerOverrideInfo override_info = data.overriddenInstances[ ii ];
			
			Vector3 pos = override_info.translate;
			
			Vector3 euler = override_info.rotate;
			
			Vector3 scale = override_info.scale;
			
			GameObject object_to_instantiate = override_info.objectToInstantiate;
			if( object_to_instantiate == null )
			{
				object_to_instantiate = GameObject.Find( override_info.objectToInstantiatePath );
				if( object_to_instantiate == null )
				{
					data.overriddenInstances.RemoveAt( ii );
					continue;
				}
			}

			instanceObject( object_to_instantiate, 
							pos, euler, override_info.instancePointNumber, true, scale, false, "" );
			
			if ( override_info.instancePointNumber < total_points )
				exclusion_list.Add( override_info.instancePointNumber );
			else
			{
				if ( current_max_point_index >= 0 )
				{
					exclusion_list.Add( current_max_point_index );
					current_max_point_index--;
				}
			}
				
		}
	}


	private void instanceObject( GameObject objToInstantiate, 
								 Vector3 pos,
								 Vector3 euler,
								 int point_index,
								 bool scale_exists,
								 Vector3 scale,
								 bool attach_script_exists,
								 string attach_script )
	{
		if ( objToInstantiate == null )
		{
			Debug.LogError( "No object to instantiate for instancer '" + this.name + "' and point index " + point_index + "!" );
			return;
		}
		
		GameObject obj = null;

#if UNITY_EDITOR
		GameObject user_instance =
			prPersistentData.getUserObjToInstantiateFromName( objToInstantiate.name, point_index );

		// See if object is a prefab instance.
		var prefab_parent = PrefabUtility.GetPrefabParent( user_instance ) as GameObject;
		if ( prefab_parent != null )
			user_instance = prefab_parent;

		if ( user_instance != null )
		{
			obj = PrefabUtility.InstantiatePrefab( user_instance ) as GameObject;
			if ( obj == null )
			{
				bool liveTransformPropagationSetting	= false;
				bool syncAssetTransformSetting			= false;
				bool enableCooking						= true;
				
				HoudiniAsset hapi_asset = user_instance.GetComponent< HoudiniAsset >();
				if ( hapi_asset != null )
				{
					liveTransformPropagationSetting			= hapi_asset.prTransformChangeTriggersCooks;
					syncAssetTransformSetting				= hapi_asset.prPushUnityTransformToHoudini;
					enableCooking							= hapi_asset.prEnableCooking;
					hapi_asset.prTransformChangeTriggersCooks	= false;
					hapi_asset.prPushUnityTransformToHoudini	= false;
					hapi_asset.prEnableCooking					= false;
				}
				
				obj = Instantiate( user_instance, new Vector3(0,0,0), Quaternion.identity ) as GameObject;
				HoudiniAsset hapi_asset_on_clone =  obj.GetComponent< HoudiniAsset >();
				if ( hapi_asset_on_clone != null )
				{
					Destroy( hapi_asset_on_clone );
				}

				if ( hapi_asset != null )
				{
					hapi_asset.prTransformChangeTriggersCooks	= liveTransformPropagationSetting;
					hapi_asset.prPushUnityTransformToHoudini	= syncAssetTransformSetting;
					hapi_asset.prEnableCooking					= enableCooking;
				}
			}

			HoudiniInstance instance = obj.AddComponent< HoudiniInstance >();
			instance.prInstancePointNumber = point_index;
			instance.prObjectToInstantiate = user_instance;
			instance.prInstancer = this;

			obj.transform.localPosition = pos;

			// Rotation order is important here. Bug: #63304
			Quaternion user_angle_rot = user_instance.transform.localRotation;
			Quaternion offset_rot = Quaternion.Euler( prPersistentData.rotationalOffset );
			Quaternion instance_rot = Quaternion.Euler( euler );
			obj.transform.localRotation = instance_rot * offset_rot * user_angle_rot;

			if ( scale_exists )
			{
				obj.transform.localScale = new Vector3(
					prPersistentData.scaleOffset.x * user_instance.transform.localScale.x * scale.x,
					prPersistentData.scaleOffset.y * user_instance.transform.localScale.y * scale.y,
					prPersistentData.scaleOffset.z * user_instance.transform.localScale.z * scale.z );
			}
			else
			{
				obj.transform.localScale = new Vector3(
					prPersistentData.scaleOffset.x * user_instance.transform.localScale.x,
					prPersistentData.scaleOffset.y * user_instance.transform.localScale.y,
					prPersistentData.scaleOffset.z * user_instance.transform.localScale.z );
			}
		}
		else
#endif // UNITY_EDITOR
		{
			obj = Instantiate( objToInstantiate, pos, Quaternion.Euler( euler ) ) as GameObject;

			HoudiniInstance instance = obj.AddComponent< HoudiniInstance >();

			instance.prInstancePointNumber = point_index;
			instance.prObjectToInstantiate = objToInstantiate;
			instance.prInstancer = this;
			
			if ( scale_exists )
			{
				if ( Mathf.Approximately( 0.0f, scale.x ) ||
					 Mathf.Approximately( 0.0f, scale.y ) ||
					 Mathf.Approximately( 0.0f, scale.z ) )
				{
					Debug.LogWarning( "Instance " + point_index + ": Scale has a zero component!" );
				}
				obj.transform.localScale = scale;
			}

			// The original object is probably set to be invisible because it just contains
			// the raw geometry with no transforms applied. We need to set the newly instanced
			// object's childrens' mesh renderers to be enabled otherwise the instanced
			// objects will also be invisible. :)
			MeshCollider[] mesh_colliders = obj.GetComponentsInChildren< MeshCollider >();
			foreach ( MeshCollider mesh_collider in mesh_colliders )
				mesh_collider.enabled = true;
			MeshRenderer[] mesh_renderers = obj.GetComponentsInChildren< MeshRenderer >();
			foreach ( MeshRenderer mesh_renderer in mesh_renderers )
				mesh_renderer.enabled = true;
		}

		obj.transform.parent = transform;
		
		if ( attach_script_exists )
			HoudiniAssetUtility.attachScript( obj, attach_script );
	}
	
	public bool hasOverriddenInstances()
	{
		HoudiniInstancerPersistentData persistent_data = prPersistentData;
		return persistent_data.overriddenInstances.Count > 0;
	}
	
	public bool isPointOverridden( int point_index )
	{
		HoudiniInstancerPersistentData persistent_data = prPersistentData;
		foreach ( HoudiniInstancerOverrideInfo override_info in persistent_data.overriddenInstances )
		{
			if( override_info.instancePointNumber == point_index )
				return true;
		}
		
		return false;
	}

	public static string getGameObjectPath( GameObject obj )
	{
		string path = "/" + obj.name;
		while ( obj.transform.parent != null )
		{
			obj = obj.transform.parent.gameObject;
			path = "/" + obj.name + path;
		}
		return path;
	}

#if UNITY_EDITOR
	public void pinObject( GameObject pin_object, bool pin )
	{		
		HoudiniInstance instance = pin_object.GetComponent< HoudiniInstance >();
		if ( instance == null )
		{
			//The user might be moving the part instead of the object, so we should
			//try to pin that in this case.
			Transform parent = pin_object.transform.parent;
			while ( parent != null )
			{
				instance = parent.gameObject.GetComponent< HoudiniInstance >();
				if( instance != null )
					break;
				parent = parent.parent;
			}
			
			if( instance == null )
				return;
		}
		
		if ( !pin )
			unPinInstance( instance.prInstancePointNumber );
		else
		{
			Transform game_object_xform = pin_object.transform;
			
			HoudiniInstancerOverrideInfo override_info =
				ScriptableObject.CreateInstance< HoudiniInstancerOverrideInfo >();
			
			override_info.translate = game_object_xform.position;
			override_info.rotate = game_object_xform.rotation.eulerAngles;
			
			Vector3 scale = game_object_xform.localScale;
			
			Transform parent = game_object_xform.parent;
			while ( parent != null )
			{
				scale.x *= parent.localScale.x;
				scale.y *= parent.localScale.y;
				scale.z *= parent.localScale.z;
				parent = parent.parent;
			}
			
			override_info.scale = scale;
			override_info.objectToInstantiate = instance.prObjectToInstantiate;
			override_info.objectToInstantiatePath = getGameObjectPath( instance.prObjectToInstantiate );
			override_info.instancePointNumber = instance.prInstancePointNumber;
			
			pinInstance( override_info );
		}
	}
	
	public bool pinInstance( HoudiniInstancerOverrideInfo info )
	{
		HoudiniInstancerPersistentData persistent_data = prPersistentData;
		Undo.RecordObject( persistent_data, "Houdini Pin Instance" );
		int ii = 0;
		for( ii = 0; ii < persistent_data.overriddenInstances.Count; ii++ )
		{
			HoudiniInstancerOverrideInfo override_info = persistent_data.overriddenInstances[ ii ];
			if( override_info.instancePointNumber == info.instancePointNumber )
			{
				persistent_data.overriddenInstances.RemoveAt( ii );
				break;
			}
		}
		
		persistent_data.overriddenInstances.Add( info );
		return true;
	}
	
	public void unPinAllInstances()
	{
		HoudiniInstancerPersistentData persistent_data = prPersistentData;
		Undo.RecordObject( persistent_data, "Houdini Unpin All Instances" );
		persistent_data.overriddenInstances.Clear();		
	}
	
	public void unPinInstance( int point_index )
	{
		HoudiniInstancerPersistentData persistent_data = prPersistentData;
		Undo.RecordObject( persistent_data, "Houdini Unpin Instance" );
		int index_to_remove = -1;
		for( int ii = 0 ; ii < persistent_data.overriddenInstances.Count ; ii++ )
		{
			HoudiniInstancerOverrideInfo override_info = persistent_data.overriddenInstances[ ii ];
			if( override_info.instancePointNumber == point_index )
			{
				index_to_remove = ii;
				break;
			}			
		}
		
		if( index_to_remove >= 0 )
		{
			persistent_data.overriddenInstances.RemoveAt( index_to_remove );
		}
	}
	
	public void drawAllPins()
	{
		HoudiniInstancerPersistentData persistent_data = prPersistentData;
		foreach ( HoudiniInstancerOverrideInfo override_info in persistent_data.overriddenInstances )
		{
			drawPin( override_info );
						
		}
	}
	
	public void drawPin( int point_index )
	{
		HoudiniInstancerPersistentData persistent_data = prPersistentData;
		foreach ( HoudiniInstancerOverrideInfo override_info in persistent_data.overriddenInstances )
		{
			if( override_info.instancePointNumber == point_index )
				drawPin( override_info );
		}				
	}
	
	public void drawPin( HoudiniInstancerOverrideInfo override_info )
	{
		// TODO: This code should be on the companion Editor class, not here!

		float handle_size 	= HandleUtility.GetHandleSize( override_info.translate );		
		
		Matrix4x4 old_handles_mat = Handles.matrix;
		Matrix4x4 mat = new Matrix4x4();	
		mat.m00 = 1; mat.m11 = 1; mat.m22 = 1; mat.m33 = 1;
		
		mat.SetColumn( 3, new Vector4( 	override_info.translate.x, 
										override_info.translate.y, 
										override_info.translate.z, 1) );
		Handles.matrix = mat;
		
		const float scale_factor_tweak1 = 0.17f;
		const float scale_factor_tweak2 = 0.2f;
		
		float scale_factor = scale_factor_tweak1*handle_size;
		if(scale_factor < scale_factor_tweak1 )
			scale_factor = scale_factor_tweak1;
		
		scale_factor *= HoudiniHost.prPinSize;
			
		Handles.color = new Color( 1.0f, 1.0f, 1.0f, 1.0f );		
		
		
		const float vertical_scale = 14.0f;
		float pin_sphere_displacement = 14.0f*scale_factor*scale_factor_tweak2;
		
		mat = Handles.matrix;
		mat.m11 = vertical_scale;
		Handles.matrix = mat;
		
		Vector3 position = new Vector3( 0, 0, 0 );
		position.y = 0.5f*pin_sphere_displacement/vertical_scale;

#if UNITY_5_5_OR_NEWER
		Handles.CylinderHandleCap(	override_info.instancePointNumber,
									position,
									Quaternion.Euler( new Vector3( 90, 0, 0 ) ),
									scale_factor * scale_factor_tweak2,
									EventType.Repaint );
#else
		Handles.CylinderCap( override_info.instancePointNumber,
							 position,
							 Quaternion.Euler( new Vector3( 90, 0, 0)),
							 scale_factor*scale_factor_tweak2 );
#endif

		mat.m11 = 1.0f;
		Handles.matrix = mat;
		
		Handles.color = HoudiniHost.prPinColour;
		position.y = pin_sphere_displacement;
#if UNITY_5_5_OR_NEWER
		Handles.SphereHandleCap(	override_info.instancePointNumber,
									position,
									Quaternion.Euler( new Vector3( 90, 0, 0 ) ),
									scale_factor * 1.3f,
									EventType.Repaint );
#else
		Handles.SphereCap  ( override_info.instancePointNumber,
							 position,
							 Quaternion.Euler( new Vector3( 90, 0, 0)),
							 scale_factor*1.3f );
#endif

		Handles.matrix = old_handles_mat;
	}
#endif // UNITY_EDITOR

	private void cacheNumInstances()
	{
		var obj_idx = prAsset.findObjectByNodeId( prObjectId );
		HAPI_ObjectInfo object_info = prAsset.prObjects[ obj_idx ];
			
		// Get Detail info.
		HAPI_GeoInfo geo_info = HoudiniHost.getDisplayGeoInfo( prObjectId );
		if ( geo_info.partCount == 0 )
			return;

		HAPI_PartInfo part_info =  HoudiniHost.getPartInfo( geo_info.nodeId, 0 );
		if ( prAsset.prEnableLogging )
			Debug.Log( "Instancer #" + prObjectId + " (" + object_info.name + "): "
					   + "points: " + part_info.pointCount );
				
		if ( part_info.pointCount > 65000 )
			throw new HoudiniError( "Point count (" + part_info.pointCount + ") above limit (" + 65000 + ")!" );
		
		myNumInstances = part_info.pointCount;
	}
	
	public void beginBakeAnimation()
	{
		try
		{
			cacheNumInstances();
			
			myCurvesCollection = new HoudiniCurvesCollection[ myNumInstances ];
			for ( int ii = 0; ii < myNumInstances; ++ii )
			{
				myCurvesCollection[ ii ] = new HoudiniCurvesCollection();
				
			}
		}
		catch ( HoudiniError error )
		{
			Debug.LogWarning( error.ToString() );
			return;
		}
		
	}	
	
	public void bakeAnimation( float curr_time, GameObject parent_object )
	{
		try
		{
			HAPI_GeoInfo geo_info = HoudiniHost.getDisplayGeoInfo( prObjectId );

			HAPI_Transform[] instance_transforms = new HAPI_Transform[ myNumInstances ];
			Utility.getArray2Id(
				geo_info.nodeId, HAPI_RSTOrder.HAPI_SRT, 
				HoudiniHost.getInstanceTransforms, instance_transforms, myNumInstances );

			Matrix4x4 parent_xform_inverse = Matrix4x4.identity;

			if ( parent_object != null )
				parent_xform_inverse = parent_object.transform.localToWorldMatrix.inverse;

			for ( int ii = 0; ii < myNumInstances; ++ii )
			{
				Vector3 pos = new Vector3();

				// Apply object transforms.
				//
				// Axis and Rotation conversions:
				// Note that Houdini's X axis points in the opposite direction that Unity's does.  Also, Houdini's 
				// rotation is right handed, whereas Unity is left handed.  To account for this, we need to invert
				// the x coordinate of the translation, and do the same for the rotations (except for the x rotation,
				// which doesn't need to be flipped because the change in handedness AND direction of the left x axis
				// causes a double negative - yeah, I know).
				
				pos[ 0 ] = -instance_transforms[ ii ].position[ 0 ];
				pos[ 1 ] =  instance_transforms[ ii ].position[ 1 ];
				pos[ 2 ] =  instance_transforms[ ii ].position[ 2 ];
				
				Quaternion quat = new Quaternion( 	instance_transforms[ ii ].rotationQuaternion[ 0 ],
													instance_transforms[ ii ].rotationQuaternion[ 1 ],
													instance_transforms[ ii ].rotationQuaternion[ 2 ],
													instance_transforms[ ii ].rotationQuaternion[ 3 ] );
				
				Vector3 euler = quat.eulerAngles;
				euler.y = -euler.y;
				euler.z = -euler.z;
									
				quat = Quaternion.Euler( euler );
				
				Vector3 scale = new Vector3 ( instance_transforms[ ii ].scale[ 0 ],
											  instance_transforms[ ii ].scale[ 1 ],
											  instance_transforms[ ii ].scale[ 2 ] );
				
				if( parent_object != null )
				{
					
					Matrix4x4 world_mat = Matrix4x4.identity;
					world_mat.SetTRS( pos, quat, scale );					
					Matrix4x4 local_mat = parent_xform_inverse  * world_mat;					
					
					quat = HoudiniAssetUtility.getQuaternion( local_mat );
					scale = HoudiniAssetUtility.getScale( local_mat );
					pos = HoudiniAssetUtility.getPosition( local_mat );
				}
				
				HoudiniCurvesCollection curves = myCurvesCollection[ ii ];						
				
				HoudiniAssetUtility.addKeyToCurve( curr_time, pos[0], curves.tx );
				HoudiniAssetUtility.addKeyToCurve( curr_time, pos[1], curves.ty );
				HoudiniAssetUtility.addKeyToCurve( curr_time, pos[2], curves.tz );
				HoudiniAssetUtility.addKeyToCurve( curr_time, quat.x, curves.qx );
				HoudiniAssetUtility.addKeyToCurve( curr_time, quat.y, curves.qy );
				HoudiniAssetUtility.addKeyToCurve( curr_time, quat.z, curves.qz );
				HoudiniAssetUtility.addKeyToCurve( curr_time, quat.w, curves.qw );
				HoudiniAssetUtility.addKeyToCurve( curr_time, scale.x, curves.sx );
				HoudiniAssetUtility.addKeyToCurve( curr_time, scale.y, curves.sy );
				HoudiniAssetUtility.addKeyToCurve( curr_time, scale.z, curves.sz );
				
				
				
			}
			
		}
		catch ( HoudiniError error )
		{
			Debug.LogWarning( error.ToString() );
			return;
		}
	}

	private void getInstanceAndNameAttrs( out int[] instance_attr, out int[] name_attr )
	{
		instance_attr = new int[ 0 ];
		name_attr = new int[ 0 ];

		HAPI_GeoInfo geo_info = HoudiniHost.getDisplayGeoInfo( prObjectId );

		HAPI_AttributeInfo instance_attr_info = new HAPI_AttributeInfo( "instance" );
		Utility.getAttribute(
			geo_info.nodeId, 0, "instance", ref instance_attr_info,
			ref instance_attr, HoudiniHost.getAttributeStringData );

		if ( !instance_attr_info.exists )
			return;

		if ( instance_attr_info.exists && instance_attr_info.owner != HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
			throw new HoudiniErrorIgnorable( "I only understand instance as point attributes!" );
		
		if ( instance_attr_info.exists && instance_attr.Length != myNumInstances )
			throw new HoudiniError(
				"Unexpected instance_hint array length found for asset: " + prAsset.prAssetId + "!" );

		HAPI_AttributeInfo name_attr_info = new HAPI_AttributeInfo( "name" );
		Utility.getAttribute(
			geo_info.nodeId, 0, "name", ref name_attr_info,
			ref name_attr, HoudiniHost.getAttributeStringData );

		if ( name_attr_info.exists && name_attr_info.owner != HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
			throw new HoudiniErrorIgnorable( "I only understand name as point attributes!" );
		
		if ( name_attr_info.exists && name_attr.Length != myNumInstances )
			throw new HoudiniError(
				"Unexpected name array length found for asset: " + prAsset.prAssetId + "!" );
	}
	
	public bool endBakeAnimation( GameObject parent_object )
	{
		try
		{						
			bool found_non_const_curve = false;
			for ( int ii = 0; ii < myNumInstances; ++ii )
			{												
				GameObject child = transform.GetChild( ii ).gameObject;
																				
				HoudiniCurvesCollection curves = myCurvesCollection[ ii ];
				
				AnimationClip clip = curves.assignCurvesToClip();
				
				if( clip != null )
				{
					Animation anim_component = child.GetComponent< Animation >();
					if( anim_component == null )
					{
						child.AddComponent< Animation >();
						anim_component = child.GetComponent< Animation >();
					}
				
					anim_component.clip = clip;					
					found_non_const_curve = true;
				}								
			}
			
			return found_non_const_curve;
						
		}
		catch ( HoudiniError error )
		{
			Debug.LogWarning( error.ToString() );
			return false;
		}
	}

	private void updateUniqueInstantiatedNames( List < string > unique_instantiated_names )
	{
		HoudiniInstancerPersistentData instancer_data = prPersistentData;
		
		List< string > existing_unique_names = instancer_data.uniqueNames;
		List< string > names_to_add = new List< string >();

		for( int ii = 0; ii < unique_instantiated_names.Count; ii++ )
		{
			string unique_name = unique_instantiated_names[ ii ];
			bool found = false;
			for( int jj = 0; jj < existing_unique_names.Count; jj++ )
			{
				string existing_unique_name = existing_unique_names[ jj ];
				if( existing_unique_name == unique_name )
				{
					found = true;
					break;
				}
			}

			if( !found )
			{
				names_to_add.Add( unique_name );
			}
		}

		foreach( string name_to_add in names_to_add )
		{
			instancer_data.uniqueNames.Add( name_to_add );
			instancer_data.numObjsToInstantiate.Add( 1 );
			instancer_data.objsToInstantiate.Add( null );
			instancer_data.recalculateVariations.Add( true );
		}


	}
			
	public void instanceObjects( HoudiniProgressBar progress_bar )
	{
		try
		{
			destroyChildren();
			
			HAPI_ObjectInfo object_info = HoudiniHost.getObjectInfo( prObjectId );
			
			// Get Detail info.
			HAPI_GeoInfo geo_info = HoudiniHost.getDisplayGeoInfo( prObjectId );
			if ( geo_info.partCount == 0 )
				return;
			
			cacheNumInstances();

			HAPI_Transform[] instance_transforms = new HAPI_Transform[ myNumInstances ];
			Utility.getArray2Id(
				geo_info.nodeId, HAPI_RSTOrder.HAPI_SRT, 
				HoudiniHost.getInstanceTransforms, instance_transforms, myNumInstances );

			// Get scale point attributes.
			HAPI_AttributeInfo scale_attr_info = new HAPI_AttributeInfo( "scale" );
			float[] scale_attr = new float[ 0 ];
			Utility.getAttribute(
				geo_info.nodeId, 0, "scale", ref scale_attr_info, ref scale_attr, HoudiniHost.getAttributeFloatData );
			
			if ( scale_attr_info.exists && scale_attr_info.owner != HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
				throw new HoudiniErrorIgnorable( "I only understand scale as point attributes!" );
			
			if ( scale_attr_info.exists && scale_attr.Length != myNumInstances * 3 )
				throw new HoudiniError( 
					"Unexpected scale array length found for asset: " + prAsset.prAssetId + "!\n" +
					"Expected length of: " + myNumInstances * 3 + " but given: " + scale_attr.Length );

			HAPI_AttributeInfo script_attr_info = new HAPI_AttributeInfo( "Unity_Script" );
			int[] script_attr = new int[ 0 ];
			Utility.getAttribute(
				geo_info.nodeId, 0, "Unity_Script", ref script_attr_info,
				ref script_attr, HoudiniHost.getAttributeStringData );

			if ( script_attr_info.exists && script_attr_info.owner != HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
				throw new HoudiniErrorIgnorable( "I only understand Unity_Script as point attributes!" );

			if ( script_attr_info.exists && script_attr.Length != myNumInstances )
				throw new HoudiniError( "Unexpected Unity_Script array length found for asset: " + prAsset.prAssetId + "!" );

			int[] instance_attr = null;
			int[] name_attr = null;
			getInstanceAndNameAttrs(out instance_attr,out name_attr );
			
			progress_bar.prTotal = myNumInstances;
			
			List <int> exclusion_list = new List<int>();
			instanceOverriddenObjects( myNumInstances, exclusion_list );

			List < string > unique_instantiated_names = new List< string >();
			HoudiniInstancerPersistentData persistent_data = prPersistentData;
			if( persistent_data.variationChoice.Count != myNumInstances )
			{
				if( myNumInstances > persistent_data.variationChoice.Count )
				{
					int difference = myNumInstances - persistent_data.variationChoice.Count;
					for( int ii = 0; ii < difference; ii++ )
					{
						persistent_data.variationChoice.Add( -1 );
					}
				}
				else
				{
					int difference = persistent_data.variationChoice.Count - myNumInstances;
					persistent_data.variationChoice.RemoveRange(
						persistent_data.variationChoice.Count - difference, difference );
				}
			}

			bool liveTransformPropagationSetting = false;
			bool syncAssetTransformSetting = false;
			bool enableCooking = true;
			for ( int i = 0; i < myNumInstances; ++i )
			{
				if ( exclusion_list.Contains( i ) )
					continue;

				GameObject obj_to_instance = null;

				if ( instance_attr.Length > 0 || name_attr.Length > 0 )
				{
					if ( name_attr.Length > 0 )
					{
						string obj_name	= HoudiniHost.getString( name_attr[ i ] );
						int object_index = prAsset.findObjectByName( obj_name );

						if ( object_index >= 0 )
							obj_to_instance = prAsset.prGameObjects[ object_index ];
						else
							obj_to_instance = prAsset.findPartByName( obj_name, true );

						if ( obj_to_instance == null )
							obj_to_instance = GameObject.Find( obj_name );
					}
					else 
					{
						string instanceObjectPath = HoudiniHost.getString( instance_attr[ i ] );
						string [] pathItems = instanceObjectPath.Split( '/' );
						string instanceObjectName = pathItems[ pathItems.Length - 1 ];

						int objectIndex = prAsset.findObjectByName( instanceObjectName );
						if ( objectIndex >= 0 )
							obj_to_instance = prAsset.prGameObjects[ objectIndex ];
						else
							obj_to_instance = GameObject.Find( instanceObjectName );
					}

					if ( obj_to_instance != null )
					{
						HoudiniAsset hapi_asset = obj_to_instance.GetComponent< HoudiniAsset >();
						if ( hapi_asset != null )
						{
							liveTransformPropagationSetting				= hapi_asset.prTransformChangeTriggersCooks;
							syncAssetTransformSetting					= hapi_asset.prPushUnityTransformToHoudini;
							enableCooking								= hapi_asset.prEnableCooking;
							hapi_asset.prTransformChangeTriggersCooks	= false;
							hapi_asset.prPushUnityTransformToHoudini	= false;
							hapi_asset.prEnableCooking					= false;
						}
					}
				}
				else if ( object_info.objectToInstanceId >= 0 )
				{
					int object_to_instance_idx = prAsset.findObjectByNodeId( object_info.objectToInstanceId );
					if ( object_to_instance_idx >= 0 )
						obj_to_instance = prAsset.prGameObjects[ object_to_instance_idx ];
				}

				if ( obj_to_instance != null )
				{
					// Set progress bar information.
					progress_bar.prCurrentValue = i;
					progress_bar.prMessage = "Instancing: " + obj_to_instance.name + " (" + i + " of " + myNumInstances + ")";
					progress_bar.displayProgressBar();

					if ( !unique_instantiated_names.Contains( obj_to_instance.name ) )
					{
						unique_instantiated_names.Add( obj_to_instance.name );
					}
	
					Vector3 pos = new Vector3();
					
					// Apply object transforms.
					//
					// Axis and Rotation conversions:
					// Note that Houdini's X axis points in the opposite direction that Unity's does.  Also, Houdini's 
					// rotation is right handed, whereas Unity is left handed.  To account for this, we need to invert
					// the x coordinate of the translation, and do the same for the rotations (except for the x rotation,
					// which doesn't need to be flipped because the change in handedness AND direction of the left x axis
					// causes a double negative - yeah, I know).
					
					pos[ 0 ] = -instance_transforms[ i ].position[ 0 ];
					pos[ 1 ] =  instance_transforms[ i ].position[ 1 ];
					pos[ 2 ] =  instance_transforms[ i ].position[ 2 ];
					
					Quaternion quat = new Quaternion( 	instance_transforms[ i ].rotationQuaternion[ 0 ],
														instance_transforms[ i ].rotationQuaternion[ 1 ],
														instance_transforms[ i ].rotationQuaternion[ 2 ],
														instance_transforms[ i ].rotationQuaternion[ 3 ] );
					
					Vector3 euler = quat.eulerAngles;
					euler.y = -euler.y;
					euler.z = -euler.z;
					
					Vector3 scale = new Vector3 ( instance_transforms[ i ].scale[ 0 ],
												  instance_transforms[ i ].scale[ 1 ],
												  instance_transforms[ i ].scale[ 2 ] );
					
					
					
					Matrix4x4 local_mat = new Matrix4x4();
					local_mat.SetTRS( pos, Quaternion.Euler( euler ), scale );

					// TODO: Now this *should* be the transform.localToWorldMatrix
					// but for some reason, after a scene load, we pick up compensating
					// factors in the local transform that cancel out the transform on the
					// asset.  For now just use the asset's transform as the parent matrix.
					Matrix4x4 parent_mat = prAsset.transform.localToWorldMatrix;
					Matrix4x4 global_mat = parent_mat * local_mat;

					
					euler = HoudiniAssetUtility.getQuaternion( global_mat ).eulerAngles;
					pos = HoudiniAssetUtility.getPosition( global_mat );
					scale = HoudiniAssetUtility.getScale( global_mat );
					
						
					//mat.SetTRS( pos, 
					
					string script_to_attach = "";
					if( script_attr_info.exists )
						script_to_attach = HoudiniHost.getString( script_attr[ i ] );
					instanceObject( obj_to_instance, 
								 	pos,
									euler,									
								 	i,
									scale_attr_info.exists,
									scale,
									script_attr_info.exists,
									script_to_attach );
						

					HoudiniAsset hapi_asset = obj_to_instance.GetComponent< HoudiniAsset >();
					if ( hapi_asset != null )
					{
						hapi_asset.prTransformChangeTriggersCooks	= liveTransformPropagationSetting;
						hapi_asset.prPushUnityTransformToHoudini			= syncAssetTransformSetting;
						hapi_asset.prEnableCooking				= enableCooking;
					}

				}
			}

			updateUniqueInstantiatedNames( unique_instantiated_names );


		}
		catch ( HoudiniError error )
		{
			Debug.LogWarning( error.ToString() );
			return;
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private Members
	
	private void destroyChildren() 
	{
		List< GameObject > children = new List< GameObject >();
		
		foreach ( Transform child in transform )
			children.Add( child.gameObject );
		
		foreach ( GameObject child in children )
			DestroyImmediate( child );
	}

	
	[SerializeField] private HoudiniAsset myAsset;
	[SerializeField] private int myObjectId;
	[SerializeField] private int myNumInstances;

	
	private HoudiniCurvesCollection[] myCurvesCollection;
}
