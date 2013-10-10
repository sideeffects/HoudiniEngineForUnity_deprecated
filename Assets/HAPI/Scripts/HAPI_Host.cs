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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

namespace HAPI 
{
	public delegate void RepaintDelegate();
	public delegate void DeselectionDelegate();

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

	public class HAPI_ErrorInvalidArgument : HAPI_Error
	{
		public HAPI_ErrorInvalidArgument()
		{
			myErrorMessage = "Invalid arugment error.";
		}
		public HAPI_ErrorInvalidArgument( string msg ) : base( msg ) {}
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
		// Global Settings Defaults ---------------------------------------------------------------------------------

		private const string myDefaultCollisionGroupName					= "collision_geo";
		private const string myDefaultRenderedCollisionGroupName			= "rendered_collision_geo";

		private const string myDefaultUnityMaterialAttribName				= "unity_material";
		private const string myDefaultUnitySubMaterialNameAttribName		= "unity_sub_material_name";
		private const string myDefaultUnitySubMaterialIndexAttribName		= "unity_sub_material_index";
		private const bool myDefaultDontCreateTextureFiles					= false;
		private const bool myDefaultExtractTexturesInRawFormat				= false;

		private const float myDefaultPinSize								= 1.0f;
		private static Color myDefaultPinColour								= new Color( 0.7f, 0.0f, 0.0f, 1.0f );
		private const bool myDefaultAutoPinInstances						= true;

		private const bool myDefaultEnableDragAndDrop						= true;
		private const bool myDefaultEnableSupportWarnings					= true;

		public const bool myDefaultAutoSelectAssetRootNode					= true;
		public const bool myDefaultHideGeometryOnLinking					= true;

		public const bool myDefaultGenerateTangents							= true;

		public const bool myDefaultEnableCooking							= true;
		public const bool myDefaultCookingTriggersDownCooks					= true;
		public const bool myDefaultPlaymodePerFrameCooking					= false;
		public const bool myDefaultPushUnityTransformToHoudini				= true;
		public const bool myDefaultTransformChangeTriggersCooks				= false;

		private const KeyCode myDefaultAddingPointsModeHotKey				= KeyCode.LeftShift;
		private static Color myDefaultAddingPointsModeColour				= Color.yellow;

		private const KeyCode myDefaultEditingPointsModeHotKey				= KeyCode.LeftControl;
		private static Color myDefaultEditingPointsModeColour				= new Color( 0.7f, 0.7f, 0.9f, 1.0f );

		private static Color myDefaultWireframeColour						= new Color( 0.0f, 1.0f, 0.0f, 1.0f );
		private static Color myDefaultGuideWireframeColour					= new Color( 0.1f, 0.1f, 0.1f, 1.0f );
		private static Color myDefaultUnselectableGuideWireframeColour		= new Color( 0.1f, 0.1f, 0.1f, 1.0f );
		private static Color myDefaultUnselectedGuideWireframeColour		= Color.white;
		private static Color myDefaultSelectedGuideWireframeColour			= Color.yellow;

		private const float myDefaultGuidePointSize							= 10.0f;
		private const float myDefaultMinDistanceForPointSelection			= 8.0f;
		private const float myDefaultGuideMinDistanceForMidPointInsertion	= 5.0f;

		private const int myDefaultCurvePrimitiveTypeDefault				= 1;
		private const int myDefaultCurveMethodDefault						= 1;

		private const bool myDefaultEnablePointsAsParticles 				= true;

		// Global Settings Initializations --------------------------------------------------------------------------

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

			setString(	"HAPI_UnityMaterialAttribName", myDefaultUnityMaterialAttribName, true );
			setString(	"HAPI_UnitySubMaterialNameAttribName", myDefaultUnitySubMaterialNameAttribName, true );
			setString(	"HAPI_UnitySubMaterialIndexAttribName", myDefaultUnitySubMaterialIndexAttribName, true );
			setBool(	"HAPI_DontCreateTextureFiles", myDefaultDontCreateTextureFiles, true );
			setBool(	"HAPI_ExtractTexturesInRawFormat", myDefaultExtractTexturesInRawFormat, true );

