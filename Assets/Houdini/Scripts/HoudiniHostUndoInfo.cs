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
	public bool splitPointsByVertexAttributes;
	
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
		// Can't call initialize() here.
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
		splitPointsByVertexAttributes	= HoudiniHost.prSplitPointsByVertexAttributes;

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

