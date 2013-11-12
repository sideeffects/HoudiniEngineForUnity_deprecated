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
 * 		Continuation of HAPI_Host class definition. This is where all the string marshalling and caching happenes.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

namespace HAPI 
{	
	/// <summary>
	/// 	Singleton Houdini host object that maintains the singleton Houdini scene and all access to the
	/// 	Houdini runtime.
	/// </summary>
	public static partial class HAPI_Host
	{
#if UNITY_EDITOR
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Public
		
		public static string getString( int string_handle )
		{
			int length = 0;
			getStringBufLength( string_handle, out length );
			
			if ( length <= 0 )
				return "";
			
			StringBuilder string_builder = new StringBuilder( length );
			getString( string_handle, string_builder, length );
			
			string string_value = string_builder.ToString();
			
			return string_value;
		}
		
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Private
		
		// TODO: We would like to cache some strings eventually.
#endif // UNITY_EDITOR	
	}

}
