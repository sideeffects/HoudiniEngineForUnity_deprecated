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
using System;
using System.Runtime.InteropServices;

// Typedefs
using HAPI_SessionId = System.Int64;
using HAPI_Int64 = System.Int64;
using HAPI_StringHandle = System.Int32;
using HAPI_NodeId = System.Int32;
using HAPI_ParmId = System.Int32;
using HAPI_PartId = System.Int32;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Defines

public struct HoudiniConstants
{
	// Unity-Only Constants ---------------------------------------------
	//
	// You may change these values if you wish. Nothing should break terribly.

	public const string HAPI_PRODUCT_NAME				= "Houdini Engine";
		
	// Used for things like window titles that have limited space.
	public const string HAPI_PRODUCT_SHORT_NAME			= "Houdini";

	public static string HAPI_TEXTURES_PATH				= Application.dataPath + "/Textures";
	public static string HAPI_BAKED_ASSETS_PATH			= Application.dataPath + "/Baked Assets";

	public const int HAPI_MAX_PAGE_SIZE					= 20000;
	public const int HAPI_SEC_BEFORE_PROGRESS_BAR_SHOW	= 3;
	public const int HAPI_MIN_VERTICES_PER_FACE			= 3;
	public const int HAPI_MAX_VERTICES_PER_FACE			= 3;

	public const bool HAPI_CURVE_REFINE_TO_LINEAR		= true;
	public const float HAPI_CURVE_LOD					= 8.0f;

	public const float HAPI_VOLUME_POSITION_MULT		= 2.0f;
	public const float HAPI_VOLUME_SURFACE_MAX_PT_PER_C = 64000; // Max points per container. 65000 is Unity max.
	public const float HAPI_VOLUME_SURFACE_DELTA_MULT	= 1.2f;
	public const float HAPI_VOLUME_SURFACE_PT_SIZE_MULT = 1800.0f;

	// Shared Constants -------------------------------------------------
	//
	// IMPORTANT: Changes to these constants will not change the behavior of the
	// underlying Houdini Engine. These are here to serve as C# duplicates of the
	// constants defined in the HAPI_Common.h C++ header. In fact, if you
	// change any of these you will most likely break the Unity plugin.

	public const int HAPI_POSITION_VECTOR_SIZE			= 3;
	public const int HAPI_SCALE_VECTOR_SIZE				= 3;
	public const int HAPI_SHEAR_VECTOR_SIZE				= 3;
	public const int HAPI_NORMAL_VECTOR_SIZE			= 3;
	public const int HAPI_QUATERNION_VECTOR_SIZE		= 4;
	public const int HAPI_EULER_VECTOR_SIZE				= 3;
	public const int HAPI_UV_VECTOR_SIZE				= 2;
	public const int HAPI_COLOR_VECTOR_SIZE				= 4;
	public const int HAPI_CV_VECTOR_SIZE				= 4;

	public const int HAPI_PRIM_MIN_VERTEX_COUNT			= 1;
	public const int HAPI_PRIM_MAX_VERTEX_COUNT			= 16;

	public const int HAPI_INVALID_PARM_ID 				= -1;

	// Default Attributes' Names
	public const string HAPI_ATTRIB_POSITION			= "P";
	public const string HAPI_ATTRIB_UV					= "uv";
	public const string HAPI_ATTRIB_UV2					= "uv2";
	public const string HAPI_ATTRIB_UV3					= "uv3";
	public const string HAPI_ATTRIB_NORMAL				= "N";
	public const string HAPI_ATTRIB_TANGENT				= "tangentu";
	public const string HAPI_ATTRIB_COLOR				= "Cd";
	public const string HAPI_ATTRIB_ALPHA				= "Alpha";
	public const string HAPI_ATTRIB_NAME				= "name";
	public const string HAPI_ATTRIB_INSTANCE			= "instance";

	public const string HAPI_UNGROUPED_GROUP_NAME		= "__ungrouped_group";

	// Common image file format names (to use with the material extract APIs).
	// Note that you may still want to check if they are supported via
	// HAPI_GetSupportedImageFileFormats() since all formats are loaded 
	// dynamically by Houdini on-demand so just because these formats are defined
	// here doesn't mean they are supported in your instance.
	public const string HAPI_RAW_FORMAT_NAME			= "HAPI_RAW"; // HAPI-only Raw Format
	public const string HAPI_PNG_FORMAT_NAME			= "PNG";
	public const string HAPI_JPEG_FORMAT_NAME			= "JPEG";
	public const string HAPI_BMP_FORMAT_NAME			= "Bitmap";
	public const string HAPI_TIFF_FORMAT_NAME			= "TIFF";
	public const string HAPI_TGA_FORMAT_NAME			= "Targa";

	// Default image file format's name - used when the image generated and has
	// no "original" file format and the user does not specify a format to
	// convert to.
	public const string HAPI_DEFAULT_IMAGE_FORMAT_NAME	= HAPI_PNG_FORMAT_NAME;

	/// Name of subnet OBJ node containing the global nodes.
	public const string HAPI_GLOBAL_NODES_NODE_NAME		= "GlobalNodes";

	/// Environment variables.
	public const string HAPI_ENV_HIP					= "HIP";
	public const string HAPI_ENV_JOB					= "JOB";
	public const string HAPI_ENV_CLIENT_NAME			= "HAPI_CLIENT_NAME";

	/// Common cache names. You can see these same cache names in the
	/// Cache Manager window in Houdini (Windows > Cache Manager).
	public const string HAPI_CACHE_COP_COOK				= "COP Cook Cache";
	public const string HAPI_CACHE_COP_FLIPBOOK			= "COP Flipbook Cache";
	public const string HAPI_CACHE_IMAGE				= "Image Cache";
	public const string HAPI_CACHE_OBJ					= "Object Transform Cache";
	public const string HAPI_CACHE_GL_TEXTURE			= "OpenGL Texture Cache";
	public const string HAPI_CACHE_GL_VERTEX			= "OpenGL Vertex Cache";
	public const string HAPI_CACHE_SOP					= "SOP Cache";
	public const string HAPI_CACHE_VEX					= "VEX File Cache";

	public const string HAPI_UNSUPPORTED_PLATFORM_MSG   =
		"Houdini Plugin for Unity currently only supports the Standalone Windows and Mac OS X platforms in Editor.\n" +
		"\n" +
		"To switch to the Standalone Windows platform go to File > Build Settings... and under 'Platform' " +
		"choose 'PC, Mac & Linux Standalone' and click 'Switch Platform'. Afterwards, on the right hand side, " +
		"under 'Target Platform' choose 'Windows' or 'Mac OS X'.\n" +
		"\n" +
		"When you switch back to the Standalone Windows or Mac platform you might need to Rebuild each asset to get back " +
		"the controls. Just click on the 'Rebuild' button in the Houdini Asset's Inspector.";
}
	
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Enums