			setFloat(	"HAPI_PinSize", myDefaultPinSize, true );
			setColour(	"HAPI_PinColour", myDefaultPinColour, true );
			setBool(	"HAPI_AutoPinInstances", myDefaultAutoPinInstances, true );

			setBool(	"HAPI_EnableDragAndDrop", myDefaultEnableDragAndDrop, true );
			setBool(	"HAPI_EnableSupportWarnings", myDefaultEnableSupportWarnings, true );
			
			setBool(	"HAPI_AutoSelectAssetRootNode", myDefaultAutoSelectAssetRootNode, true );
			setBool(	"HAPI_HideGeometryOnLinking", myDefaultHideGeometryOnLinking, true );

			setBool(	"HAPI_GenerateTangents", myDefaultGenerateTangents, true );

			setBool(	"HAPI_EnableCooking", myDefaultEnableCooking, true );
			setBool(	"HAPI_CookingTriggersDownCooks", myDefaultCookingTriggersDownCooks, true );
			setBool(	"HAPI_PlaymodePerFrameCooking", myDefaultPlaymodePerFrameCooking, true );
			setBool(	"HAPI_PushUnityTransformToHoudini", myDefaultPushUnityTransformToHoudini, true );
			setBool(	"HAPI_TransformChangeTriggersCooks", myDefaultTransformChangeTriggersCooks, true );

			setKeyCode( "HAPI_AddingPointsHotKey", myDefaultAddingPointsModeHotKey, true );
			setColour(	"HAPI_AddingPointsModeColour", myDefaultAddingPointsModeColour, true );

			setKeyCode( "HAPI_EditingPointsHotKey", myDefaultEditingPointsModeHotKey, true );
			setColour(	"HAPI_EditingPointsModeColour", myDefaultEditingPointsModeColour, true );
			
			setColour(	"HAPI_WireframeColour", myDefaultWireframeColour, true );
			setColour(	"HAPI_GuideWireframeColour", myDefaultGuideWireframeColour, true );
			setColour(	"HAPI_UnselectableGuideWireframeColour", myDefaultUnselectableGuideWireframeColour, true );
			setColour(	"HAPI_UnselectedGuideWireframeColour", myDefaultUnselectedGuideWireframeColour, true );
			setColour(	"HAPI_SelectedGuideWireframeColour", myDefaultSelectedGuideWireframeColour, true );

			setFloat(	"HAPI_GuidePointSize", myDefaultGuidePointSize, true );
			setFloat(	"HAPI_MinDistanceForPointSelection", myDefaultMinDistanceForPointSelection, true );
			setFloat(	"HAPI_GuideMinDistanceForMidPointInsertion", myDefaultGuideMinDistanceForMidPointInsertion, true );

			setInt(		"HAPI_CurvePrimitiveTypeDefault", myDefaultCurvePrimitiveTypeDefault, true );
			setInt(		"HAPI_CurveMethodDefault", myDefaultCurveMethodDefault, true );

			setBool( 	"HAPI_EnablePointsAsParticles", myDefaultEnablePointsAsParticles, true );

			myRepaintDelegate			= null;
			myDeselectionDelegate		= null;
			mySelectionTarget			= null;
			mySelectionTargetIsPrefab 	= false;
		}

		/////////////////////////////////////////////////////////////////////////////////////////////////////////////
		// Public

		// Global Settings Properties -------------------------------------------------------------------------------

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

		public static string prUnityMaterialAttribName {
												get { return getString( "HAPI_UnityMaterialAttribName" ); }
												set { setString( "HAPI_UnityMaterialAttribName", value ); } }
		public static string prUnitySubMaterialNameAttribName {
												get { return getString( "HAPI_UnitySubMaterialNameAttribName" ); }
												set { setString( "HAPI_UnitySubMaterialNameAttribName", value ); } }
		public static string prUnitySubMaterialIndexAttribName {
												get { return getString( "HAPI_UnitySubMaterialIndexAttribName" ); }
												set { setString( "HAPI_UnitySubMaterialIndexAttribName", value ); } }
		public static bool prDontCreateTextureFiles {
												get { return getBool( "HAPI_DontCreateTextureFiles" ); }
												set { setBool( "HAPI_DontCreateTextureFiles", value ); } }
		public static bool prExtractTexturesInRawFormat {
												get { return getBool( "HAPI_ExtractTexturesInRawFormat" ); }
												set { setBool( "HAPI_ExtractTexturesInRawFormat", value ); } }

