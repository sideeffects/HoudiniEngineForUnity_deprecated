using UnityEngine;
using System.Collections;

using HAPI;

public class HAPI_InstancerOverrideInfo : ScriptableObject
{
	public Vector3 translate = new Vector3( 0, 0, 0 );
	public Vector3 rotate = new Vector3( 0, 0, 0 );
	public Vector3 scale = new Vector3( 0, 0, 0 );
	public string objectToInstantiatePath;
	public int instancePointNumber = -1;
}

