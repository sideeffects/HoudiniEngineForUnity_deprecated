using UnityEngine;
using UnityEditor;
using System.Collections;

[ CustomEditor( typeof( TestScript ) ) ]
public class TestGUI : Editor {

	void OnEnable()
	{
		TestScript t = target as TestScript;
		Debug.Log( "Name: " + t.name );
	}
}
