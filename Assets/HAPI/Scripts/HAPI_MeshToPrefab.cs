using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR
using System.Collections.Generic;

using HAPI;

public class HAPI_MeshToPrefab : MonoBehaviour {
#if UNITY_STANDALONE_WIN
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	
	public GameObject prGameObject { get; set; }
	public string prMeshName { get; set; }
	public string prPathToPrefab { get; set; }
		
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods			
	
	public HAPI_MeshToPrefab () 
	{
		prGameObject = null;
		prPathToPrefab = "Assets/";
	}

	public void SaveToPrefab() 
	{
#if UNITY_EDITOR
		if( prPathToPrefab == "" )
		{
			Debug.LogError("Please specify a valid path to the prefab");
			return;
		}		
		
		if ( !prPathToPrefab.StartsWith("Assets/") )
		{
			int index = prPathToPrefab.IndexOf("/Assets/");
			
			if( index < 0)
			{
				Debug.LogError("Please specify a valid path to the prefab that is in the Assets folder.");
				return;
				
			}
						
			prPathToPrefab = "Assets/" + prPathToPrefab.Substring(index + "/Assets/".Length);
		}
		
		if( !prPathToPrefab.EndsWith(".prefab") )
			prPathToPrefab += ".prefab";
		
		Object prefab = AssetDatabase.LoadAssetAtPath(prPathToPrefab, typeof(GameObject));
	    if ( !prefab ) 
		{
	        prefab = PrefabUtility.CreateEmptyPrefab( prPathToPrefab );
	    }
		
		GameObject targetGameObj = prGameObject;
		
		Mesh inputMesh =  targetGameObj.GetComponent<MeshFilter>().sharedMesh;
		
		Mesh mesh = new Mesh();
		mesh.vertices = inputMesh.vertices;
		mesh.uv = inputMesh.uv;
		mesh.triangles = inputMesh.triangles;
		mesh.normals = inputMesh.normals;
		mesh.name = prMeshName;
	
		AssetDatabase.AddObjectToAsset( mesh , prefab );
		AssetDatabase.SaveAssets();
		
		
		/*
		// Alternate method to save the mesh: uses an existing prefab that has all the other
		// things set, and a place holder mesh which we then replace.
	    // clone the model template
	    Object templatePrefab = AssetDatabase.LoadAssetAtPath(modelTemplate, typeof(GameObject));
	    GameObject template = (GameObject)PrefabUtility.InstantiatePrefab(templatePrefab);
	
	    // this way links will persist when we regenerate the mesh
	    Object prefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
	    if (!prefab) {
	        prefab = PrefabUtility.CreateEmptyPrefab( prefabPath );
	    }
	
	    // sort of the same...
	    Mesh mesh = (Mesh)AssetDatabase.LoadAssetAtPath(prefabPath, typeof(Mesh));
	    if (!mesh) {
	        mesh = new Mesh();
	        mesh.name = name;
	        AssetDatabase.AddObjectToAsset (mesh, prefabPath);
	    } else {
	        mesh.Clear();
	    }
	    // generate your mesh in place
		mesh.vertices 	= inputMesh.vertices;
		mesh.triangles 	= inputMesh.triangles;
		mesh.uv 			= inputMesh.uv;
		mesh.normals 	= inputMesh.normals;
	    
	    // assume that MeshFilter is already there. could check and AddComponent
	    template.GetComponent<MeshFilter>().sharedMesh = mesh;
	
	    // make sure 
	    EditorUtility.ReplacePrefab(template, prefab, ReplacePrefabOptions.ReplaceNameBased);
	    // get rid of the temporary object (otherwise it stays over in scene)
	    Object.DestroyImmediate(template);
	    */
#endif // UNITY_EDITOR
	}
	
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Overrides
	
	// Use this for initialization
	
	
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private Members
	
	
#endif // UNITY_STANDALONE_WIN
}
