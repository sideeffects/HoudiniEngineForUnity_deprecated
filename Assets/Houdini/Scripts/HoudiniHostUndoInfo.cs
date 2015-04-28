using UnityEngine;
using System.Collections;

public class HoudiniHostUndoInfo : ScriptableObject
{
	// General Settings
	public string collisionGroupName;
	public string renderedCollisionGroupName;
	public float pinSize;
	public Color pinColour;
	public bool autoPinInstances;
	public bool enableDragAndDrop;
	public bool enableSupportWarnings;
	public bool autoSelectAssetRootNode;
	public bool enablePointsAsParticles;

	// Material Settings
	public string unityMaterialAttribName;
	public string unitySubMaterialNameAttribName;
	public string unitySubMaterialIndexAttribName;
	public float gamma;
	public bool dontCreateTextureFiles;
	public bool extractTexturesInRawFormat;
	
	// Cooking Settings
	public bool enableCooking;
	public bool cookingTriggersDownCooks;
	public bool playmodePerFrameCooking;
	public bool pushUnityTransformToHoudini;
	public bool transformChangeTriggersCooks;
	public bool importTemplatedGeos;
	public bool splitGeosByGroup;
	
	// Geometry Settings
	public string unityTagAttribName;
	public float paintBrushRate;
	public KeyCode paintingModeHotKey;
	public Color paintingModeColour;
	public KeyCode paintingNodeSwitchHotKey;
	public KeyCode paintingAttributeSwitchHotKey;
	public KeyCode paintingValueChangeHotKey;
	public KeyCode paintingFalloffChangeHotKey;
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
	public bool createGroupsFromBoolAttributes;

	// Curve Settings
	public int curvePrimitiveTypeDefault;
	public int curveMethodDefault;

	// Initialization --------------------------------------------------------------------------
	public HoudiniHostUndoInfo()
	{
		initialize();
	}

	public void initialize()
	{
		// Set general settings.
		collisionGroupName 				= HoudiniHost.prCollisionGroupName;
		renderedCollisionGroupName 		= HoudiniHost.prRenderedCollisionGroupName;
		pinSize 						= HoudiniHost.prPinSize;
		pinColour 						= HoudiniHost.prPinColour;
		autoPinInstances 				= HoudiniHost.prAutoPinInstances;
		enableSupportWarnings 			= HoudiniHost.prEnableSupportWarnings;
		autoSelectAssetRootNode 		= HoudiniHost.prAutoSelectAssetRootNode;
		enablePointsAsParticles 		= HoudiniHost.prEnablePointsAsParticles;
		
		// Set material settings.
		unityMaterialAttribName 		= HoudiniHost.prUnityMaterialAttribName;
		unitySubMaterialNameAttribName 	= HoudiniHost.prUnitySubMaterialNameAttribName;
		unitySubMaterialIndexAttribName = HoudiniHost.prUnitySubMaterialIndexAttribName;
		gamma							= HoudiniHost.prGamma;
		dontCreateTextureFiles 			= HoudiniHost.prDontCreateTextureFiles;
		extractTexturesInRawFormat 		= HoudiniHost.prExtractTexturesInRawFormat;
		
		// Set cooking settings.
		enableCooking 					= HoudiniHost.prEnableCooking;
		cookingTriggersDownCooks 		= HoudiniHost.prCookingTriggersDownCooks;
		playmodePerFrameCooking 		= HoudiniHost.prPlaymodePerFrameCooking;
		pushUnityTransformToHoudini 	= HoudiniHost.prPushUnityTransformToHoudini;
		transformChangeTriggersCooks 	= HoudiniHost.prTransformChangeTriggersCooks;
		importTemplatedGeos 			= HoudiniHost.prImportTemplatedGeos;
		splitGeosByGroup				= HoudiniHost.prSplitGeosByGroup;
		
		// Set geometry settings.
		unityTagAttribName 				= HoudiniHost.prUnityTagAttribName;
		paintBrushRate					= HoudiniHost.prPaintBrushRate;
		paintingModeHotKey 				= HoudiniHost.prPaintingModeHotKey;
		paintingModeColour 				= HoudiniHost.prPaintingModeColour;
		paintingNodeSwitchHotKey		= HoudiniHost.prPaintingNodeSwitchHotKey;
		paintingAttributeSwitchHotKey	= HoudiniHost.prPaintingAttributeSwitchHotKey;
		paintingValueChangeHotKey		= HoudiniHost.prPaintingValueChangeHotKey;
		paintingFalloffChangeHotKey		= HoudiniHost.prPaintingFalloffChangeHotKey;
		addingPointsModeHotKey 			= HoudiniHost.prAddingPointsModeHotKey;
		addingPointsModeColour 			= HoudiniHost.prAddingPointsModeColour;
		editingPointsModeHotKey 		= HoudiniHost.prEditingPointsModeHotKey;
		editingPointsModeColour 		= HoudiniHost.prEditingPointsModeColour;
		wireframeColour 				= HoudiniHost.prWireframeColour;
		guideWireframeColour 			= HoudiniHost.prGuideWireframeColour;
		unselectableGuideWireframeColour = HoudiniHost.prUnselectableGuideWireframeColour;
		unselectedGuideWireframeColour 	= HoudiniHost.prUnselectedGuideWireframeColour;
		selectedGuideWireframeColour 	= HoudiniHost.prSelectedGuideWireframeColour;
		guidePointSize 					= HoudiniHost.prGuidePointSize;
		minDistanceForPointSelection 	= HoudiniHost.prMinDistanceForPointSelection;
		guideMinDistanceForMidPointInsertion = HoudiniHost.prGuideMinDistanceForMidPointInsertion;
		
		// Set curve settings.
		curvePrimitiveTypeDefault		= HoudiniHost.prCurvePrimitiveTypeDefault;
		curveMethodDefault				= HoudiniHost.prCurveMethodDefault;
	}
}

