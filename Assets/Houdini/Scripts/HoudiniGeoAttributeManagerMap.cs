using UnityEngine;

public class HoudiniGeoAttributeManagerMap : HoudiniMap< string, HoudiniGeoAttributeManager >
{
	public HoudiniGeoAttributeManagerMap() : base( "GeoAttributeManagerMap" ) {}

	public HoudiniGeoAttributeManagerMap copy()
	{
		HoudiniGeoAttributeManagerMap preset_map = ScriptableObject.CreateInstance< HoudiniGeoAttributeManagerMap >();
		preset_map.name = name + "(Clone)";

		foreach ( string key in myKeys )
		{
			preset_map.add( key, get( key ).copy() );
		}

		return preset_map;
	}
}
