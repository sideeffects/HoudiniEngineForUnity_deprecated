using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

namespace HAPI 
{
	public enum HAPI_ParameterType
	{
		HAPI_PARMTYPE_INT = 0,
		HAPI_PARMTYPE_FLOAT = 1,
		HAPI_PARMTYPE_STRING = 2,
		HAPI_PARMTYPE_FOLDER = 3,
		HAPI_PARMTYPE_TOGGLE = 4,
		HAPI_PARMTYPE_COLOUR = 5
	}
	
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_AssetInfo 
	{
		public int id;
		
		[ MarshalAs( UnmanagedType.ByValTStr, SizeConst = 1024 ) ]
		public string otlFilePath;
		
		[ MarshalAs( UnmanagedType.ByValTStr, SizeConst = 256 ) ]		
		public string assetName;
		
		[ MarshalAs( UnmanagedType.ByValTStr, SizeConst = 256 ) ]		
		public string assetInstancePath;
		
		public int objectCount;
		public int parameterCount;
	}
	
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_Parameter 
	{
		public int id;
		public int parentId;
		
		public int type;
		public int size;
		
		[ MarshalAs( UnmanagedType.U1 ) ]
		public bool invisible;
		
		[ MarshalAs( UnmanagedType.U1 ) ]
		public bool joinNext;
		
		[ MarshalAs( UnmanagedType.U1 ) ]
		public bool labelNone;
		
		[ MarshalAs( UnmanagedType.ByValTStr, SizeConst = 256 ) ]
		public string name;
		
		[ MarshalAs( UnmanagedType.ByValTStr, SizeConst = 256 ) ]		
		public string label;		
		
		[ MarshalAs( UnmanagedType.ByValArray, SizeConst = 4 ) ]
		public int[] intValue;
		
		[ MarshalAs( UnmanagedType.ByValArray, SizeConst = 4 ) ]
		public float[] floatValue;
		
		[ MarshalAs( UnmanagedType.ByValTStr, SizeConst = 8192 ) ]		
		public string stringValue;
		
	}
	
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_ObjectInfo 
	{
		public int id;
		
		[ MarshalAs( UnmanagedType.ByValTStr, SizeConst = 1024 ) ]
		public string objectInstancePath;
		
		[ MarshalAs( UnmanagedType.U1 ) ]
		public bool hasTransformChanged;
		
		[ MarshalAs( UnmanagedType.U1 ) ]
		public bool hasGeoChanged;
	}
	
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_RawGeometry {
		[ MarshalAs( UnmanagedType.ByValTStr, SizeConst = 512 ) ]
		public string name;
		
		[ MarshalAs( UnmanagedType.ByValTStr, SizeConst = 1024 ) ]		
		public string textureName;
		
		[ MarshalAs( UnmanagedType.ByValTStr, SizeConst = 1024 ) ]		
		public string bumpName;		
		
		[ MarshalAs( UnmanagedType.ByValArray, SizeConst = 3 ) ]
		public float[] position;
		
		[ MarshalAs( UnmanagedType.R4) ]
		public float roll, pitch, yaw;
		
		[ MarshalAs( UnmanagedType.ByValArray, SizeConst = 3 ) ]
		public float[] scale;
		
		public int vertexCount;
		public int primCount;
		public int instanceCount;
		
		[ MarshalAs( UnmanagedType.ByValTStr, SizeConst = 16 ) ]
		public string id;
	}
	
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_RawVertex 
	{
		[ MarshalAs( UnmanagedType.ByValArray, SizeConst = 3, ArraySubType = UnmanagedType.R4 ) ]
		public float[] normal;
		
		[ MarshalAs( UnmanagedType.ByValArray, SizeConst = 3, ArraySubType = UnmanagedType.R4 ) ]
		public float[] position;
		
		[ MarshalAs( UnmanagedType.R4 ) ]
		public float u;
		
		[ MarshalAs( UnmanagedType.R4 ) ]
		public float v;
		
		public int INDEX;
		
		[ MarshalAs( UnmanagedType.ByValArray, SizeConst = 16 ) ]
		public char[] id;
		
		[ MarshalAs( UnmanagedType.ByValArray, SizeConst = 16 ) ]
		public char[] owner;		
	}
	
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_RawPrimitive 
	{	
		[ MarshalAs( UnmanagedType.ByValArray, SizeConst = 16 ) ]
		public int[] vertices;
		
		public int vertexCount;		
		public int index;
		
		[ MarshalAs( UnmanagedType.ByValArray, SizeConst = 3 ) ]
		public float[] normal;
		
		[ MarshalAs( UnmanagedType.ByValTStr, SizeConst = 16 ) ]
		public string id;
		
		[ MarshalAs( UnmanagedType.ByValTStr, SizeConst = 16 ) ]
		public string owner;	
	}
	
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_RawInstance 
	{
		public int id;
		
		[ MarshalAs( UnmanagedType.ByValArray, SizeConst = 3 ) ]
		public float[] position;
		
		[ MarshalAs( UnmanagedType.R4) ]
		public float roll, pitch, yaw;
		
		[ MarshalAs( UnmanagedType.ByValArray, SizeConst = 3 ) ]
		public float[] scale;
	}
	
}