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

// Master control for enabling runtime.
#if ( UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_LINUX )
	#define HAPI_ENABLE_RUNTIME
#endif

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
//[ InitializeOnLoad ]
#endif
public static class HoudiniDataFile
{
	private static string myDataFileName;
	private static string myDataFolderPath;
	private static string myDataFilePath;
	private static Dictionary< string, string > myData;

	public static string prDataFilePath { get { return myDataFilePath; } private set {} }

	//static HoudiniDataFile()
	private static void initialize()
	{
		if ( myData != null )
			return;

		myDataFileName = "HoudiniSessionData.txt";
		myDataFolderPath = Application.dataPath + "/../HoudiniTemp";
		myDataFilePath = myDataFolderPath + "/" + myDataFileName;
		myData = new Dictionary< string, string >();

		if ( !Directory.Exists( myDataFolderPath ) )
			Directory.CreateDirectory( myDataFolderPath );

		if ( !File.Exists( myDataFilePath ) )
		{
#if HAPI_ENABLE_RUNTIME
			Debug.Log(
				"Houdini Engine: State datafile doesn't exist. Creating it.\n" +
				"Datafile Path: " + myDataFilePath );
#endif // HAPI_ENABLE_RUNTIME
			FileStream file = File.Create( myDataFilePath );
			file.Close();
		}
		else
		{
#if HAPI_ENABLE_RUNTIME
			Debug.Log(
				"Houdini Engine: State datafile exists.\n" +
				"    Datafile Path: " + myDataFilePath );
#endif // HAPI_ENABLE_RUNTIME
			System.Diagnostics.Process current_process = System.Diagnostics.Process.GetCurrentProcess();
			int new_process_id = current_process.Id;
			int old_process_id = getInt( "CurrentProcessId", -1 );
			if ( old_process_id != new_process_id )
			{
#if HAPI_ENABLE_RUNTIME
				Debug.Log(
					"Houdini Engine: New instance of Unity detected - we have a new process id.\n" +
					"    Datafile Path: " + myDataFilePath + "\n" +
					"    Old Process Id: " + old_process_id + "\n" +
					"    New Process Id: " + new_process_id );
#endif // HAPI_ENABLE_RUNTIME
				reset();
			}

			setInt( "CurrentProcessId", current_process.Id ); 
		}
	}

	public static void reset()
	{
		if ( myData == null )
			initialize();

		myData.Clear();
		if ( File.Exists( myDataFilePath ) )
		{
			File.Delete( myDataFilePath );
			FileStream file = File.Create( myDataFilePath );
			file.Close();
		}
	}

	public static bool doesFileExist()
	{
		if ( myDataFilePath != null && myDataFilePath != "" && File.Exists( myDataFilePath ) )
			return true;
		else
			return false;
	}

	public static void load()
	{
		if ( myData == null )
			initialize();

		myData.Clear();

		if ( !doesFileExist() )
			return;

		StreamReader reader = new StreamReader( myDataFilePath );
		while ( true )
		{
			string line = reader.ReadLine();
			if ( line == null )
				break;

			char [] split_chars = { '=' };
			string [] split_line = line.Split( split_chars, 2 );
			string key = split_line[ 0 ].Trim();
			string value = split_line[ 1 ].Trim();
			myData.Add( key, value );
		}

		reader.Close();
	}

	public static void save()
	{
		if ( myData == null )
			initialize();

		if ( !doesFileExist() )
			return;

		StreamWriter writer = new StreamWriter( myDataFilePath, false );

		foreach ( KeyValuePair< string, string > entry in myData )
		{
			writer.Write( entry.Key + " = " + entry.Value );
			writer.WriteLine();
		}

		writer.Close();
	}

#if HAPI_ENABLE_RUNTIME

	public static int getInt( string name, int default_value )
	{
		string str_value = getString( name, default_value.ToString() );
		int value = default_value;
		if ( int.TryParse( str_value, out value ) )
			return value;
		else
			return default_value;
	}
	public static void setInt( string name, int value )
	{
		setString( name, value.ToString() );
	}

	public static long getLong( string name, long default_value )
	{
		string str_value = getString( name, default_value.ToString() );
		long value = default_value;
		if ( long.TryParse( str_value, out value ) )
			return value;
		else
			return default_value;
	}
	public static void setLong( string name, long value )
	{
		setString( name, value.ToString() );
	}
	
	public static bool getBool( string name, bool default_value )
	{
		string str_value = getString( name, default_value ? "true" : "false" );
		if ( str_value == "true" )
			return true;
		else if ( str_value == "false" )
			return false;
		else
			return default_value;
	}
	public static void setBool( string name, bool value )
	{
		setString( name, value ? "true" : "false" );
	}
	
	public static float getFloat( string name, float default_value )
	{
		string str_value = getString( name, default_value.ToString() );
		float value = default_value;
		if ( float.TryParse( str_value, out value ) )
			return value;
		else
			return default_value;
	}
	public static void setFloat( string name, float value )
	{
		setString( name, value.ToString() );
	}
	
	public static string getString( string name, string default_value )
	{
		load();
		string value = "";
		if ( myData.TryGetValue( name, out value ) )
			return value;
		else
			return default_value;
	}
	public static void setString( string name, string value )
	{
		if ( myData.ContainsKey( name ) )
			myData[ name ] = value;
		else
			myData.Add( name, value );
		save();
	}

#else
	public static int getInt( string name, int default_value ) { return default_value; }
	public static void setInt( string name, int value ) {}
	
	public static bool getBool( string name, bool default_value ) { return default_value; }
	public static void setBool( string name, bool value ) {}
	
	public static float getFloat( string name, float default_value ) { return default_value; }
	public static void setFloat( string name, float value ) {}
	
	public static string getString( string name, string default_value ) { return default_value; }
	public static void setString( string name, string value ) {}
#endif // HAPI_ENABLE_RUNTIME
}
