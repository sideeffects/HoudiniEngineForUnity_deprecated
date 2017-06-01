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

// Master control for enabling runtime.
#if ( UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_LINUX )
	#define HAPI_ENABLE_RUNTIME
#endif

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class HoudiniAssetGUIOTL : HoudiniAssetGUI 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	public void handlesOnSceneGUI()
	{
#if HAPI_ENABLE_RUNTIME
		if ( myAssetOTL.prShowPinnedInstances )
			drawPinnedInstances();

		// On mouse up the drag operation is completed.
		if ( Event.current.type == EventType.MouseUp )
			myOpInProgress = false;

		string currentGlobalManipTool = Tools.current.ToString();

		if ( currentGlobalManipTool == "Rotate" )
			myManipMode = XformManipMode.Rotate;
		
		else if ( currentGlobalManipTool == "Move" )
			myManipMode = XformManipMode.Translate;
		
		else if ( currentGlobalManipTool == "Scale" )
			myManipMode = XformManipMode.Scale;
		
		if ( myAssetOTL == null )
			return;
		
		int node_id						= myAssetOTL.prNodeId;
		HAPI_HandleInfo[] handleInfos 	= myAssetOTL.prHandleInfos;
		
		if ( handleInfos == null )
			return;

		// Detect changes and build asset only when changed.
		bool changed = false;

		for ( int ii = 0; ii < handleInfos.Length; ++ii )
		{
			HAPI_HandleInfo handleInfo = handleInfos[ ii ];
			if ( handleInfo.typeName == "xform" )
			{
				float tx = 0, ty = 0, tz = 0;
				float rx = 0, ry = 0, rz = 0;
				float sx = 1, sy = 1, sz = 1;
				HAPI_RSTOrder rstOrder = HAPI_RSTOrder.HAPI_SRT;
				HAPI_XYZOrder xyzOrder = HAPI_XYZOrder.HAPI_XYZ;
				
				HAPI_HandleBindingInfo[] bindingInfos = myAssetOTL.prHandleBindingInfos[ ii ];
				
				int[] parm_int_values = myAssetOTL.prParms.prParmIntValues;
				float[] parm_float_values = myAssetOTL.prParms.prParmFloatValues;
				
				if ( parm_int_values == null || parm_float_values == null )
				{
					Debug.LogError( "No parm int/float values yet handles exist?" );
					continue;
				}

				int translate_parm_id = -1;
				int rotate_parm_id = -1;
				int scale_parm_id = -1;
				int rst_order_parm_id = -1;
				int xyz_order_parm_id = -1;

				foreach ( HAPI_HandleBindingInfo bindingInfo in bindingInfos )
				{
					string parm_name = bindingInfo.handleParmName;
					if ( parm_name == "tx" )
						translate_parm_id = bindingInfo.assetParmId;
							
					else if ( parm_name == "rx" )
						rotate_parm_id = bindingInfo.assetParmId;
							
					else if ( parm_name == "sx" )
						scale_parm_id = bindingInfo.assetParmId;
						
					else if ( parm_name == "trs_order" )
						rst_order_parm_id = bindingInfo.assetParmId;
						
					else if ( parm_name == "xyz_order" )
						xyz_order_parm_id = bindingInfo.assetParmId;
				}
				
				if ( translate_parm_id >= 0 )
				{
					HAPI_ParmInfo parm_info = myAssetOTL.prParms.findParm( translate_parm_id );
					
					tx = parm_float_values[ parm_info.floatValuesIndex + 0 ];
					ty = parm_float_values[ parm_info.floatValuesIndex + 1 ];
					tz = parm_float_values[ parm_info.floatValuesIndex + 2 ];
				}
				
				if ( rotate_parm_id >= 0 )
				{
					HAPI_ParmInfo parm_info = myAssetOTL.prParms.findParm( rotate_parm_id );
					
					rx = parm_float_values[ parm_info.floatValuesIndex + 0 ];
					ry = parm_float_values[ parm_info.floatValuesIndex + 1 ];
					rz = parm_float_values[ parm_info.floatValuesIndex + 2 ];
				}
				
				if ( scale_parm_id >= 0 )
				{
					HAPI_ParmInfo parm_info = myAssetOTL.prParms.findParm( scale_parm_id );
					
					sx = parm_float_values[ parm_info.floatValuesIndex + 0 ];
					sy = parm_float_values[ parm_info.floatValuesIndex + 1 ];
					sz = parm_float_values[ parm_info.floatValuesIndex + 2 ];
				}
				
				if ( rst_order_parm_id >= 0 )
				{
					HAPI_ParmInfo parm_info = myAssetOTL.prParms.findParm( rst_order_parm_id );
					rstOrder = (HAPI_RSTOrder) parm_int_values[ parm_info.intValuesIndex ];
				}
				
				if ( xyz_order_parm_id >= 0 )
				{
					HAPI_ParmInfo parm_info = myAssetOTL.prParms.findParm( xyz_order_parm_id );
					xyzOrder = (HAPI_XYZOrder) parm_int_values[ parm_info.intValuesIndex ];
				}				
				
				HAPI_TransformEuler xform = new HAPI_TransformEuler( true );
				
				// This bit is a little tricky.  We will eventually call Handle.PositionHandle
				// or Handle.RotationHandle to display the translation and rotation handles.
				// These function take a translation parameter and a rotation parameter in 
				// order to display the handle in its proper location and orientation.  
				// These functions have an assumed order that it will put the rotation
				// and translation back together.  Depending whether the order of translation
				// and roation matches that of the rstOrder setting, we may, or may not
				// need to convert the translation parameter for use with the handle.
				if ( rstOrder == HAPI_RSTOrder.HAPI_TSR || rstOrder == HAPI_RSTOrder.HAPI_STR || rstOrder == HAPI_RSTOrder.HAPI_SRT )
				{
					xform.position[0] = tx;
					xform.position[1] = ty;
					xform.position[2] = tz;
					xform.rotationEuler[0] = rx;
					xform.rotationEuler[1] = ry;
					xform.rotationEuler[2] = rz;
					xform.scale[0] = 1;
					xform.scale[1] = 1;
					xform.scale[2] = 1;
					xform.rotationOrder = xyzOrder;
					xform.rstOrder = rstOrder;
				}
				else
				{
					xform.position[0] = 0;
					xform.position[1] = 0;
					xform.position[2] = 0;
					xform.rotationEuler[0] = rx;
					xform.rotationEuler[1] = ry;
					xform.rotationEuler[2] = rz;
					xform.scale[0] = 1;
					xform.scale[1] = 1;
					xform.scale[2] = 1;
					xform.rotationOrder = xyzOrder;
					xform.rstOrder = rstOrder;
				}
				
				xform = HoudiniHost.convertTransform( xform, HAPI_RSTOrder.HAPI_SRT, HAPI_XYZOrder.HAPI_ZXY );
				
				// Axis and Rotation conversions:
				// Note that Houdini's X axis points in the opposite direction that Unity's does.  Also, Houdini's 
				// rotation is right handed, whereas Unity is left handed.  To account for this, we need to invert
				// the x coordinate of the translation, and do the same for the rotations (except for the x rotation,
				// which doesn't need to be flipped because the change in handedness AND direction of the left x axis
				// causes a double negative - yeah, I know).
				
				xform.position[ 0 ] = -xform.position[ 0 ];
				xform.rotationEuler[ 1 ] = -xform.rotationEuler[ 1 ];
				xform.rotationEuler[ 2 ] = -xform.rotationEuler[ 2 ];
				tx = -tx;
				
				Handles.matrix = myAssetOTL.transform.localToWorldMatrix;
				
				Vector3 position;
				
				if( rstOrder == HAPI_RSTOrder.HAPI_TSR || rstOrder == HAPI_RSTOrder.HAPI_STR || rstOrder == HAPI_RSTOrder.HAPI_SRT )
					position = new Vector3( xform.position[ 0 ], xform.position[ 1 ], xform.position[ 2 ] );
				else
					position = new Vector3( tx, ty, tz );
				
				Quaternion rotation = Quaternion.Euler(
					xform.rotationEuler[ 0 ], xform.rotationEuler[ 1 ], xform.rotationEuler[ 2 ] );
				Vector3 scale = new Vector3( sx, sy, sz );
				
				if ( myManipMode == XformManipMode.Translate )
				{
					if ( translate_parm_id < 0 )
						continue;

					HAPI_ParmInfo parm_info = myAssetOTL.prParms.findParm( translate_parm_id );

					if ( parm_info.invisible )
						continue;

					GUIStyle style = new GUIStyle( EditorStyles.textField );
					style.contentOffset = new Vector2( 1.4f, 1.4f );
					string handle_name = handleInfo.name;
					if ( parm_info.disabled )
						handle_name = handle_name + " (disabled)";
					GUIContent content = new GUIContent( handle_name );
					content.tooltip = handle_name;
					Handles.Label( position, content, style );

					if ( parm_info.disabled )
					{
						Handles.lighting = false;
						Handles.PositionHandle( position, rotation );
						Handles.lighting = true;
						continue;
					}

					Vector3 new_position = Handles.PositionHandle( position, rotation );

					if ( new_position != position )
					{
						changed = true;

						if ( !myOpInProgress )
						{
							Undo.RecordObject( myAssetOTL.prParms.prParmsUndoInfo, handleInfo.name );
							myOpInProgress = true;
						}

						if ( rstOrder == HAPI_RSTOrder.HAPI_TSR 
							 || rstOrder == HAPI_RSTOrder.HAPI_STR 
							 || rstOrder == HAPI_RSTOrder.HAPI_SRT )
						{
							xform.position[ 0 ] = new_position[ 0 ];
							xform.position[ 1 ] = new_position[ 1 ];
							xform.position[ 2 ] = new_position[ 2 ];
							
							xform = HoudiniHost.convertTransform( xform, rstOrder, xyzOrder );
							new_position.x 		= xform.position[ 0 ];
							new_position.y 		= xform.position[ 1 ];
							new_position.z 		= xform.position[ 2 ];
						}

						// the - in the x coordinate is to convert back to "Houdini" coordinates
						parm_float_values[ parm_info.floatValuesIndex + 0 ] = -new_position.x; 
						parm_float_values[ parm_info.floatValuesIndex + 1 ] = new_position.y;
						parm_float_values[ parm_info.floatValuesIndex + 2 ] = new_position.z;
						
						float[] temp_float_values = new float[ HoudiniConstants.HAPI_POSITION_VECTOR_SIZE ];
						for ( int pp = 0; pp < HoudiniConstants.HAPI_POSITION_VECTOR_SIZE; ++pp )
							temp_float_values[ pp ] = parm_float_values[ parm_info.floatValuesIndex + pp ];
						HoudiniHost.setParmFloatValues( node_id, temp_float_values, parm_info.floatValuesIndex, 
													  parm_info.size );

						myAsset.savePreset();
					} // if changed
				}
				else if ( myManipMode == XformManipMode.Rotate )
				{
					if ( rotate_parm_id < 0 )
						continue;

					HAPI_ParmInfo parm_info = myAssetOTL.prParms.findParm( rotate_parm_id );

					if ( parm_info.invisible )
						continue;

					GUIStyle style = new GUIStyle( EditorStyles.textField );
					style.contentOffset = new Vector2( 1.4f, 1.4f );
					string handle_name = handleInfo.name;
					if ( parm_info.disabled )
						handle_name = handle_name + " (disabled)";
					GUIContent content = new GUIContent( handle_name );
					content.tooltip = handle_name;
					Handles.Label( position, content, style );

					if ( parm_info.disabled )
					{
						Handles.lighting = false;
						Handles.RotationHandle( rotation, position );
						Handles.lighting = true;
						continue;
					}
					
					Quaternion new_rotation = Handles.RotationHandle( rotation, position );
						
					if ( new_rotation != rotation )
					{
						changed = true;

						if ( !myOpInProgress )
						{
							Undo.RecordObject( myAssetOTL.prParms.prParmsUndoInfo, handleInfo.name );
							myOpInProgress = true;
						}

						Vector3 newRot = new_rotation.eulerAngles;
						
						xform.position[0] = 0;
						xform.position[1] = 0;
						xform.position[2] = 0;
						xform.rotationEuler[0] = newRot.x;
						xform.rotationEuler[1] = newRot.y;
						xform.rotationEuler[2] = newRot.z;
						xform.scale[0] = 1;
						xform.scale[1] = 1;
						xform.scale[2] = 1;
						xform.rotationOrder = HAPI_XYZOrder.HAPI_ZXY;
						xform.rstOrder = HAPI_RSTOrder.HAPI_SRT;
						
						xform = HoudiniHost.convertTransform( xform, rstOrder, xyzOrder );
						
						parm_float_values[ parm_info.floatValuesIndex + 0 ] = xform.rotationEuler[ 0 ];
						// the - in the y & z coordinate is to convert back to "Houdini" coordinates
						parm_float_values[ parm_info.floatValuesIndex + 1 ] = -xform.rotationEuler[ 1 ];
						parm_float_values[ parm_info.floatValuesIndex + 2 ] = -xform.rotationEuler[ 2 ];
						
						float[] temp_float_values = new float[ HoudiniConstants.HAPI_POSITION_VECTOR_SIZE ];
						for ( int pp = 0; pp < HoudiniConstants.HAPI_POSITION_VECTOR_SIZE; ++pp )
							temp_float_values[ pp ] = parm_float_values[ parm_info.floatValuesIndex + pp ];
						HoudiniHost.setParmFloatValues(
							node_id, temp_float_values, parm_info.floatValuesIndex, parm_info.size );

						myAsset.savePreset();
					} // if changed
				}
				else if ( myManipMode == XformManipMode.Scale )
				{
					if ( scale_parm_id < 0 )
						continue;

					HAPI_ParmInfo parm_info = myAssetOTL.prParms.findParm( scale_parm_id );

					if ( parm_info.invisible )
						continue;

					GUIStyle style = new GUIStyle( EditorStyles.textField );
					style.contentOffset = new Vector2( 1.4f, 1.4f );
					string handle_name = handleInfo.name;
					if ( parm_info.disabled )
						handle_name = handle_name + " (disabled)";
					GUIContent content = new GUIContent( handle_name );
					content.tooltip = handle_name;
					Handles.Label( position, content, style );

					if ( parm_info.disabled )
					{
						Handles.lighting = false;
						Handles.ScaleHandle( scale, position, rotation, 1.0f );
						Handles.lighting = true;
						continue;
					}
					
					Vector3 new_scale = Handles.ScaleHandle( scale, position, rotation, 1.0f );
					
					if ( new_scale != scale )
					{
						changed = true;

						if ( !myOpInProgress )
						{
							Undo.RecordObject( myAssetOTL.prParms.prParmsUndoInfo, handleInfo.name );
							myOpInProgress = true;
						}

						parm_float_values[ parm_info.floatValuesIndex + 0 ] = new_scale.x;
						parm_float_values[ parm_info.floatValuesIndex + 1 ] = new_scale.y;
						parm_float_values[ parm_info.floatValuesIndex + 2 ] = new_scale.z;
						
						float[] temp_float_values = new float[ HoudiniConstants.HAPI_POSITION_VECTOR_SIZE ];
						for ( int pp = 0; pp < HoudiniConstants.HAPI_POSITION_VECTOR_SIZE; ++pp )
							temp_float_values[ pp ] = parm_float_values[ parm_info.floatValuesIndex + pp ];
						HoudiniHost.setParmFloatValues(
							node_id, temp_float_values, parm_info.floatValuesIndex,
							parm_info.size );

						myAsset.savePreset();
					} // if changed
				} // if myManipMode
			} // if typeName
		} // for each handle
		
		if ( changed )
			myAssetOTL.buildClientSide();
#endif // HAPI_ENABLE_RUNTIME
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private

	private void drawPinnedInstances()
	{
		HoudiniInstancer instancer = myAsset.gameObject.GetComponentInChildren< HoudiniInstancer >();
		if( instancer == null )
			return;
		
		instancer.drawAllPins();
	}

	private enum XformManipMode 
	{
		Translate = 0,
		Rotate,
		Scale
	}

	private XformManipMode myManipMode = XformManipMode.Translate;

	private bool myOpInProgress = false;
}
