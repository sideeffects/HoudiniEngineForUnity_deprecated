using UnityEngine;
using System.Collections;

public class HoudiniAssetUndoInfo : ScriptableObject
{
	// View Settings
	public bool isGeoVisible;
	public bool showPinnedInstances;
	public bool autoSelectAssetRootNode;

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

	// Bake Settings
	public float bakeStartTime;
	public float bakeEndTime;
	public int bakeSamplesPerSecond;

	// Initialization
	public void initialize( HoudiniAsset asset )
	{
		isGeoVisible 					= asset.prIsGeoVisible;
		showPinnedInstances 			= asset.prShowPinnedInstances;
		autoSelectAssetRootNode 		= asset.prAutoSelectAssetRootNode;
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
		bakeStartTime					= asset.prBakeStartTime;
		bakeEndTime						= asset.prBakeEndTime;
		bakeSamplesPerSecond			= asset.prBakeSamplesPerSecond;
	}
}