public enum HAPI_License
{
	HAPI_LICENSE_NONE,
	HAPI_LICENSE_HOUDINI_ENGINE,
	HAPI_LICENSE_HOUDINI,
	HAPI_LICENSE_HOUDINI_FX,
	HAPI_LICENSE_HOUDINI_ENGINE_INDIE,
	HAPI_LICENSE_HOUDINI_INDIE,
	HAPI_LICENSE_MAX
};

public enum HAPI_StatusType
{
	HAPI_STATUS_CALL_RESULT,
	HAPI_STATUS_COOK_RESULT,
	HAPI_STATUS_COOK_STATE,
	HAPI_STATUS_MAX
};

public enum HAPI_StatusVerbosity
{
	HAPI_STATUSVERBOSITY_0,
	HAPI_STATUSVERBOSITY_1,
	HAPI_STATUSVERBOSITY_2,

	HAPI_STATUSVERBOSITY_ALL = HAPI_STATUSVERBOSITY_2,

	/// Used for Results.
	/// @{
	HAPI_STATUSVERBOSITY_ERRORS = HAPI_STATUSVERBOSITY_0,
	HAPI_STATUSVERBOSITY_WARNINGS = HAPI_STATUSVERBOSITY_1,
	HAPI_STATUSVERBOSITY_MESSAGES = HAPI_STATUSVERBOSITY_2
	/// @}
};

public enum HAPI_Result 
{
	HAPI_RESULT_SUCCESS									= 0,
	HAPI_RESULT_FAILURE									= 1,
	HAPI_RESULT_ALREADY_INITIALIZED						= 2,
	HAPI_RESULT_NOT_INITIALIZED							= 3,
	HAPI_RESULT_CANT_LOADFILE							= 4,
	HAPI_RESULT_PARM_SET_FAILED							= 5,
	HAPI_RESULT_INVALID_ARGUMENT						= 6,
	HAPI_RESULT_CANT_LOAD_GEO							= 7,
	HAPI_RESULT_CANT_GENERATE_PRESET					= 8,
	HAPI_RESULT_CANT_LOAD_PRESET						= 9,
	HAPI_RESULT_ASSET_DEF_ALREADY_LOADED				= 10,

	HAPI_RESULT_NO_LICENSE_FOUND						= 110,
	HAPI_RESULT_DISALLOWED_NC_LICENSE_FOUND				= 120,
	HAPI_RESULT_DISALLOWED_NC_ASSET_WITH_C_LICENSE		= 130,
	HAPI_RESULT_DISALLOWED_NC_ASSET_WITH_LC_LICENSE		= 140,
	HAPI_RESULT_DISALLOWED_LC_ASSET_WITH_C_LICENSE		= 150,
	HAPI_RESULT_DISALLOWED_HENGINEINDIE_W_3PARTY_PLUGIN = 160,

	HAPI_RESULT_ASSET_INVALID							= 200,
	HAPI_RESULT_NODE_INVALID							= 210,

	HAPI_RESULT_USER_INTERRUPTED						= 300,

	HAPI_RESULT_INVALID_SESSION							= 400
};

[ Flags ]
public enum HAPI_ErrorCode
{
	HAPI_ERRORCODE_ASSET_DEF_NOT_FOUND				= 1 << 0,
	HAPI_ERRORCODE_PYTHON_NODE_ERROR				= 1 << 1
};

public enum HAPI_SessionType
{
	HAPI_SESSION_INPROCESS,
	HAPI_SESSION_THRIFT,
	HAPI_SESSION_CUSTOM1,
	HAPI_SESSION_CUSTOM2,
	HAPI_SESSION_CUSTOM3,
	HAPI_SESSION_MAX
};

public enum HAPI_State
{
	HAPI_STATE_READY,
	HAPI_STATE_READY_WITH_FATAL_ERRORS,
	HAPI_STATE_READY_WITH_COOK_ERRORS,
	HAPI_STATE_STARTING_COOK,
	HAPI_STATE_COOKING,
	HAPI_STATE_STARTING_LOAD,
	HAPI_STATE_LOADING,
	HAPI_STATE_MAX,

	HAPI_STATE_MAX_READY_STATE = HAPI_STATE_READY_WITH_COOK_ERRORS
};

public enum HAPI_PackedPrimInstancingMode
{
	HAPI_PACKEDPRIM_INSTANCING_MODE_INVALID = -1,
	HAPI_PACKEDPRIM_INSTANCING_MODE_DISABLED,
	HAPI_PACKEDPRIM_INSTANCING_MODE_HIERARCHY,
	HAPI_PACKEDPRIM_INSTANCING_MODE_FLAT,
	HAPI_PACKEDPRIM_INSTANCING_MODE_MAX
};

public enum HAPI_Permissions
{
	HAPI_PERMISSIONS_NON_APPLICABLE,
	HAPI_PERMISSIONS_READ_WRITE,
	HAPI_PERMISSIONS_READ_ONLY,
	HAPI_PERMISSIONS_WRITE_ONLY,
	HAPI_PERMISSIONS_MAX
};

public enum HAPI_RampType
{
	HAPI_RAMPTYPE_INVALID = -1,
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
	HAPI_PARMTYPE_COLOR,

	HAPI_PARMTYPE_STRING,
	HAPI_PARMTYPE_PATH_FILE,
	HAPI_PARMTYPE_PATH_FILE_GEO,
	HAPI_PARMTYPE_PATH_FILE_IMAGE,

	HAPI_PARMTYPE_NODE,

	HAPI_PARMTYPE_FOLDERLIST,
	HAPI_PARMTYPE_FOLDERLIST_RADIO,

	HAPI_PARMTYPE_FOLDER,
	HAPI_PARMTYPE_LABEL,
	HAPI_PARMTYPE_SEPARATOR,

	// Helpers

	HAPI_PARMTYPE_MAX, // Total number of supported parameter types.

	HAPI_PARMTYPE_INT_START			= HAPI_PARMTYPE_INT,
	HAPI_PARMTYPE_INT_END			= HAPI_PARMTYPE_BUTTON,

	HAPI_PARMTYPE_FLOAT_START		= HAPI_PARMTYPE_FLOAT,
	HAPI_PARMTYPE_FLOAT_END			= HAPI_PARMTYPE_COLOR,

	HAPI_PARMTYPE_STRING_START		= HAPI_PARMTYPE_STRING,
	HAPI_PARMTYPE_STRING_END		= HAPI_PARMTYPE_NODE,

	HAPI_PARMTYPE_PATH_START		= HAPI_PARMTYPE_PATH_FILE,
	HAPI_PARMTYPE_PATH_END			= HAPI_PARMTYPE_PATH_FILE_IMAGE,

	HAPI_PARMTYPE_NODE_START		= HAPI_PARMTYPE_NODE,
	HAPI_PARMTYPE_NODE_END			= HAPI_PARMTYPE_NODE,

	HAPI_PARMTYPE_CONTAINER_START 	= HAPI_PARMTYPE_FOLDERLIST,
	HAPI_PARMTYPE_CONTAINER_END 	= HAPI_PARMTYPE_FOLDERLIST_RADIO,

