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
