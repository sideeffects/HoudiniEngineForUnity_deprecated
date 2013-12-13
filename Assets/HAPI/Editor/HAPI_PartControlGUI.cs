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
		
		HAPI_Instancer instancer = instancerFromPartObject( control.gameObject );
		if ( instancer == null )
			return;
		
		bool is_overridden = instancer.isPointOverridden( control.prInstancePointNumber );
		
		if ( is_overridden )
			instancer.drawPin( control.prInstancePointNumber );

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
			pinPartObject( control.gameObject, true );
			control.prTransformChanged = false;
			Repaint();
		}
	}

	public virtual void OnEnable() 
	{
		myPartControl = target as HAPI_PartControl;
	}

	private HAPI_Instancer instancerFromPartObject( GameObject part_object )
	{
		GameObject instancer_game_object = null;

		if ( part_object.transform.parent != null &&
			part_object.transform.parent.parent != null &&
			part_object.transform.parent.parent.parent != null )
			instancer_game_object = part_object.transform.parent.parent.parent.gameObject;

		HAPI_Instancer instancer = null;

		if( instancer_game_object != null )
			instancer = instancer_game_object.GetComponent< HAPI_Instancer >();

		return instancer;
	}

	private string findFullPath( GameObject game_obj )
	{
		GameObject obj = game_obj;
		string path = "/" + obj.name;
		while ( obj.transform.parent != null )
		{
			obj = obj.transform.parent.gameObject;
			path = "/" + obj.name + path;
		}
		return path;
	}

	private void pinPartObject( GameObject part_object, bool pin )
	{
		HAPI_Instancer instancer = instancerFromPartObject( part_object );
		if ( instancer == null )
			return;
		
		HAPI_PartControl part_control = part_object.GetComponent< HAPI_PartControl >();
		if ( part_control == null )
			return;

		if ( !pin )
			instancer.unPinInstance( part_control.prInstancePointNumber );
		else
		{
			Transform game_object_xform = part_control.gameObject.transform;

			HAPI_InstancerOverrideInfo override_info =
				ScriptableObject.CreateInstance< HAPI_InstancerOverrideInfo >();

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
			override_info.objectToInstantiatePath = findFullPath( part_control.prObjectToInstantiate );
			override_info.instancePointNumber = part_control.prInstancePointNumber;

			instancer.pinInstance( override_info );
		}
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

		Object[] selection = Selection.objects;
		HAPI_Instancer instancer = instancerFromPartObject( myPartControl.gameObject );
		if ( selection.Length > 0 )
		{
			if ( !HAPI_Host.prAutoPinInstances && GUILayout.Button( "Pin Selection" ) )
				foreach( Object obj in selection )
					if ( obj.GetType() == typeof( GameObject ) )
						pinPartObject( (GameObject) obj, true );
			
			if ( GUILayout.Button( "UnPin Selection" ) ) 
				foreach( Object obj in selection )
					if ( obj.GetType() == typeof( GameObject ) )
						pinPartObject( (GameObject) obj, false );
		}
		else if ( instancer )
		{
			bool is_overridden = instancer.isPointOverridden( myPartControl.prInstancePointNumber );
			if ( is_overridden && GUILayout.Button( "UnPin Instance" ) )
				pinPartObject( myPartControl.gameObject, false );
			else if ( !HAPI_Host.prAutoPinInstances && GUILayout.Button( "Pin Instance" ) )
				pinPartObject( myPartControl.gameObject, true );
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