		public static float prPinSize {
												get { return getFloat( "HAPI_PinSize" ); }
												set { setFloat( "HAPI_PinSize", value ); } }
		public static Color prPinColour {
												get { return getColour( "HAPI_PinColour" ); }
												set { setColour( "HAPI_PinColour", value ); } }
		public static bool prAutoPinInstances {
												get { return getBool( "HAPI_AutopinInstances" ); } 
												set { setBool( "HAPI_AutopinInstances", value ); } }

		public static bool prEnableDragAndDrop {
												get { return getBool( "HAPI_EnableDragAndDrop" ); } 
												set { setBool( "HAPI_EnableDragAndDrop", value ); } }
		public static bool prEnableSupportWarnings {
												get { return getBool( "HAPI_EnableSupportWarnings" ); } 
												set { setBool( "HAPI_EnableSupportWarnings", value ); } }

		public static bool prAutoSelectAssetRootNode {
												get { return getBool( "HAPI_AutoSelectAssetRootNode" ); } 
												set { setBool( "HAPI_AutoSelectAssetRootNode", value ); } }
		public static bool prHideGeometryOnLinking {
												get { return getBool( "HAPI_HideGeometryOnLinking" ); } 
												set { setBool( "HAPI_HideGeometryOnLinking", value ); } }

		public static bool prGenerateTangents {
												get { return getBool( "HAPI_GenerateTangents" ); } 
												set { setBool( "HAPI_GenerateTangents", value ); } }

		public static bool prEnableCooking {
												get { return getBool( "HAPI_EnableCooking" ); }
												set { setBool( "HAPI_EnableCooking", value ); } }
		public static bool prCookingTriggersDownCooks {
												get { return getBool( "HAPI_CookingTriggersDownCooks" ); }
												set { setBool( "HAPI_CookingTriggersDownCooks", value ); } }
		public static bool prPlaymodePerFrameCooking {
												get { return getBool( "HAPI_PlaymodePerFrameCooking" ); }
												set { setBool( "HAPI_PlaymodePerFrameCooking", value ); } }
		public static bool prPushUnityTransformToHoudini {
												get { return getBool( "HAPI_PushUnityTransformToHoudini" ); }
												set { setBool( "HAPI_PushUnityTransformToHoudini", value ); } }
		public static bool prTransformChangeTriggersCooks {
												get { return getBool( "HAPI_TransformChangeTriggersCooks" ); }
												set { setBool( "HAPI_TransformChangeTriggersCooks", value ); } }

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

		public static Color prWireframeColour {
												get { return getColour( "HAPI_WireframeColour" ); }
												set { setColour( "HAPI_WireframeColour", value ); } }
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

		public static float prGuidePointSize {
												get { return getFloat( "HAPI_GuidePointSize" ); }
												set { setFloat( "HAPI_GuidePointSize", value ); } }
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

		public static bool prEnablePointsAsParticles {
												get { return getBool( "HAPI_EnablePointsAsParticles" ); }
												set { setBool( "HAPI_EnablePointsAsParticles", value ); } }

		// Global Settings Default Checks ---------------------------------------------------------------------------

		public static bool isCollisionGroupNameDefault()
												{ return	prCollisionGroupName == 
															myDefaultCollisionGroupName; }
		public static bool isRenderedCollisionGroupNameDefault()
												{ return	prRenderedCollisionGroupName == 
															myDefaultRenderedCollisionGroupName; }

		public static bool isUnityMaterialAttribNameDefault()
												{ return	prUnityMaterialAttribName ==
															myDefaultUnityMaterialAttribName; }
		public static bool isUnitySubMaterialNameAttribNameDefault()
												{ return	prUnitySubMaterialNameAttribName ==
															myDefaultUnitySubMaterialNameAttribName; }
		public static bool isUnitySubMaterialIndexAttribNameDefault()
												{ return	prUnitySubMaterialIndexAttribName ==
															myDefaultUnitySubMaterialIndexAttribName; }
		public static bool isDontCreateTextureFilesDefault()
												{ return	prDontCreateTextureFiles ==
															myDefaultDontCreateTextureFiles; }
		public static bool isExtractTexturesInRawFormatDefault()
												{ return	prExtractTexturesInRawFormat ==
															myDefaultExtractTexturesInRawFormat; }