	HAPI_PARMTYPE_NONVALUE_START	= HAPI_PARMTYPE_FOLDER,
	HAPI_PARMTYPE_NONVALUE_END		= HAPI_PARMTYPE_SEPARATOR
}

public enum HAPI_ChoiceListType
{
	HAPI_CHOICELISTTYPE_NONE,
	HAPI_CHOICELISTTYPE_NORMAL,
	HAPI_CHOICELISTTYPE_MINI,
	HAPI_CHOICELISTTYPE_REPLACE,
	HAPI_CHOICELISTTYPE_TOGGLE
};

public enum HAPI_PresetType
{
	HAPI_PRESETTYPE_INVALID = -1,
	HAPI_PRESETTYPE_BINARY = 0,
	HAPI_PRESETTYPE_IDX,
	HAPI_PRESETTYPE_MAX
};

[ Flags ]
public enum HAPI_NodeType
{
	HAPI_NODETYPE_ANY		= -1,
	HAPI_NODETYPE_NONE		= 0,
	HAPI_NODETYPE_OBJ		= 1 << 0,
	HAPI_NODETYPE_SOP		= 1 << 1,
	HAPI_NODETYPE_POP		= 1 << 2,
	HAPI_NODETYPE_CHOP		= 1 << 3,
	HAPI_NODETYPE_ROP		= 1 << 4,
	HAPI_NODETYPE_SHOP		= 1 << 5,
	HAPI_NODETYPE_COP		= 1 << 6,
	HAPI_NODETYPE_VOP		= 1 << 7,
	HAPI_NODETYPE_DOP		= 1 << 8
};

[ Flags ]
public enum HAPI_NodeFlags
{
	HAPI_NODEFLAGS_ANY			= -1,
	HAPI_NODEFLAGS_NONE			= 0,
	HAPI_NODEFLAGS_DISPLAY		= 1 << 0,
	HAPI_NODEFLAGS_RENDER		= 1 << 1,
	HAPI_NODEFLAGS_TEMPLATED	= 1 << 2,
	HAPI_NODEFLAGS_LOCKED		= 1 << 3,
	HAPI_NODEFLAGS_EDITABLE		= 1 << 4,
	HAPI_NODEFLAGS_BYPASS		= 1 << 5,
	HAPI_NODEFLAGS_NETWORK		= 1 << 6,

	// OBJ Node Specific Flags
	HAPI_NODEFLAGS_OBJ_GEOMETRY	= 1 << 7,
	HAPI_NODEFLAGS_OBJ_CAMERA	= 1 << 8,
	HAPI_NODEFLAGS_OBJ_LIGHT	= 1 << 9,
	HAPI_NODEFLAGS_OBJ_SUBNET	= 1 << 10,

	/// SOP Node Specific Flags
	HAPI_NODEFLAGS_SOP_CURVE	= 1 << 11
};

public enum HAPI_GroupType
{
	HAPI_GROUPTYPE_INVALID = -1,
	HAPI_GROUPTYPE_POINT,
	HAPI_GROUPTYPE_PRIM,
	HAPI_GROUPTYPE_MAX
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
	
public enum HAPI_CurveType
{
	HAPI_CURVETYPE_INVALID = -1,
	HAPI_CURVETYPE_LINEAR,
	HAPI_CURVETYPE_NURBS,
	HAPI_CURVETYPE_BEZIER,
	HAPI_CURVETYPE_MAX
}

public enum HAPI_VolumeType
{
	HAPI_VOLUMETYPE_INVALID = -1,
	HAPI_VOLUMETYPE_HOUDINI,
	HAPI_VOLUMETYPE_VDB,
	HAPI_VOLUMETYPE_MAX
}

public enum HAPI_StorageType
{
	HAPI_STORAGETYPE_INVALID = -1,
	HAPI_STORAGETYPE_INT,
	HAPI_STORAGETYPE_INT64,
	HAPI_STORAGETYPE_FLOAT,
	HAPI_STORAGETYPE_FLOAT64,
	HAPI_STORAGETYPE_STRING,
	HAPI_STORAGETYPE_MAX
}

public enum HAPI_GeoType
{
	HAPI_GEOTYPE_INVALID = -1,
	HAPI_GEOTYPE_DEFAULT,
	HAPI_GEOTYPE_INTERMEDIATE,
	HAPI_GEOTYPE_INPUT,
	HAPI_GEOTYPE_CURVE,
	HAPI_GEOTYPE_MAX
};

public enum HAPI_PartType
{
	HAPI_PARTTYPE_INVALID = -1,
	HAPI_PARTTYPE_MESH,
	HAPI_PARTTYPE_CURVE,
	HAPI_PARTTYPE_VOLUME,
	HAPI_PARTTYPE_INSTANCER,
	HAPI_PARTTYPE_BOX,
	HAPI_PARTTYPE_SPHERE,
	HAPI_PARTTYPE_MAX
};

public enum HAPI_InputType
{
	HAPI_INPUT_INVALID = -1,
	HAPI_INPUT_TRANSFORM,
	HAPI_INPUT_GEOMETRY,
	HAPI_INPUT_MAX
};

public enum HAPI_CurveOrders
{
	HAPI_CURVE_ORDER_VARYING = 0,
	HAPI_CURVE_ORDER_INVALID = 1,
	HAPI_CURVE_ORDER_LINEAR = 2,
	HAPI_CURVE_ORDER_QUADRATIC = 3,
	HAPI_CURVE_ORDER_CUBIC = 4,
}

public enum HAPI_TransformComponent
{
	HAPI_TRANSFORM_TX = 0,
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

public enum HAPI_RSTOrder
{
	HAPI_TRS = 0,
	HAPI_TSR,
	HAPI_RTS,
	HAPI_RST,
	HAPI_STR,
	HAPI_SRT,

	HAPI_RSTORDER_DEFAULT = HAPI_SRT
}
	
public enum HAPI_XYZOrder
{
	HAPI_XYZ = 0,
	HAPI_XZY,
	HAPI_YXZ,
	HAPI_YZX,
	HAPI_ZXY,
	HAPI_ZYX,

	HAPI_XYZORDER_DEFAULT = HAPI_XYZ
}

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
	HAPI_IMAGE_PACKING_DUAL,	// Dual Channel
	HAPI_IMAGE_PACKING_RGB,		// RGB
	HAPI_IMAGE_PACKING_BGR,		// RGB Reveresed
	HAPI_IMAGE_PACKING_RGBA,	// RGBA
	HAPI_IMAGE_PACKING_ABGR,	// RGBA Reversed
	HAPI_IMAGE_PACKING_MAX,

	HAPI_IMAGE_PACKING_DEFAULT3 = HAPI_IMAGE_PACKING_RGB,
	HAPI_IMAGE_PACKING_DEFAULT4 = HAPI_IMAGE_PACKING_RGBA
};

public enum HAPI_EnvIntType
{
	HAPI_ENVINT_INVALID = -1,

	// The three components of the Houdini version that HAPI is
	// expecting to compile against.
	HAPI_ENVINT_VERSION_HOUDINI_MAJOR = 100,
	HAPI_ENVINT_VERSION_HOUDINI_MINOR = 110,
	HAPI_ENVINT_VERSION_HOUDINI_BUILD = 120,
	HAPI_ENVINT_VERSION_HOUDINI_PATCH = 130,

