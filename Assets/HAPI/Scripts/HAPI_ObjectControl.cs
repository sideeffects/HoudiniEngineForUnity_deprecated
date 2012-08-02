using UnityEngine;
using UnityEditor;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using HAPI;

using System.Text;

public class HAPI_ObjectControl : MonoBehaviour {	
	
	//
	// Public Methods
	//
	
	public HAPI_ObjectControl() {
		Debug.Log( "HAPI_ObjectControl created!" );
		
		myAssetPath = "";
		myAssetPathChanged = false;
		myAssetId = -1;
		
		myShowAssetControls = true;
		myShowObjectControls = true;
		myBoxSize = 1f;
				
	}
	
	public bool SetAssetPath( string path ) {
		if ( path != myAssetPath ) {
			myAssetPath = path;
			myAssetPathChanged = true;
		}
		return myAssetPathChanged;
	}
	
	public string GetAssetPath() {
		return myAssetPath;	
	}
	
	public void Build() {
		if ( myAssetPathChanged ) {
			HAPI_Host.UnloadOTL( myAssetId );
			myAssetId = HAPI_Host.LoadOTL( myAssetPath );
		}
			
		// clean up
		DestroyChildren();
		
		// if path was invalid, don't try to get more info
		if ( myAssetId < 0 )
			return;
		
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
		HAPI_Host.HAPI_GetGeometryInfo( myAssetId, 0, out geo );
		
		Debug.Log( "Prim Count: " + geo.primCount );
		Debug.Log( "Vertex Count: " + geo.vertexCount );
		
		geo.primCount = Mathf.Min( geo.primCount, 65000 * 3 );
		geo.vertexCount = Mathf.Min( geo.vertexCount, 65000 );
		
		//transform.rotation = Quaternion.Euler( -geo.pitch, -geo.yaw, geo.roll );		
		//transform.localScale = new Vector3( geo.scale[ 0 ], geo.scale[ 1 ], geo.scale[ 2 ] );
		//transform.localScale = new Vector3( myBoxSize, myBoxSize, myBoxSize );
		mainChild.transform.rotation = Quaternion.Euler( -geo.pitch, -geo.yaw, geo.roll );
		mainChild.transform.localScale = new Vector3( myBoxSize, myBoxSize, myBoxSize );
		
		// get geometry data
		HAPI_RawVertex[] rawVertices = new HAPI_RawVertex[ geo.vertexCount ];
		HAPI_RawPrimitive[] rawPrimitives = new HAPI_RawPrimitive[ geo.primCount ];
		//HAPI_RawInstance[] rawInstances = new HAPI_RawInstance[ geo.instanceCount ];
		
		FillArray( myAssetId, 0, rawVertices, HAPI_Host.HAPI_GetVertexArray, geo.vertexCount );
		FillArray( myAssetId, 0, rawPrimitives, HAPI_Host.HAPI_GetPrimitveArray, geo.primCount );
		
		//HAPI_Host.HAPI_GetVertexArray( myAssetId, 0, rawVertices, 0, 100 );
		//HAPI_Host.HAPI_GetPrimitveArray( myAssetId, 0, rawPrimitives, 0, geo.primCount );
		//HAPI_Host.GetInstanceArray( myAssetId, 0, rawInstances, geo.instanceCount );
		
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
		/*
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
		*/
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
	
	public string myAssetPath;
	
	public bool myShowObjectControls;
	public bool myShowAssetControls;
	public float myBoxSize;
	
	//
	// Private Methods
	//
	
	private void DestroyChildren() {
		List< GameObject > children = new List< GameObject >();
		
		foreach ( Transform child in transform ) {
			children.Add ( child.gameObject );
		}
		
		foreach ( GameObject child in children ) {
			DestroyImmediate( child );
		}
	}
	
	private delegate int FillArrayInputFunc< T >( int assetId, int objectId, [Out] T[] items, int start, int end );
	private void FillArray< T >( int assetId, int objectId, T[] items, FillArrayInputFunc< T > getFunc, int count ) {
		const int maxArraySize = 8000;
		
		int localCount = count;
		int currentIndex = 0;
		
		while ( localCount > 0 ) {			
			int delta = 0;
			if ( localCount > maxArraySize ) {
				delta = maxArraySize;
				localCount -= maxArraySize;
			} else {
				delta = localCount;
				localCount = 0;
			}
			
			//Debug.Log( "currentIndex: " + currentIndex + ", delta: " + delta );
			T[] localArray = new T[ delta ];
			getFunc( assetId, objectId, localArray, currentIndex, delta );
			
			for ( int i = currentIndex; i < currentIndex + delta; ++i ) {				
				items[ i ] = localArray[ i - currentIndex ];
			}
			
			currentIndex += delta;
		}
	}
	
	//
	// Private Variables
	//
	
	private bool myAssetPathChanged;
	private int myAssetId;
}
