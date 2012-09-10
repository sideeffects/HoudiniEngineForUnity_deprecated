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
 * 		GUI companion to HAPI_Curve.
 * 
 */

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

using HAPI;

/// <summary>
/// 	GUI companion to <see cref="HAPI_ObjectControl"/>.
/// </summary>
public partial class HAPI_Inspector : Editor 
{	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
			
	public void OnSceneGUI() 
	{
		Event current_event 		= Event.current;
		
		
		if(current_event.isKey)
		{
			if( current_event.keyCode == KeyCode.W )
			{				
				myManipMode = XformManipMode.Translate;
			}
			
			if( current_event.keyCode == KeyCode.E )
			{			
				myManipMode = XformManipMode.Rotate;
			}
			
			if( current_event.keyCode == KeyCode.R )
			{			
				myManipMode = XformManipMode.Scale;
			}
		}
		
		if(myObjectControl == null)
		{			
			return;
		}
		
		HAPI_HandleInfo[] handleInfos = myObjectControl.myHandleInfos;
		
		if ( handleInfos == null )
			return;
				
		
		for(int ii = 0; ii < handleInfos.Length; ii++)
		{
			HAPI_HandleInfo handleInfo = handleInfos[ii];
			if(handleInfo.handleTypeName == "xform")
			{
				float tx = 0, ty = 0, tz = 0;
				float rx = 0, ry = 0, rz = 0;
				float sx = 1, sy = 1, sz = 1;
				HAPI_RSTOrder rstOrder = HAPI_RSTOrder.SRT;
				HAPI_XYZOrder xyzOrder = HAPI_XYZOrder.XYZ;
				
				HAPI_HandleBindingInfo[] bindingInfos = myObjectControl.myHandleBindingInfos[ii];
												
				if( myTranslateParmId == -1 ||
					myRotateParmId == -1 ||
					myScaleParmId == -1 ||
					myRstOrderParmId == -1 ||
					myXyzOrderParmId == -1 )
				{
					foreach(HAPI_HandleBindingInfo bindingInfo in bindingInfos)
					{
						if(bindingInfo.handleParmName == "tx")
						{
							myTranslateParmId = bindingInfo.assetParmId;							
						}
																														
						if(bindingInfo.handleParmName == "rx")
						{
							myRotateParmId = bindingInfo.assetParmId;							
						}
																		
						if(bindingInfo.handleParmName == "sx")
						{
							myScaleParmId = bindingInfo.assetParmId;
							
						}						
						
						if( bindingInfo.handleParmName == "trs_order" )
						{
							myRstOrderParmId = bindingInfo.assetParmId;
							
						}
						
						if( bindingInfo.handleParmName == "xyz_order" )
						{
							myXyzOrderParmId = bindingInfo.assetParmId;							
						}
																	
					}
				}
				
				if(myTranslateParmId > 0)
				{
					HAPI_ParmInfo parmInfo = myObjectControl.myParms[myTranslateParmId];
					tx = parmInfo.floatValue[0];
					ty = parmInfo.floatValue[1];
					tz = parmInfo.floatValue[2];
				}
				
				if(myRotateParmId > 0)
				{
					HAPI_ParmInfo parmInfo = myObjectControl.myParms[myRotateParmId];
					rx = parmInfo.floatValue[0];
					ry = parmInfo.floatValue[1];
					rz = parmInfo.floatValue[2];
				}
				
				if(myScaleParmId > 0)
				{
					HAPI_ParmInfo parmInfo = myObjectControl.myParms[myScaleParmId];
					sx = parmInfo.floatValue[0];
					sy = parmInfo.floatValue[1];
					sz = parmInfo.floatValue[2];
				}
				
				if( myRstOrderParmId > 0 )
				{
					HAPI_ParmInfo parmInfo = myObjectControl.myParms[myRstOrderParmId];
					rstOrder = (HAPI_RSTOrder) parmInfo.intValue[0];
				}
				
				if( myXyzOrderParmId > 0 )
				{
					HAPI_ParmInfo parmInfo = myObjectControl.myParms[myXyzOrderParmId];
					xyzOrder = (HAPI_XYZOrder) parmInfo.intValue[0];
				}
				
				
				Handles.matrix = myObjectControl.transform.localToWorldMatrix;				
												
				
				//TODO: There is still a bug in here, where if you put the exposed transform order to STR 
				//(or something similar where T happens before R, instead of the default SRT), 
				// the translate handle is still not working properly.
				HAPI_TransformEuler xform = new HAPI_TransformEuler(true);				
				
				xform.position[0] = 0;
				xform.position[1] = 0;
				xform.position[2] = 0;
				xform.rotationeEuler[0] = rx;
				xform.rotationeEuler[1] = ry;
				xform.rotationeEuler[2] = rz;
				xform.scale[0] = 1;
				xform.scale[1] = 1;
				xform.scale[2] = 1;
				xform.rotationOrder = (int) xyzOrder;
				xform.rstOrder = (int) rstOrder;
								
				HAPI_Host.convertTransform(ref xform, (int) HAPI_RSTOrder.SRT, (int) HAPI_XYZOrder.ZXY);
				
				Vector3 translate = new Vector3(tx, ty, tz);								
				Quaternion rotate = Quaternion.Euler(xform.rotationeEuler[0], xform.rotationeEuler[1], xform.rotationeEuler[2]);				
				Vector3 scale = new Vector3( sx, sy, sz);															
				
				/*
				Debug.Log(" pos = " + xform.position[0] + "," + xform.position[1] + "," + xform.position[2] 
					    + " rot = " + xform.rotationeEuler[0] + "," + xform.rotationeEuler[1] + "," + xform.rotationeEuler[2]
					    + " s = " + xform.scale[0] + "," + xform.scale[1] + "," + xform.scale[2] 
					    + " rstorder= " + xform.rstOrder + " rotorder=" + xform.rotationOrder );
				*/								
				
				if( myManipMode == XformManipMode.Translate )
				{
					if( myTranslateParmId > 0)
					{
						Vector3 newPos = Handles.PositionHandle( translate, rotate );
																		
						myObjectControl.myParms[myTranslateParmId].floatValue[0] = newPos.x;
						myObjectControl.myParms[myTranslateParmId].floatValue[1] = newPos.y;
						myObjectControl.myParms[myTranslateParmId].floatValue[2] = newPos.z;
						
					}
				}
				else if( myManipMode == XformManipMode.Rotate )
				{
					if( myRotateParmId > 0 )
					{
						Quaternion newRotQuat = Handles.RotationHandle( rotate, translate );
						
						Vector3 newRot = newRotQuat.eulerAngles;
						
						xform.position[0] = 0;
						xform.position[1] = 0;
						xform.position[2] = 0;
						xform.rotationeEuler[0] = newRot.x;
						xform.rotationeEuler[1] = newRot.y;
						xform.rotationeEuler[2] = newRot.z;
						xform.scale[0] = 1;
						xform.scale[1] = 1;
						xform.scale[2] = 1;
						xform.rotationOrder = (int) HAPI_XYZOrder.ZXY;
						xform.rstOrder = (int) HAPI_RSTOrder.SRT;
						
						HAPI_Host.convertTransform(ref xform, (int) rstOrder, (int) xyzOrder);
						
						myObjectControl.myParms[myRotateParmId].floatValue[0] = xform.rotationeEuler[0];
						myObjectControl.myParms[myRotateParmId].floatValue[1] = xform.rotationeEuler[1];
						myObjectControl.myParms[myRotateParmId].floatValue[2] = xform.rotationeEuler[2];
					}					
				}
				else if( myManipMode == XformManipMode.Scale )
				{
					if( myScaleParmId > 0 )
					{
						Vector3 newScale = Handles.ScaleHandle( scale, translate, rotate, 1.0f);
						myObjectControl.myParms[myScaleParmId].floatValue[0] = newScale.x;
						myObjectControl.myParms[myScaleParmId].floatValue[1] = newScale.y;
						myObjectControl.myParms[myScaleParmId].floatValue[2] = newScale.z;							
					}
				}
									
			}
			else
			{
				Debug.Log("Warning: unsupported handle found " + handleInfo.handleName + " of type " + handleInfo.handleTypeName);
			}
		}
		
		if( GUI.changed)
			myObjectControl.build();
			
				
		
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private
	
	private enum XformManipMode 
	{
		Translate = 0,
		Rotate,
		Scale
	}
	
	private XformManipMode 		myManipMode 				= XformManipMode.Translate;	
	
	private int myTranslateParmId = -1;
	private int myRotateParmId = -1;
	private int myScaleParmId = -1;
	private int myRstOrderParmId = -1;
	private int myXyzOrderParmId = -1;
					
	
}
