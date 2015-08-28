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
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

public delegate void RepaintDelegate();
public delegate void DeselectionDelegate();
public delegate void PlaymodeStateChangeDelegate();

public class HoudiniError : System.Exception
{
	public HoudiniError() 
	{
		myErrorMessage = "Unknown Error";
	}
	public HoudiniError( string msg ) 
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

public class HoudiniErrorUnsupportedPlatform : HoudiniError
{
	public HoudiniErrorUnsupportedPlatform() : base( HoudiniConstants.HAPI_UNSUPPORTED_PLATFORM_MSG ) {}
}

public class HoudiniErrorInvalidArgument : HoudiniError
{
	public HoudiniErrorInvalidArgument() : base( "Invalid arugment error." ) {}
	public HoudiniErrorInvalidArgument( string msg ) : base( msg ) {}
}

public class HoudiniErrorAssetDefOverride : HoudiniError
{
	public HoudiniErrorAssetDefOverride() : base( "Asset definition already loaded." ) {}
	public HoudiniErrorAssetDefOverride( string msg ) : base( msg ) {}
}

public class HoudiniErrorNotFound : HoudiniErrorInvalidArgument
{
	public HoudiniErrorNotFound() : base( "Item not found." ) {}
	public HoudiniErrorNotFound( string msg ) : base( msg ) {}
}

public class HoudiniErrorProgressCancelled : HoudiniError 
{
	public HoudiniErrorProgressCancelled() : base( "Load Cancelled by User" ) {}
}

public class HoudiniErrorIgnorable : HoudiniError
{
	public HoudiniErrorIgnorable() : base( "Ignorable Error" ) {}
	public HoudiniErrorIgnorable( string msg ) : base( msg ) {}
}

/// <summary>
/// 	Singleton Houdini host object that maintains the singleton Houdini scene and all access to the
/// 	Houdini runtime.
/// </summary>
//
#if UNITY_EDITOR
[ InitializeOnLoad ]
#endif // UNITY_EDITOR
public static partial class HoudiniHost
{
	// Global Settings Defaults ---------------------------------------------------------------------------------

	private const string myDefaultCollisionGroupName					= "collision_geo";
	private const string myDefaultRenderedCollisionGroupName			= "rendered_collision_geo";

	private const string myDefaultUnityMaterialAttribName				= "unity_material";
	private const string myDefaultUnitySubMaterialNameAttribName		= "unity_sub_material_name";
	private const string myDefaultUnitySubMaterialIndexAttribName		= "unity_sub_material_index";

	private const float myDefaultPinSize								= 1.0f;
	private static Color myDefaultPinColour								= new Color( 0.7f, 0.0f, 0.0f, 1.0f );
	private const bool myDefaultAutoPinInstances						= true;

	private const bool myDefaultEnableSupportWarnings					= true;

	public const bool myDefaultAutoSelectAssetRootNode					= true;

	private const bool myDefaultEnablePointsAsParticles 				= true;

	public const float myDefaultGamma									= 2.2f;
	private const bool myDefaultDontCreateTextureFiles					= false;
	private const bool myDefaultExtractTexturesInRawFormat				= false;

	public const bool myDefaultEnableCooking							= true;
	public const bool myDefaultCookingTriggersDownCooks					= true;
	public const bool myDefaultPlaymodePerFrameCooking					= false;
	public const bool myDefaultPushUnityTransformToHoudini				= true;
	public const bool myDefaultTransformChangeTriggersCooks				= false;
	public const bool myDefaultImportTemplatedGeos						= false;
	public const bool myDefaultSplitGeosByGroup							= true;

	private const string myDefaultUnityTagAttribName					= "unity_tag";

	private const float myDefaultPaintBrushRate							= 0.2f; // Should be between zero and one.
	private const KeyCode myDefaultPaintingModeHotKey					= KeyCode.LeftShift;
	private static Color myDefaultPaintingModeColour					= new Color( 0.7f, 0.9f, 0.7f, 1.0f );
	private const KeyCode myDefaultPaintingNodeSwitchHotKey				= KeyCode.Q;
	private const KeyCode myDefaultPaintingAttributeSwitchHotKey		= KeyCode.W;
	private const KeyCode myDefaultPaintingValueChangeHotKey			= KeyCode.E;
	private const KeyCode myDefaultPaintingFalloffChangeHotKey			= KeyCode.R;

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

	private const bool myDefaultCreateGroupsFromBoolAttributes			= false;

	private const int myDefaultCurvePrimitiveTypeDefault				= 1;
	private const int myDefaultCurveMethodDefault						= 1;

	// Global Settings Undo Info --------------------------------------------------------------------------------

	private static HoudiniHostUndoInfo myHostUndoInfo;

	// Global Settings Initializations --------------------------------------------------------------------------

	// This is to keep track whether we actually initialized Houdini Engine in this session/state/runtime of C#.
	private static bool myCurrentCSharpSessionInitialized = false;

