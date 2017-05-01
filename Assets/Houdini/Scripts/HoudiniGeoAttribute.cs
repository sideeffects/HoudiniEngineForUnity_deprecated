/*
* Copyright (c) <2017> Side Effects Software Inc.
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*
* Produced by:
*      Side Effects Software Inc
*      123 Front Street West, Suite 1401
*      Toronto, Ontario
*      Canada   M5J 2M2
*      416-504-9876
*
*/

using UnityEngine;
using System.Collections;

public class HoudiniGeoAttribute : ScriptableObject
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Enums

	public enum Type
	{
		UNDEFINED = -1,
		BOOL,
		INT,
		FLOAT,
		STRING,
		MAX
	}

	public enum Preset
	{
		COLOR,
		UV,
		NORMAL,
		BOOL,
		INT,
		FLOAT,
		STRING,
		MAX
	}

	public enum SpecialPaintMode
	{
		COLOUR,
		MAX
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties

	public string prName
	{
		get
		{
			return myName;
		}
		set
		{
			myName = value;
		}
	}
	public Type prType
	{
		get
		{
			return myType;
		}
		set
		{
			Type old_type = myType;
			Type new_type = value;

			if ( old_type == Type.UNDEFINED )
				return;

			if ( old_type == new_type || new_type <= Type.UNDEFINED || new_type >= Type.MAX )
				return;

			if ( old_type == Type.BOOL || old_type == Type.INT )
			{
				if ( new_type == Type.FLOAT )
				{
					myFloatPaintValue = new float[ myTupleSize ];
					for ( int i = 0; i < myTupleSize; ++i )
					{
						myFloatPaintValue[ i ] = (float) myIntPaintValue[ i ];
						myFloatMin = (float) myIntMin;
						myFloatMax = (float) myIntMax;
					}
					myIntPaintValue = null;

					myFloatData = new float[ myIntData.Length ];
					for ( int i = 0; i < myIntData.Length; ++i )
						myFloatData[ i ] = (float) myIntData[ i ];
					myIntData = null;
				}
				else if ( new_type == Type.STRING )
				{
					myStringPaintValue = new string[ myTupleSize ];
					for ( int i = 0; i < myTupleSize; ++i )
						myStringPaintValue[ i ] = myIntPaintValue[ i ].ToString();

					myStringData = new string[ myIntData.Length ];
					for ( int i = 0; i < myIntData.Length; ++i )
						myStringData[ i ] = myIntData[ i ].ToString();
					myIntData = null;
				}
			}
			else if ( old_type == Type.FLOAT )
			{
				if ( new_type == Type.BOOL || new_type == Type.INT )
				{
					myIntPaintValue = new int[ myTupleSize ];
					for ( int i = 0; i < myTupleSize; ++i )
						if ( new_type == Type.BOOL )
						{
							myIntPaintValue[ i ] = (int) myFloatPaintValue[ i ] > 0 ? 1 : 0;
							myIntMin = 0;
							myIntMax = 1;
						}
						else
						{
							myIntPaintValue[ i ] = (int) myFloatPaintValue[ i ];
							myIntMin = (int) myFloatMin;
							myIntMax = (int) myFloatMax;
						}
					myFloatPaintValue = null;

					myIntData = new int[ myFloatData.Length ];
					for ( int i = 0; i < myFloatData.Length; ++i )
						if ( new_type == Type.BOOL )
							myIntData[ i ] = (int) myFloatData[ i ] > 0 ? 1 : 0;
						else
							myIntData[ i ] = (int) myFloatData[ i ];
					myFloatData = null;
				}
				else if ( new_type == Type.STRING )
				{
					myStringPaintValue = new string[ myTupleSize ];
					for ( int i = 0; i < myTupleSize; ++i )
						myStringPaintValue[ i ] = myFloatPaintValue[ i ].ToString();

					myStringData = new string[ myFloatData.Length ];
					for ( int i = 0; i < myFloatData.Length; ++i )
						myStringData[ i ] = myFloatData[ i ].ToString();
					myFloatData = null;
				}
			}
			else if ( old_type == Type.STRING )
			{
				if ( new_type == Type.BOOL || new_type == Type.INT )
				{
					myIntPaintValue = new int[ myTupleSize ];
					myIntMin = myDefaultIntMin;
					myIntMax = myDefaultIntMax;
					for ( int i = 0; i < myTupleSize; ++i )
					{
						int new_value;
						if ( !int.TryParse( myStringPaintValue[ i ], out new_value ) )
							new_value = myDefaultIntPaintValue;

						if ( new_type == Type.BOOL )
						{
							myIntPaintValue[ i ] = new_value > 0 ? 1 : 0;
							myIntMin = 0;
							myIntMax = 1;
						}
						else
							myIntPaintValue[ i ] = new_value;
					}
					myStringPaintValue = null;

					myIntData = new int[ myStringData.Length ];
					for ( int i = 0; i < myStringData.Length; ++i )
					{
						int new_value;
						if ( !int.TryParse( myStringData[ i ], out new_value ) )
							new_value = 0;

						if ( new_type == Type.BOOL )
							myIntData[ i ] = new_value > 0 ? 1 : 0;
						else
							myIntData[ i ] = new_value;
					}
					myStringData = null;
				}
				else if ( new_type == Type.FLOAT )
				{
					myFloatPaintValue = new float[ myTupleSize ];
					myFloatMin = myDefaultFloatMin;
					myFloatMax = myDefaultFloatMax;
					for ( int i = 0; i < myTupleSize; ++i )
					{
						float new_value;
						if ( !float.TryParse( myStringPaintValue[ i ], out new_value ) )
							new_value = myDefaultIntPaintValue;

						myFloatPaintValue[ i ] = new_value;
					}
					myStringPaintValue = null;

					myFloatData = new float[ myStringData.Length ];
					for ( int i = 0; i < myStringData.Length; ++i )
					{
						float new_value;
						if ( !float.TryParse( myStringData[ i ], out new_value ) )
							new_value = 0;

						myFloatData[ i ] = new_value;
					}
					myStringData = null;
				}
			}

			// Set the new type.
			myType = new_type;
		}
	}
	public HAPI_StorageType prStorageType
	{
		get
		{
			switch ( myType )
			{
				case Type.BOOL: return HAPI_StorageType.HAPI_STORAGETYPE_INT;
				case Type.INT: return HAPI_StorageType.HAPI_STORAGETYPE_INT;
				case Type.FLOAT: return HAPI_StorageType.HAPI_STORAGETYPE_FLOAT;
				case Type.STRING: return HAPI_StorageType.HAPI_STORAGETYPE_STRING;
				default: return HAPI_StorageType.HAPI_STORAGETYPE_INVALID;
			}
		}
		set
		{
			switch ( value )
			{
				case HAPI_StorageType.HAPI_STORAGETYPE_INT:
				{
					// We don't want to turn a bool type into an int type
					// just because HAPI doesn't recognize bool types.
					if ( myType == Type.BOOL || myType == Type.INT )
						return;
					else
						prType = Type.INT;
					break;
				}
				case HAPI_StorageType.HAPI_STORAGETYPE_FLOAT: prType = Type.FLOAT; break;
				case HAPI_StorageType.HAPI_STORAGETYPE_STRING: prType = Type.STRING; break;
				default: break;
			}
		}
	}
	public HAPI_AttributeOwner prOriginalAttributeOwner
	{
		get { return myOriginalAttributeOwner; }
		set { myOriginalAttributeOwner = value; }
	}
	public int prTupleSize
	{
		get
		{
			return myTupleSize;
		}
		set
		{
			if ( myType == Type.UNDEFINED )
				return;

			if ( myTupleSize == value || value < 1 || value > 5 )
				return;

			int new_tuple_size = value;

			if ( myType == Type.BOOL || myType == Type.INT )
			{
				int[] new_paint_value = new int[ new_tuple_size ];
				int[] new_data = new int[ myVertexCount * new_tuple_size ];

				int min_tuple_size = Mathf.Min( myTupleSize, new_tuple_size );
				for ( int i = 0; i < min_tuple_size; ++i )
					new_paint_value[ i ] = myIntPaintValue[ i ];
				for ( int i = min_tuple_size; i < new_tuple_size; ++i )
					if ( myType == Type.BOOL )
						new_paint_value[ i ] = 1;
					else if ( myType == Type.INT )
						new_paint_value[ i ] = myDefaultIntPaintValue;

				for ( int i = 0; i < myVertexCount; ++i )
				{
					for ( int j = 0; j < min_tuple_size; ++j )
						new_data[ i * new_tuple_size + j ] = myIntData[ i * myTupleSize + j ];
					for ( int j = min_tuple_size; j < new_tuple_size; ++j )
						new_data[ i * new_tuple_size + j ] = new_paint_value[ j ];
				}

				myIntPaintValue = new_paint_value;
				myIntData = new_data;
				myTupleSize = new_tuple_size;
				myPaintMode = Mathf.Min( myPaintMode, (int) SpecialPaintMode.MAX + new_tuple_size - 1 );
			}
			else if ( myType == Type.FLOAT )
			{
				float[] new_paint_value = new float[ new_tuple_size ];
				float[] new_data = new float[ myVertexCount * new_tuple_size ];

				int min_tuple_size = Mathf.Min( myTupleSize, new_tuple_size );
				for ( int i = 0; i < min_tuple_size; ++i )
					new_paint_value[ i ] = myFloatPaintValue[ i ];
				for ( int i = min_tuple_size; i < new_tuple_size; ++i )
					new_paint_value[ i ] = myDefaultFloatPaintValue;

				for ( int i = 0; i < myVertexCount; ++i )
				{
					for ( int j = 0; j < min_tuple_size; ++j )
						new_data[ i * new_tuple_size + j ] = myFloatData[ i * myTupleSize + j ];
					for ( int j = min_tuple_size; j < new_tuple_size; ++j )
						new_data[ i * new_tuple_size + j ] = new_paint_value[ j ];
				}

				myFloatPaintValue = new_paint_value;
				myFloatData = new_data;
				myTupleSize = new_tuple_size;
				myPaintMode = Mathf.Min( myPaintMode, (int) SpecialPaintMode.MAX + new_tuple_size - 1 );
			}
			else if ( myType == Type.STRING )
			{
				string[] new_paint_value = new string[ new_tuple_size ];
				string[] new_data = new string[ myVertexCount * new_tuple_size ];

				int min_tuple_size = Mathf.Min( myTupleSize, new_tuple_size );
				for ( int i = 0; i < min_tuple_size; ++i )
					new_paint_value[ i ] = myStringPaintValue[ i ];
				for ( int i = min_tuple_size; i < new_tuple_size; ++i )
					new_paint_value[ i ] = myDefaultStringPaintValue;

				for ( int i = 0; i < myVertexCount; ++i )
				{
					for ( int j = 0; j < min_tuple_size; ++j )
						new_data[ i * new_tuple_size + j ] = myStringData[ i * myTupleSize + j ];
					for ( int j = min_tuple_size; j < new_tuple_size; ++j )
						new_data[ i * new_tuple_size + j ] = new_paint_value[ j ];
				}

				myStringPaintValue = new_paint_value;
				myStringData = new_data;
				myTupleSize = new_tuple_size;
				myPaintMode = Mathf.Min( myPaintMode, (int) SpecialPaintMode.MAX + new_tuple_size - 1 );
			}
		}
	}

	public int prPaintMode
	{
		get
		{
			return myPaintMode;
		}
		set
		{
			myPaintMode = Mathf.Min( value, (int) SpecialPaintMode.MAX + myTupleSize - 1 );
		}
	}

	public int[] prIntPaintValue {
		get { return myIntPaintValue; }
		set
		{
			if ( value.Length == myTupleSize )
				for ( int i = 0; i < myTupleSize; ++i )
					myIntPaintValue[ i ] = Mathf.Clamp( value[ i ], myIntMin, myIntMax );
			else
				throw new HoudiniErrorInvalidArgument( "prIntPaintValue given wrong sized array." );
		}
	}
	public int prIntMin {
		get { return myIntMin; }
		set { myIntMin = Mathf.Min( value, myIntMax ); } }
	public int prIntMax {
		get { return myIntMax; } 
		set { if ( myType != Type.BOOL ) myIntMax = Mathf.Max( value, myIntMin ); } }
	public int[] prIntData { get { return myIntData; } private set {} }

	public float[] prFloatPaintValue {
		get { return myFloatPaintValue; }
		set
		{
			if ( value.Length == myTupleSize )
				for ( int i = 0; i < myTupleSize; ++i )
					myFloatPaintValue[ i ] = Mathf.Clamp( value[ i ], myFloatMin, myFloatMax );
			else
				throw new HoudiniErrorInvalidArgument( "prFloatPaintValue given wrong sized array." );
		}
	}
	public float prFloatMin {
		get { return myFloatMin; }
		set { myFloatMin = Mathf.Min( value, myFloatMax ); } }
	public float prFloatMax {
		get { return myFloatMax; }
		set { myFloatMax = Mathf.Max( value, myFloatMin ); } }
	public float[] prFloatData { get { return myFloatData; } private set {} }
	public Vector2[] prFloatDataVec2 {
		get
		{
			Vector2[] vec2_data = new Vector2[ myVertexCount ];
			for ( int i = 0; i < myVertexCount; ++i )
				for ( int j = 0; j < Mathf.Min( 2, myTupleSize ); ++j )
					vec2_data[ i ][ j ] = myFloatData[ i * myTupleSize + j ];
			return vec2_data;
		}
		private set {}
	}
	public Vector3[] prFloatDataVec3 {
		get
		{
			Vector3[] vec3_data = new Vector3[ myVertexCount ];
			for ( int i = 0; i < myVertexCount; ++i )
				for ( int j = 0; j < Mathf.Min( 3, myTupleSize ); ++j )
					vec3_data[ i ][ j ] = myFloatData[ i * myTupleSize + j ];
			return vec3_data;
		}
		private set {}
	}
	public string[] prStringPaintValue { get { return myStringPaintValue; } set { myStringPaintValue = value; } }
	public string[] prStringData { get { return myStringData; } private set {} }

	public HAPI_AttributeInfo prAttributeInfo
	{
		get
		{
			if ( myType == Type.UNDEFINED )
				throw new HoudiniErrorInvalidArgument( "Geo attribute not defined." );

			HAPI_AttributeInfo attr_info = new HAPI_AttributeInfo( prName );
			attr_info.exists = true;
			attr_info.owner = HAPI_AttributeOwner.HAPI_ATTROWNER_VERTEX;
			attr_info.storage = prStorageType;
			attr_info.count = myVertexCount;
			attr_info.tupleSize = myTupleSize;
			return attr_info;
		}
		private set {}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods

	public HoudiniGeoAttribute()
	{
		reset();
	}

	public void reset()
	{
		myName = "NO_NAME";

		myType = Type.UNDEFINED;
		myOriginalAttributeOwner = HAPI_AttributeOwner.HAPI_ATTROWNER_VERTEX;
		myTupleSize = 1;
		myVertexCount = 0;
		myInitializedVertexCount = 0;

		myPaintMode = (int) SpecialPaintMode.COLOUR;

		myIntPaintValue = null;
		myIntMin = myDefaultIntMin;
		myIntMax = myDefaultIntMax;
		myIntData = null;

		myFloatPaintValue = null;
		myFloatMin = myDefaultFloatMin;
		myFloatMax = myDefaultFloatMax;
		myFloatData = null;

		myStringPaintValue = null;
		myStringData = null;
	}

	public void init( Mesh mesh, Preset preset )
	{
		switch ( preset )
		{
			case Preset.COLOR:
			{
				init(
					mesh, HoudiniConstants.HAPI_ATTRIB_COLOR, Type.FLOAT,
					HoudiniConstants.HAPI_COLOR_VECTOR_SIZE );

				// Set the alpha values to 1.
				if ( HoudiniConstants.HAPI_COLOR_VECTOR_SIZE == 4 )
				{
					myFloatPaintValue[ HoudiniConstants.HAPI_COLOR_VECTOR_SIZE - 1 ] = 1.0f;
					for ( int i = 0; i < myVertexCount; ++i )
						myFloatData[
							( i * HoudiniConstants.HAPI_COLOR_VECTOR_SIZE ) + 
							( HoudiniConstants.HAPI_COLOR_VECTOR_SIZE - 1 ) ] = 1.0f;
				}
				break;
			}
			case Preset.UV:
			{
				init(
					mesh, HoudiniConstants.HAPI_ATTRIB_UV, Type.FLOAT,
					HoudiniConstants.HAPI_UV_VECTOR_SIZE );
				break;
			}
			case Preset.NORMAL:
			{
				init(
					mesh, HoudiniConstants.HAPI_ATTRIB_NORMAL, Type.FLOAT,
					HoudiniConstants.HAPI_NORMAL_VECTOR_SIZE );

				// Compute the normals as initial attribute values.
				mesh.RecalculateNormals();
				for ( int i = 0; i < myVertexCount; ++i )
					for ( int j = 0; j < HoudiniConstants.HAPI_NORMAL_VECTOR_SIZE; ++j )
						myFloatData[ i * HoudiniConstants.HAPI_NORMAL_VECTOR_SIZE + j ] =
							mesh.normals[ i ][ j ];
				break;
			}
			case Preset.BOOL:
			{
				init( mesh, "bool_attribute", Type.BOOL, 1 );
				break;
			}
			case Preset.INT:
			{
				init( mesh, "int_attribute", Type.INT, 1 );
				break;
			}
			case Preset.FLOAT:
			{
				init( mesh, "float_attribute", Type.FLOAT, 3 );
				break;
			}
			case Preset.STRING:
			{
				init( mesh, "string_attribute", Type.STRING, 1 );
				break;
			}
			default: throw new HoudiniErrorInvalidArgument( "Invalid HAPI_GeoAttribute.Preset!" );
		}
	}

	public void init( Mesh mesh, string name, Type type, int tuple_size )
	{
		if ( tuple_size <= 0 )
			return; // Throw error.

		if ( type <= Type.UNDEFINED || type >= Type.MAX )
			return; // Throw error.

		reset();

		myName = name;
		myType = type;
		myTupleSize = tuple_size;
		myVertexCount = mesh.vertexCount;

		// Set the paint mode to be color of it makes sense given the tuple size.
		// Otherwise, just set it to the first component.
		if ( myTupleSize != 3 && myTupleSize != 4 )
			myPaintMode = (int) SpecialPaintMode.MAX;

		int data_size = mesh.vertexCount * tuple_size;

		if ( type == Type.BOOL || type == Type.INT )
		{
			myIntPaintValue = new int[ tuple_size ];
			myIntMin = myDefaultIntMin;
			myIntMax = myDefaultIntMax;
			myIntData = new int[ data_size ];

			if ( type == Type.BOOL )
			{
				// These are hard coded because...well, BOOLs.
				myIntMin = 0;
				myIntMax = 1;

				for ( int i = 0; i < tuple_size; ++i )
					myIntPaintValue[ i ] = 1;
				for ( int i = 0; i < data_size; ++i )
					myIntData[ i ] = myIntMin;
			}
			else if ( type == Type.INT )
			{
				for ( int i = 0; i < tuple_size; ++i )
					myIntPaintValue[ i ] = myDefaultIntPaintValue;
				for ( int i = 0; i < data_size; ++i )
					myIntData[ i ] = myDefaultIntMin;
			}
		}
		else if ( type == Type.FLOAT )
		{
			myFloatPaintValue = new float[ tuple_size ];
			myFloatMin = myDefaultFloatMin;
			myFloatMax = myDefaultFloatMax;
			myFloatData = new float[ data_size ];

			for ( int i = 0; i < tuple_size; ++i )
				myFloatPaintValue[ i ] = myDefaultFloatPaintValue;
			for ( int i = 0; i < data_size; ++i )
				myFloatData[ i ] = myDefaultFloatMin;
		}
		else if ( type == Type.STRING )
		{
			myStringPaintValue = new string[ tuple_size ];
			myStringData = new string[ data_size ];

			for ( int i = 0; i < tuple_size; ++i )
				myStringPaintValue[ i ] = myDefaultStringPaintValue;
			for ( int i = 0; i < data_size; ++i )
				myStringData[ i ] = myDefaultStringPaintValue;
		}
	}

	public bool sync(
		int geo_id, int part_id,
		Mesh mesh, HAPI_AttributeInfo attribute_info )
	{
		int tuple_size = attribute_info.tupleSize;

		prStorageType = attribute_info.storage;
		prTupleSize = tuple_size;
		myVertexCount = mesh.vertexCount;

		Type type = prType;

		if ( myInitializedVertexCount != mesh.vertexCount )
		{
			int new_size = mesh.vertexCount * tuple_size;
			if ( type == Type.BOOL || type == Type.INT )
				System.Array.Resize< int >( ref myIntData, new_size );
			else if ( type == Type.FLOAT )
				System.Array.Resize< float >( ref myFloatData, new_size );
			else if ( type == Type.STRING )
				System.Array.Resize< string >( ref myStringData, new_size );
		}

		if ( myInitializedVertexCount < mesh.vertexCount )
		{
			// Get Vertex list.
			HAPI_PartInfo part_info = HoudiniHost.getPartInfo( geo_id, part_id );
			int[] vertex_list = new int[ part_info.vertexCount ];
			HoudiniAssetUtility.getArray2Id(
				geo_id, part_id, HoudiniHost.getVertexList,
				vertex_list, part_info.vertexCount );

			if ( type == Type.BOOL || type == Type.INT )
			{
				int[] data = new int[ 0 ];
				HoudiniAssetUtility.getAttribute(
					geo_id, part_id,
					myName, 
					ref attribute_info,
					ref data,
					HoudiniHost.getAttributeIntData );

				for ( int i = myInitializedVertexCount; i < part_info.vertexCount; ++i )
					for ( int tuple = 0; tuple < tuple_size; ++tuple )
						prIntData[vertex_list[ i ] * tuple_size + tuple ] =
							data[ vertex_list[ i ] * tuple_size + tuple ];
			}
			else if ( type == Type.FLOAT )
			{
				float[] data = new float[ 0 ];
				HoudiniAssetUtility.getAttribute(
					geo_id, part_id,
					myName, 
					ref attribute_info,
					ref data,
					HoudiniHost.getAttributeFloatData );

				for ( int i = myInitializedVertexCount; i < part_info.vertexCount; ++i )
					for ( int tuple = 0; tuple < tuple_size; ++tuple )
						prFloatData[ vertex_list[ i ] * tuple_size + tuple ] =
							data[ vertex_list[ i ] * tuple_size + tuple ];
			}
			else if ( type == Type.STRING )
			{
				int[] data = new int[ 0 ];
				HoudiniAssetUtility.getAttribute(
					geo_id, part_id,
					myName, 
					ref attribute_info,
					ref data,
					HoudiniHost.getAttributeStringData );

				for ( int i = myInitializedVertexCount; i < part_info.vertexCount; ++i )
					for ( int tuple = 0; tuple < tuple_size; ++tuple )
						prStringData[vertex_list[i] * tuple_size + tuple ] =
							HoudiniHost.getString( data[ vertex_list[ i ] * tuple_size + tuple ] );
			}
		}

		// If some of the data was already 
		bool needs_recook = myInitializedVertexCount > 0;

		myInitializedVertexCount = mesh.vertexCount;

		return needs_recook;
	}

	public HoudiniGeoAttribute copy()
	{
		HoudiniGeoAttribute new_attribute =
			ScriptableObject.CreateInstance< HoudiniGeoAttribute >();

		new_attribute.reset();

		new_attribute.myName = myName;

		new_attribute.myType = myType;
		new_attribute.myOriginalAttributeOwner = myOriginalAttributeOwner;
		new_attribute.myTupleSize = myTupleSize;
		new_attribute.myVertexCount = myVertexCount;
		new_attribute.myInitializedVertexCount = myInitializedVertexCount;

		new_attribute.myPaintMode = myPaintMode;

		if ( myType == Type.INT || myType == Type.BOOL )
		{
			new_attribute.myIntPaintValue = new int[ myIntPaintValue.Length ];
			myIntPaintValue.CopyTo( new_attribute.myIntPaintValue, 0 );

			new_attribute.myIntMin = myIntMin;
			new_attribute.myIntMax = myIntMax;

			new_attribute.myIntData = new int[ myIntData.Length ];
			myIntData.CopyTo( new_attribute.myIntData, 0 );
		}
		else if ( myType == Type.FLOAT )
		{
			new_attribute.myFloatPaintValue = new float[ myFloatPaintValue.Length ];
			myFloatPaintValue.CopyTo( new_attribute.myFloatPaintValue, 0 );

			new_attribute.myFloatMin = myFloatMin;
			new_attribute.myFloatMax = myFloatMax;

			new_attribute.myFloatData = new float[ myFloatData.Length ];
			myFloatData.CopyTo( new_attribute.myFloatData, 0 );
		}
		else if ( myType == Type.STRING )
		{
			new_attribute.myStringPaintValue = new string[ myStringPaintValue.Length ];
			myStringPaintValue.CopyTo( new_attribute.myStringPaintValue, 0 );

			new_attribute.myStringData = new string[ myStringData.Length ];
			myStringData.CopyTo( new_attribute.myStringData, 0 );
		}

		return new_attribute;
	}

	// -----------------------------------------------------------------------
	// Representation

	public Color[] getColorRepresentation()
	{
		if ( myType == Type.UNDEFINED )
			return null; // Throw error.

		Color[] colors = new Color[ myVertexCount ];

		if ( myPaintMode == (int) SpecialPaintMode.COLOUR )
		{
			for ( int i = 0; i < myVertexCount; ++i )
			{
				colors[ i ].r = 1.0f;
				colors[ i ].g = 1.0f;
				colors[ i ].b = 1.0f;
				colors[ i ].a = 1.0f;

				for ( int j = 0; j < Mathf.Min( 3, myTupleSize ); ++j )
					if ( myType == Type.BOOL || myType == Type.INT )
						colors[ i ][ j ] =
							Mathf.InverseLerp( 
								myIntMin, myIntMax, 
								myIntData[ i * myTupleSize + j ] );
					else if ( myType == Type.FLOAT )
						colors[ i ][ j ] =
							Mathf.InverseLerp( 
								myFloatMin, myFloatMax,
								myFloatData[ i * myTupleSize + j ] );
					else if ( myType == Type.STRING )
						colors[ i ][ j ] =
							myStringData[ i * myTupleSize + j ]
								== myStringPaintValue[ j ]
								? 1.0f : 0.0f;
			}
		}
		else
		{
			int component_index = myPaintMode - (int) SpecialPaintMode.MAX;

			for ( int i = 0; i < myVertexCount; ++i )
			{
				colors[ i ].r = 1.0f;
				colors[ i ].g = 1.0f;
				colors[ i ].b = 1.0f;
				colors[ i ].a = 1.0f;

				for ( int j = 0; j < 3; ++j )
					if ( myType == Type.BOOL || myType == Type.INT )
						colors[ i ][ j ] =
							Mathf.InverseLerp( 
								myIntMin, myIntMax,
								myIntData[ i * myTupleSize + component_index ] );
					else if ( myType == Type.FLOAT )
						colors[ i ][ j ] =
							Mathf.InverseLerp( 
								myFloatMin, myFloatMax,
								myFloatData[ i * myTupleSize + component_index ] );
					else if ( myType == Type.STRING )
						colors[ i ][ j ] =
							myStringData[ i * myTupleSize + component_index ]
								== myStringPaintValue[ component_index ]
								? 1.0f : 0.0f;
			}
		}

		return colors;
	}

	public int[] getIntPointValues( int point_count, int[] vertex_list )
	{
		int[] point_values = new int[ point_count * myTupleSize ];
		for ( int i = 0; i < vertex_list.Length; ++i )
			for ( int tuple = 0; tuple < myTupleSize; ++tuple )
				point_values[ vertex_list[ i ] * myTupleSize + tuple ] =
					prIntData[ vertex_list[ i ] * myTupleSize + tuple ];

		return point_values;
	}

	public float[] getFloatPointValues( int point_count, int[] vertex_list )
	{
		float[] point_values = new float[ point_count * myTupleSize ];
		for ( int i = 0; i < vertex_list.Length; ++i )
			for ( int tuple = 0; tuple < myTupleSize; ++tuple )
				point_values[ vertex_list[ i ] * myTupleSize + tuple ] =
					prFloatData[ vertex_list[ i ] * myTupleSize + tuple ];

		return point_values;
	}

	public void fit()
	{
		if ( myVertexCount > 0 )
			if ( myType == Type.INT )
			{
				int min = myIntData[ 0 ];
				int max = myIntData[ 0 ];
				for ( int i = 0; i < myIntData.Length; ++i )
				{
					if ( myIntData[ i ] < min )
						min = myIntData[ i ];
					if ( max < myIntData[ i ] )
						max = myIntData[ i ];
				}
				myIntMin = min;
				myIntMax = max;
			}
			else if ( myType == Type.FLOAT )
			{
				float min = myFloatData[ 0 ];
				float max = myFloatData[ 0 ];
				for ( int i = 0; i < myFloatData.Length; ++i )
				{
					if ( myFloatData[ i ] < min )
						min = myFloatData[ i ];
					if ( max < myFloatData[ i ] )
						max = myFloatData[ i ];
				}
				myFloatMin = min;
				myFloatMax = max;
			}
	}

	// -----------------------------------------------------------------------
	// Paint

	public void paint( int vertex_index, float paint_factor, bool inverse )
	{
		if ( vertex_index < 0 || vertex_index >= myVertexCount )
			return; // TODO: Throw error.

		// If we start writing to our data we assume our data is initialized.
		myInitializedVertexCount = myVertexCount;

		int start_comp_index = 0;
		int end_comp_index = myTupleSize;

		if ( myPaintMode >= (int) SpecialPaintMode.MAX )
		{
			int component_index = myPaintMode - (int) SpecialPaintMode.MAX;
			start_comp_index = component_index;
			end_comp_index = component_index + 1;
		}

		for ( int i = start_comp_index; i < end_comp_index; ++i )
			if ( myType == Type.BOOL )
				myIntData[ vertex_index * myTupleSize + i ] += (int) Mathf.Sign( paint_factor ) * myIntPaintValue[ i ];
			else if ( myType == Type.INT )
			{
				if ( myIntPaintValue[ i ] < myIntMin || myIntPaintValue[ i ] > myIntMax )
					myIntData[ vertex_index * myTupleSize + i ] = myIntPaintValue[ i ];
				else
				{
					int paint_value = myIntPaintValue[ i ];
					if ( inverse )
						paint_value = myIntMax - myIntPaintValue[ i ];

					// First bring the current values back to the min/max range.
					myIntData[ vertex_index * myTupleSize + i ] = Mathf.Clamp(
						myIntData[ vertex_index * myTupleSize + i ], myIntMin, myIntMax );

					int original_value = myIntData[ vertex_index * myTupleSize + i ];
					int new_value = paint_value;
					int distance = new_value - original_value;
					int abs_distance = Mathf.Abs( distance );

					if ( distance != 0 )
					{
						int min_max_distance = myIntMax - myIntMin;
						int max_paint_amount = Mathf.CeilToInt(
							(float) min_max_distance * HoudiniHost.prPaintBrushRate );

						int clamped_distance = Mathf.Min( abs_distance, max_paint_amount );
						float paint_amount = paint_factor * clamped_distance;
						int paint_amount_int = Mathf.CeilToInt( paint_amount );
						int paint_amount_clamped = Mathf.Min( abs_distance, paint_amount_int );
						int paint_amount_signed = (int) Mathf.Sign( distance ) * paint_amount_clamped;

						myIntData[ vertex_index * myTupleSize + i ] += paint_amount_signed;
					}
				}
			}
			else if ( myType == Type.FLOAT )
			{
				if ( myFloatPaintValue[ i ] < myFloatMin || myFloatPaintValue[ i ] > myFloatMax )
					myFloatData[ vertex_index * myTupleSize + i ] = myFloatPaintValue[ i ];
				else
				{
					float paint_value = myFloatPaintValue[ i ];
					if ( inverse )
						paint_value = myFloatMax - myFloatPaintValue[ i ];

					// First bring the current values back to the min/max range.
					myFloatData[ vertex_index * myTupleSize + i ] = Mathf.Clamp(
						myFloatData[ vertex_index * myTupleSize + i ], myFloatMin, myFloatMax );

					float original_value = myFloatData[ vertex_index * myTupleSize + i ];
					float new_value = paint_value;
					float distance = new_value - original_value;

					float min_max_distance = myFloatMax - myFloatMin;
					float max_paint_amount = min_max_distance * HoudiniHost.prPaintBrushRate;

					float clamped_distance =
						Mathf.Sign( distance ) * Mathf.Min( Mathf.Abs( distance ), Mathf.Abs( max_paint_amount ) );
					float paint_amount = paint_factor * clamped_distance;

					myFloatData[ vertex_index * myTupleSize + i ] += paint_amount;
				}
			}
			else if ( myType == Type.STRING )
			{
				// For strings it doesn't matter what the paint_factor is.
				myStringData[ vertex_index * myTupleSize + i ] = myStringPaintValue[ i ];
			}
	}

	public void fill()
	{
		// If we start writing to our data we assume our data is initialized.
		myInitializedVertexCount = myVertexCount;

		if ( myVertexCount > 0 )
			if ( myType == Type.BOOL || myType == Type.INT )
				for ( int i = 0; i < myVertexCount; ++i )
					for ( int j = 0; j < myTupleSize; ++j )
						myIntData[ i * myTupleSize + j ] = myIntPaintValue[ j ];
			else if ( myType == Type.FLOAT )
				for ( int i = 0; i < myVertexCount; ++i )
					for ( int j = 0; j < myTupleSize; ++j )
						myFloatData[ i * myTupleSize + j ] = myFloatPaintValue[ j ];
			else if ( myType == Type.STRING )
				for ( int i = 0; i < myVertexCount; ++i )
					for ( int j = 0; j < myTupleSize; ++j )
						myStringData[ i * myTupleSize + j ] = myStringPaintValue[ j ];
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private

	[SerializeField] private string myName;

	[SerializeField] private Type myType;
	[SerializeField] private HAPI_AttributeOwner myOriginalAttributeOwner;
	[SerializeField] private int myTupleSize;
	[SerializeField] private int myVertexCount;
	[SerializeField] private int myInitializedVertexCount;

	[SerializeField] private int myPaintMode;

	[SerializeField] private int[] myIntPaintValue;
	[SerializeField] private int myIntMin;
	[SerializeField] private int myIntMax;
	[SerializeField] private int[] myIntData;
	
	[SerializeField] private float[] myFloatPaintValue;
	[SerializeField] private float myFloatMin;
	[SerializeField] private float myFloatMax;
	[SerializeField] private float[] myFloatData;

	[SerializeField] private string[] myStringPaintValue;
	[SerializeField] private string[] myStringData;

	// -----------------------------------------------------------------------
	// Constants

	private const int myDefaultIntPaintValue = 1;
	private const int myDefaultIntMin = 0;
	private const int myDefaultIntMax = 10;
	private const float myDefaultFloatPaintValue = 0.1f;
	private const float myDefaultFloatMin = 0.0f;
	private const float myDefaultFloatMax = 1.0f;
	private const string myDefaultStringPaintValue = "";
}
