using UnityEngine;
using System.Collections;

public class HAPI_Curve : MonoBehaviour {
	
	public HAPI_Curve() {
		mySegmentCount = 30;
		myPointCount = 0;
	}
	
	public void AddPoint( Vector3 position ) {
		myPointCount++;
		
		// expand array
		Vector3[] newPoints = new Vector3[ myPointCount ];
		
		for ( int i = 0; i < myPointCount - 1; ++i ) {
			newPoints[ i ] = myPoints[ i ];
		}
		
		// add new one
		newPoints[ myPointCount - 1 ] = position;
		
		// save as original
		myPoints = newPoints;
	}
	
	public Vector3 GetPointPosition( int index ) {
		if ( index >= 0 && index < myPointCount ) {
			return myPoints[ index ];
		} else {
			Debug.LogError( "Index out of bounds." );
			return Vector3.zero;	
		}
	}
	
	public bool SetPointPosition( int index, Vector3 position ) {
		if ( index >= 0 && index < myPointCount ) {
			myPoints[ index ] = position;
			return true;
		} else {
			Debug.LogError( "Index out of bounds." );
			return false;	
		}
	}
	
	public int GetPointCount() {
		return myPointCount;	
	}
	
	// Use this for initialization
	public void Start() {
	
	}
	
	// Update is called once per frame
	public void Update() {
	
	}
	
	// Public Members
	
	public int mySegmentCount;
	
	// Private Members
	
	private Vector3[] myPoints;
	private int myPointCount;
	
}
