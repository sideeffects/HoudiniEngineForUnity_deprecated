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
		public const int HAPI_COLOR_VECTOR_SIZE				= 4;

		public const int HAPI_PRIM_MAX_VERTEX_COUNT			= 16;
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
		HAPI_STATUS_CANT_GENERATE_PRESET   		  		= 8,
    	HAPI_STATUS_CANT_LOAD_PRESET               	    = 9
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
	
	public enum HAPI_AssetType
	{
		HAPI_ASSETTYPE_INVALID = -1,
		HAPI_ASSETTYPE_OBJ = 0,
		HAPI_ASSETTYPE_SOP,
		HAPI_ASSETTYPE_POPNET,
		HAPI_ASSETTYPE_POP,
		HAPI_ASSETTYPE_CHOPNET,
		HAPI_ASSETTYPE_CHOP,
		HAPI_ASSETTYPE_ROP,
		HAPI_ASSETTYPE_SHOP,
		HAPI_ASSETTYPE_COP2,
		HAPI_ASSETTYPE_COPNET,
		HAPI_ASSETTYPE_VOP,
		HAPI_ASSETTYPE_VOPNET,
		HAPI_ASSETTYPE_DOP,
		HAPI_ASSETTYPE_MGR,
		HAPI_ASSETTYPE_DIR,
		HAPI_ASSETTYPE_MAX
	}
	
	public enum HAPI_AssetSubType
	{
		HAPI_ASSETSUBTYPE_INVALID = -1,
		HAPI_ASSETSUBTYPE_DEFAULT,
		HAPI_ASSETSUBTYPE_CURVE,
		HAPI_ASSETSUBTYPE_MAX
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

	public enum HAPI_GeoType
	{
		HAPI_GEOTYPE_DEFAULT = 0,
		HAPI_GEOTYPE_EXPOSED_EDIT,
		HAPI_GEOTYPE_INPUT,
		HAPI_GEOTYPE_MAX
	};
	
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
		public HAPI_TransformEuler( bool initialize_fields )
		{
			position 		= new float[ HAPI_Constants.HAPI_POSITION_VECTOR_SIZE ];
			rotationEuler 	= new float[ HAPI_Constants.HAPI_EULER_VECTOR_SIZE ];
			scale 			= new float[ HAPI_Constants.HAPI_SCALE_VECTOR_SIZE ];
			id 				= 0;
			rotationOrder 	= 0;
			rstOrder 		= 0;
		}
		
	    public int id;
	
	    [ MarshalAs( UnmanagedType.ByValArray, 
					 SizeConst = HAPI_Constants.HAPI_POSITION_VECTOR_SIZE, 
					 ArraySubType = UnmanagedType.R4 ) ]
		public float[] position;
		
		[ MarshalAs( UnmanagedType.ByValArray, 
					 SizeConst = HAPI_Constants.HAPI_EULER_VECTOR_SIZE, 
					 ArraySubType = UnmanagedType.R4 ) ]
		public float[] rotationEuler;			    
		
		
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
		public int type;
		public int subType;
		
		private int nameSH;			// string handle (SH)
		private int labelSH;		// string handle (SH)
		private int instancePathSH;	// string handle (SH)
		private int filePathSH;		// string handle (SH)
		private int texturesPathSH;	// string handle (SH)
		
		public int parmCount;
		public int parmIntValueCount;
		public int parmFloatValueCount;
		public int parmStringValueCount;
		public int parmChoiceCount;
		
		public int objectCount;
		public int handleCount;
		public int materialCount;
		
		public int minInputCount;
		public int maxInputCount;
		public int minGeoInputCount;
		public int maxGeoInputCount;
		
		public int minVerticesPerPrimitive;
		public int maxVerticesPerPrimitive;
		
		// Accessors
		public string name
		{ get { return HAPI_Host.getString( nameSH ); } private set {} }
		public string label
		{ get { return HAPI_Host.getString( labelSH ); } private set {} }
		public string instancePath
		{ get { return HAPI_Host.getString( instancePathSH ); } private set {} }
		public string filePath
		{ get { return HAPI_Host.getString( filePathSH ); } private set {} }
		public string texturesPath
		{ get { return HAPI_Host.getString( texturesPathSH ); } private set {} }
	}
	
	// PARAMETERS ---------------------------------------------------------------------------------------------------
	
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_ParmInfo
	{
		public bool isInt()
		{
			return ( type >= (int) HAPI_ParmType.HAPI_PARMTYPE_INT_START &&
				type <= (int) HAPI_ParmType.HAPI_PARMTYPE_INT_END );
		}
		public bool isFloat()
		{
			return ( type >= (int) HAPI_ParmType.HAPI_PARMTYPE_FLOAT_START &&
				type <= (int) HAPI_ParmType.HAPI_PARMTYPE_FLOAT_END );
		}
		public bool isString()
		{
			return ( type >= (int) HAPI_ParmType.HAPI_PARMTYPE_STR_START &&
				type <= (int) HAPI_ParmType.HAPI_PARMTYPE_STR_END );
		}
		public bool isNonValue()
		{
			return ( type >= (int) HAPI_ParmType.HAPI_PARMTYPE_NONVALUE_START &&
				type <= (int) HAPI_ParmType.HAPI_PARMTYPE_NONVALUE_END );
		}
		
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
		
		public int intValuesIndex;
		public int floatValuesIndex;
		public int stringValuesIndex;
		public int choiceIndex;
		
		// Accessors
		public int getNameSH()
		{ return nameSH; }
		public int getLabelSH()
		{ return labelSH; }
		public string name
		{ get { return HAPI_Host.getString( nameSH ); } private set {} }
		public string label
		{ get { return HAPI_Host.getString( labelSH ); } private set {} }
	}
	
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_ParmChoiceInfo
	{
		public int parentParmId;
		private int labelSH;		// string handle (SH)
		private int valueSH;		// string handle (SH)
		
		// Accessors
		public string label
		{ get { return HAPI_Host.getString( labelSH ); } private set {} }
		public string value
		{ get { return HAPI_Host.getString( valueSH ); } private set {} }
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
		private int assetParmNameSH;	// string handle (SH)
		
		public int 	assetParmId;
		
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
		public bool haveGeosChanged;
		
		[ MarshalAs( UnmanagedType.U1 ) ]
		public bool isVisible;
		
		[ MarshalAs( UnmanagedType.U1 ) ]
		public bool isInstancer;

		public int geoCount;

    	public int objectToInstanceId;
		
		// Accessors
		public string name
		{ get { return HAPI_Host.getString( nameSH ); } private set {} }
		public string objectInstancePath
		{ get { return HAPI_Host.getString( objectInstancePathSH ); } private set {} }

	}
	
	// GEOMETRY -----------------------------------------------------------------------------------------------------
	
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_GeoInfo
	{
		public int		id;
		public int		type;
		
		[ MarshalAs( UnmanagedType.U1 ) ]
		public bool		hasGeoChanged;
		
		[ MarshalAs( UnmanagedType.U1 ) ]
		public bool		hasMaterialChanged;

		public int		partCount;
	}

	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_PartInfo
	{
		public int getOwnerCount( HAPI_AttributeOwner owner )
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

		public int		id;

		public int 		materialId;
		
		public int		faceCount;
		public int		vertexCount;
		public int		pointCount;
		public int		pointAttributeCount;
		public int		faceAttributeCount;
		public int		vertexAttributeCount;
		public int		detailAttributeCount;
	}
	
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_AttributeInfo
	{		
		public HAPI_AttributeInfo( string attr_name )
		{
			exists 		= false;
			owner 		= (int) HAPI_AttributeOwner.HAPI_ATTROWNER_INVALID;
			storage 	= (int) HAPI_StorageType.HAPI_STORAGETYPE_INVALID;
			count 		= 0;
			tupleSize 	= 0;
		}
		
		[ MarshalAs( UnmanagedType.U1 ) ]
		public bool		exists;
		
		public int		owner;  // enum HAPI_AttributeOwner
		public int		storage; // enum HAPI_StorageType
		public int		count;
		public int		tupleSize;
	}
	
	// MATERIALS ----------------------------------------------------------------------------------------------------
	
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_MaterialInfo
	{
		public int id;
		
		private int textureFilePathSH;	// string handle (SH)
		
		public float roughness; // inverse of shininess
		
		[ MarshalAs( UnmanagedType.ByValArray, 
					 SizeConst = HAPI_Constants.HAPI_COLOR_VECTOR_SIZE, 
					 ArraySubType = UnmanagedType.R4 ) ]
	    public float[] ambient;
		
		[ MarshalAs( UnmanagedType.ByValArray, 
					 SizeConst = HAPI_Constants.HAPI_COLOR_VECTOR_SIZE, 
					 ArraySubType = UnmanagedType.R4 ) ]
	    public float[] diffuse;
		
		[ MarshalAs( UnmanagedType.ByValArray, 
					 SizeConst = HAPI_Constants.HAPI_COLOR_VECTOR_SIZE, 
					 ArraySubType = UnmanagedType.R4 ) ]
	    public float[] specular;
		
		// Accessors
		public string textureFilePath
		{ get { return HAPI_Host.getString( textureFilePathSH ); } private set {} }
	}
	
}
