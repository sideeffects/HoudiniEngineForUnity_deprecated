using UnityEngine;
using System.Collections;

using HAPI;

public class HAPI_HostUndoInfo : ScriptableObject
{
#if UNITY_EDITOR
	// General Settings
	public string collisionGroupName;
	public string renderedCollisionGroupName;
	public float pinSize;
	public Color pinColour;
	public bool autoPinInstances;
	public bool enableDragAndDrop;
	public bool enableSupportWarnings;
	public bool autoSelectAssetRootNode;
	public bool hideGeometryOnLinking;
	public bool enablePointsAsParticles;

	// Material Settings
	public string unityMaterialAttribName;
	public string unitySubMaterialNameAttribName;
	public string unitySubMaterialIndexAttribName;
	public bool generateTangents;
	public bool dontCreateTextureFiles;
	public bool extractTexturesInRawFormat;
	
	// Cooking Settings
	public bool enableCooking;
	public bool cookingTriggersDownCooks;
	public bool playmodePerFrameCooking;
	public bool pushUnityTransformToHoudini;
	public bool transformChangeTriggersCooks;
	public bool importTemplatedGeos;
	
	// Geometry Settings
	public string unityTagAttribName;
	public float paintBrushRate;
	public KeyCode paintingModeHotKey;
	public Color paintingModeColour;
	public KeyCode addingPointsModeHotKey;
	public Color addingPointsModeColour;
	public KeyCode editingPointsModeHotKey;
	public Color editingPointsModeColour;
	public Color wireframeColour;
	public Color guideWireframeColour;
	public Color unselectableGuideWireframeColour;
	public Color unselectedGuideWireframeColour;
	public Color selectedGuideWireframeColour;
	public float guidePointSize;
	public float minDistanceForPointSelection;
	public float guideMinDistanceForMidPointInsertion;
	
	// Curve Settings
	public int curvePrimitiveTypeDefault;
	public int curveMethodDefault;

	// Initialization --------------------------------------------------------------------------
	public HAPI_HostUndoInfo()
	{
		initialize();
	}

	public void initialize()
	{
		// Set general settings.
		collisionGroupName 				= HAPI_Host.prCollisionGroupName;
		renderedCollisionGroupName 		= HAPI_Host.prRenderedCollisionGroupName;
		pinSize 						= HAPI_Host.prPinSize;
		pinColour 						= HAPI_Host.prPinColour;
		autoPinInstances 				= HAPI_Host.prAutoPinInstances;
		enableDragAndDrop 				= HAPI_Host.prEnableDragAndDrop;
		enableSupportWarnings 			= HAPI_Host.prEnableSupportWarnings;
		autoSelectAssetRootNode 		= HAPI_Host.prAutoSelectAssetRootNode;
		hideGeometryOnLinking 			= HAPI_Host.prHideGeometryOnLinking;
		enablePointsAsParticles 		= HAPI_Host.prEnablePointsAsParticles;
		
		// Set material settings.
		unityMaterialAttribName 		= HAPI_Host.prUnityMaterialAttribName;
		unitySubMaterialNameAttribName 	= HAPI_Host.prUnitySubMaterialNameAttribName;
		unitySubMaterialIndexAttribName = HAPI_Host.prUnitySubMaterialIndexAttribName;
		generateTangents 				= HAPI_Host.prGenerateTangents;
		dontCreateTextureFiles 			= HAPI_Host.prDontCreateTextureFiles;
		extractTexturesInRawFormat 		= HAPI_Host.prExtractTexturesInRawFormat;
		
		// Set cooking settings.
		enableCooking 					= HAPI_Host.prEnableCooking;
		cookingTriggersDownCooks 		= HAPI_Host.prCookingTriggersDownCooks;
		playmodePerFrameCooking 		= HAPI_Host.prPlaymodePerFrameCooking;
		pushUnityTransformToHoudini 	= HAPI_Host.prPushUnityTransformToHoudini;
		transformChangeTriggersCooks 	= HAPI_Host.prTransformChangeTriggersCooks;
		importTemplatedGeos 			= HAPI_Host.prImportTemplatedGeos;
		
		// Set geometry settings.
		unityTagAttribName 				= HAPI_Host.prUnityTagAttribName;
		paintBrushRate					= HAPI_Host.prPaintBrushRate;
		paintingModeHotKey 				= HAPI_Host.prPaintingModeHotKey;
		paintingModeColour 				= HAPI_Host.prPaintingModeColour;
		addingPointsModeHotKey 			= HAPI_Host.prAddingPointsModeHotKey;
		addingPointsModeColour 			= HAPI_Host.prAddingPointsModeColour;
		editingPointsModeHotKey 		= HAPI_Host.prEditingPointsModeHotKey;
		editingPointsModeColour 		= HAPI_Host.prEditingPointsModeColour;
		wireframeColour 				= HAPI_Host.prWireframeColour;
		guideWireframeColour 			= HAPI_Host.prGuideWireframeColour;
		unselectableGuideWireframeColour = HAPI_Host.prUnselectableGuideWireframeColour;
		unselectedGuideWireframeColour 	= HAPI_Host.prUnselectedGuideWireframeColour;
		selectedGuideWireframeColour 	= HAPI_Host.prSelectedGuideWireframeColour;
		guidePointSize 					= HAPI_Host.prGuidePointSize;
		minDistanceForPointSelection 	= HAPI_Host.prMinDistanceForPointSelection;
		guideMinDistanceForMidPointInsertion = HAPI_Host.prGuideMinDistanceForMidPointInsertion;
		
		// Set curve settings.
		curvePrimitiveTypeDefault		= HAPI_Host.prCurvePrimitiveTypeDefault;
		curveMethodDefault				= HAPI_Host.prCurveMethodDefault;
	}
#endif // UNITY_EDITOR
}

