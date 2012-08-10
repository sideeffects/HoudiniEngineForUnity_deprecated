using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

namespace HAPI {

	public class HAPI_Host : MonoBehaviour {
		// Public
		
		static public bool HasScene() {
			return myHoudiniSceneExists;
		}
			
		static public HAPI_AssetInfo LoadOTL( string path ) {
			if ( myHoudiniSceneExists ) {
				Debug.Log( "Loading OTL: hip already exists" );
			} else {
				Debug.Log( "Loading OTL: new hip created" );
								
				HAPI_Initialize();
							
				//HAPI_LoadHIPFile( "C:/donut.hip" );				
				//HAPI_Cleanup();
			
				myHoudiniSceneExists = true;
			}
			
			HAPI_AssetInfo assetInfo = new HAPI_AssetInfo();
			int result = HAPI_LoadOTLFile( path, out assetInfo );
			
			StringBuilder str = new StringBuilder( 2048 );		
			HAPI_PrintNetwork( str );				
			Debug.Log( str );
			
			if ( result > 0 ) {
				Debug.LogError( "OTL File Failed to Load" );
				return assetInfo;
			}
			
			
			
			Debug.Log( "Asset Loaded - Path: " + assetInfo.assetInstancePath + ", ID: " + assetInfo.id );			
			return assetInfo;
		}
		
		static public bool UnloadOTL( int assetId ) {
			if ( assetId < 0 )
				return false;
			
			int result = HAPI_UnloadOTLFile( assetId );
			
			if ( result > 0 ) {
				Debug.LogError( "OTL File Failed to Unload" );
				return false;
			}
			
			return true;	
		}
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		public static extern int HAPI_GetParameters( int assetId, [Out] HAPI_Parameter[] parameters, int start, int end );
				
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		public static extern int HAPI_SetParameters( int assetId, [Out] HAPI_Parameter[] parameters, int start, int end );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		public static extern int HAPI_GetObjects( int assetId, [Out] HAPI_ObjectInfo[] objects, int start, int end );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		public static extern int HAPI_GetObjectTransforms( int assetId, [Out] HAPI_Transform[] transforms, int start, int end );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		public static extern int HAPI_GetGeometryInfo( int assetId, int objectId, out HAPI_RawGeometry geo );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		public static extern int HAPI_GetVertexArray( int assetId, int objectId, [Out] HAPI_RawVertex[] vertices, int start, int end );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		public static extern int HAPI_GetPrimitveArray( int assetId, int objectId, [Out] HAPI_RawPrimitive[] primitives, int start, int end );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		public static extern int HAPI_GetInstanceArray( int assetId, int objectId, [Out] HAPI_RawInstance[] instances, int count );	
		
		// Private
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_Initialize();
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_LoadHIPFile( string fileName );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_LoadOTLFile( string fileName, out HAPI_AssetInfo assetInfo );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_UnloadOTLFile( int assetId );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_PrintNetwork( StringBuilder buffer );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_Cleanup();
		
		static private bool myHoudiniSceneExists = false;
		
	
	}

}