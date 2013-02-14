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
using UnityEditor;
using System.Collections;

using HAPI;

[ CustomEditor( typeof( HAPI_PartControl ) ) ]
public class HAPI_PartControlGUI : Editor 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	public void OnSceneGUI()
	{
		HAPI_PartControl control = target as HAPI_PartControl;
		control.selectParent();
	}
	
	public virtual void OnEnable() 
	{
		myPartControl = target as HAPI_PartControl;
	}
	
	public override void OnInspectorGUI() 
	{
		if ( myPartControl.prGeoType == (int) HAPI_GeoType.HAPI_GEOTYPE_EXPOSED_EDIT )
		{
			if ( GUILayout.Button( "Update Intermediate Result" ) ) 
			{
				MeshFilter mesh_filter 	= myPartControl.gameObject.GetComponent< MeshFilter >();
				Mesh shared_mesh = mesh_filter.sharedMesh;
				HAPI_PartControl part_control = myPartControl.gameObject.GetComponent< HAPI_PartControl >();
				HAPI_AssetUtility.setMesh( myPartControl.prAsset.prAssetId, 
										   myPartControl.prObjectId,
										   myPartControl.prGeoId,
										   ref shared_mesh,
										   part_control );
				
				myPartControl.prAsset.prFullBuild = false;
				myPartControl.prAsset.build();
			}
			
			if ( GUILayout.Button( "Clear Edits" ) ) 
			{
				HAPI_Host.revertGeo( myPartControl.prAsset.prAssetId,
									 myPartControl.prObjectId,
									 myPartControl.prGeoId );
				
				myPartControl.prAsset.prFullBuild = false;
				myPartControl.prAsset.build();
			}
		}
		
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private

	[SerializeField] private HAPI_PartControl myPartControl = null;
	
}
