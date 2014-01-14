using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using Utility = HAPI_AssetUtility;
using System;

using HAPI;


public class HAPI_CurvesCollection
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
		HAPI_CurvesCollection curves = this;
						
		AnimationClip clip = new AnimationClip();			
		
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

public class HAPI_Instancer : MonoBehaviour 
{
#if UNITY_EDITOR	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties

	public HAPI_Asset 	prAsset { get { return myAsset; } set { myAsset = value; } }
	public int 			prObjectId { get { return myObjectId; } set { myObjectId = value; } }
	public List< GameObject >   prObjsToInstantiate { get { return myObjsToInstantiate; } } 	
	public List< string > prUniqueInstantiatedNames { get { return myUniqueInstantiatedNames; } }
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods
	
	public HAPI_Instancer () 
	{
		prAsset = null;
		prObjectId = -1;
		myNumInstances = 0;
		myCurvesCollection = null;
		myObjsToInstantiate = new List< GameObject >();
		myUniqueInstantiatedNames = new List< string >();
	}
	
	
	private void instanceOverriddenObjects( int total_points, List< int > exclusion_list )
	{		
		int current_max_point_index = total_points - 1;
		foreach ( HAPI_InstancerOverrideInfo override_info in prAsset.prOverriddenInstances )
		{
			
			Vector3 pos = override_info.translate;
			
			Vector3 euler = override_info.rotate;
			
			Vector3 scale = override_info.scale;
			
			GameObject object_to_instantiate = GameObject.Find( override_info.objectToInstantiatePath );
			
			//FIXME: we need to cache the scripts currently attached as well as the other info 
			// for now this is not preserved.
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
	

	public GameObject getUserObjToInstantiateFromName( string name )
	{
		for( int ii = 0; ii < prUniqueInstantiatedNames.Count; ii++ )
		{
			if( prUniqueInstantiatedNames[ ii ] == name )
			{
				return prObjsToInstantiate[ ii ];
			}
		}
		return null;
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
		
		GameObject obj;

		GameObject user_instance = getUserObjToInstantiateFromName( objToInstantiate.name );
		if ( user_instance == null )
		{
			obj = Instantiate( objToInstantiate, pos, Quaternion.Euler( euler ) ) as GameObject;
			
			HAPI_PartControl child_part_control = obj.GetComponentInChildren< HAPI_PartControl >();
			
			if ( child_part_control == null )
			{
				Debug.LogError( "No child_part_control on instantiated object (name: " + obj.name + ")" );
				return;
			}
			
			child_part_control.prInstancePointNumber = point_index;
			child_part_control.prObjectToInstantiate = objToInstantiate;
			
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
		else
		{
			obj = PrefabUtility.InstantiatePrefab( user_instance ) as GameObject;
			if( obj == null )
			{
				bool liveTransformPropagationSetting	= false;
				bool syncAssetTransformSetting			= false;
				bool enableCooking						= true;
				
				HAPI_Asset hapi_asset = user_instance.GetComponent< HAPI_Asset >();
				if( hapi_asset != null )
				{
					liveTransformPropagationSetting			= hapi_asset.prTransformChangeTriggersCooks;
					syncAssetTransformSetting				= hapi_asset.prPushUnityTransformToHoudini;
					enableCooking							= hapi_asset.prEnableCooking;
					hapi_asset.prTransformChangeTriggersCooks	= false;
					hapi_asset.prPushUnityTransformToHoudini			= false;
					hapi_asset.prEnableCooking				= false;
				}
				
				obj = Instantiate( user_instance, new Vector3(0,0,0), Quaternion.identity ) as GameObject;
				
				if( hapi_asset != null )
				{
					hapi_asset.prTransformChangeTriggersCooks	= liveTransformPropagationSetting;
					hapi_asset.prPushUnityTransformToHoudini			= syncAssetTransformSetting;
					hapi_asset.prEnableCooking				= enableCooking;
				}									
			}
			
			obj.transform.localPosition = pos;
			obj.transform.localRotation = Quaternion.Euler( euler );
			if( scale_exists )
				obj.transform.localScale = scale;
		}
					
		obj.transform.parent = transform;
		
		if ( attach_script_exists )
		{
			HAPI_AssetUtility.attachScript( obj, attach_script );			
		}
		
	}
	
	public bool hasOverriddenInstances()
	{
		return prAsset.prOverriddenInstances.Count > 0;
	}
	
	public bool isPointOverridden( int point_index )
	{
		foreach ( HAPI_InstancerOverrideInfo override_info in prAsset.prOverriddenInstances )
		{
			if( override_info.instancePointNumber == point_index )
				return true;
		}
		
		return false;
	}
	
	public bool pinInstance( HAPI_InstancerOverrideInfo info )
	{
		int ii = 0;
		for( ii = 0; ii < prAsset.prOverriddenInstances.Count; ii++ )
		{
			HAPI_InstancerOverrideInfo override_info = prAsset.prOverriddenInstances[ ii ];
			if( override_info.instancePointNumber == info.instancePointNumber )
			{
				prAsset.prOverriddenInstances.RemoveAt( ii );
				break;
			}
		}
		
		prAsset.prOverriddenInstances.Add( info );
		return true;
		
	}
	
	public void unPinAllInstances()
	{
		prAsset.prOverriddenInstances.Clear();		
	}
	
	public void unPinInstance( int point_index )
	{
		int index_to_remove = -1;
		for( int ii = 0 ; ii < prAsset.prOverriddenInstances.Count ; ii++ )
		{
			HAPI_InstancerOverrideInfo override_info = prAsset.prOverriddenInstances[ ii ];
			if( override_info.instancePointNumber == point_index )
			{
				index_to_remove = ii;
				break;
			}			
		}
		
		if( index_to_remove >= 0 )
		{
			prAsset.prOverriddenInstances.RemoveAt( index_to_remove );
		}
	}
	
	public void drawAllPins()
	{
		foreach ( HAPI_InstancerOverrideInfo override_info in prAsset.prOverriddenInstances )
		{
			drawPin( override_info );
						
		}
	}
	
	public void drawPin( int point_index )
	{
		foreach ( HAPI_InstancerOverrideInfo override_info in prAsset.prOverriddenInstances )
		{
			if( override_info.instancePointNumber == point_index )
				drawPin( override_info );
		}				
	}
	
	public void drawPin( HAPI_InstancerOverrideInfo override_info )
	{
		
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
		
		scale_factor *= HAPI_Host.prPinSize;
			
		Handles.color = new Color( 1.0f, 1.0f, 1.0f, 1.0f );		
		
		
		const float vertical_scale = 14.0f;
		float pin_sphere_displacement = 14.0f*scale_factor*scale_factor_tweak2;
		
		mat = Handles.matrix;
		mat.m11 = vertical_scale;
		Handles.matrix = mat;
		
		Vector3 position = new Vector3( 0, 0, 0 );
		position.y = 0.5f*pin_sphere_displacement/vertical_scale;
		
		Handles.CylinderCap( override_info.instancePointNumber,
							 position,
							 Quaternion.Euler( new Vector3( 90, 0, 0)),
							 scale_factor*scale_factor_tweak2 );
		
		
		mat.m11 = 1.0f;
		Handles.matrix = mat;
		
		Handles.color = HAPI_Host.prPinColour;	
		position.y = pin_sphere_displacement;
		Handles.SphereCap  ( override_info.instancePointNumber,
							 position,
							 Quaternion.Euler( new Vector3( 90, 0, 0)),
							 scale_factor*1.3f );
		
		Handles.matrix = old_handles_mat;
		
	}
	
	
	private void cacheNumInstances()
	{
		HAPI_ObjectInfo object_info = prAsset.prObjects[ prObjectId ];
			
		// Get Detail info.
		HAPI_GeoInfo geo_info = new HAPI_GeoInfo();
		HAPI_Host.getGeoInfo( prAsset.prAssetId, prObjectId, 0, out geo_info );
		if ( geo_info.partCount == 0 )
			return;
		
		HAPI_PartInfo part_info = new HAPI_PartInfo();
		HAPI_Host.getPartInfo( prAsset.prAssetId, prObjectId, 0, 0, out part_info );
		if ( prAsset.prEnableLogging )
			Debug.Log( "Instancer #" + prObjectId + " (" + object_info.name + "): "
					   + "points: " + part_info.pointCount );
				
		if ( part_info.pointCount > 65000 )
			throw new HAPI_Error( "Point count (" + part_info.pointCount + ") above limit (" + 65000 + ")!" );
		
		myNumInstances = part_info.pointCount;
	}
	
	public void beginBakeAnimation()
	{
		try
		{
			cacheNumInstances();
			
			myCurvesCollection = new HAPI_CurvesCollection[ myNumInstances ];
			for ( int ii = 0; ii < myNumInstances; ++ii )
			{
				myCurvesCollection[ ii ] = new HAPI_CurvesCollection();
				
			}
		}
		catch ( HAPI_Error error )
		{
			Debug.LogWarning( error.ToString() );
			return;
		}
		
	}	
	
	public void bakeAnimation( float curr_time, GameObject parent_object )
	{
		
		try
		{
																			
			HAPI_Transform[] instance_transforms = new HAPI_Transform[ myNumInstances ];
			Utility.getArray4Id( prAsset.prAssetId, prObjectId, 0, HAPI_RSTOrder.HAPI_SRT, 
								 HAPI_Host.getInstanceTransforms, instance_transforms, myNumInstances );
											
						
			Matrix4x4 parent_xform_inverse = Matrix4x4.identity;
				
			if( parent_object != null )
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
					
					quat = HAPI_AssetUtility.getQuaternion( local_mat );
					scale = HAPI_AssetUtility.getScale( local_mat );
					pos = HAPI_AssetUtility.getPosition( local_mat );
				}
				
				HAPI_CurvesCollection curves = myCurvesCollection[ ii ];						
				
				HAPI_AssetUtility.addKeyToCurve( curr_time, pos[0], curves.tx );
				HAPI_AssetUtility.addKeyToCurve( curr_time, pos[1], curves.ty );
				HAPI_AssetUtility.addKeyToCurve( curr_time, pos[2], curves.tz );
				HAPI_AssetUtility.addKeyToCurve( curr_time, quat.x, curves.qx );
				HAPI_AssetUtility.addKeyToCurve( curr_time, quat.y, curves.qy );
				HAPI_AssetUtility.addKeyToCurve( curr_time, quat.z, curves.qz );
				HAPI_AssetUtility.addKeyToCurve( curr_time, quat.w, curves.qw );
				HAPI_AssetUtility.addKeyToCurve( curr_time, scale.x, curves.sx );
				HAPI_AssetUtility.addKeyToCurve( curr_time, scale.y, curves.sy );
				HAPI_AssetUtility.addKeyToCurve( curr_time, scale.z, curves.sz );
				
				
				
			}
			
		}
		catch ( HAPI_Error error )
		{
			Debug.LogWarning( error.ToString() );
			return;
		}
	}
	
		
	
	private void getInstanceAndNameAttrs(out int[] instance_attr,out int[] name_attr )
	{
		
		instance_attr = new int[ 0 ];
		name_attr = new int[ 0 ];
		
		HAPI_AttributeInfo instance_attr_info = new HAPI_AttributeInfo( "instance" );		
		Utility.getAttribute( prAsset.prAssetId, prObjectId, 0, 0, "instance", 
							  ref instance_attr_info, ref instance_attr, HAPI_Host.getAttributeStrData );
		
		if ( !instance_attr_info.exists )
			return;
		
		if ( instance_attr_info.exists && instance_attr_info.owner != HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
			throw new HAPI_ErrorIgnorable( "I only understand instance as point attributes!" );
		
		if ( instance_attr_info.exists && instance_attr.Length != myNumInstances )
			throw new HAPI_Error( "Unexpected instance_hint array length found for asset: " 
								  + prAsset.prAssetId + "!" );
				
		
		HAPI_AttributeInfo name_attr_info = new HAPI_AttributeInfo( "name" );		
		Utility.getAttribute( prAsset.prAssetId, prObjectId, 0, 0, "name", 
							  ref name_attr_info, ref name_attr, HAPI_Host.getAttributeStrData );
					
		
		if ( name_attr_info.exists && name_attr_info.owner != HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
			throw new HAPI_ErrorIgnorable( "I only understand name as point attributes!" );
		
		if ( name_attr_info.exists && name_attr.Length != myNumInstances )
			throw new HAPI_Error( "Unexpected name array length found for asset: " 
								  + prAsset.prAssetId + "!" );
	}
	
	public bool endBakeAnimation( GameObject parent_object )
	{
		try
		{						
			bool found_non_const_curve = false;
			for ( int ii = 0; ii < myNumInstances; ++ii )
			{												
				GameObject child = transform.GetChild( ii ).gameObject;
																				
				HAPI_CurvesCollection curves = myCurvesCollection[ ii ];
				
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
		catch ( HAPI_Error error )
		{
			Debug.LogWarning( error.ToString() );
			return false;
		}
	}

	private void updateUniqueInstantiatedNames( List < string > unique_instantiated_names )
	{
		List < GameObject > objs_to_instantiate = new List< GameObject >();

		for( int ii = 0; ii < unique_instantiated_names.Count; ii++ )
		{
			string unique_name = unique_instantiated_names[ ii ];
			GameObject obj = null;
			for( int jj = 0; jj < prUniqueInstantiatedNames.Count; jj++ )
			{
				string existing_unique_name = prUniqueInstantiatedNames[ jj ];
				if( existing_unique_name == unique_name )
				{
					obj = prObjsToInstantiate[ jj ];
					break;
				}
			}

			objs_to_instantiate.Add( obj );
		}

		myUniqueInstantiatedNames = unique_instantiated_names;
		myObjsToInstantiate = objs_to_instantiate;

	}
			
	public void instanceObjects( HAPI_ProgressBar progress_bar )
	{
		try
		{
			destroyChildren();
			
			HAPI_ObjectInfo object_info = prAsset.prObjects[ prObjectId ];
			
			// Get Detail info.
			HAPI_GeoInfo geo_info = new HAPI_GeoInfo();
			HAPI_Host.getGeoInfo( prAsset.prAssetId, prObjectId, 0, out geo_info );
			if ( geo_info.partCount == 0 )
				return;
			
			cacheNumInstances();
											
			HAPI_Transform[] instance_transforms = new HAPI_Transform[ myNumInstances ];
			Utility.getArray4Id( prAsset.prAssetId, prObjectId, 0, HAPI_RSTOrder.HAPI_SRT, 
								 HAPI_Host.getInstanceTransforms, instance_transforms, myNumInstances );
			
			// Get scale point attributes.
			HAPI_AttributeInfo scale_attr_info = new HAPI_AttributeInfo( "scale" );
			float[] scale_attr = new float[ 0 ];
			Utility.getAttribute( prAsset.prAssetId, prObjectId, 0, 0, "scale",
								  ref scale_attr_info, ref scale_attr, HAPI_Host.getAttributeFloatData );
			
			if ( scale_attr_info.exists && scale_attr_info.owner != HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
				throw new HAPI_ErrorIgnorable( "I only understand scale as point attributes!" );
			
			if ( scale_attr_info.exists && scale_attr.Length != myNumInstances * 3 )
				throw new HAPI_Error( 
					"Unexpected scale array length found for asset: " + prAsset.prAssetId + "!\n" +
					"Expected length of: " + myNumInstances * 3 + " but given: " + scale_attr.Length );
			
						
			HAPI_AttributeInfo script_attr_info = new HAPI_AttributeInfo( "Unity_Script" );
			int[] script_attr = new int[ 0 ];
			Utility.getAttribute( prAsset.prAssetId, prObjectId, 0, 0, "Unity_Script",
								  ref script_attr_info, ref script_attr, HAPI_Host.getAttributeStrData );
			
			if ( script_attr_info.exists && script_attr_info.owner != HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
				throw new HAPI_ErrorIgnorable( "I only understand Unity_Script as point attributes!" );
			
			if ( script_attr_info.exists && script_attr.Length != myNumInstances )
				throw new HAPI_Error( "Unexpected Unity_Script array length found for asset: " + prAsset.prAssetId + "!" );
						
			
			int[] instance_attr = null;
			int[] name_attr = null;
			getInstanceAndNameAttrs(out instance_attr,out name_attr );
			
			progress_bar.prTotal = myNumInstances;
			
			List <int> exclusion_list = new List<int>();
			instanceOverriddenObjects( myNumInstances, exclusion_list );

			List < string > unique_instantiated_names = new List< string >();

			bool liveTransformPropagationSetting	= false;
			bool syncAssetTransformSetting			= false;
			bool enableCooking						= true;
			for ( int ii = 0; ii < myNumInstances; ++ii )
			{
				if ( exclusion_list.Contains( ii ) )
					continue;
				
				GameObject objToInstantiate = null;
				
				if ( object_info.objectToInstanceId >= 0 )
					objToInstantiate = prAsset.prGameObjects[ object_info.objectToInstanceId ];
				else if ( instance_attr.Length > 0 || name_attr.Length > 0 )
				{					
					if( name_attr.Length > 0 )
					{
						string obj_name	= HAPI_Host.getString( name_attr[ ii ] );
						int object_index = prAsset.findObjectByName( obj_name );
						if ( object_index >= 0 )
						{
							objToInstantiate = prAsset.prGameObjects[ object_index ];
						}
						else
						{
							objToInstantiate = prAsset.findPartByName( obj_name, true );
						}

						if( objToInstantiate == null )
						{
							objToInstantiate = GameObject.Find( obj_name );
						}
					}
					else 
					{
						string instanceObjectPath	= HAPI_Host.getString( instance_attr[ ii ] );
						string[] pathItems			= instanceObjectPath.Split('/');
						string instanceObjectName	= pathItems[ pathItems.Length - 1 ];
																																		
						int objectIndex = prAsset.findObjectByName( instanceObjectName );
						if ( objectIndex >= 0 )
							objToInstantiate = prAsset.prGameObjects[ objectIndex ];
						else
							objToInstantiate = GameObject.Find( instanceObjectName );
					}
					
					HAPI_Asset hapi_asset = objToInstantiate.GetComponent< HAPI_Asset >();
					if ( hapi_asset != null )
					{
						liveTransformPropagationSetting			= hapi_asset.prTransformChangeTriggersCooks;
						syncAssetTransformSetting				= hapi_asset.prPushUnityTransformToHoudini;
						enableCooking							= hapi_asset.prEnableCooking;
						hapi_asset.prTransformChangeTriggersCooks	= false;
						hapi_asset.prPushUnityTransformToHoudini			= false;
						hapi_asset.prEnableCooking				= false;
					}
				}

				if( !unique_instantiated_names.Contains( objToInstantiate.name ) )
				{
					unique_instantiated_names.Add( objToInstantiate.name );
				}

				// Set progress bar information.
				progress_bar.prCurrentValue = ii;
				progress_bar.prMessage = "Instancing: " + objToInstantiate.name + " (" + ii + " of " + myNumInstances + ")";
				progress_bar.displayProgressBar();
				
				if ( objToInstantiate != null )
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
					
					Vector3 scale = new Vector3 ( instance_transforms[ ii ].scale[ 0 ],
												  instance_transforms[ ii ].scale[ 1 ],
												  instance_transforms[ ii ].scale[ 2 ] );
					
					
					
					Matrix4x4 local_mat = new Matrix4x4();
					local_mat.SetTRS( pos, Quaternion.Euler( euler ), scale );

					// TODO: Now this *should* be the transform.localToWorldMatrix
					// but for some reason, after a scene load, we pick up compensating
					// factors in the local transform that cancel out the transform on the
					// asset.  For now just use the asset's transform as the parent matrix.
					Matrix4x4 parent_mat = prAsset.transform.localToWorldMatrix;
					Matrix4x4 global_mat = parent_mat * local_mat;

					
					euler = HAPI_AssetUtility.getQuaternion( global_mat ).eulerAngles;
					pos = HAPI_AssetUtility.getPosition( global_mat );
					scale = HAPI_AssetUtility.getScale( global_mat );
					
						
					//mat.SetTRS( pos, 
					
					string script_to_attach = "";
					if( script_attr_info.exists )
						script_to_attach = HAPI_Host.getString( script_attr[ ii ] );
					instanceObject( objToInstantiate, 
								 	pos,
									euler,									
								 	ii,
									scale_attr_info.exists,
									scale,
									script_attr_info.exists,
									script_to_attach );
						

					HAPI_Asset hapi_asset = objToInstantiate.GetComponent< HAPI_Asset >();
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
		catch ( HAPI_Error error )
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
	
	
	[SerializeField] private HAPI_Asset myAsset;
	[SerializeField] private int myObjectId;
	[SerializeField] private int myNumInstances;
	[SerializeField] private List< GameObject > myObjsToInstantiate;
	[SerializeField] private List< string > myUniqueInstantiatedNames;

	
	private HAPI_CurvesCollection[] myCurvesCollection;
#endif // UNITY_EDITOR
	
}
