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
using System;
using System.Collections;
using System.Collections.Generic;
using HAPI;

public partial class HAPI_AssetGUIOTL : HAPI_AssetGUI 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	public override void OnSceneGUI()
	{
		base.OnSceneGUI();

		if ( myAssetOTL.prShowPinnedInstances )
			drawPinnedInstances();
		
		string currentGlobalManipTool = Tools.current.ToString();

		if ( currentGlobalManipTool == "Rotate" )
			myManipMode = XformManipMode.Rotate;
		
		else if ( currentGlobalManipTool == "Move" )
			myManipMode = XformManipMode.Translate;
		
		else if ( currentGlobalManipTool == "Scale" )
			myManipMode = XformManipMode.Scale;
		
		if ( myAssetOTL == null )
			return;
		
		int asset_id					= myAssetOTL.prAssetId;
		HAPI_HandleInfo[] handleInfos 	= myAssetOTL.prHandleInfos;
		
		if ( handleInfos == null )
			return;
		
		for ( int ii = 0; ii < handleInfos.Length; ++ii )
		{
			HAPI_HandleInfo handleInfo = handleInfos[ ii ];
			if ( handleInfo.typeName == "xform" )
			{
				float tx = 0, ty = 0, tz = 0;
				float rx = 0, ry = 0, rz = 0;
				float sx = 1, sy = 1, sz = 1;
				HAPI_RSTOrder rstOrder = HAPI_RSTOrder.SRT;
				HAPI_XYZOrder xyzOrder = HAPI_XYZOrder.XYZ;
				
				HAPI_HandleBindingInfo[] bindingInfos = myAssetOTL.prHandleBindingInfos[ ii ];
				
				int[] parm_int_values = myAssetOTL.prParmIntValues;
				float[] parm_float_values = myAssetOTL.prParmFloatValues;
				
				if ( parm_int_values == null || parm_float_values == null )
				{
					Debug.LogError( "No parm int/float values yet handles exist?" );
					continue;
				}
					
				if ( myTranslateParmId == -1 ||
					 myRotateParmId == -1 ||
					 myScaleParmId == -1 ||
					 myRstOrderParmId == -1 ||
					 myXyzOrderParmId == -1 )
				{
					foreach ( HAPI_HandleBindingInfo bindingInfo in bindingInfos )
					{
						string parm_name = bindingInfo.handleParmName;
						if ( parm_name == "tx" )
							myTranslateParmId = bindingInfo.assetParmId;
							
						else if ( parm_name == "rx" )
							myRotateParmId = bindingInfo.assetParmId;
							
						else if ( parm_name == "sx" )
							myScaleParmId = bindingInfo.assetParmId;
						
						else if ( parm_name == "trs_order" )
							myRstOrderParmId = bindingInfo.assetParmId;
						
						else if ( parm_name == "xyz_order" )
							myXyzOrderParmId = bindingInfo.assetParmId;
					}
				}
				
				if ( myTranslateParmId >= 0 )
				{
					HAPI_ParmInfo parm_info = myAssetOTL.prParms[ myTranslateParmId ];
					
					tx = parm_float_values[ parm_info.floatValuesIndex + 0 ];
					ty = parm_float_values[ parm_info.floatValuesIndex + 1 ];
					tz = parm_float_values[ parm_info.floatValuesIndex + 2 ];
				}
				
				if ( myRotateParmId >= 0 )
				{
					HAPI_ParmInfo parm_info = myAssetOTL.prParms[ myRotateParmId ];
					
					rx = parm_float_values[ parm_info.floatValuesIndex + 0 ];
					ry = parm_float_values[ parm_info.floatValuesIndex + 1 ];
					rz = parm_float_values[ parm_info.floatValuesIndex + 2 ];
				}
				
				if ( myScaleParmId >= 0 )
				{
					HAPI_ParmInfo parm_info = myAssetOTL.prParms[ myScaleParmId ];
					
					sx = parm_float_values[ parm_info.floatValuesIndex + 0 ];
					sy = parm_float_values[ parm_info.floatValuesIndex + 1 ];
					sz = parm_float_values[ parm_info.floatValuesIndex + 2 ];
				}
				
				if ( myRstOrderParmId >= 0 )
				{
					HAPI_ParmInfo parm_info = myAssetOTL.prParms[ myRstOrderParmId ];
					rstOrder = (HAPI_RSTOrder) parm_int_values[ parm_info.intValuesIndex ];
				}
				
				if ( myXyzOrderParmId >= 0 )
				{
					HAPI_ParmInfo parm_info = myAssetOTL.prParms[ myXyzOrderParmId ];
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
				if ( rstOrder == HAPI_RSTOrder.TSR || rstOrder == HAPI_RSTOrder.STR || rstOrder == HAPI_RSTOrder.SRT )
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
					xform.rotationOrder = (int) xyzOrder;
					xform.rstOrder = (int) rstOrder;
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
					xform.rotationOrder = (int) xyzOrder;
					xform.rstOrder = (int) rstOrder;
				}
				
				HAPI_Host.convertTransform( ref xform, (int) HAPI_RSTOrder.SRT, (int) HAPI_XYZOrder.ZXY );
				
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
				
				if( rstOrder == HAPI_RSTOrder.TSR || rstOrder == HAPI_RSTOrder.STR || rstOrder == HAPI_RSTOrder.SRT )
					position = new Vector3( xform.position[ 0 ], xform.position[ 1 ], xform.position[ 2 ] );
				else
					position = new Vector3( tx, ty, tz );
				
				Quaternion rotation = Quaternion.Euler( xform.rotationEuler[ 0 ], xform.rotationEuler[ 1 ], 
													    xform.rotationEuler[ 2 ] );
				Vector3 scale = new Vector3( sx, sy, sz );
				
				if ( myManipMode == XformManipMode.Translate )
				{
					if ( myTranslateParmId < 0 )
						continue;
					
					Vector3 new_position = Handles.PositionHandle( position, rotation );
						
					if ( GUI.changed )
					{
						if ( rstOrder == HAPI_RSTOrder.TSR 
							 || rstOrder == HAPI_RSTOrder.STR 
							 || rstOrder == HAPI_RSTOrder.SRT )
						{
							xform.position[ 0 ] = new_position[ 0 ];
							xform.position[ 1 ] = new_position[ 1 ];
							xform.position[ 2 ] = new_position[ 2 ];
							
							HAPI_Host.convertTransform( ref xform, (int) rstOrder, (int) xyzOrder );
							new_position.x 		= xform.position[ 0 ];
							new_position.y 		= xform.position[ 1 ];
							new_position.z 		= xform.position[ 2 ];
						}
						
						HAPI_ParmInfo parm_info = myAssetOTL.prParms[ myTranslateParmId ];
						
						// the - in the x coordinate is to convert back to "Houdini" coordinates
						parm_float_values[ parm_info.floatValuesIndex + 0 ] = -new_position.x; 
						parm_float_values[ parm_info.floatValuesIndex + 1 ] = new_position.y;
						parm_float_values[ parm_info.floatValuesIndex + 2 ] = new_position.z;
						
						float[] temp_float_values = new float[ HAPI_Constants.HAPI_POSITION_VECTOR_SIZE ];
						for ( int pp = 0; pp < HAPI_Constants.HAPI_POSITION_VECTOR_SIZE; ++pp )
							temp_float_values[ pp ] = parm_float_values[ parm_info.floatValuesIndex + pp ];
						HAPI_Host.setParmFloatValues( asset_id, temp_float_values, parm_info.floatValuesIndex, 
													  parm_info.size );
						myAsset.savePreset();
					} // if changed
				}
				else if ( myManipMode == XformManipMode.Rotate )
				{
					if ( myRotateParmId < 0 )
						continue;
					
					Quaternion newRotQuat = Handles.RotationHandle( rotation, position );
						
					if ( GUI.changed )
					{
						Vector3 newRot = newRotQuat.eulerAngles;
						
						xform.position[0] = 0;
						xform.position[1] = 0;
						xform.position[2] = 0;
						xform.rotationEuler[0] = newRot.x;
						xform.rotationEuler[1] = newRot.y;
						xform.rotationEuler[2] = newRot.z;
						xform.scale[0] = 1;
						xform.scale[1] = 1;
						xform.scale[2] = 1;
						xform.rotationOrder = (int) HAPI_XYZOrder.ZXY;
						xform.rstOrder = (int) HAPI_RSTOrder.SRT;
						
						HAPI_Host.convertTransform( ref xform, (int) rstOrder, (int) xyzOrder );
						
						HAPI_ParmInfo parm_info = myAssetOTL.prParms[ myRotateParmId ];
						
						parm_float_values[ parm_info.floatValuesIndex + 0 ] = xform.rotationEuler[ 0 ];
						// the - in the y & z coordinate is to convert back to "Houdini" coordinates
						parm_float_values[ parm_info.floatValuesIndex + 1 ] = -xform.rotationEuler[ 1 ];
						parm_float_values[ parm_info.floatValuesIndex + 2 ] = -xform.rotationEuler[ 2 ];
						
						float[] temp_float_values = new float[ HAPI_Constants.HAPI_POSITION_VECTOR_SIZE ];
						for ( int pp = 0; pp < HAPI_Constants.HAPI_POSITION_VECTOR_SIZE; ++pp )
							temp_float_values[ pp ] = parm_float_values[ parm_info.floatValuesIndex + pp ];
						HAPI_Host.setParmFloatValues( asset_id, temp_float_values, parm_info.floatValuesIndex, 
													  parm_info.size );
						myAsset.savePreset();
					} // if changed
				}
				else if ( myManipMode == XformManipMode.Scale )
				{
					if ( myScaleParmId < 0 )
						continue;
					
					Vector3 newScale = Handles.ScaleHandle( scale, position, rotation, 1.0f );
					
					if ( GUI.changed )
					{
						HAPI_ParmInfo parm_info = myAssetOTL.prParms[ myScaleParmId ];
						
						parm_float_values[ parm_info.floatValuesIndex + 0 ] = newScale.x;
						parm_float_values[ parm_info.floatValuesIndex + 1 ] = newScale.y;
						parm_float_values[ parm_info.floatValuesIndex + 2 ] = newScale.z;
						
						float[] temp_float_values = new float[ HAPI_Constants.HAPI_POSITION_VECTOR_SIZE ];
						for ( int pp = 0; pp < HAPI_Constants.HAPI_POSITION_VECTOR_SIZE; ++pp )
							temp_float_values[ pp ] = parm_float_values[ parm_info.floatValuesIndex + pp ];
						HAPI_Host.setParmFloatValues( asset_id, temp_float_values, parm_info.floatValuesIndex, 
													  parm_info.size );
						myAsset.savePreset();
					} // if changed
				} // if myManipMode
			} // if typeName
		} // for each handle
		
		if ( GUI.changed )
			myAssetOTL.buildClientSide();
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private
	
	private void drawPinnedInstances()
	{
		HAPI_Instancer instancer = myAsset.gameObject.GetComponentInChildren< HAPI_Instancer >();
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
	
	private XformManipMode myManipMode 	= XformManipMode.Translate;	
	
	private int myTranslateParmId 		= -1;
	private int myRotateParmId 			= -1;
	private int myScaleParmId 			= -1;
	private int myRstOrderParmId 		= -1;
	private int myXyzOrderParmId 		= -1;
}
