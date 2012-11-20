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

using UnityEngine;
using UnityEditor;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using HAPI;

/// <summary>
/// 	Main script attached to an Unity game object that corresponds to a Houdini asset instance on the 
/// 	Houdini side.
/// </summary>
public class HAPI_AssetUtility
{
	// GET ----------------------------------------------------------------------------------------------------------
	
	public delegate void getArray1IdDel< T >( int id1, [Out] T[] data, int start, int end );
	public delegate void getArray2IdDel< T >( int id1, int id2, [Out] T[] data, int start, int end );
	public delegate void getArray3IdDel< T >( int id1, int id2, int id3, [Out] T[] data, int start, int end );
	public delegate void getArray4IdDel< T >( int id1, int id2, int id3, int id4, [Out] T[] data, int start, int end );
	
	public static void getArray1Id< T >( int id1, getArray1IdDel< T > func, [Out] T[] data, 
										 int count )
	{
		getArray( id1, 0, 0, 0, func, null, null, null, data, count, 1 );
	}
	public static void getArray1Id< T >( int id1, getArray1IdDel< T > func, [Out] T[] data, 
										 int count, int tuple_size )
	{
		getArray( id1, 0, 0, 0, func, null, null, null, data, count, tuple_size );
	}
	
	public static void getArray2Id< T >( int id1, int id2, getArray2IdDel< T > func, [Out] T[] data, 
										 int count )
	{
		getArray( id1, id2, 0, 0, null, func, null, null, data, count, 1 );
	}
	public static void getArray2Id< T >( int id1, int id2, getArray2IdDel< T > func, [Out] T[] data, 
										 int count, int tuple_size )
	{
		getArray( id1, id2, 0, 0, null, func, null, null, data, count, tuple_size );
	}
	
	public static void getArray3Id< T >( int id1, int id2, int id3, getArray3IdDel< T > func, [Out] T[] data, 
										 int count )
	{
		getArray( id1, id2, id3, 0, null, null, func, null, data, count, 1 );
	}
	public static void getArray3Id< T >( int id1, int id2, int id3, getArray3IdDel< T > func, [Out] T[] data, 
										 int count, int tuple_size )
	{
		getArray( id1, id2, id3, 0, null, null, func, null, data, count, tuple_size );
	}
	
	public static void getArray4Id< T >( int id1, int id2, int id3, int id4, getArray4IdDel< T > func, [Out] T[] data, 
										 int count )
	{
		getArray( id1, id2, id3, id4, null, null, null, func, data, count, 1 );
	}
	public static void getArray4Id< T >( int id1, int id2, int id3, int id4, getArray4IdDel< T > func, [Out] T[] data, 
										 int count, int tuple_size )
	{
		getArray( id1, id2, id3, id4, null, null, null, func, data, count, tuple_size );
	}
	
	private static void getArray< T >( int id1, int id2, int id3, int id4,
									   getArray1IdDel< T > func_1id, 
									   getArray2IdDel< T > func_2id, 
									   getArray3IdDel< T > func_3id,
									   getArray4IdDel< T > func_4id,
									   [Out] T[] data, int count, int tuple_size )
	{
		int max_array_size = HAPI_Constants.HAPI_MAX_PAGE_SIZE / ( Marshal.SizeOf( typeof( T ) ) * tuple_size );
		
		int local_count = count;
		int current_index = 0;
		
		while ( local_count > 0 ) 
		{			
			int length = 0;
			if ( local_count > max_array_size ) 
			{
				length = max_array_size;
				local_count -= max_array_size;
			} 
			else 
			{
				length = local_count;
				local_count = 0;
			}
			
			T[] local_array = new T[ length * tuple_size ];
			
			if ( func_1id != null )
				func_1id( id1, local_array, current_index, length );
			else if ( func_2id != null )
				func_2id( id1, id2, local_array, current_index, length );
			else if ( func_3id != null )
				func_3id( id1, id2, id3, local_array, current_index, length );
			else if ( func_4id != null )
				func_4id( id1, id2, id3, id4, local_array, current_index, length );
			else
			{
				Debug.LogError( "No valid delegates given to getArray< T >!" );
				return;
			}
				
			// Copy data from the temporary array.
			for ( int i = current_index; i < current_index + length; ++i )
				for ( int j = 0; j < tuple_size; ++j )
					data[ i * tuple_size + j ] = local_array[ ( i - current_index ) * tuple_size + j ];
			
			current_index += length;
		}
	}
	
