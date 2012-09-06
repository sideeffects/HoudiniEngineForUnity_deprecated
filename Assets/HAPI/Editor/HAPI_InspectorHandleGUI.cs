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
				
				HAPI_HandleBindingInfo[] bindingInfos = myObjectControl.myHandleBindingInfos[ii];
				
				int translateParmId = -1;
				int rotateParmId = -1;
				int scaleParmId = -1;
				
				foreach(HAPI_HandleBindingInfo bindingInfo in bindingInfos)
				{
					if(bindingInfo.handleParmName == "tx")
					{
						translateParmId = bindingInfo.assetParmId;
						if(translateParmId > 0)
						{
							HAPI_ParmInfo parmInfo = myObjectControl.myParms[translateParmId];
							tx = parmInfo.floatValue[0];
						}
					}
					
					if(bindingInfo.handleParmName == "ty")
					{
						translateParmId = bindingInfo.assetParmId;
						if(translateParmId > 0)
						{
							HAPI_ParmInfo parmInfo = myObjectControl.myParms[translateParmId];
							ty = parmInfo.floatValue[1];
						}
					}
					
					if(bindingInfo.handleParmName == "tz")
					{
						translateParmId = bindingInfo.assetParmId;
						if(translateParmId > 0)
						{
							HAPI_ParmInfo parmInfo = myObjectControl.myParms[translateParmId];
							tz = parmInfo.floatValue[2];
						}
					}
					
					if(bindingInfo.handleParmName == "rx")
					{
						rotateParmId = bindingInfo.assetParmId;
						if(rotateParmId > 0)
						{
							HAPI_ParmInfo parmInfo = myObjectControl.myParms[rotateParmId];
							rx = parmInfo.floatValue[0];
						}
					}
					
					if(bindingInfo.handleParmName == "ry")
					{
						rotateParmId = bindingInfo.assetParmId;
						if(rotateParmId > 0)
						{
							HAPI_ParmInfo parmInfo = myObjectControl.myParms[rotateParmId];
							ry = parmInfo.floatValue[1];
						}
					}
					
					if(bindingInfo.handleParmName == "rz")
					{
						rotateParmId = bindingInfo.assetParmId;
						if(rotateParmId > 0)
						{
							HAPI_ParmInfo parmInfo = myObjectControl.myParms[rotateParmId];
							rz = parmInfo.floatValue[2];
						}
					}
					
					if(bindingInfo.handleParmName == "sx")
					{
						scaleParmId = bindingInfo.assetParmId;
						if(scaleParmId > 0)
						{
							HAPI_ParmInfo parmInfo = myObjectControl.myParms[scaleParmId];
							sx = parmInfo.floatValue[0];
						}
					}

					if(bindingInfo.handleParmName == "sy")
					{
						scaleParmId = bindingInfo.assetParmId;
						if(scaleParmId > 0)
						{
							HAPI_ParmInfo parmInfo = myObjectControl.myParms[scaleParmId];
							sy = parmInfo.floatValue[1];
						}
					}
					
					if(bindingInfo.handleParmName == "sz")
					{
						scaleParmId = bindingInfo.assetParmId;
						if(scaleParmId > 0)
						{
							HAPI_ParmInfo parmInfo = myObjectControl.myParms[scaleParmId];
							sz = parmInfo.floatValue[2];
						}
					}
																
				}
				
				Vector3 translate = new Vector3(tx, ty, tz);
				
				//TODO: FIXME - This is wrong, Unity assumes ZXY rotation order, so this only works if we set
				//our rotation order to match.
				Quaternion rotate = Quaternion.Euler(rx, ry, rz);
				
				Vector3 scale = new Vector3( sx, sy, sz);
				
				
				if( myManipMode == XformManipMode.Translate )
				{
					if( translateParmId > 0)
					{
						Vector3 newPos = Handles.PositionHandle( translate, rotate );
						
						myObjectControl.myParms[translateParmId].floatValue[0] = newPos.x;
						myObjectControl.myParms[translateParmId].floatValue[1] = newPos.y;
						myObjectControl.myParms[translateParmId].floatValue[2] = newPos.z;
					}
				}
				else if( myManipMode == XformManipMode.Rotate )
				{
					if( rotateParmId > 0 )
					{
						Quaternion newRotQuat = Handles.RotationHandle( rotate, translate );
						
						Vector3 newRot = newRotQuat.eulerAngles;
						
						myObjectControl.myParms[rotateParmId].floatValue[0] = newRot.x;
						myObjectControl.myParms[rotateParmId].floatValue[1] = newRot.y;
						myObjectControl.myParms[rotateParmId].floatValue[2] = newRot.z;
					}
					
				}
				else if( myManipMode == XformManipMode.Scale )
				{
					if( scaleParmId > 0 )
					{
						Vector3 newScale = Handles.ScaleHandle( scale, translate, rotate, 1.0f);
						myObjectControl.myParms[scaleParmId].floatValue[0] = newScale.x;
						myObjectControl.myParms[scaleParmId].floatValue[1] = newScale.y;
						myObjectControl.myParms[scaleParmId].floatValue[2] = newScale.z;							
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
					
	
}
