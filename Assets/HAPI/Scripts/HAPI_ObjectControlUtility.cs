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
using HAPI;

/// <summary>
/// 	Main script attached to an Unity game object that corresponds to a Houdini asset instance on the 
/// 	Houdini side.
/// </summary>
public partial class HAPI_ObjectControl : MonoBehaviour
{
	// GET ----------------------------------------------------------------------------------------------------------
	
	private delegate void getArray1IdDel< T >( int id1, [Out] T[] data, int start, int end );
	private delegate void getArray2IdDel< T >( int id1, int id2, [Out] T[] data, int start, int end );
	private delegate void getArray3IdDel< T >( int id1, int id2, int id3, [Out] T[] data, int start, int end );
	
	private void getArray1Id< T >( int id1, getArray1IdDel< T > func, [Out] T[] data, 
								   int count )
	{
		getArray( id1, 0, 0, func, null, null, data, count, 1 );
	}
	private void getArray1Id< T >( int id1, getArray1IdDel< T > func, [Out] T[] data, 
								   int count, int tuple_size )
	{
		getArray( id1, 0, 0, func, null, null, data, count, tuple_size );
	}
	
	private void getArray2Id< T >( int id1, int id2, getArray2IdDel< T > func, [Out] T[] data, 
								   int count )
	{
		getArray( id1, id2, 0, null, func, null, data, count, 1 );
	}
	private void getArray2Id< T >( int id1, int id2, getArray2IdDel< T > func, [Out] T[] data, 
								   int count, int tuple_size )
	{
		getArray( id1, id2, 0, null, func, null, data, count, tuple_size );
	}
	
	private void getArray3Id< T >( int id1, int id2, int id3, getArray3IdDel< T > func, [Out] T[] data, 
								   int count )
	{
		getArray( id1, id2, id3, null, null, func, data, count, 1 );
	}
	private void getArray3Id< T >( int id1, int id2, int id3, getArray3IdDel< T > func, [Out] T[] data, 
								   int count, int tuple_size )
	{
		getArray( id1, id2, id3, null, null, func, data, count, tuple_size );
	}
	
	private void getArray< T >( int id1, int id2, int id3, 
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
			
			if ( func_1id != null )
				func_1id( id1, local_array, current_index, length );
			else if ( func_2id != null )
				func_2id( id1, id2, local_array, current_index, length );
			else if ( func_3id != null )
				func_3id( id1, id2, id3, local_array, current_index, length );
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
	
	private delegate void setArray1IdDel< T >( int id1, [Out] T[] data, int start, int end );
	private delegate void setArray2IdDel< T >( int id1, int id2, [Out] T[] data, int start, int end );
	private delegate void setArray3IdDel< T >( int id1, int id2, int id3, [Out] T[] data, int start, int end );
	
	private void setArray1Id< T >( int id1, getArray1IdDel< T > func, [Out] T[] data, 
								   int count )
	{
		setArray( id1, 0, 0, func, null, null, data, count, 1 );
	}
	private void setArray1Id< T >( int id1, getArray1IdDel< T > func, [Out] T[] data, 
								   int count, int tuple_size )
	{
		setArray( id1, 0, 0, func, null, null, data, count, tuple_size );
	}
	
	private void setArray2Id< T >( int id1, int id2, getArray2IdDel< T > func, [Out] T[] data, 
								   int count )
	{
		setArray( id1, id2, 0, null, func, null, data, count, 1 );
	}
	private void setArray2Id< T >( int id1, int id2, getArray2IdDel< T > func, [Out] T[] data, 
								   int count, int tuple_size )
	{
		setArray( id1, id2, 0, null, func, null, data, count, tuple_size );
	}
	
	private void setArray3Id< T >( int id1, int id2, int id3, getArray3IdDel< T > func, [Out] T[] data, 
								   int count )
	{
		setArray( id1, id2, id3, null, null, func, data, count, 1 );
	}
	private void setArray3Id< T >( int id1, int id2, int id3, getArray3IdDel< T > func, [Out] T[] data, 
								   int count, int tuple_size )
	{
		setArray( id1, id2, id3, null, null, func, data, count, tuple_size );
	}
	
	private void setArray< T >( int id1, int id2, int id3, 
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
		
	private delegate void fillAttrArrayInputFunc< T >( int asset_id, int object_id, ref HAPI_AttributeInfo info, 
													   [Out] T[] items, int start, int end );	
	private void fillAttrArray< T >( int asset_id, int object_id, ref HAPI_AttributeInfo info, 
									 T[] items, fillAttrArrayInputFunc< T > get_func, int count ) 
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
			get_func( asset_id, object_id, ref info, local_array, current_index, length );
			
			// Copy data from the temporary array.
			for ( int i = current_index; i < current_index + length; ++i )
				for ( int j = 0; j < info.tupleSize; ++j )
					items[ i * info.tupleSize + j ] = local_array[ ( i - current_index ) * info.tupleSize + j ];
			
			current_index += length;
		}
	}
	