	// SET ----------------------------------------------------------------------------------------------------------
	
	public delegate void setArray1IdDel< T >( int id1, [Out] T[] data, int start, int end );
	public delegate void setArray2IdDel< T >( int id1, int id2, [Out] T[] data, int start, int end );
	public delegate void setArray3IdDel< T >( int id1, int id2, int id3, [Out] T[] data, int start, int end );
	
	public static void setArray1Id< T >( int id1, getArray1IdDel< T > func, [Out] T[] data, 
										 int count )
	{
		setArray( id1, 0, 0, func, null, null, data, count, 1 );
	}
	public static void setArray1Id< T >( int id1, getArray1IdDel< T > func, [Out] T[] data, 
										 int count, int tuple_size )
	{
		setArray( id1, 0, 0, func, null, null, data, count, tuple_size );
	}
	
	public static void setArray2Id< T >( int id1, int id2, getArray2IdDel< T > func, [Out] T[] data, 
										 int count )
	{
		setArray( id1, id2, 0, null, func, null, data, count, 1 );
	}
	public static void setArray2Id< T >( int id1, int id2, getArray2IdDel< T > func, [Out] T[] data, 
										 int count, int tuple_size )
	{
		setArray( id1, id2, 0, null, func, null, data, count, tuple_size );
	}
	
	public static void setArray3Id< T >( int id1, int id2, int id3, getArray3IdDel< T > func, [Out] T[] data, 
										 int count )
	{
		setArray( id1, id2, id3, null, null, func, data, count, 1 );
	}
	public static void setArray3Id< T >( int id1, int id2, int id3, getArray3IdDel< T > func, [Out] T[] data, 
										 int count, int tuple_size )
	{
		setArray( id1, id2, id3, null, null, func, data, count, tuple_size );
	}
	
	private static void setArray< T >( int id1, int id2, int id3, 
									   getArray1IdDel< T > func_1id, 
									   getArray2IdDel< T > func_2id, 
									   getArray3IdDel< T > func_3id,
									   [Out] T[] data, int count, int tuple_size )
	{
		int max_array_size = HAPI_Constants.HAPI_MAX_PAGE_SIZE / ( Marshal.SizeOf( typeof( T ) ) * tuple_size );
		
		int local_count = count;
		int current_index = 0;
		
		while ( local_count > 0 ) 
		{			
			int length = 0;
			if ( local_count > max_array_size ) 
			{
				length = max_array_size;
				local_count -= max_array_size;
			} 
			else 
			{
				length = local_count;
				local_count = 0;
			}
			
			T[] local_array = new T[ length * tuple_size ];
			
			// Copy data from main array to the temporary array.
			for ( int i = current_index; i < current_index + length; ++i )
				for ( int j = 0; j < tuple_size; ++j )
					local_array[ ( i - current_index ) * tuple_size + j ] = data[ i * tuple_size + j ];
			
			if ( func_1id != null )
				func_1id( id1, local_array, current_index, length );
			else if ( func_2id != null )
				func_2id( id1, id2, local_array, current_index, length );
			else if ( func_3id != null )
				func_3id( id1, id2, id3, local_array, current_index, length );
			else
			{
				Debug.LogError( "No valid delegates given to setArray< T >!" );
				return;
			}
			
			current_index += length;
		}
	}
	
	// ATTRIBUTES ---------------------------------------------------------------------------------------------------
		
