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
		// Unity-Only Constants ---------------------------------------------

		public const string HAPI_PRODUCT_NAME				= "Houdini Engine";
		
		// Used for things like window titles that have limited space.
		public const string HAPI_PRODUCT_SHORT_NAME			= "Houdini";

		public static string HAPI_TEXTURES_PATH				= Application.dataPath + "/Textures";

		public const int HAPI_MAX_PAGE_SIZE					= 32768;
		public const int HAPI_SEC_BEFORE_PROGRESS_BAR_SHOW	= 3;
		public const int HAPI_MIN_VERTICES_PER_FACE			= 3;
		public const int HAPI_MAX_VERTICES_PER_FACE			= 3;

		// Default Attributes' Names
		public const string HAPI_ATTRIB_POSITION			= "P";
		public const string HAPI_ATTRIB_UV					= "uv";
		public const string HAPI_ATTRIB_NORMAL				= "N";
		public const string HAPI_ATTRIB_TANGENT				= "tangentu";
		public const string HAPI_ATTRIB_COLOUR				= "Cd";

		// Shared Constants -------------------------------------------------

		public const int HAPI_POSITION_VECTOR_SIZE			= 3;
		public const int HAPI_SCALE_VECTOR_SIZE				= 3;
		public const int HAPI_NORMAL_VECTOR_SIZE			= 3;
		public const int HAPI_QUATERNION_VECTOR_SIZE		= 4;
		public const int HAPI_EULER_VECTOR_SIZE				= 3;
		public const int HAPI_UV_VECTOR_SIZE				= 2;
		public const int HAPI_COLOR_VECTOR_SIZE				= 4;

		public const int HAPI_PRIM_MAX_VERTEX_COUNT			= 16;

		public const int HAPI_INVALID_PARM_ID 				= -1;
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Enums
	
	public enum HAPI_StatusType
	{
		HAPI_STATUS_RESULT,
		HAPI_STATUS_WARNING,
		HAPI_STATUS_STATE,
		HAPI_STATUS_MAX
	};

	public enum HAPI_Result 
	{
		HAPI_RESULT_SUCCESS						= 0,
		HAPI_RESULT_FAILURE						= 1,
		HAPI_RESULT_ALREADY_INITIALIZED			= 2,
		HAPI_RESULT_NOT_INITIALIZED				= 3,
		HAPI_RESULT_CANT_LOADFILE				= 4,
		HAPI_RESULT_PARM_SET_FAILED				= 5,
		HAPI_RESULT_INVALID_ARGUMENT			= 6,
		HAPI_RESULT_CANT_LOAD_GEO				= 7,
		HAPI_RESULT_CANT_GENERATE_PRESET		= 8,
		HAPI_RESULT_CANT_LOAD_PRESET			= 9
	};

	public enum HAPI_State
	{
		HAPI_STATE_READY,
		HAPI_STATE_READY_WITH_ERRORS,
		HAPI_STATE_STARTING_COOK,
		HAPI_STATE_COOKING,
		HAPI_STATE_STARTING_LOAD,
		HAPI_STATE_LOADING,
		HAPI_STATE_MAX
	};

	public enum HAPI_RampType
	{
		HAPI_RAMPTYPE_FLOAT = 0,
		HAPI_RAMPTYPE_COLOR,
		HAPI_RAMPTYPE_MAX
	};
	
	public enum HAPI_ParmType
	{
		HAPI_PARMTYPE_INT = 0,
    	HAPI_PARMTYPE_MULTIPARMLIST,
		HAPI_PARMTYPE_TOGGLE,
		HAPI_PARMTYPE_BUTTON,
		
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
		HAPI_PARMTYPE_INT_END			= HAPI_PARMTYPE_BUTTON,
		
		HAPI_PARMTYPE_FLOAT_START		= HAPI_PARMTYPE_FLOAT,
		HAPI_PARMTYPE_FLOAT_END			= HAPI_PARMTYPE_COLOUR,
		
		HAPI_PARMTYPE_STR_START	    	= HAPI_PARMTYPE_STRING,
		HAPI_PARMTYPE_STR_END	    	= HAPI_PARMTYPE_FILE,

		HAPI_PARMTYPE_CONTAINER_START 	= HAPI_PARMTYPE_FOLDERLIST,
		HAPI_PARMTYPE_CONTAINER_END 	= HAPI_PARMTYPE_FOLDERLIST,
		
		HAPI_PARMTYPE_NONVALUE_START	= HAPI_PARMTYPE_FOLDER,
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
	
	public enum HAPI_InputType
	{
		HAPI_INPUT_INVALID = -1,
		HAPI_INPUT_TRANSFORM,
		HAPI_INPUT_GEOMETRY,
		HAPI_INPUT_MAX
	};
	
	public enum HAPI_TransformComponentType
	{
		HAPI_TRANSFORM_TX,
		HAPI_TRANSFORM_TY,
		HAPI_TRANSFORM_TZ,
		HAPI_TRANSFORM_RX,
		HAPI_TRANSFORM_RY,
		HAPI_TRANSFORM_RZ,
		HAPI_TRANSFORM_QX,
		HAPI_TRANSFORM_QY,
		HAPI_TRANSFORM_QZ,
		HAPI_TRANSFORM_QW,
		HAPI_TRANSFORM_SX,
		HAPI_TRANSFORM_SY,
		HAPI_TRANSFORM_SZ
	};

	public enum HAPI_ShaderType
	{
		HAPI_SHADER_INVALID = -1,
		HAPI_SHADER_OPENGL,
		HAPI_SHADER_MANTRA,
		HAPI_SHADER_MAX
	};

	public enum HAPI_ImageDataFormat
	{
		HAPI_IMAGE_DATA_UNKNOWN = -1,
		HAPI_IMAGE_DATA_INT8,
		HAPI_IMAGE_DATA_INT16,
		HAPI_IMAGE_DATA_INT32,
		HAPI_IMAGE_DATA_FLOAT16,
		HAPI_IMAGE_DATA_FLOAT32,
		HAPI_IMAGE_DATA_MAX
	};

	public enum HAPI_ImagePacking
	{
		HAPI_IMAGE_PACKING_UNKNOWN = -1,
		HAPI_IMAGE_PACKING_SINGLE,	// Single Channel
		HAPI_IMAGE_PACKING_DUAL,	// 2 channel interleaved + 1 black channel. (12B12B).
		HAPI_IMAGE_PACKING_DUAL_NI,	// 2 channel, non-interleaved (11111....12222222...2)
		HAPI_IMAGE_PACKING_RGB,		// RGB interleaved (rgbrgb...rgb)
		HAPI_IMAGE_PACKING_RGBA,	// RGBA interleaved 
		HAPI_IMAGE_PACKING_RGB_NI,	// RGB non-interleaved (rrr...rggg...gbbb..b)
		HAPI_IMAGE_PACKING_RGBA_NI,	// RGBA non-interleaved
		HAPI_IMAGE_PACKING_MAX
	};

	public enum HAPI_ImageFileFormat // Supported export formats.
	{
		HAPI_IMAGE_FILE_FORMAT_UNKNOWN = -1,
		HAPI_IMAGE_FILE_FORMAT_RAW, // Custom RAW format.
		HAPI_IMAGE_FILE_FORMAT_PIC, // Houdini .pic file format.
		HAPI_IMAGE_FILE_FORMAT_PNG, // PNG file format.
		HAPI_IMAGE_FILE_FORMAT_JPG, // JPEG file format.
		HAPI_IMAGE_FILE_FORMAT_BMP, // Bitmap file format.
		HAPI_IMAGE_FILE_FORMAT_TIF, // TIFF file format.
		HAPI_IMAGE_FILE_FORMAT_TGA, // Targa file format.
		HAPI_IMAGE_FILE_FORMAT_MAX,

		HAPI_IMAGE_FILE_FORMAT_DEFAULT = HAPI_IMAGE_FILE_FORMAT_PNG
	};

	public enum HAPI_EnvIntType
	{
		HAPI_ENVINT_INVALID = -1,
		
		// The three components of the Houdini version that HAPI is
		// expecting to compile against.
		HAPI_ENVINT_VERSION_HOUDINI_MAJOR,
		HAPI_ENVINT_VERSION_HOUDINI_MINOR,
		HAPI_ENVINT_VERSION_HOUDINI_BUILD,
		
		// The three components of the Houdini version that HAPI belongs to.
		// The HAPI library itself can come from a different baseline than
		// the Houdini it is being compiled against when we do something like 
		// "backgrafting" where we take, say, a 13.0 (1.5) HAPI and ship it
		// with a Houdini 12.5. This version is always locked with the
		// actual Houdini Engine version (below) because Houdini Engine is in
		// the same baseline as Houdini so their releases always coincide.
		HAPI_ENVINT_VERSION_ORIG_HOUDINI_MAJOR,
		HAPI_ENVINT_VERSION_ORIG_HOUDINI_MINOR,
		HAPI_ENVINT_VERSION_ORIG_HOUDINI_BUILD,
		
		// The two components of the Houdini Engine (marketed) version.
		HAPI_ENVINT_VERSION_HOUDINI_ENGINE_MAJOR,
		HAPI_ENVINT_VERSION_HOUDINI_ENGINE_MINOR,
		
		// This is a monotonously increasing API version number that can be used
		// to lock against a certain API for compatibility purposes. Basically,
		// when this number changes code compiled against the HAPI.h methods
		// might no longer compile. Semantic changes to the methods will also
		// cause this version to increase. This number will be reset to 0
		// every time the Houdini Engine version is bumped.
		HAPI_ENVINT_VERSION_HOUDINI_ENGINE_API,
		
		HAPI_ENVINT_MAX,
	};

	// Unity-Only:

	public enum HAPI_GeoInputFormat
	{
		HAPI_GEO_INPUT_FORMAT_INVALID = -1,
		HAPI_GEO_INPUT_FORMAT_OBJECT,
		HAPI_GEO_INPUT_FORMAT_FILE,
		HAPI_GEO_INPUT_FORMAT_MAX,

		HAPI_GEO_INPUT_FORMAT_DEFAULT = HAPI_GEO_INPUT_FORMAT_OBJECT
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
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Main API Structs
	
	// GENERICS -----------------------------------------------------------------------------------------------------
	
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_Transform 
	{
		public HAPI_Transform( int id_in )
		{
			id					= id_in;
			position			= new float[ HAPI_Constants.HAPI_POSITION_VECTOR_SIZE ];
			rotationQuaternion	= new float[ HAPI_Constants.HAPI_QUATERNION_VECTOR_SIZE ];
			scale				= new float[ HAPI_Constants.HAPI_SCALE_VECTOR_SIZE ];
			rstOrder			= (int) HAPI_RSTOrder.SRT;
		}

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

		public int rstOrder;
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

		public int rotationOrder;
		public int rstOrder;
	}
	
	// ASSETS -------------------------------------------------------------------------------------------------------
		
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_AssetInfo 
	{
		public int id;
		public int type;
		public int subType;

		// This id is primarily used to check whether the asset still exists
		// within the Houdini scene running inside the runtime. The asset id
		// alone is not enough as asset ids are re-used between sessions.
		// We use this id to determine whether we need to re-instatiate an asset
		// we have on the client side so that Houdini also knows about it -
		// which is different from the case where a new asset is being loaded
		// for the first time.
		public int validationId;

		// Use the node id to get the asset's parameters.
		public int nodeId;
		
		private int nameSH;			// string handle (SH)
		private int labelSH;		// string handle (SH)
		private int instancePathSH;	// string handle (SH)
		private int filePathSH;		// string handle (SH)

		private int versionSH;		// string handle (SH)
		private int fullOpNameSH;	// full operator name with namespace (SH)
		
		public int objectCount;
		public int handleCount;
		public int materialCount;
		
		public int minTransInputCount;
		public int maxTransInputCount;
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
		public string version
		{ get { return HAPI_Host.getString( versionSH ); } private set {} }
		public string fullOpName
		{ get { return HAPI_Host.getString( fullOpNameSH ); } private set {} }
	}
	
	// NODES --------------------------------------------------------------------------------------------------------

	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_NodeInfo 
	{
		public int id;
		
		public int parmCount;
		public int parmIntValueCount;
		public int parmFloatValueCount;
		public int parmStringValueCount;
		public int parmChoiceCount;
	}

	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_GlobalNodes 
	{
		public int defaultCamera;
		public int defaultLight;
		public int mantraRenderer;
	}

	// PARAMETERS ---------------------------------------------------------------------------------------------------
	
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_ParmInfo
	{
		public bool isInt()
		{
			return ( type >= (int) HAPI_ParmType.HAPI_PARMTYPE_INT_START &&
				type <= (int) HAPI_ParmType.HAPI_PARMTYPE_INT_END )
				|| type == (int) HAPI_ParmType.HAPI_PARMTYPE_MULTIPARMLIST;
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
		
		[ MarshalAs( UnmanagedType.U1 ) ]
		public bool isChildOfMultiParm;
		public int instanceNum;

		public int instanceLength;
		public int instanceCount;

		public int rampType;
		
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
		private int		nameSH; // string handle (SH)
		
		[ MarshalAs( UnmanagedType.U1 ) ]
		public bool		hasGeoChanged;
		
		[ MarshalAs( UnmanagedType.U1 ) ]
		public bool		hasMaterialChanged;

		public int		partCount;

		// Accessors
		public string name
		{ get { return HAPI_Host.getString( nameSH ); } private set {} }
	}
	
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_GeoInputInfo
	{				
		public int		objectId;
		public int		geoId;
		public int		objectNodeId;
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
		private int		nameSH; // string handle (SH)

		public int 		materialId;
		
		public int		faceCount;
		public int		vertexCount;
		public int		pointCount;
		public int		pointAttributeCount;
		public int		faceAttributeCount;
		public int		vertexAttributeCount;
		public int		detailAttributeCount;

		[ MarshalAs( UnmanagedType.U1 ) ]
		public bool 	hasVolume;

		// Accessors
		public string name
		{ get { return HAPI_Host.getString( nameSH ); } private set {} }
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
		
		public int		owner;		// enum HAPI_AttributeOwner
		public int		storage;	// enum HAPI_StorageType
		public int		count;
		public int		tupleSize;
	}
	
	// MATERIALS ----------------------------------------------------------------------------------------------------

	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_MaterialInfo
	{
		public int id;
		public int assetId;
		public int nodeId;
	}

	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_ImageInfo
	{
		public HAPI_ImageFileFormat fileFormat;

		public int xRes;
		public int yRes;
	}

	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_ImagePlaneInfo
	{
		public HAPI_ImageDataFormat dataFormat;

		// The vector size cannot be changed, only interleaving mode can.
		public HAPI_ImagePacking packing;
	}

	// ANIMATION ----------------------------------------------------------------------------------------------------
	
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_Keyframe
	{
	
		HAPI_Keyframe( float t, float v, float in_tangent, float out_tangent )
		{
			time = t;
			value = v;
			inTangent = in_tangent;
			outTangent = out_tangent;
		}
		
		[ MarshalAs( UnmanagedType.R4) ]
		public float   time;
		
		[ MarshalAs( UnmanagedType.R4) ]
		public float   value;
		
		[ MarshalAs( UnmanagedType.R4) ]
		public float   inTangent;
		
		[ MarshalAs( UnmanagedType.R4) ]
		public float   outTangent;
	
	}

	// VOLUMES ------------------------------------------------------------------------------------------------------

	/// This represents a volume primitive--sans the actual voxel values,
	/// which can be retrieved on a per-tile basis
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_VolumeInfo
	{
		// Dimensions of the index space axis aligned bounding box of the volume
		public int	    xLength; 
		public int	    yLength;
		public int	    zLength;
		public int 	    minX;
		public int 	    minY;
		public int 	    minZ;

		// Number of values per voxel
		public int 		tupleSize;
		public int		storage; // enum HAPI_StorageType

		public int 		tileSize;

		[ MarshalAs( UnmanagedType.Struct ) ]
		public HAPI_Transform transform;

		[ MarshalAs( UnmanagedType.U1 ) ]
		public bool    hasTaper;

		[ MarshalAs( UnmanagedType.R4 ) ]
		public float   xTaper;

		[ MarshalAs( UnmanagedType.R4 ) ]
		public float   yTaper;

	};

	/// A HAPI_VolumeTileInfo represents an 8x8x8 section of a volume with
	/// bbox [(minX, minY, minZ), (minX+8, minY+8, minZ+8))
	[ StructLayout( LayoutKind.Sequential ) ]
	public struct HAPI_VolumeTileInfo
	{

		public bool isValid() 
		{
			return minX < int.MaxValue && minY < int.MaxValue && minZ < int.MaxValue;
		}

		public int minX;
		public int minY;
		public int minZ;
	};

}
