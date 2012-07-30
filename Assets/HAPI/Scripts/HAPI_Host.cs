using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

namespace HAPI {

	public class HAPI_Host : MonoBehaviour {		
		static public bool HasScene() {
			return myHoudiniSceneExists;
		}
			
		static public string LoadOTL( string path ) {
			if ( myHoudiniSceneExists ) {
				Debug.Log( "Loading OTL: hip already exists" );
			} else {
				Debug.Log( "Loading OTL: new hip created" );
				
				/*
				HAPI_Initialize();
				
				HAPI_LoadHIPFile( "C:/donut.hip" );
				
				StringBuilder str = new StringBuilder( 2048 );
		
				HAPI_PrintNetwork( str );
				
				Debug.Log( str );
				
				HAPI_Cleanup();				
				*/
			}
			
			myHoudiniSceneExists = true;
			
			return "";
		}
		
		static public bool UnloadOTL( string innerPath ) {
			return true;	
		}
		
		static private bool myHoudiniSceneExists = false;
		
		/*
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_Initialize();
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_LoadHIPFile( string fileName );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_PrintNetwork( StringBuilder buffer );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		private static extern int HAPI_Cleanup();
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		public static extern int GetGeometry( string path, out HAPI_RawGeometry geo );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		public static extern int GetPointArray( string path, [Out] HAPI_RawVertex[] vertices, int start, int end );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		public static extern int GetPrimitveArray( string path, [Out] HAPI_RawPrimitive[] primitives, int start, int end );
		
		[ DllImport( "libHAPI", CallingConvention = CallingConvention.Cdecl ) ]
		public static extern int GetInstanceArray( string path, [Out] HAPI_RawInstance[] instances, int count );	
		*/
	
	}

}