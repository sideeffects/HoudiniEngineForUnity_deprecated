#define DEBUG // since Unity doesn't seem to define it itself

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
		myAssetPathChanged = true;
		myAssetId = -1;
		
		myObjectCount = 0;
		myParameterCount = 0;
		
		myShowAssetControls = true;
		myShowObjectControls = true;
		myAssetTabSelectedIndex = 0;
	}
	
	~HAPI_ObjectControl() {
		Debug.Log( "HAPI_ObjectControl destroyed!" );
		
		if ( myAssetId > 0 )
		{
			HAPI_Host.UnloadOTL( myAssetId );	
		}
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
			
			myAssetInfo = HAPI_Host.LoadOTL( myAssetPath );
			
			// for convinience
			myAssetId = myAssetInfo.id;
			myObjectCount = myAssetInfo.objectCount;
			myParameterCount = myAssetInfo.parameterCount;
						
			// get parameters
			myParameters = new HAPI_Parameter[ myParameterCount ];
			HAPI_Host.HAPI_GetParameters( myAssetId, myParameters, 0, myParameterCount );
			
			myAssetPathChanged = false;
		}
		else
		{
			// set parameters
			HAPI_Host.HAPI_SetParameters( myAssetId, myParameters, 0, myParameterCount );	
		}
			
		// clean up
		DestroyChildren();
		
		// if path was invalid, don't try to get more info
		if ( myAssetId < 0 )
			return;
		
		myObjects = new HAPI_ObjectInfo[ myObjectCount ];
		myObjectTransforms = new HAPI_Transform[ myObjectCount ];
		
		HAPI_Host.HAPI_GetObjects( myAssetId, myObjects, 0, myObjectCount );
		HAPI_Host.HAPI_GetObjectTransforms( myAssetId, myObjectTransforms, 0, myObjectCount );
				
		for ( int objectIndex = 0; objectIndex < myObjectCount; ++objectIndex )
			CreateObject( objectIndex );
	}	
	
	// Use this for initialization
	public void Start() {		
		
	}
	
	// Update is called once per frame
	public void Update() {
		
	}
	
	// Public Variables
	 
	public string myAssetPath;
	public int myAssetId;
	public int myParameterCount;
	public int myObjectCount;
	
	public HAPI_AssetInfo myAssetInfo;
	public HAPI_ObjectInfo[] myObjects;
	public HAPI_Transform[] myObjectTransforms;
	public HAPI_Parameter[] myParameters;
	
	public bool myShowObjectControls;
	public bool myShowAssetControls;
	public int myAssetTabSelectedIndex;
	
	//
	// Private Methods
	//
	
	private void DestroyChildren() {
		List< GameObject > children = new List< GameObject >();
		
		foreach ( Transform child in transform ) {
			children.Add( child.gameObject );
		}
		
		foreach ( GameObject child in children ) {
			DestroyImmediate( child );
		}
	}
		
	private void CreateObject( int objectId )
	{
		// create main underling
		GameObject mainChild = new GameObject( "HAPI_Geo" + objectId );
		mainChild.transform.parent = transform;
		
		// add required components
		mainChild.AddComponent( "MeshFilter" );
		mainChild.AddComponent( "MeshRenderer" );
		mainChild.AddComponent( "HAPI_ChildSelectionControl" );
		
		// get or create mesh
		MeshFilter mainChildMeshFilter = mainChild.GetComponent< MeshFilter >();
		Mesh mainChildMesh = mainChildMeshFilter.sharedMesh;
		if ( mainChildMesh == null ) {
			mainChildMeshFilter.mesh = new Mesh();
			mainChildMesh = mainChildMeshFilter.sharedMesh;
		}
		
		mainChildMesh.Clear();
		
		// get transforms
		HAPI_Transform trans = myObjectTransforms[ objectId ];
				
		// get geometry
		HAPI_RawGeometry geo = new HAPI_RawGeometry();
		HAPI_Host.HAPI_GetGeometryInfo( myAssetId, objectId, out geo );
		
		// TODO: add this limit in a more proper place
		geo.primCount = Mathf.Min( geo.primCount, 65000 * 3 );
		geo.vertexCount = Mathf.Min( geo.vertexCount, 65000 );
		
		// apply object transforms
		mainChild.transform.position = new Vector3( trans.position[ 0 ], 
													trans.position[ 1 ],
													trans.position[ 2 ] );
		mainChild.transform.rotation = Quaternion.Euler( -trans.pitch, 
														 -trans.yaw, 
														  trans.roll );
		mainChild.transform.localScale = new Vector3( trans.scale[ 0 ], 
													  trans.scale[ 1 ], 
													  trans.scale[ 2 ] );
		
		// get geometry data
		HAPI_RawVertex[] rawVertices = new HAPI_RawVertex[ geo.vertexCount ];
		HAPI_RawPrimitive[] rawPrimitives = new HAPI_RawPrimitive[ geo.primCount ];
		//HAPI_RawInstance[] rawInstances = new HAPI_RawInstance[ geo.instanceCount ];
		
		FillArray( myAssetId, objectId, rawVertices, HAPI_Host.HAPI_GetVertexArray, geo.vertexCount );
		FillArray( myAssetId, objectId, rawPrimitives, HAPI_Host.HAPI_GetPrimitveArray, geo.primCount );
		//FillArray( myAssetId, objectId, rawInstances, HAPI_Host.GetInstanceArray, geo.instanceCount );
		
		// create data objects
		Vector3[] vertices = new Vector3[ geo.vertexCount ];
		int[] triangles = new int[ geo.primCount * 3 ];
		Vector2[] uvs = new Vector2[ geo.vertexCount ];
		
		for ( int i = 0; i < geo.vertexCount; ++i ) {
			for ( int j = 0; j < 3; ++j ) {
				vertices[ i ][ j ] = rawVertices[ i ].position[ j ];	
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
			Vector3 scale = new Vector3( 1.0f, 1.0f, 1.0f );
			
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
	
#if DEBUG
	public bool myAssetPathChanged;
#else
	pivate bool myAssetPathChanged;
#endif
}