		public static bool isPinSizeDefault()
												{ return	prPinSize == 
															myDefaultPinSize; }
		public static bool isPinColourDefault()
												{ return	prPinColour == 
															myDefaultPinColour; }
		public static bool isAutoPinInstancesDefault()
												{ return	prAutoPinInstances == 
															myDefaultAutoPinInstances; }

		public static bool isEnableDragAndDropDefault()
												{ return	prEnableDragAndDrop == 
															myDefaultEnableDragAndDrop; }
		public static bool isEnableSupportWarningsDefault()
												{ return	prEnableSupportWarnings == 
															myDefaultEnableSupportWarnings; }

		public static bool isAutoSelectAssetRootNodeDefault()
												{ return	prAutoSelectAssetRootNode == 
															myDefaultAutoSelectAssetRootNode; }
		public static bool isHideGeometryOnLinkingDefault()
												{ return	prHideGeometryOnLinking == 
															myDefaultHideGeometryOnLinking; }

		public static bool isGenerateTangentsDefault()
												{ return	prGenerateTangents ==
															myDefaultGenerateTangents; }

		public static bool isEnableCookingDefault()
												{ return	prEnableCooking == 
															myDefaultEnableCooking; }
		public static bool isCookingTriggersDownCooksDefault()
												{ return	prCookingTriggersDownCooks == 
															myDefaultCookingTriggersDownCooks; }
		public static bool isPlaymodePerFrameCookingDefault()
												{ return	prPlaymodePerFrameCooking == 
															myDefaultPlaymodePerFrameCooking; }
		public static bool isPushUnityTransformToHoudiniDefault()
												{ return	prPushUnityTransformToHoudini == 
															myDefaultPushUnityTransformToHoudini; }
		public static bool isTransformChangeTriggersCooksDefault()
												{ return	prTransformChangeTriggersCooks == 
															myDefaultTransformChangeTriggersCooks; }

		public static bool isAddingPointsModeHotKeyDefault()
												{ return	prAddingPointsModeHotKey == 
															myDefaultAddingPointsModeHotKey; }
		public static bool isAddingPointsModeColourDefault()
												{ return	prAddingPointsModeColour == 
															myDefaultAddingPointsModeColour; }

		public static bool isEditingPointsModeHotKeyDefault()
												{ return	prEditingPointsModeHotKey == 
															myDefaultEditingPointsModeHotKey; }
		public static bool isEditingPointsModeColourDefault()
												{ return	prEditingPointsModeColour == 
															myDefaultEditingPointsModeColour; }

		public static bool isWireframeColourDefault()
												{ return	prWireframeColour == 
															myDefaultWireframeColour; }
		public static bool isGuideWireframeColourDefault()
												{ return	prGuideWireframeColour == 
															myDefaultGuideWireframeColour; }
		public static bool isUnselectableGuideWireframeColourDefault()
												{ return	prUnselectableGuideWireframeColour == 
															myDefaultUnselectableGuideWireframeColour; }
		public static bool isUnselectedGuideWireframeColourDefault()
												{ return	prUnselectedGuideWireframeColour == 
															myDefaultUnselectedGuideWireframeColour; }
		public static bool isSelectedGuideWireframeColourDefault()
												{ return	prSelectedGuideWireframeColour == 
															myDefaultSelectedGuideWireframeColour; }

		public static bool isGuidePointSizeDefault()
												{ return	prGuidePointSize == 
															myDefaultGuidePointSize; }
		public static bool isMinDistanceForPointSelectionDefault()
												{ return	prMinDistanceForPointSelection == 
															myDefaultMinDistanceForPointSelection; }
		public static bool isGuideMinDistanceForMidPointInsertionDefault()
												{ return	prGuideMinDistanceForMidPointInsertion == 
															myDefaultGuideMinDistanceForMidPointInsertion; }

		public static bool isCurvePrimitiveTypeDefaultDefault()
												{ return	prCurvePrimitiveTypeDefault == 
															myDefaultCurvePrimitiveTypeDefault; }
		public static bool isCurveMethodDefaultDefault()
												{ return	prCurveMethodDefault == 
															myDefaultCurveMethodDefault; }

