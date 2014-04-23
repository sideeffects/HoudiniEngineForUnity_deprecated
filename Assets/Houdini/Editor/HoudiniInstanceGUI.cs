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

[ CustomEditor( typeof( HoudiniInstance ) ) ]
[ CanEditMultipleObjects ]
public class HoudiniInstanceGUI : Editor 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public

	public void OnSceneGUI()
	{
		HoudiniInstance instance = target as HoudiniInstance;

		
		HoudiniInstancer instancer = instance.prInstancer;
		if ( instancer == null )
			return;
		
		bool is_overridden = instancer.isPointOverridden( instance.prInstancePointNumber );
		
		if ( is_overridden )
			instancer.drawPin( instance.prInstancePointNumber );

		Event curr_event = Event.current;

		if (
			curr_event.isMouse && curr_event.type == EventType.MouseDown &&
			HoudiniHost.prAutoPinInstances )
		{
			instance.prTransformChanged = false;
		}
		else if (
			curr_event.isMouse && curr_event.type == EventType.MouseUp &&
			HoudiniHost.prAutoPinInstances && instance.prTransformChanged )
		{
			instancer.pinObject( instance.gameObject, true );
			instance.prTransformChanged = false;
			Repaint();
		}
	}

	public virtual void OnEnable() 
	{
		myInstance = target as HoudiniInstance;
	}


	public override void OnInspectorGUI() 
	{

		Object[] selection = Selection.objects;
		HoudiniInstancer instancer = myInstance.prInstancer;
		if ( selection.Length > 0 )
		{
			if ( !HoudiniHost.prAutoPinInstances && GUILayout.Button( "Pin Selection" ) )
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
			else if ( !HoudiniHost.prAutoPinInstances && GUILayout.Button( "Pin Instance" ) )
				instancer.pinObject( myInstance.gameObject, true );
		}

	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private

	[SerializeField] private HoudiniInstance myInstance = null;
}
