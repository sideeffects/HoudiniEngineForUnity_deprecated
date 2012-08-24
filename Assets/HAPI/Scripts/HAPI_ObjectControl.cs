/*
 * PROPRIETARY INFORMATION.  This software is proprietary to
 * Side Effects Software Inc., and is not to be reproduced,
 * transmitted, or disclosed in any way without written permission.
 *
 * Produced by:
 *      Side Effects Software Inc
 *		123 Front Street West, Suite 1401
 *		Toronto, Ontario
 *		Canada   M5J 2M2
 *		416-504-9876
 *
 * COMMENTS:
 */

#define DEBUG // since Unity doesn't seem to define it itself

using UnityEngine;
using UnityEditor;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using HAPI;

/// <summary>
/// 	Main script attached to an Unity game object that corresponds to a Houdini asset instance on the 
/// 	Houdini side.
/// </summary>
public class HAPI_ObjectControl : MonoBehaviour {	
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods
	
	/// <summary>
	/// 	Initializes a new instance of the <see cref="HAPI_ObjectControl"/> class.
	/// </summary>
	public HAPI_ObjectControl() 
	{
		Debug.Log( "HAPI_ObjectControl created!" );
		
		myAssetPath 				= "";
		myAssetPathChanged 			= true;
		myAssetId 					= -1;
		
		myObjectCount 				= 0;
		myParmCount 				= 0;
		
		myShowAssetControls 		= true;
		myShowObjectControls 		= true;
				
		myFolderListSelections 		= new List< int >();
		myFolderListSelectionIds 	= new List< int >();
		
		myFolderListSelections.Add( 0 );
		myFolderListSelectionIds.Add( -1 );
	}
	
	/// <summary>
	/// 	Releases unmanaged resources and performs other cleanup operations before the 
	/// 	<see cref="HAPI_ObjectControl"/> is reclaimed by garbage collection.
	/// </summary>
	~HAPI_ObjectControl() 
	{
		Debug.Log( "HAPI_ObjectControl destroyed!" );
		
		if ( myAssetId > 0 )
			HAPI_Host.unloadOTL( myAssetId );
	}
	
	/// <summary>
	/// 	Sets the asset path.
	/// </summary>
	/// <param name="path">
	/// 	New asset path (absolute path to the .otl file).
	/// </param>
	/// <returns>
	/// 	Whether the path was changed or not. If the new path is the same as the old path, no change will
	/// 	be made and <c>false</c> will be returned.
	/// </returns>
	public bool setAssetPath( string path ) {
		if ( path != myAssetPath ) 
		{
			myAssetPath = path;
			myAssetPathChanged = true;
		}
		return myAssetPathChanged;
	}
	
	/// <summary>
	/// 	Gets the asset .otl path.
	/// </summary>
	/// <returns>
	/// 	The asset .otl path.
	/// </returns>
	public string getAssetPath() {
		return myAssetPath;	
	}
	
	/// <summary>
	/// 	If the asset path has changed, unload the current asset and re-load from the new file along with
	/// 	all parameter information. Either way, push parameter values back to Houdini and rebuild
	///		all object geometries.
	/// </summary>
	public void build() {
		if ( myAssetPathChanged ) {
			HAPI_Host.unloadOTL( myAssetId );
			
			myAssetInfo 	= HAPI_Host.loadOTL( myAssetPath );
						
			// For convinience we copy some asset info properties locally (since they are constant anyway).
			myAssetId 		= myAssetInfo.id;
			myObjectCount 	= myAssetInfo.objectCount;
			myParmCount 	= myAssetInfo.parmCount;
						
			// Get all parameters.
			myParms 		= new HAPI_ParmInfo[ myParmCount ];
			HAPI_Host.HAPI_GetParameters( myAssetId, myParms, 0, myParmCount );
			
			myAssetPathChanged = false;
		}
		else
		{
			// Set all parameter values.
			HAPI_Host.HAPI_SetParameters( myAssetId, myParms, 0, myParmCount );	
		}
			
		// Clean up.
		destroyChildren();
		
		// If asset file path was invalid, don't try to get more info.
		if ( myAssetId < 0 )
			return;
		
		// Create local object info caches (transforms need to be stored in a parallel array).
		myObjects 			= new HAPI_ObjectInfo[ myObjectCount ];
		myObjectTransforms 	= new HAPI_Transform[ myObjectCount ];
		
		HAPI_Host.HAPI_GetObjects( 				myAssetId, myObjects, 0, myObjectCount );
		HAPI_Host.HAPI_GetObjectTransforms( 	myAssetId, myObjectTransforms, 0, myObjectCount );
		
		for ( int object_index = 0; object_index < myObjectCount; ++object_index )
			createObject( object_index );
	}
		
