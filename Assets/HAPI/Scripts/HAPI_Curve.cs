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
 * 		Contains curve object prototype.
 * 
 */

using UnityEngine;
using System.Collections;

public class HAPI_Curve : MonoBehaviour 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	public HAPI_Curve() 
	{
		mySegmentCount 	= 30;
		myPointCount 	= 0;
	}
	
	public void addPoint( Vector3 position ) 
	{
		myPointCount++;
		
		// Expand array.
		Vector3[] new_points = new Vector3[ myPointCount ];
		
		for ( int i = 0; i < myPointCount - 1; ++i )
			new_points[ i ] = myPoints[ i ];
		
		// Add new point.
		new_points[ myPointCount - 1 ] = position;
		
		// Set new array as main array.
		myPoints = new_points;
	}
	
	public Vector3 getPointPosition( int index ) 
	{
		if ( index >= 0 && index < myPointCount )
			return myPoints[ index ];
		else 
		{
			Debug.LogError( "Index out of bounds." );
			return Vector3.zero;	
		}
	}
	
	public bool setPointPosition( int index, Vector3 position ) 
	{
		if ( index >= 0 && index < myPointCount ) 
		{
			myPoints[ index ] = position;
			return true;
		} 
		else 
		{
			Debug.LogError( "Index out of bounds." );
			return false;
		}
	}
	
	public int getPointCount() 
	{
		return myPointCount;	
	}
	
	public int 			mySegmentCount;
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private
	
	private Vector3[] 	myPoints;
	private int 		myPointCount;
	
}
