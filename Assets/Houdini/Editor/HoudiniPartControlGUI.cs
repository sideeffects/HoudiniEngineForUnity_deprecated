/*
* Copyright (c) <2017> Side Effects Software Inc.
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*
* Produced by:
*      Side Effects Software Inc
*      123 Front Street West, Suite 1401
*      Toronto, Ontario
*      Canada   M5J 2M2
*      416-504-9876
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

		if ( control.prShowPointNumbers )
		{
			// Get position attributes.
			HAPI_AttributeInfo pos_attr_info = new HAPI_AttributeInfo( HoudiniConstants.HAPI_ATTRIB_POSITION );
			float[] pos_attr = new float[ 0 ];
			HoudiniAssetUtility.getAttribute(
				myPartControl.prGeoId, myPartControl.prPartId, HoudiniConstants.HAPI_ATTRIB_POSITION,
				ref pos_attr_info, ref pos_attr, HoudiniHost.getAttributeFloatData );
			if ( !pos_attr_info.exists )
				throw new HoudiniError( "No position attribute found." );

			int point_count = pos_attr.Length / 3;
			// Determine which control point was pressed for modification.
			for ( int i = 0; i < point_count; ++i ) 
			{
				Vector3 position = new Vector3(
					-pos_attr[ i * 3 + 0 ], pos_attr[ i * 3 + 1 ], pos_attr[ i * 3 + 2 ] );
				position = myPartControl.transform.TransformPoint( position );

				Color original_color = GUI.color;
				GUIStyle style = new GUIStyle( GUI.skin.label );

				GUI.color = Color.yellow;
				style.fontStyle = FontStyle.Normal;
				style.fontSize = 12;
				Handles.Label( position, new GUIContent( "" + i ), style );

				GUI.color = original_color;
			}
		}

		/*
		{
			try
			{
				HAPI_AttributeInfo pos_attr_info = new HAPI_AttributeInfo( HoudiniConstants.HAPI_ATTRIB_POSITION );
				float[] pos_attr = new float[ 0 ];
				HoudiniAssetUtility.getAttribute(
					myPartControl.prAssetId, myPartControl.prObjectId, myPartControl.prGeoId, 
					myPartControl.prPartId, HoudiniConstants.HAPI_ATTRIB_POSITION, 
					ref pos_attr_info, ref pos_attr, HoudiniHost.getAttributeFloatData );
				if ( !pos_attr_info.exists )
					throw new HoudiniError( "No position attribute found." );

				bool[] membership = HoudiniHost.getGroupMembership(
					myPartControl.prAssetId, myPartControl.prObjectId, myPartControl.prGeoId,
					myPartControl.prPartId, HAPI_GroupType.HAPI_GROUPTYPE_POINT, "exteriorPoints" );

				int point_count = pos_attr.Length / 3;

				if ( membership.Length != point_count )
					Debug.LogError( "WTF" );

				// Determine which control point was pressed for modification.
				for ( int i = 0; i < point_count; ++i ) 
				{
					if ( membership[ i ] )
					{
						Vector3 position = new Vector3( -pos_attr[ i * 3 + 0 ], pos_attr[ i * 3 + 1 ], pos_attr[ i * 3 + 2 ] );
						Handles.Label( position, new GUIContent("" + i ) );
					}
				}
			}
			catch
			{}

			try
			{
				bool[] membership = HoudiniHost.getGroupMembership(
					myPartControl.prAssetId, myPartControl.prObjectId, myPartControl.prGeoId,
					myPartControl.prPartId, HAPI_GroupType.HAPI_GROUPTYPE_PRIM, "LG_4" );

				Mesh mesh = myPartControl.GetComponent< MeshFilter >().sharedMesh;
				if ( membership.Length != mesh.triangles.Length / 3 )
					Debug.LogError( "WTF" );

				for ( int i = 0; i < mesh.triangles.Length / 3; ++i )
				{
					if ( membership[ i ] )
					{
						Vector3[] vects = new Vector3[ 4 ];
						vects[ 0 ] = mesh.vertices[ mesh.triangles[ i * 3 + 0 ] ];
						vects[ 1 ] = mesh.vertices[ mesh.triangles[ i * 3 + 0 ] ];
						vects[ 2 ] = mesh.vertices[ mesh.triangles[ i * 3 + 1 ] ];
						vects[ 3 ] = mesh.vertices[ mesh.triangles[ i * 3 + 2 ] ];
						Handles.DrawSolidRectangleWithOutline( vects, Color.red, Color.yellow );
					}
				}
			}
			catch
			{}
		}*/

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
		myPartControl.prShowDisplayOptions =
			HoudiniGUI.foldout( "Display Options", myPartControl.prShowDisplayOptions, true );
		if ( myPartControl.prShowDisplayOptions )
		{
			{
				if ( HoudiniGUI.button( "print_attribute_names", "Print Attribute Names" ) )
				{
					HoudiniAssetUtility.printAllAttributeNames(
						myPartControl.prGeoId, myPartControl.prPartId );
				}

				if ( HoudiniGUI.button( "print_group_info", "Print Group Info" ) )
				{
					HoudiniAssetUtility.printAllGroups(
						myPartControl.prGeoId, myPartControl.prPartId );
				}
			}

			{ // Show Houdini Point Numbers
				bool value = myPartControl.prShowPointNumbers;
				bool undo_value = false;
				bool changed = HoudiniGUI.toggle(
					"show_point_numbers", "Show Houdini Point Numbers", ref value,
					null, ref undo_value );
				myPartControl.prShowPointNumbers = value;
				if ( changed )
					EditorUtility.SetDirty( myPartControl );
			}
		}

		myPartControl.prShowIntermediateResultControls =
			HoudiniGUI.foldout( "Intermediate Results", myPartControl.prShowIntermediateResultControls, true );
		if ( myPartControl.prShowIntermediateResultControls )
		{
			bool gui_enabled = GUI.enabled;
			if ( myPartControl.prGeoType != HAPI_GeoType.HAPI_GEOTYPE_INTERMEDIATE )
			{
				HoudiniGUI.help(
					"Only specially marked intermediate results geometries can be edited.",
					MessageType.Info );
				GUI.enabled = false;
			}

			if ( GUILayout.Button( "Update Intermediate Result" ) ) 
			{
				MeshFilter mesh_filter = myPartControl.gameObject.GetComponent< MeshFilter >();
				Mesh shared_mesh = mesh_filter.sharedMesh;
				HoudiniPartControl part_control = myPartControl.gameObject.GetComponent< HoudiniPartControl >();

				HoudiniAssetUtility.setMesh(
					myPartControl.prAsset.prAssetId, 
					myPartControl.prObjectId,
					myPartControl.prGeoId,
					ref shared_mesh,
					part_control,
					null );
				
				myPartControl.prAsset.buildClientSide();
			}
			
			if ( GUILayout.Button( "Clear Edits" ) ) 
			{
				HoudiniHost.revertGeo( myPartControl.prGeoId );
				myPartControl.prAsset.buildClientSide();
			}

			GUI.enabled = gui_enabled;
		}

		myPartControl.prShowInfo =
			HoudiniGUI.foldout( "Info", myPartControl.prShowInfo, true );
		if ( myPartControl.prShowInfo )
		{
			HoudiniGUI.help( "Values here are for debugging only and should not be modified directly.", MessageType.Info );
			bool gui_enabled = GUI.enabled;
			GUI.enabled = false;
			DrawDefaultInspector();
			GUI.enabled = gui_enabled;
		}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private

	[SerializeField] private HoudiniPartControl myPartControl = null;
}
