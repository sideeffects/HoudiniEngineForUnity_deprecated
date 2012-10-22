using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

using HAPI;

public class HAPI_Instancer : MonoBehaviour {
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	
	public GameObject prObjToInstantiate { get; set; }
	
	public bool prOverrideInstances { get; set; }
	
	public HAPI_Asset prObjectControl { get; set; }
	
	public int prObjectId { get; set; }		
	
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods			
	
	public HAPI_Instancer () 
	{
		prObjectControl = null;
		prOverrideInstances = false;
		prObjectId = -1;
			
	}
	
	public void instanceObjects( )
	{
		destroyChildren();
		
		HAPI_ObjectInfo object_info = prObjectControl.prObjects[ prObjectId ];
		
		// Get Detail info.
		HAPI_DetailInfo detail_info = new HAPI_DetailInfo();
		HAPI_Host.getDetailInfo( prObjectControl.prAssetId, prObjectId, out detail_info );
		if ( prObjectControl.prEnableLogging )
			Debug.Log( "Instancer #" + prObjectId + " (" + object_info.name + "): "
					   + "points: " + detail_info.pointCount );
				
		if ( detail_info.pointCount > 65000 )
			throw new HAPI_Error( "Point count (" + detail_info.pointCount + ") above limit (" + 65000 + ")!" );
										
		// Get position point attributes.
		HAPI_AttributeInfo pos_attr_info = new HAPI_AttributeInfo( "P" );
		float[] pos_attr = new float[ 0 ];
		prObjectControl.getAttribute( prObjectControl.prAssetId, prObjectId, "P",
				 	  				  ref pos_attr_info, ref pos_attr, HAPI_Host.getAttributeFloatData );
		if ( !pos_attr_info.exists )
			throw new HAPI_Error( "No position attribute found." );
		else if ( pos_attr_info.owner != (int) HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
			throw new HAPI_Error( "I only understand position as point attributes!" );
		
		if( pos_attr.Length != detail_info.pointCount*3 )
		{
			throw new HAPI_Error( "Unexpected point array length found for asset: " + prObjectControl.prAssetId + "!" );
		}
		
		// Get direction point attributes.
		HAPI_AttributeInfo dir_attr_info = new HAPI_AttributeInfo( "N" );
		float[] dir_attr = new float[ 0 ];
		prObjectControl.getAttribute( prObjectControl.prAssetId, prObjectId, "N",
					  				  ref dir_attr_info, ref dir_attr, HAPI_Host.getAttributeFloatData );
		if ( !dir_attr_info.exists )
			throw new HAPI_Error( "No normal (N) attribute found." );
		else if ( dir_attr_info.owner != (int) HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
			throw new HAPI_Error( "I only understand normal as point attributes!" );
		
		if( dir_attr.Length != detail_info.pointCount*3 )
		{
			throw new HAPI_Error( "Unexpected normal array length found for asset:" + prObjectControl.prAssetId + "!" );
		}
		
		
		// Get up point attributes.
		HAPI_AttributeInfo up_attr_info = new HAPI_AttributeInfo( "up" );
		float[] up_attr = new float[ 0 ];
		prObjectControl.getAttribute( prObjectControl.prAssetId, prObjectId, "up",
									  ref up_attr_info, ref up_attr, HAPI_Host.getAttributeFloatData );
		if ( !up_attr_info.exists )
			throw new HAPI_Error( "No up attribute found." );
		else if ( up_attr_info.owner != (int) HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
			throw new HAPI_Error( "I only understand up as point attributes!" );
		
		if( up_attr.Length != detail_info.pointCount*3 )
		{
			throw new HAPI_Error( "Unexpected up array length found for asset: " + prObjectControl.prAssetId + "!" );
		}
		
		
		// Get scale point attributes.
		HAPI_AttributeInfo scale_attr_info = new HAPI_AttributeInfo( "scale" );
		float[] scale_attr = new float[ 0 ];
		prObjectControl.getAttribute( prObjectControl.prAssetId, prObjectId, "scale",
									  ref scale_attr_info, ref scale_attr, HAPI_Host.getAttributeFloatData );
		
		if ( scale_attr_info.exists && scale_attr_info.owner != (int) HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
			throw new HAPI_Error( "I only understand up as point attributes!" );
		
		if( scale_attr_info.exists && scale_attr.Length != detail_info.pointCount*3 )
		{
			throw new HAPI_Error( "Unexpected up array length found for asset: " + prObjectControl.prAssetId + "!" );
		}
				
		// Get string point attributes.
		/*HAPI_AttributeInfo instancehint_attr_info = new HAPI_AttributeInfo( "instance_hint" );
		int[] instancehint_attr = new int[ 0 ];
		prObjectControl.getAttribute( prObjectControl.prAssetId, prObjectId, "instance_hint", 
					ref instancehint_attr_info, ref instancehint_attr, HAPI_Host.getAttributeStrData );
		if ( !instancehint_attr_info.exists )
			throw new HAPI_Error( "No instance_hint attribute found." );
		else if ( instancehint_attr_info.owner != (int) HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
			throw new HAPI_Error( "I only understand instance_hint as point attributes!" );
		
		if( instancehint_attr.Length != detail_info.pointCount )
		{
			throw new HAPI_Error( "Unexpected instance_hint array length found for asset: " 
								+ prObjectControl.prAssetId + "!" );
		}*/	
		
		
		HAPI_AttributeInfo instance_attr_info = new HAPI_AttributeInfo( "instance" );
		int[] instance_attr = new int[ 0 ];
		prObjectControl.getAttribute( prObjectControl.prAssetId, prObjectId, "instance", 
					ref instance_attr_info, ref instance_attr, HAPI_Host.getAttributeStrData );
		
		if ( instance_attr_info.exists && instance_attr_info.owner != (int) HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
			throw new HAPI_Error( "I only understand instance as point attributes!" );
		
		if( instance_attr_info.exists && instance_attr.Length != detail_info.pointCount )
		{
			throw new HAPI_Error( "Unexpected instance_hint array length found for asset: " 
								+ prObjectControl.prAssetId + "!" );
		}
		
		for(int ii = 0; ii < detail_info.pointCount; ii++)
		{
			Vector3 pos = new Vector3( pos_attr[ ii*3 ], pos_attr[ ii*3 + 1 ], pos_attr[ ii*3 + 2] );
			Vector3 dir = new Vector3( dir_attr[ ii*3 ], dir_attr[ ii*3 + 1 ], dir_attr[ ii*3 + 2] );			
			Vector3 up = new Vector3( up_attr[ ii*3 ], up_attr[ ii*3 + 1 ], up_attr[ ii*3 + 2] );
			
			GameObject objToInstantiate = null;
			
			if( object_info.objectToInstanceId >= 0 )
			{
				objToInstantiate = prObjectControl.prGameObjects[ object_info.objectToInstanceId ];
			}
			else
			{
				if( instance_attr_info.exists )
				{
					string instanceObjectPath = HAPI_Host.getString( instance_attr[ ii ] );
					string[] pathItems = instanceObjectPath.Split('/');
					string instanceObjectName = pathItems[ pathItems.Length - 1 ];
					
					int objectIndex = prObjectControl.findObjectByName( instanceObjectName );
					if( objectIndex >= 0 )
					{
						objToInstantiate = prObjectControl.prGameObjects[ objectIndex ];
					}
				}
				
			}
			
			
			//string instance_hint = HAPI_Host.getString( instancehint_attr[ ii ] );
			//Debug.Log( "instance hint: " + instance_hint );
			
			
			//GameObject obj = PrefabUtility.InstantiatePrefab( prGameObjects[prObjectId] ) as GameObject;	
			if( objToInstantiate != null)
			{
				HAPI_TransformInstance instInfo = new HAPI_TransformInstance(true);
				instInfo.pos[0] = -pos[0];
				instInfo.pos[1] = pos[1];
				instInfo.pos[2] = pos[2];
				instInfo.dir[0] = -dir[0];
				instInfo.dir[1] = dir[1];
				instInfo.dir[2] = dir[2];
				instInfo.up[0] = -up[0];
				instInfo.up[1] = up[1];
				instInfo.up[2] = up[2];
				instInfo.scale = 1.0f;
				
				if( scale_attr_info.exists )
				{
					Vector3 scale = new Vector3( scale_attr[ ii*3 ], scale_attr[ ii*3 + 1 ], scale_attr[ ii*3 + 2] );			
					instInfo.scale3[0] = scale.x;
					instInfo.scale3[1] = scale.y;
					instInfo.scale3[2] = scale.z;
										
				}
				else
				{
					instInfo.scale3[0] = 1.0f;
					instInfo.scale3[1] = 1.0f;
					instInfo.scale3[2] = 1.0f;
				}
				
				instInfo.quat[0] = 0.0f;
				instInfo.quat[1] = 0.0f;
				instInfo.quat[2] = 0.0f;
				instInfo.quat[3] = 1.0f;
				instInfo.tr[0] = 0.0f;
				instInfo.tr[1] = 0.0f;
				instInfo.tr[2] = 0.0f;
				
				HAPI_Transform transform_out = new HAPI_Transform();
				HAPI_Host.computeInstanceTransform( ref instInfo, 
													(int) HAPI_RSTOrder.SRT,
													ref transform_out );
				
				pos[0] = transform_out.position[0];
				pos[1] = transform_out.position[1];
				pos[2] = transform_out.position[2];
				
				Quaternion quat = new Quaternion( 	transform_out.rotationQuaternion[ 0 ],
													transform_out.rotationQuaternion[ 1 ],
													transform_out.rotationQuaternion[ 2 ],
													transform_out.rotationQuaternion[ 3 ] );
				
				GameObject obj;
				
				if( !prOverrideInstances )
				{
					obj = Instantiate( objToInstantiate, pos, quat ) as GameObject;
					obj.transform.localScale = new Vector3( transform_out.scale[0], 
															transform_out.scale[1], transform_out.scale[2] );
				}
				else
				{
					obj = PrefabUtility.InstantiatePrefab( prObjToInstantiate ) as GameObject;
					obj.transform.localPosition = pos;
					obj.transform.localRotation = quat;
					obj.transform.localScale = new Vector3( transform_out.scale[0], 
															transform_out.scale[1], transform_out.scale[2] );
				}
				
				obj.transform.parent = transform;
				
				
				HAPI_ChildSelectionControl selection_control = obj.GetComponent<HAPI_ChildSelectionControl>();
				if(selection_control == null)
				{
					obj.AddComponent("HAPI_ChildSelectionControl");
					selection_control = obj.GetComponent<HAPI_ChildSelectionControl>();
				}
				
				if(selection_control != null)
					selection_control.setObjectControl( prObjectControl );
			}
			
			
		}
		
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Overrides
	
	// Use this for initialization
	
	
	
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
