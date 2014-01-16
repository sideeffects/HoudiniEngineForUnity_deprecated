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

[ CustomEditor( typeof( HAPI_Instance ) ) ]
[ CanEditMultipleObjects ]
public class HAPI_InstanceGUI : Editor 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public

	public void OnSceneGUI()
	{
		HAPI_Instance instance = target as HAPI_Instance;

		
		HAPI_Instancer instancer = instance.prInstancer;
		if ( instancer == null )
			return;
		
		bool is_overridden = instancer.isPointOverridden( instance.prInstancePointNumber );
		
		if ( is_overridden )
			instancer.drawPin( instance.prInstancePointNumber );

		Event curr_event = Event.current;

		if (
			curr_event.isMouse && curr_event.type == EventType.MouseDown &&
			HAPI_Host.prAutoPinInstances )
		{
			instance.prTransformChanged = false;
		}
		else if (
			curr_event.isMouse && curr_event.type == EventType.MouseUp &&
			HAPI_Host.prAutoPinInstances && instance.prTransformChanged )
		{
			instancer.pinObject( instance.gameObject, true );
			instance.prTransformChanged = false;
			Repaint();
		}
	}

	public virtual void OnEnable() 
	{
		myInstance = target as HAPI_Instance;
	}


	public override void OnInspectorGUI() 
	{

		Object[] selection = Selection.objects;
		HAPI_Instancer instancer = myInstance.prInstancer;
		if ( selection.Length > 0 )
		{
			if ( !HAPI_Host.prAutoPinInstances && GUILayout.Button( "Pin Selection" ) )
				foreach( Object obj in selection )
					if ( obj.GetType() == typeof( GameObject ) )
						instancer.pinObject( (GameObject) obj, true );
			
			if ( GUILayout.Button( "UnPin Selection" ) ) 
				foreach( Object obj in selection )
					if ( obj.GetType() == typeof( GameObject ) )
						instancer.pinObject( (GameObject) obj, false );
		}
		else if ( instancer )
		{
			bool is_overridden = instancer.isPointOverridden( myInstance.prInstancePointNumber );
			if ( is_overridden && GUILayout.Button( "UnPin Instance" ) )
				instancer.pinObject( myInstance.gameObject, false );
			else if ( !HAPI_Host.prAutoPinInstances && GUILayout.Button( "Pin Instance" ) )
				instancer.pinObject( myInstance.gameObject, true );
		}

	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private

	[SerializeField] private HAPI_Instance myInstance = null;
}
