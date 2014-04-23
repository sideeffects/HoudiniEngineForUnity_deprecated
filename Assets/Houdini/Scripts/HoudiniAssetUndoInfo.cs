using UnityEngine;
using System.Collections;

public class HoudiniAssetUndoInfo : ScriptableObject
{
	// View Settings
	public bool isGeoVisible;
	public bool showPinnedInstances;
	public bool autoSelectAssetRootNode;
	public bool hideGeometryOnLinking;

	// Material Settings
	public HAPI_ShaderType materialShaderType;
	public Vector2 renderResolution;
	public bool showOnlyVertexColours;
	public bool generateTangents;

	// Cook Settings
	public bool enableCooking;
	public bool cookingTriggersDownCooks;
	public bool playmodePerFrameCooking;
	public bool pushUnityTransformToHoudini;
	public bool transformChangeTriggersCooks;
	public bool importTemplatedGeos;

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
		hideGeometryOnLinking 			= asset.prHideGeometryOnLinking;
		materialShaderType 				= asset.prMaterialShaderType;
		renderResolution 				= asset.prRenderResolution;
		showOnlyVertexColours 			= asset.prShowOnlyVertexColours;
		generateTangents 				= asset.prGenerateTangents;
		enableCooking 					= asset.prEnableCooking;
		cookingTriggersDownCooks 		= asset.prCookingTriggersDownCooks;
		playmodePerFrameCooking 		= asset.prPlaymodePerFrameCooking;
		pushUnityTransformToHoudini		= asset.prPushUnityTransformToHoudini;
		transformChangeTriggersCooks 	= asset.prTransformChangeTriggersCooks;
		importTemplatedGeos 			= asset.prImportTemplatedGeos;
		bakeStartTime					= asset.prBakeStartTime;
		bakeEndTime						= asset.prBakeEndTime;
		bakeSamplesPerSecond			= asset.prBakeSamplesPerSecond;
	}
}
