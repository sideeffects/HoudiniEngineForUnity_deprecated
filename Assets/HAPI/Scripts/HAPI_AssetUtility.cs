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
using UnityEditor;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using HAPI;

public class HAPI_AssetUtility
{
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
		var x = Mathf.Sqrt( m.m00 * m.m00 + m.m01 * m.m01 + m.m02 * m.m02 );
		var y = Mathf.Sqrt( m.m10 * m.m10 + m.m11 * m.m11 + m.m12 * m.m12 );
		var z = Mathf.Sqrt( m.m20 * m.m20 + m.m21 * m.m21 + m.m22 * m.m22 );

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

		transform.rotationOrder		= (int) HAPI_XYZOrder.ZXY;
		transform.rstOrder			= (int) HAPI_RSTOrder.SRT;

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
				
		transform.localPosition = new Vector3( -hapi_transform.position[ 0 ], 
												hapi_transform.position[ 1 ],
												hapi_transform.position[ 2 ] );

		Quaternion quat = Quaternion.Euler( new Vector3(  hapi_transform.rotationEuler[ 0 ],
														 -hapi_transform.rotationEuler[ 1 ],
														 -hapi_transform.rotationEuler[ 2 ] ) );

		transform.localRotation = quat;
		transform.localScale = new Vector3( hapi_transform.scale[ 0 ], 
											hapi_transform.scale[ 1 ], 
											hapi_transform.scale[ 2 ] );
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

