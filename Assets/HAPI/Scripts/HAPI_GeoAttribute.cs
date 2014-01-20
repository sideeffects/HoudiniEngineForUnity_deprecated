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

	public string prName { get { return myName; } private set {} }

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
	// Getters

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
	// Set Paint Value

	public void setPaintValue( bool[] value )
	{
		if ( value.Length != myTupleSize )
			return; // Throw error.

		if ( myType != Type.BOOL )
			return; // Throw error.

		for ( int i = 0; i < myTupleSize; ++i )
			myIntPaintValue[ i ] = value[ i ] ? 1 : 0;
	}

	public void setPaintValue( int[] value )
	{
		if ( value.Length != myTupleSize )
			return; // Throw error.

		if ( myType != Type.INT )
			return; // Throw error.

		for ( int i = 0; i < myTupleSize; ++i )
			myIntPaintValue[ i ] = value[ i ];
	}

	public void setPaintValue( float[] value )
	{
		if ( value.Length != myTupleSize )
			return; // Throw error.

		if ( myType != Type.FLOAT )
			return; // Throw error.

		for ( int i = 0; i < myTupleSize; ++i )
			myFloatPaintValue[ i ] = value[ i ];
	}

	// -----------------------------------------------------------------------
	// Set Paint Value

	public void paint( int vertex_index, bool[] amount )
	{
		if ( amount.Length != myTupleSize )
			return; // Throw error.

		if ( myType != Type.BOOL )
			return; // Throw error.

		if ( vertex_index <= 0 || vertex_index >= myVertexCount )
			return; // Throw error.

		for ( int i = 0; i < myTupleSize; ++i )
			myIntData[ vertex_index * myTupleSize + i ] = amount[ i ] ? 1 : 0;
	}

	public void paint( int vertex_index, int[] amount )
	{
		if ( amount.Length != myTupleSize )
			return; // Throw error.

		if ( myType != Type.INT )
			return; // Throw error.

		if ( vertex_index <= 0 || vertex_index >= myVertexCount )
			return; // Throw error.

		for ( int i = 0; i < myTupleSize; ++i )
			myIntData[ vertex_index * myTupleSize + i ] = amount[ i ];
	}

	public void paint( int vertex_index, float[] amount )
	{
		if ( amount.Length != myTupleSize )
			return; // Throw error.

		if ( myType != Type.FLOAT )
			return; // Throw error.

		if ( vertex_index <= 0 || vertex_index >= myVertexCount )
			return; // Throw error.

		for ( int i = 0; i < myTupleSize; ++i )
			myFloatData[ vertex_index * myTupleSize + i ] = amount[ i ];
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
