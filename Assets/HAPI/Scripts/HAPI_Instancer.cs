using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using Utility = HAPI_AssetUtility;

using HAPI;

public class HAPI_Instancer : MonoBehaviour {
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	
	public GameObject 	prObjToInstantiate { get; set; }
	public bool 		prOverrideInstances { get; set; }
	public HAPI_Asset 	prAsset { get; set; }
	public int 			prObjectId { get; set; }
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods
	
	public HAPI_Instancer () 
	{
		prAsset = null;
		prOverrideInstances = false;
		prObjectId = -1;
	}
	
	public void instanceObjects( )
	{
		destroyChildren();
		
		HAPI_ObjectInfo object_info = prAsset.prObjects[ prObjectId ];
		
		// Get Detail info.
		HAPI_GeoInfo geo_info = new HAPI_GeoInfo();
		HAPI_Host.getGeoInfo( prAsset.prAssetId, prObjectId, 0, out geo_info );
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
			throw new HAPI_Error( "I only understand up as point attributes!" );
		
		if ( scale_attr_info.exists && scale_attr.Length != part_info.pointCount * 3 )
			throw new HAPI_Error( "Unexpected up array length found for asset: " + prAsset.prAssetId + "!" );
				
		// Get string point attributes.
		/*HAPI_AttributeInfo instancehint_attr_info = new HAPI_AttributeInfo( "instance_hint" );
		int[] instancehint_attr = new int[ 0 ];
		prObjectControl.getAttribute( prObjectControl.prAssetId, prObjectId, "instance_hint", 
					ref instancehint_attr_info, ref instancehint_attr, HAPI_Host.getAttributeStrData );
		if ( !instancehint_attr_info.exists )
			throw new HAPI_Error( "No instance_hint attribute found." );
		else if ( instancehint_attr_info.owner != (int) HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
			throw new HAPI_Error( "I only understand instance_hint as point attributes!" );
		
		if( instancehint_attr.Length != geo_info.pointCount )
		{
			throw new HAPI_Error( "Unexpected instance_hint array length found for asset: " 
								+ prObjectControl.prAssetId + "!" );
		}*/	
		
		HAPI_AttributeInfo instance_attr_info = new HAPI_AttributeInfo( "instance" );
		int[] instance_attr = new int[ 0 ];
		Utility.getAttribute( prAsset.prAssetId, prObjectId, 0, 0, "instance", 
							  ref instance_attr_info, ref instance_attr, HAPI_Host.getAttributeStrData );
		
		if ( instance_attr_info.exists && instance_attr_info.owner != (int) HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
			throw new HAPI_Error( "I only understand instance as point attributes!" );
		
		if ( instance_attr_info.exists && instance_attr.Length != part_info.pointCount )
			throw new HAPI_Error( "Unexpected instance_hint array length found for asset: " 
								  + prAsset.prAssetId + "!" );
		
		for (int ii = 0; ii < part_info.pointCount; ++ii )
		{
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
			}
			
			//string instance_hint = HAPI_Host.getString( instancehint_attr[ ii ] );
			//Debug.Log( "instance hint: " + instance_hint );
			
			//GameObject obj = PrefabUtility.InstantiatePrefab( prGameObjects[prObjectId] ) as GameObject;	
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
				
				pos[0] = -instance_transforms[ ii ].position[0];
				pos[1] = instance_transforms[ ii ].position[1];
				pos[2] = instance_transforms[ ii ].position[2];
				
				Quaternion quat = new Quaternion( 	instance_transforms[ ii ].rotationQuaternion[ 0 ],
													instance_transforms[ ii ].rotationQuaternion[ 1 ],
													instance_transforms[ ii ].rotationQuaternion[ 2 ],
													instance_transforms[ ii ].rotationQuaternion[ 3 ] );
				
				Vector3 euler = quat.eulerAngles;
				euler.y = -euler.y;
				euler.z = -euler.z;
				
				GameObject obj;
				
				if ( !prOverrideInstances )
				{
					obj = Instantiate( objToInstantiate, pos, Quaternion.Euler( euler ) ) as GameObject;
					if( scale_attr_info.exists )
						obj.transform.localScale = new Vector3( instance_transforms[ ii ].scale[0], 
															instance_transforms[ ii ].scale[1], 
															instance_transforms[ ii ].scale[2] );
				}
				else
				{
					obj = PrefabUtility.InstantiatePrefab( prObjToInstantiate ) as GameObject;
					obj.transform.localPosition = pos;
					obj.transform.localRotation = Quaternion.Euler( euler );
					if( scale_attr_info.exists )
						obj.transform.localScale = new Vector3( instance_transforms[ ii ].scale[ 0 ], 
															instance_transforms[ ii ].scale[ 1 ], 
															instance_transforms[ ii ].scale[ 2 ] );
				}
				
				obj.transform.parent = transform;
				
				HAPI_ChildSelectionControl selection_control = obj.GetComponent< HAPI_ChildSelectionControl >();
				if ( selection_control == null )
				{
					obj.AddComponent( "HAPI_ChildSelectionControl" );
					selection_control = obj.GetComponent< HAPI_ChildSelectionControl >();
				}
				
				if ( selection_control )
					selection_control.setAsset( prAsset );
			}
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
