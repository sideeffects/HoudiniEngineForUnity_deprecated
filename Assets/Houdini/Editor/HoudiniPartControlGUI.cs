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

[ CustomEditor( typeof( HoudiniPartControl ) ) ]
[ CanEditMultipleObjects ]
public class HoudiniPartControlGUI : Editor 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public

	public void OnSceneGUI()
	{
		HoudiniPartControl control = target as HoudiniPartControl;
		control.selectParent();

		if ( control.prShowPointNumbers )
		{
			// Get position attributes.
			HAPI_AttributeInfo pos_attr_info = new HAPI_AttributeInfo( HoudiniConstants.HAPI_ATTRIB_POSITION );
			float[] pos_attr = new float[ 0 ];
			HoudiniAssetUtility.getAttribute(
				myPartControl.prAssetId, myPartControl.prObjectId, myPartControl.prGeoId, 
				myPartControl.prPartId, HoudiniConstants.HAPI_ATTRIB_POSITION, 
				ref pos_attr_info, ref pos_attr, HoudiniHost.getAttributeFloatData );
			if ( !pos_attr_info.exists )
				throw new HoudiniError( "No position attribute found." );

			int point_count = pos_attr.Length / 3;
			// Determine which control point was pressed for modification.
			for ( int i = 0; i < point_count; ++i ) 
			{
				Vector3 position 	= new Vector3( -pos_attr[ i * 3 + 0 ], pos_attr[ i * 3 + 1 ], pos_attr[ i * 3 + 2 ] );
				//float handle_size 	= HandleUtility.GetHandleSize( position ) * 0.06f;

				Handles.Label( position, new GUIContent("" + i ) );
			}
		}

		HoudiniInstance instance = findInstanceControlInParent();
		if ( instance == null )
			return;
		
		bool is_overridden = instance.prInstancer.isPointOverridden( instance.prInstancePointNumber );
		
		if ( is_overridden )
			instance.prInstancer.drawPin( instance.prInstancePointNumber );

		Event curr_event = Event.current;

		if (
			curr_event.isMouse && curr_event.type == EventType.MouseDown &&
			HoudiniHost.prAutoPinInstances )
		{
			control.prTransformChanged = false;
		}
		else if (
			curr_event.isMouse && curr_event.type == EventType.MouseUp &&
			HoudiniHost.prAutoPinInstances && control.prTransformChanged )
		{
			instance.prInstancer.pinObject( control.gameObject, true );
			control.prTransformChanged = false;
			Repaint();
		}
	}

	private HoudiniInstance findInstanceControlInParent()
	{
		Transform parent = myPartControl.gameObject.transform.parent;
		while( parent != null )
		{
			HoudiniInstance instance = parent.gameObject.GetComponent< HoudiniInstance >();
			if( instance != null )
				return instance;
			parent = parent.parent;
		}
		return null;
	}

	public virtual void OnEnable() 
	{
		myPartControl = target as HoudiniPartControl;
	}

	public override void OnInspectorGUI() 
	{
		{
			bool value = myPartControl.prShowPointNumbers;
			bool changed = HoudiniGUI.toggle( "show_point_numbers", "Show Point Numbers", ref value,
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
				HoudiniPartControl part_control	= myPartControl.gameObject.GetComponent< HoudiniPartControl >();
				HoudiniAssetUtility.setMesh(		myPartControl.prAsset.prAssetId, 
												myPartControl.prObjectId,
												myPartControl.prGeoId,
												ref shared_mesh,
												part_control,
												null );
				
				myPartControl.prAsset.buildClientSide();
			}
			
			if ( GUILayout.Button( "Clear Edits" ) ) 
			{
				HoudiniHost.revertGeo( myPartControl.prAsset.prAssetId,
									 myPartControl.prObjectId,
									 myPartControl.prGeoId );
				
				myPartControl.prAsset.buildClientSide();
			}
		}

		HoudiniGUI.help( "Values here are for debugging only and should not be modified directly.", MessageType.Info );
		bool gui_enabled = GUI.enabled;
		GUI.enabled = false;
		DrawDefaultInspector();
		GUI.enabled = gui_enabled;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private

	[SerializeField] private HoudiniPartControl myPartControl = null;
}