	static HoudiniHost()
	{
		// Initialize the global session.
		mySession.type = HAPI_SessionType.HAPI_SESSION_INPROCESS;
		mySession.id = 0;

#if UNITY_EDITOR
		EditorApplication.update				+= update;
		EditorApplication.playmodeStateChanged	+= playmodeStateChanged;
		EditorApplication.hierarchyWindowItemOnGUI += hierarchyWindowItemOnGUI;
		SceneView.onSceneGUIDelegate			+= onSceneGUIDelegate;
#endif // UNITY_EDITOR

		// Preferences
		
		setString(	"HAPI_CollisionGroupName", myDefaultCollisionGroupName, true );
		setString(	"HAPI_RenderedCollisionGroupName", myDefaultRenderedCollisionGroupName, true );
		
		setString(	"HAPI_UnityMaterialAttribName", myDefaultUnityMaterialAttribName, true );
		setString(	"HAPI_UnitySubMaterialNameAttribName", myDefaultUnitySubMaterialNameAttribName, true );
		setString(	"HAPI_UnitySubMaterialIndexAttribName", myDefaultUnitySubMaterialIndexAttribName, true );
		
		setFloat(	"HAPI_PinSize", myDefaultPinSize, true );
		setColour(	"HAPI_PinColour", myDefaultPinColour, true );
		setBool(	"HAPI_AutoPinInstances", myDefaultAutoPinInstances, true );
		
		setBool(	"HAPI_EnableSupportWarnings", myDefaultEnableSupportWarnings, true );
		
		setBool(	"HAPI_AutoSelectAssetRootNode", myDefaultAutoSelectAssetRootNode, true );
		
		setBool( 	"HAPI_EnablePointsAsParticles", myDefaultEnablePointsAsParticles, true );
		
		setFloat(	"HAPI_Gamma", myDefaultGamma, true );
		setBool(	"HAPI_DontCreateTextureFiles", myDefaultDontCreateTextureFiles, true );
		setBool(	"HAPI_ExtractTexturesInRawFormat", myDefaultExtractTexturesInRawFormat, true );
		
		setBool(	"HAPI_EnableCooking", myDefaultEnableCooking, true );
		setBool(	"HAPI_CookingTriggersDownCooks", myDefaultCookingTriggersDownCooks, true );
		setBool(	"HAPI_PlaymodePerFrameCooking", myDefaultPlaymodePerFrameCooking, true );
		setBool(	"HAPI_PushUnityTransformToHoudini", myDefaultPushUnityTransformToHoudini, true );
		setBool(	"HAPI_TransformChangeTriggersCooks", myDefaultTransformChangeTriggersCooks, true );
		setBool(	"HAPI_ImportTemplatedGeos", myDefaultImportTemplatedGeos, true );
		setBool(	"HAPI_SplitGeosByGroup", myDefaultSplitGeosByGroup, true );
		
		setString(	"HAPI_UnityTagAttribName", myDefaultUnityTagAttribName, true );
		
		setFloat(	"HAPI_PaintBrushRate", myDefaultPaintBrushRate, true );
		setKeyCode( "HAPI_PaintingHotKey", myDefaultPaintingModeHotKey, true );
		setColour(	"HAPI_PaintingModeColour", myDefaultPaintingModeColour, true );
		setKeyCode( "HAPI_PaintingNodeSwitchHotKey", myDefaultPaintingNodeSwitchHotKey, true );
		setKeyCode( "HAPI_PaintingAttributeSwitchHotKey", myDefaultPaintingAttributeSwitchHotKey, true );
		setKeyCode( "HAPI_PaintingValueChangeHotKey", myDefaultPaintingValueChangeHotKey, true );
		setKeyCode( "HAPI_PaintingFalloffChangeHotKey", myDefaultPaintingFalloffChangeHotKey, true );
		
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
		
		setBool(	"HAPI_CreateGroupsFromBoolAttributes", myDefaultCreateGroupsFromBoolAttributes, true );
		
		setInt(		"HAPI_CurvePrimitiveTypeDefault", myDefaultCurvePrimitiveTypeDefault, true );
		setInt(		"HAPI_CurveMethodDefault", myDefaultCurveMethodDefault, true );
		
		myRepaintDelegate			= null;
		myDeselectionDelegate		= null;
		mySelectionTarget			= null;
		
		myCleanUpPrefabAssets		= new Dictionary< string, int >();
	}

	public static bool initializeHost()
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		// During the batch creation of our .unitypackage file we don't want to actually
		// initialize  We use this environment variable to inhibit initialization.
		string no_init = System.Environment.GetEnvironmentVariable( "HAPI_UNITY_NO_INIT" );
		if ( no_init != null )
			return false;
#endif // ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )

