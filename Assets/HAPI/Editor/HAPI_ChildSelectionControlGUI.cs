using UnityEngine;
using UnityEditor;
using System.Collections;

[ CustomEditor( typeof( HAPI_ChildSelectionControl ) ) ]
public class HAPI_ChildSelectionControlGUI : Editor 
{
	public void OnSceneGUI()
	{
		HAPI_ChildSelectionControl control = target as HAPI_ChildSelectionControl;		
		control.SelectParent();
	}
}
