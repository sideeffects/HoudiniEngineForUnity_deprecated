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
		//if ( prEnableLogging )
		//	Debug.Log( "Instancer #" + prObjectId + " (" + object_info.name + "): "
		//			   + "points: " + detail_info.pointCount );
				
		if ( detail_info.pointCount > 65000 )
			throw new HAPI_Error( "Point count (" + detail_info.pointCount + ") above limit (" + 65000 + ")!" );
										
		// Get position point attributes.
		HAPI_AttributeInfo pos_attr_info = new HAPI_AttributeInfo( "P" );
		float[] pos_attr = new float[ 0 ];
		prObjectControl.getAttribute( prObjectControl.prAssetId, prObjectId, 
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
		prObjectControl.getAttribute( prObjectControl.prAssetId, prObjectId, 
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
		prObjectControl.getAttribute( prObjectControl.prAssetId, prObjectId, 
					ref up_attr_info, ref up_attr, HAPI_Host.getAttributeFloatData );
		if ( !up_attr_info.exists )
			throw new HAPI_Error( "No up attribute found." );
		else if ( up_attr_info.owner != (int) HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
			throw new HAPI_Error( "I only understand up as point attributes!" );
		
		if( up_attr.Length != detail_info.pointCount*3 )
		{
			throw new HAPI_Error( "Unexpected up array length found for asset: " + prObjectControl.prAssetId + "!" );
		}
		
		
		for(int ii = 0; ii < detail_info.pointCount; ii++)
		{
			Vector3 pos = new Vector3( pos_attr[ ii*3 ], pos_attr[ ii*3 + 1 ], pos_attr[ ii*3 + 2] );
			Vector3 dir = new Vector3( dir_attr[ ii*3 ], dir_attr[ ii*3 + 1 ], dir_attr[ ii*3 + 2] );
			Vector3 up = new Vector3( up_attr[ ii*3 ], up_attr[ ii*3 + 1 ], up_attr[ ii*3 + 2] );
			
			GameObject objToInstantiate = prObjectControl.prGameObjects[ object_info.objectToInstanceId ];
			//GameObject obj = PrefabUtility.InstantiatePrefab( prGameObjects[prObjectId] ) as GameObject;	
			if( objToInstantiate != null)
			{
				HAPI_TransformInstance instInfo = new HAPI_TransformInstance(true);
				instInfo.pos[0] = pos[0];
				instInfo.pos[1] = pos[1];
				instInfo.pos[2] = pos[2];
				instInfo.dir[0] = dir[0];
				instInfo.dir[1] = dir[1];
				instInfo.dir[2] = dir[2];
				instInfo.up[0] = up[0];
				instInfo.up[1] = up[1];
				instInfo.up[2] = up[2];
				instInfo.scale = 1.0f;
				instInfo.scale3[0] = 1.0f;
				instInfo.scale3[1] = 1.0f;
				instInfo.scale3[2] = 1.0f;
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
				}
				else
				{
					obj = PrefabUtility.InstantiatePrefab( prObjToInstantiate ) as GameObject;
					obj.transform.localPosition = pos;
					obj.transform.localRotation = quat;
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
