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
[ CanEditMultipleObjects ]
public class HAPI_PartControlGUI : Editor 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public

	public void OnSceneGUI()
	{
		HAPI_PartControl control = target as HAPI_PartControl;
		control.selectParent();

		if ( control.prShowPointNumbers )
		{
			// Get position attributes.
			HAPI_AttributeInfo pos_attr_info = new HAPI_AttributeInfo( HAPI_Constants.HAPI_ATTRIB_POSITION );
			float[] pos_attr = new float[ 0 ];
			HAPI_AssetUtility.getAttribute(
				myPartControl.prAssetId, myPartControl.prObjectId, myPartControl.prGeoId, 
				myPartControl.prPartId, HAPI_Constants.HAPI_ATTRIB_POSITION, 
				ref pos_attr_info, ref pos_attr, HAPI_Host.getAttributeFloatData );
			if ( !pos_attr_info.exists )
				throw new HAPI_Error( "No position attribute found." );

			int point_count = pos_attr.Length / 3;
			// Determine which control point was pressed for modification.
			for ( int i = 0; i < point_count; ++i ) 
			{
				Vector3 position 	= new Vector3( -pos_attr[ i * 3 + 0 ], pos_attr[ i * 3 + 1 ], pos_attr[ i * 3 + 2 ] );
				//float handle_size 	= HandleUtility.GetHandleSize( position ) * 0.06f;

				Handles.Label( position, new GUIContent("" + i ) );
			}
		}

		HAPI_Instance instance = findInstanceControlInParent();
		if ( instance == null )
			return;
		
		bool is_overridden = instance.prInstancer.isPointOverridden( instance.prInstancePointNumber );
		
		if ( is_overridden )
			instance.prInstancer.drawPin( instance.prInstancePointNumber );

		Event curr_event = Event.current;

		if (
			curr_event.isMouse && curr_event.type == EventType.MouseDown &&
			HAPI_Host.prAutoPinInstances )
		{
			control.prTransformChanged = false;
		}
		else if (
			curr_event.isMouse && curr_event.type == EventType.MouseUp &&
			HAPI_Host.prAutoPinInstances && control.prTransformChanged )
		{
			instance.prInstancer.pinObject( control.gameObject, true );
			control.prTransformChanged = false;
			Repaint();
		}
	}

	private HAPI_Instance findInstanceControlInParent()
	{
		Transform parent = myPartControl.gameObject.transform.parent;
		while( parent != null )
		{
			HAPI_Instance instance = parent.gameObject.GetComponent< HAPI_Instance >();
			if( instance != null )
				return instance;
			parent = parent.parent;
		}
		return null;
	}

	public virtual void OnEnable() 
	{
		myPartControl = target as HAPI_PartControl;
	}

	public override void OnInspectorGUI() 
	{
		{
			bool value = myPartControl.prShowPointNumbers;
			bool changed = HAPI_GUI.toggle( "show_point_numbers", "Show Point Numbers", ref value,
			                                null, ref value );
			myPartControl.prShowPointNumbers = value;
			if ( changed )
				EditorUtility.SetDirty( myPartControl );
		}

		if ( myPartControl.prGeoType == HAPI_GeoType.HAPI_GEOTYPE_INTERMEDIATE )
		{
			if ( GUILayout.Button( "Update Intermediate Result" ) ) 
			{
				MeshFilter mesh_filter			= myPartControl.gameObject.GetComponent< MeshFilter >();
				Mesh shared_mesh				= mesh_filter.sharedMesh;
				HAPI_PartControl part_control	= myPartControl.gameObject.GetComponent< HAPI_PartControl >();
				HAPI_AssetUtility.setMesh(		myPartControl.prAsset.prAssetId, 
												myPartControl.prObjectId,
												myPartControl.prGeoId,
												ref shared_mesh,
												part_control );
				
				myPartControl.prAsset.buildClientSide();
			}
			
			if ( GUILayout.Button( "Clear Edits" ) ) 
			{
				HAPI_Host.revertGeo( myPartControl.prAsset.prAssetId,
									 myPartControl.prObjectId,
									 myPartControl.prGeoId );
				
				myPartControl.prAsset.buildClientSide();
			}
		}

		HAPI_GUI.help( "Values here are for debugging only and should not be modified directly.", MessageType.Info );
		bool gui_enabled = GUI.enabled;
		GUI.enabled = false;
		DrawDefaultInspector();
		GUI.enabled = gui_enabled;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private

	[SerializeField] private HAPI_PartControl myPartControl = null;
}
