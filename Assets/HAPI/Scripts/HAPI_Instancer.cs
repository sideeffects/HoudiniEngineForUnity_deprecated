using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using Utility = HAPI_AssetUtility;

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
}

public class HAPI_Instancer : MonoBehaviour {
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	
	public GameObject 	prObjToInstantiate { get { return myObjToInstantiate; }  set { myObjToInstantiate = value; } }
	public bool 		prOverrideInstances { get { return myOverrideInstances; } set { myOverrideInstances = value; } }
	public HAPI_Asset 	prAsset { get { return myAsset; } set { myAsset = value; } }
	public int 			prObjectId { get { return myObjectId; } set { myObjectId = value; } }
		
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods
	
	public HAPI_Instancer () 
	{
		prAsset = null;
		prOverrideInstances = false;
		prObjectId = -1;
		prObjToInstantiate = null;
		myNumInstances = 0;
		myCurvesCollection = null;
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
			instanceObject( object_to_instantiate, 
							pos, euler, override_info.instancePointNumber, true, scale );
			
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
								 Vector3 scale )
	{
		
		GameObject obj;
		
		if ( !prOverrideInstances )
		{
			obj = Instantiate( objToInstantiate, pos, Quaternion.Euler( euler ) ) as GameObject;
			
			HAPI_PartControl child_part_control = obj.GetComponentInChildren<HAPI_PartControl>();
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
			MeshRenderer[] mesh_renderers = obj.GetComponentsInChildren< MeshRenderer >();
			foreach ( MeshRenderer mesh_renderer in mesh_renderers )
				mesh_renderer.enabled = true;
		}
		else
		{
			obj = PrefabUtility.InstantiatePrefab( prObjToInstantiate ) as GameObject;
			if( obj == null )
			{
				bool liveTransformPropagationSetting	= false;
				bool syncAssetTransformSetting			= false;
				bool enableCooking						= true;
				
				HAPI_Asset hapi_asset = prObjToInstantiate.GetComponent< HAPI_Asset >();
				if( hapi_asset != null )
				{
					liveTransformPropagationSetting			= hapi_asset.prLiveTransformPropagation;
					syncAssetTransformSetting				= hapi_asset.prSyncAssetTransform;
					enableCooking							= hapi_asset.prEnableCooking;
					hapi_asset.prLiveTransformPropagation	= false;
					hapi_asset.prSyncAssetTransform			= false;
					hapi_asset.prEnableCooking				= false;
				}
				
				obj = Instantiate( prObjToInstantiate, new Vector3(0,0,0), Quaternion.identity ) as GameObject;
				
				if( hapi_asset != null )
				{
					hapi_asset.prLiveTransformPropagation	= liveTransformPropagationSetting;
					hapi_asset.prSyncAssetTransform			= syncAssetTransformSetting;
					hapi_asset.prEnableCooking				= enableCooking;
				}									
			}
			
			obj.transform.localPosition = pos;
			obj.transform.localRotation = Quaternion.Euler( euler );
			if( scale_exists )
				obj.transform.localScale = scale;
		}
		
		obj.transform.parent = transform;
		
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
	
	
	private void addKeyToCurve( float time, float val, AnimationCurve curve )
	{
		Keyframe curr_key = new Keyframe( time, val, 0, 0 );
		
		if( curve.length > 0 )
		{
			Keyframe prev_key = curve.keys[ curve.length - 1 ];
			float tangent = (val - prev_key.value) / (time - prev_key.time );
			prev_key.outTangent = tangent;
			curr_key.inTangent = tangent;
			
			curve.RemoveKey( curve.length - 1 );
			curve.AddKey( prev_key );			
		}
		
		curve.AddKey( curr_key );
		
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
			Utility.getArray4Id( prAsset.prAssetId, prObjectId, 0, (int) HAPI_RSTOrder.SRT, 
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
				
				addKeyToCurve( curr_time, pos[0], curves.tx );
				addKeyToCurve( curr_time, pos[1], curves.ty );
				addKeyToCurve( curr_time, pos[2], curves.tz );
				addKeyToCurve( curr_time, quat.x, curves.qx );
				addKeyToCurve( curr_time, quat.y, curves.qy );
				addKeyToCurve( curr_time, quat.z, curves.qz );
				addKeyToCurve( curr_time, quat.w, curves.qw );
				addKeyToCurve( curr_time, scale.x, curves.sx );
				addKeyToCurve( curr_time, scale.y, curves.sy );
				addKeyToCurve( curr_time, scale.z, curves.sz );
				
				
				
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
		
		if ( instance_attr_info.exists && instance_attr_info.owner != (int) HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
			throw new HAPI_ErrorIgnorable( "I only understand instance as point attributes!" );
		
		if ( instance_attr_info.exists && instance_attr.Length != myNumInstances )
			throw new HAPI_Error( "Unexpected instance_hint array length found for asset: " 
								  + prAsset.prAssetId + "!" );
				
		
		HAPI_AttributeInfo name_attr_info = new HAPI_AttributeInfo( "name" );		
		Utility.getAttribute( prAsset.prAssetId, prObjectId, 0, 0, "name", 
							  ref name_attr_info, ref name_attr, HAPI_Host.getAttributeStrData );
					
		
		if ( name_attr_info.exists && name_attr_info.owner != (int) HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
			throw new HAPI_ErrorIgnorable( "I only understand name as point attributes!" );
		
		if ( name_attr_info.exists && name_attr.Length != myNumInstances )
			throw new HAPI_Error( "Unexpected name array length found for asset: " 
								  + prAsset.prAssetId + "!" );
	}
	
	public void endBakeAnimation()
	{
		try
		{						
			
			for ( int ii = 0; ii < myNumInstances; ++ii )
			{												
				GameObject child = transform.GetChild( ii ).gameObject;
																				
				HAPI_CurvesCollection curves = myCurvesCollection[ ii ];
				
				child.AddComponent< Animation >();
				Animation anim_component = child.GetComponent< Animation >();
				AnimationClip clip = new AnimationClip();
				anim_component.clip = clip;																		
				
				clip.SetCurve( "", typeof(Transform), "localPosition.x", curves.tx );				
				clip.SetCurve( "", typeof(Transform), "localPosition.y", curves.ty );				
				clip.SetCurve( "", typeof(Transform), "localPosition.z", curves.tz );							
				clip.SetCurve( "", typeof(Transform), "localRotation.x", curves.qx );				
				clip.SetCurve( "", typeof(Transform), "localRotation.y", curves.qy );				
				clip.SetCurve( "", typeof(Transform), "localRotation.z", curves.qz );
				clip.SetCurve( "", typeof(Transform), "localRotation.w", curves.qw );								
				clip.SetCurve( "", typeof(Transform), "localScale.x", curves.sx );				
				clip.SetCurve( "", typeof(Transform), "localScale.y", curves.sy );				
				clip.SetCurve( "", typeof(Transform), "localScale.z", curves.sz );
	
				clip.EnsureQuaternionContinuity();												
				
			}
						
		}
		catch ( HAPI_Error error )
		{
			Debug.LogWarning( error.ToString() );
			return;
		}
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
			Utility.getArray4Id( prAsset.prAssetId, prObjectId, 0, (int) HAPI_RSTOrder.SRT, 
								 HAPI_Host.getInstanceTransforms, instance_transforms, myNumInstances );
			
			// Get scale point attributes.
			HAPI_AttributeInfo scale_attr_info = new HAPI_AttributeInfo( "scale" );
			float[] scale_attr = new float[ 0 ];
			Utility.getAttribute( prAsset.prAssetId, prObjectId, 0, 0, "scale",
								  ref scale_attr_info, ref scale_attr, HAPI_Host.getAttributeFloatData );
			
			if ( scale_attr_info.exists && scale_attr_info.owner != (int) HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
				throw new HAPI_ErrorIgnorable( "I only understand scale as point attributes!" );
			
			if ( scale_attr_info.exists && scale_attr.Length != myNumInstances * 3 )
				throw new HAPI_Error( "Unexpected scale array length found for asset: " + prAsset.prAssetId + "!" );
			
			
			int[] instance_attr = null;
			int[] name_attr = null;
			getInstanceAndNameAttrs(out instance_attr,out name_attr );
			
			progress_bar.prTotal = myNumInstances;
			
			List <int> exclusion_list = new List<int>();
			instanceOverriddenObjects( myNumInstances, exclusion_list );
			
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
						liveTransformPropagationSetting			= hapi_asset.prLiveTransformPropagation;
						syncAssetTransformSetting				= hapi_asset.prSyncAssetTransform;
						enableCooking							= hapi_asset.prEnableCooking;
						hapi_asset.prLiveTransformPropagation	= false;
						hapi_asset.prSyncAssetTransform			= false;
						hapi_asset.prEnableCooking				= false;
					}
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
							
					instanceObject( objToInstantiate, 
								 	pos,
									euler,									
								 	ii,
									scale_attr_info.exists,
									scale );
						
					if ( !prOverrideInstances )
					{
						HAPI_Asset hapi_asset = objToInstantiate.GetComponent< HAPI_Asset >();
						if ( hapi_asset != null )
						{
							hapi_asset.prLiveTransformPropagation	= liveTransformPropagationSetting;
							hapi_asset.prSyncAssetTransform			= syncAssetTransformSetting;
							hapi_asset.prEnableCooking				= enableCooking;
						}
					}
				}
			}
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
	[SerializeField] private GameObject myObjToInstantiate;
	[SerializeField] private bool myOverrideInstances;
	[SerializeField] private int myObjectId;
	[SerializeField] private int myNumInstances;
	
	private HAPI_CurvesCollection[] myCurvesCollection;
	
}
