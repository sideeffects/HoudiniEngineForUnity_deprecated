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
 * 		Contains the main script attached to a Unity game object that corresponds to a Houdini asset instance on the 
 *		Houdini side. 
 * 
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
public partial class HAPI_ObjectControl : MonoBehaviour 
{	
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
		myParmChoiceCount			= 0;
		myHandleCount 				= 0;
		
		myShowAssetControls 		= true;
		myShowObjectControls 		= true;
		myAutoSelectAssetNode		= true;
				
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
	public bool setAssetPath( string path ) 
	{
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
	public string getAssetPath() 
	{
		return myAssetPath;	
	}
	
	/// <summary>
	/// 	If the asset path has changed, unload the current asset and re-load from the new file along with
	/// 	all parameter information. Either way, push parameter values back to Houdini and rebuild
	///		all object geometries.
	/// </summary>
	public bool build() 
	{
		if ( myAssetPathChanged ) 
		{
			HAPI_Host.unloadOTL( myAssetId );
			
			try
			{
				myAssetInfo = HAPI_Host.loadOTL( myAssetPath );
			}
			catch ( HAPI_Error )
			{
				// Nothing to build since the load failed.
				return false; // false for failed :)
			}
						
			// For convinience we copy some asset info properties locally (since they are constant anyway).
			myAssetId 				= myAssetInfo.id;
			myObjectCount 			= myAssetInfo.objectCount;
			myParmCount 			= myAssetInfo.parmCount;
			myParmExtraValueCount 	= myAssetInfo.parmExtraValueCount;
			myParmChoiceCount		= myAssetInfo.parmChoiceCount;
			myHandleCount 			= myAssetInfo.handleCount;
						
			// Get all parameters.
			myParms = new HAPI_ParmInfo[ myParmCount ];
			HAPI_Host.getParameters( myAssetId, myParms, 0, myParmCount );
			
			// Get any parameter extra values.
			myParmExtraValues = new HAPI_ParmSingleValue[ myParmExtraValueCount ];
			HAPI_Host.getParmExtraValues( myAssetId, myParmExtraValues, 0, myParmExtraValueCount );
			
			// Get parameter choice lists.
			myParmChoiceLists = new HAPI_ParmChoiceInfo[ myParmChoiceCount ];
			HAPI_Host.getParmChoiceLists( myAssetId, myParmChoiceLists, 0, myParmChoiceCount );
			
			// Get exposed handle information.
			myHandleInfos = new HAPI_HandleInfo[ myHandleCount ];
			HAPI_Host.getHandleInfo( myAssetId, myHandleInfos, 0, myHandleCount );
			
			myHandleBindingInfos = new List< HAPI_HandleBindingInfo[] >( myHandleCount );					
			
			for ( int handle_index = 0; handle_index < myHandleCount; ++handle_index )
			{				
				
				HAPI_HandleInfo handleInfo = myHandleInfos[ handle_index ];
				HAPI_HandleBindingInfo[] handleBindingInfos = new HAPI_HandleBindingInfo[ handleInfo.bindingsCount ];
				HAPI_Host.getHandleBindingInfo( myAssetId, handle_index, 
												handleBindingInfos, 0, handleInfo.bindingsCount );
				
				myHandleBindingInfos.Add( handleBindingInfos );
			}
		
			
			myAssetPathChanged = false;
		}
		else
		{
			// Set all parameter values.
			HAPI_Host.setParameters( myAssetId, myParms, 0, myParmCount );
			
			// Set extra parameter values.
			HAPI_Host.setParmExtraValues( myAssetId, myParmExtraValues, 0, myParmExtraValueCount );
		}
					
		// Clean up.
		destroyChildren();
		
		// Create local object info caches (transforms need to be stored in a parallel array).
		myObjects 			= new HAPI_ObjectInfo[ myObjectCount ];
		myObjectTransforms 	= new HAPI_Transform[ myObjectCount ];
		
		HAPI_Host.getObjects( 			myAssetId, myObjects, 0, myObjectCount );
		HAPI_Host.getObjectTransforms(	myAssetId, myObjectTransforms, HAPI_RSTOrder.SRT, 0, myObjectCount );
		
		for ( int object_index = 0; object_index < myObjectCount; ++object_index )
			createObject( object_index );
						
		return true;
	}
		
	public string 					myAssetPath;
	public int 						myAssetId;
	public int 						myObjectCount;
	public int 						myParmCount;
	public int						myParmExtraValueCount;
	public int						myParmChoiceCount;
	public int						myHandleCount;
		
	public HAPI_AssetInfo 			myAssetInfo;
	public HAPI_ObjectInfo[] 		myObjects;
	public HAPI_Transform[] 		myObjectTransforms;
	public HAPI_ParmInfo[] 			myParms;
	public HAPI_ParmSingleValue[]	myParmExtraValues;
	public HAPI_ParmChoiceInfo[]	myParmChoiceLists;
	public HAPI_HandleInfo[]		myHandleInfos;
	public List< HAPI_HandleBindingInfo[] > myHandleBindingInfos;
	
	public bool 					myShowObjectControls;
	public bool 					myShowAssetControls;
	public bool						myAutoSelectAssetNode;
	
	/// <summary>
	/// 	Indices of the currently selected folders in the Inspector.
	/// 	A 1:1 mapping with myFolderListSelectionIds.
	/// </summary>
	public List< int > 				myFolderListSelections;
	
	/// <summary>
	/// 	Parameter ids of the currently selected folders in the Inspector. 
	/// 	A 1:1 mapping with myFolderListSelections.
	/// </summary>
	public List< int > 				myFolderListSelectionIds;
	
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
		
		// Set Object Control on child selection control so it can read settings from here.
		main_child.GetComponent< HAPI_ChildSelectionControl >().setObjectControl( this );
		
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
		
		// Get Detail info.
		HAPI_DetailInfo detail_info = new HAPI_DetailInfo();
		HAPI_Host.getDetailInfo( myAssetId, object_id, out detail_info );
		Debug.Log( "Obj #" + object_id + " (" + object_info.name + "): "
			+ "verts: " + detail_info.vertexCount + " faces: " + detail_info.faceCount );
		
		// Make sure our primitive and vertex numbers are supported by Unity.
		// TODO: add this limit in a more proper place
		if ( detail_info.faceCount > 65000 * 3 )
		{
			Debug.LogError( "Face count (" + detail_info.faceCount + ") above limit (" + ( 65000 * 3 ) + ")!" );
			return;
		}
		if ( detail_info.vertexCount > 65000 )
		{
			Debug.LogError( "Vertex count (" + detail_info.vertexCount + ") above limit (" + 65000 + ")!" );
			return;
		}
		
		// Get Face counts.
		int[] face_counts = new int[ detail_info.faceCount ];
		fillArray( myAssetId, object_id, face_counts, HAPI_Host.getFaceCounts, detail_info.faceCount );
		
		// Get Vertex list.
		int[] vertex_list = new int[ detail_info.vertexCount ];
		fillArray( myAssetId, object_id, vertex_list, HAPI_Host.getVertexList, detail_info.vertexCount );
		
		// Print attribute names.
		printAllAttributeNames( myAssetId, object_id, detail_info );
		
		// Get position vertex attributes.
		HAPI_AttributeInfo pos_attr_info = new HAPI_AttributeInfo( "P" );
		float[] pos_attr = new float[ 0 ];
		getAttribute( myAssetId, object_id, ref pos_attr_info, ref pos_attr, HAPI_Host.getAttributeFloatData );
		if ( !pos_attr_info.exists )
		{
			Debug.LogError( "No position attribute found for object " + object_info.name + " (" + object_id + ")" );
			return;
		}
		else if ( pos_attr_info.owner != (int) HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
		{
			Debug.LogError( "I only understand position as point attributes!" );
			return;
		}
				
		// Get uv attributes.
		HAPI_AttributeInfo uv_attr_info = new HAPI_AttributeInfo( "uv" );
		uv_attr_info.tupleSize = 2;
		float[] uv_attr = new float[ 0 ];
		getAttribute( myAssetId, object_id, ref uv_attr_info, ref uv_attr, HAPI_Host.getAttributeFloatData );
		
		// Apply object transforms.		
		main_child.transform.localPosition 	= new Vector3( 		trans.position[ 0 ], 
																trans.position[ 1 ],
																trans.position[ 2 ] );		
		main_child.transform.localRotation 	= new Quaternion( 	trans.rotationQuaternion[ 0 ],
														  		trans.rotationQuaternion[ 1 ],
																trans.rotationQuaternion[ 2 ],
																trans.rotationQuaternion[ 3 ] );
		main_child.transform.localScale = new Vector3( 			trans.scale[ 0 ], 
													  			trans.scale[ 1 ], 
													  			trans.scale[ 2 ] );
				
		// Create Unity-specific data objects.
		Vector3[] vertices 	= new Vector3[ detail_info.vertexCount ];
		int[] triangles 	= new int[ detail_info.faceCount * 3 ];
		Vector2[] uvs 		= new Vector2[ detail_info.vertexCount ];
		Vector3[] normals 	= new Vector3[ detail_info.vertexCount ];
		
		// Fill Unity-specific data objects with data from the runtime.
		for ( int i = 0; i < detail_info.vertexCount; ++i ) 
		{
			for ( int j = 0; j < 3; ++j ) 
			{
				vertices[ i ][ j ] = pos_attr[ vertex_list[ i ] * 3 + j ];
				//normals[ i ][ j ] 		= raw_vertices[ i ].normal[ j ];
			}
			if ( uv_attr_info.exists )
			{
				// If the UVs are per vertex just query directly into the UV array we filled above.
				if ( uv_attr_info.owner == (int) HAPI_AttributeOwner.HAPI_ATTROWNER_VERTEX )
					for ( int j = 0; j < 2; ++j )
						uvs[ i ][ j ] = uv_attr[ i * 2 + j ];
				
				// If the UVs are per point use the vertex list array point indicies to query into
				// the UV array we filled above.
				else if ( uv_attr_info.owner == (int) HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
					for ( int j = 0; j < 2; ++j )
						uvs[ i ][ j ] = uv_attr[ vertex_list[ i ] * 2 + j ];
			}
		}
		
		// Triangles are already specified by design. The only thing we need to do is reverse the order
		// for Unity. We do this with the ( 2 - j ) below.
		for ( int i = 0; i < detail_info.faceCount; ++i ) 
			for ( int j = 0; j < 3; ++j )
				triangles[ i * 3 + j ] 	= i * 3 + ( 2 - j );
		
		// Load into vertices and face into mesh.
		main_child_mesh.vertices 	= vertices;
		main_child_mesh.triangles 	= triangles;
		main_child_mesh.uv 			= uvs;
		main_child_mesh.normals 	= normals;
		
		main_child_mesh.RecalculateBounds();
		main_child_mesh.RecalculateNormals();
	}
			
#if DEBUG
	public bool myAssetPathChanged;
#else
	pivate bool myAssetPathChanged;
#endif
	
}
