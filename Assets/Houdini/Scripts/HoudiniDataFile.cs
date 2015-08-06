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
	private static bool myIsInitialized;
	private static string myDataFileName;
	private static string myDataFolderPath;
	private static string myDataFilePath;
	private static Dictionary< string, string > myData;

	//static HoudiniDataFile()
	public static void initialize()
	{
		myDataFileName = "HoudiniSessionData.txt";
		myDataFolderPath = Application.dataPath + "/../Temp";
		myDataFilePath = myDataFolderPath + "/" + myDataFileName;
		myData = new Dictionary< string, string >();

		if ( !Directory.Exists( myDataFolderPath ) )
			Directory.CreateDirectory( myDataFolderPath );

		if ( !File.Exists( myDataFilePath ) )
		{
			FileStream file = File.Create( myDataFilePath );
			file.Close();
		}

		myIsInitialized = true;
	}

	public static void reset()
	{
		if ( myIsInitialized && File.Exists( myDataFilePath ) )
			File.Delete( myDataFilePath );
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
		if ( !myIsInitialized )
			return;

		myData.Clear();
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
		if ( !myIsInitialized )
			return;

		StreamWriter writer = new StreamWriter( myDataFilePath, false );

		foreach ( KeyValuePair< string, string > entry in myData )
		{
			writer.Write( entry.Key + " = " + entry.Value );
			writer.WriteLine();
		}

		writer.Close();
	}

#if UNITY_EDITOR

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
#endif // UNITY_EDITOR
}
