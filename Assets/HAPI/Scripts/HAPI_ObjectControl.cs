using UnityEngine;
using UnityEditor;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using HAPI;

//[ RequireComponent ( typeof( MeshFilter ) ) ]
//[ RequireComponent ( typeof( MeshRenderer ) ) ]
public class HAPI_ObjectControl : MonoBehaviour {	
	
	//
	// Public Methods
	//
	
	public void OnEnable() {
		myBoxSize = 1f;
		
		Build();
	}
	
	public void Build() {
		// clean up
		DestroyChildren();
		
		// create main underling
		GameObject mainChild = new GameObject( "HAPI_MainGeo" );
		mainChild.transform.parent = transform;
		
		mainChild.AddComponent( "MeshFilter" );
		mainChild.AddComponent( "MeshRenderer" );
		
		// get or create mesh
		MeshFilter mainChildMeshFilter = mainChild.GetComponent< MeshFilter >();
		Mesh mainChildMesh = mainChildMeshFilter.sharedMesh;
		if ( mainChildMesh == null ) {
			mainChildMeshFilter.mesh = new Mesh();
			mainChildMesh = mainChildMeshFilter.sharedMesh;
		}
		
		mainChildMesh.Clear();
				
		// get geometry
		HAPI_RawGeometry geo = new HAPI_RawGeometry();
		GetGeometry( out geo );
		
		//transform.rotation = Quaternion.Euler( -geo.pitch, -geo.yaw, geo.roll );		
		//transform.localScale = new Vector3( geo.scale[ 0 ], geo.scale[ 1 ], geo.scale[ 2 ] );
		//transform.localScale = new Vector3( myBoxSize, myBoxSize, myBoxSize );
		mainChild.transform.rotation = Quaternion.Euler( -geo.pitch, -geo.yaw, geo.roll );
		mainChild.transform.localScale = new Vector3( myBoxSize, myBoxSize, myBoxSize );
		
		// get geometry data
		HAPI_RawVertex[] rawVertices = new HAPI_RawVertex[ geo.vertexCount ];
		HAPI_RawPrimitive[] rawPrimitives = new HAPI_RawPrimitive[ geo.primCount ];
		HAPI_RawInstance[] rawInstances = new HAPI_RawInstance[ geo.instanceCount ];
		
		GetVertexArray( rawVertices, 0, geo.vertexCount );
		GetPrimitveArray( rawPrimitives, 0, geo.primCount );
		GetInstanceArray( rawInstances, geo.instanceCount );
		
		// create data objects
		Vector3[] vertices = new Vector3[ geo.vertexCount ];
		int[] triangles = new int[ geo.primCount * 3 ];
		Vector2[] uvs = new Vector2[ geo.vertexCount ];
		
		for ( int i = 0; i < geo.vertexCount; ++i ) {
			for ( int j = 0; j < 3; ++j ) {
				vertices[ i ][ j ] = rawVertices[ i ].point[ j ];	
			}
		}
		for ( int i = 0; i < geo.primCount; ++i ) {
			for ( int j = 0; j < 3; ++j ) {
				triangles[ i * 3 + j ] = rawPrimitives[ i ].vertices[ j ];	
			}
		}
		for ( int i = 0; i < geo.instanceCount; ++i ) {
			Vector3 position = new Vector3( rawInstances[ i ].position[ 0 ], rawInstances[ i ].position[ 1 ], rawInstances[ i ].position[ 2 ] );
			Quaternion rotation = Quaternion.Euler( -rawInstances[ i ].pitch, -rawInstances[ i ].yaw, rawInstances[ i ].roll );
			//Vector3 scale = new Vector3( rawInstances[ i ].scale[ 0 ], rawInstances[ i ].scale[ 1 ], rawInstances[ i ].scale[ 2 ] );
			Vector3 scale = new Vector3( myBoxSize, myBoxSize, myBoxSize );
			
			GameObject instance = Instantiate( mainChild, position, rotation ) as GameObject;
			
			instance.name = "HAPI_InstanceGeo " + i.ToString();
			instance.transform.parent = transform;
			instance.transform.localScale = scale;
		}
		
		// load into vertices and face into mesh
		mainChildMesh.vertices = vertices;
		mainChildMesh.triangles = triangles;
		mainChildMesh.uv = uvs;
		
		mainChildMesh.RecalculateBounds();
		mainChildMesh.RecalculateNormals();
	}
	
	// Use this for initialization
	public void Start() {		
		
	}
	
	// Update is called once per frame
	public void Update() {
		
	}
	
	// Public Variables
	
	public float myBoxSize;	
	
	//
	// Private Methods
	//
	
	[ DllImport( "DummyDLL" ) ]
	private static extern int GetGeometry( out HAPI_RawGeometry geo );
	
	[ DllImport( "DummyDLL" ) ]
	private static extern int GetVertexArray( [Out] HAPI_RawVertex[] vertices, int start, int end );
	
	[ DllImport( "DummyDLL" ) ]
	private static extern int GetPrimitveArray( [Out] HAPI_RawPrimitive[] primitives, int start, int end );
	
	[ DllImport( "DummyDLL" ) ]
	private static extern int GetInstanceArray( [Out] HAPI_RawInstance[] instances, int count );
	
	private void DestroyChildren() {
		List< GameObject > children = new List< GameObject >();
		
		foreach ( Transform child in transform ) {
			children.Add ( child.gameObject );
		}
		
		foreach ( GameObject child in children ) {
			DestroyImmediate( child );
		}
	}
	
}