	public string 				myAssetPath;
	public int 					myAssetId;
	public int 					myParmCount;
	public int 					myObjectCount;
	
	public HAPI_AssetInfo 		myAssetInfo;
	public HAPI_ObjectInfo[] 	myObjects;
	public HAPI_Transform[] 	myObjectTransforms;
	public HAPI_ParmInfo[] 		myParms;
	
	public bool 				myShowObjectControls;
	public bool 				myShowAssetControls;
	
	/// <summary>
	/// 	Indices of the currently selected folders in the Inspector.
	/// 	A 1:1 mapping with myFolderListSelectionIds.
	/// </summary>
	public List< int > 			myFolderListSelections;
	
	/// <summary>
	/// 	Parameter ids of the currently selected folders in the Inspector. 
	/// 	A 1:1 mapping with myFolderListSelections.
	/// </summary>
	public List< int > 			myFolderListSelectionIds;
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private Methods
	
	/// <summary>
	/// 	Recursively delete all children of this Unity asset.
	/// </summary>
	private void destroyChildren() 
	{
		List< GameObject > children = new List< GameObject >();
		
		foreach ( Transform child in transform )
			children.Add( child.gameObject );
		
		foreach ( GameObject child in children )
			DestroyImmediate( child );
	}
	
	/// <summary>
	/// 	Instantiate a game object corresponding to a Houdini object of this asset, get geometry information
	/// 	on the object and re-create the geometry on the Unity side.
	/// </summary>
	/// <param name="object_id">
	/// 	Object_id as returned by <see cref="GetObjects"/>.
	/// </param>
	private void createObject( int object_id )
	{
		HAPI_ObjectInfo object_info = myObjects[ object_id ];
		
		// Create main underling.
		GameObject main_child = new GameObject( object_info.name );
		main_child.transform.parent = transform;
		
		// Add required components.
		main_child.AddComponent( "MeshFilter" );
		main_child.AddComponent( "MeshRenderer" );
		main_child.AddComponent( "HAPI_ChildSelectionControl" );
		
		// Set diffuse material.
		Material diffuse = new Material( Shader.Find( "Diffuse" ) );		
		main_child.GetComponent< MeshRenderer >().material = diffuse;
		
		// Get or create mesh.
		MeshFilter main_child_mesh_filter 	= main_child.GetComponent< MeshFilter >();
		Mesh main_child_mesh 				= main_child_mesh_filter.sharedMesh;
		if ( main_child_mesh == null ) 
		{
			main_child_mesh_filter.mesh 	= new Mesh();
			main_child_mesh 				= main_child_mesh_filter.sharedMesh;
		}
		
		main_child_mesh.Clear();
		
		// Get transforms.
		HAPI_Transform trans = myObjectTransforms[ object_id ];
				
		// Get geometry.
		HAPI_GeometryInfo geo = new HAPI_GeometryInfo();
		HAPI_Host.HAPI_GetGeometryInfo( myAssetId, object_id, out geo );
		Debug.Log( "Obj #" + object_id + ": verts: " + geo.vertexCount + " prims: " + geo.primCount );		
		
		// Make sure our primitive and vertex numbers are supported by Unity.
		// TODO: add this limit in a more proper place
		geo.primCount 		= Mathf.Min( geo.primCount, 65000 * 3 );
		geo.vertexCount 	= Mathf.Min( geo.vertexCount, 65000 );
		
		// Apply object transforms.
		main_child.transform.position 	= new Vector3( 			trans.position[ 0 ], 
																trans.position[ 1 ],
																trans.position[ 2 ] );
		main_child.transform.rotation 	= Quaternion.Euler(    -trans.pitch, 
														 	   -trans.yaw, 
														  		trans.roll );
		main_child.transform.localScale = new Vector3( 			trans.scale[ 0 ], 
													  			trans.scale[ 1 ], 
													  			trans.scale[ 2 ] );
		
		// Get geometry data.
		HAPI_RawVertex[] raw_vertices 		= new HAPI_RawVertex[ geo.vertexCount ];
		HAPI_RawPrimitive[] raw_primitives 	= new HAPI_RawPrimitive[ geo.primCount ];
		//HAPI_RawInstance[] raw_instances 	= new HAPI_RawInstance[ geo.instanceCount ];
		
		fillArray( myAssetId, object_id, raw_vertices, HAPI_Host.HAPI_GetVertexArray, geo.vertexCount );
		fillArray( myAssetId, object_id, raw_primitives, HAPI_Host.HAPI_GetPrimitveArray, geo.primCount );
		//fillArray( myAssetId, object_id, raw_instances, HAPI_Host.GetInstanceArray, geo.instanceCount );
		
		// Create Unity-specific data objects.
		Vector3[] vertices 	= new Vector3[ geo.vertexCount ];
		int[] triangles 	= new int[ geo.primCount * 3 ];
		Vector2[] uvs 		= new Vector2[ geo.vertexCount ];
		Vector3[] normals 	= new Vector3[ geo.vertexCount ];
		
		// Fill Unity-specific data objects with data from the runtime.
		for ( int i = 0; i < geo.vertexCount; ++i ) 
		{
			for ( int j = 0; j < 3; ++j ) 
			{
				vertices[ i ][ j ] 		= raw_vertices[ i ].position[ j ];
				normals[ i ][ j ] 		= raw_vertices[ i ].normal[ j ];
			}
			for ( int j = 0; j < 2; ++j )
				uvs[ i ][ j ] 			= raw_vertices[ i ].uv[ j ];
		}
		for ( int i = 0; i < geo.primCount; ++i ) 
			for ( int j = 0; j < 3; ++j )
				triangles[ i * 3 + j ] 	= raw_primitives[ i ].vertices[ j ];
		
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
		
		// Load into vertices and face into mesh.
		main_child_mesh.vertices 	= vertices;
		main_child_mesh.triangles 	= triangles;
		main_child_mesh.uv 			= uvs;
		main_child_mesh.normals 	= normals;
		
		main_child_mesh.RecalculateBounds();
	}
	