		if ( isRuntimeInitialized() )
		{
			// Just make sure the session is properly initialized.
			return initializeSession();
		}
		else
		{
			prMidPlaymodeStateChange = false;
			if ( !initialize() )
				return false;
		}

		return true;
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public

	// Global Settings Properties -------------------------------------------------------------------------------

	public static bool prHoudiniSceneExists {
											get { return HoudiniDataFile.getBool( "HoudiniSceneExists", false ); } 
											private set { HoudiniDataFile.setBool( "HoudiniSceneExists", value ); } }
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

	public static float prPinSize {
											get { return getFloat( "HAPI_PinSize" ); }
											set { setFloat( "HAPI_PinSize", value ); } }
	public static Color prPinColour {
											get { return getColour( "HAPI_PinColour" ); }
											set { setColour( "HAPI_PinColour", value ); } }
	public static bool prAutoPinInstances {
											get { return getBool( "HAPI_AutopinInstances" ); } 
											set { setBool( "HAPI_AutopinInstances", value ); } }

	public static bool prEnableSupportWarnings {
											get { return getBool( "HAPI_EnableSupportWarnings" ); } 
											set { setBool( "HAPI_EnableSupportWarnings", value ); } }

	public static bool prAutoSelectAssetRootNode {
											get { return getBool( "HAPI_AutoSelectAssetRootNode" ); } 
											set { setBool( "HAPI_AutoSelectAssetRootNode", value ); } }

	public static bool prEnablePointsAsParticles {
											get { return getBool( "HAPI_EnablePointsAsParticles" ); }
											set { setBool( "HAPI_EnablePointsAsParticles", value ); } }

	public static float prGamma {
											get { return getFloat( "HAPI_Gamma" ); } 
											set { setFloat( "HAPI_Gamma", value ); } }
	public static bool prDontCreateTextureFiles {
											get { return getBool( "HAPI_DontCreateTextureFiles" ); }
											set { setBool( "HAPI_DontCreateTextureFiles", value ); } }
	public static bool prExtractTexturesInRawFormat {
											get { return getBool( "HAPI_ExtractTexturesInRawFormat" ); }
											set { setBool( "HAPI_ExtractTexturesInRawFormat", value ); } }

	public static bool prEnableCooking {
#if UNITY_EDITOR
											get { return getBool( "HAPI_EnableCooking" ); }
											set { setBool( "HAPI_EnableCooking", value ); }
#else
											get { return getBool( "HAPI_EnableCooking" ) && HoudiniConstants.HAPI_ENABLE_RUNTIME; }
											set {}
#endif // UNITY_EDITOR
									   }
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
	public static bool prImportTemplatedGeos {
											get { return getBool( "HAPI_ImportTemplatedGeos" ); }
											set { setBool( "HAPI_ImportTemplatedGeos", value ); } }
	public static bool prSplitGeosByGroup {
											get { return getBool( "HAPI_SplitGeosByGroup" ); }
											set { setBool( "HAPI_SplitGeosByGroup", value ); } }

	public static string prUnityTagAttribName {
											get { return getString( "HAPI_UnityTagAttribName" ); }
											set { setString( "HAPI_UnityTagAttribName", value ); } } 

	public static float prPaintBrushRate {
											get { return getFloat( "HAPI_PaintBrushRate" ); }
											set { setFloat(
													"HAPI_PaintBrushRate",
													Mathf.Clamp( value, 0.0f, 1.0f ) ); } }
	public static KeyCode prPaintingModeHotKey {
											get { KeyCode key = getKeyCode( "HAPI_PaintingModeHotKey" );
													if ( key == KeyCode.None ) {
														key = myDefaultPaintingModeHotKey;
														setKeyCode( "HAPI_PaintingModeHotKey", key ); }
													return key; }
											set { setKeyCode( "HAPI_PaintingModeHotKey", value ); } }
	public static Color prPaintingModeColour {
											get { return getColour( "HAPI_PaintingModeColour" ); }
											set { setColour( "HAPI_PaintingModeColour", value ); } }
	public static KeyCode prPaintingNodeSwitchHotKey {
											get { KeyCode key = getKeyCode( "HAPI_PaintingNodeSwitchHotKey" );
													if ( key == KeyCode.None ) {
														key = myDefaultPaintingNodeSwitchHotKey;
														setKeyCode( "HAPI_PaintingNodeSwitchHotKey", key ); }
													return key; }
											set { setKeyCode( "HAPI_PaintingNodeSwitchHotKey", value ); } }
	public static KeyCode prPaintingAttributeSwitchHotKey {
											get { KeyCode key = getKeyCode( "HAPI_PaintingAttributeSwitchHotKey" );
													if ( key == KeyCode.None ) {
														key = myDefaultPaintingAttributeSwitchHotKey;
														setKeyCode( "HAPI_PaintingAttributeSwitchHotKey", key ); }
													return key; }
											set { setKeyCode( "HAPI_PaintingAttributeSwitchHotKey", value ); } }
	public static KeyCode prPaintingValueChangeHotKey {
											get { KeyCode key = getKeyCode( "HAPI_PaintingValueChangeHotKey" );
													if ( key == KeyCode.None ) {
														key = myDefaultPaintingValueChangeHotKey;
														setKeyCode( "HAPI_PaintingValueChangeHotKey", key ); }
													return key; }
											set { setKeyCode( "HAPI_PaintingValueChangeHotKey", value ); } }
	public static KeyCode prPaintingFalloffChangeHotKey {
											get { KeyCode key = getKeyCode( "HAPI_PaintingFalloffChangeHotKey" );
													if ( key == KeyCode.None ) {
														key = myDefaultPaintingFalloffChangeHotKey;
														setKeyCode( "HAPI_PaintingFalloffChangeHotKey", key ); }
													return key; }
											set { setKeyCode( "HAPI_PaintingFalloffChangeHotKey", value ); } }

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