	// The two components of the Houdini Engine (marketed) version.
	HAPI_ENVINT_VERSION_HOUDINI_ENGINE_MAJOR = 200,
	HAPI_ENVINT_VERSION_HOUDINI_ENGINE_MINOR = 210,

	// This is a monotonously increasing API version number that can be used
	// to lock against a certain API for compatibility purposes. Basically,
	// when this number changes code compiled against the h methods
	// might no longer compile. Semantic changes to the methods will also
	// cause this version to increase. This number will be reset to 0
	// every time the Houdini Engine version is bumped.
	HAPI_ENVINT_VERSION_HOUDINI_ENGINE_API = 220,

	HAPI_ENVINT_MAX,
};

public enum HAPI_SessionEnvIntType
{
	HAPI_SESSIONENVINT_INVALID = -1,

	/// License Type. See ::HAPI_License.
	HAPI_SESSIONENVINT_LICENSE = 100,

	HAPI_SESSIONENVINT_MAX,
};

public enum HAPI_CacheProperty
{
    /// Current memory usage in MB. Setting this to 0 invokes
    /// a cache clear.
    HAPI_CACHEPROP_CURRENT,

    HAPI_CACHEPROP_HAS_MIN, // True if it actually has a minimum size.
    HAPI_CACHEPROP_MIN, // Min cache memory limit in MB.
    HAPI_CACHEPROP_HAS_MAX, // True if it actually has a maximum size.
    HAPI_CACHEPROP_MAX, // Max cache memory limit in MB.

    /// How aggressive to cull memory. This only works for:
    ///     - ::HAPI_CACHE_COP_COOK where:
    ///         0   ->  Never reduce inactive cache.
    ///         1   ->  Always reduce inactive cache.
    ///     - ::HAPI_CACHE_OBJ where:
    ///         0   ->  Never enforce the max memory limit.
    ///         1   ->  Always enforce the max memory limit.
    ///     - ::HAPI_CACHE_SOP where:
    ///         0   ->  When to Unload = Never
    ///                 When to Limit Max Memory = Never
    ///         1-2 ->  When to Unload = Based on Flag
    ///                 When to Limit Max Memory = Never
    ///         3-4 ->  When to Unload = Based on Flag
    ///                 When to Limit Max Memory = Always
    ///         5   ->  When to Unload = Always
    ///                 When to Limit Max Memory = Always
    HAPI_CACHEPROP_CULL_LEVEL,
};

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Main API Structs
	
// GENERICS -----------------------------------------------------------------------------------------------------

[ StructLayout( LayoutKind.Sequential ) ]
public struct HAPI_Transform
{
	public HAPI_Transform( bool initialize_fields )
	{
		position = new float[ HoudiniConstants.HAPI_POSITION_VECTOR_SIZE ];
		rotationQuaternion = new float[ HoudiniConstants.HAPI_QUATERNION_VECTOR_SIZE ];
		scale = new float[ HoudiniConstants.HAPI_SCALE_VECTOR_SIZE ];
		shear = new float[ HoudiniConstants.HAPI_SHEAR_VECTOR_SIZE ];

		rstOrder = HAPI_RSTOrder.HAPI_SRT;

        if ( initialize_fields )
            init();
	}

    [ MarshalAs(
		UnmanagedType.ByValArray,
		SizeConst = HoudiniConstants.HAPI_POSITION_VECTOR_SIZE,
		ArraySubType = UnmanagedType.R4 ) ]
	public float[] position;

	[ MarshalAs(
		UnmanagedType.ByValArray,
		SizeConst = HoudiniConstants.HAPI_QUATERNION_VECTOR_SIZE,
		ArraySubType = UnmanagedType.R4 ) ]
	public float[] rotationQuaternion;
		
	[ MarshalAs(
		UnmanagedType.ByValArray,
		SizeConst = HoudiniConstants.HAPI_SCALE_VECTOR_SIZE,
		ArraySubType = UnmanagedType.R4 ) ]
	public float[] scale;

	[ MarshalAs(
		UnmanagedType.ByValArray,
		SizeConst = HoudiniConstants.HAPI_SHEAR_VECTOR_SIZE,
		ArraySubType = UnmanagedType.R4 ) ]
	public float[] shear;

	public HAPI_RSTOrder rstOrder;
    
    public void init()
    {
        for (int n = 0; n < HoudiniConstants.HAPI_POSITION_VECTOR_SIZE; n++)
            position[n] = 0.0f;

        for (int n = 0; n < HoudiniConstants.HAPI_QUATERNION_VECTOR_SIZE; n++)
        {
            if (n == 3)
                rotationQuaternion[n] = 1.0f;
            else
                rotationQuaternion[n] = 0.0f;
        }

        for (int n = 0; n < HoudiniConstants.HAPI_SCALE_VECTOR_SIZE; n++)
            scale[n] = 1.0f;

        for (int n = 0; n < HoudiniConstants.HAPI_SHEAR_VECTOR_SIZE; n++)
            scale[n] = 0.0f;
    }
}
	
[ StructLayout( LayoutKind.Sequential ) ]
public struct HAPI_TransformEuler
{
	public HAPI_TransformEuler( bool initialize_fields )
	{
		position = new float[ HoudiniConstants.HAPI_POSITION_VECTOR_SIZE ];
		rotationEuler = new float[ HoudiniConstants.HAPI_EULER_VECTOR_SIZE ];
		scale = new float[ HoudiniConstants.HAPI_SCALE_VECTOR_SIZE ];
		shear = new float[ HoudiniConstants.HAPI_SHEAR_VECTOR_SIZE ];

		rotationOrder = 0;
		rstOrder = 0;
	}

	[ MarshalAs(
		UnmanagedType.ByValArray,
		SizeConst = HoudiniConstants.HAPI_POSITION_VECTOR_SIZE,
		ArraySubType = UnmanagedType.R4 ) ]
	public float[] position;

	[ MarshalAs(
		UnmanagedType.ByValArray,
		SizeConst = HoudiniConstants.HAPI_EULER_VECTOR_SIZE,
		ArraySubType = UnmanagedType.R4 ) ]
	public float[] rotationEuler;

	[ MarshalAs(
		UnmanagedType.ByValArray,
		SizeConst = HoudiniConstants.HAPI_SCALE_VECTOR_SIZE,
		ArraySubType = UnmanagedType.R4 ) ]
	public float[] scale;

	[ MarshalAs(
		UnmanagedType.ByValArray,
		SizeConst = HoudiniConstants.HAPI_SHEAR_VECTOR_SIZE,
		ArraySubType = UnmanagedType.R4 ) ]
	public float[] shear;

	public HAPI_XYZOrder rotationOrder;
	public HAPI_RSTOrder rstOrder;
}

// SESSIONS -----------------------------------------------------------------------------------------------------

[ StructLayout( LayoutKind.Sequential ) ]
public struct HAPI_Session
{
	/// The type of session detemines the which implementation will be
	/// used to communicate with the Houdini Engine library.
	public HAPI_SessionType type;