	public delegate void getAttrArrayInputFunc< T >( int asset_id, int object_id, int geo_id, string name,
													 ref HAPI_AttributeInfo info, 
													 [Out] T[] items, int start, int end );
	public static void getAttrArray< T >( int asset_id, int object_id, int geo_id, string name, 
										  ref HAPI_AttributeInfo info, T[] items,
									 	  getAttrArrayInputFunc< T > get_func, int count ) 
	{
		int max_array_size = HAPI_Constants.HAPI_MAX_PAGE_SIZE / ( Marshal.SizeOf( typeof( T ) ) * info.tupleSize );
		
		int local_count = count;
		int current_index = 0;
		
		while ( local_count > 0 ) 
		{
			int length = 0;
			if ( local_count > max_array_size ) 
			{
				length = max_array_size;
				local_count -= max_array_size;
			} 
			else 
			{
				length = local_count;
				local_count = 0;
			}
			
			T[] local_array = new T[ length * info.tupleSize ];
			get_func( asset_id, object_id, geo_id, name, ref info, local_array, current_index, length );
			
			// Copy data from the temporary array.
			for ( int i = current_index; i < current_index + length; ++i )
				for ( int j = 0; j < info.tupleSize; ++j )
					items[ i * info.tupleSize + j ] = local_array[ ( i - current_index ) * info.tupleSize + j ];
			
			current_index += length;
		}
	}
	
	public delegate void setAttrArrayInputFunc< T >( int asset_id, int object_id, int geo_id, string name,
												  	 ref HAPI_AttributeInfo info, 
													 T[] items, int start, int end );
	public static void setAttrArray< T >( int asset_id, int object_id, int geo_id, string name, 
										  ref HAPI_AttributeInfo info, T[] items, 
										  setAttrArrayInputFunc< T > set_func, int count ) 
	{
		int max_array_size = HAPI_Constants.HAPI_MAX_PAGE_SIZE / ( Marshal.SizeOf( typeof( T ) ) * info.tupleSize );
		
		int local_count = count;
		int current_index = 0;
		
		while ( local_count > 0 ) 
		{			
			int length = 0;
			if ( local_count > max_array_size ) 
			{
				length = max_array_size;
				local_count -= max_array_size;
			} 
			else 
			{
				length = local_count;
				local_count = 0;
			}
			
			T[] local_array = new T[ length * info.tupleSize ];
			
			// Copy data from the temporary array.
			for ( int i = current_index; i < current_index + length; ++i )
				for ( int j = 0; j < info.tupleSize; ++j )
					local_array[ ( i - current_index ) * info.tupleSize + j ] = items[ i * info.tupleSize + j ];
			
			set_func( asset_id, object_id, geo_id, name, ref info, local_array, current_index, length );
			
			current_index += length;
		}
	}
	
	public static void getAttribute< T >( int asset_id, int object_id, int geo_id, string name, 
										  ref HAPI_AttributeInfo info, ref T[] data,
										  getAttrArrayInputFunc< T > get_func )
	{
		int original_tuple_size = info.tupleSize;		
		
		for ( int type = 0; type < (int) HAPI_AttributeOwner.HAPI_ATTROWNER_MAX; ++type )
		{
			info.owner = type;
			HAPI_Host.getAttributeInfo( asset_id, object_id, geo_id, name, ref info );
			if ( info.exists )
				break;
		}
		if ( !info.exists )
			return;			
		
		if ( original_tuple_size > 0 )
			info.tupleSize = original_tuple_size;
		
		data = new T[ info.count * info.tupleSize ];
		getAttrArray( asset_id, object_id, geo_id, name, ref info, data, get_func, info.count );
	}
	
	public static void setAttribute< T >( int asset_id, int object_id, int geo_id, string name, 
										  ref HAPI_AttributeInfo info, ref T[] data, 
										  setAttrArrayInputFunc< T > get_func )
	{
		setAttrArray( asset_id, object_id, geo_id, name, ref info, data, get_func, info.count );
	}
	
	public static string[] getAttributeNames( int asset_id, int object_id, int geo_id, HAPI_GeoInfo geo_info, 
											  HAPI_AttributeOwner owner )
	{
		int attr_count = geo_info.getOwnerCount( owner );
			
		string[] names = new string[ attr_count ];
		
		int[] attr_names = new int[ attr_count ]; // string handles (SH)
		
		HAPI_Host.getAttributeNames( asset_id, object_id, geo_id, (int) owner, attr_names, attr_count );
		for ( int ii = 0; ii < attr_count; ++ii )
			names[ ii ] = HAPI_Host.getString( attr_names[ ii ] );
		
		return names;
	}
	