	public static bool prCreateGroupsFromBoolAttributes {
											get { return getBool( "HAPI_CreateGroupsFromBoolAttributes" ); }
											set { setBool( "HAPI_CreateGroupsFromBoolAttributes", value ); } }

	public static int prCurvePrimitiveTypeDefault {
											get { return getInt( "HAPI_CurvePrimitiveTypeDefault" ); }
											set { setInt( "HAPI_CurvePrimitiveTypeDefault", value ); } }
	public static int prCurveMethodDefault {
											get { return getInt( "HAPI_CurveMethodDefault" ); }
											set { setInt( "HAPI_CurveMethodDefault", value ); } }

	// Global Settings Undo Info --------------------------------------------------------------------------------

	public static HoudiniHostUndoInfo prHostUndoInfo 
	{
		get 
		{ 
			if ( myHostUndoInfo == null )
				myHostUndoInfo = ScriptableObject.CreateInstance< HoudiniHostUndoInfo >();
			return myHostUndoInfo; 
		}
		set { myHostUndoInfo = value; } 
	}
	
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

	public static bool isPinSizeDefault()
											{ return	prPinSize == 
														myDefaultPinSize; }
	public static bool isPinColourDefault()
											{ return	prPinColour == 
														myDefaultPinColour; }
	public static bool isAutoPinInstancesDefault()
											{ return	prAutoPinInstances == 
														myDefaultAutoPinInstances; }

	public static bool isEnableSupportWarningsDefault()
											{ return	prEnableSupportWarnings == 
														myDefaultEnableSupportWarnings; }

	public static bool isAutoSelectAssetRootNodeDefault()
											{ return	prAutoSelectAssetRootNode == 
														myDefaultAutoSelectAssetRootNode; }

	public static bool isGammaDefault()
											{ return	Mathf.Approximately(
															prGamma, myDefaultGamma ); }
	public static bool isDontCreateTextureFilesDefault()
											{ return	prDontCreateTextureFiles ==
														myDefaultDontCreateTextureFiles; }
	public static bool isExtractTexturesInRawFormatDefault()
											{ return	prExtractTexturesInRawFormat ==
														myDefaultExtractTexturesInRawFormat; }

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
	public static bool isImportTemplatedGeosDefault()
											{ return	prImportTemplatedGeos == 
														myDefaultImportTemplatedGeos; }
	public static bool isSplitGeosByGroupDefault()
											{ return	prSplitGeosByGroup ==
														myDefaultSplitGeosByGroup; }

	public static bool isUnityTagAttribNameDefault()
											{ return	prUnityTagAttribName ==
														myDefaultUnityTagAttribName; }

	public static bool isPaintBrushRateDefault()
											{ return	prPaintBrushRate ==
														myDefaultPaintBrushRate; }
	public static bool isPaintingModeHotKeyDefault()
											{ return	prPaintingModeHotKey == 
														myDefaultPaintingModeHotKey; }
	public static bool isPaintingModeColourDefault()
											{ return	prPaintingModeColour == 
														myDefaultPaintingModeColour; }
	public static bool isPaintingNodeSwitchHotKeyDefault()
											{ return	prPaintingNodeSwitchHotKey == 
														myDefaultPaintingNodeSwitchHotKey; }
	public static bool isPaintingAttributeSwitchHotKeyDefault()
											{ return	prPaintingAttributeSwitchHotKey == 
														myDefaultPaintingAttributeSwitchHotKey; }
	public static bool isPaintingValueChangeHotKeyDefault()
											{ return	prPaintingValueChangeHotKey == 
														myDefaultPaintingValueChangeHotKey; }
	public static bool isPaintingFalloffChangeHotKeyDefault()
											{ return	prPaintingFalloffChangeHotKey == 
														myDefaultPaintingFalloffChangeHotKey; }

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