	/// Some session types support multiple simultanous sessions. This means
	/// that each session needs to have a unique identified.
	public HAPI_SessionId id;
};

/// Options to configure a Thrift server being started from HARC.
public struct HAPI_ThriftServerOptions
{
	/// Close the server automatically when all clients disconnect from it.
	[ MarshalAs( UnmanagedType.U1 ) ] public bool autoClose;

	/// Timeout in milliseconds for waiting on the server to
	/// signal that it's ready to serve. If the server fails
	/// to signal within this time interval, the start server call fails
	/// and the server process is terminated.
	[ MarshalAs( UnmanagedType.R4 ) ] public float timeoutMs;
};

// TIME ---------------------------------------------------------------------------------------------------------

[ StructLayout( LayoutKind.Sequential ) ]
public struct HAPI_TimelineOptions
{
    public float fps;

    public float startTime;
    public float endTime;
}

// ASSETS -------------------------------------------------------------------------------------------------------

[ StructLayout( LayoutKind.Sequential ) ]
public struct HAPI_AssetInfo 
{
	// Use the node id to get the asset's parameters.
	public HAPI_NodeId nodeId;

	// The objectNodeId differs from the regular nodeId in that for
	// geometry based assets (SOPs) it will be the node id of the dummy
	// object (OBJ) node instead of the asset node. For object based assets
	// the objectNodeId will equal the nodeId. The reason the distinction
	// exists is because transforms are always stored on the object node
	// but the asset parameters may not be on the asset node if the asset
	// is a geometry asset so we need both.
	public HAPI_NodeId objectNodeId;

	[ MarshalAs( UnmanagedType.U1 ) ] public bool hasEverCooked;

	private HAPI_StringHandle nameSH; // Instance name (the label + a number).
	private HAPI_StringHandle labelSH;
	private HAPI_StringHandle filePathSH; // Path to the .otl file for this asset.

	private HAPI_StringHandle versionSH; // User-defined asset version.
	private HAPI_StringHandle fullOpNameSH; // Full asset name and namespace.
	private HAPI_StringHandle helpTextSH; // Asset help marked-up text.

	public int objectCount;
	public int handleCount;

	public int transformInputCount;
	public int geoInputCount;

	[ MarshalAs( UnmanagedType.U1 ) ] public bool haveObjectsChanged;
	[ MarshalAs( UnmanagedType.U1 ) ] public bool haveMaterialsChanged;

	// Accessors
	public string name
	{ get { return HoudiniHost.getString( nameSH ); } private set {} }
	public string label
	{ get { return HoudiniHost.getString( labelSH ); } private set {} }
	public string filePath
	{ get { return HoudiniHost.getString( filePathSH ); } private set {} }
	public string version
	{ get { return HoudiniHost.getString( versionSH ); } private set {} }
	public string fullOpName
	{ get { return HoudiniHost.getString( fullOpNameSH ); } private set {} }
	public string helpText
	{ get { return HoudiniHost.getString( helpTextSH ); } private set {} }
}

[ StructLayout( LayoutKind.Sequential ) ]
public struct HAPI_CookOptions
{
	/// Normally, geos are split into parts in two different ways. First it
	/// is split by group and within each group it is split by primitive type.
	///
	/// For example, if you have a geo with group1 covering half of the mesh
	/// and volume1 and group2 covering the other half of the mesh, all of
	/// curve1, and volume2 you will end up with 5 parts. First two parts
	/// will be for the half-mesh of group1 and volume1, and the last three
	/// will cover group2.
	///
	/// This toggle lets you disable the splitting by group and just have
	/// the geo be split by primitive type alone. By default, this is true
	/// and therefore geos will be split by group and primitive type. If
	/// set to false, geos will only be split by primtive type.
    [ MarshalAs( UnmanagedType.U1 ) ] public bool splitGeosByGroup;

	/// For meshes only, this is enforced by convexing the mesh. Use -1
	/// to avoid convexing at all and get some performance boost.
	public int maxVerticesPerPrimitive;

	// Curves
	[ MarshalAs( UnmanagedType.U1 ) ] public bool refineCurveToLinear;
	public float curveRefineLOD;

	/// If this option is turned on, then we will recursively clear the 
	/// errors and warnings (and messages) of all nodes before performing
	/// the cook.
	[ MarshalAs( UnmanagedType.U1 ) ] public bool clearErrorsAndWarnings;

	/// Decide whether to recursively cook all templated geos or not.
	[ MarshalAs( UnmanagedType.U1 ) ] public bool cookTemplatedGeos;

	/// Decide whether to split points by vertex attributes. This takes
	/// all vertex attributes and tries to copy them to their respective
	/// points. If two vertices have any difference in their attribute values,
	/// the corresponding point is split into two points. This is repeated
	/// until all the vertex attributes have been copied to the points.
	///
	/// With this option enabled, you can reduce the total number of vertices
	/// on a game engine side as sharing of attributes (like UVs) is optimized.
	/// To make full use of this feature, you have to think of Houdini points
	/// as game engine vertices (sharable). With this option OFF (or before
	/// this feature existed) you had to map Houdini vertices to game engine
	/// vertices, to make sure all attribute values are accounted for.
	[ MarshalAs( UnmanagedType.U1 ) ] public bool splitPointsByVertexAttributes;

	/// Choose how you want the cook to handle packed primitives.
	public HAPI_PackedPrimInstancingMode packedPrimInstancingMode;

	/// Choose which special part types should be handled. Unhandled special
	/// part types will just be refined to ::HAPI_PARTTYPE_MESH.
	[ MarshalAs( UnmanagedType.U1 ) ] public bool handleBoxPartTypes;
	[ MarshalAs( UnmanagedType.U1 ) ] public bool handleSpherePartTypes;

	/// For internal use only. :)
	public int extraFlags;
}

// NODES --------------------------------------------------------------------------------------------------------

[ Serializable ]
[ StructLayout( LayoutKind.Sequential ) ]
public struct HAPI_NodeInfo
{
	public HAPI_NodeId id;
	public HAPI_NodeId parentId;
	public HAPI_StringHandle nameSH;
	public HAPI_NodeType type;

	[ MarshalAs( UnmanagedType.U1 ) ] public bool isValid;

	public int totalCookCount;

	public int uniqueHoudiniNodeId;
	private HAPI_StringHandle internalNodePathSH;

	public int parmCount;
	public int parmIntValueCount;
	public int parmFloatValueCount;
	public int parmStringValueCount;
	public int parmChoiceCount;

	public int childNodeCount;
	public int inputCount;

	[ MarshalAs( UnmanagedType.U1 ) ] public bool createdPostAssetLoad;

