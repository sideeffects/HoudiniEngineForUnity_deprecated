using UnityEngine;

public class HAPI_PresetMap : HAPI_Map< string, HAPI_Preset >
{
	public HAPI_PresetMap() : base( "PresetMap" ) {}

	public void set( string key, byte[] value )
	{
		if ( base.contains( key ) )
		{
			HAPI_Preset preset = base.get( key );
			preset.myPreset = new byte[ value.Length ];
			value.CopyTo( preset.myPreset, 0 );
		}
		else
		{
			HAPI_Preset preset = ScriptableObject.CreateInstance< HAPI_Preset >();
			preset.myPreset = new byte[ value.Length ];
			value.CopyTo( preset.myPreset, 0 );
			base.add( key, preset );
		}
	}

	public void add( string key, byte[] value )
	{
		this.set( key, value );
	}

	public new byte[] get( string key )
	{
		HAPI_Preset preset = base.get( key );
		return preset.myPreset;
	}
}