	/// <summary>
	/// 	Function pointer parameter type for <see cref="fillArray"/>. 
	/// </summary>
	private delegate int fillArrayInputFunc< T >( int asset_id, int object_id, [Out] T[] items, int start, int end );
	
	/// <summary>
	/// 	Fills an array of structs incrementally, never transferring too long of a data stream at once.
	/// </summary>
	/// <typeparam name="T">
	/// 	Array item type.
	/// </typeparam>
	/// <param name="asset_id">
	/// 	Asset_id as returned by <see cref="LoadOTLFile"/>.
	/// </param>
	/// <param name="object_id">
	/// 	Object_id as returned by <see cref="GetObjects"/>.
	/// </param>
	/// <param name="items">
	/// 	Array of items to be filled.
	/// </param>
	/// <param name="get_func">
	/// 	Function used to fill the item array. Must match the signature of <see cref="fillArrayInputFunc"/>.
	/// </param>
	/// <param name="count">
	/// 	Total number of items (size of <c>items</c> array).
	/// </param>
	private void fillArray< T >( int asset_id, int object_id, T[] items, fillArrayInputFunc< T > get_func, int count ) 
	{
		// TODO: This number works well but it depends heavily on the size of T. 
		// Should set it in a smarter way. Via sizeOf( T )?
		const int max_array_size = 8000;
		
		int local_count = count;
		int current_index = 0;
		
		while ( local_count > 0 ) 
		{			
			int delta = 0;
			if ( local_count > max_array_size ) 
			{
				delta = max_array_size;
				local_count -= max_array_size;
			} 
			else 
			{
				delta = local_count;
				local_count = 0;
			}
			
			T[] local_array = new T[ delta ];
			get_func( asset_id, object_id, local_array, current_index, delta );
			
			// Copy data from the temporary
			for ( int i = current_index; i < current_index + delta; ++i )				
				items[ i ] = local_array[ i - current_index ];
			
			current_index += delta;
		}
	}
		
#if DEBUG
	public bool myAssetPathChanged;
#else
	pivate bool myAssetPathChanged;
#endif
	
}