		// Global Settings Revert To Defaults -----------------------------------------------------------------------

		public static void revertAllSettingsToDefaults()
		{
			prCollisionGroupName					= myDefaultCollisionGroupName;
			prRenderedCollisionGroupName			= myDefaultRenderedCollisionGroupName;

			prUnityMaterialAttribName				= myDefaultUnityMaterialAttribName;
			prUnitySubMaterialNameAttribName		= myDefaultUnitySubMaterialNameAttribName;
			prUnitySubMaterialIndexAttribName		= myDefaultUnitySubMaterialIndexAttribName;
			prDontCreateTextureFiles				= myDefaultDontCreateTextureFiles;
			prExtractTexturesInRawFormat			= myDefaultExtractTexturesInRawFormat;

			prPinSize 								= myDefaultPinSize;
			prPinColour								= myDefaultPinColour;
			prAutoPinInstances						= myDefaultAutoPinInstances;

			prEnableDragAndDrop						= myDefaultEnableDragAndDrop;
			prEnableSupportWarnings					= myDefaultEnableSupportWarnings;

			prAutoSelectAssetRootNode				= myDefaultAutoSelectAssetRootNode;
			prHideGeometryOnLinking					= myDefaultHideGeometryOnLinking;

			prGenerateTangents						= myDefaultGenerateTangents;

			prEnableCooking							= myDefaultEnableCooking;
			prCookingTriggersDownCooks				= myDefaultCookingTriggersDownCooks;
			prPlaymodePerFrameCooking				= myDefaultPlaymodePerFrameCooking;
			prPushUnityTransformToHoudini			= myDefaultPushUnityTransformToHoudini;
			prTransformChangeTriggersCooks			= myDefaultTransformChangeTriggersCooks;

			prAddingPointsModeHotKey				= myDefaultAddingPointsModeHotKey;
			prAddingPointsModeColour				= myDefaultAddingPointsModeColour;

			prEditingPointsModeHotKey				= myDefaultEditingPointsModeHotKey;
			prEditingPointsModeColour				= myDefaultEditingPointsModeColour;

			prWireframeColour						= myDefaultWireframeColour;
			prGuideWireframeColour					= myDefaultGuideWireframeColour;
			prUnselectableGuideWireframeColour		= myDefaultUnselectableGuideWireframeColour;
			prUnselectedGuideWireframeColour		= myDefaultUnselectedGuideWireframeColour;
			prSelectedGuideWireframeColour			= myDefaultSelectedGuideWireframeColour;

			prGuidePointSize						= myDefaultGuidePointSize;
			prMinDistanceForPointSelection			= myDefaultMinDistanceForPointSelection;
			prGuideMinDistanceForMidPointInsertion	= myDefaultGuideMinDistanceForMidPointInsertion;

			prCurvePrimitiveTypeDefault				= myDefaultCurvePrimitiveTypeDefault;
			prCurveMethodDefault					= myDefaultCurveMethodDefault;

			prEnablePointsAsParticles 			    = myDefaultEnablePointsAsParticles;
		}

		public static RepaintDelegate			myRepaintDelegate;
		public static DeselectionDelegate		myDeselectionDelegate;

		public static GameObject				mySelectionTarget;
		public static bool						mySelectionTargetIsPrefab;

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
			
