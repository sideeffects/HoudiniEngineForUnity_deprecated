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
		MAX
	}

	public enum Preset
	{
		UNDEFINED = -1,
		POSITION,
		UV,
		NORMAL,
		COLOR,
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
			if ( myType == Type.UNDEFINED )
				return;

			if ( myType == value || value <= Type.UNDEFINED || value >= Type.MAX )
				return;

			if ( myType == Type.BOOL || myType == Type.INT )
			{
				myFloatPaintValue = new float[ myIntPaintValue.Length ];
				for ( int i = 0; i < myIntPaintValue.Length; ++i )
					myFloatPaintValue[ i ] = (float) myIntPaintValue[ i ];
				myIntPaintValue = null;

				myFloatData = new float[ myIntData.Length ];
				for ( int i = 0; i < myIntData.Length; ++i )
					myFloatData[ i ] = (float) myIntData[ i ];
				myIntData = null;
			}
			else if ( myType == Type.FLOAT )
			{
				myIntPaintValue = new int[ myFloatPaintValue.Length ];
				for ( int i = 0; i < myFloatPaintValue.Length; ++i )
					if ( value == Type.BOOL )
						myIntPaintValue[ i ] = (int) myFloatPaintValue[ i ] > 0 ? 1 : 0;
					else
						myIntPaintValue[ i ] = (int) myFloatPaintValue[ i ];
				myFloatPaintValue = null;

				myIntData = new int[ myFloatData.Length ];
				for ( int i = 0; i < myFloatData.Length; ++i )
					if ( value == Type.BOOL )
						myIntData[ i ] = (int) myFloatData[ i ] > 0 ? 1 : 0;
					else
						myIntData[ i ] = (int) myFloatData[ i ];
				myFloatData = null;
			}

			myType = value;
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

				for ( int i = 0; i < myVertexCount; ++i )
					for ( int j = 0; j < min_tuple_size; ++j )
						new_data[ i * new_tuple_size + j ] = myIntData[ i * myTupleSize + j ];

				myIntPaintValue = new_paint_value;
				myIntData = new_data;
				myTupleSize = new_tuple_size;
			}
			else if ( myType == Type.FLOAT )
			{
				float[] new_paint_value = new float[ new_tuple_size ];
				float[] new_data = new float[ myVertexCount * new_tuple_size ];

				int min_tuple_size = Mathf.Min( myTupleSize, new_tuple_size );
				for ( int i = 0; i < min_tuple_size; ++i )
					new_paint_value[ i ] = myFloatPaintValue[ i ];

				for ( int i = 0; i < myVertexCount; ++i )
					for ( int j = 0; j < min_tuple_size; ++j )
						new_data[ i * new_tuple_size + j ] = myFloatData[ i * myTupleSize + j ];

				myFloatPaintValue = new_paint_value;
				myFloatData = new_data;
				myTupleSize = new_tuple_size;
			}
		}
	}

	public int[] prIntPaintValue { get { return myIntPaintValue; } set { myIntPaintValue = value; } }
	public float[] prFloatPaintValue { get { return myFloatPaintValue; } set { myFloatPaintValue = value; } }

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

		myIntPaintValue = null;
		myIntData = null;

		myFloatPaintValue = null;
		myFloatData = null;
	}

	public void init( Mesh mesh, Preset preset )
	{
		switch ( preset )
		{
			case Preset.POSITION:
			{
				init(
					mesh, HAPI.HAPI_Constants.HAPI_ATTRIB_POSITION, Type.FLOAT,
					HAPI.HAPI_Constants.HAPI_POSITION_VECTOR_SIZE );
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
				break;
			}
			case Preset.COLOR:
			{
				init(
					mesh, HAPI.HAPI_Constants.HAPI_ATTRIB_COLOR, Type.FLOAT,
					HAPI.HAPI_Constants.HAPI_COLOR_VECTOR_SIZE );
				break;
			}
			default: return; // Throw error.
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
			myIntData = new int[ mesh.vertexCount * tuple_size ];
		}
		else if ( type == Type.FLOAT )
		{
			myFloatPaintValue = new float[ tuple_size ];
			myFloatData = new float[ mesh.vertexCount * tuple_size ];
		}
	}

	// -----------------------------------------------------------------------
	// Representation

	public Color[] getColorRepresentation()
	{
		if ( myType == Type.UNDEFINED )
			return null; // Throw error.

		Color[] colors = new Color[ myVertexCount ];

		for ( int i = 0; i < myVertexCount; ++i )
		{
			colors[ i ].r = 1.0f;
			colors[ i ].g = 1.0f;
			colors[ i ].b = 1.0f;
			colors[ i ].a = 1.0f;

			if ( myType == Type.BOOL )
				for ( int j = 0; j < myTupleSize; ++j )
					colors[ i ][ j ] = (float) myIntData[ i * myTupleSize + j ];
			else if ( myType == Type.INT )
				for ( int j = 0; j < myTupleSize; ++j )
					colors[ i ][ j ] = (float) myIntData[ i * myTupleSize + j ];
			else if ( myType == Type.FLOAT )
				for ( int j = 0; j < myTupleSize; ++j )
					colors[ i ][ j ] = myFloatData[ i * myTupleSize + j ];
		}

		return colors;
	}

	// -----------------------------------------------------------------------
	// Paint

	public void paint( int vertex_index, float paint_factor )
	{
		if ( vertex_index <= 0 || vertex_index >= myVertexCount )
			return; // Throw error.

		for ( int i = 0; i < myTupleSize; ++i )
			if ( myType == Type.BOOL )
				myIntData[ vertex_index * myTupleSize + i ] += (int) Mathf.Sign( paint_factor ) * myIntPaintValue[ i ];
			else if ( myType == Type.INT )
			{
				myIntData[ vertex_index * myTupleSize + i ] += (int) ( paint_factor * (float) myIntPaintValue[ i ] );
				myIntData[ vertex_index * myTupleSize + i ] = Mathf.Clamp( myIntData[ vertex_index * myTupleSize + i ], 0, 1 );
			}
			else if ( myType == Type.FLOAT )
			{
				myFloatData[ vertex_index * myTupleSize + i ] += paint_factor * myFloatPaintValue[ i ];
				myFloatData[ vertex_index * myTupleSize + i ] = Mathf.Clamp( myFloatData[ vertex_index * myTupleSize + i ], 0.0f, 1.0f );
			}
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private

	[SerializeField] private string myName;

	[SerializeField] private Type myType;
	[SerializeField] private int myTupleSize;
	[SerializeField] private int myVertexCount;

	[SerializeField] private int[] myIntPaintValue;
	[SerializeField] private int[] myIntData;
	
	[SerializeField] private float[] myFloatPaintValue;
	[SerializeField] private float[] myFloatData;
}
