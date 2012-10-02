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

		public const int HAPI_PARM_MAX_VECTOR_SIZE			= 4;
		public const int HAPI_PARM_MAX_NAME_SIZE			= 256;
		public const int HAPI_PARM_MAX_STRING_SIZE			= 8192;
		public const int HAPI_PARM_MAX_STRING_VEC_SIZE		= 1;

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
		HAPI_STATUS_CANT_LOAD_GEO						= 7,
		HAPI_STATUS_INSUFFICIENT_BUFFER_SIZE   		    = 8,
    	HAPI_STATUS_CANT_GENERATE_PRESET               	= 9
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
	
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_TransformInstance 
	{
		public HAPI_TransformInstance(bool initializeFields)
		{
			
			pos = new float[HAPI_Constants.HAPI_POSITION_VECTOR_SIZE];
			dir = new float[HAPI_Constants.HAPI_POSITION_VECTOR_SIZE];
			scale = 1.0f;
			scale3 = new float[HAPI_Constants.HAPI_SCALE_VECTOR_SIZE];
			up = new float[HAPI_Constants.HAPI_POSITION_VECTOR_SIZE];
			quat = new float[HAPI_Constants.HAPI_QUATERNION_VECTOR_SIZE];
			tr = new float[HAPI_Constants.HAPI_POSITION_VECTOR_SIZE];
		}
		
		[ MarshalAs( UnmanagedType.ByValArray, 
					 SizeConst = HAPI_Constants.HAPI_POSITION_VECTOR_SIZE, 
					 ArraySubType = UnmanagedType.R4 ) ]
		public float[] pos;
		
		[ MarshalAs( UnmanagedType.ByValArray, 
					 SizeConst = HAPI_Constants.HAPI_POSITION_VECTOR_SIZE, 
					 ArraySubType = UnmanagedType.R4 ) ]
	    public float[] dir;
		
	    public float scale;
		
		[ MarshalAs( UnmanagedType.ByValArray, 
					 SizeConst = HAPI_Constants.HAPI_SCALE_VECTOR_SIZE, 
					 ArraySubType = UnmanagedType.R4 ) ]
	    public float[] scale3;
		
		[ MarshalAs( UnmanagedType.ByValArray, 
					 SizeConst = HAPI_Constants.HAPI_POSITION_VECTOR_SIZE, 
					 ArraySubType = UnmanagedType.R4 ) ]
	    public float[] up;
		
		[ MarshalAs( UnmanagedType.ByValArray, 
					 SizeConst = HAPI_Constants.HAPI_QUATERNION_VECTOR_SIZE, 
					 ArraySubType = UnmanagedType.R4 ) ]
	    public float[] quat;
		
		[ MarshalAs( UnmanagedType.ByValArray, 
					 SizeConst = HAPI_Constants.HAPI_POSITION_VECTOR_SIZE, 
					 ArraySubType = UnmanagedType.R4 ) ]
	    public float[] tr;
		
	}
	
	
	// ASSETS -------------------------------------------------------------------------------------------------------
		
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_AssetInfo 
	{
		public int id;
		
		private int nameSH;			// string handle (SH)
		private int instancePathSH;	// string handle (SH)
		private int filePathSH;		// string handle (SH)
		
		public int objectCount;
		public int parmCount;
		public int handleCount;
		public int parmExtraValueCount;
		public int parmChoiceCount;
		public int instancerCount;
		
		public int minVerticesPerPrimitive;
		public int maxVerticesPerPrimitive;
		
		// Accessors
		public string name
		{ get { return HAPI_Host.getString( nameSH ); } private set {} }
		public string instancePath
		{ get { return HAPI_Host.getString( instancePathSH ); } private set {} }
		public string filePath
		{ get { return HAPI_Host.getString( filePathSH ); } private set {} }
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
		
		private int nameSH;
		private int labelSH;
		
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
		
		public int choiceIndex;
		
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
		
		// Accessors
		public string name
		{ get { return HAPI_Host.getString( nameSH ); } private set {} }
		public string label
		{ get { return HAPI_Host.getString( labelSH ); } private set {} }
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
		private int nameSH;		// string handle (SH)
		private int typeNameSH;	// string handle (SH)
		
	    public int  bindingsCount;
		
		// Accessors
		public string name
		{ get { return HAPI_Host.getString( nameSH ); } private set {} }
		public string typeName
		{ get { return HAPI_Host.getString( typeNameSH ); } private set {} }
	}

	
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_HandleBindingInfo
	{	    
		private int handleParmNameSH;	// string handle (SH)
		private int assetParmNameSH;		// string handle (SH)
		
		public int    assetParmId;
		
		// Accessors
		public string handleParmName
		{ get { return HAPI_Host.getString( handleParmNameSH ); } private set {} }
		public string assetParmName
		{ get { return HAPI_Host.getString( assetParmNameSH ); } private set {} }
	};
	
	// OBJECTS ------------------------------------------------------------------------------------------------------
	
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_ObjectInfo 
	{
		public int id;
		
		private int nameSH; 				// string handle (SH)		
		private int objectInstancePathSH; 	// string handle (SH)
		
		[ MarshalAs( UnmanagedType.U1 ) ]
		public bool hasTransformChanged;
		
		[ MarshalAs( UnmanagedType.U1 ) ]
		public bool hasGeoChanged;
		
		[ MarshalAs( UnmanagedType.U1 ) ]
		public bool isInstancer;
				
    	public int objectToInstanceId;
		
		// Accessors
		public string name
		{ get { return HAPI_Host.getString( nameSH ); } private set {} }
		public string objectInstancePath
		{ get { return HAPI_Host.getString( objectInstancePathSH ); } private set {} }

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