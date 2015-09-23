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
 * 
 */

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class HoudiniAssetUtility
{
	// TYPES -----------------------------------------------------------------------------------------------------------

	public static System.Type getTypeByName( string name )
	{
#if UNITY_EDITOR
		foreach ( System.Reflection.Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies() )
			foreach ( System.Type type in assembly.GetTypes() )
				if ( type.Name == name )
					return type;
#endif // UNITY_EDITOR
		return null;
	}

	// TRANSFORMS ------------------------------------------------------------------------------------------------------

	public static Quaternion getQuaternion( Matrix4x4 m )
	{
		// Check to stop warning about "Look rotation viewing vector is zero" from Quaternion.LookRotation().
		if ( 
			Mathf.Approximately( 0.0f, m.GetColumn( 2 ).x ) &&
			Mathf.Approximately( 0.0f, m.GetColumn( 2 ).y ) &&
			Mathf.Approximately( 0.0f, m.GetColumn( 2 ).z ) &&
			Mathf.Approximately( 0.0f, m.GetColumn( 2 ).w ) &&
			Mathf.Approximately( 0.0f, m.GetColumn( 1 ).x ) &&
			Mathf.Approximately( 0.0f, m.GetColumn( 1 ).y ) &&
			Mathf.Approximately( 0.0f, m.GetColumn( 1 ).z ) &&
			Mathf.Approximately( 0.0f, m.GetColumn( 1 ).w ) )
		{
			return new Quaternion();
		}
		else
			return Quaternion.LookRotation( m.GetColumn( 2 ), m.GetColumn( 1 ) );
	}

	public static Vector3 getPosition( Matrix4x4 m )
	{
		return m.GetColumn( 3 );
	}

	public static Vector3 getScale( Matrix4x4 m )
	{
		var x = Mathf.Sqrt( m.m00 * m.m00 + m.m10 * m.m10 + m.m20 * m.m20 );
		var y = Mathf.Sqrt( m.m01 * m.m01 + m.m11 * m.m11 + m.m21 * m.m21 );
		var z = Mathf.Sqrt( m.m02 * m.m02 + m.m12 * m.m12 + m.m22 * m.m22 );

		return new Vector3( x, y, z );
	}

	public static HAPI_TransformEuler getHapiTransform( Matrix4x4 m )
	{
		Quaternion q = getQuaternion( m );
		Vector3 r = q.eulerAngles;

		Vector3 p = getPosition( m );
		Vector3 s = getScale( m );

		HAPI_TransformEuler transform = new HAPI_TransformEuler( true );

		transform.position[ 0 ] = -p[ 0 ];
		transform.position[ 1 ] =  p[ 1 ];
		transform.position[ 2 ] =  p[ 2 ];

		transform.rotationEuler[ 0 ] =  r[ 0 ];
		transform.rotationEuler[ 1 ] = -r[ 1 ];
		transform.rotationEuler[ 2 ] = -r[ 2 ];

		transform.scale[ 0 ] = s[ 0 ];
		transform.scale[ 1 ] = s[ 1 ];
		transform.scale[ 2 ] = s[ 2 ];

		transform.rotationOrder = HAPI_XYZOrder.HAPI_ZXY;
		transform.rstOrder = HAPI_RSTOrder.HAPI_SRT;

		return transform;
	}

	public static void applyTransform( HAPI_TransformEuler hapi_transform, Transform transform )
	{
		// Apply object transforms.
		//
		// Axis and Rotation conversions:
		// Note that Houdini's X axis points in the opposite direction that Unity's does.  Also, Houdini's 
		// rotation is right handed, whereas Unity is left handed. To account for this, we need to invert
		// the x coordinate of the translation, and do the same for the rotations (except for the x rotation,
		// which doesn't need to be flipped because the change in handedness AND direction of the left x axis
		// causes a double negative - yeah, I know).
				
		transform.position = new Vector3(
			-hapi_transform.position[ 0 ],
			hapi_transform.position[ 1 ],
			hapi_transform.position[ 2 ] );

		Quaternion quat = Quaternion.Euler( new Vector3(
			hapi_transform.rotationEuler[ 0 ],
			-hapi_transform.rotationEuler[ 1 ],
			-hapi_transform.rotationEuler[ 2 ] ) );
		transform.rotation = quat;

		Vector3 world_to_local_scale = Vector3.one;
		if ( transform.parent )
		{
			Matrix4x4 world_to_local = transform.parent.worldToLocalMatrix;
			world_to_local_scale = HoudiniAssetUtility.getScale( world_to_local );
		}
		Vector3 world_scale = new Vector3(
			hapi_transform.scale[ 0 ],
			hapi_transform.scale[ 1 ],
			hapi_transform.scale[ 2 ] );
		Vector3 local_scale = new Vector3(
			world_to_local_scale.x * world_scale.x,
			world_to_local_scale.y * world_scale.y,
			world_to_local_scale.z * world_scale.z );
		transform.localScale = local_scale;
	}

	public static void applyTransform( HAPI_Transform hapi_transform, Transform transform )
	{
		// Apply object transforms.
		//
		// Axis and Rotation conversions:
		// Note that Houdini's X axis points in the opposite direction that Unity's does.  Also, Houdini's 
		// rotation is right handed, whereas Unity is left handed. To account for this, we need to invert
		// the x coordinate of the translation, and do the same for the rotations (except for the x rotation,
		// which doesn't need to be flipped because the change in handedness AND direction of the left x axis
		// causes a double negative - yeah, I know).
				
		transform.localPosition = new Vector3( -hapi_transform.position[ 0 ], 
												hapi_transform.position[ 1 ],
												hapi_transform.position[ 2 ] );
		Quaternion quat = new Quaternion(		hapi_transform.rotationQuaternion[ 0 ],
												hapi_transform.rotationQuaternion[ 1 ],
												hapi_transform.rotationQuaternion[ 2 ],
												hapi_transform.rotationQuaternion[ 3 ] );
				
		Vector3 euler = quat.eulerAngles;
		euler.y = -euler.y;
		euler.z = -euler.z;
				
		transform.localRotation = Quaternion.Euler( euler );
		transform.localScale = new Vector3( hapi_transform.scale[ 0 ], 
											hapi_transform.scale[ 1 ], 
											hapi_transform.scale[ 2 ] );
	}

	public static void getHoudiniTransformAndApply( int asset_id, string asset_name, Transform transform )
	{
		HAPI_TransformEuler hapi_transform;
		HoudiniHost.getAssetTransform( 
			asset_id, HAPI_RSTOrder.HAPI_SRT, HAPI_XYZOrder.HAPI_ZXY, out hapi_transform );
		if ( 
			Mathf.Approximately( 0.0f, hapi_transform.scale[ 0 ] ) ||
			Mathf.Approximately( 0.0f, hapi_transform.scale[ 1 ] ) ||
			Mathf.Approximately( 0.0f, hapi_transform.scale[ 2 ] ) )
		{
			Debug.LogWarning( "Asset(id: " + asset_id + ", name: " + asset_name + "): Scale has a zero component!" );
		}

		applyTransform( hapi_transform, transform );
	}

	public static void instantiateAsset( string file_path )
	{
		instantiateAsset( file_path, Vector3.zero );
	}
	public static void instantiateAsset( string file_path, Vector3 initial_position )
	{
		if ( file_path.Length <= 0 )
			return;

		// Create game object.
		GameObject game_object = new GameObject( "New Asset" );

		// Add HAPI Object Control script component.
		HoudiniAssetOTL asset = game_object.AddComponent< HoudiniAssetOTL >();
		
		asset.prAssetType = HoudiniAsset.AssetType.TYPE_OTL;
		// Set that asset path.
		asset.prAssetPath = file_path;
		
		// Save as a prefab.
		//Object prefab = PrefabUtility.CreateEmptyPrefab( "Assets/" + myDefaultPrefabLabel + ".prefab" );
		//PrefabUtility.ReplacePrefab( game_object, prefab, ReplacePrefabOptions.ConnectToPrefab );
		
		// Do first build.
		bool build_result = asset.buildAll();
		if ( build_result == false ) // Something is not right. Clean up.
		{
			UnityEngine.Object.DestroyImmediate( game_object );
			return;
		}

		// Apply transform.
		if ( initial_position != Vector3.zero )
		{
			game_object.transform.localPosition = initial_position;
			if ( asset.prPushUnityTransformToHoudini )
				asset.pushAssetTransformToHoudini();
			asset.buildClientSide();
		}

		// Set new object name from asset name.
		string asset_name		= asset.prAssetInfo.name;
		game_object.name 		= asset_name;
		
		// Select the new houdini asset.
#if UNITY_EDITOR
		GameObject[] selection 	= new GameObject[ 1 ];
		selection[ 0 ] 			= game_object;
		Selection.objects 		= selection;
#endif // UNITY_EDITOR
	}

	public static void calculateMeshTangents( Mesh mesh )
	{
		// Speed up math by copying the mesh arrays.
		int[] triangles		= mesh.triangles;
		Vector3[] vertices	= mesh.vertices;
		Vector2[] uv		= mesh.uv;
		Vector3[] normals	= mesh.normals;

		if ( vertices == null || uv == null || normals == null )
			return;
	 
		// Variable Definitions
		int triangleCount	= triangles.Length;
		int vertexCount		= vertices.Length;
	 
		Vector3[] tan1		= new Vector3[vertexCount];
		Vector3[] tan2		= new Vector3[vertexCount];
	 
		Vector4[] tangents	= new Vector4[vertexCount];
	 
		for ( long a = 0; a < triangleCount; a += 3 )
		{
			long i1 = triangles[ a + 0 ];
			long i2 = triangles[ a + 1 ];
			long i3 = triangles[ a + 2 ];
	 
			Vector3 v1 = vertices[ i1 ];
			Vector3 v2 = vertices[ i2 ];
			Vector3 v3 = vertices[ i3 ];
	 
			Vector2 w1 = uv[ i1 ];
			Vector2 w2 = uv[ i2 ];
			Vector2 w3 = uv[ i3 ];
	 
			float x1 = v2.x - v1.x;
			float x2 = v3.x - v1.x;
			float y1 = v2.y - v1.y;
			float y2 = v3.y - v1.y;
			float z1 = v2.z - v1.z;
			float z2 = v3.z - v1.z;
	 
			float s1 = w2.x - w1.x;
			float s2 = w3.x - w1.x;
			float t1 = w2.y - w1.y;
			float t2 = w3.y - w1.y;
			
			float div = s1 * t2 - s2 * t1;
			float r = div == 0.0f ? 0.0f : 1.0f / div;
			//float r = 1.0f / (s1 * t2 - s2 * t1); // Above code fixes div by zero.
	 
			Vector3 sdir = new Vector3( ( t2 * x1 - t1 * x2 ) * r, 
										( t2 * y1 - t1 * y2 ) * r, 
										( t2 * z1 - t1 * z2 ) * r);
			Vector3 tdir = new Vector3( ( s1 * x2 - s2 * x1 ) * r, 
										( s1 * y2 - s2 * y1 ) * r, 
										( s1 * z2 - s2 * z1 ) * r);
			tan1[ i1 ] += sdir;
			tan1[ i2 ] += sdir;
			tan1[ i3 ] += sdir;

			tan2[ i1 ] += tdir;
			tan2[ i2 ] += tdir;
			tan2[ i3 ] += tdir;
		}

		for ( long a = 0; a < vertexCount; ++a )
		{
			Vector3 n = normals[ a ];
			Vector3 t = tan1[ a ];
	 
			//Vector3 tmp = (t - n * Vector3.Dot(n, t)).normalized;
			//tangents[a] = new Vector4(tmp.x, tmp.y, tmp.z);
			Vector3.OrthoNormalize( ref n, ref t );
			tangents[ a ].x = t.x;
			tangents[ a ].y = t.y;
			tangents[ a ].z = t.z;
	 
			tangents[ a ].w = ( Vector3.Dot( Vector3.Cross( n, t ), tan2[ a ] ) < 0.0f ) ? -1.0f : 1.0f;
		}
	 
		mesh.tangents = tangents;
	}

	// GET ----------------------------------------------------------------------------------------------------------
	
	public delegate void getArray1IdDel< T >(
		int id1, [Out] T[] data, int start, int end );
	public delegate void getArray2IdDel< ID2, T >(
		int id1, ID2 id2, [Out] T[] data, int start, int end );
	public delegate void getArray3IdDel< ID2, T >(
		int id1, ID2 id2, int id3, [Out] T[] data, int start, int end );
	public delegate void getArray4IdDel< ID2, ID4, T >(
		int id1, ID2 id2, int id3, ID4 id4, [Out] T[] data, int start, int end );
	public delegate void getArray5IdDel< ID2, ID4, T >(
		int id1, ID2 id2, int id3, ID4 id4, int id5, [Out] T[] data, int start, int end );

	public static void getArray1Id< T >(
		int id1,
		getArray1IdDel< T > func, [Out] T[] data, int count )
	{
		getArray( id1, 0, 0, 0, 0, func, null, null, null, null, data, count, 1 );
	}
	public static void getArray1Id< T >(
		int id1,
		getArray1IdDel< T > func, [Out] T[] data, int count, int tuple_size )
	{
		getArray( id1, 0, 0, 0, 0, func, null, null, null, null, data, count, tuple_size );
	}
	
	public static void getArray2Id< ID2, T >(
		int id1, ID2 id2,
		getArray2IdDel< ID2, T > func, [Out] T[] data, int count )
	{
		getArray( id1, id2, 0, 0, 0, null, func, null, null, null, data, count, 1 );
	}
	public static void getArray2Id< ID2, T >(
		int id1, ID2 id2,
		getArray2IdDel< ID2, T > func, [Out] T[] data, int count, int tuple_size )
	{
		getArray( id1, id2, 0, 0, 0, null, func, null, null, null, data, count, tuple_size );
	}
	
	public static void getArray3Id< ID2, T >(
		int id1, ID2 id2, int id3,
		getArray3IdDel< ID2, T > func, [Out] T[] data, int count )
	{
		getArray( id1, id2, id3, 0, 0, null, null, func, null, null, data, count, 1 );
	}
	public static void getArray3Id< ID2, T >(
		int id1, ID2 id2, int id3,
		getArray3IdDel< ID2, T > func, [Out] T[] data, int count, int tuple_size )
	{
		getArray( id1, id2, id3, 0, 0, null, null, func, null, null, data, count, tuple_size );
	}
	
	public static void getArray4Id< ID2, ID4, T >(
		int id1, ID2 id2, int id3, ID4 id4,
		getArray4IdDel< ID2, ID4, T > func, [Out] T[] data, int count )
	{
		getArray( id1, id2, id3, id4, 0, null, null, null, func, null, data, count, 1 );
	}
	public static void getArray4Id< ID2, ID4, T >(
		int id1, ID2 id2, int id3, ID4 id4,
		getArray4IdDel< ID2, ID4, T > func, [Out] T[] data, int count, int tuple_size )
	{
		getArray( id1, id2, id3, id4, 0, null, null, null, func, null, data, count, tuple_size );
	}

	public static void getArray5Id< ID2, ID4, T >(
		int id1, ID2 id2, int id3, ID4 id4, int id5,
		getArray5IdDel< ID2, ID4, T > func, [Out] T [] data, int count )
	{
		getArray( id1, id2, id3, id4, id5, null, null, null, null, func, data, count, 1 );
	}
	public static void getArray5Id< ID2, ID4, T >(
		int id1, ID2 id2, int id3, ID4 id4, int id5, 
		getArray5IdDel< ID2, ID4, T > func, [Out] T [] data, int count, int tuple_size )
	{
		getArray( id1, id2, id3, id4, id5, null, null, null, null, func, data, count, tuple_size );
	}
	
	private static void getArray< ID2, ID4, T >(
		int id1, ID2 id2, int id3, ID4 id4, int id5,
		getArray1IdDel< T > func_1id, 
		getArray2IdDel< ID2, T > func_2id, 
		getArray3IdDel< ID2, T > func_3id,
		getArray4IdDel< ID2, ID4, T > func_4id,
		getArray5IdDel< ID2, ID4, T > func_5id,
		[Out] T[] data, int count, int tuple_size )
	{
		int max_array_size = HoudiniConstants.HAPI_MAX_PAGE_SIZE / ( Marshal.SizeOf( typeof( T ) ) * tuple_size );
		
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
			else if ( func_5id != null )
				func_5id( id1, id2, id3, id4, id5, local_array, current_index, length );
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
	
	public static void setArray2Id< ID2, T >( int id1, ID2 id2, getArray2IdDel< ID2, T > func, [Out] T[] data, 
										 int count )
	{
		setArray( id1, id2, 0, null, func, null, data, count, 1 );
	}
	public static void setArray2Id< ID2, T >( int id1, ID2 id2, getArray2IdDel< ID2, T > func, [Out] T[] data, 
										 int count, int tuple_size )
	{
		setArray( id1, id2, 0, null, func, null, data, count, tuple_size );
	}
	
	public static void setArray3Id< ID2, T >( int id1, ID2 id2, int id3, getArray3IdDel< ID2, T > func, [Out] T[] data, 
										 int count )
	{
		setArray( id1, id2, id3, null, null, func, data, count, 1 );
	}
	public static void setArray3Id< ID2, T >( int id1, ID2 id2, int id3, getArray3IdDel< ID2, T > func, [Out] T[] data, 
										 int count, int tuple_size )
	{
		setArray( id1, id2, id3, null, null, func, data, count, tuple_size );
	}
	
	private static void setArray< ID2, T >( int id1, ID2 id2, int id3, 
									   getArray1IdDel< T > func_1id, 
									   getArray2IdDel< ID2, T > func_2id, 
									   getArray3IdDel< ID2, T > func_3id,
									   [Out] T[] data, int count, int tuple_size )
	{
		int max_array_size = HoudiniConstants.HAPI_MAX_PAGE_SIZE / ( Marshal.SizeOf( typeof( T ) ) * tuple_size );
		
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
		
	public delegate void getAttrArrayInputFunc< T >( int asset_id, int object_id, int geo_id, int part_id, 
													 string name, ref HAPI_AttributeInfo info, 
													 [Out] T[] items, int start, int end );
	public static void getAttrArray< T >( int asset_id, int object_id, int geo_id, int part_id, 
										  string name, ref HAPI_AttributeInfo info, T[] items,
									 	  getAttrArrayInputFunc< T > get_func, int count ) 
	{
		int max_array_size = HoudiniConstants.HAPI_MAX_PAGE_SIZE / ( Marshal.SizeOf( typeof( T ) ) * info.tupleSize );
		
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
			get_func( asset_id, object_id, geo_id, part_id, name, ref info, local_array, current_index, length );
			
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
		int max_array_size = 0;
		if ( typeof( T ) == typeof( string ) )
		{
			int max_string_length = 1;
			foreach ( T s in items )
			{
				string ss = (string)(object) s;
				if ( ss.Length > max_string_length )
					max_string_length = ss.Length;
			}
			max_array_size =
				HoudiniConstants.HAPI_MAX_PAGE_SIZE
				/ ( max_string_length * Marshal.SizeOf( typeof( char ) ) * info.tupleSize );
		}
		else
			max_array_size =
				HoudiniConstants.HAPI_MAX_PAGE_SIZE
				/ ( Marshal.SizeOf( typeof( T ) ) * info.tupleSize );
		
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
	
	public static void getAttribute< T >( int asset_id, int object_id, int geo_id, int part_id, string name, 
										  ref HAPI_AttributeInfo info, ref T[] data,
										  getAttrArrayInputFunc< T > get_func )
	{
		int original_tuple_size = info.tupleSize;
		
		for ( HAPI_AttributeOwner type = 0; type < HAPI_AttributeOwner.HAPI_ATTROWNER_MAX; ++type )
		{
			info = HoudiniHost.getAttributeInfo( asset_id, object_id, geo_id, part_id, name, type );
			if ( info.exists )
				break;
		}
		if ( !info.exists )
			return;
		
		if ( original_tuple_size > 0 )
			info.tupleSize = original_tuple_size;
		
		data = new T[ info.count * info.tupleSize ];
		getAttrArray( asset_id, object_id, geo_id, part_id, name, ref info, data, get_func, info.count );
	}
	
	public static void setAttribute< T >( int asset_id, int object_id, int geo_id, string name, 
										  ref HAPI_AttributeInfo info, ref T[] data, 
										  setAttrArrayInputFunc< T > get_func )
	{
		setAttrArray( asset_id, object_id, geo_id, name, ref info, data, get_func, info.count );
	}

	public static void printAttributeNames(
		int asset_id, int object_id, int geo_id, int part_id, HAPI_AttributeOwner owner )
	{
		string[] names = HoudiniHost.getAttributeNames( asset_id, object_id, geo_id, part_id, owner );
		
		string msg = "A" + asset_id + "O" + object_id + "G" + geo_id + "P" + part_id + " - ";
		
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
			HAPI_AttributeInfo attribute_info =
				HoudiniHost.getAttributeInfo( asset_id, object_id, geo_id, part_id, name, owner );

			if ( comma )
				msg += ",";
			else
				comma = true;
			msg += " " + name + "( " + attribute_info.originalOwner + " )";
		}
		
		Debug.Log( msg );
	}
	
	public static void printAllAttributeNames( int asset_id, int object_id, int geo_id, int part_id )
	{
		for ( int owner = 0; owner < (int) HAPI_AttributeOwner.HAPI_ATTROWNER_MAX; ++owner )
			printAttributeNames( asset_id, object_id, geo_id, part_id, (HAPI_AttributeOwner) owner );
	}

	public static void printGroups(
		int asset_id, int object_id, int geo_id, int part_id, HAPI_GroupType type )
	{
		string[] names = HoudiniHost.getGroupNames( asset_id, object_id, geo_id, type );
		
		string msg = "A" + asset_id + "O" + object_id + "G" + geo_id + "P" + part_id + " - ";
		
		switch ( type )
		{
			case HAPI_GroupType.HAPI_GROUPTYPE_POINT:	msg += "Point"; break;
			case HAPI_GroupType.HAPI_GROUPTYPE_PRIM: 	msg += "Primitive"; break;
			default: Debug.LogError( "Invalid HAPI_GroupType!" ); return;
		}
		
		msg += " Groups:";

		string membership_msg = "";

		bool comma = false;
		foreach ( string name in names )
		{
			if ( comma )
				msg += ",";
			else
				comma = true;
			msg += " " + name;

			membership_msg += name + " > ";
			bool[] membership = HoudiniHost.getGroupMembership(
				asset_id, object_id, geo_id, part_id, type, name );
			for ( int i = 0; i < membership.Length; ++i )
				if ( membership[ i ] )
					membership_msg += i.ToString() + " ";
			membership_msg += "\n";
		}
		
		Debug.Log( msg + "\n" + membership_msg );
	}

	public static void printAllGroups( int asset_id, int object_id, int geo_id, int part_id )
	{
		for ( int type = 0; type < (int) HAPI_GroupType.HAPI_GROUPTYPE_MAX; ++type )
			printGroups( asset_id, object_id, geo_id, part_id, (HAPI_GroupType) type );
	}

	// PARAMETERS ---------------------------------------------------------------------------------------------------
	
	public static int findParm( ref HAPI_ParmInfo[] parms, string name )
	{
		if ( parms == null )
			return -1;
		
		for ( int i = 0; i < parms.Length; ++i )
			if ( parms[ i ].name == name )
				return parms[ i ].id;
		return -1;
	}

	public static float getParmFloatValue( int node_id, string parm_name, float default_value )
	{
		HAPI_NodeInfo node_info	= HoudiniHost.getNodeInfo( node_id );

		// Get all parameters.
		HAPI_ParmInfo[] parms = new HAPI_ParmInfo[ node_info.parmCount ];
		getArray1Id( node_id, HoudiniHost.getParameters, parms, node_info.parmCount );

		int parm_id = findParm( ref parms, parm_name );
		if ( parm_id < 0 )
			return default_value;

		int values_index = parms[ parm_id ].floatValuesIndex;
		float[] value = new float[ 1 ];

		HoudiniHost.getParmFloatValues( node_id, value, values_index, 1 );

		return value[ 0 ];
	}

	public static Color getParmColour3Value( int node_id, string parm_name, Color default_value )
	{
		HAPI_NodeInfo node_info	= HoudiniHost.getNodeInfo( node_id );

		// Get all parameters.
		HAPI_ParmInfo[] parms = new HAPI_ParmInfo[ node_info.parmCount ];
		getArray1Id( node_id, HoudiniHost.getParameters, parms, node_info.parmCount );

		int parm_id = findParm( ref parms, parm_name );
		if ( parm_id < 0 )
			return default_value;

		if ( parms[ parm_id ].size < 3 )
			Debug.LogError( "Parm size not large enough to be a color3." );

		int values_index = parms[ parm_id ].floatValuesIndex;
		float[] values = new float[ 3 ];

		HoudiniHost.getParmFloatValues( node_id, values, values_index, 3 );

		return new Color( values[ 0 ], values[ 1 ], values[ 2 ], 1.0f );
	}

	public static Color getParmColour4Value( int node_id, string parm_name, Color default_value )
	{
		HAPI_NodeInfo node_info	= HoudiniHost.getNodeInfo( node_id );

		// Get all parameters.
		HAPI_ParmInfo[] parms = new HAPI_ParmInfo[ node_info.parmCount ];
		getArray1Id( node_id, HoudiniHost.getParameters, parms, node_info.parmCount );

		int parm_id = findParm( ref parms, parm_name );
		if ( parm_id < 0 )
			return default_value;

		if ( parms[ parm_id ].size < 4 )
			Debug.LogError( "Parm size not large enough to be a color4." );

		int values_index = parms[ parm_id ].floatValuesIndex;
		float[] values = new float[ 4 ];

		HoudiniHost.getParmFloatValues( node_id, values, values_index, 4 );

		return new Color( values[ 0 ], values[ 1 ], values[ 2 ], values[ 3 ] );
	}

	// TEXTURES -----------------------------------------------------------------------------------------------------
	
	public static void reApplyMaterials( HoudiniAsset asset )
	{
		foreach ( HoudiniPartControl part_control in asset.GetComponentsInChildren< HoudiniPartControl >() )
		{
			try
			{
				if ( part_control.gameObject.GetComponent< MeshRenderer >() )
					assignMaterial( part_control, asset, true );
			}
			catch ( HoudiniError error )
			{
				Debug.Log( error.ToString() );
			}
		}
	}

	public static bool isMaterialTransparent( HAPI_MaterialInfo material_info )
	{
		float alpha = getParmFloatValue( material_info.nodeId, "ogl_alpha", 1.0f );

		return ( alpha < 0.95f );
	}

	public static void assignMaterial(
		HoudiniPartControl part_control, HoudiniAsset asset, bool update_houdini_material )
	{
		// Get all the juicy infos.
		HAPI_GeoInfo geo_info = new HAPI_GeoInfo();
		HoudiniHost.getGeoInfo(
			asset.prAssetId, part_control.prObjectId, part_control.prGeoId, out geo_info );
		HAPI_PartInfo part_info = new HAPI_PartInfo();
		HoudiniHost.getPartInfo(
			asset.prAssetId, part_control.prObjectId, part_control.prGeoId, 
			part_control.prPartId, out part_info );

		// Determine if this is a mesh and therefore actually needs to be textured.
		bool is_mesh = ( part_info.vertexCount > 0 );
		if ( !is_mesh
#if UNITY_EDITOR
			|| ( part_control.prPartName.Contains( HoudiniHost.prCollisionGroupName ) && 
				!part_control.prPartName.Contains( HoudiniHost.prRenderedCollisionGroupName ) )
#endif // UNITY_EDITOR
			)
			return;

		// Determine if we are dealing with multiple materials per-mesh.
		bool has_multiple_materials = !asset.prSplitGeosByGroup && geo_info.primitiveGroupCount > 0;

		// Count the number of materials.
		int material_count = 0;
		if ( has_multiple_materials )
			material_count = geo_info.primitiveGroupCount;
		else
			material_count = 1;

		// Get the mesh renderer.
		GameObject part_node = part_control.gameObject;
		MeshRenderer mesh_renderer = part_node.GetComponent< MeshRenderer >();
		if ( !mesh_renderer )
			throw new HoudiniError( "No mesh renderer!" );

		// Get the shared materials array or create one if it doesn't exist.
		Material[] materials = null;
		if ( mesh_renderer.sharedMaterials != null && mesh_renderer.sharedMaterials.Length == material_count )
			materials = mesh_renderer.sharedMaterials;
		else
			materials = new Material[ material_count ];

		// If we are dealing with multiple materials fetch the groups.
		string[] groups = null;
		if ( has_multiple_materials )
		{
			groups = HoudiniHost.getGroupNames(
				asset.prAssetId, part_control.prObjectId, part_control.prGeoId,
				HAPI_GroupType.HAPI_GROUPTYPE_PRIM );
			if ( groups.Length != geo_info.primitiveGroupCount )
				Debug.LogError( "Inconsistent group counts on geo." );
		}

		// Go through each material and update, create, or load it.
		for ( int m = 0; m < material_count; ++m )
		{
			Material material = null;

			// First check for a Unity material setup. If this is a Unity material
			// we can skip the rest of the loop body.
			if ( has_multiple_materials )
				material = getUnityMaterialOnGroup( part_control, groups[ m ] );
			else
				material = getUnityMaterialOnPart( part_control );
			if ( material != null )
			{
				materials[ m ] = material;
				continue;
			}

			// Get the material from the shared materials list or create it if it doesn't exist.
			if ( materials[ m ] != null )
				material = materials[ m ];
			else
			{
				material = new Material( Shader.Find( "Houdini/SpecularVertexColor" ) );
				materials[ m ] = material;
			}

			// Get the material info.
			HAPI_MaterialInfo material_info = new HAPI_MaterialInfo();
			if ( has_multiple_materials )
				material_info = HoudiniHost.getMaterialOnGroup(
					asset.prAssetId, part_control.prObjectId, part_control.prGeoId, groups[ m ] );
			else
				material_info = HoudiniHost.getMaterialOnPart(
					asset.prAssetId, part_control.prObjectId, part_control.prGeoId,
					part_control.prPartId );
			if ( !material_info.exists )
			{
				// Reset the material.
				material = new Material( Shader.Find( "Houdini/SpecularVertexColor" ) );
				materials[ m ] = material;
				part_control.prMaterialId = -1;
				continue;
			}

			// TODO: Temporary hack until we switch to centralized material depo.
			if ( part_control.prMaterialId != material_info.id )
			{
				material_info.hasChanged = true;
				part_control.prMaterialId = material_info.id;
			}

			// Check if we actually need to update the material.
			if ( !update_houdini_material && !material_info.hasChanged && material.mainTexture != null )
				continue;

			// Reset textures.
			material.mainTexture = null;
			material.SetTexture( "_NormalMap", null );
	
			// Assign vertex color shader if the flag says so.
			if ( asset.prShowOnlyVertexColours )
				material.shader = Shader.Find( "Houdini/SpecularVertexColor" );
			else
			{
				// Assign the transparency shader if this material is transparent or unassign it otherwise.
				if ( isMaterialTransparent( material_info ) )
					material.shader = Shader.Find( "Houdini/AlphaSpecularVertexColor" );
				else
					material.shader = Shader.Find( "Houdini/SpecularVertexColor" );
	
				// Before assigning material, make sure that if the asset is a prefab instance, all 
				// modifications are saved before assignHoudiniMaterial is called because ImportAsset may 
				// be called within this function which will cause OnEnable to be called on all prefab 
				// instances if the prefab is dirty.
#if UNITY_EDITOR
				if ( asset.isPrefabInstance() )
					PrefabUtility.RecordPrefabInstancePropertyModifications( asset );
#endif // UNITY_EDITOR

				string folder_path =
					HoudiniConstants.HAPI_TEXTURES_PATH + "/" + part_control.prAsset.prAssetName;
				assignHoudiniMaterial(
					ref material, material_info, folder_path );
			}

			materials[ m ] = material;
		}

		mesh_renderer.sharedMaterials = materials;
	}

	public static Texture2D extractHoudiniImageToTexture( 
		HAPI_MaterialInfo material_info, string folder_path, string image_planes )
	{
		Texture2D result = null;
		try
		{
#if UNITY_EDITOR
			if ( HoudiniHost.prDontCreateTextureFiles )
#endif // UNITY_EDITOR
			{
				if ( HoudiniHost.prExtractTexturesInRawFormat )
				{
					HAPI_ImageInfo image_info = HoudiniHost.getImageInfo( material_info.assetId, material_info.id );

					image_info.dataFormat = HAPI_ImageDataFormat.HAPI_IMAGE_DATA_INT8;
					image_info.interleaved = true;
					image_info.packing = HAPI_ImagePacking.HAPI_IMAGE_PACKING_RGBA;
					image_info.gamma = (double) HoudiniHost.prGamma;

					HoudiniHost.setImageInfo( material_info.assetId, material_info.id, ref image_info );

					// Extract image to memory.
					byte[] image_data = HoudiniHost.extractImageToMemory( 
						material_info.assetId, material_info.id, HoudiniConstants.HAPI_RAW_FORMAT_NAME, image_planes );

					int colour_data_size = image_info.xRes * image_info.yRes;
					
					if ( colour_data_size * 4 != image_data.Length )
						Debug.LogError( "Image data corrupt some how. Try not using in-memory textures." );
					else
					{
						Color32[] colour_data = new Color32[ colour_data_size ];
						for ( int i = 0; i < colour_data_size; ++i )
						{
							colour_data[ i ].r = image_data[ i * 4 + 0 ];
							colour_data[ i ].g = image_data[ i * 4 + 1 ];
							colour_data[ i ].b = image_data[ i * 4 + 2 ];
							colour_data[ i ].a = image_data[ i * 4 + 3 ];
						}

						Texture2D tex = new Texture2D( image_info.xRes, image_info.yRes, TextureFormat.ARGB32, false );
						tex.SetPixels32( colour_data );
						tex.Apply();
						result = tex;
					}
				}
				else
				{
					// Make sure the image format selected is supported by Unity's in-memory texture loading.
					string desired_file_format = null;
					HAPI_ImageInfo image_info = HoudiniHost.getImageInfo( material_info.assetId, material_info.id );
					if ( !image_info.isImageFileFormat( HoudiniConstants.HAPI_PNG_FORMAT_NAME ) &&
						 !image_info.isImageFileFormat( HoudiniConstants.HAPI_JPEG_FORMAT_NAME ) )
					{
						desired_file_format = HoudiniConstants.HAPI_PNG_FORMAT_NAME;
					}

					image_info.gamma = HoudiniHost.prGamma;
					HoudiniHost.setImageInfo( material_info.assetId, material_info.id, ref image_info );

					// Extract image to memory.
					byte[] image_data = HoudiniHost.extractImageToMemory( 
						material_info.assetId, material_info.id, desired_file_format, image_planes );

					// Initial size doesn't matter as LoadImage() will change the size and format.
					Texture2D tex = new Texture2D( 1, 1 );
					tex.LoadImage( image_data );
					result = tex;
				}
			}
#if UNITY_EDITOR
			else // Figure out the source file path and name.
			{
				// Navigate to the Assets/Textures directory and create it if it doesn't exist.
				DirectoryInfo textures_dir = new DirectoryInfo( folder_path );
				if ( !textures_dir.Exists )
					textures_dir.Create();

				// Make sure the image format selected is supported by Unity.
				string desired_file_format = null;
				HAPI_ImageInfo image_info = HoudiniHost.getImageInfo( material_info.assetId, material_info.id );
				if ( !image_info.isImageFileFormat( HoudiniConstants.HAPI_PNG_FORMAT_NAME ) &&
					 !image_info.isImageFileFormat( HoudiniConstants.HAPI_JPEG_FORMAT_NAME ) &&
					 !image_info.isImageFileFormat( HoudiniConstants.HAPI_BMP_FORMAT_NAME ) &&
					 !image_info.isImageFileFormat( HoudiniConstants.HAPI_TGA_FORMAT_NAME ) )
				{
					desired_file_format = HoudiniConstants.HAPI_PNG_FORMAT_NAME;
				}

				// Extract image to file.
				string texture_file_path = HoudiniHost.extractImageToFile(
					material_info.assetId, material_info.id, desired_file_format, image_planes, folder_path );

				string relative_file_path = texture_file_path.Replace(
					Application.dataPath, "Assets" );

				// Load the texture and assign it to the material. Note that LoadAssetAtPath only 
				// understands paths relative to the project folder.
				AssetDatabase.ImportAsset( relative_file_path, ImportAssetOptions.Default );
				Object tex_obj = AssetDatabase.LoadAssetAtPath( relative_file_path, typeof( Texture2D ) );

				// Assign main texture.
				result = (Texture2D) tex_obj;
			}
#endif // UNITY_EDITOR
		}
		catch ( HoudiniError )
		{
			result = null;
		}

		return result;
	}

	public static void assignHoudiniMaterial( 
		ref Material material, HAPI_MaterialInfo material_info, string folder_path )
	{
		// Get all parameters.
		HAPI_NodeInfo node_info	= HoudiniHost.getNodeInfo( material_info.nodeId );
		HAPI_ParmInfo[] parms = new HAPI_ParmInfo[ node_info.parmCount ];
		getArray1Id( material_info.nodeId, HoudiniHost.getParameters, parms, node_info.parmCount );
		
		// Extract diffuse map file from material.
		int diffuse_map_parm_id = findParm( ref parms, "ogl_tex1" );
		if ( diffuse_map_parm_id < 0 )
			diffuse_map_parm_id = findParm( ref parms, "baseColorMap" );
		if ( diffuse_map_parm_id < 0 )
			diffuse_map_parm_id = findParm( ref parms, "map" );
		if ( diffuse_map_parm_id >= 0 )
		{
			try
			{
				HoudiniHost.renderTextureToImage( 
					material_info.assetId, material_info.id, diffuse_map_parm_id );

				material.mainTexture = extractHoudiniImageToTexture( material_info, folder_path, "C A" );
				material.SetTexture( 
					"_NormalMap", extractHoudiniImageToTexture( material_info, folder_path, "N" ) );
			}
			catch ( HoudiniError ) {}
		}
			
		// Assign shader properties.

		material.SetFloat( "_Shininess", 1.0f - getParmFloatValue( material_info.nodeId, "ogl_rough", 0.0f ) );

		Color diffuse_colour = getParmColour3Value( material_info.nodeId, "ogl_diff", Color.white );
		diffuse_colour.a = getParmFloatValue( material_info.nodeId, "ogl_alpha", 1.0f );
		material.SetColor( "_Color", diffuse_colour );

		material.SetColor( "_SpecColor", getParmColour3Value( material_info.nodeId, "ogl_spec", Color.black ) );
	}

	public static Material getUnityMaterial( string material_path, int index, HoudiniPartControl part_control )
	{
		// Get position attributes.
		int asset_id	= part_control.prAssetId;
		int object_id	= part_control.prObjectId;
		int geo_id		= part_control.prGeoId;
		int part_id		= part_control.prPartId;

		HAPI_AttributeInfo sub_material_name_attr_info =
			new HAPI_AttributeInfo( HoudiniHost.prUnitySubMaterialNameAttribName );
		int[] sub_material_name_attr = new int[ 0 ];
		getAttribute(
			asset_id, object_id, geo_id, part_id, HoudiniHost.prUnitySubMaterialNameAttribName,
			ref sub_material_name_attr_info, ref sub_material_name_attr, HoudiniHost.getAttributeStringData );

		HAPI_AttributeInfo sub_material_index_attr_info =
			new HAPI_AttributeInfo( HoudiniHost.prUnitySubMaterialIndexAttribName );

		int[] sub_material_index_attr = new int[ 0 ];
		getAttribute(
			asset_id, object_id, geo_id, part_id, HoudiniHost.prUnitySubMaterialIndexAttribName, 
			ref sub_material_index_attr_info, ref sub_material_index_attr, HoudiniHost.getAttributeIntData );

		Material material = (Material) Resources.Load( material_path, typeof( Material ) );

#if UNITY_EDITOR
		bool has_sub_material_name =
			sub_material_name_attr_info.exists && HoudiniHost.getString( sub_material_name_attr[ index ] ) != "";
		bool has_sub_material_index = sub_material_index_attr_info.exists;

		string sub_material_name =
			has_sub_material_name ? HoudiniHost.getString( sub_material_name_attr[ index ] ) : "";
		int sub_material_index = has_sub_material_index ? sub_material_index_attr[ index ] : 0;

		if ( material == null )
		{
			// Try explicit import.
			AssetDatabase.ImportAsset( material_path, ImportAssetOptions.Default );
			material = (Material) AssetDatabase.LoadAssetAtPath( material_path, typeof( Material ) );
		}

		if ( material != null && ( has_sub_material_name || has_sub_material_index ) )
		{
			// Try Substance materials.
			string abs_path = AssetDatabase.GetAssetPath( material );

			SubstanceImporter substance_importer = AssetImporter.GetAtPath( abs_path ) as SubstanceImporter;
				
			if ( has_sub_material_name )
			{
				ProceduralMaterial[] procedural_materials = substance_importer.GetMaterials();
				for ( int i = 0; i < procedural_materials.Length; ++i )
				{
					if ( procedural_materials[ i ].name == sub_material_name )
					{
						material = procedural_materials[ i ];
						break;
					}
				}
			}
			else if ( sub_material_index >= 0 && 
						sub_material_index < substance_importer.GetMaterialCount() )
			{
				material = substance_importer.GetMaterials()[ sub_material_index ];
			}
			else
				Debug.LogWarning(
					"sub_material_index (" + sub_material_index + ") out of range for material: " + abs_path );
		}
#endif // UNITY_EDITOR
		return material;
	}

	public static Material getUnityMaterialOnGroup( HoudiniPartControl part_control, string group_name )
	{
		// Get position attributes.
		int asset_id	= part_control.prAssetId;
		int object_id	= part_control.prObjectId;
		int geo_id		= part_control.prGeoId;
		int part_id		= part_control.prPartId;

		HAPI_AttributeInfo material_attr_info = new HAPI_AttributeInfo( HoudiniHost.prUnityMaterialAttribName );
		int[] material_attr = new int[ 0 ];
		getAttribute( asset_id, object_id, geo_id, part_id, HoudiniHost.prUnityMaterialAttribName, 
					  ref material_attr_info, ref material_attr, HoudiniHost.getAttributeStringData );

		if ( !material_attr_info.exists )
			return null;

		bool[] group_membership = HoudiniHost.getGroupMembership(
			asset_id, object_id, geo_id, part_id, HAPI_GroupType.HAPI_GROUPTYPE_PRIM, group_name );

		int index = 0;
		string material_path = "";
		for ( int i = 0; i < material_attr_info.count; ++i )
			if ( group_membership[ i ] )
			{
				material_path = HoudiniHost.getString( material_attr[ i ] );
				index = i;
				break;
			}

		return getUnityMaterial( material_path, index, part_control );
	}

	public static Material getUnityMaterialOnPart( HoudiniPartControl part_control )
	{
		// Get position attributes.
		int asset_id	= part_control.prAssetId;
		int object_id	= part_control.prObjectId;
		int geo_id		= part_control.prGeoId;
		int part_id		= part_control.prPartId;

		HAPI_AttributeInfo material_attr_info = new HAPI_AttributeInfo( HoudiniHost.prUnityMaterialAttribName );
		int[] material_attr = new int[ 0 ];
		getAttribute( asset_id, object_id, geo_id, part_id, HoudiniHost.prUnityMaterialAttribName, 
					  ref material_attr_info, ref material_attr, HoudiniHost.getAttributeStringData );

		if ( !material_attr_info.exists )
			return null;

		// Need to get the material path here because the next call to HAPI_Host.getAttributeStrData will
		// overwrite the string ids in material_attr.
		string material_path = HoudiniHost.getString( material_attr[ 0 ] );

		return getUnityMaterial( material_path, 0, part_control );
	}
	
	// GEOMETRY MARSHALLING -----------------------------------------------------------------------------------------

	public static void getMesh(
		HoudiniPartControl part_control,
		Mesh mesh,
		bool generate_uvs,
		bool generate_lightmap_uv2s,
		bool generate_tangents )
	{
		int asset_id	= part_control.prAssetId;
		int object_id	= part_control.prObjectId;
		int geo_id		= part_control.prGeoId;
		int part_id		= part_control.prPartId;

		// Get Detail info.
		HAPI_GeoInfo geo_info = new HAPI_GeoInfo();
		HoudiniHost.getGeoInfo(
			 asset_id, object_id, geo_id, out geo_info );
		HAPI_PartInfo part_info = new HAPI_PartInfo();
		HoudiniHost.getPartInfo( asset_id, object_id, geo_id, part_id, out part_info );
		
		// Make sure our primitive and vertex numbers are supported by Unity.
		// TODO: add this limit in a more proper place
		if ( part_info.faceCount > 65000 * 3 )
			throw new HoudiniError( part_control.name + ": Face count (" + part_info.faceCount 
								  + ") above limit (" + ( 65000 * 3 ) + ")!" );
		if ( part_info.vertexCount > 65000 )
			throw new HoudiniError( part_control.name + ": Vertex count (" + part_info.vertexCount + ") above limit (" + 65000 + ")!" );

		// Get Face counts.
		int[] face_counts = new int[ part_info.faceCount ];
		getArray4Id( asset_id, object_id, geo_id, part_id, HoudiniHost.getFaceCounts, 
					 face_counts, part_info.faceCount );
		
		// Get Vertex list.
		int[] vertex_list = new int[ part_info.vertexCount ];
		getArray4Id( asset_id, object_id, geo_id, part_id, HoudiniHost.getVertexList, 
					 vertex_list, part_info.vertexCount );

		// Get position attributes.
		HAPI_AttributeInfo pos_attr_info = new HAPI_AttributeInfo( HoudiniConstants.HAPI_ATTRIB_POSITION );
		float[] pos_attr = new float[ 0 ];
		getAttribute( asset_id, object_id, geo_id, part_id, HoudiniConstants.HAPI_ATTRIB_POSITION, 
					  ref pos_attr_info, ref pos_attr, HoudiniHost.getAttributeFloatData );
		if ( !pos_attr_info.exists )
			throw new HoudiniError( "No position attribute found." );
		else if ( pos_attr_info.owner != HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
			throw new HoudiniErrorIgnorable( "I only understand position as point attributes!" );
				
		// Get uv attributes.
		HAPI_AttributeInfo uv_attr_info = new HAPI_AttributeInfo( HoudiniConstants.HAPI_ATTRIB_UV );
		uv_attr_info.tupleSize = 2;
		float[] uv_attr = new float[ 0 ];
		getAttribute( asset_id, object_id, geo_id, part_id, HoudiniConstants.HAPI_ATTRIB_UV, 
					  ref uv_attr_info, ref uv_attr, HoudiniHost.getAttributeFloatData );
		// Get uv2 attributes.
		HAPI_AttributeInfo uv2_attr_info = new HAPI_AttributeInfo( HoudiniConstants.HAPI_ATTRIB_UV2 );
		uv2_attr_info.tupleSize = 2;
		float[] uv2_attr = new float[ 0 ];
		getAttribute( asset_id, object_id, geo_id, part_id, HoudiniConstants.HAPI_ATTRIB_UV2, 
					  ref uv2_attr_info, ref uv2_attr, HoudiniHost.getAttributeFloatData );
		// Get uv3 attributes.
		HAPI_AttributeInfo uv3_attr_info = new HAPI_AttributeInfo( HoudiniConstants.HAPI_ATTRIB_UV3 );
		uv3_attr_info.tupleSize = 2;
		float[] uv3_attr = new float[ 0 ];
		getAttribute( asset_id, object_id, geo_id, part_id, HoudiniConstants.HAPI_ATTRIB_UV3, 
					  ref uv3_attr_info, ref uv3_attr, HoudiniHost.getAttributeFloatData );

		// Get normal attributes.
		HAPI_AttributeInfo normal_attr_info = new HAPI_AttributeInfo( HoudiniConstants.HAPI_ATTRIB_NORMAL );
		float[] normal_attr = new float[ 0 ];
		getAttribute( asset_id, object_id, geo_id, part_id, HoudiniConstants.HAPI_ATTRIB_NORMAL, 
					  ref normal_attr_info, ref normal_attr, HoudiniHost.getAttributeFloatData );

		// Get colour attributes.
		HAPI_AttributeInfo colour_attr_info = new HAPI_AttributeInfo( HoudiniConstants.HAPI_ATTRIB_COLOR );
		float[] colour_attr = new float[ 0 ];
		getAttribute( asset_id, object_id, geo_id, part_id, HoudiniConstants.HAPI_ATTRIB_COLOR, 
					  ref colour_attr_info, ref colour_attr, HoudiniHost.getAttributeFloatData );

		// Get tangent attributes.
		HAPI_AttributeInfo tangent_attr_info = new HAPI_AttributeInfo( HoudiniConstants.HAPI_ATTRIB_TANGENT );
		float[] tangent_attr = new float[ 0 ];
		getAttribute( asset_id, object_id, geo_id, part_id, HoudiniConstants.HAPI_ATTRIB_TANGENT, 
					  ref tangent_attr_info, ref tangent_attr, HoudiniHost.getAttributeFloatData );
		
		// Save properties.
		part_control.prVertexList			= vertex_list;
		
		// Create Unity-specific data objects.
		Vector3[] vertices 	= new Vector3[ 	part_info.vertexCount ];
		int[] triangles 	= new int[ 		part_info.faceCount * 3 ];
		Vector2[] uvs 		= new Vector2[ 	part_info.vertexCount ];
		Vector2[] uv2s		= new Vector2[  part_info.vertexCount ];
		Vector2[] uv3s		= new Vector2[  part_info.vertexCount ];
		Vector3[] normals 	= new Vector3[ 	part_info.vertexCount ];
		Color[] colours		= new Color[	part_info.vertexCount ];
		Vector4[] tangents	= generate_tangents ? new Vector4[ part_info.vertexCount ] : null;
		
		// Fill Unity-specific data objects with data from the runtime.
		for ( int i = 0; i < part_info.vertexCount; ++i ) 
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
				if ( uv_attr_info.owner == HAPI_AttributeOwner.HAPI_ATTROWNER_VERTEX )
					for ( int j = 0; j < 2; ++j )
						uvs[ i ][ j ] = uv_attr[ i * 2 + j ];
				
				// If the UVs are per point use the vertex list array point indicies to query into
				// the UV array we filled above.
				else if ( uv_attr_info.owner == HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
					for ( int j = 0; j < 2; ++j )
						uvs[ i ][ j ] = uv_attr[ vertex_list[ i ] * 2 + j ];
			}
			if ( uv2_attr_info.exists )
			{
				// If the UVs are per vertex just query directly into the UV array we filled above.
				if ( uv2_attr_info.owner == HAPI_AttributeOwner.HAPI_ATTROWNER_VERTEX )
					for ( int j = 0; j < 2; ++j )
						uv2s[ i ][ j ] = uv2_attr[ i * 2 + j ];
				
				// If the UVs are per point use the vertex list array point indicies to query into
				// the UV array we filled above.
				else if ( uv2_attr_info.owner == HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
					for ( int j = 0; j < 2; ++j )
						uv2s[ i ][ j ] = uv2_attr[ vertex_list[ i ] * 2 + j ];
			}
			if ( uv3_attr_info.exists )
			{
				// If the UVs are per vertex just query directly into the UV array we filled above.
				if ( uv3_attr_info.owner == HAPI_AttributeOwner.HAPI_ATTROWNER_VERTEX )
					for ( int j = 0; j < 2; ++j )
						uv3s[ i ][ j ] = uv3_attr[ i * 2 + j ];
				
				// If the UVs are per point use the vertex list array point indicies to query into
				// the UV array we filled above.
				else if ( uv3_attr_info.owner == HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
					for ( int j = 0; j < 2; ++j )
						uv3s[ i ][ j ] = uv3_attr[ vertex_list[ i ] * 2 + j ];
			}

			// Fill normals.
			if ( normal_attr_info.exists )
			{
				// If the normals are per vertex just query directly into the normals array we filled above.
				if ( normal_attr_info.owner == HAPI_AttributeOwner.HAPI_ATTROWNER_VERTEX )
					for ( int j = 0; j < 3; ++j )
					{
						normals[ i ][ j ] = normal_attr[ i * 3 + j ];
						// Flip the x coordinate.
						if ( j == 0 )
							normals[ i ][ j ] *= -1;
						
					}
				
				// If the normals are per point use the vertex list array point indicies to query into
				// the normal array we filled above.
				else if ( normal_attr_info.owner == HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
					for ( int j = 0; j < 3; ++j )
					{
						normals[ i ][ j ] = normal_attr[ vertex_list[ i ] * 3 + j ];
						// Flip the x coordinate.
						if ( j == 0 )
							normals[ i ][ j ] *= -1;
					}
				
				// If the normals are per face divide the vertex index by the number of vertices per face
				// which should always be HAPI_MAX_VERTICES_PER_FACE.
				else if ( normal_attr_info.owner == HAPI_AttributeOwner.HAPI_ATTROWNER_PRIM )
					for ( int j = 0; j < 3; ++j )
					{
						int face_index = i / HoudiniConstants.HAPI_MAX_VERTICES_PER_FACE;
						normals[ i ][ j ] 
							= normal_attr[ face_index * HoudiniConstants.HAPI_MAX_VERTICES_PER_FACE + j ];
						// Flip the x coordinate.
						if ( j == 0 )
							normals[ i ][ j ] *= -1;
					}
			}

			// Fill tangents.
			if ( generate_tangents && tangent_attr_info.exists )
			{
				int tuple_size = tangent_attr_info.tupleSize;

				// If the tangents are per vertex just query directly into the tangents array we filled above.
				if ( tangent_attr_info.owner == HAPI_AttributeOwner.HAPI_ATTROWNER_VERTEX )
					for ( int j = 0; j < tuple_size; ++j )
					{
						tangents[ i ][ j ] = tangent_attr[ i * tuple_size + j ];
						// Flip the x coordinate.
						if ( j == 0 )
							tangents[ i ][ j ] *= -1;
						
					}
				
				// If the tangent are per point use the vertex list array point indicies to query into
				// the tangent array we filled above.
				else if ( tangent_attr_info.owner == HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
					for ( int j = 0; j < tuple_size; ++j )
					{
						tangents[ i ][ j ] = tangent_attr[ vertex_list[ i ] * tuple_size + j ];
						// Flip the x coordinate.
						if ( j == 0 )
							tangents[ i ][ j ] *= -1;
					}
				
				// If the tangents are per face divide the vertex index by the number of vertices per face
				// which should always be HAPI_MAX_VERTICES_PER_FACE.
				else if ( tangent_attr_info.owner == HAPI_AttributeOwner.HAPI_ATTROWNER_PRIM )
					for ( int j = 0; j < tuple_size; ++j )
					{
						int face_index = i / HoudiniConstants.HAPI_MAX_VERTICES_PER_FACE;
						tangents[ i ][ j ] 
							= tangent_attr[ face_index * HoudiniConstants.HAPI_MAX_VERTICES_PER_FACE * tuple_size + j ];
						// Flip the x coordinate.
						if ( j == 0 )
							tangents[ i ][ j ] *= -1;
					}
			}

			// Fill colours.
			colours[ i ].r = 1.0f;
			colours[ i ].g = 1.0f;
			colours[ i ].b = 1.0f;
			colours[ i ].a = 1.0f;
			if ( colour_attr_info.exists &&
				part_control.prGeoControl.prGeoType != HAPI_GeoType.HAPI_GEOTYPE_INTERMEDIATE )
			{
				// If the colours are per vertex just query directly into the normals array we filled above.
				if ( colour_attr_info.owner == HAPI_AttributeOwner.HAPI_ATTROWNER_VERTEX )
					for ( int j = 0; j < colour_attr_info.tupleSize; ++j )
						colours[ i ][ j ] = colour_attr[ i * colour_attr_info.tupleSize + j ];
				
				// If the colours are per point use the vertex list array point indicies to query into
				// the normal array we filled above.
				else if ( colour_attr_info.owner == HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
					for ( int j = 0; j < colour_attr_info.tupleSize; ++j )
						colours[ i ][ j ] = colour_attr[ vertex_list[ i ] * colour_attr_info.tupleSize + j ];
				
				// If the colours are per face divide the vertex index by the number of vertices per face
				// which should always be HAPI_MAX_VERTICES_PER_FACE.
				else if ( colour_attr_info.owner == HAPI_AttributeOwner.HAPI_ATTROWNER_PRIM )
				{
					int face_index = i / HoudiniConstants.HAPI_MAX_VERTICES_PER_FACE;
					for ( int j = 0; j < colour_attr_info.tupleSize; ++j )
						colours[ i ][ j ] 
							= colour_attr[ face_index * HoudiniConstants.HAPI_MAX_VERTICES_PER_FACE + j ];
				}
			}
		}
		
		for ( int i = 0; i < part_info.faceCount; ++i ) 
			for ( int j = 0; j < 3; ++j )
				triangles[ i * 3 + j ] = i * 3 + j;
		
		// Set known mesh data.
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.normals = normals;

		// Set mesh UVs.
		bool has_non_trivial_uvs = uv_attr_info.exists || generate_uvs;
		mesh.uv = uvs; // Should assign, even if all-zero, to avoid warnings.
#if UNITY_EDITOR
		if ( generate_uvs )
			mesh.uv = Unwrapping.GeneratePerTriangleUV( mesh );
#endif // UNITY_EDITOR

		if ( uv2_attr_info.exists )
		{
#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6
			mesh.uv1 = uv2s;
#else
			mesh.uv2 = uv2s;
#endif // UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6
		}
		else if ( has_non_trivial_uvs && generate_lightmap_uv2s )
		{
#if UNITY_EDITOR
			UnwrapParam param;
			UnwrapParam.SetDefaults( out param );
			Unwrapping.GenerateSecondaryUVSet( mesh, param );
#endif // UNITY_EDITOR
		}

		if ( uv3_attr_info.exists )
		{
#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6
			mesh.uv2 = uv3s;
#else
			mesh.uv3	= uv3s;
#endif // UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6
		}

		// Set mesh tangents.
		if ( generate_tangents )
			mesh.tangents = tangents;

		// We don't want to set these if we're generating intermediate geo mesh because
		// we would overwrite the painted values.
		if ( part_control.prGeoControl.prGeoType != HAPI_GeoType.HAPI_GEOTYPE_INTERMEDIATE )
		{
			mesh.colors = colours;
		}

		mesh.RecalculateBounds();

		if ( !normal_attr_info.exists )
			mesh.RecalculateNormals();

		if ( mesh.uv != null && mesh.uv.Length > 0 && generate_tangents && !tangent_attr_info.exists )
			calculateMeshTangents( mesh );

		// Create the submeshes if needed.
		if ( !part_control.prAsset.prSplitGeosByGroup && geo_info.primitiveGroupCount > 0 )
		{
			string[] groups = HoudiniHost.getGroupNames(
				asset_id, object_id, geo_id, HAPI_GroupType.HAPI_GROUPTYPE_PRIM );

			// Destroy any existing colliders.
			MeshCollider[] old_colliders = part_control.gameObject.GetComponents< MeshCollider >();
			foreach ( MeshCollider collider in old_colliders )
				GameObject.DestroyImmediate( collider );

			mesh.subMeshCount = 0;
			for ( int g = 0; g < groups.Length; ++g )
			{
				string group = groups[ g ];

				bool[] mem = HoudiniHost.getGroupMembership(
					asset_id, object_id, geo_id, part_id, HAPI_GroupType.HAPI_GROUPTYPE_PRIM, group );

				int membership_count = 0;
				foreach ( bool m in mem )
					if ( m ) membership_count++;

				int[] group_triangles;
				if ( membership_count <= 0 )
				{
					// If we encounter an empty group just create a single dummy 
					// triangle (because you can't have a 0-triangle mesh) and
					// pretend like it's a valid group so the rest of the submesh
					// code can work without special cases.
					group_triangles = new int[ 3 ];
					group_triangles[ 0 ] = 0;
					group_triangles[ 1 ] = 0;
					group_triangles[ 2 ] = 0;
				}
				else
				{
					group_triangles = new int[ membership_count * 3 ];
					int current_triangle = 0;
					for ( int i = 0; i < part_info.faceCount; ++i )
						if ( mem[ i ] )
						{
							for ( int j = 0; j < 3; ++j )
								group_triangles[ current_triangle * 3 + j ] = i * 3 + j;
							current_triangle++;
						}
				}

				if ( group.Contains( HoudiniHost.prRenderedCollisionGroupName ) ||
					group.Contains( HoudiniHost.prCollisionGroupName ) )
				{
					MeshCollider new_collider = part_control.gameObject.AddComponent< MeshCollider >();
					Mesh collision_mesh = new Mesh();
					collision_mesh.vertices = vertices;
					collision_mesh.triangles = group_triangles;
					collision_mesh.normals = normals;
					collision_mesh.RecalculateBounds();
					new_collider.sharedMesh = collision_mesh;
					new_collider.enabled = false;
					new_collider.enabled = true;
				}
				
				if ( group.Contains( HoudiniHost.prRenderedCollisionGroupName )
					|| !group.Contains( HoudiniHost.prCollisionGroupName ) )
				{
					mesh.subMeshCount++;
					mesh.SetTriangles( group_triangles, g );
				}
			}
		}
	}

	private static void setMeshPointAttribute(
		int asset_id,
		int object_id,
		int geo_id,
		string attr, 
		int tuple_size,
		Vector3 [] input_data,
		bool setting_raw_mesh,
		bool adjust_for_coordinate_system,
		HAPI_PartInfo part_info,
		HoudiniPartControl part_control )
	{
		HAPI_AttributeInfo attr_info = new HAPI_AttributeInfo( attr );
		attr_info.exists 		= true;
		attr_info.owner 		= HAPI_AttributeOwner.HAPI_ATTROWNER_POINT;
		attr_info.storage 		= HAPI_StorageType.HAPI_STORAGETYPE_FLOAT;
		attr_info.count 		= part_info.pointCount;
		attr_info.tupleSize 	= tuple_size;

		float[] attr_values = new float[ part_info.pointCount * tuple_size ];

		bool data_ok = true;
		if ( setting_raw_mesh )
		{
			HoudiniHost.addAttribute(
				asset_id, object_id, geo_id,
				attr, ref attr_info );

			for ( int i = 0; i < part_info.pointCount; ++i )
				for ( int j = 0; j < tuple_size; ++j )
			{
				if( adjust_for_coordinate_system )
				{
					if ( j != 0 )
						attr_values[ i * tuple_size + j ] = input_data[ i ][ j ];
					else
						attr_values[ i * tuple_size + j ] = -input_data[ i ][ j ];
				}
				else
				{
					attr_values[ i * tuple_size + j ] = input_data[ i ][ j ];
				}
			}
		}
		else if ( attr == HoudiniConstants.HAPI_ATTRIB_POSITION )
		{
			HoudiniHost.addAttribute(
				asset_id, object_id, geo_id,
				attr, ref attr_info );

			for ( int ii = 0; ii < part_control.prVertexList.Length; ii++ )
			{
				int point_index = part_control.prVertexList[ ii ] * 3;
				
				attr_values[ point_index ] = -input_data[ ii ][ 0 ];
				attr_values[ point_index + 1 ] = input_data[ ii ][ 1 ];
				attr_values[ point_index + 2 ] = input_data[ ii ][ 2 ];
			}
		}
		else
		{
			data_ok = false;
			//Debug.LogWarning( "Marshalling of " + attr + " for editing not supported at this time." );
		}

		if ( data_ok )
		{
			setAttribute(
				asset_id, object_id, geo_id, attr,
				ref attr_info, ref attr_values, HoudiniHost.setAttributeFloatData );
		}
	}

	public static void setMesh(
		int asset_id, int object_id, int geo_id, ref Mesh mesh, 
		HoudiniPartControl part_control,
		HoudiniGeoAttributeManager attribute_manager )
	{
		bool setting_raw_mesh = ( part_control == null );

		Vector3[] vertices 				= mesh.vertices;
		int[] triangles 				= mesh.triangles;
		Vector2[] uvs 					= mesh.uv;
		Vector3[] normals 				= mesh.normals;
		
		HAPI_GeoInfo geo_info 			= new HAPI_GeoInfo();
		geo_info.id 					= geo_id;

		HAPI_PartInfo part_info			= new HAPI_PartInfo();

		if ( setting_raw_mesh )
		{
			part_info.faceCount			= triangles.Length / 3;
			part_info.vertexCount		= triangles.Length;
			part_info.pointCount		= vertices.Length;
		}
		else
		{
			part_info.faceCount = part_control.prVertexList.Length / 3;
			part_info.vertexCount = part_control.prVertexList.Length;

			int pointCount = 0;
			for ( int ii = 0; ii < part_control.prVertexList.Length; ii++ )
			{
				if ( part_control.prVertexList[ ii ] > pointCount )
					pointCount = part_control.prVertexList[ ii ];
			}
			//the values calculated from the loop are indices, so + 1 to get the count
			part_info.pointCount = pointCount + 1;
		}
		
		part_info.pointAttributeCount 	= 1;
		part_info.vertexAttributeCount 	= 0;
		part_info.faceAttributeCount 	= 0;
		part_info.detailAttributeCount 	= 0;

		if ( uvs != null )
			part_info.pointAttributeCount++;
		if ( normals != null )
			part_info.pointAttributeCount++;

		HoudiniHost.setGeoInfo( asset_id, object_id, geo_id, ref geo_info );
		HoudiniHost.setPartInfo( asset_id, object_id, geo_id, ref part_info );

		// Set Face counts.
		int[] face_counts = new int[ part_info.faceCount ];
		for ( int i = 0; i < part_info.faceCount; ++i )
			face_counts[ i ] = 3;
		setArray3Id( asset_id, object_id, geo_id, HoudiniHost.setFaceCounts, face_counts, part_info.faceCount );

		// Set Vertex list.
		int[] vertex_list = new int[ part_info.vertexCount ];
		if ( setting_raw_mesh )
			for ( int i = 0; i < part_info.faceCount; ++i )
				for ( int j = 0; j < 3; ++j )
					vertex_list[ i * 3 + j ] = triangles[ i * 3 + j ];
		else
			vertex_list = part_control.prVertexList;
		setArray3Id( asset_id, object_id, geo_id, HoudiniHost.setVertexList, vertex_list, part_info.vertexCount );

		// Set position attributes.
		setMeshPointAttribute(
			asset_id, object_id, geo_id,
			HoudiniConstants.HAPI_ATTRIB_POSITION, 3, vertices,
			setting_raw_mesh, true, part_info, part_control );

		setMeshPointAttribute(
			asset_id, object_id, geo_id, 
			HoudiniConstants.HAPI_ATTRIB_NORMAL, 3, normals,
			setting_raw_mesh, true, part_info, part_control );

		Vector3[] uvs3 = new Vector3[ uvs.Length ];
		for ( int ii = 0; ii < uvs.Length; ii++ )
		{
			uvs3[ ii ][ 0 ] = uvs[ ii ][ 0 ];
			uvs3[ ii ][ 1 ] = uvs[ ii ][ 1 ];
			uvs3[ ii ][ 2 ] = 0;
		}

		setMeshPointAttribute( 
			asset_id, object_id, geo_id, 
			HoudiniConstants.HAPI_ATTRIB_UV, 3, uvs3,
			setting_raw_mesh, false, part_info, part_control );

		// Add and set additional attributes.
		if ( attribute_manager )
		{
			if ( setting_raw_mesh )
			{
				foreach ( HoudiniGeoAttribute attribute in attribute_manager.prAttributes )
				{
					HAPI_AttributeInfo attr_info = attribute.prAttributeInfo;

					// TODO: Hack! We want to create a point attribute when marshalling input geos
					// in but the attribute manager only really works with vertices. However,
					// when marshalling geometry in, each vertex has its own point, so we just
					// "pretend" we are dealing with a point attribute.
					attr_info.owner = HAPI_AttributeOwner.HAPI_ATTROWNER_POINT;

					HoudiniHost.addAttribute( asset_id, object_id, geo_id, attribute.prName, ref attr_info );

					if ( attribute.prType == HoudiniGeoAttribute.Type.BOOL ||
						attribute.prType == HoudiniGeoAttribute.Type.INT )
					{
						int[] int_data = attribute.prIntData;
						setAttribute(
							asset_id, object_id, geo_id, attribute.prName, 
							ref attr_info, ref int_data, HoudiniHost.setAttributeIntData );

						if ( HoudiniHost.prCreateGroupsFromBoolAttributes
							&& attribute.prType == HoudiniGeoAttribute.Type.BOOL
							&& attribute.prTupleSize == 1 )
						{
							HoudiniHost.addGroup(
								asset_id, object_id, geo_id, HAPI_GroupType.HAPI_GROUPTYPE_POINT,
								attribute.prName );
							bool[] bool_data = new bool[ int_data.Length ];
							for ( int i = 0; i < int_data.Length; ++i )
								bool_data[ i ] = int_data[ i ] > 0;
							HoudiniHost.setGroupMembership(
								asset_id, object_id, geo_id, HAPI_GroupType.HAPI_GROUPTYPE_POINT,
								attribute.prName, bool_data, bool_data.Length );
						}
					}
					else if ( attribute.prType == HoudiniGeoAttribute.Type.FLOAT )
					{
						float[] float_data = attribute.prFloatData;
						setAttribute(
							asset_id, object_id, geo_id, attribute.prName, 
							ref attr_info, ref float_data, HoudiniHost.setAttributeFloatData );
					}
					else if ( attribute.prType == HoudiniGeoAttribute.Type.STRING )
					{
						string[] string_data = attribute.prStringData;
						setAttribute(
							asset_id, object_id, geo_id, attribute.prName, 
							ref attr_info, ref string_data, HoudiniHost.setAttributeStringData );
					}
				}
			}
			else
			{
				foreach ( HoudiniGeoAttribute attribute in attribute_manager.prAttributes )
				{
					HAPI_AttributeInfo attr_info = attribute.prAttributeInfo;
					attr_info.owner = attribute.prOriginalAttributeOwner;
					if ( attr_info.owner == HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
						attr_info.count = part_info.pointCount;
					HoudiniHost.addAttribute( asset_id, object_id, geo_id, attribute.prName, ref attr_info );

					if ( attribute.prType == HoudiniGeoAttribute.Type.BOOL ||
						attribute.prType == HoudiniGeoAttribute.Type.INT )
					{
						int[] int_data =
							attribute.getIntPointValues( part_info.pointCount, vertex_list );
						setAttribute(
							asset_id, object_id, geo_id, attribute.prName, 
							ref attr_info, ref int_data, HoudiniHost.setAttributeIntData );
					}
					else if ( attribute.prType == HoudiniGeoAttribute.Type.FLOAT )
					{
						float[] float_data =
							attribute.getFloatPointValues( part_info.pointCount, vertex_list );
						setAttribute(
							asset_id, object_id, geo_id, attribute.prName, 
							ref attr_info, ref float_data, HoudiniHost.setAttributeFloatData );
					}
					else if ( attribute.prType == HoudiniGeoAttribute.Type.STRING )
					{
						string[] string_data = attribute.prStringData;
						setAttribute(
							asset_id, object_id, geo_id, attribute.prName, 
							ref attr_info, ref string_data, HoudiniHost.setAttributeStringData );
					}
				}
			}
		}

		HoudiniHost.commitGeo( asset_id, object_id, geo_id );
	}
	
	// ANIMATION KEYS -----------------------------------------------------------------------------------------------
	
	public static void addKeyToCurve( float time, float val, AnimationCurve curve )
	{
		Keyframe curr_key = new Keyframe( time, val, 0, 0 );
		
		if ( curve.length > 0 )
		{
			Keyframe prev_key		= curve.keys[ curve.length - 1 ];
			float tangent			= ( val - prev_key.value ) / ( time - prev_key.time );
			prev_key.outTangent		= tangent;
			curr_key.inTangent		= tangent;
			
			curve.RemoveKey( curve.length - 1 );
			curve.AddKey( prev_key );
		}
		
		curve.AddKey( curr_key );
	}
	
	// MISC ---------------------------------------------------------------------------------------------------------
	
	public static void destroyGameObject( GameObject obj )
	{
		// Added to ensure shared mesh is deleted.
		if ( obj.GetComponent< MeshFilter >() != null ) {
			Mesh m = obj.GetComponent< MeshFilter >().sharedMesh;
			// Destroy it, but not if it is an asset.
			if ( m != null ) 
				GameObject.DestroyImmediate( m, false );
		}

		// Ensure sharedmaterials are deleted.
		if ( obj.GetComponent< Renderer >() != null ) {
			Material[] ms = obj.GetComponent< Renderer >().sharedMaterials;
			// It's a copied array, but it still refers to each the sharedMaterials.
			foreach ( Material m in ms ) {
				// Destroy it, but not if it is an asset.
				if ( m != null ) 
					GameObject.DestroyImmediate( m, false );
			}
		}

		GameObject.DestroyImmediate( obj, true );
	}

	public static void attachScript( GameObject obj, string attach_script )
	{
		JSONObject json_object = new JSONObject( attach_script );
		Dictionary< string, string > dictionary = json_object.ToDictionary();
		
		if ( !dictionary.ContainsKey( "script" ) )
		{
			Debug.LogError( "Script key not found in scripts attribute!" );
			return;
		}

		if ( (dictionary.Count - 1) % 3 != 0 )
		{
			Debug.LogError("Improper number of entries in scripts attribute!");
			return;
		}

		Component comp = obj.AddComponent( getTypeByName( dictionary[ "script" ] ) );
		if ( comp == null )
		{
			Debug.LogError( "Unable to attach component " + dictionary[ "script" ] );
			return;
		}
		
		int num_args = ( dictionary.Count - 1 ) / 3;
		
		for ( int ii = 0; ii < num_args; ii++ )
		{
			string arg_name_str = "arg" + ii + "Name";
			string arg_type_str = "arg" + ii + "Type";
			string arg_value_str = "arg" + ii + "Value";
			
			if( !dictionary.ContainsKey( arg_name_str ) ||
				!dictionary.ContainsKey( arg_type_str ) ||
				!dictionary.ContainsKey( arg_value_str ) )
			{
				Debug.LogError("Unable to find expected information " 
								+ arg_name_str + "||" 
								+ arg_type_str + "||"
								+ arg_value_str );
				return;
			}
			
			string arg_name = dictionary[ arg_name_str ];
			string arg_type = dictionary[ arg_type_str ];
			string arg_value = dictionary[ arg_value_str ];
			
			try
			{
				System.Reflection.FieldInfo fi = comp.GetType().GetField( arg_name );
				switch( arg_type )
				{
					case "int":
						{
							int val = (int) int.Parse( arg_value );
							fi.SetValue( comp, val );
						}					
						break;
					case "float":
						{
							float val = (float) float.Parse( arg_value );
							fi.SetValue( comp, val );
						}				
						break;
					case "string":
						{
							fi.SetValue( comp, arg_value );
						}				
						break;
					case "bool":
						{
							bool val = (bool) bool.Parse( arg_value );
							fi.SetValue( comp, val );
						}
						break;					
					case "Vector3":
						{
							char [] separators = {',','(',')' } ;
							string[] values = arg_value.Split( separators );
							
							int num_values_found = 0;
							Vector3 vec3 = new Vector3( 0, 0, 0 );
							foreach( string val in values )
							{
								if( val.Length > 0 )
								{
									vec3[ num_values_found ] = float.Parse( val );
									num_values_found++;
								}							
							}
							if( num_values_found != 3 )
							{
								Debug.LogError("Bad formatting found in Vector3: " + arg_value );
								break;
							}

							fi.SetValue( comp, vec3 );
						}
						break;
					default:
						{
							Debug.LogError("Unknown Type: " + arg_type + " Found in scripts attribute");
						}
						break;
				}
			}
			catch
			{
				Debug.LogError("Unable to set property " + arg_name_str );
				return;
			}
		}
	}

	public static void loadHipFile( string file_path )
	{
		try
		{
			if ( file_path.Length <= 0 )
				return;
			
			HoudiniHost.loadHip( file_path );
			
			HoudiniProgressBar progressBar = new HoudiniProgressBar();
			progressBar.statusCheckLoop();
			
			int[] asset_ids = HoudiniHost.getNewAssetIds();
			
			foreach ( int asset_id in asset_ids )
			{
				// Create game object.
				GameObject game_object = new GameObject( "Houdini Asset" );
				
				// Add HAPI Object Control script component.
				game_object.AddComponent< HoudiniAssetOTL >();
				HoudiniAssetOTL asset = game_object.GetComponent< HoudiniAssetOTL >();
				
				asset.prAssetType = HoudiniAsset.AssetType.TYPE_HIP;
				// Set that asset path.
				asset.prAssetPath = file_path;
				
				asset.prAssetId = asset_id;
				
				bool build_result = asset.buildAll();
				if ( build_result == false ) // Something is not right. Clean up.
				{
					Object.DestroyImmediate( game_object );
					return;
				}
				
				// Set new object name from asset name.
				string asset_name		= asset.prAssetInfo.name;
				game_object.name 		= asset_name;
			}
		}
		catch ( HoudiniError error )
		{
			Debug.LogError( error.ToString() );
		}
	}
	
	public static void checkForNewAssets()
	{
		try
		{			
			int[] asset_ids = HoudiniHost.getNewAssetIds();
			if( asset_ids.Length == 0 )
				return;

			foreach ( int asset_id in asset_ids )
			{
				// Create game object.
				GameObject game_object = new GameObject( "Generated Houdini Asset" );
				
				// Add HAPI Object Control script component.
				game_object.AddComponent< HoudiniAssetOTL >();
				HoudiniAssetOTL asset = game_object.GetComponent< HoudiniAssetOTL >();
				
				asset.prAssetType = HoudiniAsset.AssetType.TYPE_HIP;
				// Set that asset path.
				asset.prAssetPath = "";
				
				asset.prAssetId = asset_id;
				HoudiniHost.cookAsset(
					asset_id, HoudiniHost.prSplitGeosByGroup, HoudiniHost.prImportTemplatedGeos );
				
				bool build_result = asset.buildAll();
				if ( build_result == false ) // Something is not right. Clean up.
				{
					Object.DestroyImmediate( game_object );
					return;
				}
				
				// Set new object name from asset name.
				string asset_name		= asset.prAssetInfo.name;
				game_object.name 		= asset_name;

			}
		}
		catch ( HoudiniError error )
		{
			Debug.LogError( error.ToString() );
		}
	}
}