	public static bool isCreateGroupsFromBoolAttributes()
											{ return	prCreateGroupsFromBoolAttributes ==
														myDefaultCreateGroupsFromBoolAttributes; }

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

		prPinSize 								= myDefaultPinSize;
		prPinColour								= myDefaultPinColour;
		prAutoPinInstances						= myDefaultAutoPinInstances;

		prEnableSupportWarnings					= myDefaultEnableSupportWarnings;

		prAutoSelectAssetRootNode				= myDefaultAutoSelectAssetRootNode;

		prEnablePointsAsParticles 			    = myDefaultEnablePointsAsParticles;

		prGamma									= myDefaultGamma;
		prDontCreateTextureFiles				= myDefaultDontCreateTextureFiles;
		prExtractTexturesInRawFormat			= myDefaultExtractTexturesInRawFormat;

		prEnableCooking							= myDefaultEnableCooking;
		prCookingTriggersDownCooks				= myDefaultCookingTriggersDownCooks;
		prPlaymodePerFrameCooking				= myDefaultPlaymodePerFrameCooking;
		prPushUnityTransformToHoudini			= myDefaultPushUnityTransformToHoudini;
		prTransformChangeTriggersCooks			= myDefaultTransformChangeTriggersCooks;
		prImportTemplatedGeos					= myDefaultImportTemplatedGeos;
		prSplitGeosByGroup						= myDefaultSplitGeosByGroup;

		prUnityTagAttribName					= myDefaultUnityTagAttribName;

		prPaintBrushRate						= myDefaultPaintBrushRate;
		prPaintingModeHotKey					= myDefaultPaintingModeHotKey;
		prPaintingModeColour					= myDefaultPaintingModeColour;
		prPaintingNodeSwitchHotKey				= myDefaultPaintingNodeSwitchHotKey;
		prPaintingAttributeSwitchHotKey			= myDefaultPaintingAttributeSwitchHotKey;
		prPaintingValueChangeHotKey				= myDefaultPaintingValueChangeHotKey;
		prPaintingFalloffChangeHotKey			= myDefaultPaintingFalloffChangeHotKey;

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

		prCreateGroupsFromBoolAttributes		= myDefaultCreateGroupsFromBoolAttributes;

		prCurvePrimitiveTypeDefault				= myDefaultCurvePrimitiveTypeDefault;
		prCurveMethodDefault					= myDefaultCurveMethodDefault;

		// Reset undo info so values match with above settings.
		prHostUndoInfo.initialize();
	}

	public static RepaintDelegate			myRepaintDelegate;
	public static DeselectionDelegate		myDeselectionDelegate;
	public static PlaymodeStateChangeDelegate myPlaymodeStateChangeDelegate;

	public static GameObject				mySelectionTarget;

	// maps prefab path to asset id
	public static Dictionary< string, int >	myCleanUpPrefabAssets;

	public static void saveScene( string file_name, bool lock_nodes )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		HAPI_SaveHIPFile( ref mySession, file_name, lock_nodes );
#endif // ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
	}

	public static int loadOTL(
		string path, bool split_geos_by_group, bool import_templated_geos,
		HoudiniProgressBar progress_bar )
	{
		return loadOTL( path, split_geos_by_group, import_templated_geos, progress_bar, true );
	}
	public static int loadOTL(
		string path, bool split_geos_by_group, bool import_templated_geos,
		HoudiniProgressBar progress_bar, bool allow_asset_def_overwrite ) 
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		if ( !isInstallationOk() )
			throw new HoudiniError( "DLL Not Found." );

		int library_id = -1;
		HAPI_Result status_code;

		// This can be used to test in-memory OTL loading.
#if false
		byte[] file = File.ReadAllBytes( path );
		status_code = HAPI_LoadAssetLibraryFromMemory(
			file, file.Length, allow_asset_def_overwrite, out library_id );
#else
	#if UNITY_EDITOR
		status_code = HAPI_LoadAssetLibraryFromFile(
			ref mySession, path, allow_asset_def_overwrite, out library_id );

		if ( status_code == HAPI_Result.HAPI_RESULT_ASSET_DEF_ALREADY_LOADED )
		{
			if ( EditorUtility.DisplayDialog(
				"Houdini Asset Definition Overwriting",
				"The asset library file being loaded: \n" + path + "\n" +
				"contains asset defintions that have already been loaded before from " +
				"another asset library file. \n\nWould you like to overwrite them?",
				"Yes", "No" ) )
			{
				status_code = HAPI_LoadAssetLibraryFromFile( ref mySession, path, true, out library_id );
			}
		}
	#else
		status_code = HAPI_LoadAssetLibraryFromFile( ref mySession, path, true, out library_id );
	#endif // UNITY_EDITOR
