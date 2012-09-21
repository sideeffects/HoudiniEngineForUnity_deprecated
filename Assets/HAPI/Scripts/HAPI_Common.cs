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
 * 		Contains main HAPI API constants and structures.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

namespace HAPI 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Defines
	
	public struct HAPI_Constants
	{
		// Unity-only constants.
		public const int HAPI_MAX_PAGE_SIZE					= 32768;
		public const int HAPI_SEC_BEFORE_PROGRESS_BAR_SHOW	= 5;
		public const int HAPI_MIN_VERTICES_PER_FACE			= 3;
		public const int HAPI_MAX_VERTICES_PER_FACE			= 3;
		
		public const int HAPI_POSITION_VECTOR_SIZE			= 3;
		public const int HAPI_SCALE_VECTOR_SIZE				= 3;
		public const int HAPI_NORMAL_VECTOR_SIZE			= 3;
		public const int HAPI_QUATERNION_VECTOR_SIZE		= 4;
		public const int HAPI_EULER_VECTOR_SIZE				= 3;		
		public const int HAPI_UV_VECTOR_SIZE				= 2;
		
		public const int HAPI_GENERIC_MAX_NAME_SIZE			= 256;
		
		public const int HAPI_ASSET_MAX_FILE_PATH_SIZE		= 1024;
		public const int HAPI_ASSET_MAX_NAME_SIZE			= 256;
		public const int HAPI_ASSET_MAX_INSTANCE_PATH_SIZE	= 256;

		public const int HAPI_PARM_MAX_VECTOR_SIZE			= 4;
		public const int HAPI_PARM_MAX_NAME_SIZE			= 256;
		public const int HAPI_PARM_MAX_STRING_SIZE			= 8192;
		public const int HAPI_PARM_MAX_STRING_VEC_SIZE		= 1;

		public const int HAPI_OBJ_MAX_NAME_SIZE				= 512;
		public const int HAPI_OBJ_MAX_PATH_SIZE				= 1024;

		public const int HAPI_GEO_MAX_NAME_SIZE				= 512;
		public const int HAPI_GEO_MAX_TEX_NAME_SIZE			= 1024;
		public const int HAPI_GEO_MAX_BUMP_NAME_SIZE		= 1024;

		public const int HAPI_PRIM_MAX_VERTEX_COUNT			= 16;
		
		public const int HAPI_ATTR_MAX_NAME_SIZE			= 64;
		public const int HAPI_ATTR_MAX_STRING_SIZE			= 128;
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Enums
	
	public enum HAPI_StatusCode 
	{
		HAPI_STATUS_SUCCESS				    			= 0,
		HAPI_STATUS_FAILURE				    			= 1,
		HAPI_STATUS_ALREADY_INITIALIZED		    		= 2,
		HAPI_STATUS_NOT_INITIALIZED			    		= 3,
		HAPI_STATUS_CANT_LOADFILE			    		= 4,
		HAPI_STATUS_PARM_SET_FAILED			    		= 5,
		HAPI_STATUS_INVALID_ARGUMENT		    		= 6,
		HAPI_STATUS_CANT_LOAD_GEO						= 7
	};
	
	public enum HAPI_ParmType
	{
		HAPI_PARMTYPE_INT = 0,
		HAPI_PARMTYPE_TOGGLE,
		
		HAPI_PARMTYPE_FLOAT,
		HAPI_PARMTYPE_COLOUR,
		
		HAPI_PARMTYPE_STRING,
		HAPI_PARMTYPE_FILE,
		
		HAPI_PARMTYPE_FOLDERLIST,
		HAPI_PARMTYPE_FOLDER,
		HAPI_PARMTYPE_SEPARATOR,
		
		// Helpers
		
		HAPI_PRMTYPE_MAX, // Total number of supported parameter types.
		
		HAPI_PARMTYPE_INT_START			= HAPI_PARMTYPE_INT,
		HAPI_PARMTYPE_INT_END			= HAPI_PARMTYPE_TOGGLE,
		
		HAPI_PARMTYPE_FLOAT_START		= HAPI_PARMTYPE_FLOAT,
		HAPI_PARMTYPE_FLOAT_END			= HAPI_PARMTYPE_COLOUR,
		
		HAPI_PARMTYPE_STR_START	    	= HAPI_PARMTYPE_STRING,
		HAPI_PARMTYPE_STR_END	    	= HAPI_PARMTYPE_FILE,
		
		HAPI_PARMTYPE_NONVALUE_START	= HAPI_PARMTYPE_FOLDERLIST,
		HAPI_PARMTYPE_NONVALUE_END		= HAPI_PARMTYPE_SEPARATOR
	}
	
	public enum HAPI_RSTOrder
	{
		TRS = 0, 
		TSR, 
		RTS, 
		RST, 
		STR,
		SRT
	}
	
	public enum HAPI_XYZOrder
	{
		XYZ = 0, 
		XZY, 
		YXZ, 
		YZX, 
		ZXY, 
		ZYX
	}
	
	public enum HAPI_AttributeOwner
	{
		HAPI_ATTROWNER_INVALID = -1,
		HAPI_ATTROWNER_VERTEX,
	    HAPI_ATTROWNER_POINT,
	    HAPI_ATTROWNER_PRIM,
		HAPI_ATTROWNER_DETAIL,
	    HAPI_ATTROWNER_MAX
	}
	
	public enum HAPI_StorageType
	{
		HAPI_STORAGETYPE_INVALID = -1,
		HAPI_STORAGETYPE_INT,
		HAPI_STORAGETYPE_FLOAT,
		HAPI_STORAGETYPE_STR,
		HAPI_STORAGETYPE_MAX
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Main API Structs
	
	// GENERICS -----------------------------------------------------------------------------------------------------
	
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_Transform 
	{
		public int id;
		
		[ MarshalAs( UnmanagedType.ByValArray, 
					 SizeConst = HAPI_Constants.HAPI_POSITION_VECTOR_SIZE, 
					 ArraySubType = UnmanagedType.R4 ) ]
		public float[] position;
				
		[ MarshalAs( UnmanagedType.ByValArray, 
					 SizeConst = HAPI_Constants.HAPI_QUATERNION_VECTOR_SIZE, 
					 ArraySubType = UnmanagedType.R4 ) ]
		public float[] rotationQuaternion;
		
		[ MarshalAs( UnmanagedType.ByValArray, 
					 SizeConst = HAPI_Constants.HAPI_SCALE_VECTOR_SIZE, 
					 ArraySubType = UnmanagedType.R4 ) ]
		public float[] scale;
	}
	
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_TransformEuler 
	{
		public HAPI_TransformEuler(bool initializeFields)
		{
			position = new float[HAPI_Constants.HAPI_POSITION_VECTOR_SIZE];
			rotationeEuler = new float[HAPI_Constants.HAPI_EULER_VECTOR_SIZE];
			scale = new float[HAPI_Constants.HAPI_SCALE_VECTOR_SIZE];
			id = 0;
			rotationOrder = 0;
			rstOrder = 0;
		}
		
	    public int id;
	
	    [ MarshalAs( UnmanagedType.ByValArray, 
					 SizeConst = HAPI_Constants.HAPI_POSITION_VECTOR_SIZE, 
					 ArraySubType = UnmanagedType.R4 ) ]
		public float[] position;
		
		[ MarshalAs( UnmanagedType.ByValArray, 
					 SizeConst = HAPI_Constants.HAPI_EULER_VECTOR_SIZE, 
					 ArraySubType = UnmanagedType.R4 ) ]
		public float[] rotationeEuler;			    
		
		
	    [ MarshalAs( UnmanagedType.ByValArray, 
					 SizeConst = HAPI_Constants.HAPI_SCALE_VECTOR_SIZE, 
					 ArraySubType = UnmanagedType.R4 ) ]
		public float[] scale;
	
	    public int     rotationOrder;
	    public int     rstOrder;
	}
	
	// ASSETS -------------------------------------------------------------------------------------------------------
	
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_AssetInfo 
	{
		public int id;
		
		public int nameSH;			// string handle (SH)
		public int instancePathSH;	// string handle (SH)
		public int filePathSH;		// string handle (SH)
		
		[ MarshalAs( UnmanagedType.ByValTStr, 
					 SizeConst = HAPI_Constants.HAPI_ASSET_MAX_FILE_PATH_SIZE ) ]
		public string otlFilePath;
		
		[ MarshalAs( UnmanagedType.ByValTStr, 
					 SizeConst = HAPI_Constants.HAPI_ASSET_MAX_NAME_SIZE ) ]		
		public string assetName;
		
		[ MarshalAs( UnmanagedType.ByValTStr, 
					 SizeConst = HAPI_Constants.HAPI_ASSET_MAX_INSTANCE_PATH_SIZE ) ]		
		public string assetInstancePath;
		
		public int objectCount;
		public int parmCount;
		public int handleCount;
		public int parmExtraValueCount;
		public int parmChoiceCount;
		
		public int minVerticesPerPrimitive;
		public int maxVerticesPerPrimitive;
	}
	
	// PARAMETERS ---------------------------------------------------------------------------------------------------
	
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_ParmSingleValue
	{
		public int parentParmId;
		public int vectorIndex;
		
		public int intValue;
		
		[ MarshalAs( UnmanagedType.R4 ) ]
		public float floatValue;
		
		[ MarshalAs( UnmanagedType.ByValTStr, 
					 SizeConst = HAPI_Constants.HAPI_PARM_MAX_STRING_SIZE ) ]		
		public string stringValue;	
	}
	
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_ParmInfo
	{
		public int id;
		public int parentId;
		
		public int type;
		public int size;
		public int choiceCount;
		
		[ MarshalAs( UnmanagedType.U1 ) ]
		public bool hasMin;
		
		[ MarshalAs( UnmanagedType.U1 ) ]
		public bool hasMax;
		
		[ MarshalAs( UnmanagedType.U1 ) ]
		public bool hasUIMin;
		
		[ MarshalAs( UnmanagedType.U1 ) ]
		public bool hasUIMax;
		
		[ MarshalAs( UnmanagedType.R4) ]
		public float min;
		
		[ MarshalAs( UnmanagedType.R4) ]
		public float max;
		
		[ MarshalAs( UnmanagedType.R4) ]
		public float UIMin;
		
		[ MarshalAs( UnmanagedType.R4) ]
		public float UIMax;
		
		[ MarshalAs( UnmanagedType.U1 ) ]
		public bool invisible;
		
		[ MarshalAs( UnmanagedType.U1 ) ]
		public bool joinNext;
		
		[ MarshalAs( UnmanagedType.U1 ) ]
		public bool labelNone;
		
		[ MarshalAs( UnmanagedType.ByValTStr, 
					 SizeConst = HAPI_Constants.HAPI_PARM_MAX_NAME_SIZE ) ]
		public string name;
		
		[ MarshalAs( UnmanagedType.ByValTStr, 
					 SizeConst = HAPI_Constants.HAPI_PARM_MAX_NAME_SIZE ) ]		
		public string label;		
		
		[ MarshalAs( UnmanagedType.ByValArray, 
					 SizeConst = HAPI_Constants.HAPI_PARM_MAX_VECTOR_SIZE ) ]
		public int[] intValue;
		
		[ MarshalAs( UnmanagedType.ByValArray, 
					 SizeConst = HAPI_Constants.HAPI_PARM_MAX_VECTOR_SIZE, 
					 ArraySubType = UnmanagedType.R4 ) ]
		public float[] floatValue;
		
		[ MarshalAs( UnmanagedType.ByValTStr, 
					 SizeConst = HAPI_Constants.HAPI_PARM_MAX_STRING_SIZE ) ]		
		public string stringValue;	
	}
	
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_ParmChoiceInfo
	{
		public int parentParmId;
		
		[ MarshalAs( UnmanagedType.ByValTStr, 
					 SizeConst = HAPI_Constants.HAPI_PARM_MAX_NAME_SIZE ) ]		
		public string label;
		
		[ MarshalAs( UnmanagedType.ByValTStr, 
					 SizeConst = HAPI_Constants.HAPI_PARM_MAX_STRING_SIZE ) ]		
		public string value;
	}
	
	// HANDLES ------------------------------------------------------------------------------------------------------
	
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_HandleInfo
	{
		public int nameSH;		// string handle (SH)
		public int typeNameSH;	// string handle (SH)
		
		[ MarshalAs( UnmanagedType.ByValTStr, 
					 SizeConst = HAPI_Constants.HAPI_GENERIC_MAX_NAME_SIZE ) ]		
	    public string handleTypeName;
		
		[ MarshalAs( UnmanagedType.ByValTStr, 
					 SizeConst = HAPI_Constants.HAPI_GENERIC_MAX_NAME_SIZE ) ]		
	    public string handleName;
		
	    public int  bindingsCount;
	}

	
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_HandleBindingInfo
	{	    
		public int handleParmNameSH;	// string handle (SH)
		public int assetParmNameSH;		// string handle (SH)
		
		[ MarshalAs( UnmanagedType.ByValTStr, 
					 SizeConst = HAPI_Constants.HAPI_GENERIC_MAX_NAME_SIZE ) ]	
	    public string handleParmName;
		
		[ MarshalAs( UnmanagedType.ByValTStr, 
					 SizeConst = HAPI_Constants.HAPI_GENERIC_MAX_NAME_SIZE ) ]
	    public string assetParmName;
		
		public int    assetParmId;
	};
	
	// OBJECTS ------------------------------------------------------------------------------------------------------
	
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_ObjectInfo 
	{
		public int id;
		
		public int nameSH; 					// string handle (SH)
		public int objectInstancePathSH;	// string handle (SH)
		
		[ MarshalAs( UnmanagedType.ByValTStr, 
					 SizeConst = HAPI_Constants.HAPI_OBJ_MAX_NAME_SIZE ) ]
		public string name;
		
		[ MarshalAs( UnmanagedType.ByValTStr, 
					 SizeConst = HAPI_Constants.HAPI_OBJ_MAX_PATH_SIZE ) ]
		public string objectInstancePath;
		
		[ MarshalAs( UnmanagedType.U1 ) ]
		public bool hasTransformChanged;
		
		[ MarshalAs( UnmanagedType.U1 ) ]
		public bool hasGeoChanged;
	}
	
	// DETAILS ------------------------------------------------------------------------------------------------------
	
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_DetailInfo
	{
		public int		getOwnerCount( HAPI_AttributeOwner owner )
		{
			switch ( owner )
			{
				case HAPI_AttributeOwner.HAPI_ATTROWNER_VERTEX: return vertexAttributeCount;
				case HAPI_AttributeOwner.HAPI_ATTROWNER_POINT: return pointAttributeCount;
				case HAPI_AttributeOwner.HAPI_ATTROWNER_PRIM: return faceAttributeCount;
				case HAPI_AttributeOwner.HAPI_ATTROWNER_DETAIL: return detailAttributeCount;
				default: return 0;
			}
		}
		
		public int	    faceCount;
		public int	    vertexCount;
		public int		pointCount;
		public int	    pointAttributeCount;
		public int	    faceAttributeCount;
		public int	    vertexAttributeCount;
		public int	    detailAttributeCount;
	}
	
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_AttributeInfo
	{		
		public HAPI_AttributeInfo( string attr_name )
		{
			nameSH		= 0;
			name 		= attr_name;
			exists 		= false;
			owner 		= (int) HAPI_AttributeOwner.HAPI_ATTROWNER_INVALID;
			storage 	= (int) HAPI_StorageType.HAPI_STORAGETYPE_INVALID;
			count 		= 0;
			tupleSize 	= 0;
		}
		
		public int		nameSH;	// string handle (SH)
		
		[ MarshalAs( UnmanagedType.ByValTStr, 
					 SizeConst = HAPI_Constants.HAPI_ATTR_MAX_NAME_SIZE ) ]
		public string	name;
		
		[ MarshalAs( UnmanagedType.U1 ) ]
		public bool		exists;
		
		public int		owner;  // enum HAPI_AttributeOwner
		public int		storage; // enum HAPI_StorageType
		public int		count;
		public int		tupleSize;
	}
	
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_AttributeStrValue
	{
		[ MarshalAs( UnmanagedType.ByValTStr, 
					 SizeConst = HAPI_Constants.HAPI_ATTR_MAX_STRING_SIZE ) ]
		public string	value;
	}
	
}