			int asset_id = -1;
			HAPI_Result status_code = (HAPI_Result) HAPI_LoadOTLFile( path,
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
																
			HAPI_Result status_code = (HAPI_Result) HAPI_LoadHIPFile( path,
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
			
			processStatusCode( (HAPI_Result) result );
			
			return true;
		}

		public static bool isRealDestroy()
		{
			return !EditorApplication.isPlayingOrWillChangePlaymode && !prMidPlaymodeStateChange;
		}

		public static bool initialize()
		{
			if ( !prHoudiniSceneExists )
			{
				HAPI_Result status_code;
				try
				{
					string otls_path = getAllFoldersInPath( Application.dataPath + "/OTLs/Scanned" );
					string dsos_path = getAllFoldersInPath( Application.dataPath + "/DSOs" );

					// Check version match.
					
					int houdini_major 			= getEnvInt( HAPI_EnvIntType.HAPI_ENVINT_VERSION_HOUDINI_MAJOR );
					int houdini_minor 			= getEnvInt( HAPI_EnvIntType.HAPI_ENVINT_VERSION_HOUDINI_MINOR );
					int houdini_build 			= getEnvInt( HAPI_EnvIntType.HAPI_ENVINT_VERSION_HOUDINI_BUILD );
					int houdini_engine_major 	= getEnvInt( HAPI_EnvIntType.HAPI_ENVINT_VERSION_HOUDINI_ENGINE_MAJOR );
					int houdini_engine_minor 	= getEnvInt( HAPI_EnvIntType.HAPI_ENVINT_VERSION_HOUDINI_ENGINE_MINOR );
					int houdini_engine_api 		= getEnvInt( HAPI_EnvIntType.HAPI_ENVINT_VERSION_HOUDINI_ENGINE_API );
					
					Debug.Log(  "Running Houdini Engine Unity Plugin Version: " +
								HAPI_Version.HOUDINI_ENGINE_MAJOR + "." + 
								HAPI_Version.HOUDINI_ENGINE_MINOR + "." +
								", API: " + HAPI_Version.HOUDINI_ENGINE_API );
					
					Debug.Log(  "Linking Against Houdini Engine Version: " + 
								houdini_engine_major + "." + houdini_engine_minor +
								", API: " + houdini_engine_api );
					
					Debug.Log(  "Underlying Houdini Core Version: " + 
								houdini_major + "." + houdini_minor + "." + houdini_build );
					
					// Make sure we are linking against the expected Houdini Engine API version.
					// Note: We don't need to be so strict as to require the BUILD to match.
					if ( houdini_engine_major != HAPI_Version.HOUDINI_ENGINE_MAJOR ||
						 houdini_engine_minor != HAPI_Version.HOUDINI_ENGINE_MINOR ||
						 houdini_engine_api	  != HAPI_Version.HOUDINI_ENGINE_API )
					{
						throw new HAPI_Error( "Cannot link to Houdini Engine because of version mismatch." );
					}
					
					// Initialize.
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
			int code;
			HAPI_GetStatus( (int) HAPI_StatusType.HAPI_STATUS_RESULT, out code );
			HAPI_GetStatusStringBufLength( (int) HAPI_StatusType.HAPI_STATUS_RESULT, out buffer_size );
			StringBuilder error_str = new StringBuilder( buffer_size );
			HAPI_GetStatusString( (int) HAPI_StatusType.HAPI_STATUS_RESULT, error_str );

			if ( code == (int) HAPI_Result.HAPI_RESULT_INVALID_ARGUMENT )
				throw new HAPI_ErrorInvalidArgument( error_str.ToString() );
			else
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
			// We need to catch any exceptions here because if we let any out they will stall
			// the entire callback chain bound to EditorApplication.update which
			// causes other bound functions in this callback list to never be called.
			try
			{
				if ( HAPI_Host.mySelectionTarget != null && myDeselectionDelegate != null )
				{
					GameObject selected = Selection.activeGameObject;
					if ( selected != mySelectionTarget )
						myDeselectionDelegate();
				}
			}
			catch ( System.Exception error )
			{
				Debug.Log( error.ToString() + "\nSource: " + error.Source );	
			}
		}

		private static void playmodeStateChanged()
		{
			// We need to catch any exceptions here because if we let any out they will stall
			// the entire callback chain bound to EditorApplication.playmodeStateChanged which
			// causes other bound functions in this callback list to never be called.
			try
			{
				prMidPlaymodeStateChange = !prMidPlaymodeStateChange;
				setTime( 0.0f );
			}
			catch ( System.Exception error )
			{
				Debug.Log( error.ToString() + "\nSource: " + error.Source );	
			}
		}

		private static string getAllFoldersInPath( string path )
		{
			string paths = "";

			if ( !Directory.Exists( path ) )
				return "";

			DirectoryInfo di = new DirectoryInfo( path );
			foreach ( DirectoryInfo child_directory in di.GetDirectories() )
				paths += ";" + getAllFoldersInPath( child_directory.FullName );

			return path + paths;
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