	public static void printAttributeNames( int asset_id, int object_id, int geo_id, HAPI_GeoInfo geo_info,
											HAPI_AttributeOwner owner )
	{
		string[] names = getAttributeNames( asset_id, object_id, geo_id, geo_info, owner );
		
		string msg = "A" + asset_id + "O" + object_id + " - ";
		
		switch ( owner )
		{
			case HAPI_AttributeOwner.HAPI_ATTROWNER_VERTEX: msg += "Vertex"; break;
			case HAPI_AttributeOwner.HAPI_ATTROWNER_POINT:	msg += "Point"; break;
			case HAPI_AttributeOwner.HAPI_ATTROWNER_PRIM: 	msg += "Primitive"; break;
			case HAPI_AttributeOwner.HAPI_ATTROWNER_DETAIL:	msg += "Detail"; break;
			default: Debug.LogError( "Invalid HAPI_AttributeOwner!" ); return;
		}
		
		msg += " Attributes:";
		
		bool comma = false;
		foreach ( string name in names )
		{
			if ( comma )
				msg += ",";
			else
				comma = true;
			msg += " " + name;
		}
		
		Debug.Log( msg );
	}
	
	public static void printAllAttributeNames( int asset_id, int object_id, int geo_id, HAPI_GeoInfo geo_info )
	{
		for ( int owner = 0; owner < (int) HAPI_AttributeOwner.HAPI_ATTROWNER_MAX; ++owner )
			printAttributeNames( asset_id, object_id, geo_id, geo_info, (HAPI_AttributeOwner) owner );
	}
	
	// TEXTURES -----------------------------------------------------------------------------------------------------
	
	public static void assignTexture( ref Material material, HAPI_MaterialInfo material_info )
	{
		// Navigate to the Assets/Textures directory and create it if it doesn't exist.
		string assets_root_path 		= Application.dataPath;
		string textures_root_path 		= assets_root_path + "/Textures";
		DirectoryInfo textures_dir 		= new DirectoryInfo( textures_root_path );
		if ( !textures_dir.Exists )
			textures_dir.Create();
		
		// Figure out the source file path and name.
		string tex_file_path 		= material_info.textureFilePath.Replace( "\\", "/" );
		string relative_file_path 	= tex_file_path.Replace( assets_root_path, "Assets" );
		
		// Load the texture and assign it to the material. Note that LoadAssetAtPath only understands paths
		// relative to the project folder.
		Object tex_obj = AssetDatabase.LoadAssetAtPath( relative_file_path, typeof( Texture2D ) );
		if ( tex_obj == null || !AssetDatabase.Contains( tex_obj ) )
		{
			// Asset has not been imported yet so import and try again.
			AssetDatabase.ImportAsset( relative_file_path, ImportAssetOptions.Default );
			tex_obj = AssetDatabase.LoadAssetAtPath( relative_file_path, typeof( Texture2D ) );
		}
		
		// Assign main texture.
		material.mainTexture = (Texture2D) tex_obj;
		
		// Assign shader properties.
		material.SetFloat( "_Shininess", 1.0f - material_info.roughness );
		material.SetColor( "_Color", new Color( material_info.diffuse[ 0 ], 
												material_info.diffuse[ 1 ],
												material_info.diffuse[ 2 ],
												material_info.diffuse[ 3 ] ) );
		material.SetColor( "_SpecColor", new Color( material_info.specular[ 0 ], 
													material_info.specular[ 1 ],
													material_info.specular[ 2 ],
													material_info.specular[ 3 ] ) );
		
		// Refresh all assets just in case.
		AssetDatabase.Refresh();
	}
	
	// GEOMETRY MARSHALLING -----------------------------------------------------------------------------------------
	
