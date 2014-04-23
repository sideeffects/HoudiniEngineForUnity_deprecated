using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR
using System.Collections.Generic;

public class HoudiniMeshToPrefab : MonoBehaviour
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	
	public GameObject prGameObject { get; set; }
	public string prMeshName { get; set; }
	public string prPathToPrefab { get; set; }
		
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods
	
	public HoudiniMeshToPrefab () 
	{
		prGameObject = null;
		prPathToPrefab = "Assets/";
	}

	public void SaveToPrefab() 
	{
#if UNITY_EDITOR
		if ( prPathToPrefab == "" )
		{
			Debug.LogError( "Please specify a valid path to the prefab" );
			return;
		}
		
		if ( !prPathToPrefab.StartsWith( "Assets/" ) )
		{
			int index = prPathToPrefab.IndexOf( "/Assets/" );
			
			if ( index < 0)
			{
				Debug.LogError( "Please specify a valid path to the prefab that is in the Assets folder." );
				return;
			}

			prPathToPrefab = "Assets/" + prPathToPrefab.Substring(index + "/Assets/".Length);
		}
		
		if ( !prPathToPrefab.EndsWith(".prefab") )
			prPathToPrefab += ".prefab";
		
		Object prefab = AssetDatabase.LoadAssetAtPath(prPathToPrefab, typeof( GameObject ) );
		if ( !prefab ) 
		{
			prefab = PrefabUtility.CreateEmptyPrefab( prPathToPrefab );
		}

		GameObject targetGameObj = prGameObject;
		
		Mesh inputMesh =  targetGameObj.GetComponent< MeshFilter >().sharedMesh;
		
		Mesh mesh = new Mesh();
		mesh.vertices = inputMesh.vertices;
		mesh.uv = inputMesh.uv;
		mesh.triangles = inputMesh.triangles;
		mesh.normals = inputMesh.normals;
		mesh.name = prMeshName;
	
		AssetDatabase.AddObjectToAsset( mesh, prefab );
		AssetDatabase.SaveAssets();
#endif // UNITY_EDITOR
	}
}