	public string name
	{ get { return HoudiniHost.getString( nameSH ); } private set {} }
	public string internalNodePath
	{ get { return HoudiniHost.getString( internalNodePathSH ); } private set {} }
}

// PARAMETERS ---------------------------------------------------------------------------------------------------

[ Serializable ]
[ StructLayout( LayoutKind.Sequential ) ]
public struct HAPI_ParmInfo
{
	public bool isInt()
	{
		return ( type >= HAPI_ParmType.HAPI_PARMTYPE_INT_START &&
			type <= HAPI_ParmType.HAPI_PARMTYPE_INT_END )
			|| type == HAPI_ParmType.HAPI_PARMTYPE_MULTIPARMLIST;
	}
	public bool isFloat()
	{
		return ( type >= HAPI_ParmType.HAPI_PARMTYPE_FLOAT_START &&
			type <= HAPI_ParmType.HAPI_PARMTYPE_FLOAT_END );
	}
	public bool isString()
	{
		return ( type >= HAPI_ParmType.HAPI_PARMTYPE_STRING_START &&
			type <= HAPI_ParmType.HAPI_PARMTYPE_STRING_END );
	}
	public bool isPath()
	{
		return ( type >= HAPI_ParmType.HAPI_PARMTYPE_PATH_START &&
			type <= HAPI_ParmType.HAPI_PARMTYPE_PATH_END );
	}
	public bool isNode()
	{
		return ( type >= HAPI_ParmType.HAPI_PARMTYPE_NODE_START &&
			type <= HAPI_ParmType.HAPI_PARMTYPE_NODE_END );
	}
	public bool isNonValue()
	{
		return ( type >= HAPI_ParmType.HAPI_PARMTYPE_NONVALUE_START &&
			type <= HAPI_ParmType.HAPI_PARMTYPE_NONVALUE_END );
	}

	// The parent id points to the id of the parent parm
	// of this parm. The parent parm is something like a folder.
	public HAPI_ParmId id;
	public HAPI_ParmId parentId;
	public int childIndex;

	public HAPI_ParmType type;
	public HAPI_StringHandle typeInfoSH;

	public HAPI_Permissions permissions;

	public int tagCount;

	public int size; // Tuple Size

	HAPI_ChoiceListType choiceListType;
	public int choiceCount;

	// Note that folders are not real parameters in Houdini so they do not
	// have names. The folder names given here are generated from the name
	// of the folderlist (or switcher) parameter which is a parameter. The
	// folderlist parameter simply defines how many of the "next" parameters
	// belong to the first folder, how many of the parameters after that
	// belong to the next folder, and so on. This means that folder names
	// can change just by reordering the folders around so don't rely on
	// them too much. The only guarantee here is that the folder names will
	// be unique among all other parameter names.
	private HAPI_StringHandle nameSH;
	private HAPI_StringHandle labelSH;
	
	// If this parameter is a multiparm instance than the templateNameSH
	// will be the hash-templated parm name, containing #'s for the 
	// parts of the name that use the instance number. Compared to the
	// nameSH, the nameSH will be the templateNameSH but with the #'s
	// replaced by the instance number. For regular parms, the templateNameSH
	// is identical to the nameSH.
	private HAPI_StringHandle templateNameSH;
	
	private HAPI_StringHandle helpSH;

	[ MarshalAs( UnmanagedType.U1 ) ]
	public bool hasMin;

	[ MarshalAs( UnmanagedType.U1 ) ]
	public bool hasMax;

	[ MarshalAs( UnmanagedType.U1 ) ]
	public bool hasUIMin;

	[ MarshalAs( UnmanagedType.U1 ) ]
	public bool hasUIMax;

	[ MarshalAs( UnmanagedType.R4 ) ]
	public float min;

	[ MarshalAs( UnmanagedType.R4 ) ]
	public float max;

	[ MarshalAs( UnmanagedType.R4 ) ]
	public float UIMin;

	[ MarshalAs( UnmanagedType.R4 ) ]
	public float UIMax;

	[ MarshalAs( UnmanagedType.U1 ) ]
	public bool invisible;

	[ MarshalAs( UnmanagedType.U1 ) ]
	public bool disabled;

	[ MarshalAs( UnmanagedType.U1 ) ]
	public bool spare;

	// Whether this parm should be on the same line as the next parm.
	[ MarshalAs( UnmanagedType.U1 ) ]
	public bool joinNext;

	[ MarshalAs( UnmanagedType.U1 ) ]
	public bool labelNone;

	public int intValuesIndex;
	public int floatValuesIndex;
	public int stringValuesIndex;
	public int choiceIndex;

	HAPI_NodeType inputNodeType;
	HAPI_NodeFlags inputNodeFlag;

	[ MarshalAs( UnmanagedType.U1 ) ]
	public bool isChildOfMultiParm;

	public int instanceNum; // The index of the instance in the multiparm.
	public int instanceLength; // The number of parms in a multiparm instance.
	public int instanceCount; // The number of instances in a multiparm.
	public int instanceStartOffset; // First instanceNum either 0 or 1.

	public HAPI_RampType rampType;

	// Accessors
	public int getTypeInfoSH()
	{ return typeInfoSH; }
	public int getNameSH()
	{ return nameSH; }
	public int getLabelSH()
	{ return labelSH; }
	public string typeInfo
	{ get { return HoudiniHost.getString( typeInfoSH ); } private set {} }
	public string name
	{ get { return HoudiniHost.getString( nameSH ); } private set {} }
	public string label
	{ get { return HoudiniHost.getString( labelSH ); } private set {} }
	public string templateName
	{ get { return HoudiniHost.getString( templateNameSH ); } private set {} }
	public string help
	{ get { return HoudiniHost.getString( helpSH ); } private set {} }
}

// Used for caching HAPI_ParmInfo strings.
[ Serializable ]
public struct HAPI_ParmInfoStrings
{
	public void cacheStrings( HAPI_ParmInfo parm_info )
	{
		typeInfo = parm_info.typeInfo;
		name = parm_info.name;
		label = parm_info.label;
		templateName = parm_info.templateName;
		help = parm_info.help;
	}

	public string typeInfo;
	public string name;
	public string label;
	public string templateName;
	public string help;
}

// Used for input parameters.
[ Serializable ]
public struct HAPI_ParmInput
{
	public bool isAsset;
	public GameObject inputObject;
	public GameObject newInputObject;
	public HAPI_NodeId inputNodeId;
	public int inputNodeUniqueId;
}

[ Serializable ]
[ StructLayout( LayoutKind.Sequential ) ]
public struct HAPI_ParmChoiceInfo
{
	public HAPI_ParmId parentParmId;
	private HAPI_StringHandle labelSH;
	private HAPI_StringHandle valueSH;
		
	// Accessors
	public string label
	{ get { return HoudiniHost.getString( labelSH ); } private set {} }
	public string value
	{ get { return HoudiniHost.getString( valueSH ); } private set {} }
}

// Used for caching HAPI_ParmChoiceInfo strings.
[ Serializable ]
public struct HAPI_ParmChoiceInfoStrings
{
	public void cacheStrings( HAPI_ParmChoiceInfo choice_info )
	{
		label = choice_info.label;
		value = choice_info.value;
	}

