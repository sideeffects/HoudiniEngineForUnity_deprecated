/*
* Copyright (c) <2017> Side Effects Software Inc.
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*
* Produced by:
*      Side Effects Software Inc
*      123 Front Street West, Suite 1401
*      Toronto, Ontario
*      Canada   M5J 2M2
*      416-504-9876
*
*/

using UnityEngine;

public class HoudiniPresetMap : HoudiniMap< string, HoudiniPreset >
{
	public HoudiniPresetMap() : base( "PresetMap" ) {}

	public void set( string key, byte[] value )
	{
		if ( base.contains( key ) )
		{
			HoudiniPreset preset = base.get( key );
			preset.myPreset = new byte[ value.Length ];
			value.CopyTo( preset.myPreset, 0 );
		}
		else
		{
			HoudiniPreset preset = ScriptableObject.CreateInstance< HoudiniPreset >();
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
		HoudiniPreset preset = base.get( key );
		return preset.myPreset;
	}

	public HoudiniPresetMap clone()
	{
		HoudiniPresetMap preset_map = ScriptableObject.CreateInstance< HoudiniPresetMap >();
		preset_map.name = name + "(Clone)";

		foreach ( string key in myKeys )
		{
			preset_map.add( key, get( key ) );
		}

		return preset_map;
	}
}
