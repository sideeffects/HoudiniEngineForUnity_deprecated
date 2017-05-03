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

public class HoudiniAssetUndoInfo : ScriptableObject
{
	// View Settings
	public bool isGeoVisible;
	public bool showPinnedInstances;

	// Material Settings
	public bool showOnlyVertexColours;
	public bool generateUVs;
	public bool generateLightmapUV2s;
	public bool generateTangents;

	// Cook Settings
	public bool enableCooking;
	public bool cookingTriggersDownCooks;
	public bool playmodePerFrameCooking;
	public bool pushUnityTransformToHoudini;
	public bool transformChangeTriggersCooks;
	public bool importTemplatedGeos;
	public bool splitGeosByGroupOverride;
	public bool splitGeosByGroup;
	public bool splitPointsGeosByGroupOverride;
	public bool splitPointsByVertexAttribute;
	public bool splitPointsByVertexAttributeOverride;
	public bool omitPartNameEnumeration;

	// Bake Settings
	public float bakeStartTime;
	public float bakeEndTime;
	public int bakeSamplesPerSecond;

	// Initialization
	public void initialize( HoudiniAsset asset )
	{
		isGeoVisible 					= asset.prIsGeoVisible;
		showPinnedInstances 			= asset.prShowPinnedInstances;
		showOnlyVertexColours 			= asset.prShowOnlyVertexColours;
		generateUVs						= asset.prGenerateUVs;
		generateLightmapUV2s			= asset.prGenerateLightmapUV2s;
		generateTangents 				= asset.prGenerateTangents;
		enableCooking 					= asset.prEnableCooking;
		cookingTriggersDownCooks 		= asset.prCookingTriggersDownCooks;
		playmodePerFrameCooking 		= asset.prPlaymodePerFrameCooking;
		pushUnityTransformToHoudini		= asset.prPushUnityTransformToHoudini;
		transformChangeTriggersCooks 	= asset.prTransformChangeTriggersCooks;
		importTemplatedGeos 			= asset.prImportTemplatedGeos;
		splitGeosByGroupOverride		= asset.prSplitGeosByGroupOverride;
		splitGeosByGroup				= asset.prSplitGeosByGroup;
		splitPointsByVertexAttribute    = asset.prSplitPointsByVertexAttribute;
		splitPointsByVertexAttributeOverride = asset.prSplitPointsByVertexAttributeOverride;
		omitPartNameEnumeration			= asset.prOmitPartNameEnumeration;
		bakeStartTime					= asset.prBakeStartTime;
		bakeEndTime						= asset.prBakeEndTime;
		bakeSamplesPerSecond			= asset.prBakeSamplesPerSecond;
	}
}