	public string label;
	public string value;
}

// HANDLES ------------------------------------------------------------------------------------------------------

[ Serializable ]
[ StructLayout( LayoutKind.Sequential ) ]
public struct HAPI_HandleInfo
{
	private HAPI_StringHandle nameSH;
	private HAPI_StringHandle typeNameSH;

	public int bindingsCount;

	// Accessors
	public string name
	{ get { return HoudiniHost.getString( nameSH ); } private set {} }
	public string typeName
	{ get { return HoudiniHost.getString( typeNameSH ); } private set {} }
}

[ Serializable ]
[ StructLayout( LayoutKind.Sequential ) ]
public struct HAPI_HandleBindingInfo
{
	private HAPI_StringHandle handleParmNameSH;
	private HAPI_StringHandle assetParmNameSH;

	public HAPI_ParmId assetParmId;

	// Accessors
	public string handleParmName
	{ get { return HoudiniHost.getString( handleParmNameSH ); } private set {} }
	public string assetParmName
	{ get { return HoudiniHost.getString( assetParmNameSH ); } private set {} }
};
	
// OBJECTS ------------------------------------------------------------------------------------------------------

[ Serializable ]
[ StructLayout( LayoutKind.Sequential ) ]
public struct HAPI_ObjectInfo 
{
	private HAPI_StringHandle nameSH;
	private HAPI_StringHandle objectInstancePathSH;

	[ MarshalAs( UnmanagedType.U1 ) ] public bool hasTransformChanged;
	[ MarshalAs( UnmanagedType.U1 ) ] public bool haveGeosChanged;

	[ MarshalAs( UnmanagedType.U1 ) ] public bool isVisible;
	[ MarshalAs( UnmanagedType.U1 ) ] public bool isInstancer;
	[ MarshalAs( UnmanagedType.U1 ) ] public bool isInstanced;

	public int geoCount;

	// Use the node id to get the node's parameters.
	public HAPI_NodeId nodeId;

	public HAPI_NodeId objectToInstanceId;

	// Accessors
	public string name
	{ get { return HoudiniHost.getString( nameSH ); } private set {} }
	public string objectInstancePath
	{ get { return HoudiniHost.getString( objectInstancePathSH ); } private set {} }
}
	
// GEOMETRY -----------------------------------------------------------------------------------------------------
	
[ StructLayout( LayoutKind.Sequential ) ]
public struct HAPI_GeoInfo
{
	public int getGroupCountByType( HAPI_GroupType type )
	{
		switch ( type )
		{
			case HAPI_GroupType.HAPI_GROUPTYPE_POINT: return pointGroupCount;
			case HAPI_GroupType.HAPI_GROUPTYPE_PRIM: return primitiveGroupCount;
			default: return 0;
		}
	}

	public HAPI_GeoType type;
	private HAPI_StringHandle nameSH;

	// Use the node id to get the node's parameters.
	public HAPI_NodeId nodeId;

	[ MarshalAs( UnmanagedType.U1 ) ] public bool isEditable;
	[ MarshalAs( UnmanagedType.U1 ) ] public bool isTemplated;
	[ MarshalAs( UnmanagedType.U1 ) ] public bool isDisplayGeo; // Final Result (Display SOP)
		
	[ MarshalAs( UnmanagedType.U1 ) ] public bool hasGeoChanged;
	[ MarshalAs( UnmanagedType.U1 ) ] public bool hasMaterialChanged;

	public int pointGroupCount;
	public int primitiveGroupCount;

	public int partCount;

	// Accessors
	public string name
	{ get { return HoudiniHost.getString( nameSH ); } private set {} }
}

[ StructLayout( LayoutKind.Sequential ) ]
public struct HAPI_PartInfo
{
	public int getElementCountByAttributeOwner( HAPI_AttributeOwner owner )
	{
		switch ( owner )
		{
			case HAPI_AttributeOwner.HAPI_ATTROWNER_VERTEX: return vertexCount;
			case HAPI_AttributeOwner.HAPI_ATTROWNER_POINT: return pointCount;
			case HAPI_AttributeOwner.HAPI_ATTROWNER_PRIM: return faceCount;
			case HAPI_AttributeOwner.HAPI_ATTROWNER_DETAIL: return 1;
			default: return 0;
		}
	}

	public int getElementCountByGroupType( HAPI_GroupType type )
	{
		switch ( type )
		{
			case HAPI_GroupType.HAPI_GROUPTYPE_POINT: return pointCount;
			case HAPI_GroupType.HAPI_GROUPTYPE_PRIM: return faceCount;
			default: return 0;
		}
	}

	public HAPI_PartId id;
	private HAPI_StringHandle nameSH;
	public HAPI_PartType type;

	public int faceCount;
	public int vertexCount;
	public int pointCount;

	[ MarshalAs( UnmanagedType.ByValArray,
		SizeConst = (int) HAPI_AttributeOwner.HAPI_ATTROWNER_MAX, 
		ArraySubType = UnmanagedType.I4 ) ]
	public int[] attributeCounts;

	[ MarshalAs( UnmanagedType.U1 ) ] public bool isInstanced;
	public int instancedPartCount;
	public int instanceCount;

	// Accessors
	public void init() { if ( attributeCounts == null ) attributeCounts  = new int[ (int) HAPI_AttributeOwner.HAPI_ATTROWNER_MAX ]; }
	public string name
	{ get { return HoudiniHost.getString( nameSH ); } private set {} }
	public int pointAttributeCount
	{	get { init(); return attributeCounts[ (int) HAPI_AttributeOwner.HAPI_ATTROWNER_POINT ]; }
		set { init(); attributeCounts[ (int) HAPI_AttributeOwner.HAPI_ATTROWNER_POINT ] = value; } }
	public int primitiveAttributeCount
	{	get { init(); return attributeCounts[ (int) HAPI_AttributeOwner.HAPI_ATTROWNER_PRIM ]; }
		set { init(); attributeCounts[ (int) HAPI_AttributeOwner.HAPI_ATTROWNER_PRIM ] = value; } }
	public int vertexAttributeCount
	{	get { init(); return attributeCounts[ (int) HAPI_AttributeOwner.HAPI_ATTROWNER_VERTEX ]; }
		set { init(); attributeCounts[ (int) HAPI_AttributeOwner.HAPI_ATTROWNER_VERTEX ] = value; } }
	public int detailAttributeCount
	{	get { init(); return attributeCounts[ (int) HAPI_AttributeOwner.HAPI_ATTROWNER_DETAIL ]; }
		set { init(); attributeCounts[ (int) HAPI_AttributeOwner.HAPI_ATTROWNER_DETAIL ] = value; } }
}
	
[ StructLayout( LayoutKind.Sequential ) ]
public struct HAPI_AttributeInfo
{		
	public HAPI_AttributeInfo( string attr_name )
	{
		exists 			= false;
		owner 			= HAPI_AttributeOwner.HAPI_ATTROWNER_INVALID;
		storage 		= HAPI_StorageType.HAPI_STORAGETYPE_INVALID;
		originalOwner	= HAPI_AttributeOwner.HAPI_ATTROWNER_INVALID;
		count 			= 0;
		tupleSize 		= 0;
	}
		