	public static void calculateMeshTangents( Mesh mesh )
	{
		// Speed up math by copying the mesh arrays.
		int[] triangles		= mesh.triangles;
		Vector3[] vertices	= mesh.vertices;
		Vector2[] uv		= mesh.uv;
		Vector3[] normals	= mesh.normals;
	 
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
			//float r = 1.0f / (s1 * t2 - s2 * t1);      //Above code fixes div by zero.
	 
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
	
	public delegate void getArray1IdDel< T >( int id1, [Out] T[] data, int start, int end );
	public delegate void getArray2IdDel< T >( int id1, int id2, [Out] T[] data, int start, int end );
	public delegate void getArray3IdDel< T >( int id1, int id2, int id3, [Out] T[] data, int start, int end );
	public delegate void getArray4IdDel< T >( int id1, int id2, int id3, int id4, [Out] T[] data, int start, int end );
	public delegate void getArray5IdDel< T >( int id1, int id2, int id3, int id4, int id5, 
											  [Out] T[] data, int start, int end );

	public static void getArray1Id< T >( int id1, getArray1IdDel< T > func, [Out] T[] data, 
										 int count )
	{
		getArray( id1, 0, 0, 0, 0, func, null, null, null, null, data, count, 1 );
	}
	public static void getArray1Id< T >( int id1, getArray1IdDel< T > func, [Out] T[] data, 
										 int count, int tuple_size )
	{
		getArray( id1, 0, 0, 0, 0, func, null, null, null, null, data, count, tuple_size );
	}
	
	public static void getArray2Id< T >( int id1, int id2, getArray2IdDel< T > func, [Out] T[] data, 
										 int count )
	{
		getArray( id1, id2, 0, 0, 0, null, func, null, null, null, data, count, 1 );
	}
	public static void getArray2Id< T >( int id1, int id2, getArray2IdDel< T > func, [Out] T[] data, 
										 int count, int tuple_size )
	{
		getArray( id1, id2, 0, 0, 0, null, func, null, null, null, data, count, tuple_size );
	}
	
	public static void getArray3Id< T >( int id1, int id2, int id3, getArray3IdDel< T > func, [Out] T[] data, 
										 int count )
	{
		getArray( id1, id2, id3, 0, 0, null, null, func, null, null, data, count, 1 );
	}
	public static void getArray3Id< T >( int id1, int id2, int id3, getArray3IdDel< T > func, [Out] T[] data, 
										 int count, int tuple_size )
	{
		getArray( id1, id2, id3, 0, 0, null, null, func, null, null, data, count, tuple_size );
	}
	
	public static void getArray4Id< T >( int id1, int id2, int id3, int id4, getArray4IdDel< T > func, [Out] T[] data, 
										 int count )
	{
		getArray( id1, id2, id3, id4, 0, null, null, null, func, null, data, count, 1 );
	}
	public static void getArray4Id< T >( int id1, int id2, int id3, int id4, getArray4IdDel< T > func, [Out] T[] data, 
										 int count, int tuple_size )
	{
		getArray( id1, id2, id3, id4, 0, null, null, null, func, null, data, count, tuple_size );
	}

	public static void getArray5Id< T >( int id1, int id2, int id3, int id4, int id5, 
										 getArray5IdDel<T> func, [Out] T [] data, int count )
	{
		getArray( id1, id2, id3, id4, id5, null, null, null, null, func, data, count, 1 );
	}
	public static void getArray5Id< T >( int id1, int id2, int id3, int id4, int id5, 
										 getArray5IdDel<T> func, [Out] T [] data, int count, int tuple_size )
	{
		getArray( id1, id2, id3, id4, id5, null, null, null, null, func, data, count, tuple_size );
	}
	
	private static void getArray< T >( int id1, int id2, int id3, int id4, int id5,
									   getArray1IdDel< T > func_1id, 
									   getArray2IdDel< T > func_2id, 
									   getArray3IdDel< T > func_3id,
									   getArray4IdDel< T > func_4id,
									   getArray5IdDel< T > func_5id,
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
		
	public delegate void getAttrArrayInputFunc< T >( int asset_id, int object_id, int geo_id, int part_id, 
													 string name, ref HAPI_AttributeInfo info, 
													 [Out] T[] items, int start, int end );
	public static void getAttrArray< T >( int asset_id, int object_id, int geo_id, int part_id, 
										  string name, ref HAPI_AttributeInfo info, T[] items,
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
	
	public static void getAttribute< T >( int asset_id, int object_id, int geo_id, int part_id, string name, 
										  ref HAPI_AttributeInfo info, ref T[] data,
										  getAttrArrayInputFunc< T > get_func )
	{
		int original_tuple_size = info.tupleSize;		
		
		for ( int type = 0; type < (int) HAPI_AttributeOwner.HAPI_ATTROWNER_MAX; ++type )
		{
			info.owner = type;
			HAPI_Host.getAttributeInfo( asset_id, object_id, geo_id, part_id, name, ref info );
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
	
	public static string[] getAttributeNames( int asset_id, int object_id, int geo_id, int part_id, 
											  HAPI_PartInfo part_info, HAPI_AttributeOwner owner )
	{
		int attr_count = part_info.getOwnerCount( owner );
			
		string[] names = new string[ attr_count ];
		
		int[] attr_names = new int[ attr_count ]; // string handles (SH)
		
		HAPI_Host.getAttributeNames( asset_id, object_id, geo_id, part_id, (int) owner, attr_names, attr_count );
		for ( int ii = 0; ii < attr_count; ++ii )
			names[ ii ] = HAPI_Host.getString( attr_names[ ii ] );
		
		return names;
	}
	
	public static void printAttributeNames( int asset_id, int object_id, int geo_id, int part_id, 
											HAPI_PartInfo part_info, HAPI_AttributeOwner owner )
	{
		string[] names = getAttributeNames( asset_id, object_id, geo_id, part_id, part_info, owner );
		
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
	
	public static void printAllAttributeNames( int asset_id, int object_id, int geo_id, int part_id, 
											   HAPI_PartInfo part_info )
	{
		for ( int owner = 0; owner < (int) HAPI_AttributeOwner.HAPI_ATTROWNER_MAX; ++owner )
			printAttributeNames( asset_id, object_id, geo_id, part_id, part_info, (HAPI_AttributeOwner) owner );
	}

	// PARAMETERS ---------------------------------------------------------------------------------------------------
	
	public static int findParm( ref HAPI_ParmInfo[] parms, string name )
	{
		if ( parms == null )
			return -1;
		
		for ( int i = 0; i < parms.Length; ++i )
		{
			string current_parm_name = parms[ i ].name;
			if ( parms[ i ].isChildOfMultiParm )
				current_parm_name = current_parm_name.Replace( "#", parms[ i ].instanceNum.ToString() );
			
			if ( current_parm_name == name )
				return parms[ i ].id;
		}
		return -1;
	}

	public static float getParmFloatValue( int node_id, string parm_name, float default_value )
	{
		HAPI_NodeInfo node_info	= HAPI_Host.getNodeInfo( node_id );

		// Get all parameters.
		HAPI_ParmInfo[] parms = new HAPI_ParmInfo[ node_info.parmCount ];
		getArray1Id( node_id, HAPI_Host.getParameters, parms, node_info.parmCount );

		int parm_id = findParm( ref parms, parm_name );
		if ( parm_id < 0 )
			return default_value;

		int values_index = parms[ parm_id ].floatValuesIndex;
		float[] value = new float[ 1 ];

		HAPI_Host.getParmFloatValues( node_id, value, values_index, 1 );

		return value[ 0 ];
	}

	public static Color getParmColour3Value( int node_id, string parm_name, Color default_value )
	{
		HAPI_NodeInfo node_info	= HAPI_Host.getNodeInfo( node_id );

		// Get all parameters.
		HAPI_ParmInfo[] parms = new HAPI_ParmInfo[ node_info.parmCount ];
		getArray1Id( node_id, HAPI_Host.getParameters, parms, node_info.parmCount );

		int parm_id = findParm( ref parms, parm_name );
		if ( parm_id < 0 )
			return default_value;

		if ( parms[ parm_id ].size < 3 )
			Debug.LogError( "Parm size not large enough to be a color3." );

		int values_index = parms[ parm_id ].floatValuesIndex;
		float[] values = new float[ 3 ];

		HAPI_Host.getParmFloatValues( node_id, values, values_index, 3 );

		return new Color( values[ 0 ], values[ 1 ], values[ 2 ], 1.0f );
	}

	public static Color getParmColour4Value( int node_id, string parm_name, Color default_value )
	{
		HAPI_NodeInfo node_info	= HAPI_Host.getNodeInfo( node_id );

		// Get all parameters.
		HAPI_ParmInfo[] parms = new HAPI_ParmInfo[ node_info.parmCount ];
		getArray1Id( node_id, HAPI_Host.getParameters, parms, node_info.parmCount );

		int parm_id = findParm( ref parms, parm_name );
		if ( parm_id < 0 )
			return default_value;

		if ( parms[ parm_id ].size < 4 )
			Debug.LogError( "Parm size not large enough to be a color4." );

		int values_index = parms[ parm_id ].floatValuesIndex;
		float[] values = new float[ 4 ];

		HAPI_Host.getParmFloatValues( node_id, values, values_index, 4 );

		return new Color( values[ 0 ], values[ 1 ], values[ 2 ], values[ 3 ] );
	}

	public static void setRenderResolution( Vector2 resolution )
	{
		HAPI_GlobalNodes global_nodes = HAPI_Host.getGlobalNodes();

		HAPI_NodeInfo node_info	= HAPI_Host.getNodeInfo( global_nodes.defaultCamera );

		// Get all parameters.
		HAPI_ParmInfo[] parms = new HAPI_ParmInfo[ node_info.parmCount ];
		getArray1Id( global_nodes.defaultCamera, HAPI_Host.getParameters, parms, node_info.parmCount );

		int parm_id = findParm( ref parms, "res" );
		if ( parm_id < 0 )
			return;

		int values_index = parms[ parm_id ].intValuesIndex;
		int[] values = new int[ 2 ];
		values[ 0 ] = (int) resolution.x;
		values[ 1 ] = (int) resolution.y;

		HAPI_Host.setParmIntValues( global_nodes.defaultCamera, values, values_index, 2 );
	}

	// TEXTURES -----------------------------------------------------------------------------------------------------
	
	public static void reApplyMaterials( HAPI_Asset asset )
	{
		foreach ( HAPI_PartControl part_control in asset.GetComponentsInChildren< HAPI_PartControl >() )
		{
			try
			{
				assignMaterial( part_control, asset, true );
			}
			catch ( HAPI_Error error )
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

	public static void assignMaterial( HAPI_PartControl part_control, HAPI_Asset asset, 
									   bool update_houdini_material )
	{
		GameObject part_node = part_control.gameObject;
		HAPI_PartInfo part_info = new HAPI_PartInfo();
		HAPI_Host.getPartInfo( asset.prAssetId, part_control.prObjectId, part_control.prGeoId, 
							   part_control.prPartId, out part_info );
		bool is_mesh = ( part_info.vertexCount > 0 );

		if ( part_control.prPartName != HAPI_Host.prCollisionGroupName && is_mesh )
		{
			part_control.prMaterialId = part_info.materialId;
		
			MeshRenderer mesh_renderer = part_node.GetComponent< MeshRenderer >();
			if ( !mesh_renderer )
				throw new HAPI_Error( "No mesh renderer!" );

			mesh_renderer.enabled = 
			part_control.prObjectVisible && 
				( asset.prIsGeoVisible || part_control.prGeoType == HAPI_GeoType.HAPI_GEOTYPE_EXPOSED_EDIT );

			if ( !assignUnityMaterial( part_control, part_node, mesh_renderer ) )
			{
				if ( mesh_renderer.sharedMaterial == null )
					mesh_renderer.sharedMaterial = new Material( Shader.Find( "HAPI/SpecularVertexColor" ) );

				if ( ( update_houdini_material || mesh_renderer.sharedMaterial.mainTexture == null ) 
						&& part_control.prMaterialId >= 0 )
				{
					HAPI_MaterialInfo material_info = HAPI_Host.getMaterial( asset.prAssetId, part_info.materialId );

					// Assign vertex color shader if the flag says so.
					if ( asset.prShowOnlyVertexColours )
					{
						mesh_renderer.sharedMaterial.shader = Shader.Find( "HAPI/SpecularVertexColor" );
					}
					else
					{
						// Assign the transparency shader if this material is transparent or unassign it otherwise.
						if ( isMaterialTransparent( material_info ) )
							mesh_renderer.sharedMaterial.shader = Shader.Find( "HAPI/AlphaSpecularVertexColor" );
						else
							mesh_renderer.sharedMaterial.shader = Shader.Find( "HAPI/SpecularVertexColor" );

						setRenderResolution( asset.prRenderResolution );

						Material material = mesh_renderer.sharedMaterial;
						string folder_path = HAPI_Constants.HAPI_TEXTURES_PATH + "/" + 
											 part_control.prAsset.prAssetName;
						assignHoudiniMaterial( ref material, material_info, folder_path, 
											   asset.prMaterialShaderType );
					}
				}
			}
		}
	}

	public static Texture2D extractHoudiniImageToTexture( 
		HAPI_MaterialInfo material_info, string folder_path, string image_planes )
	{
		Texture2D result = null;
		try
		{
			if ( HAPI_Host.prDontCreateTextureFiles )
			{
				// Make sure the image format selected is supported by Unity's in-memory texture loading.
				HAPI_ImageInfo image_info = HAPI_Host.getImageInfo( material_info.assetId, material_info.id );
				if ( image_info.fileFormat != HAPI_ImageFileFormat.HAPI_IMAGE_FILE_FORMAT_PNG &&
					 image_info.fileFormat != HAPI_ImageFileFormat.HAPI_IMAGE_FILE_FORMAT_JPG )
				{
					image_info.fileFormat = HAPI_ImageFileFormat.HAPI_IMAGE_FILE_FORMAT_DEFAULT;
					HAPI_Host.setImageInfo( material_info.assetId, material_info.id, image_info );
				}

				// Extract image to memory.
				byte[] image_data = HAPI_Host.extractImageToMemory( 
					material_info.assetId, material_info.id, image_planes );

				// Initial size doesn't matter as LoadImage() will change the size and format.
				Texture2D tex = new Texture2D( 1, 1 );
				tex.LoadImage( image_data );

				result = tex;
			}
			else // Figure out the source file path and name.
			{
				// Navigate to the Assets/Textures directory and create it if it doesn't exist.
				DirectoryInfo textures_dir = new DirectoryInfo( folder_path );
				if ( !textures_dir.Exists )
					textures_dir.Create();

				// Make sure the image format selected is supported by Unity.
				HAPI_ImageInfo image_info = HAPI_Host.getImageInfo( material_info.assetId, material_info.id );
				if ( image_info.fileFormat != HAPI_ImageFileFormat.HAPI_IMAGE_FILE_FORMAT_PNG &&
					 image_info.fileFormat != HAPI_ImageFileFormat.HAPI_IMAGE_FILE_FORMAT_JPG &&
					 image_info.fileFormat != HAPI_ImageFileFormat.HAPI_IMAGE_FILE_FORMAT_BMP &&
					 image_info.fileFormat != HAPI_ImageFileFormat.HAPI_IMAGE_FILE_FORMAT_TGA )
				{
					image_info.fileFormat = HAPI_ImageFileFormat.HAPI_IMAGE_FILE_FORMAT_DEFAULT;
					HAPI_Host.setImageInfo( material_info.assetId, material_info.id, image_info );
				}

				// Extract image to file.
				string texture_file_path = HAPI_Host.extractImageToFile(
					material_info.assetId, material_info.id, image_planes, folder_path );

				string relative_file_path = texture_file_path.Replace(
					Application.dataPath, "Assets" );

				// Load the texture and assign it to the material. Note that LoadAssetAtPath only 
				// understands paths relative to the project folder.
				Object tex_obj = AssetDatabase.LoadAssetAtPath( relative_file_path, typeof( Texture2D ) );
				if ( tex_obj == null || !AssetDatabase.Contains( tex_obj ) )
				{
					// Asset has not been imported yet so import and try again.
					AssetDatabase.ImportAsset( relative_file_path, ImportAssetOptions.Default );
					tex_obj = AssetDatabase.LoadAssetAtPath( relative_file_path, typeof( Texture2D ) );
				}

				// Assign main texture.
				result = (Texture2D) tex_obj;
			}
		}
		catch ( HAPI_Error )
		{
			result = null;
		}

		return result;
	}

	public static void assignHoudiniMaterial( 
		ref Material material, HAPI_MaterialInfo material_info, string folder_path, HAPI_ShaderType shader_type )
	{
		// Reset textures.
		material.mainTexture = null;
		material.SetTexture( "_NormalMap", null );

		// Get all parameters.
		HAPI_NodeInfo node_info	= HAPI_Host.getNodeInfo( material_info.nodeId );
		HAPI_ParmInfo[] parms = new HAPI_ParmInfo[ node_info.parmCount ];
		getArray1Id( material_info.nodeId, HAPI_Host.getParameters, parms, node_info.parmCount );

		if ( shader_type == HAPI_ShaderType.HAPI_SHADER_OPENGL )
		{
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
					HAPI_Host.renderTextureToImage( 
						material_info.assetId, material_info.id, diffuse_map_parm_id );

					material.mainTexture = extractHoudiniImageToTexture( material_info, folder_path, "C A" );
					material.SetTexture( 
						"_NormalMap", extractHoudiniImageToTexture( material_info, folder_path, "N" ) );
				}
				catch ( HAPI_Error ) {}
			}
			
			// Assign shader properties.

			material.SetFloat( "_Shininess", 1.0f - getParmFloatValue( material_info.nodeId, "ogl_rough", 0.0f ) );

			Color diffuse_colour = getParmColour3Value( material_info.nodeId, "ogl_diff", Color.white );
			diffuse_colour.a = getParmFloatValue( material_info.nodeId, "ogl_alpha", 1.0f );
			material.SetColor( "_Color", diffuse_colour );

			material.SetColor( "_SpecColor", getParmColour3Value( material_info.nodeId, "ogl_spec", Color.black ) );

		}
		else if ( shader_type == HAPI_ShaderType.HAPI_SHADER_MANTRA )
		{
			// Render the material to image.
			HAPI_ProgressBar progress_bar = new HAPI_ProgressBar();
			progress_bar.prTitle		= "Rendering Material using Houdini Mantra";
			progress_bar.prMessage		= "Rendering...";
			progress_bar.prStartTime	= System.DateTime.Now;
			progress_bar.prUseDelay		= false;
			progress_bar.displayProgressBar();
			try
			{
				HAPI_Host.renderMaterialToImage( 
					material_info.assetId, material_info.id, HAPI_ShaderType.HAPI_SHADER_MANTRA );

				// Extract and assign textures.
				material.mainTexture = extractHoudiniImageToTexture( material_info, folder_path, "C A" ); 
				material.SetTexture( "_NormalMap", extractHoudiniImageToTexture( material_info, folder_path, "N" ) );
			}
			catch ( HAPI_Error ) {}
			progress_bar.clearProgressBar();

			// Assign shader properties.

			material.SetFloat( "_Shininess", 1.0f - getParmFloatValue( material_info.nodeId, "ogl_rough", 0.0f ) );

			Color diffuse_colour = getParmColour3Value( material_info.nodeId, "ogl_diff", Color.white );
			diffuse_colour.a = getParmFloatValue( material_info.nodeId, "ogl_alpha", 1.0f );
			material.SetColor( "_Color", diffuse_colour );

			material.SetColor( "_SpecColor", getParmColour3Value( material_info.nodeId, "ogl_spec", Color.black ) );
		}
		
		// Refresh all assets just in case.
		AssetDatabase.Refresh();
	}

	public static bool assignUnityMaterial( HAPI_PartControl part_control, GameObject part_node,
											MeshRenderer mesh_renderer )
	{
		// Get position attributes.
		int asset_id	= part_control.prAssetId;
		int object_id	= part_control.prObjectId;
		int geo_id		= part_control.prGeoId;
		int part_id		= part_control.prPartId;

		HAPI_AttributeInfo material_attr_info = new HAPI_AttributeInfo( HAPI_Host.prUnityMaterialAttribName );
		int[] material_attr = new int[ 0 ];
		getAttribute( asset_id, object_id, geo_id, part_id, HAPI_Host.prUnityMaterialAttribName, 
					  ref material_attr_info, ref material_attr, HAPI_Host.getAttributeStrData );

		// Need to get the material path here because the next call to HAPI_Host.getAttributeStrData will
		// overwrite the string ids in material_attr.
		string material_path = material_attr_info.exists ? HAPI_Host.getString( material_attr[ 0 ] )
														 : "";

		HAPI_AttributeInfo sub_material_name_attr_info = new HAPI_AttributeInfo( 
																HAPI_Host.prUnitySubMaterialNameAttribName );
		int[] sub_material_name_attr = new int[ 0 ];
		getAttribute( asset_id, object_id, geo_id, part_id, HAPI_Host.prUnitySubMaterialNameAttribName, 
					  ref sub_material_name_attr_info, ref sub_material_name_attr, HAPI_Host.getAttributeStrData );

		HAPI_AttributeInfo sub_material_index_attr_info = new HAPI_AttributeInfo( 
																HAPI_Host.prUnitySubMaterialIndexAttribName );
		int[] sub_material_index_attr = new int[ 0 ];
		getAttribute( asset_id, object_id, geo_id, part_id, HAPI_Host.prUnitySubMaterialIndexAttribName, 
					  ref sub_material_index_attr_info, ref sub_material_index_attr, HAPI_Host.getAttributeIntData );

		bool has_sub_material_name		= sub_material_name_attr_info.exists && 
										  HAPI_Host.getString( sub_material_name_attr[ 0 ] ) != "";
		bool has_sub_material_index		= sub_material_index_attr_info.exists;

		if ( material_attr_info.exists )
		{
			string sub_material_name	= has_sub_material_name ? HAPI_Host.getString( sub_material_name_attr[ 0 ] ) 
																: "";
			int sub_material_index		= has_sub_material_index ? sub_material_index_attr[ 0 ] : 0;
			Material material			= (Material) Resources.Load( material_path, typeof( Material ) );

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
					Debug.LogWarning( "sub_material_index (" + sub_material_index + ") out of range for " +
									  "material: " + abs_path );
			}

			mesh_renderer.sharedMaterial = material;

			return true;
		}
		else
			return false;
	}
	
	// GEOMETRY MARSHALLING -----------------------------------------------------------------------------------------

	public static void getMesh( HAPI_PartControl part_control, Mesh mesh, bool generate_tangents )
	{
		int asset_id	= part_control.prAssetId;
		int object_id	= part_control.prObjectId;
		int geo_id		= part_control.prGeoId;
		int part_id		= part_control.prPartId;

		// Get Detail info.
		HAPI_PartInfo part_info = new HAPI_PartInfo();
		HAPI_Host.getPartInfo( asset_id, object_id, geo_id, part_id, out part_info );
		
		// Make sure our primitive and vertex numbers are supported by Unity.
		// TODO: add this limit in a more proper place
		if ( part_info.faceCount > 65000 * 3 )
			throw new HAPI_Error( "Face count (" + part_info.faceCount 
								  + ") above limit (" + ( 65000 * 3 ) + ")!" );
		if ( part_info.vertexCount > 65000 )
			throw new HAPI_Error( "Vertex count (" + part_info.vertexCount + ") above limit (" + 65000 + ")!" );
		
		// Get Face counts.
		int[] face_counts = new int[ part_info.faceCount ];
		getArray4Id( asset_id, object_id, geo_id, part_id, HAPI_Host.getFaceCounts, 
					 face_counts, part_info.faceCount );
		
		// Get Vertex list.
		int[] vertex_list = new int[ part_info.vertexCount ];
		getArray4Id( asset_id, object_id, geo_id, part_id, HAPI_Host.getVertexList, 
					 vertex_list, part_info.vertexCount );
		
		// Get position attributes.
		HAPI_AttributeInfo pos_attr_info = new HAPI_AttributeInfo( HAPI_Constants.HAPI_ATTRIB_POSITION );
		float[] pos_attr = new float[ 0 ];
		getAttribute( asset_id, object_id, geo_id, part_id, HAPI_Constants.HAPI_ATTRIB_POSITION, 
					  ref pos_attr_info, ref pos_attr, HAPI_Host.getAttributeFloatData );
		if ( !pos_attr_info.exists )
			throw new HAPI_Error( "No position attribute found." );
		else if ( pos_attr_info.owner != (int) HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
			throw new HAPI_ErrorIgnorable( "I only understand position as point attributes!" );
				
		// Get uv attributes.
		HAPI_AttributeInfo uv_attr_info = new HAPI_AttributeInfo( HAPI_Constants.HAPI_ATTRIB_UV );
		uv_attr_info.tupleSize = 2;
		float[] uv_attr = new float[ 0 ];
		getAttribute( asset_id, object_id, geo_id, part_id, HAPI_Constants.HAPI_ATTRIB_UV, 
					  ref uv_attr_info, ref uv_attr, HAPI_Host.getAttributeFloatData );
		
		// Get normal attributes.
		HAPI_AttributeInfo normal_attr_info = new HAPI_AttributeInfo( HAPI_Constants.HAPI_ATTRIB_NORMAL );
		float[] normal_attr = new float[ 0 ];
		getAttribute( asset_id, object_id, geo_id, part_id, HAPI_Constants.HAPI_ATTRIB_NORMAL, 
					  ref normal_attr_info, ref normal_attr, HAPI_Host.getAttributeFloatData );

		// Get colour attributes.
		HAPI_AttributeInfo colour_attr_info = new HAPI_AttributeInfo( HAPI_Constants.HAPI_ATTRIB_COLOUR );
		float[] colour_attr = new float[ 0 ];
		getAttribute( asset_id, object_id, geo_id, part_id, HAPI_Constants.HAPI_ATTRIB_COLOUR, 
					  ref colour_attr_info, ref colour_attr, HAPI_Host.getAttributeFloatData );

		// Get tangent attributes.
		HAPI_AttributeInfo tangent_attr_info = new HAPI_AttributeInfo( HAPI_Constants.HAPI_ATTRIB_TANGENT );
		float[] tangent_attr = new float[ 0 ];
		getAttribute( asset_id, object_id, geo_id, part_id, HAPI_Constants.HAPI_ATTRIB_TANGENT, 
					  ref tangent_attr_info, ref tangent_attr, HAPI_Host.getAttributeFloatData );
		
		// Save properties.
		part_control.prVertexList			= vertex_list;
		
		// Create Unity-specific data objects.
		Vector3[] vertices 	= new Vector3[ 	part_info.vertexCount ];
		int[] triangles 	= new int[ 		part_info.faceCount * 3 ];
		Vector2[] uvs 		= new Vector2[ 	part_info.vertexCount ];
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
						int face_index = i / HAPI_Constants.HAPI_MAX_VERTICES_PER_FACE;
						normals[ i ][ j ] 
							= normal_attr[ face_index * HAPI_Constants.HAPI_MAX_VERTICES_PER_FACE + j ];
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
				if ( tangent_attr_info.owner == (int) HAPI_AttributeOwner.HAPI_ATTROWNER_VERTEX )
					for ( int j = 0; j < tuple_size; ++j )
					{
						tangents[ i ][ j ] = tangent_attr[ i * tuple_size + j ];
						// Flip the x coordinate.
						if ( j == 0 )
							tangents[ i ][ j ] *= -1;
						
					}
				
				// If the tangent are per point use the vertex list array point indicies to query into
				// the tangent array we filled above.
				else if ( tangent_attr_info.owner == (int) HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
					for ( int j = 0; j < tuple_size; ++j )
					{
						tangents[ i ][ j ] = tangent_attr[ vertex_list[ i ] * tuple_size + j ];
						// Flip the x coordinate.
						if ( j == 0 )
							tangents[ i ][ j ] *= -1;
					}
				
				// If the tangents are per face divide the vertex index by the number of vertices per face
				// which should always be HAPI_MAX_VERTICES_PER_FACE.
				else if ( tangent_attr_info.owner == (int) HAPI_AttributeOwner.HAPI_ATTROWNER_PRIM )
					for ( int j = 0; j < tuple_size; ++j )
					{
						int face_index = i / HAPI_Constants.HAPI_MAX_VERTICES_PER_FACE;
						tangents[ i ][ j ] 
							= tangent_attr[ face_index * HAPI_Constants.HAPI_MAX_VERTICES_PER_FACE * tuple_size + j ];
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
			if ( colour_attr_info.exists )
			{
				// If the colours are per vertex just query directly into the normals array we filled above.
				if ( colour_attr_info.owner == (int) HAPI_AttributeOwner.HAPI_ATTROWNER_VERTEX )
					for ( int j = 0; j < 3; ++j )
						colours[ i ][ j ] = colour_attr[ i * 3 + j ];
				
				// If the colours are per point use the vertex list array point indicies to query into
				// the normal array we filled above.
				else if ( colour_attr_info.owner == (int) HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
					for ( int j = 0; j < 3; ++j )
						colours[ i ][ j ] = colour_attr[ vertex_list[ i ] * 3 + j ];
				
				// If the colours are per face divide the vertex index by the number of vertices per face
				// which should always be HAPI_MAX_VERTICES_PER_FACE.
				else if ( colour_attr_info.owner == (int) HAPI_AttributeOwner.HAPI_ATTROWNER_PRIM )
				{
					int face_index = i / HAPI_Constants.HAPI_MAX_VERTICES_PER_FACE;
					for ( int j = 0; j < 3; ++j )
						colours[ i ][ j ] 
							= colour_attr[ face_index * HAPI_Constants.HAPI_MAX_VERTICES_PER_FACE + j ];
				}
			}
		}
		
		for ( int i = 0; i < part_info.faceCount; ++i ) 
			for ( int j = 0; j < 3; ++j )
				triangles[ i * 3 + j ] = i * 3 + j;
		
		// Load into vertices and face into mesh.
		mesh.vertices 	= vertices;
		mesh.triangles 	= triangles;
		mesh.uv 		= uvs;
		mesh.normals 	= normals;
		mesh.colors		= colours;
		if ( generate_tangents )
			mesh.tangents	= tangents;
		
		mesh.RecalculateBounds();
		
		if ( !normal_attr_info.exists )
			mesh.RecalculateNormals();

		if ( generate_tangents && !tangent_attr_info.exists )
			calculateMeshTangents( mesh );
	}
	
	public static void setMesh( int asset_id, int object_id, int geo_id, ref Mesh mesh, 
								HAPI_PartControl part_control )
	{
		bool setting_raw_mesh = ( part_control == null );

		Vector3[] vertices 				= mesh.vertices;
		int[] triangles 				= mesh.triangles;
		//Vector2[] uvs 				= mesh.uv;
		//Vector3[] normals 			= mesh.normals;
		
		HAPI_GeoInfo geo_info 			= new HAPI_GeoInfo();
		geo_info.id 					= geo_id;

		HAPI_PartInfo part_info			= new HAPI_PartInfo();
		part_info.materialId 			= -1;

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
		
		/* Not yet supported.
		if ( uvs != null )
			part_info.vertexAttributeCount++;
		if ( normals != null )
			part_info.vertexAttributeCount++;
		*/
		
		HAPI_Host.setGeoInfo( asset_id, object_id, geo_id, ref geo_info );
		HAPI_Host.setPartInfo( asset_id, object_id, geo_id, ref part_info );
		
		// Set Face counts.
		int[] face_counts = new int[ part_info.faceCount ];
		for ( int i = 0; i < part_info.faceCount; ++i )
			face_counts[ i ] = 3;
		setArray3Id( asset_id, object_id, geo_id, HAPI_Host.setFaceCounts, face_counts, part_info.faceCount );
		
		// Set Vertex list.
		int[] vertex_list = new int[ part_info.vertexCount ];
		if ( setting_raw_mesh )
			for ( int i = 0; i < part_info.faceCount; ++i )
				for ( int j = 0; j < 3; ++j )
					vertex_list[ i * 3 + j ] = triangles[ i * 3 + j ];
		else
			vertex_list = part_control.prVertexList;
		setArray3Id( asset_id, object_id, geo_id, HAPI_Host.setVertexList, vertex_list, part_info.vertexCount );
		
		// Set position attributes.
		HAPI_AttributeInfo pos_attr_info = new HAPI_AttributeInfo( HAPI_Constants.HAPI_ATTRIB_POSITION );
		pos_attr_info.exists 		= true;
		pos_attr_info.owner 		= (int) HAPI.HAPI_AttributeOwner.HAPI_ATTROWNER_POINT;
		pos_attr_info.storage 		= (int) HAPI.HAPI_StorageType.HAPI_STORAGETYPE_FLOAT;
		pos_attr_info.count 		= part_info.pointCount;
		pos_attr_info.tupleSize 	= 3;
		HAPI_Host.addAttribute( asset_id, object_id, geo_id, 
								HAPI_Constants.HAPI_ATTRIB_POSITION, ref pos_attr_info );
		
		float[] pos_attr = new float[ part_info.pointCount * 3 ];
		
		if ( setting_raw_mesh )
		{
			for ( int i = 0; i < part_info.pointCount; ++i )
				for ( int j = 0; j < 3; ++j )
				{
					if ( j != 0 )
						pos_attr[ i * 3 + j ] = vertices[ i ][ j ];
					else
						pos_attr[ i * 3 + j ] = -vertices[ i ][ j ];
				}
		}
		else
		{
			for ( int ii = 0; ii < part_control.prVertexList.Length; ii++ )
			{
				int point_index = part_control.prVertexList[ ii ] * 3;

				pos_attr[ point_index ] = -vertices[ ii ][ 0 ];
				pos_attr[ point_index + 1 ] = vertices[ ii ][ 1 ];
				pos_attr[ point_index + 2 ] = vertices[ ii ][ 2 ];
			}
		}
		
		setAttribute( asset_id, object_id, geo_id, HAPI_Constants.HAPI_ATTRIB_POSITION, 
					  ref pos_attr_info, ref pos_attr, HAPI_Host.setAttributeFloatData );
		
		HAPI_Host.commitGeo( asset_id, object_id, geo_id );
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
	
	
	// MISC --------- -----------------------------------------------------------------------------------------------
	public static void attachScript( GameObject obj, string attach_script )
	{
		JSONObject json_object = new JSONObject( attach_script );
		Dictionary< string, string > dictionary = json_object.ToDictionary();
		
		if( !dictionary.ContainsKey("script") )
		{
			Debug.LogError("script key not found in scripts attribute!");
			return;
		}
											
		if( (dictionary.Count - 1) % 3 != 0 )
		{
			Debug.LogError("Improper number of entries in scripts attribute!");
			return;
		}
					
		Component comp = obj.AddComponent( dictionary["script"] );
		if( comp == null )
		{
			Debug.LogError("Unable to attach component " + dictionary["script"] );
			return;
		}
		
		int num_args = (dictionary.Count - 1) / 3;
		
		for( int ii = 0; ii < num_args; ii++ )
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
}
