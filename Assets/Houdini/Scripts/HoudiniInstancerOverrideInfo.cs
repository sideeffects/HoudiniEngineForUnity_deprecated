using UnityEngine;
using System.Collections;

public class HoudiniInstancerOverrideInfo : ScriptableObject
{
	public Vector3 translate = new Vector3( 0, 0, 0 );
	public Vector3 rotate = new Vector3( 0, 0, 0 );
	public Vector3 scale = new Vector3( 0, 0, 0 );
	public GameObject objectToInstantiate = null;
	public string objectToInstantiatePath;
	public int instancePointNumber = -1;
}