	[ MarshalAs( UnmanagedType.U1 ) ] public bool exists;
		
	public HAPI_AttributeOwner owner;
	public HAPI_StorageType storage;

	public HAPI_AttributeOwner originalOwner;

	public int count;
	public int tupleSize;
}
	
// MATERIALS ----------------------------------------------------------------------------------------------------

[ StructLayout( LayoutKind.Sequential ) ]
public struct HAPI_MaterialInfo
{
	public HAPI_NodeId nodeId;

	[ MarshalAs( UnmanagedType.U1 ) ]
	public bool exists;

	[ MarshalAs( UnmanagedType.U1 ) ]
	public bool hasChanged;
}

[ StructLayout( LayoutKind.Sequential ) ]
public struct HAPI_ImageFileFormat
{
	public HAPI_StringHandle nameSH;
	public HAPI_StringHandle descriptionSH;
	public HAPI_StringHandle defaultExtensionSH;

	// Accessors
	public string name
	{ get { return HoudiniHost.getString( nameSH ); } private set {} }
	public string description
	{ get { return HoudiniHost.getString( descriptionSH ); } private set {} }
	public string defaultExtension
	{ get { return HoudiniHost.getString( defaultExtensionSH ); } private set {} }
};

[ StructLayout( LayoutKind.Sequential ) ]
public struct HAPI_ImageInfo
{
	// Unlike the other members of this struct changing imageFileFormatNameSH and 
	// giving this struct back to HAPI_Host.setImageInfo() nothing will happen.
	// Use this member variable to derive which image file format will be used
	// by the HAPI_Host.extractImageTo...() functions if called with image_file_format_name
	// set to (null). This way, you can decide whether to ask for a file format
	// conversion (slower) or not (faster).
	public HAPI_StringHandle imageFileFormatNameSH; // Readonly

	public int xRes;
	public int yRes;

	public HAPI_ImageDataFormat dataFormat;

	[ MarshalAs( UnmanagedType.U1 ) ]
	public bool interleaved; // ex: true = RGBRGBRGB, false = RRRGGGBBB

	public HAPI_ImagePacking packing;

	[ MarshalAs( UnmanagedType.R8 ) ]
	public double gamma;

	// Accessors
	public string imageFileFormatName
	{ get { return HoudiniHost.getString( imageFileFormatNameSH ); } private set {} }

	// Utility
	public bool isImageFileFormat( string image_file_format_name )
	{ return ( imageFileFormatName == image_file_format_name ); }
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
		
	[ MarshalAs( UnmanagedType.R4 ) ]
	public float time;
		
	[ MarshalAs( UnmanagedType.R4 ) ]
	public float value;
		
	[ MarshalAs( UnmanagedType.R4 ) ]
	public float inTangent;
		
	[ MarshalAs( UnmanagedType.R4 ) ]
	public float outTangent;
	
}

// VOLUMES ------------------------------------------------------------------------------------------------------

/// This represents a volume primitive--sans the actual voxel values,
/// which can be retrieved on a per-tile basis
[ StructLayout( LayoutKind.Sequential ) ]
public struct HAPI_VolumeInfo
{
	public HAPI_StringHandle nameSH;

	public HAPI_VolumeType type;

	// Each voxel is identified with an index. The indices will range between:
	// [ ( minX, minY, minZ ), ( minX+xLength, minY+yLength, minZ+zLength ) )
	public int xLength;
	public int yLength;
	public int zLength;
	public int minX;
	public int minY;
	public int minZ;

	// Number of values per voxel.
	public int tupleSize;

	public HAPI_StorageType storage;

	// The dimensions of each tile.
	public int tileSize;

	// The transform of the volume with respect to the above lengths.

	[ MarshalAs( UnmanagedType.Struct ) ]
	public HAPI_Transform transform;

	[ MarshalAs( UnmanagedType.U1 ) ]
	public bool hasTaper;

	[ MarshalAs( UnmanagedType.R4 ) ]
	public float xTaper;

	[ MarshalAs( UnmanagedType.R4 ) ]
	public float yTaper;
};

/// A HAPI_VolumeTileInfo represents an 8x8x8 section of a volume with
/// bbox [(minX, minY, minZ), (minX+8, minY+8, minZ+8))
[ StructLayout( LayoutKind.Sequential ) ]
public struct HAPI_VolumeTileInfo
{
	public int minX;
	public int minY;
	public int minZ;

	[ MarshalAs( UnmanagedType.U1 ) ]
	public bool isValid;
};

// CURVES -------------------------------------------------------------------------------------------------------

[ StructLayout( LayoutKind.Sequential ) ]
public struct HAPI_CurveInfo
{
	public HAPI_CurveType curveType;
	public int curveCount;
	public int vertexCount;
	public int knotCount;

	[ MarshalAs( UnmanagedType.U1 ) ]
	public bool isPeriodic;

	[ MarshalAs( UnmanagedType.U1 ) ]
	public bool isRational;

	public int order; // Order of 1 is invalid. 0 means there is a varying order.

	[ MarshalAs( UnmanagedType.U1 ) ]
	public bool hasKnots;
};

// BASIC PRIMITIVES ---------------------------------------------------------------------------------------------

[ StructLayout( LayoutKind.Sequential ) ]
public struct HAPI_BoxInfo
{
	public HAPI_BoxInfo( bool initialize_fields )
	{
		center = new float[ HoudiniConstants.HAPI_POSITION_VECTOR_SIZE ];
		size = new float[ HoudiniConstants.HAPI_SCALE_VECTOR_SIZE ];
		rotation = new float[ HoudiniConstants.HAPI_EULER_VECTOR_SIZE ];
	}

	[ MarshalAs(
		UnmanagedType.ByValArray,
		SizeConst = HoudiniConstants.HAPI_POSITION_VECTOR_SIZE,
		ArraySubType = UnmanagedType.R4 ) ]
	public float[] center;

	[ MarshalAs(
		UnmanagedType.ByValArray,
		SizeConst = HoudiniConstants.HAPI_SCALE_VECTOR_SIZE,
		ArraySubType = UnmanagedType.R4 ) ]
	public float[] size;
		
	[ MarshalAs(
		UnmanagedType.ByValArray,
		SizeConst = HoudiniConstants.HAPI_EULER_VECTOR_SIZE,
		ArraySubType = UnmanagedType.R4 ) ]
	public float[] rotation;
}

[ StructLayout( LayoutKind.Sequential ) ]
public struct HAPI_SphereInfo
{
	public HAPI_SphereInfo( bool initialize_fields )
	{
		center = new float[ HoudiniConstants.HAPI_POSITION_VECTOR_SIZE ];
		radius = 0.0f;
	}

	[ MarshalAs(
		UnmanagedType.ByValArray,
		SizeConst = HoudiniConstants.HAPI_POSITION_VECTOR_SIZE,
		ArraySubType = UnmanagedType.R4 ) ]
	public float[] center;

	[ MarshalAs( UnmanagedType.R4 ) ]
	public float radius;
}
