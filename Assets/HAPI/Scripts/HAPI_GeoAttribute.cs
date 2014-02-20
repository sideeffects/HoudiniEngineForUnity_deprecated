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
using System.Collections;

public class HAPI_GeoAttribute : ScriptableObject
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
					for ( int j = 0; j < min_tuple_size; ++j )
						new_data[ i * new_tuple_size + j ] = myIntData[ i * myTupleSize + j ];

				myIntPaintValue = new_paint_value;
				myIntData = new_data;
				myTupleSize = new_tuple_size;
				myPaintMode = Mathf.Min( myPaintMode, (int) SpecialPaintMode.MAX + new_tuple_size );
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
					for ( int j = 0; j < min_tuple_size; ++j )
						new_data[ i * new_tuple_size + j ] = myFloatData[ i * myTupleSize + j ];

				myFloatPaintValue = new_paint_value;
				myFloatData = new_data;
				myTupleSize = new_tuple_size;
				myPaintMode = Mathf.Min( myPaintMode, (int) SpecialPaintMode.MAX + new_tuple_size );
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
					for ( int j = 0; j < min_tuple_size; ++j )
						new_data[ i * new_tuple_size + j ] = myStringData[ i * myTupleSize + j ];

				myStringPaintValue = new_paint_value;
				myStringData = new_data;
				myTupleSize = new_tuple_size;
				myPaintMode = Mathf.Min( myPaintMode, (int) SpecialPaintMode.MAX + new_tuple_size );
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
			myPaintMode = Mathf.Min( value, (int) SpecialPaintMode.MAX + myTupleSize );
		}
	}

	public int[] prIntPaintValue { get { return myIntPaintValue; } set { myIntPaintValue = value; } }
	public int prIntMin { get { return myIntMin; } set { myIntMin = value; } }
	public int prIntMax {
		get { return myIntMax; } 
		set { if ( myType != Type.BOOL ) myIntMax = value; } }
	public int[] prIntData { get { return myIntData; } private set {} }
	public float[] prFloatPaintValue { get { return myFloatPaintValue; } set { myFloatPaintValue = value; } }
	public float prFloatMin { get { return myFloatMin; } set { myFloatMin = value; } }
	public float prFloatMax { get { return myFloatMax; } set { myFloatMax = value; } }
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

	public HAPI.HAPI_AttributeInfo prAttributeInfo
	{
		get
		{
			if ( myType == Type.UNDEFINED )
				throw new HAPI.HAPI_ErrorInvalidArgument( "Geo attribute not defined." );

			HAPI.HAPI_AttributeInfo attr_info = new HAPI.HAPI_AttributeInfo( prName );
			attr_info.exists = true;
			attr_info.owner = HAPI.HAPI_AttributeOwner.HAPI_ATTROWNER_POINT;

			if ( myType == Type.BOOL || myType == Type.INT )
				attr_info.storage = HAPI.HAPI_StorageType.HAPI_STORAGETYPE_INT;
			else if ( myType == Type.FLOAT )
				attr_info.storage = HAPI.HAPI_StorageType.HAPI_STORAGETYPE_FLOAT;
			else if ( myType == Type.STRING )
				attr_info.storage = HAPI.HAPI_StorageType.HAPI_STORAGETYPE_STRING;
			else
				throw new HAPI.HAPI_ErrorInvalidArgument( "Invalid geo attribute type." );

			attr_info.count = myVertexCount;
			attr_info.tupleSize = myTupleSize;
			return attr_info;
		}
		private set {}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods

	public HAPI_GeoAttribute()
	{
		reset();
	}

	public void reset()
	{
		myName = "NO_NAME";

		myType = Type.UNDEFINED;
		myTupleSize = 1;
		myVertexCount = 0;

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
					mesh, HAPI.HAPI_Constants.HAPI_ATTRIB_COLOR, Type.FLOAT,
					HAPI.HAPI_Constants.HAPI_COLOR_VECTOR_SIZE );

				// Set the alpha values to 1.
				if ( HAPI.HAPI_Constants.HAPI_COLOR_VECTOR_SIZE == 4 )
				{
					myFloatPaintValue[ HAPI.HAPI_Constants.HAPI_COLOR_VECTOR_SIZE - 1 ] = 1.0f;
					for ( int i = 0; i < myVertexCount; ++i )
						myFloatData[
							( i * HAPI.HAPI_Constants.HAPI_COLOR_VECTOR_SIZE ) + 
							( HAPI.HAPI_Constants.HAPI_COLOR_VECTOR_SIZE - 1 ) ] = 1.0f;
				}
				break;
			}
			case Preset.UV:
			{
				init(
					mesh, HAPI.HAPI_Constants.HAPI_ATTRIB_UV, Type.FLOAT,
					HAPI.HAPI_Constants.HAPI_UV_VECTOR_SIZE );
				break;
			}
			case Preset.NORMAL:
			{
				init(
					mesh, HAPI.HAPI_Constants.HAPI_ATTRIB_NORMAL, Type.FLOAT,
					HAPI.HAPI_Constants.HAPI_NORMAL_VECTOR_SIZE );

				// Compute the normals as initial attribute values.
				mesh.RecalculateNormals();
				for ( int i = 0; i < myVertexCount; ++i )
					for ( int j = 0; j < HAPI.HAPI_Constants.HAPI_NORMAL_VECTOR_SIZE; ++j )
						myFloatData[ i * HAPI.HAPI_Constants.HAPI_NORMAL_VECTOR_SIZE + j ] =
							mesh.normals[ i ][ j ];
				break;
			}
			default: throw new HAPI.HAPI_ErrorInvalidArgument( "Invalid HAPI_GeoAttribute.Preset!" );
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

		if ( type == Type.BOOL || type == Type.INT )
		{
			myIntPaintValue = new int[ tuple_size ];
			myIntMin = myDefaultIntMin;
			myIntMax = myDefaultIntMax;
			myIntData = new int[ mesh.vertexCount * tuple_size ];

			for ( int i = 0; i < tuple_size; ++i )
				if ( type == Type.BOOL )
				{
					// These are hard coded because...well, BOOLs.
					myIntPaintValue[ i ] = 1;
					myIntMin = 0;
					myIntMax = 1;
				}
				else if ( type == Type.INT )
					myIntPaintValue[ i ] = myDefaultIntPaintValue;
		}
		else if ( type == Type.FLOAT )
		{
			myFloatPaintValue = new float[ tuple_size ];
			myFloatMin = myDefaultFloatMin;
			myFloatMax = myDefaultFloatMax;
			myFloatData = new float[ mesh.vertexCount * tuple_size ];

			for ( int i = 0; i < tuple_size; ++i )
				myFloatPaintValue[ i ] = myDefaultFloatPaintValue;
		}
		else if ( type == Type.STRING )
		{
			myStringPaintValue = new string[ tuple_size ];
			myStringData = new string[ mesh.vertexCount * tuple_size ];

			for ( int i = 0; i < tuple_size; ++i )
				myStringPaintValue[ i ] = myDefaultStringPaintValue;
		}
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

	// -----------------------------------------------------------------------
	// Paint

	public void paint( int vertex_index, float paint_factor )
	{
		if ( vertex_index <= 0 || vertex_index >= myVertexCount )
			return; // TODO: Throw error.

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
					int original_value = myIntData[ vertex_index * myTupleSize + i ];
					int new_value = myIntPaintValue[ i ];
					int distance = new_value - original_value;

					int min_max_distance = myIntMax - myIntMin;
					float max_paint_amount = (float) min_max_distance * HAPI.HAPI_Host.prPaintBrushRate;

					if ( paint_factor < 0.0f )
						distance = min_max_distance - Mathf.Abs( distance ) + (int) Mathf.Sign( distance );

					float clamped_distance =
						Mathf.Sign( distance ) * Mathf.Min( Mathf.Abs( distance ), Mathf.Abs( max_paint_amount ) );
					float paint_amount = paint_factor * clamped_distance;

					int paint_amount_int =
						distance > 0 ? Mathf.CeilToInt( paint_amount ) : Mathf.FloorToInt( paint_amount );

					myIntData[ vertex_index * myTupleSize + i ] += paint_amount_int;
				}
			}
			else if ( myType == Type.FLOAT )
			{
				if ( myFloatPaintValue[ i ] < myFloatMin || myFloatPaintValue[ i ] > myFloatMax )
					myFloatData[ vertex_index * myTupleSize + i ] = myFloatPaintValue[ i ];
				else
				{
					float original_value = myFloatData[ vertex_index * myTupleSize + i ];
					float new_value = myFloatPaintValue[ i ];
					float distance = new_value - original_value;

					float min_max_distance = myFloatMax - myFloatMin;
					float max_paint_amount = min_max_distance * HAPI.HAPI_Host.prPaintBrushRate;

					if ( paint_factor < 0.0f )
						distance = min_max_distance - distance;

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

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private

	[SerializeField] private string myName;

	[SerializeField] private Type myType;
	[SerializeField] private int myTupleSize;
	[SerializeField] private int myVertexCount;

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
