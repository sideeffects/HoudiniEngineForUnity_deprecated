using UnityEngine;
using UnityEditor;
using System.Collections;

public class HAPI_ChildSelectionControl : MonoBehaviour {

	public void SelectParent()
	{
		GameObject[] selection = new GameObject[ 1 ];
		selection[ 0 ] = transform.parent.gameObject;
		Selection.objects = selection;
	}
}