	private void getAttribute< T >( int asset_id, int object_id, ref HAPI_AttributeInfo info, ref T[] data,
									fillAttrArrayInputFunc< T > get_func )
	{
		int original_tuple_size = info.tupleSize;		
		
		for ( int type = 0; type < (int) HAPI_AttributeOwner.HAPI_ATTROWNER_MAX; ++type )
		{
			info.owner = type;
			HAPI_Host.getAttributeInfo( asset_id, object_id, type, ref info );
			if ( info.exists )
				break;
		}
		if ( !info.exists )
			return;			
		
		if ( original_tuple_size > 0 )
			info.tupleSize = original_tuple_size;
		
		data = new T[ info.count * info.tupleSize ];
		fillAttrArray( asset_id, object_id, ref info, data, get_func, info.count );
	}
	
	private string[] getAttributeNames( int asset_id, int object_id, HAPI_DetailInfo detail_info, 
										HAPI_AttributeOwner owner )
	{
		int attr_count = detail_info.getOwnerCount( owner );
			
		string[] names = new string[ attr_count ];
		
		HAPI_AttributeStrValue[] attr_names = new HAPI_AttributeStrValue[ attr_count ];
		HAPI_Host.getAttributeNames( asset_id, object_id, (int) owner, attr_names, attr_count );
		for ( int ii = 0; ii < attr_count; ++ii )
			names[ ii ] = attr_names[ ii ].value;
		
		return names;
	}
	
	private void printAttributeNames( int asset_id, int object_id, HAPI_DetailInfo detail_info,
									  HAPI_AttributeOwner owner )
	{
		string[] names = getAttributeNames( asset_id, object_id, detail_info, owner );
		
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
	
	private void printAllAttributeNames( int asset_id, int object_id, HAPI_DetailInfo detail_info )
	{
		for ( int owner = 0; owner < (int) HAPI_AttributeOwner.HAPI_ATTROWNER_MAX; ++owner )
			printAttributeNames( asset_id, object_id, detail_info, (HAPI_AttributeOwner) owner );
	}
		
	// PROGRESS BAR -------------------------------------------------------------------------------------------------
	
	private void incrementProgressBar()
	{
		displayProgressBar( 1 );
	}
	
	private void displayProgressBar()
	{
		displayProgressBar( 0 );	
	}
	
	private void displayProgressBar( int increment )
	{
		System.DateTime current = System.DateTime.Now;
		System.TimeSpan delta = current - myProgressBarStartTime;
		
		// This delay for displaying the progress bar is so the bar won't flicker for really quick updates
		// (less than a few seconds). Also, when we do show the progress bar the focus of the current 
		// inspector control is lost.
		if ( delta.Seconds < HAPI_Constants.HAPI_SEC_BEFORE_PROGRESS_BAR_SHOW )
		{
			EditorUtility.ClearProgressBar();
			return;
		}
		
		myProgressBarJustUsed = true;
				
		myProgressBarCurrent += increment;
		string message = myProgressBarMsg + " Item " + myProgressBarCurrent + " of " + myProgressBarTotal;
		bool result = !EditorUtility.DisplayCancelableProgressBar( myProgressBarTitle, message, 
												Mathf.InverseLerp( 0, myProgressBarTotal, myProgressBarCurrent ) );
		
		if ( !result )
			throw new HAPI_ErrorProgressCancelled();
	}
	
	public bool hasProgressBarBeenUsed()
	{
		return myProgressBarJustUsed;	
	}
	
	private void clearProgressBar()
	{
		myProgressBarJustUsed = false;
		myProgressBarCurrent = 0;
		EditorUtility.ClearProgressBar();
	}
}
