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
 * 		Contains HAPI_Host which is a singleton class that maintains the singleton Houdini scene and 
 * 		all access to the Houdini runtime.
 * 
 */

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

namespace HAPI 
{
	public delegate void RepaintDelegate();

	public class HAPI_Error : System.Exception 
	{
		public HAPI_Error() 
		{
			myErrorMessage = "Unknown Error";
		}
		public HAPI_Error( string msg ) 
		{
			myErrorMessage = msg;
		}
		
		public void addMessagePrefix( string prefix )
		{
			myErrorMessage = prefix + ": " + myErrorMessage;
		}
		
		public void addMessageDetail( string detail )
		{
			myErrorMessage = myErrorMessage + "\n" + detail;
		}
		
		public override string ToString()
		{
			return myErrorMessage;
		}
		
		protected string myErrorMessage;
	}
	
	public class HAPI_ErrorProgressCancelled : HAPI_Error 
	{
		public override string ToString()
		{
			return "Load Cancelled by User";
		}
	}

	public class HAPI_ErrorIgnorable : HAPI_Error
	{
		public HAPI_ErrorIgnorable() 
		{
			myErrorMessage = "Ignorable Error";
		}
		public HAPI_ErrorIgnorable( string msg ) 
		{
			myErrorMessage = msg;
		}
	}
	
	/// <summary>
	/// 	Singleton Houdini host object that maintains the singleton Houdini scene and all access to the
	/// 	Houdini runtime.
	/// </summary>
	[ InitializeOnLoad ]
	public static partial class HAPI_Host
	{
		private const string myDefaultCollisionGroupName					= "collision_geo";
		private const string myDefaultRenderedCollisionGroupName			= "rendered_collision_geo";

		private const bool myDefaultEnableDragAndDrop						= true;
		private const bool myDefaultEnableSupportWarnings					= true;

		private const bool myDefaultEnableCooking							= true;
		private const bool myDefaultAutoSelectParent						= true;
		private const bool myDefaultHideGeometryOnLinking					= true;
		private const bool myDefaultAutoPinInstances						= true;
		private const float myDefaultPinSize								= 1.0f;

		private static KeyCode myDefaultAddingPointsModeHotKey				= KeyCode.LeftShift;
		private static Color myDefaultAddingPointsModeColour				= Color.yellow;
		private static KeyCode myDefaultEditingPointsModeHotKey				= KeyCode.LeftControl;
		private static Color myDefaultEditingPointsModeColour				= new Color( 0.7f, 0.7f, 0.9f, 1.0f );

		private static Color myDefaultGuideWireframeColour					= new Color( 0.1f, 0.1f, 0.1f, 1.0f );
		private static Color myDefaultUnselectableGuideWireframeColour		= new Color( 0.1f, 0.1f, 0.1f, 1.0f );
		private static Color myDefaultUnselectedGuideWireframeColour		= Color.white;
		private static Color myDefaultSelectedGuideWireframeColour			= Color.yellow;
		private const float myDefaultMinDistanceForPointSelection			= 8.0f;
		private const float myDefaultGuideMinDistanceForMidPointInsertion	= 5.0f;

		private const int myDefaultCurvePrimitiveTypeDefault				= 1;
		private const int myDefaultCurveMethodDefault						= 1;

