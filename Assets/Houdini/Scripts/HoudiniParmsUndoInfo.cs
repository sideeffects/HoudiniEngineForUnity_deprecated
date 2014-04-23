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
using System.Collections.Generic;

public class HoudiniParmsUndoInfo : ScriptableObject
{	
	public List< string > parmNames = new List< string >();
	public List< int > parmIndices = new List< int >();

	public int[] parmIntValues;
	public float[] parmFloatValues;
	public string[] parmStringValues;
}
