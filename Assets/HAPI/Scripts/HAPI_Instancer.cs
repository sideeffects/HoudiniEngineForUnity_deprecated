using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using Utility = HAPI_AssetUtility;

using HAPI;

public class HAPI_InstancerOverrideInfo
{
	public HAPI_TransformEuler xform = new HAPI_TransformEuler( true );
	public GameObject objectToInstantiate = null;
	public int instancePointNumber = -1;
}

public class HAPI_Instancer : MonoBehaviour {
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	
	public GameObject 	prObjToInstantiate { get; set; }
	public bool 		prOverrideInstances { get; set; }
	public HAPI_Asset 	prAsset { get; set; }
	public int 			prObjectId { get; set; }
	
	public List< HAPI_InstancerOverrideInfo > prOverriddenInstances { get; set; }
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods
	
	public HAPI_Instancer () 
	{
		prAsset = null;
		prOverrideInstances = false;
		prObjectId = -1;
		prOverriddenInstances = new List< HAPI_InstancerOverrideInfo >();
	}
	
	
	private void instanceOverriddenObjects( int total_points, List< int > exclusion_list )
	{
		int current_max_point_index = total_points - 1;
		foreach ( HAPI_InstancerOverrideInfo override_info in prOverriddenInstances )
		{
			
			Vector3 pos = new Vector3( override_info.xform.position[ 0 ],
									   override_info.xform.position[ 1 ],
									   override_info.xform.position[ 2 ] );
			
			Vector3 euler = new Vector3 ( override_info.xform.rotationEuler[ 0 ],
										  override_info.xform.rotationEuler[ 1 ],
										  override_info.xform.rotationEuler[ 2 ] );
			
			Vector3 scale = new Vector3 ( override_info.xform.scale[ 0 ],
										  override_info.xform.scale[ 1 ],
										  override_info.xform.scale[ 2 ] );
			
			instanceObject( override_info.objectToInstantiate, 
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
	
	public bool isPointOverridden( int point_index )
	{
		foreach ( HAPI_InstancerOverrideInfo override_info in prOverriddenInstances )
		{
			if( override_info.instancePointNumber == point_index )
				return true;
		}
		
		return false;
	}
	
	public bool pinInstance( HAPI_InstancerOverrideInfo info )
	{
		foreach ( HAPI_InstancerOverrideInfo override_info in prOverriddenInstances )
		{
			if( override_info.instancePointNumber == info.instancePointNumber )
				return false;
		}
		
		prOverriddenInstances.Add( info );
		return true;
		
	}
	
	public void unPinInstance( int point_index )
	{
		int index_to_remove = -1;
		for( int ii = 0 ; ii < prOverriddenInstances.Count ; ii++ )
		{
			HAPI_InstancerOverrideInfo override_info = prOverriddenInstances[ ii ];
			if( override_info.instancePointNumber == point_index )
			{
				index_to_remove = ii;
				break;
			}			
		}
		
		if( index_to_remove >= 0 )
		{
			prOverriddenInstances.RemoveAt( index_to_remove );
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
			
			HAPI_PartInfo part_info = new HAPI_PartInfo();
			HAPI_Host.getPartInfo( prAsset.prAssetId, prObjectId, 0, 0, out part_info );
			if ( prAsset.prEnableLogging )
				Debug.Log( "Instancer #" + prObjectId + " (" + object_info.name + "): "
						   + "points: " + part_info.pointCount );
					
			if ( part_info.pointCount > 65000 )
				throw new HAPI_Error( "Point count (" + part_info.pointCount + ") above limit (" + 65000 + ")!" );
											
			HAPI_Transform[] instance_transforms = new HAPI_Transform[ part_info.pointCount ];
			Utility.getArray4Id( prAsset.prAssetId, prObjectId, 0, (int) HAPI_RSTOrder.SRT, 
								 HAPI_Host.getInstanceTransforms, instance_transforms, part_info.pointCount );
			
			// Get scale point attributes.
			HAPI_AttributeInfo scale_attr_info = new HAPI_AttributeInfo( "scale" );
			float[] scale_attr = new float[ 0 ];
			Utility.getAttribute( prAsset.prAssetId, prObjectId, 0, 0, "scale",
								  ref scale_attr_info, ref scale_attr, HAPI_Host.getAttributeFloatData );
			
			if ( scale_attr_info.exists && scale_attr_info.owner != (int) HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
				throw new HAPI_ErrorIgnorable( "I only understand scale as point attributes!" );
			
			if ( scale_attr_info.exists && scale_attr.Length != part_info.pointCount * 3 )
				throw new HAPI_Error( "Unexpected scale array length found for asset: " + prAsset.prAssetId + "!" );
			
			HAPI_AttributeInfo instance_attr_info = new HAPI_AttributeInfo( "instance" );
			int[] instance_attr = new int[ 0 ];
			Utility.getAttribute( prAsset.prAssetId, prObjectId, 0, 0, "instance", 
								  ref instance_attr_info, ref instance_attr, HAPI_Host.getAttributeStrData );
			
			if ( instance_attr_info.exists && instance_attr_info.owner != (int) HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
				throw new HAPI_ErrorIgnorable( "I only understand instance as point attributes!" );
			
			if ( instance_attr_info.exists && instance_attr.Length != part_info.pointCount )
				throw new HAPI_Error( "Unexpected instance_hint array length found for asset: " 
									  + prAsset.prAssetId + "!" );
			
			progress_bar.prTotal = part_info.pointCount;
			
			List <int> exclusion_list = new List<int>();
			instanceOverriddenObjects( part_info.pointCount, exclusion_list );
			
			bool liveTransformPropagationSetting	= false;
			bool syncAssetTransformSetting			= false;
			bool enableCooking						= true;
			for ( int ii = 0; ii < part_info.pointCount; ++ii )
			{
				if ( exclusion_list.Contains( ii ) )
					continue;
				
				GameObject objToInstantiate = null;
				
				if ( object_info.objectToInstanceId >= 0 )
					objToInstantiate = prAsset.prGameObjects[ object_info.objectToInstanceId ];
				else if ( instance_attr_info.exists )
				{

					string instanceObjectPath	= HAPI_Host.getString( instance_attr[ ii ] );
					string[] pathItems			= instanceObjectPath.Split('/');
					string instanceObjectName	= pathItems[ pathItems.Length - 1 ];
													
					int objectIndex = prAsset.findObjectByName( instanceObjectName );
					if ( objectIndex >= 0 )
						objToInstantiate = prAsset.prGameObjects[ objectIndex ];
					else
						objToInstantiate = GameObject.Find( instanceObjectName );
					
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
				progress_bar.prMessage = "Instancing: " + objToInstantiate.name + " (" + ii + " of " + part_info.pointCount + ")";
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
	
}