		static HAPI_Host()
		{
			EditorApplication.update				+= update;
			EditorApplication.playmodeStateChanged	+= playmodeStateChanged;

			if ( !isRuntimeInitialized() )
			{
				prHoudiniSceneExists		= false;
				prMidPlaymodeStateChange	= false;

				initialize();
			}

			// Preferences
			
			setString(	"HAPI_CollisionGroupName", myDefaultCollisionGroupName, true );
			setString(	"HAPI_RenderedCollisionGroupName", myDefaultRenderedCollisionGroupName, true );

			setBool(	"HAPI_EnableDragAndDrop", myDefaultEnableDragAndDrop, true );
			setBool(	"HAPI_EnableSupportWarnings", myDefaultEnableSupportWarnings, true );
			
			setBool(	"HAPI_EnableCooking", myDefaultEnableCooking, true );
			setBool(	"HAPI_AutoSelectParent", myDefaultAutoSelectParent, true );
			setBool(	"HAPI_HideGeometryOnLinking", myDefaultHideGeometryOnLinking, true );
			setBool(	"HAPI_AutoPinInstances", myDefaultAutoPinInstances, true );
			setFloat(	"HAPI_PinSize", myDefaultPinSize, true );

			setKeyCode( "HAPI_AddingPointsHotKey", myDefaultAddingPointsModeHotKey, true );
			setColour(	"HAPI_AddingPointsModeColour", myDefaultAddingPointsModeColour, true );
			setKeyCode( "HAPI_EditingPointsHotKey", myDefaultEditingPointsModeHotKey, true );
			setColour(	"HAPI_EditingPointsModeColour", myDefaultEditingPointsModeColour, true );

			setColour(	"HAPI_GuideWireframeColour", myDefaultGuideWireframeColour, true );
			setColour(	"HAPI_UnselectableGuideWireframeColour", myDefaultUnselectableGuideWireframeColour, true );
			setColour(	"HAPI_UnselectedGuideWireframeColour", myDefaultUnselectedGuideWireframeColour, true );
			setColour(	"HAPI_SelectedGuideWireframeColour", myDefaultSelectedGuideWireframeColour, true );
			setFloat(	"HAPI_MinDistanceForPointSelection", myDefaultMinDistanceForPointSelection, true );
			setFloat(	"HAPI_GuideMinDistanceForMidPointInsertion", myDefaultGuideMinDistanceForMidPointInsertion, true );

			setInt(		"HAPI_CurvePrimitiveTypeDefault", myDefaultCurvePrimitiveTypeDefault, true );
			setInt(		"HAPI_CurveMethodDefault", myDefaultCurveMethodDefault, true );

			myRepaintDelegate = null;
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Public

		public static bool prHoudiniSceneExists {
												get { return getBool( "HAPI_HoudiniSceneExists" ); } 
												private set { setBool( "HAPI_HoudiniSceneExists", value ); } }
		public static bool prMidPlaymodeStateChange {	
												get { return getBool( "HAPI_MidPlaymodeStateChange" ); } 
												private set { setBool( "HAPI_MidPlaymodeStateChange", value ); } }

		public static string prCollisionGroupName {
												get { return getString( "HAPI_CollisionGroupName" ); }
												set { setString( "HAPI_CollisionGroupName", value ); } }
		public static string prRenderedCollisionGroupName {
												get { return getString( "HAPI_RenderedCollisionGroupName" ); }
												set { setString( "HAPI_RenderedCollisionGroupName", value ); } }

		public static bool prEnableDragAndDrop {
												get { return getBool( "HAPI_EnableDragAndDrop" ); } 
												set { setBool( "HAPI_EnableDragAndDrop", value ); } }
		public static bool prEnableSupportWarnings {
												get { return getBool( "HAPI_EnableSupportWarnings" ); } 
												set { setBool( "HAPI_EnableSupportWarnings", value ); } }

		public static bool prEnableCooking {
												get { return getBool( "HAPI_EnableCooking" ); }
												set { setBool( "HAPI_EnableCooking", value ); } }
		public static bool prAutoSelectParent {
												get { return getBool( "HAPI_AutoSelectParent" ); } 
												set { setBool( "HAPI_AutoSelectParent", value ); } }
		public static bool prHideGeometryOnLinking {
												get { return getBool( "HAPI_HideGeometryOnLinking" ); } 
												set { setBool( "HAPI_HideGeometryOnLinking", value ); } }
		public static bool prAutoPinInstances {
												get { return getBool( "HAPI_AutopinInstances" ); } 
												set { setBool( "HAPI_AutopinInstances", value ); } }
		
		public static float prPinSize {
												get { return getFloat( "HAPI_PinSize" ); }
												set { setFloat( "HAPI_PinSize", value ); } }

		public static KeyCode prAddingPointsModeHotKey {
												get { KeyCode key = getKeyCode( "HAPI_AddingPointsModeHotKey" );
													  if ( key == KeyCode.None ) {
														  key = myDefaultAddingPointsModeHotKey;
														  setKeyCode( "HAPI_AddingPointsModeHotKey", key ); }
													  return key; }
												set { setKeyCode( "HAPI_AddingPointsModeHotKey", value ); } }
		public static Color prAddingPointsModeColour {
												get { return getColour( "HAPI_AddingPointsModeColour" ); }
												set { setColour( "HAPI_AddingPointsModeColour", value ); } }
		public static KeyCode prEditingPointsModeHotKey {
												get { KeyCode key = getKeyCode( "HAPI_EditingPointsModeHotKey" );
													  if ( key == KeyCode.None ) {
														key = myDefaultEditingPointsModeHotKey;
														setKeyCode( "HAPI_EditingPointsModeHotKey", key ); }
													  return key; }
												set { setKeyCode( "HAPI_EditingPointsModeHotKey", value ); } }
		public static Color prEditingPointsModeColour {
												get { return getColour( "HAPI_EditingPointsModeColour" ); }
												set { setColour( "HAPI_EditingPointsModeColour", value ); } }

		public static Color prGuideWireframeColour {
												get { return getColour( "HAPI_GuideWireframeColour" ); }
												set { setColour( "HAPI_GuideWireframeColour", value ); } }
		public static Color prUnselectableGuideWireframeColour {
												get { return getColour( "HAPI_UnselectableGuideWireframeColour" ); }
												set { setColour( "HAPI_UnselectableGuideWireframeColour", value ); } }
		public static Color prUnselectedGuideWireframeColour {
												get { return getColour( "HAPI_UnselectedGuideWireframeColour" ); }
												set { setColour( "HAPI_UnselectedGuideWireframeColour", value ); } }
		public static Color prSelectedGuideWireframeColour {
												get { return getColour( "HAPI_SelectedGuideWireframeColour" ); }
												set { setColour( "HAPI_SelectedGuideWireframeColour", value ); } }
		public static float prMinDistanceForPointSelection {
												get { return getFloat( "HAPI_MinDistanceForPointSelection" ); }
												set { setFloat( "HAPI_MinDistanceForPointSelection", value ); } }
		public static float prGuideMinDistanceForMidPointInsertion {
												get { return getFloat( "HAPI_GuideMinDistanceForMidPointInsertion" ); }
												set { setFloat( "HAPI_GuideMinDistanceForMidPointInsertion", value ); } }

		public static int prCurvePrimitiveTypeDefault {
												get { return getInt( "HAPI_CurvePrimitiveTypeDefault" ); }
												set { setInt( "HAPI_CurvePrimitiveTypeDefault", value ); } }
		public static int prCurveMethodDefault {
												get { return getInt( "HAPI_CurveMethodDefault" ); }
												set { setInt( "HAPI_CurveMethodDefault", value ); } }

		public static RepaintDelegate			myRepaintDelegate;

		public static void revertAllSettingsToDefaults()
		{
			prCollisionGroupName					= myDefaultCollisionGroupName;
			prRenderedCollisionGroupName			= myDefaultRenderedCollisionGroupName;
			
			prEnableDragAndDrop						= myDefaultEnableDragAndDrop;
			prEnableSupportWarnings					= myDefaultEnableSupportWarnings;

			prEnableCooking							= myDefaultEnableCooking;
			prAutoSelectParent						= myDefaultAutoSelectParent;
			prHideGeometryOnLinking					= myDefaultHideGeometryOnLinking;
			prAutoPinInstances						= myDefaultAutoPinInstances;
			prPinSize 								= myDefaultPinSize;

			prAddingPointsModeHotKey				= myDefaultAddingPointsModeHotKey;
			prAddingPointsModeColour				= myDefaultAddingPointsModeColour;
			prEditingPointsModeHotKey				= myDefaultEditingPointsModeHotKey;
			prEditingPointsModeColour				= myDefaultEditingPointsModeColour;

			prGuideWireframeColour					= myDefaultGuideWireframeColour;
			prUnselectableGuideWireframeColour		= myDefaultUnselectableGuideWireframeColour;
			prUnselectedGuideWireframeColour		= myDefaultUnselectedGuideWireframeColour;
			prSelectedGuideWireframeColour			= myDefaultSelectedGuideWireframeColour;
			prMinDistanceForPointSelection			= myDefaultMinDistanceForPointSelection;
			prGuideMinDistanceForMidPointInsertion	= myDefaultGuideMinDistanceForMidPointInsertion;
		}

		public static bool hasScene() 
		{
			return prHoudiniSceneExists;
		}
		
		public static void saveScene( string file_name )
		{
			HAPI_SaveHIPFile( file_name );
		}
		
		public static int loadOTL( string path ) 
		{
			if ( !initialize() )
				throw new HAPI_Error( "DLL Not Found." );

			string textures_path = Application.dataPath + "/Textures";
			
			int asset_id = -1;
			HAPI_Result status_code = (HAPI_Result) HAPI_LoadOTLFile( path, textures_path,
																	  HAPI_Constants.HAPI_MIN_VERTICES_PER_FACE,
																	  HAPI_Constants.HAPI_MAX_VERTICES_PER_FACE,
																	  ref asset_id );
			processStatusCode( status_code );

			return asset_id;
		}
		
		public static void loadHip( string path ) 
		{
			if ( !initialize() )
				throw new HAPI_Error( "DLL Not Found." );

			string textures_path = Application.dataPath + "/Textures";
																
			HAPI_Result status_code = (HAPI_Result) HAPI_LoadHIPFile( path, textures_path, 
																	  HAPI_Constants.HAPI_MIN_VERTICES_PER_FACE,
																	  HAPI_Constants.HAPI_MAX_VERTICES_PER_FACE );
			
			processStatusCode( status_code );						
						
		}
		
		public static int getAssetCountFromLoadHip() 
		{
			int num_assets = 0;
			
			HAPI_Result status_code = (HAPI_Result) HAPI_GetAssetCountFromLoadHIPFile( ref num_assets );
			
			processStatusCode( status_code );
			
			return num_assets;
						
		}
		
		public static void getAssetIdsFromLoadHIPFile( int [] assetIds )
		{
			HAPI_Result status_code = (HAPI_Result) HAPI_GetAssetIdsFromLoadHIPFile( assetIds );
			
			processStatusCode( status_code );
			
		}
		
		public static int createCurve()
		{
			if ( !initialize() )
				throw new HAPI_Error( "DLL Not Found." );

			int asset_id = -1;
			HAPI_Result status_code = (HAPI_Result) HAPI_CreateCurve( ref asset_id );
			processStatusCode( status_code );

			return asset_id;
		}
		
		public static bool unloadOTL( int asset_id ) 
		{
			if ( asset_id < 0 )
				return false;
			
			int result = HAPI_UnloadOTLFile( asset_id );
			
			try
			{
				processStatusCode( (HAPI_Result) result );
			}
			catch ( HAPI_Error error )
			{
				Debug.LogError( "Asset failed to unload: " + error.ToString() );
			}
			
			return true;
		}

		public static bool isRealDestroy()
		{
			return !EditorApplication.isPlayingOrWillChangePlaymode && !prMidPlaymodeStateChange;
		}

		public static bool initialize()
		{
			string otls_path = Application.dataPath + "/OTLs/Scanned";
			string dsos_path = "@/dso_^;@/dso;" + Application.dataPath + "/DSOs";

			if ( !prHoudiniSceneExists )
			{
				HAPI_Result status_code;
				try
				{
					status_code = (HAPI_Result) HAPI_Initialize( HAPI_SetPath.prHoudiniPath, otls_path,
																 dsos_path, true, -1 );
					if ( status_code != HAPI_Result.HAPI_RESULT_ALREADY_INITIALIZED )
						processStatusCode( status_code );
				}
				catch ( HAPI_Error error )
				{
					Debug.LogError( error.ToString() );
					return false;
				}
				catch ( System.Exception error )
				{
					Debug.LogError( error.ToString() );
					return false;
				}

				prHoudiniSceneExists = true;
			}

			return true;
		}

		public static void throwRuntimeError()
		{
			int buffer_size = 4000;
			HAPI_GetStatusStringBufLength( (int) HAPI_StatusType.HAPI_STATUS_RESULT, out buffer_size );
			StringBuilder error_str = new StringBuilder( buffer_size );
			HAPI_GetStatusString( (int) HAPI_StatusType.HAPI_STATUS_RESULT, error_str );
			throw new HAPI_Error( error_str.ToString() );
		}

		public static bool isRuntimeInitialized()
		{
			if ( !HAPI_SetPath.prIsPathSet )
				return false;
			else
			{
				try
				{
					processStatusCode( (HAPI_Result) HAPI_IsInitialized() );
					return true;
				}
				catch
				{
					return false;
				}
			}
		}
		
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Private

		private static void update()
		{

		}

		private static void playmodeStateChanged()
		{
			prMidPlaymodeStateChange = !prMidPlaymodeStateChange;
			setTime( 0.0f );
		}

		private static bool hasCallFailed( HAPI_Result code )
		{
			return ( (int) code > 0 );
		}
		
		private static void processStatusCode( HAPI_Result code )
		{
			if ( hasCallFailed( code ) )
				throwRuntimeError();
		}

		private static int getInt( string name )
		{
			return EditorPrefs.GetInt( name );
		}
		private static void setInt( string name, int value )
		{
			setInt( name, value, false );
		}
		private static void setInt( string name, int value, bool only_if_new )
		{
			if ( !only_if_new || !EditorPrefs.HasKey( name ) )
				EditorPrefs.SetInt( name, value );
		}

		private static bool getBool( string name )
		{
			return EditorPrefs.GetInt( name ) == 0 ? false : true;
		}
		private static void setBool( string name, bool value )
		{
			setBool( name, value, false );
		}
		private static void setBool( string name, bool value, bool only_if_new )
		{
			if ( !only_if_new || !EditorPrefs.HasKey( name ) )
				EditorPrefs.SetInt( name, value ? 1 : 0 );
		}

		private static float getFloat( string name )
		{
			return EditorPrefs.GetFloat( name );
		}
		private static void setFloat( string name, float value )
		{
			setFloat( name, value, false );
		}
		private static void setFloat( string name, float value, bool only_if_new )
		{
			if ( !only_if_new || !EditorPrefs.HasKey( name ) )
				EditorPrefs.SetFloat( name, value );
		}

		private static string getString( string name )
		{
			return EditorPrefs.GetString( name );
		}
		private static void setString( string name, string value )
		{
			setString( name, value, false );
		}
		private static void setString( string name, string value, bool only_if_new )
		{
			if ( !only_if_new || !EditorPrefs.HasKey( name ) )
				EditorPrefs.SetString( name, value );
		}

		private static KeyCode getKeyCode( string name )
		{
			return (KeyCode) EditorPrefs.GetInt( name );
		}
		private static void setKeyCode( string name, KeyCode value )
		{
			setKeyCode( name, value, false );
		}
		private static void setKeyCode( string name, KeyCode value, bool only_if_new )
		{
			if ( !only_if_new || !EditorPrefs.HasKey( name ) )
				EditorPrefs.SetInt( name, (int) value );
		}

		private static Color getColour( string name )
		{
			return new Color( getFloat( name + "_r" ), getFloat( name + "_g" ), 
							  getFloat( name + "_b" ), getFloat( name + "_a" ) );
		}
		private static void setColour( string name, Color value )
		{
			setColour( name, value, false );
		}
		private static void setColour( string name, Color value, bool only_if_new )
		{
			if ( !only_if_new || 
					!( EditorPrefs.HasKey( name + "_r" ) || EditorPrefs.HasKey( name + "_g" ) ||
					   EditorPrefs.HasKey( name + "_b" ) || EditorPrefs.HasKey( name + "_a" ) ) )
			{
				setFloat( name + "_r", value.r );
				setFloat( name + "_g", value.g );
				setFloat( name + "_b", value.b );
				setFloat( name + "_a", value.a );
			}
		}
	}

} // namespace HAPI