#endif // HAPI_LoadAssetLibraryFromMemory
		processStatusCode( status_code );

		int asset_count = 0;
		status_code = HAPI_GetAvailableAssetCount( ref mySession, library_id, out asset_count );
		processStatusCode( status_code );

		int[] asset_names_sh = new int[ asset_count ];
		status_code = HAPI_GetAvailableAssets( ref mySession, library_id, asset_names_sh, asset_count );
		processStatusCode( status_code );

		string[] asset_names = new string[ asset_count ];
		for ( int i = 0; i < asset_count; ++i )
		{
			asset_names[ i ] = getString( asset_names_sh[ i ] );
			//Debug.Log( asset_names[ i ] );
		}

		int asset_id = -1;
		string first_asset_name = asset_names[ 0 ];
		bool cook_on_load = false;
		status_code = HAPI_InstantiateAsset( ref mySession, first_asset_name, cook_on_load, out asset_id );
		processStatusCode( status_code );
		progress_bar.statusCheckLoop();
		cookAsset( asset_id, split_geos_by_group, import_templated_geos );
		progress_bar.statusCheckLoop();

		return asset_id;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif // ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
	}
		
	public static void loadHip( string path ) 
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		if ( !isInstallationOk() )
			throw new HoudiniError( "DLL Not Found." );

		HAPI_Result status_code = HAPI_LoadHIPFile( ref mySession, path, true );
		processStatusCode( status_code );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif // ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
	}
		
	public static int[] getNewAssetIds()
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		int asset_count = 0;
		HAPI_Result status_code = HAPI_CheckForNewAssets( ref mySession, ref asset_count );
		processStatusCode( status_code );

		int[] asset_ids_array = new int[ asset_count ];
		status_code = HAPI_GetNewAssetIds( ref mySession, asset_ids_array, asset_count );
		processStatusCode( status_code );

		return asset_ids_array;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif // ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
	}
		
	public static int createCurve()
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		if ( !isInstallationOk() )
			throw new HoudiniError( "DLL Not Found." );

		int asset_id = -1;
		HAPI_Result status_code = HAPI_CreateCurve( ref mySession, out asset_id );
		processStatusCode( status_code );

		return asset_id;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif // ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
	}

	public static int createInputAsset( string name )
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		if ( !isInstallationOk() )
			throw new HoudiniError( "DLL Not Found." );

		int asset_id = -1;
		HAPI_Result status_code = HAPI_CreateInputAsset( ref mySession, out asset_id, name );
		processStatusCode( status_code );

		return asset_id;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif // ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
	}

	public static bool destroyAsset( int asset_id ) 
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		if ( asset_id < 0 || !isInstallationOk() )
			return false;
			
		HAPI_Result result = HAPI_DestroyAsset( ref mySession, asset_id );
			
		processStatusCode( result );
			
		return true;
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif // ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
	}

	public static bool isRealDestroy()
	{
#if UNITY_EDITOR
		return !EditorApplication.isPlayingOrWillChangePlaymode && !prMidPlaymodeStateChange;
#else
		return true;
#endif // UNITY_EDITOR
	}

	public static string getCallErrorMessage()
	{
		// It is important that we use the NoExceptions version of getStatusString because
		// getStatusString() will also call throwCallError() if it fails which then
		// ends up calling itself and...stack overflow. :)
		return getStatusStringNoExceptions(
			HAPI_StatusType.HAPI_STATUS_CALL_RESULT,
			HAPI_StatusVerbosity.HAPI_STATUSVERBOSITY_WARNINGS );
	}

	public static string getCookErrorMessage()
	{
		// It is important that we use the NoExceptions version of getStatusString because
		// getStatusString() will also call throwCallError() if it fails which then
		// ends up calling itself and...stack overflow. :)
		return getStatusStringNoExceptions(
			HAPI_StatusType.HAPI_STATUS_COOK_RESULT,
			HAPI_StatusVerbosity.HAPI_STATUSVERBOSITY_WARNINGS );
	}

	public static void throwCallError()
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		int code;
		HAPI_GetStatus( ref mySession, HAPI_StatusType.HAPI_STATUS_CALL_RESULT, out code );

		string status_string = getCallErrorMessage();

		if ( code == (int) HAPI_Result.HAPI_RESULT_INVALID_ARGUMENT )
			throw new HoudiniErrorInvalidArgument( status_string );
		else if ( code == (int) HAPI_Result.HAPI_RESULT_ASSET_DEF_ALREADY_LOADED )
			throw new HoudiniErrorAssetDefOverride( status_string );
		else
			throw new HoudiniError( status_string );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif // ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
	}

	public static void throwCookError()
	{
#if ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		int code;
		HAPI_GetStatus( ref mySession, HAPI_StatusType.HAPI_STATUS_COOK_RESULT, out code );

		string status_string = getCookErrorMessage();

		if ( code == (int) HAPI_Result.HAPI_RESULT_INVALID_ARGUMENT )
			throw new HoudiniErrorInvalidArgument( status_string );
		else
			throw new HoudiniError( status_string );
#else
		throw new HoudiniErrorUnsupportedPlatform();
#endif // ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
	}
	
	public static void repaint()
	{
		if ( myRepaintDelegate != null )
			myRepaintDelegate();
	}

	public static HoudiniAsset[] getAssets()
	{
		return UnityEngine.Object.FindObjectsOfType( typeof( HoudiniAsset ) ) as HoudiniAsset[];
	}

	public static void log( string message )
	{
		message = processStringForConsole( message );
		Debug.Log( message );
	}

	public static void logWarning( string message )
	{
		message = processStringForConsole( message );
		Debug.LogWarning( message );
	}

	public static void logError( string message )
	{
		message = processStringForConsole( message );
		Debug.LogError( message );
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private

	private static string processStringForConsole( string str )
	{
		// Truncate the message manually due to bug:
		// http://fogbugz.unity3d.com/default.asp?621738_mhks4v903k1799iv
		if ( str.Length > 10000 )
		{
			str = str.Substring( 0, 9000 );
			str += "...";
		}
		return str;
	}

	private static void update()
	{
#if UNITY_EDITOR
		// We need to catch any exceptions here because if we let any out they will stall
		// the entire callback chain bound to EditorApplication.update which
		// causes other bound functions in this callback list to never be called.
		try
		{
			if ( HoudiniHost.mySelectionTarget != null && myDeselectionDelegate != null )
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
#endif // UNITY_EDITOR
	}

	private static void playmodeStateChanged()
	{
#if UNITY_EDITOR && ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		// We need to catch any exceptions here because if we let any out they will stall
		// the entire callback chain bound to EditorApplication.playmodeStateChanged which
		// causes other bound functions in this callback list to never be called.
		try
		{
			prMidPlaymodeStateChange = !prMidPlaymodeStateChange;

			// Find all prefabs created from Houdini assets and change the value of 
			// prReloadPrefabOnPlaymodeChange so that when going into play mode the 
			// asset will not be unloaded in the destructor and when selected
			// serialization recovery will occur for the parameters
			if ( prMidPlaymodeStateChange )
			{
				if ( isInstallationOk() )
				{
					foreach ( string asset_path in AssetDatabase.GetAllAssetPaths() )
					{
						if ( asset_path.EndsWith( ".prefab" ) )
						{
							GameObject prefab = AssetDatabase.LoadAssetAtPath( asset_path, typeof( GameObject ) ) as GameObject;
							if ( prefab )
							{
								// Only need to do this if the prefab has been previously loaded.
								HoudiniAsset prefab_asset = prefab.GetComponent< HoudiniAsset >();
								if ( prefab_asset && 
										isAssetValid( prefab_asset.prAssetId, prefab_asset.prAssetValidationId ) )
								{
									prefab_asset.prReloadPrefabOnPlaymodeChange = true;
								}
							}
						}
					}

					setTime( 0.0f );
				}

				if ( myPlaymodeStateChangeDelegate != null )
					myPlaymodeStateChangeDelegate();
			}
		}
		catch ( System.Exception error )
		{
			Debug.Log( error.ToString() + "\nSource: " + error.Source );	
		}
#endif // UNITY_EDITOR && ( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
	}

#if UNITY_EDITOR
	private static void hierarchyWindowItemOnGUI( int instanceID, Rect selectionRect )
	{
		processDragEvent( Event.current, null );
	}

	private static void onSceneGUIDelegate( SceneView sceneView )
	{
		processDragEvent( Event.current, sceneView );
	}

	private static bool isHoudiniAssetFile( string path )
	{
		return ( path.EndsWith( ".otl" ) || path.EndsWith( ".otllc" ) || path.EndsWith( ".otlnc" )
			|| path.EndsWith( ".hda" ) || path.EndsWith( ".hdalc" ) || path.EndsWith( ".hdanc" ) );
	}

	private static void processDragEvent( Event drag_event, SceneView scene_view )
	{
		if ( drag_event != null &&
				( drag_event.type == EventType.DragUpdated || drag_event.type == EventType.DragPerform ) )
		{
			bool drag_contains_otls = false;
			foreach ( string path in DragAndDrop.paths )
				if ( isHoudiniAssetFile( path ) )
				{
					drag_contains_otls = true;
					DragAndDrop.visualMode = DragAndDropVisualMode.Move;
					break;
				}
				
			if ( drag_event.type == EventType.DragPerform )
			{
				Vector3 initial_asset_position = new Vector3( 0.0f, 0.0f, 0.0f );
				if ( scene_view )
				{
					Camera camera = scene_view.camera;
					Vector3 mouse_position = drag_event.mousePosition;
					// Camera.current.pixelHeight != Screen.height for some reason.
					mouse_position.y = camera.pixelHeight - mouse_position.y;
					Ray ray = camera.ScreenPointToRay( mouse_position );
					ray.origin = camera.transform.position;
					Plane plane = new Plane();
					plane.SetNormalAndPosition( Vector3.up, Vector3.zero );
					float enter = 0.0f;
					plane.Raycast( ray, out enter );
					enter = Mathf.Clamp( enter, camera.nearClipPlane, camera.farClipPlane );
 					initial_asset_position = ray.origin + ray.direction * enter;
				}

				for ( int i = 0; i < DragAndDrop.paths.Length; ++i )
				{
					string path = DragAndDrop.paths[ i ];
					if ( isHoudiniAssetFile( path ) )
					{
						if ( !isInstallationOk() )
							displayHoudiniEngineInstallInfo();
						else
							HoudiniAssetUtility.instantiateAsset( path, initial_asset_position );
					}
				}
			}

			if ( drag_contains_otls )
				drag_event.Use();
		}
	}
#endif // UNITY_EDITOR

	private static bool hasCallFailed( HAPI_Result code )
	{
		return ( (int) code > 0 );
	}
		
	private static void processStatusCode( HAPI_Result code )
	{
		if ( hasCallFailed( code ) )
			throwCallError();
	}

	private static int getInt( string name )
	{
#if UNITY_EDITOR
		return EditorPrefs.GetInt( name );
#else
		return PlayerPrefs.GetInt( name );
#endif // UNITY_EDITOR
	}
	private static void setInt( string name, int value )
	{
		setInt( name, value, false );
	}
	private static void setInt( string name, int value, bool only_if_new )
	{
#if UNITY_EDITOR
		if ( !only_if_new || !EditorPrefs.HasKey( name ) )
		{
			EditorPrefs.SetInt( name, value );
			PlayerPrefs.SetInt( name, value );
		}
#else
		if ( !only_if_new || !PlayerPrefs.HasKey( name ) )
			PlayerPrefs.SetInt( name, value );
#endif // UNITY_EDITOR
	}

	private static bool getBool( string name )
	{
#if UNITY_EDITOR
		return EditorPrefs.GetInt( name ) == 0 ? false : true;
#else
		return PlayerPrefs.GetInt( name ) == 0 ? false : true;
#endif // UNITY_EDITOR
	}
	private static void setBool( string name, bool value )
	{
		setBool( name, value, false );
	}
	private static void setBool( string name, bool value, bool only_if_new )
	{
#if UNITY_EDITOR
		if ( !only_if_new || !EditorPrefs.HasKey( name ) )
		{
			EditorPrefs.SetInt( name, value ? 1 : 0 );
			PlayerPrefs.SetInt( name, value ? 1 : 0 );
		}
#else
		if ( !only_if_new || !PlayerPrefs.HasKey( name ) )
			PlayerPrefs.SetInt( name, value ? 1 : 0 );
#endif // UNITY_EDITOR
	}

	private static float getFloat( string name )
	{
#if UNITY_EDITOR
		return EditorPrefs.GetFloat( name );
#else
		return PlayerPrefs.GetFloat( name );
#endif // UNITY_EDITOR
	}
	private static void setFloat( string name, float value )
	{
		setFloat( name, value, false );
	}
	private static void setFloat( string name, float value, bool only_if_new )
	{
#if UNITY_EDITOR
		if ( !only_if_new || !EditorPrefs.HasKey( name ) )
		{
			EditorPrefs.SetFloat( name, value );
			PlayerPrefs.SetFloat( name, value );
		}
#else
		if ( !only_if_new || !PlayerPrefs.HasKey( name ) )
			PlayerPrefs.SetFloat( name, value );
#endif // UNITY_EDITOR
	}

	private static string getString( string name )
	{
#if UNITY_EDITOR
		return EditorPrefs.GetString( name );
#else
		return PlayerPrefs.GetString( name );
#endif // UNITY_EDITOR
	}
	private static void setString( string name, string value )
	{
		setString( name, value, false );
	}
	private static void setString( string name, string value, bool only_if_new )
	{
#if UNITY_EDITOR
		if ( !only_if_new || !EditorPrefs.HasKey( name ) )
		{
			EditorPrefs.SetString( name, value );
			PlayerPrefs.SetString( name, value );
		}
#else
		if ( !only_if_new || !PlayerPrefs.HasKey( name ) )
			PlayerPrefs.SetString( name, value );
#endif // UNITY_EDITOR
	}

	private static KeyCode getKeyCode( string name )
	{
		return (KeyCode) getInt( name );
	}
	private static void setKeyCode( string name, KeyCode value )
	{
		setKeyCode( name, value, false );
	}
	private static void setKeyCode( string name, KeyCode value, bool only_if_new )
	{
		setInt( name, (int) value, only_if_new );
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
		setFloat( name + "_r", value.r, only_if_new );
		setFloat( name + "_g", value.g, only_if_new );
		setFloat( name + "_b", value.b, only_if_new );
		setFloat( name + "_a", value.a, only_if_new );
	}
}
