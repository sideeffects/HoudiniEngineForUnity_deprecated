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
using System.Collections;

using HAPI;

public class HAPI_GeoControl : HAPI_ObjectControl 
{

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	
	// Please keep these in the same order and grouping as their initializations in HAPI_Control.reset().

	public int prGeoId {				get { return myGeoId; }		set { myGeoId = value; } }
	public string prGeoName {			get { return myGeoName; }	set { myGeoName = value; } }
	public HAPI_GeoType prGeoType {		get { return myGeoType; }	set { myGeoType = value; } }

	public HAPI_GeoControl() 
	{
		reset();
	}

	~HAPI_GeoControl()
	{

	}

	public override void reset()
	{
		base.reset();

		// Please keep these in the same order and grouping as their declarations at the top.
		
		prGeoId			= -1;
		prGeoName		= "geo_name";
		prGeoType		= HAPI_GeoType.HAPI_GEOTYPE_DEFAULT;
	}

	public void init( HAPI_GeoControl geo_control )
	{
		init( (HAPI_ObjectControl) geo_control );

		prGeoId			= geo_control.prGeoId;
		prGeoName		= geo_control.prGeoName;
		prGeoType		= geo_control.prGeoType;
	}
	public void init( int geo_id, string geo_name, HAPI_GeoType geo_type )
	{
		prGeoId			= geo_id;
		prGeoName		= geo_name;
		prGeoType		= geo_type;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Serialized Private Data

	[SerializeField] private int			myGeoId;
	[SerializeField] private string			myGeoName;
	[SerializeField] private HAPI_GeoType	myGeoType;
}
