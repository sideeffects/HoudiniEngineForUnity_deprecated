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
	// TRANSFORMS ------------------------------------------------------------------------------------------------------

	public static Quaternion getQuaternion( Matrix4x4 m )
	{
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
			if ( parms[ i ].name == name )
				return i;
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

	// TEXTURES -----------------------------------------------------------------------------------------------------
	
	public static void reApplyMaterials( HAPI_Asset asset )
	{
		foreach ( MeshRenderer renderer in asset.GetComponentsInChildren< MeshRenderer >() )
		{
			// Set material.
			if ( renderer.sharedMaterial == null )
				renderer.sharedMaterial = new Material( Shader.Find( "HAPI/SpecularVertexColor" ) );

			if ( asset.prShowOnlyVertexColours )
			{
				renderer.sharedMaterial.mainTexture = null;
				renderer.sharedMaterial.shader = Shader.Find( "HAPI/SpecularVertexColor" );
			}
			else
			{
				Transform parent = renderer.transform;
				HAPI_PartControl part_control = parent.GetComponent< HAPI_PartControl >();
						
				if ( part_control.prMaterialId >= 0 )
				{
					try
					{
						HAPI_MaterialInfo material_info = HAPI_Host.getMaterial( asset.prAssetId, 
																				 part_control.prMaterialId );
								
						bool is_transparent = HAPI_AssetUtility.isMaterialTransparent( material_info );
						if ( is_transparent )
							renderer.sharedMaterial.shader = Shader.Find( "HAPI/AlphaSpecularVertexColor" );
						else
							renderer.sharedMaterial.shader = Shader.Find( "HAPI/SpecularVertexColor" );
								
						Material material  = renderer.sharedMaterial;
						string folder_path = HAPI_Constants.HAPI_TEXTURES_PATH + "/" + 
											 part_control.prAsset.prAssetName;
						HAPI_AssetUtility.assignMaterial( ref material, material_info, folder_path, 
														  asset.prMaterialShaderType );
					}
					catch ( HAPI_Error error )
					{
						Debug.LogError( error.ToString() );
					}
				}
				else
					renderer.sharedMaterial.shader = Shader.Find( "HAPI/SpecularVertexColor" );
			}
		}
	}

	public static bool isMaterialTransparent( HAPI_MaterialInfo material_info )
	{
		float alpha = getParmFloatValue( material_info.nodeId, "ogl_alpha", 1.0f );

		return ( alpha < 0.95f );
	}

	public static void assignMaterial( ref Material material, HAPI_MaterialInfo material_info,
									   string folder_path, HAPI_ShaderType shader_type )
	{
		// Navigate to the Assets/Textures directory and create it if it doesn't exist.
		DirectoryInfo textures_dir = new DirectoryInfo( folder_path );
		if ( !textures_dir.Exists )
			textures_dir.Create();

		// Get all parameters.
		HAPI_NodeInfo node_info	= HAPI_Host.getNodeInfo( material_info.nodeId );
		HAPI_ParmInfo[] parms = new HAPI_ParmInfo[ node_info.parmCount ];
		getArray1Id( material_info.nodeId, HAPI_Host.getParameters, parms, node_info.parmCount );

		string relative_file_path = "";

		if ( shader_type == HAPI_ShaderType.HAPI_SHADER_OPENGL )
		{
			// Extract diffuse map file from material.
			int diffuse_map_parm_id = findParm( ref parms, "baseColorMap" );
			if ( diffuse_map_parm_id < 0 )
				diffuse_map_parm_id = findParm( ref parms, "map" );
			if ( diffuse_map_parm_id >= 0 )
			{
				// Figure out the source file path and name.
				try
				{
					string texture_file_path= HAPI_Host.extractTextureToFile( material_info.nodeId, 
																			  diffuse_map_parm_id, 
																			  folder_path );

					relative_file_path 		= texture_file_path.Replace(	  Application.dataPath, 
																			  "Assets" );
		
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
					material.mainTexture = (Texture2D) tex_obj;
				}
				catch ( HAPI_ErrorInvalidArgument )
				{
					// No diffuse map.
				}

				// Assign shader properties.

				material.SetFloat( "_Shininess", 
									1.0f - getParmFloatValue( material_info.nodeId, "ogl_rough", 0.0f ) );
	
				Color diffuse_colour	= getParmColour3Value( material_info.nodeId, "ogl_diff", Color.white );
				diffuse_colour.a		= getParmFloatValue( material_info.nodeId, "ogl_alpha", 1.0f );
				material.SetColor( "_Color", diffuse_colour );
	
				material.SetColor( "_SpecColor", 
									getParmColour3Value( material_info.nodeId, "ogl_spec", Color.black ) );
			}
		}
		else if ( shader_type == HAPI_ShaderType.HAPI_SHADER_MANTRA )
		{
			HAPI_ProgressBar progress_bar = new HAPI_ProgressBar();
			progress_bar.prTitle		= "Rendering Material using Houdini Mantra";
			progress_bar.prMessage		= "Rendering...";
			progress_bar.prStartTime	= System.DateTime.Now;
			progress_bar.prUseDelay		= false;
			progress_bar.displayProgressBar();
			string texture_file_path	= HAPI_Host.renderMaterialToFile( material_info.assetId, material_info.id, 
																		  HAPI_ShaderType.HAPI_SHADER_MANTRA,
																		  folder_path );
			progress_bar.clearProgressBar();

			relative_file_path 			= texture_file_path.Replace(	  Application.dataPath, 
																		  "Assets" );

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

			material.SetFloat( "_Shininess", 
							   1.0f - getParmFloatValue( material_info.nodeId, "ogl_rough", 0.0f ) );

			Color diffuse_colour	= getParmColour3Value( material_info.nodeId, "ogl_diff", Color.white );
			diffuse_colour.a		= getParmFloatValue( material_info.nodeId, "ogl_alpha", 1.0f );
			material.SetColor( "_Color", diffuse_colour );

			material.SetColor( "_SpecColor", 
							   getParmColour3Value( material_info.nodeId, "ogl_spec", Color.black ) );
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

		if ( material_attr_info.exists )
		{
			string material_path = HAPI_Host.getString( material_attr[ 0 ] );
			Material material = (Material) Resources.Load( material_path, typeof( Material ) );

			if ( material == null )
			{
				AssetDatabase.ImportAsset( material_path, ImportAssetOptions.Default );
				material = (Material) AssetDatabase.LoadAssetAtPath( material_path, typeof( Material ) );
			}
			mesh_renderer.sharedMaterial = material;

			return true;
		}
		else
			return false;
	}
	
	// GEOMETRY MARSHALLING -----------------------------------------------------------------------------------------
	
	public static void getMesh( HAPI_PartControl part_control, Mesh mesh )
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
		HAPI_AttributeInfo pos_attr_info = new HAPI_AttributeInfo( "P" );
		float[] pos_attr = new float[ 0 ];
		getAttribute( asset_id, object_id, geo_id, part_id, "P", ref pos_attr_info, ref pos_attr, 
					  HAPI_Host.getAttributeFloatData );
		if ( !pos_attr_info.exists )
			throw new HAPI_Error( "No position attribute found." );
		else if ( pos_attr_info.owner != (int) HAPI_AttributeOwner.HAPI_ATTROWNER_POINT )
			throw new HAPI_ErrorIgnorable( "I only understand position as point attributes!" );
				
		// Get uv attributes.
		HAPI_AttributeInfo uv_attr_info = new HAPI_AttributeInfo( "uv" );
		uv_attr_info.tupleSize = 2;
		float[] uv_attr = new float[ 0 ];
		getAttribute( asset_id, object_id, geo_id, part_id, "uv", ref uv_attr_info, ref uv_attr, 
					  HAPI_Host.getAttributeFloatData );
		
		// Get normal attributes.
		HAPI_AttributeInfo normal_attr_info = new HAPI_AttributeInfo( "N" );
		float[] normal_attr = new float[ 0 ];
		getAttribute( asset_id, object_id, geo_id, part_id, "N", ref normal_attr_info, ref normal_attr, 
					  HAPI_Host.getAttributeFloatData );

		// Get colour attributes.
		HAPI_AttributeInfo colour_attr_info = new HAPI_AttributeInfo( "Cd" );
		float[] colour_attr = new float[ 0 ];
		getAttribute( asset_id, object_id, geo_id, part_id, "Cd", ref colour_attr_info, ref colour_attr, 
					  HAPI_Host.getAttributeFloatData );
		
		// Save properties.
		part_control.prVertexList			= vertex_list;
		
		// Create Unity-specific data objects.
		Vector3[] vertices 	= new Vector3[ 	part_info.vertexCount ];
		int[] triangles 	= new int[ 		part_info.faceCount * 3 ];
		Vector2[] uvs 		= new Vector2[ 	part_info.vertexCount ];
		Vector3[] normals 	= new Vector3[ 	part_info.vertexCount ];
		Color[] colours		= new Color[	part_info.vertexCount ];
		
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
				triangles[ i * 3 + j ] 	= i * 3 + j;
		
		// Load into vertices and face into mesh.
		mesh.vertices 	= vertices;
		mesh.triangles 	= triangles;
		mesh.uv 		= uvs;
		mesh.normals 	= normals;
		mesh.colors		= colours;
		
		mesh.RecalculateBounds();
		
		if ( !normal_attr_info.exists )
			mesh.RecalculateNormals();
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
		HAPI_AttributeInfo pos_attr_info = new HAPI_AttributeInfo( "P" );
		pos_attr_info.exists 		= true;
		pos_attr_info.owner 		= (int) HAPI.HAPI_AttributeOwner.HAPI_ATTROWNER_POINT;
		pos_attr_info.storage 		= (int) HAPI.HAPI_StorageType.HAPI_STORAGETYPE_FLOAT;
		pos_attr_info.count 		= part_info.pointCount;
		pos_attr_info.tupleSize 	= 3;
		HAPI_Host.addAttribute( asset_id, object_id, geo_id, "P", ref pos_attr_info );
		
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
		
		setAttribute( asset_id, object_id, geo_id, "P", ref pos_attr_info, ref pos_attr, 
					  HAPI_Host.setAttributeFloatData );
		
		HAPI_Host.commitGeo( asset_id, object_id, geo_id );
	}
	
	// GEOMETRY MARSHALLING -----------------------------------------------------------------------------------------
	public static void addKeyToCurve( float time, float val, AnimationCurve curve )
	{
		Keyframe curr_key = new Keyframe( time, val, 0, 0 );
		
		if( curve.length > 0 )
		{
			Keyframe prev_key = curve.keys[ curve.length - 1 ];
			float tangent = (val - prev_key.value) / (time - prev_key.time );
			prev_key.outTangent = tangent;
			curr_key.inTangent = tangent;
			
			curve.RemoveKey( curve.length - 1 );
			curve.AddKey( prev_key );			
		}
		
		curve.AddKey( curr_key );
		
	}
	
}