	public static void getMesh( int asset_id, int object_id, int geo_id, Mesh mesh )
	{
		// Get Detail info.
		HAPI_GeoInfo geo_info = new HAPI_GeoInfo();
		HAPI_Host.getGeoInfo( asset_id, object_id, geo_id, out geo_info );
		
		// Make sure our primitive and vertex numbers are supported by Unity.
		// TODO: add this limit in a more proper place
		if ( geo_info.faceCount > 65000 * 3 )
			throw new HAPI_Error( "Face count (" + geo_info.faceCount 
								  + ") above limit (" + ( 65000 * 3 ) + ")!" );
		if ( geo_info.vertexCount > 65000 )
			throw new HAPI_Error( "Vertex count (" + geo_info.vertexCount + ") above limit (" + 65000 + ")!" );
		
		// Get Face counts.
		int[] face_counts = new int[ geo_info.faceCount ];
		getArray3Id( asset_id, object_id, geo_id, HAPI_Host.getFaceCounts, face_counts, geo_info.faceCount );
		
		// Get Vertex list.
		int[] vertex_list = new int[ geo_info.vertexCount ];
		getArray3Id( asset_id, object_id, geo_id, HAPI_Host.getVertexList, vertex_list, geo_info.vertexCount );
		
		// Print attribute names.
		//printAllAttributeNames( asset_id, object_id, geo_id, geo_info );
		
		// Get position attributes.
		HAPI_AttributeInfo pos_attr_info = new HAPI_AttributeInfo( "P" );
		float[] pos_attr = new float[ 0 ];
		getAttribute( asset_id, object_id, geo_id, "P", ref pos_attr_info, ref pos_attr, 
					  HAPI_Host.getAttributeFloatData );
		if ( !pos_attr_info.exists )
			throw new HAPI_Error( "No position attribute found." );
		else if ( pos_attr_info.owner != (int) HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
			throw new HAPI_Error( "I only understand position as point attributes!" );
				
		// Get uv attributes.
		HAPI_AttributeInfo uv_attr_info = new HAPI_AttributeInfo( "uv" );
		uv_attr_info.tupleSize = 2;
		float[] uv_attr = new float[ 0 ];
		getAttribute( asset_id, object_id, geo_id, "uv", ref uv_attr_info, ref uv_attr, 
					  HAPI_Host.getAttributeFloatData );
		
		// Get normal attributes.
		HAPI_AttributeInfo normal_attr_info = new HAPI_AttributeInfo( "N" );
		float[] normal_attr = new float[ 0 ];
		getAttribute( asset_id, object_id, geo_id, "N", ref normal_attr_info, ref normal_attr, 
					  HAPI_Host.getAttributeFloatData );
				
		// Create Unity-specific data objects.
		Vector3[] vertices 	= new Vector3[ 	geo_info.vertexCount ];
		int[] triangles 	= new int[ 		geo_info.faceCount * 3 ];
		Vector2[] uvs 		= new Vector2[ 	geo_info.vertexCount ];
		Vector3[] normals 	= new Vector3[ 	geo_info.vertexCount ];
		
		// Fill Unity-specific data objects with data from the runtime.
		for ( int i = 0; i < geo_info.vertexCount; ++i ) 
		{
			// Fill position information.
			for ( int j = 0; j < 3; ++j )
			{
				vertices[ i ][ j ] = pos_attr[ vertex_list[ i ] * 3 + j ];
				// Flip the x coordinate.
				if ( j == 0 )
					vertices[ i ][ j ] *= -1;
			}
			
			// Fill UVs.
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
			
			// Fill normals.
			if ( normal_attr_info.exists )
			{
				// If the normals are per vertex just query directly into the normals array we filled above.
				if ( normal_attr_info.owner == (int) HAPI_AttributeOwner.HAPI_ATTROWNER_VERTEX )
					for ( int j = 0; j < 3; ++j )
					{
						normals[ i ][ j ] = normal_attr[ i * 3 + j ];
						// Flip the x coordinate.
						if ( j == 0 )
							normals[ i ][ j ] *= -1;
						
					}
				
				// If the normals are per point use the vertex list array point indicies to query into
				// the normal array we filled above.
				else if ( normal_attr_info.owner == (int) HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
					for ( int j = 0; j < 3; ++j )
					{
						normals[ i ][ j ] = normal_attr[ vertex_list[ i ] * 3 + j ];
						// Flip the x coordinate.
						if ( j == 0 )
							normals[ i ][ j ] *= -1;
					}
				
				// If the normals are per face divide the vertex index by the number of vertices per face
				// which should always be HAPI_MAX_VERTICES_PER_FACE.
				else if ( normal_attr_info.owner == (int) HAPI_AttributeOwner.HAPI_ATTROWNER_PRIM )
					for ( int j = 0; j < 3; ++j )
					{
						normals[ i ][ j ] 
							= normal_attr[ (int) Mathf.Floor( i / HAPI_Constants.HAPI_MAX_VERTICES_PER_FACE ) ];
						// Flip the x coordinate.
						if ( j == 0 )
							normals[ i ][ j ] *= -1;
					}
			}
		}
		
		for ( int i = 0; i < geo_info.faceCount; ++i ) 
			for ( int j = 0; j < 3; ++j )
				triangles[ i * 3 + j ] 	= i * 3 + j;
		
		// Load into vertices and face into mesh.
		mesh.vertices 	= vertices;
		mesh.triangles 	= triangles;
		mesh.uv 		= uvs;
		mesh.normals 	= normals;
		
		mesh.RecalculateBounds();
		
		if ( !normal_attr_info.exists )
			mesh.RecalculateNormals();
	}
	
	public static void setMesh( int asset_id, int object_id, int geo_id, ref Mesh mesh )
	{
		Vector3[] vertices 				= mesh.vertices;
		int[] triangles 				= mesh.triangles;
		//Vector2[] uvs 					= mesh.uv;
		//Vector3[] normals 				= mesh.normals;
		
		HAPI_GeoInfo geo_info 			= new HAPI_GeoInfo();
		
		geo_info.id 					= geo_id;
		geo_info.materialId 			= -1;
		geo_info.faceCount 				= triangles.Length / 3;
		geo_info.vertexCount 			= triangles.Length;
		geo_info.pointCount 			= vertices.Length;
		
		geo_info.pointAttributeCount 	= 1;
		geo_info.vertexAttributeCount 	= 0;
		geo_info.faceAttributeCount 	= 0;
		geo_info.detailAttributeCount 	= 0;
		
		/* Not yet supported.
		if ( uvs != null )
			geo_info.vertexAttributeCount++;
		if ( normals != null )
			geo_info.vertexAttributeCount++;
		*/
		
		HAPI_Host.setGeoInfo( asset_id, object_id, geo_id, ref geo_info );
		
		// Set Face counts.
		int[] face_counts = new int[ geo_info.faceCount ];
		for ( int i = 0; i < geo_info.faceCount; ++i )
			face_counts[ i ] = 3;
		setArray3Id( asset_id, object_id, geo_id, HAPI_Host.setFaceCounts, face_counts, geo_info.faceCount );
		
		// Set Vertex list.
		int[] vertex_list = new int[ geo_info.vertexCount ];
		for ( int i = 0; i < geo_info.faceCount; ++i )
			for ( int j = 0; j < 3; ++j )
				vertex_list[ i * 3 + j ] = triangles[ i * 3 + ( 2 - j ) ];
		setArray3Id( asset_id, object_id, geo_id, HAPI_Host.setVertexList, vertex_list, geo_info.vertexCount );
		
		// Set position attributes.
		HAPI_AttributeInfo pos_attr_info = new HAPI_AttributeInfo( "P" );
		pos_attr_info.exists 		= true;
		pos_attr_info.owner 		= (int) HAPI.HAPI_AttributeOwner.HAPI_ATTROWNER_POINT;
		pos_attr_info.storage 		= (int) HAPI.HAPI_StorageType.HAPI_STORAGETYPE_FLOAT;
		pos_attr_info.count 		= geo_info.pointCount;
		pos_attr_info.tupleSize 	= 3;
		HAPI_Host.addAttribute( asset_id, object_id, geo_id, "P", ref pos_attr_info );
		
		float[] pos_attr = new float[ geo_info.pointCount * 3 ];
		for ( int i = 0; i < geo_info.pointCount; ++i )
			for ( int j = 0; j < 3; ++j )
				pos_attr[ i * 3 + j ] = vertices[ i ][ j ];
		setAttribute( asset_id, object_id, geo_id, "P", ref pos_attr_info, ref pos_attr, 
					  HAPI_Host.setAttributeFloatData );
		
		HAPI_Host.commitGeo( asset_id, object_id, geo_id );
	}
	
}
